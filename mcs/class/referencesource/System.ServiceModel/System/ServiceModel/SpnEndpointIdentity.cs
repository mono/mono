//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.DirectoryServices;
    using System.IdentityModel.Claims;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Xml;

    public class SpnEndpointIdentity : EndpointIdentity
    {
        static TimeSpan spnLookupTime = TimeSpan.FromMinutes(1);

        SecurityIdentifier spnSid;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile bool hasSpnSidBeenComputed;

        Object thisLock = new Object();

        static Object typeLock = new Object();

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile DirectoryEntry directoryEntry;

        public static TimeSpan SpnLookupTime
        {
            get
            {
                return spnLookupTime;
            }
            set
            {
                if (value.Ticks < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value.Ticks,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
                }
                spnLookupTime = value;
            }
        }

        public SpnEndpointIdentity(string spnName)
        {
            if (spnName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("spnName");

            base.Initialize(Claim.CreateSpnClaim(spnName));
        }

        public SpnEndpointIdentity(Claim identity)
        {
            if (identity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");

            // PreSharp Bug: Parameter 'identity.ResourceType' to this public method must be validated: A null-dereference can occur here.
#pragma warning suppress 56506 // Claim.ClaimType will never return null
            if (!identity.ClaimType.Equals(ClaimTypes.Spn))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.UnrecognizedClaimTypeForIdentity, identity.ClaimType, ClaimTypes.Spn));

            base.Initialize(identity);
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");

            writer.WriteElementString(XD.AddressingDictionary.Spn, XD.AddressingDictionary.IdentityExtensionNamespace, (string)this.IdentityClaim.Resource);
        }

        internal SecurityIdentifier GetSpnSid()
        {
            Fx.Assert(ClaimTypes.Spn.Equals(this.IdentityClaim.ClaimType) || ClaimTypes.Dns.Equals(this.IdentityClaim.ClaimType), "");
            if (!hasSpnSidBeenComputed)
            {
                lock (thisLock)
                {
                    if (!hasSpnSidBeenComputed)
                    {
                        string spn = null;
                        try
                        {

                            if (ClaimTypes.Dns.Equals(this.IdentityClaim.ClaimType))
                            {
                                spn = "host/" + (string)this.IdentityClaim.Resource;
                            }
                            else
                            {
                                spn = (string)this.IdentityClaim.Resource;
                            }
                            // canonicalize SPN for use in LDAP filter following RFC 1960:
                            if (spn != null)
                            {
                                spn = spn.Replace("*", @"\*").Replace("(", @"\(").Replace(")", @"\)");
                            }

                            DirectoryEntry de = GetDirectoryEntry();
                            using (DirectorySearcher searcher = new DirectorySearcher(de))
                            {
                                searcher.CacheResults = true;
                                searcher.ClientTimeout = SpnLookupTime;
                                searcher.Filter = "(&(objectCategory=Computer)(objectClass=computer)(servicePrincipalName=" + spn + "))";
                                searcher.PropertiesToLoad.Add("objectSid");
                                SearchResult result = searcher.FindOne();
                                if (result != null)
                                {
                                    byte[] sidBinaryForm = (byte[])result.Properties["objectSid"][0];
                                    this.spnSid = new SecurityIdentifier(sidBinaryForm, 0);
                                }
                                else
                                {
                                    SecurityTraceRecordHelper.TraceSpnToSidMappingFailure(spn, null);
                                }
                            }
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            // Always immediately rethrow fatal exceptions.
                            if (Fx.IsFatal(e)) throw;

                            if (e is NullReferenceException || e is SEHException)
                                throw;

                            SecurityTraceRecordHelper.TraceSpnToSidMappingFailure(spn, e);
                        }
                        finally
                        {
                            hasSpnSidBeenComputed = true;
                        }
                    }
                }
            }
            return this.spnSid;
        }

        static DirectoryEntry GetDirectoryEntry()
        {
            if (directoryEntry == null)
            {
                lock (typeLock)
                {
                    if (directoryEntry == null)
                    {
                        DirectoryEntry tmp = new DirectoryEntry(@"LDAP://" + SecurityUtils.GetPrimaryDomain());
                        tmp.RefreshCache(new string[] { "name" });
                        directoryEntry = tmp;
                    }
                }
            }
            return directoryEntry;
        }
    }

}
