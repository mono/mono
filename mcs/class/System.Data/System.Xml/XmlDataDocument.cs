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
//     Atsushi Enomoto <atsushi@ximian.com>
//
// (c)copyright 2002 Daniel Morgan
// (c)copyright 2003 Ville Palo
// (c)2004 Novell Inc.
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

namespace System.Xml 
{

	public class XmlDataDocument : XmlDocument 
	{
		// Should we consider overriding CloneNode() ? By default
		// base CloneNode() will be invoked and thus no DataRow conflict
		// would happen, that sounds the best (that means, no mapped
		// DataRow will be provided).
		internal class XmlDataElement : XmlElement
		{
			DataRow row;

			internal XmlDataElement (DataRow row, string prefix, string localName, string ns, XmlDataDocument doc)
				: base (prefix, localName, ns, doc)
			{
				this.row = row;
				// Embed row ID only when the element is mapped to
				// certain DataRow.
				if (row != null) {
					row.DataElement = this;
					row.XmlRowID = doc.dataRowID;
					doc.dataRowIDList.Add (row.XmlRowID);
					// It should not be done here. The node is detached
					// dt.Rows.Add (tempRow);
					doc.dataRowID++;
				}
			}

			internal DataRow DataRow {
				get { return row; }
			}
		}

		#region Fields

		private DataSet dataSet;

		private int dataRowID = 1;
		private ArrayList dataRowIDList = new ArrayList ();

		// this keeps whether table change events should be handles
		private bool raiseDataSetEvents = true;
		private bool raiseDocumentEvents = true;

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

			XmlElement docElem = CreateElement (dataSet.Prefix, dataSet.DataSetName, dataSet.Namespace);
			foreach (DataTable dt in dataSet.Tables) {
				if (dt.ParentRelations.Count > 0)
					continue; // don't add them here
				FillNodeRows (docElem, dt, dt.Rows);
			}

			// This seems required to avoid Load() error when for
			// example empty DataSet will be filled on Load().
			if (docElem.ChildNodes.Count > 0)
				AppendChild (docElem);

			foreach (DataTable dt in dataSet.Tables) {
				dt.ColumnChanged += columnChanged;
				dt.RowDeleted += rowDeleted;
				dt.RowChanged += rowChanged;
			}

			AddXmlDocumentListeners ();
		}

		// bool clone. If we are cloning XmlDataDocument then clone should be true.
		// FIXME: shouldn't DataSet be mapped to at most one document??
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

		private void FillNodeRows (XmlElement parent, DataTable dt, ICollection rows)
		{
			foreach (DataRow dr in dt.Rows) {
				XmlDataElement el = new XmlDataElement (dr, dt.Prefix, dt.TableName, dt.Namespace, this);
				for (int i = 0; i < dt.Columns.Count; i++) {
					DataColumn col = dt.Columns [i];
					string value = dr.IsNull (col) ? String.Empty : dr [col].ToString ();
					switch (col.ColumnMapping) {
					case MappingType.Element:
						XmlElement cel = CreateElement (col.Prefix, col.ColumnName, col.Namespace);
						cel.InnerText = value;
						el.AppendChild (cel);
						break;
					case MappingType.Attribute:
						XmlAttribute a = CreateAttribute (col.Prefix, col.ColumnName, col.Namespace);
						a.Value = value;
						el.SetAttributeNode (a);
						break;
					case MappingType.SimpleContent:
						XmlText t = CreateTextNode (value);
						el.AppendChild (t);
						break;
					}
				}
				foreach (DataRelation rel in dt.ChildRelations)
					FillNodeRows (el, rel.ChildTable, dr.GetChildRows (rel));
				parent.AppendChild (el);
			}
		}

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
			DataTable dt = DataSet.Tables [localName];
			DataRow row = dt != null ? dt.NewRow () : null;
			if (row != null)
				return GetElementFromRow (row);
			else
				return base.CreateElement (prefix, localName, namespaceURI);
		}

		#endregion // overloaded CreateElement Methods
			
		// It is not supported in XmlDataDocument
		public override XmlEntityReference CreateEntityReference(string name) 
		{
			throw new NotSupportedException ();
		}
		
		// It is not supported in XmlDataDocument
		public override XmlElement GetElementById (string elemId) 
		{
			throw new NotSupportedException ();
		}

		// get the XmlElement associated with the DataRow
		public XmlElement GetElementFromRow (DataRow r) 
		{
			return r.DataElement;
		}

		// get the DataRow associated with the XmlElement
		public DataRow GetRowFromElement (XmlElement e)
		{
			XmlDataElement el = e as XmlDataElement;
			if (el == null)
				return null;
			return el.DataRow;
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

			base.Load (reader);

			DataSet.EnforceConstraints = OldEC;
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
			if (!this.raiseDocumentEvents)
				return;
			if (DataSet.EnforceConstraints) 
				throw new InvalidOperationException (Locale.GetText ("Please set DataSet.EnforceConstraints == false before trying to edit XmlDataDocument using XML operations."));
		}

		// Invoked when XmlNode is changed colum is changed
		[MonoTODO]
		private void OnNodeChanged (object sender, XmlNodeChangedEventArgs args)
		{
			if (!raiseDocumentEvents)
				return;
			bool escapedRaiseDataSetEvents = raiseDataSetEvents;
			raiseDataSetEvents = false;
			try {

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
			} finally {
				raiseDataSetEvents = escapedRaiseDataSetEvents;
			}
		}

		private void OnNodeRemoving (object sender, XmlNodeChangedEventArgs args) 
		{
			if (!this.raiseDocumentEvents)
				return;
			if (DataSet.EnforceConstraints) 
				throw new InvalidOperationException (Locale.GetText ("Please set DataSet.EnforceConstraints == false before trying to edit XmlDataDocument using XML operations."));
			
		}
		
		// Invoked when XmlNode is removed
		[MonoTODO]
		private void OnNodeRemoved (object sender, XmlNodeChangedEventArgs args)
		{
			if (!raiseDocumentEvents)
				return;
			bool escapedRaiseDataSetEvents = raiseDataSetEvents;
			raiseDataSetEvents = false;

			try {
				// FIXME: This code is obsolete one.

				if (args.OldParent == null)
					return;

				if (!(args.OldParent is XmlElement))
					return;
				
				DataRow row = GetRowFromElement ((XmlElement)args.OldParent);
				
				if (row == null)
					return ;

				row [args.Node.Name] = null;

				// FIXME: Should we detach rows and descendants as well?
			} finally {
				raiseDataSetEvents = escapedRaiseDataSetEvents;
			}
		}

		private void OnNodeInserting (object sender, XmlNodeChangedEventArgs args) 
		{
			if (!this.raiseDocumentEvents)
				return;
			if (DataSet.EnforceConstraints) 
				throw new InvalidOperationException (Locale.GetText ("Please set DataSet.EnforceConstraints == false before trying to edit XmlDataDocument using XML operations."));
			
		}
		
		private void OnNodeInserted (object sender, XmlNodeChangedEventArgs args)
		{
			if (!raiseDocumentEvents)
				return;
			bool escapedRaiseDataSetEvents = raiseDataSetEvents;
			raiseDataSetEvents = false;

			// If the parent node is mapped to a DataTable, then
			// add a DataRow and map the parent element to it.
			//
			// AND If the child node is mapped to a DataTable, then
			// 1. if it is mapped to a DataTable and relation, add
			// a new DataRow and map the child element to it.
			// 2. if it is mapped to a DataColumn, set the column
			// value of the parent DataRow as the child

			try {
				if (! (args.NewParent is XmlElement)) {
					// i.e. adding document element
					foreach (XmlNode table in args.Node.ChildNodes)
						CheckDescendantRelationship (table);
					return;
				}

				DataRow row = GetRowFromElement (args.NewParent as XmlElement);
				if (row == null) {
					// That happens only when adding table to existing DocumentElement (aka DataSet element)
					if (args.NewParent == DocumentElement)
						CheckDescendantRelationship (args.Node);
					return;
				}

				XmlAttribute attr = args.Node as XmlAttribute;
				if (attr != null) { // fill attribute value
					DataColumn col = row.Table.Columns [attr.LocalName];
					if (col != null)
						row [col] = args.Node.Value;
				} else {
					DataRow childRow = GetRowFromElement (args.Node as XmlElement);
					if (childRow != null) {
						// child might be a table row.
						// I might be impossible to set parent
						// since either of them might be detached
						if (childRow.RowState != DataRowState.Detached && row.RowState != DataRowState.Detached) {
							FillRelationship (row, childRow, args.NewParent, args.Node);
						}
					} else {
						// child might be a column
						DataColumn col = row.Table.Columns [args.Node.LocalName];
						if (col != null)
							row [col] = args.Node.InnerText;
					}
				}
			} finally {
				raiseDataSetEvents = escapedRaiseDataSetEvents;
			}
		}

		private void CheckDescendantRelationship (XmlNode n)
		{
			XmlElement el = n as XmlElement;
			DataRow row = GetRowFromElement (el);
			if (row == null)
				return;
			row.Table.Rows.Add (row); // attach
			CheckDescendantRelationship (n, row);
		}

		private void CheckDescendantRelationship (XmlNode p, DataRow row)
		{
			foreach (XmlNode n in p.ChildNodes) {
				XmlElement el = n as XmlElement;
				if (el == null)
					continue;
				DataRow childRow = GetRowFromElement (el);
				if (childRow == null)
					continue;
				childRow.Table.Rows.Add (childRow);
				FillRelationship (row, childRow, p, el);
			}
		}

		private void FillRelationship (DataRow row, DataRow childRow, XmlNode parentNode, XmlNode childNode)
		{
			for (int i = 0; i < childRow.Table.ParentRelations.Count; i++) {
				DataRelation rel = childRow.Table.ParentRelations [i];
				if (rel.ParentTable == row.Table) {
					childRow.SetParentRow (row);
					break;
				}
			}
			CheckDescendantRelationship (childNode, childRow);
		}
		#endregion // DataSet event handlers

		#region DataSet event handlers

		// If DataTable is added or removed from DataSet
		private void OnDataTableChanged (object sender, CollectionChangeEventArgs eventArgs)
		{
			if (!raiseDataSetEvents)
				return;
			bool escapedRaiseDocumentEvents = raiseDocumentEvents;
			raiseDocumentEvents = false;

			try {
				DataTable Table = (DataTable)eventArgs.Element;
				switch (eventArgs.Action) {
				case CollectionChangeAction.Add:
					Table.ColumnChanged += columnChanged;
					Table.RowDeleted += rowDeleted;
					Table.RowChanged += rowChanged;
					break;
				case CollectionChangeAction.Remove:
					Table.ColumnChanged -= columnChanged;
					Table.RowDeleted -= rowDeleted;
					Table.RowChanged -= rowChanged;
					break;
				}
			} finally {
				raiseDocumentEvents = escapedRaiseDocumentEvents;
			}
		}

		// If column has changed 
		[MonoTODO]			
		private void OnDataTableColumnChanged(object sender, 
							     DataColumnChangeEventArgs eventArgs)
		{
			if (!raiseDataSetEvents)
				return;
			bool escapedRaiseDocumentEvents = raiseDocumentEvents;
			raiseDocumentEvents = false;

			try {
				DataRow row = eventArgs.Row;
				XmlElement el = GetElementFromRow (row);
				if (el == null)
					return;
				DataColumn col = eventArgs.Column;
				string value = row.IsNull (col) ? String.Empty : row [col].ToString ();
				switch (col.ColumnMapping) {
				case MappingType.Attribute:
					el.SetAttribute (col.ColumnName, col.Namespace, value);
					break;
				case MappingType.SimpleContent:
					el.InnerText = value;
					break;
				case MappingType.Element:
					bool exists = false;
					for (int i = 0; i < el.ChildNodes.Count; i++) {
						XmlElement c = el.ChildNodes [i] as XmlElement;
						if (c != null && c.LocalName == col.ColumnName && c.NamespaceURI == col.Namespace) {
							exists = true;
							c.InnerText = value;
							break;
						}
					}
					if (!exists) {
						XmlElement cel = CreateElement (col.Prefix, col.ColumnName, col.Namespace);
						cel.InnerText = value;
						el.AppendChild (cel);
					}
					break;
				// FIXME: how to handle hidden?
				}
			} finally {
				raiseDocumentEvents = escapedRaiseDocumentEvents;
			}
		}
	
		[MonoTODO]
		private void OnDataTableRowDeleted(object sender,
							  DataRowChangeEventArgs eventArgs)
		{
			if (!raiseDataSetEvents)
				return;
			bool escapedRaiseDocumentEvents = raiseDocumentEvents;
			raiseDocumentEvents = false;

			try {
				// This code is obsolete XmlDataDocument one

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
			} finally {
				raiseDocumentEvents = escapedRaiseDocumentEvents;
			}
		}
		
		[MonoTODO]
		private void OnDataTableRowChanged(object sender, DataRowChangeEventArgs eventArgs)
		{
			if (!raiseDataSetEvents)
				return;
			bool escapedRaiseDocumentEvents = raiseDocumentEvents;
			raiseDocumentEvents = false;
			try {

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
			} finally {
				raiseDocumentEvents = escapedRaiseDocumentEvents;
			}
		}

		// Added
		[MonoTODO]
		private void OnDataTableRowAdded (DataRowChangeEventArgs args)
		{
			if (!raiseDataSetEvents)
				return;
			bool escapedRaiseDocumentEvents = raiseDocumentEvents;
			raiseDocumentEvents = false;

			try {

				// Create row element. Row's name same as TableName					
				DataRow row = args.Row;

				// create document element if it does not exist
				if (DocumentElement == null)
					this.AppendChild (CreateElement (DataSet.DataSetName));

				DataTable table= args.Row.Table;
				XmlElement element = GetElementFromRow (row);
				if (element == null)
					element = CreateElement (table.Prefix, table.TableName, table.Namespace);
				if (element.ParentNode == null) {
					// parent is not always DocumentElement.
					XmlElement parent = null;

					if (table.ParentRelations.Count > 0) {
						for (int i = 0; i < table.ParentRelations.Count; i++) {
							DataRelation rel = table.ParentRelations [i];
							DataRow parentRow = row.GetParentRow (rel);
							if (parentRow == null)
								continue;
							parent = GetElementFromRow (parentRow);
						}
					}

					// The row might be orphan. In such case, the 
					// element is appended to DocumentElement.
					if (parent == null)
						parent = DocumentElement;
					parent.AppendChild (element);
				}
			} finally {			
				raiseDocumentEvents = escapedRaiseDocumentEvents;
			}
		}

		private void FillNodeChildrenFromRow (DataRow row, XmlElement element)
		{
			DataTable table = row.Table;
			// fill columns for the row
			for (int i = 0; i < table.Columns.Count; i++) {
				DataColumn col = table.Columns [i];
				string value = row.IsNull (col) ? String.Empty : row [col].ToString ();
				switch (col.ColumnMapping) {
				case MappingType.Element:
					XmlElement el = CreateElement (col.Prefix, col.ColumnName, col.Namespace);
					el.InnerText = value;
					element.AppendChild (el);
					break;
				case MappingType.Attribute:
					XmlAttribute attr = CreateAttribute (col.Prefix, col.ColumnName, col.Namespace);
					attr.Value = value;
					element.SetAttributeNode (attr);
					break;
				case MappingType.SimpleContent:
					XmlText text = CreateTextNode (value);
					element.AppendChild (text);
					break;
				// FIXME: how to handle hidden?
				}
			}
		}

		// Rollback
		[MonoTODO]
		private void OnDataTableRowRollback (DataRowChangeEventArgs args)
		{
			if (!raiseDataSetEvents)
				return;
			bool escapedRaiseDocumentEvents = raiseDocumentEvents;
			raiseDocumentEvents = false;

			try {
				// This code is obsolete XmlDataDocument one.

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
			} finally {
				raiseDocumentEvents = escapedRaiseDocumentEvents;
			}
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
		
		private void RemoveXmlDocumentListeners ()
		{
			this.NodeInserting -= new XmlNodeChangedEventHandler (OnNodeInserting);
			this.NodeInserted -= new XmlNodeChangedEventHandler (OnNodeInserted);
			this.NodeChanging -= new XmlNodeChangedEventHandler (OnNodeChanging);
			this.NodeChanged -= new XmlNodeChangedEventHandler (OnNodeChanged);
			this.NodeRemoving -= new XmlNodeChangedEventHandler (OnNodeRemoving);
			this.NodeRemoved -= new XmlNodeChangedEventHandler (OnNodeRemoved);
		}

		private void AddXmlDocumentListeners ()
		{
			this.NodeInserting += new XmlNodeChangedEventHandler (OnNodeInserting);
			this.NodeInserted += new XmlNodeChangedEventHandler (OnNodeInserted);
			this.NodeChanging += new XmlNodeChangedEventHandler (OnNodeChanging);
			this.NodeChanged += new XmlNodeChangedEventHandler (OnNodeChanged);
			this.NodeRemoving += new XmlNodeChangedEventHandler (OnNodeRemoving);
			this.NodeRemoved += new XmlNodeChangedEventHandler (OnNodeRemoved);
		}
		#endregion // Private methods
	}
}

