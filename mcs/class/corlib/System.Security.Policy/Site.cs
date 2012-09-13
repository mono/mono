//
// System.Security.Policy.Site.cs
//
// Authors
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
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

using System.Globalization;
using System.Security.Permissions;
using System.Runtime.InteropServices;

using Mono.Security;

namespace System.Security.Policy {

	[Serializable]
	[ComVisible (true)]
	public sealed class Site:
#if NET_4_0
		EvidenceBase,
#endif
		IIdentityPermissionFactory, IBuiltInEvidence {

		internal string origin_site;

		public Site (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("url");
			if (!IsValid (name))
				throw new ArgumentException (Locale.GetText ("name is not valid"));
			
			origin_site = name;
                }

                public static Site CreateFromUrl (string url)
                {
			if (url == null)
				throw new ArgumentNullException ("url");
			if (url.Length == 0)
				throw new FormatException (Locale.GetText ("Empty URL."));

			string site = UrlToSite (url);
			if (site == null) {
				string msg = String.Format (Locale.GetText ("Invalid URL '{0}'."), url);
				throw new ArgumentException (msg, "url");
			}

                        return new Site (site);
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
			Site s = (o as System.Security.Policy.Site);
			if (s == null)
				return false;
			return (String.Compare (s.Name, origin_site, true, CultureInfo.InvariantCulture) == 0);
                }

                public override int GetHashCode ()
                {
                        return origin_site.GetHashCode ();
                }

                public override string ToString ()
                {
			SecurityElement element = new SecurityElement ("System.Security.Policy.Site");
			element.AddAttribute ("version", "1");
			element.AddChild (new SecurityElement ("Name", origin_site));
			return element.ToString ();
                }

		// properties

                public string Name {
                        get { return origin_site; }
                }

		// interface IBuiltInEvidence

		int IBuiltInEvidence.GetRequiredSize (bool verbose) 
		{
			return (verbose ? 3 : 1) + origin_site.Length;
		}

		[MonoTODO ("IBuiltInEvidence")]
		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
		{
			return 0;
		}

		[MonoTODO ("IBuiltInEvidence")]
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
					bool result = ((x == 33) || (x == 45)	// !-
						|| (x >= 35 && x <= 41)		// #$%&'()
						|| (x >= 48 && x <= 57)		// 0-9
						|| (x >= 64 && x <= 90)		// @,A-Z
						|| (x >= 94 && x <= 95)		// ^_
						|| (x >= 97 && x <= 123)	// a-z{
						|| (x >= 125 && x <= 126));	// }~
					if (!result)
						return false;
				}
			}
                        return true;
                }

		// no exception - we return null if a site couldn't be created
		// this is useful for creating the default evidence as the majority of URL will be local (file://)
		// and throw an unrequired exception
		internal static string UrlToSite (string url)
		{
			if (url == null)
				return null;

			Uri uri = new Uri (url);
			if (uri.Scheme == Uri.UriSchemeFile)
				return null;
			string site = uri.Host;
			return IsValid (site) ? site : null;
		}
        }
}
