//------------------------------------------------------------------------------
// <copyright file="HttpPostProtocolImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.Services.Description {
    using System.Web.Services.Protocols;

    internal class HttpPostProtocolImporter : HttpProtocolImporter {

        public HttpPostProtocolImporter() : base(true) { }

        public override string ProtocolName {
            get { return "HttpPost"; }
        }

        internal override Type BaseClass { 
            get {
                if (Style == ServiceDescriptionImportStyle.Client) {
                    return typeof(HttpPostClientProtocol);
                }
                else {
                    return typeof(WebService);
                }
            }
        }
        protected override bool IsBindingSupported() {
            HttpBinding httpBinding = (HttpBinding)Binding.Extensions.Find(typeof(HttpBinding));
            if (httpBinding == null) return false;
            if (httpBinding.Verb != "POST") return false;
            return true;
        }
    }
}
