using System;
using System.Security.Permissions;
using System.Web;
using System.Web.Caching;

namespace System.Web.Caching {
    [Serializable]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Unrestricted)]
    public class MemoryResponseElement: ResponseElement {
        private  byte[]               _buffer;
        private  long                 _length;

        public   byte[]               Buffer   { get { return _buffer; } }
        public   long                 Length   { get { return _length; } }

        private MemoryResponseElement() { } // hide default constructor

        public MemoryResponseElement(byte[] buffer, long length) {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (length < 0 || length > buffer.Length)
                throw new ArgumentOutOfRangeException("length");

            _buffer = buffer;
            _length = length;
        }
    }
}
