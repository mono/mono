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
			this.NodeChanged += new XmlNodeChangedEventHandler (OnXmlDataChanged);
			//this.NodeChanging += new XmlNodeChangedEventHandler (OnXmlDataColumnChanged);
			//this.NodeInserted += new XmlNodeChangedEventHandler (OnXmlDataColumnChanged);
			this.NodeRemoved += new XmlNodeChangedEventHandler (OnXmlDataChanged);
		}

		public XmlDataDocument(DataSet dataset) {
			this.dataSet = dataset;
			this.NodeChanged += new XmlNodeChangedEventHandler (OnXmlDataChanged);
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
		[MonoTODO ("Exceptions")]
		public XmlElement GetElementFromRow(DataRow r) 
		{
			if (r.XmlRowID == 0) // datarow was not in xmldatadocument
				throw new Exception ();

			int elementRow = dataRowIDList.IndexOf (r.XmlRowID);
			
			return (XmlElement)GetElementsByTagName (r.Table.TableName) [elementRow];
		}

		// get the DataRow associated with the XmlElement
		[MonoTODO ("Exceptions")]
		public DataRow GetRowFromElement(XmlElement e)
		{
			XmlElement node = e;
			if (node == null)
				return null;

			XPathNavigator nodeNavigator = node.CreateNavigator ();
			int c  = GetElementsByTagName (node.Name).Count;
			
			if (c == 0)
				return null;

			// FIXME: I dont know which way it should be but this work on linux.
			// could be also GetElementsByTagName (args.OldParent.Name) []
			XmlNodeList nodeList = GetElementsByTagName (node.Name);
			
			int i = 0;
			bool isSame = false;

			while (i < c && !isSame) {

				XPathNavigator docNavigator = nodeList [i].CreateNavigator ();
				isSame = docNavigator.IsSamePosition (nodeNavigator);
				docNavigator = nodeList [i].CreateNavigator ();
				if (!isSame)
					i++;
			}

			if (!isSame)
				return null;

			if (i >= dataRowIDList.Count)
				return null;

			// now we know rownum			
			int xmlrowid = (int)dataRowIDList [i];
			if (xmlrowid <= 0)
				return null;

			DataTable dt = DataSet.Tables [node.Name];
			DataRow row = null;

			if (dt == null)
				return null;

			foreach (DataRow r in dt.Rows) {
				if (xmlrowid == r.XmlRowID) {
					row = r;
				}
			}

			
			return row;			
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
			if (reader.NodeType != XmlNodeType.Element) {
				throw new Exception ();
			}

			if (dataSet.DataSetName != reader.Name) {
				throw new Exception ();
			}

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

				// Read rows to table
				DataRow tempRow = dt.NewRow ();
				while ((reader.NodeType != XmlNodeType.EndElement ||
					reader.Name != dt.TableName) && reader.Read()) {
					
					switch (reader.NodeType) {
						
				        case XmlNodeType.Element:
						// Add column to DataRow
						LoadRow (reader, ref tempRow);
						break;
				        default:
						break;
					}			
				}

				// Every row must have unique id.
				tempRow.XmlRowID = dataRowID;
				dataRowIDList.Add (dataRowID);
				dt.Rows.Add (tempRow);
				dataRowID++;					
				
				
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
		
		// Invoked when XmlNode is changed colum is changed
		[MonoTODO]
		private void OnXmlChanged (object sender, XmlNodeChangedEventArgs args)
		{
			if (args.Node == null)
				return;
			
			DataRow row = GetRowFromElement ((XmlElement)args.Node);

			if (row == null)
				return;
			
			if (row [args.Node.Name] != args.Node.InnerText)
				row [args.Node.Name] = args.Node.InnerText;
		}

		// Invoked when XmlNode is removed
		[MonoTODO]
		private void OnXmlRemoved (object sender, XmlNodeChangedEventArgs args)
		{
			if (args.OldParent == null)
				return;

			DataRow row = GetRowFromElement ((XmlElement)args.OldParent);
			
			if (row == null) {
				return ;
			}

			// Dont trig event again
			row.Table.ColumnChanged -= new DataColumnChangeEventHandler (OnDataTableColumnChanged);
			row [args.Node.Name] = null;

			// if all columns are "nulled" we can remove the row. 
		        // FIXME: This is rather slow, try to find faster way
			bool allNulls = true;
			foreach (DataColumn dc in row.Table.Columns) {
				
				if (row [dc.ColumnName] != DBNull.Value) {
					allNulls = false;
					break;
				} 
			}

			if (allNulls) {
				dataRowIDList.Remove (row.XmlRowID);
				row.Delete ();
			}

			row.Table.ColumnChanged += new DataColumnChangeEventHandler (OnDataTableColumnChanged);
		}

		// this changed datatable values when some of xmldocument elements is changed
		[MonoTODO("Insert")]
		private void OnXmlDataChanged (object sender, XmlNodeChangedEventArgs args)
		{
			if (args == null)
				return ;

			switch  (args.Action) {
				
			        case XmlNodeChangedAction.Change:
					OnXmlChanged (sender, args);
					break;
			        case XmlNodeChangedAction.Remove:
					OnXmlRemoved (sender, args);
					break;
			        case XmlNodeChangedAction.Insert:
					break;
			}
					
		}

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
			if (rowIndex <= 0 || rowIndex > GetElementsByTagName (deletedRow.Table.TableName).Count - 1)
				return;
			
			// Remove element from xmldocument and row indexlist
			// FIXME: this is one way to do this, but i hope someday i find out much better way.
			XmlNode p = GetElementsByTagName (deletedRow.Table.TableName) [rowIndex].ParentNode;
			if (p != null) {
				p.RemoveChild (GetElementsByTagName (deletedRow.Table.TableName) [rowIndex]);
				dataRowIDList.RemoveAt (rowIndex);
			}
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

			        case DataRowAction.Rollback:
					OnDataTableRowRollback (eventArgs);
					break;
			        default:
					break;
			} 
		}

		// Added
		[MonoTODO]
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
			DocumentElement.AppendChild (element);
			
			XmlElement rowElement = null;
			for (int i = 0; i < row.Table.Columns.Count; i++) {

				rowElement = CreateElement (row.Table.Columns [i].ToString ());
				rowElement.InnerText = (string)row [i];
				element.AppendChild (rowElement);
			}
		}

		// Rollback
		[MonoTODO]
		private void OnDataTableRowRollback (DataRowChangeEventArgs args)
		{
			DataRow row = args.Row;
			
			int rowid = dataRowIDList.IndexOf (row.XmlRowID);
			
			// find right element in xmldocument
			XmlNode node = GetElementsByTagName (row.Table.TableName) [rowid];

			int rowValue = 0;
			for (int i = 0; i < node.ChildNodes.Count; i++) {

				XmlNode child = node.ChildNodes [i];
				if (child.NodeType != XmlNodeType.Whitespace)				
					child.InnerText = (string)row [rowValue++];
				
			}
		}

		#endregion // DataSet event handlers

		#region Private methods

		[MonoTODO]
		private void LoadRow (XmlReader reader, ref DataRow row)
		{			
			// dt.Rows.Add (LoadRow (reader, dt.NewRow ()));
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
		}
		
		#endregion // Private methods
	}
}

