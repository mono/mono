//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Description;

    delegate object InvokeDelegate(object target, object[] inputs, object[] outputs);
    delegate IAsyncResult InvokeBeginDelegate(object target, object[] inputs, AsyncCallback asyncCallback, object state);
    delegate object InvokeEndDelegate(object target, object[] outputs, IAsyncResult result);
    delegate object CreateInstanceDelegate();

    sealed class InvokerUtil
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that was produced within an assert.")]
        [SecurityCritical]
        CriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        public InvokerUtil()
        {
            helper = new CriticalHelper();
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.",
            Safe = "Resultant delegate is self-contained and safe for general consumption, nothing else leaks.")]
        [SecuritySafeCritical]
        internal CreateInstanceDelegate GenerateCreateInstanceDelegate(Type type, ConstructorInfo constructor)
        {
            return helper.GenerateCreateInstanceDelegate(type, constructor);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.",
            Safe = "Resultant delegate is self-contained and safe for general consumption, parameter counts are safe, nothing else leaks.")]
        [SecuritySafeCritical]
        internal InvokeDelegate GenerateInvokeDelegate(MethodInfo method, out int inputParameterCount, out int outputParameterCount)
        {
            return helper.GenerateInvokeDelegate(method, out inputParameterCount, out outputParameterCount);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.",
            Safe = "Resultant delegate is self-contained and safe for general consumption, parameter counts are safe, nothing else leaks.")]
        [SecuritySafeCritical]
        internal InvokeBeginDelegate GenerateInvokeBeginDelegate(MethodInfo method, out int inputParameterCount)
        {
            return helper.GenerateInvokeBeginDelegate(method, out inputParameterCount);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical helper class 'CriticalHelper'.",
            Safe = "Resultant delegate is self-contained and safe for general consumption, parameter counts are safe, nothing else leaks.")]
        [SecuritySafeCritical]
        internal InvokeEndDelegate GenerateInvokeEndDelegate(MethodInfo method, out int outputParameterCount)
        {
            return helper.GenerateInvokeEndDelegate(method, out outputParameterCount);
        }
        
        [Fx.Tag.SecurityNote(Critical = "Handles all aspects of IL generation including initializing the DynamicMethod, which requires an elevation."
            + "Since the ILGenerator is created under an elevation, we lock down access to every aspect of the generation.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
        [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
        class CriticalHelper
        {
            static Type TypeOfObject = typeof(object);

            CodeGenerator ilg;

            internal CreateInstanceDelegate GenerateCreateInstanceDelegate(Type type, ConstructorInfo constructor)
            {
                bool requiresMemberAccess = !IsTypeVisible(type) || ConstructorRequiresMemberAccess(constructor);

                this.ilg = new CodeGenerator();
                try
                {
                    ilg.BeginMethod("Create" + type.FullName, typeof(CreateInstanceDelegate), requiresMemberAccess);
                }
                catch (SecurityException securityException)
                {
                    if (requiresMemberAccess && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        DiagnosticUtility.TraceHandledException(securityException, TraceEventType.Warning);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityException(SR.GetString(
                                    SR.PartialTrustServiceCtorNotVisible,
                                    type.FullName)));
                    }
                    else
                    {
                        throw;
                    }
                }

                if (type.IsValueType)
                {
                    LocalBuilder instanceLocal = ilg.DeclareLocal(type, type.Name + "Instance");
                    ilg.LoadZeroValueIntoLocal(type, instanceLocal);
                    ilg.Load(instanceLocal);
                }
                else
                {
                    ilg.New(constructor);
                }
                ilg.ConvertValue(type, ilg.CurrentMethod.ReturnType);
                return (CreateInstanceDelegate)ilg.EndMethod();
            }

            internal InvokeDelegate GenerateInvokeDelegate(MethodInfo method, out int inputParameterCount, out int outputParameterCount)
            {
                bool requiresMemberAccess = MethodRequiresMemberAccess(method);

                this.ilg = new CodeGenerator();
                try
                {
                    this.ilg.BeginMethod("SyncInvoke" + method.Name, typeof(InvokeDelegate), requiresMemberAccess);
                }
                catch (SecurityException securityException)
                {
                    if (requiresMemberAccess && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        DiagnosticUtility.TraceHandledException(securityException, TraceEventType.Warning);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityException(SR.GetString(
                                    SR.PartialTrustServiceMethodNotVisible,
                                    method.DeclaringType.FullName,
                                    method.Name)));
                    }
                    else
                    {
                        throw;
                    }
                }

                ArgBuilder targetArg = ilg.GetArg(0);
                ArgBuilder inputParametersArg = ilg.GetArg(1);
                ArgBuilder outputParametersArg = ilg.GetArg(2);

                ParameterInfo[] parameters = method.GetParameters();
                LocalBuilder returnLocal = ilg.DeclareLocal(ilg.CurrentMethod.ReturnType, "returnParam");
                LocalBuilder[] parameterLocals = new LocalBuilder[parameters.Length];
                DeclareParameterLocals(parameters, parameterLocals);

                LoadInputParametersIntoLocals(parameters, parameterLocals, inputParametersArg, out inputParameterCount);
                LoadTarget(targetArg, method.ReflectedType);
                LoadParameters(parameters, parameterLocals);
                InvokeMethod(method, returnLocal);
                LoadOutputParametersIntoArray(parameters, parameterLocals, outputParametersArg, out outputParameterCount);

                ilg.Load(returnLocal);
                return (InvokeDelegate)this.ilg.EndMethod();
            }

            internal InvokeBeginDelegate GenerateInvokeBeginDelegate(MethodInfo method, out int inputParameterCount)
            {
                bool requiresMemberAccess = MethodRequiresMemberAccess(method);

                this.ilg = new CodeGenerator();
                try
                {
                    this.ilg.BeginMethod("AsyncInvokeBegin" + method.Name, typeof(InvokeBeginDelegate), requiresMemberAccess);
                }
                catch (SecurityException securityException)
                {
                    if (requiresMemberAccess && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        DiagnosticUtility.TraceHandledException(securityException, TraceEventType.Warning);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityException(SR.GetString(
                                    SR.PartialTrustServiceMethodNotVisible,
                                    method.DeclaringType.FullName,
                                    method.Name)));
                    }
                    else
                    {
                        throw;
                    }
                }

                ArgBuilder targetArg = ilg.GetArg(0);
                ArgBuilder inputParametersArg = ilg.GetArg(1);
                ArgBuilder callbackArg = ilg.GetArg(2);
                ArgBuilder stateArg = ilg.GetArg(3);

                ParameterInfo[] parameters = method.GetParameters();
                LocalBuilder returnLocal = ilg.DeclareLocal(ilg.CurrentMethod.ReturnType, "returnParam");
                LocalBuilder[] parameterLocals = new LocalBuilder[parameters.Length - 2];
                DeclareParameterLocals(parameters, parameterLocals);

                LoadInputParametersIntoLocals(parameters, parameterLocals, inputParametersArg, out inputParameterCount);
                LoadTarget(targetArg, method.ReflectedType);
                LoadParameters(parameters, parameterLocals);
                ilg.Load(callbackArg);
                ilg.Load(stateArg);
                InvokeMethod(method, returnLocal);

                ilg.Load(returnLocal);
                return (InvokeBeginDelegate)this.ilg.EndMethod();
            }

            internal InvokeEndDelegate GenerateInvokeEndDelegate(MethodInfo method, out int outputParameterCount)
            {
                bool requiresMemberAccess = MethodRequiresMemberAccess(method);

                this.ilg = new CodeGenerator();
                try
                {
                    this.ilg.BeginMethod("AsyncInvokeEnd" + method.Name, typeof(InvokeEndDelegate), requiresMemberAccess);
                }
                catch (SecurityException securityException)
                {
                    if (requiresMemberAccess && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                    {
                        DiagnosticUtility.TraceHandledException(securityException, TraceEventType.Warning);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new SecurityException(SR.GetString(
                                    SR.PartialTrustServiceMethodNotVisible,
                                    method.DeclaringType.FullName,
                                    method.Name)));
                    }
                    else
                    {
                        throw;
                    }
                }

                ArgBuilder targetArg = ilg.GetArg(0);
                ArgBuilder outputParametersArg = ilg.GetArg(1);
                ArgBuilder resultArg = ilg.GetArg(2);

                ParameterInfo[] parameters = method.GetParameters();
                LocalBuilder returnLocal = ilg.DeclareLocal(ilg.CurrentMethod.ReturnType, "returnParam");
                LocalBuilder[] parameterLocals = new LocalBuilder[parameters.Length - 1];
                DeclareParameterLocals(parameters, parameterLocals);

                LoadZeroValueInputParametersIntoLocals(parameters, parameterLocals);
                LoadTarget(targetArg, method.ReflectedType);
                LoadParameters(parameters, parameterLocals);
                ilg.Load(resultArg);
                InvokeMethod(method, returnLocal);
                LoadOutputParametersIntoArray(parameters, parameterLocals, outputParametersArg, out outputParameterCount);

                ilg.Load(returnLocal);
                return (InvokeEndDelegate)this.ilg.EndMethod();
            }

            void DeclareParameterLocals(ParameterInfo[] parameters, LocalBuilder[] parameterLocals)
            {
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    parameterLocals[i] = ilg.DeclareLocal(TypeLoader.GetParameterType(parameters[i]), "param" + i.ToString(CultureInfo.InvariantCulture));
                }
            }

            void LoadInputParametersIntoLocals(ParameterInfo[] parameters, LocalBuilder[] parameterLocals, ArgBuilder inputParametersArg, out int inputParameterCount)
            {
                inputParameterCount = 0;
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    if (ServiceReflector.FlowsIn(parameters[i]))
                    {
                        Type parameterType = parameterLocals[i].LocalType;
                        ilg.LoadArrayElement(inputParametersArg, inputParameterCount);
                        if (!parameterType.IsValueType)
                        {
                            ilg.ConvertValue(TypeOfObject, parameterType);
                            ilg.Store(parameterLocals[i]);
                        }
                        else
                        {
                            ilg.Dup();
                            ilg.If();
                            ilg.ConvertValue(TypeOfObject, parameterType);
                            ilg.Store(parameterLocals[i]);
                            ilg.Else();
                            ilg.Pop();
                            ilg.LoadZeroValueIntoLocal(parameterType, parameterLocals[i]);
                            ilg.EndIf();
                        }
                        inputParameterCount++;
                    }
                }
            }

            void LoadZeroValueInputParametersIntoLocals(ParameterInfo[] parameters, LocalBuilder[] parameterLocals)
            {
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    if (ServiceReflector.FlowsIn(parameters[i]))
                    {
                        ilg.LoadZeroValueIntoLocal(parameterLocals[i].LocalType, parameterLocals[i]);
                    }
                }
            }

            void LoadTarget(ArgBuilder targetArg, Type targetType)
            {
                ilg.Load(targetArg);
                ilg.ConvertValue(targetArg.ArgType, targetType);
                if (targetType.IsValueType)
                {
                    LocalBuilder targetLocal = ilg.DeclareLocal(targetType, "target");
                    ilg.Store(targetLocal);
                    ilg.LoadAddress(targetLocal);
                }
            }

            void LoadParameters(ParameterInfo[] parameters, LocalBuilder[] parameterLocals)
            {
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    if (parameters[i].ParameterType.IsByRef)
                        ilg.Ldloca(parameterLocals[i]);
                    else
                        ilg.Ldloc(parameterLocals[i]);
                }
            }

            void InvokeMethod(MethodInfo method, LocalBuilder returnLocal)
            {
                ilg.Call(method);
                if (method.ReturnType == typeof(void))
                    ilg.Load(null);
                else
                    ilg.ConvertValue(method.ReturnType, ilg.CurrentMethod.ReturnType);
                ilg.Store(returnLocal);
            }

            void LoadOutputParametersIntoArray(ParameterInfo[] parameters, LocalBuilder[] parameterLocals, ArgBuilder outputParametersArg, out int outputParameterCount)
            {
                outputParameterCount = 0;
                for (int i = 0; i < parameterLocals.Length; i++)
                {
                    if (ServiceReflector.FlowsOut(parameters[i]))
                    {
                        ilg.Load(outputParametersArg);
                        ilg.Load(outputParameterCount);
                        ilg.Load(parameterLocals[i]);
                        ilg.ConvertValue(parameterLocals[i].LocalType, TypeOfObject);
                        ilg.Stelem(TypeOfObject);
                        outputParameterCount++;
                    }
                }
            }

            static bool IsTypeVisible(Type t)
            {
                if (t.Module == typeof(InvokerUtil).Module)
                    return true;

                if (!t.IsVisible)
                    return false;

                foreach (Type genericType in t.GetGenericArguments())
                {
                    if (!genericType.IsGenericParameter && !IsTypeVisible(genericType))
                        return false;
                }

                return true;
            }

            static bool ConstructorRequiresMemberAccess(ConstructorInfo ctor)
            {
                return ctor != null && (!ctor.IsPublic || !IsTypeVisible(ctor.DeclaringType)) && ctor.Module != typeof(InvokerUtil).Module;
            }

            static bool MethodRequiresMemberAccess(MethodInfo method)
            {
                return method != null && (!method.IsPublic || !IsTypeVisible(method.DeclaringType)) && method.Module != typeof(InvokerUtil).Module;
            }
        }
    }
}
