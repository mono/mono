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
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Data.Common;

namespace System.Data
{
	[ToolboxItem ("Microsoft.VSDesigner.Data.VS.DataSetToolboxItem, " + Consts.AssemblyMicrosoft_VSDesigner)]
	[DefaultProperty ("DataSetName")]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.DataSetDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[Serializable]
	public partial class DataSet : MarshalByValueComponent, IListSource, ISupportInitialize,
			       ISerializable, IXmlSerializable {
		private string dataSetName;
		private string _namespace = string.Empty;
		private string prefix;
		private bool caseSensitive;
		private bool enforceConstraints = true;
		private DataTableCollection tableCollection;
		private DataRelationCollection relationCollection;
		private PropertyCollection properties;
		private DataViewManager defaultView;
		private CultureInfo locale;
		internal XmlDataDocument _xmlDataDocument;

#if NET_2_0
		internal TableAdapterSchemaInfo tableAdapterSchemaInfo;
#endif
		bool initInProgress;

		#region Constructors

		public DataSet ()
			: this ("NewDataSet")
		{
		}

		public DataSet (string dataSetName)
		{
			this.dataSetName = dataSetName;
			tableCollection = new DataTableCollection (this);
			relationCollection = new DataRelationCollection.DataSetRelationCollection (this);
			properties = new PropertyCollection ();
			prefix = String.Empty;
		}

		protected DataSet (SerializationInfo info, StreamingContext context)
			: this ()
		{
#if NET_2_0
			if (IsBinarySerialized (info, context)) {
				BinaryDeserialize (info);
				return;
			}
#endif
			string s = info.GetValue ("XmlSchema", typeof (String)) as String;
			XmlTextReader reader = new XmlTextReader (new StringReader (s));
			ReadXmlSchema (reader);
			reader.Close ();

			GetSerializationData (info, context);
		}

		#endregion // Constructors

		#region Public Properties

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates whether comparing strings within the DataSet is case sensitive.")]
#endif
		[DefaultValue (false)]
		public bool CaseSensitive {
			get { return caseSensitive; }
			set {
				caseSensitive = value;
				if (!caseSensitive) {
					foreach (DataTable table in Tables) {
						table.ResetCaseSensitiveIndexes ();
						foreach (Constraint c in table.Constraints)
							c.AssertConstraint ();
					}
				} else {
					foreach (DataTable table in Tables) {
						table.ResetCaseSensitiveIndexes ();
					}
				}
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("The name of this DataSet.")]
#endif
		[DefaultValue ("")]
		public string DataSetName {
			get { return dataSetName; }
			set { dataSetName = value; }
		}

#if !NET_2_0
		[DataSysDescription ("Indicates a custom \"view\" of the data contained by the DataSet. This view allows filtering, searching, and navigating through the custom data view.")]
#endif
		[Browsable (false)]
		public DataViewManager DefaultViewManager {
			get {
				if (defaultView == null)
					defaultView = new DataViewManager (this);
				return defaultView;
			}
		}

#if !NET_2_0
		[DataSysDescription ("Indicates whether constraint rules are to be followed.")]
#endif
		[DefaultValue (true)]
		public bool EnforceConstraints {
			get { return enforceConstraints; }
			set { InternalEnforceConstraints (value, true); }
		}

		[Browsable (false)]
		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("The collection that holds custom user information.")]
#endif
		public PropertyCollection ExtendedProperties {
			get { return properties; }
		}

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("Indicates that the DataSet has errors.")]
#endif
		public bool HasErrors {
			get {
				for (int i = 0; i < Tables.Count; i++) {
					if (Tables[i].HasErrors)
						return true;
				}
				return false;
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Indicates a locale under which to compare strings within the DataSet.")]
#endif
		public CultureInfo Locale {
			get { return locale != null ? locale : Thread.CurrentThread.CurrentCulture; }
			set {
				if (locale == null || !locale.Equals (value)) {
					// TODO: check if the new locale is valid
					// TODO: update locale of all tables
					locale = value;
				}
			}
		}

		internal bool LocaleSpecified {
			get { return locale != null; }
		}

		
#if NET_2_0
		internal TableAdapterSchemaInfo TableAdapterSchemaData {
			get { return tableAdapterSchemaInfo; }
		}
#endif
		
		internal void InternalEnforceConstraints (bool value,bool resetIndexes)
		{
			if (value == enforceConstraints)
				return;

			if (value) {
				if (resetIndexes) {
					// FIXME : is that correct?
					// By design the indexes should be updated at this point.
					// In Fill from BeginLoadData till EndLoadData indexes are not updated (reset in EndLoadData)
					// In DataRow.EndEdit indexes are always updated.
					foreach (DataTable table in Tables)
						table.ResetIndexes ();
				}

				// TODO : Need to take care of Error handling and settting of RowErrors
				bool constraintViolated = false;
				foreach (DataTable table in Tables) {
					foreach (Constraint constraint in table.Constraints)
						constraint.AssertConstraint();
					table.AssertNotNullConstraints ();
					if (!constraintViolated && table.HasErrors)
						constraintViolated = true;
				}

				if (constraintViolated)
					Constraint.ThrowConstraintException ();
			}
			enforceConstraints = value;
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

		public void Merge (DataRow[] rows, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if (rows == null)
				throw new ArgumentNullException ("rows");
			if (!IsLegalSchemaAction (missingSchemaAction))
				throw new ArgumentOutOfRangeException ("missingSchemaAction");

			MergeManager.Merge (this, rows, preserveChanges, missingSchemaAction);
		}

		public void Merge (DataSet dataSet, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if (dataSet == null)
				throw new ArgumentNullException ("dataSet");
			if (!IsLegalSchemaAction (missingSchemaAction))
				throw new ArgumentOutOfRangeException ("missingSchemaAction");

			MergeManager.Merge (this, dataSet, preserveChanges, missingSchemaAction);
		}

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
#if !NET_2_0
		[DataSysDescription ("Indicates the XML uri namespace for the root element pointed at by this DataSet.")]
#endif
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
#if !NET_2_0
		[DataSysDescription ("Indicates the prefix of the namespace used for this DataSet.")]
#endif
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

				if (value != this.prefix)
					RaisePropertyChanging ("Prefix");
				prefix = value;
			}
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("The collection that holds the relations for this DatSet.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataRelationCollection Relations {
			get { return relationCollection; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ISite Site {
			get { return base.Site; }
			set { base.Site = value; }
		}

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("The collection that holds the tables for this DataSet.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataTableCollection Tables {
			get { return tableCollection; }
		}

		#endregion // Public Properties

		#region Public Methods

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
			for (int t = 0; t < tableCollection.Count; t++)
				tableCollection[t].Clear ();
			this.EnforceConstraints = enforceConstraints;
		}

		public virtual DataSet Clone ()
		{
			// need to return the same type as this...
			DataSet Copy = (DataSet) Activator.CreateInstance (GetType (), true);

			CopyProperties (Copy);

			foreach (DataTable Table in Tables) {
				// tables are often added in no-args constructor, don't add them
				// twice.
				if (!Copy.Tables.Contains (Table.TableName))
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
			// need to return the same type as this...
			DataSet Copy = (DataSet) Activator.CreateInstance (GetType (), true);

			CopyProperties (Copy);

			// Copy DatSet's tables
			foreach (DataTable Table in Tables) {
				if (! Copy.Tables.Contains (Table.TableName)) {
					Copy.Tables.Add (Table.Copy ());
					continue;
				}
				foreach (DataRow row in Table.Rows)
					Copy.Tables [Table.TableName].ImportRow (row);
			}

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
				// Cannot copy extended properties directly as the property does not have a set accessor
				Array tgtArray = Array.CreateInstance( typeof (object), ExtendedProperties.Count);
				ExtendedProperties.Keys.CopyTo (tgtArray, 0);
				for (int i = 0; i < ExtendedProperties.Count; i++)
					Copy.ExtendedProperties.Add (tgtArray.GetValue (i), ExtendedProperties[tgtArray.GetValue (i)]);
			}
			Copy.locale = locale;
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

				// typed datasets create relations through ctor.
				if (Copy.Relations.Contains (MyRelation.RelationName))
					continue;

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

				DataRelation cRel = new DataRelation (MyRelation.RelationName, P_DC, C_DC, false);
				Copy.Relations.Add (cRel);
			}

			// Foreign Key constraints are not cloned in DataTable.Clone
			// so, these constraints should be cloned when copying the relations.
			foreach (DataTable table in this.Tables) {
				foreach (Constraint c in table.Constraints) {
					if (!(c is ForeignKeyConstraint)
						|| Copy.Tables[table.TableName].Constraints.Contains (c.ConstraintName))
						continue;
					ForeignKeyConstraint fc = (ForeignKeyConstraint)c;
					DataTable parentTable = Copy.Tables [fc.RelatedTable.TableName];
					DataTable currTable = Copy.Tables [table.TableName];
					DataColumn[] parentCols = new DataColumn [fc.RelatedColumns.Length];
					DataColumn[] childCols = new DataColumn [fc.Columns.Length];
					for (int j=0; j < parentCols.Length; ++j)
						parentCols [j] = parentTable.Columns[fc.RelatedColumns[j].ColumnName];
					for (int j=0; j < childCols.Length; ++j)
						childCols [j] = currTable.Columns[fc.Columns[j].ColumnName];
					currTable.Constraints.Add (fc.ConstraintName, parentCols, childCols);
				}
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
			bool prev = copySet.EnforceConstraints;
			copySet.EnforceConstraints = false;

			Hashtable addedRows = new Hashtable ();

			for (int i = 0; i < Tables.Count; i++) {
				DataTable origTable = Tables [i];
				DataTable copyTable = copySet.Tables[origTable.TableName];
				for (int j = 0; j < origTable.Rows.Count; j++) {
					DataRow row = origTable.Rows [j];
					if (!row.IsRowChanged (rowStates) || addedRows.Contains (row))
						continue;
					AddChangedRow (addedRows, copyTable, row);
				}
			}
			copySet.EnforceConstraints = prev;
			return copySet;
		}

		private void AddChangedRow (Hashtable addedRows, DataTable copyTable, DataRow row)
		{
			if (addedRows.ContainsKey (row))
				return;

			foreach (DataRelation relation in row.Table.ParentRelations) {
				DataRow parent = ( row.RowState != DataRowState.Deleted ?
						   row.GetParentRow (relation) :
						   row.GetParentRow (relation, DataRowVersion.Original)
						   );
				if (parent == null)
					continue;
				// add the parent row
				DataTable parentCopyTable = copyTable.DataSet.Tables [parent.Table.TableName];
				AddChangedRow (addedRows, parentCopyTable, parent);
			}

			// add the current row
			DataRow newRow = copyTable.NewNotInitializedRow ();
			copyTable.Rows.AddInternal (newRow);
			// Don't check for ReadOnly, when cloning data to new uninitialized row.
			row.CopyValuesToRow (newRow, false);
			newRow.XmlRowID = row.XmlRowID;
			addedRows.Add (row, row);
		}

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

		public bool HasChanges ()
		{
			return HasChanges (DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
		}

		public bool HasChanges (DataRowState rowStates)
		{
			if (((int) rowStates & 0xffffffe0) != 0)
				throw new ArgumentOutOfRangeException ("rowStates");

			DataTableCollection tableCollection = Tables;
			DataTable table;
			DataRowCollection rowCollection;
			DataRow row;

			for (int i = 0; i < tableCollection.Count; i++) {
				table = tableCollection [i];
				rowCollection = table.Rows;
				for (int j = 0; j < rowCollection.Count; j++) {
					row = rowCollection [j];
					if ((row.RowState & rowStates) != 0)
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
			} finally {
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

		public void WriteXml (string fileName, XmlWriteMode mode)
		{
			XmlTextWriter writer = new XmlTextWriter (fileName, null);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument (true);

			try {
				WriteXml (writer, mode);
			} finally {
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
				WriteStartElement (writer, mode, Namespace, Prefix, XmlHelper.Encode (DataSetName));

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

		public void ReadXmlSchema (string fileName)
		{
			XmlReader reader = new XmlTextReader (fileName);
			try {
				ReadXmlSchema (reader);
			} finally {
				reader.Close ();
			}
		}

		public void ReadXmlSchema (TextReader reader)
		{
			XmlReader xr = new XmlTextReader (reader);
			ReadXmlSchema (xr);
		}

		public void ReadXmlSchema (XmlReader reader)
		{
#if true
			XmlSchemaDataImporter xsdImporter = new XmlSchemaDataImporter (this, reader, true);
			xsdImporter.Process ();
#if NET_2_0
			tableAdapterSchemaInfo = xsdImporter.CurrentAdapter;
#endif
#else
			XmlSchemaMapper SchemaMapper = new XmlSchemaMapper (this);
			SchemaMapper.Read (reader);
#endif
		}

		public XmlReadMode ReadXml (Stream stream)
		{
			return ReadXml (new XmlTextReader (stream));
		}

		public XmlReadMode ReadXml (string fileName)
		{
			XmlTextReader reader = new XmlTextReader (fileName);
			try {
				return ReadXml (reader);
			} finally {
				reader.Close ();
			}
		}

		public XmlReadMode ReadXml (TextReader reader)
		{
			return ReadXml (new XmlTextReader (reader));
		}

		public XmlReadMode ReadXml (XmlReader reader)
		{
			return ReadXml (reader, XmlReadMode.Auto);
		}

		public XmlReadMode ReadXml (Stream stream, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (stream), mode);
		}

		public XmlReadMode ReadXml (string fileName, XmlReadMode mode)
		{
			XmlTextReader reader = new XmlTextReader (fileName);
			try {
				return ReadXml (reader, mode);
			} finally {
				reader.Close ();
			}
		}

		public XmlReadMode ReadXml (TextReader reader, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (reader), mode);
		}

		// LAMESPEC: XmlReadMode.Fragment is far from presisely
		// documented. MS.NET infers schema against this mode.
		public XmlReadMode ReadXml (XmlReader reader, XmlReadMode mode)
		{
			if (reader == null)
				return mode;

			switch (reader.ReadState) {
			case ReadState.EndOfFile:
			case ReadState.Error:
			case ReadState.Closed:
				return mode;
			}
			// Skip XML declaration and prolog
			reader.MoveToContent ();
			if (reader.EOF)
				return mode;

			if (reader is XmlTextReader) {
				// we dont need whitespace
				((XmlTextReader) reader).WhitespaceHandling = WhitespaceHandling.None;
			}

			XmlDiffLoader DiffLoader = null;

			// If diffgram, then read the first element as diffgram
			if (reader.LocalName == "diffgram" && reader.NamespaceURI == XmlConstants.DiffgrNamespace) {
				switch (mode) {
					case XmlReadMode.Auto:
					case XmlReadMode.DiffGram:
						if (DiffLoader == null)
							DiffLoader = new XmlDiffLoader (this);
						DiffLoader.Load (reader);
						// (and leave rest of the reader as is)
						return XmlReadMode.DiffGram;
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

			if (reader.EOF)
				return mode;

			int depth = (reader.NodeType == XmlNodeType.Element) ? reader.Depth : -1;

			XmlDocument doc = new XmlDocument ();
			XmlElement root = doc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
			if (reader.HasAttributes) {
				for (int i = 0; i < reader.AttributeCount; i++) {
					reader.MoveToAttribute(i);
					if (reader.NamespaceURI == XmlConstants.XmlnsNS)
						root.SetAttribute(reader.Name, reader.GetAttribute(i));
					else {
						XmlAttribute attr = root.SetAttributeNode(reader.LocalName, reader.NamespaceURI);
						attr.Prefix = reader.Prefix;
						attr.Value = reader.GetAttribute(i);
					}
				}
			}

			reader.Read();
			XmlReadMode retMode = mode;
			bool schemaLoaded = false;

			for (;;) {
				if( reader.Depth == depth ||
					reader.NodeType == XmlNodeType.EndElement)
					break;

				if (reader.NodeType != XmlNodeType.Element) {
					if (!reader.Read())
						break;
					continue;
				}

				if (reader.LocalName == "schema" && reader.NamespaceURI == XmlSchema.Namespace) {
					switch (mode) {
						case XmlReadMode.IgnoreSchema:
						case XmlReadMode.InferSchema:
							reader.Skip ();
							break;

						default:
							ReadXmlSchema (reader);
							retMode = XmlReadMode.ReadSchema;
							schemaLoaded = true;
							// (and leave rest of the reader as is)
							break;
					}

					continue;
				}

				if ((reader.LocalName == "diffgram") && (reader.NamespaceURI == XmlConstants.DiffgrNamespace)) {
					if ((mode == XmlReadMode.DiffGram) || (mode == XmlReadMode.IgnoreSchema)
						|| mode == XmlReadMode.Auto) {
						if (DiffLoader == null)
							DiffLoader = new XmlDiffLoader (this);
						DiffLoader.Load (reader);
						// (and leave rest of the reader as is)
						retMode = XmlReadMode.DiffGram;
					}
					else
						reader.Skip();

					continue;
				}

				//collect data
				XmlNode n = doc.ReadNode(reader);
				root.AppendChild(n);
			}

			if (reader.NodeType == XmlNodeType.EndElement)
				reader.Read ();
			reader.MoveToContent();

			if (mode == XmlReadMode.DiffGram) {
				return retMode;
			}

			doc.AppendChild(root);

			if (!schemaLoaded &&
				retMode != XmlReadMode.ReadSchema &&
				mode != XmlReadMode.IgnoreSchema &&
				mode != XmlReadMode.Fragment &&
				(Tables.Count == 0 || mode == XmlReadMode.InferSchema)) {
				InferXmlSchema(doc, null);
				if (mode == XmlReadMode.Auto)
					retMode = XmlReadMode.InferSchema;
			}

			reader = new XmlNodeReader (doc);
			XmlDataReader.ReadXml (this, reader, mode);

			return retMode == XmlReadMode.Auto ?
				XmlReadMode.IgnoreSchema : retMode;
		}
		#endregion // Public Methods

		#region Public Events

		[DataCategory ("Action")]
#if !NET_2_0
		[DataSysDescription ("Occurs when it is not possible to merge schemas for two tables with the same name.")]
#endif
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

		internal bool InitInProgress {
			get { return initInProgress; }
			set { initInProgress = value; }
		}

		public void BeginInit ()
		{
			InitInProgress = true;
#if NET_2_0
			dataSetInitialized = false;
#endif
		}

		public void EndInit ()
		{
			// Finsh the init'ing the tables only after adding all the
			// tables to the collection.
			Tables.PostAddRange ();
			for (int i=0; i < Tables.Count; ++i) {
				if (!Tables [i].InitInProgress)
					continue;
				Tables [i].FinishInit ();
			}

			Relations.PostAddRange ();
			InitInProgress = false;
#if NET_2_0
			dataSetInitialized = true;
			DataSetInitialized ();
#endif
		}
		#endregion

		#region ISerializable
#if NET_2_0
		public virtual
#endif
		void
#if !NET_2_0
		ISerializable.
#endif
		GetObjectData (SerializationInfo info, StreamingContext context)
		{
#if NET_2_0
			if (RemotingFormat == SerializationFormat.Xml) {
				info.AddValue ("SchemaSerializationMode.DataSet", this.SchemaSerializationMode);
#endif
				StringWriter sw = new StringWriter ();
				XmlTextWriter writer = new XmlTextWriter (sw);
				DoWriteXmlSchema (writer);
				writer.Flush ();
				info.AddValue ("XmlSchema", sw.ToString ());

				sw = new StringWriter ();
				writer = new XmlTextWriter (sw);
				WriteXml (writer, XmlWriteMode.DiffGram);
				writer.Flush ();
				info.AddValue ("XmlDiffGram", sw.ToString ());
#if NET_2_0
			} else /*if (DataSet.RemotingFormat == SerializationFormat.Binary)*/ {
				BinarySerialize (info);
			}
#endif
		}
		#endregion

		#region Protected Methods
		protected void GetSerializationData (SerializationInfo info, StreamingContext context)
		{
			string s = info.GetValue ("XmlDiffGram", typeof (String)) as String;
			XmlTextReader reader = new XmlTextReader (new StringReader (s));
			ReadXml (reader, XmlReadMode.DiffGram);
			reader.Close ();
		}


		protected virtual System.Xml.Schema.XmlSchema GetSchemaSerializable ()
		{
			return null;
		}

		protected virtual void ReadXmlSerializable (XmlReader reader)
		{
			ReadXml (reader, XmlReadMode.DiffGram);
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
			if (GetType() == typeof(DataSet))
				return null;
			MemoryStream stream = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(stream, null);
			WriteXmlSchema(writer);
			stream.Position = 0;
			return XmlSchema.Read(new XmlTextReader(stream), (ValidationEventHandler)null);
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnRemoveRelation (DataRelation relation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnRemoveTable (DataTable table)
		{
			throw new NotImplementedException ();
		}

		internal virtual void OnMergeFailed (MergeFailedEventArgs e)
		{
			if (MergeFailed != null)
				MergeFailed (this, e);
			else
				throw new DataException (e.Conflict);
		}

		[MonoTODO]
		protected internal void RaisePropertyChanging (string name)
		{
		}

		#endregion

		#region Private Methods

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
#if NET_2_0
					return XmlConvert.ToString ((DateTime) o, XmlDateTimeSerializationMode.Unspecified);
#else
					return XmlConvert.ToString ((DateTime) o);
#endif
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
			//WriteTable takes care of skipping a table if it has a
			//Nested Parent Relationship
			foreach (DataTable table in tableCollection)
				WriteTable ( writer, table, mode, version);
		}

		internal static void WriteTable (XmlWriter writer, DataTable table, XmlWriteMode mode, DataRowVersion version)
		{
			DataRow[] rows = table.NewRowArray(table.Rows.Count);
			table.Rows.CopyTo (rows, 0);
			WriteTable (writer, rows, mode, version, true);
		}

		internal static void WriteTable (XmlWriter writer,
			DataRow [] rows,
			XmlWriteMode mode,
			DataRowVersion version, bool skipIfNested)
		{
			if (rows.Length == 0) return;
			DataTable table = rows[0].Table;

			if (table.TableName == null || table.TableName == "")
				throw new InvalidOperationException("Cannot serialize the DataTable. DataTable name is not set.");

			//The columns can be attributes, hidden, elements, or simple content
			//There can be 0-1 simple content cols or 0-* elements
			System.Collections.ArrayList atts;
			System.Collections.ArrayList elements;
			DataColumn simple = null;

			SplitColumns (table, out atts, out elements, out simple);
			//sort out the namespacing
			int relationCount = table.ParentRelations.Count;

			foreach (DataRow row in rows) {
				if (skipIfNested) {
					// Skip rows that is a child of any tables.
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
					writer.WriteElementString (XmlHelper.Encode (table.TableName), "");
					continue;
				}

				WriteTableElement (writer, mode, table, row, version);

				foreach (DataColumn col in atts)
					WriteColumnAsAttribute (writer, mode, col, row, version);

				if (simple != null) {
					writer.WriteString (WriteObjectXml (row[simple, version]));
				} else {
					foreach (DataColumn col in elements)
						WriteColumnAsElement (writer, mode, col, row, version);
				}

				foreach (DataRelation relation in table.ChildRelations) {
					if (relation.Nested)
						WriteTable (writer, row.GetChildRows (relation), mode, version, false);
				}

				writer.WriteEndElement ();
			}

		}

		internal static void WriteColumnAsElement (XmlWriter writer, XmlWriteMode mode, DataColumn col, DataRow row, DataRowVersion version)
		{
			string colnspc = null;
			object rowObject = row [col, version];

			if (rowObject == null || rowObject == DBNull.Value)
				return;

			if (col.Namespace != String.Empty)
				colnspc = col.Namespace;

			//TODO check if I can get away with write element string
			WriteStartElement (writer, mode, colnspc, col.Prefix, XmlHelper.Encode (col.ColumnName));	
			if (typeof (IXmlSerializable).IsAssignableFrom (col.DataType) 
			    || col.DataType == typeof (object)) {
				IXmlSerializable serializableObj = rowObject as IXmlSerializable;
				if (serializableObj == null)
					throw new InvalidOperationException ();
				((IXmlSerializable)rowObject).WriteXml (writer);				
			} else {
				writer.WriteString (WriteObjectXml (rowObject));
			}

			writer.WriteEndElement ();
		}

		internal static void WriteColumnAsAttribute (XmlWriter writer, XmlWriteMode mode, DataColumn col, DataRow row, DataRowVersion version)
		{
			if (!row.IsNull (col))
				WriteAttributeString (writer, mode, col.Namespace, col.Prefix, XmlHelper.Encode (col.ColumnName), WriteObjectXml (row[col, version]));
		}

		internal static void WriteTableElement (XmlWriter writer, XmlWriteMode mode, DataTable table, DataRow row, DataRowVersion version)
		{
			//sort out the namespacing
			string nspc = (table.Namespace.Length > 0 || table.DataSet == null) ? table.Namespace : table.DataSet.Namespace;

			WriteStartElement (writer, mode, nspc, table.Prefix, XmlHelper.Encode (table.TableName));

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

		internal static void WriteStartElement (XmlWriter writer, XmlWriteMode mode, string nspc, string prefix, string name)
		{
			writer.WriteStartElement (prefix, name, nspc);
		}

		internal static void WriteAttributeString (XmlWriter writer, XmlWriteMode mode, string nspc, string prefix, string name, string stringValue)
		{
			switch ( mode) {
			//	case XmlWriteMode.WriteSchema:
			//		writer.WriteAttributeString (prefix, name, nspc);
			//		break;
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
				table.SetRowsID ();
				WriteDiffGramElement (writer);
			}

			WriteStartElement (writer, mode, Namespace, Prefix, XmlHelper.Encode (DataSetName));

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

		internal static void WriteDiffGramElement (XmlWriter writer)
		{
			WriteStartElement (writer, XmlWriteMode.DiffGram, XmlConstants.DiffgrNamespace, XmlConstants.DiffgrPrefix, "diffgram");
			WriteAttributeString (writer, XmlWriteMode.DiffGram, null, "xmlns", XmlConstants.MsdataPrefix, XmlConstants.MsdataNamespace);
		}

		private void SetRowsID ()
		{
			foreach (DataTable table in Tables)
				table.SetRowsID ();
		}

		#endregion //Private Xml Serialisation
	}

#if NET_2_0
	[XmlSchemaProvider ("GetDataSetSchema")]
	[XmlRoot ("DataSet")]
	partial class DataSet : ISupportInitializeNotification {
		private bool dataSetInitialized = true;
		public event EventHandler Initialized;

		protected DataSet (SerializationInfo info, StreamingContext context, bool constructSchema)
			: this ()
		{
			if (DetermineSchemaSerializationMode (info, context) == SchemaSerializationMode.ExcludeSchema) {
				InitializeDerivedDataSet ();
			}
			
			if (IsBinarySerialized (info, context)) {
				BinaryDeserialize (info);
				return;
			}
			
			if (constructSchema) {
				string s = info.GetValue ("XmlSchema", typeof (String)) as String;
				XmlTextReader reader = new XmlTextReader (new StringReader (s));
				ReadXmlSchema (reader);
				reader.Close ();
				
				GetSerializationData (info, context);
			}
		}

		SerializationFormat remotingFormat = SerializationFormat.Xml;
		[DefaultValue (SerializationFormat.Xml)]
		public SerializationFormat RemotingFormat {
			get { return remotingFormat; }
			set { remotingFormat = value; }
		}

		[Browsable (false)]
		public bool IsInitialized {
			get { return dataSetInitialized; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public virtual SchemaSerializationMode SchemaSerializationMode {
			get { return SchemaSerializationMode.IncludeSchema; }
			set {
				if (value != SchemaSerializationMode.IncludeSchema)
					throw new InvalidOperationException (
							"Only IncludeSchema Mode can be set for Untyped DataSet");
			}
		}

		public DataTableReader CreateDataReader (params DataTable[] dataTables)
		{
			return new DataTableReader (dataTables);
		}

		public DataTableReader CreateDataReader ()
		{
			return new DataTableReader ((DataTable[])Tables.ToArray (typeof (DataTable)));
		}

		public static XmlSchemaComplexType GetDataSetSchema (XmlSchemaSet schemaSet)
		{
			return new XmlSchemaComplexType ();
		}

		public void Load (IDataReader reader, LoadOption loadOption, params DataTable[] tables)
		{
			if (reader == null)
				throw new ArgumentNullException ("Value cannot be null. Parameter name: reader");

			foreach (DataTable dt in tables) {
				if (dt.DataSet == null || dt.DataSet != this)
					throw new ArgumentException ("Table " + dt.TableName + " does not belong to this DataSet.");
				dt.Load (reader, loadOption);
				reader.NextResult ();
			}
		}

		public void Load (IDataReader reader, LoadOption loadOption, params string[] tables)
		{
			if (reader == null)
				throw new ArgumentNullException ("Value cannot be null. Parameter name: reader");

			foreach (string tableName in tables) {
				DataTable dt = Tables [tableName];

				if (dt == null) {
					dt = new DataTable (tableName);
					Tables.Add (dt);
				}
				dt.Load (reader, loadOption);
				reader.NextResult ();
			}
		}

		public virtual void Load (IDataReader reader, LoadOption loadOption,
					  FillErrorEventHandler errorHandler, params DataTable[] tables)
		{
			if (reader == null)
				throw new ArgumentNullException ("Value cannot be null. Parameter name: reader");

			foreach (DataTable dt in tables) {
				if (dt.DataSet == null || dt.DataSet != this)
					throw new ArgumentException ("Table " + dt.TableName + " does not belong to this DataSet.");
				dt.Load (reader, loadOption, errorHandler);
				reader.NextResult ();
			}
		}

		void BinarySerialize (SerializationInfo si)
		{
			Version vr = new Version(2, 0);
			si.AddValue ("DataSet.RemotingVersion", vr, typeof (Version));
			si.AddValue ("DataSet.RemotingFormat", RemotingFormat, typeof (SerializationFormat));
			si.AddValue ("DataSet.DataSetName", DataSetName);
			si.AddValue ("DataSet.Namespace", Namespace);
			si.AddValue ("DataSet.Prefix", Prefix);
			si.AddValue ("DataSet.CaseSensitive", CaseSensitive);
			si.AddValue ("DataSet.LocaleLCID", Locale.LCID);
			si.AddValue ("DataSet.EnforceConstraints", EnforceConstraints);
			si.AddValue ("DataSet.ExtendedProperties", properties, typeof (PropertyCollection));

			Tables.BinarySerialize_Schema (si);
			Tables.BinarySerialize_Data (si);

			Relations.BinarySerialize (si);
		}

		void BinaryDeserialize (SerializationInfo info)
		{
			ArrayList arrayList = null;

			DataSetName = info.GetString ("DataSet.DataSetName");
			Namespace = info.GetString ("DataSet.Namespace");
			CaseSensitive = info.GetBoolean ("DataSet.CaseSensitive");
			Locale = new CultureInfo (info.GetInt32 ("DataSet.LocaleLCID"));
			EnforceConstraints = info.GetBoolean ("DataSet.EnforceConstraints");
			Prefix = info.GetString ("DataSet.Prefix");
			/*
			  FIXME: Private variable available in SerializationInfo
			  this.RemotingVersion = (System.Version) info.GetValue("DataSet.RemotingVersion",
			  typeof(System.Version));
			*/
			properties = (PropertyCollection) info.GetValue ("DataSet.ExtendedProperties",
									 typeof (PropertyCollection));
			int tableCount = info.GetInt32 ("DataSet.Tables.Count");

			Byte [] bytes;
			DataTable dt = null;
			for (int i = 0; i < tableCount; i++) {
				bytes = (Byte []) info.GetValue ("DataSet.Tables_" + i,
								 typeof (Byte[]));
				MemoryStream ms = new MemoryStream (bytes);
				BinaryFormatter bf = new BinaryFormatter ();
				dt = (DataTable) bf.Deserialize (ms);
				ms.Close ();
				for (int j = 0; j < dt.Columns.Count; j++) {
					dt.Columns[j].Expression = info.GetString ("DataTable_" + i +
										   ".DataColumn_" + j +
										   ".Expression");
				}
				/*
				  Not using
				  int rowsCount = info.GetInt32 ("DataTable_" + i + ".Rows.Count");
				  int recordsCount = info.GetInt32 ("DataTable_" + i + ".Records.Count");
				*/
				ArrayList nullBits = (ArrayList) info.GetValue ("DataTable_" + i + ".NullBits",
										typeof (ArrayList));
				arrayList = (ArrayList) info.GetValue ("DataTable_" + i + ".Records",
								       typeof (ArrayList));
				BitArray rowStateBitArray = (BitArray) info.GetValue ("DataTable_" + i + ".RowStates",
										      typeof (BitArray));
				dt.DeserializeRecords (arrayList, nullBits, rowStateBitArray);
				Tables.Add (dt);
			}
			for (int i = 0; i < tableCount; i++) {
				dt = Tables [i];
				dt.dataSet = this;
				arrayList = (ArrayList) info.GetValue ("DataTable_" + i + ".Constraints",
								       typeof (ArrayList));
				if (dt.Constraints == null)
					dt.Constraints = new ConstraintCollection (dt);
				dt.DeserializeConstraints (arrayList);
			}
			arrayList = (ArrayList) info.GetValue ("DataSet.Relations",
							       typeof (ArrayList));
			bool bParentColumn = true;
			for (int l = 0; l < arrayList.Count; l++) {
				ArrayList tmpArrayList = (ArrayList) arrayList[l];
				ArrayList childColumns = new ArrayList ();
				ArrayList parentColumns = new ArrayList ();
				for (int k = 0; k < tmpArrayList.Count; k++) {
					if (tmpArrayList[k] != null && typeof (int) == tmpArrayList[k].GetType().GetElementType()) {
						Array dataColumnArray = (Array)tmpArrayList[k];
						if (bParentColumn) {
							parentColumns.Add (Tables [(int) dataColumnArray.GetValue (0)].
									   Columns [(int) dataColumnArray.GetValue (1)]);
							bParentColumn = false;
						}
						else {
							childColumns.Add (Tables [(int) dataColumnArray.GetValue (0)].
									  Columns [(int) dataColumnArray.GetValue (1)]);
							bParentColumn = true;
						}
					}
				}
				Relations.Add ((string) tmpArrayList [0],
					       (DataColumn []) parentColumns.ToArray (typeof (DataColumn)),
					       (DataColumn []) childColumns.ToArray (typeof (DataColumn)),
					       false);
			}
		}

		private void OnDataSetInitialized (EventArgs e)
		{
			if (null != Initialized)
				Initialized (this, e);
		}

		private void DataSetInitialized ()
		{
			EventArgs e = new EventArgs ();
			OnDataSetInitialized (e);
		}

		protected virtual void InitializeDerivedDataSet ()
		{
		}

		protected SchemaSerializationMode DetermineSchemaSerializationMode (XmlReader reader)
		{
			return SchemaSerializationMode.IncludeSchema;
		}

		protected SchemaSerializationMode DetermineSchemaSerializationMode (SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator e = info.GetEnumerator ();
			while (e.MoveNext ()) {
				if (e.Name == "SchemaSerializationMode.DataSet") {
					return (SchemaSerializationMode) e.Value;
				}
			}
			
			return SchemaSerializationMode.IncludeSchema;
		}

		protected bool IsBinarySerialized (SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator e = info.GetEnumerator ();
			while (e.MoveNext ()) {
				if (e.ObjectType == typeof (System.Data.SerializationFormat))
					return true;
			}
			return false;
		}
	}
#endif
}
