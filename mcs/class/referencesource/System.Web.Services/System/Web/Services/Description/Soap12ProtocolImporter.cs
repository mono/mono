//------------------------------------------------------------------------------
//  <copyright from='1997' to='2001' company='Microsoft Corporation'>           
//     Copyright (c) Microsoft Corporation. All Rights Reserved.                
//     Information Contained Herein is Proprietary and Confidential.            
//  </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.CodeDom;
    using System.Web.Services.Configuration;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Globalization;
    
    /// <include file='doc\Soap12ProtocolImporter.uex' path='docs/doc[@for="Soap12ProtocolImporter"]/*' />
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    internal class Soap12ProtocolImporter : SoapProtocolImporter {

        public override string ProtocolName {
            get { return "Soap12"; }
        }

        protected override bool IsBindingSupported() {
            Soap12Binding soapBinding = (Soap12Binding)Binding.Extensions.Find(typeof(Soap12Binding));
            if (soapBinding == null) return false;

            if (GetTransport(soapBinding.Transport) == null) {
                UnsupportedBindingWarning(Res.GetString(Res.ThereIsNoSoapTransportImporterThatUnderstands1, soapBinding.Transport));
                return false;
            }
                            
            return true;
        }

        protected override bool IsSoapEncodingPresent(string uriList) {
            int iStart = 0;
            do {
                iStart = uriList.IndexOf(Soap12.Encoding, iStart, StringComparison.Ordinal);
                if (iStart < 0)
                    break;
                int iEnd = iStart + Soap12.Encoding.Length;
                if (iStart == 0 || uriList[iStart - 1] == ' ')
                    if (iEnd == uriList.Length || uriList[iEnd] == ' ')
                        return true;
                iStart = iEnd;
            } while (iStart < uriList.Length);

            // not soap 1.2 encoding. let's detect the soap 1.1 encoding and give a better error message.
            // otherwise just default to the normal "encoding style not supported" error.
            if (base.IsSoapEncodingPresent(uriList))
                UnsupportedOperationBindingWarning(Res.GetString(Res.WebSoap11EncodingStyleNotSupported1, Soap12.Encoding));

            return false;
        }
    }
}
