//------------------------------------------------------------------------------
// <copyright file="HttpGetProtocolImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.Services.Description {
    using System.Web.Services.Protocols;

    internal class HttpGetProtocolImporter : HttpProtocolImporter {

        public HttpGetProtocolImporter() : base(false) { }

        public override string ProtocolName {
            get { return "HttpGet"; }
        }

        internal override Type BaseClass { 
            get {
                if (Style == ServiceDescriptionImportStyle.Client) {
                    return typeof(HttpGetClientProtocol);
                }
                else {
                    return typeof(WebService);
                }
            }
        }

        protected override bool IsBindingSupported() {
            HttpBinding httpBinding = (HttpBinding)Binding.Extensions.Find(typeof(HttpBinding));
            if (httpBinding == null) return false;
            if (httpBinding.Verb != "GET") return false;
            return true;
        }
    }
}
