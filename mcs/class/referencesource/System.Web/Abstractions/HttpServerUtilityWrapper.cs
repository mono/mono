//------------------------------------------------------------------------------
// <copyright file="HttpServerUtilityWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpServerUtilityWrapper : HttpServerUtilityBase {
        private HttpServerUtility _httpServerUtility;

        public HttpServerUtilityWrapper(HttpServerUtility httpServerUtility) {
            if (httpServerUtility == null) {
                throw new ArgumentNullException("httpServerUtility");
            }
            _httpServerUtility = httpServerUtility;
        }

        public override Exception GetLastError() {
            return _httpServerUtility.GetLastError();
        }

        public override string MachineName {
            get {
                return _httpServerUtility.MachineName;
            }
        }

        public override int ScriptTimeout {
            get {
                return _httpServerUtility.ScriptTimeout;
            }
            set {
                _httpServerUtility.ScriptTimeout = value;
            }
        }

        public override void ClearError() {
            _httpServerUtility.ClearError();
        }

        public override object CreateObject(string progID) {
            return _httpServerUtility.CreateObject(progID);
        }

        public override object CreateObject(Type type) {
            return _httpServerUtility.CreateObject(type);
        }

        public override object CreateObjectFromClsid(string clsid) {
            return _httpServerUtility.CreateObjectFromClsid(clsid);
        }

        public override void Execute(string path) {
            _httpServerUtility.Execute(path);
        }

        public override void Execute(string path, TextWriter writer) {
            _httpServerUtility.Execute(path, writer);
        }

        public override void Execute(string path, bool preserveForm) {
            _httpServerUtility.Execute(path, preserveForm);
        }

        public override void Execute(string path, TextWriter writer, bool preserveForm) {
            _httpServerUtility.Execute(path, writer, preserveForm);
        }

        public override void Execute(IHttpHandler handler, TextWriter writer, bool preserveForm) {
            _httpServerUtility.Execute(handler, writer, preserveForm);
        }

        public override string HtmlDecode(string s) {
            return _httpServerUtility.HtmlDecode(s);
        }

        public override void HtmlDecode(string s, TextWriter output) {
            _httpServerUtility.HtmlDecode(s, output);
        }

        public override string HtmlEncode(string s) {
            return _httpServerUtility.HtmlEncode(s);
        }

        public override void HtmlEncode(string s, TextWriter output) {
            _httpServerUtility.HtmlEncode(s, output);
        }

        public override string MapPath(string path) {
            return _httpServerUtility.MapPath(path);
        }

        public override void Transfer(string path, bool preserveForm) {
            _httpServerUtility.Transfer(path, preserveForm);
        }

        public override void Transfer(string path) {
            _httpServerUtility.Transfer(path);
        }

        public override void Transfer(IHttpHandler handler, bool preserveForm) {
            _httpServerUtility.Transfer(handler, preserveForm);
        }

        public override void TransferRequest(string path) {
            _httpServerUtility.TransferRequest(path);
        }

        public override void TransferRequest(string path, bool preserveForm) {
            _httpServerUtility.TransferRequest(path, preserveForm);
        }

        public override void TransferRequest(string path, bool preserveForm, string method, NameValueCollection headers) {
            _httpServerUtility.TransferRequest(path, preserveForm, method, headers);
        }

        public override void TransferRequest(string path, bool preserveForm, string method, NameValueCollection headers, bool preserveUser) {
            _httpServerUtility.TransferRequest(path, preserveForm, method, headers, preserveUser);
        }

        public override string UrlDecode(string s) {
            return _httpServerUtility.UrlDecode(s);
        }

        public override void UrlDecode(string s, TextWriter output) {
            _httpServerUtility.UrlDecode(s, output);
        }

        public override string UrlEncode(string s) {
            return _httpServerUtility.UrlEncode(s);
        }

        public override void UrlEncode(string s, TextWriter output) {
            _httpServerUtility.UrlEncode(s, output);
        }

        public override string UrlPathEncode(string s) {
            return _httpServerUtility.UrlPathEncode(s);
        }

        public override byte[] UrlTokenDecode(string input) {
            return HttpServerUtility.UrlTokenDecode(input);
        }

        public override string UrlTokenEncode(byte[] input) {
            return HttpServerUtility.UrlTokenEncode(input);
        }
    }
}
