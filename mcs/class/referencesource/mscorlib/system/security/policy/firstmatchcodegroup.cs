// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  FirstMatchCodeGroup.cs
// 
// <OWNER>[....]</OWNER>
//
//  Representation for code groups used for the policy mechanism
//

namespace System.Security.Policy {
    
    using System;
    using System.Security;
    using System.Security.Util;
    using System.Collections;
    using System.Diagnostics.Contracts;
    
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    [Obsolete("This type is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
    sealed public class FirstMatchCodeGroup : CodeGroup
    {
        internal FirstMatchCodeGroup()
            : base()
        {
        }
        
        public FirstMatchCodeGroup( IMembershipCondition membershipCondition, PolicyStatement policy )
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
            if (PolicyManager.CheckMembershipCondition(MembershipCondition,
                                                       evidence,
                                                       out usedEvidence))
            {
                PolicyStatement childPolicy = null;

                IEnumerator enumerator = this.Children.GetEnumerator();
                
                while (enumerator.MoveNext())
                {
                    childPolicy = PolicyManager.ResolveCodeGroup(enumerator.Current as CodeGroup,
                                                                 evidence);

                    // If the child has a policy, we are done.                    
                    if (childPolicy != null)
                    {
                        break;
                    }
                }

                // If any delay-evidence was used to generate this grant set, then we need to keep track of
                // that for potentially later forcing it to be verified.
                IDelayEvaluatedEvidence delayEvidence = usedEvidence as IDelayEvaluatedEvidence;
                bool delayEvidenceNeedsVerification = delayEvidence != null && !delayEvidence.IsVerified;

                PolicyStatement thisPolicy = this.PolicyStatement; // PolicyStatement getter makes a copy for us

                if (thisPolicy == null)
                {
                    // We didn't add any permissions, but we enabled our children to be evaluated, and
                    // therefore its grant set is dependent on any of our delay evidence.
                    if (delayEvidenceNeedsVerification)
                    {
                        childPolicy = childPolicy.Copy();
                        childPolicy.AddDependentEvidence(delayEvidence);
                    }

                    return childPolicy;
                }
                else if (childPolicy != null)
                {
                    // Combine the child and this policy and return it.

                    PolicyStatement combined = thisPolicy.Copy();

                    if (delayEvidenceNeedsVerification)
                    {
                        combined.AddDependentEvidence(delayEvidence);
                    }

                    combined.InplaceUnion(childPolicy);
                    return combined;
                }
                else
                {  
                    // Otherwise we just copy the this policy.
                    if (delayEvidenceNeedsVerification)
                    {
                        thisPolicy.AddDependentEvidence(delayEvidence);
                    }

                    return thisPolicy;
                }
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
                        break;
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
            FirstMatchCodeGroup group = new FirstMatchCodeGroup();
            
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
                return Environment.GetResourceString( "MergeLogic_FirstMatch" );
            }
        }     
    
        internal override String GetTypeName()
        {
            return "System.Security.Policy.FirstMatchCodeGroup";
        }
    
    }   
        

}
