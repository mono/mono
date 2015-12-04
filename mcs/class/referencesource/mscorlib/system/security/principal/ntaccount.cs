// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Security.Permissions;
using System.Diagnostics.Contracts;

namespace System.Security.Principal
{
[System.Runtime.InteropServices.ComVisible(false)]
    public sealed class NTAccount : IdentityReference
    {
        #region Private members

        private readonly string _Name;

        //
        // Limit for nt account names for users is 20 while that for groups is 256
        //
        internal const int MaximumAccountNameLength = 256;

        //
        // Limit for dns domain names is 255
        //
        internal const int MaximumDomainNameLength = 255;

        #endregion

        #region Constructors

        public NTAccount( string domainName, string accountName )
        {
            if ( accountName == null )
            {
                throw new ArgumentNullException( "accountName" );
            }

            if ( accountName.Length == 0 )
            {
                throw new ArgumentException( Environment.GetResourceString( "Argument_StringZeroLength" ), "accountName" );
            }

            if ( accountName.Length > MaximumAccountNameLength )
            {
                throw new ArgumentException( Environment.GetResourceString( "IdentityReference_AccountNameTooLong" ), "accountName");  
            }

            if ( domainName != null && domainName.Length > MaximumDomainNameLength )
            {
                throw new ArgumentException( Environment.GetResourceString( "IdentityReference_DomainNameTooLong" ), "domainName");  
            }   
            Contract.EndContractBlock();

            if ( domainName == null || domainName.Length == 0 )
            {
                _Name = accountName;
            }
            else
            {
                _Name = domainName + "\\" + accountName;
            }
        }

        public NTAccount( string name )
        {
            if ( name == null )
            {
                throw new ArgumentNullException( "name" );
            }

            if ( name.Length == 0 )
            {
                throw new ArgumentException( Environment.GetResourceString( "Argument_StringZeroLength" ), "name" );
            }

            if ( name.Length > ( MaximumDomainNameLength + 1 /* '\' */ + MaximumAccountNameLength ))
            {
                throw new ArgumentException( Environment.GetResourceString( "IdentityReference_AccountNameTooLong" ), "name");  
            }
            Contract.EndContractBlock();

            _Name = name;
        }

        #endregion

        #region Inherited properties and methods
        public override string Value
        {
            get { return ToString(); }
        }

        public override bool IsValidTargetType( Type targetType )
        {
            if ( targetType == typeof( SecurityIdentifier ))
            {
                return true;
            }
            else if ( targetType == typeof( NTAccount ))
            {
                return true;
            }
            else
            {
                return false;
            }
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
        
            if ( targetType == typeof( NTAccount ))
            {
                return this; // assumes that NTAccount objects are immutable
            }
            else if ( targetType == typeof( SecurityIdentifier ))
            {
                IdentityReferenceCollection irSource = new IdentityReferenceCollection( 1 );
                irSource.Add( this );
                IdentityReferenceCollection irTarget;

                irTarget = NTAccount.Translate( irSource, targetType, true );

                return irTarget[0];
            }
            else
            {
                throw new ArgumentException( Environment.GetResourceString( "IdentityReference_MustBeIdentityReference" ), "targetType" );
            }
        }

        public override bool Equals( object o )
        {
            if ( o == null )
            {
                return false;
            }

            NTAccount nta = o as NTAccount;

            if ( nta == null )
            {
                return false;
            }

            return ( this == nta ); // invokes operator==
        }

        public override int GetHashCode() {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(_Name);
        }

        public override string ToString()
        {
            return _Name;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static IdentityReferenceCollection Translate( IdentityReferenceCollection sourceAccounts, Type targetType, bool forceSuccess)
        {
            bool SomeFailed = false;
            IdentityReferenceCollection Result;
        
            
            Result = Translate( sourceAccounts, targetType, out SomeFailed );

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
        internal static IdentityReferenceCollection Translate( IdentityReferenceCollection sourceAccounts, Type targetType, out bool someFailed )
        {
            if ( sourceAccounts == null )
            {
                throw new ArgumentNullException( "sourceAccounts" );
            }
            Contract.EndContractBlock();

            if ( targetType == typeof( SecurityIdentifier ))
            {
                return TranslateToSids( sourceAccounts, out someFailed );
            }

            throw new ArgumentException( Environment.GetResourceString( "IdentityReference_MustBeIdentityReference" ), "targetType" );
        }

        #endregion

        #region Operators

        public static bool operator==( NTAccount left, NTAccount right )
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
                return ( left.ToString().Equals(right.ToString(), StringComparison.OrdinalIgnoreCase));
            }
        }

        public static bool operator!=( NTAccount left, NTAccount right )
        {
            return !( left == right ); // invoke operator==
        }

        #endregion

        #region Private methods

        [System.Security.SecurityCritical]  // auto-generated
        private static IdentityReferenceCollection TranslateToSids( IdentityReferenceCollection sourceAccounts, out bool someFailed )
        {
            if ( sourceAccounts == null )
            {
                throw new ArgumentNullException( "sourceAccounts" );
            }

            if ( sourceAccounts.Count == 0 )
            {
                throw new ArgumentException( Environment.GetResourceString( "Arg_EmptyCollection" ), "sourceAccounts" );
            }
            Contract.EndContractBlock();

            SafeLsaPolicyHandle LsaHandle = SafeLsaPolicyHandle.InvalidHandle;
            SafeLsaMemoryHandle ReferencedDomainsPtr = SafeLsaMemoryHandle.InvalidHandle;
            SafeLsaMemoryHandle SidsPtr = SafeLsaMemoryHandle.InvalidHandle;

            try
            {
                //
                // Construct an array of unicode strings
                //

                Win32Native.UNICODE_STRING[] Names = new Win32Native.UNICODE_STRING[ sourceAccounts.Count ];

                int currentName = 0;
                foreach ( IdentityReference id in sourceAccounts )
                {
                    NTAccount nta = id as NTAccount;

                    if ( nta == null )
                    {
                        throw new ArgumentException(  Environment.GetResourceString( "Argument_ImproperType" ), "sourceAccounts" );
                    }

                    Names[currentName].Buffer = nta.ToString();

                    if (Names[currentName].Buffer.Length * 2 + 2 > ushort.MaxValue)
                    {
                        // this should never happen since we are already validating account name length in constructor and 
                        // it is less than this limit
                        Contract.Assert(false, "NTAccount::TranslateToSids - source account name is too long.");
                        throw new SystemException();
                    }

                    Names[currentName].Length = (ushort)(Names[currentName].Buffer.Length * 2);
                    Names[currentName].MaximumLength = (ushort)(Names[currentName].Length + 2);
                    currentName++;
                }

                //
                // Open LSA policy (for lookup requires it)
                //

                LsaHandle = Win32.LsaOpenPolicy( null, PolicyRights.POLICY_LOOKUP_NAMES );

                //
                // Now perform the actual lookup
                //

                someFailed = false;
                uint ReturnCode;

                if ( Win32.LsaLookupNames2Supported )
                {
                    ReturnCode = Win32Native.LsaLookupNames2( LsaHandle, 0, sourceAccounts.Count, Names, ref ReferencedDomainsPtr, ref SidsPtr );
                }
                else
                {
                    ReturnCode = Win32Native.LsaLookupNames( LsaHandle, sourceAccounts.Count, Names, ref ReferencedDomainsPtr, ref SidsPtr );
                }

                //
                // Make a decision regarding whether it makes sense to proceed
                // based on the return code and the value of the forceSuccess argument
                //

                if ( ReturnCode == Win32Native.STATUS_NO_MEMORY ||
                    ReturnCode == Win32Native.STATUS_INSUFFICIENT_RESOURCES )
                {
                    throw new OutOfMemoryException();
                }
                else if ( ReturnCode == Win32Native.STATUS_ACCESS_DENIED )
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

                    if (win32ErrorCode != Win32Native.ERROR_TRUSTED_RELATIONSHIP_FAILURE)
                    {
                        Contract.Assert( false, string.Format( CultureInfo.InvariantCulture, "Win32Native.LsaLookupNames(2) returned unrecognized error {0}", win32ErrorCode ));
                    }
                    
                    throw new SystemException(Win32Native.GetMessage(win32ErrorCode));
                }

                //
                // Interpret the results and generate SID objects
                //

                IdentityReferenceCollection Result = new IdentityReferenceCollection( sourceAccounts.Count );

                if ( ReturnCode == 0 || ReturnCode == Win32Native.STATUS_SOME_NOT_MAPPED )
                {
                    if ( Win32.LsaLookupNames2Supported )
                    {
                        SidsPtr.Initialize((uint)sourceAccounts.Count, (uint)Marshal.SizeOf(typeof(Win32Native.LSA_TRANSLATED_SID2)));
                        Win32.InitializeReferencedDomainsPointer(ReferencedDomainsPtr);
                        Win32Native.LSA_TRANSLATED_SID2[] translatedSids = new Win32Native.LSA_TRANSLATED_SID2[sourceAccounts.Count];
                        SidsPtr.ReadArray(0, translatedSids, 0, translatedSids.Length);

                        for (int i = 0; i < sourceAccounts.Count; i++)
                        {
                            Win32Native.LSA_TRANSLATED_SID2 Lts = translatedSids[i];

                            //
                            // Only some names are recognized as NTAccount objects
                            //

                            switch ((SidNameUse)Lts.Use)
                            {
                                case SidNameUse.User:
                                case SidNameUse.Group:
                                case SidNameUse.Alias:
                                case SidNameUse.Computer:
                                case SidNameUse.WellKnownGroup:
                                    Result.Add( new SecurityIdentifier( Lts.Sid, true ));
                                    break;

                                default:
                                    someFailed = true;                         
                                    Result.Add( sourceAccounts[i] );
                                    break;
                            }
                        }
                    }
                    else
                    {
                        SidsPtr.Initialize((uint)sourceAccounts.Count, (uint)Marshal.SizeOf(typeof(Win32Native.LSA_TRANSLATED_SID)));
                        Win32.InitializeReferencedDomainsPointer(ReferencedDomainsPtr);
                        Win32Native.LSA_REFERENCED_DOMAIN_LIST rdl = ReferencedDomainsPtr.Read<Win32Native.LSA_REFERENCED_DOMAIN_LIST>(0);
                        SecurityIdentifier[] ReferencedDomains = new SecurityIdentifier[ rdl.Entries ];

                        for (int i = 0; i < rdl.Entries; i++)
                        {
                            Win32Native.LSA_TRUST_INFORMATION ti = ( Win32Native.LSA_TRUST_INFORMATION )Marshal.PtrToStructure( new IntPtr(( long )rdl.Domains + i * Marshal.SizeOf( typeof( Win32Native.LSA_TRUST_INFORMATION ))), typeof( Win32Native.LSA_TRUST_INFORMATION ));
                    
                            ReferencedDomains[i] = new SecurityIdentifier( ti.Sid, true );
                        }

                        Win32Native.LSA_TRANSLATED_SID[] translatedSids = new Win32Native.LSA_TRANSLATED_SID[sourceAccounts.Count];
                        SidsPtr.ReadArray(0, translatedSids, 0, translatedSids.Length);

                        for (int i = 0; i < sourceAccounts.Count; i++)
                        {
                            Win32Native.LSA_TRANSLATED_SID Lts = translatedSids[i];

                            switch ((SidNameUse)Lts.Use)
                            {
                                case SidNameUse.User:
                                case SidNameUse.Group:
                                case SidNameUse.Alias:
                                case SidNameUse.Computer:
                                case SidNameUse.WellKnownGroup:
                                    Result.Add( new SecurityIdentifier( ReferencedDomains[ Lts.DomainIndex ], Lts.Rid ));
                                    break;

                                default:
                                   someFailed = true;
                                   Result.Add( sourceAccounts[i] );
                                   break;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sourceAccounts.Count; i++)
                    {
                        Result.Add( sourceAccounts[i] );
                    }
                }

                return Result;
            }
            finally
            {
                LsaHandle.Dispose();
                ReferencedDomainsPtr.Dispose();
                SidsPtr.Dispose();
            }
        }

        #endregion
    }
}
