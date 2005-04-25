//
// System.Security.Policy.FirstMatchCodeGroup
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Security.Policy {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class FirstMatchCodeGroup : CodeGroup {
		
		public FirstMatchCodeGroup (IMembershipCondition membershipCondition, PolicyStatement policy)
			: base (membershipCondition, policy)
		{
		}

		// for PolicyLevel (to avoid validation duplication)
		internal FirstMatchCodeGroup (SecurityElement e, PolicyLevel level)
			: base (e, level)
		{
		}

		//
		// Public Properties
		//

		public override string MergeLogic {
			get { return "First Match"; }
		}

		//
		// Public Methods
		//
		
		public override CodeGroup Copy ()
		{
			FirstMatchCodeGroup copy = CopyNoChildren ();
			foreach (CodeGroup child in Children) {
				copy.AddChild (child.Copy ());	// deep copy
			}
			return copy;
		}

		public override PolicyStatement Resolve (Evidence evidence)
		{
			if (evidence == null)
				throw new ArgumentNullException ("evidence");

			if (!MembershipCondition.Check (evidence))
				return null;

			foreach (CodeGroup child in Children) {
				PolicyStatement policy = child.Resolve (evidence);
				if (policy != null) {
					return policy;	// first match
				}
			}
			return this.PolicyStatement;	// default
		}
		
		public override CodeGroup ResolveMatchingCodeGroups (Evidence evidence)
		{
			if (evidence == null)
				throw new ArgumentNullException ("evidence");

			if (!MembershipCondition.Check (evidence))
				return null;

			foreach (CodeGroup child in Children) {
				if (child.Resolve (evidence) != null) {
					return child.Copy ();	// first match
					// FIXME copy childrens ?
				}
			}
			return this.CopyNoChildren ();	// default
		}
	
		//
		// Private Methods
		//
	
		private FirstMatchCodeGroup CopyNoChildren ()
		{
			FirstMatchCodeGroup copy = new FirstMatchCodeGroup (MembershipCondition, PolicyStatement);

			copy.Name = Name;
			copy.Description = Description;

			return copy;
		}
	}
}
