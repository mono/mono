//------------------------------------------------------------------------------
// <copyright file="SoapHttpTransportImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {
    using System.CodeDom;
    using System.Web.Services.Protocols;

    internal class SoapHttpTransportImporter : SoapTransportImporter {
        public override bool IsSupportedTransport(string transport) {
            return transport == SoapBinding.HttpTransport;
        }

        public override void ImportClass() {
            // grab this here so it gets marked "handled" for both client and server
            SoapAddressBinding soapAddress = ImportContext.Port == null ? null : (SoapAddressBinding)ImportContext.Port.Extensions.Find(typeof(SoapAddressBinding));
            if (ImportContext.Style == ServiceDescriptionImportStyle.Client) {
                ImportContext.CodeTypeDeclaration.BaseTypes.Add(typeof(SoapHttpClientProtocol).FullName);                                
                CodeConstructor ctor = WebCodeGenerator.AddConstructor(ImportContext.CodeTypeDeclaration, new string[0], new string[0], null, CodeFlags.IsPublic);
                ctor.Comments.Add(new CodeCommentStatement(Res.GetString(Res.CodeRemarks), true));
                bool soap11 = true;
                if (ImportContext is Soap12ProtocolImporter) {
                    soap11 = false;
                    // add version code
                    CodeTypeReferenceExpression versionEnumTypeReference = new CodeTypeReferenceExpression(typeof(SoapProtocolVersion));
                    CodeFieldReferenceExpression versionEnumFieldReference = new CodeFieldReferenceExpression(versionEnumTypeReference, Enum.Format(typeof(SoapProtocolVersion), SoapProtocolVersion.Soap12, "G"));
                    CodePropertyReferenceExpression versionPropertyReference = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "SoapVersion");
                    CodeAssignStatement assignVersionStatement = new CodeAssignStatement(versionPropertyReference, versionEnumFieldReference);
                    ctor.Statements.Add(assignVersionStatement);
                }
                ServiceDescription serviceDescription = ImportContext.Binding.ServiceDescription;
                string url = (soapAddress != null) ? soapAddress.Location : null;
                string urlKey = serviceDescription.AppSettingUrlKey;
                string baseUrl = serviceDescription.AppSettingBaseUrl;
                ProtocolImporterUtil.GenerateConstructorStatements(ctor, url, urlKey, baseUrl, soap11 && !ImportContext.IsEncodedBinding);
            }
            else if (ImportContext.Style == ServiceDescriptionImportStyle.Server) {
                ImportContext.CodeTypeDeclaration.BaseTypes.Add(typeof(WebService).FullName);
            }
        }
    }
}
