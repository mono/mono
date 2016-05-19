//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.CodeDom;
    using WsdlNS = System.Web.Services.Description;
    using System.Xml.Serialization;

    public class DataContractSerializerMessageContractImporter : IWsdlImportExtension
    {
        bool enabled = true;
        internal const string GenericMessageSchemaTypeName = "MessageBody";
        internal const string GenericMessageSchemaTypeNamespace = "http://schemas.microsoft.com/Message";
        const string StreamBodySchemaTypeName = "StreamBody";
        const string StreamBodySchemaTypeNamespace = GenericMessageSchemaTypeNamespace;

        static internal XmlQualifiedName GenericMessageTypeName = new XmlQualifiedName(GenericMessageSchemaTypeName, GenericMessageSchemaTypeNamespace);
        static internal XmlQualifiedName StreamBodyTypeName = new XmlQualifiedName(StreamBodySchemaTypeName, StreamBodySchemaTypeNamespace);

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext endpointContext)
        {
            if (endpointContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endpointContext"));

            if (enabled)
                MessageContractImporter.ImportMessageBinding(importer, endpointContext, typeof(MessageContractImporter.DataContractSerializerSchemaImporter));
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext contractContext)
        {
            if (contractContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contractContext"));

            if (enabled)
                MessageContractImporter.ImportMessageContract(importer, contractContext, MessageContractImporter.DataContractSerializerSchemaImporter.Get(importer));
        }

        void IWsdlImportExtension.BeforeImport(WsdlNS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy) { }

        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }

        }
    }

    public class XmlSerializerMessageContractImporter : IWsdlImportExtension
    {

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext endpointContext)
        {
            if (endpointContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endpointContext"));

            MessageContractImporter.ImportMessageBinding(importer, endpointContext, typeof(MessageContractImporter.XmlSerializerSchemaImporter));
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext contractContext)
        {
            if (contractContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contractContext"));

            MessageContractImporter.ImportMessageContract(importer, contractContext, MessageContractImporter.XmlSerializerSchemaImporter.Get(importer));
        }

        void IWsdlImportExtension.BeforeImport(WsdlNS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy) { }

    }

}
