// System.Security.Policy.FirstMatchCodeGroup
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.

using System;

namespace System.Security.Policy {

	public sealed class FirstMatchCodeGroup : CodeGroup {
		
		public FirstMatchCodeGroup(IMembershipCondition membershipCondition, PolicyStatement policy) :
			base (membershipCondition, policy)
		{
		}

		//
		// Public Properties
		//

		public override string MergeLogic
		{
			get { return "First Match"; }
		}

		//
		// Public Methods
		//
		
		public override CodeGroup Copy()
		{
			FirstMatchCodeGroup copy = CopyNoChildren ();

			foreach (CodeGroup group in Children) {
				copy.AddChild ( group );
			}

			return copy;
		}

		public override PolicyStatement Resolve(Evidence evidence)
		{
			PolicyStatement policy = null;
			PolicyStatement child_policy;

			if (null == evidence)
				throw new ArgumentNullException ();

			if (MembershipCondition.Check (evidence)) {
				if (null != PolicyStatement) {
					policy = PolicyStatement;
				} else {
					// Loop through all children breaking on the first one that resolves
					foreach (CodeGroup child in Children) {
						if (null == (child_policy = child.Resolve (evidence)))
							continue;
						policy = child_policy;
						break;
					}
				}
			}
			
			return policy;
		}
		
		public override CodeGroup ResolveMatchingCodeGroups(Evidence evidence)
		{
			CodeGroup group = null;

			if (null == evidence)
				throw new ArgumentNullException ();
			
			if (MembershipCondition.Check (evidence)) {
				group = CopyNoChildren ();
				
				// Add the first child that resolves
				foreach (CodeGroup child in Children) {
					if ( null == child.Resolve (evidence))
						continue;
					group.AddChild (child);
					break;
				}
			}

			return group;
		}
	
		//
		// Private Methods
		//
	
		private FirstMatchCodeGroup CopyNoChildren()
		{
			FirstMatchCodeGroup copy = new FirstMatchCodeGroup (MembershipCondition, PolicyStatement);

			copy.Name = Name;
			copy.Description = Description;

			return copy;
		}
	}

}

