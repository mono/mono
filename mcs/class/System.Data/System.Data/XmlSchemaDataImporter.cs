//
// XmlSchemaDataImporter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
//
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
//	(FIXME: Not implemented yet)
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

using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;


namespace System.Data
{
	internal class XmlSchemaDataImporter
	{
		static readonly XmlSchemaDatatype schemaIntegerType;
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
			XmlSchemaElement e = new XmlSchemaElement ();
			e.Name = "bar";
			s.Items.Add (e);
			s.Compile (null);
			schemaIntegerType = a.AttributeType as XmlSchemaDatatype;
			schemaAnyType = e.ElementType as XmlSchemaComplexType;
		}

		DataSet dataset;
		XmlSchema schema;

		Hashtable relationParentColumns = new Hashtable ();
		Hashtable nestedRelationColumns = new Hashtable ();

		// such element that has an attribute msdata:IsDataSet="true"
		XmlSchemaElement datasetElement;

		// choice alternatives in the "dataset element"
		ArrayList topLevelElements = new ArrayList ();

		// import target elements
		ArrayList targetElements = new ArrayList ();

		// The columns and orders which will be added to the context
		// table (See design notes; Because of the ordinal problem)
		// [DataColumn] -> int ordinal
		Hashtable currentOrdinalColumns = new Hashtable ();
		ArrayList currentNonOrdinalColumns = new ArrayList ();
		ArrayList currentColumnNames = new ArrayList ();

		// used to determine if it handles the only one element as DataSet
		int globalElementCount;

		// .ctor()

		public XmlSchemaDataImporter (DataSet dataset, XmlReader reader)
		{
			this.dataset = dataset;
			schema = XmlSchema.Read (reader, null);
			// FIXME: Just XmlSchema.Namespace should work (mcs bug)
			if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "schema" && reader.NamespaceURI == System.Xml.Schema.XmlSchema.Namespace)
				reader.ReadEndElement ();
			schema.Compile (null);
		}

		// types
		struct RelationMapping
		{
			public RelationMapping (XmlSchemaElement el, DataTable table, DataColumn col)
			{
				Element = el;
				Table = table;
				Column = col;
			}

			public XmlSchemaElement Element;
			public DataTable Table;
			public DataColumn Column;
		}

		// properties

		public DataSet DataSet {
			get { return dataset; }
		}

		public XmlSchema XmlSchema {
			get { return schema; }
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
			globalElementCount = targetElements.Count;

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
						ProcessParentKey ((XmlSchemaIdentityConstraint) obj);
				// Then keyrefs.
				foreach (XmlSchemaObject obj in datasetElement.Constraints)
					if (obj is XmlSchemaKeyref)
						ProcessReferenceKey (datasetElement, (XmlSchemaKeyref) obj);
			}
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

			// FIXME: It should be more simple. It is likely to occur when the type was not resulted in a DataTable

			// Read the design notes for detail... it is very complicated.

			if (globalElementCount == 1 && el.SchemaType is XmlSchemaComplexType) {
				XmlSchemaComplexType ct = (XmlSchemaComplexType) el.SchemaType;
				XmlSchemaGroupBase gb = ct.ContentTypeParticle as XmlSchemaGroupBase;
				XmlSchemaElement cpElem = gb != null && gb.Items.Count > 0 ? gb.Items [0] as XmlSchemaElement : null;
				XmlSchemaComplexType ct2 = cpElem != null ? cpElem.ElementType as XmlSchemaComplexType : null;
				// What a complex condition!!!!!!!!!
				if (ct2 != null && ct != schemaAnyType && ct.AttributeUses.Count == 0) {
					ProcessDataSetElement (el);
					return;
				}
			}


			// If type is not complex, just skip this element
			if (! (el.ElementType is XmlSchemaComplexType && el.ElementType != schemaAnyType))
				return;

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
			// If it is already registered, just ignore.
			if (dataset.Tables.Contains (el.QualifiedName.Name))
				return;

			string name = el.QualifiedName.Name;

			currentOrdinalColumns.Clear ();
			currentNonOrdinalColumns.Clear ();
			currentColumnNames.Clear ();
			DataTable contextTable = new DataTable ();
			contextTable.TableName = name;

			dataset.Tables.Add (contextTable);

			// Find Locale
			if (el.UnhandledAttributes != null) {
				foreach (XmlAttribute attr in el.UnhandledAttributes) {
					if (attr.LocalName == "Locale" &&
						attr.NamespaceURI == XmlConstants.MsdataNamespace)
						contextTable.Locale = new CultureInfo (attr.Value);
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
			case XmlSchemaContentType.Mixed:
				// LAMESPEC: When reading from XML Schema, it maps to "_text", while on the data inference, it is mapped to "_Text" (case ignorant).
				string simpleName = el.QualifiedName.Name + "_text";
				DataColumn simple = new DataColumn (simpleName);
				simple.AllowDBNull = (el.MinOccurs == 0);
				simple.ColumnMapping = MappingType.SimpleContent;
				simple.DataType = ConvertDatatype (ct.Datatype);
				currentNonOrdinalColumns.Add (simple);
				currentColumnNames.Add (simpleName);
				break;
			}

			// add columns to the table in specified order 
			// (by msdata:Ordinal attributes)
			SortedList sd = new SortedList ();
			foreach (DictionaryEntry de in currentOrdinalColumns)
				sd.Add (de.Value, de.Key);
			foreach (DictionaryEntry de in sd)
				contextTable.Columns.Add ((DataColumn) de.Value);
			foreach (DataColumn dc in currentNonOrdinalColumns)
				contextTable.Columns.Add (dc);

			HandlePopulatedRelationship (contextTable, el);
		}

		private void HandlePopulatedRelationship (DataTable table, XmlSchemaElement el)
		{
			// If there is a parent column, create DataRelation and pk
			DataColumn col = relationParentColumns [el] as DataColumn;
			if (col != null) {
				if (col.Table.PrimaryKey.Length > 0)
					throw new DataException (String.Format ("There is already primary key columns in the table \"{0}\".", table.TableName));
				col.Table.PrimaryKey = new DataColumn [] {col};

				DataColumn child = new DataColumn ();
				child.ColumnName = col.ColumnName;
				child.Namespace = String.Empty; // don't copy
				child.ColumnMapping = MappingType.Hidden;
				child.DataType = col.DataType;
				table.Columns.Add (child);

				DataRelation rel = new DataRelation (col.Table.TableName + '_' + child.Table.TableName, col, child);
				if (nestedRelationColumns.ContainsValue (col))
					rel.Nested = true;
				dataset.Relations.Add (rel);
			}
		}

		/*
		private void HandleAttributes (XmlSchemaObjectCollection atts)
		{
			foreach (XmlSchemaObject attrObj in atts) {
				XmlSchemaAttribute attr = attrObj as XmlSchemaAttribute;
				if (attr != null)
					ImportColumnAttribute (attr);
				else
					HandleAttributes (((XmlSchemaAttributeGroup) attrObj).Attributes);
			}
		}
		*/

		private void ImportColumnGroupBase (XmlSchemaElement parent, XmlSchemaGroupBase gb)
		{
			foreach (XmlSchemaParticle p in gb.Items) {
				XmlSchemaElement el = p as XmlSchemaElement;
				if (el != null)
					ImportColumnElement (parent, el);
				else
					ImportColumnGroupBase (parent, (XmlSchemaGroupBase) gb);
			}
		}

		private XmlSchemaDatatype GetSchemaPrimitiveType (object type)
		{
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
			ImportColumnCommon (attr, attr.QualifiedName, col);
		}

		private void ImportColumnElement (XmlSchemaElement parent, XmlSchemaElement el)
		{
			// FIXME: element nest check

			DataColumn col = new DataColumn ();
			col.DefaultValue = GetElementDefaultValue (el);
			col.AllowDBNull = (el.MinOccurs == 0);

			// FIXME: need to handle array item for maxOccurs > 1
			if (el.ElementType is XmlSchemaComplexType && el.ElementType != schemaAnyType) {
				// import new table and set this column as reference.
				FillDataColumnComplexElement (parent, el, col);
				// If the element is not referenced one, the element will be handled later.
				if (el.RefName == XmlQualifiedName.Empty)
					targetElements.Add (el);
			}
			else
				FillDataColumnSimpleElement (el, col);

			// Call this method after filling the name
			ImportColumnCommon (el, el.QualifiedName, col);
		}

		// common process for element and attribute
		private void ImportColumnCommon (XmlSchemaAnnotated obj, XmlQualifiedName name, DataColumn col)
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
					case XmlConstants.Ordinal:
						ordinal = int.Parse (attr.Value);
						break;
					}
				}
			}
			if (ordinal < 0)
				currentNonOrdinalColumns.Add (col);
			else
				currentOrdinalColumns.Add (col, ordinal);
			currentColumnNames.Add (col.ColumnName);
		}

		[MonoTODO]
		private void FillDataColumnComplexElement (XmlSchemaElement parent, XmlSchemaElement el, DataColumn col)
		{
			if (targetElements.Contains (el))
				return; // do nothing

			FillUniqueKeyColumn (parent, el, col);

			// For nested relations, DataRelation.Nested will be filled later
			if (el.SchemaType != el.ElementType)
				nestedRelationColumns [el] = col;

			// FIXME: Handle "relationship" annotations
//			if (el.Annotation != null)
//				HandleAnnotations (el.Annotation, true);

			relationParentColumns.Add (el, col);
		}

		private void FillUniqueKeyColumn (XmlSchemaElement parent, XmlSchemaElement el, DataColumn col)
		{
			// check name identity
			string name = parent.QualifiedName.Name + "_Id";
			if (currentColumnNames.Contains (name))
				throw new DataException (String.Format ("There is already a column that has the same name: {0}", name));

			col.ColumnName = name;
			col.ColumnMapping = MappingType.Hidden;
			col.Namespace = parent.QualifiedName.Namespace;
			col.DataType = typeof (int);
			col.Unique = true;
			col.AutoIncrement = true;
			col.AllowDBNull = false;
		}

		private void FillDataColumnRepeatedSimpleElement (XmlSchemaElement parent, XmlSchemaElement el, DataColumn col)
		{
			if (targetElements.Contains (el))
				return; // do nothing

			FillUniqueKeyColumn (parent, el, col);
			// FIXME: confirm.
			// Those components will be handled later.
			targetElements.Add (el);

			DataTable dt = new DataTable ();
			dt.TableName = parent.QualifiedName.Name + "_" + el.QualifiedName.Name;
			DataColumn cc = new DataColumn ();
			cc.ColumnName = el.QualifiedName.Name;
			cc.Namespace = dataset.Namespace;
			cc.ColumnMapping = MappingType.Hidden;
			cc.DataType = this.ConvertDatatype (GetSchemaPrimitiveType (el.ElementType));
		}

		private void FillDataColumnSimpleElement (XmlSchemaElement el, DataColumn col)
		{
			col.ColumnName = el.QualifiedName.Name;
			col.Namespace = el.QualifiedName.Namespace;
			col.ColumnMapping = MappingType.Element;
			XmlSchemaDatatype dt = el.ElementType as XmlSchemaDatatype;
			if (dt == null && el.ElementType != null) {
				XmlSchemaSimpleType st = el.ElementType as XmlSchemaSimpleType;
				dt = st != null ? st.Datatype : null;
			}
			col.DataType = ConvertDatatype (dt);
			FillFacet (col, el.ElementType as XmlSchemaSimpleType);
		}

		private void FillFacet (DataColumn col, XmlSchemaSimpleType st)
		{
			if (st == null || st.Content == null)
				return;

			// Handle restriction facets
			// FIXME: how to handle list and union??

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
			else if (dt == schemaIntegerType)
				// LAMESPEC: MSDN documentation says it is based 
				// on ValueType. However, in the System.Xml.Schema
				// context, xs:integer is mapped to Decimal, while
				// in DataSet context it is mapped to Int64.
				return typeof (long);
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

			return tableName;
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
				// FIXME: attribute XPath
				int index = colName.LastIndexOf (':');
				if (index > 0)
					colName = colName.Substring (index + 1);

				DataColumn col = dt.Columns [colName];
				if (col == null)
					throw new DataException (String.Format ("Invalid XPath selection inside field. Cannot find: {0}", tableName));

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
				// FIXME: attribute XPath
				string colName = Field.XPath;
				int index = colName.LastIndexOf (':');
				if (index != -1)
					colName = colName.Substring (index + 1);

				cols [i] = dt.Columns [colName];
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
			string ptn = el.GetAttribute ("parent", XmlConstants.MsdataNamespace);
			string ctn = el.GetAttribute ("child", XmlConstants.MsdataNamespace);
			string pkn = el.GetAttribute ("parentkey", XmlConstants.MsdataNamespace);
			string fkn = el.GetAttribute ("childkey", XmlConstants.MsdataNamespace);

			DataTable pt = dataset.Tables [ptn];
			DataTable ct = dataset.Tables [ctn];
			DataColumn pc = pt.Columns [pkn];
			DataColumn cc = ct.Columns [fkn];

			DataRelation rel = new DataRelation (el.GetAttribute ("name"), pc, cc, false);
			rel.Nested = nested;
			dataset.Relations.Add (rel);
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
