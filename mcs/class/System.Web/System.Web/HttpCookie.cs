//
// System.Web.HttpCookie.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web
{
        public sealed class HttpCookie
        {
                private string _name;
                private string _value = null;
                private string _domain; //TODO: default to some pref.
                private DateTime _expires;
                private string _path;  //TODO: default is the current request path.
                private bool _secure = false;
                public HttpCookie(string name)
                {
                        _name = name;
                }
                public HttpCookie(string name, string value)
                {
                        _name = name;
                        _value = value;
                }
                public string Domain
                {
                        get
                        {
                                return _domain;
                        }
                        set
                        {
                                _domain = value;
                        }
                }
                public DateTime Expires
                {
                        get
                        {
                                return _expires;
                        }
                        set
                        {
                                _expires = value;
                        }
                }
                public bool HasKeys
                        get
                        {
                                return FALSE; //TODO
                        }
               public string this[string key] {get; set;} //TODO: get subcookie.
               public string Name
                {
                        get
                        {
                                return _name;
                        }
                        set
                        {
                                _name = value;
                        }
                }
                public string Path
                {
                        get
                        {
                                return _path;
                        }
                        set
                        {
                                _path = value;
                        }
                }
                public bool Secure
                {
                        get
                        {
                                return _secure;
                        }
                        set
                        {
                                _secure = value;
                        }
                }
                public string Value
                {
                        get
                        {
                                return _value;
                        }
                        set
                        {
                                _value = value;
                        }
                }
                public NameValueCollection Values {get;} //TODO
        }
}
