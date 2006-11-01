// 
// System.Xml.Serialization.SoapSchemaImporter 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@novell.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Xml;
using System.CodeDom.Compiler;

namespace System.Xml.Serialization 
{
	public class SoapSchemaImporter 
#if NET_2_0
		: SchemaImporter
#endif
	{

		#region Fields

		XmlSchemaImporter _importer;

		#endregion

		#region Constructors

		public SoapSchemaImporter (XmlSchemas schemas)
		{
			_importer = new XmlSchemaImporter (schemas);
			_importer.UseEncodedFormat = true;
		}

		public SoapSchemaImporter (XmlSchemas schemas, CodeIdentifiers typeIdentifiers)
		{
			_importer = new XmlSchemaImporter (schemas, typeIdentifiers);
			_importer.UseEncodedFormat = true;
		}

#if NET_2_0

		public SoapSchemaImporter (XmlSchemas schemas, CodeGenerationOptions options, ImportContext context)
		{
			_importer = new XmlSchemaImporter (schemas, options, context);
			_importer.UseEncodedFormat = true;
		}
		
		public SoapSchemaImporter (XmlSchemas schemas, CodeIdentifiers typeIdentifiers, CodeGenerationOptions options)
		{
			_importer = new XmlSchemaImporter (schemas, typeIdentifiers, options);
			_importer.UseEncodedFormat = true;
		}
		
		public SoapSchemaImporter (XmlSchemas schemas,CodeGenerationOptions options, 
									CodeDomProvider codeProvider, ImportContext context)
		{
			_importer = new XmlSchemaImporter (schemas, options, codeProvider, context);
			_importer.UseEncodedFormat = true;
		}

#endif

		#endregion // Constructors

		#region Methods

		public XmlTypeMapping ImportDerivedTypeMapping (XmlQualifiedName name, Type baseType, bool baseTypeCanBeIndirect)
		{
			return _importer.ImportDerivedTypeMapping (name, baseType, baseTypeCanBeIndirect);
		}

		public XmlMembersMapping ImportMembersMapping (string name, string ns, SoapSchemaMember member)
		{
			return _importer.ImportEncodedMembersMapping (name, ns, member);
		}

		public XmlMembersMapping ImportMembersMapping (string name, string ns, SoapSchemaMember[] members)
		{
			return _importer.ImportEncodedMembersMapping (name, ns, members, false);
		}

		public XmlMembersMapping ImportMembersMapping (string name, string ns, SoapSchemaMember[] members, bool hasWrapperElement)
		{
			return _importer.ImportEncodedMembersMapping (name, ns, members, hasWrapperElement);
		}

		[MonoTODO]
		public XmlMembersMapping ImportMembersMapping (string name, string ns, SoapSchemaMember[] members, bool hasWrapperElement, Type baseType, bool baseTypeCanBeIndirect)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
