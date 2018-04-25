// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Runtime.Versioning;

namespace System.Security.Principal
{
    using BOOL = System.Int32;
    using DWORD = System.UInt32;
    using System.Globalization;
    using System.Diagnostics.Contracts;

    [Flags]
    internal enum PolicyRights
    {
        POLICY_VIEW_LOCAL_INFORMATION            = 0x00000001,
        POLICY_VIEW_AUDIT_INFORMATION            = 0x00000002,
        POLICY_GET_PRIVATE_INFORMATION           = 0x00000004,
        POLICY_TRUST_ADMIN                       = 0x00000008,
        POLICY_CREATE_ACCOUNT                    = 0x00000010,
        POLICY_CREATE_SECRET                     = 0x00000020,
        POLICY_CREATE_PRIVILEGE                  = 0x00000040,
        POLICY_SET_DEFAULT_QUOTA_LIMITS          = 0x00000080,
        POLICY_SET_AUDIT_REQUIREMENTS            = 0x00000100,
        POLICY_AUDIT_LOG_ADMIN                   = 0x00000200,
        POLICY_SERVER_ADMIN                      = 0x00000400,
        POLICY_LOOKUP_NAMES                      = 0x00000800,
        POLICY_NOTIFICATION                      = 0x00001000,
    }

    internal static class Win32
    {
        internal const BOOL FALSE = 0;
        internal const BOOL TRUE = 1;

        private static bool _LsaLookupNames2Supported;
        private static bool _WellKnownSidApisSupported;

        [System.Security.SecuritySafeCritical]  // auto-generated
        static Win32() 
        {
            
            Win32Native.OSVERSIONINFO osvi = new Win32Native.OSVERSIONINFO();

            bool r = Environment.GetVersion(osvi);
            if ( !r )
            {
                Contract.Assert( r, "OSVersion native call failed." );
                throw new SystemException( Environment.GetResourceString( "InvalidOperation_GetVersion" ));
            }
                if (osvi.MajorVersion > 5 || osvi.MinorVersion > 0 ) // Windows XP/2003 and above
                {

                    //
                    // LsaLookupNames2 supported only on XP and Windows 2003 and above
                    //
                    _LsaLookupNames2Supported = true;
                    _WellKnownSidApisSupported = true;
                }
                else 
                {
                    // Win2000
                    _LsaLookupNames2Supported = false;
                

                    //
                    // WellKnownSid apis are only supported on Windows 2000 SP3 and above
                    // (so we need sp info)
                    //
                    Win32Native.OSVERSIONINFOEX osviex = new Win32Native.OSVERSIONINFOEX();

                    r = Environment.GetVersionEx(osviex);
                    if ( !r )
                    {
                        Contract.Assert( r, "OSVersion native call failed");
                        throw new SystemException( Environment.GetResourceString( "InvalidOperation_GetVersion" ));
                    }

                    if (osviex.ServicePackMajor < 3) 
                    {
                        _WellKnownSidApisSupported = false;    
                    }
                    else 
                    {
                        _WellKnownSidApisSupported = true; 
                    }
                }
            }

        internal static bool LsaLookupNames2Supported
        {
            get {
                return _LsaLookupNames2Supported;
            }
        }

        internal static bool WellKnownSidApisSupported
        {
            get {
                return _WellKnownSidApisSupported;
            }
        }

        //
        // Wrapper around advapi32.LsaOpenPolicy
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static SafeLsaPolicyHandle LsaOpenPolicy(
            string systemName,
            PolicyRights rights )
        {
            uint ReturnCode;
            SafeLsaPolicyHandle Result;
            Win32Native.LSA_OBJECT_ATTRIBUTES Loa;

            Loa.Length = Marshal.SizeOf( typeof( Win32Native.LSA_OBJECT_ATTRIBUTES ));
            Loa.RootDirectory = IntPtr.Zero;
            Loa.ObjectName = IntPtr.Zero;
            Loa.Attributes = 0;
            Loa.SecurityDescriptor = IntPtr.Zero;
            Loa.SecurityQualityOfService = IntPtr.Zero;

            if ( 0 == ( ReturnCode = Win32Native.LsaOpenPolicy( systemName, ref Loa, ( int )rights, out Result )))
            {
                return Result;
            }
            else if ( ReturnCode == Win32Native.STATUS_ACCESS_DENIED ) 
            {
                throw new UnauthorizedAccessException();
            }
            else if ( ReturnCode == Win32Native.STATUS_INSUFFICIENT_RESOURCES ||
                      ReturnCode == Win32Native.STATUS_NO_MEMORY ) 
            {
                throw new OutOfMemoryException();
            }
            else
            {
                int win32ErrorCode = Win32Native.LsaNtStatusToWinError(unchecked((int) ReturnCode));
                
                throw new SystemException(Win32Native.GetMessage(win32ErrorCode));
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static byte[] ConvertIntPtrSidToByteArraySid( IntPtr binaryForm )
        {
            byte[] ResultSid;

            //
            // Verify the revision (just sanity, should never fail to be 1)
            //

            byte Revision = Marshal.ReadByte( binaryForm, 0 );

            if ( Revision != SecurityIdentifier.Revision )
            {
                throw new ArgumentException(Environment.GetResourceString( "IdentityReference_InvalidSidRevision" ), "binaryForm");
            }

            //
            // Need the subauthority count in order to figure out how many bytes to read
            //

            byte SubAuthorityCount = Marshal.ReadByte( binaryForm, 1 );

            if ( SubAuthorityCount < 0 ||
                SubAuthorityCount > SecurityIdentifier.MaxSubAuthorities )
            {
                throw new ArgumentException(Environment.GetResourceString( "IdentityReference_InvalidNumberOfSubauthorities", SecurityIdentifier.MaxSubAuthorities), "binaryForm");
            }

            //
            // Compute the size of the binary form of this SID and allocate the memory
            //

            int BinaryLength = 1 + 1 + 6 + SubAuthorityCount * 4;
            ResultSid = new byte[ BinaryLength ];

            //
            // Extract the data from the returned pointer
            //

            Marshal.Copy( binaryForm, ResultSid, 0, BinaryLength );

            return ResultSid;
        }

        //
        // Wrapper around advapi32.ConvertStringSidToSidW
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static int CreateSidFromString(
            string stringSid,
            out byte[] resultSid
            )
        {
            int ErrorCode;
            IntPtr ByteArray = IntPtr.Zero;

            try
            {
                if ( TRUE != Win32Native.ConvertStringSidToSid( stringSid, out ByteArray ))
                {
                    ErrorCode = Marshal.GetLastWin32Error();
                    goto Error;
                }

                resultSid = ConvertIntPtrSidToByteArraySid( ByteArray );
            }
            finally
            {
                //
                // Now is a good time to get rid of the returned pointer
                //

                Win32Native.LocalFree( ByteArray );
            }

            //
            // Now invoke the SecurityIdentifier factory method to create the result
            //

            return Win32Native.ERROR_SUCCESS;

        Error:

            resultSid = null;
            return ErrorCode;
        }

        //
        // Wrapper around advapi32.CreateWellKnownSid
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static int CreateWellKnownSid(
            WellKnownSidType sidType,
            SecurityIdentifier domainSid,
            out byte[] resultSid
            )
        {

            //
            // Check if the api is supported
            //
            if (!WellKnownSidApisSupported) {
                throw new PlatformNotSupportedException( Environment.GetResourceString( "PlatformNotSupported_RequiresW2kSP3" ));
            }
        
            //
            // Passing an array as big as it can ever be is a small price to pay for
            // not having to P/Invoke twice (once to get the buffer, once to get the data)
            //

            uint length = ( uint )SecurityIdentifier.MaxBinaryLength;
            resultSid = new byte[ length ];

            if ( FALSE != Win32Native.CreateWellKnownSid(( int )sidType, domainSid == null ? null : domainSid.BinaryForm, resultSid, ref length ))
            {
                return Win32Native.ERROR_SUCCESS;
            }
            else
            {
                resultSid = null;

                return Marshal.GetLastWin32Error();
            }
        }

        //
        // Wrapper around advapi32.EqualDomainSid
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static bool IsEqualDomainSid( SecurityIdentifier sid1, SecurityIdentifier sid2 )
        {
            //
            // Check if the api is supported
            //
            if (!WellKnownSidApisSupported) {
                throw new PlatformNotSupportedException( Environment.GetResourceString( "PlatformNotSupported_RequiresW2kSP3" ));
            }
        
            if ( sid1 == null || sid2 == null )
            {
                return false;
            }
            else
            {
                bool result;
                
                byte[] BinaryForm1 = new Byte[sid1.BinaryLength];
                sid1.GetBinaryForm( BinaryForm1, 0 );

                byte[] BinaryForm2 = new Byte[sid2.BinaryLength];
                sid2.GetBinaryForm( BinaryForm2, 0 );

                return ( Win32Native.IsEqualDomainSid( BinaryForm1, BinaryForm2, out result ) == FALSE ? false : result );
            }
        }

        /// <summary>
        ///     Setup the size of the buffer Windows provides for an LSA_REFERENCED_DOMAIN_LIST
        /// </summary>
        [System.Security.SecurityCritical]  // auto-generated
        internal static void InitializeReferencedDomainsPointer(SafeLsaMemoryHandle referencedDomains)
        {
            Contract.Assert(referencedDomains != null, "referencedDomains != null");

            // We don't know the real size of the referenced domains yet, so we need to set an initial
            // size based on the LSA_REFERENCED_DOMAIN_LIST structure, then resize it to include all of
            // the domains.
            referencedDomains.Initialize((uint)Marshal.SizeOf(typeof(Win32Native.LSA_REFERENCED_DOMAIN_LIST)));
            Win32Native.LSA_REFERENCED_DOMAIN_LIST domainList = referencedDomains.Read<Win32Native.LSA_REFERENCED_DOMAIN_LIST>(0);

            unsafe
            {
                byte* pRdl = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    referencedDomains.AcquirePointer(ref pRdl);

                    // If there is a trust information list, then the buffer size is the end of that list minus
                    // the beginning of the domain list. Otherwise, then the buffer is just the size of the
                    // referenced domain list structure, which is what we defaulted to.
                    if (!domainList.Domains.IsNull())
                    {
                        Win32Native.LSA_TRUST_INFORMATION* pTrustInformation = (Win32Native.LSA_TRUST_INFORMATION*)domainList.Domains;
                        pTrustInformation = pTrustInformation + domainList.Entries;

                        long bufferSize = (byte*)pTrustInformation - pRdl;
                        Contract.Assert(bufferSize > 0, "bufferSize > 0");
                        referencedDomains.Initialize((ulong)bufferSize);
                    }
                }
                finally
                {
                    if (pRdl != null)
                        referencedDomains.ReleasePointer();
                }
            }
        }

        //
        // Wrapper around avdapi32.GetWindowsAccountDomainSid
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static int GetWindowsAccountDomainSid(
            SecurityIdentifier sid,
            out SecurityIdentifier resultSid
            )
        {

            //
            // Check if the api is supported
            //
            if (!WellKnownSidApisSupported) {
                throw new PlatformNotSupportedException( Environment.GetResourceString( "PlatformNotSupported_RequiresW2kSP3" ));
            }
        
            //
            // Passing an array as big as it can ever be is a small price to pay for
            // not having to P/Invoke twice (once to get the buffer, once to get the data)
            //

            byte[] BinaryForm = new Byte[sid.BinaryLength];
            sid.GetBinaryForm( BinaryForm, 0 );
            uint sidLength = ( uint )SecurityIdentifier.MaxBinaryLength;
            byte[] resultSidBinary = new byte[ sidLength ];

            if ( FALSE != Win32Native.GetWindowsAccountDomainSid( BinaryForm, resultSidBinary, ref sidLength ))
            {
                resultSid = new SecurityIdentifier( resultSidBinary, 0 );

                return Win32Native.ERROR_SUCCESS;
            }
            else
            {
                resultSid = null;

                return Marshal.GetLastWin32Error();
            }
        }

        //
        // Wrapper around advapi32.IsWellKnownSid
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static bool IsWellKnownSid(
            SecurityIdentifier sid,
            WellKnownSidType type
            )
        {
            //
            // Check if the api is supported
            //
            if (!WellKnownSidApisSupported) {
                throw new PlatformNotSupportedException( Environment.GetResourceString( "PlatformNotSupported_RequiresW2kSP3" ));
            }
      
            byte[] BinaryForm = new byte[sid.BinaryLength];
            sid.GetBinaryForm( BinaryForm, 0 );

            if ( FALSE == Win32Native.IsWellKnownSid( BinaryForm, ( int )type ))
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        // When the CLR is hosted, the host gets to implement these calls,
        // otherwise, we call down into the Win32 APIs.

#if FEATURE_IMPERSONATION
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        internal static extern int ImpersonateLoggedOnUser (SafeAccessTokenHandle hToken);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Process)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int OpenThreadToken (TokenAccessLevels dwDesiredAccess, WinSecurityContext OpenAs, out SafeAccessTokenHandle phThreadToken);

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        internal static extern int RevertToSelf ();

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        internal static extern int SetThreadToken(SafeAccessTokenHandle hToken);
#endif        
    }
}
