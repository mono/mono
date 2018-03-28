using System;
using System.Security.Permissions;
using System.Web;
using System.Web.Caching;

namespace System.Web.Caching {
    [Serializable]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Unrestricted)]
    public class FileResponseElement: ResponseElement {
        private  String               _path;
        private  long                 _offset;
        private  long                 _length;

        public   String               Path     { get { return _path; } }
        public   long                 Offset   { get { return _offset; } }
        public   long                 Length   { get { return _length; } }

        private FileResponseElement() { } // hide default constructor

        public FileResponseElement(String path, long offset, long length) {
            if (path == null)
                throw new ArgumentNullException("path");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            _path = path;
            _offset = offset;
            _length = length;
        }
    }
}
