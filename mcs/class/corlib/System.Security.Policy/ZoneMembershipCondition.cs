//
// System.Security.Policy.ZoneMembershipCondition.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003, Ximian Inc.
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

using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Policy {

	[Serializable]
	[ComVisible (true)]
	public sealed class ZoneMembershipCondition : IMembershipCondition, IConstantMembershipCondition {

		private readonly int version = 1;

                private SecurityZone zone;

		// so System.Activator.CreateInstance can create an instance...
		internal ZoneMembershipCondition ()
		{
		}
                
                public ZoneMembershipCondition (SecurityZone zone)
                {
			// we need the validations
                        SecurityZone = zone;
                }

                public SecurityZone SecurityZone {
                        get { return zone; }
                        set {
				if (!Enum.IsDefined (typeof (SecurityZone), value)) {
					throw new ArgumentException (Locale.GetText (
						"invalid zone"));
				}
				if (value == SecurityZone.NoZone) {
					throw new ArgumentException (Locale.GetText (
						"NoZone isn't valid for membership condition"));
				}

				zone = value;
			}
                }

                public bool Check (Evidence evidence)
                {
			if (evidence == null)
				return false;

			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				Zone z = (e.Current as Zone);
				if (z != null) {
					if (z.SecurityZone == zone)
						return true;
				}
			}
                        return false;
                }

                public IMembershipCondition Copy ()
                {
                        return new ZoneMembershipCondition (zone);
                }

		public override bool Equals (object o)
		{
			ZoneMembershipCondition zmc = (o as ZoneMembershipCondition);
			if (zmc == null)
				return false;
			return (zmc.SecurityZone == zone);
		}

                public void FromXml (SecurityElement e)
                {
                        FromXml (e, null);
                }

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);

			string z = e.Attribute ("Zone");
			if (z != null) {
				zone = (SecurityZone) Enum.Parse (typeof (SecurityZone), z);
			}
		}

                public override int GetHashCode ()
                {
                        return zone.GetHashCode ();
                }

                public override string ToString ()
                {
                        return "Zone - " + zone;
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (ZoneMembershipCondition), version);
                        se.AddAttribute ("Zone", zone.ToString ());
                        return se;
                }
        }
}
