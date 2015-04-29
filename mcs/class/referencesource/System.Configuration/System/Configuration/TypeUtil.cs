//------------------------------------------------------------------------------
// <copyright file="TypeUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace System.Configuration {

    internal static class TypeUtil {

        //
        // Since the config APIs were originally implemented in System.dll,
        // references to types without assembly names could be resolved to
        // System.dll in Everett. Emulate that behavior by trying to get the
        // type from System.dll
        //
        static private Type GetLegacyType(string typeString) {
            Type type = null;

            //
            // Ignore all exceptions, otherwise callers will get unexpected
            // exceptions not related to the original failure to load the
            // desired type.
            //
            try {
                Assembly systemAssembly = typeof(ConfigurationException).Assembly;
                type = systemAssembly.GetType(typeString, false);
            }
            catch {
            }

            return type;
        }

        //
        // Get the type specified by typeString. If it fails, try to retrieve it 
        // as a type from System.dll. If that fails,  return null or throw the original
        // exception as indicated by throwOnError.
        //
        static private Type GetTypeImpl(string typeString, bool throwOnError) {
            Type type = null;
            Exception originalException = null;

            try {
                type = Type.GetType(typeString, throwOnError);
            }
            catch (Exception e) {
                originalException = e;
            }

            if (type == null) {
                type = GetLegacyType(typeString);
                if (type == null && originalException != null) {
                    throw originalException;
                }
            }

            return type;
        }

        //
        // Ask the host to get the type specified by typeString. If it fails, try to retrieve it 
        // as a type from System.dll. If that fails, return null or throw the original
        // exception as indicated by throwOnError.
        //
        static internal Type GetTypeWithReflectionPermission(IInternalConfigHost host, string typeString, bool throwOnError) {
            Type type = null;
            Exception originalException = null;

            try {
                type = host.GetConfigType(typeString, throwOnError);
            }
            catch (Exception e) {
                originalException = e;
            }

            if (type == null) {
                type = GetLegacyType(typeString);
                if (type == null && originalException != null) {
                    throw originalException;
                }
            }

            return type;
        }

        static internal Type GetTypeWithReflectionPermission(string typeString, bool throwOnError) {
            return GetTypeImpl(typeString, throwOnError);
        }

        static internal T CreateInstance<T>(string typeString) {
            return CreateInstanceRestricted<T>(null, typeString);
        }

        static internal T CreateInstanceRestricted<T>(Type callingType, string typeString) {
            Type type = GetTypeImpl(typeString, true); // catch the errors and report them
            VerifyAssignableType(typeof(T), type, true /* throwOnError */);
            return (T)CreateInstanceRestricted(callingType, type);
        }

        [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.MemberAccess)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This assert is potentially dangerous and shouldn't be present but is necessary for back-compat.")]
        static internal object CreateInstanceWithReflectionPermission(Type type) {
            object result = Activator.CreateInstance(type, true); // create non-public types
            return result;
        }

        // This is intended to be similar to CreateInstanceWithReflectionPermission, but there is
        // an extra check to make sure that the calling type is allowed to access the target type.
        static internal object CreateInstanceRestricted(Type callingType, Type targetType) {
            if (CallerHasMemberAccessOrAspNetPermission()) {
                // If the caller asserts MemberAccess (or this is a full-trust AD),
                // access to any type in any assembly is allowed.
                return CreateInstanceWithReflectionPermission(targetType);
            }
            else {
                // This DynamicMethod is just a thin wrapper around Activator.CreateInstance, but it is
                // crafted to make the call site of CreateInstance look like it belongs to 'callingType'.
                // If the calling type cannot be determined, an AHDM will be used so Activator.CreateInstance
                // doesn't think System.Configuration.dll is the immediate caller.
                DynamicMethod dm = CreateDynamicMethod(callingType, returnType: typeof(object), parameterTypes: new Type[] { typeof(Type) });

                // type => Activator.CreateInstance(type, true)
                var ilGen = dm.GetILGenerator();
                ilGen.Emit(OpCodes.Ldarg_0); // stack = { type }
                ilGen.Emit(OpCodes.Ldc_I4_1); // stack = { type, TRUE }
                ilGen.Emit(OpCodes.Call, typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type), typeof(bool) })); // stack = { retVal }
                ilGen.Emit(OpCodes.Ret);
                var createInstanceDel = (Func<Type, object>)dm.CreateDelegate(typeof(Func<Type, object>));
                return createInstanceDel(targetType);
            }
        }

        // This is intended to be similar to Delegate.CreateDelegate, but there is
        // an extra check to make sure that the calling type is allowed to access the target method.
        static internal Delegate CreateDelegateRestricted(Type callingType, Type delegateType, MethodInfo targetMethod) {
            if (CallerHasMemberAccessOrAspNetPermission()) {
                // If the caller asserts MemberAccess (or this is a full-trust AD),
                // access to any type in any assembly is allowed. Note: the original
                // code path didn't assert before the call to CreateDelegate, so we
                // won't, either.
                return Delegate.CreateDelegate(delegateType, targetMethod);
            }
            else {
                // This DynamicMethod is just a thin wrapper around Delegate.CreateDelegate, but it is
                // crafted to make the call site of CreateInstance look like it belongs to 'callingType'.
                // If the calling type cannot be determined, an AHDM will be used so Activator.CreateInstance
                // doesn't think System.Configuration.dll is the immediate caller.
                DynamicMethod dm = CreateDynamicMethod(callingType, returnType: typeof(Delegate), parameterTypes: new Type[] { typeof(Type), typeof(MethodInfo) });

                // (type, method) => Delegate.CreateDelegate(type, method)
                var ilGen = dm.GetILGenerator();
                ilGen.Emit(OpCodes.Ldarg_0); // stack = { type }
                ilGen.Emit(OpCodes.Ldarg_1); // stack = { type, method }
                ilGen.Emit(OpCodes.Call, typeof(Delegate).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(MethodInfo) })); // stack = { retVal }
                ilGen.Emit(OpCodes.Ret);
                var createDelegateDel = (Func<Type, MethodInfo, Delegate>)dm.CreateDelegate(typeof(Func<Type, MethodInfo, Delegate>));
                return createDelegateDel(delegateType, targetMethod);
            }
        }

        private static DynamicMethod CreateDynamicMethod(Type owner, Type returnType, Type[] parameterTypes) {
            if (owner != null) {
                return CreateDynamicMethodWithUnrestrictedPermission(owner, returnType, parameterTypes);
            }
            else {
                // Don't assert when creating AHDM instances.
                return new DynamicMethod("temp-dynamic-method", returnType, parameterTypes);
            }
        }

        // Injecting a DynamicMethod into another module could end up demanding more than the grant set of the destination assembly,
        // so we'll need to assert full trust instead of just MemberAccess. This should be safe since simply creating the
        // DynamicMethod doesn't result in user code being run, and invocation of the method does not occur under any assert.
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static DynamicMethod CreateDynamicMethodWithUnrestrictedPermission(Type owner, Type returnType, Type[] parameterTypes) {
            Debug.Assert(owner != null);
            return new DynamicMethod("temp-dynamic-method", returnType, parameterTypes, owner);
        }

        static internal ConstructorInfo GetConstructorWithReflectionPermission(Type type, Type baseType, bool throwOnError) {
            type = VerifyAssignableType(baseType, type, throwOnError);
            if (type == null) {
                return null;
            }

            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            ConstructorInfo ctor = type.GetConstructor(bindingFlags, null, CallingConventions.HasThis, Type.EmptyTypes, null);
            if (ctor == null && throwOnError) {
                throw new TypeLoadException(SR.GetString(SR.TypeNotPublic, type.AssemblyQualifiedName));
            }

            return ctor;
        }

        [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.MemberAccess)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "A permission check is already performed by BaseConfigurationRecord.FindAndEnsureFactoryRecord before code execution reaches this point.")]
        static internal object InvokeCtorWithReflectionPermission(ConstructorInfo ctor) {
            return ctor.Invoke(null);
        }

        static internal bool IsTypeFromTrustedAssemblyWithoutAptca(Type type) {
            Assembly assembly = type.Assembly;
            return assembly.GlobalAssemblyCache && !HasAptcaBit(assembly);
        }

        static internal Type VerifyAssignableType(Type baseType, Type type, bool throwOnError) {
            if (baseType.IsAssignableFrom(type)) {
                return type;
            }

            if (throwOnError) {
                throw new TypeLoadException(
                    SR.GetString(SR.Config_type_doesnt_inherit_from_type, type.FullName, baseType.FullName));
            }

            return null;
        }

            private static bool HasAptcaBit(Assembly assembly) {
            Object[] attrs = assembly.GetCustomAttributes(
                typeof(System.Security.AllowPartiallyTrustedCallersAttribute), /*inherit*/ false);

            return (attrs != null && attrs.Length > 0);
        }

        static private volatile PermissionSet s_fullTrustPermissionSet;
        private static readonly ReflectionPermission s_memberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
        private static readonly AspNetHostingPermission s_aspNetHostingPermission = new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal);

        // Check if the caller is fully trusted
        static internal bool IsCallerFullTrust {
            get {
                bool isFullTrust = false;
                try {
                    if (s_fullTrustPermissionSet == null) {
                        s_fullTrustPermissionSet = new PermissionSet(PermissionState.Unrestricted);
                    }

                    s_fullTrustPermissionSet.Demand();
                    isFullTrust = true;
                }
                catch {
                }

                return isFullTrust;
            }
        }

        private static bool CallerHasMemberAccessOrAspNetPermission() {
            try {
                s_memberAccessPermission.Demand();
                return true;
            }
            catch (SecurityException) { }

            // ASP.NET partial trust scenarios don't have MemberAccess permission, but we want to allow
            // all reflection in these scenarios since ASP.NET partial trust isn't a security boundary.
            try {
                s_aspNetHostingPermission.Demand();
                return true;
            }
            catch (SecurityException) { }

            // Fallback: Missing permission and not hosted in ASP.NET
            return false;
        }

        // Check if the type is allowed to be used in config by checking the APTCA bit
        internal static bool IsTypeAllowedInConfig(Type t) {
            // Note:
            // This code is copied from HttpRuntime.IsTypeAllowedInConfig, but modified in
            // how it checks for fulltrust this can be called from non-ASP.NET apps.
            
            // Allow everything in full trust
            if (IsCallerFullTrust) {
                return true;
            }
            
            // The APTCA bit is only relevant for assemblies living in the GAC, since the rest runs
            // under partial trust (VSWhidbey 422183)
            Assembly assembly = t.Assembly;
            if (!assembly.GlobalAssemblyCache)
                return true;

            // If it has the APTCA bit, allow it
            if (HasAptcaBit(assembly))
                return true;
        
            // It's a GAC type without APTCA in partial trust scenario: block it
            return false;
        }
    }
}
