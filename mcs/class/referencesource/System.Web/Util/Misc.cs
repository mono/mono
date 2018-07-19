//------------------------------------------------------------------------------
// <copyright file="FileChangesMonitor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Hosting;
    using Microsoft.Win32;

    internal sealed class Misc {

        const string APPLICATION_ID  = "\r\n\r\nApplication ID: ";
        const string PROCESS_ID      = "\r\n\r\nProcess ID: ";
        const string EXCEPTION       = "\r\n\r\nException: ";
        const string INNER_EXCEPTION = "\r\n\r\nInnerException: ";
        const string MESSAGE         = "\r\n\r\nMessage: ";
        const string STACK_TRACE     = "\r\n\r\nStackTrace: ";

        static StringComparer s_caseInsensitiveInvariantKeyComparer;

        internal static StringComparer CaseInsensitiveInvariantKeyComparer {
            get {
                if (s_caseInsensitiveInvariantKeyComparer == null) {
                    s_caseInsensitiveInvariantKeyComparer = StringComparer.Create(CultureInfo.InvariantCulture, true);
                }

                return s_caseInsensitiveInvariantKeyComparer;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString(System.IFormatProvider)",
            Justification = "This is the proper culture for writing to the event log.")]
        internal static void WriteUnhandledExceptionToEventLog(AppDomain appDomain, Exception exception) {
            if (appDomain == null || exception == null) {
                return;
            }

            ProcessImpersonationContext imperContext = null;
            try {
                imperContext = new ProcessImpersonationContext();
                String appId = appDomain.GetData(".appId") as String;
                if (appId == null) {
                    appId = appDomain.FriendlyName;
                }
                string pid = SafeNativeMethods.GetCurrentProcessId().ToString(CultureInfo.InstalledUICulture);
                string description = SR.Resources.GetString(SR.Unhandled_Exception, CultureInfo.InstalledUICulture);
                Misc.ReportUnhandledException(exception, new string[5] {description, APPLICATION_ID, appId, PROCESS_ID, pid});
            }
            catch {
                // ignore exceptions so that WriteErrorToEventLog never throws
            }
            finally {
                if (imperContext != null) {
                    imperContext.Undo();
                }
            }
        }

        internal static void ReportUnhandledException(Exception e, String[] strings) {
            UnsafeNativeMethods.ReportUnhandledException(FormatExceptionMessage(e, strings));
        }

        internal static String FormatExceptionMessage(Exception e, String[] strings) {
            StringBuilder sb = new StringBuilder(4096);
            for (int i = 0; i < strings.Length; i++) {
                sb.Append(strings[i]);
            }
            for (Exception current = e; current != null; current = current.InnerException) {
                if (current == e) 
                    sb.Append(EXCEPTION);
                else
                    sb.Append(INNER_EXCEPTION);
                sb.Append(current.GetType().FullName);
                sb.Append(MESSAGE);
                sb.Append(current.Message);
                sb.Append(STACK_TRACE);
                sb.Append(current.StackTrace);                    
            }

            return sb.ToString();
        }

        internal unsafe static void CopyMemory(IntPtr src, int srcOffset, byte[] dest, int destOffset, int size) {
            // 
            System.Runtime.InteropServices.Marshal.Copy(new IntPtr(src.ToInt64()+srcOffset), dest, destOffset, size);
        }

        internal unsafe static void CopyMemory(byte[] src, int srcOffset, IntPtr dest, int destOffset, int size) {
            // 
            System.Runtime.InteropServices.Marshal.Copy(src, srcOffset, new IntPtr(dest.ToInt64()+destOffset), size);
        }

        internal unsafe static void CopyMemory(IntPtr src, int srcOffset, IntPtr dest, int destOffset, int size) {
            byte *s = ((byte*)src) + srcOffset;
            byte *d = ((byte*)dest) + destOffset;
            StringUtil.memcpyimpl(s, d, size);
        }

        internal static void ThrowIfFailedHr(int hresult) {
            // SUCCEEDED >= 0
            // FAILED < 0
            if (hresult < 0) {
                Marshal.ThrowExceptionForHR(hresult);
            }
        }

        internal static IProcessHostSupportFunctions CreateLocalSupportFunctions(IProcessHostSupportFunctions proxyFunctions) {
            IProcessHostSupportFunctions localFunctions = null;

            // get the underlying COM object
            IntPtr pUnk = Marshal.GetIUnknownForObject(proxyFunctions);

            // this object isn't a COM object
            if (IntPtr.Zero == pUnk) {
                return null;
            }
            
            IntPtr ppv = IntPtr.Zero;
            try {
                // QI it for the interface
                Guid g = typeof(IProcessHostSupportFunctions).GUID;

                int hresult = Marshal.QueryInterface(pUnk, ref g, out ppv);
                if (hresult < 0)  {
                    Marshal.ThrowExceptionForHR(hresult);
                }

                // create a RCW we can hold onto in this domain
                // this bumps the ref count so we can drop our refs on the raw interfaces
                localFunctions = (IProcessHostSupportFunctions)Marshal.GetObjectForIUnknown(ppv);
            }
            finally {
                // drop our explicit refs and keep the managed instance
                if (IntPtr.Zero != ppv) {
                    Marshal.Release(ppv);
                }
                if (IntPtr.Zero != pUnk) {
                    Marshal.Release(pUnk);
                }
            }

            return localFunctions;
        }


        // Open ASP.NET's reg key, or one of its subkeys
        internal static RegistryKey OpenAspNetRegKey(string subKey) {
            String ver = VersionInfo.SystemWebVersion;

            // Zero out minor version number VSWhidbey 602541
            // Eg. 2.0.50727.42 becomes 2.0.50727.0
            if (!string.IsNullOrEmpty(ver)) {
                int pos = ver.LastIndexOf('.');
                if (pos > -1) {
                    ver = ver.Substring(0, pos + 1) + "0";
                }
            }
 
            // The main ASP.NET reg key
            string key = @"Software\Microsoft\ASP.NET\" + ver;

            // If we're asked for a subkey, append it
            if (subKey != null)
                key += @"\" + subKey;

            // Open and return the key
            return Registry.LocalMachine.OpenSubKey(key);
        }

        // Get an ASP.NET registry value, from the main key or a subkey
        // The Registry class does a full demand, so we'll turn it into a LinkDemand
        [RegistryPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [RegistryPermission(SecurityAction.Assert, Unrestricted = true)]
        internal static object GetAspNetRegValue(string subKey, string valueName, object defaultValue) {
            try {
                using (RegistryKey regKey = OpenAspNetRegKey(subKey)) {

                    // Return the default value if the key doesn't exist
                    if (regKey == null)
                        return defaultValue;

                    return regKey.GetValue(valueName, defaultValue);
                }
            }
            catch {
                // Return the default value if anything goes wrong
                return defaultValue;
            }
        }

    }
}


