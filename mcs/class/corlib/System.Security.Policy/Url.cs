//
// System.Security.Policy.Url.cs
//
// Author
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Security.Policy {

        [Serializable]
        public sealed class Url: IIdentityPermissionFactory, IBuiltInEvidence {

                string origin_url;
                
                public Url (string name)
                {
			origin_url = Prepare (name);
                }

                public object Copy ()
                {
                        return new Url (origin_url);
                }

                public IPermission CreateIdentityPermission (Evidence evidence)
                {
                        return new UrlIdentityPermission (origin_url);
                }

                public override bool Equals (object o)
                {
			if (o is System.Security.Policy.Url)
				return (String.Compare (((Url) o).Value, Value, true, CultureInfo.InvariantCulture) == 0);
			return false;
                }

                public override int GetHashCode ()
                {
                        return origin_url.GetHashCode ();
                }

                public override string ToString ()
                {
			SecurityElement element = new SecurityElement (typeof (System.Security.Policy.Url).FullName);
			element.AddAttribute ("version", "1");
			element.AddChild (new SecurityElement ("Url", origin_url));
			return element.ToString ();
                }

                public string Value {
			get { return origin_url; }
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

		// internal

		[MonoTODO ("missing site validation")]
		internal static string Prepare (string url) 
		{
			if (url == null)
				throw new ArgumentNullException ("Url");
			if (url == String.Empty)
				throw new FormatException (Locale.GetText ("Invalid (empty) Url"));

			// is a protocol specified
			int protocolPos = url.IndexOf ("://");
			if (protocolPos == -1)
				return "file://" + url.ToUpper ();
			
			if (url.StartsWith ("file://"))
				return "file://" + url.Substring (7).ToUpper ();

			// add a trailing slash if none (lonely one) is present
			if (url.LastIndexOf ("/") == protocolPos + 2)
				return url + "/";
			else
				return url;
		}

		internal static bool Compare (string mask, string url) 
		{
			int wildcard = mask.LastIndexOf ("*");
			if (wildcard > 0) {
				// partial match with a wildcard at the end
				return (String.Compare (mask, 0, url, 0, wildcard, true, CultureInfo.InvariantCulture) == 0);
			}
			else {
				// exact match
				return (String.Compare (mask, url, true, CultureInfo.InvariantCulture) == 0);
			}
		}
        }
}
