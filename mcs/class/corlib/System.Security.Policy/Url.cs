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
                SecurityElement element;
                
                public Url (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException (Locale.GetText ("name is null"));

                        origin_url = name;
                        element = new SecurityElement (
                                typeof (System.Security.Policy.Url).FullName);

                        element.AddAttribute ("version", "1");
                        element.AddChild (new SecurityElement ("Url", name));
                }

                private Url (string name, SecurityElement security_element)
                {
                        origin_url = name;
                        element = security_element;
                }

                public object Copy ()
                {
                        return new Url (origin_url, element);
                }

                [MonoTODO]
                public IPermission CreateIdentityPermission (Evidence evidence)
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
                        return element.ToString ();
                }

                public string Value {
                        get {
                                return origin_url;
                        }
                }
        }
}
