//
// XmlDataInferenceLoader.cs
//
// Author:
//
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
//
// * Design Notes
//
//	This class is used to implement DataSet's ReadXml() and 
//	InferXmlSchema() methods. That is, 1) to infer dataset schema 
//	structure and 2) to read data rows.
//
//	It is instantiated from DataSet, XmlReader and XmlReadMode.
//
//
// ** General Design
//
// *** Read mode
//
//	Data rows are not read when XmlReadMode is ReadSchema or InferSchema
//	(well, actually ReadSchema should not be passed).
//
//	Schema inference is done only when XmlReadMode is Auto or InferSchema.
//
// *** Information Set
//
//	Only elements, "attributes", and "text" are considered. Here "text" 
//	includes text node and CDATA section, but does not include whitespace
//	and even significant whitespace (as MS does). Also, here "attributes"
//	does not include "namespace" nodes.
//
//
// ** Inference Design
//
// *** MappingType
//
//	There are four type of mapping in MappingType enumeration:
//
//	Element : Mapped to a simple type element.
//	Attribute : Mapped to an attribute
//	SimpleContent : Mapped to the text content of complex type element.
//			(i.e. xs:element/xs:complexType/xs:simpleContent)
//	Hidden : Used for both key and reference, for auto-generated columns.
//
// *** Mapping strategy
//
//	Attributes are always (except for namespace nodes) infered as 
//	DataColumn (of MappingType.Attribute).
//
//	When an element has attributes, it always becomes a DataTable.
//	Otherwise if there is a child element, it becomes DataTable as well.
//
//	When there is a text content, 1) if the container element has 
//	attribute(s), the text content becomes a SimpleContent DataColumn
//	in the container "table" element (yes, it becomes a DataTable).
//	2) if the container has no attribute, the element becomes DataColumn.
//
//	If there are both text content and child element(s), the text content
//	is ignored (thus, it always become DataTable).
//
// *** Mapping conflicts
//
//	If there has been already a different MappingType of DataColumn,
//	it is DataException. For example, it is an error if there are an
//	attribute and an element child those names are the same.
//
//	Name remapping will never be done. It introduces complicated rules.
//
// *** Upgrading from DataColumn to DataTable
//
//	If there has been the same Element type of mapping (that is, when
//	the same-name child elements appeared in the element), then the
//	child elements become a DataTable (so here must be a conversion
//	from DataColumn/value_DataRow to DataTable/value_DataRow in the new
//	table and reference_to_new_table in the old DataColumn.
//
// ** Ordinal Columns
//
//	Under MS.NET, columns seems to be sorted in certain order.
//
//		* Element content comes earlier than attributes, even those attributes
//		  appeared in front of elements.
//		* Simple type element child and complex type element (i.e. turns
//		  out to be foreign key column) are in the appearance order.
//
//	This means, attribute columns are added after all contents are
//	iterated. Since values can be filled only after adding columns,
//	XML document need to be stored *before finishing schema inference*.
//	(otherwise we will have to maintain "only-attributes-row-and-attributes"
//	map in our inference engine). That is nothing more than wasting the
//	resources.
//
//	This MUST happen only when it is "both infering structures and storing data",
//	but is LIKELY to happen all other processing.
//
// ** Implementation
//
// *** XmlReader based implementation
//
//	This class uses XmlReader to avoid having the entire XML document
//	object. The basic stategy is
//
//		1) handle attributes at startElement
//		2) store text content (if it "stores" in data rows) while
//		   EndElement
//		3) dispose of elements at endElement
//		4) Empty element without attributes is equal to a column 
//		   that holds "".
//
//	In XmlSchemaMapper.cs (by Ville Palo) there is an enumeration type
//	ElementType (undefined, table, column). This concept is nice to reuse.
//
// *** Top level inference
//
//	The process starts with ReadElement() for the top-level element.
//	(considering Fragment mode, it might not be the document element.
//	However, no inference is done in that mode.)
//
//	If the top level element was not a DataTable and there is
//	no more content, the element is regarded as DataSet with no tables.
//
// *** First child of the DataSet element
//
//	There are some special cases.
//
// *** ReadElement()
//
//	The main inference process is ReadElement(). This method consumes
//	(should consume) exactly one element and interpret it as either
//	DataTable or DataColumn.
//

using System;
using System.Collections;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

namespace System.Data
{

	internal enum ReadElementResult
	{
		DataTable,
		DataColumn,
		Undetermined
	}

	internal class XmlDataInferenceLoader
	{
		public static XmlReadMode Infer (DataSet dataset, XmlReader reader, XmlReadMode mode, bool readData)
		{
			return new XmlDataInferenceLoader (dataset, reader, mode, readData).ReadXml ();
		}

		private XmlDataInferenceLoader (DataSet ds, XmlReader xr, XmlReadMode mode, bool readData)
		{
			dataset = ds;
			reader = xr;
			this.mode = mode;
			this.readData = readData;
			switch (mode) {
			case XmlReadMode.Auto:
				inferSchema = dataset.Tables.Count == 0;
				break;
			case XmlReadMode.InferSchema:
				inferSchema = true;
				break;
			}
		}

		DataSet dataset;
		XmlReader reader;
		XmlReadMode mode;
		bool readData;
		bool inferSchema;
		ArrayList relations = new ArrayList ();

		private bool ReadData {
			get { return readData; }
		}

		private bool InferSchema {
			get { return inferSchema; }
		}

		private XmlReadMode ReadXml ()
		{
			reader.MoveToContent ();
			// If the root element is not a data table, treat 
			// this element as DataSet.
			if (mode == XmlReadMode.Fragment) {
				// Read till the end of the reader
				while (!reader.EOF) {
					switch (reader.NodeType) {
					case XmlNodeType.Element:
						ReadElement (null);
						break;
					case XmlNodeType.EndElement:
						reader.ReadEndElement ();
						break;
					default:
						reader.Read ();
						break;
					}
					if (reader.IsEmptyElement)
						reader.Read ();
					reader.MoveToContent ();
				}
			} else {
				// Read one element. It might be DataSet element.
				string topprefix = reader.Prefix;
				string topname = reader.LocalName;
				string topns = reader.NamespaceURI;

				object o = ReadElement (null);
				DataTable top = o as DataTable;
				bool isDataSet = true;
				// If only one table, don't remove it
				if (top == null)
					isDataSet = true;
				else if (dataset.Tables.Count < 2)
					isDataSet = false;
				else if (top != null) {
					foreach (DataColumn col in top.Columns) {
						switch (col.ColumnMapping) {
						case MappingType.Attribute:
						case MappingType.SimpleContent:
							isDataSet = false;
							break;
						default:
							continue;
						}
						break;
					}
				}

				if (isDataSet) {
					if (top != null) {
						ArrayList removeList = new ArrayList ();
						foreach (DataRelation rel in relations) {
							if (rel.ParentTable != top && rel.ChildTable != top)
								continue;
							removeList.Add (rel);
							foreach (DataColumn cc in rel.ChildColumns)
								rel.ChildTable.Columns.Remove (cc);
						}
						foreach (DataRelation rel in removeList)
							relations.Remove (rel);

						dataset.Tables.Remove (top);
					}
					dataset.DataSetName = topname;
					dataset.Namespace = topns;
					dataset.Prefix = topprefix;
				}
				foreach (DataRelation rel in relations) {
					rel.ParentTable.PrimaryKey = rel.ParentColumns;
					dataset.Relations.Add (rel);
				}

				if (reader.IsEmptyElement)
					reader.Read ();
			}
			// ReadElement does not always end as EndElement. 
			// Consider an empty element
			if (reader.NodeType == XmlNodeType.EndElement)
				reader.ReadEndElement ();
			switch (mode) {
			case XmlReadMode.Fragment:
				return XmlReadMode.Fragment;
			case XmlReadMode.Auto:
				return InferSchema ? XmlReadMode.InferSchema : XmlReadMode.IgnoreSchema;
			default:
				return mode;
			}
		}

		// Return values are:
		//	DataColumn
		//	DataTable
		private object ReadElement (DataRow currentRow)
		{
			DataTable table = null;

			bool uniqueKeyAlreadyExist = false;
			object ret = null;
			string elementName = reader.LocalName;
			string textContent = null;
			bool hasChildElements = false;
			bool hasAttributes = reader.HasAttributes;

			// FIXME: Just skipping the element would be much 
			// better if no mapped table was found and no suitable
			// table to fill data.

//Console.WriteLine ("**** Start Element " + elementName);

			if (reader.MoveToFirstAttribute ()) {
				do {
					if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
						continue;
					if (table == null) {
						table = GetMappedTable (elementName);
						currentRow = table.NewRow ();
						if (ReadData)
							table.Rows.Add (currentRow);
					}
					ret = table;
					ReadAttribute (table, currentRow);
				} while (reader.MoveToNextAttribute ());
				reader.MoveToElement ();
			}

			if (reader.AttributeCount == 0 && reader.IsEmptyElement) {
				if (currentRow == null)
					return null;
				// no need to fill row ... it is empty
				return GetMappedColumn (currentRow.Table, reader.LocalName, reader.Prefix, reader.NamespaceURI, MappingType.Element);
			}

			if (reader.IsEmptyElement) {
				if (currentRow != null)
					reader.Read ();
			} else {
				// read content
				reader.Read ();
				reader.MoveToContent ();

				while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement) {
					switch (reader.NodeType) {
					case XmlNodeType.Element: // child

						if (table == null) {
							table = GetMappedTable (elementName);
							currentRow = table.NewRow ();
							if (ReadData)
								table.Rows.Add (currentRow);
						}
						hasChildElements = true;

						object o = ReadElement (currentRow);
						DataTable childTable = o as DataTable;
						if (childTable != null) {
							// Add thisTable_Id column if not exist.
//							if (uniqueKeyAlreadyExist)
//								break;
							// Add unique key to "current" table.
//							uniqueKeyAlreadyExist = true;
							DataColumn col = GetMappedColumn (table, table.TableName + "_Id", String.Empty, String.Empty, MappingType.Hidden);
							if (col != null) {
								// Remove SimpleContent column if exists
								foreach (DataColumn c in table.Columns)
									if (c.ColumnMapping == MappingType.SimpleContent)
										table.Columns.Remove (c);
								col.Unique = true;
								col.AllowDBNull = false;

								// Add foreign key column and DataRelation
								bool exists = false;
								foreach (DataColumn cc in childTable.Columns) {
									if (cc.ColumnName == table.TableName + "_Id" && cc.ColumnMapping == MappingType.Hidden) {
										exists = true;
										break;
									}
								}
								DataColumn childColumn = GetMappedColumn (childTable, table.TableName + "_Id", String.Empty, String.Empty, MappingType.Hidden);
								childColumn.AutoIncrement = false;
								if (!exists) {
									DataRelation rel = new DataRelation (table.TableName + "_" + childTable.TableName, col, childColumn);
									rel.Nested = true;
									relations.Add (rel);
								}
								if (ReadData) {
Console.WriteLine ("******* Filling refkey column: " + currentRow [col] + " to " + childTable.Rows [childTable.Rows.Count - 1] [childColumn]);
									childTable.Rows [childTable.Rows.Count - 1] [childColumn] = currentRow [col];
								}
							}
						}// else {
							// Simple content element column.
							// That should be already added
							// to the current table.
							// (cannot be done here since text contents cannot be acquired here).
							// FIXME: If the name has already appeared, then the element must be DataTable (oneOrMore).
						//}

						if (!reader.IsEmptyElement)
							reader.ReadEndElement ();
						else
							reader.Read ();
						reader.MoveToContent ();
						ret = table;
						break;
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
						// skip mixed content.
						if (!hasChildElements) {
							if (ReadData)
								textContent += reader.Value;
							else
								textContent = String.Empty;
						}
						reader.Read ();
						reader.MoveToContent ();
						break;
					case XmlNodeType.EndElement:
						break; // end
					default:
						reader.Read ();
						break;
					}
				}
			}

			if (currentRow != null && !hasChildElements) {
				DataTable currentTable = currentRow.Table;
				// Attributes + !Children + Text = SimpleContent
				if (textContent != null && hasAttributes) {
					DataColumn col = GetMappedColumn (currentTable, currentTable.TableName + "_Text", String.Empty, String.Empty, MappingType.SimpleContent);
					if (ReadData && col != null) // read, only when the column exists
						currentRow [col] = StringToObject (col.DataType, textContent);
				} else if (!hasAttributes) {
					if (textContent == null)
						textContent = "";
					// Otherwise, simple element column
					DataColumn col = GetMappedColumn (currentTable, reader.LocalName,
						reader.Prefix,
						reader.NamespaceURI,
						MappingType.Element);
					if (ReadData && col != null && col.ColumnMapping != MappingType.Hidden)
						currentRow [col] = StringToObject (col.DataType, textContent);
					ret = ReadElementResult.DataColumn;
				}
			}

			return ret;
		}

		private DataTable GetMappedTable (string tableName)
		{
			DataTable dt = dataset.Tables [tableName];
			if (dt == null) {
//Console.WriteLine ("****** Creating New DataTable: " + tableName);
				dt = new DataTable (tableName);
				dataset.Tables.Add (dt);
			}
			return dt;
		}

		private void ReadAttribute (DataTable table, DataRow currentRow)
		{
			DataColumn col = GetMappedColumn (table,
				reader.LocalName,
				reader.Prefix,
				reader.NamespaceURI,
				MappingType.Attribute);

			// Read data
			if (ReadData && col != null)
				currentRow [col] = StringToObject (col.DataType, reader.Value);
		}

		// FIXME: When arg is primary key column, it should be added
		// on the top of the column list, so Columns.Add() should not 
		// be done here.
		//
		// However, we need to fill data to the column, which requires
		// DataRow (and subsequently DataTable), so inference and
		// data fill will have to be done separately.
		//
		private DataColumn GetMappedColumn (DataTable table, string name, string prefix, string ns, MappingType type)
		{
			DataColumn col = table.Columns [name];
			// Infer schema
			if (col == null) {
//Console.WriteLine ("****** New DataColumn: " + name);
				if (InferSchema) {
					col = new DataColumn ();
					col.ColumnName = name;
					col.Prefix = prefix;
					col.Namespace = ns;
					col.ColumnMapping = type;
					if (type == MappingType.Hidden) {
						col.DataType = typeof (int);
						col.AutoIncrement = true;
					}
					table.Columns.Add (col);
				}
				else
					return null; // no mappable column
			}
			// Check mapping type
			else if (col.ColumnMapping != type)
				throw new DataException (String.Format ("There are already another column that has different mapping type. Column is {0}, existing mapping type is {1}", col.ColumnName, col.ColumnMapping));

			return col;
		}

		// Copied from XmlDataLoader.cs
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
			if (type == typeof (Guid)) return XmlConvert.ToGuid (value);
			if (type == typeof (byte[])) return Convert.FromBase64String (value);

			return Convert.ChangeType (value, type);
		}
	}
}
