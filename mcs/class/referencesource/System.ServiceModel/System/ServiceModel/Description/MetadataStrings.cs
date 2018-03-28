//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    internal static class MetadataStrings
    {
        public static class MetadataExchangeStrings
        {
            /*
             * This file has a counterpart XmlStrings.cs in the svcutil codebase. 
             * When making chnages here, please consider whether they should be made there as well
             */
            public const string Prefix = "wsx";
            public const string Name = "WS-MetadataExchange";
            public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex";
            public const string HttpBindingName = "MetadataExchangeHttpBinding";
            public const string HttpsBindingName = "MetadataExchangeHttpsBinding";
            public const string TcpBindingName = "MetadataExchangeTcpBinding";
            public const string NamedPipeBindingName = "MetadataExchangeNamedPipeBinding";
            public const string BindingNamespace = "http://schemas.microsoft.com/ws/2005/02/mex/bindings";
    
            public const string Metadata = "Metadata";
            public const string MetadataSection = "MetadataSection";
            public const string Dialect = "Dialect";
            public const string Identifier = "Identifier";
            public const string MetadataReference = "MetadataReference";
            public const string Location = "Location";

        }

        public static class WSTransfer
        {
            public const string Prefix = "wxf";
            public const string Name = "WS-Transfer";
            public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/09/transfer";

            public const string GetAction = Namespace + "/Get";
            public const string GetResponseAction = Namespace + "/GetResponse";
        }

        public static class ServiceDescription
        {
            public const string Definitions = "definitions";
            public const string ArrayType = "arrayType";
        }
        
        public static class XmlSchema
        {
            public const string Schema = "schema";
        }

        public static class Xml
        {
            public const string Prefix = "xml";
            public const string NamespaceUri = "http://www.w3.org/XML/1998/namespace";

            public static class Attributes
            {
                public const string Id = "id";
            }
                
        }

        public static class Addressing200408
        {
            public const string Prefix = "wsa";
            public const string NamespaceUri = Addressing200408Strings.Namespace;

            public static class Policy
            {
                public const string Prefix = "wsap";
                public const string NamespaceUri = Addressing200408Strings.Namespace + "/policy";
                public const string UsingAddressing = "UsingAddressing";
            }
        }

        public static class Addressing10
        {
            public const string Prefix = "wsa10";
            public const string NamespaceUri = Addressing10Strings.Namespace;

            public static class WsdlBindingPolicy
            {
                public const string Prefix = "wsaw";
                public const string NamespaceUri = "http://www.w3.org/2006/05/addressing/wsdl";
                public const string UsingAddressing = "UsingAddressing";
            }

            public static class MetadataPolicy
            {
                public const string Prefix = "wsam";
                public const string NamespaceUri = "http://www.w3.org/2007/05/addressing/metadata";
                public const string Addressing = "Addressing";
                public const string AnonymousResponses = "AnonymousResponses";
                public const string NonAnonymousResponses = "NonAnonymousResponses";
            }
        }

        public static class AddressingWsdl
        {
            public const string Prefix = "wsaw";
            public const string NamespaceUri = "http://www.w3.org/2006/05/addressing/wsdl";
            public const string Action = "Action";
        }

        public static class AddressingMetadata
        {
            public const string Prefix = "wsam";
            public const string NamespaceUri = "http://www.w3.org/2007/05/addressing/metadata";
            public const string Action = "Action";
        }

        public static class Wsu
        {
            public const string Prefix = "wsu";
            public const string NamespaceUri = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
            public static class Attributes
            {    
                public const string Id = "Id";
            }
        }

        public static class WSPolicy
        {
            public const string Prefix = "wsp";
            public const string NamespaceUri = "http://schemas.xmlsoap.org/ws/2004/09/policy";
            public const string NamespaceUri15 = "http://www.w3.org/ns/ws-policy";

            public static class Attributes
            {
                public const string Optional = "Optional";
                public const string PolicyURIs = "PolicyURIs";
                public const string URI = "URI";
                public const string TargetNamespace = "TargetNamespace";
            }
            public static class Elements
            {
                public const string PolicyReference = "PolicyReference";
                public const string All = "All";
                public const string ExactlyOne = "ExactlyOne";
                public const string Policy = "Policy";
            }
        }

    }
}
