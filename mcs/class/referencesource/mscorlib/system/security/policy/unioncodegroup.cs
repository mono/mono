// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  UnionCodeGroup.cs
// 
// <OWNER>Microsoft</OWNER>
//
//  Representation for code groups used for the policy mechanism
//

namespace System.Security.Policy {
    
    using System;
    using System.Security.Util;
    using System.Security;
    using System.Collections;
    using System.Diagnostics.Contracts;
    
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    [Obsolete("This type is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
    sealed public class UnionCodeGroup : CodeGroup, IUnionSemanticCodeGroup
    {
        internal UnionCodeGroup()
            : base()
        {
        }
        
        internal UnionCodeGroup( IMembershipCondition membershipCondition, PermissionSet permSet )
            : base( membershipCondition, permSet )
        {
        }
        
        public UnionCodeGroup( IMembershipCondition membershipCondition, PolicyStatement policy )
            : base( membershipCondition, policy )
        {
        }
        
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        public override PolicyStatement Resolve( Evidence evidence )
        {
            if (evidence == null)
                throw new ArgumentNullException("evidence");
            Contract.EndContractBlock();

            object usedEvidence = null;
            if (PolicyManager.CheckMembershipCondition(MembershipCondition, evidence, out usedEvidence))
            {
                PolicyStatement thisPolicy = PolicyStatement;   // PolicyStatement getter makes a copy for us

                // If any delay-evidence was used to generate this grant set, then we need to keep track of
                // that for potentially later forcing it to be verified.
                IDelayEvaluatedEvidence delayEvidence = usedEvidence as IDelayEvaluatedEvidence;
                bool delayEvidenceNeedsVerification = delayEvidence != null && !delayEvidence.IsVerified;
                if (delayEvidenceNeedsVerification)
                {
                    thisPolicy.AddDependentEvidence(delayEvidence);
                }

                bool foundExclusiveChild = false;
                IEnumerator enumerator = this.Children.GetEnumerator();
                while (enumerator.MoveNext() && !foundExclusiveChild)
                {
                    PolicyStatement childPolicy = PolicyManager.ResolveCodeGroup(enumerator.Current as CodeGroup,
                                                                                 evidence);

                    if (childPolicy != null)
                    {
                        thisPolicy.InplaceUnion(childPolicy);

                        if ((childPolicy.Attributes & PolicyStatementAttribute.Exclusive) == PolicyStatementAttribute.Exclusive)
                        {
                            foundExclusiveChild = true;
                        }
                    }
                }

                return thisPolicy;
            }           
            else
            {
                return null;
            }
        }        

        /// <internalonly/>
        PolicyStatement IUnionSemanticCodeGroup.InternalResolve( Evidence evidence )
        {
            if (evidence == null)
                throw new ArgumentNullException("evidence");
            Contract.EndContractBlock();
                
            if (this.MembershipCondition.Check( evidence ))
            {
                return this.PolicyStatement;
            }
            else
            {
                return null;
            }        
        }

        public override CodeGroup ResolveMatchingCodeGroups( Evidence evidence )
        {
            if (evidence == null)
                throw new ArgumentNullException("evidence");
            Contract.EndContractBlock();

            if (this.MembershipCondition.Check( evidence ))
            {
                CodeGroup retGroup = this.Copy();

                retGroup.Children = new ArrayList();

                IEnumerator enumerator = this.Children.GetEnumerator();
                
                while (enumerator.MoveNext())
                {
                    CodeGroup matchingGroups = ((CodeGroup)enumerator.Current).ResolveMatchingCodeGroups( evidence );
                    
                    // If the child has a policy, we are done.
                    
                    if (matchingGroups != null)
                    {
                        retGroup.AddChild( matchingGroups );
                    }
                }

                return retGroup;
                
            }
            else
            {
                return null;
            }
        }


        public override CodeGroup Copy()
        {
            UnionCodeGroup group = new UnionCodeGroup();
            
            group.MembershipCondition = this.MembershipCondition;
            group.PolicyStatement = this.PolicyStatement;
            group.Name = this.Name;
            group.Description = this.Description;

            IEnumerator enumerator = this.Children.GetEnumerator();

            while (enumerator.MoveNext())
            {
                group.AddChild( (CodeGroup)enumerator.Current );
            }

            
            return group;
        }
        
        public override String MergeLogic
        {
            get
            {
                return Environment.GetResourceString( "MergeLogic_Union" );
            }
        }
        
        internal override String GetTypeName()
        {
            return "System.Security.Policy.UnionCodeGroup";
        }
    
    }                

}
