//------------------------------------------------------------------------------
// <copyright file="GacUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Web.Configuration;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /*
    class for installing ASP.BrowserCapabilitiesFactory into gac
    */
    internal sealed class GacUtil : IGac {


        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void GacInstall(string assemblyPath) {

#if !FEATURE_PAL
            IAssemblyCache ac = null;
            int hr = NativeMethods.CreateAssemblyCache(out ac, 0);
            if (0 == hr) hr = ac.InstallAssembly(0, assemblyPath, IntPtr.Zero);
#else // !FEATURE_PAL
            int hr = -1;
            try
            {
                Process gacutilprocess = new System.Diagnostics.Process();
                if (gacutilprocess != null)
                {
                    gacutilprocess.StartInfo.CreateNoWindow = true;
#if PLATFORM_UNIX
                    gacutilprocess.StartInfo.FileName = "gacutil";
#else
                    gacutilprocess.StartInfo.FileName = "gacutil.exe";
#endif
                    gacutilprocess.StartInfo.UseShellExecute = false;
                    gacutilprocess.StartInfo.Arguments = "/i " + assemblyPath;
                    gacutilprocess.Start();
                    while (!gacutilprocess.HasExited)
                    {
                        Thread.Sleep(250);
                    }
                    hr = gacutilprocess.ExitCode;
                }
            }
            catch (Exception)
            {
                hr = -1;
            }
#endif // FEATURE_PAL

            if (0 != hr) {
                throw new Exception(SR.GetString(SR.Failed_gac_install));
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public bool GacUnInstall(string assemblyName) {
            IAssemblyCache ac = null;
            uint position = 0;
            int hr = NativeMethods.CreateAssemblyCache(out ac, 0);

            if (0 == hr) {
                hr = ac.UninstallAssembly(0, assemblyName, IntPtr.Zero, out position);
                if (position == 3 /*IASSEMBLYCACHE_UNINSTALL_DISPOSITION_ALREADY_UNINSTALLED*/) {
                    return false;
                }
            }

            if (0 != hr) {
                throw new Exception(SR.GetString(SR.Failed_gac_uninstall));
            }

            return true;
        }
    }
}
