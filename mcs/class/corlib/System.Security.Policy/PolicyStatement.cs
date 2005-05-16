//
// System.Security.Policy.PolicyStatement
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002
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
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class PolicyStatement : ISecurityEncodable, ISecurityPolicyEncodable {

		private PermissionSet perms;
		private PolicyStatementAttribute attrs;

		public PolicyStatement (PermissionSet perms) :
			this (perms, PolicyStatementAttribute.Nothing)
		{
		}

		public PolicyStatement (PermissionSet perms, PolicyStatementAttribute attrs) 
		{
			if (perms != null) {
				this.perms = perms.Copy ();
				this.perms.SetReadOnly (true);
			}
			this.attrs = attrs;
		}
		
		public PermissionSet PermissionSet {
			get {
				if (perms == null) {
					perms = new PermissionSet (PermissionState.None);
					perms.SetReadOnly (true);
				}
				return perms;
			}
			set { perms = value; }
		}
		
		public PolicyStatementAttribute Attributes {
			get { return attrs; }
			set {
				// note: yes it's a flag but all possible values have a corresponding name
				switch (value) {
				case PolicyStatementAttribute.Nothing:
				case PolicyStatementAttribute.Exclusive:
				case PolicyStatementAttribute.LevelFinal:
				case PolicyStatementAttribute.All:
					attrs = value;
					break;
				default:
					string msg = Locale.GetText ("Invalid value for {0}.");
					throw new ArgumentException (String.Format (msg, "PolicyStatementAttribute"));
				}
			}
		}

		public string AttributeString {
			get {
				switch (attrs) {
				case PolicyStatementAttribute.Exclusive:
					return "Exclusive";
				case PolicyStatementAttribute.LevelFinal:
					return "LevelFinal";
				case PolicyStatementAttribute.All:
					return "Exclusive LevelFinal";
				default:
					return String.Empty;
				}
			}
		}

		public PolicyStatement Copy ()
		{
			return new PolicyStatement (perms, attrs);
		}

		// ISecurityEncodable

		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			if (e == null)
				throw new ArgumentNullException ("e");
			if (e.Tag != "PolicyStatement")
				throw new ArgumentException (Locale.GetText ("Invalid tag."));


			string attributes = e.Attribute ("Attributes");
			if (attributes != null) {
				attrs = (PolicyStatementAttribute) Enum.Parse (
					typeof (PolicyStatementAttribute), attributes);
			}

			SecurityElement permissions = e.SearchForChildByTag ("PermissionSet");
			PermissionSet.FromXml (permissions);
		}
		
		public SecurityElement ToXml ()
		{
			return ToXml (null);
		}

		public SecurityElement ToXml (PolicyLevel level)
		{
			SecurityElement element = new SecurityElement ("PolicyStatement");
			element.AddAttribute ("version", "1");

			if (attrs != PolicyStatementAttribute.Nothing)
				element.AddAttribute ("Attributes", attrs.ToString ());
			
			element.AddChild (PermissionSet.ToXml ());

			return element;
		}

#if NET_2_0
		[ComVisible (false)]
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			PolicyStatement ps = (obj as PolicyStatement);
			if (ps == null)
				return false;

			return (PermissionSet.Equals (obj) && (attrs == ps.attrs));
		}

		[ComVisible (false)]
		public override int GetHashCode ()
		{
			// return same hash code if two PolicyStatement are equals
			return (PermissionSet.GetHashCode () ^ (int) attrs);
		}
#endif

		internal static PolicyStatement Empty ()
		{
			return new PolicyStatement (new PermissionSet (PermissionState.None));
		}
	}
}
