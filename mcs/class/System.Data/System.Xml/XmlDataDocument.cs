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
		public override XmlElement CreateElement(string prefix,
				string localName, string namespaceURI) 
		{
			if ((localName == null) || (localName == String.Empty))
				throw new ArgumentException ("The local name for elements or attributes cannot be null" +
							     "or an empty string.");
			string pref = prefix != null ? prefix : String.Empty;
			return base.CreateElement (pref, localName, namespaceURI != null ? namespaceURI : String.Empty);

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

		public override void Load(Stream inStream) {
			Load (new XmlTextReader (inStream));
		}

		public override void Load(string filename) {
			Load (new XmlTextReader (filename));
		}

		public override void Load(TextReader txtReader) {
			Load (new XmlTextReader (txtReader));
		}

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
						dt.RowDeleted += new DataRowChangeEventHandler (OnDataTableRowDeleted);
						dt.RowChanged += new DataRowChangeEventHandler (OnDataTableRowChanged);
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

		//FIXME: how do you handle this?
		//[MonoTODO]
		//protected internal override XPathNavigator CreateNavigator(XmlNode node) {
		//	throw new NotImplementedException();
		//}

		[MonoTODO]
		public new XPathNavigator CreateNavigator() {
			throw new NotImplementedException();
		}

		#endregion // Protected Methods
		
		#region DataSet event handlers

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
		private void OnDataTableRowDeleted(object sender,
							  DataRowChangeEventArgs eventArgs)
		{
			DataRow deletedRow = null;
			deletedRow = eventArgs.Row;

			if (eventArgs.Row.XmlRowID == 0)
				return;

			int rowIndex = dataRowIDList.IndexOf (eventArgs.Row.XmlRowID);

			// Remove element from xmldocument and row indexlist
			GetElementsByTagName (deletedRow.Table.TableName) [rowIndex].RemoveAll ();
			dataRowIDList.RemoveAt (rowIndex);
		}
		
		[MonoTODO]
		private void OnDataTableRowChanged(object sender, DataRowChangeEventArgs eventArgs)
		{
			switch (eventArgs.Action) {

			        case DataRowAction.Delete:
					OnDataTableRowDeleted (sender, eventArgs);
					break;

			        case DataRowAction.Add:
					OnDataTableRowAdded (eventArgs);
					break;

			        default:
					break;
			} 
		}

		private void OnDataTableRowAdded (DataRowChangeEventArgs args)
		{
			// If XmlRowID is != 0 then it is already added
			if (args.Row.XmlRowID != 0)
				return;
			
			// Create row element. Row's name same as TableName					
			DataRow row = args.Row;
			row.XmlRowID = dataRowID;
			dataRowID++;
			XmlElement element = CreateElement (args.Row.Table.TableName);
			Console.WriteLine ("-" + args.Row.Table.TableName);
			
			DocumentElement.AppendChild (element);
			
			XmlElement rowElement = null;
			for (int i = 0; i < row.Table.Columns.Count; i++) {
								
				Console.WriteLine (row.Table.Columns [i].ToString () + " " + row[i]);
				rowElement = CreateElement (row.Table.Columns [i].ToString ());
				rowElement.InnerText = (string)row [i];
				element.AppendChild (rowElement);
			}
		}

		#endregion // DataSet event handlers

		#region Private methods

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

		#endregion // Private methods
	}
}
