//
// System.Security.Policy.Site.cs
//
// Author
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Security.Policy {

        [Serializable]
        public sealed class Site: IIdentityPermissionFactory
        {
                string origin_site;
                SecurityElement element;

                public Site (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException (Locale.GetText ("name is null"));

                        if (IsValidSite (name) == false)
                                throw new ArgumentException (Locale.GetText ("name is not valid"));

                        origin_site = name;
                        element = new SecurityElement (
                                typeof (System.Security.Policy.Site).FullName);

                        element.AddAttribute ("version", "1");
                        element.AddChild (new SecurityElement ("Name", name));
                }

                private Site (string name, SecurityElement security_element)
                {
                        origin_site = name;
                        element = security_element;
                }
                public static Site CreateFromUrl (string url)
                {
                        return new Site (url);
                }

                public object Copy ()
                {
                        return new Site (origin_site, element);
                }

                [MonoTODO]
                public IPermission CreateIdentityPermission (Evidence evidence)
                {
                        throw new NotImplementedException ();
                }

                public override bool Equals (object o)
                {
                        return (o is System.Security.Policy.Site && ((Site) o).Name == Name);
                }

                public override int GetHashCode ()
                {
                        return origin_site.GetHashCode ();
                }

                public override string ToString ()
                {
                        return element.ToString ();
                }

                public string Name {
                        get {
                                return origin_site;
                        }
                }

                [MonoTODO ("Improve check")]
                bool IsValidSite (string site)
                {
                        if (site.StartsWith ("file"))
                                return false;

                        string [] parts = site.Split ('.');

                        foreach (string part in parts)
                                if (!IsValidPart (part))
                                        return false;
                        
                        return true;
                }

                bool IsValidPart (string part)
                {
                        foreach (char c in part) {
                                if (Char.IsLetterOrDigit (c))
                                        continue;
                        
                                if (c == '/' || c == '*')
                                        continue;

                                else
                                        return false;
                        }

                        return true;
                }
        }
}
