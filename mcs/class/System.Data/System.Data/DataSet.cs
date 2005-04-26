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
//   Konstantin Triger <kostat@mainsoft.com>
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
						table.ResetCaseSensitiveIndexes();
						foreach (Constraint c in table.Constraints)
							c.AssertConstraint ();
					}
				}
				else {
					foreach (DataTable table in Tables) {
						table.ResetCaseSensitiveIndexes();
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
				InternalEnforceConstraints(value,true);
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

		internal void InternalEnforceConstraints(bool value,bool resetIndexes)
		{
				if (value != enforceConstraints) {
					if (value) {
						foreach (DataTable table in Tables) {
							// FIXME : is that correct?
							// By design  the indexes should be updated at this point.
							// In Fill from BeginLoadData till EndLoadData indexes are not updated (reset in EndLoadData)
							// In DataRow.EndEdit indexes are always updated.
							if (resetIndexes) {
								table.ResetIndexes();
							}
							// assert all constraints
							foreach (Constraint constraint in table.Constraints)
								constraint.AssertConstraint();
						}
					}

					enforceConstraints = value;
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
				tableCollection[t].Clear ();
			}
                        this.EnforceConstraints = enforceConstraints;
		}

		public virtual DataSet Clone ()
		{
			// need to return the same type as this...
			DataSet Copy = (DataSet) Activator.CreateInstance(GetType(), true);
	
			CopyProperties (Copy);

			foreach (DataTable Table in Tables) {
		        // tables are often added in no-args constructor, don't add them
		        // twice.
		        	if (!Copy.Tables.Contains(Table.TableName)) {
			    		Copy.Tables.Add (Table.Clone ());
        			}
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
				WriteXmlSchema (xwriter);
			} finally {
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

		// LAMESPEC: XmlReadMode.Fragment is far from presisely
		// documented. MS.NET infers schema against this mode.
		public XmlReadMode ReadXml (XmlReader input, XmlReadMode mode)
		{
			switch (input.ReadState) {
			case ReadState.EndOfFile:
			case ReadState.Error:
			case ReadState.Closed:
				return mode;
			}
			// Skip XML declaration and prolog
			input.MoveToContent ();
			if (input.EOF)
				return mode;

			// FIXME: We need more decent code here, but for now
			// I don't know the precise MS.NET behavior, I just
			// delegate to specific read process.
			switch (mode) {
			case XmlReadMode.IgnoreSchema:
				return ReadXmlIgnoreSchema (input, mode, true);
			case XmlReadMode.ReadSchema:
				return ReadXmlReadSchema (input, mode, true);
			}
			// remaining modes are: Auto, InferSchema, Fragment, Diffgram

			XmlReader reader = input;

			int depth = reader.Depth;
			XmlReadMode result = mode;
			bool skippedTopLevelElement = false;
			string potentialDataSetName = null;
			XmlDocument doc = null;
			bool shouldReadData = mode != XmlReadMode.DiffGram;
			bool shouldNotInfer = Tables.Count > 0;

			switch (mode) {
			case XmlReadMode.Auto:
			case XmlReadMode.InferSchema:
				doc = new XmlDocument ();
				do {
					doc.AppendChild (doc.ReadNode (reader));
				} while (!reader.EOF &&
					doc.DocumentElement == null);
				reader = new XmlNodeReader (doc);
				reader.MoveToContent ();
				break;
			case XmlReadMode.DiffGram:
				if (!(reader.LocalName == "diffgram" &&
					reader.NamespaceURI == XmlConstants.DiffgrNamespace))
					goto case XmlReadMode.Auto;
				break;
			}

			switch (mode) {
			case XmlReadMode.Auto:
			case XmlReadMode.InferSchema:
			case XmlReadMode.ReadSchema:
				if (!(reader.LocalName == "diffgram" &&
					reader.NamespaceURI == XmlConstants.DiffgrNamespace) &&
					!(reader.LocalName == "schema" &&
					reader.NamespaceURI == XmlSchema.Namespace))
					potentialDataSetName = reader.LocalName;
				goto default;
			case XmlReadMode.Fragment:
				break;
			default:
				if (!(reader.LocalName == "diffgram" &&
					reader.NamespaceURI == XmlConstants.DiffgrNamespace) &&
					!(reader.LocalName == "schema" &&
					reader.NamespaceURI == XmlSchema.Namespace)) {
					if (!reader.IsEmptyElement) {
						reader.Read ();
						reader.MoveToContent ();
						skippedTopLevelElement = true;
					}
					else {
						switch (mode) {
						case XmlReadMode.Auto:
						case XmlReadMode.InferSchema:
							DataSetName = reader.LocalName;
							break;
						}
						reader.Read ();
					}
				}
				break;
			}

			// If schema, then read the first element as schema 
			if (reader.LocalName == "schema" && reader.NamespaceURI == XmlSchema.Namespace) {
				shouldNotInfer = true;
				switch (mode) {
				case XmlReadMode.IgnoreSchema:
				case XmlReadMode.InferSchema:
					reader.Skip ();
					break;
				case XmlReadMode.Fragment:
					ReadXmlSchema (reader);
					break;
				case XmlReadMode.DiffGram:
				case XmlReadMode.Auto:
					if (Tables.Count == 0) {
						ReadXmlSchema (reader);
						if (mode == XmlReadMode.Auto)
							result = XmlReadMode.ReadSchema;
					} else {
					// otherwise just ignore and return IgnoreSchema
						reader.Skip ();
						result = XmlReadMode.IgnoreSchema;
					}
					break;
				case XmlReadMode.ReadSchema:
					ReadXmlSchema (reader);
					break;
				}
			}

			// If diffgram, then read the first element as diffgram 
			if (reader.LocalName == "diffgram" && reader.NamespaceURI == XmlConstants.DiffgrNamespace) {
				switch (mode) {
				case XmlReadMode.Auto:
				case XmlReadMode.IgnoreSchema:
				case XmlReadMode.DiffGram:
					XmlDiffLoader DiffLoader = new XmlDiffLoader (this);
					DiffLoader.Load (reader);
					if (mode == XmlReadMode.Auto)
						result = XmlReadMode.DiffGram;
					shouldReadData = false;
					break;
				case XmlReadMode.Fragment:
					reader.Skip ();
					break;
				default:
					reader.Skip ();
					break;
				}
			}

			// if schema after diffgram, just skip it.
			if (!shouldReadData && reader.LocalName == "schema" && reader.NamespaceURI == XmlSchema.Namespace) {
				shouldNotInfer = true;
				switch (mode) {
				default:
					reader.Skip ();
					break;
				case XmlReadMode.ReadSchema:
				case XmlReadMode.DiffGram:
					if (Tables.Count == 0)
						ReadXmlSchema (reader);
					break;
				}
			}

			if (reader.EOF)
				return result == XmlReadMode.Auto ?
					potentialDataSetName != null && !shouldNotInfer ?
					XmlReadMode.InferSchema :
					XmlReadMode.IgnoreSchema : result;

			// Otherwise, read as dataset... but only when required.
			if (shouldReadData && !shouldNotInfer) {
				switch (mode) {
				case XmlReadMode.Auto:
					if (Tables.Count > 0)
						goto case XmlReadMode.IgnoreSchema;
					else
						goto case XmlReadMode.InferSchema;
				case XmlReadMode.InferSchema:
					InferXmlSchema (doc, null);
					if (mode == XmlReadMode.Auto)
						result = XmlReadMode.InferSchema;
					break;
				case XmlReadMode.IgnoreSchema:
				case XmlReadMode.Fragment:
				case XmlReadMode.DiffGram:
					break;
				default:
					shouldReadData = false;
					break;
				}
			}

			if (shouldReadData) {
				XmlReader dataReader = reader;
				if (doc != null) {
					dataReader = new XmlNodeReader (doc);
					dataReader.MoveToContent ();
				}
				if (reader.NodeType == XmlNodeType.Element)
					XmlDataReader.ReadXml (this, dataReader,
						mode);
			}

			if (skippedTopLevelElement) {
				switch (result) {
				case XmlReadMode.Auto:
				case XmlReadMode.InferSchema:
//					DataSetName = potentialDataSetName;
//					result = XmlReadMode.InferSchema;
					break;
				}
				if (reader.NodeType == XmlNodeType.EndElement)
					reader.ReadEndElement ();
			}
//*
			while (input.Depth > depth)
				input.Read ();
			if (input.NodeType == XmlNodeType.EndElement)
				input.Read ();
//*/
			input.MoveToContent ();

			return result == XmlReadMode.Auto ?
				XmlReadMode.IgnoreSchema : result;
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
			ReadXml (reader);
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
			return GetSchemaSerializable ();
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

		#region Private Methods

		private XmlReadMode ReadXmlIgnoreSchema (XmlReader input, XmlReadMode mode, bool checkRecurse)
		{
			if (input.LocalName == "schema" &&
				input.NamespaceURI == XmlSchema.Namespace) {
				input.Skip ();
			}
			else if (input.LocalName == "diffgram" &&
				input.NamespaceURI == XmlConstants.DiffgrNamespace) {
				XmlDiffLoader DiffLoader = new XmlDiffLoader (this);
				DiffLoader.Load (input);
			}
			else if (checkRecurse ||
				input.LocalName == DataSetName &&
				input.NamespaceURI == Namespace) {
				XmlDataReader.ReadXml (this, input, mode);
			}
			else if (checkRecurse && !input.IsEmptyElement) {
				int depth = input.Depth;
				input.Read ();
				input.MoveToContent ();
				ReadXmlIgnoreSchema (input, mode, false);
				while (input.Depth > depth)
					input.Skip ();
				if (input.NodeType == XmlNodeType.EndElement)
					input.ReadEndElement ();
			}
			input.MoveToContent ();
			return XmlReadMode.IgnoreSchema;
		}

		private XmlReadMode ReadXmlReadSchema (XmlReader input, XmlReadMode mode, bool checkRecurse)
		{
			if (input.LocalName == "schema" &&
				input.NamespaceURI == XmlSchema.Namespace) {
				ReadXmlSchema (input);
			}
			else if (checkRecurse && !input.IsEmptyElement) {
				int depth = input.Depth;
				input.Read ();
				input.MoveToContent ();
				ReadXmlReadSchema (input, mode, false);
				while (input.Depth > depth)
					input.Skip ();
				if (input.NodeType == XmlNodeType.EndElement)
					input.ReadEndElement ();
			}
			else
				input.Skip ();
			input.MoveToContent ();
			return XmlReadMode.ReadSchema;
		}

		internal static string WriteObjectXml (object o)
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

		private void WriteTable (XmlWriter writer,
			DataRow [] rows,
			XmlWriteMode mode,
			DataRowVersion version, bool skipIfNested)
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
			if (!row.IsNull (col))
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
		
		private void DoWriteXmlSchema (XmlWriter writer)
		{
			if (writer.WriteState == WriteState.Start)
				writer.WriteStartDocument ();
			XmlSchemaWriter.WriteXmlSchema (this, writer);
		}
		
		///<summary>
		/// Helper function to split columns into attributes elements and simple
		/// content
		/// </summary>
		internal static void SplitColumns (DataTable table, 
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
		#endregion //Private Xml Serialisation
	}
}
