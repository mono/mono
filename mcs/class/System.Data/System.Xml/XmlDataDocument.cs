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
//     Ville Palo <vi64pa@koti.soon.fi>
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
using System.Collections;

namespace System.Xml {

	public class XmlDataDocument : XmlDocument {

		#region Fields

		private DataSet dataSet;
		private bool isReadOnly = false;

		private int dataRowID = 1;
		private ArrayList dataRowIDList = new ArrayList ();

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
				return isReadOnly;
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
			
			DataTable dt = null;

			// For reading xml to XmlDocument
			XmlTextReader textReader = new XmlTextReader (
				reader.BaseURI);

			if (reader.NodeType != XmlNodeType.Element)
				reader.MoveToContent ();

			// TODO: Findout which exception should be throwen
			if (reader.NodeType != XmlNodeType.Element)
				throw new Exception ();

			if (dataSet.DataSetName != reader.Name)
				throw new Exception ();

			// read to next element
			while (reader.Read () && reader.NodeType != XmlNodeType.Element);

			do {

				// Find right table from tablecollection
				for (int i = 0; i < DataSet.Tables.Count && dt == null; i++) {
					if (reader.Name == DataSet.Tables [i].TableName) {
						dt = DataSet.Tables [i];
						dt.ColumnChanged += new DataColumnChangeEventHandler (OnDataTableColumnChanged);

					}
				}
				
				// TODO: Findout what kind of exception 
				if (dt == null) 
					throw new Exception (); // there were no correct table
				
				while ((reader.NodeType != XmlNodeType.EndElement ||
					reader.Name != dt.TableName) && reader.Read()) {
					
					switch (reader.NodeType) {
						
				        case XmlNodeType.Element:
						dt.Rows.Add (LoadRow (reader, dt.NewRow ()));
						
						break;
				        default:
						break;
					}			
				}		
			} while (reader.Read ());

			base.Load (textReader);
		}
		
		#endregion // overloaded Load methods

		[MonoTODO]
		public override void WriteContentTo(XmlWriter xw) {
			base.WriteContentTo (xw);
		}

		[MonoTODO]
		public override void WriteTo(XmlWriter w) {
			base.WriteTo (w);
		}

		#endregion // Public Methods

		#region Protected Methods

		[MonoTODO]
		protected internal override XPathNavigator CreateNavigator(XmlNode node) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public new XPathNavigator CreateNavigator() {
			throw new NotImplementedException();
		}

		#endregion // Protected Methods
		
		#region Private Methods

		[MonoTODO]
		private void OnDataTableColumnChanged(object sender, 
							     DataColumnChangeEventArgs eventArgs)
		{
			// row is not yet in datatable
			if (eventArgs.Row.XmlRowID == 0)
				return;

			// TODO: Here should be some kind of error checking.
			GetElementsByTagName (eventArgs.Column.ToString ()) [dataRowIDList.IndexOf (
				eventArgs.Row.XmlRowID)].InnerText = (string)eventArgs.ProposedValue;
			
			
		}
	
		[MonoTODO]
		private static void OnDataTableRowDeleted(object sender,
							  DataRowChangeEventArgs eventArgs)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		private DataRow LoadRow (XmlReader reader, DataRow row)
		{
			// This method returns DataRow filled by values
			// from xmldocument
			string rowname = reader.Name;
			string column = "";
			     		
			if (reader.NodeType == XmlNodeType.Element)
				column = reader.Name;

			reader.Read ();

			if (reader.NodeType == XmlNodeType.Text) {

					string val = reader.Value;
					if (row.Table.Columns.Contains (column))
						row [column] = val;
			}

			// Every row must have unique id.
			row.XmlRowID = dataRowID;
			dataRowIDList.Add (dataRowID);
			dataRowID++;					

			return row;
		}

		#endregion
	}
}
