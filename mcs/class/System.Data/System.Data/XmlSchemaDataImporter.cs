//
// XmlSchemaDataImporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
//
// ***** The design note became somewhat obsolete. Should be rewritten. *****
//
// * Design Notes
//
// ** Abstract
//
//	This class is used to import an XML Schema into a DataSet schema.
//
//	Only XmlReader is acceptable as the input to the class.
//	This class is not expected to read XML Schema multi time.
//
// ** Targetable Schema Components
//
//	Only global global elements that hold complex type are converted 
//	into a table. 
//	<del>
//	The components of the type of the element are subsequently converted
//	into a table, BUT there is an exception. As for "DataSet elements",
//	the type is just ignored (see "DataSet Element definition" below).
//	</del><ins>
//	The components of the type of the element are subsequently converted
//	into a table. As for "DataSet elements", its complex type is also 
//	handled.
//	</ins>
//
//	Unused complex types are never be converted.
//
//	Global simple types and global attributes are never converted.
//	They cannot be a table.
//	Local complex types are also converted into a table.
//
//	Local elements are converted into either a table or a column in
//	the "context DataTable". Simple-typed element is not always converted
//	into a DataColumn; if maxOccurs > 1, it will be converted as a table.
//
// ** Name Convention
//
//	Ignore this section. Microsoft.NET was buggy enough to confuse
//	against these name conflicts.
//
//	Since local complex types are anonymous, we have to name for each
//	component. Thus, and since complex types and elements can have the 
//	same name each other, we have to manage a table for mappings from 
//	a name to a component. The names must be also used in DataRelation
//	definitions correctly.
//
// ** DataSet element definition
//
//	"DataSet element" is 1) such element that has an attribute 
//	msdata:IsDataSet (where prefix "msdata" is bound to 
//	urn:schemas-microsoft-com:xml-msdata), or 2) the only one
//	element definition in the schema.
//
//	There is another complicated rule. 1) If there is only one element EL 
//	in the schema, and 2) if the type of EL is complex named CT, and 3)
//	the content of the CT is a group base, and 4) the group base contains 
//	an element EL2, and finally 5) if EL2 is complex, THEN the element is
//	the DataSet element.
//
//	Only the first global element that matches the condition above is
//	regarded as DataSet element (by necessary design or just a bug?) 
//	instead of handling as an error.
//
//	All global elements are considered as an alternative in the dataset
//	element.
//
//	For local elements, msdata:IsDataSet are just ignored.
//
// ** Importing Complex Types as Columns
//
//	When an xs:element is going to be mapped, its complex type (remember
//	that only complex-typed elements are targettable) are expanded to
//	DataColumn.
//
//	DataColumn has a property MappingType that shows whether this column
//	 came from attribute or element.
//
//	[Question: How about MappingType.Simple? How is it used?]
//
//	Additionally, for particle elements, it might also create another
//	DataTable (but for the particle elements in context DataTable, it
//	will create an index to the new table).
//
//	For group base particles (XmlSchemaGroupBase; sequence, choice, all)
//	each component in those groups are mapped to a column. Even if you
//	import "choice" or "all" components, DataSet.WriteXmlSchema() will
//	output them just as a "sequence".
//
//	Columns cannot be added directly to current context DataTable; they
//	need to be added after processing all the columns, because they may
//	have msdata:Ordinal attribute that specifies the order of the columns
//	in the DataTable.
//
//	"Nested elements" are not allowed. (Clarification required?)
//
// ** Identity Constraints and DataRelations
//
// *** DataRelations from element identity constraints
//
//	Only constraints on "DataSet element" is considered. All other
//	constraint definitions are ignored. Note that it is DataSet that has
//	the property Relations (of type DataRelationCollection).
//
//	xs:key and xs:unique are handled as the same (then both will be
//	serialized as xs:unique).
//
//	The XPath expressions in the constraints are strictly limited; they
//	are expected to be expandable enough to be mappable for each
//
//		* selector to "any_valid_XPath/is/OK/blah" 
//		  where "blah" is one of the DataTable name. It looks that
//		  only the last QName section is significant and any heading
//		  XPath step is OK (even if the mapped node does not exist).
//		* field to QName that is mapped to DataColumn in the DataTable
//		  (even ./QName is not allowed)
//
// *** DataRelations from annotations
//
//	See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/_mapping_relationship_specified_for_nested_elements.asp and http://msdn.microsoft.com/library/en-us/cpguide/html/_specifying_relationship_between_elements_with_no_nesting.asp
//
// ** Informative References
//
// Generating DataSet Relational Structure from XML Schema (XSD)
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/_generating_dataset_relational_structure_from_xsd.asp
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
using System.Xml;
using System.Xml.Schema;


namespace System.Data
{
	internal class TableStructureCollection : CollectionBase
	{
		public void Add (TableStructure table)
		{
			List.Add (table);
		}

		public TableStructure this [int i] {
			get { return List [i] as TableStructure; }
		}

		public TableStructure this [string name] {
			get {
				foreach (TableStructure ts in List)
					if (ts.Table.TableName == name)
						return ts;
				return null;
			}
		}
	}

	internal class RelationStructureCollection : CollectionBase
	{
		public void Add (RelationStructure rel)
		{
			List.Add (rel);
		}

		public RelationStructure this [int i] {
			get { return List [i] as RelationStructure; }
		}

		public RelationStructure this [string parent, string child] {
			get {
				foreach (RelationStructure rel in List)
					if (rel.ParentTableName == parent && rel.ChildTableName == child)
						return rel;
				return null;
			}
		}
	}

	internal class TableStructure
	{
		public TableStructure (DataTable table)
		{
			this.Table = table;
		}

		// The columns and orders which will be added to the context
		// table (See design notes; Because of the ordinal problem)
		public DataTable Table;
		public Hashtable OrdinalColumns = new Hashtable ();
		public ArrayList NonOrdinalColumns = new ArrayList ();
		public DataColumn PrimaryKey;

		public bool ContainsColumn (string name)
		{
			foreach (DataColumn col in NonOrdinalColumns)
				if (col.ColumnName == name)
					return true;
			foreach (DataColumn col in OrdinalColumns.Keys)
				if (col.ColumnName == name)
					return true;
			return false;
		}
	}

	internal class RelationStructure
	{
		public string ExplicitName;
		public string ParentTableName;
		public string ChildTableName;
		public string ParentColumnName;
		public string ChildColumnName;
		public bool IsNested;
		public bool CreateConstraint;
	}

	internal class XmlSchemaDataImporter
	{
		static readonly XmlSchemaDatatype schemaIntegerType;
		static readonly XmlSchemaDatatype schemaDecimalType;
		static readonly XmlSchemaComplexType schemaAnyType;

		static XmlSchemaDataImporter ()
		{
			XmlSchema s = new XmlSchema ();
			XmlSchemaAttribute a = new XmlSchemaAttribute ();
			a.Name = "foo";
			// FIXME: mcs looks to have a bug around static 
			// reference resolution. XmlSchema.Namespace should work.
			a.SchemaTypeName = new XmlQualifiedName ("integer", System.Xml.Schema.XmlSchema.Namespace);
			s.Items.Add (a);
			XmlSchemaAttribute b = new XmlSchemaAttribute ();
			b.Name = "bar";
			// FIXME: mcs looks to have a bug around static 
			// reference resolution. XmlSchema.Namespace should work.
			b.SchemaTypeName = new XmlQualifiedName ("decimal", System.Xml.Schema.XmlSchema.Namespace);
			s.Items.Add (b);
			XmlSchemaElement e = new XmlSchemaElement ();
			e.Name = "bar";
			s.Items.Add (e);
			s.Compile (null);
			schemaIntegerType = a.AttributeType as XmlSchemaDatatype;
			schemaDecimalType = b.AttributeType as XmlSchemaDatatype;
			schemaAnyType = e.ElementType as XmlSchemaComplexType;
		}

		#region Fields

		DataSet dataset;
		XmlSchema schema;

		ArrayList relations = new ArrayList ();

		// such element that has an attribute msdata:IsDataSet="true"
		XmlSchemaElement datasetElement;

		// choice alternatives in the "dataset element"
		ArrayList topLevelElements = new ArrayList ();

		// import target elements
		ArrayList targetElements = new ArrayList ();

		TableStructure currentTable;

		#endregion

		// .ctor()

		public XmlSchemaDataImporter (DataSet dataset, XmlReader reader)
		{
			this.dataset = dataset;
			dataset.DataSetName = "NewDataSet"; // Initialize always
			schema = XmlSchema.Read (reader, null);
			// FIXME: Just XmlSchema.Namespace should work (mcs bug)
			if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "schema" && reader.NamespaceURI == System.Xml.Schema.XmlSchema.Namespace)
				reader.ReadEndElement ();
			schema.Compile (null);
		}

		// methods

		public void Process ()
		{
			if (schema.Id != null)
				dataset.DataSetName = schema.Id; // default. Overridable by "DataSet element"
			dataset.Namespace = schema.TargetNamespace;

			foreach (XmlSchemaObject obj in schema.Items) {
				if (obj is XmlSchemaElement) {
					XmlSchemaElement el = obj as XmlSchemaElement;
					if (el.ElementType is XmlSchemaComplexType &&
el.ElementType != schemaAnyType)
						targetElements.Add (obj);
				}
			}

			// This collection will grow up while processing elements.
			int globalElementCount = targetElements.Count;

			for (int i = 0; i < globalElementCount; i++)
				ProcessGlobalElement ((XmlSchemaElement) targetElements [i]);

			// Rest are local elements.
			for (int i = globalElementCount; i < targetElements.Count; i++)
				ProcessDataTableElement ((XmlSchemaElement) targetElements [i]);

			// Handle relation definitions written as xs:annotation.
			// See detail: http://msdn.microsoft.com/library/shared/happyUrl/fnf_msdn.asp?Redirect=%22http://msdn.microsoft.com/404/default.asp%22
			foreach (XmlSchemaObject obj in schema.Items)
				if (obj is XmlSchemaAnnotation)
					HandleAnnotations ((XmlSchemaAnnotation) obj, false);

			foreach (RelationStructure rs in this.relations)
				dataset.Relations.Add (GenerateRelationship (rs));

			if (datasetElement != null) {
				// Handle constraints in the DataSet element. First keys.
				foreach (XmlSchemaObject obj in datasetElement.Constraints)
					if (! (obj is XmlSchemaKeyref))
						ProcessParentKey ((XmlSchemaIdentityConstraint) obj);
				// Then keyrefs.
				foreach (XmlSchemaObject obj in datasetElement.Constraints)
					if (obj is XmlSchemaKeyref)
						ProcessReferenceKey (datasetElement, (XmlSchemaKeyref) obj);
			}
		}

		private bool IsDataSetElement (XmlSchemaElement el)
		{
			if (schema.Elements.Count != 1)
				return false;
			if (!(el.SchemaType is XmlSchemaComplexType))
				return false;
			XmlSchemaComplexType ct = (XmlSchemaComplexType) el.SchemaType;
			if (ct.AttributeUses.Count > 0)
				return false;
			XmlSchemaGroupBase gb = ct.ContentTypeParticle as XmlSchemaGroupBase;
			if (gb == null || gb.Items.Count == 0)
				return false;
			foreach (XmlSchemaParticle p in gb.Items) {
				if (ContainsColumn (p))
					return false;
			}
			return true;
		}

		private bool ContainsColumn (XmlSchemaParticle p)
		{
			XmlSchemaElement el = p as XmlSchemaElement;
			if (el != null) {
				XmlSchemaComplexType ct = el.ElementType as XmlSchemaComplexType;
				if (ct == null || ct == schemaAnyType)
					return true; // column element
				if (ct.AttributeUses.Count > 0)
					return false; // table element
				switch (ct.ContentType) {
				case XmlSchemaContentType.Empty:
				case XmlSchemaContentType.TextOnly:
					return true; // column element
				default:
					return false; // table element
				}
			}
			XmlSchemaGroupBase gb = p as XmlSchemaGroupBase;
			for (int i = 0; i < gb.Items.Count; i++) {
				if (ContainsColumn ((XmlSchemaParticle) gb.Items [i]))
					return true;
			}
			return false;
		}

		private void ProcessGlobalElement (XmlSchemaElement el)
		{
			// If it is already registered (by resolving reference
			// in previously-imported elements), just ignore.
			if (dataset.Tables.Contains (el.QualifiedName.Name))
				return;

			// Check if element is DataSet element
			if (el.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in el.UnhandledAttributes) {
					if (attr.LocalName == "IsDataSet" &&
						attr.NamespaceURI == XmlConstants.MsdataNamespace) {
						switch (attr.Value) {
						case "true": // case sensitive
							ProcessDataSetElement (el);
							return;
						case "false":
							break;
						default:
							throw new DataException (String.Format ("Value {0} is invalid for attribute 'IsDataSet'.", attr.Value));
						}
					}
				}
			}

			// If type is not complex, just skip this element
			if (! (el.ElementType is XmlSchemaComplexType && el.ElementType != schemaAnyType))
				return;

			if (IsDataSetElement (el)) {
				ProcessDataSetElement (el);
				return;
			}

			// Register as a top-level element
			topLevelElements.Add (el);
			// Create DataTable for this element
			ProcessDataTableElement (el);
		}

		private void ProcessDataSetElement (XmlSchemaElement el)
		{
			dataset.DataSetName = el.Name;
			this.datasetElement = el;

			// Search for msdata:Locale attribute
			if (el.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in el.UnhandledAttributes) {
					if (attr.LocalName == "Locale" &&
						attr.NamespaceURI == XmlConstants.MsdataNamespace) {
						CultureInfo ci = new CultureInfo (attr.Value);
						dataset.Locale = ci;
					}
				}
			}

			// Process content type particle (and create DataTable)
			XmlSchemaComplexType ct = el.ElementType as XmlSchemaComplexType;
			XmlSchemaParticle p = ct != null ? ct.ContentTypeParticle : null;
			if (p != null)
				HandleDataSetContentTypeParticle (p);
		}

		private void HandleDataSetContentTypeParticle (XmlSchemaParticle p)
		{
			XmlSchemaElement el = p as XmlSchemaElement;
			if (el != null) {
				if (el.ElementType is XmlSchemaComplexType && el.RefName != el.QualifiedName)
					ProcessDataTableElement (el);
			}
			else if (p is XmlSchemaGroupBase) {
				foreach (XmlSchemaParticle pc in ((XmlSchemaGroupBase) p).Items)
					HandleDataSetContentTypeParticle (pc);
			}
		}

		private void ProcessDataTableElement (XmlSchemaElement el)
		{
			string tableName = XmlConvert.DecodeName (el.QualifiedName.Name);
			// If it is already registered, just ignore.
			if (dataset.Tables.Contains (tableName))
				return;

			DataTable table = new DataTable (tableName);
			table.Namespace = el.QualifiedName.Namespace;
			currentTable = new TableStructure (table);

			dataset.Tables.Add (table);

			// Find Locale
			if (el.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in el.UnhandledAttributes) {
					if (attr.LocalName == "Locale" &&
						attr.NamespaceURI == XmlConstants.MsdataNamespace)
						table.Locale = new CultureInfo (attr.Value);
				}
			}

			// Handle complex type (NOTE: It is or should be 
			// impossible the type is other than complex type).
			XmlSchemaComplexType ct = (XmlSchemaComplexType) el.ElementType;

			// Handle attributes
			foreach (DictionaryEntry de in ct.AttributeUses)
				ImportColumnAttribute ((XmlSchemaAttribute) de.Value);

			// Handle content type particle
			if (ct.ContentTypeParticle is XmlSchemaElement)
				ImportColumnElement (el, (XmlSchemaElement) ct.ContentTypeParticle);
			else if (ct.ContentTypeParticle is XmlSchemaGroupBase)
				ImportColumnGroupBase (el, (XmlSchemaGroupBase) ct.ContentTypeParticle);
			// else if null then do nothing.

			// Handle simple content
			switch (ct.ContentType) {
			case XmlSchemaContentType.TextOnly:
//			case XmlSchemaContentType.Mixed:
				// LAMESPEC: When reading from XML Schema, it maps to "_text", while on the data inference, it is mapped to "_Text" (case ignorant).
				string simpleName = el.QualifiedName.Name + "_text";
				DataColumn simple = new DataColumn (simpleName);
				simple.Namespace = el.QualifiedName.Namespace;
				simple.AllowDBNull = (el.MinOccurs == 0);
				simple.ColumnMapping = MappingType.SimpleContent;
				simple.DataType = ConvertDatatype (ct.Datatype);
				currentTable.NonOrdinalColumns.Add (simple);
				break;
			}

			// add columns to the table in specified order 
			// (by msdata:Ordinal attributes)
			SortedList sd = new SortedList ();
			foreach (DictionaryEntry de in currentTable.OrdinalColumns)
				sd.Add (de.Value, de.Key);
			foreach (DictionaryEntry de in sd)
				table.Columns.Add ((DataColumn) de.Value);
			foreach (DataColumn dc in currentTable.NonOrdinalColumns)
				table.Columns.Add (dc);
		}

		private DataRelation GenerateRelationship (RelationStructure rs)
		{
			DataTable ptab = dataset.Tables [rs.ParentTableName];
			DataTable ctab = dataset.Tables [rs.ChildTableName];
			DataColumn pcol = ptab.Columns [rs.ParentColumnName];
			DataColumn ccol = ctab.Columns [rs.ChildColumnName];

			if (ccol == null) {
				ccol = new DataColumn ();
				ccol.ColumnName = pcol.ColumnName;
				ccol.Namespace = String.Empty; // don't copy
				ccol.ColumnMapping = MappingType.Hidden;
				ccol.DataType = pcol.DataType;
				ctab.Columns.Add (ccol);
			}

			string name = rs.ExplicitName != null ? rs.ExplicitName : XmlConvert.DecodeName (ptab.TableName) + '_' + XmlConvert.DecodeName (ctab.TableName);
			DataRelation rel = new DataRelation (name, pcol, ccol, rs.CreateConstraint);
			rel.Nested = rs.IsNested;
			if (rs.CreateConstraint)
				rel.ParentTable.PrimaryKey = rel.ParentColumns;
			return rel;
		}

		private void ImportColumnGroupBase (XmlSchemaElement parent, XmlSchemaGroupBase gb)
		{
			foreach (XmlSchemaParticle p in gb.Items) {
				XmlSchemaElement el = p as XmlSchemaElement;
				if (el != null)
					ImportColumnElement (parent, el);
				else
					ImportColumnGroupBase (parent, (XmlSchemaGroupBase) p);
			}
		}

		private XmlSchemaDatatype GetSchemaPrimitiveType (object type)
		{
			if (type is XmlSchemaComplexType)
				return null; // It came here, so that maybe it is xs:anyType
			XmlSchemaDatatype dt = type as XmlSchemaDatatype;
			if (dt == null && type != null)
				dt = ((XmlSchemaSimpleType) type).Datatype;
			return dt;
		}

		// Note that this column might be Hidden
		private void ImportColumnAttribute (XmlSchemaAttribute attr)
		{
			DataColumn col = new DataColumn ();
			col.ColumnName = attr.QualifiedName.Name;
			col.Namespace = attr.QualifiedName.Namespace;
			XmlSchemaDatatype dt = GetSchemaPrimitiveType (attr.AttributeType);
			// This complicated check comes from the fact that
			// MS.NET fails to map System.Object to anyType (that
			// will cause ReadTypedObject() fail on XmlValidatingReader).
			// ONLY In DataSet context, we set System.String for
			// simple ur-type.
			col.DataType = ConvertDatatype (dt);
			if (col.DataType == typeof (object))
				col.DataType = typeof (string);
			// When attribute use="prohibited", then it is regarded as 
			// Hidden column.
			if (attr.Use == XmlSchemaUse.Prohibited)
				col.ColumnMapping = MappingType.Hidden;
			else {
				col.ColumnMapping = MappingType.Attribute;
				col.DefaultValue = GetAttributeDefaultValue (attr);
			}
			if (attr.Use == XmlSchemaUse.Required)
				col.AllowDBNull = false;

			FillFacet (col, attr.AttributeType as XmlSchemaSimpleType);

			// Call this method after filling the name
			ImportColumnMetaInfo (attr, attr.QualifiedName, col);
			AddColumn (col);
		}

		private void ImportColumnElement (XmlSchemaElement parent, XmlSchemaElement el)
		{
			// FIXME: element nest check

			DataColumn col = new DataColumn ();
			col.DefaultValue = GetElementDefaultValue (el);
			col.AllowDBNull = (el.MinOccurs == 0);

			if (el.ElementType is XmlSchemaComplexType && el.ElementType != schemaAnyType)
				FillDataColumnComplexElement (parent, el, col);
			else if (el.MaxOccurs != 1)
				FillDataColumnRepeatedSimpleElement (parent, el, col);
			else
				FillDataColumnSimpleElement (el, col);
		}

		// common process for element and attribute
		private void ImportColumnMetaInfo (XmlSchemaAnnotated obj, XmlQualifiedName name, DataColumn col)
		{
			int ordinal = -1;
			if (obj.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in obj.UnhandledAttributes) {
					if (attr.NamespaceURI != XmlConstants.MsdataNamespace)
						continue;
					switch (attr.LocalName) {
					case XmlConstants.Caption:
						col.Caption = attr.Value;
						break;
					case XmlConstants.DataType:
						col.DataType = Type.GetType (attr.Value);
						break;
					case XmlConstants.AutoIncrement:
						col.AutoIncrement = bool.Parse (attr.Value);
						break;
					case XmlConstants.AutoIncrementSeed:
						col.AutoIncrementSeed = int.Parse (attr.Value);
						break;
					case XmlConstants.ReadOnly:
						col.ReadOnly = XmlConvert.ToBoolean (attr.Value);
						break;
					case XmlConstants.Ordinal:
						ordinal = int.Parse (attr.Value);
						break;
					}
				}
			}
		}

		private void FillDataColumnComplexElement (XmlSchemaElement parent, XmlSchemaElement el, DataColumn col)
		{
			if (targetElements.Contains (el))
				return; // do nothing

			string elName = XmlConvert.DecodeName (el.QualifiedName.Name);
			if (elName == dataset.DataSetName)
				// Well, why it is ArgumentException :-?
				throw new ArgumentException ("Nested element must not have the same name as DataSet's name.");

			if (el.Annotation != null)
				HandleAnnotations (el.Annotation, true);
			else {
				AddParentKeyColumn (parent, el, col);
				DataColumn pkey = currentTable.PrimaryKey;

				RelationStructure rel = new RelationStructure ();
				rel.ParentTableName = XmlConvert.DecodeName (parent.QualifiedName.Name);
				rel.ChildTableName = elName;
				rel.ParentColumnName = pkey.ColumnName;
				rel.ChildColumnName = pkey.ColumnName;
				rel.CreateConstraint = true;
				rel.IsNested = true;
				relations.Add (rel);
			}

			// If the element is not referenced one, the element will be handled later.
			if (el.RefName == XmlQualifiedName.Empty)
				targetElements.Add (el);

		}

		private void AddParentKeyColumn (XmlSchemaElement parent, XmlSchemaElement el, DataColumn col)
		{
			if (currentTable.PrimaryKey != null)
				return;

			// check name identity
			string name = XmlConvert.DecodeName (parent.QualifiedName.Name) + "_Id";
			if (currentTable.ContainsColumn (name))
				throw new DataException (String.Format ("There is already a column that has the same name: {0}", name));
			// check existing primary key
			if (currentTable.Table.PrimaryKey.Length > 0)
				throw new DataException (String.Format ("There is already primary key columns in the table \"{0}\".", currentTable.Table.TableName));

			col.ColumnName = name;
			col.ColumnMapping = MappingType.Hidden;
			col.Namespace = parent.QualifiedName.Namespace;
			col.DataType = typeof (int);
			col.Unique = true;
			col.AutoIncrement = true;
			col.AllowDBNull = false;

			ImportColumnMetaInfo (el, el.QualifiedName, col);
			AddColumn (col);
			currentTable.PrimaryKey = col;
		}

		private void FillDataColumnRepeatedSimpleElement (XmlSchemaElement parent, XmlSchemaElement el, DataColumn col)
		{
			if (targetElements.Contains (el))
				return; // do nothing

			AddParentKeyColumn (parent, el, col);
			DataColumn pkey = currentTable.PrimaryKey;

			string elName = XmlConvert.DecodeName (el.QualifiedName.Name);
			string parentName = XmlConvert.DecodeName (parent.QualifiedName.Name);

			DataTable dt = new DataTable ();
			dt.TableName = elName;
			dt.Namespace = el.QualifiedName.Namespace;
			// reference key column to parent
			DataColumn cc = new DataColumn ();
			cc.ColumnName = parentName + "_Id";
			cc.Namespace = parent.QualifiedName.Namespace;
			cc.ColumnMapping = MappingType.Hidden;
			cc.DataType = typeof (int);

			// repeatable content simple element
			DataColumn cc2 = new DataColumn ();
			cc2.ColumnName = elName + "_Column";
			cc2.Namespace = el.QualifiedName.Namespace;
			cc2.ColumnMapping = MappingType.SimpleContent;
			cc2.AllowDBNull = false;
			cc2.DataType = ConvertDatatype (GetSchemaPrimitiveType (el.ElementType));

			dt.Columns.Add (cc2);
			dt.Columns.Add (cc);
			dataset.Tables.Add (dt);

			RelationStructure rel = new RelationStructure ();
			rel.ParentTableName = parentName;
			rel.ChildTableName = dt.TableName;
			rel.ParentColumnName = pkey.ColumnName;
			rel.ChildColumnName = cc.ColumnName;
			rel.IsNested = true;
			rel.CreateConstraint = true;
			relations.Add (rel);
		}

		private void FillDataColumnSimpleElement (XmlSchemaElement el, DataColumn col)
		{
			col.ColumnName = XmlConvert.DecodeName (el.QualifiedName.Name);
			col.Namespace = el.QualifiedName.Namespace;
			col.ColumnMapping = MappingType.Element;
			col.DataType = ConvertDatatype (GetSchemaPrimitiveType (el.ElementType));
			FillFacet (col, el.ElementType as XmlSchemaSimpleType);

			ImportColumnMetaInfo (el, el.QualifiedName, col);

			AddColumn (col);
		}

		private void AddColumn (DataColumn col)
		{
			if (col.Ordinal < 0)
				currentTable.NonOrdinalColumns.Add (col);
			else
				currentTable.OrdinalColumns.Add (col, col.Ordinal);
		}

		private void FillFacet (DataColumn col, XmlSchemaSimpleType st)
		{
			if (st == null || st.Content == null)
				return;

			// Handle restriction facets

			XmlSchemaSimpleTypeRestriction restriction = st == null ? null : st.Content as XmlSchemaSimpleTypeRestriction;
			if (restriction == null)
				throw new DataException ("DataSet does not suport 'list' nor 'union' simple type.");

			foreach (XmlSchemaFacet f in restriction.Facets) {
				if (f is XmlSchemaMaxLengthFacet)
					// There is no reason why MaxLength is limited to int, except for the fact that DataColumn.MaxLength property is int.
					col.MaxLength = int.Parse (f.Value);
			}
		}

		private Type ConvertDatatype (XmlSchemaDatatype dt)
		{
			if (dt == null)
				return typeof (string);
			else if (dt.ValueType == typeof (decimal)) {
				// LAMESPEC: MSDN documentation says it is based 
				// on ValueType. However, in the System.Xml.Schema
				// context, xs:integer is mapped to Decimal, while
				// in DataSet context it is mapped to Int64.
				if (dt == schemaDecimalType)
					return typeof (decimal);
				else if (dt == schemaIntegerType)
					return typeof (long);
				else
					return typeof (ulong);
			}
			else
				return dt.ValueType;
		}

		// This method cuts out the local name of the last step from XPath.
		// It is nothing more than hack. However, MS looks to do similar.
		private string GetSelectorTarget (string xpath)
		{
			string tableName = xpath;
			int index = tableName.LastIndexOf ('/');
			// '>' is enough. If XPath [0] = '/', it is invalid. 
			// Selector can specify only element axes.
			if (index > 0)
				tableName = tableName.Substring (index + 1);

			// Round QName to NSName
			index = tableName.LastIndexOf (':');
			if (index > 0)
				tableName = tableName.Substring (index + 1);

			return XmlConvert.DecodeName (tableName);
		}

		private void ProcessParentKey (XmlSchemaIdentityConstraint ic)
		{
			// Basic concept came from XmlSchemaMapper.cs

			string tableName = GetSelectorTarget (ic.Selector.XPath);
			
			DataTable dt = dataset.Tables [tableName];
			if (dt == null)
				throw new DataException (String.Format ("Invalid XPath selection inside selector. Cannot find: {0}", tableName));

			DataColumn [] cols = new DataColumn [ic.Fields.Count];
			int i = 0;
			foreach (XmlSchemaXPath Field in ic.Fields) {
				string colName = Field.XPath;
				bool isAttr = colName.Length > 0 && colName [0] == '@';
				int index = colName.LastIndexOf (':');
				if (index > 0)
					colName = colName.Substring (index + 1);
				else if (isAttr)
					colName = colName.Substring (1);

				colName = XmlConvert.DecodeName (colName);
				DataColumn col = dt.Columns [colName];
				if (col == null)
					throw new DataException (String.Format ("Invalid XPath selection inside field. Cannot find: {0}", tableName));
				if (isAttr && col.ColumnMapping != MappingType.Attribute)
					throw new DataException ("The XPath specified attribute field, but mapping type is not attribute.");
				if (!isAttr && col.ColumnMapping != MappingType.Element)
					throw new DataException ("The XPath specified simple element field, but mapping type is not simple element.");

				cols [i] = dt.Columns [colName];
				i++;
			}
			
			bool isPK = false;
			// find if there is an attribute with the constraint name
			// if not use the XmlSchemaConstraint's name.
			string constraintName = ic.Name;
			if (ic.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in ic.UnhandledAttributes) {
					if (attr.NamespaceURI != XmlConstants.MsdataNamespace)
						continue;
					switch (attr.LocalName) {
					case XmlConstants.ConstraintName:
						constraintName = attr.Value;
						break;
					case XmlConstants.PrimaryKey:
						isPK = bool.Parse(attr.Value);
						break;
					}
				}
			}
			UniqueConstraint c = new UniqueConstraint (constraintName, cols, isPK);
			dt.Constraints.Add (c);
		}

		private void ProcessReferenceKey (XmlSchemaElement element, XmlSchemaKeyref keyref)
		{
			// Basic concept came from XmlSchemaMapper.cs

			string tableName = GetSelectorTarget (keyref.Selector.XPath);

			DataColumn [] cols;
			DataTable dt = dataset.Tables [tableName];
			if (dt == null)
				throw new DataException (String.Format ("Invalid XPath selection inside selector. Cannot find: {0}", tableName));

			cols = new DataColumn [keyref.Fields.Count];
			int i = 0;
			foreach (XmlSchemaXPath Field in keyref.Fields) {
				string colName = Field.XPath;
				bool isAttr = colName.Length > 0 && colName [0] == '@';
				int index = colName.LastIndexOf (':');
				if (index > 0)
					colName = colName.Substring (index + 1);
				else if (isAttr)
					colName = colName.Substring (1);

				colName = XmlConvert.DecodeName (colName);
				DataColumn col = dt.Columns [colName];
				if (isAttr && col.ColumnMapping != MappingType.Attribute)
					throw new DataException ("The XPath specified attribute field, but mapping type is not attribute.");
				if (!isAttr && col.ColumnMapping != MappingType.Element)
					throw new DataException ("The XPath specified simple element field, but mapping type is not simple element.");
				cols [i] = col;
				i++;
			}
			string name = keyref.Refer.Name;
			// get the unique constraint for the releation
			UniqueConstraint uniq = FindConstraint (name, element);
			// generate the FK.
			ForeignKeyConstraint fkc = new ForeignKeyConstraint(keyref.Name, uniq.Columns, cols);
			dt.Constraints.Add (fkc);
			// generate the relation.
			DataRelation rel = new DataRelation (keyref.Name, uniq.Columns, cols, false);
			if (keyref.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in keyref.UnhandledAttributes)
					if (attr.LocalName == "IsNested" && attr.Value == "true" && attr.NamespaceURI == XmlConstants.MsdataNamespace)
						rel.Nested = true;
			}
			rel.SetParentKeyConstraint (uniq);
			rel.SetChildKeyConstraint (fkc);

			dataset.Relations.Add (rel);
		}

		// get the unique constraint for the relation.
		// name - the name of the XmlSchemaUnique element
		private UniqueConstraint FindConstraint (string name, XmlSchemaElement element)
		{
			// Copied from XmlSchemaMapper.cs

			// find the element in the constraint collection.
			foreach (XmlSchemaIdentityConstraint c in element.Constraints) {
				if (c is XmlSchemaKeyref)
					continue;

				if (c.Name == name) {
					string tableName = GetSelectorTarget (c.Selector.XPath);

					// find the table in the dataset.
					DataTable dt = dataset.Tables [tableName];

					string constraintName = c.Name;
					// find if there is an attribute with the constraint name
					// if not use the XmlSchemaUnique name.
					if (c.UnhandledAttributes != null)
						foreach (XmlAttribute attr in c.UnhandledAttributes)
							if (attr.LocalName == "ConstraintName" && attr.NamespaceURI == XmlConstants.MsdataNamespace)
								constraintName = attr.Value;
					return (UniqueConstraint) dt.Constraints [constraintName];
				}
			}
			throw new DataException ("Target identity constraint was not found: " + name);
		}

		private void HandleAnnotations (XmlSchemaAnnotation an, bool nested)
		{
			foreach (XmlSchemaObject content in an.Items) {
				XmlSchemaAppInfo ai = content as XmlSchemaAppInfo;
				if (ai != null) {
					foreach (XmlNode n in ai.Markup) {
						XmlElement el = n as XmlElement;
						if (el != null && el.LocalName == "Relationship" && el.NamespaceURI == XmlConstants.MsdataNamespace)
							HandleRelationshipAnnotation (el, nested);
					}
				}
			}
		}

		private void HandleRelationshipAnnotation (XmlElement el, bool nested)
		{
			string name = el.GetAttribute ("name");
			string ptn = el.GetAttribute ("parent", XmlConstants.MsdataNamespace);
			string ctn = el.GetAttribute ("child", XmlConstants.MsdataNamespace);
			string pkn = el.GetAttribute ("parentkey", XmlConstants.MsdataNamespace);
			string fkn = el.GetAttribute ("childkey", XmlConstants.MsdataNamespace);

			RelationStructure rel = new RelationStructure ();
			rel.ExplicitName = name;
			rel.ParentTableName = ptn;
			rel.ChildTableName = ctn;
			rel.ParentColumnName = pkn;
			rel.ChildColumnName = fkn;
			rel.IsNested = nested;
			rel.CreateConstraint = false; // by default?
			relations.Add (rel);
		}

		private object GetElementDefaultValue (XmlSchemaElement elem)
		{
			// Unlike attribute, element cannot have a default value.
			if (elem.RefName == XmlQualifiedName.Empty)
				return elem.DefaultValue;
			XmlSchemaElement referenced = schema.Elements [elem.RefName] as XmlSchemaElement;
			if (referenced == null) // considering missing sub components
				return null;
			return referenced.DefaultValue;
		}

		private object GetAttributeDefaultValue (XmlSchemaAttribute attr)
		{
#if BUGGY_MS_COMPATIBLE
			if (attr == null)
				return null;
			else if (attr.RefName != XmlQualifiedName.Empty) {
				XmlSchemaAttribute referenced = schema.Attributes [attr.RefName] as XmlSchemaAttribute;
				if (referenced != null)
					return referenced.DefaultValue;
				else
					return null;
			}
			if (attr.DefaultValue != null)
				return attr.DefaultValue;
			return attr.FixedValue;
#else
			if (attr.DefaultValue != null)
				return attr.DefaultValue;
			else if (attr.FixedValue != null)
				return attr.FixedValue;
			else if (attr.RefName == XmlQualifiedName.Empty)
				return null;
			XmlSchemaAttribute referenced = schema.Attributes [attr.RefName] as XmlSchemaAttribute;
			if (referenced == null) // considering missing sub components
				return null;
			if (referenced.DefaultValue != null)
				return referenced.DefaultValue;
			return referenced.FixedValue;
#endif
		}
	}
}
