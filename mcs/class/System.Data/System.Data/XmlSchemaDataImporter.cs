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
using System.Data.Common;
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

	internal class ConstraintStructure
	{
		public readonly string TableName;
		public readonly string [] Columns;
		public readonly bool [] IsAttribute;
		public readonly string ConstraintName;
		public readonly bool IsPrimaryKey;
		public readonly string ReferName;
		public readonly bool IsNested;
		public readonly bool IsConstraintOnly;

		public ConstraintStructure (string tname, string [] cols, bool [] isAttr, string cname, bool isPK, string refName, bool isNested, bool isConstraintOnly)
		{
			TableName = tname;
			Columns = cols;
			IsAttribute = isAttr;
			ConstraintName = XmlHelper.Decode (cname);
			IsPrimaryKey = isPK;
			ReferName = refName;
			IsNested = isNested;
			IsConstraintOnly = isConstraintOnly;
		}
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
			a.SchemaTypeName = new XmlQualifiedName ("integer", XmlSchema.Namespace);
			s.Items.Add (a);
			XmlSchemaAttribute b = new XmlSchemaAttribute ();
			b.Name = "bar";
			b.SchemaTypeName = new XmlQualifiedName ("decimal", XmlSchema.Namespace);
			s.Items.Add (b);
			XmlSchemaElement e = new XmlSchemaElement ();
			e.Name = "bar";
			s.Items.Add (e);
			s.Compile (null);
#if NET_2_0
			schemaIntegerType = ((XmlSchemaSimpleType) a.AttributeSchemaType).Datatype;
			schemaDecimalType = ((XmlSchemaSimpleType) b.AttributeSchemaType).Datatype;
			schemaAnyType = e.ElementSchemaType as XmlSchemaComplexType;
#else
			schemaIntegerType = a.AttributeType as XmlSchemaDatatype;
			schemaDecimalType = b.AttributeType as XmlSchemaDatatype;
			schemaAnyType = e.ElementType as XmlSchemaComplexType;
#endif
		}

		#region Fields

		DataSet dataset;
		bool forDataSet;
		XmlSchema schema;

		ArrayList relations = new ArrayList ();
		Hashtable reservedConstraints = new Hashtable ();

		// such element that has an attribute msdata:IsDataSet="true"
		XmlSchemaElement datasetElement;

		// choice alternatives in the "dataset element"
		ArrayList topLevelElements = new ArrayList ();

		// import target elements
		ArrayList targetElements = new ArrayList ();

		TableStructure currentTable;
	
#if NET_2_0
		// TODO: Do we need a collection here?
		TableAdapterSchemaInfo currentAdapter;
#endif
		#endregion

		// .ctor()

		public XmlSchemaDataImporter (DataSet dataset, XmlReader reader, bool forDataSet)
		{
			this.dataset = dataset;
			this.forDataSet = forDataSet;
			dataset.DataSetName = "NewDataSet"; // Initialize always
			schema = XmlSchema.Read (reader, null);
			if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "schema" && reader.NamespaceURI == XmlSchema.Namespace)
				reader.ReadEndElement ();
			schema.Compile (null);
		}

#if NET_2_0
		// properties
		internal TableAdapterSchemaInfo CurrentAdapter {
			get { return currentAdapter; }
		}
#endif
		
		// methods

		public void Process ()
		{
			if (schema.Id != null)
				dataset.DataSetName = schema.Id; // default. Overridable by "DataSet element"
			dataset.Namespace = schema.TargetNamespace;

			// Find dataset element
			foreach (XmlSchemaObject obj in schema.Items) {
				XmlSchemaElement el = obj as XmlSchemaElement;
				if (el != null) {
					if (datasetElement == null &&
						IsDataSetElement (el))
						datasetElement = el;
#if NET_2_0
					if (el.ElementSchemaType is XmlSchemaComplexType &&
					    el.ElementSchemaType != schemaAnyType)
#else
					if (el.ElementType is XmlSchemaComplexType &&
					    el.ElementType != schemaAnyType)
#endif
						targetElements.Add (obj);
				}
			}

			// make reservation of identity constraints
			if (datasetElement != null) {
				// keys/uniques.
				foreach (XmlSchemaObject obj in datasetElement.Constraints)
					if (! (obj is XmlSchemaKeyref))
						ReserveSelfIdentity ((XmlSchemaIdentityConstraint) obj);
				// keyrefs.
				foreach (XmlSchemaObject obj in datasetElement.Constraints)
					if (obj is XmlSchemaKeyref)
						ReserveRelationIdentity (datasetElement, (XmlSchemaKeyref) obj);
			}

			foreach (XmlSchemaObject obj in schema.Items) {
				if (obj is XmlSchemaElement) {
					XmlSchemaElement el = obj as XmlSchemaElement;
#if NET_2_0
					if (el.ElementSchemaType is XmlSchemaComplexType &&
					    el.ElementSchemaType != schemaAnyType)
#else
					if (el.ElementType is XmlSchemaComplexType &&
					    el.ElementType != schemaAnyType)
#endif
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

			if (datasetElement != null) {
				// Handle constraints in the DataSet element. First keys.
				foreach (XmlSchemaObject obj in datasetElement.Constraints)
					if (! (obj is XmlSchemaKeyref))
						ProcessSelfIdentity (reservedConstraints [obj] as ConstraintStructure);
				// Then keyrefs.
				foreach (XmlSchemaObject obj in datasetElement.Constraints)
					if (obj is XmlSchemaKeyref)
						ProcessRelationIdentity (datasetElement, reservedConstraints [obj] as ConstraintStructure);
			}

			foreach (RelationStructure rs in this.relations)
				dataset.Relations.Add (GenerateRelationship (rs));
		}

		private bool IsDataSetElement (XmlSchemaElement el)
		{
			if (el.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in el.UnhandledAttributes) {
					if (attr.LocalName == "IsDataSet" &&
						attr.NamespaceURI == XmlConstants.MsdataNamespace) {
						switch (attr.Value) {
						case "true": // case sensitive
							return true;
						case "false":
							break;
						default:
							throw new DataException (String.Format ("Value {0} is invalid for attribute 'IsDataSet'.", attr.Value));
						}
					}
				}
			}

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
				XmlSchemaComplexType ct = null;
#if NET_2_0
				ct = el.ElementSchemaType as XmlSchemaComplexType;
#else
				ct = el.ElementType as XmlSchemaComplexType;
#endif
				if (ct == null || ct == schemaAnyType)
					return true; // column element
				if (ct.AttributeUses.Count > 0)
					return false; // table element
				if (ct.ContentType == XmlSchemaContentType.TextOnly)
					return true; // column element
				else
					return false; // table element
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

			// If type is not complex, just skip this element
#if NET_2_0
			if (! (el.ElementSchemaType is XmlSchemaComplexType && el.ElementSchemaType != schemaAnyType))
#else
			if (! (el.ElementType is XmlSchemaComplexType && el.ElementType != schemaAnyType))
#endif
				return;

			if (IsDataSetElement (el)) {
				ProcessDataSetElement (el);
				return;
			}
			else
				dataset.Locale = CultureInfo.CurrentCulture;

			// Register as a top-level element
			topLevelElements.Add (el);
			// Create DataTable for this element
			ProcessDataTableElement (el);
		}

		private void ProcessDataSetElement (XmlSchemaElement el)
		{
			dataset.DataSetName = el.Name;
			this.datasetElement = el;

			// Search for locale attributes
			bool useCurrent = false;
			if (el.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in el.UnhandledAttributes) {
#if NET_2_0
					if (attr.LocalName == "UseCurrentLocale" &&
						attr.NamespaceURI == XmlConstants.MsdataNamespace)
						useCurrent = true;
#endif

					if (attr.NamespaceURI == XmlConstants.MspropNamespace && 
					    !dataset.ExtendedProperties.ContainsKey(attr.Name))
					{
						dataset.ExtendedProperties.Add (attr.Name, attr.Value);
						continue;
					}
					
					if (attr.LocalName == "Locale" &&
						attr.NamespaceURI == XmlConstants.MsdataNamespace) {
						CultureInfo ci = new CultureInfo (attr.Value);
						dataset.Locale = ci;
					}
				}
			}
#if NET_2_0
			if (!useCurrent && !dataset.LocaleSpecified) // then set current culture instance _explicitly_
				dataset.Locale = CultureInfo.CurrentCulture;
#endif

			// Process content type particle (and create DataTable)
			XmlSchemaComplexType ct = null;
#if NET_2_0
			ct = el.ElementSchemaType as XmlSchemaComplexType;
#else
			ct = el.ElementType as XmlSchemaComplexType;
#endif
			XmlSchemaParticle p = ct != null ? ct.ContentTypeParticle : null;
			if (p != null)
				HandleDataSetContentTypeParticle (p);
		}

		private void HandleDataSetContentTypeParticle (XmlSchemaParticle p)
		{
			XmlSchemaElement el = p as XmlSchemaElement;
			if (el != null) {
#if NET_2_0
				if (el.ElementSchemaType is XmlSchemaComplexType && el.RefName != el.QualifiedName)
#else
				if (el.ElementType is XmlSchemaComplexType && el.RefName != el.QualifiedName)
#endif
					ProcessDataTableElement (el);
			}
			else if (p is XmlSchemaGroupBase) {
				foreach (XmlSchemaParticle pc in ((XmlSchemaGroupBase) p).Items)
					HandleDataSetContentTypeParticle (pc);
			}
		}

		private void ProcessDataTableElement (XmlSchemaElement el)
		{
			string tableName = XmlHelper.Decode (el.QualifiedName.Name);
			// If it is already registered, just ignore.
			if (dataset.Tables.Contains (tableName))
				return;

			DataTable table = new DataTable (tableName);
			table.Namespace = el.QualifiedName.Namespace;
			TableStructure oldTable = currentTable;
			currentTable = new TableStructure (table);

			dataset.Tables.Add (table);

			// Find Locale
			if (el.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in el.UnhandledAttributes) {

					if (attr.NamespaceURI == XmlConstants.MspropNamespace)
					{
						table.ExtendedProperties.Add (attr.Name, attr.Value);
						continue;
					}

					if (attr.LocalName == "Locale" &&
						attr.NamespaceURI == XmlConstants.MsdataNamespace)
						table.Locale = new CultureInfo (attr.Value);
				}
			}

			// Handle complex type (NOTE: It is (or should be)
			// impossible the type is other than complex type).
			XmlSchemaComplexType ct = null;
#if NET_2_0
			ct = (XmlSchemaComplexType) el.ElementSchemaType;
#else
			ct = (XmlSchemaComplexType) el.ElementType;
#endif

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

			currentTable = oldTable;
		}

		private DataRelation GenerateRelationship (RelationStructure rs)
		{
			DataTable ptab = dataset.Tables [rs.ParentTableName];
			DataTable ctab = dataset.Tables [rs.ChildTableName];

			DataRelation rel ;
			string name = rs.ExplicitName != null ? rs.ExplicitName : XmlHelper.Decode (ptab.TableName) + '_' + XmlHelper.Decode (ctab.TableName);

			// Annotation Relations belonging to a DataSet can contain multiple colnames
			// in parentkey and childkey.
			if (datasetElement != null) {
				String[] pcolnames = rs.ParentColumnName.Split (null);
				String[] ccolnames = rs.ChildColumnName.Split (null);

				DataColumn[] pcol = new DataColumn [pcolnames.Length];
				for (int i=0; i<pcol.Length; ++i)
					pcol [i] = ptab.Columns [XmlHelper.Decode (pcolnames [i])];

				DataColumn[] ccol = new DataColumn [ccolnames.Length];
				for (int i=0; i < ccol.Length; ++i) {
					ccol [i] = ctab.Columns [XmlHelper.Decode (ccolnames [i])];
					if (ccol [i] == null)
						ccol [i] = CreateChildColumn (pcol [i], ctab);
				}
				rel = new DataRelation (name, pcol, ccol, rs.CreateConstraint);
			} else {
				DataColumn pcol = ptab.Columns [XmlHelper.Decode (rs.ParentColumnName)];
				DataColumn ccol = ctab.Columns [XmlHelper.Decode (rs.ChildColumnName)];
				if (ccol == null) 
					ccol = CreateChildColumn (pcol, ctab);
				rel = new DataRelation (name, pcol, ccol, rs.CreateConstraint);
			}
			rel.Nested = rs.IsNested;
			if (rs.CreateConstraint)
				rel.ParentTable.PrimaryKey = rel.ParentColumns;
			return rel;
		}

		private DataColumn CreateChildColumn (DataColumn parentColumn, DataTable childTable)
		{
			DataColumn col = childTable.Columns.Add (parentColumn.ColumnName, 
								parentColumn.DataType);
			col.Namespace = String.Empty;
			col.ColumnMapping = MappingType.Hidden;
			return col;
		}

		private void ImportColumnGroupBase (XmlSchemaElement parent, XmlSchemaGroupBase gb)
		{
			foreach (XmlSchemaParticle p in gb.Items) {
				XmlSchemaElement el = p as XmlSchemaElement;
				if (el != null)
					ImportColumnElement (parent, el);
				else if (p is XmlSchemaGroupBase)
					ImportColumnGroupBase (parent, (XmlSchemaGroupBase) p);
				// otherwise p is xs:any
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
			XmlSchemaDatatype dt = null;
#if NET_2_0
			dt = GetSchemaPrimitiveType (((XmlSchemaSimpleType) attr.AttributeSchemaType).Datatype);
#else
			dt = GetSchemaPrimitiveType (attr.AttributeType);
#endif
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

#if NET_2_0
			FillFacet (col, attr.AttributeSchemaType as XmlSchemaSimpleType);
#else
			FillFacet (col, attr.AttributeType as XmlSchemaSimpleType);
#endif

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

#if NET_2_0
			if (el.ElementSchemaType is XmlSchemaComplexType && el.ElementSchemaType != schemaAnyType)
#else
			if (el.ElementType is XmlSchemaComplexType && el.ElementType != schemaAnyType)
#endif
				FillDataColumnComplexElement (parent, el, col);
			else if (el.MaxOccurs != 1)
				FillDataColumnRepeatedSimpleElement (parent, el, col);
			else
				FillDataColumnSimpleElement (el, col);
		}

		// common process for element and attribute
		private void ImportColumnMetaInfo (XmlSchemaAnnotated obj, XmlQualifiedName name, DataColumn col)
		{
			if (obj.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in obj.UnhandledAttributes) {
					if (attr.NamespaceURI == XmlConstants.MspropNamespace)
					{
						col.ExtendedProperties.Add (attr.Name, attr.Value);
						continue;
					}

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
					case XmlConstants.AutoIncrementStep:
						col.AutoIncrementStep = int.Parse (attr.Value);
						break;
					case XmlConstants.ReadOnly:
						col.ReadOnly = XmlConvert.ToBoolean (attr.Value);
						break;
					case XmlConstants.Ordinal:
						int ordinal = int.Parse (attr.Value);
						break;
					}
				}
			}
		}

		private void FillDataColumnComplexElement (XmlSchemaElement parent, XmlSchemaElement el, DataColumn col)
		{
			if (targetElements.Contains (el))
				return; // do nothing

			string elName = XmlHelper.Decode (el.QualifiedName.Name);
			if (elName == dataset.DataSetName)
				// Well, why it is ArgumentException :-?
				throw new ArgumentException ("Nested element must not have the same name as DataSet's name.");

			if (el.Annotation != null)
				HandleAnnotations (el.Annotation, true);
			// If xsd:keyref xsd:key for this table exists, then don't add
			// relation here manually.
			else if (!DataSetDefinesKey (elName)) {
				AddParentKeyColumn (parent, el, col);

				RelationStructure rel = new RelationStructure ();
				rel.ParentTableName = XmlHelper.Decode (parent.QualifiedName.Name);
				rel.ChildTableName = elName;
				rel.ParentColumnName = col.ColumnName;
				rel.ChildColumnName = col.ColumnName;
				rel.CreateConstraint = true;
				rel.IsNested = true;
				relations.Add (rel);
			}

			// If the element is not referenced one, the element will be handled later.
			if (el.RefName == XmlQualifiedName.Empty)
				ProcessDataTableElement (el);

		}

		private bool DataSetDefinesKey (string name)
		{
			foreach (ConstraintStructure c in reservedConstraints.Values)
				if (c.TableName == name && (c.IsPrimaryKey || c.IsNested))
					return true;
			return false;
		}

		private void AddParentKeyColumn (XmlSchemaElement parent, XmlSchemaElement el, DataColumn col)
		{
			// check existing primary key
			if (currentTable.Table.PrimaryKey.Length > 0)
				throw new DataException (String.Format ("There is already primary key columns in the table \"{0}\".", currentTable.Table.TableName));

			if (currentTable.PrimaryKey != null) {
				// fill pk column info and return
				col.ColumnName = currentTable.PrimaryKey.ColumnName;
				col.ColumnMapping = currentTable.PrimaryKey.ColumnMapping;
				col.Namespace = currentTable.PrimaryKey.Namespace;
				col.DataType = currentTable.PrimaryKey.DataType;
				col.AutoIncrement = currentTable.PrimaryKey.AutoIncrement;
				col.AllowDBNull = currentTable.PrimaryKey.AllowDBNull;
				
				ImportColumnMetaInfo (el, el.QualifiedName, col);
				return;
			}

			// check name identity
			string name = XmlHelper.Decode (parent.QualifiedName.Name) + "_Id";
			int count = 0;
			while (currentTable.ContainsColumn (name))
				name = String.Format ("{0}_{1}", name, count++);

			col.ColumnName = name;
			col.ColumnMapping = MappingType.Hidden;
			col.Namespace = parent.QualifiedName.Namespace;
			col.DataType = typeof (int);
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

			string elName = XmlHelper.Decode (el.QualifiedName.Name);
			string parentName = XmlHelper.Decode (parent.QualifiedName.Name);

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
#if NET_2_0
			cc2.DataType = ConvertDatatype (GetSchemaPrimitiveType (el.ElementSchemaType));
#else
			cc2.DataType = ConvertDatatype (GetSchemaPrimitiveType (el.ElementType));
#endif

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
			col.ColumnName = XmlHelper.Decode (el.QualifiedName.Name);
			col.Namespace = el.QualifiedName.Namespace;
			col.ColumnMapping = MappingType.Element;
#if NET_2_0
			col.DataType = ConvertDatatype (GetSchemaPrimitiveType (el.ElementSchemaType));
			FillFacet (col, el.ElementSchemaType as XmlSchemaSimpleType);
#else
			col.DataType = ConvertDatatype (GetSchemaPrimitiveType (el.ElementType));
			FillFacet (col, el.ElementType as XmlSchemaSimpleType);
#endif

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

			return XmlHelper.Decode (tableName);
		}

		private void ReserveSelfIdentity (XmlSchemaIdentityConstraint ic)
		{
			string tableName = GetSelectorTarget (ic.Selector.XPath);

			string [] cols = new string [ic.Fields.Count];
			bool [] isAttrSpec = new bool [cols.Length];

			int i = 0;
			foreach (XmlSchemaXPath Field in ic.Fields) {
				string colName = Field.XPath;
				bool isAttr = colName.Length > 0 && colName [0] == '@';
				int index = colName.LastIndexOf (':');
				if (index > 0)
					colName = colName.Substring (index + 1);
				else if (isAttr)
					colName = colName.Substring (1);

				colName = XmlHelper.Decode (colName);
				cols [i] = colName;
				isAttrSpec [i] = isAttr;
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
			reservedConstraints.Add (ic,
				new ConstraintStructure (tableName, cols,
					isAttrSpec, constraintName, isPK, null, false, false));
		}

		private void ProcessSelfIdentity (ConstraintStructure c)
		{
			// Basic concept came from XmlSchemaMapper.cs

			string tableName = c.TableName;
			
			DataTable dt = dataset.Tables [tableName];
			if (dt == null) {
				if (forDataSet)
					throw new DataException (String.Format ("Invalid XPath selection inside selector. Cannot find: {0}", tableName));
				else
					// nonexistent table name. .NET ignores it for DataTable.ReadXmlSchema().
					return;
			}

			DataColumn [] cols = new DataColumn [c.Columns.Length];
			for (int i = 0; i < cols.Length; i++) {
				string colName = c.Columns [i];
				bool isAttr = c.IsAttribute [i];
				DataColumn col = dt.Columns [colName];
				if (col == null)
					throw new DataException (String.Format ("Invalid XPath selection inside field. Cannot find: {0}", tableName));
				if (isAttr && col.ColumnMapping != MappingType.Attribute)
					throw new DataException ("The XPath specified attribute field, but mapping type is not attribute.");
				if (!isAttr && col.ColumnMapping != MappingType.Element)
					throw new DataException ("The XPath specified simple element field, but mapping type is not simple element.");

				cols [i] = dt.Columns [colName];
			}
			
			bool isPK = c.IsPrimaryKey;
			string constraintName = c.ConstraintName;
			dt.Constraints.Add (new UniqueConstraint (
				constraintName, cols, isPK));
		}

		private void ReserveRelationIdentity (XmlSchemaElement element, XmlSchemaKeyref keyref)
		{
			// Basic concept came from XmlSchemaMapper.cs

			string tableName = GetSelectorTarget (keyref.Selector.XPath);

			string [] cols = new string [keyref.Fields.Count];
			bool [] isAttrSpec = new bool [cols.Length];
			int i = 0;
			foreach (XmlSchemaXPath Field in keyref.Fields) {
				string colName = Field.XPath;
				bool isAttr = colName.Length > 0 && colName [0] == '@';
				int index = colName.LastIndexOf (':');
				if (index > 0)
					colName = colName.Substring (index + 1);
				else if (isAttr)
					colName = colName.Substring (1);

				colName = XmlHelper.Decode (colName);
				cols [i] = colName;
				isAttrSpec [i] = isAttr;
				i++;
			}
			string constraintName = keyref.Name;
			bool isNested = false;
			bool isConstraintOnly = false;
			if (keyref.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in keyref.UnhandledAttributes) {
					if (attr.NamespaceURI != XmlConstants.MsdataNamespace)
						continue;
					switch (attr.LocalName) {
					case XmlConstants.ConstraintName:
						constraintName = attr.Value;
						break;
					case XmlConstants.IsNested:
						if (attr.Value == "true")
							isNested = true;
						break;
					case XmlConstants.ConstraintOnly:
						if (attr.Value == "true")
							isConstraintOnly = true;
						break;
					}
				}
			}

			reservedConstraints.Add (keyref, new ConstraintStructure (
				tableName, cols, isAttrSpec, constraintName,
				false, keyref.Refer.Name, isNested, isConstraintOnly));
		}

		private void ProcessRelationIdentity (XmlSchemaElement element, ConstraintStructure c)
		{
			// Basic concept came from XmlSchemaMapper.cs

			string tableName = c.TableName;

			DataColumn [] cols;
			DataTable dt = dataset.Tables [tableName];
			if (dt == null)
				throw new DataException (String.Format ("Invalid XPath selection inside selector. Cannot find: {0}", tableName));

			cols = new DataColumn [c.Columns.Length];
			for (int i = 0; i < cols.Length; i++) {
				string colName = c.Columns [i];
				bool isAttr = c.IsAttribute [i];
				DataColumn col = dt.Columns [colName];
				if (isAttr && col.ColumnMapping != MappingType.Attribute)
					throw new DataException ("The XPath specified attribute field, but mapping type is not attribute.");
				if (!isAttr && col.ColumnMapping != MappingType.Element)
					throw new DataException ("The XPath specified simple element field, but mapping type is not simple element.");
				cols [i] = col;
			}
			string name = c.ReferName;
			// get the unique constraint for the releation
			UniqueConstraint uniq = FindConstraint (name, element);
			// generate the FK.
			ForeignKeyConstraint fkc = new ForeignKeyConstraint(c.ConstraintName, uniq.Columns, cols);
			dt.Constraints.Add (fkc);

			if (!c.IsConstraintOnly) {
				// generate the relation.
				DataRelation rel = new DataRelation (c.ConstraintName, uniq.Columns, cols, true);
				rel.Nested = c.IsNested;
				rel.SetParentKeyConstraint (uniq);
				rel.SetChildKeyConstraint (fkc);

				dataset.Relations.Add (rel);
			}
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
						
						// #325464 debugging
						//Console.WriteLine ("Name: " + el.LocalName + " NS: " + el.NamespaceURI + " Const: " + XmlConstants.MsdataNamespace);
						if (el != null && el.LocalName == "Relationship" && el.NamespaceURI == XmlConstants.MsdataNamespace)
							HandleRelationshipAnnotation (el, nested);
#if NET_2_0
						if (el != null && el.LocalName == "DataSource" && el.NamespaceURI == XmlConstants.MsdatasourceNamespace)
							HandleDataSourceAnnotation (el, nested);
#endif
					}
				}
			}
		}

#if NET_2_0
		private void HandleDataSourceAnnotation (XmlElement el, bool nested)
		{
			// Handle: Connections and Tables
			// For Tables: extract the provider information from connection and use
			// the corresponding providerfactory to create the adapter and et al objects 
			// and populate them
			
			// #325464 debugging
			//Console.WriteLine ("In HandleDataSourceAnnotation... ");
			string providerName = null;
			string connString = null;
			DbProviderFactory provider = null;
			XmlElement e, tablesElement = null, firstChild;
			
			foreach (XmlNode n in el.ChildNodes) {
				e = n as XmlElement;
				
				if (e == null)
					continue;
				
#if !MOBILE
				if (e.LocalName == "Connections" && (firstChild = e.FirstChild as XmlElement) != null) {
					providerName = firstChild.GetAttribute ("Provider");
					connString = firstChild.GetAttribute ("AppSettingsPropertyName");
					provider = DbProviderFactories.GetFactory (providerName);
					continue;
				}
#endif
				// #325464 debugging
				//Console.WriteLine ("ProviderName: " + providerName + "Connstr: " + connString);
				
				if (e.LocalName == "Tables")
					tablesElement = e;
			}
				
			if (tablesElement != null && provider != null) {
				foreach (XmlNode node in tablesElement.ChildNodes) {
					ProcessTableAdapter (node as XmlElement, provider, connString);
				}
			}
		}
		
		private void ProcessTableAdapter (XmlElement el, DbProviderFactory provider, string connStr)
		{
			XmlElement e;
			string datasetTableName = null;
			
			if (el == null)
				return;
			
			// #325464 debugging
			//Console.WriteLine ("in ProcessTableAdapters...");
			currentAdapter = new TableAdapterSchemaInfo (provider); 
			currentAdapter.ConnectionString = connStr;
			
			//Console.WriteLine ("Provider: {0}, connection: {1}, adapter: {2}", 
			//                   provider, currentAdapter.Connection, currentAdapter.Adapter);
			currentAdapter.BaseClass = el.GetAttribute ("BaseClass");
			datasetTableName = el.GetAttribute ("Name");
			currentAdapter.Name = el.GetAttribute ("GeneratorDataComponentClassName");
			
			if (String.IsNullOrEmpty (currentAdapter.Name))
				currentAdapter.Name = el.GetAttribute ("DataAccessorName");

			//Console.WriteLine ("Name: "+currentAdapter.Name);
			foreach (XmlNode n in el.ChildNodes) {
				e = n as XmlElement;
				
				//Console.WriteLine ("Children of Tables: "+e.LocalName);
				if (e == null)
					continue;
				
				switch (e.LocalName) {
					case "MainSource": 
					case "Sources": 
						foreach (XmlNode msn in e.ChildNodes)
							ProcessDbSource (msn as XmlElement);
						break;
					
					case "Mappings":
						DataTableMapping tableMapping = new DataTableMapping ();
						tableMapping.SourceTable = "Table";
						tableMapping.DataSetTable = datasetTableName;
						
						foreach (XmlNode mps in e.ChildNodes)
							ProcessColumnMapping (mps as XmlElement, tableMapping);
						
						currentAdapter.Adapter.TableMappings.Add (tableMapping);
						break;						
				}
			}
		}
		
		private void ProcessDbSource (XmlElement el)
		{
			string tmp = null;
			XmlElement e;
			
			if (el == null)
				return;
			
			//Console.WriteLine ("ProcessDbSources: "+el.LocalName);

			tmp = el.GetAttribute ("GenerateShortCommands");
			//Console.WriteLine ("GenerateShortCommands: {0}", tmp);
			if (!String.IsNullOrEmpty (tmp))
				currentAdapter.ShortCommands = Convert.ToBoolean (tmp);
		
			DbCommandInfo cmdInfo = new DbCommandInfo ();
			tmp = el.GetAttribute ("GenerateMethods");
			if (!String.IsNullOrEmpty (tmp)) {
				DbSourceMethodInfo mthdInfo = null;
				
				switch ((GenerateMethodsType) Enum.Parse (typeof (GenerateMethodsType), tmp)) {
				case GenerateMethodsType.Get:
					mthdInfo = new DbSourceMethodInfo ();
					mthdInfo.Name = el.GetAttribute ("GetMethodName");
					mthdInfo.Modifier = el.GetAttribute ("GetMethodModifier");
					if (String.IsNullOrEmpty (mthdInfo.Modifier))
						mthdInfo.Modifier = "Public";
					mthdInfo.ScalarCallRetval = el.GetAttribute ("ScalarCallRetval");
					mthdInfo.QueryType = el.GetAttribute ("QueryType");
					mthdInfo.MethodType = GenerateMethodsType.Get;
					cmdInfo.Methods = new DbSourceMethodInfo [1];
					cmdInfo.Methods[0] = mthdInfo;
					break;
					
				case GenerateMethodsType.Fill:
					mthdInfo = new DbSourceMethodInfo ();
					mthdInfo.Name = el.GetAttribute ("FillMethodName");
					mthdInfo.Modifier = el.GetAttribute ("FillMethodModifier");
					if (String.IsNullOrEmpty (mthdInfo.Modifier))
						mthdInfo.Modifier = "Public";
					mthdInfo.ScalarCallRetval = null;
					mthdInfo.QueryType = null;
					mthdInfo.MethodType = GenerateMethodsType.Fill;
					cmdInfo.Methods = new DbSourceMethodInfo [1];
					cmdInfo.Methods[0] = mthdInfo;
					break;
					
				case GenerateMethodsType.Both:
					mthdInfo = new DbSourceMethodInfo ();
					// Get
					mthdInfo.Name = el.GetAttribute ("GetMethodName");
					mthdInfo.Modifier = el.GetAttribute ("GetMethodModifier");
					if (String.IsNullOrEmpty (mthdInfo.Modifier))
						mthdInfo.Modifier = "Public";
					mthdInfo.ScalarCallRetval = el.GetAttribute ("ScalarCallRetval");
					mthdInfo.QueryType = el.GetAttribute ("QueryType");
					mthdInfo.MethodType = GenerateMethodsType.Get;
					cmdInfo.Methods = new DbSourceMethodInfo [2];
					cmdInfo.Methods[0] = mthdInfo;
					
					// Fill
					mthdInfo = new DbSourceMethodInfo ();
					mthdInfo.Name = el.GetAttribute ("FillMethodName");
					mthdInfo.Modifier = el.GetAttribute ("FillMethodModifier");
					if (String.IsNullOrEmpty (mthdInfo.Modifier))
						mthdInfo.Modifier = "Public";
					mthdInfo.ScalarCallRetval = null;
					mthdInfo.QueryType = null;
					mthdInfo.MethodType = GenerateMethodsType.Fill;
					cmdInfo.Methods[1] = mthdInfo;
					break;
				}
			} else {
				// no Get or Fill methods - non <MainSource> sources
				DbSourceMethodInfo mthdInfo = new DbSourceMethodInfo ();
				mthdInfo.Name = el.GetAttribute ("Name");
				mthdInfo.Modifier = el.GetAttribute ("Modifier");
				if (String.IsNullOrEmpty (mthdInfo.Modifier))
					mthdInfo.Modifier = "Public";
				mthdInfo.ScalarCallRetval = el.GetAttribute ("ScalarCallRetval");
				mthdInfo.QueryType = el.GetAttribute ("QueryType");
				mthdInfo.MethodType = GenerateMethodsType.None;
				// Add MethodInfo to DbCommandInfo
				cmdInfo.Methods = new DbSourceMethodInfo [1];
				cmdInfo.Methods[0] = mthdInfo;
			}
			
			foreach (XmlNode n in el.ChildNodes) {
				e = n as XmlElement;
				
				if (e == null) 
					continue;
				
				switch (e.LocalName) {
					case "SelectCommand": 
						cmdInfo.Command = ProcessDbCommand (e.FirstChild as XmlElement);
						currentAdapter.Commands.Add (cmdInfo);
						break;
					case "InsertCommand": 
						currentAdapter.Adapter.InsertCommand = ProcessDbCommand (e.FirstChild as XmlElement);
						break;
					case "UpdateCommand": 
						currentAdapter.Adapter.UpdateCommand = ProcessDbCommand (e.FirstChild as XmlElement);
						break;
					case "DeleteCommand": 
						currentAdapter.Adapter.DeleteCommand = ProcessDbCommand (e.FirstChild as XmlElement);
						break;
				}
			}
		}
		
		private DbCommand ProcessDbCommand (XmlElement el)
		{
			XmlElement e;
			//Console.WriteLine (el.LocalName);
			string cmdText = null;
			string cmdType = null;
			ArrayList parameters = null;
			
			if (el == null)
				return null;
			
			cmdType = el.GetAttribute ("CommandType");
			foreach (XmlNode n in el.ChildNodes) {
				e = n as XmlElement;
				if (e != null && e.LocalName == "CommandText")
					cmdText = e.InnerText;
				else if (e != null && e.LocalName == "Parameters" && !e.IsEmpty)
					parameters = ProcessDbParameters (e);
			}
			
			DbCommand cmd = currentAdapter.Provider.CreateCommand ();
			cmd.CommandText = cmdText;
			if (cmdType == "StoredProcedure")
				cmd.CommandType = CommandType.StoredProcedure;
			else
				cmd.CommandType = CommandType.Text;

			if (parameters != null)
				cmd.Parameters.AddRange (parameters.ToArray ());
			
			//Console.WriteLine ("Parameters count: {0}", cmd.Parameters.Count);
			return cmd;
		}
		
		private ArrayList ProcessDbParameters (XmlElement el)
		{
			//Console.WriteLine ("ProcessDbParameters: "+el.LocalName);
			string tmp = null;
			XmlElement e;
			DbParameter param = null;
			ArrayList parameters = new ArrayList ();
			
			if (el == null)
				return parameters;
			
			foreach (XmlNode n in el.ChildNodes) {
				e = n as XmlElement;
				
				if (e == null)
					continue;
				param = currentAdapter.Provider.CreateParameter ();

				tmp = e.GetAttribute ("AllowDbNull");
				if (!String.IsNullOrEmpty (tmp))
					param.IsNullable = Convert.ToBoolean (tmp);
				
				param.ParameterName = e.GetAttribute ("ParameterName");
				tmp = e.GetAttribute ("ProviderType");
				if (!String.IsNullOrEmpty (tmp))
					tmp = e.GetAttribute ("DbType");
				param.FrameworkDbType = tmp;
				
				tmp = e.GetAttribute ("Direction");
				param.Direction = (ParameterDirection) Enum.Parse (typeof (ParameterDirection), tmp);
				
				((IDbDataParameter)param).Precision = Convert.ToByte (e.GetAttribute ("Precision"));
				((IDbDataParameter)param).Scale = Convert.ToByte (e.GetAttribute ("Scale"));
				param.Size = Convert.ToInt32 (e.GetAttribute ("Size"));
				param.SourceColumn = e.GetAttribute ("SourceColumn");
				
				tmp = e.GetAttribute ("SourceColumnNullMapping");
				if (!String.IsNullOrEmpty (tmp))
					param.SourceColumnNullMapping = Convert.ToBoolean (tmp);
				
				tmp = e.GetAttribute ("SourceVersion");
				param.SourceVersion = (DataRowVersion) Enum.Parse (typeof (DataRowVersion), tmp);				
				parameters.Add (param);
			}
			
			return parameters;
		}

		private void ProcessColumnMapping (XmlElement el, DataTableMapping tableMapping)
		{
			if (el == null)
				return;
			
			tableMapping.ColumnMappings.Add (el.GetAttribute ("SourceColumn"), 
			                                 el.GetAttribute ("DataSetColumn"));
		}
		
#endif
		
		private void HandleRelationshipAnnotation (XmlElement el, bool nested)
		{
			string name = el.GetAttribute ("name");
			string ptn = el.GetAttribute ("parent", XmlConstants.MsdataNamespace);
			string ctn = el.GetAttribute ("child", XmlConstants.MsdataNamespace);
			string pkn = el.GetAttribute ("parentkey", XmlConstants.MsdataNamespace);
			string fkn = el.GetAttribute ("childkey", XmlConstants.MsdataNamespace);

			RelationStructure rel = new RelationStructure ();
			rel.ExplicitName = XmlHelper.Decode (name);
			rel.ParentTableName = XmlHelper.Decode (ptn);
			rel.ChildTableName = XmlHelper.Decode (ctn);
			// ColumnNames will be decoded wherever they are used as they can
			// contain 'space' separated list of column-names.
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
