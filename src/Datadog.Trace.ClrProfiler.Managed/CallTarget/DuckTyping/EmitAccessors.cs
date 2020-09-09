using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Accessors using IL
    /// </summary>
    public static class EmitAccessors
    {
        /// <summary>
        /// Creates the IL code for a get property
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature:
        /// [returnType] ([instanceType] instance);
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="property">Property info</param>
        /// <param name="instanceType">Instance type</param>
        /// <param name="returnType">Return type</param>
        public static void CreateGetAccessor(ILGenerator il, PropertyInfo property, Type instanceType, Type returnType)
        {
            if (!property.CanRead)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Throw);
            }

            if (property.GetMethod.IsStatic)
            {
                il.EmitCall(OpCodes.Call, property.GetMethod, null);
            }
            else
            {
                ILHelpers.LoadInstanceArgument(il, instanceType, property.DeclaringType);
                il.EmitCall(OpCodes.Callvirt, property.GetMethod, null);
            }

            ILHelpers.TypeConversion(il, property.PropertyType, returnType);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Creates the IL code for a set property
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature when the declaring type is a class:
        /// void ([instanceType] instance, [valueType] value);
        /// Methods should accomplish the following signature when the declaring type is a value:
        /// void (ref [instanceType] instance, [valueType] value);
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="property">Property info</param>
        /// <param name="instanceType">Instance type</param>
        /// <param name="valueType">Value type</param>
        public static void CreateSetAccessor(ILGenerator il, PropertyInfo property, Type instanceType, Type valueType)
        {
            if (!property.CanWrite)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Throw);
            }

            if (property.SetMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_1);
                ILHelpers.TypeConversion(il, valueType, property.PropertyType);
                il.EmitCall(OpCodes.Call, property.SetMethod, null);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (property.DeclaringType!.IsValueType)
                {
                    il.DeclareLocal(property.DeclaringType);
                    il.Emit(OpCodes.Ldind_Ref);
                    il.Emit(OpCodes.Unbox_Any, property.DeclaringType);
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldloca_S, 0);
                }
                else if (property.DeclaringType != instanceType)
                {
                    il.Emit(OpCodes.Castclass, property.DeclaringType);
                }

                il.Emit(OpCodes.Ldarg_1);
                ILHelpers.TypeConversion(il, valueType, property.PropertyType);
                il.EmitCall(OpCodes.Callvirt, property.SetMethod, null);
                if (property.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Box, property.DeclaringType);
                    il.Emit(OpCodes.Stind_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Create an accessor delegate for a MethodInfo
        /// </summary>
        /// <param name="method">Method info instance</param>
        /// <returns>Accessor delegate</returns>
        /// <param name="strict">Creates an strict accessor without basic conversion for IConvertibles</param>
        public static Func<object, object[], object> BuildMethodAccessor(MethodInfo method, bool strict)
        {
            var lstParams = new List<string>();
            var gParams = method.GetParameters();
            foreach (var p in gParams)
            {
                lstParams.Add(p.ParameterType.Name);
            }

            var callMethod = new DynamicMethod($"Call+{method.DeclaringType!.Name}.{method.Name}+{string.Join("_", lstParams)}", typeof(object), new[] { typeof(object), typeof(object) }, typeof(EmitAccessors).Module);
            CreateMethodAccessor(callMethod.GetILGenerator(), method, strict);
            return (Func<object, object[], object>)callMethod.CreateDelegate(typeof(Func<object, object[], object>));
        }

        /// <summary>
        /// Creates the IL code for calling a method
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature:
        /// object (object instance, object[] args)
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="method">Method info</param>
        /// <param name="strict">Creates an strict accessor without basic conversion for IConvertibles</param>
        public static void CreateMethodAccessor(ILGenerator il, MethodInfo method, bool strict)
        {
            // Prepare instance
            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                if (method.DeclaringType!.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, method.DeclaringType);
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldloca_S, 0);
                }
                else if (method.DeclaringType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, method.DeclaringType);
                }
            }

            // Prepare arguments
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var pType = parameters[i].ParameterType;
                var rType = Util.GetRootType(pType);
                var callEnum = false;
                if (rType.IsEnum)
                {
                    il.Emit(OpCodes.Ldtoken, rType);
                    il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                    callEnum = true;
                }

                il.Emit(OpCodes.Ldarg_1);
                ILHelpers.WriteIlIntValue(il, i);
                il.Emit(OpCodes.Ldelem_Ref);

                if (callEnum)
                {
                    il.EmitCall(OpCodes.Call, Util.EnumToObjectMethodInfo, null);
                }
                else if (!strict && pType != typeof(object))
                {
                    il.Emit(OpCodes.Ldtoken, rType);
                    il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);
                    il.EmitCall(OpCodes.Call, Util.ConvertTypeMethodInfo, null);
                }

                if (pType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, pType);
                }
                else if (pType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, pType);
                }
            }

            // Call method
            il.EmitCall(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method, null);

            // Prepare return
            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (method.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }

            il.Emit(OpCodes.Ret);
        }
    }
}
