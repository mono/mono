//
// System.Security.Policy.GacMembershipCondition
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System.Collections;
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
	public sealed class GacMembershipCondition : IMembershipCondition, IConstantMembershipCondition {

		private readonly int version = 1;

		public GacMembershipCondition ()
		{
		}

		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				return false;

			// true only if Gac is in host-supplied evidences
			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				if (e.Current is Gac)
					return true;
			}
			return false;
		}

		public IMembershipCondition Copy ()
		{
			return new GacMembershipCondition ();
		}

		public override bool Equals (object o)
		{
			if (o == null)
				return false;
			return (o is GacMembershipCondition);
		}

		public void FromXml (SecurityElement element)
		{
			FromXml (element, null);
		}

                public void FromXml (SecurityElement element, PolicyLevel level)
                {
			MembershipConditionHelper.CheckSecurityElement (element, "element", version, version);
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
		}

		public override int GetHashCode ()
		{
			return 0; // always the same
		}

		// LAMESPEC: "Gac" is documented - but Fx 2.0 beta 1 returns "GAC"
		public override string ToString ()
		{
			return "GAC";
		}

		public SecurityElement ToXml ()
		{
			return ToXml (null);
		}

		public SecurityElement ToXml (PolicyLevel level)
		{
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (GacMembershipCondition), version);
			// nothing to add
			return se;
		}
	}
}

#endif
