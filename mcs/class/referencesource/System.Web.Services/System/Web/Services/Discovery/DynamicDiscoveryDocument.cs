//------------------------------------------------------------------------------
// <copyright file="DynamicDiscoveryDocument.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.IO;
    using System.Collections;
    using System.Xml.Serialization;
    using System.Text;
    
    /// <include file='doc\DynamicDiscoveryDocument.uex' path='docs/doc[@for="DynamicDiscoveryDocument"]/*' />
    /// <devdoc>
    ///    This represents a discovery file.
    /// </devdoc>
    [XmlRoot("dynamicDiscovery", Namespace = DynamicDiscoveryDocument.Namespace)]
    public sealed class DynamicDiscoveryDocument {
        private ExcludePathInfo[] excludePaths = new ExcludePathInfo[0];

        /// <include file='doc\DynamicDiscoveryDocument.uex' path='docs/doc[@for="DynamicDiscoveryDocument.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const string Namespace = "urn:schemas-dynamicdiscovery:disco.2000-03-17";

        /// <include file='doc\DynamicDiscoveryDocument.uex' path='docs/doc[@for="DynamicDiscoveryDocument.DynamicDiscoveryDocument"]/*' />
        /// <devdoc>
        ///     Default constructor.
        /// </devdoc>
        public DynamicDiscoveryDocument() {
        }

        /// <include file='doc\DynamicDiscoveryDocument.uex' path='docs/doc[@for="DynamicDiscoveryDocument.ExcludePaths"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("exclude", typeof(ExcludePathInfo))]
        public ExcludePathInfo[] ExcludePaths {
            get {
                return excludePaths;
            }
            set {
                if (value == null)
                    value = new ExcludePathInfo[0];
                excludePaths = value;
            }
        }

        /// <include file='doc\DynamicDiscoveryDocument.uex' path='docs/doc[@for="DynamicDiscoveryDocument.Write"]/*' />
        /// <devdoc>
        ///    Write this instance to a stream.
        /// </devdoc>
        public void Write(Stream stream) {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DynamicDiscoveryDocument));
            xmlSerializer.Serialize(new StreamWriter(stream, new UTF8Encoding(false)), this);
        }

        /// <include file='doc\DynamicDiscoveryDocument.uex' path='docs/doc[@for="DynamicDiscoveryDocument.Load"]/*' />
        /// <devdoc>
        ///    Read an instance of WebMethodsFile from a stream.
        /// </devdoc>
        public static DynamicDiscoveryDocument Load(Stream stream) {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DynamicDiscoveryDocument));
            return (DynamicDiscoveryDocument) xmlSerializer.Deserialize(stream);
        }
    }
}
