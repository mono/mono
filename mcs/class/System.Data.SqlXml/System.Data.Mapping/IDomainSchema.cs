//
// System.Data.Mapping.IDomainSchema
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

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

#endif // NET_2_0
