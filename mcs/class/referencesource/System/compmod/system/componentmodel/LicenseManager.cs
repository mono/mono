//------------------------------------------------------------------------------
// <copyright file="LicenseManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Diagnostics;
    using System;
    using System.Text;
    using System.Collections;
    using System.ComponentModel.Design;
    using Microsoft.Win32;
    using System.Security.Permissions;

    /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager"]/*' />
    /// <devdoc>
    ///    <para>Provides properties and methods to add a license
    ///       to a component and to manage a <see cref='System.ComponentModel.LicenseProvider'/>. This class cannot be inherited.</para>
    /// </devdoc>
    [HostProtection(ExternalProcessMgmt=true)]
    public sealed class LicenseManager {
        private static readonly object selfLock = new object();

        private static volatile LicenseContext context = null;
        private static object contextLockHolder = null;
        private static volatile Hashtable providers;
        private static volatile Hashtable providerInstances;
        private static object internalSyncObject = new object();

        // not creatable...
        //
        private LicenseManager() {
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.CurrentContext"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the current <see cref='System.ComponentModel.LicenseContext'/> which specifies when the licensed object can be
        ///       used.
        ///    </para>
        /// </devdoc>
        public static LicenseContext CurrentContext {
            get {
                if (context == null) {
                    lock(internalSyncObject) {
                        if (context == null) {
                            context = new System.ComponentModel.Design.RuntimeLicenseContext();
                        }
                    }
                }
                return context;
            }
            set {
                lock(internalSyncObject) {
                    if (contextLockHolder != null) {
                        throw new InvalidOperationException(SR.GetString(SR.LicMgrContextCannotBeChanged));
                    }
                    context = value;
                }
            }
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.UsageMode"]/*' />
        /// <devdoc>
        /// <para>Gets the <see cref='System.ComponentModel.LicenseUsageMode'/> that
        ///    specifies when the licensed object can be used, for the <see cref='System.ComponentModel.LicenseManager.CurrentContext'/>.</para>
        /// </devdoc>
        public static LicenseUsageMode UsageMode {
            get {
                if (context != null) {
                    return context.UsageMode;
                }
                return LicenseUsageMode.Runtime;
            }
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.CacheProvider"]/*' />
        /// <devdoc>
        ///     Caches the provider, both in the instance cache, and the type
        ///     cache.
        /// </devdoc>
        private static void CacheProvider(Type type, LicenseProvider provider) {
            if (providers == null) {
                providers = new Hashtable();
            }
            providers[type] = provider;

            if (provider != null) {
                if (providerInstances == null) {
                    providerInstances = new Hashtable();
                }
                providerInstances[provider.GetType()] = provider;
            }
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.CreateWithContext"]/*' />
        /// <devdoc>
        ///    <para>Creates an instance of the specified type, using 
        ///       creationContext
        ///       as the context in which the licensed instance can be used.</para>
        /// </devdoc>
        public static object CreateWithContext(Type type, LicenseContext creationContext) {
            return CreateWithContext(type, creationContext, new object[0]);
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.CreateWithContext1"]/*' />
        /// <devdoc>
        ///    <para>Creates an instance of the specified type with the 
        ///       specified arguments, using creationContext as the context in which the licensed
        ///       instance can be used.</para>
        /// </devdoc>
        public static object CreateWithContext(Type type, LicenseContext creationContext, object[] args) {
            object created = null;

            lock(internalSyncObject) {
                LicenseContext normal = CurrentContext;
                try {
                    CurrentContext = creationContext;
                    LockContext(selfLock);
                    try {
                        created = SecurityUtils.SecureCreateInstance(type, args);
                    }
                    catch (TargetInvocationException e) {
                        throw e.InnerException;
                    }
                }
                finally {
                    UnlockContext(selfLock);
                    CurrentContext = normal;
                }
            }

            return created;
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.GetCachedNoLicenseProvider"]/*' />
        /// <devdoc>
        ///     Determines if type was actually cached to have _no_ provider,
        ///     as opposed to not being cached.
        /// </devdoc>
        private static bool GetCachedNoLicenseProvider(Type type) {
            if (providers != null) {
                return providers.ContainsKey(type);
            }
            return false;
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.GetCachedProvider"]/*' />
        /// <devdoc>
        ///     Retrieves a cached instance of the provider associated with the
        ///     specified type.
        /// </devdoc>
        private static LicenseProvider GetCachedProvider(Type type) {
            if (providers != null) {
                return(LicenseProvider)providers[type];
            }
            return null;
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.GetCachedProviderInstance"]/*' />
        /// <devdoc>
        ///     Retrieves a cached instance of the provider of the specified
        ///     type.
        /// </devdoc>
        private static LicenseProvider GetCachedProviderInstance(Type providerType) {
            Debug.Assert(providerType != null, "Type cannot ever be null");
            if (providerInstances != null) {
                return(LicenseProvider)providerInstances[providerType];
            }
            return null;
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.GetLicenseInteropHelperType"]/*' />
        /// <devdoc>
        ///     Retrieves the typehandle of the interop helper
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static IntPtr GetLicenseInteropHelperType() {
            return typeof(LicenseInteropHelper).TypeHandle.Value;
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.IsLicensed"]/*' />
        /// <devdoc>
        ///    <para>Determines if the given type has a valid license or not.</para>
        /// </devdoc>
        public static bool IsLicensed(Type type) {
            Debug.Assert(type != null, "IsValid Type cannot ever be null");
            License license;
            bool value = ValidateInternal(type, null, false, out license);
            if (license != null) {
                license.Dispose();
                license = null;
            }
            return value;
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.IsValid"]/*' />
        /// <devdoc>
        ///    <para>Determines if a valid license can be granted for the specified type.</para>
        /// </devdoc>
        public static bool IsValid(Type type) {
            Debug.Assert(type != null, "IsValid Type cannot ever be null");
            License license;
            bool value = ValidateInternal(type, null, false, out license);
            if (license != null) {
                license.Dispose();
                license = null;
            }
            return value;
        }


        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.IsValid1"]/*' />
        /// <devdoc>
        ///    <para>Determines if a valid license can be granted for the 
        ///       specified instance of the type. This method creates a valid <see cref='System.ComponentModel.License'/>. </para>
        /// </devdoc>
        public static bool IsValid(Type type, object instance, out License license) {
            return ValidateInternal(type, instance, false, out license);
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.LockContext"]/*' />
        /// <devdoc>
        /// </devdoc>
        public static void LockContext(object contextUser) {
            lock(internalSyncObject) {
                if (contextLockHolder != null) {
                    throw new InvalidOperationException(SR.GetString(SR.LicMgrAlreadyLocked));
                }
                contextLockHolder = contextUser;
            }
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.UnlockContext"]/*' />
        /// <devdoc>
        /// </devdoc>
        public static void UnlockContext(object contextUser) {
            lock(internalSyncObject) {
                if (contextLockHolder != contextUser) {
                    throw new ArgumentException(SR.GetString(SR.LicMgrDifferentUser));
                }
                contextLockHolder = null;
            }
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.ValidateInternal"]/*' />
        /// <devdoc>
        ///     Internal validation helper.
        /// </devdoc>
        private static bool ValidateInternal(Type type, object instance, bool allowExceptions, out License license) {
            string licenseKey;
            return ValidateInternalRecursive(CurrentContext, 
                                             type, 
                                             instance, 
                                             allowExceptions, 
                                             out license,
                                             out licenseKey);
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.ValidateInternalRecursive"]/*' />
        /// <devdoc>
        ///     Since we want to walk up the entire inheritance change, when not 
        ///     give an instance, we need another helper method to walk up
        ///     the chain...
        /// </devdoc>
        private static bool ValidateInternalRecursive(LicenseContext context, Type type, object instance, bool allowExceptions, out License license, out string licenseKey) {
            LicenseProvider provider = GetCachedProvider(type);
            if (provider == null && !GetCachedNoLicenseProvider(type)) {
                // NOTE : Must look directly at the class, we want no inheritance.
                //
                LicenseProviderAttribute attr = (LicenseProviderAttribute)Attribute.GetCustomAttribute(type, typeof(LicenseProviderAttribute), false);
                if (attr != null) {
                    Type providerType = attr.LicenseProvider;
                    provider = GetCachedProviderInstance(providerType);

                    if (provider == null) {
                        provider = (LicenseProvider)SecurityUtils.SecureCreateInstance(providerType);
                    }
                }

                CacheProvider(type, provider);
            }

            license = null;
            bool isValid = true;

            licenseKey = null;
            if (provider != null) {
                license = provider.GetLicense(context, type, instance, allowExceptions);
                if (license == null) {
                    isValid = false;
                }
                else {
                    // For the case where a COM client is calling "RequestLicKey", 
                    // we try to squirrel away the first found license key
                    //
                    licenseKey = license.LicenseKey;
                }
            }

            // When looking only at a type, we need to recurse up the inheritence
            // chain, however, we can't give out the license, since this may be
            // from more than one provider.
            //
            if (isValid && instance == null) {
                Type baseType = type.BaseType;
                if (baseType != typeof(object) && baseType != null) {
                    if (license != null) {
                        license.Dispose();
                        license = null;
                    }
                    string temp;
                    isValid = ValidateInternalRecursive(context, baseType, null, allowExceptions, out license, out temp);
                    if (license != null) {
                        license.Dispose();
                        license = null;
                    }
                }
            }

            return isValid;
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.Validate"]/*' />
        /// <devdoc>
        ///    <para>Determines if a license can be granted for the specified type.</para>
        /// </devdoc>
        public static void Validate(Type type) {
            License lic;

            if (!ValidateInternal(type, null, true, out lic)) {
                throw new LicenseException(type);
            }

            if (lic != null) {
                lic.Dispose();
                lic = null;
            }
        }

        /// <include file='doc\LicenseManager.uex' path='docs/doc[@for="LicenseManager.Validate1"]/*' />
        /// <devdoc>
        ///    <para>Determines if a license can be granted for the instance of the specified type.</para>
        /// </devdoc>
        public static License Validate(Type type, object instance) {
            License lic;

            if (!ValidateInternal(type, instance, true, out lic)) {
                throw new LicenseException(type, instance);
            }

            return lic;
        }

        // FxCop complaint about uninstantiated internal classes
        // Re-activate if this class proves to be useful.

        // This is a helper class that supports the CLR's IClassFactory2 marshaling
        // support.
        //
        // When a managed object is exposed to COM, the CLR invokes
        // AllocateAndValidateLicense() to set up the appropriate
        // license context and instantiate the object.
        //
        // When the CLR consumes an unmanaged COM object, the CLR invokes
        // GetCurrentContextInfo() to figure out the licensing context
        // and decide whether to call ICF::CreateInstance() (designtime) or
        // ICF::CreateInstanceLic() (runtime). In the former case, it also
        // requests the class factory for a runtime license key and invokes
        // SaveKeyInCurrentContext() to stash a copy in the current licensing
        // context
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        class LicenseInteropHelper {

            // Define some common HRESULTs.
            const int S_OK = 0;
            const int E_NOTIMPL = unchecked((int)0x80004001); 
            const int CLASS_E_NOTLICENSED = unchecked((int)0x80040112);
            const int E_FAIL              = unchecked((int)0x80000008);

            DesigntimeLicenseContext helperContext;
            LicenseContext    savedLicenseContext;
            Type              savedType;

            // The CLR invokes this whenever a COM client invokes
            // IClassFactory::CreateInstance() or IClassFactory2::CreateInstanceLic()
            // on a managed managed that has a LicenseProvider custom attribute.
            //
            // If we are being entered because of a call to ICF::CreateInstance(),
            // fDesignTime will be "true".
            //
            // If we are being entered because of a call to ICF::CreateInstanceLic(),
            // fDesignTime will be "false" and bstrKey will point a non-null
            // license key.
            private static object AllocateAndValidateLicense(RuntimeTypeHandle rth, IntPtr bstrKey, int fDesignTime) {
                Type type = Type.GetTypeFromHandle(rth);
                CLRLicenseContext licensecontext = new CLRLicenseContext(fDesignTime != 0 ? LicenseUsageMode.Designtime : LicenseUsageMode.Runtime, type);
                if (fDesignTime == 0 && bstrKey != (IntPtr)0) {
                    licensecontext.SetSavedLicenseKey(type, Marshal.PtrToStringBSTR(bstrKey));
                }


                try {
                    return LicenseManager.CreateWithContext(type, licensecontext);
                }
                catch (LicenseException lexp) {
                    throw new COMException(lexp.Message, CLASS_E_NOTLICENSED);
                }
            }

            // The CLR invokes this whenever a COM client invokes
            // IClassFactory2::RequestLicKey on a managed class.
            //
            // This method should return the appropriate HRESULT and set pbstrKey
            // to the licensing key.
            private static int RequestLicKey(RuntimeTypeHandle rth, ref IntPtr pbstrKey) {
                Type type = Type.GetTypeFromHandle(rth);
                License license;
                string licenseKey;

                // license will be null, since we passed no instance,
                // however we can still retrieve the "first" license
                // key from the file. This really will only
                // work for simple COM-compatible license providers
                // like LicFileLicenseProvider that don't require the
                // instance to grant a key.
                //
                if (!LicenseManager.ValidateInternalRecursive(LicenseManager.CurrentContext, 
                                                              type, 
                                                              null, 
                                                              false, 
                                                              out license, 
                                                              out licenseKey)) {
                    return E_FAIL;
                }

                if (licenseKey == null) {
                    return E_FAIL;
                }

                pbstrKey = Marshal.StringToBSTR(licenseKey);

                if (license != null) {
                    license.Dispose();
                    license = null;
                }

                return S_OK;

            }


            // The CLR invokes this whenever a COM client invokes
            // IClassFactory2::GetLicInfo on a managed class.
            //
            // COM normally doesn't expect this function to fail so this method
            // should only throw in the case of a catastrophic error (stack, memory, etc.)
            private void GetLicInfo(RuntimeTypeHandle rth, ref int pRuntimeKeyAvail, ref int pLicVerified) {
                pRuntimeKeyAvail = 0;
                pLicVerified = 0;

                Type type = Type.GetTypeFromHandle(rth);
                License license;
                string licenseKey;

                if (helperContext == null) {
                    helperContext = new DesigntimeLicenseContext();
                }
                else {
                    helperContext.savedLicenseKeys.Clear();
                }

                if (LicenseManager.ValidateInternalRecursive(helperContext, type, null, false, out license, out licenseKey)) {

                    if (helperContext.savedLicenseKeys.Contains(type.AssemblyQualifiedName)) {
                        pRuntimeKeyAvail = 1;
                    }

                    if (license != null) {
                        license.Dispose();
                        license = null;

                        pLicVerified = 1;
                    }
                }
            }

            // The CLR invokes this when instantiating an unmanaged COM
            // object. The purpose is to decide which classfactory method to
            // use.
            //
            // If the current context is design time, the CLR will
            // use ICF::CreateInstance().
            //
            // If the current context is runtime and the current context
            // exposes a non-null license key and the COM object supports
            // IClassFactory2, the CLR will use ICF2::CreateInstanceLic().
            // Otherwise, the CLR will use ICF::CreateInstance.
            //
            // Arguments:
            //    ref int fDesignTime:   on exit, this will be set to indicate
            //                           the nature of the current license context.
            //    ref int bstrKey:       on exit, this will point to the
            //                           licensekey saved inside the license context.
            //                           (only if the license context is runtime)
            //    RuntimeTypeHandle rth: the managed type of the wrapper
            private void GetCurrentContextInfo(ref int fDesignTime, ref IntPtr bstrKey, RuntimeTypeHandle rth) {
                this.savedLicenseContext = LicenseManager.CurrentContext;
                this.savedType = Type.GetTypeFromHandle(rth);
                if (this.savedLicenseContext.UsageMode == LicenseUsageMode.Designtime) {
                    fDesignTime = 1;
                    bstrKey = (IntPtr)0;
                }
                else {
                    fDesignTime = 0;
                    String key = this.savedLicenseContext.GetSavedLicenseKey(this.savedType, null);
                    bstrKey = Marshal.StringToBSTR(key);

                }
            }

            // The CLR invokes this when instantiating a licensed COM
            // object inside a designtime license context.
            // It's purpose is to save away the license key that the CLR
            // retrieved using RequestLicKey(). This license key can be NULL.
            private void SaveKeyInCurrentContext(IntPtr bstrKey) {
                if (bstrKey != (IntPtr)0) {
                    this.savedLicenseContext.SetSavedLicenseKey(this.savedType, Marshal.PtrToStringBSTR(bstrKey));
                }
            }

            // A private implementation of a LicenseContext used for instantiating
            // managed objects exposed to COM. It has memory for the license key
            // of a single Type.
            internal class CLRLicenseContext : LicenseContext {
                LicenseUsageMode  usageMode;
                Type              type;
                string            key;

                public CLRLicenseContext(LicenseUsageMode usageMode, Type type) {
                    this.usageMode = usageMode;
                    this.type      = type;
                }

                public override LicenseUsageMode UsageMode
                {
                    get {
                        return this.usageMode;
                    }
                }


                public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly) {
                    return type == this.type ? this.key : null;
                }
                public override void SetSavedLicenseKey(Type type, string key) {
                    if (type == this.type) {
                        this.key = key;
                    }
                }
            }
        }
    }
}
