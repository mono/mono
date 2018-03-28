//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System.IdentityModel;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Security.Claims;

using Claim = System.Security.Claims.Claim;

namespace System.Security.Claims
{
    internal static class ClaimsHelper
    {

        /// <summary>
        /// Creates a <see cref="WindowsIdentity"/> associated with a given X509 certificate.
        /// </summary>
        /// <param name="x509Certificate">The certificate to use to map to the associated <see cref="WindowsIdentity"/></param>
        /// <returns></returns>
        public static WindowsIdentity CertificateLogon(X509Certificate2 x509Certificate)
        {
            // for Vista, LsaLogon supporting mapping cert to NTToken
            if (Environment.OSVersion.Version.Major >= CryptoHelper.WindowsVistaMajorNumber)
            {
                return X509SecurityTokenHandler.KerberosCertificateLogon(x509Certificate);
            }
            else
            {
                // Downlevel, S4U over PrincipalName SubjectAltNames
                string upn = x509Certificate.GetNameInfo(X509NameType.UpnName, false);
                if (string.IsNullOrEmpty(upn))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4067,
                        X509Util.GetCertificateId(x509Certificate))));
                }

                return new WindowsIdentity(upn);
            }
        }

        /// <summary>
        /// Finds the UPN claim value in the provided <see cref="ClaimsIdentity" /> object for the purpose
        /// of mapping the identity to a <see cref="WindowsIdentity" /> object.
        /// </summary>
        /// <param name="claimsIdentity">The claims identity object containing the desired UPN claim.</param>
        /// <returns>The UPN claim value found.</returns>
        /// <exception cref="SecurityTokenException">
        /// If <paramref name="claimsIdentity"/> contains zero UPN claims or more than one UPN claim.
        /// </exception>
        public static string FindUpn(ClaimsIdentity claimsIdentity)
        {
            string upn = null;
            foreach (Claim claim in claimsIdentity.Claims)
            {
                if (StringComparer.Ordinal.Equals(ClaimTypes.Upn, claim.Type))
                {
                    // Complain if we already found a UPN claim
                    if (upn != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID1053)));
                    }
                    upn = claim.Value;
                }
            }

            if (string.IsNullOrEmpty(upn))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.ID1054)));
            }
            return upn;
        }

        #region FIP 12979 GetAnonymous resolved as Postponed

        /*
        /// <summary>
        /// Generates a <see cref="WindowsClaimsIdentity"/> based on an anonymous user Windows token.
        /// </summary>
        /// <returns>A <see cref="WindowsClaimsIdentity"/> whose base <see cref="WindowsIdentity"/> has a Windows token for the NT AUTHORITY\ANONYMOUS LOGON user.</returns>
        /// <exception cref="Win32Exception">Thrown if this method fails to open the current thread token.</exception>
        /// <exception cref="Win32Exception">Thrown if this method fails to impersonate NT AUTHORITY\ANONYMOUS LOGON.</exception>
        /// <exception cref="Win32Exception">Thrown if this method fails in attempt to reset thread token.</exception>
        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [SecurityPermissionAttribute( SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal )]
        new public static WindowsClaimsIdentity GetAnonymous()
        {
            SafeCloseHandle originalThreadToken;
            WindowsClaimsIdentity result = null;

            //
            // If the thread is impersonating
            // preserve the token so we can put it back
            // after capturing the anonymous token.
            //
            if( !NativeMethods.OpenThreadToken(
                        NativeMethods.GetCurrentThread(),
                        TokenAccessLevels.Impersonate,
                        true, // Use the process identity permissions
                        out originalThreadToken ) )
            {
                int win32Result = Marshal.GetLastWin32Error();
                if( ( (int) Win32Error.ERROR_NO_TOKEN ) != win32Result )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new Win32Exception( win32Result ) );
                }
            }

            //
            // Use CER to prevent the app-domain from unloading before
            // we can set the thread token back to the original value.
            //
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if( !NativeMethods.ImpersonateAnonymousToken( NativeMethods.GetCurrentThread() ) )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new Win32Exception() );
                }

                result = WindowsClaimsIdentity.GetCurrent();
            }
            finally
            {
                //
                // Replace the thread token with the original.
                // Setting the thread token to zero will stop impersonating.
                // 
                if( !NativeMethods.SetThreadToken(
                            IntPtr.Zero, // current thread
                            originalThreadToken ) )
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new Win32Exception() );
                }

                originalThreadToken.Close();
            }

            return result;
        }
 */
        #endregion
    }
}
