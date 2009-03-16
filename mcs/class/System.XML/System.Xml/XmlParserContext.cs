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
using System.Collections;
using System.IO;
using System.Text;
using Mono.Xml;

#if NET_2_0
using XmlTextReaderImpl = Mono.Xml2.XmlTextReader;
#else
using XmlTextReaderImpl = System.Xml.XmlTextReader;
#endif

namespace System.Xml
{
	public class XmlParserContext
	{
		#region Class
		class ContextItem
		{
			public string BaseURI;
			public string XmlLang;
			public XmlSpace XmlSpace;
		}
		#endregion

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
					new XmlTextReaderImpl (TextReader.Null, nt).GenerateDTDObjectModel (
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
			this.namespaceManager = nsMgr;
			this.nameTable = nt != null ? nt : nsMgr != null ? nsMgr.NameTable : null;
			if (dtd != null) {
				this.DocTypeName = dtd.Name;
				this.PublicId = dtd.PublicId;
				this.SystemId = dtd.SystemId;
				this.InternalSubset = dtd.InternalSubset;
				this.dtd = dtd;
			}
			this.encoding = enc;

			this.BaseURI = baseURI;
			this.XmlLang = xmlLang;
			this.xmlSpace = xmlSpace;

			contextItems = new ArrayList ();
		}
		#endregion

		#region Fields

		private string baseURI = String.Empty;
		private string docTypeName = String.Empty;
		private Encoding encoding;
		private string internalSubset = String.Empty;
		private XmlNamespaceManager namespaceManager;
		private XmlNameTable nameTable;
		private string publicID = String.Empty;
		private string systemID = String.Empty;
		private string xmlLang = String.Empty;
		private XmlSpace xmlSpace;
		private ArrayList contextItems;
		private int contextItemCount;
		private DTDObjectModel dtd;

		#endregion

		#region Properties

		public string BaseURI {
			get { return baseURI; }
			set { baseURI = value != null ? value : String.Empty; }
		}

		public string DocTypeName {
			get { return docTypeName != null ? docTypeName : dtd != null ? dtd.Name : null; }
			set { docTypeName = value != null ? value : String.Empty; }
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
			set { internalSubset = value != null ? value : String.Empty; }
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
			set { publicID = value != null ? value : String.Empty; }
		}

		public string SystemId {
			get { return systemID != null ? systemID : dtd != null ? dtd.SystemId : null; }
			set { systemID = value != null ? value : String.Empty; }
		}

		public string XmlLang {
			get { return xmlLang; }
			set { xmlLang = value != null ? value : String.Empty; }
		}

		public XmlSpace XmlSpace {
			get { return xmlSpace; }
			set { xmlSpace = value; }
		}

		#endregion

		#region Methods
		internal void PushScope ()
		{
			ContextItem item = null;
			if (contextItems.Count == contextItemCount) {
				item = new ContextItem ();
				contextItems.Add (item);
			}
			else
				item = (ContextItem) contextItems [contextItemCount];
			item.BaseURI = BaseURI;
			item.XmlLang = XmlLang;
			item.XmlSpace = XmlSpace;
			contextItemCount++;
		}

		internal void PopScope ()
		{
			if (contextItemCount == 0)
				throw new XmlException ("Unexpected end of element scope.");
			contextItemCount--;
			ContextItem prev = (ContextItem) contextItems [contextItemCount];
			baseURI = prev.BaseURI;
			xmlLang = prev.XmlLang;
			xmlSpace = prev.XmlSpace;
		}
		#endregion
	}
}
