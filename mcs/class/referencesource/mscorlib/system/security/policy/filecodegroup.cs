// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// FileCodeGroup.cs
//
// Representation for code groups used for the policy mechanism.
//

namespace System.Security.Policy {
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Security.Util;
    using System.Runtime.Serialization;
    using System.Runtime.Versioning;
    using System.Diagnostics.Contracts;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    sealed public class FileCodeGroup : CodeGroup, IUnionSemanticCodeGroup {
        private FileIOPermissionAccess m_access;

        internal FileCodeGroup() : base() {}

        public FileCodeGroup(IMembershipCondition membershipCondition, FileIOPermissionAccess access)
            : base(membershipCondition, (PolicyStatement)null) {
            m_access = access;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public override PolicyStatement Resolve(Evidence evidence) {
            if (evidence == null)
                throw new ArgumentNullException("evidence");
            Contract.EndContractBlock();

            object usedEvidence = null;

            if (PolicyManager.CheckMembershipCondition(MembershipCondition, evidence, out usedEvidence)) {
                PolicyStatement thisPolicy = CalculateAssemblyPolicy(evidence);

                // If any delay-evidence was used to generate this grant set, then we need to keep track of
                // that for potentially later forcing it to be verified.
                IDelayEvaluatedEvidence delayEvidence = usedEvidence as IDelayEvaluatedEvidence;
                bool delayEvidenceNeedsVerification = delayEvidence != null && !delayEvidence.IsVerified;
                if (delayEvidenceNeedsVerification) {
                    thisPolicy.AddDependentEvidence(delayEvidence);
                }

                bool foundExclusiveChild = false;
                IEnumerator enumerator = this.Children.GetEnumerator();
                while (enumerator.MoveNext() && !foundExclusiveChild) {
                    PolicyStatement childPolicy = PolicyManager.ResolveCodeGroup(enumerator.Current as CodeGroup,
                                                                                 evidence);

                    if (childPolicy != null) {
                        thisPolicy.InplaceUnion(childPolicy);

                        if ((childPolicy.Attributes & PolicyStatementAttribute.Exclusive) == PolicyStatementAttribute.Exclusive) {
                            foundExclusiveChild = true;
                        }
                    }
                }

                return thisPolicy;
            }
            else {
                return null;
            }
        }

        /// <internalonly/>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        PolicyStatement IUnionSemanticCodeGroup.InternalResolve(Evidence evidence) {
            if (evidence == null)
                throw new ArgumentNullException("evidence");
            Contract.EndContractBlock();

            if (this.MembershipCondition.Check(evidence)) {
                return CalculateAssemblyPolicy(evidence);
            }

            return null;
        }

        public override CodeGroup ResolveMatchingCodeGroups(Evidence evidence) {
            if (evidence == null)
                throw new ArgumentNullException("evidence");
            Contract.EndContractBlock();

            if (this.MembershipCondition.Check(evidence)) {
                CodeGroup retGroup = this.Copy();
                retGroup.Children = new ArrayList();
                IEnumerator enumerator = this.Children.GetEnumerator();
                while (enumerator.MoveNext()) {
                    CodeGroup matchingGroups = ((CodeGroup)enumerator.Current).ResolveMatchingCodeGroups(evidence);
                    // If the child has a policy, we are done.
                    if (matchingGroups != null)
                        retGroup.AddChild(matchingGroups);
                }
                return retGroup;
            }
            else {
                return null;
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal PolicyStatement CalculatePolicy(Url url) {
            URLString urlString = url.GetURLString();
            if (String.Compare(urlString.Scheme, "file", StringComparison.OrdinalIgnoreCase) != 0)
                return null;

            string directory = urlString.GetDirectoryName();
            PermissionSet permSet = new PermissionSet(PermissionState.None);
            permSet.SetPermission(new FileIOPermission(m_access, System.IO.Path.GetFullPath(directory)));

            return new PolicyStatement(permSet, PolicyStatementAttribute.Nothing);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private PolicyStatement CalculateAssemblyPolicy(Evidence evidence) {
            PolicyStatement thisPolicy = null;

            Url url = evidence.GetHostEvidence<Url>();
            if (url != null) {
                thisPolicy = CalculatePolicy(url);
            }

            if (thisPolicy == null) {
                thisPolicy = new PolicyStatement(new PermissionSet(false), PolicyStatementAttribute.Nothing);
            }

            return thisPolicy;
        }

        public override CodeGroup Copy() {
            FileCodeGroup group = new FileCodeGroup(this.MembershipCondition, this.m_access);
            group.Name = this.Name;
            group.Description = this.Description;

            IEnumerator enumerator = this.Children.GetEnumerator();
            while (enumerator.MoveNext()) {
                group.AddChild((CodeGroup)enumerator.Current);
            }
            return group;
        }

        public override string MergeLogic {
            get {
                return Environment.GetResourceString("MergeLogic_Union");
            }
        }

        public override string PermissionSetName {
            get {
                return Environment.GetResourceString("FileCodeGroup_PermissionSet", XMLUtil.BitFieldEnumToString(typeof(FileIOPermissionAccess), m_access));
            }
        }

        public override string AttributeString {
            get {
                return null;
            }
        }

        protected override void CreateXml(SecurityElement element, PolicyLevel level) {
            element.AddAttribute("Access", XMLUtil.BitFieldEnumToString(typeof(FileIOPermissionAccess), m_access));
        }

        protected override void ParseXml(SecurityElement e, PolicyLevel level) {
            string access = e.Attribute("Access");
            if (access != null)
                m_access = (FileIOPermissionAccess) Enum.Parse(typeof(FileIOPermissionAccess), access);
            else
                m_access = FileIOPermissionAccess.NoAccess;
        }

        public override bool Equals(Object o) {
            FileCodeGroup that = (o as FileCodeGroup);
            if (that != null && base.Equals(that)) {
                if (this.m_access == that.m_access)
                    return true;
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode() + m_access.GetHashCode();
        }

        internal override string GetTypeName() {
            return "System.Security.Policy.FileCodeGroup";
        }
    }
}
