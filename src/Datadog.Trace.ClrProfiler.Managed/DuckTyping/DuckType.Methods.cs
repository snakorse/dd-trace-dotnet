using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public static partial class DuckType
    {
        private static List<MethodInfo> GetMethods(Type baseType)
        {
            List<MethodInfo> selectedMethods = new List<MethodInfo>(GetBaseMethods(baseType));
            Type[] implementedInterfaces = baseType.GetInterfaces();
            foreach (Type imInterface in implementedInterfaces)
            {
                if (imInterface == typeof(IDuckType))
                {
                    continue;
                }

                IEnumerable<MethodInfo> newMethods = imInterface.GetMethods()
                    .Where(iMethod =>
                    {
                        if (iMethod.IsSpecialName)
                        {
                            return false;
                        }

                        string iMethodString = iMethod.ToString();
                        return selectedMethods.All(i => i.ToString() != iMethodString);
                    });
                selectedMethods.AddRange(newMethods);
            }

            return selectedMethods;
            static IEnumerable<MethodInfo> GetBaseMethods(Type baseType)
            {
                foreach (MethodInfo method in baseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.IsSpecialName || method.IsFinal || method.IsPrivate)
                    {
                        continue;
                    }

                    if (baseType.IsInterface || method.IsAbstract || method.IsVirtual)
                    {
                        yield return method;
                    }
                }
            }
        }

        private static void CreateMethods(TypeBuilder proxyTypeBuilder, Type proxyType, Type targetType, FieldInfo instanceField)
        {
            List<MethodInfo> proxyMethodsDefinitions = GetMethods(proxyType);
            foreach (MethodInfo proxyMethodDefinition in proxyMethodsDefinitions)
            {
                // Extract the method parameters types
                ParameterInfo[] proxyMethodDefinitionParameters = proxyMethodDefinition.GetParameters();
                Type[] proxyMethodDefinitionParametersTypes = proxyMethodDefinitionParameters.Select(p => p.ParameterType).ToArray();

                // We select the target method to call
                MethodInfo targetMethod = SelectTargetMethod(targetType, proxyMethodDefinition, proxyMethodDefinitionParameters, proxyMethodDefinitionParametersTypes);

                // If the target method couldn't be found and the proxy method doesn't have an implementation already (ex: abstract and virtual classes) we throw.
                if (targetMethod is null && proxyMethodDefinition.IsVirtual)
                {
                    throw new DuckTypeTargetMethodNotFoundException(proxyMethodDefinition);
                }

                // Gets target method parameters
                ParameterInfo[] targetMethodParameters = targetMethod.GetParameters();
                Type[] targetMethodParametersTypes = targetMethodParameters.Select(p => p.ParameterType).ToArray();

                // Make sure we have the right methods attributes, for proxy methods declared in abstract and virtual classes
                // a new slot on the vtable is not required, for interfaces is required.
                MethodAttributes proxyMethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig;
                if (!proxyMethodDefinition.IsAbstract && !proxyMethodDefinition.IsVirtual)
                {
                    proxyMethodAttributes |= MethodAttributes.NewSlot;
                }

                // Gets the proxy method definition generic arguments
                Type[] proxyMethodDefinitionGenericArguments = proxyMethodDefinition.GetGenericArguments();
                string[] proxyMethodDefinitionGenericArgumentsNames = proxyMethodDefinitionGenericArguments.Select(a => a.Name).ToArray();

                // Create the proxy method implementation
                ParameterBuilder[] proxyMethodParametersBuilders = new ParameterBuilder[proxyMethodDefinitionParameters.Length];
                MethodBuilder proxyMethod = proxyTypeBuilder.DefineMethod(proxyMethodDefinition.Name, proxyMethodAttributes, proxyMethodDefinition.ReturnType, proxyMethodDefinitionParametersTypes);
                if (proxyMethodDefinitionGenericArgumentsNames.Length > 0)
                {
                    GenericTypeParameterBuilder[] proxyMethodGenericTypeParametersBuilders = proxyMethod.DefineGenericParameters(proxyMethodDefinitionGenericArgumentsNames);
                }

                // Define the proxy method implementation parameters for optional parameters with default values
                for (int j = 0; j < proxyMethodDefinitionParameters.Length; j++)
                {
                    ParameterInfo pmDefParameter = proxyMethodDefinitionParameters[j];
                    ParameterBuilder pmImpParameter = proxyMethod.DefineParameter(j, pmDefParameter.Attributes, pmDefParameter.Name);
                    if (pmDefParameter.HasDefaultValue)
                    {
                        pmImpParameter.SetConstant(pmDefParameter.RawDefaultValue);
                    }

                    proxyMethodParametersBuilders[j] = pmImpParameter;
                }

                ILGenerator il = proxyMethod.GetILGenerator();
                bool publicInstance = targetType.IsPublic || targetType.IsNestedPublic;
                Type returnType = targetMethod.ReturnType;
                List<OutputAndRefParameterData> outputAndRefParameters = null;

                // Load the instance if needed
                if (!targetMethod.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, instanceField);
                }

                int maxParamLength = Math.Max(proxyMethodDefinitionParameters.Length, targetMethodParameters.Length);
                for (int idx = 0; idx < maxParamLength; idx++)
                {
                    ParameterInfo proxyParamInfo = idx < proxyMethodDefinitionParameters.Length ? proxyMethodDefinitionParameters[idx] : null;
                    ParameterInfo targetParamInfo = idx < targetMethodParameters.Length ? targetMethodParameters[idx] : null;

                    if (targetParamInfo is null)
                    {
                        // The target method is missing parameters
                        throw new DuckTypeTargetMethodParameterIsMissingException(targetMethod, proxyParamInfo);
                    }
                    else if (proxyParamInfo is null)
                    {
                        // The proxy method is missing parameters, we check if the target parameter is optional
                        if (!targetParamInfo.IsOptional)
                        {
                            // The target method parameter is not optional.
                            throw new DuckTypeProxyMethodParameterIsMissingException(proxyMethodDefinition, targetParamInfo);
                        }
                    }
                    else
                    {
                        if (proxyParamInfo.IsOut != targetParamInfo.IsOut || proxyParamInfo.IsIn != targetParamInfo.IsIn)
                        {
                            // The target method parameter is not optional.
                            throw new DuckTypeProxyAndTargetMethodParameterSignatureMismatch(proxyMethodDefinition, targetMethod);
                        }

                        Type proxyParamType = proxyParamInfo.ParameterType;
                        Type targetParamType = targetParamInfo.ParameterType;

                        // We check if we have to handle an output parameter, by ref parameter or a normal parameter
                        if (proxyParamInfo.IsOut)
                        {
                            // If is an output parameter with diferent types we need to handle differently
                            // by creating a local var first to store the target parameter out value
                            // and then try to set the output parameter of the proxy method by converting the value (a base class or a duck typing)
                            if (proxyParamType != targetParamType)
                            {
                                LocalBuilder localTargetArg = il.DeclareLocal(targetParamType);

                                // We need to store the output parameter data to set the proxy parameter value after we call the target method
                                if (outputAndRefParameters is null)
                                {
                                    outputAndRefParameters = new List<OutputAndRefParameterData>();
                                }

                                outputAndRefParameters.Add(new OutputAndRefParameterData(localTargetArg.LocalIndex, targetParamType, idx, proxyParamType));

                                // Load the local var ref (to be used in the target method param as output)
                                il.Emit(OpCodes.Ldloca_S, localTargetArg.LocalIndex);
                            }
                            else
                            {
                                ILHelpers.WriteLoadArgument(idx, il, false);
                            }
                        }
                        else if (proxyParamType.IsByRef)
                        {
                            // If is a ref parameter with diferent types we need to handle differently
                            // by creating a local var first to store the initial proxy parameter ref value casted to the target parameter type ( this cast may fail at runtime )
                            // later pass this local var ref to the target method, and then, modify the proxy parameter ref with the new reference from the target method
                            // by converting the value (a base class or a duck typing)
                            if (proxyParamType != targetParamType)
                            {
                                Type proxyParamTypeElementType = proxyParamType.GetElementType();
                                Type targetParamTypeElementType = targetParamType.GetElementType();

                                LocalBuilder localTargetArg = il.DeclareLocal(targetParamType);

                                // We need to store the ref parameter data to set the proxy parameter value after we call the target method
                                if (outputAndRefParameters is null)
                                {
                                    outputAndRefParameters = new List<OutputAndRefParameterData>();
                                }

                                outputAndRefParameters.Add(new OutputAndRefParameterData(localTargetArg.LocalIndex, targetParamType, idx, proxyParamType));

                                // Load the argument (ref)
                                ILHelpers.WriteLoadArgument(idx, il, false);

                                // Load the value inside the ref
                                il.Emit(OpCodes.Ldind_Ref);

                                // Check if the type can be converted of if we need to enable duck chaining
                                if (!proxyParamTypeElementType.IsValueType &&
                                    !proxyParamTypeElementType.IsGenericParameter &&
                                    !proxyParamTypeElementType.IsAssignableFrom(targetParamTypeElementType))
                                {
                                    // First we check if the value is null before trying to get the instance value
                                    Label lblCallGetInstance = il.DefineLabel();
                                    Label lblAfterGetInstance = il.DefineLabel();

                                    il.Emit(OpCodes.Dup);
                                    il.Emit(OpCodes.Brtrue_S, lblCallGetInstance);

                                    il.Emit(OpCodes.Pop);
                                    il.Emit(OpCodes.Ldnull);
                                    il.Emit(OpCodes.Br_S, lblAfterGetInstance);

                                    // Call IDuckType.Instance property to get the actual value
                                    il.MarkLabel(lblCallGetInstance);
                                    il.Emit(OpCodes.Castclass, typeof(IDuckType));
                                    il.EmitCall(OpCodes.Callvirt, DuckTypeInstancePropertyInfo.GetMethod, null);
                                    il.MarkLabel(lblAfterGetInstance);
                                }

                                // Cast the value to the target type
                                ILHelpers.TypeConversion(il, proxyParamTypeElementType, targetParamTypeElementType);

                                // Store the casted value to the local var
                                ILHelpers.WriteStoreLocal(localTargetArg.LocalIndex, il);

                                // Load the local var ref (to be used in the target method param)
                                il.Emit(OpCodes.Ldloca_S, localTargetArg.LocalIndex);
                            }
                            else
                            {
                                ILHelpers.WriteLoadArgument(idx, il, false);
                            }
                        }
                        else
                        {
                            // Check if the type can be converted of if we need to enable duck chaining
                            if (proxyParamType != targetParamType &&
                                !proxyParamType.IsValueType &&
                                !proxyParamType.IsGenericParameter &&
                                !proxyParamType.IsAssignableFrom(targetParamType))
                            {
                                // Load the argument and cast it as Duck type
                                ILHelpers.WriteLoadArgument(idx, il, false);
                                il.Emit(OpCodes.Castclass, typeof(IDuckType));

                                // Call IDuckType.Instance property to get the actual value
                                il.EmitCall(OpCodes.Callvirt, DuckTypeInstancePropertyInfo.GetMethod, null);
                            }
                            else
                            {
                                ILHelpers.WriteLoadArgument(idx, il, false);
                            }

                            // If the target parameter type is public or if it's by ref we have to actually use the original target type.
                            targetParamType = targetParamType.IsPublic || targetParamType.IsNestedPublic || targetParamType.IsByRef ? targetParamType : typeof(object);
                            ILHelpers.TypeConversion(il, proxyParamType, targetParamType);

                            targetMethodParametersTypes[idx] = targetParamType;
                        }
                    }
                }

                // Call the target method
                if (publicInstance)
                {
                    // If the instance is public we can emit directly without any dynamic method

                    // Create generic method call
                    if (proxyMethodDefinitionGenericArguments.Length > 0)
                    {
                        targetMethod = targetMethod.MakeGenericMethod(proxyMethodDefinitionGenericArguments);
                    }

                    // Method call
                    if (targetMethod.IsPublic)
                    {
                        // We can emit a normal call if we have a public instance with a public target method.
                        il.EmitCall(targetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, targetMethod, null);
                    }
                    else
                    {
                        // In case we have a public instance and a non public target method we can use [Calli] with the function pointer
                        il.Emit(OpCodes.Ldc_I8, (long)targetMethod.MethodHandle.GetFunctionPointer());
                        il.Emit(OpCodes.Conv_I);
                        il.EmitCalli(
                            OpCodes.Calli,
                            targetMethod.CallingConvention,
                            targetMethod.ReturnType,
                            targetMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                            null);
                    }
                }
                else
                {
                    // A generic method call can't be made from a DynamicMethod
                    if (proxyMethodDefinitionGenericArguments.Length > 0)
                    {
                        throw new DuckTypeProxyMethodsWithGenericParametersNotSupportedInNonPublicInstancesException(proxyMethod);
                    }

                    // If the instance is not public we need to create a Dynamic method to overpass the visibility checks
                    // we can't access non public types so we have to cast to object type (in the instance object and the return type).

                    string dynMethodName = $"_callMethod+{targetMethod.DeclaringType.Name}.{targetMethod.Name}";
                    returnType = (targetMethod.ReturnType.IsPublic || targetMethod.ReturnType.IsNestedPublic) && !targetMethod.ReturnType.IsGenericParameter ? targetMethod.ReturnType : typeof(object);

                    // We create the dynamic method
                    Type[] originalTargetParameters = targetMethod.GetParameters().Select(p => p.ParameterType).ToArray();
                    Type[] targetParameters = targetMethod.IsStatic ? originalTargetParameters : (new[] { typeof(object) }).Concat(originalTargetParameters).ToArray();
                    Type[] dynParameters = targetMethod.IsStatic ? targetMethodParametersTypes : (new[] { typeof(object) }).Concat(targetMethodParametersTypes).ToArray();
                    DynamicMethod dynMethod = new DynamicMethod(dynMethodName, returnType, dynParameters, typeof(DuckType).Module, true);

                    // We store the dynamic method in a bag to avoid getting collected by the GC.
                    DynamicMethods.Add(dynMethod);

                    // Emit the dynamic method body
                    ILGenerator dynIL = dynMethod.GetILGenerator();

                    if (!targetMethod.IsStatic)
                    {
                        ILHelpers.LoadInstanceArgument(dynIL, typeof(object), targetMethod.DeclaringType);
                    }

                    for (int idx = targetMethod.IsStatic ? 0 : 1; idx < dynParameters.Length; idx++)
                    {
                        ILHelpers.WriteLoadArgument(idx, dynIL, true);
                        ILHelpers.TypeConversion(dynIL, dynParameters[idx], targetParameters[idx]);
                    }

                    // Check if we can emit a normal Call/CallVirt to the target method
                    if (!targetMethod.ContainsGenericParameters)
                    {
                        dynIL.EmitCall(targetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, targetMethod, null);
                    }
                    else
                    {
                        // We can't emit a call to a method with generics from a DynamicMethod
                        // Instead we emit a Calli with the function pointer.
                        dynIL.Emit(OpCodes.Ldc_I8, (long)targetMethod.MethodHandle.GetFunctionPointer());
                        dynIL.Emit(OpCodes.Conv_I);
                        dynIL.EmitCalli(
                            OpCodes.Calli,
                            targetMethod.CallingConvention,
                            targetMethod.ReturnType,
                            targetMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                            null);
                    }

                    ILHelpers.TypeConversion(dynIL, targetMethod.ReturnType, returnType);
                    dynIL.Emit(OpCodes.Ret);

                    // Emit the Call to the dynamic method pointer [Calli]
                    il.Emit(OpCodes.Ldc_I8, (long)GetRuntimeHandle(dynMethod).GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, dynMethod.ReturnType, dynParameters, null);
                }

                // We check if we have output or ref parameters to set in the proxy method
                if (outputAndRefParameters != null)
                {
                    foreach (OutputAndRefParameterData outOrRefParameter in outputAndRefParameters)
                    {
                        Type proxyArgumentType = outOrRefParameter.ProxyArgumentType.GetElementType();
                        Type localType = outOrRefParameter.LocalType.GetElementType();

                        // We load the argument to be set
                        ILHelpers.WriteLoadArgument(outOrRefParameter.ProxyArgumentIndex, il, false);

                        // We load the value from the local
                        ILHelpers.WriteLoadLocal(outOrRefParameter.LocalIndex, il);

                        // If we detect duck chaining we create a new proxy instance with the output of the original target method
                        if (!proxyArgumentType.IsValueType &&
                            !proxyArgumentType.IsGenericParameter &&
                            !proxyArgumentType.IsAssignableFrom(localType))
                        {
                            // If we are in a duck chaining scenario we convert the field value to an object and push it to the stack
                            ILHelpers.TypeConversion(il, localType, typeof(object));

                            // Load the property type to the stack
                            il.Emit(OpCodes.Ldtoken, proxyArgumentType);
                            il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);

                            // We call DuckType.GetStructDuckTypeChainningValue() with the 2 loaded values from the stack: field value, property type
                            il.EmitCall(OpCodes.Call, GetDuckTypeChainningValueMethodInfo, null);
                        }
                        else
                        {
                            ILHelpers.TypeConversion(il, localType, proxyArgumentType);
                        }

                        // We store the value
                        il.Emit(OpCodes.Stind_Ref);
                    }
                }

                // Check if the target method returns something
                if (targetMethod.ReturnType != typeof(void))
                {
                    // Handle the return value
                    // Check if the type can be converted or if we need to enable duck chaining
                    if (proxyMethodDefinition.ReturnType != targetMethod.ReturnType && !proxyMethodDefinition.ReturnType.IsValueType && !proxyMethodDefinition.ReturnType.IsAssignableFrom(targetMethod.ReturnType))
                    {
                        // If we are in a duck chaining scenario we convert the field value to an object and push it to the stack
                        ILHelpers.TypeConversion(il, returnType, typeof(object));

                        // Load the property type to the stack
                        il.Emit(OpCodes.Ldtoken, proxyMethodDefinition.ReturnType);
                        il.EmitCall(OpCodes.Call, Util.GetTypeFromHandleMethodInfo, null);

                        // We call DuckType.GetStructDuckTypeChainningValue() with the 2 loaded values from the stack: field value, property type
                        il.EmitCall(OpCodes.Call, GetDuckTypeChainningValueMethodInfo, null);
                    }
                    else if (returnType != proxyMethodDefinition.ReturnType)
                    {
                        // If the type is not the expected type we try a conversion.
                        ILHelpers.TypeConversion(il, returnType, proxyMethodDefinition.ReturnType);
                    }
                }

                il.Emit(OpCodes.Ret);
            }
        }

        private static MethodInfo SelectTargetMethod(Type targetType, MethodInfo proxyMethod, ParameterInfo[] proxyMethodParameters, Type[] proxyMethodParametersTypes)
        {
            DuckAttribute proxyMethodDuckAttribute = proxyMethod.GetCustomAttributes<DuckAttribute>(true).FirstOrDefault() ?? new DuckAttribute();
            proxyMethodDuckAttribute.Name ??= proxyMethod.Name;
            string proxyMethodString = proxyMethod.ToString();

            // We select the method to call
            MethodInfo targetMethod = targetType.GetMethod(proxyMethodDuckAttribute.Name, proxyMethodDuckAttribute.BindingFlags, null, proxyMethodParametersTypes, null);

            if (!(targetMethod is null))
            {
                // We check if the target method has a duck attribute (Used by a reverse proxy scenario)
                DuckAttribute targetMethodDuckAttribute = targetMethod.GetCustomAttributes<DuckAttribute>().FirstOrDefault();
                if (targetMethodDuckAttribute is null || targetMethodDuckAttribute.Name == proxyMethodString)
                {
                    return targetMethod;
                }
            }

            // If the method wasn't found (DuckTyped parameter or return value) we try to find a similar that will work.
            MethodInfo[] allTargetMethods = targetType.GetMethods(proxyMethodDuckAttribute.BindingFlags);
            List<MethodInfo> preselectedTargetMethods = allTargetMethods.Where(method =>
            {
                // Check if the target method contains a DuckAttribute to match the proxy method (reverse proxy scenario)
                DuckAttribute methodDuckAttribute = method.GetCustomAttributes<DuckAttribute>(true).FirstOrDefault();
                if (methodDuckAttribute != null)
                {
                    return methodDuckAttribute.Name == proxyMethodString;
                }

                // We omit target methods with different names.
                if (method.Name != proxyMethodDuckAttribute.Name)
                {
                    return false;
                }

                // We pre-select the ones with the same parameters count
                ParameterInfo[] methodParametersInfo = method.GetParameters();
                if (methodParametersInfo.Length == proxyMethodParameters.Length)
                {
                    return true;
                }

                // We pre-select the ones with differents parameters count but with default values to
                // fulfill the missing parameters count.
                int minCount = Math.Min(methodParametersInfo.Length, proxyMethodParameters.Length);
                int maxCount = Math.Max(methodParametersInfo.Length, proxyMethodParameters.Length);
                for (int i = minCount; i < maxCount; i++)
                {
                    if (methodParametersInfo.Length > i && !methodParametersInfo[i].HasDefaultValue)
                    {
                        return false;
                    }

                    if (proxyMethodParameters.Length > i && !proxyMethodParameters[i].HasDefaultValue)
                    {
                        return false;
                    }
                }
                return true;
            }).ToList();

            // If there are not preselected target methods we bailout.
            if (preselectedTargetMethods.Count == 0)
            {
                return null;
            }

            // If there's only one target method we return it.
            if (preselectedTargetMethods.Count == 1)
            {
                return preselectedTargetMethods[0];
            }

            // If there is at least one preselected with a Duck attribute (reverse proxy) we return that one.
            MethodInfo preselectedWithDuckAttribute = preselectedTargetMethods.FirstOrDefault(m => m.GetCustomAttributes<DuckAttribute>().Any());
            if (preselectedWithDuckAttribute != null)
            {
                return preselectedWithDuckAttribute;
            }

            // Trying to select the ones with the same return type
            List<MethodInfo> sameReturnType = preselectedTargetMethods.Where(method => method.ReturnType == proxyMethod.ReturnType).ToList();
            if (sameReturnType.Count == 1)
            {
                return sameReturnType[0];
            }

            if (sameReturnType.Count > 1)
            {
                preselectedTargetMethods = sameReturnType;
            }

            if (proxyMethod.ReturnType.IsInterface && proxyMethod.ReturnType.GetInterface(proxyMethod.ReturnType.FullName) == null)
            {
                List<MethodInfo> duckReturnType = preselectedTargetMethods.Where(method => !method.ReturnType.IsValueType).ToList();
                if (duckReturnType.Count == 1)
                {
                    return duckReturnType[0];
                }

                if (duckReturnType.Count > 1)
                {
                    preselectedTargetMethods = duckReturnType;
                }
            }

            // Trying to select the one with the same parameters types
            List<MethodInfo> sameParameters = preselectedTargetMethods.Where(method =>
            {
                ParameterInfo[] mParams = method.GetParameters();
                int min = Math.Min(mParams.Length, proxyMethodParameters.Length);
                for (int i = 0; i < min; i++)
                {
                    Type expectedType = mParams[i].ParameterType;
                    Type actualType = proxyMethodParameters[i].ParameterType;

                    if (expectedType == actualType)
                    {
                        continue;
                    }

                    if (expectedType.IsAssignableFrom(actualType))
                    {
                        continue;
                    }

                    if (!expectedType.IsValueType && actualType == typeof(object))
                    {
                        continue;
                    }

                    if (expectedType.IsValueType && actualType.IsValueType)
                    {
                        continue;
                    }

                    return false;
                }
                return true;
            }).ToList();

            if (sameParameters.Count == 1)
            {
                return sameParameters[0];
            }

            return preselectedTargetMethods[0];
        }

        private static void CreateMethodAccessor(ILGenerator il, MethodInfo method, bool strict)
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
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                Type pType = parameters[i].ParameterType;
                Type rType = Util.GetRootType(pType);
                bool callEnum = false;
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

        private readonly struct OutputAndRefParameterData
        {
            public readonly Type LocalType;
            public readonly Type ProxyArgumentType;
            public readonly int LocalIndex;
            public readonly int ProxyArgumentIndex;

            public OutputAndRefParameterData(int localIndex, Type localType, int proxyArgumentIndex, Type proxyArgumentType)
            {
                LocalIndex = localIndex;
                LocalType = localType;
                ProxyArgumentIndex = proxyArgumentIndex;
                ProxyArgumentType = proxyArgumentType;
            }
        }
    }
}
