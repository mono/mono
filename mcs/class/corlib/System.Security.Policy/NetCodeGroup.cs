//
// System.Security.Policy.NetCodeGroup
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved
//

using System;

namespace System.Security.Policy {

	public sealed class NetCodeGroup : CodeGroup {

		public NetCodeGroup (IMembershipCondition condition) 
			: base (condition,null) 
		{
		}
	
		//
		// Public Properties
		//

		public override string AttributeString {
			get { return null; }
		}
	
		public override string MergeLogic {
			get { return "Union"; }
		}

		public override string PermissionSetName {
			get { return "Same site Web"; }
		}


		//
		// Public Methods
		//

		public override CodeGroup Copy ()
		{
			NetCodeGroup copy = new NetCodeGroup (MembershipCondition);
	
			copy.Name = Name;
			copy.Description = Description;
			copy.PolicyStatement = PolicyStatement;		

			foreach (CodeGroup child in Children) {
				copy.AddChild (child);	
			}

			return copy;	
		}

		[MonoTODO]
		public override PolicyStatement Resolve (Evidence evidence)
		{
			if (evidence == null) 
				throw new ArgumentNullException ();

			throw new NotImplementedException ();
		}
	
		public override CodeGroup ResolveMatchingCodeGroups (Evidence evidence) 
		{
			if (evidence == null)
				throw new ArgumentNullException ();
			
			CodeGroup return_group = null;
			if (MembershipCondition.Check (evidence)) {
				return_group = Copy ();

				foreach (CodeGroup child_group in Children) {
					CodeGroup matching = 
						child_group.ResolveMatchingCodeGroups (evidence);
					if (matching == null)
						continue;
					return_group.AddChild (matching);
				}
			}

			return return_group;
		}
	}
}

