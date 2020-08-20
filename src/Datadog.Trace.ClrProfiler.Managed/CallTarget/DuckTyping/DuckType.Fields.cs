using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType
    {
        private static MethodBuilder GetFieldGetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo duckTypeProperty, FieldInfo field, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod(
                "get_" + duckTypeProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                duckTypeProperty.PropertyType,
                Type.EmptyTypes);

            var il = method.GetILGenerator();
            var isPublicInstance = instanceType.IsPublic || instanceType.IsNestedPublic;
            var returnType = field.FieldType;

            // Validate property return value
            var duckChaining = false;
            var duckTypePropertyType = duckTypeProperty.PropertyType;
            if (duckTypePropertyType.IsGenericType)
            {
                duckTypePropertyType = duckTypePropertyType.GetGenericTypeDefinition();
            }

            // Check if the type can be converted of if we need to enable duck chaining
            if (duckTypeProperty.PropertyType != field.FieldType && !duckTypeProperty.PropertyType.IsValueType && !duckTypeProperty.PropertyType.IsAssignableFrom(field.FieldType))
            {
                // Create and load the duck type field reference to the stack
                if (field.IsStatic)
                {
                    var innerDuckField = DynamicFields.GetOrAdd(
                        new VTuple<string, TypeBuilder>("_duckStatic_" + duckTypeProperty.Name, typeBuilder),
                        tuple => tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerDuckField);
                }
                else
                {
                    var innerDuckField = DynamicFields.GetOrAdd(
                        new VTuple<string, TypeBuilder>("_duck_" + duckTypeProperty.Name, typeBuilder),
                        tuple => tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerDuckField);
                }

                // Load the property type to the stack
                il.Emit(OpCodes.Ldtoken, duckTypeProperty.PropertyType);
                il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                duckChaining = true;
            }

            // Load the field value to the stack
            if (isPublicInstance && field.IsPublic)
            {
                // In case is public is pretty simple
                if (field.IsStatic)
                {
                    il.Emit(OpCodes.Ldsfld, field);
                }
                else
                {
                    ILHelpers.LoadInstance(il, instanceField, instanceType);
                    il.Emit(OpCodes.Ldfld, field);
                }
            }
            else
            {
                // If the instance or the field are non public we need to create a Dynamic method to overpass the visibility checks

                DynamicMethod dynMethod = null;

                if (field.IsStatic)
                {
                    il.Emit(OpCodes.Ldnull);
                }
                else
                {
                    ILHelpers.LoadInstance(il, instanceField, instanceType);
                }

                // Create dynamic method
                returnType = field.FieldType.IsPublic || field.FieldType.IsNestedPublic ? field.FieldType : typeof(object);
                var dynParameters = new[] { typeof(object) };
                dynMethod = new DynamicMethod($"_getNonPublicField+{field.DeclaringType.Name}.{field.Name}", returnType, dynParameters, typeof(EmitAccessors).Module, true);
                EmitAccessors.CreateGetAccessor(dynMethod.GetILGenerator(), field, typeof(object), returnType);
                DynamicMethods.Add(dynMethod);

                // Emit the Call to the dynamic method
                il.Emit(OpCodes.Ldc_I8, (long)GetRuntimeHandle(dynMethod).GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, returnType, dynParameters, null);
            }

            if (duckChaining)
            {
                // If we are in a duck chaining scenario we convert the field value to an object and push it to the stack
                ILHelpers.TypeConversion(il, returnType, typeof(object));

                // We call DuckType.GetInnerDuckType() with the 3 loaded values from the stack: ducktype field reference, property type and the, field value
                il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
            }
            else if (returnType != duckTypeProperty.PropertyType)
            {
                // If the type is not the expected type we try a conversion.
                ILHelpers.TypeConversion(il, returnType, duckTypeProperty.PropertyType);
            }

            il.Emit(OpCodes.Ret);
            return method;
        }

        private static MethodBuilder GetFieldSetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo duckTypeProperty, FieldInfo field, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod(
                "set_" + duckTypeProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(void),
                new[] { duckTypeProperty.PropertyType });

            var il = method.GetILGenerator();
            var isPublicInstance = instanceType.IsPublic || instanceType.IsNestedPublic;

            // Check if the field is marked as InitOnly (readonly) and throw an exception in that case
            if ((field.Attributes & FieldAttributes.InitOnly) != 0)
            {
                il.Emit(OpCodes.Newobj, typeof(DuckTypeFieldIsReadonlyException).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Throw);
                return method;
            }

            // Load instance
            if (!isPublicInstance || !field.IsPublic)
            {
                // If the instance or the field is non public we load the instance field to the stack (needed when calling the Dynamic method to overpass the visibility checks)
                if (field.IsStatic)
                {
                    il.Emit(OpCodes.Ldnull);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, instanceField);
                }
            }
            else if (!field.IsStatic)
            {
                // If the instance and the field are public then we load the instance field
                ILHelpers.LoadInstance(il, instanceField, instanceType);
            }

            // Check if a duck type object
            var duckTypePropertyType = duckTypeProperty.PropertyType;
            if (duckTypePropertyType.IsGenericType)
            {
                duckTypePropertyType = duckTypePropertyType.GetGenericTypeDefinition();
            }

            // Check if the type can be converted of if we need to enable duck chaining
            if (duckTypeProperty.PropertyType != field.FieldType && !duckTypeProperty.PropertyType.IsValueType && !duckTypeProperty.PropertyType.IsAssignableFrom(field.FieldType))
            {
                // Create and load the duck type field reference to the stack
                if (field.IsStatic)
                {
                    var innerDuckField = DynamicFields.GetOrAdd(
                        new VTuple<string, TypeBuilder>("_duckStatic_" + duckTypeProperty.Name, typeBuilder),
                        tuple => tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerDuckField);
                }
                else
                {
                    var innerDuckField = DynamicFields.GetOrAdd(
                        new VTuple<string, TypeBuilder>("_duck_" + duckTypeProperty.Name, typeBuilder),
                        tuple => tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerDuckField);
                }

                // Load the argument and cast it as Duck type
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, typeof(DuckType));

                // Call the DuckType.SetInnerDuckType() with 2 loaded values from the stack: the inner ducktype field and the value argument to be setted
                // This call push a new value to be used in the stack
                il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
            }
            else
            {
                // Load the value into the stack
                il.Emit(OpCodes.Ldarg_1);
            }

            // We set the field value
            if (isPublicInstance && field.IsPublic)
            {
                // If the instance and the field are public then is easy to set.
                var fieldRootType = Util.GetRootType(field.FieldType);
                var dPropRootType = Util.GetRootType(duckTypeProperty.PropertyType);
                ILHelpers.TypeConversion(il, dPropRootType, fieldRootType);

                il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
            }
            else
            {
                // If the instance or the field are non public we need to create a Dynamic method to overpass the visibility checks

                // Convert the field type for the dynamic method
                var dynValueType = field.FieldType.IsPublic || field.FieldType.IsNestedPublic ? field.FieldType : typeof(object);
                var dPropRootType = Util.GetRootType(duckTypeProperty.PropertyType);
                ILHelpers.TypeConversion(il, dPropRootType, dynValueType);

                // Create dynamic method
                var dynParameters = new[] { typeof(object), dynValueType };
                var dynMethod = new DynamicMethod($"_setField+{field.DeclaringType.Name}.{field.Name}", typeof(void), dynParameters, typeof(EmitAccessors).Module, true);
                EmitAccessors.CreateSetAccessor(dynMethod.GetILGenerator(), field, dynParameters[0], dynParameters[1]);
                DynamicMethods.Add(dynMethod);

                // Emit the call to the dynamic method
                il.Emit(OpCodes.Ldc_I8, (long)GetRuntimeHandle(dynMethod).GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, typeof(void), dynParameters, null);
            }

            il.Emit(OpCodes.Ret);
            return method;
        }
    }
}
