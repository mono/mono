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
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002, 2003
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
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.DataSetDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]

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
		internal XmlDataDocument _xmlDataDocument = null;
		
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

		protected DataSet (SerializationInfo info, StreamingContext context) : this ()
		{
			GetSerializationData (info, context);
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
							// first assert all unique constraints
							foreach (UniqueConstraint uc in table.Constraints.UniqueConstraints)
								uc.AssertConstraint ();
							// then assert all foreign keys
							foreach (ForeignKeyConstraint fk in table.Constraints.ForeignKeyConstraints)
								fk.AssertConstraint ();
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
			get { return _namespace; } 
			set {
				//TODO - trigger an event if this happens?
				if (value == null)
					value = String.Empty;
				 if (value != this._namespace)
                                        RaisePropertyChanging ("Namespace");
 				_namespace = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the prefix of the namespace used for this DataSet.")]
		[DefaultValue ("")]
		public string Prefix {
			get { return prefix; } 
			set {
				if (value == null)
					value = String.Empty;
                              // Prefix cannot contain any special characters other than '_' and ':'
                               for (int i = 0; i < value.Length; i++) {
                                       if (!(Char.IsLetterOrDigit (value [i])) && (value [i] != '_') && (value [i] != ':'))
                                               throw new DataException ("Prefix '" + value + "' is not valid, because it contains special characters.");
                               }


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

                /// <summary>
                /// Clears all the tables
                /// </summary>
		public void Clear ()
		{
			if (_xmlDataDocument != null)
				throw new NotSupportedException ("Clear function on dataset and datatable is not supported when XmlDataDocument is bound to the DataSet.");
                        bool enforceConstraints = this.EnforceConstraints;
                        this.EnforceConstraints = false;
                        for (int t = 0; t < tableCollection.Count; t++) {
				tableCollection [t].Clear ();
			}
                        this.EnforceConstraints = enforceConstraints;
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

		private void CopyProperties (DataSet Copy)
		{
			Copy.CaseSensitive = CaseSensitive;
			//Copy.Container = Container
			Copy.DataSetName = DataSetName;
			//Copy.DefaultViewManager
			//Copy.DesignMode
			Copy.EnforceConstraints = EnforceConstraints;
			if(ExtendedProperties.Count > 0) {
				//  Cannot copy extended properties directly as the property does not have a set accessor
                Array tgtArray = Array.CreateInstance( typeof (object), ExtendedProperties.Count);
                ExtendedProperties.Keys.CopyTo (tgtArray, 0);
                for (int i=0; i < ExtendedProperties.Count; i++)
					Copy.ExtendedProperties.Add (tgtArray.GetValue (i), ExtendedProperties[tgtArray.GetValue (i)]);
			}
            Copy.Locale = Locale;
			Copy.Namespace = Namespace;
			Copy.Prefix = Prefix;			
			//Copy.Site = Site; // FIXME : Not sure of this.

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

#if NET_2_0
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
			WriteXml (Writer, XmlWriteMode.IgnoreSchema);
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

		public void InferXmlSchema (XmlReader reader, string[] nsArray)
		{
			if (reader == null)
				return;
			XmlDocument doc = new XmlDocument ();
			doc.Load (reader);
			InferXmlSchema (doc, nsArray);
		}

		private void InferXmlSchema (XmlDocument doc, string [] nsArray)
		{
			XmlDataInferenceLoader.Infer (this, doc, XmlReadMode.InferSchema, nsArray);
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

#if NET_2_0
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
			writer.WriteStartDocument (true);
			try {
				WriteXml (writer);
			}
			finally {
				writer.WriteEndDocument ();
				writer.Close ();
			}
		}

		public void WriteXml (TextWriter writer)
		{
			XmlTextWriter xwriter = new XmlTextWriter (writer);
			xwriter.Formatting = Formatting.Indented;
			WriteXml (xwriter);
		}

		public void WriteXml (XmlWriter writer)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema);
		}

		public void WriteXml (string filename, XmlWriteMode mode)
		{
			XmlTextWriter writer = new XmlTextWriter (filename, null);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument (true);
			
			try {
				WriteXml (writer, mode);
			}
			finally {
				writer.WriteEndDocument ();
				writer.Close ();
			}
		}

		public void WriteXml (Stream stream, XmlWriteMode mode)
		{
			XmlTextWriter writer = new XmlTextWriter (stream, null);
			writer.Formatting = Formatting.Indented;
			WriteXml (writer, mode);
		}

		public void WriteXml (TextWriter writer, XmlWriteMode mode)
		{
			XmlTextWriter xwriter = new XmlTextWriter (writer);
			xwriter.Formatting = Formatting.Indented;
			WriteXml (xwriter, mode);
		}

		public void WriteXml (XmlWriter writer, XmlWriteMode mode)
		{
			if (mode == XmlWriteMode.DiffGram) {
				SetRowsID();
				WriteDiffGramElement(writer);
			}
			
			// It should not write when there is no content to be written
			bool shouldOutputContent = (mode != XmlWriteMode.DiffGram);
			for (int n = 0; n < tableCollection.Count && !shouldOutputContent; n++)
				shouldOutputContent = tableCollection [n].Rows.Count > 0;
				
			if (shouldOutputContent) {
				WriteStartElement (writer, mode, Namespace, Prefix, XmlConvert.EncodeName (DataSetName));
				
				if (mode == XmlWriteMode.WriteSchema)
					DoWriteXmlSchema (writer);
				
				WriteTables (writer, mode, Tables, DataRowVersion.Default);
				writer.WriteEndElement ();
			}
			
			if (mode == XmlWriteMode.DiffGram) {
				if (HasChanges(DataRowState.Modified | DataRowState.Deleted)) {

					DataSet beforeDS = GetChanges (DataRowState.Modified | DataRowState.Deleted);
					WriteStartElement (writer, XmlWriteMode.DiffGram, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "before");
					WriteTables (writer, mode, beforeDS.Tables, DataRowVersion.Original);
					writer.WriteEndElement ();
				}
			}
			
			if (mode == XmlWriteMode.DiffGram)
				writer.WriteEndElement (); // diffgr:diffgram

			writer.Flush ();
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
			try {
				writer.Formatting = Formatting.Indented;
				writer.WriteStartDocument (true);
				WriteXmlSchema (writer);
			} finally {
				writer.WriteEndDocument ();
				writer.Close ();
			}
		}

		public void WriteXmlSchema (TextWriter writer)
		{
			XmlTextWriter xwriter = new XmlTextWriter (writer);
			try {
				xwriter.Formatting = Formatting.Indented;
//				xwriter.WriteStartDocument ();
				WriteXmlSchema (xwriter);
			} finally {
//				xwriter.WriteEndDocument ();
				xwriter.Close ();
			}
		}

		public void WriteXmlSchema (XmlWriter writer)
		{
			//Create a skeleton doc and then write the schema 
			//proper which is common to the WriteXml method in schema mode
			DoWriteXmlSchema (writer);
		}

		public void ReadXmlSchema (Stream stream)
		{
			XmlReader reader = new XmlTextReader (stream, null);
			ReadXmlSchema (reader);
		}

		public void ReadXmlSchema (string str)
		{
			XmlReader reader = new XmlTextReader (str);
			try {
				ReadXmlSchema (reader);
			}
			finally {
				reader.Close ();
			}
		}

		public void ReadXmlSchema (TextReader treader)
		{
			XmlReader reader = new XmlTextReader (treader);
			ReadXmlSchema (reader);			
		}

		public void ReadXmlSchema (XmlReader reader)
		{
#if true
			new XmlSchemaDataImporter (this, reader).Process ();
#else
			XmlSchemaMapper SchemaMapper = new XmlSchemaMapper (this);
			SchemaMapper.Read (reader);
#endif
		}

		public XmlReadMode ReadXml (Stream stream)
		{
			return ReadXml (new XmlTextReader (stream));
		}

		public XmlReadMode ReadXml (string str)
		{
			XmlTextReader reader = new XmlTextReader (str);
			try {
				return ReadXml (reader);
			}
			finally {
				reader.Close ();
			}
		}

		public XmlReadMode ReadXml (TextReader reader)
		{
			return ReadXml (new XmlTextReader (reader));
		}

		public XmlReadMode ReadXml (XmlReader r)
		{
			return ReadXml (r, XmlReadMode.Auto);
		}

		public XmlReadMode ReadXml (Stream stream, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (stream), mode);
		}

		public XmlReadMode ReadXml (string str, XmlReadMode mode)
		{
			XmlTextReader reader = new XmlTextReader (str);
			try {
				return ReadXml (reader, mode);
			}
			finally {
				reader.Close ();
			}
		}

		public XmlReadMode ReadXml (TextReader reader, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (reader), mode);
		}

		public XmlReadMode ReadXml (XmlReader reader, XmlReadMode mode)
		{
			switch (reader.ReadState) {
			case ReadState.EndOfFile:
			case ReadState.Error:
			case ReadState.Closed:
				return mode;
			}
			// Skip XML declaration and prolog
			reader.MoveToContent();
			if (reader.EOF)
				return mode;

			XmlReadMode Result = mode;

			// If diffgram, then read the first element as diffgram 
			if (reader.LocalName == "diffgram" && reader.NamespaceURI == XmlConstants.DiffgrNamespace) {
				switch (mode) {
				case XmlReadMode.Auto:
				case XmlReadMode.DiffGram:
					XmlDiffLoader DiffLoader = new XmlDiffLoader (this);
					DiffLoader.Load (reader);
					// (and leave rest of the reader as is)
					return  XmlReadMode.DiffGram;
				case XmlReadMode.Fragment:
					reader.Skip ();
					// (and continue to read)
					break;
				default:
					reader.Skip ();
					// (and leave rest of the reader as is)
					return mode;
				}
			}
			// If schema, then read the first element as schema 
			if (reader.LocalName == "schema" && reader.NamespaceURI == XmlSchema.Namespace) {
				switch (mode) {
				case XmlReadMode.IgnoreSchema:
				case XmlReadMode.InferSchema:
					reader.Skip ();
					// (and break up read)
					return mode;
				case XmlReadMode.Fragment:
					ReadXmlSchema (reader);
					// (and continue to read)
					break;
				case XmlReadMode.Auto:
					if (Tables.Count == 0) {
						ReadXmlSchema (reader);
						return XmlReadMode.ReadSchema;
					} else {
					// otherwise just ignore and return IgnoreSchema
						reader.Skip ();
						return XmlReadMode.IgnoreSchema;
					}
				default:
					ReadXmlSchema (reader);
					// (and leave rest of the reader as is)
					return mode; // When DiffGram, return DiffGram
				}
			}
			// Otherwise, read as dataset... but only when required.
			XmlReadMode explicitReturnMode = XmlReadMode.Auto;
			XmlDocument doc;
			switch (mode) {
			case XmlReadMode.Auto:
				if (Tables.Count > 0)
					goto case XmlReadMode.IgnoreSchema;
				else
					goto case XmlReadMode.InferSchema;
			case XmlReadMode.InferSchema:
				doc = new XmlDocument ();
				do {
					doc.AppendChild (doc.ReadNode (reader));
					reader.MoveToContent ();
					if (doc.DocumentElement != null)
						break;
				} while (!reader.EOF);
				InferXmlSchema (doc, null);
				reader = new XmlNodeReader (doc);
				explicitReturnMode = XmlReadMode.InferSchema;
				break;
			case XmlReadMode.ReadSchema:
				doc = new XmlDocument ();
				do {
					doc.AppendChild (doc.ReadNode (reader));
					reader.MoveToContent ();
					if (doc.DocumentElement != null)
						break;
				} while (!reader.EOF);
				if (doc.DocumentElement != null) {
					XmlElement schema = doc.DocumentElement ["schema", XmlSchema.Namespace] as XmlElement;
					if (schema != null) {
						ReadXmlSchema (new XmlNodeReader (schema));
						explicitReturnMode = XmlReadMode.ReadSchema;
					}
				}
				reader = new XmlNodeReader (doc);
				break;
			case XmlReadMode.IgnoreSchema:
			case XmlReadMode.Fragment:
				break;
			default:
				reader.Skip ();
				return mode;
			}

			XmlDataReader.ReadXml (this, reader, mode);
			if (explicitReturnMode != XmlReadMode.Auto)
				return explicitReturnMode;
			return mode == XmlReadMode.Auto ? XmlReadMode.IgnoreSchema : mode;
		}
		#endregion // Public Methods

		#region Public Events

		[DataCategory ("Action")]
		[DataSysDescription ("Occurs when it is not possible to merge schemas for two tables with the same name.")]
		public event MergeFailedEventHandler MergeFailed;

		#endregion // Public Events

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
			StringWriter sw = new StringWriter ();
			XmlTextWriter writer = new XmlTextWriter (sw);
			DoWriteXmlSchema (writer);
			writer.Flush ();
			si.AddValue ("XmlSchema", sw.ToString ());
			
			sw = new StringWriter ();
			writer = new XmlTextWriter (sw);
			WriteXml (writer, XmlWriteMode.DiffGram);
			writer.Flush ();
			si.AddValue ("XmlDiffGram", sw.ToString ());
		}
		#endregion
		
		#region Protected Methods
		protected void GetSerializationData (SerializationInfo info, StreamingContext context)
		{
			string s = info.GetValue ("XmlSchema", typeof (String)) as String;
			XmlTextReader reader = new XmlTextReader (new StringReader (s));
			ReadXmlSchema (reader);
			reader.Close ();
			
			s = info.GetValue ("XmlDiffGram", typeof (String)) as String;
			reader = new XmlTextReader (new StringReader (s));
			ReadXml (reader, XmlReadMode.DiffGram);
			reader.Close ();
		}
		
		
		protected virtual System.Xml.Schema.XmlSchema GetSchemaSerializable ()
		{
			return null;
		}
		
		protected virtual void ReadXmlSerializable (XmlReader reader)
		{
			reader.MoveToContent ();
			reader.ReadStartElement ();
			reader.MoveToContent ();
			ReadXmlSchema (reader);
			reader.MoveToContent ();
			ReadXml (reader, XmlReadMode.DiffGram);
			reader.MoveToContent ();
			reader.ReadEndElement ();
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			ReadXmlSerializable(reader);
		}
		
		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			DoWriteXmlSchema (writer);
			WriteXml (writer, XmlWriteMode.DiffGram);
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return BuildSchema ();
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

		internal virtual void OnMergeFailed (MergeFailedEventArgs e)
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
			if (o is byte[]) return Convert.ToBase64String ((byte[])o);
			return o.ToString ();
		}
		
		private void WriteTables (XmlWriter writer, XmlWriteMode mode, DataTableCollection tableCollection, DataRowVersion version)
		{
			//Write out each table in order, providing it is not
			//part of another table structure via a nested parent relationship
			foreach (DataTable table in tableCollection) {
				bool isTopLevel = true;
				/*
				foreach (DataRelation rel in table.ParentRelations) {
					if (rel.Nested) {
						isTopLevel = false;
						break;
					}
				}
				*/
				if (isTopLevel) {
					WriteTable ( writer, table, mode, version);
				}
			}
		}

		private void WriteTable (XmlWriter writer, DataTable table, XmlWriteMode mode, DataRowVersion version)
		{
			DataRow[] rows = new DataRow [table.Rows.Count];
			table.Rows.CopyTo (rows, 0);
			WriteTable (writer, rows, mode, version, true);
		}

		private void WriteTable (XmlWriter writer, DataRow[] rows, XmlWriteMode mode, DataRowVersion version, bool skipIfNested)
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
			int relationCount = table.ParentRelations.Count;
			DataRelation oneRel = relationCount == 1 ? table.ParentRelations [0] : null;

			foreach (DataRow row in rows) {
				if (skipIfNested) {
					// Skip rows that is a child of any tables.
					switch (relationCount) {
					case 0:
						break;
					case 1:
						if (!oneRel.Nested)
							break;
						if (row.GetParentRow (oneRel) != null)
							continue;
						break;
					case 2:
						bool skip = false;
						for (int i = 0; i < table.ParentRelations.Count; i++) {
							DataRelation prel = table.ParentRelations [i];
							if (!prel.Nested)
								continue;
							if (row.GetParentRow (prel) != null) {
								skip = true;
								continue;
							}
						}
						if (skip)
							continue;
						break;
					}
				}

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
					writer.WriteElementString (XmlConvert.EncodeLocalName (table.TableName), "");
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
						WriteColumnAsElement (writer, mode, col, row, version);
					}
				}
				
				foreach (DataRelation relation in table.ChildRelations) {
					if (relation.Nested) {
						WriteTable (writer, row.GetChildRows (relation), mode, version, false);
					}
				}
				
				writer.WriteEndElement ();
			}

		}

		private void WriteColumnAsElement (XmlWriter writer, XmlWriteMode mode, DataColumn col, DataRow row, DataRowVersion version)
		{
			string colnspc = null;
			object rowObject = row [col, version];
									
			if (rowObject == null || rowObject == DBNull.Value)
				return;

			if (col.Namespace != String.Empty)
				colnspc = col.Namespace;
	
			//TODO check if I can get away with write element string
			WriteStartElement (writer, mode, colnspc, col.Prefix, XmlConvert.EncodeLocalName (col.ColumnName));
			writer.WriteString (WriteObjectXml (rowObject));
			writer.WriteEndElement ();
		}

		private void WriteColumnAsAttribute (XmlWriter writer, XmlWriteMode mode, DataColumn col, DataRow row, DataRowVersion version)
		{
			WriteAttributeString (writer, mode, col.Namespace, col.Prefix, XmlConvert.EncodeLocalName (col.ColumnName), WriteObjectXml (row[col, version]));
		}

		private void WriteTableElement (XmlWriter writer, XmlWriteMode mode, DataTable table, DataRow row, DataRowVersion version)
		{
			//sort out the namespacing
			string nspc = table.Namespace.Length > 0 ? table.Namespace : Namespace;

			WriteStartElement (writer, mode, nspc, table.Prefix, XmlConvert.EncodeLocalName (table.TableName));

			if (mode == XmlWriteMode.DiffGram) {
				WriteAttributeString (writer, mode, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "id", table.TableName + (row.XmlRowID + 1));
				WriteAttributeString (writer, mode, XmlConstants.MsdataNamespace, XmlConstants.MsdataPrefix, "rowOrder", XmlConvert.ToString (row.XmlRowID));
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
			writer.WriteStartElement (prefix, name, nspc);
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
		

		private void CheckNamespace (string prefix, string ns, XmlNamespaceManager nsmgr, XmlSchema schema)
		{
			if (ns == String.Empty)
				return;
			if (ns != nsmgr.DefaultNamespace) {
				if (nsmgr.LookupNamespace (nsmgr.NameTable.Get (prefix)) != ns) {
					for (int i = 1; i < int.MaxValue; i++) {
						string p = nsmgr.NameTable.Add ("app" + i);
						if (!nsmgr.HasNamespace (p)) {
							nsmgr.AddNamespace (p, ns);
							HandleExternalNamespace (p, ns, schema);
							break;
						}
					}
				}
			}
		}
		
		XmlSchema BuildSchema ()
		{
			return BuildSchema (Tables, Relations);
		}
		
		internal XmlSchema BuildSchema (DataTableCollection tables, DataRelationCollection relations)
		{
			string constraintPrefix = "";
			XmlSchema schema = new XmlSchema ();
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (new NameTable ());
			
			if (Namespace != "") {
				schema.AttributeFormDefault = XmlSchemaForm.Qualified;
				schema.ElementFormDefault = XmlSchemaForm.Qualified;
				schema.TargetNamespace = Namespace;
				constraintPrefix = XmlConstants.TnsPrefix + ":";
			}

			// set the schema id
			string xmlNSURI = "http://www.w3.org/2000/xmlns/";
			schema.Id = DataSetName;
			XmlDocument doc = new XmlDocument ();
			XmlAttribute attr = null;
			ArrayList atts = new ArrayList ();

			nsmgr.AddNamespace ("xs", XmlSchema.Namespace);
			nsmgr.AddNamespace (XmlConstants.MsdataPrefix, XmlConstants.MsdataNamespace);
			if (Namespace != "") {
				nsmgr.AddNamespace (XmlConstants.TnsPrefix, Namespace);
				nsmgr.AddNamespace (String.Empty, Namespace);
			}
			if (CheckExtendedPropertyExists ())
				nsmgr.AddNamespace (XmlConstants.MspropPrefix, XmlConstants.MspropNamespace);

			if (atts.Count > 0)
				schema.UnhandledAttributes = atts.ToArray (typeof (XmlAttribute)) as XmlAttribute [];

			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = XmlConvert.EncodeName (DataSetName);

			// Add namespaces used in DataSet components (tables, columns, ...)
			foreach (DataTable dt in Tables) {
				foreach (DataColumn col in dt.Columns)
					CheckNamespace (col.Prefix, col.Namespace, nsmgr, schema);
				CheckNamespace (dt.Prefix, dt.Namespace, nsmgr, schema);
			}

			// Attributes for DataSet element
			atts.Clear ();
			attr = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.IsDataSet, XmlConstants.MsdataNamespace);
			attr.Value = "true";
			atts.Add (attr);

			attr = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.Locale, XmlConstants.MsdataNamespace);
			attr.Value = locale.Name;
			atts.Add (attr);

			elem.UnhandledAttributes = atts.ToArray (typeof (XmlAttribute)) as XmlAttribute [];

			AddExtendedPropertyAttributes (elem, ExtendedProperties, doc);

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
				
				if (isTopLevel) {
					if (table.Namespace != SafeNS (schema.TargetNamespace)) {
						XmlSchemaElement extElem = new XmlSchemaElement ();
						extElem.RefName = new XmlQualifiedName (table.TableName, table.Namespace);
						choice.Items.Add (extElem);
					}
					else
						choice.Items.Add (GetTableSchema (doc, table, schema, nsmgr));
				}
			}

			schema.Items.Add (elem);
			
			AddConstraintsToSchema (elem, constraintPrefix, tables, relations, doc);
			foreach (string prefix in nsmgr) {
				string ns = nsmgr.LookupNamespace (nsmgr.NameTable.Get (prefix));
				if (prefix != "xmlns" && prefix != "xml" && ns != null && ns != String.Empty)
					schema.Namespaces.Add (prefix, ns);
			}
			return schema;
		}
		
		private bool CheckExtendedPropertyExists ()
		{
			if (ExtendedProperties.Count > 0)
				return true;
			foreach (DataTable dt in Tables) {
				if (dt.ExtendedProperties.Count > 0)
					return true;
				foreach (DataColumn col in dt.Columns)
					if (col.ExtendedProperties.Count > 0)
						return true;
				foreach (Constraint c in dt.Constraints)
					if (c.ExtendedProperties.Count > 0)
						return true;
			}
			foreach (DataRelation rel in Relations)
				if (rel.ExtendedProperties.Count > 0)
					return true;
			return false;
		}

		// Add all constraints in all tables to the schema.
		private void AddConstraintsToSchema (XmlSchemaElement elem, string constraintPrefix, DataTableCollection tables, DataRelationCollection relations, XmlDocument doc)
		{
			// first add all unique constraints.
			Hashtable uniqueNames = AddUniqueConstraints (elem, constraintPrefix, tables, doc);
			// Add all foriegn key constraints.
			AddForeignKeys (uniqueNames, elem, constraintPrefix, relations, doc);
		}
		
		// Add unique constaraints to the schema.
		// return hashtable with the names of all XmlSchemaUnique elements we created.
		private Hashtable AddUniqueConstraints (XmlSchemaElement elem, string constraintPrefix, DataTableCollection tables, XmlDocument doc)
		{
			Hashtable uniqueNames = new Hashtable();
			foreach (DataTable table in tables) {
				
				foreach (Constraint constraint in table.Constraints) {
					
					if (constraint is UniqueConstraint) {
						ArrayList attrs = new ArrayList ();
						XmlAttribute attrib;
						UniqueConstraint uqConst = (UniqueConstraint) constraint;
						XmlSchemaUnique uniq = new XmlSchemaUnique ();
						
						// if column of the constraint is hidden do not write the constraint.
						bool isHidden = false;
						foreach (DataColumn column in uqConst.Columns) {
							if (column.ColumnMapping == MappingType.Hidden) {
								isHidden = true;
								break;
							}
						}

						if (isHidden)
							continue;

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
							string typePrefix = column.ColumnMapping == MappingType.Attribute ? "@" : "";
							field.XPath = typePrefix + constraintPrefix + column.ColumnName;
							uniq.Fields.Add(field);
						}
				
						AddExtendedPropertyAttributes (uniq, constraint.ExtendedProperties, doc);

						elem.Constraints.Add (uniq);
						uniqueNames.Add (uniq.Name, null);
					}
				}
			}
			return uniqueNames;
		}
		
		// Add the foriegn keys to the schema.
		private void AddForeignKeys (Hashtable uniqueNames, XmlSchemaElement elem, string constraintPrefix, DataRelationCollection relations, XmlDocument doc)
		{
			if (relations == null) return;
			
			foreach (DataRelation rel in relations) {
				
				if (rel.ParentKeyConstraint == null || rel.ChildKeyConstraint == null)
					continue;

				bool isHidden = false;
				foreach (DataColumn col in rel.ParentColumns) {
					if (col.ColumnMapping == MappingType.Hidden) {
						isHidden = true;
						break;
					}
				}
				foreach (DataColumn col in rel.ChildColumns) {
					if (col.ColumnMapping == MappingType.Hidden) {
						isHidden = true;
						break;
					}
				}
				if (isHidden)
					continue;
				
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
					string typePrefix = column.ColumnMapping == MappingType.Attribute ? "@" : "";
					field.XPath = typePrefix + constraintPrefix + column.ColumnName;
					keyRef.Fields.Add(field);
				}

				keyRef.UnhandledAttributes = (XmlAttribute[])attrs.ToArray (typeof (XmlAttribute));
				AddExtendedPropertyAttributes (keyRef, rel.ExtendedProperties, doc);

				elem.Constraints.Add (keyRef);
			}
		}

		private XmlSchemaElement GetTableSchema (XmlDocument doc, DataTable table, XmlSchema schemaToAdd, XmlNamespaceManager nsmgr)
		{
			ArrayList elements;
			ArrayList atts;
			DataColumn simple;
			
			ArrayList xattrs = new ArrayList();
			XmlAttribute xattr;

			SplitColumns (table, out atts, out elements, out simple);

			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = table.TableName;

			XmlSchemaComplexType complex = new XmlSchemaComplexType ();
			elem.SchemaType = complex;

			XmlSchemaObjectCollection schemaAttributes = null;

			if (simple != null) {
				// add simpleContent
				XmlSchemaSimpleContent simpleContent = new XmlSchemaSimpleContent();
				complex.ContentModel = simpleContent;

				// add column name attribute
				XmlAttribute[] xlmAttrs = new XmlAttribute [2];
				xlmAttrs[0] = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.ColumnName, XmlConstants.MsdataNamespace);
				xlmAttrs[0].Value = simple.ColumnName;
				
				// add ordinal attribute
				xlmAttrs[1] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.Ordinal, XmlConstants.MsdataNamespace);
				xlmAttrs[1].Value = XmlConvert.ToString (simple.Ordinal);
				simpleContent.UnhandledAttributes = xlmAttrs;
				
			        
				// add extension
				XmlSchemaSimpleContentExtension extension = new XmlSchemaSimpleContentExtension();
				simpleContent.Content = extension;
				extension.BaseTypeName = MapType (simple.DataType);
				schemaAttributes = extension.Attributes;
			} else {
				schemaAttributes = complex.Attributes;
				//A sequence of element types or a simple content node
				//<xs:sequence>
				XmlSchemaSequence seq = new XmlSchemaSequence ();

				foreach (DataColumn col in elements) {
					
					// Add element for the column.
					XmlSchemaElement colElem = new XmlSchemaElement ();
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
						xattr.Value = XmlConvert.ToString (col.AutoIncrementSeed);
						xattrs.Add (xattr);
					}

					if (col.DefaultValue.ToString () != String.Empty)
						colElem.DefaultValue = WriteObjectXml (col.DefaultValue);

					if (col.ReadOnly) {
						xattr = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.ReadOnly, XmlConstants.MsdataNamespace);
						xattr.Value = "true";
						xattrs.Add (xattr);
					}

					if (col.MaxLength < 0)
						colElem.SchemaTypeName = MapType (col.DataType);
					
					if (colElem.SchemaTypeName == XmlConstants.QnString && col.DataType != typeof (string) 
						&& col.DataType != typeof (char)) {
						xattr = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.DataType, XmlConstants.MsdataNamespace);
						xattr.Value = col.DataType.AssemblyQualifiedName;
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
					AddExtendedPropertyAttributes (colElem, col.ExtendedProperties, doc);
					seq.Items.Add (colElem);
				}

				foreach (DataRelation rel in table.ChildRelations) {
					if (rel.Nested) {
						if (rel.ChildTable.Namespace != SafeNS (schemaToAdd.TargetNamespace)) {
							XmlSchemaElement el = new XmlSchemaElement ();
							el.RefName = new XmlQualifiedName (rel.ChildTable.TableName, rel.ChildTable.Namespace);
						} else {
							XmlSchemaElement el = GetTableSchema (doc, rel.ChildTable, schemaToAdd, nsmgr);
							el.MinOccurs = 0;
							el.MaxOccursString = "unbounded";
							XmlSchemaComplexType ct = (XmlSchemaComplexType) el.SchemaType;
							ct.Name = el.Name;
							el.SchemaType = null;
							el.SchemaTypeName = new XmlQualifiedName (ct.Name, schemaToAdd.TargetNamespace);
							schemaToAdd.Items.Add (ct);
							seq.Items.Add (el);
						}
					}
				}

				if (seq.Items.Count > 0)
					complex.Particle = seq;
			}

			//Then a list of attributes
			foreach (DataColumn col in atts) {
				//<xs:attribute name=col.ColumnName form="unqualified" type=MappedType/>
				XmlSchemaAttribute att = new XmlSchemaAttribute ();
				att.Name = col.ColumnName;
				if (col.Namespace != String.Empty) {
					att.Form = XmlSchemaForm.Qualified;
					string prefix = col.Prefix == String.Empty ? "app" + schemaToAdd.Namespaces.Count : col.Prefix;
					att.Name = prefix + ":" + col.ColumnName;
					// FIXME: Handle prefix mapping correctly.
					schemaToAdd.Namespaces.Add (prefix, col.Namespace);
				}
				if (!col.AllowDBNull)
					att.Use = XmlSchemaUse.Required;

				if (col.MaxLength > -1)
					att.SchemaType = GetTableSimpleType (doc, col);
				else
					att.SchemaTypeName = MapType (col.DataType);
				// FIXME: what happens if extended properties are set on attribute columns??
				if (!col.AllowDBNull)
					att.Use = XmlSchemaUse.Required;
				if (col.DefaultValue.ToString () != String.Empty)
					att.DefaultValue = WriteObjectXml (col.DefaultValue);

				if (col.ReadOnly) {
					xattr = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.ReadOnly, XmlConstants.MsdataNamespace);
					xattr.Value = "true";
					xattrs.Add (xattr);
				}

				att.UnhandledAttributes = xattrs.ToArray (typeof (XmlAttribute)) as XmlAttribute [];

				if (col.MaxLength > -1)
					att.SchemaType = GetTableSimpleType (doc, col);
				else
					att.SchemaTypeName = MapType (col.DataType);
				schemaAttributes.Add (att);
			}

			AddExtendedPropertyAttributes (elem, table.ExtendedProperties, doc);

			return elem;
		}

		private void AddExtendedPropertyAttributes (XmlSchemaAnnotated xsobj, PropertyCollection props, XmlDocument doc)
		{
			ArrayList attList = new ArrayList ();
			XmlAttribute xmlAttr;

			if (xsobj.UnhandledAttributes != null)
				attList.AddRange (xsobj.UnhandledAttributes);

			// add extended properties to xs:element
			foreach (DictionaryEntry de in props) {
				xmlAttr = doc.CreateAttribute (XmlConstants.MspropPrefix, XmlConvert.EncodeName (de.Key.ToString ()), XmlConstants.MspropNamespace);
				xmlAttr.Value = de.Value != null ? WriteObjectXml (de.Value) : String.Empty;
				attList.Add (xmlAttr);
			}
			if (attList.Count > 0)
				xsobj.UnhandledAttributes = attList.ToArray (typeof (XmlAttribute)) as XmlAttribute [];
 		}

		private string SafeNS (string ns)
		{
			return ns != null ? ns : String.Empty;
		}

		private void HandleExternalNamespace (string prefix, string ns, XmlSchema schema)
		{
			foreach (XmlSchemaExternal ext in schema.Includes) {
				XmlSchemaImport imp = ext as XmlSchemaImport;
				if (imp != null && imp.Namespace == ns)
					return; // nothing to do
			}
			XmlSchemaImport i = new XmlSchemaImport ();
			i.Namespace = ns;
			i.SchemaLocation = "_" + prefix + ".xsd";
			schema.Includes.Add (i);
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
			BuildSchema ().Write (writer);
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
			foreach (DataTable Table in Tables)
				SetTableRowsID (Table);
		}
		
		private void SetTableRowsID (DataTable Table)
		{
			int dataRowID = 0;
			foreach (DataRow Row in Table.Rows) {
				Row.XmlRowID = dataRowID;
				dataRowID++;
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
