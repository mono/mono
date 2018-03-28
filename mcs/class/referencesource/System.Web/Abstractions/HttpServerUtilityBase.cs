//------------------------------------------------------------------------------
// <copyright file="HttpServerUtilityBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class HttpServerUtilityBase {
        public virtual string MachineName {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int ScriptTimeout {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual void ClearError() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="ID")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public virtual object CreateObject(string progID) {
            throw new NotImplementedException();
        }

        public virtual object CreateObject(Type type) {
            throw new NotImplementedException();
        }

        public virtual object CreateObjectFromClsid(string clsid) {
            throw new NotImplementedException();
        }

        public virtual void Execute(string path) {
            throw new NotImplementedException();
        }

        public virtual void Execute(string path, TextWriter writer) {
            throw new NotImplementedException();
        }

        public virtual void Execute(string path, bool preserveForm) {
            throw new NotImplementedException();
        }

        public virtual void Execute(string path, TextWriter writer, bool preserveForm) {
            throw new NotImplementedException();
        }

        public virtual void Execute(IHttpHandler handler, TextWriter writer, bool preserveForm) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Matches HttpServerUtility class")]
        public virtual Exception GetLastError() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        public virtual string HtmlDecode(string s) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        public virtual void HtmlDecode(string s, TextWriter output) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        public virtual string HtmlEncode(string s) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        public virtual void HtmlEncode(string s, TextWriter output) {
            throw new NotImplementedException();
        }

        public virtual string MapPath(string path) {
            throw new NotImplementedException();
        }

        public virtual void Transfer(string path, bool preserveForm) {
            throw new NotImplementedException();
        }

        public virtual void Transfer(string path) {
            throw new NotImplementedException();
        }

        public virtual void Transfer(IHttpHandler handler, bool preserveForm) {
            throw new NotImplementedException();
        }

        public virtual void TransferRequest(string path) {
            throw new NotImplementedException();
        }

        public virtual void TransferRequest(string path, bool preserveForm) {
            throw new NotImplementedException();
        }

        public virtual void TransferRequest(string path, bool preserveForm, string method, NameValueCollection headers) {
            throw new NotImplementedException();
        }

        public virtual void TransferRequest(string path, bool preserveForm, string method, NameValueCollection headers, bool preserveUser) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Matches HttpServerUtility class")]
        public virtual string UrlDecode(string s) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        public virtual void UrlDecode(string s, TextWriter output) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Matches HttpServerUtility class")]
        public virtual string UrlEncode(string s) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        public virtual void UrlEncode(string s, TextWriter output) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Matches HttpServerUtility class")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpServerUtility class")]
        public virtual string UrlPathEncode(string s) {
            throw new NotImplementedException();
        }

        public virtual byte[] UrlTokenDecode(string input) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Matches HttpServerUtility class")]
        public virtual string UrlTokenEncode(byte[] input) {
            throw new NotImplementedException();
        }
    }
}
