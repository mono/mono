//
// System.Security.Policy.NetCodeGroup
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.Security.Policy {

	[Serializable]
	public sealed class NetCodeGroup : CodeGroup {

		public NetCodeGroup (IMembershipCondition condition) 
			: base (condition, null) {}

		// for PolicyLevel (to avoid validation duplication)
		internal NetCodeGroup (SecurityElement e) : base (e) {}
	
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
			get { return "Same site Web."; }
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
				copy.AddChild (child.Copy ());	// deep copy
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

