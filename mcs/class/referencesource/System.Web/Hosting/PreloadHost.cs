//------------------------------------------------------------------------------
// <copyright file="PreloadHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    
    using System;
    using System.Web;
    using System.Web.Util;
    using System.Configuration;

    internal sealed class PreloadHost : MarshalByRefObject, IRegisteredObject {

        public PreloadHost() {
            HostingEnvironment.RegisterObject(this);
        }

        public void CreateIProcessHostPreloadClientInstanceAndCallPreload(string preloadObjTypeName, string[] paramsForStartupObj) {

            using (new ApplicationImpersonationContext()) {
                // Check the type
                Type preloadObjType = null;
                try {
                    preloadObjType = Type.GetType(preloadObjTypeName, true);
                }
                catch (Exception e) {
                    throw new InvalidOperationException (
                        Misc.FormatExceptionMessage(e, new string[]{
                            SR.GetString(SR.Failure_Create_Application_Preload_Provider_Type, preloadObjTypeName)} ));
                }

                if (!typeof(IProcessHostPreloadClient).IsAssignableFrom(preloadObjType)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_Application_Preload_Provider_Type, preloadObjTypeName));
                }

                // Let all other exceptons fall through to the default AppDomain
                IProcessHostPreloadClient preloadClient = (IProcessHostPreloadClient)Activator.CreateInstance(preloadObjType);
                preloadClient.Preload(paramsForStartupObj);
            }
        }

        internal Exception InitializationException { get { return HttpRuntime.InitializationException; } }

        void IRegisteredObject.Stop(bool immediate) {
            HostingEnvironment.UnregisterObject(this);
        }

        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }
    }
}

