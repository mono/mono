//
// System.Security.Policy.Url.cs
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
        public sealed class Url: IIdentityPermissionFactory
        {
                string origin_url;
                
                public Url (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException (Locale.GetText ("name is null"));

                        origin_url = name;
                }

                public object Copy ()
                {
                        return new Url (origin_url);
                }

                [MonoTODO]
                IPermission IIdentityPermissionFactory.CreateIdentityPermission (Evidence evidence)
                {
                        return null;
                }

                public override bool Equals (object o)
                {
                        return (o is System.Security.Policy.Url && ((Url) o).Value == Value);
                }

                public override int GetHashCode ()
                {
                        return origin_url.GetHashCode ();
                }

                public override string ToString ()
                {
                        return origin_url;
                }

                public string Value {
                        get {
                                return origin_url;
                        }
                }
        }
}
