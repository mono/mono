//
// System.Security.Policy.UnionCodeGroup.cs
//
// Author
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Security.Policy {

        [Serializable]
        public sealed class UnionCodeGroup : CodeGroup {

                public UnionCodeGroup (
                        IMembershipCondition membershipCondition,
                        PolicyStatement policyStatement)
                        : base (membershipCondition, policyStatement)
                {
                }

		// for PolicyLevel (to avoid validation duplication)
		internal UnionCodeGroup (SecurityElement e) : base (e) {}

                public override CodeGroup Copy ()
                {
                        UnionCodeGroup copy = new UnionCodeGroup (MembershipCondition, PolicyStatement);
			foreach (CodeGroup child in Children) {
				copy.AddChild (child.Copy ());	// deep copy
			}
			return copy;
                }

                [MonoTODO]
                public override PolicyStatement Resolve (Evidence evidence)
                {
                        if (evidence == null)
                                throw new ArgumentNullException ("evidence");

                        throw new NotImplementedException ();
                }

                [MonoTODO]
                public override CodeGroup ResolveMatchingCodeGroups (Evidence evidence)
                {
                        if (evidence == null)
				throw new ArgumentNullException ("evidence");

                        throw new NotImplementedException ();
                }

                public override string MergeLogic {
                        get {
                                return "Union";
                        }
                }
        }
}
