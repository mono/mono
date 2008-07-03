//
// System.Security.Policy.NetCodeGroup.cs
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Jackson Harper, All rights reserved
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
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class NetCodeGroup : CodeGroup {

#if NET_2_0
		public static readonly string AbsentOriginScheme = String.Empty;
		public static readonly string AnyOtherOriginScheme = "*";

		private Hashtable _rules = new Hashtable ();
		private int _hashcode;
#endif

		public NetCodeGroup (IMembershipCondition membershipCondition) 
			: base (membershipCondition, null) 
		{
		}

		// for PolicyLevel (to avoid validation duplication)
		internal NetCodeGroup (SecurityElement e, PolicyLevel level)
			: base (e, level)
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
#if NET_2_0
			get { return "Same site Web"; }
#else
			get { return "Same site Web."; }
#endif
		}


		//
		// Public Methods
		//

#if NET_2_0
		[MonoTODO ("(2.0) missing validations")]
		public void AddConnectAccess (string originScheme, CodeConnectAccess connectAccess)
		{
			if (originScheme == null)
				throw new ArgumentException ("originScheme");

			// TODO (2.0) - invalid characters in originScheme
			if ((originScheme == AbsentOriginScheme) && (connectAccess.Scheme == CodeConnectAccess.OriginScheme)) {
				throw new ArgumentOutOfRangeException ("connectAccess", Locale.GetText (
					"Schema == CodeConnectAccess.OriginScheme"));
			}

			if (_rules.ContainsKey (originScheme)) {
				// NULL has no effect
				if (connectAccess != null) {
					CodeConnectAccess[] existing = (CodeConnectAccess[]) _rules [originScheme];
					CodeConnectAccess[] array = new CodeConnectAccess [existing.Length + 1];
					Array.Copy (existing, 0, array, 0, existing.Length);
					array [existing.Length] = connectAccess;
					_rules [originScheme] = array;
				}
			}
			else {
				CodeConnectAccess[] array = new CodeConnectAccess [1];
				array [0] = connectAccess;
				_rules.Add (originScheme, array);
				// add null to prevent access
			}
		}
#endif

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

#if NET_2_0
		private bool Equals (CodeConnectAccess[] rules1, CodeConnectAccess[] rules2)
		{
			for (int i=0; i < rules1.Length; i++) {
				bool found = false;
				for (int j=0; j < rules2.Length; j++) {
					if (rules1 [i].Equals (rules2 [j])) {
						found = true;
						break;
					}
				}
				if (!found)
					return false;
			}
			return true;
		}

		public override bool Equals (object o)
		{
			if (!base.Equals (o))
				return false;
			NetCodeGroup ncg = (o as NetCodeGroup);
			if (ncg == null) 
				return false;
	
			// check rules
			foreach (DictionaryEntry de in _rules) {
				bool found = false;
				CodeConnectAccess[] ccas = (CodeConnectAccess[]) ncg._rules [de.Key];
				if (ccas != null)
					found = Equals ((CodeConnectAccess[]) de.Value, ccas);
				else
					found = (de.Value == null);

				if (!found)
					return false;
			}
			return true;
		}

		public DictionaryEntry[] GetConnectAccessRules ()
		{
			DictionaryEntry[] result = new DictionaryEntry [_rules.Count];
			_rules.CopyTo (result, 0);
			return result;
		}

		public override int GetHashCode ()
		{
			if (_hashcode == 0) {
				_hashcode = base.GetHashCode ();
				foreach (DictionaryEntry de in _rules) {
					CodeConnectAccess[] ccas = (CodeConnectAccess[]) de.Value;
					if (ccas != null) {
						foreach (CodeConnectAccess cca in ccas) {
							_hashcode ^= cca.GetHashCode ();
						}
					}
				}
			}
			return _hashcode;
		}
#endif

		public override PolicyStatement Resolve (Evidence evidence)
		{
			if (evidence == null) 
				throw new ArgumentNullException ("evidence");

 			if (!MembershipCondition.Check (evidence))
				return null;

			PermissionSet ps = null;
			if (this.PolicyStatement == null)
				ps = new PermissionSet (PermissionState.None);
			else
				ps = this.PolicyStatement.PermissionSet.Copy ();

			if (this.Children.Count > 0) {
				foreach (CodeGroup child_cg in this.Children) {
					PolicyStatement child_pst = child_cg.Resolve (evidence);
					if (child_pst != null) {
						ps = ps.Union (child_pst.PermissionSet);
					}
				}
			}

			PolicyStatement pst = this.PolicyStatement.Copy ();
			pst.PermissionSet = ps;
			return pst;
		}

#if NET_2_0
		public void ResetConnectAccess ()
		{
			_rules.Clear ();
		}
#endif

		public override CodeGroup ResolveMatchingCodeGroups (Evidence evidence) 
		{
			if (evidence == null)
				throw new ArgumentNullException ("evidence");
			
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

#if NET_2_0
		[MonoTODO ("(2.0) Add new stuff (CodeConnectAccess) into XML")]
		protected override void CreateXml (SecurityElement element, PolicyLevel level)
		{
			base.CreateXml (element, level);
		}

		[MonoTODO ("(2.0) Parse new stuff (CodeConnectAccess) from XML")]
		protected override void ParseXml (SecurityElement e, PolicyLevel level)
		{
			base.ParseXml (e, level);
		}
#endif
	}
}
