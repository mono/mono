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
// (c)copyright 2003 Ville Palo
//
// XmlDataDocument is included within the Mono Class Library.
//

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.XPath;
using System.Collections;
using System.Globalization;
using System.ComponentModel;

namespace System.Xml {

	public class XmlDataDocument : XmlDocument {

		#region Fields

		private DataSet dataSet;
		private bool isReadOnly = false;

		private int dataRowID = 1;
		private ArrayList dataRowIDList = new ArrayList ();

		// this is needed for inserting new row to datatable via xml
		private Hashtable TempTable = new Hashtable ();

		DataColumnChangeEventHandler columnChanged;
		DataRowChangeEventHandler rowDeleted;
		DataRowChangeEventHandler rowChanged;
		CollectionChangeEventHandler tablesChanged;
		#endregion // Fields

		#region Constructors

		public XmlDataDocument ()
		{
			InitDelegateFields ();

			dataSet = new DataSet();
			dataSet._xmlDataDocument = this;
			dataSet.Tables.CollectionChanged += tablesChanged;

			AddXmlDocumentListeners ();
			DataSet.EnforceConstraints = false;
		}

		public XmlDataDocument (DataSet dataset) 
		{
			if (dataset == null)
				throw new ArgumentException ("Parameter dataset cannot be null.");
			if (dataset._xmlDataDocument != null)
				throw new ArgumentException ("DataSet cannot be associated with two or more XmlDataDocument.");

			InitDelegateFields ();

			this.dataSet = dataset;
			this.dataSet._xmlDataDocument = this;

			XmlReader xmlReader = new XmlTextReader (new StringReader (dataSet.GetXml ()));

			// Load DataSet's xml-data
			base.Load (xmlReader);
			xmlReader.Close ();
			// FIXME: This is required to avoid Load() error when for
			// example empty DataSet will be filled on Load(), but
			// not sure if it works correct.
			if (DocumentElement.ChildNodes.Count == 0)
				RemoveChild (DocumentElement);

			foreach (DataTable Table in DataSet.Tables) {
				
				foreach (DataRow Row in Table.Rows) {
					Row.XmlRowID = dataRowID;
					dataRowIDList.Add (dataRowID);
					dataRowID++;
				}
			}

			AddXmlDocumentListeners ();

			foreach (DataTable Table in dataSet.Tables) {
				Table.ColumnChanged += columnChanged;
				Table.RowDeleted += rowDeleted;
				Table.RowChanged += rowChanged;
			}
		}

		// bool clone. If we are cloning XmlDataDocument then clone should be true.
		private XmlDataDocument (DataSet dataset, bool clone)
		{
			InitDelegateFields ();

			this.dataSet = dataset;
			this.dataSet._xmlDataDocument = this;

			foreach (DataTable Table in DataSet.Tables) {
				
				foreach (DataRow Row in Table.Rows) {
					Row.XmlRowID = dataRowID;
					dataRowIDList.Add (dataRowID);
					dataRowID++;
				}
			}

			AddXmlDocumentListeners ();

			foreach (DataTable Table in dataSet.Tables) {
				Table.ColumnChanged += columnChanged;
				Table.RowDeleted += rowDeleted;
				Table.RowChanged += rowChanged;
			}
		}

		#endregion // Constructors

		#region Public Properties

		public DataSet DataSet {
			get {
				return dataSet;
			}
		}

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public override XmlNode CloneNode(bool deep) 
		{
			XmlDataDocument Document;
			if (deep)
				Document = new XmlDataDocument (DataSet.Copy (), true);
			else
				Document = new XmlDataDocument (DataSet.Clone (), true);

			Document.RemoveXmlDocumentListeners ();

			Document.PreserveWhitespace = PreserveWhitespace;
			if (deep) {
				foreach(XmlNode n in ChildNodes)
					Document.AppendChild (Document.ImportNode (n, deep));
			}

			Document.AddXmlDocumentListeners ();

			return Document;			
		}

		#region overloaded CreateElement methods

		public override XmlElement CreateElement(
                        string prefix, string localName, string namespaceURI) 
		{
			return base.CreateElement (prefix, localName, namespaceURI);
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

		public override void Load (XmlReader reader) 
		{
			if (DocumentElement != null)
				throw new InvalidOperationException ("XmlDataDocument does not support multi-time loading. New XmlDadaDocument is always required.");

			bool OldEC = DataSet.EnforceConstraints;
			DataSet.EnforceConstraints = false;

			dataSet.Tables.CollectionChanged -= tablesChanged;

			// For reading xml to XmlDocument
//			XmlTextReader textReader = new XmlTextReader (
//				reader.BaseURI);

			// dont listen these events
			RemoveXmlDocumentListeners ();
			DataTable dt = null;

			base.Load (reader);
			reader = new XmlNodeReader (this);

			if (reader.NodeType != XmlNodeType.Element)
				reader.MoveToContent ();

			// read to next element
			while (reader.Read () && reader.NodeType != XmlNodeType.Element);

			do {
				// Find right table from tablecollection
				if (DataSet.Tables.Contains (reader.LocalName)) {

					dt = DataSet.Tables [reader.LocalName];

					// Make sure event handlers are not added twice
					dt.ColumnChanged -= columnChanged;
					dt.ColumnChanged += columnChanged;

					dt.RowDeleted -= rowDeleted;
					dt.RowDeleted += rowDeleted;
					
					dt.RowChanged -= rowChanged;
					dt.RowChanged += rowChanged;
				}
				else
					continue;

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

//			base.Load (textReader);
//			textReader.Close ();

			DataSet.EnforceConstraints = OldEC;
			AddXmlDocumentListeners ();
			dataSet.Tables.CollectionChanged += tablesChanged;
		}
		
		#endregion // overloaded Load methods
		#endregion // Public Methods

		#region Protected Methods

		[MonoTODO ("Create optimized XPathNavigator")]
		protected override XPathNavigator CreateNavigator(XmlNode node) {
			return base.CreateNavigator (node);
		}

		#endregion // Protected Methods
		
		#region XmlDocument event handlers

		private void OnNodeChanging (object sender, XmlNodeChangedEventArgs args)
		{
			if (DataSet.EnforceConstraints) 
				throw new InvalidOperationException (Locale.GetText ("Please set DataSet.EnforceConstraints == false before trying to edit XmlDataDocument using XML operations."));
		}

		// Invoked when XmlNode is changed colum is changed
		[MonoTODO]
		private void OnNodeChanged (object sender, XmlNodeChangedEventArgs args)
		{

			if (args.Node == null)
				return;

			DataRow row = GetRowFromElement ((XmlElement)args.Node.ParentNode.ParentNode);

			if (row == null)
				return;

			if (!row.Table.Columns.Contains (args.Node.ParentNode.Name))
				return;

			row.Table.ColumnChanged -= columnChanged;

			if (row [args.Node.ParentNode.Name].ToString () != args.Node.InnerText)		
				row [args.Node.ParentNode.Name] = args.Node.InnerText;		

			row.Table.ColumnChanged += columnChanged;
		}

		// Invoked when XmlNode is removed
		[MonoTODO]
		private void OnNodeRemoved (object sender, XmlNodeChangedEventArgs args)
		{
			if (args.OldParent == null)
				return;

			if (!(args.OldParent is XmlElement))
				return;
			
			DataRow row = GetRowFromElement ((XmlElement)args.OldParent);
			
			if (row == null)
				return ;

			// Dont trig event again
			row.Table.ColumnChanged -= columnChanged;
			row [args.Node.Name] = null;
			row.Table.ColumnChanged += columnChanged;
		}

		private void OnNodeInserting (object sender, XmlNodeChangedEventArgs args) 
		{
			if (DataSet.EnforceConstraints) 
				throw new InvalidOperationException (Locale.GetText ("Please set DataSet.EnforceConstraints == false before trying to edit XmlDataDocument using XML operations."));
			
		}
		
		private void OnNodeInserted (object sender, XmlNodeChangedEventArgs args)
		{

			// this is table element 
			if (DataSet.Tables.Contains (args.NewParent.Name)) {

				Hashtable ht = null;
				if (TempTable.ContainsKey (args.NewParent.Name)) {

					// if TempTable contains table name, get it and remove it from hashtable
					// so we can later add it :)
					ht = TempTable [args.NewParent.Name] as Hashtable;
					TempTable.Remove (args.NewParent.Name);
				}
				else 
					ht = new Hashtable ();

				ht.Add (args.Node.Name, args.Node.InnerText);				
				TempTable.Add (args.NewParent.Name, ht);
			} 
			else if (DataSet.Tables.Contains (args.Node.Name)) {
				
				// if nodes name is same as some table in the list is is time to 
				// add row to datatable

				DataTable dt = DataSet.Tables [args.Node.Name];
				dt.RowChanged -= rowChanged;

				DataRow row = dt.NewRow ();
				Hashtable ht = TempTable [args.Node.Name] as Hashtable;
				
				IDictionaryEnumerator enumerator = ht.GetEnumerator ();
				while (enumerator.MoveNext ()) {
					if (dt.Columns.Contains (enumerator.Key.ToString ()))
						row [enumerator.Key.ToString ()] = enumerator.Value.ToString ();
				}
				
				DataSet.Tables [args.Node.Name].Rows.Add (row);
				dt.RowChanged += rowChanged;
			} 

		}

		#endregion // DataSet event handlers

		#region DataSet event handlers

		// If DataTable is added or removed from DataSet
		private void OnDataTableChanged (object sender, CollectionChangeEventArgs eventArgs)
		{
			DataTable Table = (DataTable)eventArgs.Element;
			if (eventArgs.Action == CollectionChangeAction.Add) {
				Table.ColumnChanged += columnChanged;
				Table.RowDeleted += rowDeleted;
				Table.RowChanged += rowChanged;
			}
		}

		// If column has changed 
		[MonoTODO]			
		private void OnDataTableColumnChanged(object sender, 
							     DataColumnChangeEventArgs eventArgs)
		{
			RemoveXmlDocumentListeners ();

			// row is not yet in datatable
			if (eventArgs.Row.XmlRowID == 0)
				return;

			// TODO: Here should be some kind of error checking.
			GetElementsByTagName (eventArgs.Column.ColumnName) [dataRowIDList.IndexOf (
				eventArgs.Row.XmlRowID)].InnerText = eventArgs.ProposedValue.ToString ();
			
			AddXmlDocumentListeners ();
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
			if (rowIndex == -1 || eventArgs.Row.XmlRowID == 0 || 
			    rowIndex > GetElementsByTagName (deletedRow.Table.TableName).Count - 1)
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
			RemoveXmlDocumentListeners ();

			// If XmlRowID is != 0 then it is already added
			if (args.Row.XmlRowID != 0)
				return;
			
			// Create row element. Row's name same as TableName					
			DataRow row = args.Row;
			row.XmlRowID = dataRowID;
			dataRowIDList.Add (dataRowID);
			dataRowID++;

			if (DocumentElement == null)
				this.AppendChild (CreateElement (DataSet.DataSetName));

			XmlElement element = CreateElement (args.Row.Table.TableName);
			DocumentElement.AppendChild (element);

			XmlElement rowElement = null;

			for (int i = 0; i < row.Table.Columns.Count; i++) {

				rowElement = CreateElement (row.Table.Columns [i].ColumnName);
				object v = row [i];
				rowElement.InnerText = v != null ? v.ToString () : String.Empty;
				element.AppendChild (rowElement);
			}
			
			AddXmlDocumentListeners ();
		}

		// Rollback
		[MonoTODO]
		private void OnDataTableRowRollback (DataRowChangeEventArgs args)
		{
			RemoveXmlDocumentListeners ();

			DataRow row = args.Row;			
			int rowid = dataRowIDList.IndexOf (row.XmlRowID);

			// find right element in xmldocument
			if (rowid == 0 || rowid >= GetElementsByTagName (row.Table.TableName).Count)
				return;

			XmlNode node = GetElementsByTagName (row.Table.TableName) [rowid];
			
			int rowValue = 0;
			for (int i = 0; i < node.ChildNodes.Count; i++) {
				
				XmlNode child = node.ChildNodes [i];
				if (child.NodeType != XmlNodeType.Whitespace) {
					child.InnerText = (string)row [rowValue++];
				}
			}

			AddXmlDocumentListeners ();
		}

		#endregion // DataSet event handlers

		#region Private methods
		private void InitDelegateFields ()
		{
			columnChanged = new DataColumnChangeEventHandler (OnDataTableColumnChanged);
			rowDeleted = new DataRowChangeEventHandler (OnDataTableRowDeleted);
			rowChanged = new DataRowChangeEventHandler (OnDataTableRowChanged);
			tablesChanged = new CollectionChangeEventHandler (OnDataTableChanged);
		}

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
		
		private void RemoveXmlDocumentListeners ()
		{
			this.NodeInserting -= new XmlNodeChangedEventHandler (OnNodeInserting);
			this.NodeInserted -= new XmlNodeChangedEventHandler (OnNodeInserted);
			this.NodeChanged -= new XmlNodeChangedEventHandler (OnNodeChanged);
			this.NodeChanging -= new XmlNodeChangedEventHandler (OnNodeChanging);
			this.NodeRemoved -= new XmlNodeChangedEventHandler (OnNodeRemoved);
		}

		private void AddXmlDocumentListeners ()
		{
			this.NodeInserting += new XmlNodeChangedEventHandler (OnNodeInserting);
			this.NodeInserted += new XmlNodeChangedEventHandler (OnNodeInserted);
			this.NodeChanged += new XmlNodeChangedEventHandler (OnNodeChanged);
			this.NodeChanging += new XmlNodeChangedEventHandler (OnNodeChanging);
			this.NodeRemoved += new XmlNodeChangedEventHandler (OnNodeRemoved);
		}
		#endregion // Private methods
	}
}

