// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlParserContext.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

namespace System.Xml
{
	public class XmlParserContext
	{
		// constructors

		public XmlParserContext(
			XmlNameTable nameTable,
			XmlNamespaceManager namespaceManager,
			string xmlLang,
			XmlSpace xmlSpace) :

			this(
				nameTable,
				namespaceManager,
				null,
				null,
				null,
				null,
				null,
				xmlLang,
				xmlSpace
			)
		{
		}

		public XmlParserContext(
			XmlNameTable nameTable,
			XmlNamespaceManager namespaceManager,
			string docTypeName,
			string publicID,
			string systemID,
			string internalSubset,
			string baseURI,
			string xmlLang,
			XmlSpace xmlSpace)
		{
			this.nameTable = nameTable;
			this.namespaceManager = namespaceManager;
			this.docTypeName = docTypeName;
			this.publicID = publicID;
			this.systemID = systemID;
			this.internalSubset = internalSubset;
			this.baseURI = baseURI;
			this.xmlLang = xmlLang;
			this.xmlSpace = xmlSpace;
		}

		// properties

		public string BaseURI
		{
			get { return baseURI; }
			set { baseURI = value; }
		}

		public string DocTypeName
		{
			get { return docTypeName; }
			set { docTypeName = value; }
		}

		public string InternalSubset
		{
			get { return internalSubset; }
			set { internalSubset = value; }
		}

		public XmlNamespaceManager NamespaceManager
		{
			get { return namespaceManager; }
			set { namespaceManager = value; }
		}

		public XmlNameTable NameTable
		{
			get { return nameTable; }
			set { nameTable = nameTable; }
		}

		public string PublicId
		{
			get { return publicID; }
			set { publicID = value; }
		}

		public string SystemId
		{
			get { return systemID; }
			set { systemID = value; }
		}

		public string XmlLang
		{
			get { return xmlLang; }
			set { xmlLang = value; }
		}

		public XmlSpace XmlSpace
		{
			get { return xmlSpace; }
			set { xmlSpace = value; }
		}

		// privates

		private string baseURI;
		private string docTypeName;
		private string internalSubset;
		private XmlNamespaceManager namespaceManager;
		private XmlNameTable nameTable;
		private string publicID;
		private string systemID;
		private string xmlLang;
		private XmlSpace xmlSpace;
	}
}
