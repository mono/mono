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
using System.Xml.Serialization;

namespace System.Data {
	/// <summary>
	/// an in-memory cache of data 
	/// </summary>
	//[Designer]
	[ToolboxItem (false)]
	[DefaultProperty ("DataSetName")]
	[Serializable]
	public class DataSet : MarshalByValueComponent, IListSource,
		ISupportInitialize, ISerializable, IXmlSerializable {
		private string dataSetName;
		private string _namespace = "";
		private string prefix;
		private bool caseSensitive;
		private bool enforceConstraints = true;
		private DataTableCollection tableCollection;
		private DataRelationCollection relationCollection;
		private PropertyCollection properties;
		private DataViewManager defaultView;
		private CultureInfo locale = System.Threading.Thread.CurrentThread.CurrentCulture;
		
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
			return BuildSchema ();
		}
		
		protected virtual void ReadXmlSerializable(XmlReader reader)
		{
			ReadXml(reader, XmlReadMode.DiffGram); // FIXME
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			reader.MoveToContent ();
			reader.ReadStartElement ();	// <DataSet>

			reader.MoveToContent ();
			ReadXmlSchema (reader);

			reader.MoveToContent ();
			ReadXml(reader, XmlReadMode.IgnoreSchema);

			reader.MoveToContent ();
			reader.ReadEndElement ();	// </DataSet>
		}
		
		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			DoWriteXmlSchema (writer);
			WriteXml(writer, XmlWriteMode.IgnoreSchema);
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

		XmlSchema IXmlSerializable.GetSchema()
		{
			return BuildSchema ();
		}
		
		XmlSchema BuildSchema()
		{
			XmlSchema schema = new XmlSchema ();
			schema.AttributeFormDefault = XmlSchemaForm.Qualified;

			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = DataSetName;

			XmlDocument doc = new XmlDocument ();

			XmlAttribute[] atts = new XmlAttribute [2];
			atts[0] = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.IsDataSet, XmlConstants.MsdataNamespace);
			atts[0].Value = "true";

			atts[1] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.Locale, XmlConstants.MsdataNamespace);
			atts[1].Value = locale.Name;
			elem.UnhandledAttributes = atts;

			schema.Items.Add (elem);

			XmlSchemaComplexType complex = new XmlSchemaComplexType ();
			elem.SchemaType = complex;

			XmlSchemaChoice choice = new XmlSchemaChoice ();
			complex.Particle = choice;
			choice.MaxOccursString = XmlConstants.Unbounded;
			
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
					choice.Items.Add (GetTableSchema (doc, table));
				}
			}
			
			//TODO - now add in the relationships as key and unique constraints etc

			return schema;
		}

		private XmlSchemaElement GetTableSchema (XmlDocument doc, DataTable table)
		{
			ArrayList elements;
			ArrayList atts;
			DataColumn simple;
			
			SplitColumns (table, out atts, out elements, out simple);

			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = table.TableName;

			XmlSchemaComplexType complex = new XmlSchemaComplexType ();
			elem.SchemaType = complex;

			//TODO - what about the simple content?
			if( elements.Count == 0 )				
			{				
			}
			else
			{
				//A sequence of element types or a simple content node
				//<xs:sequence>
				XmlSchemaSequence seq = new XmlSchemaSequence ();
				complex.Particle = seq;

				foreach( DataColumn col in elements )
				{
					//<xs:element name=ColumnName type=MappedType Ordinal=index>
					XmlSchemaElement colElem = new XmlSchemaElement ();
					colElem.Name = col.ColumnName;
				
					if (col.ColumnName != col.Caption && col.Caption != string.Empty)
					{
						XmlAttribute[] xatts = new XmlAttribute[1];
						xatts[0] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.Caption, XmlConstants.MsdataNamespace);
						xatts[0].Value = col.Caption;
						colElem.UnhandledAttributes = xatts;
					}

					if (col.DefaultValue.ToString () != string.Empty)
						colElem.DefaultValue = col.DefaultValue.ToString ();

					colElem.SchemaTypeName = MapType (col.DataType);

					if( col.AllowDBNull )
					{
						colElem.MinOccurs = 0;
					}

					//writer.WriteAttributeString( XmlConstants.MsdataPrefix,
					//                            XmlConstants.Ordinal,
					//                            XmlConstants.MsdataNamespace,
					//                            col.Ordinal.ToString() );

					// Write SimpleType if column have MaxLength
					if (col.MaxLength > -1) 
					{
						colElem.SchemaType = GetTableSimpleType (doc, col);
					}

					seq.Items.Add (colElem);
				}
			}

			//Then a list of attributes
			foreach( DataColumn col in atts )
			{
				//<xs:attribute name=col.ColumnName form="unqualified" type=MappedType/>
				XmlSchemaAttribute att = new XmlSchemaAttribute ();
				att.Name = col.ColumnName;
				att.Form = XmlSchemaForm.Unqualified;
				att.SchemaTypeName = MapType (col.DataType);
				complex.Attributes.Add (att);
			}
			return elem;
		}

		private XmlSchemaSimpleType GetTableSimpleType (XmlDocument doc, DataColumn col)
		{
			// SimpleType
			XmlSchemaSimpleType simple = new XmlSchemaSimpleType ();

			// Restriction
			XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction ();
			restriction.BaseTypeName = MapType (col.DataType);
			
			// MaxValue
			XmlSchemaMaxLengthFacet max = new XmlSchemaMaxLengthFacet ();
			max.Value = XmlConvert.ToString (col.MaxLength);
			restriction.Facets.Add (max);

			return simple;
		}

		private void DoWriteXmlSchema( XmlWriter writer )
		{
			GetSchemaSerializable ().Write (writer);
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
		
		private XmlQualifiedName MapType (Type type)
		{
			switch (Type.GetTypeCode (type))
			{
				case TypeCode.String: return XmlConstants.QnString;
				case TypeCode.Int16: return XmlConstants.QnShort;
				case TypeCode.Int32: return XmlConstants.QnInt;
				case TypeCode.Int64: return XmlConstants.QnLong;
				case TypeCode.Boolean: return XmlConstants.QnBoolean;
				case TypeCode.Byte: return XmlConstants.QnUnsignedByte;
				case TypeCode.Char: return XmlConstants.QnChar;
				case TypeCode.DateTime: return XmlConstants.QnDateTime;
				case TypeCode.Decimal: return XmlConstants.QnDecimal;
				case TypeCode.Double: return XmlConstants.QnDouble;
				case TypeCode.SByte: return XmlConstants.QnSbyte;
				case TypeCode.Single: return XmlConstants.QnFloat;
				case TypeCode.UInt16: return XmlConstants.QnUsignedShort;
				case TypeCode.UInt32: return XmlConstants.QnUnsignedInt;
				case TypeCode.UInt64: return XmlConstants.QnUnsignedLong;
			}
			
			if (typeof (TimeSpan) == type) return XmlConstants.QnDuration;
			else if (typeof (System.Uri) == type) return XmlConstants.QnUri;
			else if (typeof (byte[]) == type) return XmlConstants.QnBase64Binary;
			else if (typeof (XmlQualifiedName) == type) return XmlConstants.QnXmlQualifiedName;
			else return XmlConstants.QnString;
		}

		#endregion //Private Xml Serialisation
	}
}
