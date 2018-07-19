#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Discovery = System.Web.Services.Discovery;
using Description = System.Web.Services.Description;
using MetadataSection = System.ServiceModel.Description.MetadataSection;
using XmlSerialization = System.Xml.Serialization;

#if WEB_EXTENSIONS_CODE
using System.Web.Resources;
using System.Diagnostics.CodeAnalysis;
#else
using Microsoft.VSDesigner.WCF.Resources;
#endif

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// This class presents a single metadata file in the ReferenceGroup
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal class MetadataFile : ExternalFile
#else
    [CLSCompliant(true)]
    public class MetadataFile : ExternalFile
#endif
    {

        // Default File Name
        public const string DEFAULT_FILE_NAME = "service";

        private MetadataType m_MetadataType;
        private string m_SourceUrl;

        // A GUID string
        private string m_ID;

        private int m_SourceId;

        // properties to merge metadata
        private bool m_Ignore;
        private bool m_IsMergeResult;

        private int SOURCE_ID_NOT_SPECIFIED = 0;

        private MetadataContent m_CachedMetadata;

        // Content of the metadata file, one of them must be
        private byte[] m_BinaryContent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks> Must support a default construct for XmlSerializer</remarks>
        public MetadataFile()
        {
            m_ID = Guid.NewGuid().ToString();
            m_BinaryContent = new byte[] { };
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">File Name</param>
        /// <param name="Url">SourceUrl</param>
        /// <param name="Content">File Content</param>
        /// <remarks></remarks>
        public MetadataFile(string name, string url, string content)
            : base(name)
        {
            m_ID = Guid.NewGuid().ToString();

            m_SourceUrl = url;

            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            LoadContent(content);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">File Name</param>
        /// <param name="Url">SourceUrl</param>
        /// <param name="byteContent">File Content</param>
        /// <remarks></remarks>
        public MetadataFile(string name, string url, byte[] byteContent)
            : base(name)
        {
            m_ID = Guid.NewGuid().ToString();

            m_SourceUrl = url;

            if (byteContent == null)
            {
                throw new ArgumentNullException("byteContent");
            }

            LoadContent(byteContent);
        }

        /// <summary>
        /// Retrieves the file content in binary format
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public byte[] BinaryContent
        {
            get
            {
                return m_BinaryContent;
            }
        }

        /// <summary>
        /// Cached state
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        private MetadataContent CachedMetadata
        {
            get
            {
                if (m_CachedMetadata == null)
                {
                    m_CachedMetadata = LoadMetadataContent(m_MetadataType);
                }
                return m_CachedMetadata;
            }
        }


        /// <summary>
        /// Retrieves the file content
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string Content
        {
            get
            {
                StreamReader memReader = new StreamReader(new MemoryStream(m_BinaryContent));
                return memReader.ReadToEnd();
            }
        }

        /// <summary>
        /// The Type of Metadata
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute("MetadataType")]
        public MetadataType FileType
        {
            get
            {
                return m_MetadataType;
            }
            set
            {
                m_MetadataType = value;
            }
        }

        /// <summary>
        ///  GUID string, it is used to track a metadata item (when it is updated)
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string ID
        {
            get
            {
                return m_ID;
            }
            set
            {
                m_ID = value;
            }
        }

        /// <summary>
        /// If it is true, the metadata file will be ignored by the code generator
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public bool Ignore
        {
            get
            {
                return m_Ignore;
            }
            set
            {
                m_Ignore = value;
            }
        }

        /// <summary>
        /// A special attribute used by XmlSerializer to decide whether Ignore attribute exists
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public bool IgnoreSpecified
        {
            get
            {
                return m_Ignore;
            }
            set
            {
                if (!value)
                {
                    m_Ignore = false;
                }
            }
        }

        /// <summary>
        /// whether the metadata file is a result of merging
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public bool IsMergeResult
        {
            get
            {
                return m_IsMergeResult;
            }
            set
            {
                m_IsMergeResult = value;
            }
        }

        /// <summary>
        /// A special attribute used by XmlSerializer to decide whether IsMergeResult attribute exists
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public bool IsMergeResultSpecified
        {
            get
            {
                return m_IsMergeResult;
            }
            set
            {
                if (!value)
                {
                    m_IsMergeResult = false;
                }
            }
        }

        /// <summary>
        /// Retrieves the content of a discovery file
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Discovery.DiscoveryDocument MetadataDiscoveryDocument
        {
            get
            {
                return CachedMetadata.MetadataDiscoveryDocument;
            }
        }

        /// <summary>
        /// format error
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public Exception MetadataFormatError
        {
            get
            {
                return CachedMetadata.MetadataFormatError;
            }
        }

        /// <summary>
        /// Retrieves the content of a WSDL file
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Description.ServiceDescription MetadataServiceDescription
        {
            get
            {
                return CachedMetadata.MetadataServiceDescription;
            }
        }

        /// <summary>
        /// Retrieves the content of a schema file
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public XmlSchema MetadataXmlSchema
        {
            get
            {
                return CachedMetadata.MetadataXmlSchema;
            }
        }

        /// <summary>
        /// Retrieves the content of an Xml file
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public XmlDocument MetadataXmlDocument
        {
            get
            {
                return CachedMetadata.MetadataXmlDocument;
            }
        }

        /// <summary>
        /// the SourceId links the the SourceId in the MetadataSource table
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public int SourceId
        {
            get
            {
                return m_SourceId;
            }
            set
            {
                if (value < 0)
                {
                    Debug.Fail("Source ID shouldn't be a nagtive number");
                    throw new ArgumentException(WCFModelStrings.ReferenceGroup_InvalidSourceId);
                }
                m_SourceId = value;
            }
        }

        /// <summary>
        /// A special attribute used by XmlSerializer to decide whether SourceId attribute exists
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public bool SourceIdSpecified
        {
            get
            {
                return m_SourceId != SOURCE_ID_NOT_SPECIFIED;
            }
            set
            {
                if (!value)
                {
                    m_SourceId = SOURCE_ID_NOT_SPECIFIED;
                }
            }
        }

        /// <summary>
        ///  The sourceUrl of the metadata file
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string SourceUrl
        {
            get
            {
                return m_SourceUrl;
            }
            set
            {
                m_SourceUrl = value;
            }
        }

        /// <summary>
        /// Retrieves the TargetNamespace when it is a schema item or a WSDL item
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string TargetNamespace
        {
            get
            {
                return CachedMetadata.TargetNamespace;
            }
        }

        /// <summary>
        /// Detemine the type of a metadata item
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>File Type</returns>
        /// <remarks></remarks>
        private MetadataType DetermineFileType(XmlReader reader)
        {
            try
            {
                if (reader.IsStartElement(XmlStrings.WSDL.Elements.Root, XmlStrings.WSDL.NamespaceUri))
                {
                    return MetadataType.Wsdl;
                }
                else if (reader.IsStartElement(XmlStrings.XmlSchema.Elements.Root, XmlStrings.XmlSchema.NamespaceUri))
                {
                    return MetadataType.Schema;
                }
                else if (reader.IsStartElement(XmlStrings.WSPolicy.Elements.Policy, XmlStrings.WSPolicy.NamespaceUri)
                         || reader.IsStartElement(XmlStrings.WSPolicy.Elements.Policy, XmlStrings.WSPolicy.NamespaceUri15))
                {
                    return MetadataType.Policy;
                }
                else if (reader.IsStartElement(XmlStrings.DISCO.Elements.Root, XmlStrings.DISCO.NamespaceUri))
                {
                    return MetadataType.Disco;
                }
                else if (reader.IsStartElement(XmlStrings.DataServices.Elements.Root, XmlStrings.DataServices.NamespaceUri))
                {
                    return MetadataType.Edmx;
                }
                else
                {
                    return MetadataType.Xml;
                }
            }
            catch (XmlException)
            {
                // This must mean that the document isn't an XML Document so we continue trying other things...
                return MetadataType.Unknown;
            }
        }

        /// <summary>
        /// return the default extension
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        public string GetDefaultExtension()
        {
            switch (m_MetadataType)
            {
                case MetadataType.Disco:
                    return "disco";
                case MetadataType.Wsdl:
                    return "wsdl";
                case MetadataType.Schema:
                    return "xsd";
                case MetadataType.Xml:
                    return "xml";
                case MetadataType.Policy:
                    return "xml";
                case MetadataType.Edmx:
                    return "edmx";
                default:
                    return "data";
            }
        }

        /// <summary>
        /// return the default filename without extension
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        public string GetDefaultFileName()
        {
            if (!String.IsNullOrEmpty(TargetNamespace))
            {
                string ns = TargetNamespace;
                if (!ns.EndsWith("/", StringComparison.Ordinal))
                {
                    int i = ns.LastIndexOfAny(Path.GetInvalidFileNameChars());
                    if (i >= 0)
                    {
                        ns = ns.Substring(i + 1);
                    }

                    string defaultExtension = "." + GetDefaultExtension();
                    if (ns.Length > defaultExtension.Length && ns.EndsWith(defaultExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        ns = ns.Substring(0, ns.Length - defaultExtension.Length);
                    }

                    if (ns.Length > 0)
                    {
                        return ns;
                    }
                }
            }

            return DEFAULT_FILE_NAME;
        }

        /// <summary>
        /// Load the content of a Metadata item into the object model
        /// </summary>
        /// <param name="byteContent"></param>
        /// <remarks></remarks>
        internal void LoadContent(byte[] byteContent)
        {
            m_BinaryContent = byteContent;
            LoadContentFromTextReader(new StreamReader(new MemoryStream(byteContent)));
        }

        /// <summary>
        /// Load the content of a Metadata item into the object model
        /// </summary>
        /// <param name="content"></param>
        /// <remarks></remarks>
        internal void LoadContent(string content)
        {
            MemoryStream memStream = new MemoryStream();
            StreamWriter contentWriter = new StreamWriter(memStream);
            contentWriter.Write(content);
            contentWriter.Flush();
            m_BinaryContent = memStream.ToArray();

            LoadContentFromTextReader(new StringReader(content));
        }

        /// <summary>
        /// Load the content of a Metadata item into the object model
        /// </summary>
        /// <param name="contentReader"></param>
        /// <remarks></remarks>
        [SuppressMessage("Microsoft.Security.Xml", "CA3054:DoNotAllowDtdOnXmlTextReader", Justification = "Legacy code that trusts our developer-controlled input.")]
        private void LoadContentFromTextReader(TextReader contentReader)
        {
            if (contentReader == null)
            {
                throw new ArgumentNullException("contentReader");
            }

            // reset...
            ErrorInLoading = null;

            m_CachedMetadata = null;

            using (XmlTextReader xmlReader = new XmlTextReader(contentReader))
            {
                if (m_MetadataType == MetadataType.Unknown)
                {
                    // If we don't know the metedata type, we try to sniff it...
                    MetadataType fileType = DetermineFileType(xmlReader);

                    // try
                    m_CachedMetadata = LoadMetadataContent(fileType, xmlReader);
                    if (m_CachedMetadata.MetadataFormatError == null)
                    {
                        m_MetadataType = fileType;
                    }
                }
            }
        }

        /// <summary>
        ///  the function is called when the metadata is removed, and we need clean up the content
        /// </summary>
        /// <remarks></remarks>
        internal void CleanUpContent()
        {
            ErrorInLoading = null;
            m_BinaryContent = new byte[] { };

            m_CachedMetadata = null;
        }

        /// <summary>
        /// Load schema/wsdl model from binary content.  -- Parse the metadata content 
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        [SuppressMessage("Microsoft.Security.Xml", "CA3054:DoNotAllowDtdOnXmlTextReader", Justification = "Legacy code that trusts our developer-controlled input.")]
        private MetadataContent LoadMetadataContent(MetadataType fileType)
        {
            if (ErrorInLoading != null)
            {
                return new MetadataContent(ErrorInLoading);
            }
            using (XmlTextReader xmlReader = new XmlTextReader(new StreamReader(new MemoryStream(m_BinaryContent))))
            {
                return LoadMetadataContent(fileType, xmlReader);
            }
        }

        /// <summary>
        /// Load schema/wsdl model from text reader.  -- it will parse the metadata content.
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private MetadataContent LoadMetadataContent(MetadataType fileType, XmlTextReader xmlReader)
        {
            MetadataContent cachedMetadata = new MetadataContent();

            try
            {
                switch (fileType)
                {
                    case MetadataType.Disco:
                        cachedMetadata = new MetadataContent(Discovery.DiscoveryDocument.Read(xmlReader));
                        break;
                    case MetadataType.Wsdl:
                        cachedMetadata = new MetadataContent(Description.ServiceDescription.Read(xmlReader));
                        cachedMetadata.MetadataServiceDescription.RetrievalUrl = GetMetadataSourceUrl();
                        break;
                    case MetadataType.Schema:
                        cachedMetadata = new MetadataContent(XmlSchema.Read(xmlReader, null));
                        cachedMetadata.MetadataXmlSchema.SourceUri = GetMetadataSourceUrl();
                        break;
                    case MetadataType.Unknown:
                        // For unknown types, we don't do nothing...
                        break;
                    default:
                        Debug.Assert(fileType == MetadataType.Xml || fileType == MetadataType.Policy || fileType == MetadataType.Edmx);
                        XmlDocument tempDoc = new XmlDocument();
                        tempDoc.Load(xmlReader);
                        cachedMetadata = new MetadataContent(tempDoc);
                        break;
                }
            }
            catch (Exception ex)
            {
                cachedMetadata = new MetadataContent(ex);
            }

            return cachedMetadata;
        }

        /// <summary>
        /// convert metadata file to MetadataSection (to feed code/proxy generator)
        ///  We don't reuse the buffered object model, because the generator could modify & corrupt them.
        /// </summary>
        /// <remarks></remarks>
        internal MetadataSection CreateMetadataSection()
        {
            MetadataContent metadata = LoadMetadataContent(m_MetadataType);
            if (metadata.MetadataFormatError != null)
            {
                throw metadata.MetadataFormatError;
            }

            MetadataSection metadataSection = null;

            switch (FileType)
            {
                case MetadataType.Unknown:
                    break;
                case MetadataType.Disco:
                    if (metadata.MetadataServiceDescription != null)
                    {
                        metadataSection = MetadataSection.CreateFromServiceDescription(metadata.MetadataServiceDescription);
                    }
                    break;
                case MetadataType.Wsdl:
                    // We need to make a copy of the WSDL object model since the act of importing it actuall
                    // modifies it, and we don't want the cached instance to be polluted...
                    System.Web.Services.Description.ServiceDescription description = metadata.MetadataServiceDescription;
                    if (description != null)
                    {
                        metadataSection = MetadataSection.CreateFromServiceDescription(description);
                    }
                    break;
                case MetadataType.Schema:
                    if (metadata.MetadataXmlSchema != null)
                    {
                        metadataSection = MetadataSection.CreateFromSchema(metadata.MetadataXmlSchema);
                    }
                    break;
                case MetadataFile.MetadataType.Policy:
                    if (metadata.MetadataXmlDocument != null)
                    {
                        metadataSection = MetadataSection.CreateFromPolicy(metadata.MetadataXmlDocument.DocumentElement, null);
                    }
                    break;
                case MetadataFile.MetadataType.Xml:
                case MetadataFile.MetadataType.Edmx:
                    if (metadata.MetadataXmlDocument != null)
                    {
                        metadataSection = new MetadataSection(null, null, metadata.MetadataXmlDocument.DocumentElement);
                    }
                    break;
                default:
                    System.Diagnostics.Debug.Fail("Unknown Type?");
                    break;
            }
            return metadataSection;
        }

        /// <summary>
        /// Metadata source Url is used in error messages
        /// </summary>
        /// <remarks></remarks>
        internal string GetMetadataSourceUrl()
        {
            if (String.IsNullOrEmpty(SourceUrl))
            {
                return FileName;
            }
            else
            {
                return SourceUrl;
            }
        }

        /// <summary>
        /// Metadata File Type Enum
        /// </summary>
        /// <remarks></remarks>
        public enum MetadataType
        {
            [XmlSerialization.XmlEnum(Name = "Unknown")]
            Unknown = 0,
            [XmlSerialization.XmlEnum(Name = "Disco")]
            Disco = 1,
            [XmlSerialization.XmlEnum(Name = "Wsdl")]
            Wsdl = 2,
            [XmlSerialization.XmlEnum(Name = "Schema")]
            Schema = 3,
            [XmlSerialization.XmlEnum(Name = "Policy")]
            Policy = 4,
            [XmlSerialization.XmlEnum(Name = "Xml")]
            Xml = 5,
            [XmlSerialization.XmlEnum(Name = "Edmx")]
            Edmx = 6,
        }

        /// <summary>
        /// Metadata contained inside the file. Only one of field is valid, which depends on the MetadataType
        /// </summary>
        /// <remarks></remarks>
        private class MetadataContent
        {

            private Discovery.DiscoveryDocument m_MetadataDiscoveryDocument;
            private Description.ServiceDescription m_MetadataServiceDescription;
            private XmlSchema m_MetadataXmlSchema;
            private XmlDocument m_MetadataXmlDocument;

            private Exception m_MetadataFormatError;

            private string m_TargetNamespace;

            internal MetadataContent()
            {
                m_TargetNamespace = String.Empty;
            }

            internal MetadataContent(Discovery.DiscoveryDocument discoveryDocument)
            {
                m_MetadataDiscoveryDocument = discoveryDocument;
                m_TargetNamespace = String.Empty;
            }

            internal MetadataContent(Description.ServiceDescription serviceDescription)
            {
                m_MetadataServiceDescription = serviceDescription;
                m_TargetNamespace = serviceDescription.TargetNamespace;
            }

            internal MetadataContent(XmlSchema schema)
            {
                m_MetadataXmlSchema = schema;
                m_TargetNamespace = schema.TargetNamespace;
            }

            internal MetadataContent(XmlDocument document)
            {
                m_MetadataXmlDocument = document;
                m_TargetNamespace = String.Empty;
            }

            internal MetadataContent(Exception metadataFormatError)
            {
                m_MetadataFormatError = metadataFormatError;
            }

            /// <summary>
            /// Retrieves the content of a discovery file
            /// </summary>
            /// <value></value>
            /// <remarks></remarks>
            public Discovery.DiscoveryDocument MetadataDiscoveryDocument
            {
                get
                {
                    return m_MetadataDiscoveryDocument;
                }
            }

            /// <summary>
            /// Error message if 
            /// </summary>
            /// <value></value>
            /// <remarks></remarks>
            public Exception MetadataFormatError
            {
                get
                {
                    return m_MetadataFormatError;
                }
            }

            /// <summary>
            /// Retrieves the content of a WSDL file
            /// </summary>
            /// <value></value>
            /// <remarks></remarks>
            public Description.ServiceDescription MetadataServiceDescription
            {
                get
                {
                    return m_MetadataServiceDescription;
                }
            }

            /// <summary>
            /// Retrieves the content of a schema file
            /// </summary>
            /// <value></value>
            /// <remarks></remarks>
            public XmlSchema MetadataXmlSchema
            {
                get
                {
                    return m_MetadataXmlSchema;
                }
            }

            /// <summary>
            /// Retrieves the content of an Xml file
            /// </summary>
            /// <value></value>
            /// <remarks></remarks>
            public XmlDocument MetadataXmlDocument
            {
                get
                {
                    return m_MetadataXmlDocument;
                }
            }

            /// <summary>
            /// Retrieves the TargetNamespace when it is a schema item or a WSDL item
            /// </summary>
            /// <value></value>
            /// <remarks></remarks>
            public string TargetNamespace
            {
                get
                {
                    return m_TargetNamespace;
                }
            }

        }
    }

}
