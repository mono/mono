// 
// System.Data/DataSet.cs
//
// Author:
//   Christopher Podurgiel <cpodurgiel@msn.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Rodrigo Moya <rodrigo@ximian.com>
//   Stuart Caborn <stuart.caborn@virgin.net>
//   Tim Coleman (tim@timcoleman.com)
//   Ville Palo <vi64pa@koti.soon.fi>
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace System.Data {
	/// <summary>
	/// an in-memory cache of data 
	/// </summary>
	[DefaultProperty ("DataSetName")]
	[Serializable]
	public class DataSet : MarshalByValueComponent, IListSource,
		ISupportInitialize, ISerializable {

		private string dataSetName;
		private string _namespace;
		private string prefix;
		private bool caseSensitive;
		private bool enforceConstraints = true;
		private DataTableCollection tableCollection;
		// private DataTableRelationCollection relationCollection;
		private PropertyCollection properties;
		private DataViewManager defaultView;
		
		#region Constructors

		[MonoTODO]
		public DataSet() {
			tableCollection = new DataTableCollection (this);
		}

		[MonoTODO]
		public DataSet(string name) : this () {
			dataSetName = name;
		}

		[MonoTODO]
		protected DataSet(SerializationInfo info, StreamingContext context) : this () {
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Public Properties

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether comparing strings within the DataSet is case sensitive.")]
		[DefaultValue (false)]
		public bool CaseSensitive {
			get { return caseSensitive; } 
			set { caseSensitive = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The name of this DataSet.")]
		[DefaultValue ("")]
		public string DataSetName {
			get { return dataSetName; } 
			set { dataSetName = value; }
		}

		[DataSysDescription ("Indicates a custom \"view\" of the data contained by the DataSet. This view allows filtering, searching, and navigating through the custom data view.")]
		[Browsable (false)]
		public DataViewManager DefaultViewManager {
			get {
				if (defaultView == null)
					defaultView = new DataViewManager (this);
				return defaultView;
			} 
		}

		[DataSysDescription ("Indicates whether constraint rules are to be followed.")]
		[DefaultValue (true)]
		public bool EnforceConstraints {
			get { return enforceConstraints; } 
			set { enforceConstraints = value; }
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds custom user information.")]
		public PropertyCollection ExtendedProperties {
			[MonoTODO]
			get { return properties; }
		}

		[Browsable (false)]
		[DataSysDescription ("Indicates that the DataSet has errors.")]
		public bool HasErrors {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates a locale under which to compare strings within the DataSet.")]
		public CultureInfo Locale {
			[MonoTODO]
			get { 
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the XML uri namespace for the root element pointed at by this DataSet.")]
		[DefaultValue ("")]
		public string Namespace {
			[MonoTODO]
			get { return _namespace; } 
			[MonoTODO]
			set {
				//TODO - trigger an event if this happens?
				_namespace = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the prefix of the namespace used for this DataSet.")]
		[DefaultValue ("")]
		public string Prefix {
			[MonoTODO]
			get { return prefix; } 
			[MonoTODO]
			set {
				//TODO - trigger an event if this happens?
				prefix = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds the relations for this DatSet.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataRelationCollection Relations {
			[MonoTODO]
			get{
				//return relationCollection;
				throw new NotImplementedException();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ISite Site {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds the tables for this DataSet.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataTableCollection Tables {
			get { return tableCollection; }
		}

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public void AcceptChanges()
		{
			foreach (DataTable tempTable in tableCollection)
				tempTable.AcceptChanges ();
		}

		public void Clear()
		{
			throw new NotImplementedException ();
		}

		public virtual DataSet Clone()
		{
			throw new NotImplementedException ();
		}

		public DataSet Copy()
		{
			throw new NotImplementedException ();
		}

		public DataSet GetChanges()
		{
			throw new NotImplementedException ();
		}

		
		public DataSet GetChanges(DataRowState rowStates)
		{
			throw new NotImplementedException ();
		}

		public string GetXml()
		{
			return "Fish!";
		}

		public string GetXmlSchema()
		{
			throw new NotImplementedException ();
		}

		public virtual void RejectChanges()
		{
			throw new NotImplementedException ();
		}

		public virtual void Reset()
		{
			throw new NotImplementedException ();
		}

		public void WriteXml(Stream stream)
		{
			XmlWriter writer = new XmlTextWriter(stream, null );
			
			WriteXml( writer );
		}

		///<summary>
		/// Writes the current data for the DataSet to the specified file.
		/// </summary>
		/// <param name="filename">Fully qualified filename to write to</param>
		public void WriteXml(string fileName)
		{
			XmlWriter writer = new XmlTextWriter(fileName, null );
			
			WriteXml( writer );
		}

		public void WriteXml(TextWriter writer)
		{
			XmlWriter xwriter = new XmlTextWriter(writer );
			
			WriteXml( xwriter );
		}

		public void WriteXml(XmlWriter writer)
		{
			WriteXml( writer, XmlWriteMode.IgnoreSchema );
		}

		public void WriteXml(Stream stream, XmlWriteMode mode)
		{
			XmlWriter writer = new XmlTextWriter(stream, null );
			
			WriteXml( writer, mode );
		}

		public void WriteXml(string fileName, XmlWriteMode mode)
		{
			XmlWriter writer = new XmlTextWriter(fileName, null );
			
			WriteXml( writer, mode );
		}

		public void WriteXml(TextWriter writer,	XmlWriteMode mode)
		{
			XmlWriter xwriter = new XmlTextWriter(writer);
			
			WriteXml( xwriter, mode );
		}

		public void WriteXml(XmlWriter writer, XmlWriteMode mode)
		{

			WriteStartElement( writer, mode, Namespace, Prefix, DataSetName );
			
			if( mode == XmlWriteMode.WriteSchema )
			{
				DoWriteXmlSchema( writer );
			}
			
			//Write out each table in order, providing it is not
			//part of another table structure via a nested parent relationship
			foreach( DataTable table in Tables )
			{		
				bool isTopLevel = true;
				//FIXME: Uncomment this when Parentrelations is implemented
				/*
				foreach( DataRelation rel in table.ParentRelations )
				{
					if( rel.Nested )
					{
						isTopLevel = false;
						break;
					}
				}
				*/
				
				if( isTopLevel )
				{
					WriteTable(  writer, table, mode );
				}
			}
			
			writer.WriteEndElement();
			
			writer.WriteEndDocument();
					
		}

		public void WriteXmlSchema(Stream stream)
		{
			XmlWriter writer = new XmlTextWriter(stream, null  );
			
			WriteXmlSchema( writer );	
		}

		public void WriteXmlSchema(string fileName)
		{
			XmlWriter writer = new XmlTextWriter( fileName, null );
	    	
			WriteXmlSchema( writer );
		}

		public void WriteXmlSchema(TextWriter writer)
		{
			XmlWriter xwriter = new XmlTextWriter( writer );
			
			WriteXmlSchema( xwriter );
		}

		public void WriteXmlSchema(XmlWriter writer)
		{
			//Create a skeleton doc and then write the schema 
			//proper which is common to the WriteXml method in schema mode
			writer.WriteStartDocument();
			
			DoWriteXmlSchema( writer );
			
			writer.WriteEndDocument();
			
		}

		public void ReadXmlSchema(Stream stream)
		{
			XmlReader reader = new XmlTextReader( stream, null );
			ReadXmlSchema( reader);
		}

		public void ReadXmlSchema(string str)
		{
			XmlReader reader = new XmlTextReader( str );
			ReadXmlSchema( reader );
		}

		public void ReadXmlSchema(TextReader treader)
		{
			XmlReader reader = new XmlTextReader( treader );

			ReadXmlSchema( reader );			
		}

		public void ReadXmlSchema(XmlReader reader)
		{
			DoReadXmlSchema( reader );
		}

		public XmlReadMode ReadXml (Stream stream)
		{
			return ReadXml (new XmlTextReader (stream));
		}

		public XmlReadMode ReadXml (string str)
		{
			return ReadXml (new XmlTextReader (str));
		}

		public XmlReadMode ReadXml (TextReader reader)
		{
			return ReadXml (new XmlTextReader (reader));
		}

		[MonoTODO]
		public XmlReadMode ReadXml (XmlReader r)
		{
			// FIXME: somekinda exception?
			if (!r.Read ())
				return XmlReadMode.Auto; // FIXME

			/*\
			 *  If document is diffgram we will use diffgram
			\*/
			if (r.LocalName == "diffgram")
				return ReadXml (r, XmlReadMode.DiffGram);

			/*\
			 *  If we already have a schema, or the document 
			 *  contains an in-line schema, sets XmlReadMode to ReadSchema.
		        \*/

			// FIXME: is this always true: "if we have tables we have to have schema also"
			if (Tables.Count > 0)				
				return ReadXml (r, XmlReadMode.ReadSchema);

			/*\
			 *  If we dont have a schema yet and document 
			 *  contains no inline-schema  mode is XmlReadMode.InferSchema
			\*/

			return ReadXml (r, XmlReadMode.InferSchema);
		}

		public XmlReadMode ReadXml (Stream stream, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (stream), mode);
		}

		public XmlReadMode ReadXml (string str, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (str), mode);
		}

		public XmlReadMode ReadXml (TextReader reader, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (reader), mode);
		}

		[MonoTODO]
		public XmlReadMode ReadXml (XmlReader reader, XmlReadMode mode)
		{
			XmlReadMode readMode = XmlReadMode.Auto;

			switch (mode) {

			        case XmlReadMode.DiffGram:
					break;
				case XmlReadMode.ReadSchema:
					readMode = XmlReadMode.ReadSchema;
					ReadXmlReadSchema (reader);					
					break;
			        case XmlReadMode.InferSchema:
					readMode = XmlReadMode.InferSchema;
					ReadXmlInferSchemaMode (reader);
					break;
			        default:
					break;
			}

			return readMode;
		}

		#endregion // Public Methods

		#region Public Events

		[DataCategory ("Action")]
		[DataSysDescription ("Occurs when it is not possible to merge schemas for two tables with the same name.")]
		public event MergeFailedEventHandler MergeFailed;

		#endregion // Public Events

		#region Destructors

		~DataSet()
		{
		}

		#endregion Destructors

		#region IListSource methods
		IList IListSource.GetList ()
		{
			return DefaultViewManager;
		}

		bool IListSource.ContainsListCollection {
			get {
				return true;
			}
		}
		#endregion IListSource methods
		
		#region ISupportInitialize methods
		void ISupportInitialize.BeginInit ()
		{
			throw new NotImplementedException ();
		}

		void ISupportInitialize.EndInit ()
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region ISerializable
		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Private ReadXml-methods

		[MonoTODO]
		private void ReadXmlInferSchemaMode (XmlReader reader)
		{
			// root element is DataSets name
			reader.MoveToContent ();

			dataSetName = reader.LocalName;

			// And now comes tables
			while (reader.Read ()) {

				// skip possible inline-schema
				if (String.Compare (reader.LocalName, "schema", true) == 0 && reader.NodeType == XmlNodeType.Element) {
					while (reader.Read () && (reader.NodeType != XmlNodeType.EndElement 
								  || String.Compare (reader.LocalName, "schema", true) != 0));
				}


				if (reader.NodeType == XmlNodeType.Element) {
					
					string datatablename = reader.LocalName;
					bool addTable = true;
					DataTable table;

					if (!Tables.Contains (datatablename)) {
						table = new DataTable (reader.LocalName);
					}
					else {
						table = Tables [datatablename];
						addTable = false;
					}

					Hashtable rowValue = new Hashtable ();

					while (reader.Read () && (reader.NodeType != XmlNodeType.EndElement 
								  || reader.LocalName != datatablename))
					{
						if (reader.NodeType == XmlNodeType.Element) {

							string dataColumnName = reader.LocalName;
							if (addTable)
								table.Columns.Add (dataColumnName);

							// FIXME: exception?
							if (!reader.Read ())
								return;

							rowValue.Add (dataColumnName, reader.Value);
						}
					}
					
					DataRow row = table.NewRow ();
					
					IDictionaryEnumerator enumerator = rowValue.GetEnumerator ();
					while (enumerator.MoveNext ()) {
						row [enumerator.Key.ToString ()] = enumerator.Value.ToString ();
					}

					table.Rows.Add (row);
					
					if (addTable)
						Tables.Add (table);
				}
			}			
		}

		[MonoTODO]
		private void ReadXmlReadSchema (XmlReader reader)
		{
			/*\
			 *  Reads any inline schema, but an exception is thrown 
			 *  if any tables in the inline schema already exist in the DataSet.
			\*/

			reader.MoveToContent ();

			while (reader.Read ()) {

				// FIXME: possible inline-schema should be readed here
				if (String.Compare (reader.LocalName, "schema", true) == 0 && reader.NodeType == XmlNodeType.Element) {
					ReadXmlSchema (reader);
				}

				// find table
				if (reader.NodeType == XmlNodeType.Element && Tables.Contains (reader.LocalName)) {
					
					DataTable table = Tables [reader.LocalName];
					DataRow row = table.NewRow ();
					do {
						if (reader.NodeType == XmlNodeType.Element && 
						    table.Columns.Contains (reader.LocalName)) {
							string columName = reader.LocalName;
							reader.Read ();
							row [columName] = reader.Value;
						}

					} while (table.TableName != reader.LocalName 
								    || reader.NodeType != XmlNodeType.EndElement);
					
					table.Rows.Add (row);
				}
			}
		}

		#endregion // Private ReadXml-methods

		#region Private Xml Serialisation
	
		private void WriteTable( XmlWriter writer, DataTable table, XmlWriteMode mode )
		{
			//The columns can be attributes, hidden, elements, or simple content
			//There can be 0-1 simple content cols or 0-* elements
			System.Collections.ArrayList atts;
			System.Collections.ArrayList elements;
			DataColumn simple = null;
			
			SplitColumns( table, out atts, out elements, out simple );
			
			foreach( DataRow row in table.Rows )
			{
				//sort out the namespacing
				string nspc = table.Namespace.Length > 0 ? table.Namespace : Namespace;

				WriteStartElement( writer, mode, nspc, table.Prefix, table.TableName );
				
				foreach( DataColumn col in atts )
				{					
					WriteAttributeString( writer, mode, col.Namespace, col.Prefix, col.ColumnName, row[col].ToString() );
				}
				
				if( simple != null )
				{
					writer.WriteString( row[simple].ToString() );
				}
				else
				{					
					foreach( DataColumn col in elements )
					{
						string colnspc = nspc;
						object rowObject = row [col];
												
						if (rowObject == null)
							continue;

						if( col.Namespace != null )
						{
							colnspc = col.Namespace;
						}
				
						//TODO check if I can get away with write element string
						WriteStartElement( writer, mode, colnspc, col.Prefix, col.ColumnName );
						writer.WriteString( rowObject.ToString() );
						writer.WriteEndElement();
					}
				}
				
				//TODO write out the nested child relations
				
				writer.WriteEndElement();
			}
		}
		    
		private void WriteStartElement( XmlWriter writer, XmlWriteMode mode, string nspc, string prefix, string name )
		{			
			switch(  mode )
				{
					case XmlWriteMode.WriteSchema:
						if( nspc == null || nspc == "" )
						{
							writer.WriteStartElement( name );
						}
						else if( prefix != null )
						{							
							writer.WriteStartElement(prefix, name, nspc );
						}						
						else
						{					
							writer.WriteStartElement( writer.LookupPrefix( nspc ), name, nspc );
						}
						break;
					case XmlWriteMode.DiffGram:
						throw new NotImplementedException();
					default:
						writer.WriteStartElement(name );
						break;					
				};
		}
		
		private void WriteAttributeString( XmlWriter writer, XmlWriteMode mode, string nspc, string prefix, string name, string stringValue )
		{
			switch(  mode )
				{
					case XmlWriteMode.WriteSchema:
						writer.WriteAttributeString(prefix, name, nspc );
						break;
					case XmlWriteMode.DiffGram:
						throw new NotImplementedException();				
					default:
						writer.WriteAttributeString(name, stringValue );
						break;					
				};
		}

		private void DoWriteXmlSchema( XmlWriter writer )
		{
			//Create the root element and declare all the namespaces etc
			writer.WriteStartElement( 	XmlConstants.SchemaPrefix,
			                         	XmlConstants.SchemaElement,			                         											 
										XmlConstants.SchemaNamespace );
			
			writer.WriteAttributeString( XmlConstants.Id, DataSetName );
			writer.WriteAttributeString( XmlConstants.TargetNamespace, Namespace );
			writer.WriteAttributeString( "xmlns:" + XmlConstants.TnsPrefix, Namespace );
			writer.WriteAttributeString( "xmlns", Namespace );
			writer.WriteAttributeString(  "xmlns:" + XmlConstants.MsdataPrefix,                 
			                            XmlConstants.MsdataNamespace );
			//Set up the attribute and element forms.  
			//TODO - is it possible to change this?
			//I couldn't spot if it was so I assumed
			//that this is set to qualified all round basedon the MS output
			writer.WriteAttributeString( XmlConstants.AttributeFormDefault, 
			                            XmlConstants.Qualified );
			writer.WriteAttributeString( XmlConstants.ElementFormDefault, 
			                            XmlConstants.Qualified );
			
			
			//<xs:element name="DSName msdata:IsDataSet="true" msdata:Locale="machine-locale">
			//Create the data set element
			//All the tables are represented as choice elements in an unlimited series
			writer.WriteStartElement( XmlConstants.SchemaPrefix,
			                         	XmlConstants.Element,
			                         	XmlConstants.SchemaNamespace );
			
			writer.WriteAttributeString( XmlConstants.Name, DataSetName );
			writer.WriteAttributeString( XmlConstants.MsdataPrefix,  XmlConstants.IsDataSet, XmlConstants.MsdataNamespace, "true" );
			//FIXME - sort out the locale string!
			writer.WriteAttributeString( XmlConstants.MsdataPrefix, XmlConstants.Locale, XmlConstants.MsdataNamespace, "en-us" );
			
			//<xs:complexType>
			writer.WriteStartElement( XmlConstants.SchemaPrefix,
			                         	XmlConstants.ComplexType,
			                         	XmlConstants.SchemaNamespace );
			
			//<xs:choice maxOccurs="unbounded">
			writer.WriteStartElement( XmlConstants.SchemaPrefix,
			                         	XmlConstants.Choice,
			                         	XmlConstants.SchemaNamespace );
			
			writer.WriteAttributeString( XmlConstants.MaxOccurs, XmlConstants.Unbounded );
			
			
			//Write out schema for each table in order, providing it is not
			//part of another table structure via a nested parent relationship
			//TODO - is this correct? should I be using nested objects?
			foreach( DataTable table in Tables )
			{		
				bool isTopLevel = true;
				//FIXME: Uncomment this when ParentRelations class is implemented
				/*
				foreach( DataRelation rel in table.ParentRelations )
				{
					if( rel.Nested )
					{
						isTopLevel = false;
						break;
					}
				}
				*/
				
				if( isTopLevel )
				{
					WriteTableSchema(  writer, table );
				}
			}
			
			//</xs:choice>
			writer.WriteEndElement();
			//</xs:complexType>
			writer.WriteEndElement();
			
			//TODO - now add in the relationships as key and unique constraints etc
			
			//</xs:element>
			writer.WriteEndElement();
			
			//</schema>
			writer.WriteEndElement();
		}
		
		private void WriteTableSchema( XmlWriter writer, DataTable table )
		{
			ArrayList elements;
			ArrayList atts;
			DataColumn simple;
			
			SplitColumns( table,out  atts, out elements, out simple );
			
			//<xs:element name="TableName">
			writer.WriteStartElement( XmlConstants.SchemaPrefix,
			                         XmlConstants.Element,
			                         XmlConstants.SchemaNamespace );
			
			writer.WriteAttributeString( XmlConstants.Name, table.TableName );
			
			//<xs:complexType>
			writer.WriteStartElement( XmlConstants.SchemaPrefix,
			                         XmlConstants.ComplexType,
			                         XmlConstants.SchemaNamespace );
			
			//TODO - what about the simple content?
			if( elements.Count == 0 )				
			{			
			}
			else
			{				
			//A sequence of element types or a simple content node
			//<xs:sequence>
			writer.WriteStartElement( XmlConstants.SchemaPrefix,
			                         XmlConstants.Sequence,
			                         XmlConstants.SchemaNamespace );
			foreach( DataColumn col in elements )
			{
				//<xs:element name=ColumnName type=MappedType Ordinal=index>
				writer.WriteStartElement( XmlConstants.SchemaPrefix,
			                         XmlConstants.Element,
			                         XmlConstants.SchemaNamespace );
				
				writer.WriteAttributeString( XmlConstants.Name, col.ColumnName );
				writer.WriteAttributeString( XmlConstants.Type, MapType( col.DataType ) );
				if( col.AllowDBNull )
				{
					writer.WriteAttributeString( XmlConstants.MinOccurs, "0" );
				}
				writer.WriteAttributeString( XmlConstants.MsdataPrefix,
				                            XmlConstants.Ordinal,
				                            XmlConstants.MsdataNamespace,
				                            col.Ordinal.ToString() );
				
				//</xs:element>
				writer.WriteEndElement();
			}
			//</xs:sequence>
			writer.WriteEndElement();
				
			}
			//Then a list of attributes
			foreach( DataColumn col in atts )
			{
				//<xs:attribute name=col.ColumnName form="unqualified" type=MappedType/>
				writer.WriteStartElement( XmlConstants.SchemaPrefix,
			                         XmlConstants.Attribute,
			                         XmlConstants.SchemaNamespace );
				
				writer.WriteAttributeString( XmlConstants.Name, col.ColumnName );
				writer.WriteAttributeString( XmlConstants.Form, XmlConstants.Unqualified );
				writer.WriteAttributeString( XmlConstants.Type, MapType( col.DataType ) );
				
				writer.WriteEndElement();
			}
			
			//</xs:complexType>
			writer.WriteEndElement();
			
			//</xs:element>
			writer.WriteEndElement();
		}

		[MonoTODO]
		private void DoReadXmlSchema(XmlReader reader)
		{
			DataTable dt = new DataTable ();

			// FIXME: add OnXmlSchemaValidation when ready.
			XmlSchema schema = XmlSchema.Read (reader, null);
			XmlSchemaObjectCollection SimpleTypes = new XmlSchemaObjectCollection ();
			XmlSchemaObjectCollection ComplexTypes = new XmlSchemaObjectCollection ();
			
			// Get types
			for (int i = 0; i < schema.Items.Count; i++) {

				XmlSchemaSimpleType simpleType = null;
				XmlSchemaComplexType complexType = null;
				if ((simpleType = schema.Items [i] as XmlSchemaSimpleType) != null) {
					SimpleTypes.Add (simpleType);
				} 
				else if ((complexType = schema.Items [i] as XmlSchemaComplexType) != null) {
					ComplexTypes.Add (complexType);
				}
			}
			
			// Get elements
			XmlSchemaObjectCollection Elements = new XmlSchemaObjectCollection ();
			for (int i = 0; i < schema.Items.Count; i++) {

				XmlSchemaElement element = null;
				XmlSchemaComplexType complexType = null;
				if ((element = schema.Items [i] as XmlSchemaElement) != null) {

				        // does element have type defined
					if (!element.SchemaTypeName.IsEmpty) {
						dataSetName = element.Name;
						ReadTypeElement (element, ComplexTypes, ref dt);
					}
					else if ((complexType = element.SchemaType as XmlSchemaComplexType) != null) {
						dataSetName = element.Name;
						ReadColumnsFromSchema (complexType, ComplexTypes, ref dt);
					}

					Tables.Add (dt);
				}
			}		
		}

		[MonoTODO]
		private bool ReadTypeElement (XmlSchemaElement el, XmlSchemaObjectCollection ComplexTypes, 
					      ref DataTable datatable) 
		{
			/*
			 * reads element's type
			 */

			bool found = false;
			
			if (ComplexTypes.Count <= 0)
				return found;
			
			if (ComplexTypes[0] is XmlSchemaComplexType) {

				foreach (XmlSchemaComplexType c in ComplexTypes) {
					
					if (el.SchemaTypeName.ToString () == c.Name.ToString ()) {
						
						found = true;
						ReadColumnsFromSchema (c, ComplexTypes, ref datatable);
					}								
				}
			} 

			return found;
		}

		[MonoTODO]
		private void ReadColumnsFromSchema (XmlSchemaComplexType c, XmlSchemaObjectCollection ComplexTypes, 
						    ref DataTable datatable)
		{
			// FIXME: There is so much work to do. And i dont event know is this right way but here it is
			XmlSchemaSequence seq = null;
			XmlSchemaChoice choice = null;
			ArrayList names = new ArrayList ();

			if ((seq = c.Particle as XmlSchemaSequence) != null) {

				for (int i = 0; i < seq.Items.Count; i++) {

					XmlSchemaElement element = null;
					if ((element = seq.Items [i] as XmlSchemaElement) != null) {

						// first is table name and after that comes column names
						if (datatable.TableName == string.Empty)
							datatable.TableName = element.Name;						
						else if (i + 1 > datatable.Columns.Count)
							datatable.Columns.Add (element.Name);

						ReadTypeElement (element, ComplexTypes, ref datatable);
					}
				}
			}
			else if ((choice = c.Particle as XmlSchemaChoice) != null) {

				foreach (XmlSchemaObject obj in choice.Items) {
					XmlSchemaElement e = null;

					if ((e = obj as XmlSchemaElement) != null) {

						
						// if element has type we got to find out what is it
						if (e.SchemaType != null) {
							datatable.TableName = e.Name;
							ReadColumnsFromSchema (e.SchemaType as XmlSchemaComplexType, ComplexTypes, 
								       ref datatable);
						}
						else if (!e.RefName.IsEmpty) {
							
							// FIXME: this is wrong way to do. We should find element which is
							// referenced by e. I do that later
							dataSetName = e.Name;
							datatable.TableName = e.RefName.ToString ();
						}
					}
				}
			}
		}

		#endregion

		[MonoTODO]
		private void OnXmlSchemaValidation (object sender, ValidationEventArgs args)
		{
		}
	
		///<summary>
		/// Helper function to split columns into attributes elements and simple
		/// content
		/// </summary>
		private void SplitColumns( 	DataTable table,
									out ArrayList atts,
									out ArrayList elements,
									out DataColumn simple)
		{
			//The columns can be attributes, hidden, elements, or simple content
			//There can be 0-1 simple content cols or 0-* elements
			atts = new System.Collections.ArrayList();
			elements = new System.Collections.ArrayList();
			simple = null;
			
			//Sort out the columns
			foreach( DataColumn col in table.Columns )
			{
				switch( col.ColumnMapping )
				{
					case MappingType.Attribute:
						atts.Add( col );
						break;
					case MappingType.Element:
						elements.Add( col );
						break;
					case MappingType.SimpleContent:
						if( simple != null )
						{
							throw new System.InvalidOperationException( "There may only be one simple content element" );
						}
						simple = col;
						break;
					default:
						//ignore Hidden elements
						break;
				}
			}
		}
		
		private string MapType( Type type )
		{
			//TODO - 
			return "xs:string";
		}
	}
}
