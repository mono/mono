// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// PolicyManager.cs
//

namespace System.Security {
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Versioning;
    using System.Security.Util;
    using System.Security.Policy;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Diagnostics.Contracts;

    internal class PolicyManager {
        // Only parse the system CAS policy levels when needed. In particular,
        // we do not use these when the AppDomain is homogeneous for example.
        private object m_policyLevels;
        private IList PolicyLevels {
            [System.Security.SecurityCritical]  // auto-generated
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                if (m_policyLevels == null) {
                    ArrayList policyLevels = new ArrayList();

                    string enterpriseConfig = PolicyLevel.GetLocationFromType(System.Security.PolicyLevelType.Enterprise);
                    policyLevels.Add(new PolicyLevel(System.Security.PolicyLevelType.Enterprise, enterpriseConfig, ConfigId.EnterprisePolicyLevel));

                    string machineConfig = PolicyLevel.GetLocationFromType(System.Security.PolicyLevelType.Machine);
                    policyLevels.Add(new PolicyLevel(System.Security.PolicyLevelType.Machine, machineConfig, ConfigId.MachinePolicyLevel));

                    // The user directory could be null if the user does not have a user profile for example.
                    if (Config.UserDirectory != null) {
                        string userConfig = PolicyLevel.GetLocationFromType(System.Security.PolicyLevelType.User);
                        policyLevels.Add(new PolicyLevel(System.Security.PolicyLevelType.User, userConfig, ConfigId.UserPolicyLevel));
                    }
                    Interlocked.CompareExchange(ref m_policyLevels, policyLevels, null);
                }
                return m_policyLevels as ArrayList;
            }
        }

        internal PolicyManager() {}

        [System.Security.SecurityCritical]  // auto-generated
        internal void AddLevel (PolicyLevel level) {
            PolicyLevels.Add(level);
        }

        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
        internal IEnumerator PolicyHierarchy() {
            return PolicyLevels.GetEnumerator();
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal PermissionSet Resolve(Evidence evidence)
        {
            // If we can resolve the grant set for the evidence via the host or the current AppDomain state,
            // then return that.  Otherwise, call back out to code group resolution.
            PermissionSet grantSet = null;
            if (CodeAccessSecurityEngine.TryResolveGrantSet(evidence, out grantSet))
            {
                return grantSet;
            }
            else 
            {
                BCLDebug.Assert(AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled, "We're about to apply policy in a policy disabled app");
                return CodeGroupResolve(evidence, false);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.AppDomain, ResourceScope.AppDomain)]
        internal PermissionSet CodeGroupResolve (Evidence evidence, bool systemPolicy) {
            Contract.Assert(AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled);

            PermissionSet grant = null;
            PolicyStatement policy;
            PolicyLevel currentLevel = null;

            IEnumerator levelEnumerator = PolicyLevels.GetEnumerator();

            // We're optimized for standard policy, where the only evidence that is generally evaluated are
            // Zone, StrongName and Url.  Since all of these are relatively inexpensive, we'll force them to
            // generate, then use that as a key into the cache.
            evidence.GetHostEvidence<Zone>();
            evidence.GetHostEvidence<StrongName>();
            evidence.GetHostEvidence<Url>();
            byte[] serializedEvidence = evidence.RawSerialize();
            int count = evidence.RawCount;

            bool legacyIgnoreSystemPolicy = (AppDomain.CurrentDomain.GetData("IgnoreSystemPolicy") != null);
            bool testApplicationLevels = false;
            while (levelEnumerator.MoveNext())
            {
                currentLevel = (PolicyLevel)levelEnumerator.Current;
                if (systemPolicy) {
                    if (currentLevel.Type == PolicyLevelType.AppDomain)
                        continue;
                } else if (legacyIgnoreSystemPolicy && currentLevel.Type != PolicyLevelType.AppDomain)
                    continue;

                policy = currentLevel.Resolve(evidence, count, serializedEvidence);

                // If the grant is "AllPossible", the intersection is just the other permission set.
                // Otherwise, do an inplace intersection (since we know we can alter the grant set since
                // it is a copy of the first policy statement's permission set).

                if (grant == null)
                    grant = policy.PermissionSet;
                else
                    grant.InplaceIntersect(policy.GetPermissionSetNoCopy());

                if (grant == null || grant.FastIsEmpty())
                {
                    break;
                }
                else if ((policy.Attributes & PolicyStatementAttribute.LevelFinal) == PolicyStatementAttribute.LevelFinal)
                {
                    if (currentLevel.Type != PolicyLevelType.AppDomain)
                    {
                        testApplicationLevels = true;
                    }
                    break;
                }
            }

            if (grant != null && testApplicationLevels)
            {
                PolicyLevel appDomainLevel = null;

                for (int i = PolicyLevels.Count - 1; i >= 0; --i)
                {
                    currentLevel = (PolicyLevel) PolicyLevels[i];
                    if (currentLevel.Type == PolicyLevelType.AppDomain)
                    {
                        appDomainLevel = currentLevel;
                        break;
                    }
                }

                if (appDomainLevel != null)
                {
                    policy = appDomainLevel.Resolve(evidence, count, serializedEvidence);
                    grant.InplaceIntersect(policy.GetPermissionSetNoCopy());
                }
            }

            if (grant == null)
                grant = new PermissionSet(PermissionState.None);

            // Each piece of evidence can possibly create an identity permission that we
            // need to add to our grant set.  Therefore, for all pieces of evidence that
            // implement the IIdentityPermissionFactory interface, ask it for its
            // adjoining identity permission and add it to the grant.

            if (!grant.IsUnrestricted())
            {
                IEnumerator enumerator = evidence.GetHostEnumerator();
                while (enumerator.MoveNext())
                {
                    Object obj = enumerator.Current;
                    IIdentityPermissionFactory factory = obj as IIdentityPermissionFactory;
                    if (factory != null)
                    {
                        IPermission perm = factory.CreateIdentityPermission( evidence );
                        if (perm != null)
                            grant.AddPermission( perm );
                    }
                }
            }

            grant.IgnoreTypeLoadFailures = true;
            return grant;
        }

        internal static bool IsGacAssembly (Evidence evidence) {
            return new GacMembershipCondition().Check(evidence);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal IEnumerator ResolveCodeGroups (Evidence evidence) {
            ArrayList accumList = new ArrayList();
            IEnumerator levelEnumerator = PolicyLevels.GetEnumerator();

            while (levelEnumerator.MoveNext())
            {
                CodeGroup temp = ((PolicyLevel)levelEnumerator.Current).ResolveMatchingCodeGroups(evidence);
                if (temp != null)
                    accumList.Add(temp);
            }

            return accumList.GetEnumerator(0, accumList.Count);
        }

#pragma warning disable 618
        internal static PolicyStatement ResolveCodeGroup(CodeGroup codeGroup, Evidence evidence)
        {
            // Custom code groups won't know how to mark the evidence they're using, so we need to
            // be pessimistic and mark it all as used if we encounter a code group from outside of mscorlib.
            if (codeGroup.GetType().Assembly != typeof(UnionCodeGroup).Assembly)
            {
                evidence.MarkAllEvidenceAsUsed();
            }

            return codeGroup.Resolve(evidence);
        }
#pragma warning restore 618

        /// <summary>
        ///     Check the membership condition to see if it matches the given evidence, and if the
        ///     membership condition supports it also return the evidence which was used to match the
        ///     membership condition.
        /// </summary>
        internal static bool CheckMembershipCondition(IMembershipCondition membershipCondition,
                                                      Evidence evidence,
                                                      out object usedEvidence) {
            BCLDebug.Assert(membershipCondition != null, "membershipCondition != null");
            BCLDebug.Assert(evidence != null, "evidence != null");

            IReportMatchMembershipCondition reportMatchMembershipCondition = membershipCondition as IReportMatchMembershipCondition;

            // If the membership condition supports telling us which evidence was used to match, then use
            // that capability.  Otherwise, we cannot report this information - which means we need to be
            // conservative and assume that all of the evidence was used and mark it as such.
            if (reportMatchMembershipCondition != null) {
                return reportMatchMembershipCondition.Check(evidence, out usedEvidence);
            }
            else {
                usedEvidence = null;
                evidence.MarkAllEvidenceAsUsed();

                return membershipCondition.Check(evidence);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void Save () {
            EncodeLevel(Environment.GetResourceString("Policy_PL_Enterprise"));
            EncodeLevel(Environment.GetResourceString("Policy_PL_Machine"));
            EncodeLevel(Environment.GetResourceString("Policy_PL_User"));
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void EncodeLevel (string label) {
            for (int i = 0; i < PolicyLevels.Count; ++i)
            {
                PolicyLevel currentLevel = (PolicyLevel) PolicyLevels[i];
                if (currentLevel.Label.Equals(label))
                {
                    EncodeLevel(currentLevel);
                    return;
                }
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal static void EncodeLevel (PolicyLevel level)
        {
            Contract.Assert(level != null, "No policy level to encode.");

            // We cannot encode a policy level without a backing file  
            if (level.Path == null)
            {
                string errorMessage = Environment.GetResourceString("Policy_UnableToSave",
                                                    level.Label,
                                                    Environment.GetResourceString("Policy_SaveNotFileBased"));
                throw new PolicyException(errorMessage);
            }
                
            SecurityElement elConf = new SecurityElement("configuration");
            SecurityElement elMscorlib = new SecurityElement("mscorlib");
            SecurityElement elSecurity = new SecurityElement("security");
            SecurityElement elPolicy = new SecurityElement("policy");

            elConf.AddChild(elMscorlib);
            elMscorlib.AddChild(elSecurity);
            elSecurity.AddChild(elPolicy);
            elPolicy.AddChild(level.ToXml());

            try
            {
                StringBuilder sb = new StringBuilder();
                Encoding encoding = Encoding.UTF8;

                SecurityElement format = new SecurityElement("xml");
                format.m_type = SecurityElementType.Format;
                format.AddAttribute("version", "1.0");
                format.AddAttribute("encoding", encoding.WebName);
                sb.Append(format.ToString());
                sb.Append(elConf.ToString());

                byte[] data = encoding.GetBytes(sb.ToString());

                // Write out the new config.
                int hrSave = Config.SaveDataByte(level.Path, data, data.Length);
                Exception extendedError = Marshal.GetExceptionForHR(hrSave);
                if (extendedError != null)
                {
                    string extendedInformation = extendedError != null ? extendedError.Message : String.Empty;
                    throw new PolicyException(Environment.GetResourceString("Policy_UnableToSave", level.Label, extendedInformation), extendedError);
                }
            }
            catch (Exception e)
            {
                if (e is PolicyException)
                    throw e;
                else
                    throw new PolicyException(Environment.GetResourceString("Policy_UnableToSave", level.Label, e.Message), e);
            }

            Config.ResetCacheData(level.ConfigId);
            if (CanUseQuickCache(level.RootCodeGroup))
                Config.SetQuickCache(level.ConfigId, GenerateQuickCache(level));
        }

        // Here is the managed portion of the QuickCache code.  It
        // is mainly concerned with detecting whether it is valid
        // for us to use the quick cache, and then calculating the
        // proper mapping of partial evidence to partial mapping.
        //
        // The choice of the partial evidence sets is fairly arbitrary
        // and in this case is tailored to give us meaningful
        // results from default policy.
        //
        // The choice of whether or not we can use the quick cache
        // is far from arbitrary.  There are a number of conditions that must
        // be true for the QuickCache to produce valid result.  These
        // are:
        // 
        // * equivalent evidence objects must produce the same
        //   grant set (i.e. it must be independent of time of day,
        //   space on the harddisk, other "external" factors, and
        //   cannot be random).
        //
        // * evidence must be used positively (i.e. if evidence A grants
        //   permission X, then evidence A+B must grant at least permission
        //   X).
        //
        // In particular for our implementation, this means that we
        // limit the classes that can be used by policy to just
        // the ones defined within mscorlib and that there are
        // no Exclusive bits set on any code groups.

        internal static bool CanUseQuickCache (CodeGroup group) {
            ArrayList list = new ArrayList();

            list.Add(group);

            for (int i = 0; i < list.Count; ++i)
            {
                group = (CodeGroup)list[i];

                IUnionSemanticCodeGroup unionGroup = group as IUnionSemanticCodeGroup;

                if (unionGroup != null)
                {
                    if (!TestPolicyStatement(group.PolicyStatement))
                        return false;
                }
                else
                {
                    return false;
                }

                IMembershipCondition cond = group.MembershipCondition;
                if (cond != null && !(cond is IConstantMembershipCondition))
                {
                    return false;
                }

                IList children = group.Children;

                if (children != null && children.Count > 0)
                {
                    IEnumerator enumerator = children.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        list.Add(enumerator.Current);
                    }
                }
            }

            return true;
        }

        private static bool TestPolicyStatement (PolicyStatement policy) {
            if (policy == null)
                return true;
            return (policy.Attributes & PolicyStatementAttribute.Exclusive) == 0;
        }

        private volatile static QuickCacheEntryType[] FullTrustMap;
        private static QuickCacheEntryType GenerateQuickCache(PolicyLevel level)
        {
            if (FullTrustMap == null)
            {
                // This mapping must stay in sync with the SecurityZone enumeration in SecurityZone.cs
                FullTrustMap = new QuickCacheEntryType[]
                {
                    QuickCacheEntryType.FullTrustZoneMyComputer,
                  QuickCacheEntryType.FullTrustZoneIntranet,
                    QuickCacheEntryType.FullTrustZoneTrusted,
                  QuickCacheEntryType.FullTrustZoneInternet,
                    QuickCacheEntryType.FullTrustZoneUntrusted
                };
            }

            QuickCacheEntryType accumulator = (QuickCacheEntryType)0;

            Evidence noEvidence = new Evidence();

            PermissionSet policy = null;

            try
            {
                policy = level.Resolve( noEvidence ).PermissionSet;
                if (policy.IsUnrestricted())
                    accumulator |= QuickCacheEntryType.FullTrustAll;
            }
            catch (PolicyException)
            {
            }

            foreach (SecurityZone zone in Enum.GetValues(typeof(SecurityZone)))
            {
                if (zone == SecurityZone.NoZone)
                    continue;

                Evidence zoneEvidence = new Evidence();
                zoneEvidence.AddHostEvidence(new Zone(zone));

                PermissionSet zonePolicy = null;

                try
                {
                    zonePolicy = level.Resolve( zoneEvidence ).PermissionSet;
                    if (zonePolicy.IsUnrestricted())
                    {
                        Contract.Assert(0 <= (int)zone && (int)zone < FullTrustMap.Length, "FullTrustMap does not contain a mapping for this zone.");
                        accumulator |= FullTrustMap[(int)zone];
                    }
                }
                catch (PolicyException)
                {
                }
            }

            return accumulator;
        }

#if _DEBUG
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        internal static extern int DebugOut(String file, String message);
#endif
    }
}
