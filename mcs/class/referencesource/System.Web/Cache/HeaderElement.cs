using System;
using System.Security.Permissions;
using System.Web;
using System.Web.Caching;

namespace System.Web.Caching {
    // A header element holds the header name and value.
    [Serializable]
    public sealed class HeaderElement {
        private  string                     _name;
        private  string                     _value;

        public   string                     Name    { get { return _name; } }
        public   string                     Value   { get { return _value; } }

        private HeaderElement() { } // hide default constructor

        public HeaderElement(string name, string value) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");

            _name = name;
            _value = value;
        }
    }
}
