// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;

namespace System.Xaml.Schema
{
    /// <SecurityNote>
    /// All invocation of user-provided reflection objects inside System.Xaml should be routed through this class.
    ///
    /// Invoking a reflection object directly from this class (or any class in System.Xaml)
    /// could potentially expose internal types or methods in System.Xaml, because
    /// mscorlib sees the invocation as coming from System.Xaml.  Instead, we do
    /// the invocation from a dynamic assembly created for this purpose.  Because
    /// the wrapper methods live in a separate assembly (with no internals) and are
    /// marked as security-transparent, the security checks in mscorlib will treat
    /// attempts to use internals of System.xaml the same as it treats attempts to
    /// use internals of any other assembly; i.e. the caller (that is, the user's
    /// application) must have the appropriate permissions.
    ///
    /// This class exposes proxy methods for each of the reflection methods we need.
    /// The static constructor creates the dynamic assembly and populates it with
    /// the wrapper methods that actually call into reflection code.  The proxy
    /// methods in this class merely call the corresponding dynamic wrapper methods.
    ///
    /// The dynamic assembly technique turns out to have perf implications - it loads
    /// parts of mscorlib and clr that are otherwise unneeded (for Reflection.Emit et al.),
    /// and requires JIT compilation of the resulting IL.  We don't need the elaborate
    /// technique in full-trust scenarios (a malicious full-trust app can already
    /// invoke internals just by calling reflection directly).  So in full-trust we
    /// use the old code.
    /// </SecurityNote>
    static class SafeReflectionInvoker
    {
#if PARTIALTRUST
        private static bool s_UseDynamicAssembly = false;   // true when we use the dynamic assembly approach (i.e. in partial-trust)
        private static object lockObject = new Object();    // for synchronizing the assembly-building step

        // delegate types for the wrapping methods
        private delegate Delegate CreateDelegate1Delegate(Type delegateType, Type targetType, string methodName);
        private delegate Delegate CreateDelegate2Delegate(Type delegateType, object target, string methodName);
        private delegate object CreateInstanceDelegate(Type type, object[] arguments);
        private delegate object InvokeMethodDelegate(MethodInfo method, object instance, object[] args);

        // wrapping delegates
        private static CreateDelegate1Delegate s_CreateDelegate1;
        private static CreateDelegate2Delegate s_CreateDelegate2;
        private static CreateInstanceDelegate s_CreateInstance;
        private static InvokeMethodDelegate s_InvokeMethod;

        /// <SecurityNote>
        ///     Critical:  calls critical method CreateDynamicAssembly
        /// </SecurityNote
        [SecurityCritical]
        private static bool UseDynamicAssembly()
        {
            // Use the dynamic assembly technique as soon as any call occurs in partial-trust
            // (and thereafter).
            if (!s_UseDynamicAssembly)
            {
                bool useDynamicAssembly = false;

                try
                {
                    PermissionSet fullTrustPermissionSet = new PermissionSet(PermissionState.Unrestricted);
                    fullTrustPermissionSet.Demand();
                }
                catch (SecurityException)
                {
                    useDynamicAssembly = true;
                }

                // the first time we're called in partial-trust, we have to emit the dynamic methods.
                if (useDynamicAssembly)
                {
                    // multiple threads can reach this point more or less simultaneously.
                    // Ensure that exactly one of them creates the assembly, and that
                    // the assembly is ready for use by all of them before they proceed.
                    lock(lockObject)
                    {
                        if (!s_UseDynamicAssembly)
                        {
                            CreateDynamicAssembly();
                            s_UseDynamicAssembly = true;
                        }
                    }
                }
            }

            return s_UseDynamicAssembly;
        }

        /// <SecurityNote>
        ///     Critical:  creates dynamic methods
        /// </SecurityNote
        /// <PerfNote>
        ///     NoInline|NoOpt : don't request Reflection.Emit code from mscorlib until we really need it
        /// </PerfNote>
        [SecurityCritical]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
                                                    System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        private static void CreateDynamicAssembly()
        {
            // 1. Assert permissions demanded by the DynamicMethod ctor.
            new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Assert(); // BlessedAssert

            // 2. Create the transparent methods, each wrapping a call to a reflection method,
            //    and cache a delegate to each method.
            Type[] parameterTypes;      // signature of the reflection method
            Type[] wrappedParameterTypes; // signature of the wrapping method (when different)
            MethodInfo mi;              // descriptor for the reflection method
            DynamicMethod method;       // wrapping method
            ILGenerator il;             // wrapping method's generator

            // 2a. Delegate.CreateDelegate( Type, Type, String )
            parameterTypes = new Type[] { typeof(Type), typeof(Type), typeof(String) };
            mi = typeof(Delegate).GetMethod("CreateDelegate", parameterTypes);

            method = new DynamicMethod( "CreateDelegate", typeof(Delegate), parameterTypes );
            method.DefineParameter(1, ParameterAttributes.In, "delegateType");
            method.DefineParameter(2, ParameterAttributes.In, "targetType");
            method.DefineParameter(3, ParameterAttributes.In, "methodName");

            il = method.GetILGenerator(5);
            il.Emit(OpCodes.Ldarg_0);               // push delegateType
            il.Emit(OpCodes.Ldarg_1);               // push targetType
            il.Emit(OpCodes.Ldarg_2);               // push methodName
            il.EmitCall(OpCodes.Call, mi, null);    // call Delegate.CreateDelegate
            il.Emit(OpCodes.Ret);                   // return the result

            s_CreateDelegate1 = (CreateDelegate1Delegate)method.CreateDelegate(typeof(CreateDelegate1Delegate));

            // 2b. Delegate.CreateDelegate( Type, Object, String )
            parameterTypes = new Type[] { typeof(Type), typeof(Object), typeof(String) };
            mi = typeof(Delegate).GetMethod("CreateDelegate", parameterTypes);

            method = new DynamicMethod( "CreateDelegate", typeof(Delegate), parameterTypes );
            method.DefineParameter(1, ParameterAttributes.In, "delegateType");
            method.DefineParameter(2, ParameterAttributes.In, "target");
            method.DefineParameter(3, ParameterAttributes.In, "methodName");

            il = method.GetILGenerator(5);
            il.Emit(OpCodes.Ldarg_0);               // push delegateType
            il.Emit(OpCodes.Ldarg_1);               // push target
            il.Emit(OpCodes.Ldarg_2);               // push methodName
            il.EmitCall(OpCodes.Call, mi, null);    // call Delegate.CreateDelegate
            il.Emit(OpCodes.Ret);                   // return the result

            s_CreateDelegate2 = (CreateDelegate2Delegate)method.CreateDelegate(typeof(CreateDelegate2Delegate));

            // 2c. Activator.CreateInstance( Type, Object[] )
            parameterTypes = new Type[] { typeof(Type), typeof(Object[]) };
            mi = typeof(Activator).GetMethod("CreateInstance", parameterTypes);

            method = new DynamicMethod( "CreateInstance", typeof(Object), parameterTypes );
            method.DefineParameter(1, ParameterAttributes.In, "type");
            method.DefineParameter(2, ParameterAttributes.In, "arguments");

            il = method.GetILGenerator(4);
            il.Emit(OpCodes.Ldarg_0);               // push type
            il.Emit(OpCodes.Ldarg_1);               // push arguments
            il.EmitCall(OpCodes.Call, mi, null);    // call Activator.CreateInstance
            il.Emit(OpCodes.Ret);                   // return the result

            s_CreateInstance = (CreateInstanceDelegate)method.CreateDelegate(typeof(CreateInstanceDelegate));

            // 2d. MethodInfo.Invoke(object, args)
            parameterTypes = new Type[] { typeof(Object), typeof(Object[]) };
            wrappedParameterTypes = new Type[] { typeof(MethodInfo), typeof(Object), typeof(Object[]) };
            mi = typeof(MethodInfo).GetMethod("Invoke", parameterTypes);

            method = new DynamicMethod( "InvokeMethod", typeof(Object), wrappedParameterTypes );
            method.DefineParameter(1, ParameterAttributes.In, "method");
            method.DefineParameter(2, ParameterAttributes.In, "instance");
            method.DefineParameter(3, ParameterAttributes.In, "args");

            il = method.GetILGenerator(5);
            il.Emit(OpCodes.Ldarg_0);               // push method
            il.Emit(OpCodes.Ldarg_1);               // push instance
            il.Emit(OpCodes.Ldarg_2);               // push args
            il.EmitCall(OpCodes.Callvirt, mi, null); // call method.Invoke
            il.Emit(OpCodes.Ret);                   // return the result

            s_InvokeMethod = (InvokeMethodDelegate)method.CreateDelegate(typeof(InvokeMethodDelegate));
        }
#endif

        // vvvvv---- Unused members.  Servicing policy is to retain these anyway.  -----vvvvv
        /// <SecurityNote>
        /// Demanded to allow instantiation of System.Xaml internals
        /// </SecurityNote>
        [SecurityCritical]
        private static ReflectionPermission s_reflectionMemberAccess;

        static readonly Assembly SystemXaml = typeof(SafeReflectionInvoker).Assembly;

        /// <SecurityNote>
        /// Critical: Used to detect luring attack described in class-level comments.
        /// Safe: Gets the information from reflection.
        /// </SecurityNote>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Retained per servicing policy.")]
        public static bool IsInSystemXaml(Type type)
        {
            if (type.Assembly == SystemXaml)
            {
                return true;
            }
            if (type.IsGenericType)
            {
                foreach (Type typeArg in type.GetGenericArguments())
                {
                    if (IsInSystemXaml(typeArg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        // ^^^^^----- End of unused members.  -----^^^^^

        /// <SecurityNote>
        /// Critical: See class-level comment
        /// Safe: See class-level comment
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal static Delegate CreateDelegate(Type delegateType, Type targetType, string methodName)
        {
#if PARTIALTRUST
            return UseDynamicAssembly() ? s_CreateDelegate1(delegateType, targetType, methodName)
                                        : CreateDelegateCritical(delegateType, targetType, methodName);
#else
            return CreateDelegateCritical(delegateType, targetType, methodName);
#endif
        }

        /// <SecurityNote>
        /// This method doesn't do security checks, it should be treated as critical.
        /// The reason it's not marked as critical is so that it doesn't satisfy a SecurityCritical
        /// requirement on the target method.
        /// The reason it's marked NoInlining|NoOptimization is so that the call
        /// isn't optimized back into a critical caller.
        /// </SecurityNote>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
                                                    System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal static Delegate CreateDelegateCritical(Type delegateType, Type targetType, string methodName)
        {
            return Delegate.CreateDelegate(delegateType, targetType, methodName);
        }

        /// <SecurityNote>
        /// Critical: See class-level comment
        /// Safe: See class-level comment
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal static Delegate CreateDelegate(Type delegateType, object target, string methodName)
        {
#if PARTIALTRUST
            return UseDynamicAssembly() ? s_CreateDelegate2(delegateType, target, methodName)
                                        : CreateDelegateCritical(delegateType, target, methodName);
#else
            return CreateDelegateCritical(delegateType, target, methodName);
#endif
        }

        /// <SecurityNote>
        /// This method doesn't do security checks, it should be treated as critical.
        /// The reason it's not marked as critical is so that it doesn't satisfy a SecurityCritical
        /// requirement on the target method.
        /// The reason it's marked NoInlining|NoOptimization is so that the call
        /// isn't optimized back into a critical caller.
        /// </SecurityNote>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
                                                    System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal static Delegate CreateDelegateCritical(Type delegateType, object target, string methodName)
        {
            return Delegate.CreateDelegate(delegateType, target, methodName);
        }

        /// <SecurityNote>
        /// Critical: See class-level comment
        /// Safe: See class-level comment. Note that this checks the UnderlyingSystemType,
        ///       which is what is actually created by Activator.CreateInstance.
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal static object CreateInstance(Type type, object[] arguments)
        {
#if PARTIALTRUST
            return UseDynamicAssembly() ? s_CreateInstance(type, arguments)
                                        : CreateInstanceCritical(type, arguments);
#else
            return CreateInstanceCritical(type, arguments);
#endif
        }

        /// <SecurityNote>
        /// This method doesn't do security checks, it should be treated as critical.
        /// The reason it's not marked as critical is so that it doesn't satisfy a SecurityCritical
        /// requirement on the target of the invocation.
        /// The reason it's marked NoInlining|NoOptimization is so that the call
        /// isn't optimized back into a critical caller.
        /// CLR is currently planning a change to turn all reflection invocation of SecurityCritical
        /// members into demands for full trust; if that change goes through, then this method will
        /// become unnecessary.
        /// </SecurityNote>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
                                                    System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal static object CreateInstanceCritical(Type type, object[] arguments)
        {
            return Activator.CreateInstance(type, arguments);
        }

        // vvvvv---- Unused members.  Servicing policy is to retain these anyway.  -----vvvvv
        /// <SecurityNote>
        /// Critical: Sets critical field s_reflectionMemberAccess
        /// Safe: Sets the field to a known good value
        /// </SecurityNote>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Retained per servicing policy.")]
        internal static void DemandMemberAccessPermission()
        {
            if (s_reflectionMemberAccess == null)
            {
                s_reflectionMemberAccess = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
            }
            s_reflectionMemberAccess.Demand();
        }
        // ^^^^^----- End of unused members.  -----^^^^^

        /// <SecurityNote>
        /// Critical: See class-level comment
        /// Safe: See class-level comment
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal static object InvokeMethod(MethodInfo method, object instance, object[] args)
        {
#if PARTIALTRUST
            return UseDynamicAssembly() ? s_InvokeMethod(method, instance, args)
                                        : InvokeMethodCritical(method, instance, args);
#else
            return InvokeMethodCritical(method, instance, args);
#endif
        }

        /// <SecurityNote>
        /// This method doesn't do security checks, it should be treated as critical.
        /// The reason it's not marked as critical is so that it doesn't satisfy a SecurityCritical
        /// requirement on the target of the invocation.
        /// The reason it's marked NoInlining|NoOptimization is so that the call
        /// isn't optimized back into a critical caller.
        /// CLR is currently planning a change to turn all reflection invocation of SecurityCritical
        /// members into demands for full trust; if that change goes through, then this method will
        /// become unnecessary.
        /// </SecurityNote>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining |
                                                    System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal static object InvokeMethodCritical(MethodInfo method, object instance, object[] args)
        {
            return method.Invoke(instance, args);
        }

        // vvvvv---- Unused members.  Servicing policy is to retain these anyway.  -----vvvvv
        /// <SecurityNote>
        /// Critical: Used to detect luring attack described in class-level comments.
        /// Safe: Gets the information from reflection.
        ///       The MethodInfo (and MemberInfo) have an InheritanceDemand so derived class
        ///       spoofing in PT is not an issue.
        /// Note: The [SecurityCritical] attribute isn't functionally necessary but flags the
        ///       method as security critical and changes should be reviewed.
        /// </SecurityNote>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Retained per servicing policy.")]
        internal static bool IsSystemXamlNonPublic(MethodInfo method)
        {
            Type declaringType = method.DeclaringType;
            if (IsInSystemXaml(declaringType) && (!method.IsPublic || !declaringType.IsVisible))
            {
                return true;
            }
            if (method.IsGenericMethod)
            {
                foreach (Type typeArg in method.GetGenericArguments())
                {
                    if (IsInSystemXaml(typeArg) && !typeArg.IsVisible)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        // ^^^^^----- End of unused members.  -----^^^^^
    }
}
