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

                [MonoTODO ("Check for name validity")]
                public Site (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException (Locale.GetText ("name is null"));

                        origin_site = name;
                }

                [MonoTODO ("Check for url validity")]
                public static Site CreateFromUrl (string url)
                {
                        return new Site (url);
                }

                public object Copy ()
                {
                        return new Site (origin_site);
                }

                [MonoTODO]
                IPermission IIdentityPermissionFactory.CreateIdentityPermission (Evidence evidence)
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
                        return origin_site;
                }

                public string Name {
                        get {
                                return origin_site;
                        }
                }
        }
}
