//
// System.Data.Mapping.IDomainSchema
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace System.Data.Mapping {
        public interface IDomainSchema
        {
		#region Properties

		MappingDataSourceType DomainType { get; }

		#endregion // Properties

		#region Methods

		IDomainConstraint GetDomainConstraint (string name, IXmlNamespaceResolver namespaces);
		IDomainStructure GetDomainStructure (string select, IXmlNamespaceResolver namespaces);
		void Read (string url, ValidationEventHandler validationEventHandler);
		void Read (string url);
		void Read (XmlReader reader, ValidationEventHandler validationEventHandler);
		void Read (XmlReader reader);
		void ReadExtensions (XmlReader reader, ValidationEventHandler validationEventHandler);
		void ReadExtensions (XmlReader reader);
		void Write (string url, IXmlNamespaceResolver namespaceResolver);
		void Write (string url);
		void Write (XmlWriter writer, IXmlNamespaceResolver namespaceResolver);
		void Write (XmlWriter writer);
		void Write (Stream stream, IXmlNamespaceResolver namespaceResolver);
		void Write (Stream stream);
		void Write (TextWriter writer, IXmlNamespaceResolver namespaceResolver);
		void Write (TextWriter writer);
		void WriteExtensions (XmlWriter writer, IXmlNamespaceResolver namespaceResolver);
		void WriteExtensions (XmlWriter writer);

		#endregion // Methods
        }
}

#endif // NET_1_2
