//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Runtime;

namespace System.ServiceModel.Security
{
    static class SecurityUtilsEx
    {
        static int fipsAlgorithmPolicy = -1;

        // Federal Information Processing Standards Publications
        // at http://www.itl.nist.gov/fipspubs/geninfo.htm
        // Note: this is copied from System.IdentityModel.SecurityUtilsEx.RequiresFipsCompliance.
        internal static bool RequiresFipsCompliance
        {
            [Fx.Tag.SecurityNote(Critical = "Calls an UnsafeNativeMethod and a Critical method (GetFipsAlgorithmPolicyKeyFromRegistry).",
                Safe = "Processes the return and just returns a bool, which is safe.")]
            [SecuritySafeCritical]
            get
            {
                if (fipsAlgorithmPolicy == -1)
                {
                    if (OSEnvironmentHelper.IsVistaOrGreater)
                    {
                        bool fipsEnabled;
#pragma warning suppress 56523 //  we check for the return code of the method instead of calling GetLastWin32Error
                        bool readPolicy = (System.ServiceModel.Channels.UnsafeNativeMethods.ERROR_SUCCESS == System.ServiceModel.Channels.UnsafeNativeMethods.BCryptGetFipsAlgorithmMode(out fipsEnabled));

                        if (readPolicy && fipsEnabled)
                            fipsAlgorithmPolicy = 1;
                        else
                            fipsAlgorithmPolicy = 0;
                    }
                    else
                    {
                        fipsAlgorithmPolicy = GetFipsAlgorithmPolicyKeyFromRegistry();
                        if (fipsAlgorithmPolicy != 1)
                            fipsAlgorithmPolicy = 0;
                    }
                }
                return fipsAlgorithmPolicy == 1;
            }
        }

        const string fipsPolicyRegistryKey = @"System\CurrentControlSet\Control\Lsa";

        [Fx.Tag.SecurityNote(Critical = "Asserts to get a value from the registry.")]
        [SecurityCritical]
        [RegistryPermission(SecurityAction.Assert, Read = @"HKEY_LOCAL_MACHINE\" + fipsPolicyRegistryKey)]
        static int GetFipsAlgorithmPolicyKeyFromRegistry()
        {
            int fipsAlgorithmPolicy = -1;
            using (RegistryKey fipsAlgorithmPolicyKey = Registry.LocalMachine.OpenSubKey(fipsPolicyRegistryKey, false))
            {
                if (fipsAlgorithmPolicyKey != null)
                {
                    object data = fipsAlgorithmPolicyKey.GetValue("FIPSAlgorithmPolicy");
                    if (data != null)
                        fipsAlgorithmPolicy = (int)data;
                }
            }
            return fipsAlgorithmPolicy;
        }
    }

}
