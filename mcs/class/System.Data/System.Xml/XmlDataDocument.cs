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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

			XmlElement docElem = CreateElement (dataSet.Prefix, XmlHelper.Encode (dataSet.DataSetName), dataSet.Namespace);
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
				XmlDataElement el = dr.DataElement;
				FillNodeChildrenFromRow (dr, el);

				foreach (DataRelation rel in dt.ChildRelations)
					FillNodeRows (el, rel.ChildTable, dr.GetChildRows (rel));
				parent.AppendChild (el);
			}
		}

		public override XmlNode CloneNode (bool deep) 
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
			DataTable dt = DataSet.Tables [XmlHelper.Decode (localName)];
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

				if (row [args.Node.ParentNode.Name].ToString () != args.Node.InnerText) {
					DataColumn col = row.Table.Columns [args.Node.ParentNode.Name];
					row [col] = StringToObject (col.DataType, args.Node.InnerText);
				}

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
		private void OnNodeRemoved (object sender, XmlNodeChangedEventArgs args)
		{
			if (!raiseDocumentEvents)
				return;
			bool escapedRaiseDataSetEvents = raiseDataSetEvents;
			raiseDataSetEvents = false;

			try {
				if (args.OldParent == null)
					return;

				XmlElement oldParentElem = args.OldParent as XmlElement;
				if (oldParentElem == null)
					return;
				
				// detach child row (if exists)
				XmlElement childElem = args.Node as XmlElement;
				if (childElem != null) {
					DataRow childRow = GetRowFromElement (childElem);
					if (childRow != null)
						childRow.Table.Rows.Remove (childRow);
				}

				DataRow row = GetRowFromElement (oldParentElem);
				
				if (row == null)
					return ;

				row [args.Node.Name] = null;

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
					DataColumn col = row.Table.Columns [XmlHelper.Decode (attr.LocalName)];
					if (col != null)
						row [col] = StringToObject (col.DataType, args.Node.Value);
				} else {
					DataRow childRow = GetRowFromElement (args.Node as XmlElement);
					if (childRow != null) {
						// child might be a table row.
						// I might be impossible to set parent
						// since either of them might be detached
						if (childRow.RowState != DataRowState.Detached && row.RowState != DataRowState.Detached) {
							FillRelationship (row, childRow, args.NewParent, args.Node);
						}
					} else if (args.Node.NodeType == XmlNodeType.Element) {
						// child element might be a column
						DataColumn col = row.Table.Columns [XmlHelper.Decode (args.Node.LocalName)];
						if (col != null)
							row [col] = StringToObject (col.DataType, args.Node.InnerText);
					} else if (args.Node is XmlCharacterData) {
						if (args.Node.NodeType != XmlNodeType.Comment) {
							for (int i = 0; i < row.Table.Columns.Count; i++) {
								DataColumn col = row.Table.Columns [i];
								if (col.ColumnMapping == MappingType.SimpleContent)
									row [col] = StringToObject (col.DataType, args.Node.Value);
							}
						}
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
		private void OnDataTableColumnChanged (object sender, 
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
					el.SetAttribute (XmlHelper.Encode (col.ColumnName), col.Namespace, value);
					break;
				case MappingType.SimpleContent:
					el.InnerText = value;
					break;
				case MappingType.Element:
					bool exists = false;
					for (int i = 0; i < el.ChildNodes.Count; i++) {
						XmlElement c = el.ChildNodes [i] as XmlElement;
						if (c != null && c.LocalName == XmlHelper.Encode (col.ColumnName) && c.NamespaceURI == col.Namespace) {
							exists = true;
							c.InnerText = value;
							break;
						}
					}
					if (!exists) {
						XmlElement cel = CreateElement (col.Prefix, XmlHelper.Encode (col.ColumnName), col.Namespace); 
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
	
		private void OnDataTableRowDeleted (object sender,
							  DataRowChangeEventArgs eventArgs)
		{
			if (!raiseDataSetEvents)
				return;
			bool escapedRaiseDocumentEvents = raiseDocumentEvents;
			raiseDocumentEvents = false;

			try {
				// This code is obsolete XmlDataDocument one

				XmlElement el = GetElementFromRow (eventArgs.Row);
				if (el == null)
					return;

				el.ParentNode.RemoveChild (el);
			} finally {
				raiseDocumentEvents = escapedRaiseDocumentEvents;
			}
		}
		
		[MonoTODO ("Need to handle hidden columns? - see comments on each private method")]
		private void OnDataTableRowChanged (object sender, DataRowChangeEventArgs eventArgs)
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

		// Added - see FillNodeChildrenFromRow comment
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
					this.AppendChild (CreateElement (XmlHelper.Encode (DataSet.DataSetName)));

				DataTable table= args.Row.Table;
				XmlElement element = GetElementFromRow (row);
				if (element == null)
					element = CreateElement (table.Prefix, XmlHelper.Encode (table.TableName), table.Namespace);

				if (element.ChildNodes.Count == 0)
					FillNodeChildrenFromRow (row, element);

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
					XmlElement el = CreateElement (col.Prefix, XmlHelper.Encode (col.ColumnName), col.Namespace);
					el.InnerText = value;
					element.AppendChild (el);
					break;
				case MappingType.Attribute:
					XmlAttribute attr = CreateAttribute (col.Prefix, XmlHelper.Encode (col.ColumnName), col.Namespace);
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
		[MonoTODO ("It does not look complete.")]
		private void OnDataTableRowRollback (DataRowChangeEventArgs args)
		{
			if (!raiseDataSetEvents)
				return;
			bool escapedRaiseDocumentEvents = raiseDocumentEvents;
			raiseDocumentEvents = false;

			try {
				DataRow r = args.Row;
				XmlElement el = GetElementFromRow (r);
				if (el == null)
					return;
				DataTable tab = r.Table;
				ArrayList al = new ArrayList ();
				foreach (XmlAttribute attr in el.Attributes) {
					DataColumn col = tab.Columns [XmlHelper.Decode (attr.LocalName)];
					if (col != null) {
						if (r.IsNull (col))
							// should be removed
							al.Add (attr);
						else
							attr.Value = r [col].ToString ();
					}
				}
				foreach (XmlAttribute attr in al)
					el.RemoveAttributeNode (attr);
				al.Clear ();
				foreach (XmlNode child in el.ChildNodes) {
					if (child.NodeType == XmlNodeType.Element) {
						DataColumn col = tab.Columns [XmlHelper.Decode (child.LocalName)];
						if (col != null) {
							if (r.IsNull (col))
								al.Add (child);
							else
								child.InnerText = r [col].ToString ();
						}
					}
				}
				foreach (XmlNode n in al)
					el.RemoveChild (n);
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

		internal static object StringToObject (Type type, string value)
		{
			if (value == null || value == String.Empty)
				return DBNull.Value;

			switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean: return XmlConvert.ToBoolean (value);
				case TypeCode.Byte: return XmlConvert.ToByte (value);
				case TypeCode.Char: return (char)XmlConvert.ToInt32 (value);
#if NET_2_0
				case TypeCode.DateTime: return XmlConvert.ToDateTime (value, XmlDateTimeSerializationMode.Unspecified);
#else
				case TypeCode.DateTime: return XmlConvert.ToDateTime (value);
#endif
				case TypeCode.Decimal: return XmlConvert.ToDecimal (value);
				case TypeCode.Double: return XmlConvert.ToDouble (value);
				case TypeCode.Int16: return XmlConvert.ToInt16 (value);
				case TypeCode.Int32: return XmlConvert.ToInt32 (value);
				case TypeCode.Int64: return XmlConvert.ToInt64 (value);
				case TypeCode.SByte: return XmlConvert.ToSByte (value);
				case TypeCode.Single: return XmlConvert.ToSingle (value);
				case TypeCode.UInt16: return XmlConvert.ToUInt16 (value);
				case TypeCode.UInt32: return XmlConvert.ToUInt32 (value);
				case TypeCode.UInt64: return XmlConvert.ToUInt64 (value);
			}

			if (type == typeof (TimeSpan)) return XmlConvert.ToTimeSpan (value);
			if (type == typeof (Guid)) return XmlConvert.ToGuid (value);
			if (type == typeof (byte[])) return Convert.FromBase64String (value);

			return Convert.ChangeType (value, type);
		}
		#endregion // Private methods
	}
}

