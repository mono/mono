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
//     Atsushi Enomoto <atsushi@ximian.com>
//
// (c)copyright 2002 Ville Palo
// (C)2004 Novell Inc.
//
// XmlDataLoader is included within the Mono Class Library.
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
using System.Xml;
using System.Collections;
using System.Globalization;

namespace System.Data 
{

	internal class XmlDataLoader
	{
	
		private DataSet DSet;

		public XmlDataLoader (DataSet set) 
		{
			DSet = set;
		}

		public XmlReadMode LoadData (XmlReader reader, XmlReadMode mode)
		{
			XmlReadMode Result = mode;

			switch (mode) {
			case XmlReadMode.Auto:
				Result = DSet.Tables.Count == 0 ? XmlReadMode.InferSchema : XmlReadMode.IgnoreSchema;
				ReadModeSchema (reader, DSet.Tables.Count == 0 ? XmlReadMode.Auto : XmlReadMode.IgnoreSchema);
				break;
			case XmlReadMode.InferSchema:
				Result = XmlReadMode.InferSchema;
				ReadModeSchema (reader, mode);
				break;
			case XmlReadMode.IgnoreSchema:
				Result = XmlReadMode.IgnoreSchema;
				ReadModeSchema (reader, mode);
				break;
			default:
				reader.Skip ();
				break;
			}

			return Result;
		}

		#region reading

		// Read information from the reader.
		private void ReadModeSchema (XmlReader reader, XmlReadMode mode)
		{
			bool inferSchema = mode == XmlReadMode.InferSchema || mode == XmlReadMode.Auto;
			bool fillRows = mode != XmlReadMode.InferSchema;
			// This check is required for full DiffGram.
			// It is not described in MSDN and it is impossible
			// with WriteXml(), but when writing XML using
			// XmlSerializer, the output is like this:
			// <dataset>
			//  <schema>...</schema>
			//  <diffgram>...</diffgram>
			// </dataset>
			//
			// FIXME: This, this check should (also) be done
			// after reading the top-level element.

			//check if the current element is schema.
			if (reader.LocalName == "schema") {
				if (mode != XmlReadMode.Auto)
					reader.Skip(); // skip the schema node.
				else
					DSet.ReadXmlSchema(reader);
				
				reader.MoveToContent();
			}

			// load an XmlDocument from the reader.
			XmlDocument doc = new XmlDocument ();
			doc.Load (reader);
			if (doc.DocumentElement == null)
				return;

			// treatment for .net compliancy :
			// if xml representing dataset has exactly depth of 2 elements,
			// than the root element actually represents datatable and not dataset
			// so we add new root element to doc 
			// in order to create an element representing dataset.
			//
			// FIXME: Consider attributes. 
			// <root a='1' b='2' /> is regarded as a valid DataTable.
			int rootNodeDepth = XmlNodeElementsDepth(doc.DocumentElement);
			switch (rootNodeDepth) {
			case 1:
				if (inferSchema) {
					DSet.DataSetName = doc.DocumentElement.LocalName;
					DSet.Prefix = doc.DocumentElement.Prefix;
					DSet.Namespace = doc.DocumentElement.NamespaceURI;
				}
				return;
			case 2:
				// create new document
				XmlDocument newDoc = new XmlDocument();
				// create element for dataset
				XmlElement datasetElement = newDoc.CreateElement("dummy");
				// make the new created element to be the new doc root
				newDoc.AppendChild(datasetElement);
				// import all the elements from doc and insert them into new doc
				XmlNode root = newDoc.ImportNode(doc.DocumentElement,true);
				datasetElement.AppendChild(root);
				doc = newDoc;
				break;
			default:
				if (inferSchema) {
					DSet.DataSetName = doc.DocumentElement.LocalName;
					DSet.Prefix = doc.DocumentElement.Prefix;
					DSet.Namespace = doc.DocumentElement.NamespaceURI;
				}
				break;
			}

			// set EnforceConstraint to false - we do not want any validation during 
			// load time.
			bool origEnforceConstraint = DSet.EnforceConstraints;
			DSet.EnforceConstraints = false;

			// The childs are tables.
			XmlNodeList nList = doc.DocumentElement.ChildNodes;

			// FIXME: When reading DataTable (not DataSet), 
			// the nodes are column items, not rows.
			for (int i = 0; i < nList.Count; i++) {
				XmlNode node = nList[i];
				// node represents a table onky if it is of type XmlNodeType.Element
				if (node.NodeType == XmlNodeType.Element) {
					AddRowToTable(node, null, inferSchema, fillRows);
				}
			}
			// set the EnforceConstraints to original value;
			DSet.EnforceConstraints = origEnforceConstraint;
		}

		#endregion // reading

		#region Private helper methods
/*		
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
*/
		internal static object StringToObject (Type type, string value)
		{
			if (type == null) return value;

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
			if (type == typeof (System.Type)) return System.Type.GetType (value);

			return Convert.ChangeType (value, type);
		}

		private void AddRowToTable(XmlNode tableNode, DataColumn relationColumn, bool inferSchema, bool fillRows)
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

			// For elements that are inferred as tables and that contain text 
			// but have no child elements, a new column named "TableName_Text" 
			// is created for the text of each of the elements. 
			// If an element is inferred as a table and has text, but also has child elements,
			// the text is ignored.
			// Note : if an element is inferred as a table and has text 
			// and has no child elements, 
			// but the repeated ements of this table have child elements, 
			// then the text is ignored.
			if(!HaveChildElements(tableNode) && HaveText(tableNode) &&
				!IsRepeatedHaveChildNodes(tableNode)) {
				string columnName = tableNode.Name + "_Text";
				if (!table.Columns.Contains(columnName)) {
					table.Columns.Add(columnName);
				}
				rowValue.Add(columnName, tableNode.InnerText);
			}
			
			// Get the child nodes of the table. Any child can be one of the following tow:
			// 1. DataTable - if there was a relation with another table..
			// 2. DataColumn - column of the current table.
			XmlNodeList childList = tableNode.ChildNodes;
			for (int i = 0; i < childList.Count; i++) {
				XmlNode childNode = childList[i];

				// we are looping through elements only
				// Note : if an element is inferred as a table and has text, but also has child elements,
				// the text is ignored.
				if (childNode.NodeType != XmlNodeType.Element)
					continue;
				
				// Elements that have attributes are inferred as tables. 
				// Elements that have child elements are inferred as tables. 
				// Elements that repeat are inferred as a single table. 
				if (IsInferredAsTable(childNode)) {
					// child node inferred as table
					if (inferSchema) {
						// We need to create new column for the relation between the current
						// table and the new table we found (the child table).
						string newRelationColumnName = table.TableName + "_Id";
						if (!table.Columns.Contains(newRelationColumnName)) {
							DataColumn newRelationColumn = new DataColumn(newRelationColumnName, typeof(int));
							newRelationColumn.AllowDBNull = false;
							newRelationColumn.AutoIncrement = true;
							// we do not want to serialize this column so MappingType is Hidden.
							newRelationColumn.ColumnMapping = MappingType.Hidden;
							table.Columns.Add(newRelationColumn);
						}
						// Add a row to the new table we found.
						AddRowToTable(childNode, table.Columns[newRelationColumnName], inferSchema, fillRows);
					}
					else
						AddRowToTable(childNode, null, inferSchema, fillRows);
					
				}
				else {
					// Elements that have no attributes or child elements, and do not repeat, 
					// are inferred as columns.
					object val = null;
					if (childNode.FirstChild != null)
						val = childNode.FirstChild.Value;
					else
						val = "";
					if (table.Columns.Contains(childNode.LocalName))
						rowValue.Add(childNode.LocalName, val);
					else if (inferSchema) {
						table.Columns.Add(childNode.LocalName);
						rowValue.Add(childNode.LocalName, val);
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
					if (!table.Columns.Contains(attr.LocalName)) {
						DataColumn col = table.Columns.Add(attr.LocalName);
						col.ColumnMapping = MappingType.Attribute;
					}
					table.Columns[attr.LocalName].Namespace = table.Namespace;

					rowValue.Add(attr.LocalName, attr.Value);
				}
			}

			// If the current table is a child table we need to add a new column for the relation
			// and add a new relation to the DataSet.
			if (relationColumn != null) {
				if (!table.Columns.Contains(relationColumn.ColumnName)) {
					DataColumn dc = new DataColumn(relationColumn.ColumnName, typeof(int));
					// we do not want to serialize this column so MappingType is Hidden.
					dc.ColumnMapping = MappingType.Hidden;
					table.Columns.Add(dc);
					// Convention of relation name is: ParentTableName_ChildTableName
					DataRelation dr = new DataRelation(relationColumn.Table.TableName + "_" + dc.Table.TableName, relationColumn, dc);
					dr.Nested = true;
					DSet.Relations.Add(dr);
					UniqueConstraint.SetAsPrimaryKey (dr.ParentTable.Constraints, dr.ParentKeyConstraint);
				}
				rowValue.Add (relationColumn.ColumnName, relationColumn.GetAutoIncrementValue());
			}

			// Create new row and add all values to the row.
			// then add it to the table.
			DataRow row = table.NewRow ();
					
			IDictionaryEnumerator enumerator = rowValue.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				row [enumerator.Key.ToString ()] = StringToObject (table.Columns[enumerator.Key.ToString ()].DataType, enumerator.Value.ToString ());
			}

			if (fillRows)
				table.Rows.Add (row);
			
		}

		// this method calculates the depth of child nodes tree
		// and it counts nodes of type XmlNodeType.Element only
		private static int XmlNodeElementsDepth(XmlNode node)
		{
			int maxDepth = -1;
            if ((node != null)) {
				if  ((node.HasChildNodes) && (node.FirstChild.NodeType == XmlNodeType.Element)) {
					for (int i=0; i<node.ChildNodes.Count; i++) {
						if (node.ChildNodes[i].NodeType == XmlNodeType.Element) {
							int childDepth = XmlNodeElementsDepth(node.ChildNodes[i]);
							maxDepth = (maxDepth < childDepth) ? childDepth : maxDepth;
						}
					}
				}
				else {
					return 1;
				}
			}
			else {
				return -1;
			}

			return (maxDepth + 1);
		}

		private bool HaveChildElements(XmlNode node)
		{
			bool haveChildElements = true;
			if(node.ChildNodes.Count > 0) {
				foreach(XmlNode childNode in node.ChildNodes) {
					if (childNode.NodeType != XmlNodeType.Element) {
						haveChildElements = false;
						break;
					}
				}
			}
			else {
				haveChildElements = false;
			}
			return haveChildElements;
		}

		private bool HaveText(XmlNode node)
		{
			bool haveText = true;
			if(node.ChildNodes.Count > 0) {
				foreach(XmlNode childNode in node.ChildNodes) {
					if (childNode.NodeType != XmlNodeType.Text) {
						haveText = false;
						break;
					}
				}
			}
			else {
				haveText = false;
			}
			return haveText;
		}

		private bool IsRepeat(XmlNode node)
		{
			bool isRepeat = false;
			if(node.ParentNode != null) {
				foreach(XmlNode childNode in node.ParentNode.ChildNodes) {
					if(childNode != node && childNode.Name == node.Name) {
						isRepeat = true;
						break;
					}
				}
			}
			return isRepeat;
		}

		private bool HaveAttributes(XmlNode node)
		{
			return (node.Attributes != null && node.Attributes.Count > 0);
		}

		private bool IsInferredAsTable(XmlNode node)
		{
			// Elements that have attributes are inferred as tables. 
			// Elements that have child elements are inferred as tables. 
			// Elements that repeat are inferred as a single table. 
			return (HaveChildElements(node) || HaveAttributes(node) ||
					IsRepeat(node));
		}

		/// <summary>
		/// Returns true is any node that is repeated node for the node supplied
		/// (i.e. is child node of node's parent, have the same name and is not the node itself)
		/// have child elements
		/// </summary>
		private bool IsRepeatedHaveChildNodes(XmlNode node)
		{
			bool isRepeatedHaveChildElements = false;
			if(node.ParentNode != null) {
				foreach(XmlNode childNode in node.ParentNode.ChildNodes) {
					if(childNode != node && childNode.Name == node.Name) {
						if (HaveChildElements(childNode)) {
							isRepeatedHaveChildElements = true;
							break;
						}
					}
				}
			}
			return isRepeatedHaveChildElements;
		}

		#endregion // Private helper methods

		
	}

}
