//------------------------------------------------------------------------------
// <copyright file="HttpPostedFileWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpPostedFileWrapper : HttpPostedFileBase {

        private HttpPostedFile _file;

        public HttpPostedFileWrapper(HttpPostedFile httpPostedFile) {
            if (httpPostedFile == null) {
                throw new ArgumentNullException("httpPostedFile");
            }
            _file = httpPostedFile;
        }

        public override int ContentLength {
            get {
                return _file.ContentLength;
            }
        }

        public override string ContentType {
            get {
                return _file.ContentType;
            }
        }

        public override string FileName {
            get {
                return _file.FileName;
            }
        }

        public override Stream InputStream {
            get {
                return _file.InputStream;
            }
        }

        public override void SaveAs(string filename) {
            _file.SaveAs(filename);
        }

    }
}
