//
// System.Security.Policy.ApplicationDirectoryMembershipCondition
//
// Authors:
//	Nick Drochak (ndrochak@gol.com)
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Nick Drochak, All rights reserved.
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

using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;

using Mono.Security;

namespace System.Security.Policy {

	[Serializable]
	public sealed class ApplicationDirectoryMembershipCondition : IConstantMembershipCondition, IMembershipCondition {

		private readonly int version = 1;

		public ApplicationDirectoryMembershipCondition ()
		{
		}

		// Methods
		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				return false;

			string codebase = Assembly.GetCallingAssembly ().CodeBase;
			Uri local = new Uri (codebase);
			Url ucode = new Url (codebase);

			// *both* ApplicationDirectory and Url must be in *Host* evidences
			bool adir = false;
			bool url = false;
			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				object o = e.Current;

				if (!adir && (o is ApplicationDirectory)) {
					ApplicationDirectory ad = (o as ApplicationDirectory);
					string s = ad.Directory;
					adir = (String.Compare (s, 0, local.ToString (), 0, s.Length, true, CultureInfo.InvariantCulture) == 0);
				}
				else if (!url && (o is Url)) {
					url = ucode.Equals (o);
				}

				// got both ?
				if (adir && url)
					return true;
			}
			return false;
		}

		public IMembershipCondition Copy () 
		{ 
			return new ApplicationDirectoryMembershipCondition ();
		}
		
		public override bool Equals (object o) 
		{ 
			return (o is ApplicationDirectoryMembershipCondition); 
		}
		
		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}
		
		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);
		}
		
		// All instances of ApplicationDirectoryMembershipCondition are equal so they should
		// have the same hashcode
		public override int GetHashCode () 
		{ 
			return typeof (ApplicationDirectoryMembershipCondition).GetHashCode ();
		}
		
		public override string ToString () 
		{ 
			return "ApplicationDirectory";
		}
		
		public SecurityElement ToXml () 
		{ 
			return ToXml (null);
		}
		
		public SecurityElement ToXml (PolicyLevel level) 
		{
			SecurityElement se = MembershipConditionHelper.Element (typeof (ApplicationDirectoryMembershipCondition), version);
			// nothing to add
			return se;
		}
	}
}
