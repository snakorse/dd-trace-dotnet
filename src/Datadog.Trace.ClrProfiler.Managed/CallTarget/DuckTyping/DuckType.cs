using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType : IDuckType
    {
        /// <summary>
        /// Current instance
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#pragma warning disable SA1401 // Fields must be private
        protected object _currentInstance;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Instance type
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Type _type;

        /// <summary>
        /// Assembly version
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Version _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuckType"/> class.
        /// </summary>
        protected DuckType()
        {
        }

        /// <summary>
        /// Gets instance
        /// </summary>
        public object Instance => _currentInstance;

        /// <summary>
        /// Gets instance Type
        /// </summary>
        public Type Type => _type ??= _currentInstance?.GetType();

        /// <summary>
        /// Gets assembly version
        /// </summary>
        public Version AssemblyVersion => _version ??= Type?.Assembly?.GetName().Version;

        private static CreateTypeResult GetOrCreateProxyType(Type proxyType, Type targetType)
        {
            VTuple<Type, Type> key = new VTuple<Type, Type>(proxyType, targetType);

            if (DuckTypeCache.TryGetValue(key, out CreateTypeResult proxyTypeResult))
            {
                return proxyTypeResult;
            }

            lock (DuckTypeCache)
            {
                if (!DuckTypeCache.TryGetValue(key, out proxyTypeResult))
                {
                    proxyTypeResult = CreateProxyType(proxyType, targetType);
                    DuckTypeCache[key] = proxyTypeResult;
                }

                return proxyTypeResult;
            }
        }

        private static CreateTypeResult CreateProxyType(Type proxyType, Type targetType)
        {
            try
            {
                // Define parent type, interface types
                Type parentType;
                Type[] interfaceTypes;
                if (proxyType.IsInterface)
                {
                    parentType = typeof(DuckType);
                    interfaceTypes = new[] { proxyType };
                }
                else
                {
                    parentType = proxyType;
                    interfaceTypes = Type.EmptyTypes;
                }

                // Gets the current instance field info
                FieldInfo instanceField = parentType.GetField(nameof(_currentInstance), BindingFlags.Instance | BindingFlags.NonPublic);
                if (instanceField is null)
                {
                    interfaceTypes = DefaultInterfaceTypes;
                }

                // Ensures the module builder
                if (_moduleBuilder is null)
                {
                    lock (_locker)
                    {
                        if (_moduleBuilder is null)
                        {
                            AssemblyName aName = new AssemblyName("DuckTypeAssembly");
                            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
                            _moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
                        }
                    }
                }

                string proxyTypeName = $"{proxyType.FullName}->{targetType.FullName}";
                Log.Information("Creating type proxy: " + proxyTypeName);

                // Create Type
                TypeBuilder proxyTypeBuilder = _moduleBuilder.DefineType(
                    proxyTypeName,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout | TypeAttributes.Sealed,
                    parentType,
                    interfaceTypes);

                // Define .ctor
                proxyTypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                // Create instance field if is null
                instanceField ??= CreateInstanceField(proxyTypeBuilder);

                // Create Members
                CreateProperties(proxyTypeBuilder, proxyType, targetType, instanceField);
                CreateMethods(proxyTypeBuilder, proxyType, targetType, instanceField);

                // Create Type
                return new CreateTypeResult(proxyTypeBuilder.CreateTypeInfo().AsType(), null);
            }
            catch (Exception ex)
            {
                return new CreateTypeResult(null, ExceptionDispatchInfo.Capture(ex));
            }
        }

        private static FieldInfo CreateInstanceField(TypeBuilder proxyTypeBuilder)
        {
            var instanceField = proxyTypeBuilder.DefineField(nameof(_currentInstance), typeof(object), FieldAttributes.Family);

            var setInstance = proxyTypeBuilder.DefineMethod(
                nameof(SetInstance),
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(void),
                new[] { typeof(object) });
            var il = setInstance.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, instanceField);
            il.Emit(OpCodes.Ret);

            var propInstance = proxyTypeBuilder.DefineProperty(nameof(Instance), PropertyAttributes.None, typeof(object), null);
            var getPropInstance = proxyTypeBuilder.DefineMethod(
                $"get_{nameof(Instance)}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(object),
                Type.EmptyTypes);
            il = getPropInstance.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.Emit(OpCodes.Ret);
            propInstance.SetGetMethod(getPropInstance);

            var propType = proxyTypeBuilder.DefineProperty(nameof(Type), PropertyAttributes.None, typeof(Type), null);
            var getPropType = proxyTypeBuilder.DefineMethod(
                $"get_{nameof(Type)}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(Type),
                Type.EmptyTypes);
            il = getPropType.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.EmitCall(OpCodes.Callvirt, typeof(object).GetMethod("GetType"), null);
            il.Emit(OpCodes.Ret);
            propType.SetGetMethod(getPropType);

            var propVersion = proxyTypeBuilder.DefineProperty(nameof(AssemblyVersion), PropertyAttributes.None, typeof(Version), null);
            var getPropVersion = proxyTypeBuilder.DefineMethod(
                $"get_{nameof(AssemblyVersion)}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(Version),
                Type.EmptyTypes);
            il = getPropVersion.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);
            il.EmitCall(OpCodes.Callvirt, typeof(Type).GetProperty("Assembly").GetMethod, null);
            il.EmitCall(OpCodes.Callvirt, typeof(Assembly).GetMethod("GetName", Type.EmptyTypes), null);
            il.EmitCall(OpCodes.Callvirt, typeof(AssemblyName).GetProperty("Version").GetMethod, null);
            il.Emit(OpCodes.Ret);
            propVersion.SetGetMethod(getPropVersion);

            return instanceField;
        }

        private static List<PropertyInfo> GetProperties(Type proxyType)
        {
            var selectedProperties = new List<PropertyInfo>(proxyType.IsInterface ? proxyType.GetProperties() : GetBaseProperties(proxyType));
            var implementedInterfaces = proxyType.GetInterfaces();
            foreach (var imInterface in implementedInterfaces)
            {
                if (imInterface == typeof(IDuckType))
                {
                    continue;
                }

                var newProps = imInterface.GetProperties().Where(p => selectedProperties.All(i => i.Name != p.Name));
                selectedProperties.AddRange(newProps);
            }

            return selectedProperties;

            static IEnumerable<PropertyInfo> GetBaseProperties(Type baseType)
            {
                foreach (var prop in baseType.GetProperties())
                {
                    if (prop.DeclaringType == typeof(DuckType))
                    {
                        continue;
                    }

                    if (prop.CanRead && (prop.GetMethod.IsAbstract || prop.GetMethod.IsVirtual))
                    {
                        yield return prop;
                    }
                    else if (prop.CanWrite && (prop.SetMethod.IsAbstract || prop.SetMethod.IsVirtual))
                    {
                        yield return prop;
                    }
                }
            }
        }

        private static void CreateProperties(TypeBuilder proxyTypeBuilder, Type proxyType, Type targetType, FieldInfo instanceField)
        {
            var targetTypeAssemblyVersion = targetType.Assembly.GetName().Version;
            // Gets all properties to be implemented
            var proxyTypeProperties = GetProperties(proxyType);

            foreach (var proxyProperty in proxyTypeProperties)
            {
                PropertyBuilder propertyBuilder = null;

                // If the property is abstract or interface we make sure that we have the property defined in the new class
                if ((proxyProperty.CanRead && proxyProperty.GetMethod.IsAbstract) || (proxyProperty.CanWrite && proxyProperty.SetMethod.IsAbstract))
                {
                    propertyBuilder = proxyTypeBuilder.DefineProperty(proxyProperty.Name, PropertyAttributes.None, proxyProperty.PropertyType, null);
                }

                var duckAttrs = new List<DuckAttribute>(proxyProperty.GetCustomAttributes<DuckAttribute>(true));
                if (duckAttrs.Count == 0)
                {
                    duckAttrs.Add(new DuckAttribute());
                }

                duckAttrs.Sort((x, y) =>
                {
                    if (x.Version is null)
                    {
                        return 1;
                    }

                    if (y.Version is null)
                    {
                        return -1;
                    }

                    return x.Version.CompareTo(y.Version);
                });

                foreach (var duckAttr in duckAttrs)
                {
                    if (!(duckAttr.Version is null) && targetTypeAssemblyVersion > duckAttr.Version)
                    {
                        continue;
                    }

                    duckAttr.Name ??= proxyProperty.Name;

                    switch (duckAttr.Kind)
                    {
                        case DuckKind.Property:
                            PropertyInfo targetProperty = null;
                            try
                            {
                                targetProperty = targetType.GetProperty(duckAttr.Name, duckAttr.BindingFlags);
                            }
                            catch
                            {
                                targetProperty = targetType.GetProperty(duckAttr.Name, proxyProperty.PropertyType, proxyProperty.GetIndexParameters().Select(i => i.ParameterType).ToArray());
                            }

                            if (targetProperty is null)
                            {
                                continue;
                            }

                            propertyBuilder ??= proxyTypeBuilder.DefineProperty(proxyProperty.Name, PropertyAttributes.None, proxyProperty.PropertyType, null);

                            if (proxyProperty.CanRead)
                            {
                                // Check if the target property can be read
                                if (!targetProperty.CanRead)
                                {
                                    throw new DuckTypePropertyCantBeReadException(targetProperty);
                                }

                                propertyBuilder.SetGetMethod(GetPropertyGetMethod(proxyTypeBuilder, targetType, proxyProperty, targetProperty, instanceField));
                            }

                            if (proxyProperty.CanWrite)
                            {
                                // Check if the target property can be written
                                if (!targetProperty.CanWrite)
                                {
                                    throw new DuckTypePropertyCantBeWrittenException(targetProperty);
                                }

                                // Check if the target property declaring type is an struct (structs modification is not supported)
                                if (targetProperty.DeclaringType.IsValueType)
                                {
                                    throw new DuckTypeStructMembersCannotBeChangedException(targetProperty.DeclaringType);
                                }

                                propertyBuilder.SetSetMethod(GetPropertySetMethod(proxyTypeBuilder, targetType, proxyProperty, targetProperty, instanceField));
                            }

                            break;

                        case DuckKind.Field:
                            FieldInfo targetField = targetType.GetField(duckAttr.Name, duckAttr.BindingFlags);
                            if (targetField is null)
                            {
                                continue;
                            }

                            propertyBuilder ??= proxyTypeBuilder.DefineProperty(proxyProperty.Name, PropertyAttributes.None, proxyProperty.PropertyType, null);

                            if (proxyProperty.CanRead)
                            {
                                propertyBuilder.SetGetMethod(GetFieldGetMethod(proxyTypeBuilder, targetType, proxyProperty, targetField, instanceField));
                            }

                            if (proxyProperty.CanWrite)
                            {
                                // Check if the target field is marked as InitOnly (readonly) and throw an exception in that case
                                if ((targetField.Attributes & FieldAttributes.InitOnly) != 0)
                                {
                                    throw new DuckTypeFieldIsReadonlyException(targetField);
                                }

                                // Check if the target field declaring type is an struct (structs modification is not supported)
                                if (targetField.DeclaringType.IsValueType)
                                {
                                    throw new DuckTypeStructMembersCannotBeChangedException(targetField.DeclaringType);
                                }

                                propertyBuilder.SetSetMethod(GetFieldSetMethod(proxyTypeBuilder, targetType, proxyProperty, targetField, instanceField));
                            }

                            break;
                    }

                    break;
                }

                if (propertyBuilder is null)
                {
                    continue;
                }

                if (proxyProperty.CanRead && propertyBuilder.GetMethod is null)
                {
                    throw new DuckTypePropertyOrFieldNotFoundException(proxyProperty.Name);
                }

                if (proxyProperty.CanWrite && propertyBuilder.SetMethod is null)
                {
                    throw new DuckTypePropertyOrFieldNotFoundException(proxyProperty.Name);
                }
            }
        }

        /// <inheritdoc/>
        void IDuckType.SetInstance(object instance)
        {
            _currentInstance = instance;
        }

        private void SetInstance(object instance)
        {
            _currentInstance = instance;
        }

        private readonly struct CreateTypeResult
        {
            public readonly Type ProxyType;
            public readonly ExceptionDispatchInfo ExceptionInfo;
            public readonly bool Success;

            public CreateTypeResult(Type proxyType, ExceptionDispatchInfo exceptionInfo)
            {
                ProxyType = proxyType;
                ExceptionInfo = exceptionInfo;
                Success = proxyType != null && exceptionInfo == null;
            }
        }
    }
}
