//
// System.Xml.XmlParserContext
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2001, 2002 Jason Diamond  http://injektilo.org/
// (C) 2003 Atsushi Enomoto
//
using System.Collections;
using System.Text;
using Mono.Xml;

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
				docTypeName,
				pubId,
				sysId,
				internalSubset,
				baseURI,
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
			: this (
				nt,
				nsMgr,
				(docTypeName != null && docTypeName != String.Empty) ?
					new XmlTextReader ("", nt).GenerateDTDObjectModel (
						docTypeName, pubId, sysId, internalSubset) : null,
				baseURI,
				xmlLang,
				xmlSpace,
				enc)
		{
		}

		internal XmlParserContext (XmlNameTable nt,
			XmlNamespaceManager nsMgr,
			DTDObjectModel dtd,
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
			if (dtd != null) {
				this.docTypeName = dtd.Name;
				this.publicID = dtd.PublicId;
				this.systemID = dtd.SystemId;
				this.internalSubset = dtd.InternalSubset;
				this.dtd = dtd;
			}
			this.encoding = enc;

			baseURIStack = new Stack ();
			xmlLangStack = new Stack ();
			xmlSpaceStack = new Stack ();
			baseURIStack.Push (baseURI != null ? baseURI : String.Empty);
			xmlLangStack.Push (xmlLang);
			xmlSpaceStack.Push (xmlSpace);
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
		private Stack baseURIStack;
		private Stack xmlLangStack;
		private Stack xmlSpaceStack;
		private DTDObjectModel dtd;

		#endregion

		#region Properties

		public string BaseURI {
			get { return baseURI != null ? baseURI : baseURIStack.Peek () as string; }
			set { baseURI = value; }
		}

		public string DocTypeName {
			get { return docTypeName != null ? docTypeName : dtd != null ? dtd.Name : null; }
			set { docTypeName = value; }
		}

		internal DTDObjectModel Dtd {
			get { return dtd; }
			set { dtd = value; }
		}

		public Encoding Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		public string InternalSubset {
			get { return internalSubset != null ? internalSubset : dtd != null ? dtd.InternalSubset : null; }
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
			get { return publicID != null ? publicID : dtd != null ? dtd.PublicId : null; }
			set { publicID = value; }
		}

		public string SystemId {
			get { return systemID != null ? systemID : dtd != null ? dtd.SystemId : null; }
			set { systemID = value; }
		}

		public string XmlLang {
			get { return xmlLang != null ? xmlLang : xmlLangStack.Peek () as string; }
			set { xmlLang = value; }
		}

		public XmlSpace XmlSpace {
			get { return xmlSpace != XmlSpace.None ? xmlSpace : (XmlSpace) xmlSpaceStack.Peek (); }
			set { xmlSpace = value; }
		}

		#endregion

		#region Methods
		internal void PushScope ()
		{
			baseURIStack.Push (BaseURI);
			xmlLangStack.Push (XmlLang);
			xmlSpaceStack.Push (XmlSpace);
			baseURI = null;
			xmlLang = null;
			xmlSpace = XmlSpace.None;
		}

		internal void PopScope ()
		{
			baseURIStack.Pop ();
			xmlLangStack.Pop ();
			xmlSpaceStack.Pop ();
			baseURI = null;
			xmlLang = null;
			xmlSpace = XmlSpace.None;
		}
		#endregion
	}
}
