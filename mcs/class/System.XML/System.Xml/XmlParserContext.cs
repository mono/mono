//
// System.Xml.XmlParserContext
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
//

using System.Text;

namespace System.Xml
{
	public class XmlParserContext
	{
		#region Constructors

		public XmlParserContext (
			XmlNameTable nt,
			XmlNamespaceManager nsMgr,
			string xmlLang,
			XmlSpace xmlSpace) :

			this (
				nt,
				nsMgr,
				null,
				null,
				null,
				null,
				null,
				xmlLang,
				xmlSpace,
				null
			)
		{
		}

		public XmlParserContext (
			XmlNameTable nt,
			XmlNamespaceManager nsMgr,
			string xmlLang,
			XmlSpace xmlSpace,
			Encoding enc) :

			this (
				nt,
				nsMgr,
				null,
				null,
				null,
				null,
				null,
				xmlLang,
				xmlSpace,
				enc
			)
		{
		}

		public XmlParserContext (
			XmlNameTable nt,
			XmlNamespaceManager nsMgr,
			string docTypeName,
			string pubId,
			string sysId,
			string internalSubset,
			string baseURI,
			string xmlLang,
			XmlSpace xmlSpace) :

			this (
				nt,
				nsMgr,
				null,
				null,
				null,
				null,
				null,
				xmlLang,
				xmlSpace,
				null
			)
		{
		}

		public XmlParserContext (
			XmlNameTable nt,
			XmlNamespaceManager nsMgr,
			string docTypeName,
			string pubId,
			string sysId,
			string internalSubset,
			string baseURI,
			string xmlLang,
			XmlSpace xmlSpace,
			Encoding enc)
		{
			if (nt == null)
				this.nameTable = nsMgr.NameTable;
			else
				this.NameTable = nt;

			this.namespaceManager = nsMgr;
			this.docTypeName = docTypeName;
			this.publicID = pubId;
			this.systemID = sysId;
			this.internalSubset = internalSubset;
			this.baseURI = baseURI != null ? baseURI : String.Empty;
			this.xmlLang = xmlLang;
			this.xmlSpace = xmlSpace;
			this.encoding = enc;
		}

		#endregion

		#region Fields

		private string baseURI;
		private string docTypeName;
		private Encoding encoding;
		private string internalSubset;
		private XmlNamespaceManager namespaceManager;
		private XmlNameTable nameTable;
		private string publicID;
		private string systemID;
		private string xmlLang;
		private XmlSpace xmlSpace;

		#endregion

		#region Properties

		public string BaseURI {
			get { return baseURI; }
			set { baseURI = value; }
		}

		public string DocTypeName {
			get { return docTypeName; }
			set { docTypeName = value; }
		}

		public Encoding Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		public string InternalSubset {
			get { return internalSubset; }
			set { internalSubset = value; }
		}

		public XmlNamespaceManager NamespaceManager {
			get { return namespaceManager; }
			set { namespaceManager = value; }
		}

		public XmlNameTable NameTable {
			get { return nameTable; }
			set { nameTable = value; }
		}

		public string PublicId {
			get { return publicID; }
			set { publicID = value; }
		}

		public string SystemId {
			get { return systemID; }
			set { systemID = value; }
		}

		public string XmlLang {
			get { return xmlLang; }
			set { xmlLang = value; }
		}

		public XmlSpace XmlSpace {
			get { return xmlSpace; }
			set { xmlSpace = value; }
		}

		#endregion
	}
}
