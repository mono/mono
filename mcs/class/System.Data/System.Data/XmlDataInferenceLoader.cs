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
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO; // for Driver
using System.Text; // for Driver
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data
{
	internal enum ElementMappingType {
		Simple,
		Repeated,
		Complex
	}

	internal class TableMappingCollection : CollectionBase
	{
		public void Add (TableMapping map)
		{
			this.List.Add (map);
		}

		public TableMapping this [string name] {
			get {
				foreach (TableMapping map in List)
					if (map.Table.TableName == name)
						return map;
				return null;
			}
		}
	}

	internal class TableMapping
	{
		private bool existsInDataSet;

		public DataTable Table;
		public ArrayList Elements = new ArrayList ();
		public ArrayList Attributes = new ArrayList ();
		public DataColumn SimpleContent;
		public DataColumn PrimaryKey;
		public DataColumn ReferenceKey;
#if NET_2_0
		public int lastElementIndex = -1;
#endif

		// Parent TableMapping
		public TableMapping ParentTable;

		// decoded LocalName -> TableMapping
		public TableMappingCollection ChildTables = new TableMappingCollection ();

		public TableMapping (string name, string ns)
		{
			Table = new DataTable (name);
			Table.Namespace = ns;
		}

		public TableMapping (DataTable dt)
		{
			existsInDataSet = true;
			Table = dt;
			foreach (DataColumn col in dt.Columns) {
				switch (col.ColumnMapping) {
				case MappingType.Element:
					Elements.Add (col);
					break;
				case MappingType.Attribute:
					Attributes.Add (col);
					break;
				case MappingType.SimpleContent:
					SimpleContent = col;
					break;
				}
			}
			PrimaryKey = dt.PrimaryKey.Length > 0 ? dt.PrimaryKey [0] : null;
		}

		public bool ExistsInDataSet {
			get { return existsInDataSet; }
		}

		public bool ContainsColumn (string name)
		{
			return GetColumn (name) != null;
		}

		public DataColumn GetColumn (string name)
		{
			foreach (DataColumn col in Elements)
				if (col.ColumnName == name)
					return col;
			foreach (DataColumn col in Attributes)
				if (col.ColumnName == name)
					return col;
			if (SimpleContent != null && name == SimpleContent.ColumnName)
				return SimpleContent;
			if (PrimaryKey != null && name == PrimaryKey.ColumnName)
				return PrimaryKey;
			return null;
		}

		public void RemoveElementColumn (string name)
		{
			foreach (DataColumn col in Elements) {
				if (col.ColumnName == name) {
					Elements.Remove (col);
					return;
				}
			}
		}
	}

	internal class XmlDataInferenceLoader
	{
		public static void Infer (DataSet dataset, XmlDocument document, XmlReadMode mode, string [] ignoredNamespaces)
		{
			new XmlDataInferenceLoader (dataset, document, mode, ignoredNamespaces).ReadXml ();
		}

		private XmlDataInferenceLoader (DataSet ds, XmlDocument doc, XmlReadMode mode, string [] ignoredNamespaces)
		{
			dataset = ds;
			document = doc;
			this.mode = mode;
			this.ignoredNamespaces = ignoredNamespaces != null ? new ArrayList (ignoredNamespaces) : new ArrayList ();

			// Fill existing table info
			foreach (DataTable dt in dataset.Tables)
				tables.Add (new TableMapping (dt));
		}

		DataSet dataset;
		XmlDocument document;
		XmlReadMode mode;
		ArrayList ignoredNamespaces;
		TableMappingCollection tables = new TableMappingCollection ();
		RelationStructureCollection relations = new RelationStructureCollection ();

		private void ReadXml ()
		{
			if (document.DocumentElement == null)
				return;

			dataset.Locale = new CultureInfo ("en-US"); // default(!)

			// If the root element is not a data table, treat 
			// this element as DataSet.
			// Read one element. It might be DataSet element.
			XmlElement el = document.DocumentElement;

			if (el.NamespaceURI == XmlSchema.Namespace)
				throw new InvalidOperationException ("DataSet is not designed to handle XML Schema as data content. Please use ReadXmlSchema method instead of InferXmlSchema method.");

			if (IsDocumentElementTable ())
				InferTopLevelTable (el);
			else {
				string localName = XmlHelper.Decode (el.LocalName);
				dataset.DataSetName = localName;
				dataset.Namespace = el.NamespaceURI;
				dataset.Prefix = el.Prefix;
				foreach (XmlNode n in el.ChildNodes) {
					if (n.NamespaceURI == XmlSchema.Namespace)
						continue;
					if (n.NodeType == XmlNodeType.Element)
						InferTopLevelTable (n as XmlElement);
				}
			}

			int count = 0;			
			foreach (TableMapping map in tables) {
				string baseName = map.PrimaryKey != null ? map.PrimaryKey.ColumnName : map.Table.TableName + "_Id";
				
				// Make sure name of RK column is unique
				string rkName = baseName;
				if (map.ChildTables [map.Table.TableName] != null) {
					rkName = baseName + '_' + count;
					while (map.GetColumn (rkName) != null) {
						count++;
						rkName = baseName + '_' + count;
					}
				}
				
				foreach (TableMapping ct in map.ChildTables) {
					ct.ReferenceKey = GetMappedColumn (ct, rkName, map.Table.Prefix, map.Table.Namespace, MappingType.Hidden, map.PrimaryKey != null ? map.PrimaryKey.DataType : typeof (int));
				}
			}

			foreach (TableMapping map in tables) {
				if (map.ExistsInDataSet)
					continue;
				if (map.PrimaryKey != null)
					map.Table.Columns.Add (map.PrimaryKey);

				foreach (DataColumn col in map.Elements) 
					map.Table.Columns.Add (col);

				foreach (DataColumn col in map.Attributes)
					map.Table.Columns.Add (col);
				
				if (map.SimpleContent != null) 
					map.Table.Columns.Add (map.SimpleContent);
				
				if (map.ReferenceKey != null) 
					map.Table.Columns.Add (map.ReferenceKey);
				dataset.Tables.Add (map.Table);
			}

			foreach (RelationStructure rs in relations) {
				string relName = rs.ExplicitName != null ? rs.ExplicitName : rs.ParentTableName + "_" + rs.ChildTableName;
				DataTable pt = dataset.Tables [rs.ParentTableName];
				DataTable ct = dataset.Tables [rs.ChildTableName];
				DataColumn pc = pt.Columns [rs.ParentColumnName];
				DataColumn cc = null;
				
				// If both parent and child tables have same name, it is quite
				// possible to have column names suffixed with some numbers.
				if (rs.ParentTableName == rs.ChildTableName) {
					cc = ct.Columns [rs.ChildColumnName + "_" + count];
				}
				if (cc == null)
					cc = ct.Columns [rs.ChildColumnName];
				
				if (pt == null)
					throw new DataException ("Parent table was not found : " + rs.ParentTableName);
				else if (ct == null)
					throw new DataException ("Child table was not found : " + rs.ChildTableName);
				else if (pc == null)
					throw new DataException ("Parent column was not found :" + rs.ParentColumnName);
				else if (cc == null)
					throw new DataException ("Child column was not found :" + rs.ChildColumnName);
				DataRelation rel = new DataRelation (relName, pc, cc, rs.CreateConstraint);
				if (rs.IsNested) {
					rel.Nested = true;
					rel.ParentTable.PrimaryKey = rel.ParentColumns;
				}
				dataset.Relations.Add (rel);
			}
		}

		private void InferTopLevelTable (XmlElement el)
		{
			InferTableElement (null, el);
		}

		private void InferColumnElement (TableMapping table, XmlElement el)
		{
			string localName = XmlHelper.Decode (el.LocalName);
			DataColumn col = table.GetColumn (localName);
			if (col != null) {
				if (col.ColumnMapping != MappingType.Element)
					throw new DataException (String.Format ("Column {0} is already mapped to {1}.", localName, col.ColumnMapping));
#if NET_2_0
				table.lastElementIndex = table.Elements.IndexOf (col);
#endif
				return;
			}
			if (table.ChildTables [localName] != null)
				// Child is already mapped, or inferred as a table
				// (in that case, that takes precedence than
				// this simple column inference.)
				return;

			col = new DataColumn (localName, typeof (string));
			col.Namespace = el.NamespaceURI;
			col.Prefix = el.Prefix;
#if NET_2_0
			table.Elements.Insert (++table.lastElementIndex, col);
#else
			table.Elements.Add (col);
#endif
		}

		private void CheckExtraneousElementColumn (TableMapping parentTable, XmlElement el)
		{
			if (parentTable == null)
				return;
			string localName = XmlHelper.Decode (el.LocalName);
			DataColumn elc = parentTable.GetColumn (localName);
			if (elc != null)
				parentTable.RemoveElementColumn (localName);
		}

		private void PopulatePrimaryKey (TableMapping table)
		{
			DataColumn col = new DataColumn (table.Table.TableName + "_Id");
			col.ColumnMapping = MappingType.Hidden;
			col.DataType = typeof (int);
			col.AllowDBNull = false;
			col.AutoIncrement = true;
			col.Namespace = table.Table.Namespace;
			col.Prefix = table.Table.Prefix;
			table.PrimaryKey = col;
		}

		private void PopulateRelationStructure (string parent, string child, string pkeyColumn)
		{
			if (relations [parent, child] != null)
				return;
			RelationStructure rs = new RelationStructure ();
			rs.ParentTableName = parent;
			rs.ChildTableName = child;
			rs.ParentColumnName = pkeyColumn;
			rs.ChildColumnName = pkeyColumn;
			rs.CreateConstraint = true;
			rs.IsNested = true;
			relations.Add (rs);
		}

		private void InferRepeatedElement (TableMapping parentTable, XmlElement el)
		{
			string localName = XmlHelper.Decode (el.LocalName);
			// FIXME: can be checked later
			CheckExtraneousElementColumn (parentTable, el);
			TableMapping table = GetMappedTable (parentTable, localName, el.NamespaceURI);

			// If the mapping is actually complex type (not simple
			// repeatable), then ignore it.
			if (table.Elements.Count > 0)
				return;

			// If simple column already exists, do nothing
			if (table.SimpleContent != null)
				return;

			GetMappedColumn (table, localName + "_Column", el.Prefix, el.NamespaceURI, MappingType.SimpleContent, null);
		}

		private void InferTableElement (TableMapping parentTable, XmlElement el)
		{
			// If parent table already has the same name column but
			// mapped as Element, that must be removed.
			// FIXME: This can be done later (doing it here is
			// loss of performance.
			CheckExtraneousElementColumn (parentTable, el);

			string localName = XmlHelper.Decode (el.LocalName);
			TableMapping table = GetMappedTable (parentTable, localName, el.NamespaceURI);

			bool hasChildElements = false;
			bool hasAttributes = false;
			bool hasText = false;
			bool isElementRepeated = false;

			foreach (XmlAttribute attr in el.Attributes) {
				if (attr.NamespaceURI == XmlConstants.XmlnsNS
#if NET_2_0
					|| attr.NamespaceURI == XmlConstants.XmlNS
#endif
					)
					continue;
				if (ignoredNamespaces != null &&
					ignoredNamespaces.Contains (attr.NamespaceURI))
					continue;

				hasAttributes = true;
				GetMappedColumn (table,
					XmlHelper.Decode (attr.LocalName),
					attr.Prefix,
				        attr.NamespaceURI,
          				MappingType.Attribute,
          				null); 
			}

			foreach (XmlNode n in el.ChildNodes) {
				switch (n.NodeType) {
				case XmlNodeType.Comment:
				case XmlNodeType.ProcessingInstruction: // ignore
					continue;
				default: // text content
					hasText = true;
					if (GetElementMappingType (el, ignoredNamespaces, null) == ElementMappingType.Repeated)
						isElementRepeated = true;
					break;
				case XmlNodeType.Element: // child
					hasChildElements = true;
					XmlElement cel = n as XmlElement;
					string childLocalName = XmlHelper.Decode (cel.LocalName);

					switch (GetElementMappingType (cel, ignoredNamespaces, null)) {
					case ElementMappingType.Simple:
						InferColumnElement (table, cel);
						break;
					case ElementMappingType.Repeated:
						if (table.PrimaryKey == null)
							PopulatePrimaryKey (table);
						PopulateRelationStructure (table.Table.TableName, childLocalName, table.PrimaryKey.ColumnName);
						InferRepeatedElement (table, cel);
						break;
					case ElementMappingType.Complex:
						if (table.PrimaryKey == null)
							PopulatePrimaryKey (table);
						PopulateRelationStructure (table.Table.TableName, childLocalName, table.PrimaryKey.ColumnName);
						InferTableElement (table, cel);
						break;
					}
					break;
				}
			}

			// Attributes + !Children + Text = SimpleContent
			if (table.SimpleContent == null // no need to create
				&& !hasChildElements && hasText && (hasAttributes || isElementRepeated)) {
				GetMappedColumn (table, table.Table.TableName + "_Text", String.Empty, String.Empty, MappingType.SimpleContent, null);
			}
		}

		private TableMapping GetMappedTable (TableMapping parent, string tableName, string ns)
		{
			TableMapping map = tables [tableName];
			if (map != null) {
				if (parent != null && map.ParentTable != null && map.ParentTable != parent)
					throw new DataException (String.Format ("The table '{0}' is already allocated as a child of another table '{1}'. Cannot set table '{2}' as parent table.", tableName, map.ParentTable.Table.TableName, parent.Table.TableName));
			} else {
				map = new TableMapping (tableName, ns);
				map.ParentTable = parent;
				tables.Add (map);
			}
			if (parent != null) {
				bool shouldAdd = true;
				foreach (TableMapping child in parent.ChildTables) {
					if (child.Table.TableName == tableName) {
						shouldAdd = false;
						break;
					}
				}
				if (shouldAdd)
					parent.ChildTables.Add (map);
			}
			return map;
		}

		private DataColumn GetMappedColumn (TableMapping table, string name, string prefix, string ns, MappingType type, Type optColType)
		{
			DataColumn col = table.GetColumn (name);
			// Infer schema
			if (col == null) {
				col = new DataColumn (name);
				col.Prefix = prefix;
				col.Namespace = ns;
				col.ColumnMapping = type;
				switch (type) {
				case MappingType.Element:
					table.Elements.Add (col);
					break;
				case MappingType.Attribute:
					table.Attributes.Add (col);
					break;
				case MappingType.SimpleContent:
					table.SimpleContent = col;
					break;
				case MappingType.Hidden:
					// To generate parent key
					col.DataType = optColType;
					table.ReferenceKey = col;
					break;
				}
			}
			else if (col.ColumnMapping != type) // Check mapping type
				throw new DataException (String.Format ("There are already another column that has different mapping type. Column is {0}, existing mapping type is {1}", col.ColumnName, col.ColumnMapping));

			return col;
		}

		private static void SetAsExistingTable (XmlElement el, Hashtable existingTables)
		{
			if (existingTables == null)
				return;
			ArrayList al = existingTables [el.NamespaceURI] as ArrayList;
			if (al == null) {
				al = new ArrayList ();
				existingTables [el.NamespaceURI] = al;
			}
			if (al.Contains (el.LocalName))
				return;
			al.Add (el.LocalName);
		}

		private static ElementMappingType GetElementMappingType (
			XmlElement el, ArrayList ignoredNamespaces, Hashtable existingTables)
		{
			if (existingTables != null) {
				ArrayList al = existingTables [el.NamespaceURI] as ArrayList;
				if (al != null && al.Contains (el.LocalName))
					// this is not precise, but it is enough
					// for IsDocumentElementTable().
					return ElementMappingType.Complex;
			}

			foreach (XmlAttribute attr in el.Attributes) {
				if (attr.NamespaceURI == XmlConstants.XmlnsNS 
#if NET_2_0
					|| attr.NamespaceURI == XmlConstants.XmlNS
#endif
					)
					continue;
				if (ignoredNamespaces != null && ignoredNamespaces.Contains (attr.NamespaceURI))
					continue;
				SetAsExistingTable (el, existingTables);
				return ElementMappingType.Complex;
			}
			foreach (XmlNode n in el.ChildNodes) {
				if (n.NodeType == XmlNodeType.Element) {
					SetAsExistingTable (el, existingTables);
					return ElementMappingType.Complex;
				}
			}

			for (XmlNode n = el.NextSibling; n != null; n = n.NextSibling) {
				if (n.NodeType == XmlNodeType.Element && n.LocalName == el.LocalName) {
					SetAsExistingTable (el, existingTables);
					return GetElementMappingType (n as XmlElement,
						ignoredNamespaces, null)
						== ElementMappingType.Complex ?
						ElementMappingType.Complex :
						ElementMappingType.Repeated;
				}
			}

			return ElementMappingType.Simple;
		}

		private bool IsDocumentElementTable ()
		{
			return IsDocumentElementTable (
				document.DocumentElement,
				ignoredNamespaces);
		}

		internal static bool IsDocumentElementTable (XmlElement top,
			ArrayList ignoredNamespaces)
		{
			foreach (XmlAttribute attr in top.Attributes) {
				if (attr.NamespaceURI == XmlConstants.XmlnsNS
#if NET_2_0
					|| attr.NamespaceURI == XmlConstants.XmlNS
#endif
					)
					continue;
				if (ignoredNamespaces != null &&
					ignoredNamespaces.Contains (attr.NamespaceURI))
					continue;
				// document element has attributes other than xmlns
				return true;
			}
			Hashtable existingTables = new Hashtable ();
			foreach (XmlNode n in top.ChildNodes) {
				XmlElement el = n as XmlElement;
				if (el == null)
					continue;
				if (GetElementMappingType (el, ignoredNamespaces,
					existingTables)
					== ElementMappingType.Simple)
					return true;
			}
			return false;
		}

		// Returns if it "might" be a column element (this method is
		// called per child element, thus it might still consist of
		// table, since it might be repeated).
		/*
		private bool IsPossibleColumnElement (XmlElement el)
		{
			foreach (XmlAttribute attr in el.Attributes) {
				if (attr.NamespaceURI == XmlConstants.XmlnsNS
#if NET_2_0
					|| attr.NamespaceURI == XmlConstants.XmlNS
#endif
					)
					continue;
				return false;
			}
			foreach (XmlNode n in el.ChildNodes)
				if (n.NodeType == XmlNodeType.Element)
					return false;
			return true;
		}
		*/
	}
}


#if TEST_STANDALONE_INFERENCE
internal class Driver
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
			XmlDataInferenceLoader.Infer (ds, doc, XmlReadMode.Auto, null);
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

#endif

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
//	Attributes are always (except for namespace nodes) inferred as 
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
