//
// XmlDataInferenceLoader.cs
//
// Author:
//
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// Design notes are the bottom of the source.
//
using System;
using System.Collections;
using System.Data;
using System.IO; // for Driver
using System.Text; // for Driver
using System.Xml;
using System.Xml.Serialization;

namespace System.Data
{

	internal enum ElementMapType
	{
		DataTable,
		DataColumn,
		Undetermined
	}

	internal class XmlDataInferenceLoader
	{
		public static XmlReadMode Infer (DataSet dataset, XmlDocument document, XmlReadMode mode, bool readData)
		{
			return new XmlDataInferenceLoader (dataset, document, mode, readData).ReadXml ();
		}

		private XmlDataInferenceLoader (DataSet ds, XmlDocument doc, XmlReadMode mode, bool readData)
		{
			dataset = ds;
			document = doc;
			reader = new XmlNodeReader (doc);
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
		XmlDocument document;
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
			if (document.DocumentElement != null)
				// no dataset infered.
				ReadTopLevelElement ();
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

		private void ReadTopLevelElement ()
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

				if (IsDocumentElementTable ())
					ReadElement (null);
				else {
					dataset.DataSetName = topname;
					dataset.Namespace = topns;
					dataset.Prefix = topprefix;
					if (reader.IsEmptyElement)
						reader.Read ();
					else {
						int depth = reader.Depth;
						reader.Read ();
						reader.MoveToContent ();
						do {
							if (reader.NodeType == XmlNodeType.Element) {
								ReadElement (null);
								if (reader.NodeType == XmlNodeType.EndElement)
									reader.ReadEndElement ();
							}
							else
								reader.Skip ();
							reader.MoveToContent ();
						} while (reader.Depth > depth);
						if (reader.NodeType == XmlNodeType.EndElement)
							reader.Read ();
					}
					reader.MoveToContent ();
				}
				
				foreach (DataRelation rel in relations) {
					rel.ParentTable.PrimaryKey = rel.ParentColumns;
					dataset.Relations.Add (rel);
				}

				if (reader.IsEmptyElement)
					reader.Read ();
			}
		}

		private bool IsDocumentElementTable ()
		{
			XmlElement top = document.DocumentElement;
			foreach (XmlAttribute attr in top.Attributes) {
				if (attr.NamespaceURI == "http://www.w3.org/2000/xmlns/")
					continue;
				// document element has attributes other than xmlns
				return true;
			}
			ArrayList tableNames = new ArrayList ();
			ArrayList columnNames = new ArrayList ();
			foreach (XmlNode n in top.ChildNodes) {
				XmlElement el = n as XmlElement;
				if (el == null)
					continue;

				bool loop = true;
				for (int i = 0; loop && i < tableNames.Count; i++)
					if (tableNames.Contains (el.LocalName))
						loop = false;
				loop = false;
				for (int i = 0; loop && i < columnNames.Count; i++) {
					if (columnNames.Contains (el.LocalName)) {
						// it turned out a table
						columnNames.Remove (el.LocalName);
						tableNames.Add (el.LocalName);
						loop = false;
					}
				}
				if (IsPossibleColumnElement (el))
					// document element has column element
					columnNames.Add (el.LocalName);
				else
					tableNames.Add (el.LocalName);
			}
			return columnNames.Count > 0;
		}

		// Returns if it "might" be a column element (this method is
		// called per child element, thus it might still consist of
		// table, since it might be repeated).
		private bool IsPossibleColumnElement (XmlElement el)
		{
			foreach (XmlAttribute attr in el.Attributes) {
				if (attr.NamespaceURI == "http://www.w3.org/2000/xmlns/")
					continue;
				return false;
			}
			foreach (XmlNode n in el.ChildNodes)
				if (n.NodeType == XmlNodeType.Element)
					return false;
			return true;
		}

		// Return values are:
		//	DataColumn
		//	DataTable
		private object ReadElement (DataRow currentRow)
		{
			DataTable table = null;

			object ret = null;
			string elementName = reader.LocalName;
			string textContent = null;
			bool hasChildElements = false;
			bool hasAttributes = reader.HasAttributes;

			// FIXME: Just skipping the element would be much 
			// better if no mapped table was found and no suitable
			// table to fill data.

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
					ret = col;
				}
			}

			return ret;
		}

		private DataTable GetMappedTable (string tableName)
		{
			DataTable dt = dataset.Tables [tableName];
			if (dt == null) {
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


#region FOR_TEST
public class Driver
{
	private static void DumpDataTable (DataTable dt)
	{
		Console.WriteLine ("<Table>");
		Console.WriteLine (dt.TableName);
		Console.WriteLine ("ChildRelationCount: " + dt.ChildRelations.Count);
		Console.WriteLine ("ConstraintsCount: " + dt.Constraints.Count);
		Console.WriteLine ("ParentRelationCount: " + dt.ParentRelations.Count);
		Console.WriteLine ("Prefix: " + dt.Prefix);
		Console.WriteLine ("Namespace: " + dt.Namespace);
		Console.WriteLine ("Site: " + dt.Site);
		Console.WriteLine ("RowCount: " + dt.Rows.Count);
		Console.WriteLine ("<Columns count='" + dt.Columns.Count + "'>");
		foreach (DataColumn col in dt.Columns)
			DumpDataColumn (col);
		Console.WriteLine ("</Columns>");
		Console.WriteLine ("</Table>");
	}

	private static void DumpDataRelation (DataRelation rel)
	{
		Console.WriteLine ("<Relation>");
		Console.WriteLine (rel.RelationName);
		Console.WriteLine (rel.Nested);
		Console.Write ("  <ParentColumns>");
		foreach (DataColumn col in rel.ParentColumns)
			Console.Write (col.ColumnName + " ");
		Console.WriteLine ("</ParentColumns>");
		Console.Write ("  <ChildColumns>");
		foreach (DataColumn col in rel.ChildColumns)
			Console.Write (col.ColumnName + " ");
		Console.WriteLine ("</ChildColumns>");
		if (rel.ParentKeyConstraint != null) {
			Console.WriteLine ("  <ParentKeyConstraint>");
			DumpUniqueConstraint (rel.ParentKeyConstraint);
			Console.WriteLine ("  </ParentKeyConstraint>");
		}
		if (rel.ChildKeyConstraint != null) {
			Console.WriteLine ("  <ChildKeyConstraint>");
			DumpForeignKeyConstraint (rel.ChildKeyConstraint);
			Console.WriteLine ("  </ChildKeyConstraint>");
		}
		Console.WriteLine ("</Relation>");
	}

	public static void DumpUniqueConstraint (UniqueConstraint uc)
	{
		Console.WriteLine ("Name " + uc.ConstraintName);
		Console.WriteLine ("PK? " + uc.IsPrimaryKey);
		Console.Write ("  <Columns>");
		foreach (DataColumn col in uc.Columns)
			Console.Write (col.ColumnName + " ");
		Console.WriteLine ("</Columns>");
	}

	public static void DumpForeignKeyConstraint (ForeignKeyConstraint fk)
	{
		Console.WriteLine ("Name " + fk.ConstraintName);
		Console.WriteLine ("  <Rules>" + fk.AcceptRejectRule + ", " +
			fk.DeleteRule + ", " + fk.UpdateRule + "</Rules>");
		Console.Write ("  <Columns>");
		foreach (DataColumn col in fk.Columns)
			Console.Write (col.ColumnName + " ");
		Console.WriteLine ("</Columns>");
		Console.Write ("  <RelatedColumns>");
		foreach (DataColumn col in fk.RelatedColumns)
			Console.Write (col.ColumnName + " ");
		Console.WriteLine ("</RelatedColumns>");
	}

	private static void DumpDataSet (DataSet ds)
	{
		Console.WriteLine ("-----------------------");
		Console.WriteLine ("name: " + ds.DataSetName);
		Console.WriteLine ("ns: " + ds.Namespace);
		Console.WriteLine ("prefix: " + ds.Prefix);
		Console.WriteLine ("extprop: " + ds.ExtendedProperties.Count);
		Console.WriteLine ("<Tables count='" + ds.Tables.Count + "'>");
		foreach (DataTable dt in ds.Tables)
			DumpDataTable (dt);
		Console.WriteLine ("</Tables>");
		Console.WriteLine ("<Relations count='" + ds.Relations.Count + "'>");
		foreach (DataRelation rel in ds.Relations)
			DumpDataRelation (rel);
		Console.WriteLine ("</Relations>");
	}

	public static void DumpDataColumn (DataColumn col)
	{
		Console.WriteLine ("<Column>");
		Console.WriteLine ("  ColumnName: " + col.ColumnName);
		Console.WriteLine ("  AllowDBNull? " + col.AllowDBNull);
		Console.WriteLine ("  AutoIncrement? " + col.AutoIncrement);
		Console.WriteLine ("    Seed: " + col.AutoIncrementSeed);
		Console.WriteLine ("    Step: " + col.AutoIncrementStep);
		Console.WriteLine ("  Caption " + col.Caption);
		Console.WriteLine ("  Mapping: " + col.ColumnMapping);
		Console.WriteLine ("  Type: " + col.DataType);
		Console.WriteLine ("  DefaultValue: " + (col.DefaultValue == DBNull.Value ? "(DBNull)" : col.DefaultValue));
		Console.WriteLine ("  Expression: " + (col.Expression == "" ? "(empty)" : col.Expression));
		Console.WriteLine ("  MaxLength: " + col.MaxLength);
		Console.WriteLine ("  Namespace: " + (col.Namespace == null ? "(null)" : col.Namespace));
		Console.WriteLine ("  Ordinal: " + col.Ordinal);
		Console.WriteLine ("  Prefix: " + (col.Prefix == null ? "(null)" : col.Prefix));
		Console.WriteLine ("  ReadOnly: " + col.ReadOnly);
		Console.WriteLine ("  Unique: " + col.Unique);
		Console.WriteLine ("</Column>");
	}

	public static void Main (string [] args)
	{
		if (args.Length < 1) {
			Console.WriteLine ("reader.exe xmlfilename");
			return;
		}
		try {
			XmlSerializer ser = new XmlSerializer (typeof (DataSet));

			DataSet ds = new DataSet ();
			XmlTextReader xtr = new XmlTextReader (args [0]);
			ds.ReadXml (xtr, XmlReadMode.Auto);
DumpDataSet (ds);
			TextWriter sw = new StringWriter ();
			ser.Serialize (sw, ds);
			using (TextWriter w = new StreamWriter (Path.ChangeExtension (args [0], "ms.txt"), false, Encoding.ASCII)) {
				w.WriteLine (sw.ToString ());
			}

			ds = new DataSet ();
			xtr = new XmlTextReader (args [0]);
			XmlDocument doc = new XmlDocument ();
			doc.Load (xtr);
			XmlDataInferenceLoader.Infer (ds, doc, XmlReadMode.Auto, false);
DumpDataSet (ds);
			sw = new StringWriter ();
sw = Console.Out;
			ser.Serialize (sw, ds);
			using (TextWriter w = new StreamWriter (Path.ChangeExtension (args [0], "my.txt"), false, Encoding.ASCII)) {
				w.WriteLine (sw.ToString ());
			}

		} catch (Exception ex) {
			Console.WriteLine (ex);
		}
	}
}

#endregion

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
