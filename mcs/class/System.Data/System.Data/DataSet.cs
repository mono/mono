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
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace System.Data {
	/// <summary>
	/// an in-memory cache of data 
	/// </summary>
	//[Designer]
	[ToolboxItem (false)]
	[DefaultProperty ("DataSetName")]
	[Serializable]
	public class DataSet : MarshalByValueComponent, IListSource,
		ISupportInitialize, ISerializable {
		private string dataSetName;
		private string _namespace = "";
		private string prefix;
		private bool caseSensitive;
		private bool enforceConstraints = true;
		private DataTableCollection tableCollection;
		private DataRelationCollection relationCollection;
		private PropertyCollection properties;
		private DataViewManager defaultView;
		private CultureInfo locale;
		
		#region Constructors

		public DataSet() : this ("NewDataSet") {		
		}

		public DataSet(string name) {
			dataSetName = name;
			tableCollection = new DataTableCollection (this);
			relationCollection = new DataRelationCollection.DataSetRelationCollection (this);
			properties = new PropertyCollection();
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
			set {
				foreach (DataTable T in Tables) {
					if (T.VirginCaseSensitive)
						T.CaseSensitive = value;
				}

				caseSensitive = value; 
			}
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
			get {
				return locale;
			}
			set {
				if (locale == null || !locale.Equals(value)) {
					// TODO: check if the new locale is valid
					// TODO: update locale of all tables
					locale = value;
				}
			}
		}

		public void Merge (DataRow[] rows)
		{
			Merge (rows, false, MissingSchemaAction.Add);
		}
		
		public void Merge (DataSet dataSet)
		{
			Merge (dataSet, false, MissingSchemaAction.Add);
		}
		
		public void Merge (DataTable table)
		{
			Merge (table, false, MissingSchemaAction.Add);
		}
		
		public void Merge (DataSet dataSet, bool preserveChanges)
		{
			Merge (dataSet, preserveChanges, MissingSchemaAction.Add);
		}
		
		[MonoTODO]
		public void Merge (DataRow[] rows, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void Merge (DataSet dataSet, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void Merge (DataTable table, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			throw new NotImplementedException();
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
			get {
				return relationCollection;		
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
			// TODO: if currently bound to a XmlDataDocument
			//       throw a NotSupportedException
			for (int t = 0; t < tableCollection.Count; t++) {
				tableCollection[t].Clear ();
			}
		}

		public virtual DataSet Clone()
		{
			DataSet Copy = new DataSet ();
			CopyProperties (Copy);

			foreach (DataTable Table in Tables) {
				Copy.Tables.Add (Table.Clone ());
			}	
			
			return Copy;
		}

		// Copies both the structure and data for this DataSet.
		public DataSet Copy()
		{
			DataSet Copy = new DataSet ();
			CopyProperties (Copy);

			// Copy DatSet's tables
			foreach (DataTable Table in Tables) {				
				Copy.Tables.Add (Table.Copy ());
			}

			return Copy;
		}

		[MonoTODO]
		private void CopyProperties (DataSet Copy)
		{
			Copy.CaseSensitive = CaseSensitive;
			//Copy.Container = Container
			Copy.DataSetName = DataSetName;
			//Copy.DefaultViewManager
			//Copy.DesignMode
			Copy.EnforceConstraints = EnforceConstraints;
			//Copy.ExtendedProperties 
			//Copy.HasErrors
			//Copy.Locale = Locale;
			Copy.Namespace = Namespace;
			Copy.Prefix = Prefix;
			//Copy.Relations = Relations;
			//Copy.Site = Site;

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
			StringWriter Writer = new StringWriter ();
			WriteXml (Writer, XmlWriteMode.IgnoreSchema);
			return Writer.ToString ();
		}

		public string GetXmlSchema()
		{
			StringWriter Writer = new StringWriter ();
			WriteXmlSchema (Writer);
			return Writer.ToString ();
		}

		[MonoTODO]
		public bool HasChanges()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasChanges(DataRowState rowState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InferXmlSchema(XmlReader reader, string[] nsArray)
		{
		}

		public void InferXmlSchema(Stream stream, string[] nsArray)
		{
			InferXmlSchema (new XmlTextReader(stream), nsArray);
		}

		public void InferXmlSchema(TextReader reader, string[] nsArray)
		{
			InferXmlSchema (new XmlTextReader(reader), nsArray);
		}

		public void InferXmlSchema(string fileName, string[] nsArray)
		{
			XmlTextReader reader = new XmlTextReader(fileName);
			try {
				InferXmlSchema (reader, nsArray);
			} finally {
				reader.Close ();
			}
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
			
			writer.Close();
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
			
			writer.Close();
		}

		public void WriteXml(TextWriter writer,	XmlWriteMode mode)
		{
			XmlWriter xwriter = new XmlTextWriter(writer);
			
			WriteXml( xwriter, mode );
		}

		public void WriteXml(XmlWriter writer, XmlWriteMode mode)
		{
			if (writer.WriteState == WriteState.Start)
				writer.WriteStartDocument (true);

			((XmlTextWriter)writer).Formatting = Formatting.Indented;
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
				foreach( DataRelation rel in table.ParentRelations )
				{
					if( rel.Nested )
					{
						isTopLevel = false;
						break;
					}
				}
				
				if( isTopLevel )
				{
					WriteTable(  writer, table, mode );
				}
			}
			
			writer.WriteEndElement();			
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
			((XmlTextWriter)writer).Formatting = Formatting.Indented;
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
			XmlSchemaMapper SchemaMapper = new XmlSchemaMapper (this);
			SchemaMapper.Read (reader);
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

		public XmlReadMode ReadXml (XmlReader r)
		{
			XmlDataLoader Loader = new XmlDataLoader (this);
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
			XmlReadMode Result = XmlReadMode.Auto;

			if (mode == XmlReadMode.DiffGram) {
				XmlDiffLoader DiffLoader = new XmlDiffLoader (this);
				DiffLoader.Load (reader);
				Result =  XmlReadMode.DiffGram;
			}
			else {
				XmlDataLoader Loader = new XmlDataLoader (this);
				Result = Loader.LoadData (reader, mode);
			}

			return Result;
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
		public void BeginInit ()
		{
			throw new NotImplementedException ();
		}
		
		public void EndInit ()
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
		
		#region Protected Methods
		protected void GetSerializationData(SerializationInfo info, StreamingContext context)
		{
			string s = info.GetValue ("XmlDiffGram", typeof (String)) as String;
			if (s != null) ReadXmlSerializable (new XmlTextReader(new StringReader(s)));
		}
		
		
		protected virtual System.Xml.Schema.XmlSchema GetSchemaSerializable()
		{
			return null; // FIXME
		}
		
		protected virtual void ReadXmlSerializable(XmlReader reader)
		{
			ReadXml(reader, XmlReadMode.DiffGram); // FIXME
		}
		
		protected virtual bool ShouldSerializeRelations ()
		{
			return true;
		}
		
		protected virtual bool ShouldSerializeTables ()
		{
			return true;
		}

		[MonoTODO]
		protected internal virtual void OnPropertyChanging (PropertyChangedEventArgs pcevent)
		{
		}

		[MonoTODO]
		protected virtual void OnRemoveRelation (DataRelation relation)
		{
		}

		[MonoTODO]
		protected virtual void OnRemoveTable (DataTable table)
		{
		}

		[MonoTODO]
		protected internal void RaisePropertyChanging (string name)
		{
		}
		#endregion

		#region Private Xml Serialisation

		private string WriteObjectXml( object o ) {
			switch (Type.GetTypeCode (o.GetType ())) {
			case TypeCode.Boolean:
				return XmlConvert.ToString ((Boolean) o);
			case TypeCode.Byte:
				return XmlConvert.ToString ((Byte) o);
			case TypeCode.Char:
				return XmlConvert.ToString ((Char) o);
			case TypeCode.DateTime:
				return XmlConvert.ToString ((DateTime) o);
			case TypeCode.Decimal:
				return XmlConvert.ToString ((Decimal) o);
			case TypeCode.Double:
				return XmlConvert.ToString ((Double) o);
			case TypeCode.Int16:
				return XmlConvert.ToString ((Int16) o);
			case TypeCode.Int32:
				return XmlConvert.ToString ((Int32) o);
			case TypeCode.Int64:
				return XmlConvert.ToString ((Int64) o);
			case TypeCode.SByte:
				return XmlConvert.ToString ((SByte) o);
			case TypeCode.Single:
				return XmlConvert.ToString ((Single) o);
			case TypeCode.UInt16:
				return XmlConvert.ToString ((UInt16) o);
			case TypeCode.UInt32:
				return XmlConvert.ToString ((UInt32) o);
			case TypeCode.UInt64:
				return XmlConvert.ToString ((UInt64) o);
			}
			if (o is TimeSpan) return XmlConvert.ToString ((TimeSpan) o);
			if (o is Guid) return XmlConvert.ToString ((Guid) o);
			return o.ToString();
		}
	
		private void WriteTable( XmlWriter writer, DataTable table, XmlWriteMode mode )
		{
			DataRow[] rows = new DataRow [table.Rows.Count];
			table.Rows.CopyTo (rows, 0);
			WriteTable (writer, rows, mode);
		}

		private void WriteTable( XmlWriter writer, DataRow[] rows, XmlWriteMode mode )
		{
			//The columns can be attributes, hidden, elements, or simple content
			//There can be 0-1 simple content cols or 0-* elements
			System.Collections.ArrayList atts;
			System.Collections.ArrayList elements;
			DataColumn simple = null;

			if (rows.Length == 0) return;
			DataTable table = rows[0].Table;
			SplitColumns( table, out atts, out elements, out simple );

			foreach( DataRow row in rows )
			{
				//sort out the namespacing
				string nspc = table.Namespace.Length > 0 ? table.Namespace : Namespace;

				// First check are all the rows null. If they are we just write empty element
				bool AllNulls = true;
				foreach (DataColumn dc in table.Columns) {
				
					if (row [dc.ColumnName] != DBNull.Value) {
						AllNulls = false;
						break;
					} 
				}

				// If all of the columns were null, we have to write empty element
				if (AllNulls) {
					writer.WriteElementString (table.TableName, "");
					continue;
				}

				WriteStartElement( writer, mode, nspc, table.Prefix, table.TableName );
				
				foreach( DataColumn col in atts )
				{					
					WriteAttributeString( writer, mode, col.Namespace, col.Prefix, col.ColumnName, row[col].ToString() );
				}
				
				if( simple != null )
				{
					writer.WriteString( WriteObjectXml(row[simple]) );
				}
				else
				{					
					foreach( DataColumn col in elements )
					{
						string colnspc = nspc;
						object rowObject = row [col];
												
						if (rowObject == null || rowObject == DBNull.Value)
							continue;

						if( col.Namespace != null )
						{
							colnspc = col.Namespace;
						}
				
						//TODO check if I can get away with write element string
						WriteStartElement( writer, mode, colnspc, col.Prefix, col.ColumnName );
						writer.WriteString( WriteObjectXml(rowObject) );
						writer.WriteEndElement();
					}
				}
				
				foreach (DataRelation relation in table.ChildRelations) {
					if (relation.Nested) {
						WriteTable (writer, row.GetChildRows(relation), mode);
					}
				}
				
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
			writer.WriteStartElement(XmlConstants.SchemaPrefix, XmlConstants.SchemaElement,
										XmlConstants.SchemaNamespace );
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

			writer.WriteAttributeString( XmlConstants.MsdataPrefix, XmlConstants.Locale, XmlConstants.MsdataNamespace, Thread.CurrentThread.CurrentCulture.Name);
			
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
			foreach( DataTable table in Tables )
			{		
				bool isTopLevel = true;
				foreach( DataRelation rel in table.ParentRelations )
				{
					if( rel.Nested )
					{
						isTopLevel = false;
						break;
					}
				}
				
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
				
				if (col.ColumnName != col.Caption && col.Caption != string.Empty)
					writer.WriteAttributeString( XmlConstants.MsdataPrefix, XmlConstants.Caption, 
								     XmlConstants.MsdataNamespace, col.Caption); 

				if (col.DefaultValue.ToString () != string.Empty)
					writer.WriteAttributeString( XmlConstants.Default, col.DefaultValue.ToString ());

				writer.WriteAttributeString( XmlConstants.Type, MapType( col.DataType ) );

				if( col.AllowDBNull )
				{
					writer.WriteAttributeString( XmlConstants.MinOccurs, "0" );
				}

				//writer.WriteAttributeString( XmlConstants.MsdataPrefix,
				//                            XmlConstants.Ordinal,
				//                            XmlConstants.MsdataNamespace,
				//                            col.Ordinal.ToString() );

				// Write SimpleType if column have MaxLength
				if (col.MaxLength > -1) {

					WriteTableSimpleType (writer, col);
				}
				

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

		private void WriteTableSimpleType (XmlWriter writer, DataColumn col)
		{
			// SimpleType
			writer.WriteStartElement( XmlConstants.SchemaPrefix, XmlConstants.SimpleType, 
						  XmlConstants.SchemaNamespace);
			
			// Restriction
			writer.WriteStartElement( XmlConstants.SchemaPrefix, XmlConstants.Restriction, 
						  XmlConstants.SchemaNamespace);
			
			writer.WriteAttributeString( XmlConstants.Base, MapType( col.DataType ) );
			
			// MaxValue
			writer.WriteStartElement( XmlConstants.SchemaPrefix, XmlConstants.MaxLength, 
						  XmlConstants.SchemaNamespace);
			writer.WriteAttributeString( XmlConstants.Value, col.MaxLength.ToString ());
			
			
			writer.WriteEndElement();
			
			writer.WriteEndElement();
			
			writer.WriteEndElement();
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
		
		[MonoTODO]
		private string MapType( Type type )
		{
			string Result = "xs:string";

			// TODO: More types to map?

			if (typeof (string) == type)
				Result = "xs:string";
			else if (typeof (short) == type)
				Result = "xs:short";
			else if (typeof (int) == type)
				Result = "xs:int";
			else if (typeof (long) == type)
				Result = "xs:long";
			else if (typeof (bool) == type)
				Result = "xs:boolean";
			else if (typeof (byte) == type)
				Result = "xs:unsignedByte";
			else if (typeof (char) == type)
				Result = "xs:char";
			else if (typeof (DateTime) == type)
				Result = "xs:dateTime";
			else if (typeof (decimal) == type)
				Result = "xs:decimal";
			else if (typeof (double) == type)
				Result = "xs:double";
			else if (typeof (sbyte) == type)
				Result = "xs:sbyte";
			else if (typeof (Single) == type)
				Result = "xs:float";
			else if (typeof (TimeSpan) == type)
				Result = "xs:duration";
			else if (typeof (ushort) == type)
				Result = "xs:usignedShort";
			else if (typeof (uint) == type)
				Result = "xs:unsignedInt";
			else if (typeof (ulong) == type)
				Result = "xs:unsignedLong";
		
			return Result;
		}
		
		#endregion //Private Xml Serialisation
	}
}
