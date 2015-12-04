// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics.Contracts;

namespace System.Security.Principal
{
    //
    // Identifier authorities
    //
    
    internal enum IdentifierAuthority : long
    {
        NullAuthority                            = 0,
        WorldAuthority                           = 1,
        LocalAuthority                           = 2,
        CreatorAuthority                         = 3,
        NonUniqueAuthority                       = 4,
        NTAuthority                              = 5,
        SiteServerAuthority                      = 6,
        InternetSiteAuthority                    = 7,
        ExchangeAuthority                        = 8,
        ResourceManagerAuthority                 = 9,
    }

    //
    // SID name usage
    //

    internal enum SidNameUse
    {
        User                              = 1,
        Group                             = 2,
        Domain                            = 3,
        Alias                             = 4,
        WellKnownGroup                    = 5,
        DeletedAccount                    = 6,
        Invalid                           = 7,
        Unknown                           = 8,
        Computer                          = 9,
    }

    //
    // Well-known SID types
    //

[System.Runtime.InteropServices.ComVisible(false)]
    public enum WellKnownSidType
    {
        NullSid                                  = 0,
        WorldSid                                 = 1,
        LocalSid                                 = 2,
        CreatorOwnerSid                          = 3,
        CreatorGroupSid                          = 4,
        CreatorOwnerServerSid                    = 5,
        CreatorGroupServerSid                    = 6,
        NTAuthoritySid                           = 7,
        DialupSid                                = 8,
        NetworkSid                               = 9,
        BatchSid                                 = 10,
        InteractiveSid                           = 11,
        ServiceSid                               = 12,
        AnonymousSid                             = 13,
        ProxySid                                 = 14,
        EnterpriseControllersSid                 = 15,
        SelfSid                                  = 16,
        AuthenticatedUserSid                     = 17,
        RestrictedCodeSid                        = 18,
        TerminalServerSid                        = 19,
        RemoteLogonIdSid                         = 20,
        LogonIdsSid                              = 21,
        LocalSystemSid                           = 22,
        LocalServiceSid                          = 23,
        NetworkServiceSid                        = 24,
        BuiltinDomainSid                         = 25,
        BuiltinAdministratorsSid                 = 26,
        BuiltinUsersSid                          = 27,
        BuiltinGuestsSid                         = 28,
        BuiltinPowerUsersSid                     = 29,
        BuiltinAccountOperatorsSid               = 30,
        BuiltinSystemOperatorsSid                = 31,
        BuiltinPrintOperatorsSid                 = 32,
        BuiltinBackupOperatorsSid                = 33,
        BuiltinReplicatorSid                     = 34,
        BuiltinPreWindows2000CompatibleAccessSid = 35,
        BuiltinRemoteDesktopUsersSid             = 36,
        BuiltinNetworkConfigurationOperatorsSid  = 37,
        AccountAdministratorSid                  = 38,
        AccountGuestSid                          = 39,
        AccountKrbtgtSid                         = 40,
        AccountDomainAdminsSid                   = 41,
        AccountDomainUsersSid                    = 42,
        AccountDomainGuestsSid                   = 43,
        AccountComputersSid                      = 44,
        AccountControllersSid                    = 45,
        AccountCertAdminsSid                     = 46,
        AccountSchemaAdminsSid                   = 47,
        AccountEnterpriseAdminsSid               = 48,
        AccountPolicyAdminsSid                   = 49,
        AccountRasAndIasServersSid               = 50,
        NtlmAuthenticationSid                    = 51,
        DigestAuthenticationSid                  = 52,
        SChannelAuthenticationSid                = 53,
        ThisOrganizationSid                      = 54,
        OtherOrganizationSid                     = 55,
        BuiltinIncomingForestTrustBuildersSid    = 56,
        BuiltinPerformanceMonitoringUsersSid     = 57,
        BuiltinPerformanceLoggingUsersSid        = 58,
        BuiltinAuthorizationAccessSid            = 59,
        WinBuiltinTerminalServerLicenseServersSid= 60,
        MaxDefined = WinBuiltinTerminalServerLicenseServersSid,
    }

    //
    // This class implements revision 1 SIDs
    // NOTE: The SecurityIdentifier class is immutable and must remain this way
    //

[System.Runtime.InteropServices.ComVisible(false)]
    public sealed class SecurityIdentifier : IdentityReference, IComparable<SecurityIdentifier>
    {
        #region Public Constants

        //
        // Identifier authority must be at most six bytes long
        //

        internal static readonly long MaxIdentifierAuthority = 0xFFFFFFFFFFFF;

        //
        // Maximum number of subauthorities in a SID
        //

        internal static readonly byte MaxSubAuthorities = 15;

        //
        // Minimum length of a binary representation of a SID
        //

        public static readonly int MinBinaryLength = 1 + 1 + 6; // Revision (1) + subauth count (1) + identifier authority (6)

        //
        // Maximum length of a binary representation of a SID
        //

        public static readonly int MaxBinaryLength = 1 + 1 + 6 + MaxSubAuthorities * 4; // 4 bytes for each subauth

        #endregion

        #region Private Members

        //
        // Immutable properties of a SID
        //

        private IdentifierAuthority _IdentifierAuthority;
        private int[] _SubAuthorities;
        private byte[] _BinaryForm;
        private SecurityIdentifier _AccountDomainSid;
        bool _AccountDomainSidInitialized = false;

        //
        // Computed attributes of a SID
        //

        private string _SddlForm = null;

        #endregion

        #region Constructors

        //
        // Shared constructor logic
        // NOTE: subauthorities are really unsigned integers, but due to CLS
        //       lack of support for unsigned integers the caller must perform
        //       the typecast
        //

        private void CreateFromParts( IdentifierAuthority identifierAuthority, int[] subAuthorities )
        {
            if ( subAuthorities == null )
            {
                throw new ArgumentNullException( "subAuthorities" );
            }

            Contract.EndContractBlock();

            //
            // Check the number of subauthorities passed in 
            //

            if ( subAuthorities.Length > MaxSubAuthorities )
            {
                throw new ArgumentOutOfRangeException(
                    "subAuthorities.Length",
                    subAuthorities.Length,
                    Environment.GetResourceString( "IdentityReference_InvalidNumberOfSubauthorities",  MaxSubAuthorities));
            }

            //
            // Identifier authority is atmost 6 bytes long
            //
            
            if ( identifierAuthority < 0  ||
                (long) identifierAuthority > MaxIdentifierAuthority )
            {
                throw new ArgumentOutOfRangeException(
                    "identifierAuthority",
                    identifierAuthority,
                    Environment.GetResourceString( "IdentityReference_IdentifierAuthorityTooLarge" ));
            }

            //
            // Create a local copy of the data passed in
            //

            _IdentifierAuthority = identifierAuthority;
            _SubAuthorities = new int[ subAuthorities.Length ];
            subAuthorities.CopyTo( _SubAuthorities, 0 );

            //
            // Compute and store the binary form
            //
            // typedef struct _SID {
            //     UCHAR Revision;
            //     UCHAR SubAuthorityCount;
            //     SID_IDENTIFIER_AUTHORITY IdentifierAuthority;
            //     ULONG SubAuthority[ANYSIZE_ARRAY]
            // } SID, *PISID;
            //

            byte i;
            _BinaryForm = new byte[1 + 1 + 6 + 4 * this.SubAuthorityCount];

            //
            // First two bytes contain revision and subauthority count
            //

            _BinaryForm[0] = Revision;
            _BinaryForm[1] = ( byte )this.SubAuthorityCount;

            //
            // Identifier authority takes up 6 bytes
            //

            for ( i = 0; i < 6; i++ )
            {
                _BinaryForm[2+i] = ( byte )(((( ulong )this._IdentifierAuthority ) >> (( 5 - i ) * 8 )) & 0xFF ) ;
            }

            //
            // Subauthorities go last, preserving big-endian representation
            //

            for ( i = 0; i < this.SubAuthorityCount; i++ )
            {
                byte shift;
                for ( shift = 0; shift < 4; shift += 1 )
                {
                    _BinaryForm[8 + 4*i + shift] = ( byte )((( ulong )_SubAuthorities[i] ) >> ( shift * 8 ));
                }
            }
        }

        private void CreateFromBinaryForm( byte[] binaryForm, int offset )
        {
            //
            // Give us something to work with
            //
        
            if ( binaryForm == null )
            {
                throw new ArgumentNullException( "binaryForm" );
            }
        
            //
            // Negative offsets are not allowed
            //
        
            if ( offset < 0 )
            {
                throw new ArgumentOutOfRangeException(
                    "offset",
                    offset,
                    Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ));
            }
        
            //
            // At least a minimum-size SID should fit in the buffer
            //
        
            if ( binaryForm.Length - offset < SecurityIdentifier.MinBinaryLength )
            {
                throw new ArgumentOutOfRangeException(
                    "binaryForm",
                    Environment.GetResourceString( "ArgumentOutOfRange_ArrayTooSmall" ));
            }
            Contract.EndContractBlock();

            IdentifierAuthority Authority;
            int[] SubAuthorities;
        
            //
            // Extract the elements of a SID
            //
        
            if ( binaryForm[offset] != Revision )
            {
                //
                // Revision is incorrect
                //

                throw new ArgumentException(
                    Environment.GetResourceString( "IdentityReference_InvalidSidRevision" ),
                    "binaryForm" );
            }
        
            //
            // Insist on the correct number of subauthorities
            //
        
            if ( binaryForm[offset + 1] > MaxSubAuthorities )
            {
                throw new ArgumentException(
                    Environment.GetResourceString( "IdentityReference_InvalidNumberOfSubauthorities", MaxSubAuthorities ),
                    "binaryForm" );
            }
        
            //
            // Make sure the buffer is big enough
            //
        
            int Length = 1 + 1 + 6 + 4 * binaryForm[offset + 1];
        
            if ( binaryForm.Length - offset < Length )
            {
                throw new ArgumentException(
                    Environment.GetResourceString( "ArgumentOutOfRange_ArrayTooSmall" ),
                    "binaryForm" );
            }
        
            Authority =
                ( IdentifierAuthority )(
                ((( long )binaryForm[offset + 2]) << 40 ) +
                ((( long )binaryForm[offset + 3]) << 32 ) +
                ((( long )binaryForm[offset + 4]) << 24 ) +
                ((( long )binaryForm[offset + 5]) << 16 ) +
                ((( long )binaryForm[offset + 6]) << 8 ) +
                ((( long )binaryForm[offset + 7]) ));
            
            SubAuthorities = new int[binaryForm[offset + 1]];
        
            //
            // Subauthorities are represented in big-endian format
            //
        
            for ( byte i = 0; i < binaryForm[offset + 1]; i++ )
            {
                SubAuthorities[i] =
                    ( int )(
                    ((( uint )binaryForm[offset + 8 + 4*i + 0]) << 0 ) +
                    ((( uint )binaryForm[offset + 8 + 4*i + 1]) << 8 ) +
                    ((( uint )binaryForm[offset + 8 + 4*i + 2]) << 16 ) +
                    ((( uint )binaryForm[offset + 8 + 4*i + 3]) << 24 ));
            }
        
            CreateFromParts( Authority, SubAuthorities );

            return;

        }

        //
        // Constructs a SecurityIdentifier object from its string representation
        // Returns 'null' if string passed in is not a valid SID
        // NOTE: although there is a P/Invoke call involved in the implementation of this method,
        //       there is no security risk involved, so no security demand is being made.
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public SecurityIdentifier( string sddlForm )
        {
            byte[] resultSid;

            //
            // Give us something to work with
            //

            if ( sddlForm == null )
            {
                throw new ArgumentNullException( "sddlForm" );
            }
            Contract.EndContractBlock();

            //
            // Call into the underlying O/S conversion routine
            //

            int Error = Win32.CreateSidFromString( sddlForm, out resultSid );

            if ( Error == Win32Native.ERROR_INVALID_SID )
            {
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidValue" ), "sddlForm" );
            }
            else if ( Error == Win32Native.ERROR_NOT_ENOUGH_MEMORY )
            {
                  throw new OutOfMemoryException();
            }
            else if ( Error != Win32Native.ERROR_SUCCESS )
            {
                Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "Win32.CreateSidFromString returned unrecognized error {0}", Error ));
                throw new SystemException(Win32Native.GetMessage(Error));
            }

            CreateFromBinaryForm( resultSid, 0 );
        }

        //
        // Constructs a SecurityIdentifier object from its binary representation
        //

        public SecurityIdentifier( byte[] binaryForm, int offset )
        {
            CreateFromBinaryForm( binaryForm, offset );
        }

        //
        // Constructs a SecurityIdentifier object from an IntPtr 
        //
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public SecurityIdentifier( IntPtr binaryForm )
            : this( binaryForm, true )
        {
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal SecurityIdentifier( IntPtr binaryForm, bool noDemand )
            : this( Win32.ConvertIntPtrSidToByteArraySid( binaryForm ), 0 )
        {
        }

        //
        // Constructs a well-known SID
        // The 'domainSid' parameter is optional and only used
        // by the well-known types that require it
        // NOTE: although there is a P/Invoke call involved in the implementation of this constructor,
        //       there is no security risk involved, so no security demand is being made.
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public SecurityIdentifier( WellKnownSidType sidType, SecurityIdentifier domainSid )
        {
            //
            // sidType must not be equal to LogonIdsSid
            //

            if (sidType == WellKnownSidType.LogonIdsSid)
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_CannotCreateLogonIdsSid"), "sidType");
            }
            Contract.EndContractBlock();

            byte[] resultSid;
            int Error;

            //
            // Check if well known sids are supported on this platform
            //
            if (!Win32.WellKnownSidApisSupported) {
                throw new PlatformNotSupportedException( Environment.GetResourceString( "PlatformNotSupported_RequiresW2kSP3" ));
            }

            //
            // sidType should not exceed the max defined value
            //

            if ((sidType < WellKnownSidType.NullSid) || (sidType > WellKnownSidType.MaxDefined))
            {
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidValue" ), "sidType" );
            }

            //
            // for sidType between 38 to 50, the domainSid parameter must be specified
            //

            if ((sidType >= WellKnownSidType.AccountAdministratorSid) && (sidType <= WellKnownSidType.AccountRasAndIasServersSid)) 
            {
                if (domainSid == null) {
                    throw new ArgumentNullException( "domainSid", Environment.GetResourceString( "IdentityReference_DomainSidRequired", sidType) );
                }

                //
                // verify that the domain sid is a valid windows domain sid
                // to do that we call GetAccountDomainSid and the return value should be the same as the domainSid
                //

                SecurityIdentifier resultDomainSid;
                int ErrorCode;

                ErrorCode = Win32.GetWindowsAccountDomainSid( domainSid, out resultDomainSid );

                if ( ErrorCode == Win32Native.ERROR_INSUFFICIENT_BUFFER ) {
                    throw new OutOfMemoryException();
                }
                else if ( ErrorCode == Win32Native.ERROR_NON_ACCOUNT_SID )
                {
                    // this means that the domain sid is not valid
                    throw new ArgumentException( Environment.GetResourceString( "IdentityReference_NotAWindowsDomain" ), "domainSid" );
                }
                else if ( ErrorCode != Win32Native.ERROR_SUCCESS )
                {
                    Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "Win32.GetWindowsAccountDomainSid returned unrecognized error {0}", ErrorCode) );
                    throw new SystemException(Win32Native.GetMessage(ErrorCode));
                }

                //
                // if domainSid is passed in as S-1-5-21-3-4-5-6,  the above api will return S-1-5-21-3-4-5 as the domainSid
                // Since these do not match S-1-5-21-3-4-5-6 is not a valid domainSid (wrong number of subauthorities)
                //
                if (resultDomainSid != domainSid) {
                    throw new ArgumentException( Environment.GetResourceString( "IdentityReference_NotAWindowsDomain" ), "domainSid" );
                }
                
            }
            

            Error = Win32.CreateWellKnownSid( sidType, domainSid, out resultSid );

            if ( Error == Win32Native.ERROR_INVALID_PARAMETER )
            {
                throw new ArgumentException( Win32Native.GetMessage(Error), "sidType/domainSid" );
            }
            else if ( Error != Win32Native.ERROR_SUCCESS )
            {
                Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "Win32.CreateWellKnownSid returned unrecognized error {0}", Error ));
                throw new SystemException( Win32Native.GetMessage( Error ));
            }

            CreateFromBinaryForm( resultSid, 0 );
        }

        internal SecurityIdentifier( SecurityIdentifier domainSid, uint rid )
        {
            int i;
            int[] SubAuthorities = new int[ domainSid.SubAuthorityCount + 1 ];

            for ( i = 0; i < domainSid.SubAuthorityCount; i++ )
            {
                SubAuthorities[i] = domainSid.GetSubAuthority( i );
            }

            SubAuthorities[i] = ( int )rid;

            CreateFromParts( domainSid.IdentifierAuthority, SubAuthorities );
        }

        internal SecurityIdentifier( IdentifierAuthority identifierAuthority, int[] subAuthorities )
        {
            CreateFromParts( identifierAuthority, subAuthorities );            
        }      

        #endregion
        
        #region Static Properties

        //
        // Revision is always '1'
        //

        internal static byte Revision
        {
            get { return 1; }
        }
        
        #endregion

        #region Non-static Properties

        //
        // This is for internal consumption only, hence it is marked 'internal'
        // Making this call public would require a deep copy of the data to
        // prevent the caller from messing with the internal representation.
        //

        internal byte[] BinaryForm
        {
            get { return _BinaryForm; }
        }

        internal IdentifierAuthority IdentifierAuthority
        {
            get { return _IdentifierAuthority; }
        }

        internal int SubAuthorityCount
        {
            get { return _SubAuthorities.Length; }
        }

        public int BinaryLength
        {
            get
            {
                return _BinaryForm.Length;
            }
        }

        //
        // Returns the domain portion of a SID or null if the specified
        // SID is not an account SID
        // NOTE: although there is a P/Invoke call involved in the implementation of this method,
        //       there is no security risk involved, so no security demand is being made.
        //

        public SecurityIdentifier AccountDomainSid
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get
            {
                if ( !_AccountDomainSidInitialized ) 
                {    
                    _AccountDomainSid = GetAccountDomainSid();
                    _AccountDomainSidInitialized = true;
                }
                
                return _AccountDomainSid;
            }
        }

        #endregion

        #region Inherited properties and methods

        public override bool Equals( object o )
        {
            if ( o == null )
            {
                return false;
            }

            SecurityIdentifier sid = o as SecurityIdentifier;

            if ( sid == null )
            {
                return false;
            }

            return ( this == sid ); // invokes operator==
        }

        public bool Equals( SecurityIdentifier sid )
        {
            if ( sid == null )
            {
                return false;
            }

            return ( this == sid ); // invokes operator==
        }

        public override int GetHashCode() {
            int hashCode = ((long)this.IdentifierAuthority).GetHashCode();
            for(int i = 0; i < SubAuthorityCount; i++) {
                hashCode ^= this.GetSubAuthority(i);
            }            
            return hashCode;
        }

        public override string ToString()
        {
            if ( _SddlForm == null )
            {
                StringBuilder result = new StringBuilder();

                //
                // Typecasting of _IdentifierAuthority to a long below is important, since
                // otherwise you would see this: "S-1-NTAuthority-32-544"
                //

                result.AppendFormat( "S-1-{0}", ( long )_IdentifierAuthority );
            
                for ( int i = 0; i < SubAuthorityCount; i++ )
                {
                    result.AppendFormat( "-{0}", ( uint )( _SubAuthorities[i] ));
                }

                _SddlForm = result.ToString();
            }

            return _SddlForm;
        }
#if false
        public override string Scheme
        {
            get { return "ms-sid"; }
        }
#endif
        public override string Value
        {
            get { return ToString().ToUpper(CultureInfo.InvariantCulture); }
        }

        internal static bool IsValidTargetTypeStatic( Type targetType )
       { 
            if ( targetType == typeof( NTAccount ))
            {
                return true;
            }
            else if ( targetType == typeof( SecurityIdentifier ))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool IsValidTargetType( Type targetType )
        {
            return IsValidTargetTypeStatic( targetType );
        }


        [System.Security.SecurityCritical]  // auto-generated
        internal SecurityIdentifier GetAccountDomainSid() 
        {
            SecurityIdentifier ResultSid;
            int Error;

            Error = Win32.GetWindowsAccountDomainSid( this, out ResultSid );

            if ( Error == Win32Native.ERROR_INSUFFICIENT_BUFFER ) 
            {
                throw new OutOfMemoryException();
            }
            else if ( Error == Win32Native.ERROR_NON_ACCOUNT_SID )
            {
                ResultSid = null;
            }
            else if ( Error != Win32Native.ERROR_SUCCESS )
            {
                Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "Win32.GetWindowsAccountDomainSid returned unrecognized error {0}", Error) );
                throw new SystemException(Win32Native.GetMessage(Error));
            }
            return ResultSid;
        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        public bool IsAccountSid() {
            
            if ( !_AccountDomainSidInitialized ) 
            {    
                    _AccountDomainSid = GetAccountDomainSid();
                    _AccountDomainSidInitialized = true;
            }

            if (_AccountDomainSid == null) {
                return false;
            }
                
            return true;
        }

        [SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
        [SecuritySafeCritical]
        public override IdentityReference Translate( Type targetType )
        {
            if ( targetType == null ) 
            {
                throw new ArgumentNullException( "targetType" );
            }
            Contract.EndContractBlock();
        
            if ( targetType == typeof( SecurityIdentifier ))
            {
                return this; // assumes SecurityIdentifier objects are immutable
            }
            else if ( targetType == typeof( NTAccount ))
            {
                IdentityReferenceCollection irSource = new IdentityReferenceCollection( 1 );
                irSource.Add( this );
                IdentityReferenceCollection irTarget;

                irTarget = SecurityIdentifier.Translate( irSource, targetType, true );

                return irTarget[0];
            }
            else
            {
                throw new ArgumentException( Environment.GetResourceString( "IdentityReference_MustBeIdentityReference" ), "targetType" );
            }
        }

        #endregion

        #region Operators

        public static bool operator== ( SecurityIdentifier left, SecurityIdentifier right )
        {
            object l = left;
            object r = right;

            if ( l == null && r == null )
            {
                return true;
            }
            else if ( l == null || r == null )
            {
                return false;
            }
            else
            {
                return ( left.CompareTo( right ) == 0 );
            }
        }

        public static bool operator!= ( SecurityIdentifier left, SecurityIdentifier right )
        {
            return !( left == right );
        }

        #endregion

        #region IComparable implementation

        public int CompareTo( SecurityIdentifier sid )
        {
            if ( sid == null )
            {
                throw new ArgumentNullException( "sid" );
            }
            Contract.EndContractBlock();

            if ( this.IdentifierAuthority < sid.IdentifierAuthority )
            {
                return -1;
            }

            if ( this.IdentifierAuthority > sid.IdentifierAuthority )
            {
                return 1;
            }

            if ( this.SubAuthorityCount < sid.SubAuthorityCount )
            {
                return -1;
            }

            if ( this.SubAuthorityCount > sid.SubAuthorityCount )
            {
                return 1;
            }

            for ( int i = 0; i < this.SubAuthorityCount; i++ )
            {
                int diff = this.GetSubAuthority( i ) - sid.GetSubAuthority( i );

                if ( diff != 0 )
                {
                    return diff;
                }
            }

            return 0;
        }

        #endregion

        #region Public Methods

        internal int GetSubAuthority( int index )
        {
            return this._SubAuthorities[ index ];
        }

        //
        // Determines whether this SID is a well known SID of the specified type
        //
        // NOTE: although there is a P/Invoke call involved in the implementation of this method,
        //       there is no security risk involved, so no security demand is being made.
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public bool IsWellKnown( WellKnownSidType type )
        {
            return Win32.IsWellKnownSid( this, type );
        }

        public void GetBinaryForm( byte[] binaryForm, int offset )
        {
            _BinaryForm.CopyTo( binaryForm, offset );
        }

        //
        // NOTE: although there is a P/Invoke call involved in the implementation of this method,
        //       there is no security risk involved, so no security demand is being made.
        //

        [System.Security.SecuritySafeCritical]  // auto-generated
        public bool IsEqualDomainSid( SecurityIdentifier sid )
        {
            return Win32.IsEqualDomainSid( this, sid );
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static IdentityReferenceCollection TranslateToNTAccounts( IdentityReferenceCollection sourceSids, out bool someFailed )
        {
            if (sourceSids == null)
            {
                throw new ArgumentNullException("sourceSids");
            }

            if (sourceSids.Count == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyCollection"), "sourceSids");
            }
            Contract.EndContractBlock();

            IntPtr[] SidArrayPtr = new IntPtr[sourceSids.Count];
            GCHandle[] HandleArray = new GCHandle[ sourceSids.Count ];
            SafeLsaPolicyHandle LsaHandle = SafeLsaPolicyHandle.InvalidHandle;
            SafeLsaMemoryHandle ReferencedDomainsPtr = SafeLsaMemoryHandle.InvalidHandle;
            SafeLsaMemoryHandle NamesPtr = SafeLsaMemoryHandle.InvalidHandle;
 
            try
            {
                //
                // Pin all elements in the array of SIDs
                //

                int currentSid = 0;
                foreach ( IdentityReference id in sourceSids )
                {
                    SecurityIdentifier sid = id as SecurityIdentifier;

                    if ( sid == null )
                    {
                        throw new ArgumentException( Environment.GetResourceString( "Argument_ImproperType" ), "sourceSids" );
                    }

                    HandleArray[currentSid] = GCHandle.Alloc(sid.BinaryForm, GCHandleType.Pinned);
                    SidArrayPtr[currentSid] = HandleArray[currentSid].AddrOfPinnedObject();
                    currentSid++;
                }

                //
                // Open LSA policy (for lookup requires it)
                //

                LsaHandle = Win32.LsaOpenPolicy( null, PolicyRights.POLICY_LOOKUP_NAMES );

                //
                // Perform the actual lookup
                //

                someFailed = false;
                uint ReturnCode;
                ReturnCode = Win32Native.LsaLookupSids( LsaHandle, sourceSids.Count, SidArrayPtr, ref ReferencedDomainsPtr, ref NamesPtr );
                
                //
                // Make a decision regarding whether it makes sense to proceed
                // based on the return code and the value of the forceSuccess argument
                //

                if ( ReturnCode == Win32Native.STATUS_NO_MEMORY ||
                    ReturnCode == Win32Native.STATUS_INSUFFICIENT_RESOURCES )
                {
                    throw new OutOfMemoryException();
                }
                else  if ( ReturnCode == Win32Native.STATUS_ACCESS_DENIED )
                {
                    throw new UnauthorizedAccessException();
                }
                else if ( ReturnCode == Win32Native.STATUS_NONE_MAPPED ||
                    ReturnCode == Win32Native.STATUS_SOME_NOT_MAPPED )
                {
                    someFailed = true;
                }
                else if ( ReturnCode != 0 )
                {
                    int win32ErrorCode = Win32Native.LsaNtStatusToWinError(unchecked((int)ReturnCode));
                    
                    Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "Win32Native.LsaLookupSids returned {0}", win32ErrorCode));
                    throw new SystemException(Win32Native.GetMessage(win32ErrorCode));
                }

                
                NamesPtr.Initialize((uint)sourceSids.Count, (uint)Marshal.SizeOf(typeof(Win32Native.LSA_TRANSLATED_NAME)));
                Win32.InitializeReferencedDomainsPointer(ReferencedDomainsPtr);

                //
                // Interpret the results and generate NTAccount objects
                //

                IdentityReferenceCollection Result = new IdentityReferenceCollection( sourceSids.Count );

                if ( ReturnCode == 0 || ReturnCode == Win32Native.STATUS_SOME_NOT_MAPPED )
                {
                    //
                    // Interpret the results and generate NT Account objects
                    //

                    Win32Native.LSA_REFERENCED_DOMAIN_LIST rdl = ReferencedDomainsPtr.Read<Win32Native.LSA_REFERENCED_DOMAIN_LIST>(0);
                    string[] ReferencedDomains = new string[ rdl.Entries ];

                    for (int i = 0; i < rdl.Entries; i++)
                    {
                        Win32Native.LSA_TRUST_INFORMATION ti = ( Win32Native.LSA_TRUST_INFORMATION )Marshal.PtrToStructure( new IntPtr(( long )rdl.Domains + i * Marshal.SizeOf( typeof( Win32Native.LSA_TRUST_INFORMATION ))), typeof( Win32Native.LSA_TRUST_INFORMATION ));
                        ReferencedDomains[i] = Marshal.PtrToStringUni(ti.Name.Buffer, ti.Name.Length / sizeof(char));
                    }

                    Win32Native.LSA_TRANSLATED_NAME[] translatedNames = new Win32Native.LSA_TRANSLATED_NAME[sourceSids.Count];
                    NamesPtr.ReadArray(0, translatedNames, 0, translatedNames.Length);

                    for (int i = 0; i < sourceSids.Count; i++)
                    {        
                        Win32Native.LSA_TRANSLATED_NAME Ltn = translatedNames[i];

                        switch ((SidNameUse)Ltn.Use)
                        {
                            case SidNameUse.User:
                            case SidNameUse.Group:
                            case SidNameUse.Alias:
                            case SidNameUse.Computer:
                            case SidNameUse.WellKnownGroup:
                                string account = Marshal.PtrToStringUni(Ltn.Name.Buffer, Ltn.Name.Length / sizeof(char)); ;
                                string domain = ReferencedDomains[Ltn.DomainIndex];
                                Result.Add( new NTAccount( domain, account ));
                                break;

                            default:
                                someFailed = true;
                                Result.Add( sourceSids[i] );
                                break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sourceSids.Count; i++)
                    {
                        Result.Add( sourceSids[i] );
                    }
                }
                
                return Result;       
            }
            finally
            {
                for (int i = 0; i < sourceSids.Count; i++)
                {
                    if ( HandleArray[i].IsAllocated )
                    {
                        HandleArray[i].Free();
                    }
                }

                LsaHandle.Dispose();
                ReferencedDomainsPtr.Dispose();
                NamesPtr.Dispose();
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static IdentityReferenceCollection Translate( IdentityReferenceCollection sourceSids, Type targetType, bool forceSuccess)
        {
            bool SomeFailed = false;
            IdentityReferenceCollection Result;
        
            
            Result = Translate( sourceSids, targetType, out SomeFailed );

            if (forceSuccess && SomeFailed) {

                IdentityReferenceCollection UnmappedIdentities = new IdentityReferenceCollection();

                foreach (IdentityReference id in Result) 
                {    
                    if (id.GetType() != targetType) 
                    {
                        UnmappedIdentities.Add(id);
                    }
                }

                throw new IdentityNotMappedException(Environment.GetResourceString("IdentityReference_IdentityNotMapped"), UnmappedIdentities);
                
            }

            return Result;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static IdentityReferenceCollection Translate( IdentityReferenceCollection sourceSids, Type targetType, out bool someFailed )
        {
            if ( sourceSids == null )
            {
                throw new ArgumentNullException( "sourceSids" );
            }
            Contract.EndContractBlock();

            if ( targetType == typeof( NTAccount ))
            {
                return TranslateToNTAccounts( sourceSids, out someFailed );
            }

            throw new ArgumentException( Environment.GetResourceString( "IdentityReference_MustBeIdentityReference" ), "targetType" );
        }

        #endregion
    }
}
