//
// mcs/class/System.Data/System.Data/XmlSchemaMapper.cs
//
// Purpose: Maps XmlSchema to DataSet 
//
// class: XmlSchemaMapper
// assembly: System.Data.dll
// namespace: System.Data
//
// Author:
//     Ville Palo <vi64pa@koti.soon.fi>
//
// (C) 2002 Ville Palo
//
//

using System;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Collections;
using System.Globalization;

namespace System.Data {

        internal class XmlSchemaMapper
	{	
	        #region Fields

		private DataSet DSet;
		private DataTable table;
		enum ElementType {ELEMENT_UNDEFINED, ELEMENT_TABLE, ELEMENT_COLUMN};
		private Hashtable TypeCollection = new Hashtable ();
		private Hashtable ElementCollection = new Hashtable ();

		#endregion // Fields

		#region Constructors

		public XmlSchemaMapper (DataSet dataset)
		{
			DSet = dataset;
		}

		public XmlSchemaMapper (DataTable datatable)
		{
			table = datatable;
		}
		
		#endregion // Constructors

		#region Public methods

		public void Read (XmlReader Reader)
		{
			XmlSchema Schema = XmlSchema.Read (Reader, new ValidationEventHandler (OnXmlSchemaValidation));
			if (DSet != null) DSet.Namespace = Schema.TargetNamespace;

			// read items
			foreach (XmlSchemaObject Item in Schema.Items)
				ReadXmlSchemaItem (Item);
		}

		#endregion // Public methods

		#region Private methods

		private void ReadXmlSchemaItem (XmlSchemaObject Item)
		{
			XmlSchemaObject SchemaObject;
			
			if (Item is XmlSchemaType)
				ReadXmlSchemaType ((XmlSchemaType)Item);
			else if (Item is XmlSchemaElement)
				ReadXmlSchemaElement (Item as XmlSchemaElement, ElementType.ELEMENT_UNDEFINED);
		}

		private void ReadXmlSchemaSequence (XmlSchemaSequence Sequence)
		{
			ReadXmlSchemaSequence (Sequence, null);
		}

		private void ReadXmlSchemaSequence (XmlSchemaSequence Sequence, DataTable Table)
		{
			foreach (XmlSchemaObject TempObj in Sequence.Items) {
				if (TempObj is XmlSchemaElement){
					XmlSchemaElement schemaElement = (XmlSchemaElement)TempObj;
					// the element can be a Column or a Table
					// tables do not have a type.
					if (schemaElement.SchemaTypeName.Name.Length > 0 || (schemaElement.SchemaType is XmlSchemaSimpleType))
						ReadXmlSchemaElement (schemaElement, ElementType.ELEMENT_COLUMN, Table);
					else
						ReadXmlSchemaElement (schemaElement, ElementType.ELEMENT_TABLE, Table);
				}
			}
		}

		private void ReadXmlSchemaChoice (XmlSchemaChoice Choice)
		{
			XmlSchemaObject SchemaObject;
			foreach (XmlSchemaObject TempObject in Choice.Items) {
				if ((SchemaObject = TempObject as XmlSchemaElement) != null)
					ReadXmlSchemaElement ((XmlSchemaElement)SchemaObject, ElementType.ELEMENT_TABLE);
			}
		}

		private void ReadXmlSchemaElement (XmlSchemaElement Element)
		{
			ReadXmlSchemaElement (Element, ElementType.ELEMENT_UNDEFINED);
		}

		private void ReadXmlSchemaElement (XmlSchemaElement Element, ElementType ElType)
		{
			ReadXmlSchemaElement (Element, ElType, null);
		}

		private void ReadXmlSchemaElement (XmlSchemaElement Element, ElementType ElType, DataTable Table)
		{
			Hashtable Attributes = ReadUnhandledAttributes (Element.UnhandledAttributes);
			DataTable Table2 = null;

			if (Attributes.Contains (XmlConstants.IsDataSet)) { // DataSet -elemt
			
				if (String.Compare (Attributes [XmlConstants.IsDataSet].ToString (), "true", true) == 0 && DSet != null)
					DSet.DataSetName = Element.Name;

				if (Attributes.Contains (XmlConstants.Locale)) {
					CultureInfo cinfo = new CultureInfo((String)Attributes [XmlConstants.Locale]);
					if (DSet != null) DSet.Locale = cinfo;
					else table.Locale = cinfo;
				}
			}
			else if (Element.SchemaTypeName != null && Element.SchemaTypeName.Namespace != XmlConstants.SchemaNamespace 
				 && Element.SchemaTypeName.Name != String.Empty) {

				//
				// If type is not standard type
				//

				if (DSet == null) throw new InvalidOperationException ("Schema not valid for a DataTable");
				
				DataTable TempTable = new DataTable (Element.Name);
				DSet.Tables.Add (TempTable);
				
				// If type is already defined in schema read it...				
				if (TypeCollection.Contains (Element.SchemaTypeName.ToString ()))
					ReadXmlSchemaType ((XmlSchemaType)TypeCollection [Element.SchemaTypeName.ToString ()], TempTable);
				else // but if it's not yet defined put it safe to wait if we need it later. 
					ElementCollection.Add (Element.SchemaTypeName.Name, TempTable);

			}
			else if (Element.RefName != null && Element.RefName.Name != string.Empty) { // if there is a ref=

				if (ElementCollection.Contains (Element.RefName.Name))
					ReadXmlSchemaElement ((XmlSchemaElement)ElementCollection [Element.RefName.Name], ElementType.ELEMENT_TABLE);
			}
			else if (ElementType.ELEMENT_UNDEFINED != ElType) {
				
				if (ElType == ElementType.ELEMENT_TABLE){
					ReadTable (Element);
					// we have to return else all child element of the table will be computed again.
					return;
				}
				else if (ElType == ElementType.ELEMENT_COLUMN && Table != null)
					ReadColumn (Element, Table);
			}
			else {
				// this element is undefined, for now
				ElementCollection.Add (Element.Name, Element);
			}

			// Read Element type
			if (Element.SchemaType != null)
				ReadXmlSchemaType (Element.SchemaType);

			// Read possible constraints
			if (Element.Constraints != null && Element.Constraints.Count > 0){
				ReadXmlSchemaConstraints (Element.Constraints);
			}
		}

		private void ReadTable (XmlSchemaElement Element)
		{
			DataTable TempTable = null;
			
			// Add the table to the DataSet only if it is not already in there.
			if (DSet != null) {
				if (DSet.Tables.Contains (Element.Name)) return;
				TempTable = new DataTable (Element.Name);
				DSet.Tables.Add (TempTable);
			}
			else {
				if (table.TableName.Length != 0) 
					throw new InvalidOperationException ("More than one table is defined in this schema");
				table.TableName = Element.Name;
				TempTable = table;
			}
			ReadXmlSchemaType (Element.SchemaType, TempTable);
		}

		private void ReadColumn (XmlSchemaElement Element, DataTable Table)
		{
			DataColumn Column = new DataColumn (Element.Name);
			Column.DataType = GetColumnType(Element.SchemaTypeName.Name);
			Table.Columns.Add (Column);

			if (Element.UnhandledAttributes != null) {
				
				foreach (XmlAttribute Attr in Element.UnhandledAttributes) {
					switch (Attr.LocalName) {
						
				        case XmlConstants.Caption:
						Column.Caption = Attr.Value;
						break;
				        case XmlConstants.DataType:
						Column.DataType = Type.GetType (Attr.Value);
						break;
					case XmlConstants.AutoIncrement:
						Column.AutoIncrement = bool.Parse(Attr.Value);
						break;
					case XmlConstants.AutoIncrementSeed:
						Column.AutoIncrementSeed = int.Parse(Attr.Value);
						break;
				        default:
						break;
					}
				}
			}

			//
			// Handel rest of the parameters
			//

			if (Column.DataType == null)
				Column.DataType = Type.GetType ("System.String");
			
			if (Element.DefaultValue != null)
				Column.DefaultValue = Element.DefaultValue;

			// If Element have type
			if (Element.SchemaType != null)
				ReadXmlSchemaType (Element.SchemaType, Column);

		}

		private Type GetColumnType (String typeName)
		{
			if (typeName == null || typeName.Length == 0)
				return typeof (string);
			Type t;
			switch (typeName) {
			case "char":
				t = typeof (char);
				break;
			case "int" :
				t = typeof (int);
				break;
			case "unsignedInt" :
				t = typeof (uint);
				break;
			case "unsignedByte" :
				t = typeof (byte);
				break;
			case "byte" :
				t = typeof (sbyte);
				break;
			case "short" :
				t = typeof (short);
				break;
			case "usignedShort" :
				t = typeof (ushort);
				break;
			case "long" :
				t = typeof (long);
				break;
			case "unsignedLong" :
				t = typeof (ulong);
				break;
			case "boolean" :
				t = typeof (bool);
				break;
			case "float" :
				t = typeof (float);
				break;
			case "double" :
				t = typeof (double);
				break;
			case "decimal" :
				t = typeof (decimal);
				break;
			case "dateTime" :
				t = typeof (DateTime);
				break;
			case "duration" :
				t = typeof (TimeSpan);
				break;
			case "base64Binary" :
				t = typeof (byte[]);
				break;
			default :
				t = typeof (string);
				break;
			}
			
			return t;
		}

		// Makes new Hashtable of the attributes.
		private Hashtable ReadUnhandledAttributes (XmlAttribute [] Attributes)
		{
			Hashtable Result = new Hashtable ();

			if (Attributes == null)
				return Result;

			foreach (XmlAttribute attribute in Attributes) {
				Result.Add (attribute.LocalName, attribute.Value);
			}
			
			return Result;
		}

		private void ReadXmlSchemaConstraints (XmlSchemaObjectCollection Constraints)
		{
			foreach (XmlSchemaObject Constraint in Constraints) {
				if (Constraint is XmlSchemaUnique)
					ReadXmlSchemaUnique ((XmlSchemaUnique)Constraint);
				if (Constraint is XmlSchemaKeyref)
					ReadXmlSchemaKeyref ((XmlSchemaKeyref)Constraint, Constraints);
			}
		}

		[MonoTODO()]
		private void ReadXmlSchemaUnique (XmlSchemaUnique Unique)
		{
			// FIXME: Parsing XPath
			string TableName = Unique.Selector.XPath;
			int index = TableName.IndexOf(':');
			if (index != -1)
				TableName = TableName.Substring (index + 1);
			else if(TableName.StartsWith (".//"))
				TableName = TableName.Substring (3);
			
			DataColumn [] Columns;
			DataTable Table = GetTable (TableName);
			if (Table != null) {
				Columns = new DataColumn [Unique.Fields.Count];
				int i = 0;
				foreach (XmlSchemaXPath Field in Unique.Fields) {
					string columnName = Field.XPath;
					index = columnName.IndexOf (':');
					if (index != -1)
						columnName = columnName.Substring (index + 1);
					if (Table.Columns.Contains (columnName)) {
						Columns [i] = Table.Columns [columnName];
						i++;
					}
				}
				
				bool isPK = false;
				// find if there is an attribute with the constraint name
				// if not use the XmlSchemaUnique name.
				string constraintName = Unique.Name;
				if (Unique.UnhandledAttributes != null){
					foreach (XmlAttribute attr in Unique.UnhandledAttributes){
						if (attr.LocalName == "ConstraintName"){
							constraintName = attr.Value;
						}
						else if (attr.LocalName == XmlConstants.PrimaryKey){
							isPK = bool.Parse(attr.Value);
						}

					}
				}
				UniqueConstraint Constraint = new UniqueConstraint (constraintName, Columns, isPK);
				Table.Constraints.Add (Constraint);
			}
		}

		[MonoTODO()]
		private void ReadXmlSchemaKeyref (XmlSchemaKeyref KeyRef, XmlSchemaObjectCollection collection) {
			
			if (DSet == null) return;	// Ignore relations for table-only schemas
			
			string TableName = KeyRef.Selector.XPath;
			int index = TableName.IndexOf(':');
			if (index != -1)
				TableName = TableName.Substring (index + 1);
			else if (TableName.StartsWith (".//"))
				TableName = TableName.Substring (3);
			DataColumn [] Columns;
			DataTable Table = GetTable (TableName);
			if (Table != null) {
				Columns = new DataColumn [KeyRef.Fields.Count];
				int i = 0;
				foreach (XmlSchemaXPath Field in KeyRef.Fields) {
					string columnName = Field.XPath;
					index = columnName.IndexOf (':');
					if (index != -1)
						columnName = columnName.Substring (index + 1);
					if (Table.Columns.Contains (columnName)) {
						Columns [i] = Table.Columns [columnName];
						i++;
					}
				}
				string name = KeyRef.Refer.Name;
				// get the unique constraint for the releation
				UniqueConstraint constraint = GetDSConstraint(name, collection);
				// generate the FK.
				ForeignKeyConstraint fkConstraint = new ForeignKeyConstraint(constraint.Columns, Columns);
				Table.Constraints.Add (fkConstraint);
				// generate the relation.
				DataRelation relation = new DataRelation(KeyRef.Name, constraint.Columns, Columns, false);
				if (KeyRef.UnhandledAttributes != null){
					foreach (XmlAttribute attr in KeyRef.UnhandledAttributes){
						if (attr.LocalName == "IsNested"){
							if (attr.Value == "true")
								relation.Nested = true;
						}
					}
				}

				DSet.Relations.Add(relation);
			}
		}
		
		// get the unique constraint for the relation.
		// name - the name of the XmlSchemaUnique element
		private UniqueConstraint GetDSConstraint(string name, XmlSchemaObjectCollection collection)
		{
			// find the element in the constraint collection.
			foreach (XmlSchemaObject shemaObj in collection){
				if (shemaObj is XmlSchemaUnique){
					XmlSchemaUnique unique = (XmlSchemaUnique) shemaObj;
					if (unique.Name == name){
						string tableName = unique.Selector.XPath;
						int index = tableName.IndexOf (':');
						if (index != -1)
							tableName = tableName.Substring (index + 1);
						else if (tableName.StartsWith (".//"))
							tableName = tableName.Substring (3);
						
						// find the table in the dataset.
						DataTable table = GetTable (tableName);
						if (table != null){
							
							string constraintName = unique.Name;
							// find if there is an attribute with the constraint name
							// if not use the XmlSchemaUnique name.
							if (unique.UnhandledAttributes != null){
								foreach (XmlAttribute attr in unique.UnhandledAttributes){
									if (attr.LocalName == "ConstraintName"){
										constraintName = attr.Value;
										break;
									}
								}
							}
							if (table.Constraints.Contains(constraintName))
								return (UniqueConstraint)table.Constraints[constraintName];
						}

					}
				}
			}
			return null;
		}


		#endregion // Private methods

		#region Private listeners

		private void OnXmlSchemaValidation (object sender, ValidationEventArgs args)
		{
			;
		}

		#endregion // Private listeners

		#region Private TypeReaders

		// Reads XmlSchemaType
		private void ReadXmlSchemaType (XmlSchemaType SchemaType)
		{
			ReadXmlSchemaType (SchemaType, (DataTable)null);
		}

		// Reads XmlSchemaType and decides is it Complex or Simple and continue reading those types
		private void ReadXmlSchemaType (XmlSchemaType SchemaType, DataTable Table)
		{
			if (SchemaType is XmlSchemaComplexType)
				ReadXmlSchemaComplexType ((XmlSchemaComplexType)SchemaType, Table);
			else if (SchemaType is XmlSchemaSimpleType)
				ReadXmlSchemaSimpleType ((XmlSchemaSimpleType)SchemaType, Table);
		}

		// Same as above but with DataColumn
		private void ReadXmlSchemaType (XmlSchemaType SchemaType, DataColumn Column)
		{
			if (SchemaType is XmlSchemaComplexType)
				ReadXmlSchemaComplexType ((XmlSchemaComplexType)SchemaType, Column);
			else if (SchemaType is XmlSchemaSimpleType)
				ReadXmlSchemaSimpleType ((XmlSchemaSimpleType)SchemaType, Column);
		}

		#endregion // PrivateTypeReader

		#region TypeReaderHelppers

		private void ReadXmlSchemaSimpleType (XmlSchemaSimpleType SimpleType, DataColumn Column)
		{
			// Read Contents
			if (SimpleType.Content is XmlSchemaSimpleTypeRestriction)
				ReadXmlSchemaSimpleTypeRestriction ((XmlSchemaSimpleTypeRestriction)SimpleType.Content, Column);
		}

		[MonoTODO]
		private void ReadXmlSchemaSimpleType (XmlSchemaSimpleType SimpleType, DataTable Table)
		{
			// TODO: Is it possible that Table-element have simpletype???
		}

		[MonoTODO]
		private void ReadXmlSchemaSimpleTypeRestriction (XmlSchemaSimpleTypeRestriction Restriction, DataColumn Column)
		{
			foreach (XmlSchemaObject Facet in Restriction.Facets) {
				
				// FIXME: I dont know are everyone of these needed but, let them be here for now
				if (Facet is XmlSchemaMaxLengthFacet) 
					Column.MaxLength = Int32.Parse(((XmlSchemaFacet)Facet).Value);
				//else if (Facet is XmlSchemaMinLengthFacet) 
				//	;
				//else if (Facet is XmlSchemaLengthFacet)
				//      ;
				//else if (Facet is XmlSchemaPatternFacet)
				//	;
				//else if (Facet is XmlSchemaEnumerationFacet)
				//	;
				//else if (Facet is XmlSchemaMaxInclusiveFacet)
				//	;
				//else if (Facet is XmlSchemaMaxExclusiveFacet)
				//	;
				//else if (Facet is XmlSchemaMinInclusiveFacet)
				//	;
				//else if (Facet is XmlSchemaMinExclusiveFacet)
				//	;
				//else if (Facet is XmlSchemaFractionDigitsFacet)
				//	;
				//else if (Facet is XmlSchemaTotalDigitsFacet)
				//	;
				//else if (Facet is XmlSchemaWhiteSpaceFacet)
				//	;
			}
		}

		[MonoTODO]
		private void ReadXmlSchemaComplexType (XmlSchemaComplexType Type, DataColumn Column)
		{
			// TODO: is it possible that column-element have complextype
		}

		// Reads XmlSchemaComplexType with DataTable
		private void ReadXmlSchemaComplexType (XmlSchemaComplexType Type, DataTable Table)
		{
			XmlSchemaComplexType ComplexType = Type as XmlSchemaComplexType;
			
			if (ComplexType.Name != null && ComplexType.Name != string.Empty) {

				if (ElementCollection.Contains (ComplexType.Name)) {

					if (ComplexType.Particle is XmlSchemaChoice) {
						ReadXmlSchemaChoice (ComplexType.Particle as XmlSchemaChoice);
					}
					else if (ComplexType.Particle is XmlSchemaSequence) {

						DataTable TempTable = ElementCollection [ComplexType.Name] as DataTable;
						ElementCollection.Remove (ComplexType.Name);
						ReadXmlSchemaSequence (ComplexType.Particle as XmlSchemaSequence, TempTable);
					}
				}
				else if (ComplexType.Name != null && !TypeCollection.Contains (ComplexType.Name)) {
					TypeCollection.Add (ComplexType.Name, ComplexType);
				} 
				else {

					// If we are here it means that types of elements are Tables :-P
					if (ComplexType.Particle is XmlSchemaSequence)
						ReadXmlSchemaSequence (ComplexType.Particle as XmlSchemaSequence, Table);
				}

			}
			else {
				XmlSchemaParticle Particle;
				if ((Particle = ComplexType.Particle as XmlSchemaChoice) != null) {
					ReadXmlSchemaChoice (Particle as XmlSchemaChoice);
				}
				else if ((Particle = ComplexType.Particle as XmlSchemaSequence) != null) {
					ReadXmlSchemaSequence (Particle as XmlSchemaSequence, Table);
				}
			}				
		}
		
		DataTable GetTable (string name)
		{
			if (DSet != null)
				return DSet.Tables [name];
			else if (name == table.TableName) 
				return table;
			else
				throw new InvalidOperationException ("Schema not valid for table '" + table.TableName + "'");
		}
		
		#endregion // TypeReaderHelppers
	}
}
	
