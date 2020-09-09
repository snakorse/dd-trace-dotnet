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

        private static CreateTypeResult GetOrCreateProxyType(Type duckType, Type instanceType)
        {
            VTuple<Type, Type> key = new VTuple<Type, Type>(duckType, instanceType);

            if (DuckTypeCache.TryGetValue(key, out CreateTypeResult proxyTypeResult))
            {
                return proxyTypeResult;
            }

            lock (DuckTypeCache)
            {
                if (!DuckTypeCache.TryGetValue(key, out proxyTypeResult))
                {
                    proxyTypeResult = CreateProxyType(duckType, instanceType);
                    DuckTypeCache[key] = proxyTypeResult;
                }

                return proxyTypeResult;
            }
        }

        private static CreateTypeResult CreateProxyType(Type duckType, Type instanceType)
        {
            try
            {
                // Define parent type, interface types
                Type parentType;
                Type[] interfaceTypes;
                if (duckType.IsInterface)
                {
                    parentType = typeof(DuckType);
                    interfaceTypes = new[] { duckType };
                }
                else
                {
                    parentType = duckType;
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

                string proxyTypeName = $"{duckType.FullName}->{instanceType.FullName}";
                Log.Information("Creating type proxy: " + proxyTypeName);

                // Create Type
                TypeBuilder typeBuilder = _moduleBuilder.DefineType(
                    proxyTypeName,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout | TypeAttributes.Sealed,
                    parentType,
                    interfaceTypes);

                // Define .ctor
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                // Create instance field if is null
                instanceField ??= CreateInstanceField(typeBuilder);

                // Create Members
                CreateProperties(duckType, instanceType, instanceField, typeBuilder);
                CreateMethods(duckType, instanceType, instanceField, typeBuilder);

                // Create Type
                return new CreateTypeResult(typeBuilder.CreateTypeInfo().AsType(), null);
            }
            catch (Exception ex)
            {
                return new CreateTypeResult(null, ExceptionDispatchInfo.Capture(ex));
            }
        }

        private static FieldInfo CreateInstanceField(TypeBuilder typeBuilder)
        {
            var instanceField = typeBuilder.DefineField(nameof(_currentInstance), typeof(object), FieldAttributes.Family);

            var setInstance = typeBuilder.DefineMethod(
                nameof(SetInstance),
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(void),
                new[] { typeof(object) });
            var il = setInstance.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, instanceField);
            il.Emit(OpCodes.Ret);

            var propInstance = typeBuilder.DefineProperty(nameof(Instance), PropertyAttributes.None, typeof(object), null);
            var getPropInstance = typeBuilder.DefineMethod(
                $"get_{nameof(Instance)}",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(object),
                Type.EmptyTypes);
            il = getPropInstance.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.Emit(OpCodes.Ret);
            propInstance.SetGetMethod(getPropInstance);

            var propType = typeBuilder.DefineProperty(nameof(Type), PropertyAttributes.None, typeof(Type), null);
            var getPropType = typeBuilder.DefineMethod(
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

            var propVersion = typeBuilder.DefineProperty(nameof(AssemblyVersion), PropertyAttributes.None, typeof(Version), null);
            var getPropVersion = typeBuilder.DefineMethod(
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

        private static List<PropertyInfo> GetProperties(Type baseType)
        {
            var selectedProperties = new List<PropertyInfo>(baseType.IsInterface ? baseType.GetProperties() : GetBaseProperties(baseType));
            var implementedInterfaces = baseType.GetInterfaces();
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

        private static void CreateProperties(Type baseType, Type instanceType, FieldInfo instanceField, TypeBuilder typeBuilder)
        {
            var asmVersion = instanceType.Assembly.GetName().Version;
            // Gets all properties to be implemented
            var selectedProperties = GetProperties(baseType);

            foreach (var property in selectedProperties)
            {
                PropertyBuilder propertyBuilder = null;

                // If the property is abstract or interface we make sure that we have the property defined in the new class
                if ((property.CanRead && property.GetMethod.IsAbstract) || (property.CanWrite && property.SetMethod.IsAbstract))
                {
                    propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
                }

                var duckAttrs = new List<DuckAttribute>(property.GetCustomAttributes<DuckAttribute>(true));
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
                    if (!(duckAttr.Version is null) && asmVersion > duckAttr.Version)
                    {
                        continue;
                    }

                    duckAttr.Name ??= property.Name;

                    switch (duckAttr.Kind)
                    {
                        case DuckKind.Property:
                            var prop = instanceType.GetProperty(duckAttr.Name, duckAttr.BindingFlags);
                            if (prop is null)
                            {
                                continue;
                            }

                            propertyBuilder ??= typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);

                            if (property.CanRead)
                            {
                                propertyBuilder.SetGetMethod(GetPropertyGetMethod(instanceType, typeBuilder, property, prop, instanceField));
                            }

                            if (property.CanWrite)
                            {
                                propertyBuilder.SetSetMethod(GetPropertySetMethod(instanceType, typeBuilder, property, prop, instanceField));
                            }

                            break;

                        case DuckKind.Field:
                            var field = instanceType.GetField(duckAttr.Name, duckAttr.BindingFlags);
                            if (field is null)
                            {
                                continue;
                            }

                            propertyBuilder ??= typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);

                            if (property.CanRead)
                            {
                                propertyBuilder.SetGetMethod(GetFieldGetMethod(instanceType, typeBuilder, property, field, instanceField));
                            }

                            if (property.CanWrite)
                            {
                                propertyBuilder.SetSetMethod(GetFieldSetMethod(instanceType, typeBuilder, property, field, instanceField));
                            }

                            break;
                    }

                    break;
                }

                if (propertyBuilder is null)
                {
                    continue;
                }

                if (property.CanRead && propertyBuilder.GetMethod is null)
                {
                    propertyBuilder.SetGetMethod(GetNotFoundGetMethod(instanceType, typeBuilder, property));
                }

                if (property.CanWrite && propertyBuilder.SetMethod is null)
                {
                    propertyBuilder.SetSetMethod(GetNotFoundSetMethod(instanceType, typeBuilder, property));
                }
            }
        }

        private static MethodBuilder GetNotFoundGetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo duckTypeProperty)
        {
            Type[] parameterTypes;
            var idxParams = duckTypeProperty.GetIndexParameters();
            if (idxParams.Length > 0)
            {
                parameterTypes = new Type[idxParams.Length];
                for (var i = 0; i < idxParams.Length; i++)
                {
                    parameterTypes[i] = idxParams[i].ParameterType;
                }
            }
            else
            {
                parameterTypes = Type.EmptyTypes;
            }

            var method = typeBuilder.DefineMethod(
                $"get_{duckTypeProperty.Name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                duckTypeProperty.PropertyType,
                parameterTypes);

            var il = method.GetILGenerator();
            il.Emit(OpCodes.Newobj, typeof(DuckTypePropertyOrFieldNotFoundException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);
            return method;
        }

        private static MethodBuilder GetNotFoundSetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo duckTypeProperty)
        {
            Type[] parameterTypes;
            var idxParams = duckTypeProperty.GetIndexParameters();
            if (idxParams.Length > 0)
            {
                parameterTypes = new Type[idxParams.Length + 1];
                for (var i = 0; i < idxParams.Length; i++)
                {
                    parameterTypes[i] = idxParams[i].ParameterType;
                }

                parameterTypes[idxParams.Length] = duckTypeProperty.PropertyType;
            }
            else
            {
                parameterTypes = new[] { duckTypeProperty.PropertyType };
            }

            var method = typeBuilder.DefineMethod(
                "set_" + duckTypeProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(void),
                parameterTypes);

            var il = method.GetILGenerator();
            il.Emit(OpCodes.Newobj, typeof(DuckTypePropertyOrFieldNotFoundException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);
            return method;
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
            public readonly Type Type;
            public readonly ExceptionDispatchInfo ExceptionInfo;
            public readonly bool Success;

            public CreateTypeResult(Type type, ExceptionDispatchInfo exceptionInfo)
            {
                Type = type;
                ExceptionInfo = exceptionInfo;
                Success = type != null && exceptionInfo == null;
            }
        }
    }
}
