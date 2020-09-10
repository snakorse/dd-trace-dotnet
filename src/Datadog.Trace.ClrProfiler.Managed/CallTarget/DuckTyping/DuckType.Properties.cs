using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType
    {
        private static MethodBuilder GetPropertyGetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo duckTypeProperty, PropertyInfo property, FieldInfo instanceField)
        {
            Type[] parameterTypes = GetPropertyGetParametersTypes(duckTypeProperty).ToArray();

            MethodBuilder method = typeBuilder.DefineMethod(
                "get_" + duckTypeProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                duckTypeProperty.PropertyType,
                parameterTypes);

            ILGenerator il = method.GetILGenerator();
            MethodInfo propertyMethod = property.GetMethod;
            bool publicInstance = instanceType.IsPublic || instanceType.IsNestedPublic;

            // Check if an inner duck type is needed
            bool duckChaining = false;
            var iPropTypeInterface = duckTypeProperty.PropertyType;
            if (iPropTypeInterface.IsGenericType)
            {
                iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
            }

            if (duckTypeProperty.PropertyType != property.PropertyType && parameterTypes.Length == 0 &&
                !duckTypeProperty.PropertyType.IsValueType && !duckTypeProperty.PropertyType.IsAssignableFrom(property.PropertyType))
            {
                if (propertyMethod.IsStatic)
                {
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + duckTypeProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerField);
                }
                else
                {
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + duckTypeProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                }

                il.Emit(OpCodes.Ldtoken, duckTypeProperty.PropertyType);
                il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                duckChaining = true;
            }

            // Load the instance
            if (!propertyMethod.IsStatic)
            {
                ILHelpers.LoadInstance(il, instanceField, instanceType);
            }

            if (publicInstance)
            {
                // If we have index parameters we need to pass it
                if (parameterTypes.Length > 0)
                {
                    var propIdxParams = property.GetIndexParameters();
                    for (var i = 0; i < parameterTypes.Length; i++)
                    {
                        ILHelpers.WriteLoadArgument(i, il, propertyMethod.IsStatic);
                        var iPType = Util.GetRootType(parameterTypes[i]);
                        var pType = Util.GetRootType(propIdxParams[i].ParameterType);
                        ILHelpers.TypeConversion(il, iPType, pType);
                    }
                }

                // Method call
                if (propertyMethod.IsPublic)
                {
                    il.EmitCall(propertyMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propertyMethod, null);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long)propertyMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(
                        OpCodes.Calli,
                        propertyMethod.CallingConvention,
                        propertyMethod.ReturnType,
                        propertyMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                }

                // Handle return value
                if (duckChaining)
                {
                    ILHelpers.TypeConversion(il, property.PropertyType, typeof(object));
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                }
                else if (property.PropertyType != duckTypeProperty.PropertyType)
                {
                    ILHelpers.TypeConversion(il, property.PropertyType, duckTypeProperty.PropertyType);
                }
            }
            else
            {
                if (propertyMethod.IsStatic)
                {
                    il.Emit(OpCodes.Ldnull);
                }

                var dynReturnType = typeof(object);
                var dynParameters = GetPropertyGetParametersTypes(property, true).ToArray();
                if (property.PropertyType.IsPublic || property.PropertyType.IsNestedPublic)
                {
                    dynReturnType = property.PropertyType;
                }

                var dynMethod = new DynamicMethod("getDyn_" + property.Name, dynReturnType, dynParameters, typeof(EmitAccessors).Module, true);
                EmitAccessors.CreateGetAccessor(dynMethod.GetILGenerator(), property, typeof(object), dynReturnType);
                var handle = GetRuntimeHandle(dynMethod);

                il.Emit(OpCodes.Ldc_I8, (long)handle.GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, dynMethod.ReturnType, dynParameters, null);
                DynamicMethods.Add(dynMethod);

                // Handle return value
                if (duckChaining)
                {
                    ILHelpers.TypeConversion(il, dynReturnType, typeof(object));
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                }
                else
                {
                    ILHelpers.TypeConversion(il, dynReturnType, duckTypeProperty.PropertyType);
                }
            }

            il.Emit(OpCodes.Ret);
            return method;
        }

        private static MethodBuilder GetPropertySetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo duckTypeProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            var parameterTypes = GetPropertySetParametersTypes(duckTypeProperty).ToArray();
            var method = typeBuilder.DefineMethod(
                "set_" + duckTypeProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(void),
                parameterTypes);

            var il = method.GetILGenerator();
            var propMethod = prop.SetMethod;
            var publicInstance = instanceType.IsPublic || instanceType.IsNestedPublic;

            // Load instance
            if (!propMethod.IsStatic)
            {
                ILHelpers.LoadInstance(il, instanceField, instanceType);
            }
            else if (!publicInstance)
            {
                il.Emit(OpCodes.Ldnull);
            }

            // Check if a duck type object
            var iPropTypeInterface = duckTypeProperty.PropertyType;
            if (iPropTypeInterface.IsGenericType)
            {
                iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
            }

            if (duckTypeProperty.PropertyType != prop.PropertyType && parameterTypes.Length == 1 &&
                !duckTypeProperty.PropertyType.IsValueType && !duckTypeProperty.PropertyType.IsAssignableFrom(prop.PropertyType))
            {
                if (propMethod.IsStatic)
                {
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + duckTypeProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerField);
                }
                else
                {
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + duckTypeProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                }

                // Load value
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, typeof(DuckType));
                il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
            }
            else
            {
                // Load values
                // If we have index parameters we need to pass it
                var propTypes = GetPropertySetParametersTypes(prop).ToList();
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    ILHelpers.WriteLoadArgument(i, il, method.IsStatic);
                    var iPropRootType = Util.GetRootType(parameterTypes[i]);
                    var propRootType = propTypes[i].IsPublic || propTypes[i].IsNestedPublic ? Util.GetRootType(propTypes[i]) : typeof(object);
                    ILHelpers.TypeConversion(il, iPropRootType, propRootType);
                }
            }

            if (publicInstance)
            {
                // Call method
                if (propMethod.IsPublic)
                {
                    il.EmitCall(propMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propMethod, null);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long)propMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(
                        OpCodes.Calli,
                        propMethod.CallingConvention,
                        propMethod.ReturnType,
                        propMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                }
            }
            else
            {
                var dynParameters = GetPropertySetParametersTypes(prop, true).ToArray();
                var dynMethod = new DynamicMethod("setDyn_" + prop.Name, typeof(void), dynParameters, typeof(EmitAccessors).Module, true);
                EmitAccessors.CreateSetAccessor(dynMethod.GetILGenerator(), prop, dynParameters[0], dynParameters[1]);
                var handle = GetRuntimeHandle(dynMethod);

                il.Emit(OpCodes.Ldc_I8, (long)handle.GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, dynMethod.ReturnType, dynParameters, null);
                DynamicMethods.Add(dynMethod);
            }

            il.Emit(OpCodes.Ret);
            return method;
        }

        private static IEnumerable<Type> GetPropertyGetParametersTypes(PropertyInfo property, bool isDynamicSignature = false)
        {
            if (isDynamicSignature)
            {
                yield return typeof(object);
            }

            ParameterInfo[] idxParams = property.GetIndexParameters();
            foreach (ParameterInfo parameter in idxParams)
            {
                if (property.PropertyType.IsPublic || property.PropertyType.IsNestedPublic)
                {
                    yield return parameter.ParameterType;
                }
                else
                {
                    yield return typeof(object);
                }
            }
        }

        private static IEnumerable<Type> GetPropertySetParametersTypes(PropertyInfo property, bool isDynamicSignature = false)
        {
            if (isDynamicSignature)
            {
                yield return typeof(object);
            }

            foreach (Type indexType in GetPropertyGetParametersTypes(property))
            {
                yield return indexType;
            }

            if (property.PropertyType.IsPublic || property.PropertyType.IsNestedPublic)
            {
                yield return property.PropertyType;
            }
            else
            {
                yield return typeof(object);
            }
        }
    }
}
