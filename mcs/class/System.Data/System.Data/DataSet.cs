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
// Copyright (C) Tim Coleman, 2002, 2003
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
using System.Data.Common;

namespace System.Data {

	[ToolboxItem (false)]
	[DefaultProperty ("DataSetName")]
	[Serializable]
	public class DataSet : MarshalByValueComponent, IListSource, 
		ISupportInitialize, ISerializable, IXmlSerializable 
	{
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

		public DataSet () : this ("NewDataSet") 
		{		
		}
		
		public DataSet (string name)
		{
			dataSetName = name;
			tableCollection = new DataTableCollection (this);
			relationCollection = new DataRelationCollection.DataSetRelationCollection (this);
			properties = new PropertyCollection ();
			this.prefix = String.Empty;
			
			this.Locale = CultureInfo.CurrentCulture;
		}

		[MonoTODO]
		protected DataSet (SerializationInfo info, StreamingContext context) : this ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Public Properties

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether comparing strings within the DataSet is case sensitive.")]
		[DefaultValue (false)]
		public bool CaseSensitive {
			get {
				return caseSensitive;
			} 
			set {
				foreach (DataTable T in Tables) {
					if (T.VirginCaseSensitive)
						T.CaseSensitive = value;
				}

				caseSensitive = value; 
				if (!caseSensitive) {
					foreach (DataTable table in Tables) {
						foreach (Constraint c in table.Constraints)
							c.AssertConstraint ();
					}
				}
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
			set { 
				if (value != enforceConstraints) {
					enforceConstraints = value; 
					if (value) {
						foreach (DataTable table in Tables) {
							foreach (Constraint c in table.Constraints)
								c.AssertConstraint ();
						}
					}
				}
			}
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
				for (int i = 0; i < Tables.Count; i++) {
					if (Tables[i].HasErrors)
						return true;
				}
				return false;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates a locale under which to compare strings within the DataSet.")]
		public CultureInfo Locale {
			get {
				return locale;
			}
			set {
				if (locale == null || !locale.Equals (value)) {
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
			if (rows == null)
				throw new ArgumentNullException ("rows");
			if (!IsLegalSchemaAction (missingSchemaAction))
				throw new ArgumentOutOfRangeException ("missingSchemaAction");
			
			MergeManager.Merge (this, rows, preserveChanges, missingSchemaAction);
		}
		
		[MonoTODO]
		public void Merge (DataSet dataSet, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if (dataSet == null)
				throw new ArgumentNullException ("dataSet");
			if (!IsLegalSchemaAction (missingSchemaAction))
				throw new ArgumentOutOfRangeException ("missingSchemaAction");
			
			MergeManager.Merge (this, dataSet, preserveChanges, missingSchemaAction);
		}
		
		[MonoTODO]
		public void Merge (DataTable table, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			if (!IsLegalSchemaAction (missingSchemaAction))
				throw new ArgumentOutOfRangeException ("missingSchemaAction");
			
			MergeManager.Merge (this, table, preserveChanges, missingSchemaAction);
		}

		private static bool IsLegalSchemaAction (MissingSchemaAction missingSchemaAction)
		{
			if (missingSchemaAction == MissingSchemaAction.Add || missingSchemaAction == MissingSchemaAction.AddWithKey
				|| missingSchemaAction == MissingSchemaAction.Error || missingSchemaAction == MissingSchemaAction.Ignore)
				return true;
			return false;
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

				if (value == null)
					value = string.Empty;
				
				if (value != this.prefix) 
					RaisePropertyChanging ("Prefix");
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
		public void AcceptChanges ()
		{
			foreach (DataTable tempTable in tableCollection)
				tempTable.AcceptChanges ();
		}

		public void Clear ()
		{
			// TODO: if currently bound to a XmlDataDocument
			//       throw a NotSupportedException
			for (int t = 0; t < tableCollection.Count; t++) {
				tableCollection[t].Clear ();
			}
		}

		public virtual DataSet Clone ()
		{
			DataSet Copy = new DataSet ();
			CopyProperties (Copy);

			foreach (DataTable Table in Tables) {
				Copy.Tables.Add (Table.Clone ());
			}

			//Copy Relationships between tables after existance of tables
			//and setting properties correctly
			CopyRelations (Copy);
			
			return Copy;
		}

		// Copies both the structure and data for this DataSet.
		public DataSet Copy ()
		{
			DataSet Copy = new DataSet ();
			CopyProperties (Copy);

			// Copy DatSet's tables
			foreach (DataTable Table in Tables) 
				Copy.Tables.Add (Table.Copy ());

			//Copy Relationships between tables after existance of tables
			//and setting properties correctly
			CopyRelations (Copy);

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
			//Copy.Site = Site;

		}
		
		
		private void CopyRelations (DataSet Copy)
		{

			//Creation of the relation contains some of the properties, and the constructor
			//demands these values. instead changing the DataRelation constructor and behaviour the
			//parameters are pre-configured and sent to the most general constructor

			foreach (DataRelation MyRelation in this.Relations) {
				string pTable = MyRelation.ParentTable.TableName;
				string cTable = MyRelation.ChildTable.TableName;
				DataColumn[] P_DC = new DataColumn[MyRelation.ParentColumns.Length]; 
				DataColumn[] C_DC = new DataColumn[MyRelation.ChildColumns.Length];
				int i = 0;
				
				foreach (DataColumn DC in MyRelation.ParentColumns) {
					P_DC[i]=Copy.Tables[pTable].Columns[DC.ColumnName];
					i++;
				}

				i = 0;

				foreach (DataColumn DC in MyRelation.ChildColumns) {
					C_DC[i]=Copy.Tables[cTable].Columns[DC.ColumnName];
					i++;
				}
				
				DataRelation cRel = new DataRelation (MyRelation.RelationName, P_DC, C_DC);
				//cRel.ChildColumns = MyRelation.ChildColumns;
				//cRel.ChildTable = MyRelation.ChildTable;
				//cRel.ExtendedProperties = cRel.ExtendedProperties; 
				//cRel.Nested = MyRelation.Nested;
				//cRel.ParentColumns = MyRelation.ParentColumns;
				//cRel.ParentTable = MyRelation.ParentTable;
								
				Copy.Relations.Add (cRel);
			}
		}

		


		public DataSet GetChanges ()
		{
			return GetChanges (DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
		}

		
		public DataSet GetChanges (DataRowState rowStates)
		{
			if (!HasChanges (rowStates))
				return null;
			
			DataSet copySet = Clone ();
			Hashtable addedRows = new Hashtable ();

			IEnumerator tableEnumerator = Tables.GetEnumerator ();
			DataTable origTable;
			DataTable copyTable;
			while (tableEnumerator.MoveNext ()) {
				origTable = (DataTable)tableEnumerator.Current;
				copyTable = copySet.Tables[origTable.TableName];
				
				// Look for relations that have this table as child
				IEnumerator relations = origTable.ParentRelations.GetEnumerator ();

				IEnumerator rowEnumerator = origTable.Rows.GetEnumerator ();
				while (rowEnumerator.MoveNext ()) {
					DataRow row = (DataRow)rowEnumerator.Current;
					
					if (row.IsRowChanged (rowStates))
						AddChangedRow (addedRows, copySet, copyTable, relations, row);
				}
			}
			return copySet;
		}
		
		void AddChangedRow (Hashtable addedRows, DataSet copySet, DataTable copyTable, IEnumerator relations, DataRow row)
		{
			if (addedRows.ContainsKey (row)) return;
			
			relations.Reset ();
			while (relations.MoveNext ()) {
				DataRow parentRow = row.GetParentRow ((DataRelation) relations.Current);
				if (parentRow == null || addedRows.ContainsKey (parentRow)) continue;
				DataTable parentCopyTable = copySet.Tables [parentRow.Table.TableName];
				AddChangedRow (addedRows, copySet, parentCopyTable, parentRow.Table.ParentRelations.GetEnumerator (), parentRow);
			}
		
			DataRow newRow = copyTable.NewRow ();
			copyTable.Rows.Add (newRow);
			row.CopyValuesToRow (newRow);
			newRow.XmlRowID = row.XmlRowID;
			addedRows.Add (row,row);
		}

#if NET_1_2
		[MonoTODO]
		public DataTableReader GetDataReader (DataTable[] dataTables)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTableReader GetDataReader ()
		{
			throw new NotImplementedException ();
		}
#endif
		
		public string GetXml ()
		{
			StringWriter Writer = new StringWriter ();

			// Sending false for not printing the Processing instruction
			WriteXml (Writer, XmlWriteMode.IgnoreSchema, false);
			return Writer.ToString ();
		}

		public string GetXmlSchema ()
		{
			StringWriter Writer = new StringWriter ();
			WriteXmlSchema (Writer);
			return Writer.ToString ();
		}

		[MonoTODO]
		public bool HasChanges ()
		{
			return HasChanges (DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
		}

		[MonoTODO]
		public bool HasChanges (DataRowState rowState)
		{
			if (((int)rowState & 0xffffffe0) != 0)
				throw new ArgumentOutOfRangeException ("rowState");

			DataTableCollection tableCollection = Tables;
			DataTable table;
			DataRowCollection rowCollection;
			DataRow row;

			for (int i = 0; i < tableCollection.Count; i++) {
				table = tableCollection[i];
				rowCollection = table.Rows;
				for (int j = 0; j < rowCollection.Count; j++) {
					row = rowCollection[j];
					if ((row.RowState & rowState) != 0)
						return true;
				}
			}

			return false;		
		}

		[MonoTODO]
		public void InferXmlSchema (XmlReader reader, string[] nsArray)
		{
		}

		public void InferXmlSchema (Stream stream, string[] nsArray)
		{
			InferXmlSchema (new XmlTextReader (stream), nsArray);
		}

		public void InferXmlSchema (TextReader reader, string[] nsArray)
		{
			InferXmlSchema (new XmlTextReader (reader), nsArray);
		}

		public void InferXmlSchema (string fileName, string[] nsArray)
		{
			XmlTextReader reader = new XmlTextReader (fileName);
			try {
				InferXmlSchema (reader, nsArray);
			} finally {
				reader.Close ();
			}
		}

#if NET_1_2
		[MonoTODO]
		public void Load (IDataReader reader, LoadOption loadOption, DataTable[] tables)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load (IDataReader reader, LoadOption loadOption, string[] tables)
		{
			throw new NotImplementedException ();
		}
#endif

		public virtual void RejectChanges ()
		{
			int i;
			bool oldEnforceConstraints = this.EnforceConstraints;
			this.EnforceConstraints = false;
			
			for (i = 0; i < this.Tables.Count;i++) 
				this.Tables[i].RejectChanges ();

			this.EnforceConstraints = oldEnforceConstraints;
		}

		public virtual void Reset ()
		{
			IEnumerator constraintEnumerator;

			// first we remove all ForeignKeyConstraints (if we will not do that
			// we will get an exception when clearing the tables).
			for (int i = 0; i < Tables.Count; i++) {
				ConstraintCollection cc = Tables[i].Constraints;
				for (int j = 0; j < cc.Count; j++) {
					if (cc[j] is ForeignKeyConstraint)
						cc.Remove (cc[j]);
				}
			}

			Clear ();
			Relations.Clear ();
			Tables.Clear ();
		}

		public void WriteXml (Stream stream)
		{
			XmlTextWriter writer = new XmlTextWriter (stream, null);
			writer.Formatting = Formatting.Indented;
			WriteXml (writer);
		}

		///<summary>
		/// Writes the current data for the DataSet to the specified file.
		/// </summary>
		/// <param name="filename">Fully qualified filename to write to</param>
		public void WriteXml (string fileName)
		{
			XmlTextWriter writer = new XmlTextWriter (fileName, null);
			writer.Formatting = Formatting.Indented;
			WriteXml (writer);
			
			writer.Close ();
		}

		public void WriteXml (TextWriter writer)
		{
			XmlTextWriter xwriter = new XmlTextWriter (writer);
			xwriter.Formatting = Formatting.Indented;
			WriteXml (xwriter);
		}

		public void WriteXml (XmlWriter writer)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema, true);
		}

		public void WriteXml (string filename, XmlWriteMode mode)
		{
			XmlTextWriter writer = new XmlTextWriter (filename, null);
			writer.Formatting = Formatting.Indented;
			WriteXml (writer, mode, true);
		}

		public void WriteXml (Stream stream, XmlWriteMode mode)
		{
			XmlTextWriter writer = new XmlTextWriter (stream, null);
			writer.Formatting = Formatting.Indented;
			WriteXml (writer, mode, true);
		}

		public void WriteXml (TextWriter writer, XmlWriteMode mode)
		{
			XmlTextWriter xwriter = new XmlTextWriter (writer);
			xwriter.Formatting = Formatting.Indented;
			WriteXml (xwriter, mode, true);
		}

		public void WriteXml (XmlWriter writer, XmlWriteMode mode)
		{
			WriteXml (writer, mode, true);
		}
		
		internal void WriteXml (Stream stream, XmlWriteMode mode, bool writePI)
		{
			XmlTextWriter writer = new XmlTextWriter (stream, null);
			writer.Formatting = Formatting.Indented;
			WriteXml (writer, mode, writePI);
		}

		internal void WriteXml (string fileName, XmlWriteMode mode, bool writePI)
		{
			XmlTextWriter writer = new XmlTextWriter (fileName, null);
			writer.Formatting = Formatting.Indented;
			WriteXml (writer, mode, writePI);
			
			writer.Close ();
		}

		internal void WriteXml (TextWriter writer, XmlWriteMode mode, bool writePI)
		{
			XmlTextWriter xwriter = new XmlTextWriter (writer);
			xwriter.Formatting = Formatting.Indented;
			WriteXml (xwriter, mode, writePI);
		}

		internal void WriteXml (XmlWriter writer, XmlWriteMode mode, bool writePI)
		{
			if (writePI && (writer.WriteState == WriteState.Start))
				writer.WriteStartDocument (true);

			if (mode == XmlWriteMode.DiffGram) {
				SetRowsID();
				WriteDiffGramElement(writer);
			}
			
			WriteStartElement (writer, mode, Namespace, Prefix, XmlConvert.EncodeName (DataSetName));
			
			/*********************************************************
			 * This is a patch for interoperability with ms.net.     *
			 * Because in web services the .net client expects this  *
			 * atrribute even if namespace is an empty string        *
			 ********************************************************/
			if (Namespace == null || Namespace.Length == 0)
				WriteAttributeString (writer, mode, null, null, "xmlns", Namespace);
			
			
			if (mode == XmlWriteMode.WriteSchema) {
				DoWriteXmlSchema (writer);
			}
			
			WriteTables (writer, mode, Tables, DataRowVersion.Default);
			if (mode == XmlWriteMode.DiffGram) {
				writer.WriteEndElement (); //DataSet name
				if (HasChanges(DataRowState.Modified | DataRowState.Deleted)) {

					DataSet beforeDS = GetChanges (DataRowState.Modified | DataRowState.Deleted);	
					WriteStartElement (writer, XmlWriteMode.DiffGram, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "before");
					WriteTables (writer, mode, beforeDS.Tables, DataRowVersion.Original);
					writer.WriteEndElement ();
				}
			}
			writer.WriteEndElement (); // DataSet name or diffgr:diffgram
		}

		public void WriteXmlSchema (Stream stream)
		{
			XmlTextWriter writer = new XmlTextWriter (stream, null );
			writer.Formatting = Formatting.Indented;
			WriteXmlSchema (writer);	
		}

		public void WriteXmlSchema (string fileName)
		{
			XmlTextWriter writer = new XmlTextWriter (fileName, null);
			writer.Formatting = Formatting.Indented;
			WriteXmlSchema (writer);
		}

		public void WriteXmlSchema (TextWriter writer)
		{
			XmlTextWriter xwriter = new XmlTextWriter (writer);
			xwriter.Formatting = Formatting.Indented;
			WriteXmlSchema (xwriter);
		}

		public void WriteXmlSchema (XmlWriter writer)
		{
			//Create a skeleton doc and then write the schema 
			//proper which is common to the WriteXml method in schema mode
			writer.WriteStartDocument ();
			DoWriteXmlSchema (writer);
			
			writer.WriteEndDocument ();
		}

		public void ReadXmlSchema (Stream stream)
		{
			XmlReader reader = new XmlTextReader (stream, null);
			ReadXmlSchema (reader);
		}

		public void ReadXmlSchema (string str)
		{
			XmlReader reader = new XmlTextReader (str);
			ReadXmlSchema (reader);
		}

		public void ReadXmlSchema (TextReader treader)
		{
			XmlReader reader = new XmlTextReader (treader);
			ReadXmlSchema (reader);			
		}

		public void ReadXmlSchema (XmlReader reader)
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
			
			// Check if the curent element is the process instruction (PI).
			// if it is move to next element.
			if (r.LocalName == "xml")
				r.MoveToContent();

			/*\
			 *  If document is diffgram we will use diffgram
			\*/
			if (r.LocalName == "diffgram")
				return ReadXml (r, XmlReadMode.DiffGram);
			
			// Get the DataSet name.
			string dataSetName = XmlConvert.DecodeName (r.LocalName);
			DataSetName = dataSetName;
			
			r.ReadStartElement ();
			r.MoveToContent();
			
			bool schemaRead = false;
			// Check if the current element is the schema
			if (r.LocalName == "schema") {
				ReadXmlSchema (r);
				r.MoveToContent();
				schemaRead = true;
			}
			
			if (r.LocalName == "diffgram") {
				return ReadXml (r, XmlReadMode.DiffGram);
			}
			
			// If the schema has been read we should read the rest of the document
			if (schemaRead) {
				ReadXml (r, XmlReadMode.IgnoreSchema, false);
				return XmlReadMode.ReadSchema;
			}
			
			// Read with inferschema.
			return ReadXml (r, XmlReadMode.InferSchema, false);

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
			// we have to initiate the reader.
			if (reader.ReadState == ReadState.Initial)
				reader.Read();
			
			// Check if the curent element is the process instruction (PI).
			// if it is move to next element.
			if (reader.LocalName == "xml")
				reader.MoveToContent();

			XmlReadMode Result = XmlReadMode.Auto;

			if (mode == XmlReadMode.DiffGram) {
				if (reader.LocalName != "diffgram"){
					reader.MoveToContent ();
					reader.ReadStartElement ();	// <DataSet>

					reader.MoveToContent ();
					if (reader.LocalName == "schema")
						ReadXmlSchema (reader);

					reader.MoveToContent ();
				}
				XmlDiffLoader DiffLoader = new XmlDiffLoader (this);
				DiffLoader.Load (reader);
				Result =  XmlReadMode.DiffGram;
			}
			else 
				Result = ReadXml(reader, mode, true);

			return Result;
		}

		private XmlReadMode ReadXml (XmlReader r, XmlReadMode mode, bool readDataSet) {
			
			if (readDataSet) {
				string dataSetName = XmlConvert.DecodeName (r.LocalName);
				DataSetName = dataSetName;
				// get the Namespace of the DataSet.
				string tmp = r.GetAttribute("xmlns");
				if (tmp != null)
					Namespace = tmp;
				
				r.ReadStartElement ();
				r.MoveToContent();
			}

			XmlDataLoader Loader = new XmlDataLoader (this);
			return Loader.LoadData (r, mode);
		}

		#endregion // Public Methods

		#region Public Events

		[DataCategory ("Action")]
		[DataSysDescription ("Occurs when it is not possible to merge schemas for two tables with the same name.")]
		public event MergeFailedEventHandler MergeFailed;

		#endregion // Public Events

		#region Destructors

		~DataSet ()
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
		}
		
		public void EndInit ()
		{
		}
		#endregion

		#region ISerializable
		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Protected Methods
		protected void GetSerializationData (SerializationInfo info, StreamingContext context)
		{
			string s = info.GetValue ("XmlDiffGram", typeof (String)) as String;
			if (s != null) ReadXmlSerializable (new XmlTextReader (new StringReader (s)));
		}
		
		
		protected virtual System.Xml.Schema.XmlSchema GetSchemaSerializable ()
		{
			return BuildSchema ();
		}
		
		protected virtual void ReadXmlSerializable (XmlReader reader)
		{
			ReadXml (reader, XmlReadMode.DiffGram); // FIXME
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{

			ReadXmlSerializable(reader);
			
			// the XmlSerializationReader does this lines!!!
			//reader.MoveToContent ();
			//reader.ReadEndElement ();	// </DataSet>
		}
		
		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			DoWriteXmlSchema (writer);
			WriteXml (writer, XmlWriteMode.DiffGram, true);
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

		protected internal virtual void OnMergeFailed (MergeFailedEventArgs e)
		{
			if (MergeFailed != null)
				MergeFailed (this, e);
		}

		[MonoTODO]
		protected internal void RaisePropertyChanging (string name)
		{
		}
		#endregion

		#region Private Xml Serialisation

		private string WriteObjectXml (object o)
		{
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
			return o.ToString ();
		}
		
		private void WriteTables (XmlWriter writer, XmlWriteMode mode, DataTableCollection tableCollection, DataRowVersion version)
		{
			//Write out each table in order, providing it is not
			//part of another table structure via a nested parent relationship
			foreach (DataTable table in tableCollection) {
				bool isTopLevel = true;
				foreach (DataRelation rel in table.ParentRelations) {
					if (rel.Nested) {
						isTopLevel = false;
						break;
					}
				}
				
				if (isTopLevel) {
					WriteTable ( writer, table, mode, version);
				}
			}
		}

		private void WriteTable (XmlWriter writer, DataTable table, XmlWriteMode mode, DataRowVersion version)
		{
			DataRow[] rows = new DataRow [table.Rows.Count];
			table.Rows.CopyTo (rows, 0);
			WriteTable (writer, rows, mode, version);
		}

		private void WriteTable (XmlWriter writer, DataRow[] rows, XmlWriteMode mode, DataRowVersion version)
		{
			//The columns can be attributes, hidden, elements, or simple content
			//There can be 0-1 simple content cols or 0-* elements
			System.Collections.ArrayList atts;
			System.Collections.ArrayList elements;
			DataColumn simple = null;

			if (rows.Length == 0) return;
			DataTable table = rows[0].Table;
			SplitColumns (table, out atts, out elements, out simple);
			//sort out the namespacing
			string nspc = table.Namespace.Length > 0 ? table.Namespace : Namespace;

			foreach (DataRow row in rows) {
				if (!row.HasVersion(version) || 
				   (mode == XmlWriteMode.DiffGram && row.RowState == DataRowState.Unchanged 
				      && version == DataRowVersion.Original))
					continue;
				
				// First check are all the rows null. If they are we just write empty element
				bool AllNulls = true;
				foreach (DataColumn dc in table.Columns) {
				
					if (row [dc.ColumnName, version] != DBNull.Value) {
						AllNulls = false;
						break;
					} 
				}

				// If all of the columns were null, we have to write empty element
				if (AllNulls) {
					writer.WriteElementString (table.TableName, "");
					continue;
				}
				
				WriteTableElement (writer, mode, table, row, version);
				
				foreach (DataColumn col in atts) {					
					WriteColumnAsAttribute (writer, mode, col, row, version);
				}
				
				if (simple != null) {
					writer.WriteString (WriteObjectXml (row[simple, version]));
				}
				else {					
					foreach (DataColumn col in elements) {
						WriteColumnAsElement (writer, mode, nspc, col, row, version);
					}
				}
				
				foreach (DataRelation relation in table.ChildRelations) {
					if (relation.Nested) {
						WriteTable (writer, row.GetChildRows (relation), mode, version);
					}
				}
				
				writer.WriteEndElement ();
			}

		}

		private void WriteColumnAsElement (XmlWriter writer, XmlWriteMode mode, string nspc, DataColumn col, DataRow row, DataRowVersion version)
		{
			string colnspc = nspc;
			object rowObject = row [col, version];
									
			if (rowObject == null || rowObject == DBNull.Value)
				return;

			if (col.Namespace != null) {
				colnspc = col.Namespace;
			}
	
			//TODO check if I can get away with write element string
			WriteStartElement (writer, mode, colnspc, col.Prefix, col.ColumnName);
			writer.WriteString (WriteObjectXml (rowObject));
			writer.WriteEndElement ();
		}

		private void WriteColumnAsAttribute (XmlWriter writer, XmlWriteMode mode, DataColumn col, DataRow row, DataRowVersion version)
		{
			WriteAttributeString (writer, mode, col.Namespace, col.Prefix, col.ColumnName, row[col, version].ToString ());
		}

		private void WriteTableElement (XmlWriter writer, XmlWriteMode mode, DataTable table, DataRow row, DataRowVersion version)
		{
			//sort out the namespacing
			string nspc = table.Namespace.Length > 0 ? table.Namespace : Namespace;

			WriteStartElement (writer, mode, nspc, table.Prefix, table.TableName);

			if (mode == XmlWriteMode.DiffGram) {
				WriteAttributeString (writer, mode, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "id", table.TableName + (row.XmlRowID + 1));
				WriteAttributeString (writer, mode, XmlConstants.MsdataNamespace, XmlConstants.MsdataPrefix, "rowOrder", row.XmlRowID.ToString());
				string modeName = null;
				if (row.RowState == DataRowState.Modified)
					modeName = "modified";
				else if (row.RowState == DataRowState.Added)
					modeName = "inserted";

				if (version != DataRowVersion.Original && modeName != null)
					WriteAttributeString (writer, mode, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "hasChanges", modeName);
			}
		}
		    
		private void WriteStartElement (XmlWriter writer, XmlWriteMode mode, string nspc, string prefix, string name)
		{
			if (nspc == null || nspc == "") {
				writer.WriteStartElement (name);
			}
			else if (prefix != null) {
				writer.WriteStartElement (prefix, name, nspc);
			}
			else {
				writer.WriteStartElement (writer.LookupPrefix (nspc), name, nspc);
			}
		}
		
		private void WriteAttributeString (XmlWriter writer, XmlWriteMode mode, string nspc, string prefix, string name, string stringValue)
		{
			switch ( mode) {
				case XmlWriteMode.WriteSchema:
					writer.WriteAttributeString (prefix, name, nspc);
					break;
				case XmlWriteMode.DiffGram:
					writer.WriteAttributeString (prefix, name, nspc,stringValue);
					break;
				default:
					writer.WriteAttributeString (name, stringValue);
					break;					
			};
		}
		
		internal void WriteIndividualTableContent (XmlWriter writer, DataTable table, XmlWriteMode mode)
		{
			((XmlTextWriter)writer).Formatting = Formatting.Indented;

			if (mode == XmlWriteMode.DiffGram) {
				SetTableRowsID (table);
				WriteDiffGramElement (writer);
			}
			
			WriteStartElement (writer, mode, Namespace, Prefix, XmlConvert.EncodeName (DataSetName));
			
			WriteTable (writer, table, mode, DataRowVersion.Default);
			
			if (mode == XmlWriteMode.DiffGram) {
				writer.WriteEndElement (); //DataSet name
				if (HasChanges (DataRowState.Modified | DataRowState.Deleted)) {

					DataSet beforeDS = GetChanges (DataRowState.Modified | DataRowState.Deleted);	
					WriteStartElement (writer, XmlWriteMode.DiffGram, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "before");
					WriteTable (writer, beforeDS.Tables [table.TableName], mode, DataRowVersion.Original);
					writer.WriteEndElement ();
				}
			}
			writer.WriteEndElement (); // DataSet name or diffgr:diffgram
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return BuildSchema ();
		}
		
		XmlSchema BuildSchema ()
		{
			return BuildSchema (Tables, Relations);
		}
		
		internal XmlSchema BuildSchema (DataTableCollection tables, DataRelationCollection relations)
		{
			string constraintPrefix = "";
			XmlSchema schema = new XmlSchema ();

			schema.Namespaces.Add("xs", XmlSchema.Namespace);
			schema.Namespaces.Add(XmlConstants.MsdataPrefix, XmlConstants.MsdataNamespace);
			
			if (Namespace != "" && Namespace != null) {
				schema.AttributeFormDefault = XmlSchemaForm.Qualified;
				schema.ElementFormDefault = XmlSchemaForm.Qualified;
				schema.TargetNamespace = Namespace;
				schema.Namespaces.Add(XmlConstants.TnsPrefix, Namespace);
				constraintPrefix = XmlConstants.TnsPrefix + ":";
			}
				
			// set the schema id
			schema.Id = DataSetName;
			XmlDocument doc = new XmlDocument ();
			XmlAttribute xmlnsAttr = doc.CreateAttribute("xmlns");
			xmlnsAttr.Value = Namespace;

			schema.UnhandledAttributes = new XmlAttribute[] {xmlnsAttr};
						
			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = XmlConvert.EncodeName (DataSetName);

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
			
			//Write out schema for each table in order
			foreach (DataTable table in tables) {		
				bool isTopLevel = true;
				foreach (DataRelation rel in table.ParentRelations) {
					if (rel.Nested) {
						isTopLevel = false;
						break;
					}
				}
				
				if (isTopLevel){
					choice.Items.Add (GetTableSchema (doc, table));
				}
			}
			
			AddConstraintsToSchema (elem, constraintPrefix, tables, relations);
			return schema;
		}
		
		// Add all constraints in all tables to the schema.
		private void AddConstraintsToSchema (XmlSchemaElement elem, string constraintPrefix, DataTableCollection tables, DataRelationCollection relations) 
		{
			// first add all unique constraints.
			Hashtable uniqueNames = AddUniqueConstraints (elem, constraintPrefix, tables);
			// Add all foriegn key constraints.
			AddForeignKeys (uniqueNames, elem, constraintPrefix, relations);
		}
		
		// Add unique constaraints to the schema.
		// return hashtable with the names of all XmlSchemaUnique elements we created.
		private Hashtable AddUniqueConstraints (XmlSchemaElement elem, string constraintPrefix, DataTableCollection tables)
		{
			XmlDocument doc = new XmlDocument();
			Hashtable uniqueNames = new Hashtable();
			foreach (DataTable table in tables) {
				
				foreach (Constraint constaint in table.Constraints) {
					
					if (constaint is UniqueConstraint) {
						ArrayList attrs = new ArrayList ();
						XmlAttribute attrib;
						UniqueConstraint uqConst = (UniqueConstraint)constaint;
						XmlSchemaUnique uniq = new XmlSchemaUnique ();
						
						// if constaraint name do not exist in the hashtable we can use it.
						if (!uniqueNames.ContainsKey (uqConst.ConstraintName)) {
							uniq.Name = uqConst.ConstraintName;
						}
						// generate new constraint name for the XmlSchemaUnique element.
						else {
							uniq.Name = uqConst.Table.TableName + "_" + uqConst.ConstraintName;
							attrib = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.ConstraintName, XmlConstants.MsdataNamespace);
							attrib.Value = uqConst.ConstraintName;
							attrs.Add (attrib);
						}
						if (uqConst.IsPrimaryKey) {
							attrib = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.PrimaryKey, XmlConstants.MsdataNamespace);
							attrib.Value = "true";
							attrs.Add (attrib);
						}
		
						uniq.UnhandledAttributes = (XmlAttribute[])attrs.ToArray (typeof (XmlAttribute));

						uniq.Selector = new XmlSchemaXPath();
						uniq.Selector.XPath = ".//"+constraintPrefix + uqConst.Table.TableName;
						XmlSchemaXPath field;
						foreach (DataColumn column in uqConst.Columns) {
				 			field = new XmlSchemaXPath();
							field.XPath = constraintPrefix+column.ColumnName;
							uniq.Fields.Add(field);
						}
				
						elem.Constraints.Add (uniq);
						uniqueNames.Add (uniq.Name, null);
					}
				}
			}
			return uniqueNames;
		}
		
		// Add the foriegn keys to the schema.
		private void AddForeignKeys (Hashtable uniqueNames, XmlSchemaElement elem, string constraintPrefix, DataRelationCollection relations)
		{
			if (relations == null) return;
			
			XmlDocument doc = new XmlDocument();
			foreach (DataRelation rel in relations) {
				
				ArrayList attrs = new ArrayList ();
				XmlAttribute attrib;
				XmlSchemaKeyref keyRef = new XmlSchemaKeyref();
				keyRef.Name = rel.RelationName;
				ForeignKeyConstraint fkConst = rel.ChildKeyConstraint;
				UniqueConstraint uqConst = rel.ParentKeyConstraint;
				
				string concatName = rel.ParentTable.TableName + "_" + uqConst.ConstraintName;
				// first try to find the concatenated name. If we didn't find it - use constraint name.
				if (uniqueNames.ContainsKey (concatName)) {
					keyRef.Refer = new XmlQualifiedName(concatName);
				}
				else {
					keyRef.Refer = new XmlQualifiedName(uqConst.ConstraintName);
				}

				if (rel.Nested)	{
					attrib = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.IsNested, XmlConstants.MsdataNamespace);
					attrib.Value = "true";
					attrs.Add (attrib);
				}

				keyRef.Selector = new XmlSchemaXPath();
				keyRef.Selector.XPath = ".//" + constraintPrefix + rel.ChildTable.TableName;
				XmlSchemaXPath field;
				foreach (DataColumn column in rel.ChildColumns)	{
				 	field = new XmlSchemaXPath();
					field.XPath = constraintPrefix+column.ColumnName;
					keyRef.Fields.Add(field);
				}
				keyRef.UnhandledAttributes = (XmlAttribute[])attrs.ToArray (typeof (XmlAttribute));
				elem.Constraints.Add (keyRef);
			}
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
			if (elements.Count == 0) {				
			}
			else {
				//A sequence of element types or a simple content node
				//<xs:sequence>
				XmlSchemaSequence seq = new XmlSchemaSequence ();
				complex.Particle = seq;

				foreach (DataColumn col in elements) {
					//<xs:element name=ColumnName type=MappedType Ordinal=index>
					XmlSchemaElement colElem = new XmlSchemaElement ();
					ArrayList xattrs = new ArrayList();
					XmlAttribute xattr;
					colElem.Name = col.ColumnName;
				
					if (col.ColumnName != col.Caption && col.Caption != String.Empty) {
						xattr = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.Caption, XmlConstants.MsdataNamespace);
						xattr.Value = col.Caption;
						xattrs.Add (xattr);
					}

					if (col.AutoIncrement == true) {
						xattr = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.AutoIncrement, XmlConstants.MsdataNamespace);
						xattr.Value = "true";
						xattrs.Add (xattr);
					}

					if (col.AutoIncrementSeed != 0) {
						xattr = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.AutoIncrementSeed, XmlConstants.MsdataNamespace);
						xattr.Value = col.AutoIncrementSeed.ToString();
						xattrs.Add (xattr);
					}

					if (col.DefaultValue.ToString () != String.Empty)
						colElem.DefaultValue = col.DefaultValue.ToString ();
					
					if (col.MaxLength < 0)
						colElem.SchemaTypeName = MapType (col.DataType);
					
					if (colElem.SchemaTypeName == XmlConstants.QnString && col.DataType != typeof (string) 
						&& col.DataType != typeof (char)) {
						xattr = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.DataType, XmlConstants.MsdataNamespace);
						xattr.Value = col.DataType.ToString();
						xattrs.Add (xattr);
					}

					if (col.AllowDBNull) {
						colElem.MinOccurs = 0;
					}

					//writer.WriteAttributeString (XmlConstants.MsdataPrefix, 
					//                            XmlConstants.Ordinal, 
					//                            XmlConstants.MsdataNamespace, 
					//                            col.Ordinal.ToString ());

					// Write SimpleType if column have MaxLength
					if (col.MaxLength > -1) {
						colElem.SchemaType = GetTableSimpleType (doc, col);
					}
					
					colElem.UnhandledAttributes = (XmlAttribute[])xattrs.ToArray(typeof (XmlAttribute));
					seq.Items.Add (colElem);
				}

				foreach (DataRelation rel in table.ChildRelations) {
					if (rel.Nested) {
						seq.Items.Add(GetTableSchema (doc, rel.ChildTable));
					}
				}
			}

			//Then a list of attributes
			foreach (DataColumn col in atts) {
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
			
			simple.Content = restriction;
			return simple;
		}

		private void DoWriteXmlSchema (XmlWriter writer)
		{
			GetSchemaSerializable ().Write (writer);
		}
		
		///<summary>
		/// Helper function to split columns into attributes elements and simple
		/// content
		/// </summary>
		private void SplitColumns (DataTable table, 
			out ArrayList atts, 
			out ArrayList elements, 
			out DataColumn simple)
		{
			//The columns can be attributes, hidden, elements, or simple content
			//There can be 0-1 simple content cols or 0-* elements
			atts = new System.Collections.ArrayList ();
			elements = new System.Collections.ArrayList ();
			simple = null;
			
			//Sort out the columns
			foreach (DataColumn col in table.Columns) {
				switch (col.ColumnMapping) {
					case MappingType.Attribute:
						atts.Add (col);
						break;
					case MappingType.Element:
						elements.Add (col);
						break;
					case MappingType.SimpleContent:
						if (simple != null) {
							throw new System.InvalidOperationException ("There may only be one simple content element");
						}
						simple = col;
						break;
					default:
						//ignore Hidden elements
						break;
				}
			}
		}

		private void WriteDiffGramElement(XmlWriter writer)
		{
			WriteStartElement (writer, XmlWriteMode.DiffGram, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "diffgram");
			WriteAttributeString(writer, XmlWriteMode.DiffGram, null, "xmlns", XmlConstants.MsdataPrefix, XmlConstants.MsdataNamespace);
		}

		private void SetRowsID()
		{
			foreach (DataTable Table in Tables) {
				int dataRowID = 0;
				foreach (DataRow Row in Table.Rows) {
					Row.XmlRowID = dataRowID;
					dataRowID++;
				}
			}
		}

		
		private XmlQualifiedName MapType (Type type)
		{
			switch (Type.GetTypeCode (type)) {
				case TypeCode.String: return XmlConstants.QnString;
				case TypeCode.Int16: return XmlConstants.QnShort;
				case TypeCode.Int32: return XmlConstants.QnInt;
				case TypeCode.Int64: return XmlConstants.QnLong;
				case TypeCode.Boolean: return XmlConstants.QnBoolean;
				case TypeCode.Byte: return XmlConstants.QnUnsignedByte;
				//case TypeCode.Char: return XmlConstants.QnChar;
				case TypeCode.DateTime: return XmlConstants.QnDateTime;
				case TypeCode.Decimal: return XmlConstants.QnDecimal;
				case TypeCode.Double: return XmlConstants.QnDouble;
				case TypeCode.SByte: return XmlConstants.QnSbyte;
				case TypeCode.Single: return XmlConstants.QnFloat;
				case TypeCode.UInt16: return XmlConstants.QnUsignedShort;
				case TypeCode.UInt32: return XmlConstants.QnUnsignedInt;
				case TypeCode.UInt64: return XmlConstants.QnUnsignedLong;
			}
			
			if (typeof (TimeSpan) == type)
				return XmlConstants.QnDuration;
			else if (typeof (System.Uri) == type)
				return XmlConstants.QnUri;
			else if (typeof (byte[]) == type)
				return XmlConstants.QnBase64Binary;
			else if (typeof (XmlQualifiedName) == type)
				return XmlConstants.QnXmlQualifiedName;
			else
				return XmlConstants.QnString;
		}

		#endregion //Private Xml Serialisation
	}
}
