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

using System;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Collections;
using System.Globalization;

namespace System.Data {

        /*\
	 *  There is so much TODO here that i dont bother to list them.
	 *  But colum types, attributes, references ...
	\*/


        internal class XmlSchemaMapper
	{	
	        #region Fields

	        private DataSet DSet;
		enum ElementType {ELEMENT_UNDEFINED, ELEMENT_TABLE, ELEMENT_COLUMN};
		private Hashtable TypeCollection = new Hashtable ();

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
			
			foreach (XmlSchemaObject Item in Schema.Items) {
				ReadXmlSchemaItem (Item);
			}
		}

		#endregion // Public methods

		#region Private methods

		private void ReadXmlSchemaItem (XmlSchemaObject Item)
		{
			XmlSchemaType Type;
			XmlSchemaObject SchemaObject;
			if ((Type = Item as XmlSchemaSimpleType) != null) {
				ReadXmlSchemaSimpleType (Type);
			}
			else if ((Type = Item as XmlSchemaComplexType) != null) {
				ReadXmlSchemaComplexType (Type);
			}
			else if ((SchemaObject = Item as XmlSchemaElement) != null) {
				ReadXmlSchemaElement (SchemaObject as XmlSchemaElement, ElementType.ELEMENT_UNDEFINED);
			}
		}

		[MonoTODO]
		private void ReadXmlSchemaSimpleType (XmlSchemaType Type)
		{
			
		}

		private void ReadXmlSchemaComplexType (XmlSchemaType Type)
		{
			ReadXmlSchemaComplexType (Type, null);
		}

		private void ReadXmlSchemaComplexType (XmlSchemaType Type, DataTable Table)
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

				DataTable TempTable = new DataTable (Element.Name);
				DSet.Tables.Add (TempTable);
				
				// FIXME: if this element comes before types
				if (TypeCollection.Contains (Element.SchemaTypeName.ToString ()))
					ReadXmlSchemaComplexType ((XmlSchemaComplexType)TypeCollection [Element.SchemaTypeName.ToString ()], TempTable);

				
			} 
			else {
				if (ElType == ElementType.ELEMENT_TABLE) {

					DataTable TempTable = new DataTable (Element.Name);
					DSet.Tables.Add (TempTable);
					ReadXmlSchemaComplexType (Element.SchemaType, TempTable);
					return;
				}
				else if (ElType == ElementType.ELEMENT_COLUMN) {

					DataColumn Column = new DataColumn (Element.Name);
					Table.Columns.Add (Column);
				}
			}

			XmlSchemaType Type;
			if ((Type = Element.SchemaType as XmlSchemaComplexType) != null) {
				ReadXmlSchemaComplexType (Type);
			}

		}

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

		#endregion // Private methods

		#region Private listeners

		private void OnXmlSchemaValidation (object sender, ValidationEventArgs args)
		{
			;
		}

		#endregion // Private listeners
	}
}
	
