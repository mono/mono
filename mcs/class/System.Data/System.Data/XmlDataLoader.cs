//
// mcs/class/System.Data/System.Data/XmlDataLoader.cs
//
// Purpose: Loads XmlDocument to DataSet 
//
// class: XmlDataLoader
// assembly: System.Data.dll
// namespace: System.Data
//
// Author:
//     Ville Palo <vi64pa@koti.soon.fi>
//
// (c)copyright 2002 Ville Palo
//
// XmlDataLoader is included within the Mono Class Library.
//

using System;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Globalization;

namespace System.Data 
{

	internal class XmlDataLoader
	{
	
		private DataSet DSet;
		Hashtable DiffGrRows = new Hashtable ();

		public XmlDataLoader (DataSet set) 
		{
			DSet = set;
		}

		public XmlReadMode LoadData (XmlReader reader, XmlReadMode mode)
		{
			XmlReadMode Result = XmlReadMode.Auto;

			switch (mode) {

				case XmlReadMode.Fragment:
					break;
				case XmlReadMode.ReadSchema:
				case XmlReadMode.IgnoreSchema:
				case XmlReadMode.InferSchema:
					Result = mode;
					ReadModeSchema (reader, mode);
					break;
				default:
					break;
			}

			return Result;
		}

		#region reading

		// Read information from the reader.
		private void ReadModeSchema (XmlReader reader, XmlReadMode mode)
		{
			bool inferSchema = mode == XmlReadMode.InferSchema ? true : false;
			//check if the current element is schema.
			if (String.Compare (reader.LocalName, "schema", true) == 0) {
				
				if (mode == XmlReadMode.InferSchema || mode == XmlReadMode.IgnoreSchema)
					reader.Skip(); // skip the schema node.
				else
					DSet.ReadXmlSchema(reader);
				
				reader.MoveToContent();
			}
			// load an XmlDocument from the reader.
			XmlDocument doc = BuildXmlDocument(reader);

			// set EnforceConstraint to false - we do not want any validation during 
			// load time.
			bool origEnforceConstraint = DSet.EnforceConstraints;
			DSet.EnforceConstraints = false;

			// The childs are tables.
			XmlNodeList nList = doc.DocumentElement.ChildNodes;

			for (int i = 0; i < nList.Count; i++) {
				XmlNode node = nList[i];
				AddRowToTable(node, null, inferSchema);
			}
			// set the EnforceConstraints to original value;
			DSet.EnforceConstraints = origEnforceConstraint;
		}

		#endregion // reading

		#region Private helper methods
		
		private void ReadColumns (XmlReader reader, DataRow row, DataTable table, string TableName)
		{
			do {
				if (reader.NodeType == XmlNodeType.Element) {
					DataColumn col = table.Columns [reader.LocalName];
					if (col != null) {
						row [col] = StringToObject (col.DataType, reader.Value);
					}
					reader.Read ();
				}
				else {
					reader.Read ();
				}
				
			} while (table.TableName != reader.LocalName 
				|| reader.NodeType != XmlNodeType.EndElement);
		}

		internal static object StringToObject (Type type, string value)
		{
			if (type == null) return value;

			switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean: return XmlConvert.ToBoolean (value);
				case TypeCode.Byte: return XmlConvert.ToByte (value);
				case TypeCode.Char: return (char)XmlConvert.ToInt32 (value);
				case TypeCode.DateTime: return XmlConvert.ToDateTime (value);
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
			if (type == typeof (byte[])) return Convert.FromBase64String (value);

			return Convert.ChangeType (value, type);
		}

		private void AddRowToTable(XmlNode tableNode, DataColumn relationColumn, bool inferSchema)
		{
			Hashtable rowValue = new Hashtable();
			DataTable table;
			
			// Check if the table exists in the DataSet. If not create one.
			if (DSet.Tables.Contains(tableNode.LocalName))
				table = DSet.Tables[tableNode.LocalName];
			else if (inferSchema) {
				table = new DataTable(tableNode.LocalName);
				DSet.Tables.Add(table);
			}
			else
				return;
			
			// Get the child nodes of the table. Any child can be one of the following tow:
			// 1. DataTable - if there was a relation with another table..
			// 2. DataColumn - column of the current table.
			XmlNodeList childList = tableNode.ChildNodes;
			for (int i = 0; i < childList.Count; i++) {
				XmlNode childNode = childList[i];
				
				// The child node is a table if:
				// 1. He has attributes it means that it is a table OR
				// 2. He has more then one child nodes. Columns has only one child node
				// which is a Text node type that has the column value.
				if (childNode.ChildNodes.Count > 1 || childNode.Attributes.Count > 0) {
					
					if (inferSchema) {
						// We need to create new column for the relation between the current
						// table and the new table we found (the child table).
						string newRelationColumnName = table.TableName + "_Id";
						if (!table.Columns.Contains(newRelationColumnName)) {
							DataColumn newRelationColumn = new DataColumn(newRelationColumnName, typeof(int));
							newRelationColumn.AutoIncrement = true;
							table.Columns.Add(newRelationColumn);
						}
						// Add a row to the new table we found.
						AddRowToTable(childNode, table.Columns[newRelationColumnName], inferSchema);
					}
					else
						AddRowToTable(childNode, null, inferSchema);
					
				}
				else { //Child node is a column.
					if (table.Columns.Contains(childNode.LocalName))
						rowValue.Add(childNode.LocalName, childNode.FirstChild.Value);
					else if (inferSchema) {
						table.Columns.Add(childNode.LocalName);
						rowValue.Add(childNode.LocalName, childNode.FirstChild.Value);
					}
				}
			}

			// Column can be attribute of the table element.
			XmlAttributeCollection aCollection = tableNode.Attributes;
			for (int i = 0; i < aCollection.Count; i++) {
				XmlAttribute attr = aCollection[i];
				//the atrribute can be the namespace.
				if (attr.Prefix.Equals("xmlns"))
					table.Namespace = attr.Value;
				else { // the attribute is a column.
					if (!table.Columns.Contains(attr.LocalName))
						table.Columns.Add(attr.LocalName);
					table.Columns[attr.LocalName].Namespace = table.Namespace;

					rowValue.Add(attr.LocalName, attr.Value);
				}
			}

			// If the current table is a child table we need to add a new column for the relation
			// and add a new relation to the DataSet.
			if (relationColumn != null) {
				if (!table.Columns.Contains(relationColumn.ColumnName)) {
					DataColumn dc = new DataColumn(relationColumn.ColumnName, typeof(int));
					dc.AutoIncrement = true;
					table.Columns.Add(dc);
					DSet.Relations.Add(relationColumn, dc);
				}
			}

			// Create new row and add all values to the row.
			// then add it to the table.
			DataRow row = table.NewRow ();
					
			IDictionaryEnumerator enumerator = rowValue.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				row [enumerator.Key.ToString ()] = enumerator.Value.ToString ();
			}

			table.Rows.Add (row);
			
		}
		
		// bulid the document from the reader.
		private XmlDocument BuildXmlDocument(XmlReader reader)
		{
			XmlDocument doc = new XmlDocument();
			// Create the root element. This is the DataSet element.
			XmlElement dataSetElement = doc.CreateElement(DSet.DataSetName);
			
			do {
				XmlNode n = doc.ReadNode (reader);
				if(n == null) break;
				// Add the table nodes to the DataSet node.
				dataSetElement.AppendChild (n);
			} while (reader.IsStartElement());
			
			// Add the DataSet element to the document.
			doc.AppendChild(dataSetElement);
			return doc;
		}

		#endregion // Private helper methods

		
	}

}
