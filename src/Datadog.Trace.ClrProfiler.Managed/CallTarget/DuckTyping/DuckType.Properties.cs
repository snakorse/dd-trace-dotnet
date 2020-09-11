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
            Type returnType = property.PropertyType;

            // Validate the property return value
            bool duckChaining = false;
            Type duckTypePropertyType = duckTypeProperty.PropertyType;
            if (duckTypePropertyType.IsGenericType)
            {
                duckTypePropertyType = duckTypePropertyType.GetGenericTypeDefinition();
            }

            // Check if the type can be converted of if we need to enable duck chaining
            if (duckTypeProperty.PropertyType != property.PropertyType && parameterTypes.Length == 0 &&
                !duckTypeProperty.PropertyType.IsValueType && !duckTypeProperty.PropertyType.IsAssignableFrom(property.PropertyType))
            {
                // Create and load the duck type field reference to the stack
                if (propertyMethod.IsStatic)
                {
                    FieldInfo innerDuckField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_duckStatic_" + duckTypeProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerDuckField);
                }
                else
                {
                    FieldInfo innerDuckField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_duck_" + duckTypeProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerDuckField);
                }

                // Load the property type to the stack
                il.Emit(OpCodes.Ldtoken, duckTypeProperty.PropertyType);
                il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                duckChaining = true;
            }

            // Load the instance if needed
            if (!propertyMethod.IsStatic)
            {
                ILHelpers.LoadInstance(il, instanceField, instanceType);
            }

            if (publicInstance)
            {
                // If the instance is public we can emit directly without any dynamic method

                // Checks if we have index parameters and ensure to pass it
                if (parameterTypes.Length > 0)
                {
                    var propertyIndexParameters = property.GetIndexParameters();
                    for (var i = 0; i < parameterTypes.Length; i++)
                    {
                        ILHelpers.WriteLoadArgument(i, il, propertyMethod.IsStatic);
                        Type parameterType = Util.GetRootType(parameterTypes[i]);
                        Type targetParameterType = Util.GetRootType(propertyIndexParameters[i].ParameterType);
                        ILHelpers.TypeConversion(il, parameterType, targetParameterType);
                    }
                }

                // Method call
                if (propertyMethod.IsPublic)
                {
                    // We can emit a normal call if we have a public instance with a public property method.
                    il.EmitCall(propertyMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propertyMethod, null);
                }
                else
                {
                    // In case we have a public instance and a non public property method we can use [Calli] with the function pointer
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
                    // If we are in a duck chaining scenario we convert the field value to an object and push it to the stack
                    ILHelpers.TypeConversion(il, property.PropertyType, typeof(object));

                    // We call DuckType.GetInnerDuckType() with the 3 loaded values from the stack: ducktype field reference, property type and the, field value
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                }
                else if (property.PropertyType != duckTypeProperty.PropertyType)
                {
                    // If the type is not the expected type we try a conversion.
                    ILHelpers.TypeConversion(il, property.PropertyType, duckTypeProperty.PropertyType);
                }
            }
            else
            {
                // If the instance is not public we need to create a Dynamic method to overpass the visibility checks
                // we can't access non public types so we have to cast to object type (in the instance object and the return type).

                if (propertyMethod.IsStatic)
                {
                    il.Emit(OpCodes.Ldnull);
                }

                string dynMethodName = $"_getNonPublicProperty+{property.DeclaringType.Name}.{property.Name}";
                Type dynReturnType = property.PropertyType.IsPublic || property.PropertyType.IsNestedPublic ? property.PropertyType : typeof(object);

                // We create the dynamic method
                Type[] dynParameters = GetPropertyGetParametersTypes(property, true).ToArray();
                if (property.PropertyType.IsPublic || property.PropertyType.IsNestedPublic)
                {
                    dynReturnType = property.PropertyType;
                }

                DynamicMethod dynMethod = new DynamicMethod(dynMethodName, dynReturnType, dynParameters, typeof(DuckType).Module, true);

                // We store the dynamic method in a bag to avoid getting collected by the GC.
                DynamicMethods.Add(dynMethod);

                // Emit the dynamic method body
                EmitAccessors.CreateGetAccessor(dynMethod.GetILGenerator(), property, typeof(object), dynReturnType);

                // Emit the Call to the dynamic method pointer [Calli]
                il.Emit(OpCodes.Ldc_I8, (long)GetRuntimeHandle(dynMethod).GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, dynMethod.ReturnType, dynParameters, null);

                // Handle return value
                if (duckChaining)
                {
                    // If we are in a duck chaining scenario we convert the field value to an object and push it to the stack
                    ILHelpers.TypeConversion(il, dynReturnType, typeof(object));

                    // We call DuckType.GetInnerDuckType() with the 3 loaded values from the stack: ducktype field reference, property type and the, field value
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                }
                else
                {
                    // If the type is not the expected type we try a conversion.
                    ILHelpers.TypeConversion(il, dynReturnType, duckTypeProperty.PropertyType);
                }
            }

            il.Emit(OpCodes.Ret);
            return method;
        }

        private static MethodBuilder GetPropertySetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo duckTypeProperty, PropertyInfo property, FieldInfo instanceField)
        {
            Type[] parameterTypes = GetPropertySetParametersTypes(duckTypeProperty).ToArray();
            MethodBuilder method = typeBuilder.DefineMethod(
                "set_" + duckTypeProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(void),
                parameterTypes);

            ILGenerator il = method.GetILGenerator();
            MethodInfo propertyMethod = property.SetMethod;
            bool publicInstance = instanceType.IsPublic || instanceType.IsNestedPublic;

            // Load the instance if needed
            if (!propertyMethod.IsStatic)
            {
                ILHelpers.LoadInstance(il, instanceField, instanceType);
            }
            else if (!publicInstance)
            {
                il.Emit(OpCodes.Ldnull);
            }

            // Validate the property return value
            Type duckTypePropertyType = duckTypeProperty.PropertyType;
            if (duckTypePropertyType.IsGenericType)
            {
                duckTypePropertyType = duckTypePropertyType.GetGenericTypeDefinition();
            }

            // Check if the type can be converted of if we need to enable duck chaining
            if (duckTypeProperty.PropertyType != property.PropertyType && parameterTypes.Length == 1 &&
                !duckTypeProperty.PropertyType.IsValueType && !duckTypeProperty.PropertyType.IsAssignableFrom(property.PropertyType))
            {
                // Create and load the duck type field reference to the stack
                if (propertyMethod.IsStatic)
                {
                    FieldInfo innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_duckStatic_" + duckTypeProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerField);
                }
                else
                {
                    FieldInfo innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_duck_" + duckTypeProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                }

                // Load the property type to the stack
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, typeof(DuckType));
                il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
            }
            else
            {
                // Checks if we have index parameters and ensure to pass it
                List<Type> propertyIndexParameters = GetPropertySetParametersTypes(property).ToList();
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    ILHelpers.WriteLoadArgument(i, il, method.IsStatic);
                    Type parameterType = Util.GetRootType(parameterTypes[i]);
                    Type targetParameterType = propertyIndexParameters[i].IsPublic || propertyIndexParameters[i].IsNestedPublic ? Util.GetRootType(propertyIndexParameters[i]) : typeof(object);
                    ILHelpers.TypeConversion(il, parameterType, targetParameterType);
                }
            }

            if (publicInstance)
            {
                // If the instance is public we can emit directly without any dynamic method

                if (propertyMethod.IsPublic)
                {
                    // We can emit a normal call if we have a public instance with a public property method.
                    il.EmitCall(propertyMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propertyMethod, null);
                }
                else
                {
                    // In case we have a public instance and a non public property method we can use [Calli] with the function pointer
                    il.Emit(OpCodes.Ldc_I8, (long)propertyMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(
                        OpCodes.Calli,
                        propertyMethod.CallingConvention,
                        propertyMethod.ReturnType,
                        propertyMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                }
            }
            else
            {
                // If the instance is not public we need to create a Dynamic method to overpass the visibility checks
                // we can't access non public types so we have to cast to object type (in the instance object and the return type).

                string dynMethodName = $"_setNonPublicProperty+{property.DeclaringType.Name}.{property.Name}";

                // We create the dynamic method
                Type[] dynParameters = GetPropertySetParametersTypes(property, true).ToArray();
                DynamicMethod dynMethod = new DynamicMethod(dynMethodName, typeof(void), dynParameters, typeof(EmitAccessors).Module, true);
                // We store the dynamic method in a bag to avoid getting collected by the GC.
                DynamicMethods.Add(dynMethod);

                // Emit the dynamic method body
                EmitAccessors.CreateSetAccessor(dynMethod.GetILGenerator(), property, dynParameters[0], dynParameters[1]);

                // Emit the Call to the dynamic method pointer [Calli]
                il.Emit(OpCodes.Ldc_I8, (long)GetRuntimeHandle(dynMethod).GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, dynMethod.ReturnType, dynParameters, null);
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
