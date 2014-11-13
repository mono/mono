//------------------------------------------------------------------------------
// <copyright file="DataSet.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Text;
    using System.Runtime.Serialization;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Serialization.Advanced;
    using System.Xml.Schema;
    using System.Runtime.Serialization.Formatters.Binary; //Binary Formatter
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Data.Common;
    using System.Runtime.Versioning;
    using System.Runtime.CompilerServices;

    /// <devdoc>
    ///    <para>
    ///       Represents an in-memory cache of data.
    ///    </para>
    /// </devdoc>
    [
    ResDescriptionAttribute(Res.DataSetDescr),
    DefaultProperty("DataSetName"),
    ToolboxItem("Microsoft.VSDesigner.Data.VS.DataSetToolboxItem, " + AssemblyRef.MicrosoftVSDesigner),
    Designer("Microsoft.VSDesigner.Data.VS.DataSetDesigner, " + AssemblyRef.MicrosoftVSDesigner),
    Serializable,
    XmlSchemaProvider("GetDataSetSchema"),
    XmlRoot("DataSet")
    ]
 public class DataSet : MarshalByValueComponent, System.ComponentModel.IListSource, IXmlSerializable, ISupportInitializeNotification, ISerializable {

        private DataViewManager defaultViewManager;

        // Public Collections
        private readonly DataTableCollection tableCollection;
        private readonly DataRelationCollection relationCollection;
        internal PropertyCollection extendedProperties = null;
        private string dataSetName = "NewDataSet";
        private string _datasetPrefix = String.Empty;
        internal string namespaceURI = string.Empty;
        private bool enforceConstraints = true;
        private const String KEY_XMLSCHEMA = "XmlSchema";
        private const String KEY_XMLDIFFGRAM = "XmlDiffGram";

        // globalization stuff
        private bool _caseSensitive;
        private CultureInfo _culture;
        private bool _cultureUserSet;

        // Internal definitions
        internal bool fInReadXml = false;
        internal bool fInLoadDiffgram = false;
        internal bool fTopLevelTable = false;
        internal bool fInitInProgress = false;
        internal bool fEnableCascading = true;
        internal bool fIsSchemaLoading = false;
        private bool fBoundToDocument;        // for XmlDataDocument

        // Events
        private PropertyChangedEventHandler onPropertyChangingDelegate;
        private MergeFailedEventHandler onMergeFailed;
        private DataRowCreatedEventHandler onDataRowCreated;   // Internal for XmlDataDocument only
        private DataSetClearEventhandler onClearFunctionCalled;   // Internal for XmlDataDocument only
        private System.EventHandler onInitialized;


        internal readonly static DataTable[] zeroTables = new DataTable[0];
        internal string mainTableName = "";

        //default remoting format is XML
        private SerializationFormat _remotingFormat = SerializationFormat.Xml;

        private object _defaultViewManagerLock = new Object();

        private static int _objectTypeCount; // Bid counter
        private readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);
        private static XmlSchemaComplexType schemaTypeForWSDL = null;

        internal bool UseDataSetSchemaOnly; // UseDataSetSchemaOnly  , for YUKON
        internal bool UdtIsWrapped; // if UDT is wrapped , for YUKON

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.DataSet'/> class.</para>
        /// </devdoc>
        public DataSet() {
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataSet.DataSet|API> %d#\n", ObjectID); // others will call this constr
            // Set default locale
            this.tableCollection = new DataTableCollection(this);
            this.relationCollection = new DataRelationCollection.DataSetRelationCollection(this);
            _culture = CultureInfo.CurrentCulture; // Set default locale
        }

        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Data.DataSet'/>
        /// class with the given name.</para>
        /// </devdoc>
        public DataSet(string dataSetName) 
            : this()
        {
            this.DataSetName = dataSetName;
        }

        [
        DefaultValue(SerializationFormat.Xml)
        ]
        public SerializationFormat RemotingFormat {
            get {
                return _remotingFormat;
            }
            set {
                if (value != SerializationFormat.Binary && value != SerializationFormat.Xml) {
                    throw ExceptionBuilder.InvalidRemotingFormat(value);
                }
                _remotingFormat = value;
                // this property is inherited to DataTable from DataSet.So we set this value to DataTable also
                for (int i = 0; i < Tables.Count; i++) {
                    Tables[i].RemotingFormat = value;
                }
            }
        }

        [BrowsableAttribute(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual SchemaSerializationMode SchemaSerializationMode { //Typed DataSet calls into this
            get {
                return SchemaSerializationMode.IncludeSchema;
            }
            set {
                if (value != SchemaSerializationMode.IncludeSchema) {
                    throw ExceptionBuilder.CannotChangeSchemaSerializationMode();
                }
            }
        }

        //      Check whether the stream is binary serialized.
        // 'static' function that consumes SerializationInfo
        protected bool IsBinarySerialized(SerializationInfo info, StreamingContext context) {// mainly for typed DS
            // our default remoting format is XML
            SerializationFormat remotingFormat = SerializationFormat.Xml;
            SerializationInfoEnumerator e = info.GetEnumerator();

            while (e.MoveNext()) {
                if (e.Name == "DataSet.RemotingFormat") {//DataSet.RemotingFormat does not exist in V1/V1.1 versions
                    remotingFormat = (SerializationFormat)e.Value;
                    break;
                }
            }
            return (remotingFormat == SerializationFormat.Binary);
        }

        //      Should Schema be included during Serialization
        // 'static' function that consumes SerializationInfo
        protected SchemaSerializationMode DetermineSchemaSerializationMode(SerializationInfo info, StreamingContext context) { //Typed DataSet calls into this
            SchemaSerializationMode schemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            SerializationInfoEnumerator e = info.GetEnumerator();

            while (e.MoveNext()) {
                if (e.Name == "SchemaSerializationMode.DataSet") { //SchemaSerializationMode.DataSet does not exist in V1/V1.1 versions
                    schemaSerializationMode = (SchemaSerializationMode)e.Value;
                    break;
                }
            }
            return schemaSerializationMode;
        }

        protected SchemaSerializationMode DetermineSchemaSerializationMode(XmlReader reader) { //Typed DataSet calls into this
            SchemaSerializationMode schemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            reader.MoveToContent();
            if (reader.NodeType == XmlNodeType.Element) {
                if (reader.HasAttributes) {
                    string attribValue = reader.GetAttribute(Keywords.MSD_SCHEMASERIALIZATIONMODE, Keywords.MSDNS);
                    if ((String.Compare(attribValue, Keywords.MSD_EXCLUDESCHEMA, StringComparison.OrdinalIgnoreCase) == 0)) {
                        schemaSerializationMode = SchemaSerializationMode.ExcludeSchema;
                    }
                    else if ((String.Compare(attribValue, Keywords.MSD_INCLUDESCHEMA, StringComparison.OrdinalIgnoreCase) == 0)) {
                        schemaSerializationMode = SchemaSerializationMode.IncludeSchema;
                    }
                    else if (attribValue != null) { // if attrib does not exist, then don't throw
                        throw ExceptionBuilder.InvalidSchemaSerializationMode(typeof(SchemaSerializationMode), attribValue);
                    }
                }
            }
            return schemaSerializationMode;
        }


        //      Deserialize all the tables data of the dataset from binary/xml stream.
        // 'instance' method that consumes SerializationInfo
        protected void GetSerializationData(SerializationInfo info, StreamingContext context) {// mainly for typed DS
            SerializationFormat remotingFormat = SerializationFormat.Xml;
            SerializationInfoEnumerator e = info.GetEnumerator();

            while (e.MoveNext()) {
                if (e.Name == "DataSet.RemotingFormat") { //DataSet.RemotingFormat does not exist in V1/V1.1 versions
                    remotingFormat = (SerializationFormat)e.Value;
                    break;
                }
            }
            DeserializeDataSetData(info, context, remotingFormat);
        }


        //      Deserialize all the tables schema and data of the dataset from binary/xml stream.
        protected DataSet(SerializationInfo info, StreamingContext context)
            : this(info, context, true) {
        }
        protected DataSet(SerializationInfo info, StreamingContext context, bool ConstructSchema)
            : this() {
            SerializationFormat remotingFormat = SerializationFormat.Xml;
            SchemaSerializationMode schemaSerializationMode = SchemaSerializationMode.IncludeSchema;
            SerializationInfoEnumerator e = info.GetEnumerator();

            while (e.MoveNext()) {
                switch (e.Name) {
                    case "DataSet.RemotingFormat": //DataSet.RemotingFormat does not exist in V1/V1.1 versions
                        remotingFormat = (SerializationFormat)e.Value;
                        break;
                    case "SchemaSerializationMode.DataSet": //SchemaSerializationMode.DataSet does not exist in V1/V1.1 versions
                        schemaSerializationMode = (SchemaSerializationMode)e.Value;
                        break;
                }
            }

            if (schemaSerializationMode == SchemaSerializationMode.ExcludeSchema) {
                InitializeDerivedDataSet();
            }

            // adding back this check will fix typed dataset XML remoting, but we have to fix case that 
            // a class inherits from DataSet and just relies on DataSet to deserialize (see SQL BU DT 374717)
            // to fix that case also, we need to add a flag and add it to below check so return (no-op) will be 
            // conditional (flag needs to be set in TypedDataSet
            if (remotingFormat == SerializationFormat.Xml && !ConstructSchema /* && this.GetType() != typeof(DataSet)*/) {
                return; //For typed dataset xml remoting, this is a no-op
            }
            DeserializeDataSet(info, context, remotingFormat, schemaSerializationMode);
        }



        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags = System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            SerializationFormat remotingFormat = RemotingFormat;
            SerializeDataSet(info, context, remotingFormat);
        }

        //      Deserialize all the tables data of the dataset from binary/xml stream.
        protected virtual void InitializeDerivedDataSet() {
            return;
        }

        //      Serialize all the tables.
        private void SerializeDataSet(SerializationInfo info, StreamingContext context, SerializationFormat remotingFormat) {
            Debug.Assert(info != null);
            info.AddValue("DataSet.RemotingVersion", new Version(2, 0));

            // SqlHotFix 299, SerializationFormat enumeration types don't exist in V1.1 SP1
            if (SerializationFormat.Xml != remotingFormat) {
                info.AddValue("DataSet.RemotingFormat", remotingFormat);
            }

            // SqlHotFix 299, SchemaSerializationMode enumeration types don't exist in V1.1 SP1
            if (SchemaSerializationMode.IncludeSchema != SchemaSerializationMode) {
                //SkipSchemaDuringSerialization
                info.AddValue("SchemaSerializationMode.DataSet", SchemaSerializationMode);
            }

            if (remotingFormat != SerializationFormat.Xml) {
                if (SchemaSerializationMode == SchemaSerializationMode.IncludeSchema) {

                    //DataSet public state properties
                    SerializeDataSetProperties(info, context);

                    //Tables Count
                    info.AddValue("DataSet.Tables.Count", Tables.Count);

                    //Tables, Columns, Rows
                    for (int i = 0; i < Tables.Count; i++) {
                        BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(context.State, false));
                        MemoryStream memStream = new MemoryStream();
                        bf.Serialize(memStream, Tables[i]);
                        memStream.Position = 0;
                        info.AddValue(String.Format(CultureInfo.InvariantCulture, "DataSet.Tables_{0}", i), memStream.GetBuffer());
                    }

                    //Constraints
                    for (int i = 0; i < Tables.Count; i++) {
                        Tables[i].SerializeConstraints(info, context, i, true);
                    }

                    //Relations
                    SerializeRelations(info, context);

                    //Expression Columns
                    for (int i = 0; i < Tables.Count; i++) {
                        Tables[i].SerializeExpressionColumns(info, context, i);
                    }
                }
                else {
                    //Serialize  DataSet public properties.
                    SerializeDataSetProperties(info, context);
                }
                //Rows
                for (int i = 0; i < Tables.Count; i++) {
                    Tables[i].SerializeTableData(info, context, i);
                }
            } else { // old behaviour

                String strSchema = this.GetXmlSchemaForRemoting(null);

                String strData = null;
                info.AddValue(KEY_XMLSCHEMA, strSchema);

                StringBuilder strBuilder;
                strBuilder = new StringBuilder(EstimatedXmlStringSize() * 2);
                StringWriter strWriter = new StringWriter(strBuilder, CultureInfo.InvariantCulture);
                XmlTextWriter w = new XmlTextWriter(strWriter);
                WriteXml(w, XmlWriteMode.DiffGram);
                strData = strWriter.ToString();
                info.AddValue(KEY_XMLDIFFGRAM, strData);
            }
        }

        //      Deserialize all the tables - marked internal so that DataTable can call into this
        internal void DeserializeDataSet(SerializationInfo info, StreamingContext context, SerializationFormat remotingFormat, SchemaSerializationMode schemaSerializationMode) {
            // deserialize schema
            DeserializeDataSetSchema(info, context, remotingFormat, schemaSerializationMode);
            // deserialize data
            DeserializeDataSetData(info, context, remotingFormat);
        }

        //      Deserialize schema.
        private void DeserializeDataSetSchema(SerializationInfo info, StreamingContext context, SerializationFormat remotingFormat, SchemaSerializationMode schemaSerializationMode) {
            if (remotingFormat != SerializationFormat.Xml) {
                if (schemaSerializationMode == SchemaSerializationMode.IncludeSchema) {
                    //DataSet public state properties
                    DeserializeDataSetProperties(info, context);

                    //Tables Count
                    int tableCount = info.GetInt32("DataSet.Tables.Count");

                    //Tables, Columns, Rows
                    for (int i = 0; i < tableCount; i++) {
                        Byte[] buffer = (Byte[])info.GetValue(String.Format(CultureInfo.InvariantCulture, "DataSet.Tables_{0}", i), typeof(Byte[]));
                        MemoryStream memStream = new MemoryStream(buffer);
                        memStream.Position = 0;
                        BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(context.State, false));
                        DataTable dt = (DataTable)bf.Deserialize(memStream);
                        Tables.Add(dt);
                    }

                    //Constraints
                    for (int i = 0; i < tableCount; i++) {
                        Tables[i].DeserializeConstraints(info, context,  /* table index */i,  /* serialize all constraints */ true); //
                    }

                    //Relations
                    DeserializeRelations(info, context);

                    //Expression Columns
                    for (int i = 0; i < tableCount; i++) {
                        Tables[i].DeserializeExpressionColumns(info, context, i);
                    }
                } else {
                    //DeSerialize DataSet public properties.[Locale, CaseSensitive and EnforceConstraints]
                    DeserializeDataSetProperties(info, context);
                }
            } else {
                string strSchema = (String)info.GetValue(KEY_XMLSCHEMA, typeof(System.String));

                if (strSchema != null) {
                    this.ReadXmlSchema(new XmlTextReader(new StringReader(strSchema)), true);
                }
            }
        }

        //        Deserialize all  data.
        private void DeserializeDataSetData(SerializationInfo info, StreamingContext context, SerializationFormat remotingFormat) {
            if (remotingFormat != SerializationFormat.Xml) {
                for (int i = 0; i < Tables.Count; i++) {
                    Tables[i].DeserializeTableData(info, context, i);
                }
            }
            else {
                string strData = (String)info.GetValue(KEY_XMLDIFFGRAM, typeof(System.String));

                if (strData != null) {
                    this.ReadXml(new XmlTextReader(new StringReader(strData)), XmlReadMode.DiffGram);
                }
            }
        }

        //        Serialize just the dataset properties
        private void SerializeDataSetProperties(SerializationInfo info, StreamingContext context) {
            //DataSet basic properties
            info.AddValue("DataSet.DataSetName", DataSetName);
            info.AddValue("DataSet.Namespace", Namespace);
            info.AddValue("DataSet.Prefix", Prefix);
            //DataSet runtime properties
            info.AddValue("DataSet.CaseSensitive", CaseSensitive);
            info.AddValue("DataSet.LocaleLCID", Locale.LCID);
            info.AddValue("DataSet.EnforceConstraints", EnforceConstraints);

            //ExtendedProperties
            info.AddValue("DataSet.ExtendedProperties", ExtendedProperties);
        }

        //      DeSerialize dataset properties
        private void DeserializeDataSetProperties(SerializationInfo info, StreamingContext context) {
            //DataSet basic properties
            dataSetName = info.GetString("DataSet.DataSetName");
            namespaceURI = info.GetString("DataSet.Namespace");
            _datasetPrefix = info.GetString("DataSet.Prefix");
            //DataSet runtime properties
            _caseSensitive = info.GetBoolean("DataSet.CaseSensitive");
            int lcid = (int)info.GetValue("DataSet.LocaleLCID", typeof(int));
            _culture = new CultureInfo(lcid);
            _cultureUserSet = true;
            enforceConstraints = info.GetBoolean("DataSet.EnforceConstraints");



            //ExtendedProperties
            extendedProperties = (PropertyCollection)info.GetValue("DataSet.ExtendedProperties", typeof(PropertyCollection));
        }

        //        Gets relation info from the dataset.
        //        ***Schema for Serializing ArrayList of Relations***
        //        Relations -> [relationName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[Nested]->[extendedProperties]
        private void SerializeRelations(SerializationInfo info, StreamingContext context) {
            ArrayList relationList = new ArrayList();

            foreach (DataRelation rel in Relations) {
                int[] parentInfo = new int[rel.ParentColumns.Length + 1];

                parentInfo[0] = Tables.IndexOf(rel.ParentTable);
                for (int j = 1; j < parentInfo.Length; j++) {
                    parentInfo[j] = rel.ParentColumns[j - 1].Ordinal;
                }

                int[] childInfo = new int[rel.ChildColumns.Length + 1];
                childInfo[0] = Tables.IndexOf(rel.ChildTable);
                for (int j = 1; j < childInfo.Length; j++) {
                    childInfo[j] = rel.ChildColumns[j - 1].Ordinal;
                }

                ArrayList list = new ArrayList();
                list.Add(rel.RelationName);
                list.Add(parentInfo);
                list.Add(childInfo);
                list.Add(rel.Nested);
                list.Add(rel.extendedProperties);

                relationList.Add(list);
            }
            info.AddValue("DataSet.Relations", relationList);
        }

        /*
                Adds relations to the dataset.
                ***Schema for Serializing ArrayList of Relations***
                Relations -> [relationName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[Nested]->[extendedProperties]
        */
        private void DeserializeRelations(SerializationInfo info, StreamingContext context) {
            ArrayList relationList = (ArrayList)info.GetValue("DataSet.Relations", typeof(ArrayList));

            foreach (ArrayList list in relationList) {
                string relationName = (string)list[0];
                int[] parentInfo = (int[])list[1];
                int[] childInfo = (int[])list[2];
                bool isNested = (bool)list[3];
                PropertyCollection extendedProperties = (PropertyCollection)list[4];

                //ParentKey Columns.
                DataColumn[] parentkeyColumns = new DataColumn[parentInfo.Length - 1];
                for (int i = 0; i < parentkeyColumns.Length; i++) {
                    parentkeyColumns[i] = Tables[parentInfo[0]].Columns[parentInfo[i + 1]];
                }

                //ChildKey Columns.
                DataColumn[] childkeyColumns = new DataColumn[childInfo.Length - 1];
                for (int i = 0; i < childkeyColumns.Length; i++) {
                    childkeyColumns[i] = Tables[childInfo[0]].Columns[childInfo[i + 1]];
                }

                //Create the Relation, without any constraints[Assumption: The constraints are added earlier than the relations]
                DataRelation rel = new DataRelation(relationName, parentkeyColumns, childkeyColumns, false);
                rel.CheckMultipleNested = false; // disable the check for multiple nested parent
                rel.Nested = isNested;
                rel.extendedProperties = extendedProperties;

                Relations.Add(rel);
                rel.CheckMultipleNested = true; // enable the check for multiple nested parent
            }
        }

        internal void FailedEnableConstraints() {
            this.EnforceConstraints = false;
            throw ExceptionBuilder.EnforceConstraint();
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether string
        ///       comparisons within <see cref='System.Data.DataTable'/>
        ///       objects are
        ///       case-sensitive.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(false),
        ResDescriptionAttribute(Res.DataSetCaseSensitiveDescr)
        ]
        public bool CaseSensitive {
            get {
                return _caseSensitive;
            }
            set {
                if (_caseSensitive != value)
                {
                    bool oldValue = _caseSensitive;
                    _caseSensitive = value;

                    if (!ValidateCaseConstraint()) {
                        _caseSensitive = oldValue;
                        throw ExceptionBuilder.CannotChangeCaseLocale();
                    }

                    foreach (DataTable table in Tables) {
                        table.SetCaseSensitiveValue(value, false, true);
                    }
                }
            }
        }

        bool System.ComponentModel.IListSource.ContainsListCollection {
            get {
                return true;
            }
        }

        /// <devdoc>
        /// <para>Gets a custom view of the data contained by the <see cref='System.Data.DataSet'/> , one
        ///    that allows filtering, searching, and navigating through the custom data view.</para>
        /// </devdoc>
        [Browsable(false), ResDescriptionAttribute(Res.DataSetDefaultViewDescr)]
        public DataViewManager DefaultViewManager {
            get {
                if (defaultViewManager == null) {
                    lock (_defaultViewManagerLock) {
                        if (defaultViewManager == null) {
                            defaultViewManager = new DataViewManager(this, true);
                        }
                    }
                }
                return defaultViewManager;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether constraint rules are followed when
        ///       attempting any update operation.</para>
        /// </devdoc>
        [DefaultValue(true), ResDescriptionAttribute(Res.DataSetEnforceConstraintsDescr)]
        public bool EnforceConstraints {
            get {
                return enforceConstraints;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataSet.set_EnforceConstraints|API> %d#, %d{bool}\n", ObjectID, value);
                try {
                    if (this.enforceConstraints != value) {
                        if (value)
                            EnableConstraints();
                        this.enforceConstraints = value;
                    }
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        internal void RestoreEnforceConstraints(bool value) {
            this.enforceConstraints = value;
        }

        internal void EnableConstraints()
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.EnableConstraints|INFO> %d#\n", ObjectID);
            try {
                bool errors = false;
                for (ConstraintEnumerator constraints = new ConstraintEnumerator(this); constraints.GetNext(); ) {
                    Constraint constraint = (Constraint)constraints.GetConstraint();
                    errors |= constraint.IsConstraintViolated();
                }

                foreach (DataTable table in Tables) {
                    foreach (DataColumn column in table.Columns) {
                        if (!column.AllowDBNull) {
                            errors |= column.IsNotAllowDBNullViolated();
                        }
                        if (column.MaxLength >= 0) {
                            errors |= column.IsMaxLengthViolated();
                        }
                    }
                }

                if (errors)
                    this.FailedEnableConstraints();
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>Gets or
        ///       sets the name of this <see cref='System.Data.DataSet'/> .</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(""),
        ResDescriptionAttribute(Res.DataSetDataSetNameDescr)
        ]
        public string DataSetName {
            get {
                return dataSetName;
            }
            set {
                Bid.Trace("<ds.DataSet.set_DataSetName|API> %d#, '%ls'\n", ObjectID, value);
                if (value != dataSetName) {
                    if (value == null || value.Length == 0)
                        throw ExceptionBuilder.SetDataSetNameToEmpty();
                    DataTable conflicting = Tables[value, Namespace];
                    if ((conflicting != null) && (!conflicting.fNestedInDataset))
                        throw ExceptionBuilder.SetDataSetNameConflicting(value);
                    RaisePropertyChanging("DataSetName");
                    this.dataSetName = value;
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(""),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataSetNamespaceDescr)
        ]
        public string Namespace {
            get {
                return namespaceURI;
            }
            set {
                Bid.Trace("<ds.DataSet.set_Namespace|API> %d#, '%ls'\n", ObjectID, value);
                if (value == null)
                    value = String.Empty;
                if (value != namespaceURI) {
                    RaisePropertyChanging("Namespace");
                    foreach (DataTable dt in Tables) {
                        if (dt.tableNamespace != null)
                            continue;
                        if ((dt.NestedParentRelations.Length == 0) ||
                            (dt.NestedParentRelations.Length == 1 && dt.NestedParentRelations[0].ChildTable == dt)) {
                            //                            dt.SelfNestedWithOneRelation) { // this is wrong bug it was previous behavior
                            if (Tables.Contains(dt.TableName, value, false, true))
                                throw ExceptionBuilder.DuplicateTableName2(dt.TableName, value);
                            dt.CheckCascadingNamespaceConflict(value);
                            dt.DoRaiseNamespaceChange();
                        }
                    }
                    namespaceURI = value;


                    if (Common.ADP.IsEmpty(value))
                        _datasetPrefix = String.Empty;
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(""),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataSetPrefixDescr)
        ]
        public string Prefix {
            get { return _datasetPrefix; }
            set {
                if (value == null)
                    value = String.Empty;

                if ((XmlConvert.DecodeName(value) == value) &&
                    (XmlConvert.EncodeName(value) != value))
                    throw ExceptionBuilder.InvalidPrefix(value);


                if (value != _datasetPrefix) {
                    RaisePropertyChanging("Prefix");
                    _datasetPrefix = value;
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets the collection of custom user information.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        Browsable(false),
        ResDescriptionAttribute(Res.ExtendedPropertiesDescr)
        ]
        public PropertyCollection ExtendedProperties {
            get {
                if (extendedProperties == null) {
                    extendedProperties = new PropertyCollection();
                }
                return extendedProperties;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether there are errors in any
        ///       of the rows in any of the tables of this <see cref='System.Data.DataSet'/> .
        ///    </para>
        /// </devdoc>
        [Browsable(false), ResDescriptionAttribute(Res.DataSetHasErrorsDescr)]
        public bool HasErrors {
            get {
                for (int i = 0; i < Tables.Count; i++) {
                    if (Tables[i].HasErrors)
                        return true;
                }
                return false;
            }
        }

        [Browsable(false)]
        public bool IsInitialized {
            get {
                return !fInitInProgress;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the locale information used to compare strings within the table.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataSetLocaleDescr)
        ]
        public CultureInfo Locale {
            get {
                // used for comparing not formating/parsing
                Debug.Assert(null != _culture, "DataSet.Locale: null culture");
                return _culture;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataSet.set_Locale|API> %d#\n", ObjectID);
                try {
                    if (value != null) {
                        if (!_culture.Equals(value)) {
                            SetLocaleValue(value, true);
                        }
                        _cultureUserSet = true;
                    }
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        internal void SetLocaleValue(CultureInfo value, bool userSet) {
            bool flag = false;
            bool exceptionThrown = false;
            int tableCount = 0;

            CultureInfo oldLocale = _culture;
            bool oldUserSet = _cultureUserSet;

            try {
                _culture = value;
                _cultureUserSet = userSet;

                foreach (DataTable table in Tables) {
                    if (!table.ShouldSerializeLocale()) {
                        bool retchk = table.SetLocaleValue(value, false, false);
                        //Debug.Assert(retchk == table.ShouldSerializeLocale(), "unexpected setting of table locale"); may fire with Deserialize
                    }
                }

                flag = ValidateLocaleConstraint();
                if (flag) {
                    flag = false;
                    foreach (DataTable table in Tables) {
                        tableCount++;
                        if (!table.ShouldSerializeLocale()) {
                            table.SetLocaleValue(value, false, true);
                        }
                    }
                    flag = true;
                }
            }
            catch {
                exceptionThrown = true;
                throw;
            }
            finally {
                if (!flag) { // reset old locale if ValidationFailed or exception thrown
                    _culture = oldLocale;
                    _cultureUserSet = oldUserSet;
                    foreach (DataTable table in Tables) {
                        if (!table.ShouldSerializeLocale()) {
                            table.SetLocaleValue(oldLocale, false, false);
                        }
                    }
                    try {
                        for (int i = 0; i < tableCount; ++i) {
                            if (!Tables[i].ShouldSerializeLocale()) {
                                Tables[i].SetLocaleValue(oldLocale, false, true);
                            }
                        }
                    }
                    catch (Exception e) {
                        if (!Common.ADP.IsCatchableExceptionType(e)) {
                            throw;
                        }
                        Common.ADP.TraceExceptionWithoutRethrow(e);
                    }
                    if (!exceptionThrown) {
                        throw ExceptionBuilder.CannotChangeCaseLocale(null);
                    }
                }
            }
        }

        internal bool ShouldSerializeLocale() {
            // this method is used design-time scenarios via reflection
            //   by the property grid to show the Locale property in bold or not
            //   by the code dom for persisting the Locale property or not

            // we always want the locale persisted if set by user or different the current thread
            // but that logic should by performed by the serializion code
            return _cultureUserSet;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site {
            get {
                return base.Site;
            }
            set {
                ISite oldSite = Site;
                if (value == null && oldSite != null) {
                    IContainer cont = oldSite.Container;

                    if (cont != null) {
                        for (int i = 0; i < Tables.Count; i++) {
                            if (Tables[i].Site != null) {
                                cont.Remove(Tables[i]);
                            }
                        }
                    }
                }
                base.Site = value;
            }
        }



        /// <devdoc>
        ///    <para>
        ///       Get the collection of relations that link tables and
        ///       allow navigation from parent tables to child tables.
        ///    </para>
        /// </devdoc>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataSetRelationsDescr)
        ]
        public DataRelationCollection Relations {
            get {
                return relationCollection;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Indicates whether <see cref='Relations'/> property should be persisted.
        ///    </para>
        /// </devdoc>
        protected virtual bool ShouldSerializeRelations() {
            return true; /*Relations.Count > 0;*/ // VS7 300569
        }

        /// <devdoc>
        ///    <para>
        ///       Resets the <see cref='System.Data.DataSet.Relations'/> property to its default state.
        ///    </para>
        /// </devdoc>
        private void ResetRelations()
        {
            Relations.Clear();
        }

        /// <devdoc>
        /// <para>Gets the collection of tables contained in the <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataSetTablesDescr)
        ]
        public DataTableCollection Tables {
            get {
                return tableCollection;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Indicates whether <see cref='System.Data.DataSet.Tables'/> property should be persisted.
        ///    </para>
        /// </devdoc>
        protected virtual bool ShouldSerializeTables() {
            return true;/*(Tables.Count > 0);*/ // VS7 300569
        }

        /// <devdoc>
        ///    <para>
        ///       Resets the <see cref='System.Data.DataSet.Tables'/> property to its default state.
        ///    </para>
        /// </devdoc>
        private void ResetTables()
        {
            Tables.Clear();
        }

        internal bool FBoundToDocument {
            get {
                return fBoundToDocument;
            }
            set {
                fBoundToDocument = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Commits all the changes made to this <see cref='System.Data.DataSet'/> since it was loaded or the last
        ///       time <see cref='System.Data.DataSet.AcceptChanges'/> was called.
        ///    </para>
        /// </devdoc>
        public void AcceptChanges()
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.AcceptChanges|API> %d#\n", ObjectID);
            try {
                for (int i = 0; i < Tables.Count; i++)
                    Tables[i].AcceptChanges();
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal event PropertyChangedEventHandler PropertyChanging {
            add {
                onPropertyChangingDelegate += value;
            }
            remove {
                onPropertyChangingDelegate -= value;
            }
        }

        /// <devdoc>
        ///    <para>Occurs when attempting to merge schemas for two tables with the same
        ///       name.</para>
        /// </devdoc>
        [
         ResCategoryAttribute(Res.DataCategory_Action),
         ResDescriptionAttribute(Res.DataSetMergeFailedDescr)
        ]
        public event MergeFailedEventHandler MergeFailed {
            add {
                onMergeFailed += value;
            }
            remove {
                onMergeFailed -= value;
            }
        }

        internal event DataRowCreatedEventHandler DataRowCreated {
            add {
                onDataRowCreated += value;
            }
            remove {
                onDataRowCreated -= value;
            }
        }

        internal event DataSetClearEventhandler ClearFunctionCalled {
            add {
                onClearFunctionCalled += value;
            }
            remove {
                onClearFunctionCalled -= value;
            }
        }

        [
         ResCategoryAttribute(Res.DataCategory_Action),
         ResDescriptionAttribute(Res.DataSetInitializedDescr)
        ]
        public event System.EventHandler Initialized {
            add {
                onInitialized += value;
            }
            remove {
                onInitialized -= value;
            }
        }

        public void BeginInit() {
            fInitInProgress = true;
        }

        public void EndInit() {
            Tables.FinishInitCollection();
            for (int i = 0; i < Tables.Count; i++) {
                Tables[i].Columns.FinishInitCollection();
            }

            for (int i = 0; i < Tables.Count; i++) {
                Tables[i].Constraints.FinishInitConstraints();
            }

            ((DataRelationCollection.DataSetRelationCollection)Relations).FinishInitRelations();

            fInitInProgress = false;
            OnInitialized();
        }

        /// <devdoc>
        /// <para>Clears the <see cref='System.Data.DataSet'/> of any data by removing all rows in all tables.</para>
        /// </devdoc>
        public void Clear()
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Clear|API> %d#\n", ObjectID);
            try {
                OnClearFunctionCalled(null);
                bool fEnforce = EnforceConstraints;
                EnforceConstraints = false;
                for (int i = 0; i < Tables.Count; i++)
                    Tables[i].Clear();
                EnforceConstraints = fEnforce;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        /// <para>Clones the structure of the <see cref='System.Data.DataSet'/>, including all <see cref='System.Data.DataTable'/> schemas, relations, and
        ///    constraints.</para>
        /// </devdoc>
        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)] 
        public virtual DataSet Clone() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Clone|API> %d#\n", ObjectID);
            try {
                DataSet ds = (DataSet)Activator.CreateInstance(this.GetType(), true);

                if (ds.Tables.Count > 0)  // [....] : To clean up all the schema in strong typed dataset.
                    ds.Reset();

                //copy some original dataset properties
                ds.DataSetName = DataSetName;
                ds.CaseSensitive = CaseSensitive;
                ds._culture = _culture;
                ds._cultureUserSet = _cultureUserSet;
                ds.EnforceConstraints = EnforceConstraints;
                ds.Namespace = Namespace;
                ds.Prefix = Prefix;
                ds.RemotingFormat = RemotingFormat;
                ds.fIsSchemaLoading = true; //delay expression evaluation


                // ...Tables...
                DataTableCollection tbls = Tables;
                for (int i = 0; i < tbls.Count; i++) {
                    DataTable dt = tbls[i].Clone(ds);
                    dt.tableNamespace = tbls[i].Namespace; // hardcode the namespace for a second to not mess up
                    // DataRelation cloning.
                    ds.Tables.Add(dt);
                }

                // ...Constraints...
                for (int i = 0; i < tbls.Count; i++) {
                    ConstraintCollection constraints = tbls[i].Constraints;
                    for (int j = 0; j < constraints.Count; j++) {
                        if (constraints[j] is UniqueConstraint)
                            continue;
                        ForeignKeyConstraint foreign = constraints[j] as ForeignKeyConstraint;
                        if (foreign.Table == foreign.RelatedTable)
                            continue;// we have already added this foreign key in while cloning the datatable
                        ds.Tables[i].Constraints.Add(constraints[j].Clone(ds));
                    }
                }

                // ...Relations...
                DataRelationCollection rels = Relations;
                for (int i = 0; i < rels.Count; i++) {
                    DataRelation rel = rels[i].Clone(ds);
                    rel.CheckMultipleNested = false; // disable the check for multiple nested parent
                    ds.Relations.Add(rel);
                    rel.CheckMultipleNested = true; // enable the check for multiple nested parent
                }

                // ...Extended Properties...
                if (this.extendedProperties != null) {
                    foreach (Object key in this.extendedProperties.Keys) {
                        ds.ExtendedProperties[key] = this.extendedProperties[key];
                    }
                }

                foreach (DataTable table in Tables)
                    foreach (DataColumn col in table.Columns)
                        if (col.Expression.Length != 0)
                            ds.Tables[table.TableName, table.Namespace].Columns[col.ColumnName].Expression = col.Expression;

                for (int i = 0; i < tbls.Count; i++) {
                    ds.Tables[i].tableNamespace = tbls[i].tableNamespace; // undo the hardcoding of the namespace
                }

                ds.fIsSchemaLoading = false; //reactivate column computations

                return ds;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        /// <para>Copies both the structure and data for this <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        public DataSet Copy()
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Copy|API> %d#\n", ObjectID);
            try {
                DataSet dsNew = Clone();
                bool fEnforceConstraints = dsNew.EnforceConstraints;
                dsNew.EnforceConstraints = false;
                foreach (DataTable table in this.Tables)
                {
                    DataTable destTable = dsNew.Tables[table.TableName, table.Namespace];

                    foreach (DataRow row in table.Rows)
                        table.CopyRow(destTable, row);
                }

                dsNew.EnforceConstraints = fEnforceConstraints;

                return dsNew;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal Int32 EstimatedXmlStringSize()
        {
            Int32 bytes = 100;
            for (int i = 0; i < Tables.Count; i++) {
                Int32 rowBytes = (Tables[i].TableName.Length + 4) << 2;
                DataTable table = Tables[i];
                for (int j = 0; j < table.Columns.Count; j++) {
                    rowBytes += ((table.Columns[j].ColumnName.Length + 4) << 2);
                    rowBytes += 20;
                }
                bytes += table.Rows.Count * rowBytes;
            }

            return bytes;
        }

        /// <devdoc>
        /// <para>Returns a copy of the <see cref='System.Data.DataSet'/> that contains all changes made to
        ///    it since it was loaded or <see cref='System.Data.DataSet.AcceptChanges'/>
        ///    was last called.</para>
        /// </devdoc>
        public DataSet GetChanges()
        {
            return GetChanges(DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
        }

        private struct TableChanges {
            private BitArray _rowChanges;
            private int _hasChanges;

            internal TableChanges(int rowCount) {
                _rowChanges = new BitArray(rowCount);
                _hasChanges = 0;
            }
            internal int HasChanges {
                get { return _hasChanges; }
                set { _hasChanges = value; }
            }
            internal bool this[int index] {
                get { return _rowChanges[index]; }
                set {
                    Debug.Assert(value && !_rowChanges[index], "setting twice or to false");
                    _rowChanges[index] = value;
                    _hasChanges++;
                }
            }
        }

        public DataSet GetChanges(DataRowState rowStates)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.GetChanges|API> %d#, rowStates=%d{ds.DataRowState}\n", ObjectID, (int)rowStates);
            try {
                DataSet dsNew = null;
                bool fEnforceConstraints = false;
                if (0 != (rowStates & ~(DataRowState.Added | DataRowState.Deleted | DataRowState.Modified | DataRowState.Unchanged))) {
                    throw ExceptionBuilder.InvalidRowState(rowStates);
                }

                // Initialize all the individual table bitmaps.
                TableChanges[] bitMatrix = new TableChanges[Tables.Count];
                for (int i = 0; i < bitMatrix.Length; ++i) {
                    bitMatrix[i] = new TableChanges(Tables[i].Rows.Count);
                }

                // find all the modified rows and their parents
                MarkModifiedRows(bitMatrix, rowStates);

                // copy the changes to a cloned table
                for (int i = 0; i < bitMatrix.Length; ++i) {
                    Debug.Assert(0 <= bitMatrix[i].HasChanges, "negative change count");
                    if (0 < bitMatrix[i].HasChanges) {
                        if (null == dsNew) {
                            dsNew = this.Clone();
                            fEnforceConstraints = dsNew.EnforceConstraints;
                            dsNew.EnforceConstraints = false;
                        }

                        DataTable table = this.Tables[i];
                        DataTable destTable = dsNew.Tables[table.TableName, table.Namespace];
                        Debug.Assert(bitMatrix[i].HasChanges <= table.Rows.Count, "to many changes");

                        for (int j = 0; 0 < bitMatrix[i].HasChanges; ++j) { // Loop through the rows.
                            if (bitMatrix[i][j]) {
                                table.CopyRow(destTable, table.Rows[j]);
                                bitMatrix[i].HasChanges--;
                            }
                        }
                    }
                }

                if (null != dsNew) {
                    dsNew.EnforceConstraints = fEnforceConstraints;
                }
                return dsNew;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private void MarkModifiedRows(TableChanges[] bitMatrix, DataRowState rowStates) {
            // for every table, every row & every relation find the modified rows and for non-deleted rows, their parents
            for (int tableIndex = 0; tableIndex < bitMatrix.Length; ++tableIndex) {
                DataRowCollection rows = Tables[tableIndex].Rows;
                int rowCount = rows.Count;

                for (int rowIndex = 0; rowIndex < rowCount; ++rowIndex) {
                    DataRow row = rows[rowIndex];
                    DataRowState rowState = row.RowState;
                    Debug.Assert(DataRowState.Added == rowState ||
                                 DataRowState.Deleted == rowState ||
                                 DataRowState.Modified == rowState ||
                                 DataRowState.Unchanged == rowState,
                                 "unexpected DataRowState");

                    // if bit not already set and row is modified
                    if ((0 != (rowStates & rowState)) && !bitMatrix[tableIndex][rowIndex]) {
                        bitMatrix[tableIndex][rowIndex] = true;

                        if (DataRowState.Deleted != rowState) {
                            MarkRelatedRowsAsModified(bitMatrix, row);
                        }
                    }
                }
            }
        }

        private void MarkRelatedRowsAsModified(TableChanges[] bitMatrix, DataRow row) {
            DataRelationCollection relations = row.Table.ParentRelations;
            int relationCount = relations.Count;
            for (int relatedIndex = 0; relatedIndex < relationCount; ++relatedIndex) {
                DataRow[] relatedRows = row.GetParentRows(relations[relatedIndex], DataRowVersion.Current);

                foreach (DataRow relatedRow in relatedRows) {
                    int relatedTableIndex = this.Tables.IndexOf(relatedRow.Table);
                    int relatedRowIndex = relatedRow.Table.Rows.IndexOf(relatedRow);

                    if (!bitMatrix[relatedTableIndex][relatedRowIndex]) {
                        bitMatrix[relatedTableIndex][relatedRowIndex] = true;

                        if (DataRowState.Deleted != relatedRow.RowState) {
                            // recurse into related rows
                            MarkRelatedRowsAsModified(bitMatrix, relatedRow);
                        }
                    }
                }
            }
        }

        IList System.ComponentModel.IListSource.GetList() {
            return DefaultViewManager;
        }

        internal string GetRemotingDiffGram(DataTable table)
        {
            StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter writer = new XmlTextWriter(strWriter);
            writer.Formatting = Formatting.Indented;
            if (strWriter != null) {
                // Create and save the updates
                new NewDiffgramGen(table, false).Save(writer, table);
            }

            return strWriter.ToString();
        }

        public string GetXml()
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.GetXml|API> %d#\n", ObjectID);
            try {

                // StringBuilder strBuilder = new StringBuilder(EstimatedXmlStringSize());
                // StringWriter strWriter = new StringWriter(strBuilder);
                StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
                if (strWriter != null) {
                    XmlTextWriter w = new XmlTextWriter(strWriter);
                    w.Formatting = Formatting.Indented;
                    new XmlDataTreeWriter(this).Save(w, false);
                }
                return strWriter.ToString();
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public string GetXmlSchema()
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.GetXmlSchema|API> %d#\n", ObjectID);
            try {
                StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
                XmlTextWriter writer = new XmlTextWriter(strWriter);
                writer.Formatting = Formatting.Indented;
                if (strWriter != null) {
                    (new XmlTreeGen(SchemaFormat.Public)).Save(this, writer);
                }

                return strWriter.ToString();
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal string GetXmlSchemaForRemoting(DataTable table)
        {
            StringWriter strWriter = new StringWriter(CultureInfo.InvariantCulture);
            XmlTextWriter writer = new XmlTextWriter(strWriter);
            writer.Formatting = Formatting.Indented;
            if (strWriter != null) {
                if (table == null) {
                    if (this.SchemaSerializationMode == SchemaSerializationMode.ExcludeSchema)
                        (new XmlTreeGen(SchemaFormat.RemotingSkipSchema)).Save(this, writer);
                    else
                        (new XmlTreeGen(SchemaFormat.Remoting)).Save(this, writer);
                }
                else { // no skip schema support for typed datatable
                    (new XmlTreeGen(SchemaFormat.Remoting)).Save(table, writer);
                }
            }

            return strWriter.ToString();
        }


        /// <devdoc>
        /// <para>Gets a value indicating whether the <see cref='System.Data.DataSet'/> has changes, including new,
        ///    deleted, or modified rows.</para>
        /// </devdoc>
        public bool HasChanges()
        {
            return HasChanges(DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
        }

        /// <devdoc>
        /// <para>Gets a value indicating whether the <see cref='System.Data.DataSet'/> has changes, including new,
        ///    deleted, or modified rows, filtered by <see cref='System.Data.DataRowState'/>.</para>
        /// </devdoc>
        public bool HasChanges(DataRowState rowStates)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.HasChanges|API> %d#, rowStates=%d{ds.DataRowState}\n", ObjectID, (int)rowStates);

            try {
                const DataRowState allRowStates = DataRowState.Detached | DataRowState.Unchanged | DataRowState.Added | DataRowState.Deleted | DataRowState.Modified;

                if ((rowStates & (~allRowStates)) != 0) {
                    throw ExceptionBuilder.ArgumentOutOfRange("rowState");
                }

                for (int i = 0; i < Tables.Count; i++) {
                    DataTable table = Tables[i];

                    for (int j = 0; j < table.Rows.Count; j++) {
                        DataRow row = table.Rows[j];
                        if ((row.RowState & rowStates) != 0) {
                            return true;
                        }
                    }
                }
                return false;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        /// <para>Infer the XML schema from the specified <see cref='System.IO.TextReader'/> into the <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        public void InferXmlSchema(XmlReader reader, string[] nsArray)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.InferXmlSchema|API> %d#\n", ObjectID);
            try {
                if (reader == null)
                    return;

                XmlDocument xdoc = new XmlDocument();
                if (reader.NodeType == XmlNodeType.Element) {
                    XmlNode node = xdoc.ReadNode(reader);
                    xdoc.AppendChild(node);
                }
                else
                    xdoc.Load(reader);
                if (xdoc.DocumentElement == null)
                    return;

                InferSchema(xdoc, nsArray, XmlReadMode.InferSchema);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        /// <para>Infer the XML schema from the specified <see cref='System.IO.TextReader'/> into the <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        public void InferXmlSchema(Stream stream, string[] nsArray)
        {
            if (stream == null)
                return;

            InferXmlSchema(new XmlTextReader(stream), nsArray);
        }

        /// <devdoc>
        /// <para>Infer the XML schema from the specified <see cref='System.IO.TextReader'/> into the <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        public void InferXmlSchema(TextReader reader, string[] nsArray)
        {
            if (reader == null)
                return;

            InferXmlSchema(new XmlTextReader(reader), nsArray);
        }

        /// <devdoc>
        /// <para>Infer the XML schema from the specified file into the <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void InferXmlSchema(String fileName, string[] nsArray)
        {
            XmlTextReader xr = new XmlTextReader(fileName);
            try {
                InferXmlSchema(xr, nsArray);
            }
            finally {
                xr.Close();
            }
        }

        /// <devdoc>
        /// <para>Reads the XML schema from the specified <see cref='T:System.Xml.XMLReader'/> into the <see cref='System.Data.DataSet'/>
        /// .</para>
        /// </devdoc>
        public void ReadXmlSchema(XmlReader reader)
        {
            ReadXmlSchema(reader, false);
        }

        internal void ReadXmlSchema(XmlReader reader, bool denyResolving)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.ReadXmlSchema|INFO> %d#, reader, denyResolving=%d{bool}\n", ObjectID, denyResolving);
            try {
                int iCurrentDepth = -1;

                if (reader == null)
                    return;

                if (reader is XmlTextReader)
                    ((XmlTextReader)reader).WhitespaceHandling = WhitespaceHandling.None;

                XmlDocument xdoc = new XmlDocument(); // we may need this to infer the schema

                if (reader.NodeType == XmlNodeType.Element)
                    iCurrentDepth = reader.Depth;

                reader.MoveToContent();

                if (reader.NodeType == XmlNodeType.Element) {
                    // if reader points to the schema load it...

                    if (reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI == Keywords.XDRNS) {
                        // load XDR schema and exit
                        ReadXDRSchema(reader);
                        return;
                    }

                    if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI == Keywords.XSDNS) {
                        // load XSD schema and exit
                        ReadXSDSchema(reader, denyResolving);
                        return;
                    }

                    if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))
                        throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);

                    // ... otherwise backup the top node and all its attributes
                    XmlElement topNode = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    if (reader.HasAttributes) {
                        int attrCount = reader.AttributeCount;
                        for (int i = 0; i < attrCount; i++) {
                            reader.MoveToAttribute(i);
                            if (reader.NamespaceURI.Equals(Keywords.XSD_XMLNS_NS))
                                topNode.SetAttribute(reader.Name, reader.GetAttribute(i));
                            else {
                                XmlAttribute attr = topNode.SetAttributeNode(reader.LocalName, reader.NamespaceURI);
                                attr.Prefix = reader.Prefix;
                                attr.Value = reader.GetAttribute(i);
                            }
                        }
                    }
                    reader.Read();

                    while (MoveToElement(reader, iCurrentDepth)) {

                        // if reader points to the schema load it...
                        if (reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI == Keywords.XDRNS) {
                            // load XDR schema and exit
                            ReadXDRSchema(reader);
                            return;
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI == Keywords.XSDNS) {
                            // load XSD schema and exit
                            ReadXSDSchema(reader, denyResolving);
                            return;
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))
                            throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);


                        XmlNode node = xdoc.ReadNode(reader);
                        topNode.AppendChild(node);

                    }

                    // read the closing tag of the current element
                    ReadEndElement(reader);

                    // if we are here no schema has been found
                    xdoc.AppendChild(topNode);

                    // so we InferSchema
                    InferSchema(xdoc, null, XmlReadMode.Auto);
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal bool MoveToElement(XmlReader reader, int depth) {
            while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.Element && reader.Depth > depth) {
                reader.Read();
            }
            return (reader.NodeType == XmlNodeType.Element);
        }

        private static void MoveToElement(XmlReader reader) {
            while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.Element) {
                reader.Read();
            }
        }
        internal void ReadEndElement(XmlReader reader) {
            while (reader.NodeType == XmlNodeType.Whitespace) {
                reader.Skip();
            }
            if (reader.NodeType == XmlNodeType.None) {
                reader.Skip();
            }
            else if (reader.NodeType == XmlNodeType.EndElement) {
                reader.ReadEndElement();
            }
        }

        internal void ReadXSDSchema(XmlReader reader, bool denyResolving) {
            XmlSchemaSet sSet = new XmlSchemaSet();

            int schemaFragmentCount = 1;
            //read from current schmema element
            if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI == Keywords.XSDNS) {
                if (reader.HasAttributes) {
                    string attribValue = reader.GetAttribute(Keywords.MSD_FRAGMENTCOUNT, Keywords.MSDNS); // this must not move the position
                    if (!Common.ADP.IsEmpty(attribValue)) {
                        schemaFragmentCount = int.Parse(attribValue, null);
                    }
                }
            }

            while (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI == Keywords.XSDNS) {
                XmlSchema s = XmlSchema.Read(reader, null);
                sSet.Add(s);
                //read the end tag
                ReadEndElement(reader);

                if (--schemaFragmentCount > 0) {
                    MoveToElement(reader);
                }
                while (reader.NodeType == XmlNodeType.Whitespace) {
                    reader.Skip();
                }
            }
            sSet.Compile();
            XSDSchema schema = new XSDSchema();
            schema.LoadSchema(sSet, this);
        }

        internal void ReadXDRSchema(XmlReader reader) {
            XmlDocument xdoc = new XmlDocument(); // we may need this to infer the schema
            XmlNode schNode = xdoc.ReadNode(reader);
            xdoc.AppendChild(schNode);
            XDRSchema schema = new XDRSchema(this, false);
            this.DataSetName = xdoc.DocumentElement.LocalName;
            schema.LoadSchema((XmlElement)schNode, this);
        }

        /// <devdoc>
        /// <para>Reads the XML schema from the specified <see cref='System.IO.Stream'/> into the
        /// <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        public void ReadXmlSchema(Stream stream)
        {
            if (stream == null)
                return;

            ReadXmlSchema(new XmlTextReader(stream), false);
        }

        /// <devdoc>
        /// <para>Reads the XML schema from the specified <see cref='System.IO.TextReader'/> into the <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        public void ReadXmlSchema(TextReader reader)
        {
            if (reader == null)
                return;

            ReadXmlSchema(new XmlTextReader(reader), false);
        }

        /// <devdoc>
        /// <para>Reads the XML schema from the specified file into the <see cref='System.Data.DataSet'/>.</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void ReadXmlSchema(String fileName)
        {
            XmlTextReader xr = new XmlTextReader(fileName);
            try {
                ReadXmlSchema(xr, false);
            }
            finally {
                xr.Close();
            }
        }

        #region WriteXmlSchema
        /// <summary>Writes the <see cref='DataSet'/> structure as an XML schema to using the specified <see cref='Stream'/> object.</summary>
        /// <param name="stream">A <see cref='Stream'/> object used to write to a file.</param>
        public void WriteXmlSchema(Stream stream)
        {
            this.WriteXmlSchema(stream, SchemaFormat.Public, (Converter<Type, string>)null);
        }

        /// <summary>Writes the <see cref='DataSet'/> structure as an XML schema to using the specified <see cref='Stream'/> object.</summary>
        /// <param name="stream">A <see cref='Stream'/> object used to write to a file.</param>
        /// <param name="multipleTargetConverter">A delegate used to convert <see cref='Type'/> into string.</param>
        public void WriteXmlSchema(Stream stream, Converter<Type, string> multipleTargetConverter)
        {
            ADP.CheckArgumentNull(multipleTargetConverter, "multipleTargetConverter");
            this.WriteXmlSchema(stream, SchemaFormat.Public, multipleTargetConverter);
        }

        /// <summary>Writes the <see cref='DataSet'/> structure as an XML schema to a file.</summary>
        /// <param name="fileName">The file name (including the path) to which to write.</param>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXmlSchema(String fileName)
        {
            this.WriteXmlSchema(fileName, SchemaFormat.Public, (Converter<Type, string>)null);
        }

        /// <summary>Writes the <see cref='DataSet'/> structure as an XML schema to a file.</summary>
        /// <param name="fileName">The file name (including the path) to which to write.</param>
        /// <param name="multipleTargetConverter">A delegate used to convert <see cref='Type'/> into string.</param>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXmlSchema(String fileName, Converter<Type, string> multipleTargetConverter)
        {
            ADP.CheckArgumentNull(multipleTargetConverter, "multipleTargetConverter");
            this.WriteXmlSchema(fileName, SchemaFormat.Public, multipleTargetConverter);
        }

        /// <summary>Writes the <see cref='DataSet'/> structure as an XML schema to a <see cref='TextWriter'/> object.</summary>
        /// <param name="writer">The <see cref='TextWriter'/> object with which to write.</param>
        public void WriteXmlSchema(TextWriter writer)
        {
            this.WriteXmlSchema(writer, SchemaFormat.Public, (Converter<Type, string>)null);
        }

        /// <summary>Writes the <see cref='DataSet'/> structure as an XML schema to a <see cref='TextWriter'/> object.</summary>
        /// <param name="writer">The <see cref='TextWriter'/> object with which to write.</param>
        /// <param name="multipleTargetConverter">A delegate used to convert <see cref='Type'/> into string.</param>
        public void WriteXmlSchema(TextWriter writer, Converter<Type, string> multipleTargetConverter)
        {
            ADP.CheckArgumentNull(multipleTargetConverter, "multipleTargetConverter");
            this.WriteXmlSchema(writer, SchemaFormat.Public, multipleTargetConverter);
        }

        /// <summary>Writes the <see cref='DataSet'/> structure as an XML schema to an <see cref='XmlWriter'/> object.</summary>
        /// <param name="writer">The <see cref='XmlWriter'/> object with which to write.</param>
        public void WriteXmlSchema(XmlWriter writer)
        {
            this.WriteXmlSchema(writer, SchemaFormat.Public, (Converter<Type, string>)null);
        }

        /// <summary>Writes the <see cref='DataSet'/> structure as an XML schema to an <see cref='XmlWriter'/> object.</summary>
        /// <param name="writer">The <see cref='XmlWriter'/> object with which to write.</param>
        /// <param name="multipleTargetConverter">A delegate used to convert <see cref='Type'/> into string.</param>
        public void WriteXmlSchema(XmlWriter writer, Converter<Type, string> multipleTargetConverter)
        {
            ADP.CheckArgumentNull(multipleTargetConverter, "multipleTargetConverter");
            this.WriteXmlSchema(writer, SchemaFormat.Public, multipleTargetConverter);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private void WriteXmlSchema(String fileName, SchemaFormat schemaFormat, Converter<Type, string> multipleTargetConverter)
        {
            XmlTextWriter xw = new XmlTextWriter( fileName, null );
            try {
                xw.Formatting = Formatting.Indented;
                xw.WriteStartDocument(true);
                this.WriteXmlSchema(xw, schemaFormat, multipleTargetConverter);
                xw.WriteEndDocument();
            }
            finally {
                xw.Close();
            }
        }

        private void WriteXmlSchema(Stream stream, SchemaFormat schemaFormat, Converter<Type, string> multipleTargetConverter)
        {
            if (stream == null)
                return;

            XmlTextWriter w = new XmlTextWriter(stream, null);
            w.Formatting = Formatting.Indented;

            this.WriteXmlSchema(w, schemaFormat, multipleTargetConverter);
        }

        private void WriteXmlSchema(TextWriter writer, SchemaFormat schemaFormat, Converter<Type, string> multipleTargetConverter)
        {
            if (writer == null)
                return;

            XmlTextWriter w = new XmlTextWriter(writer);
            w.Formatting = Formatting.Indented;

            this.WriteXmlSchema(w, schemaFormat, multipleTargetConverter);
        }

        private void WriteXmlSchema(XmlWriter writer, SchemaFormat schemaFormat, Converter<Type, string> multipleTargetConverter)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.WriteXmlSchema|INFO> %d#, schemaFormat=%d{ds.SchemaFormat}\n", ObjectID, (int)schemaFormat);
            try {
                // Generate SchemaTree and write it out
                if (writer != null) {
                    XmlTreeGen treeGen = null;
                    if (schemaFormat == SchemaFormat.WebService &&
                        SchemaSerializationMode == SchemaSerializationMode.ExcludeSchema &&
                        writer.WriteState == WriteState.Element) {
                        treeGen = new XmlTreeGen(SchemaFormat.WebServiceSkipSchema);
                    }
                    else {
                        treeGen = new XmlTreeGen(schemaFormat);
                    }

                    treeGen.Save(this, (DataTable)null, writer, false, multipleTargetConverter);
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }
        #endregion

        /// <devdoc>
        /// </devdoc>
        public XmlReadMode ReadXml(XmlReader reader)
        {
            return ReadXml(reader, false);
        }


        internal XmlReadMode ReadXml(XmlReader reader, bool denyResolving)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.ReadXml|INFO> %d#, denyResolving=%d{bool}\n", ObjectID, denyResolving);
            try {
                
                DataTable.DSRowDiffIdUsageSection rowDiffIdUsage = new DataTable.DSRowDiffIdUsageSection();
                try {
                    bool fDataFound = false;
                    bool fSchemaFound = false;
                    bool fDiffsFound = false;
                    bool fIsXdr = false;
                    int iCurrentDepth = -1;
                    XmlReadMode ret = XmlReadMode.Auto;
                    bool isEmptyDataSet = false;
                    bool topNodeIsProcessed = false; // we chanche topnode and there is just one case that we miss to process it
                    // it is : <elem attrib1="Attrib">txt</elem>

                    // clear the hashtable to avoid conflicts between diffgrams, SqlHotFix 782
                    rowDiffIdUsage.Prepare(this);

                    if (reader == null)
                        return ret;

                    if (Tables.Count == 0) {
                        isEmptyDataSet = true;
                    }

                    if (reader is XmlTextReader)
                        ((XmlTextReader)reader).WhitespaceHandling = WhitespaceHandling.Significant;

                    XmlDocument xdoc = new XmlDocument(); // we may need this to infer the schema
                    XmlDataLoader xmlload = null;


                    reader.MoveToContent();

                    if (reader.NodeType == XmlNodeType.Element)
                        iCurrentDepth = reader.Depth;

                    if (reader.NodeType == XmlNodeType.Element) {
                        if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                            this.ReadXmlDiffgram(reader);
                            // read the closing tag of the current element
                            ReadEndElement(reader);
                            return XmlReadMode.DiffGram;
                        }

                        // if reader points to the schema load it
                        if (reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI == Keywords.XDRNS) {
                            // load XDR schema and exit
                            ReadXDRSchema(reader);
                            return XmlReadMode.ReadSchema; //since the top level element is a schema return
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI == Keywords.XSDNS) {
                            // load XSD schema and exit
                            ReadXSDSchema(reader, denyResolving);
                            return XmlReadMode.ReadSchema; //since the top level element is a schema return
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))
                            throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);

                        // now either the top level node is a table and we load it through dataReader...

                        // ... or backup the top node and all its attributes because we may need to InferSchema
                        XmlElement topNode = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        if (reader.HasAttributes) {
                            int attrCount = reader.AttributeCount;
                            for (int i = 0; i < attrCount; i++) {
                                reader.MoveToAttribute(i);
                                if (reader.NamespaceURI.Equals(Keywords.XSD_XMLNS_NS))
                                    topNode.SetAttribute(reader.Name, reader.GetAttribute(i));
                                else {
                                    XmlAttribute attr = topNode.SetAttributeNode(reader.LocalName, reader.NamespaceURI);
                                    attr.Prefix = reader.Prefix;
                                    attr.Value = reader.GetAttribute(i);
                                }
                            }
                        }
                        reader.Read();
                        string rootNodeSimpleContent = reader.Value;

                        while (MoveToElement(reader, iCurrentDepth)) {

                            if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                                this.ReadXmlDiffgram(reader);
                                // read the closing tag of the current element
                                // YUKON FIX                            ReadEndElement(reader);
                                //                            return XmlReadMode.DiffGram;
                                ret = XmlReadMode.DiffGram; // continue reading for multiple schemas
                            }

                            // if reader points to the schema load it...


                            if (!fSchemaFound && !fDataFound && reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI == Keywords.XDRNS) {
                                // load XDR schema and exit
                                ReadXDRSchema(reader);
                                fSchemaFound = true;
                                fIsXdr = true;
                                continue;
                            }

                            if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI == Keywords.XSDNS) {
                                // load XSD schema and exit
                                ReadXSDSchema(reader, denyResolving);
                                fSchemaFound = true;
                                continue;
                            }

                            if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))
                                throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);

                            if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                                this.ReadXmlDiffgram(reader);
                                fDiffsFound = true;
                                ret = XmlReadMode.DiffGram;
                            }
                            else {
                                // We have found data IFF the reader.NodeType == Element and reader.depth == currentDepth-1
                                // if reader.NodeType == whitespace, skip all white spaces.
                                // skip processing i.e. continue if the first non-whitespace node is not of type element.
                                while (!reader.EOF && reader.NodeType == XmlNodeType.Whitespace)
                                    reader.Read();
                                if (reader.NodeType != XmlNodeType.Element)
                                    continue;
                                // we found data here
                                fDataFound = true;

                                if (!fSchemaFound && Tables.Count == 0) {
                                    XmlNode node = xdoc.ReadNode(reader);
                                    topNode.AppendChild(node);
                                }
                                else {
                                    if (xmlload == null)
                                        xmlload = new XmlDataLoader(this, fIsXdr, topNode, false);
                                    xmlload.LoadData(reader);
                                    topNodeIsProcessed = true; // we process the topnode
                                    if (fSchemaFound)
                                        ret = XmlReadMode.ReadSchema;
                                    else
                                        ret = XmlReadMode.IgnoreSchema;
                                }
                            }

                        }
                        // read the closing tag of the current element
                        ReadEndElement(reader);
                        bool isfTopLevelTableSet = false;
                        bool tmpValue = this.fTopLevelTable;
                        //While inference we ignore root elements text content
                        if (!fSchemaFound && Tables.Count == 0 && !topNode.HasChildNodes) { //We shoule not come add SC of root elemnt to topNode if we are not infering
                            this.fTopLevelTable = true;
                            isfTopLevelTableSet = true;
                            if ((rootNodeSimpleContent != null && rootNodeSimpleContent.Length > 0))
                                topNode.InnerText = rootNodeSimpleContent;
                        }
                        if (!isEmptyDataSet) {
                            if ((rootNodeSimpleContent != null && rootNodeSimpleContent.Length > 0))
                                topNode.InnerText = rootNodeSimpleContent;
                        }

                        // now top node contains the data part
                        xdoc.AppendChild(topNode);

                        if (xmlload == null)
                            xmlload = new XmlDataLoader(this, fIsXdr, topNode, false);

                        if (!isEmptyDataSet && !topNodeIsProcessed) {
                            XmlElement root = xdoc.DocumentElement;
                            Debug.Assert(root.NamespaceURI != null, "root.NamespaceURI should not ne null, it should be empty string");
                            // just recognize that below given Xml represents datatable in toplevel
                            //<table attr1="foo" attr2="bar" table_Text="junk">text</table>
                            // only allow root element with simple content, if any
                            if (root.ChildNodes.Count == 0 ||
                                ((root.ChildNodes.Count == 1) && root.FirstChild.GetType() == typeof(System.Xml.XmlText))) {
                                bool initfTopLevelTable = this.fTopLevelTable;
                                // if root element maps to a datatable
                                // ds and dt cant have the samm name and ns at the same time, how to write to xml
                                if (this.DataSetName != root.Name && this.namespaceURI != root.NamespaceURI &&
                                    Tables.Contains(root.Name, (root.NamespaceURI.Length == 0) ? null : root.NamespaceURI, false, true)) {
                                    this.fTopLevelTable = true;
                                }
                                try {
                                    xmlload.LoadData(xdoc);
                                }
                                finally {
                                    this.fTopLevelTable = initfTopLevelTable; // this is not for inference, we have schema and we were skipping
                                    // topnode where it was a datatable, We must restore the value
                                }
                            }
                        }// above check and below check are orthogonal
                        // so we InferSchema
                        if (!fDiffsFound) {
                            // Load Data
                            if (!fSchemaFound && Tables.Count == 0) {
                                InferSchema(xdoc, null, XmlReadMode.Auto);
                                ret = XmlReadMode.InferSchema;
                                xmlload.FromInference = true;
                                try {
                                    xmlload.LoadData(xdoc);
                                }
                                finally {
                                    xmlload.FromInference = false;
                                }
                            }
                            //We dont need this assignement. Once we set it(where we set it during inference), it won't be changed
                            if (isfTopLevelTableSet)
                                this.fTopLevelTable = tmpValue;
                        }
                    }

                    return ret;
                }
                finally {
                    rowDiffIdUsage.Cleanup();
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }


        /// <devdoc>
        /// </devdoc>
        public XmlReadMode ReadXml(Stream stream)
        {
            if (stream == null)
                return XmlReadMode.Auto;

            return ReadXml(new XmlTextReader(stream), false);
        }

        /// <devdoc>
        /// </devdoc>
        public XmlReadMode ReadXml(TextReader reader)
        {
            if (reader == null)
                return XmlReadMode.Auto;

            return ReadXml(new XmlTextReader(reader), false);
        }

        /// <devdoc>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public XmlReadMode ReadXml(string fileName)
        {
            XmlTextReader xr = new XmlTextReader(fileName);
            try {
                return ReadXml(xr, false);
            }
            finally {
                xr.Close();
            }
        }

        internal void InferSchema(XmlDocument xdoc, string[] excludedNamespaces, XmlReadMode mode) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.InferSchema|INFO> %d#, mode=%d{ds.XmlReadMode}\n", ObjectID, (int)mode);
            try {
                string ns = xdoc.DocumentElement.NamespaceURI;
                if (null == excludedNamespaces) {
                    excludedNamespaces = new string[0];
                }
                XmlNodeReader xnr = new XmlIgnoreNamespaceReader(xdoc, excludedNamespaces);
                System.Xml.Schema.XmlSchemaInference infer = new System.Xml.Schema.XmlSchemaInference();

                infer.Occurrence = XmlSchemaInference.InferenceOption.Relaxed;

                if (mode == XmlReadMode.InferTypedSchema)
                    infer.TypeInference = XmlSchemaInference.InferenceOption.Restricted;
                else
                    infer.TypeInference = XmlSchemaInference.InferenceOption.Relaxed;

                XmlSchemaSet schemaSet = infer.InferSchema(xnr);
                schemaSet.Compile();

                XSDSchema schema = new XSDSchema();
                schema.FromInference = true;

                try {
                    schema.LoadSchema(schemaSet, this);
                }
                finally {
                    schema.FromInference = false; // this is always false if you are not calling fron inference
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private bool IsEmpty() {
            foreach (DataTable table in this.Tables)
                if (table.Rows.Count > 0)
                    return false;
            return true;
        }

        private void ReadXmlDiffgram(XmlReader reader) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.ReadXmlDiffgram|INFO> %d#\n", ObjectID);
            try {
                int d = reader.Depth;
                bool fEnforce = this.EnforceConstraints;
                this.EnforceConstraints = false;
                DataSet newDs;
                bool isEmpty = this.IsEmpty();

                if (isEmpty) {
                    newDs = this;
                }
                else {
                    newDs = this.Clone();
                    newDs.EnforceConstraints = false;
                }

                foreach (DataTable t in newDs.Tables) {
                    t.Rows.nullInList = 0;
                }
                reader.MoveToContent();
                if ((reader.LocalName != Keywords.DIFFGRAM) && (reader.NamespaceURI != Keywords.DFFNS))
                    return;
                reader.Read();
                if (reader.NodeType == XmlNodeType.Whitespace)
                    MoveToElement(reader, reader.Depth - 1 /*iCurrentDepth*/); // skip over whitespaces.

                newDs.fInLoadDiffgram = true;

                if (reader.Depth > d) {
                    if ((reader.NamespaceURI != Keywords.DFFNS) && (reader.NamespaceURI != Keywords.MSDNS)) {
                        //we should be inside the dataset part
                        XmlDocument xdoc = new XmlDocument();
                        XmlElement node = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        reader.Read();
                        if (reader.NodeType == XmlNodeType.Whitespace) {
                            MoveToElement(reader, reader.Depth - 1 /*iCurrentDepth*/); // skip over whitespaces.
                        }
                        if (reader.Depth - 1 > d) {
                            XmlDataLoader xmlload = new XmlDataLoader(newDs, false, node, false);
                            xmlload.isDiffgram = true; // turn on the special processing
                            xmlload.LoadData(reader);
                        }
                        ReadEndElement(reader);
                        if (reader.NodeType == XmlNodeType.Whitespace) {
                            MoveToElement(reader, reader.Depth - 1 /*iCurrentDepth*/); // skip over whitespaces.
                        }
                    }
                    Debug.Assert(reader.NodeType != XmlNodeType.Whitespace, "Should not be on Whitespace node");

                    if (((reader.LocalName == Keywords.SQL_BEFORE) && (reader.NamespaceURI == Keywords.DFFNS)) ||
                        ((reader.LocalName == Keywords.MSD_ERRORS) && (reader.NamespaceURI == Keywords.DFFNS)))

                    {
                        //this will consume the changes and the errors part
                        XMLDiffLoader diffLoader = new XMLDiffLoader();
                        diffLoader.LoadDiffGram(newDs, reader);
                    }

                    // get to the closing diff tag
                    while (reader.Depth > d) {
                        reader.Read();
                    }
                    // read the closing tag
                    ReadEndElement(reader);
                }

                foreach (DataTable t in newDs.Tables) {
                    if (t.Rows.nullInList > 0)
                        throw ExceptionBuilder.RowInsertMissing(t.TableName);
                }

                newDs.fInLoadDiffgram = false;

                //terrible performance!
                foreach (DataTable t in newDs.Tables) {
                    DataRelation[] nestedParentRelations = t.NestedParentRelations;
                    foreach (DataRelation rel in nestedParentRelations) {
                        if (rel.ParentTable == t) {
                            foreach (DataRow r in t.Rows) {
                                foreach (DataRelation rel2 in nestedParentRelations) {
                                    r.CheckForLoops(rel2);
                                }
                            }
                        }
                    }
                }

                if (!isEmpty) {
                    this.Merge(newDs);
                    if (this.dataSetName == "NewDataSet")
                        this.dataSetName = newDs.dataSetName;
                    newDs.EnforceConstraints = fEnforce;
                }
                this.EnforceConstraints = fEnforce;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }

        }

        /// <devdoc>
        /// </devdoc>
        public XmlReadMode ReadXml(XmlReader reader, XmlReadMode mode)
        {
            return ReadXml(reader, mode, false);
        }

        internal XmlReadMode ReadXml(XmlReader reader, XmlReadMode mode, bool denyResolving)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.ReadXml|INFO> %d#, mode=%d{ds.XmlReadMode}, denyResolving=%d{bool}\n", ObjectID, (int)mode, denyResolving);
            try {

                XmlReadMode ret = mode;

                if (reader == null)
                    return ret;

                if (mode == XmlReadMode.Auto) {
                    // Dev11 915079: nested ReadXml calls on the same DataSet must be done outside of RowDiffIdUsage scope
                    return ReadXml(reader);
                }

                DataTable.DSRowDiffIdUsageSection rowDiffIdUsage = new DataTable.DSRowDiffIdUsageSection();
                try {

                    bool fSchemaFound = false;
                    bool fDataFound = false;
                    bool fIsXdr = false;
                    int iCurrentDepth = -1;

                    // Dev11 904428: prepare and cleanup rowDiffId hashtable
                    rowDiffIdUsage.Prepare(this);

                    if (reader is XmlTextReader)
                        ((XmlTextReader)reader).WhitespaceHandling = WhitespaceHandling.Significant;

                    XmlDocument xdoc = new XmlDocument(); // we may need this to infer the schema

                    if ((mode != XmlReadMode.Fragment) && (reader.NodeType == XmlNodeType.Element))
                        iCurrentDepth = reader.Depth;

                    reader.MoveToContent();
                    XmlDataLoader xmlload = null;

                    if (reader.NodeType == XmlNodeType.Element) {
                        XmlElement topNode = null;
                        if (mode == XmlReadMode.Fragment) {
                            xdoc.AppendChild(xdoc.CreateElement("ds_sqlXmlWraPPeR"));
                            topNode = xdoc.DocumentElement;
                        }
                        else { //handle the top node
                            if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                                if ((mode == XmlReadMode.DiffGram) || (mode == XmlReadMode.IgnoreSchema)) {
                                    this.ReadXmlDiffgram(reader);
                                    // read the closing tag of the current element
                                    ReadEndElement(reader);
                                }
                                else {
                                    reader.Skip();
                                }
                                return ret;
                            }

                            if (reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI == Keywords.XDRNS) {
                                // load XDR schema and exit
                                if ((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema) &&
                                    (mode != XmlReadMode.InferTypedSchema))
                                {
                                    ReadXDRSchema(reader);
                                }
                                else {
                                    reader.Skip();
                                }
                                return ret; //since the top level element is a schema return
                            }

                            if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI == Keywords.XSDNS) {
                                // load XSD schema and exit
                                if ((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema) &&
                                    (mode != XmlReadMode.InferTypedSchema))
                                {
                                    ReadXSDSchema(reader, denyResolving);
                                }
                                else
                                    reader.Skip();
                                return ret; //since the top level element is a schema return
                            }

                            if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))
                                throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);

                            // now either the top level node is a table and we load it through dataReader...
                            // ... or backup the top node and all its attributes
                            topNode = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                            if (reader.HasAttributes) {
                                int attrCount = reader.AttributeCount;
                                for (int i = 0; i < attrCount; i++) {
                                    reader.MoveToAttribute(i);
                                    if (reader.NamespaceURI.Equals(Keywords.XSD_XMLNS_NS))
                                        topNode.SetAttribute(reader.Name, reader.GetAttribute(i));
                                    else {
                                        XmlAttribute attr = topNode.SetAttributeNode(reader.LocalName, reader.NamespaceURI);
                                        attr.Prefix = reader.Prefix;
                                        attr.Value = reader.GetAttribute(i);
                                    }
                                }
                            }
                            reader.Read();
                        }

                        while (MoveToElement(reader, iCurrentDepth)) {

                            if (reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI == Keywords.XDRNS) {
                                // load XDR schema
                                if (!fSchemaFound && !fDataFound && (mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema) &&
                                    (mode != XmlReadMode.InferTypedSchema))
                                {
                                    ReadXDRSchema(reader);
                                    fSchemaFound = true;
                                    fIsXdr = true;
                                }
                                else {
                                    reader.Skip();
                                }
                                continue;
                            }

                            if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI == Keywords.XSDNS) {
                                // load XSD schema and exit
                                if ((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema) &&
                                    (mode != XmlReadMode.InferTypedSchema))
                                {
                                    ReadXSDSchema(reader, denyResolving);
                                    fSchemaFound = true;
                                }
                                else {
                                    reader.Skip();
                                }
                                continue;
                            }

                            if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                                if ((mode == XmlReadMode.DiffGram) || (mode == XmlReadMode.IgnoreSchema)) {
                                    this.ReadXmlDiffgram(reader);
                                    ret = XmlReadMode.DiffGram;
                                }
                                else {
                                    reader.Skip();
                                }
                                continue;
                            }

                            if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))
                                throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);

                            if (mode == XmlReadMode.DiffGram) {
                                reader.Skip();
                                continue; // we do not read data in diffgram mode
                            }

                            // if we are here we found some data
                            fDataFound = true;

                            if (mode == XmlReadMode.InferSchema || mode == XmlReadMode.InferTypedSchema) { //save the node in DOM until the end;
                                XmlNode node = xdoc.ReadNode(reader);
                                topNode.AppendChild(node);
                            }
                            else {
                                if (xmlload == null)
                                    xmlload = new XmlDataLoader(this, fIsXdr, topNode, mode == XmlReadMode.IgnoreSchema);
                                xmlload.LoadData(reader);
                            }
                        } //end of the while

                        // read the closing tag of the current element
                        ReadEndElement(reader);

                        // now top node contains the data part
                        xdoc.AppendChild(topNode);
                        if (xmlload == null)
                            xmlload = new XmlDataLoader(this, fIsXdr, mode == XmlReadMode.IgnoreSchema);

                        if (mode == XmlReadMode.DiffGram) {
                            // we already got the diffs through XmlReader interface
                            return ret;
                        }

                        // Load Data
                        if (mode == XmlReadMode.InferSchema || mode == XmlReadMode.InferTypedSchema) {
                            InferSchema(xdoc, null, mode);
                            ret = XmlReadMode.InferSchema;
                            xmlload.FromInference = true;
                            //                }
                            try {
                                xmlload.LoadData(xdoc);
                            }
                            finally {
                                xmlload.FromInference = false;
                            }
                        }
                    }

                    return ret;
                }
                finally {
                    // Dev11 904428: prepare and cleanup rowDiffId hashtable
                    rowDiffIdUsage.Cleanup();
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }


        /// <devdoc>
        /// </devdoc>
        public XmlReadMode ReadXml(Stream stream, XmlReadMode mode)
        {
            if (stream == null)
                return XmlReadMode.Auto;

            XmlTextReader reader = (mode == XmlReadMode.Fragment) ? new XmlTextReader(stream, XmlNodeType.Element, null) : new XmlTextReader(stream);
            return ReadXml(reader, mode, false);
        }

        /// <devdoc>
        /// </devdoc>
        public XmlReadMode ReadXml(TextReader reader, XmlReadMode mode)
        {
            if (reader == null)
                return XmlReadMode.Auto;

            XmlTextReader xmlreader = (mode == XmlReadMode.Fragment) ? new XmlTextReader(reader.ReadToEnd(), XmlNodeType.Element, null) : new XmlTextReader(reader);
            return ReadXml(xmlreader, mode, false);
        }

        /// <devdoc>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public XmlReadMode ReadXml(string fileName, XmlReadMode mode)
        {
            XmlTextReader xr = null;
            if (mode == XmlReadMode.Fragment) {
                FileStream stream = new FileStream(fileName, FileMode.Open);
                xr = new XmlTextReader(stream, XmlNodeType.Element, null);
            }
            else
                xr = new XmlTextReader(fileName);
            try {
                return ReadXml(xr, mode, false);
            }
            finally {
                xr.Close();
            }
        }


        /// <devdoc>
        ///    Writes schema and data for the DataSet.
        /// </devdoc>
        public void WriteXml(Stream stream)
        {
            WriteXml(stream, XmlWriteMode.IgnoreSchema);
        }

        /// <devdoc>
        /// </devdoc>
        public void WriteXml(TextWriter writer)
        {
            WriteXml(writer, XmlWriteMode.IgnoreSchema);
        }

        /// <devdoc>
        /// </devdoc>
        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, XmlWriteMode.IgnoreSchema);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXml(String fileName)
        {
            WriteXml(fileName, XmlWriteMode.IgnoreSchema);
        }

        /// <devdoc>
        ///    Writes schema and data for the DataSet.
        /// </devdoc>
        public void WriteXml(Stream stream, XmlWriteMode mode)
        {
            if (stream != null) {
                XmlTextWriter w = new XmlTextWriter(stream, null);
                w.Formatting = Formatting.Indented;

                WriteXml(w, mode);
            }
        }

        /// <devdoc>
        /// </devdoc>
        public void WriteXml(TextWriter writer, XmlWriteMode mode)
        {
            if (writer != null) {
                XmlTextWriter w = new XmlTextWriter(writer);
                w.Formatting = Formatting.Indented;

                WriteXml(w, mode);
            }
        }

        /// <devdoc>
        /// </devdoc>
        public void WriteXml(XmlWriter writer, XmlWriteMode mode)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.WriteXml|API> %d#, mode=%d{ds.XmlWriteMode}\n", ObjectID, (int)mode);
            try {
                // Generate SchemaTree and write it out
                if (writer != null) {

                    if (mode == XmlWriteMode.DiffGram) {
                        // Create and save the updates
                        //                    new UpdateTreeGen(UpdateTreeGen.UPDATE, (DataRowState)(-1), this).Save(writer, null);
                        new NewDiffgramGen(this).Save(writer);
                    }
                    else {
                        // Create and save xml data
                        new XmlDataTreeWriter(this).Save(writer, mode == XmlWriteMode.WriteSchema);
                    }
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXml(String fileName, XmlWriteMode mode)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.WriteXml|API> %d#, fileName='%ls', mode=%d{ds.XmlWriteMode}\n", ObjectID, fileName, (int)mode);
            XmlTextWriter xw = new XmlTextWriter(fileName, null);
            try {
                xw.Formatting = Formatting.Indented;
                xw.WriteStartDocument(true);
                if (xw != null) {
                    // Create and save the updates
                    if (mode == XmlWriteMode.DiffGram) {
                        new NewDiffgramGen(this).Save(xw);
                    }
                    else {
                        // Create and save xml data
                        new XmlDataTreeWriter(this).Save(xw, mode == XmlWriteMode.WriteSchema);
                    }
                }
                xw.WriteEndDocument();
            }
            finally {
                xw.Close();
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the collection of parent relations which belong to a
        ///       specified table.
        ///    </para>
        /// </devdoc>
        internal DataRelationCollection GetParentRelations(DataTable table)
        {
            return table.ParentRelations;
        }

        /// <devdoc>
        ///    <para>
        ///       Merges this <see cref='System.Data.DataSet'/> into a specified <see cref='System.Data.DataSet'/>.
        ///    </para>
        /// </devdoc>
        public void Merge(DataSet dataSet)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Merge|API> %d#, dataSet=%d\n", ObjectID, (dataSet != null) ? dataSet.ObjectID : 0);
            try {
                Merge(dataSet, false, MissingSchemaAction.Add);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Merges this <see cref='System.Data.DataSet'/> into a specified <see cref='System.Data.DataSet'/> preserving changes according to
        ///       the specified argument.
        ///    </para>
        /// </devdoc>
        public void Merge(DataSet dataSet, bool preserveChanges)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Merge|API> %d#, dataSet=%d, preserveChanges=%d{bool}\n", ObjectID, (dataSet != null) ? dataSet.ObjectID : 0, preserveChanges);
            try {
                Merge(dataSet, preserveChanges, MissingSchemaAction.Add);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Merges this <see cref='System.Data.DataSet'/> into a specified <see cref='System.Data.DataSet'/> preserving changes according to
        ///       the specified argument, and handling an incompatible schema according to the
        ///       specified argument.
        ///    </para>
        /// </devdoc>
        public void Merge(DataSet dataSet, bool preserveChanges, MissingSchemaAction missingSchemaAction) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Merge|API> %d#, dataSet=%d, preserveChanges=%d{bool}, missingSchemaAction=%d{ds.MissingSchemaAction}\n", ObjectID, (dataSet != null) ? dataSet.ObjectID : 0, preserveChanges, (int)missingSchemaAction);
            try {

                // Argument checks
                if (dataSet == null)
                    throw ExceptionBuilder.ArgumentNull("dataSet");

                switch (missingSchemaAction) { // @perfnote: Enum.IsDefined
                    case MissingSchemaAction.Add:
                    case MissingSchemaAction.Ignore:
                    case MissingSchemaAction.Error:
                    case MissingSchemaAction.AddWithKey:
                        Merger merger = new Merger(this, preserveChanges, missingSchemaAction);
                        merger.MergeDataSet(dataSet);
                        break;
                    default:
                        throw Common.ADP.InvalidMissingSchemaAction(missingSchemaAction);
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Merges this <see cref='System.Data.DataTable'/> into a specified <see cref='System.Data.DataTable'/>.
        ///    </para>
        /// </devdoc>
        public void Merge(DataTable table)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Merge|API> %d#, table=%d\n", ObjectID, (table != null) ? table.ObjectID : 0);
            try {
                Merge(table, false, MissingSchemaAction.Add);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Merges this <see cref='System.Data.DataTable'/> into a specified <see cref='System.Data.DataTable'/>. with a value to preserve changes
        ///       made to the target, and a value to deal with missing schemas.
        ///    </para>
        /// </devdoc>
        public void Merge(DataTable table, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Merge|API> %d#, table=%d, preserveChanges=%d{bool}, missingSchemaAction=%d{ds.MissingSchemaAction}\n", ObjectID, (table != null) ? table.ObjectID : 0, preserveChanges, (int)missingSchemaAction);
            try {
                // Argument checks
                if (table == null)
                    throw ExceptionBuilder.ArgumentNull("table");

                switch (missingSchemaAction) { // @perfnote: Enum.IsDefined
                    case MissingSchemaAction.Add:
                    case MissingSchemaAction.Ignore:
                    case MissingSchemaAction.Error:
                    case MissingSchemaAction.AddWithKey:
                        Merger merger = new Merger(this, preserveChanges, missingSchemaAction);
                        merger.MergeTable(table);
                        break;
                    default:
                        throw Common.ADP.InvalidMissingSchemaAction(missingSchemaAction);
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Merge(DataRow[] rows)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Merge|API> %d#, rows\n", ObjectID);
            try {
                Merge(rows, false, MissingSchemaAction.Add);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Merge(DataRow[] rows, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Merge|API> %d#, preserveChanges=%d{bool}, missingSchemaAction=%d{ds.MissingSchemaAction}\n", ObjectID, preserveChanges, (int)missingSchemaAction);
            try {
                // Argument checks
                if (rows == null)
                    throw ExceptionBuilder.ArgumentNull("rows");

                switch (missingSchemaAction) { // @perfnote: Enum.IsDefined
                    case MissingSchemaAction.Add:
                    case MissingSchemaAction.Ignore:
                    case MissingSchemaAction.Error:
                    case MissingSchemaAction.AddWithKey:
                        Merger merger = new Merger(this, preserveChanges, missingSchemaAction);
                        merger.MergeRows(rows);
                        break;
                    default:
                        throw Common.ADP.InvalidMissingSchemaAction(missingSchemaAction);
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        protected virtual void OnPropertyChanging(PropertyChangedEventArgs pcevent)
        {
            if (onPropertyChangingDelegate != null)
                onPropertyChangingDelegate(this, pcevent);
        }

        /// <devdoc>
        ///     Inheriting classes should override this method to handle this event.
        ///     Call base.OnMergeFailed to send this event to any registered event
        ///     listeners.
        /// </devdoc>
        internal void OnMergeFailed(MergeFailedEventArgs mfevent)
        {
            if (onMergeFailed != null)
                onMergeFailed(this, mfevent);
            else
                throw ExceptionBuilder.MergeFailed(mfevent.Conflict);
        }

        internal void RaiseMergeFailed(DataTable table, string conflict, MissingSchemaAction missingSchemaAction)
        {
            if (MissingSchemaAction.Error == missingSchemaAction)
                throw ExceptionBuilder.MergeFailed(conflict);

            MergeFailedEventArgs mfevent = new MergeFailedEventArgs(table, conflict);
            OnMergeFailed(mfevent);
            return;
        }

        internal void OnDataRowCreated(DataRow row) {
            if (onDataRowCreated != null)
                onDataRowCreated(this, row);
        }

        internal void OnClearFunctionCalled(DataTable table) {
            if (onClearFunctionCalled != null)
                onClearFunctionCalled(this, table);
        }

        private void OnInitialized() {
            if (onInitialized != null) {
                onInitialized(this, EventArgs.Empty);
            }
        }

        /// <devdoc>
        /// This method should be overriden by subclasses to restrict tables being removed.
        /// </devdoc>
        protected internal virtual void OnRemoveTable(DataTable table) {
        }

        internal void OnRemovedTable(DataTable table) {
            DataViewManager viewManager = defaultViewManager;
            if (null != viewManager) {
                viewManager.DataViewSettings.Remove(table);
            }
        }

        /// <devdoc>
        /// This method should be overriden by subclasses to restrict tables being removed.
        /// </devdoc>
        protected virtual void OnRemoveRelation(DataRelation relation)
        {
        }

        // 
        internal void OnRemoveRelationHack(DataRelation relation)
        {
            OnRemoveRelation(relation);
        }


        protected internal void RaisePropertyChanging(string name)
        {
            OnPropertyChanging(new PropertyChangedEventArgs(name));
        }

        internal DataTable[] TopLevelTables()
        {
            return TopLevelTables(false);
        }

        internal DataTable[] TopLevelTables(bool forSchema)
        {
            // first let's figure out if we can represent the given dataSet as a tree using
            // the fact that all connected undirected graphs with n-1 edges are trees.
            List<DataTable> topTables = new List<DataTable>();

            if (forSchema) {
                // prepend the tables that are nested more than once
                for (int i = 0; i < Tables.Count; i++)
                {
                    DataTable table = Tables[i];
                    if (table.NestedParentsCount > 1 || table.SelfNested)
                        topTables.Add(table);
                }
            }
            for (int i = 0; i < Tables.Count; i++)
            {
                DataTable table = Tables[i];
                if (table.NestedParentsCount == 0 && !topTables.Contains(table))
                    topTables.Add(table);
            }
            if (topTables.Count == 0)
                return zeroTables;
            return topTables.ToArray();
        }

        /// <devdoc>
        /// This method rolls back all the changes to have been made to this DataSet since
        /// it was loaded or the last time AcceptChanges was called.
        /// Any rows still in edit-mode cancel their edits.  New rows get removed.  Modified and
        /// Deleted rows return back to their original state.
        /// </devdoc>
        public virtual void RejectChanges()
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.RejectChanges|API> %d#\n", ObjectID);
            try {
                bool fEnforce = EnforceConstraints;
                EnforceConstraints = false;
                for (int i = 0; i < Tables.Count; i++)
                    Tables[i].RejectChanges();
                EnforceConstraints = fEnforce;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    Resets the dataSet back to it's original state.  Subclasses should override
        ///    to restore back to it's original state.
        ///    

        public virtual void Reset()
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Reset|API> %d#\n", ObjectID);
            try {
                for (int i = 0; i < Tables.Count; i++) {
                    ConstraintCollection cons = Tables[i].Constraints;
                    for (int j = 0; j < cons.Count; ) {
                        if (cons[j] is ForeignKeyConstraint) {
                            cons.Remove(cons[j]);
                        }
                        else
                            j++;
                    }
                }
                // SQLBU 502734: because of SQLBU 501916, dependent tables need to be notified when a table is cleared
                // if relations are removed first, then exceptions happen
                Clear();
                Relations.Clear();
                Tables.Clear();
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal bool ValidateCaseConstraint() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.ValidateCaseConstraint|INFO> %d#\n", ObjectID);
            try {
                DataRelation relation = null;
                for (int i = 0; i < Relations.Count; i++) {
                    relation = Relations[i];
                    if (relation.ChildTable.CaseSensitive != relation.ParentTable.CaseSensitive)
                        return false;
                }

                ForeignKeyConstraint constraint = null;
                ConstraintCollection constraints = null;
                for (int i = 0; i < Tables.Count; i++) {
                    constraints = Tables[i].Constraints;
                    for (int j = 0; j < constraints.Count; j++) {
                        if (constraints[j] is ForeignKeyConstraint) {
                            constraint = (ForeignKeyConstraint)constraints[j];
                            if (constraint.Table.CaseSensitive != constraint.RelatedTable.CaseSensitive)
                                return false;
                        }
                    }
                }
                return true;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal bool ValidateLocaleConstraint() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.ValidateLocaleConstraint|INFO> %d#\n", ObjectID);
            try {
                DataRelation relation = null;
                for (int i = 0; i < Relations.Count; i++) {
                    relation = Relations[i];
                    if (relation.ChildTable.Locale.LCID != relation.ParentTable.Locale.LCID)
                        return false;
                }

                ForeignKeyConstraint constraint = null;
                ConstraintCollection constraints = null;
                for (int i = 0; i < Tables.Count; i++) {
                    constraints = Tables[i].Constraints;
                    for (int j = 0; j < constraints.Count; j++) {
                        if (constraints[j] is ForeignKeyConstraint) {
                            constraint = (ForeignKeyConstraint)constraints[j];
                            if (constraint.Table.Locale.LCID != constraint.RelatedTable.Locale.LCID)
                                return false;
                        }
                    }
                }
                return true;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        // [....]: may be better to rewrite this as nonrecursive?
        internal DataTable FindTable(DataTable baseTable, PropertyDescriptor[] props, int propStart) {
            if (props.Length < propStart + 1)
                return baseTable;

            PropertyDescriptor currentProp = props[propStart];

            if (baseTable == null) {
                // the accessor is the table name.  if we don't find it, return null.
                if (currentProp is DataTablePropertyDescriptor) {
                    return FindTable(((DataTablePropertyDescriptor)currentProp).Table, props, propStart + 1);
                }
                return null;
            }

            if (currentProp is DataRelationPropertyDescriptor) {
                return FindTable(((DataRelationPropertyDescriptor)currentProp).Relation.ChildTable, props, propStart + 1);
            }

            return null;
        }

        protected virtual void ReadXmlSerializable(XmlReader reader) {
            //WebData 96421 and 104709, this is DataSet side fix for the thrown exception
            // <DataSet xsi:nil="true"> does not mean DataSet is null,but it does not have any child
            // so  dont do anything, ignore the attributes and just return empty DataSet;
            this.UseDataSetSchemaOnly = false;
            this.UdtIsWrapped = false;

            if (reader.HasAttributes) {
                const string xsinill = Keywords.XSI + ":" + Keywords.XSI_NIL;
                if (reader.MoveToAttribute(xsinill)) {
                    string nilAttrib = reader.GetAttribute(xsinill);
                    if (string.Compare(nilAttrib, "true", StringComparison.Ordinal) == 0) {// case sensitive true comparison
                        MoveToElement(reader, 1);
                        return;
                    }
                }

                const string useDataSetSchemaOnlyString = Keywords.MSD + ":" + Keywords.USEDATASETSCHEMAONLY;
                if (reader.MoveToAttribute(useDataSetSchemaOnlyString)) {
                    string _useDataSetSchemaOnly = reader.GetAttribute(useDataSetSchemaOnlyString);
                    if (string.Equals(_useDataSetSchemaOnly, "true", StringComparison.Ordinal) ||
                        string.Equals(_useDataSetSchemaOnly, "1", StringComparison.Ordinal))
                    {
                        this.UseDataSetSchemaOnly = true;
                    }
                    else if (!string.Equals(_useDataSetSchemaOnly, "false", StringComparison.Ordinal) &&
                             !string.Equals(_useDataSetSchemaOnly, "0", StringComparison.Ordinal))
                    {
                        throw ExceptionBuilder.InvalidAttributeValue(Keywords.USEDATASETSCHEMAONLY, _useDataSetSchemaOnly);
                    }
                }

                const string udtIsWrappedString = Keywords.MSD + ":" + Keywords.UDTCOLUMNVALUEWRAPPED;
                if (reader.MoveToAttribute(udtIsWrappedString)) {
                    string _udtIsWrappedString = reader.GetAttribute(udtIsWrappedString);
                    if (string.Equals(_udtIsWrappedString, "true", StringComparison.Ordinal) ||
                        string.Equals(_udtIsWrappedString, "1", StringComparison.Ordinal))
                    {
                        this.UdtIsWrapped = true;

                    }
                    else if (!string.Equals(_udtIsWrappedString, "false", StringComparison.Ordinal) &&
                             !string.Equals(_udtIsWrappedString, "0", StringComparison.Ordinal))
                    {
                        throw ExceptionBuilder.InvalidAttributeValue(Keywords.UDTCOLUMNVALUEWRAPPED, _udtIsWrappedString);
                    }
                }
            }
            ReadXml(reader, XmlReadMode.DiffGram, true);
        }

        protected virtual System.Xml.Schema.XmlSchema GetSchemaSerializable() {
            return null;
        }

        /************************************************************************
        To publish
        int_tools\webserviceadmin.exe -install Application Application.dll 

        To test
        http://localhost/application/service.asmx?wsdl
        or
        wsdl.exe /namespace:UserNamespace http://localhost/Application/Service.asmx?wsdl /out:Service.cs

        The V1.0 & V1.1 WSDL for Untyped DataSet being returned as a result (no parameters)
        <s:element name="anyUserSpecifiedMethodName">
            <!--  This is where parameters go -->
            <s:complexType /> 
        </s:element>
        <s:element name="anyUserSpecifiedMethodName"+"Response">
            <s:complexType>
                <s:sequence>
                    <s:element minOccurs="0" maxOccurs="1" name="anyUserSpecifiedMethodName"+"Result">
                        <s:complexType>
                            <s:sequence>
                                <s:element ref="s:schema" /> 
                                <s:any /> 
                            </s:sequence>
                        </s:complexType>
                    </s:element>
                </s:sequence>
            </s:complexType>
        </s:element>

        The V1.0 & V1.1 WSDL for Untyped DataSet as parameter, string for the result
        <s:element name="anyUserSpecifiedMethodName">
            <!--  This is where parameters go -->
            <s:complexType>
                <s:sequence>
                    <s:element minOccurs="0" maxOccurs="1" name="set">
                        <s:complexType>
                            <s:sequence>
                                <s:element ref="s:schema" /> 
                                <s:any /> 
                            </s:sequence>
                        </s:complexType>
                    </s:element>
                </s:sequence>
            </s:complexType>
        </s:element>
        <s:element name="anyUserSpecifiedMethodName"+"Response">
            <s:complexType>
                <s:sequence>
                    <s:element minOccurs="0" maxOccurs="1" name="anyUserSpecifiedMethodName"+"Result" type="s:string" /> 
                </s:sequence>
            </s:complexType>
        </s:element>
  
        The V2.0 WSDL for Untyped DataSet being returned as a result (no parameters)
        <s:element name="anyUserSpecifiedMethodName">
            <!--  This is where parameters go -->
            <s:complexType /> 
        </s:element>
        <s:element name="anyUserSpecifiedMethodName"+"Response">
            <s:complexType>
                <s:sequence>
                    <s:element minOccurs="0" maxOccurs="1" name="anyUserSpecifiedMethodName"+"Result">
                        <s:complexType>
                            <s:sequence maxOccurs="unbounded">
                                <s:any minOccurs="0" namespace="http://www.w3.org/2001/XMLSchema" processContents="lax" /> 
                                <s:any minOccurs="0" namespace="urn:schemas-microsoft-com:xml-diffgram-v1" processContents="lax" /> 
                            </s:sequence>
                        </s:complexType>
                    </s:element>
                </s:sequence>
            </s:complexType>
        </s:element>
        
        The V2.0 WSDL for Untyped DataSet as a parameter, string for the result
        <s:element name="anyUserSpecifiedMethodName">
            <!--  This is where parameters go -->
            <s:complexType>
                <s:sequence>
                    <s:element minOccurs="0" maxOccurs="1" name="anyUserSpecifiedParameterName">
                        <s:complexType>
                            <s:sequence maxOccurs="unbounded">
                                <s:any minOccurs="0" namespace="http://www.w3.org/2001/XMLSchema" processContents="lax" /> 
                                <s:any minOccurs="0" namespace="urn:schemas-microsoft-com:xml-diffgram-v1" processContents="lax" /> 
                            </s:sequence>
                        </s:complexType>
                    </s:element>
                </s:sequence>
            </s:complexType>
        </s:element>
        <s:element name="anyUserSpecifiedMethodName"+"Response">
            <s:complexType>
                <s:sequence>
                    <s:element minOccurs="0" maxOccurs="1" name="anyUserSpecifiedMethodName"+"Result" type="s:string" /> 
                </s:sequence>
            </s:complexType>
        </s:element>
  
        The V1.0, V1.1 & V2.0 WSDL for Typed DataSet
        <s:import schemaLocation="http://localhost/application/service.asmx?schema=typedDataSetName" namespace="typedDataSetName" /> 
        <s:element name="anyUserSpecifiedMethodName">
            <!--  This is where parameters go -->
            <s:complexType /> 
        </s:element>
        <s:element name="anyUserSpecifiedMethodName"+"Response">
            <s:complexType>
                <s:sequence>
                    <s:element minOccurs="0" maxOccurs="1" name="anyUserSpecifiedMethodName"+"Result">
                        <s:complexType>
                            <s:sequence>
                                <s:any namespace="typedDataSetName" /> 
                            </s:sequence>
                        </s:complexType>
                    </s:element>
                </s:sequence>
            </s:complexType>
        </s:element>  
        ************************************************************************/
        public static XmlSchemaComplexType GetDataSetSchema(XmlSchemaSet schemaSet) {
            // For performance resons we are exploiting the fact that config files content is constant 
            // for a given appdomain so we can safely cache the prepared schema complex type and reuse it
            if (schemaTypeForWSDL == null) { // to change the config file, appdomain needs to restart; so it seems safe to cache the schema
                XmlSchemaComplexType tempWSDL = new XmlSchemaComplexType();
                XmlSchemaSequence sequence = new XmlSchemaSequence();

                if (PublishLegacyWSDL())
                {   // Default is Version 1.0
                    XmlSchemaElement elem = new XmlSchemaElement();
                    elem.RefName = new XmlQualifiedName(Keywords.XSD_SCHEMA, Keywords.XSDNS);
                    sequence.Items.Add(elem);
                    XmlSchemaAny any = new XmlSchemaAny();
                    sequence.Items.Add(any);
                }
                else
                {   // this means Version == V2.0 or newer
                    XmlSchemaAny any = new XmlSchemaAny();
                    any.Namespace = XmlSchema.Namespace;
                    any.MinOccurs = 0;
                    any.ProcessContents = XmlSchemaContentProcessing.Lax;
                    sequence.Items.Add(any);

                    any = new XmlSchemaAny();
                    any.Namespace = Keywords.DFFNS;
                    any.MinOccurs = 0; // when recognizing WSDL - MinOccurs="0" denotes DataSet, a MinOccurs="1" for DataTable
                    any.ProcessContents = XmlSchemaContentProcessing.Lax;
                    sequence.Items.Add(any);
                    sequence.MaxOccurs = Decimal.MaxValue;
                }
                tempWSDL.Particle = sequence;

                schemaTypeForWSDL = tempWSDL;
            }
            return schemaTypeForWSDL;
        }

        /********************************************
        <configuration>
            <configSections>
                <section name="system.data.dataset" type="System.Configuration.NameValueFileSectionHandler, System, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%" restartOnExternalChanges="false" />
            </configSections>
            <system.data.dataset>
                <!--  if WSDL_VERSION is missing it will default to 1.0
                    <add key="WSDL_VERSION" value="1.0"/>
                -->
                <add key="WSDL_VERSION" value="2.0"/>
            </system.data.dataset>
        </configuration>
        *******************************************/
        private static bool PublishLegacyWSDL()
        {
            Single version = 1.0f; // Default is Version 1.0
            NameValueCollection settings = (NameValueCollection)PrivilegedConfigurationManager.GetSection(Keywords.WS_DATASETFULLQNAME);
            if (settings != null)
            {
                string[] values = settings.GetValues(Keywords.WS_VERSION);
                if ((null != values) && (0 < values.Length) && (null != values[0]))
                {   // will throw FormatException if not a valid number
                    version = Single.Parse(values[0], CultureInfo.InvariantCulture);
                }
            }
            return (version < 2.0f); // if config does not exist, Default is Version 1.0
        }

        XmlSchema IXmlSerializable.GetSchema() {
            if (GetType() == typeof(DataSet)) {
                return null;
            }
            MemoryStream stream = new MemoryStream();
            // WriteXmlSchema(new XmlTextWriter(stream, null));
            XmlWriter writer = new XmlTextWriter(stream, null);
            if (writer != null) {
                (new XmlTreeGen(SchemaFormat.WebService)).Save(this, writer);
            }
            stream.Position = 0;
            return XmlSchema.Read(new XmlTextReader(stream), null);
            //            return GetSchemaSerializable();
        }

        void IXmlSerializable.ReadXml(XmlReader reader) {
            bool fNormalization = true;
            XmlTextReader xmlTextReader = null;
            IXmlTextParser xmlTextParser = reader as IXmlTextParser;
            if (xmlTextParser != null) {
                fNormalization = xmlTextParser.Normalized;
                xmlTextParser.Normalized = false;
            }
            else {
                xmlTextReader = reader as XmlTextReader;
                if (xmlTextReader != null) {
                    fNormalization = xmlTextReader.Normalization;
                    xmlTextReader.Normalization = false;
                }
            }

            ReadXmlSerializable(reader);

            if (xmlTextParser != null)
                xmlTextParser.Normalized = fNormalization;
            else if (xmlTextReader != null)
                xmlTextReader.Normalization = fNormalization;
        }

        void IXmlSerializable.WriteXml(XmlWriter writer) {
            this.WriteXmlSchema(writer, SchemaFormat.WebService, (Converter<Type, string>)null);
            this.WriteXml(writer, XmlWriteMode.DiffGram);
        }

        public virtual void Load(IDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler, params DataTable[] tables) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.Load|API> reader, loadOption=%d{ds.LoadOption}", (int)loadOption);
            try {
                foreach (DataTable dt in tables) {
                    Common.ADP.CheckArgumentNull(dt, "tables");
                    if (dt.DataSet != this) {
                        throw ExceptionBuilder.TableNotInTheDataSet(dt.TableName);
                    }
                }
                Common.LoadAdapter adapter = new Common.LoadAdapter();
                adapter.FillLoadOption = loadOption;
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                if (null != errorHandler) {
                    adapter.FillError += errorHandler;
                }
                adapter.FillFromReader(tables, reader, 0, 0);

                if (!reader.IsClosed && !reader.NextResult()) { // 
                    reader.Close();
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public void Load(IDataReader reader, LoadOption loadOption, params DataTable[] tables) {
            Load(reader, loadOption, null, tables);
        }

        public void Load(IDataReader reader, LoadOption loadOption, params string[] tables) {
            Common.ADP.CheckArgumentNull(tables, "tables");
            DataTable[] dataTables = new DataTable[tables.Length];
            for (int i = 0; i < tables.Length; i++) {
                DataTable tempDT = this.Tables[tables[i]];
                if (null == tempDT) {
                    tempDT = new DataTable(tables[i]);
                    // fxcop: new DataTable should inherit the CaseSensitive, Locale, Namespace from DataSet
                    Tables.Add(tempDT);
                }
                dataTables[i] = tempDT;
            }
            Load(reader, loadOption, null, dataTables);
        }

        public DataTableReader CreateDataReader() {
            if (Tables.Count == 0)
                throw ExceptionBuilder.CannotCreateDataReaderOnEmptyDataSet();

            DataTable[] dataTables = new DataTable[Tables.Count];
            for (int i = 0; i < Tables.Count; i++) {
                dataTables[i] = Tables[i];
            }
            return CreateDataReader(dataTables);
        }

        public DataTableReader CreateDataReader(params DataTable[] dataTables) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataSet.GetDataReader|API> %d#\n", ObjectID);
            try {
                if (dataTables.Length == 0)
                    throw ExceptionBuilder.DataTableReaderArgumentIsEmpty();

                for (int i = 0; i < dataTables.Length; i++) {
                    if (dataTables[i] == null)
                        throw ExceptionBuilder.ArgumentContainsNullValue();
                }

                return new DataTableReader(dataTables);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal string MainTableName {
            get {
                return mainTableName;
            }
            set {
                this.mainTableName = value;
            }
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }

    }

 public class DataSetSchemaImporterExtension : SchemaImporterExtension {
        // DataSetSchemaImporterExtension is used for WebServices, it is used to recognize the schema of DataSet within wsdl
        // If a non 2.0 enabled DataSetSchemaImporterExtension, wsdl will generate a classes that you can't cast to dataset / datatable

        Hashtable importedTypes = new Hashtable();

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string ImportSchemaType(string name, string schemaNamespace, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider) {
            IList values = schemas.GetSchemas(schemaNamespace);
            if (values.Count != 1) {
                return null;
            }
            XmlSchema schema = values[0] as XmlSchema;
            if (schema == null)
                return null;
            XmlSchemaType type = (XmlSchemaType)schema.SchemaTypes[new XmlQualifiedName(name, schemaNamespace)];
            return ImportSchemaType(type, context, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string ImportSchemaType(XmlSchemaType type, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider) {
            if (type == null) {
                return null;
            }
            if (importedTypes[type] != null) {
                mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
                compileUnit.ReferencedAssemblies.Add("System.Data.dll");
                return (string)importedTypes[type];
            }
            if (!(context is XmlSchemaElement))
                return null;

            XmlSchemaElement e = (XmlSchemaElement)context;

            // recognizing the following is important, but not as part of SQLPT 120015394: Support WSI Compliant WSDL for DataSet
            // <xs:element name="NewDataSet" msdata:IsDataSet="true">
            // see also SQLBU 338644, 410965, 423446
            //if (IsDataSet(e))
            //{
            //    return GenerateTypedDataSet(e, schemas, mainNamespace, compileUnit.ReferencedAssemblies, codeProvider);
            //}

            if (type is XmlSchemaComplexType) {
                XmlSchemaComplexType ct = (XmlSchemaComplexType)type;

                if (ct.Particle is XmlSchemaSequence)
                {
                    XmlSchemaObjectCollection items = ((XmlSchemaSequence)ct.Particle).Items;
                    if ((2 == items.Count) && (items[0] is XmlSchemaAny) && (items[1] is XmlSchemaAny))
                    {
                        XmlSchemaAny any0 = (XmlSchemaAny)items[0];
                        XmlSchemaAny any1 = (XmlSchemaAny)items[1];
                        if ((any0.Namespace == XmlSchema.Namespace) && (any1.Namespace == "urn:schemas-microsoft-com:xml-diffgram-v1"))
                        {   // new diffgramm format
                            string ns = null;
                            foreach (XmlSchemaAttribute a in ct.Attributes)
                            {
                                if (a.Name == "namespace")
                                {
                                    ns = a.FixedValue.Trim();
                                    break;
                                }
                            }
                            bool isDataSet = false;

                            // check for DataSet or DataTable
                            if (((XmlSchemaSequence)ct.Particle).MaxOccurs == Decimal.MaxValue)
                            {
                                isDataSet = true;
                            }
                            else if (any0.MaxOccurs == Decimal.MaxValue)
                            {
                                isDataSet = false;
                            }
                            else
                            {
                                return null;
                            }

                            if (ns == null)
                            {   //Un-Typed DataSet / DataTable
                                string typeName = isDataSet ? typeof(DataSet).FullName : typeof(DataTable).FullName;
                                importedTypes.Add(type, typeName);
                                mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
                                compileUnit.ReferencedAssemblies.Add("System.Data.dll");
                                return typeName;
                            }
                            else
                            {   // Typed DataSet / DataTable
                                foreach (XmlSchema schema in schemas.GetSchemas(ns))
                                {
                                    if ((schema != null) && (schema.Id != null))
                                    {
                                        XmlSchemaElement ds = FindDataSetElement(schema); // implement  FindDataTableElement(schema)
                                        if (ds != null)
                                        {
                                            return ImportSchemaType(ds.SchemaType, ds, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
                                        }
                                        // else return null
                                    }
                                }
                                return null;
                            }
                        }
                    }
                }
                if (ct.Particle is XmlSchemaSequence || ct.Particle is XmlSchemaAll) {
                    XmlSchemaObjectCollection items = ((XmlSchemaGroupBase)ct.Particle).Items;
                    if (items.Count == 2) {
                        if (!(items[0] is XmlSchemaElement && items[1] is XmlSchemaAny)) return null;
                        XmlSchemaElement schema = (XmlSchemaElement)items[0];
                        if (!(schema.RefName.Name == "schema" && schema.RefName.Namespace == XmlSchema.Namespace)) return null;
                        string typeName = typeof(DataSet).FullName;
                        importedTypes.Add(type, typeName);
                        mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
                        compileUnit.ReferencedAssemblies.Add("System.Data.dll");
                        return typeName;
                    }
                    else if (1 == items.Count)
                    {
                        XmlSchemaAny any = items[0] as XmlSchemaAny;
                        if ((null != any) &&
                            (null != any.Namespace) &&
                            (any.Namespace.IndexOfAny(new char[] { '#', ' ' }) < 0)) // special syntax (##any, ##other, ...) or more than one Uri present
                        {
                            foreach (XmlSchema schema in schemas.GetSchemas(any.Namespace))
                            {
                                if ((null != schema) &&
                                    (null != schema.Id))
                                {
                                    XmlSchemaElement ds = FindDataSetElement(schema);
                                    if (ds != null)
                                    {
                                        return ImportSchemaType(ds.SchemaType, ds, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
                                    }
                                    // else return null
                                }
                                // else return null
                            }
                        }
                        // else return null
                    }
                }
            }
            return null;
        }


        internal XmlSchemaElement FindDataSetElement(XmlSchema schema) {
            foreach (XmlSchemaObject item in schema.Items) {
                if (item is XmlSchemaElement && IsDataSet((XmlSchemaElement)item)) {
                    return (XmlSchemaElement)item;
                }
            }
            return null;
        }

        internal string GenerateTypedDataSet(XmlSchemaElement element, XmlSchemas schemas, CodeNamespace codeNamespace, StringCollection references, CodeDomProvider codeProvider) {
            if (element == null)
                return null;

            if (importedTypes[element.SchemaType] != null)
                return (string)importedTypes[element.SchemaType];

            IList values = schemas.GetSchemas(element.QualifiedName.Namespace);
            if (values.Count != 1) {
                return null;
            }
            XmlSchema schema = values[0] as XmlSchema;
            if (schema == null)
                return null;

            DataSet ds = new DataSet();

            // 
            using (MemoryStream stream = new MemoryStream()) {
                schema.Write(stream);
                stream.Position = 0;
                ds.ReadXmlSchema(stream);
            }

#pragma warning disable 618 // ignore obsolete warning about TypedDataSetGenerator
            CodeTypeDeclaration dsClass = new TypedDataSetGenerator().GenerateCode(ds, codeNamespace, codeProvider.CreateGenerator());
#pragma warning restore 618
            string typeName = dsClass.Name;
            importedTypes.Add(element.SchemaType, typeName);
            references.Add("System.Data.dll");
            return typeName;
        }

        internal static bool IsDataSet(XmlSchemaElement e) {
            if (e.UnhandledAttributes != null) {
                foreach (XmlAttribute a in e.UnhandledAttributes) {
                    if (a.LocalName == "IsDataSet" && a.NamespaceURI == Keywords.MSDNS) {
                        // currently the msdata:IsDataSet uses its own format for the boolean values
                        if (a.Value == "True" || a.Value == "true" || a.Value == "1") return true;
                    }
                }
            }
            return false;
        }
    }
}
