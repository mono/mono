//------------------------------------------------------------------------------
// <copyright file="CustomLoaderHelper.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Versioning;

    // Used to locate a custom loader implementation within a bin-deployed assembly.

    internal sealed class CustomLoaderHelper : MarshalByRefObject {

        // the first framework version where the custom loader feature was implemented
        private static readonly string _customLoaderTargetFrameworkName = new FrameworkName(".NETFramework", new Version(4, 5, 1)).ToString();

        private static readonly string _customLoaderAssemblyName = typeof(CustomLoaderHelper).Assembly.FullName;
        private static readonly string _customLoaderTypeName = typeof(CustomLoaderHelper).FullName;
        private static readonly Guid IID_ICustomLoader = new Guid("50A3CE65-2F9F-44E9-9094-32C6C928F966");

        // Instances of this type should only ever be created via reflection (see call to CreateObjectAndUnwrap
        // in GetCustomLoader).
        private CustomLoaderHelper() { }

        internal static IObjectHandle GetCustomLoader(ICustomLoaderHelperFunctions helperFunctions, string appConfigMetabasePath, string configFilePath, string customLoaderPhysicalPath, out AppDomain newlyCreatedAppDomain) {
            // Step 1: Does the host allow custom loaders?

            bool? customLoaderIsEnabled = helperFunctions.CustomLoaderIsEnabled;
            if (customLoaderIsEnabled.HasValue) {
                if ((bool)customLoaderIsEnabled) {
                    // The custom loader is enabled; move on to the next step.
                }
                else {
                    // The custom loader is disabled, fail.
                    throw new NotSupportedException(ApplicationServicesStrings.CustomLoader_ForbiddenByHost);
                }
            }
            else {
                // The host hasn't set a policy, so we'll fall back to our default logic of checking the application's trust level.
                if (!IsFullyTrusted(helperFunctions, appConfigMetabasePath)) {
                    throw new NotSupportedException(ApplicationServicesStrings.CustomLoader_NotInFullTrust);
                }
            }

            // Step 2: Create the new AD

            string binFolderPhysicalPath = helperFunctions.MapPath("/bin/");

            AppDomainSetup setup = new AppDomainSetup() {
                PrivateBinPathProbe = "*",  // disable loading from app base
                PrivateBinPath = binFolderPhysicalPath,
                ApplicationBase = helperFunctions.AppPhysicalPath,
                TargetFrameworkName = _customLoaderTargetFrameworkName
            };

            if (configFilePath != null) {
                setup.ConfigurationFile = configFilePath;
            }

            AppDomain newAppDomainForCustomLoader = AppDomain.CreateDomain("aspnet-custom-loader-" + Guid.NewGuid(), null, setup);
            try {
                // Step 3: Instantiate helper in new AD so that we can get a reference to the loader
                CustomLoaderHelper helper = (CustomLoaderHelper)newAppDomainForCustomLoader.CreateInstanceAndUnwrap(_customLoaderAssemblyName, _customLoaderTypeName,
                    ignoreCase: false,
                    bindingAttr: BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance,
                    binder: null,
                    args: null,
                    culture: null,
                    activationAttributes: null);
                ObjectHandle ohCustomLoader = helper.GetCustomLoaderImpl(customLoaderPhysicalPath);

                // If we got this far, success!
                newlyCreatedAppDomain = newAppDomainForCustomLoader;
                return ohCustomLoader;
            }
            catch {
                // If something went wrong, kill the new AD.
                AppDomain.Unload(newAppDomainForCustomLoader);
                throw;
            }
        }

        private ObjectHandle GetCustomLoaderImpl(string customLoaderPhysicalPath) {
            // Step 4: Find the implementation in the custom loader assembly

            // Since we have set the private bin path, we can use this call to Assembly.Load
            // to avoid the load-from context, which has weird behaviors.
            AssemblyName customLoaderAssemblyName = AssemblyName.GetAssemblyName(customLoaderPhysicalPath);
            Assembly customLoaderAssembly = Assembly.Load(customLoaderAssemblyName);
            CustomLoaderAttribute customLoaderAttribute = customLoaderAssembly.GetCustomAttribute<CustomLoaderAttribute>();

            if (customLoaderAttribute == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ApplicationServicesStrings.CustomLoader_NoAttributeFound, customLoaderAssemblyName));
            }

            // Step 5: Instantiate the custom loader and return a reference back to native code

            object customLoader = Activator.CreateInstance(customLoaderAttribute.CustomLoaderType);

            // This check isn't strictly necessary since the unmanaged layer will handle QueryInterface failures
            // appropriately, but we have an opportunity to provide a better error message at this layer.
            if (!ObjectImplementsComInterface(customLoader, IID_ICustomLoader)) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ApplicationServicesStrings.CustomLoader_MustImplementICustomLoader, customLoader.GetType()));
            }

            return new ObjectHandle(customLoader);
        }

        private static bool IsFullyTrusted(ICustomLoaderHelperFunctions helperFunctions, string appConfigMetabasePath) {
            // The managed configuration system hasn't yet been instantiated but the IIS native config system understands
            // ASP.NET configuration and honors hierarchy and section locking.

            try {
                // Must exactly match <trust level="Full" />, as this is what ApplicationManager expects.
                string trustLevel = helperFunctions.GetTrustLevel(appConfigMetabasePath);
                return String.Equals("Full", trustLevel, StringComparison.Ordinal);
            }
            catch {
                // If any of the sections are locked or there is a config error, bail.
                return false;
            }
        }

        private static bool ObjectImplementsComInterface(object o, Guid iid) {
            IntPtr pUnknown = IntPtr.Zero;
            IntPtr pInterface = IntPtr.Zero;

            try {
                pUnknown = Marshal.GetIUnknownForObject(o); // AddRef
                int hr = Marshal.QueryInterface(pUnknown, ref iid, out pInterface); // AddRef
                return (hr == 0 && pInterface != IntPtr.Zero);
            }
            finally {
                if (pUnknown != IntPtr.Zero) {
                    Marshal.Release(pUnknown);
                }
                if (pInterface != IntPtr.Zero) {
                    Marshal.Release(pInterface);
                }
            }
        }
    }
}
