//
// SiteMembershipCondition.cs: Site MembershipCondition
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Globalization;
using System.Security;

namespace System.Security.Policy {

	[Serializable]
	public sealed class SiteMembershipCondition : IMembershipCondition, IConstantMembershipCondition {

		private string _site;

		// constructors

		public SiteMembershipCondition (string site)
		{
			Site = site;
		}

		// properties

		public string Site { 
			get { return _site; }
			set {
				if (value == null)
					throw new ArgumentNullException ("site");
				if (!System.Security.Policy.Site.IsValid (value))
					throw new ArgumentException ("invalid site");
				_site = value;
			}
		}

		// methods

		public bool Check (Evidence evidence) 
		{
			if (evidence == null)
				return false;

			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				if (e.Current is Site) {
					string[] s1 = _site.Split ('.');
					string[] s2 = (e.Current as Site).origin_site.Split ('.');
					for (int i = s1.Length - 1, j = s2.Length - 1; i>=0; i--, j--) {
						if (i == 0) {
							// special * case
							return (String.Compare (s1 [0], "*", true, CultureInfo.InvariantCulture) == 0);
						}
						if (String.Compare (s1 [i], s2 [j], true, CultureInfo.InvariantCulture) != 0)
							return false;
					}
					return true;
				}
			}
			return false;
		}
		
		public IMembershipCondition Copy () 
		{
			return new SiteMembershipCondition (_site);
		}

		public override bool Equals (object o) 
		{
			if (o == null)
				return false;
			if (o is SiteMembershipCondition) {
				Site s = new Site ((o as SiteMembershipCondition)._site);
				return s.Equals (new Site (_site));
			}
			return false;
		}

		public void FromXml (SecurityElement e) 
		{
			FromXml (e, null);
		}

		public void FromXml (SecurityElement e, PolicyLevel level) 
		{
			if (e == null)
				throw new ArgumentNullException ("e");
			if (e.Tag != "IMembershipCondition")
				throw new ArgumentException (Locale.GetText ("Invalid XML - not a IMembershipCondition tag."));
			if (e.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (Locale.GetText ("Invalid class."));
			if (e.Attribute ("version") != "1")
				throw new ArgumentException (Locale.GetText ("Invalid version."));

			_site = e.Attribute ("Site");
		}

		public override int GetHashCode () 
		{
			return _site.GetHashCode ();
		}

		public override string ToString () 
		{
			return "Site - " + _site;
		}

		public SecurityElement ToXml () 
		{
			return ToXml (null);
		}

		public SecurityElement ToXml (PolicyLevel level) 
		{
			SecurityElement element = new SecurityElement ("IMembershipCondition");
			element.AddAttribute ("class", this.GetType ().AssemblyQualifiedName);
			element.AddAttribute ("version", "1");
			element.AddAttribute ("Site", _site);
			return element;
		}
	}
}
