#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// This class defines strings used in different type of metadata files
    /// </summary>
    /// <remarks></remarks>
    internal class XmlStrings
    {
        /// <summary>
        /// Strings for Disco File
        /// </summary>
        /// <remarks></remarks>
        internal class DISCO
        {
            internal const string Prefix = "disco";
            internal const string NamespaceUri = System.Web.Services.Discovery.DiscoveryDocument.Namespace;

            internal class Elements
            {
                internal const string Root = "discovery";
            }
        }

        /// <summary>
        /// Strings for WSDL File
        /// </summary>
        /// <remarks></remarks>
        internal class WSDL
        {
            internal const string Prefix = "wsdl";
            internal const string NamespaceUri = System.Web.Services.Description.ServiceDescription.Namespace;

            internal class Elements
            {
                internal const string Root = "definitions";
            }
        }

        /// <summary>
        /// Strings for Xml Schema File
        /// </summary>
        /// <remarks></remarks>
        internal class XmlSchema
        {
            internal const string Prefix = "xsd";
            internal const string NamespaceUri = System.Xml.Schema.XmlSchema.Namespace;

            internal class Elements
            {
                internal const string Root = "schema";
            }
        }

        /// <summary>
        /// Strings for DataSet File
        /// </summary>
        /// <remarks></remarks>
        internal class DataSet
        {
            internal const string NamespaceUri = "urn:schemas-microsoft-com:xml-msdata";

            internal class Attributes
            {
                internal const string IsDataSet = "IsDataSet";
            }
        }

        /// <summary>
        /// Strings for MetadataExchange File
        /// </summary>
        /// <remarks></remarks>
        internal class MetadataExchange
        {
            internal const string Prefix = "wsx";
            internal const string Name = "WS-MetadataExchange";
            internal const string NamespaceUri = "http://schemas.xmlsoap.org/ws/2004/09/mex";

            internal class Elements
            {
                internal const string Metadata = "Metadata";
            }
        }

        internal class WsdlContractInheritance
        {
            internal const string Prefix = "wsdl-ex";
            internal const string NamespaceUri = "http://schemas.microsoft.com/ws/2005/01/WSDL/Extensions/ContractInheritance";
        }

        /// <summary>
        /// Strings for general Xml File
        /// </summary>
        /// <remarks></remarks>
        internal class Xml
        {
            internal const string Prefix = "xml";
            internal const string NamespaceUri = "http://www.w3.org/XML/1998/namespace";

            internal class Attributes
            {
                internal const string Base = "base";
                internal const string Id = "id";
            }
        }

        internal class WSAddressing
        {
            internal const string Prefix = "wsa";
            internal const string NamespaceUri = "http://schemas.xmlsoap.org/ws/2004/08/addressing";

            internal class Elements
            {
                internal const string EndpointReference = "EndpointReference";
            }
        }

        internal class Wsu
        {
            internal const string Prefix = "wsu";
            internal const string NamespaceUri = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

            internal class Attributes
            {
                internal const string Id = "Id";
            }
        }

        /// <summary>
        /// Strings for Policy File
        /// </summary>
        /// <remarks></remarks>
        internal class WSPolicy
        {
            internal const string Prefix = "wsp";
            internal const string NamespaceUri = "http://schemas.xmlsoap.org/ws/2004/09/policy";
            internal const string NamespaceUri15 = "http://www.w3.org/ns/ws-policy";

            internal class Attributes
            {
                internal const string PolicyURIs = "PolicyURIs";
            }
            internal class Elements
            {
                internal const string PolicyReference = "PolicyReference";
                internal const string All = "All";
                internal const string ExactlyOne = "ExactlyOne";
                internal const string Policy = "Policy";
            }
        }

        internal class DataServices
        {
            internal const string NamespaceUri = "http://schemas.microsoft.com/ado/2007/06/edmx";

            internal class Elements
            {
                internal const string Root = "Edmx";
            }
        }
    }
}


