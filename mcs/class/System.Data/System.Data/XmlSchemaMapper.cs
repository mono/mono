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
// TODO: Relations
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
		enum ElementType {ELEMENT_UNDEFINED, ELEMENT_TABLE, ELEMENT_COLUMN};
		private Hashtable TypeCollection = new Hashtable ();
		private Hashtable ElementCollection = new Hashtable ();

		#endregion // Fields

		#region Constructors

	        public XmlSchemaMapper (DataSet dataset)
		{
			DSet = dataset;
		}

		#endregion // Constructors

		#region Public methods

		public void Read (XmlReader Reader)
		{
			XmlSchema Schema = XmlSchema.Read (Reader, new ValidationEventHandler (OnXmlSchemaValidation));
			
			// read items
			foreach (XmlSchemaObject Item in Schema.Items) {
				ReadXmlSchemaItem (Item);
			}
		}

		#endregion // Public methods

		#region Private methods

		private void ReadXmlSchemaItem (XmlSchemaObject Item)
		{
			XmlSchemaObject SchemaObject;
			
			if (Item is XmlSchemaType)
				ReadXmlSchemaType ((XmlSchemaType)Item);
			else if ((SchemaObject = Item as XmlSchemaElement) != null)
				ReadXmlSchemaElement (SchemaObject as XmlSchemaElement, ElementType.ELEMENT_UNDEFINED);
		}

		private void ReadXmlSchemaSequence (XmlSchemaSequence Sequence)
		{
			ReadXmlSchemaSequence (Sequence, null);
		}

		private void ReadXmlSchemaSequence (XmlSchemaSequence Sequence, DataTable Table)
		{
			foreach (XmlSchemaObject TempObj in Sequence.Items) {
				
				XmlSchemaObject SchemaObject;
				if ((SchemaObject = TempObj as XmlSchemaElement) != null)
					ReadXmlSchemaElement ((XmlSchemaElement)SchemaObject, ElementType.ELEMENT_COLUMN, Table);
			}
		}

		private void ReadXmlSchemaChoice (XmlSchemaChoice Choice)
		{
			//MaxOccurs

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

			if (Attributes.Contains ("IsDataSet")) { // DataSet -elemt

				if (String.Compare (Attributes ["IsDataSet"].ToString (), "true", true) == 0)
					DSet.DataSetName = Element.Name;
			}
			else if (Element.SchemaTypeName != null && Element.SchemaTypeName.Namespace != XmlConstants.SchemaNamespace 
				 && Element.SchemaTypeName.Name != String.Empty) {

				//
				// If type is not standard type
				//

				DataTable TempTable = new DataTable (Element.Name);
				DSet.Tables.Add (TempTable);
				
				// FIXME: if this element comes before types
				if (TypeCollection.Contains (Element.SchemaTypeName.ToString ()))
					ReadXmlSchemaType ((XmlSchemaType)TypeCollection [Element.SchemaTypeName.ToString ()], TempTable);

			}
			else if (Element.RefName != null && Element.RefName.Name != string.Empty) { // if there is a ref=

				if (ElementCollection.Contains (Element.RefName.Name))
					ReadXmlSchemaElement ((XmlSchemaElement)ElementCollection [Element.RefName.Name], ElementType.ELEMENT_TABLE);
			}
			else if (ElementType.ELEMENT_UNDEFINED != ElType) {

				if (ElType == ElementType.ELEMENT_TABLE)
					ReadTable (Element);
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
			if (Element.Constraints != null && Element.Constraints.Count > 0)
				ReadXmlSchemaConstraints (Element.Constraints);
		}

		private void ReadTable (XmlSchemaElement Element)
		{
			DataTable TempTable = new DataTable (Element.Name);
			DSet.Tables.Add (TempTable);
			ReadXmlSchemaType (Element.SchemaType, TempTable);			
		}

		private void ReadColumn (XmlSchemaElement Element, DataTable Table)
		{
			DataColumn Column = new DataColumn (Element.Name);
			Table.Columns.Add (Column);

			if (Element.UnhandledAttributes == null)
				return;

			foreach (XmlAttribute Attr in Element.UnhandledAttributes) {

				switch (Attr.LocalName) {

				        case "Caption":
						Column.Caption = Attr.Value;
						break;
				        case "DataType":
						Column.DataType = Type.GetType (Attr.Value);
						break;
				        case "type":
						// FIXME:

						break;						
				        default:
						break;
				}
			}

			//
			// Handel rest of the parameters
			//

			// FIXME: Default columntype is string???
			if (Column.DataType == null)
				Column.DataType = Type.GetType ("System.String");
			
			if (Element.DefaultValue != null)
				Column.DefaultValue = Element.DefaultValue;

			// If Element have type
			if (Element.SchemaType != null)
				ReadXmlSchemaType (Element.SchemaType, Column);
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
			}
		}

		[MonoTODO()]
		private void ReadXmlSchemaUnique (XmlSchemaUnique Unique)
		{
			// FIXME: Parsing XPath
			
			string TableName = Unique.Selector.XPath;
			if (TableName.StartsWith (".//"))
				TableName = TableName.Substring (3);
			
			DataColumn [] Columns;
			if (DSet.Tables.Contains (TableName)) {
				
				DataTable Table = DSet.Tables [TableName];
				Columns = new DataColumn [Unique.Fields.Count];
				int i = 0;
				foreach (XmlSchemaXPath Field in Unique.Fields) {

					if (Table.Columns.Contains (Field.XPath)) {
						Table.Columns [Field.XPath].Unique = true;
						Columns [i] = Table.Columns [Field.XPath];
						i++;
					}
				}

				UniqueConstraint Constraint = new UniqueConstraint (Unique.Name, Columns);
			}
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

			if (ComplexType.Name != null && !TypeCollection.Contains (ComplexType.Name)) {
				TypeCollection.Add (ComplexType.Name, ComplexType);
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
		
		#endregion // TypeReaderHelppers
	}
}
	
