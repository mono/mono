//------------------------------------------------------------------------------
// <copyright file="AppDomainFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * AppDomain factory -- creates app domains on demand from ISAPI
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Hosting {
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;   
    using System.Runtime.Remoting;   
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Util;

    //
    // IAppDomainFactory / AppDomainFactory are obsolete and stay public
    // only to avoid breaking changes.
    //
    // The new code uses IAppManagerAppDomainFactory / AppAppManagerDomainFactory
    //


    /// <internalonly/>
    [ComImport, Guid("e6e21054-a7dc-4378-877d-b7f4a2d7e8ba"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAppDomainFactory {

#if !FEATURE_PAL // FEATURE_PAL does not enable COM
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object Create(
                     [In, MarshalAs(UnmanagedType.BStr)]
                     String module, 
                     [In, MarshalAs(UnmanagedType.BStr)]
                     String typeName, 
                     [In, MarshalAs(UnmanagedType.BStr)]
                     String appId, 
                     [In, MarshalAs(UnmanagedType.BStr)]
                     String appPath,
                     [In, MarshalAs(UnmanagedType.BStr)]
                     String strUrlOfAppOrigin,
                     [In, MarshalAs(UnmanagedType.I4)]
                     int iZone);
#else // !FEATURE_PAL
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        [return: MarshalAs(UnmanagedType.Error)]
        Object Create(
                     [In, MarshalAs(UnmanagedType.Error)]
                     String module, 
                     [In, MarshalAs(UnmanagedType.Error)]
                     String typeName, 
                     [In, MarshalAs(UnmanagedType.Error)]
                     String appId, 
                     [In, MarshalAs(UnmanagedType.Error)]
                     String appPath,
                     [In, MarshalAs(UnmanagedType.Error)]
                     String strUrlOfAppOrigin,
                     [In, MarshalAs(UnmanagedType.I4)]
                     int iZone);

#endif // FEATURE_PAL
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    /// <internalonly/>
    [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
    public sealed class AppDomainFactory : IAppDomainFactory {

        private AppManagerAppDomainFactory _realFactory;
        

        public AppDomainFactory() {
            _realFactory = new AppManagerAppDomainFactory();
        }

        /*
         *  Creates an app domain with an object inside
         */
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
        [return: MarshalAs(UnmanagedType.Interface)] 
#endif // !FEATURE_PAL
        public Object Create(String module, String typeName, String appId, String appPath,
                             String strUrlOfAppOrigin, int iZone) {
            return _realFactory.Create(appId, appPath);
        }
    }

    //
    // The new code -- IAppManagerAppDomainFactory / AppAppManagerDomainFactory
    //


    /// <internalonly/>
    [ComImport, Guid("02998279-7175-4d59-aa5a-fb8e44d4ca9d"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAppManagerAppDomainFactory {
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
        [return: MarshalAs(UnmanagedType.Interface)]
#else // !FEATURE_PAL
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        Object Create(String appId, String appPath);
#endif // !FEATURE_PAL
        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        Object Create([In, MarshalAs(UnmanagedType.BStr)] String appId, 
                      [In, MarshalAs(UnmanagedType.BStr)] String appPath);

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        void Stop();
    }


    /// <internalonly/>
    [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
    public sealed class AppManagerAppDomainFactory : IAppManagerAppDomainFactory {

        private ApplicationManager _appManager;
        

        public AppManagerAppDomainFactory() {
            _appManager = ApplicationManager.GetApplicationManager();
            _appManager.Open();
        }

        /*
         *  Creates an app domain with an object inside
         */
#if FEATURE_PAL // FEATURE_PAL does not enable COM
        [return: MarshalAs(UnmanagedType.Error)] 
#else // FEATURE_PAL
        [return: MarshalAs(UnmanagedType.Interface)] 
#endif // FEATURE_PAL

        public Object Create(String appId, String appPath) {
            try {
                //
                //  Fill app a Dictionary with 'binding rules' -- name value string pairs
                //  for app domain creation
                //

                // 


                if (appPath[0] == '.') {
                    System.IO.FileInfo file = new System.IO.FileInfo(appPath);
                    appPath = file.FullName;
                }

                if (!StringUtil.StringEndsWith(appPath, '\\')) {
                    appPath = appPath + "\\";
                }

                // Create new app domain via App Manager
#if FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
                throw new NotImplementedException("ROTORTODO");
#else // FEATURE_PAL 

                ISAPIApplicationHost appHost = new ISAPIApplicationHost(appId, appPath, false /*validatePhysicalPath*/);

                ISAPIRuntime isapiRuntime = (ISAPIRuntime)_appManager.CreateObjectInternal(appId, typeof(ISAPIRuntime), appHost, 
                        false /*failIfExists*/, null /*hostingParameters*/);


                isapiRuntime.StartProcessing();

                return new ObjectHandle(isapiRuntime);
#endif // FEATURE_PAL 
            }
            catch (Exception e) {
                Debug.Trace("internal", "AppDomainFactory::Create failed with " + e.GetType().FullName + ": " + e.Message + "\r\n" + e.StackTrace);
                throw;
            }
        }


        public void Stop() {
            // wait for all app domains to go away
            _appManager.Close();
        }

        internal static String ConstructSimpleAppName(string virtPath, bool isDevEnvironment) {
            // devdiv 710164:	Still repro - ctrl-f5 a WAP project might show "Cannot create/shadow copy when a file exists" error
            // since the hash file lists are different, IISExpress launched by VS cannot reuse the build result from VS build.
            // It deletes the build files generated by CBM and starts another round of build. This causes interruption between IISExpress and VS 
            // and leads to this shallow copy exception.
            // fix: make the dev IISExpress build to a special drop location 
            // devdiv 1038337: execution permission cannot be acquired under partial trust with ctrl-f5.
            // We previously use HostingEnvironment to determine whether it is under dev environment and the returned string. However, 
            // for partial trust scenario, this method is called before HostingEnvironment has been initialized, we thus may return a wrong value.
            // To fix it, we requir the caller to pass in a flag indicating whether it is under dev environment.   
            if (virtPath.Length <= 1) { // root?
                if (!BuildManagerHost.InClientBuildManager && isDevEnvironment) 
                   return "vs";
                else 
                   return "root";
            }
            else
                return virtPath.Substring(1).ToLower(CultureInfo.InvariantCulture).Replace('/', '_');
        }
    }
}
