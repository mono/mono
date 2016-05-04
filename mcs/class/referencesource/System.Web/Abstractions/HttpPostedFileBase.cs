//------------------------------------------------------------------------------
// <copyright file="HttpPostedFileBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class HttpPostedFileBase {

        public virtual int ContentLength {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string ContentType {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string FileName {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Stream InputStream {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "filename",
            Justification = "Matches HttpPostedFile class")]
        public virtual void SaveAs(string filename) {
            throw new NotImplementedException();
        }

    }
}
