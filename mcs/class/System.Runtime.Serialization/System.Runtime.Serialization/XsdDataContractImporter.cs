#if NET_2_0
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	public class XsdDataContractImporter
	{
		ImportOptions options;
		CodeCompileUnit ccu;

		public XsdDataContractImporter ()
		{
		}

		public XsdDataContractImporter (CodeCompileUnit ccu)
		{
			this.ccu = ccu;
		}

		public CodeCompileUnit CodeCompileUnit {
			get { return ccu; }
		}

		public ImportOptions Options {
			get { return options; }
			set { options = value; }
		}

		public CodeTypeReference GetCodeTypeReference (QName typeName)
		{
			throw new NotImplementedException ();
		}

		public bool CanImport (XmlSchemaSet schemas)
		{
			foreach (XmlSchemaElement e in schemas.GlobalElements)
				if (!CanImport (schemas, e))
					return false;
			return true;
		}

		public bool CanImport (XmlSchemaSet schemas,
			IList<QName> typeNames)
		{
			foreach (QName name in typeNames)
				if (!CanImport (schemas, name))
					return false;
			return true;
		}

		public bool CanImport (XmlSchemaSet schemas, QName name)
		{
			return CanImport (schemas,
				(XmlSchemaElement) schemas.GlobalElements [name]);
		}

		public bool CanImport (XmlSchemaSet schemas, XmlSchemaElement element)
		{
			throw new NotImplementedException ();
		}

		public void Import (XmlSchemaSet schemas)
		{
			foreach (XmlSchemaElement e in schemas.GlobalElements)
				Import (schemas, e);
		}

		public void Import (XmlSchemaSet schemas,
			IList<QName> typeNames)
		{
			foreach (QName name in typeNames)
				Import (schemas, name);
		}

		public void Import (XmlSchemaSet schemas, QName name)
		{
			Import (schemas,
				(XmlSchemaElement) schemas.GlobalElements [name]);
		}

		public QName Import (XmlSchemaSet schemas, XmlSchemaElement element)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
