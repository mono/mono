//
// System.Security.Policy.UnionCodeGroup.cs
//
// Authors
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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

using System.Globalization;
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
	public sealed class UnionCodeGroup : CodeGroup {

		public UnionCodeGroup (IMembershipCondition membershipCondition, PolicyStatement policyStatement)
			: base (membershipCondition, policyStatement)
		{
		}

		// for PolicyLevel (to avoid validation duplication)
		internal UnionCodeGroup (SecurityElement e, PolicyLevel level)
			: base (e, level)
		{
		}

		public override CodeGroup Copy ()
		{
			return Copy (true);
		}

		internal CodeGroup Copy (bool childs) 
		{
			UnionCodeGroup copy = new UnionCodeGroup (MembershipCondition, PolicyStatement);
			copy.Name = Name;
			copy.Description = Description;
			if (childs) {
				foreach (CodeGroup child in Children) {
					copy.AddChild (child.Copy ());
				}
			}
			return copy;
		}

		[MonoTODO ("no children processing")]
		public override PolicyStatement Resolve (Evidence evidence)
		{
			if (evidence == null)
				throw new ArgumentNullException ("evidence");

 			if (!MembershipCondition.Check (evidence))
				return null;

			PolicyStatement pst = this.PolicyStatement.Copy ();
			if (this.Children.Count > 0) {
				foreach (CodeGroup cg in this.Children) {
					PolicyStatement child = cg.Resolve (evidence);
					if (child != null) {
						// TODO union
					}
				}
			}
			return pst;
		}

		public override CodeGroup ResolveMatchingCodeGroups (Evidence evidence)
		{
			if (evidence == null)
				throw new ArgumentNullException ("evidence");

 			if (!MembershipCondition.Check (evidence))
				return null;

			// Copy() would add the child (even if they didn't match)
			CodeGroup match = Copy (false);
			if (this.Children.Count > 0) {
				foreach (CodeGroup cg in this.Children) {
					CodeGroup child = cg.ResolveMatchingCodeGroups (evidence);
					if (child != null)
						match.AddChild (child);
				}
			}
			return match;
		}

		public override string MergeLogic {
			get { return "Union"; }
		}
	}
}
