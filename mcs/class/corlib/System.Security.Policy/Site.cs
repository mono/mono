//
// System.Security.Policy.Site.cs
//
// Authors
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Security.Policy {

        [Serializable]
        public sealed class Site: IIdentityPermissionFactory, IBuiltInEvidence {

		internal string origin_site;

		public Site (string name)
		{
			if (name == null)
				throw new ArgumentNullException (Locale.GetText ("name is null"));
			if (!IsValid (name))
				throw new ArgumentException (Locale.GetText ("name is not valid"));
			
			origin_site = name;
                }

                public static Site CreateFromUrl (string url)
                {
                        return new Site (url);
                }

                public object Copy ()
                {
                        return new Site (origin_site);
                }

                public IPermission CreateIdentityPermission (Evidence evidence)
                {
                        return new SiteIdentityPermission (origin_site);
                }

                public override bool Equals (object o)
                {
			if (o is System.Security.Policy.Site) {
				return (String.Compare (((Site) o).Name, origin_site, true, CultureInfo.InvariantCulture) == 0);
			}
			return false;
                }

                public override int GetHashCode ()
                {
                        return origin_site.GetHashCode ();
                }

                public override string ToString ()
                {
			SecurityElement element = new SecurityElement (typeof (System.Security.Policy.Site).FullName);
			element.AddAttribute ("version", "1");
			element.AddChild (new SecurityElement ("Name", origin_site));
			return element.ToString ();
                }

		// properties

                public string Name {
                        get { return origin_site; }
                }

		// interface IBuiltInEvidence

		[MonoTODO]
		int IBuiltInEvidence.GetRequiredSize (bool verbose) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
		{
			return 0;
		}

		// internals

                internal static bool IsValid (string name)
                {
			if (name == String.Empty)
				return false;
			if ((name.Length == 1) && (name == "."))		// split would remove .
				return false;

			string [] parts = name.Split ('.');
			for (int i=0; i < parts.Length; i++) {
				string part = parts [i];
				if ((i == 0) && (part == "*"))			// * (only in first part)
					continue;
				foreach (char c in part) {
					int x = Convert.ToInt32 (c);
					bool result = ((x == 45)		// -
						|| (x >= 47 && x <= 57)		// /,0-9
						|| (x >= 64 && x <= 90)		// @,A-Z
						|| (x == 95)			// _
						|| (x >= 97 && x <= 122));	// a-z
					if (!result)
						return false;
				}
			}
                        return true;
                }
        }
}
