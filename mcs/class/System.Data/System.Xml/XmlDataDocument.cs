//
// mcs/class/System.Data/System.Xml/XmlDataDocument.cs
//
// Purpose: Provides a W3C XML DOM Document to interact with
//          relational data in a DataSet
//
// class: XmlDataDocument
// assembly: System.Data.dll
// namespace: System.Xml
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Daniel Morgan
//
// XmlDataDocument is included within the Mono Class Library.
//

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace System.Xml {

	public class XmlDataDocument : XmlDocument {

		#region Fields

		private DataSet dataSet;

		#endregion // Fields

		#region Constructors

		public XmlDataDocument() {
			dataSet = new DataSet();
		}

		public XmlDataDocument(DataSet dataset) {
			this.dataSet = dataset;
		}

		#endregion // Constructors

		#region Public Properties

		public override string BaseURI {
			[MonoTODO]
			get {
				// TODO: why are we overriding?
				return base.BaseURI;
			}
		}

		public DataSet DataSet {
			[MonoTODO]
			get {
				return dataSet;
			}
		}

		// override inheritted method from XmlDocument
		public override string InnerXml {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}
				 
			[MonoTODO]
			set {
				throw new NotImplementedException();
			}
		}

		public override bool IsReadOnly {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}

		}

		// Item indexer
		public override XmlElement this[string name] {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}
		}

		// Item indexer
		public override XmlElement this[string localname, string ns] {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}
		}

		public override string LocalName {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}
		}

		public override string Name {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}
		}

		public override XmlDocument OwnerDocument {
			[MonoTODO]
			get {
				return null;
			}
		}

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public override XmlNode CloneNode(bool deep) 
		{
			throw new NotImplementedException();
		}

		#region overloaded CreateElement methods

		[MonoTODO]
		public new XmlElement CreateElement(string prefix,
				string localName, string namespaceURI) 
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public new XmlElement CreateElement(string qualifiedName,
				string namespaceURI) 
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public new XmlElement CreateElement(string name) 
		{
			throw new NotImplementedException();
		}

		#endregion // overloaded CreateElement Methods
			
		// will not be supported
		public override XmlEntityReference CreateEntityReference(string name) 
		{
			throw new NotSupportedException();
		}
		
		// will not be supported
		public override XmlElement GetElementById(string elemId) 
		{
			throw new NotSupportedException();
		}

		// get the XmlElement associated with the DataRow
		public XmlElement GetElementFromRow(DataRow r) 
		{
			throw new NotImplementedException();
		}

		// get the DataRow associated with the XmlElement
		[MonoTODO]
		public DataRow GetRowFromElement(XmlElement e) 
		{
			throw new NotImplementedException();
		}

		#region overload Load methods

		[MonoTODO]
		public override void Load(Stream inStream) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Load(string filename) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Load(TextReader txtReader) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Load(XmlReader reader) {
			throw new NotImplementedException();
		}

		#endregion // overloaded Load methods

		[MonoTODO]
		public override void WriteContentTo(XmlWriter xw) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void WriteTo(XmlWriter w) {
			throw new NotImplementedException();
		}

		#endregion // Public Methods

		#region Protected Methods

		[MonoTODO]
		protected override XPathNavigator CreateNavigator(XmlNode node) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public new XPathNavigator CreateNavigator() {
			throw new NotImplementedException();
		}

		#endregion // Protected Methods

	}
}
