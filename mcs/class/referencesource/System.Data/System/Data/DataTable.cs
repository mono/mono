//------------------------------------------------------------------------------
// <copyright file="DataTable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Data.Common;
    using System.Runtime.Versioning;
    using System.Runtime.CompilerServices;

    /// <devdoc>
    ///    <para>Represents one table of in-memory data.</para>
    /// </devdoc>
    [
    ToolboxItem(false),
    DesignTimeVisible(false),
    DefaultProperty("TableName"),
    Editor("Microsoft.VSDesigner.Data.Design.DataTableEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    DefaultEvent("RowChanging"),
    XmlSchemaProvider("GetDataTableSchema"),
    Serializable
    ]
    public class DataTable : MarshalByValueComponent, System.ComponentModel.IListSource, ISupportInitializeNotification, ISerializable, IXmlSerializable{
        private DataSet dataSet;
        private DataView defaultView = null;

        // rows
        /// <summary>
        /// Monotonically increasing number representing the order <see cref="DataRow"/> have been added to <see cref="DataRowCollection"/>.
        /// </summary>
        /// <remarks>This limits <see cref="DataRowCollection.Add(DataRow)"/> to <see cref="Int32.MaxValue"/> operations.</remarks>
        internal long nextRowID;
        internal readonly DataRowCollection rowCollection;

        // columns
        internal readonly DataColumnCollection columnCollection;

        // constraints
        private readonly ConstraintCollection constraintCollection;

        //SimpleContent implementation
        private int elementColumnCount = 0;

        // relations
        internal DataRelationCollection parentRelationsCollection;
        internal DataRelationCollection childRelationsCollection;

        // RecordManager
        internal readonly RecordManager recordManager;

        // index mgmt
        internal readonly List<Index> indexes;

        private List<Index> shadowIndexes;
        private int shadowCount;

        // props
        internal PropertyCollection extendedProperties = null;
        private string tableName = "";
        internal string tableNamespace = null;
        private string tablePrefix = "";
        internal DataExpression displayExpression;
        internal bool fNestedInDataset = true;

        // globalization stuff
        private CultureInfo _culture;
        private bool _cultureUserSet;
        private CompareInfo _compareInfo;
        private CompareOptions _compareFlags = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;
        private IFormatProvider _formatProvider;
        private StringComparer _hashCodeProvider;
        private bool _caseSensitive;
        private bool _caseSensitiveUserSet;

        // XML properties
        internal string encodedTableName;           // For XmlDataDocument only
        internal DataColumn xmlText;            // text values of a complex xml element
        internal DataColumn _colUnique;
        internal bool textOnly = false;         // the table has only text value with possible attributes
        internal decimal minOccurs = 1;    // default = 1
        internal decimal maxOccurs = 1;    // default = 1
        internal bool repeatableElement = false;
        private object typeName = null;

        // primary key info
        private readonly static Int32[] zeroIntegers = new Int32[0];
        internal readonly static DataColumn[] zeroColumns = new DataColumn[0];
        internal readonly static DataRow[] zeroRows = new DataRow[0];
        internal UniqueConstraint primaryKey;
        internal readonly static IndexField[] zeroIndexField = new IndexField[0];
        internal IndexField[] _primaryIndex = zeroIndexField;
        private DataColumn[] delayedSetPrimaryKey = null;

        // Loading Schema and/or Data related optimization
        private Index loadIndex;
        private Index loadIndexwithOriginalAdded = null;
        private Index loadIndexwithCurrentDeleted = null;
        private int _suspendIndexEvents;

        private bool savedEnforceConstraints = false;
        private bool inDataLoad = false;
        private bool initialLoad;
        private bool schemaLoading = false;
        private bool enforceConstraints = true;
        internal bool _suspendEnforceConstraints = false;

        protected internal bool fInitInProgress = false;
        private bool inLoad = false;
        internal bool fInLoadDiffgram = false;

        private byte _isTypedDataTable; // 0 == unknown, 1 = yes, 2 = No
        private DataRow[] EmptyDataRowArray;


        // Property Descriptor Cache for DataBinding
        private PropertyDescriptorCollection propertyDescriptorCollectionCache = null;

        // Cache for relation that has this table as nested child table.
        private static readonly DataRelation[] EmptyArrayDataRelation = new DataRelation[0];
        private DataRelation[] _nestedParentRelations = EmptyArrayDataRelation;

        // Dependent column list for expression evaluation
        internal List<DataColumn> dependentColumns = null;

        // events
        private bool mergingData = false;
        private DataRowChangeEventHandler onRowChangedDelegate;
        private DataRowChangeEventHandler onRowChangingDelegate;
        private DataRowChangeEventHandler onRowDeletingDelegate;
        private DataRowChangeEventHandler onRowDeletedDelegate;
        private DataColumnChangeEventHandler onColumnChangedDelegate;
        private DataColumnChangeEventHandler onColumnChangingDelegate;

        private DataTableClearEventHandler onTableClearingDelegate;
        private DataTableClearEventHandler onTableClearedDelegate;
        private DataTableNewRowEventHandler onTableNewRowDelegate;

        private PropertyChangedEventHandler onPropertyChangingDelegate;

        private System.EventHandler  onInitialized;


        // misc
        private readonly DataRowBuilder rowBuilder;
        private const String KEY_XMLSCHEMA = "XmlSchema";
        private const String KEY_XMLDIFFGRAM = "XmlDiffGram";
        private const String KEY_NAME = "TableName";

        internal readonly List<DataView> delayedViews = new List<DataView>();
        private readonly List<DataViewListener> _dataViewListeners = new List<DataViewListener>();

//        private bool serializeHierarchy = false;
        internal Hashtable rowDiffId = null;
        internal readonly ReaderWriterLock indexesLock = new ReaderWriterLock();
        internal int ukColumnPositionForInference= -1;

        // default remoting format is Xml
        private SerializationFormat _remotingFormat = SerializationFormat.Xml;

        private static int _objectTypeCount; // Bid counter
        private readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.DataTable'/> class with no arguments.</para>
        /// </devdoc>
        public DataTable() {
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataTable.DataTable|API> %d#\n", ObjectID);
            nextRowID = 1;
            recordManager = new RecordManager(this);

            _culture = CultureInfo.CurrentCulture;
            this.columnCollection = new DataColumnCollection(this);
            this.constraintCollection = new ConstraintCollection(this);
            this.rowCollection = new DataRowCollection(this);
            this.indexes = new List<Index>();

            rowBuilder = new DataRowBuilder(this, -1);
        }

        /// <devdoc>
        /// <para>Intitalizes a new instance of the <see cref='System.Data.DataTable'/> class with the specified table
        ///    name.</para>
        /// </devdoc>
        public DataTable(string tableName) : this() {
            this.tableName = tableName == null ? "" : tableName;
        }

        public DataTable(string tableName, string tableNamespace) : this(tableName) {
            this.Namespace = tableNamespace;
        }

//        Deserialize the table from binary/xml stream.
        protected DataTable(SerializationInfo info, StreamingContext context) : this()
        {
            bool isSingleTable = context.Context != null ? Convert.ToBoolean(context.Context, CultureInfo.InvariantCulture) : true;
            SerializationFormat remotingFormat = SerializationFormat.Xml;
            SerializationInfoEnumerator e = info.GetEnumerator();
            while (e.MoveNext()) {
                switch(e.Name) {
                    case "DataTable.RemotingFormat" : //DataTable.RemotingFormat does not exist in V1/V1.1 versions
                    remotingFormat = (SerializationFormat)e.Value;
                    break;
                }
            }

            DeserializeDataTable(info, context, isSingleTable, remotingFormat);
        }

        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            SerializationFormat remotingFormat = RemotingFormat;
            bool isSingleTable = context.Context != null ? Convert.ToBoolean(context.Context, CultureInfo.InvariantCulture) : true;
            SerializeDataTable(info, context, isSingleTable, remotingFormat);
        }

//        Serialize the table schema and data.
        private void SerializeDataTable(SerializationInfo info, StreamingContext context, bool isSingleTable, SerializationFormat remotingFormat) {
            info.AddValue("DataTable.RemotingVersion", new Version(2, 0));

            // SqlHotFix 299, SerializationFormat enumeration types don't exist in V1.1 SP1
            if (SerializationFormat.Xml != remotingFormat) {
                info.AddValue("DataTable.RemotingFormat", remotingFormat);
            }

            if (remotingFormat != SerializationFormat.Xml) {//Binary
                SerializeTableSchema(info, context, isSingleTable);
                if (isSingleTable) {
                    SerializeTableData(info, context, 0);
                }
            } else {//XML/V1.0/V1.1
                string tempDSNamespace = "";
                Boolean fCreatedDataSet = false;

                if (dataSet == null) {
                    DataSet ds = new DataSet("tmpDataSet");
                    // if user set values on DataTable, it isn't necessary
                    // to set them on the DataSet because they won't be inherited
                    // but it is simpler to set them in both places

                    // if user did not set values on DataTable, it is required
                    // to set them on the DataSet so the table will inherit
                    // the value already on the Datatable
                    ds.SetLocaleValue(_culture, _cultureUserSet);
                    ds.CaseSensitive = this.CaseSensitive;
                    ds.namespaceURI  = this.Namespace;
                    Debug.Assert(ds.RemotingFormat == SerializationFormat.Xml, "RemotingFormat must be SerializationFormat.Xml");
                    ds.Tables.Add(this);
                    fCreatedDataSet = true;
                } else {
                    tempDSNamespace = this.DataSet.Namespace;
                    this.DataSet.namespaceURI = this.Namespace; //this.DataSet.Namespace = this.Namespace; ??
                }

                info.AddValue(KEY_XMLSCHEMA, dataSet.GetXmlSchemaForRemoting(this));
                info.AddValue(KEY_XMLDIFFGRAM, dataSet.GetRemotingDiffGram(this));

                if (fCreatedDataSet) {
                    dataSet.Tables.Remove(this);
                }
                else{
                    dataSet.namespaceURI  = tempDSNamespace;
                }
            }
        }

//        Deserialize the table schema and data.
        internal void DeserializeDataTable(SerializationInfo info, StreamingContext context, bool isSingleTable, SerializationFormat remotingFormat) {
            if (remotingFormat != SerializationFormat.Xml) {//Binary
                DeserializeTableSchema(info, context, isSingleTable);
                if (isSingleTable) {
                    DeserializeTableData(info, context, 0);
                    this.ResetIndexes();
                }
            } else {//XML/V1.0/V1.1
                string strSchema = (String)info.GetValue(KEY_XMLSCHEMA, typeof(System.String));
                string strData = (String)info.GetValue(KEY_XMLDIFFGRAM, typeof(System.String));

                if (strSchema != null) {
                    DataSet ds = new DataSet();
                    // fxcop: ReadXmlSchema will provide the CaseSensitive, Locale, Namespace information
                    ds.ReadXmlSchema(new XmlTextReader( new StringReader( strSchema ) ) );

                    Debug.Assert(ds.Tables.Count == 1, "There should be exactly 1 table here");
                    DataTable table = ds.Tables[0];
                    table.CloneTo(this, null, false);// WebData 111656
                    //this is to avoid the cascading rules in the namespace
                    this.Namespace = table.Namespace;

                    if (strData != null) {
                        ds.Tables.Remove(ds.Tables[0]);
                        ds.Tables.Add(this);
                        ds.ReadXml(new XmlTextReader( new StringReader( strData ) ), XmlReadMode.DiffGram);
                        ds.Tables.Remove(this);
                    }
                }
            }
        }

//        Serialize the columns
        internal void SerializeTableSchema(SerializationInfo info, StreamingContext context, bool isSingleTable) {
            //DataTable basic  properties
            info.AddValue("DataTable.TableName", TableName);
            info.AddValue("DataTable.Namespace", Namespace);
            info.AddValue("DataTable.Prefix", Prefix);
            info.AddValue("DataTable.CaseSensitive", _caseSensitive);
            info.AddValue("DataTable.caseSensitiveAmbient", !_caseSensitiveUserSet);
            info.AddValue("DataTable.LocaleLCID", Locale.LCID);
            info.AddValue("DataTable.MinimumCapacity", recordManager.MinimumCapacity);
            //info.AddValue("DataTable.DisplayExpression", DisplayExpression);

            //DataTable state internal properties
            info.AddValue("DataTable.NestedInDataSet", fNestedInDataset);
            info.AddValue("DataTable.TypeName", TypeName.ToString());
            info.AddValue("DataTable.RepeatableElement", repeatableElement);


            //ExtendedProperties
            info.AddValue("DataTable.ExtendedProperties", ExtendedProperties);

            //Columns
            info.AddValue("DataTable.Columns.Count", Columns.Count);

            //Check for closure of expression in case of single table.
            if (isSingleTable) {
                List<DataTable> list = new List<DataTable>();
                list.Add(this);
                if (!CheckForClosureOnExpressionTables(list))
                    throw ExceptionBuilder.CanNotRemoteDataTable();
            }

            IFormatProvider formatProvider = CultureInfo.InvariantCulture;
            for (int i = 0; i < Columns.Count; i++) {
                //DataColumn basic properties
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.ColumnName", i), Columns[i].ColumnName);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.Namespace", i), Columns[i]._columnUri);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.Prefix", i), Columns[i].Prefix);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.ColumnMapping", i), Columns[i].ColumnMapping);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.AllowDBNull", i), Columns[i].AllowDBNull);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.AutoIncrement", i), Columns[i].AutoIncrement);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.AutoIncrementStep", i), Columns[i].AutoIncrementStep);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.AutoIncrementSeed", i), Columns[i].AutoIncrementSeed);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.Caption", i), Columns[i].Caption);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.DefaultValue", i), Columns[i].DefaultValue);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.ReadOnly", i), Columns[i].ReadOnly);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.MaxLength", i), Columns[i].MaxLength);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.DataType", i), Columns[i].DataType);

                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.XmlDataType", i), Columns[i].XmlDataType);
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.SimpleType", i), Columns[i].SimpleType);

                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.DateTimeMode", i), Columns[i].DateTimeMode);

                //DataColumn internal state properties
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.AutoIncrementCurrent", i), Columns[i].AutoIncrementCurrent);

                //Expression
                if (isSingleTable) {
                    info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.Expression", i), Columns[i].Expression);
                }

                //ExtendedProperties
                info.AddValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.ExtendedProperties", i), Columns[i].extendedProperties);
            }

            //Constraints
            if (isSingleTable) {
                SerializeConstraints(info, context, 0, false);
            }
        }

//        Deserialize all the Columns
        internal void DeserializeTableSchema(SerializationInfo info, StreamingContext context, bool isSingleTable) {
            //DataTable basic properties
            tableName = info.GetString("DataTable.TableName");
            tableNamespace = info.GetString("DataTable.Namespace");
            tablePrefix = info.GetString("DataTable.Prefix");

            bool caseSensitive = info.GetBoolean("DataTable.CaseSensitive");
            SetCaseSensitiveValue(caseSensitive, true, false);
            _caseSensitiveUserSet = !info.GetBoolean("DataTable.caseSensitiveAmbient");

            int lcid = (int)info.GetValue("DataTable.LocaleLCID", typeof(int));
            CultureInfo culture = new CultureInfo(lcid);
            SetLocaleValue(culture, true, false);
            _cultureUserSet = true;


            MinimumCapacity = info.GetInt32("DataTable.MinimumCapacity");
            //DisplayExpression = info.GetString("DataTable.DisplayExpression");

            //DataTable state internal properties
            fNestedInDataset = (bool) info.GetBoolean("DataTable.NestedInDataSet");
            string tName = info.GetString("DataTable.TypeName");
            typeName =  new XmlQualifiedName(tName);
            repeatableElement = info.GetBoolean("DataTable.RepeatableElement");

            //ExtendedProperties
            extendedProperties = (PropertyCollection) info.GetValue("DataTable.ExtendedProperties", typeof(PropertyCollection));

            //Columns
            int colCount = info.GetInt32("DataTable.Columns.Count");
            string [] expressions = new string[colCount];
            Debug.Assert(Columns.Count == 0, "There is column in Table");

            IFormatProvider formatProvider = CultureInfo.InvariantCulture;
            for (int i = 0; i < colCount; i++) {
                DataColumn dc = new DataColumn();

                //DataColumn public state properties
                dc.ColumnName = info.GetString(String.Format(formatProvider, "DataTable.DataColumn_{0}.ColumnName", i));
                dc._columnUri = info.GetString(String.Format(formatProvider, "DataTable.DataColumn_{0}.Namespace", i));
                dc.Prefix = info.GetString(String.Format(formatProvider, "DataTable.DataColumn_{0}.Prefix", i));

                dc.DataType = (Type) info.GetValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.DataType", i), typeof(Type));
                dc.XmlDataType = (string) info.GetValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.XmlDataType", i), typeof(string));
                dc.SimpleType = (SimpleType) info.GetValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.SimpleType", i), typeof(SimpleType));

                dc.ColumnMapping = (MappingType) info.GetValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.ColumnMapping", i), typeof(MappingType));
                dc.DateTimeMode = (DataSetDateTime) info.GetValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.DateTimeMode", i), typeof(DataSetDateTime));

                dc.AllowDBNull = info.GetBoolean(String.Format(formatProvider, "DataTable.DataColumn_{0}.AllowDBNull", i));
                dc.AutoIncrement = info.GetBoolean(String.Format(formatProvider, "DataTable.DataColumn_{0}.AutoIncrement", i));
                dc.AutoIncrementStep = info.GetInt64(String.Format(formatProvider, "DataTable.DataColumn_{0}.AutoIncrementStep", i));
                dc.AutoIncrementSeed = info.GetInt64(String.Format(formatProvider, "DataTable.DataColumn_{0}.AutoIncrementSeed", i));
                dc.Caption = info.GetString(String.Format(formatProvider, "DataTable.DataColumn_{0}.Caption", i));
                dc.DefaultValue = info.GetValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.DefaultValue", i), typeof(object));
                dc.ReadOnly = info.GetBoolean(String.Format(formatProvider, "DataTable.DataColumn_{0}.ReadOnly", i));
                dc.MaxLength= info.GetInt32(String.Format(formatProvider, "DataTable.DataColumn_{0}.MaxLength", i));

                //DataColumn internal state properties
                dc.AutoIncrementCurrent = info.GetValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.AutoIncrementCurrent", i), typeof(object));

                //Expression
                if (isSingleTable) {
                    expressions[i] = info.GetString(String.Format(formatProvider, "DataTable.DataColumn_{0}.Expression", i));
                }

                //ExtendedProperties
                dc.extendedProperties = (PropertyCollection) info.GetValue(String.Format(formatProvider, "DataTable.DataColumn_{0}.ExtendedProperties", i), typeof(PropertyCollection));
                Columns.Add(dc);
            }
            if (isSingleTable) {
                for(int i = 0; i < colCount; i++) {
                    if (expressions[i] != null) {
                        Columns[i].Expression = expressions[i];
                    }
                }
            }

            //Constraints
            if (isSingleTable) {
                DeserializeConstraints(info, context, /*table index */ 0, /* serialize all constraints */false);// since single table, send table index as 0, meanwhile passing
                // false for 'allConstraints' means, handle all the constraint related to the table
            }
        }

/*
        Serialize constraints availabe on the table - note this function is marked internal because it is called by the DataSet deserializer.
        ***Schema for Serializing ArrayList of Constraints***
        Unique Constraint - ["U"]->[constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]
        Foriegn Key Constraint - ["F"]->[constraintName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[AcceptRejectRule, UpdateRule, DeleteRule]->[extendedProperties]
*/
        internal void SerializeConstraints(SerializationInfo info, StreamingContext context, int serIndex, bool allConstraints) {
            if (allConstraints) {
                Debug.Assert(DataSet != null);
            }

            ArrayList constraintList = new ArrayList();

            for (int i = 0; i < Constraints.Count; i++) {
                Constraint c = Constraints[i];

                UniqueConstraint uc = c as UniqueConstraint;
                if (uc != null) {
                    int[] colInfo = new int[uc.Columns.Length];
                    for (int j = 0; j < colInfo.Length; j++) {
                        colInfo[j] = uc.Columns[j].Ordinal;
                    }

                    ArrayList list = new ArrayList();
                    list.Add("U");
                    list.Add(uc.ConstraintName);
                    list.Add(colInfo);
                    list.Add(uc.IsPrimaryKey);
                    list.Add(uc.ExtendedProperties);

                    constraintList.Add(list);
                } else {
                    ForeignKeyConstraint fk = c as ForeignKeyConstraint;
                    Debug.Assert(fk != null);
                    bool shouldSerialize = (allConstraints == true) || (fk.Table == this && fk.RelatedTable == this);

                    if (shouldSerialize) {
                        int[] parentInfo = new int[fk.RelatedColumns.Length + 1];
                        parentInfo[0] = allConstraints ? this.DataSet.Tables.IndexOf(fk.RelatedTable) : 0;
                        for (int j = 1; j < parentInfo.Length; j++) {
                            parentInfo[j] = fk.RelatedColumns[j - 1].Ordinal;
                        }

                        int[] childInfo = new int[fk.Columns.Length + 1];
                        childInfo[0] = allConstraints ? this.DataSet.Tables.IndexOf(fk.Table) : 0 ;   //Since the constraint is on the current table, this is the child table.
                        for (int j = 1; j < childInfo.Length; j++) {
                            childInfo[j] = fk.Columns[j - 1].Ordinal;
                        }

                        ArrayList list = new ArrayList();
                        list.Add("F");
                        list.Add(fk.ConstraintName);
                        list.Add(parentInfo);
                        list.Add(childInfo);
                        list.Add(new int[] { (int) fk.AcceptRejectRule, (int) fk.UpdateRule, (int) fk.DeleteRule });
                        list.Add(fk.ExtendedProperties);

                        constraintList.Add(list);
                    }
                }
            }
            info.AddValue(String.Format(CultureInfo.InvariantCulture, "DataTable_{0}.Constraints", serIndex), constraintList);
        }

/*
        Deserialize the constraints on the table.
        ***Schema for Serializing ArrayList of Constraints***
        Unique Constraint - ["U"]->[constraintName]->[columnIndexes]->[IsPrimaryKey]->[extendedProperties]
        Foriegn Key Constraint - ["F"]->[constraintName]->[parentTableIndex, parentcolumnIndexes]->[childTableIndex, childColumnIndexes]->[AcceptRejectRule, UpdateRule, DeleteRule]->[extendedProperties]
*/
        internal void DeserializeConstraints(SerializationInfo info, StreamingContext context, int serIndex, bool allConstraints) {
            ArrayList constraintList = (ArrayList) info.GetValue(String.Format(CultureInfo.InvariantCulture, "DataTable_{0}.Constraints", serIndex), typeof(ArrayList));

            foreach (ArrayList list in constraintList) {
                string con = (string) list[0];

                if (con.Equals("U")) { //Unique Constraints
                    string constraintName = (string) list[1];

                    int[] keyColumnIndexes = (int[]) list[2];
                    bool isPrimaryKey = (bool) list[3];
                    PropertyCollection extendedProperties = (PropertyCollection) list[4];

                    DataColumn[] keyColumns = new DataColumn[keyColumnIndexes.Length];
                    for (int i = 0; i < keyColumnIndexes.Length; i++) {
                        keyColumns[i] = Columns[keyColumnIndexes[i]];
                    }

                    //Create the constraint.
                    UniqueConstraint uc = new UniqueConstraint(constraintName, keyColumns, isPrimaryKey);
                    uc.extendedProperties = extendedProperties;

                    //Add the unique constraint and it will in turn set the primary keys also if needed.
                    Constraints.Add(uc);
                } else { //ForeignKeyConstraints
                    Debug.Assert(con.Equals("F"));

                    string constraintName = (string) list[1];
                    int[] parentInfo = (int[]) list[2];
                    int[] childInfo = (int[]) list[3];
                    int[] rules = (int[]) list[4];
                    PropertyCollection extendedProperties = (PropertyCollection) list[5];

                    //ParentKey Columns.
                    DataTable parentTable = (allConstraints == false) ? this : this.DataSet.Tables[parentInfo[0]];
                    DataColumn[] parentkeyColumns = new DataColumn[parentInfo.Length - 1];
                    for (int i = 0; i < parentkeyColumns.Length; i++) {
                        parentkeyColumns[i] = parentTable.Columns[parentInfo[i + 1]];
                    }

                    //ChildKey Columns.
                    DataTable childTable = (allConstraints == false) ? this : this.DataSet.Tables[childInfo[0]];
                    DataColumn[] childkeyColumns = new DataColumn[childInfo.Length - 1];
                    for (int i = 0; i < childkeyColumns.Length; i++) {
                        childkeyColumns[i] = childTable.Columns[childInfo[i + 1]];
                    }

                    //Create the Constraint.
                    ForeignKeyConstraint fk = new ForeignKeyConstraint(constraintName, parentkeyColumns, childkeyColumns);
                    fk.AcceptRejectRule = (AcceptRejectRule) rules[0];
                    fk.UpdateRule = (Rule) rules[1];
                    fk.DeleteRule = (Rule) rules[2];
                    fk.extendedProperties = extendedProperties;

                    //Add just the foreign key constraint without creating unique constraint.
                    Constraints.Add(fk, false);
                }
            }
        }

//        Serialize the expressions on the table - Marked internal so that DataSet deserializer can call into this
        internal void SerializeExpressionColumns(SerializationInfo info, StreamingContext context, int serIndex) {
            int colCount = Columns.Count;
            for (int i = 0; i < colCount; i++) {
                info.AddValue(String.Format(CultureInfo.InvariantCulture, "DataTable_{0}.DataColumn_{1}.Expression", serIndex, i), Columns[i].Expression);
            }
        }

//        Deserialize the expressions on the table - Marked internal so that DataSet deserializer can call into this
        internal void DeserializeExpressionColumns(SerializationInfo info, StreamingContext context, int serIndex) {
            int colCount = Columns.Count;
            for (int i = 0; i < colCount; i++) {
                string expr = info.GetString(String.Format(CultureInfo.InvariantCulture, "DataTable_{0}.DataColumn_{1}.Expression", serIndex, i));
                if (0 != expr.Length) {
                    Columns[i].Expression = expr;
                }
            }
        }

//        Serialize all the Rows.
        internal void SerializeTableData(SerializationInfo info, StreamingContext context, int serIndex) {
            //Cache all the column count, row count
            int colCount = Columns.Count;
            int rowCount = Rows.Count;
            int modifiedRowCount = 0;
            int editRowCount = 0;

            //Compute row states and assign the bits accordingly - 00[Unchanged], 01[Added], 10[Modifed], 11[Deleted]
            BitArray rowStates = new BitArray(rowCount * 3, false); //All bit flags are set to false on initialization of the BitArray.
            for (int i = 0; i < rowCount; i++) {
                int bitIndex = i * 3;
                DataRow row = Rows[i];
                DataRowState rowState = row.RowState;
                switch (rowState) {
                    case DataRowState.Unchanged:
                        //rowStates[bitIndex] = false;
                        //rowStates[bitIndex + 1] = false;
                        break;
                    case DataRowState.Added:
                        //rowStates[bitIndex] = false;
                        rowStates[bitIndex + 1] = true;
                        break;
                    case DataRowState.Modified:
                        rowStates[bitIndex] = true;
                        //rowStates[bitIndex + 1] = false;
                        modifiedRowCount++;
                        break;
                    case DataRowState.Deleted:
                        rowStates[bitIndex] = true;
                        rowStates[bitIndex + 1] = true;
                        break;
                    default:
                        throw ExceptionBuilder.InvalidRowState(rowState);
                }
                if (-1 != row.tempRecord) {
                    rowStates[bitIndex + 2] = true;
                    editRowCount++;
                }
            }

            //Compute the actual storage records that need to be created.
            int recordCount = rowCount + modifiedRowCount + editRowCount;

            //Create column storages.
            ArrayList storeList = new ArrayList();
            ArrayList nullbitList = new ArrayList();
            if (recordCount > 0) { //Create the storage only if have records.
                for (int i = 0; i < colCount; i++) {
                    object store = Columns[i].GetEmptyColumnStore(recordCount);
                    storeList.Add(store);
                    BitArray nullbits = new BitArray(recordCount);
                    nullbitList.Add(nullbits);
                }
            }

            //Copy values into column storages
            int recordsConsumed = 0;
            Hashtable rowErrors = new Hashtable();
            Hashtable colErrors = new Hashtable();
            for (int i = 0; i < rowCount; i++) {
                int recordsPerRow = Rows[i].CopyValuesIntoStore(storeList, nullbitList, recordsConsumed);
                GetRowAndColumnErrors(i, rowErrors, colErrors);
                recordsConsumed += recordsPerRow;
            }

            IFormatProvider formatProvider = CultureInfo.InvariantCulture;
            //Serialize all the computed values.
            info.AddValue(String.Format(formatProvider, "DataTable_{0}.Rows.Count", serIndex), rowCount);
            info.AddValue(String.Format(formatProvider, "DataTable_{0}.Records.Count", serIndex), recordCount);
            info.AddValue(String.Format(formatProvider, "DataTable_{0}.RowStates", serIndex), rowStates);
            info.AddValue(String.Format(formatProvider, "DataTable_{0}.Records", serIndex), storeList);
            info.AddValue(String.Format(formatProvider, "DataTable_{0}.NullBits", serIndex), nullbitList);
            info.AddValue(String.Format(formatProvider, "DataTable_{0}.RowErrors", serIndex), rowErrors);
            info.AddValue(String.Format(formatProvider, "DataTable_{0}.ColumnErrors", serIndex), colErrors);
        }

//        Deserialize all the Rows.
        internal void DeserializeTableData(SerializationInfo info, StreamingContext context, int serIndex) {
            bool enforceConstraintsOrg = enforceConstraints;
            bool inDataLoadOrg = inDataLoad;


            try {
                enforceConstraints = false;
                inDataLoad = true;
                IFormatProvider formatProvider = CultureInfo.InvariantCulture;
                int rowCount = info.GetInt32(String.Format(formatProvider, "DataTable_{0}.Rows.Count", serIndex));
                int recordCount = info.GetInt32(String.Format(formatProvider, "DataTable_{0}.Records.Count", serIndex));
                BitArray rowStates = (BitArray) info.GetValue(String.Format(formatProvider, "DataTable_{0}.RowStates", serIndex), typeof(BitArray));
                ArrayList storeList = (ArrayList) info.GetValue(String.Format(formatProvider, "DataTable_{0}.Records", serIndex), typeof(ArrayList));
                ArrayList nullbitList = (ArrayList) info.GetValue(String.Format(formatProvider, "DataTable_{0}.NullBits", serIndex), typeof(ArrayList));
                Hashtable rowErrors = (Hashtable) info.GetValue(String.Format(formatProvider, "DataTable_{0}.RowErrors", serIndex), typeof(Hashtable));
                rowErrors.OnDeserialization(this);//OnDeSerialization must be called since the hashtable gets deserialized after the whole graph gets deserialized
                Hashtable colErrors = (Hashtable) info.GetValue(String.Format(formatProvider, "DataTable_{0}.ColumnErrors", serIndex), typeof(Hashtable));
                colErrors.OnDeserialization(this);//OnDeSerialization must be called since the hashtable gets deserialized after the whole graph gets deserialized


                if (recordCount <= 0) { //No need for deserialization of the storage and errors if there are no records.
                    return;
                }

                //Point the record manager storage to the deserialized values.
                for (int i = 0; i < Columns.Count; i++) {
                    Columns[i].SetStorage(storeList[i], (BitArray) nullbitList[i]);
                }

                //Create rows and set the records appropriately.
                int recordIndex = 0;
                DataRow[] rowArr = new DataRow[recordCount];
                for (int i = 0; i < rowCount; i++) {
                    //Create a new row which sets old and new records to -1.
                    DataRow row = NewEmptyRow();
                    rowArr[recordIndex] = row;
                    int bitIndex = i * 3;
                    switch (ConvertToRowState(rowStates, bitIndex)) {
                        case DataRowState.Unchanged:
                            row.oldRecord = recordIndex;
                            row.newRecord = recordIndex;
                            recordIndex += 1;
                            break;
                        case DataRowState.Added:
                            row.oldRecord = -1;
                            row.newRecord = recordIndex;
                            recordIndex += 1;
                            break;
                        case DataRowState.Modified:
                            row.oldRecord = recordIndex;
                            row.newRecord = recordIndex + 1;
                            rowArr[recordIndex + 1] = row;
                            recordIndex += 2;
                            break;
                        case DataRowState.Deleted:
                            row.oldRecord = recordIndex;
                            row.newRecord = -1;
                            recordIndex += 1;
                            break;
                    }
                    if (rowStates[bitIndex + 2]) {
                        row.tempRecord = recordIndex;
                        rowArr[recordIndex] = row;
                        recordIndex += 1;
                    } else {
                        row.tempRecord = -1;
                    }
                    Rows.ArrayAdd(row);
                    row.rowID = nextRowID;
                    nextRowID++;
                    ConvertToRowError(i, rowErrors, colErrors);
                }
                recordManager.SetRowCache(rowArr);
                ResetIndexes();
            } finally {
                enforceConstraints = enforceConstraintsOrg;
                inDataLoad = inDataLoadOrg;
            }
        }

//        Constructs the RowState from the two bits in the bitarray.
        private DataRowState ConvertToRowState(BitArray bitStates, int bitIndex) {
            Debug.Assert(bitStates != null);
            Debug.Assert(bitStates.Length > bitIndex);

            bool b1 = bitStates[bitIndex];
            bool b2 = bitStates[bitIndex + 1];

            if (!b1 && !b2) {
                return DataRowState.Unchanged;
            } else if (!b1 && b2) {
                return DataRowState.Added;
            } else if (b1 && !b2) {
                return DataRowState.Modified;
            } else if (b1 && b2) {
                return DataRowState.Deleted;
            } else {
                throw ExceptionBuilder.InvalidRowBitPattern();
            }
        }

//        Get the error on the row and columns - Marked internal so that DataSet deserializer can call into this
        internal void GetRowAndColumnErrors(int rowIndex, Hashtable rowErrors, Hashtable colErrors) {
            Debug.Assert(Rows.Count > rowIndex);
            Debug.Assert(rowErrors != null);
            Debug.Assert(colErrors != null);

            DataRow row = Rows[rowIndex];

            if (row.HasErrors) {
                rowErrors.Add(rowIndex, row.RowError);
                DataColumn[] dcArr = row.GetColumnsInError();
                if (dcArr.Length > 0) {
                    int[] columnsInError = new int[dcArr.Length];
                    string[] columnErrors = new string[dcArr.Length];
                    for (int i = 0; i < dcArr.Length; i++) {
                        columnsInError[i] = dcArr[i].Ordinal;
                        columnErrors[i] = row.GetColumnError(dcArr[i]);
                    }
                    ArrayList list = new ArrayList();
                    list.Add(columnsInError);
                    list.Add(columnErrors);
                    colErrors.Add(rowIndex, list);
                }
            }
        }

//        Set the row and columns in error..
        private void ConvertToRowError(int rowIndex, Hashtable rowErrors, Hashtable colErrors) {
            Debug.Assert(Rows.Count > rowIndex);
            Debug.Assert(rowErrors != null);
            Debug.Assert(colErrors != null);

            DataRow row = Rows[rowIndex];

            if (rowErrors.ContainsKey(rowIndex)) {
                row.RowError = (string) rowErrors[rowIndex];
            }
            if (colErrors.ContainsKey(rowIndex)) {
                ArrayList list = (ArrayList) colErrors[rowIndex];
                int[] columnsInError = (int[]) list[0];
                string[] columnErrors = (string[]) list[1];
                Debug.Assert(columnsInError.Length == columnErrors.Length);
                for (int i = 0; i < columnsInError.Length; i++) {
                    row.SetColumnError(columnsInError[i], columnErrors[i]);
                }
            }
        }

        /// <devdoc>
        ///    <para>Indicates whether string comparisons within the table are case-sensitive.</para>
        /// </devdoc>
        [ResDescriptionAttribute(Res.DataTableCaseSensitiveDescr)]
        public bool CaseSensitive {
            get {
                //The following assert is valid except when calling DataSet.set_CaseSensitive which Validates constraints and failing here
                //Debug.Assert(_caseSensitiveUserSet || (null == dataSet) || (dataSet.CaseSensitive == _caseSensitive), "CaseSensitive mismatch");
                return _caseSensitive;
            }
            set {
                if (_caseSensitive != value) {
                    bool oldValue = _caseSensitive;
                    bool oldUserSet = _caseSensitiveUserSet;
                    _caseSensitive = value;
                    _caseSensitiveUserSet = true;

                    if (DataSet != null && !DataSet.ValidateCaseConstraint()) {
                        _caseSensitive = oldValue;
                        _caseSensitiveUserSet = oldUserSet;
                        throw ExceptionBuilder.CannotChangeCaseLocale();
                    }
                    SetCaseSensitiveValue(value, true, true);
                }
                _caseSensitiveUserSet = true;
            }
        }

        internal bool AreIndexEventsSuspended {
            get { return (0 < _suspendIndexEvents); }
        }

        internal void RestoreIndexEvents(bool forceReset) {
            Bid.Trace("<ds.DataTable.RestoreIndexEvents|Info> %d#, %d\n", ObjectID, _suspendIndexEvents);
            if (0 < _suspendIndexEvents) {
                _suspendIndexEvents--;
                if (0 == _suspendIndexEvents) {
                    Exception first = null;
                    SetShadowIndexes();
                    try{
                    // the length of shadowIndexes will not change
                    // but the array instance may change during
                    // events during Index.Reset
                        int numIndexes = shadowIndexes.Count;
                        for (int i = 0; i < numIndexes; i++) {
                            Index ndx = shadowIndexes[i];// shadowindexes may change, see ShadowIndexCopy()
                            try {
                                if (forceReset || ndx.HasRemoteAggregate) {
                                    ndx.Reset(); // resets & fires
                                }
                                else {
                                    ndx.FireResetEvent(); // fire the Reset event we were firing
                                }
                            }
                            catch(Exception e) {
                                if (!ADP.IsCatchableExceptionType (e)) {
                                    throw;
                                }
                                ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                                if (null == first) {
                                    first = e;
                                }
                            }
                        }
                        if (null != first) {
                            throw first;
                        }
                    }
                   finally {
                       RestoreShadowIndexes();
                   }
                }
            }
        }

        internal void SuspendIndexEvents() {
            Bid.Trace("<ds.DataTable.SuspendIndexEvents|Info> %d#, %d\n", ObjectID, _suspendIndexEvents);
            _suspendIndexEvents++;
        }

        [Browsable(false)]
        public bool IsInitialized {
            get {
                return !fInitInProgress;
            }
        }

        private bool IsTypedDataTable {
            get {
                switch (_isTypedDataTable) {
                case 0:
                    _isTypedDataTable = (byte)((this.GetType() != typeof(DataTable))? 1 : 2);
                    return (1 == _isTypedDataTable);
                case 1:
                    return true;
                default:
                    return false;
                }
            }
        }

        internal bool SetCaseSensitiveValue(bool isCaseSensitive, bool userSet, bool resetIndexes) {
            if (userSet || (!_caseSensitiveUserSet && (_caseSensitive != isCaseSensitive))) {
                _caseSensitive = isCaseSensitive;
                if (isCaseSensitive) {
                    _compareFlags = CompareOptions.None;
                }
                else {
                    _compareFlags = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;
                }
                if (resetIndexes) {
                    ResetIndexes();
                    foreach (Constraint constraint in Constraints) {
                       constraint.CheckConstraint();
                    }
                }
                return true;
            }
            return false;
        }


        private void ResetCaseSensitive() {
            // this method is used design-time scenarios via reflection
            //   by the property grid context menu to show the Reset option or not
            SetCaseSensitiveValue((null != dataSet) && dataSet.CaseSensitive, true, true);
            _caseSensitiveUserSet = false;
        }

        internal bool ShouldSerializeCaseSensitive() {
            // this method is used design-time scenarios via reflection
            //   by the property grid to show the CaseSensitive property in bold or not
            //   by the code dom for persisting the CaseSensitive property or not
            return _caseSensitiveUserSet;
        }

        internal bool SelfNested {
            get {
                // Is this correct? if ((top[i].nestedParentRelation!= null) && (top[i].nestedParentRelation.ParentTable == top[i]))
                foreach(DataRelation rel in ParentRelations) {
                    if (rel.Nested && rel.ParentTable == this) {
                        return true;
                    }
                }
                return false;
            }
        }
/*        internal bool SelfNestedWithOneRelation {
            get {
                return (this.ParentRelations.Count == 1 && (this.ParentRelations[0].ParentTable == this));
            }
        }
*/

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // don't have debugger view expand this
        internal List<Index> LiveIndexes {
            get {
                if (!AreIndexEventsSuspended) {
                    for (int i = indexes.Count-1; 0 <= i; --i) {
                        Index index = indexes[i];
                        if (index.RefCount <= 1) {
                            index.RemoveRef();
                        }
                    }
                }
                return indexes;
            }
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
                // table can not have different format than its dataset, unless it is stand alone datatable
                if (this.DataSet != null && value != this.DataSet.RemotingFormat) {
                    throw ExceptionBuilder.CanNotSetRemotingFormat();
                }
                _remotingFormat = value;
            }
        }

// used to keep temporary state of unique Key posiotion to be added for inference only
        internal int UKColumnPositionForInference {
            get {
                return ukColumnPositionForInference;
            }
            set{
                ukColumnPositionForInference= value;
            }
        }

        /// <devdoc>
        /// <para>Gets the collection of child relations for this <see cref='System.Data.DataTable'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        ResDescriptionAttribute(Res.DataTableChildRelationsDescr),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public DataRelationCollection ChildRelations {
            get {
                if (childRelationsCollection == null)
                    childRelationsCollection = new DataRelationCollection.DataTableRelationCollection(this, false);
                return childRelationsCollection;
            }
        }

        /// <devdoc>
        ///    <para>Gets the collection of columns that belong to this table.</para>
        /// </devdoc>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataTableColumnsDescr)
        ]
        public DataColumnCollection Columns {
            get {
                return columnCollection;
            }
        }

        private void ResetColumns() {
            // this method is used design-time scenarios via reflection
            //   by the property grid context menu to show the Reset option or not
            Columns.Clear();
        }

        private CompareInfo CompareInfo {
            get {
                if (null == _compareInfo) {
                    _compareInfo = Locale.CompareInfo;
                }
                return _compareInfo;
            }
        }

        /// <devdoc>
        ///    <para>Gets the collection of constraints maintained by this table.</para>
        /// </devdoc>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataTableConstraintsDescr)
        ]
        public ConstraintCollection Constraints {
            get {
                return constraintCollection;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Resets the <see cref='System.Data.DataTable.Constraints'/> property to its default state.
        ///    </para>
        /// </devdoc>
        private void ResetConstraints() {
            Constraints.Clear();
        }

        /// <devdoc>
        /// <para>Gets the <see cref='System.Data.DataSet'/> that this table belongs to.</para>
        /// </devdoc>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), ResDescriptionAttribute(Res.DataTableDataSetDescr)]
        public DataSet DataSet {
            get {
                return dataSet;
            }
        }

        /// <devdoc>
        /// Internal method for setting the DataSet pointer.
        /// </devdoc>
        internal void SetDataSet(DataSet dataSet) {
            if (this.dataSet != dataSet) {
                this.dataSet = dataSet;

                // Inform all the columns of the dataset being set.
                DataColumnCollection   cols = Columns;
                for (int i = 0; i < cols.Count; i++)
                    cols[i].OnSetDataSet();

                if (this.DataSet != null) {
                    defaultView = null;
                }
                //Set the remoting format variable directly
                if (dataSet != null) {
                    _remotingFormat = dataSet.RemotingFormat;
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets a customized view of the table which may include a
        ///       filtered view, or a cursor position.</para>
        /// </devdoc>
        [Browsable(false), ResDescriptionAttribute(Res.DataTableDefaultViewDescr)]
        public DataView DefaultView {
            get {
                DataView view = defaultView;
                if (null == view) {
                    if (null != dataSet) {
                        view = dataSet.DefaultViewManager.CreateDataView(this);
                    }
                    else {
                        view = new DataView(this, true);
                        view.SetIndex2("", DataViewRowState.CurrentRows, null, true);
                    }
                    // avoid HostProtectionAttribute(Synchronization=true) by not calling virtual methods from inside a lock
                    view = Interlocked.CompareExchange<DataView>(ref defaultView, view, null);
                    if (null == view) {
                        view = defaultView;
                    }
                }
                return view;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the expression that will return a value used to represent
        ///       this table in UI.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataTableDisplayExpressionDescr)
        ]
        public string DisplayExpression {
            get {
                return DisplayExpressionInternal;
            }
            set {
                if (value != null && value.Length > 0) {
                    this.displayExpression = new DataExpression(this, value);
                }
                else {
                    this.displayExpression = null;
                }
            }
        }
        internal string DisplayExpressionInternal {
            get {
                return(displayExpression != null ? displayExpression.Expression : "");
            }
        }

        internal bool EnforceConstraints {
            get {
                if (SuspendEnforceConstraints) {
                    return false;
                }
                if (dataSet != null)
                    return dataSet.EnforceConstraints;

                return this.enforceConstraints;
            }
            set {
                if (dataSet == null && this.enforceConstraints != value) {
                    if (value)
                        EnableConstraints();

                    this.enforceConstraints = value;
                }
            }
        }

        internal bool SuspendEnforceConstraints {
            get {
                return _suspendEnforceConstraints ;
            }
            set {
                _suspendEnforceConstraints = value;
            }
        }

        internal void EnableConstraints()
        {
            bool errors = false;
            foreach (Constraint constr in Constraints)
            {
                if (constr is UniqueConstraint)
                    errors |= constr.IsConstraintViolated();
            }

            foreach (DataColumn column in Columns) {
                if (!column.AllowDBNull) {
                    errors |= column.IsNotAllowDBNullViolated();
                }
                if (column.MaxLength >= 0) {
                    errors |= column.IsMaxLengthViolated();
                }
            }

            if (errors) {
                this.EnforceConstraints = false;
                throw ExceptionBuilder.EnforceConstraint();
            }
        }

        /// <devdoc>
        ///    <para>Gets the collection of customized user information.</para>
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

        internal IFormatProvider FormatProvider {
            get {
                // used for Formating/Parsing
                // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfsystemglobalizationcultureinfoclassisneutralculturetopic.asp
                if (null == _formatProvider) {
                    CultureInfo culture = Locale;
                    if (culture.IsNeutralCulture) {
                        culture = CultureInfo.InvariantCulture;
                    }
                    _formatProvider = (IFormatProvider)culture;
                }
                return _formatProvider;
            }
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether there are errors in any of the rows in any of
        ///       the tables of the <see cref='System.Data.DataSet'/> to which the table belongs.</para>
        /// </devdoc>
        [Browsable(false), ResDescriptionAttribute(Res.DataTableHasErrorsDescr)]
        public bool HasErrors {
            get {
                for (int i = 0; i < Rows.Count; i++) {
                    if (Rows[i].HasErrors) {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the locale information used to compare strings within the table.</para>
        ///    <para>Also used for locale sensitive, case,kana,width insensitive column name lookups</para>
        ///    <para>Also used for converting values to and from string</para>
        /// </devdoc>
        [ResDescriptionAttribute(Res.DataTableLocaleDescr)]
        public CultureInfo Locale {
            get {
                // used for Comparing not Formatting/Parsing
                Debug.Assert(null != _culture, "null culture");
                Debug.Assert(_cultureUserSet || (null == dataSet) || _culture.Equals(dataSet.Locale), "Locale mismatch");
                return _culture;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataTable.set_Locale|API> %d#\n", ObjectID);
                try {
                    bool userSet = true;
                    if (null == value)  {
                        // reset Locale to inherit from DataSet
                        userSet = false;
                        value = (null != dataSet) ? dataSet.Locale : _culture;
                    }
                    if (_culture != value && !_culture.Equals(value)) {
                        bool flag = false;
                        bool exceptionThrown = false;
                        CultureInfo oldLocale = _culture;
                        bool oldUserSet = _cultureUserSet;
                        try {
                            _cultureUserSet = true;
                            SetLocaleValue(value, true, false);
                            if ((null == DataSet) || DataSet.ValidateLocaleConstraint()) {
                                flag = false;
                                SetLocaleValue(value, true, true);
                                flag = true;
                            }
                        }
                        catch {
                            exceptionThrown = true;
                            throw;
                        }
                        finally {
                            if (!flag) { // reset old locale if ValidationFailed or exception thrown
                                try {
                                    SetLocaleValue(oldLocale, true, true);
                                }
                                catch(Exception e) { // failed to reset all indexes for all constraints
                                    if (!Common.ADP.IsCatchableExceptionType(e)) {
                                        throw;
                                    }
                                    Common.ADP.TraceExceptionWithoutRethrow(e);
                                }
                                _cultureUserSet = oldUserSet;
                                if (!exceptionThrown) {
                                    throw ExceptionBuilder.CannotChangeCaseLocale(null);
                                }
                            }
                        }
                        SetLocaleValue(value, true, true);
                    }
                    _cultureUserSet = userSet;
                }
                finally{
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        internal bool SetLocaleValue(CultureInfo culture, bool userSet, bool resetIndexes) {
            Debug.Assert(null != culture, "SetLocaleValue: no locale");
            if (userSet || resetIndexes || (!_cultureUserSet && !_culture.Equals(culture))) {
                _culture = culture;
                _compareInfo = null;
                _formatProvider = null;
                _hashCodeProvider = null;

                foreach(DataColumn column in Columns) {
                    column._hashCode = GetSpecialHashCode(column.ColumnName);
                }
                if (resetIndexes) {
                    ResetIndexes();
                    foreach (Constraint constraint in Constraints) {
                        constraint.CheckConstraint();
                    }
                }
                return true;
            }
            return false;
        }

        internal bool ShouldSerializeLocale() {
            // this method is used design-time scenarios via reflection
            //   by the property grid to show the Locale property in bold or not
            //   by the code dom for persisting the Locale property or not

            // we always want the locale persisted if set by user or different the current thread if standalone table
            // but that logic should by performed by the serializion code
            return _cultureUserSet;
        }

        /// <devdoc>
        ///    <para>Gets or sets the initial starting size for this table.</para>
        /// </devdoc>
        [
        DefaultValue(50),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataTableMinimumCapacityDescr)
        ]
        public int MinimumCapacity {
            get {
                return recordManager.MinimumCapacity;
            }
            set {
                if (value != recordManager.MinimumCapacity) {
                    recordManager.MinimumCapacity = value;
                }
            }
        }

        internal int RecordCapacity {
            get {
                return recordManager.RecordCapacity;
            }
        }


        internal int ElementColumnCount {
            get {
                return elementColumnCount;
            }
            set {
                if ((value > 0) && (xmlText != null))
                    throw ExceptionBuilder.TableCannotAddToSimpleContent();
                else elementColumnCount = value;
            }
        }

        /// <devdoc>
        /// <para>Gets the collection of parent relations for this <see cref='System.Data.DataTable'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        ResDescriptionAttribute(Res.DataTableParentRelationsDescr),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public DataRelationCollection ParentRelations {
            get {
                if (parentRelationsCollection == null)
                    parentRelationsCollection = new DataRelationCollection.DataTableRelationCollection(this, true);
                return parentRelationsCollection;
            }
        }

        internal bool MergingData {
            get {
                return mergingData;
            }
            set {
                mergingData = value;
            }
        }

        internal DataRelation[] NestedParentRelations {
            get {
#if DEBUG
                DataRelation[] nRel = FindNestedParentRelations();
                Debug.Assert(nRel.Length == _nestedParentRelations.Length, "nestedParent cache is broken");
                for(int i = 0; i < nRel.Length; i++) {
                    Debug.Assert(null != nRel[i], "null relation");
                    Debug.Assert(null != _nestedParentRelations[i], "null relation");
                    Debug.Assert(nRel[i] == _nestedParentRelations[i], "unequal relations");
                }
#endif
                return _nestedParentRelations;
            }
        }

        internal bool SchemaLoading {
            get {
                return schemaLoading;
            }
        }


        internal void CacheNestedParent() {
            _nestedParentRelations = FindNestedParentRelations();
        }

        private DataRelation[] FindNestedParentRelations() {
            List<DataRelation> nestedParents = null;
            foreach(DataRelation relation in this.ParentRelations) {
                if(relation.Nested) {
                    if (null == nestedParents) {
                        nestedParents = new List<DataRelation>();
                    }
                    nestedParents.Add(relation);
                }
            }
            if ((null == nestedParents) || (nestedParents.Count == 0)) {
                return EmptyArrayDataRelation;
            }
            return nestedParents.ToArray();
        }


        internal int NestedParentsCount {
            get {
                int count = 0;
                foreach(DataRelation relation in this.ParentRelations) {
                    if(relation.Nested)
                        count++;
                }
                return count;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets an array of columns that function as primary keys for the data
        ///       table.</para>
        /// </devdoc>
        [
        TypeConverter(typeof(PrimaryKeyTypeConverter)),
        ResDescriptionAttribute(Res.DataTablePrimaryKeyDescr),
        ResCategoryAttribute(Res.DataCategory_Data),
        Editor("Microsoft.VSDesigner.Data.Design.PrimaryKeyEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing)
        ]
        public DataColumn[] PrimaryKey {
            get {
                UniqueConstraint primayKeyConstraint = primaryKey;
                if (null != primayKeyConstraint) {
                    Debug.Assert(2 <= primaryKey.ConstraintIndex.RefCount, "bad primaryKey index RefCount");
                    return primayKeyConstraint.Key.ToArray();
                }
                return zeroColumns;
            }
            set {
                UniqueConstraint key = null;
                UniqueConstraint existingKey = null;

                // Loading with persisted property
                if (fInitInProgress && value != null) {
                    delayedSetPrimaryKey = value;
                    return;
                }

                if ((value != null) && (value.Length != 0)) {
                    int count = 0;
                    for (int i = 0; i < value.Length; i++) {
                        if (value[i] != null)
                            count++;
                        else
                            break;
                    }

                    if (count != 0) {
                        DataColumn[] newValue = value;
                        if (count != value.Length) {
                            newValue = new DataColumn[count];
                            for (int i = 0; i < count; i++)
                                newValue[i] = value[i];
                        }
                        key = new UniqueConstraint(newValue);
                        if (key.Table != this)
                            throw ExceptionBuilder.TableForeignPrimaryKey();
                    }
                }

                if (key == primaryKey || (key != null && key.Equals(primaryKey)))
                    return;

                // Use an existing UniqueConstraint that matches if one exists
                if ((existingKey = (UniqueConstraint)Constraints.FindConstraint(key)) != null) {
                    key.ColumnsReference.CopyTo(existingKey.Key.ColumnsReference, 0);
                    key = existingKey;
                }

                UniqueConstraint oldKey = primaryKey;
                primaryKey = null;
                if (oldKey != null) {
                    oldKey.ConstraintIndex.RemoveRef();

                    // SQLBU 429176: if PrimaryKey is removed, reset LoadDataRow indexes
                    if (null != loadIndex) {
                        loadIndex.RemoveRef();
                        loadIndex = null;
                    }
                    if (null != loadIndexwithOriginalAdded) {
                        loadIndexwithOriginalAdded.RemoveRef();
                        loadIndexwithOriginalAdded = null;
                    }
                    if (null != loadIndexwithCurrentDeleted) {
                        loadIndexwithCurrentDeleted.RemoveRef();
                        loadIndexwithCurrentDeleted = null;
                    }
                    Constraints.Remove(oldKey);
                }

                // Add the key if there isnt an existing matching key in collection
                if (key != null && existingKey == null)
                    Constraints.Add(key);

                primaryKey = key;

                Debug.Assert(Constraints.FindConstraint(primaryKey) == primaryKey, "PrimaryKey is not in ConstraintCollection");
                _primaryIndex = (key != null) ? key.Key.GetIndexDesc() : zeroIndexField;

                if (primaryKey != null) {
                    // must set index for DataView.Sort before setting AllowDBNull which can fail
                    key.ConstraintIndex.AddRef();

                    for (int i = 0; i < key.ColumnsReference.Length; i++)
                        key.ColumnsReference[i].AllowDBNull = false;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Indicates whether the <see cref='System.Data.DataTable.PrimaryKey'/> property should be persisted.
        ///    </para>
        /// </devdoc>
        private bool ShouldSerializePrimaryKey() {
            return(primaryKey != null);
        }

        /// <devdoc>
        ///    <para>
        ///       Resets the <see cref='System.Data.DataTable.PrimaryKey'/> property to its default state.
        ///    </para>
        /// </devdoc>
        private void ResetPrimaryKey() {
            PrimaryKey = null;
        }

        /// <devdoc>
        ///    <para>Gets the collection of rows that belong to this table.</para>
        /// </devdoc>
        [Browsable(false), ResDescriptionAttribute(Res.DataTableRowsDescr)]
        public DataRowCollection Rows {
            get {
                return rowCollection;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the name of the table.</para>
        /// </devdoc>
        [
        RefreshProperties(RefreshProperties.All),
        DefaultValue(""),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataTableTableNameDescr)
        ]
        public string TableName {
            get {
                return tableName;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataTable.set_TableName|API> %d#, value='%ls'\n", ObjectID, value);
                try {
                    if (value == null) {
                        value = "";
                    }
                    CultureInfo currentLocale = this.Locale;
                    if (String.Compare(tableName, value, true, currentLocale) != 0) {
                        if (dataSet != null) {
                            if (value.Length == 0)
                                throw ExceptionBuilder.NoTableName();
                            if ((0 == String.Compare(value, dataSet.DataSetName, true, dataSet.Locale)) && !fNestedInDataset)
                               throw ExceptionBuilder.DatasetConflictingName(dataSet.DataSetName);

                            DataRelation[] nestedRelations = NestedParentRelations;
                            if (nestedRelations.Length == 0) {
                                dataSet.Tables.RegisterName(value, this.Namespace);
                            }
                            else {
                                foreach(DataRelation rel in nestedRelations) {
                                    if (!rel.ParentTable.Columns.CanRegisterName(value)) {
                                        throw ExceptionBuilder.CannotAddDuplicate2(value);
                                    }
                                }
                                // if it cannot register the following line will throw exception
                                dataSet.Tables.RegisterName(value, this.Namespace);

                                foreach(DataRelation rel in nestedRelations) {
                                    rel.ParentTable.Columns.RegisterColumnName(value, null);
                                    rel.ParentTable.Columns.UnregisterName(this.TableName);
                                }
                            }

                            if (tableName.Length != 0) {
                                dataSet.Tables.UnregisterName(tableName);
                            }
                        }
                        RaisePropertyChanging("TableName");
                        tableName = value;
                        encodedTableName = null;
                    }
                    else if (String.Compare(tableName, value, false, currentLocale) != 0) {
                        RaisePropertyChanging("TableName");
                        tableName = value;
                        encodedTableName = null;
                    }
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }


        internal string EncodedTableName {
            get {
                string encodedTblName = this.encodedTableName;
                if (null == encodedTblName) {
                    encodedTblName = XmlConvert.EncodeLocalName( this.TableName );
                    this.encodedTableName = encodedTblName;
                }
                return encodedTblName;
            }
        }
        private string GetInheritedNamespace(List<DataTable> visitedTables){
            // if there is nested relation: ie: this table is nested child of a another table and
            // if it is not self nested, return parent tables NS: Meanwhile make sure SQLBUDT 240219 is FIXED
            DataRelation[] nestedRelations = NestedParentRelations;
            if (nestedRelations.Length > 0) {
                for(int i =0; i < nestedRelations.Length; i++) {
                    DataRelation rel = nestedRelations[i];
                    if (rel.ParentTable.tableNamespace != null) {
                        return rel.ParentTable.tableNamespace; // if parent table has a non-null NS, return it
                    }
                }
                // Assumption, in hierarchy of multiple nested relation, a child table with no NS, has DataRelation
                // only and only with parent DataTable witin the same namespace
                int j = 0;
                while(j < nestedRelations.Length &&((nestedRelations[j].ParentTable == this)||(visitedTables.Contains(nestedRelations[j].ParentTable)))) {
                    j++;
                }
                if (j < nestedRelations.Length) {
                    DataTable parentTable = nestedRelations[j].ParentTable;
                    if (!visitedTables.Contains(parentTable))
                        visitedTables.Add(parentTable);
                        return parentTable.GetInheritedNamespace(visitedTables);// this is the same as return parentTable.Namespace
                }
            } // dont put else
            if (DataSet != null) { // if it cant return from parent tables, return NS from dataset, if exists
                return  DataSet.Namespace;
            }
            else {
                return string.Empty;
            }

        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the namespace for the <see cref='System.Data.DataTable'/>.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataTableNamespaceDescr)
        ]
        public string Namespace {
            get {
                if (tableNamespace == null) {
                    return GetInheritedNamespace(new List<DataTable>());
                }
                return tableNamespace;
            }
            set {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataTable.set_Namespace|API> %d#, value='%ls'\n", ObjectID, value);
                try {
                    if(value != tableNamespace) {
                        if (dataSet != null) {
                            string realNamespace = (value == null ? GetInheritedNamespace(new List<DataTable>()) : value);
                            if (realNamespace != Namespace) {
                                // do this extra check only if the namespace is really going to change
                                // inheritance-wise.
                                if (dataSet.Tables.Contains( this.TableName, realNamespace, true, true))
                                    throw ExceptionBuilder.DuplicateTableName2(this.TableName, realNamespace);

                                CheckCascadingNamespaceConflict(realNamespace);
                            }
                        }
                        CheckNamespaceValidityForNestedRelations(value);
                        DoRaiseNamespaceChange();
                    }
                    tableNamespace = value;
                }
                finally{
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }
        internal bool IsNamespaceInherited() {
            return (null == tableNamespace);
        }

        internal void CheckCascadingNamespaceConflict(string realNamespace){
            foreach (DataRelation rel in ChildRelations)
                if ((rel.Nested) && (rel.ChildTable != this) && (rel.ChildTable.tableNamespace == null)) {
                    DataTable childTable = rel.ChildTable;
                    if (dataSet.Tables.Contains( childTable.TableName, realNamespace, false, true))
                        throw ExceptionBuilder.DuplicateTableName2(this.TableName, realNamespace);

                    childTable.CheckCascadingNamespaceConflict(realNamespace);
                }

        }

        internal void CheckNamespaceValidityForNestedRelations(string realNamespace){
            foreach(DataRelation rel in ChildRelations) {
                if (rel.Nested) {
                    if (realNamespace != null) {
                        rel.ChildTable.CheckNamespaceValidityForNestedParentRelations(realNamespace, this);
                    }
                    else{
                        rel.ChildTable.CheckNamespaceValidityForNestedParentRelations(GetInheritedNamespace(new List<DataTable>()), this);
                    }
                }
            }
            if (realNamespace == null) { // this will affect this table if it has parent relations
                this.CheckNamespaceValidityForNestedParentRelations(GetInheritedNamespace(new List<DataTable>()), this);
            }

        }
        internal void CheckNamespaceValidityForNestedParentRelations(string ns, DataTable parentTable) {
            foreach(DataRelation rel in ParentRelations){
                if (rel.Nested) {
                    if (rel.ParentTable != parentTable && rel.ParentTable.Namespace != ns) {
                        throw ExceptionBuilder.InValidNestedRelation(this.TableName);
                    }
                }
            }

        }

        internal void DoRaiseNamespaceChange(){
            RaisePropertyChanging("Namespace");
            // raise column Namespace change

            foreach (DataColumn col in Columns)
                if (col._columnUri == null)
                    col.RaisePropertyChanging("Namespace");

            foreach (DataRelation rel in ChildRelations)
                if ((rel.Nested) && (rel.ChildTable != this)) {
                    DataTable childTable = rel.ChildTable;

                    rel.ChildTable.DoRaiseNamespaceChange();
                }
        }
        /// <devdoc>
        ///    <para>
        ///       Indicates whether the <see cref='System.Data.DataTable.Namespace'/> property should be persisted.
        ///    </para>
        /// </devdoc>
        private bool ShouldSerializeNamespace() {
            return(tableNamespace != null);
        }

        /// <devdoc>
        ///    <para>
        ///       Resets the <see cref='System.Data.DataTable.Namespace'/> property to its default state.
        ///    </para>
        /// </devdoc>
        private void ResetNamespace() {
            this.Namespace = null;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        virtual public void BeginInit() {
            fInitInProgress = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        virtual public void EndInit() {
            if (dataSet == null || !dataSet.fInitInProgress) {
                Columns.FinishInitCollection();
                Constraints.FinishInitConstraints();
                foreach(DataColumn dc in Columns){
                    if (dc.Computed) {
                        dc.Expression = dc.Expression;
                    }
                }
            }
            fInitInProgress = false; // Microsoft : 77890. It is must that we set off this flag after calling FinishInitxxx();
            if (delayedSetPrimaryKey != null) {
                PrimaryKey = delayedSetPrimaryKey;
                delayedSetPrimaryKey = null;
            }
            if (delayedViews.Count > 0) {
                foreach(DataView dv in delayedViews) {
                    dv.EndInit();
                }
                delayedViews.Clear();
            }
            OnInitialized();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(""),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataTablePrefixDescr)
        ]
        public string Prefix {
            get { return tablePrefix;}
            set {
                if (value == null) {
                    value = "";
                }
                Bid.Trace("<ds.DataTable.set_Prefix|API> %d#, value='%ls'\n", ObjectID, value);
                if ((XmlConvert.DecodeName(value) == value) &&
                    (XmlConvert.EncodeName(value) != value))
                    throw ExceptionBuilder.InvalidPrefix(value);


                tablePrefix = value;
            }
        }

        internal DataColumn XmlText {
            get {
                return xmlText;
            }
            set {
                if (xmlText != value) {
                    if (xmlText != null) {
                        if (value != null) {
                            throw ExceptionBuilder.MultipleTextOnlyColumns();
                        }
                        Columns.Remove(xmlText);
                    }
                    else {
                        Debug.Assert(value != null, "Value shoud not be null ??");
                        Debug.Assert(value.ColumnMapping == MappingType.SimpleContent, "should be text node here");
                        if (value != Columns[value.ColumnName])
                            Columns.Add(value);
                    }
                    xmlText = value;
                }
            }
        }

        internal decimal MaxOccurs {
            get {
                return maxOccurs;
            }
            set {
                maxOccurs = value;
            }
        }

        internal decimal MinOccurs {
            get {
                return minOccurs;
            }
            set {
                minOccurs = value;
            }
        }

        internal void SetKeyValues(DataKey key, object[] keyValues, int record) {
            for (int i = 0; i < keyValues.Length; i++) {
                key.ColumnsReference[i][record] = keyValues[i];
            }
        }

        internal DataRow FindByIndex(Index ndx, object[] key) {
            Range range = ndx.FindRecords(key);
            if (range.IsNull) {
                return null;
            }
            return this.recordManager[ndx.GetRecord(range.Min)];
        }

        internal DataRow FindMergeTarget(DataRow row, DataKey key, Index ndx) {
            DataRow targetRow = null;

            // Primary key match
            if (key.HasValue) {
                Debug.Assert(ndx != null);
                int   findRecord = (row.oldRecord == -1) ? row.newRecord : row.oldRecord;
                object[] values = key.GetKeyValues(findRecord);
                targetRow = FindByIndex(ndx, values);
            }
            return targetRow;
        }

        private void SetMergeRecords(DataRow row, int newRecord, int oldRecord, DataRowAction action) {
            if (newRecord != -1) {
                SetNewRecord(row, newRecord, action, true, true);
                SetOldRecord(row, oldRecord);
            }
            else {
                SetOldRecord(row, oldRecord);
                if (row.newRecord != -1) {
                    Debug.Assert(action == DataRowAction.Delete, "Unexpected SetNewRecord action in merge function.");
                    SetNewRecord(row, newRecord, action, true, true);
                }
            }
        }

        internal DataRow MergeRow(DataRow row, DataRow targetRow, bool preserveChanges, Index idxSearch) {
             if (targetRow == null) {
                targetRow = this.NewEmptyRow();
                targetRow.oldRecord = recordManager.ImportRecord(row.Table, row.oldRecord);
                targetRow.newRecord = targetRow.oldRecord;
                if(row.oldRecord != row.newRecord) {
                    targetRow.newRecord = recordManager.ImportRecord(row.Table, row.newRecord);
                }
                InsertRow(targetRow, -1);
            }
            else {
                // SQLBU 500789: Record Manager corruption during Merge when target row in edit state
                // the newRecord would be freed and overwrite tempRecord (which became the newRecord)
                // this would leave the DataRow referencing a freed record and leaking memory for the now lost record
                int proposedRecord = targetRow.tempRecord; // by saving off the tempRecord, EndEdit won't free newRecord
                targetRow.tempRecord = -1;
                try {
                    DataRowState saveRowState = targetRow.RowState;
                    int saveIdxRecord = (saveRowState == DataRowState.Added) ? targetRow.newRecord : saveIdxRecord = targetRow.oldRecord;
                     int newRecord;
                     int oldRecord;
                    if (targetRow.RowState == DataRowState.Unchanged && row.RowState == DataRowState.Unchanged) {
                        // unchanged row merging with unchanged row
                        oldRecord = targetRow.oldRecord;
                        newRecord = (preserveChanges) ? recordManager.CopyRecord(this, oldRecord, -1) : targetRow.newRecord;
                        oldRecord = recordManager.CopyRecord(row.Table, row.oldRecord, targetRow.oldRecord);
                        SetMergeRecords(targetRow, newRecord, oldRecord, DataRowAction.Change);
                    }
                    else if (row.newRecord == -1) {
                        // Incoming row is deleted
                        oldRecord = targetRow.oldRecord;
                        if (preserveChanges) {
                          newRecord = (targetRow.RowState == DataRowState.Unchanged)? recordManager.CopyRecord(this, oldRecord, -1) : targetRow.newRecord;
                        }
                        else
                            newRecord = -1;
                        oldRecord = recordManager.CopyRecord(row.Table, row.oldRecord, oldRecord);

                        // Change index record, need to update index
                        if (saveIdxRecord != ((saveRowState == DataRowState.Added) ? newRecord : oldRecord)) {
                            SetMergeRecords(targetRow, newRecord, oldRecord, (newRecord == -1) ? DataRowAction.Delete : DataRowAction.Change);
                            idxSearch.Reset();
                            saveIdxRecord = ((saveRowState == DataRowState.Added) ? newRecord : oldRecord);
                        } else {
                            SetMergeRecords(targetRow, newRecord, oldRecord, (newRecord == -1) ? DataRowAction.Delete : DataRowAction.Change);
                        }
                    }
                    else {
                        // incoming row is added, modified or unchanged (targetRow is not unchanged)
                        oldRecord = targetRow.oldRecord;
                        newRecord = targetRow.newRecord;
                        if (targetRow.RowState == DataRowState.Unchanged) {
                            newRecord = recordManager.CopyRecord(this, oldRecord, -1);
                        }
                        oldRecord = recordManager.CopyRecord(row.Table, row.oldRecord, oldRecord);

                        if (!preserveChanges) {
                            newRecord = recordManager.CopyRecord(row.Table, row.newRecord, newRecord);
                        }
                        SetMergeRecords(targetRow, newRecord, oldRecord, DataRowAction.Change);
                    }

                    if (saveRowState == DataRowState.Added && targetRow.oldRecord != -1)
                        idxSearch.Reset();
                    Debug.Assert(saveIdxRecord == ((saveRowState == DataRowState.Added) ? targetRow.newRecord : targetRow.oldRecord), "oops, you change index record without noticing it");
                }
                finally {
                    targetRow.tempRecord = proposedRecord;
                }
            }

            // Merge all errors
            if (row.HasErrors) {
                if (targetRow.RowError.Length == 0) {
                    targetRow.RowError = row.RowError;
                } else {
                    targetRow.RowError += " ]:[ " + row.RowError;
                }
                DataColumn[] cols = row.GetColumnsInError();

                for (int i = 0; i < cols.Length; i++) {
                    DataColumn col = targetRow.Table.Columns[cols[i].ColumnName];
                    targetRow.SetColumnError(col, row.GetColumnError(cols[i]));
                }
            }else {
                if (!preserveChanges) {
                    targetRow.ClearErrors();
                }
            }

            return targetRow;
        }

        /// <devdoc>
        /// <para>Commits all the changes made to this table since the last time <see cref='System.Data.DataTable.AcceptChanges'/> was called.</para>
        /// </devdoc>
        public void AcceptChanges() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.AcceptChanges|API> %d#\n", ObjectID);
            try {
                DataRow[] oldRows = new DataRow[Rows.Count];
                Rows.CopyTo(oldRows, 0);

                // delay updating of indexes until after all
                // AcceptChange calls have been completed
                SuspendIndexEvents();
                try {
                    for (int i = 0; i < oldRows.Length; ++i) {
                        if (oldRows[i].rowID != -1) {
                            oldRows[i].AcceptChanges();
                        }
                    }
                }
                finally {
                    RestoreIndexEvents(false);
                }
            }
            finally{
                Bid.ScopeLeave(ref hscp);
             }
        }

        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)] 
        protected virtual DataTable CreateInstance() {
            return (DataTable) Activator.CreateInstance(this.GetType(), true);
        }

        public virtual DataTable Clone() {
            return Clone(null);
        }

        internal DataTable Clone(DataSet cloneDS) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.Clone|INFO> %d#, cloneDS=%d\n", ObjectID, (cloneDS != null) ? cloneDS.ObjectID : 0);
            try {
                DataTable clone = CreateInstance();
                if (clone.Columns.Count > 0) // Microsoft : To clean up all the schema in strong typed dataset.
                    clone.Reset();
                return CloneTo(clone, cloneDS, false);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }


        private DataTable IncrementalCloneTo (DataTable sourceTable, DataTable targetTable) {
            foreach(DataColumn dc in sourceTable.Columns) {
                if (targetTable.Columns[dc.ColumnName] == null) {
                    targetTable.Columns.Add(dc.Clone());
                }
            }

            return targetTable;
        }

        private DataTable CloneHierarchy (DataTable sourceTable, DataSet ds, Hashtable visitedMap) {
            if (visitedMap == null)
                visitedMap = new Hashtable();
            if (visitedMap.Contains(sourceTable))
                return ((DataTable)visitedMap[sourceTable]);


            DataTable destinationTable = ds.Tables[sourceTable.TableName, sourceTable.Namespace];

            if ((destinationTable != null && destinationTable.Columns.Count > 0)) {
                destinationTable = IncrementalCloneTo(sourceTable,destinationTable);
                   // get extra columns from source into destination , increamental read
            }
            else {
                if (destinationTable == null) {
                    destinationTable = new DataTable();
                    // fxcop: new DataTable values for CaseSensitive, Locale, Namespace will come from CloneTo
                    ds.Tables.Add(destinationTable);
                }
                destinationTable = sourceTable.CloneTo(destinationTable, ds, true);
            }
            visitedMap[sourceTable] = destinationTable;


            // start cloning relation
            foreach( DataRelation r in sourceTable.ChildRelations ) {
                DataTable childTable = CloneHierarchy((DataTable)r.ChildTable, ds, visitedMap);
             }

            return destinationTable;
         }


        private DataTable CloneTo(DataTable clone, DataSet cloneDS, bool skipExpressionColumns) {
// we do clone datatables while we do readxmlschema, so we do not want to clone columnexpressions if we call this from ReadXmlSchema
// it will cause exception to be thrown in cae expression refers to a table that is not in hirerachy or not created yet
            Debug.Assert(clone != null, "The table passed in has to be newly created empty DataTable.");

            // set All properties
            clone.tableName = tableName;

            clone.tableNamespace = tableNamespace;
            clone.tablePrefix = tablePrefix;
            clone.fNestedInDataset = fNestedInDataset;

            clone._culture = _culture;
            clone._cultureUserSet = _cultureUserSet;
            clone._compareInfo = _compareInfo;
            clone._compareFlags = _compareFlags;
            clone._formatProvider = _formatProvider;
            clone._hashCodeProvider = _hashCodeProvider;
            clone._caseSensitive = _caseSensitive;
            clone._caseSensitiveUserSet = _caseSensitiveUserSet;

            clone.displayExpression = displayExpression;
            clone.typeName = typeName; //Microsoft
            clone.repeatableElement = repeatableElement; //Microsoft
            clone.MinimumCapacity = MinimumCapacity;
            clone.RemotingFormat = RemotingFormat;
//            clone.SerializeHierarchy = SerializeHierarchy;

            // add all columns
            DataColumnCollection clmns = this.Columns;
            for (int i = 0; i < clmns.Count; i++) {
                clone.Columns.Add(clmns[i].Clone());
            }

            // add all expressions if Clone is invoked only on DataTable otherwise DataSet.Clone will assign expressions after creating all relationships.
            if (!skipExpressionColumns && cloneDS == null) {
                for (int i = 0; i < clmns.Count; i++) {
                    clone.Columns[clmns[i].ColumnName].Expression = clmns[i].Expression;
                }
            }

            // Create PrimaryKey
            DataColumn[] pkey = PrimaryKey;
            if (pkey.Length > 0) {
                DataColumn[] key = new DataColumn[pkey.Length];
                for (int i = 0; i < pkey.Length; i++) {
                    key[i] = clone.Columns[pkey[i].Ordinal];
                }
                clone.PrimaryKey = key;
            }

            // now clone all unique constraints
            // Rename first
            for (int j = 0; j < Constraints.Count; j++)  {
                ForeignKeyConstraint foreign = Constraints[j] as ForeignKeyConstraint;
                UniqueConstraint unique = Constraints[j] as UniqueConstraint;
                if (foreign  != null) {
                    if (foreign.Table == foreign.RelatedTable) {
                        ForeignKeyConstraint clonedConstraint = foreign.Clone(clone);
                        Constraint oldConstraint = clone.Constraints.FindConstraint(clonedConstraint);
                        if (oldConstraint != null) {
                            oldConstraint.ConstraintName = Constraints[j].ConstraintName;
                        }
                    }
                }
                else if (unique != null) {
                    UniqueConstraint clonedConstraint = unique.Clone(clone);
                    Constraint oldConstraint = clone.Constraints.FindConstraint(clonedConstraint);
                    if (oldConstraint != null) {
                        oldConstraint.ConstraintName = Constraints[j].ConstraintName;
                        foreach (Object key in clonedConstraint.ExtendedProperties.Keys) {
                            oldConstraint.ExtendedProperties[key] = clonedConstraint.ExtendedProperties[key];
                        }
                    }
                }
            }

            // then add
            for (int j = 0; j < Constraints.Count; j++)  {
                if (! clone.Constraints.Contains(Constraints[j].ConstraintName, true)) {
                    ForeignKeyConstraint foreign = Constraints[j] as ForeignKeyConstraint;
                    UniqueConstraint unique = Constraints[j] as UniqueConstraint;
                    if (foreign  != null) {
                        if (foreign.Table == foreign.RelatedTable) {
                            ForeignKeyConstraint newforeign = foreign.Clone(clone);
                            if (newforeign != null) { // we cant make sure that we recieve a cloned FKC,since it depends if table and relatedtable be the same
                                clone.Constraints.Add(newforeign);
                            }
                        }
                    }
                    else if (unique != null) {
                        clone.Constraints.Add(unique.Clone(clone));
                    }
                 }
            }

            // ...Extended Properties...

            if (this.extendedProperties != null) {
                foreach(Object key in this.extendedProperties.Keys) {
                    clone.ExtendedProperties[key]=this.extendedProperties[key];
                }
            }

            return clone;
        }


        public DataTable Copy(){
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.Copy|API> %d#\n", ObjectID);
            try {
                DataTable destTable = this.Clone();

                foreach (DataRow row in Rows)
                    CopyRow(destTable, row);

                return destTable;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>Occurs when a value has been submitted for this column.</para>
        /// </devdoc>
        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableColumnChangingDescr)]
        public event DataColumnChangeEventHandler ColumnChanging {
            add {
                Bid.Trace("<ds.DataTable.add_ColumnChanging|API> %d#\n", ObjectID);
                onColumnChangingDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_ColumnChanging|API> %d#\n", ObjectID);
                onColumnChangingDelegate -= value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableColumnChangedDescr)]
        public event DataColumnChangeEventHandler ColumnChanged {
            add  {
                Bid.Trace("<ds.DataTable.add_ColumnChanged|API> %d#\n", ObjectID);
                onColumnChangedDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_ColumnChanged|API> %d#\n", ObjectID);
                onColumnChangedDelegate -= value;
            }
        }

        [
            ResCategoryAttribute(Res.DataCategory_Action),
            ResDescriptionAttribute(Res.DataSetInitializedDescr)
        ]
        public event System.EventHandler  Initialized {
            add {
                onInitialized += value;
            }
            remove {
                onInitialized -= value;
            }
        }

        internal event PropertyChangedEventHandler PropertyChanging {
            add {
                Bid.Trace("<ds.DataTable.add_PropertyChanging|INFO> %d#\n", ObjectID);
                onPropertyChangingDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_PropertyChanging|INFO> %d#\n", ObjectID);
                onPropertyChangingDelegate -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs after a row in the table has been successfully edited.
        ///    </para>
        /// </devdoc>
        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableRowChangedDescr)]
        public event DataRowChangeEventHandler RowChanged {
            add {
                Bid.Trace("<ds.DataTable.add_RowChanged|API> %d#\n", ObjectID);
                onRowChangedDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_RowChanged|API> %d#\n", ObjectID);
                onRowChangedDelegate -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs when the <see cref='System.Data.DataRow'/> is changing.
        ///    </para>
        /// </devdoc>
        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableRowChangingDescr)]
        public event DataRowChangeEventHandler RowChanging {
            add {
                Bid.Trace("<ds.DataTable.add_RowChanging|API> %d#\n", ObjectID);
                onRowChangingDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_RowChanging|API> %d#\n", ObjectID);
                onRowChangingDelegate -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs before a row in the table is
        ///       about to be deleted.
        ///    </para>
        /// </devdoc>
        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableRowDeletingDescr)]
        public event DataRowChangeEventHandler RowDeleting {
            add {
                Bid.Trace("<ds.DataTable.add_RowDeleting|API> %d#\n", ObjectID);
                onRowDeletingDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_RowDeleting|API> %d#\n", ObjectID);
                onRowDeletingDelegate -= value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs after a row in the
        ///       table has been deleted.
        ///    </para>
        /// </devdoc>
        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableRowDeletedDescr)]
        public event DataRowChangeEventHandler RowDeleted {
            add {
                Bid.Trace("<ds.DataTable.add_RowDeleted|API> %d#\n", ObjectID);
                onRowDeletedDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_RowDeleted|API> %d#\n", ObjectID);
                onRowDeletedDelegate -= value;
            }
        }

        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableRowsClearingDescr)]
        public event DataTableClearEventHandler TableClearing {
            add {
                Bid.Trace("<ds.DataTable.add_TableClearing|API> %d#\n", ObjectID);
                onTableClearingDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_TableClearing|API> %d#\n", ObjectID);
                onTableClearingDelegate -= value;
            }
        }

        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableRowsClearedDescr)]
        public event DataTableClearEventHandler TableCleared {
            add {
                Bid.Trace("<ds.DataTable.add_TableCleared|API> %d#\n", ObjectID);
                onTableClearedDelegate += value;
            }
            remove {
                Bid.Trace("<ds.DataTable.remove_TableCleared|API> %d#\n", ObjectID);
                onTableClearedDelegate -= value;
            }
        }

        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataTableRowsNewRowDescr)]
        public event DataTableNewRowEventHandler TableNewRow {
            add {
                onTableNewRowDelegate += value;
            }
            remove {
                onTableNewRowDelegate -= value;
            }
        }

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
                        for (int i = 0; i < Columns.Count; i++) {
                            if (Columns[i].Site != null) {
                                cont.Remove(Columns[i]);
                            }
                        }
                    }
                }
                base.Site = value;
            }
        }

        internal DataRow AddRecords(int oldRecord, int newRecord) {
            DataRow row;
            if (oldRecord == -1 && newRecord == -1)
            {
                row = NewRow(-1);
                AddRow(row);
            }
            else
            {
                row = NewEmptyRow();
                row.oldRecord = oldRecord;
                row.newRecord = newRecord;
                InsertRow(row, -1);
            }
            return row;
        }

        internal void AddRow(DataRow row) {
            AddRow(row, -1);
        }

        internal void AddRow(DataRow row, int proposedID) {
            InsertRow(row, proposedID, -1);
        }

        internal void InsertRow(DataRow row, int proposedID, int pos) {
            InsertRow(row, proposedID, pos, /*fireEvent*/true);
        }

        internal void InsertRow(DataRow row, long proposedID, int pos, bool fireEvent) {
            Exception deferredException = null;

            if (row == null) {
                throw ExceptionBuilder.ArgumentNull("row");
            }
            if (row.Table != this) {
                throw ExceptionBuilder.RowAlreadyInOtherCollection();
            }
            if (row.rowID != -1) {
                throw ExceptionBuilder.RowAlreadyInTheCollection();
            }
            row.BeginEdit(); // ensure something's there.            

            int record = row.tempRecord;
            row.tempRecord = -1;

            if (proposedID == -1) {
                proposedID = this.nextRowID;
            }

            bool rollbackOnException;
            if (rollbackOnException = (nextRowID <= proposedID)) { // WebData 109005
                nextRowID = checked(proposedID + 1);
            }

            try {
                try {
                    row.rowID = proposedID;
                    // this method may cause DataView.OnListChanged in which another row may be added
                    SetNewRecordWorker(row, record, DataRowAction.Add, false, false, pos, fireEvent, out deferredException); // now we do add the row to collection before OnRowChanged (RaiseRowChanged)
                }
                catch {
                    if (rollbackOnException && (nextRowID == proposedID+1)) {
                        nextRowID = proposedID;
                    }
                    row.rowID = -1;
                    row.tempRecord = record;
                    throw;
                }

                // since expression evaluation occurred in SetNewRecordWorker, there may have been a problem that
                // was deferred to this point.  If so, throw now since row has already been added.
                if (deferredException != null)
                    throw deferredException;

                if (EnforceConstraints && !inLoad ) { // if we are evaluating expression, we need to validate constraints
                    int columnCount = columnCollection.Count;
                    for (int i = 0; i < columnCount; ++i) {
                        DataColumn column = columnCollection[i];
                        if (column.Computed) {
                            column.CheckColumnConstraint(row, DataRowAction.Add);
                        }
                    }
                }
            }
            finally {
                row.ResetLastChangedColumn();// if expression is evaluated while adding, before  return, we want to clear it
            }
        }

        internal void CheckNotModifying(DataRow row) {
            if (row.tempRecord != -1) {
                row.EndEdit();
                //throw ExceptionBuilder.ModifyingRow();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Clears the table of all data.</para>
        /// </devdoc>

        public void Clear() {
            Clear(true);
        }
        internal void Clear(bool clearAll) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.Clear|INFO> %d#, clearAll=%d{bool}\n", ObjectID, clearAll);

            try {
                Debug.Assert(null == rowDiffId, "wasn't previously cleared");
                rowDiffId = null;

                if (dataSet != null)
                    dataSet.OnClearFunctionCalled(this);
                bool shouldFireClearEvents = (this.Rows.Count != 0); // if Rows is already empty, this is noop

                DataTableClearEventArgs e = null;
                if (shouldFireClearEvents) {
                    e = new DataTableClearEventArgs (this);
                    OnTableClearing(e);
                }

                if (dataSet != null && dataSet.EnforceConstraints) {

                    for (ParentForeignKeyConstraintEnumerator constraints = new ParentForeignKeyConstraintEnumerator(dataSet, this); constraints.GetNext();) {
                        ForeignKeyConstraint constraint = constraints.GetForeignKeyConstraint();
                        constraint.CheckCanClearParentTable(this);
                    }
                }

                recordManager.Clear(clearAll);

                // SQLBU 415729: Serious performance issue when calling Clear()
                // this improves performance by iterating over rows instead of computing by index
                foreach(DataRow row in Rows) {
                    row.oldRecord = -1;
                    row.newRecord = -1;
                    row.tempRecord = -1;
                    row.rowID = -1;
                    row.RBTreeNodeId = 0;
                }
                Rows.ArrayClear();

                ResetIndexes();

                if (shouldFireClearEvents) {
                    OnTableCleared(e);
                }

                // SQLBU 501916 - DataTable internal index is corrupted:'5'
                foreach(DataColumn column in Columns) {
                    EvaluateDependentExpressions(column);
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal void CascadeAll(DataRow row, DataRowAction action) {
            if (DataSet != null && DataSet.fEnableCascading) {
                for (ParentForeignKeyConstraintEnumerator constraints = new ParentForeignKeyConstraintEnumerator(dataSet, this); constraints.GetNext();) {
                    constraints.GetForeignKeyConstraint().CheckCascade(row, action);
                }
            }
        }

        internal void CommitRow(DataRow row) {
            // Fire Changing event
            DataRowChangeEventArgs drcevent = OnRowChanging(null, row, DataRowAction.Commit);

            if (!inDataLoad)
                CascadeAll(row, DataRowAction.Commit);

            SetOldRecord(row, row.newRecord);

            OnRowChanged(drcevent, row, DataRowAction.Commit);
        }

        internal int Compare(string s1, string s2) {
            return Compare(s1, s2, null);
        }

        internal int Compare(string s1, string s2, CompareInfo comparer) {
            object obj1 = s1;
            object obj2 = s2;
            if (obj1 == obj2)
                return 0;
            if (obj1 == null)
                return -1;
            if (obj2 == null)
                return 1;

            int leng1 = s1.Length;
            int leng2 = s2.Length;

            for (; leng1 > 0; leng1--) {
                if (s1[leng1-1] != 0x20 && s1[leng1-1] != 0x3000) // 0x3000 is Ideographic Whitespace
                    break;
            }
            for (; leng2 > 0; leng2--) {
                if (s2[leng2-1] != 0x20 && s2[leng2-1] != 0x3000)
                    break;
            }

            return (comparer ?? this.CompareInfo).Compare(s1, 0, leng1, s2, 0, leng2, _compareFlags);
        }

        internal int IndexOf(string s1, string s2) {
            return CompareInfo.IndexOf(s1, s2, _compareFlags);
        }

        internal bool IsSuffix(string s1, string s2) {
            return CompareInfo.IsSuffix(s1, s2, _compareFlags);
        }

        /// <devdoc>
        ///    <para>Computes the given expression on the current rows that pass the filter criteria.</para>
        /// </devdoc>
        public object Compute(string expression, string filter) {
            DataRow[] rows = Select(filter, "", DataViewRowState.CurrentRows);
            DataExpression expr = new DataExpression(this, expression);
            return expr.Evaluate(rows);
        }

        bool System.ComponentModel.IListSource.ContainsListCollection {
            get {
                return false;
            }
        }

        internal void CopyRow(DataTable table, DataRow row)
        {
            int oldRecord = -1, newRecord = -1;

            if (row == null)
                return;

            if (row.oldRecord != -1) {
                oldRecord = table.recordManager.ImportRecord(row.Table, row.oldRecord);
            }
            if (row.newRecord != -1) {
                if (row.newRecord != row.oldRecord) {
                    newRecord = table.recordManager.ImportRecord(row.Table, row.newRecord);
                }
                else
                    newRecord = oldRecord;
            }

            DataRow targetRow = table.AddRecords(oldRecord, newRecord);

            if (row.HasErrors) {
                targetRow.RowError = row.RowError;

                DataColumn[] cols = row.GetColumnsInError();

                for (int i = 0; i < cols.Length; i++) {
                    DataColumn col = targetRow.Table.Columns[cols[i].ColumnName];
                    targetRow.SetColumnError(col, row.GetColumnError(cols[i]));
                }
            }

       }


        internal void DeleteRow(DataRow row) {
            if (row.newRecord == -1) {
                throw ExceptionBuilder.RowAlreadyDeleted();
            }

            // Store.PrepareForDelete(row);
            SetNewRecord(row, -1, DataRowAction.Delete, false, true);
        }

        private void CheckPrimaryKey() {
            if (primaryKey == null) throw ExceptionBuilder.TableMissingPrimaryKey();
        }

        internal DataRow FindByPrimaryKey(object[] values) {
            CheckPrimaryKey();
            return FindRow(primaryKey.Key, values);
        }

        internal DataRow FindByPrimaryKey(object value) {
            CheckPrimaryKey();
            return FindRow(primaryKey.Key, value);
        }

        private DataRow FindRow(DataKey key, object[] values) {
            Index index = GetIndex(NewIndexDesc(key));
            Range range = index.FindRecords(values);
            if (range.IsNull)
                return null;
            return recordManager[index.GetRecord(range.Min)];
        }

        private DataRow FindRow(DataKey key, object value) {
            Index index = GetIndex(NewIndexDesc(key));
            Range range = index.FindRecords(value);
            if (range.IsNull)
                return null;
            return recordManager[index.GetRecord(range.Min)];
        }

        internal string FormatSortString(IndexField[] indexDesc) {
            StringBuilder builder = new StringBuilder();
            foreach (IndexField field in indexDesc) {
                if (0 < builder.Length) {
                    builder.Append(", ");
                }
                builder.Append(field.Column.ColumnName);
                if (field.IsDescending) {
                    builder.Append(" DESC");
                }
            }
            return builder.ToString();
        }

        internal void FreeRecord(ref int record) {
            recordManager.FreeRecord(ref record);
        }

        public DataTable GetChanges() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.GetChanges|API> %d#\n", ObjectID);
            try {
                DataTable dtChanges = this.Clone();
                DataRow row = null;

                for (int i = 0; i < Rows.Count; i++) {
                    row = Rows[i];
                    if (row.oldRecord != row.newRecord)
                        dtChanges.ImportRow(row);
                }

                if (dtChanges.Rows.Count == 0)
                    return null;

                return dtChanges;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public DataTable GetChanges(DataRowState rowStates)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.GetChanges|API> %d#, rowStates=%d{ds.DataRowState}\n", ObjectID, (int)rowStates);
            try {
                DataTable dtChanges = this.Clone();
                DataRow row = null;

                // check that rowStates is valid DataRowState
                Debug.Assert(Enum.GetUnderlyingType(typeof(DataRowState)) == typeof(Int32), "Invalid DataRowState type");

                for (int i = 0; i < Rows.Count; i++) {
                    row = Rows[i];
                    if ((row.RowState & rowStates) != 0)
                        dtChanges.ImportRow(row);
                }

                if (dtChanges.Rows.Count == 0)
                    return null;

                return dtChanges;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        /// <para>Returns an array of <see cref='System.Data.DataRow'/> objects that contain errors.</para>
        /// </devdoc>
        public DataRow[] GetErrors() {
            List<DataRow> errorList = new List<DataRow>();

            for (int i = 0; i < Rows.Count; i++) {
                DataRow row = Rows[i];
                if (row.HasErrors) {
                    errorList.Add(row);
                }
            }
            DataRow[] temp = NewRowArray(errorList.Count);
            errorList.CopyTo(temp);
            return temp;
        }

        internal Index GetIndex(IndexField[] indexDesc) {
            return GetIndex(indexDesc, DataViewRowState.CurrentRows, (IFilter)null);
        }

        internal Index GetIndex(string sort, DataViewRowState recordStates, IFilter rowFilter) {
            return GetIndex(ParseSortString(sort), recordStates, rowFilter);
        }

        internal Index GetIndex(IndexField[] indexDesc, DataViewRowState recordStates, IFilter rowFilter) {
            indexesLock.AcquireReaderLock(-1);
            try {
                for (int i = 0; i < indexes.Count; i++) {
                    Index index = indexes[i];
                    if (index != null) {
                        if (index.Equal(indexDesc, recordStates, rowFilter)) {
                            return index;
                        }
                    }
                }
            }
            finally {
                indexesLock.ReleaseReaderLock();
            }
            Index ndx = new Index(this, indexDesc, recordStates, rowFilter);
            ndx.AddRef();
            return ndx;
        }

        IList System.ComponentModel.IListSource.GetList() {
            return DefaultView;
        }


        internal List<DataViewListener> GetListeners() {
            return _dataViewListeners;
        }

        // We need a HashCodeProvider for Case, Kana and Width insensitive
        internal int GetSpecialHashCode(string name) {
            int i;
            for (i = 0; (i < name.Length) && (0x3000 > name[i]); ++i);

            if (name.Length == i) {
                if (null == _hashCodeProvider) {
                    // it should use the CaseSensitive property, but V1 shipped this way
                    _hashCodeProvider = StringComparer.Create(Locale, true);
                }
                return _hashCodeProvider.GetHashCode(name);
            }
            else {
                return 0;
            }
        }

        public void ImportRow(DataRow row)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.ImportRow|API> %d#\n", ObjectID);
            try {
                int oldRecord = -1, newRecord = -1;

                if (row == null)
                    return;

                if (row.oldRecord != -1) {
                    oldRecord = recordManager.ImportRecord(row.Table, row.oldRecord);
                }
                if (row.newRecord != -1) {  // row not deleted
                    if (row.RowState != DataRowState.Unchanged) { // not unchanged, it means Added or modified
                        newRecord = recordManager.ImportRecord(row.Table, row.newRecord);
                    }
                    else
                        newRecord = oldRecord;
                }

                if (oldRecord != -1 || newRecord != -1) {
                    DataRow targetRow = AddRecords(oldRecord, newRecord);

                    if (row.HasErrors) {
                        targetRow.RowError = row.RowError;

                        DataColumn[] cols = row.GetColumnsInError();

                        for (int i = 0; i < cols.Length; i++) {
                            DataColumn col = targetRow.Table.Columns[cols[i].ColumnName];
                            targetRow.SetColumnError(col, row.GetColumnError(cols[i]));
                        }
                    }
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }

       }

        internal void InsertRow(DataRow row, long proposedID) {
            IntPtr hscp;

            Bid.ScopeEnter(out hscp, "<ds.DataTable.InsertRow|INFO> %d#, row=%d\n", ObjectID, row.ObjectID);
            try {
                if (row.Table != this) {
                    throw ExceptionBuilder.RowAlreadyInOtherCollection();
                }
                if (row.rowID != -1) {
                    throw ExceptionBuilder.RowAlreadyInTheCollection();
                }
                if (row.oldRecord == -1 && row.newRecord == -1) {
                    throw ExceptionBuilder.RowEmpty();
                }

                if (proposedID == -1)
                    proposedID = nextRowID;

                row.rowID = proposedID;
                if (nextRowID <= proposedID)
                    nextRowID = checked(proposedID + 1);

                DataRowChangeEventArgs drcevent = null;


                if (row.newRecord != -1) {
                    row.tempRecord = row.newRecord;
                    row.newRecord = -1;

                    try {
                        drcevent = RaiseRowChanging(null, row, DataRowAction.Add, true);
                    }
                    catch {
                        row.tempRecord = -1;
                        throw;
                    }

                    row.newRecord = row.tempRecord;
                    row.tempRecord = -1;
                }

                if (row.oldRecord != -1)
                    recordManager[row.oldRecord] = row;

                if (row.newRecord != -1)
                    recordManager[row.newRecord] = row;

                Rows.ArrayAdd(row); // SQL BU Defect Tracking 247738, 323482 row should be in the
                                    // collection when maintaining the indexes                  

                if (row.RowState == DataRowState.Unchanged){ //  how about row.oldRecord == row.newRecord both == -1
                    RecordStateChanged(row.oldRecord, DataViewRowState.None, DataViewRowState.Unchanged);
                }
                else {
                    RecordStateChanged(row.oldRecord, DataViewRowState.None, row.GetRecordState(row.oldRecord),
                                       row.newRecord, DataViewRowState.None, row.GetRecordState(row.newRecord));
                }

                if (dependentColumns != null && dependentColumns.Count > 0)
                    EvaluateExpressions(row, DataRowAction.Add, null);

                RaiseRowChanged(drcevent, row, DataRowAction.Add);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private IndexField [] NewIndexDesc(DataKey key) {
            Debug.Assert(key.HasValue);
            IndexField[] indexDesc = key.GetIndexDesc();
            IndexField[] newIndexDesc = new IndexField[indexDesc.Length];
            Array.Copy(indexDesc, 0, newIndexDesc, 0, indexDesc.Length);
            return newIndexDesc;
        }

        internal int NewRecord() {
            return NewRecord(-1);
        }

        internal int NewUninitializedRecord() {
            return recordManager.NewRecordBase();
        }

        internal int NewRecordFromArray(object[] value) {
            int colCount = columnCollection.Count; // Perf: use the readonly columnCollection field directly
            if (colCount < value.Length) {
                throw ExceptionBuilder.ValueArrayLength();
            }
            int record = recordManager.NewRecordBase();
            try {
                for (int i = 0; i < value.Length; i++) {
                    if (null != value[i]) {
                        columnCollection[i][record] = value[i];
                    }
                    else {
                        columnCollection[i].Init(record);  // Increase AutoIncrementCurrent
                    }
                }
                for (int i = value.Length; i < colCount; i++) {
                    columnCollection[i].Init(record);
                }
                return record;
            }
            catch (Exception e) {
                // 
                if (Common.ADP.IsCatchableOrSecurityExceptionType (e)) {
                    FreeRecord(ref record); // WebData 104246
                }
                throw;
            }
        }

        internal int NewRecord(int sourceRecord) {
            int record = recordManager.NewRecordBase();

            int count = columnCollection.Count;
            if (-1 == sourceRecord) {
                for (int i = 0; i < count; ++i) {
                    columnCollection[i].Init(record);
                }
            }
            else {
                for (int i = 0; i < count; ++i) {
                    columnCollection[i].Copy(sourceRecord, record);
                }
            }
            return record;
        }

        internal DataRow NewEmptyRow() {
            rowBuilder._record = -1;
            DataRow dr = NewRowFromBuilder( rowBuilder );
            if (dataSet != null) {
                DataSet.OnDataRowCreated( dr );
            }
            return dr;
        }

        private DataRow NewUninitializedRow() {
            DataRow dr = NewRow(NewUninitializedRecord());
            return dr;
        }

        /// <devdoc>
        /// <para>Creates a new <see cref='System.Data.DataRow'/>
        /// with the same schema as the table.</para>
        /// </devdoc>
        public DataRow NewRow() {
            DataRow dr = NewRow(-1);
            NewRowCreated(dr); // this is the only API we want this event to be fired
            return dr;
        }

        // Only initialize DataRelation mapping columns (approximately hidden columns)
        internal DataRow CreateEmptyRow() {
            DataRow row = this.NewUninitializedRow();

            foreach( DataColumn c in this.Columns ) {
                if (!XmlToDatasetMap.IsMappedColumn(c)) {
                    if (!c.AutoIncrement) {
                        if (c.AllowDBNull) {
                            row[c] = DBNull.Value;
                        }
                        else if(c.DefaultValue!=null){
                            row[c] = c.DefaultValue;
                        }
                    }
                    else {
                        c.Init(row.tempRecord);
                    }
                }
            }
            return row;
        }

        private void NewRowCreated(DataRow row) {
            if (null != onTableNewRowDelegate) {
                DataTableNewRowEventArgs eventArg =  new DataTableNewRowEventArgs(row);
                OnTableNewRow(eventArg);
            }
        }

        internal DataRow NewRow(int record) {
            if (-1 == record) {
                record = NewRecord(-1);
            }

            rowBuilder._record = record;
            DataRow row = NewRowFromBuilder( rowBuilder );
            recordManager[record] = row;

            if (dataSet != null)
                DataSet.OnDataRowCreated( row );

            return row;
        }

        // This is what a subclassed dataSet overrides to create a new row.
        protected virtual DataRow NewRowFromBuilder(DataRowBuilder builder) {
            return new DataRow(builder);
        }

        /// <devdoc>
        ///    <para>Gets the row type.</para>
        /// </devdoc>
        protected virtual Type GetRowType() {
            return typeof(DataRow);
        }

        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)] 
        protected internal DataRow[] NewRowArray(int size) {
            if (IsTypedDataTable) {
                if (0 == size) {
                    if (null == EmptyDataRowArray) {
                        EmptyDataRowArray = (DataRow[]) Array.CreateInstance(GetRowType(), 0);
                    }
                    return EmptyDataRowArray;
                }
                return (DataRow[]) Array.CreateInstance(GetRowType(), size);
            }
            else {
                return ((0 == size) ? DataTable.zeroRows : new DataRow[size]);
            }
        }

        internal bool NeedColumnChangeEvents {
            get {
                return (IsTypedDataTable || (null != onColumnChangingDelegate) || (null != onColumnChangedDelegate));
            }
        }

        protected internal virtual void OnColumnChanging(DataColumnChangeEventArgs e) {
            // intentionally allow exceptions to bubble up.  We haven't committed anything yet.
            Debug.Assert(e != null, "e should not be null");
            if (onColumnChangingDelegate != null) {
                Bid.Trace("<ds.DataTable.OnColumnChanging|INFO> %d#\n", ObjectID);
                onColumnChangingDelegate(this, e);
            }
        }

        protected internal virtual void OnColumnChanged(DataColumnChangeEventArgs e) {
            Debug.Assert(e != null, "e should not be null");
            if (onColumnChangedDelegate != null) {
                Bid.Trace("<ds.DataTable.OnColumnChanged|INFO> %d#\n", ObjectID);
                onColumnChangedDelegate(this, e);
            }
        }

        protected virtual void OnPropertyChanging(PropertyChangedEventArgs pcevent) {
            if (onPropertyChangingDelegate != null) {
                Bid.Trace("<ds.DataTable.OnPropertyChanging|INFO> %d#\n", ObjectID);
                onPropertyChangingDelegate(this, pcevent);
            }
        }

        internal void OnRemoveColumnInternal(DataColumn column) {
            OnRemoveColumn(column);
        }

        /// <devdoc>
        /// <para>Notifies the <see cref='System.Data.DataTable'/> that a <see cref='System.Data.DataColumn'/> is
        ///    being removed.</para>
        /// </devdoc>
        protected virtual void OnRemoveColumn(DataColumn column) {
        }

        private DataRowChangeEventArgs OnRowChanged(DataRowChangeEventArgs args, DataRow eRow, DataRowAction eAction) {
            if ((null != onRowChangedDelegate) || IsTypedDataTable) {
                if (null == args) {
                    args = new DataRowChangeEventArgs(eRow, eAction);
                }
                OnRowChanged(args);
            }
            return args;
        }

        private DataRowChangeEventArgs OnRowChanging(DataRowChangeEventArgs args, DataRow eRow, DataRowAction eAction) {
            if ((null != onRowChangingDelegate) || IsTypedDataTable) {
                if (null == args) {
                    args = new DataRowChangeEventArgs(eRow, eAction);
                }
                OnRowChanging(args);
            }
            return args;
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.Data.DataTable.RowChanged'/> event.
        ///    </para>
        /// </devdoc>
        protected virtual void OnRowChanged(DataRowChangeEventArgs e) {
            Debug.Assert((null != e) && ((null != onRowChangedDelegate) || IsTypedDataTable), "OnRowChanged arguments");
            if (onRowChangedDelegate != null) {
                Bid.Trace("<ds.DataTable.OnRowChanged|INFO> %d#\n", ObjectID);
                onRowChangedDelegate(this, e);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.Data.DataTable.RowChanging'/> event.
        ///    </para>
        /// </devdoc>
        protected virtual void OnRowChanging(DataRowChangeEventArgs e) {
            Debug.Assert((null != e) && ((null != onRowChangingDelegate) || IsTypedDataTable), "OnRowChanging arguments");
            if (onRowChangingDelegate != null) {
                Bid.Trace("<ds.DataTable.OnRowChanging|INFO> %d#\n", ObjectID);
                onRowChangingDelegate(this, e);
           }
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.Data.DataTable.OnRowDeleting'/> event.
        ///    </para>
        /// </devdoc>
        protected virtual void OnRowDeleting(DataRowChangeEventArgs e) {
            Debug.Assert((null != e) && ((null != onRowDeletingDelegate) || IsTypedDataTable), "OnRowDeleting arguments");
            if (onRowDeletingDelegate != null) {
                Bid.Trace("<ds.DataTable.OnRowDeleting|INFO> %d#\n", ObjectID);
                onRowDeletingDelegate(this, e);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='System.Data.DataTable.OnRowDeleted'/> event.
        ///    </para>
        /// </devdoc>
        protected virtual void OnRowDeleted(DataRowChangeEventArgs e) {
            Debug.Assert((null != e) && ((null != onRowDeletedDelegate) || IsTypedDataTable), "OnRowDeleted arguments");
            if (onRowDeletedDelegate != null) {
                Bid.Trace("<ds.DataTable.OnRowDeleted|INFO> %d#\n", ObjectID);
                onRowDeletedDelegate(this, e);
            }
        }

        protected virtual void OnTableCleared(DataTableClearEventArgs e) {
            if (onTableClearedDelegate != null) {
                Bid.Trace("<ds.DataTable.OnTableCleared|INFO> %d#\n", ObjectID);
                onTableClearedDelegate(this, e);
            }
        }

        protected virtual void OnTableClearing(DataTableClearEventArgs e) {
            if (onTableClearingDelegate != null) {
                Bid.Trace("<ds.DataTable.OnTableClearing|INFO> %d#\n", ObjectID);
                onTableClearingDelegate(this, e);
            }
        }

        protected virtual void OnTableNewRow(DataTableNewRowEventArgs  e) {
            if (onTableNewRowDelegate != null) {
                Bid.Trace("<ds.DataTable.OnTableNewRow|INFO> %d#\n", ObjectID);
                onTableNewRowDelegate(this, e);
            }
        }

        private void OnInitialized() {
            if (onInitialized != null) {
                Bid.Trace("<ds.DataTable.OnInitialized|INFO> %d#\n", ObjectID);
                onInitialized(this, EventArgs.Empty);
            }
        }


        internal IndexField[] ParseSortString(string sortString) {
            IndexField[] indexDesc = zeroIndexField;
            if ((null != sortString) && (0 < sortString.Length)) {
                string[] split = sortString.Split(new char[] { ','});
                indexDesc = new IndexField[split.Length];

                for (int i = 0; i < split.Length; i++) {
                    string current = split[i].Trim();

                    // handle ASC and DESC.
                    int length = current.Length;
                    bool descending = false;
                    if (length >= 5 && String.Compare(current, length - 4, " ASC", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) {
                        current = current.Substring(0, length - 4).Trim();
                    }
                    else if (length >= 6 && String.Compare(current, length - 5, " DESC", 0, 5, StringComparison.OrdinalIgnoreCase) == 0) {
                        descending = true;
                        current = current.Substring(0, length - 5).Trim();
                    }

                    // handle brackets.
                    if (current.StartsWith("[", StringComparison.Ordinal)) {
                        if (current.EndsWith("]", StringComparison.Ordinal)) {
                            current = current.Substring(1, current.Length - 2);
                        }
                        else {
                            throw ExceptionBuilder.InvalidSortString(split[i]);
                        }
                    }

                    // find the column.
                    DataColumn column = Columns[current];
                    if(column == null) {
                        throw ExceptionBuilder.ColumnOutOfRange(current);
                    }
                    indexDesc[i] = new IndexField(column, descending);
                }
            }
            return indexDesc;
        }

        internal void RaisePropertyChanging(string name) {
            OnPropertyChanging(new PropertyChangedEventArgs(name));
        }

        // Notify all indexes that record changed.
        // Only called when Error was changed.
        internal void RecordChanged(int record) {
            Debug.Assert (record != -1, "Record number must be given");
            SetShadowIndexes(); // how about new assert?
            try {
                int numIndexes = shadowIndexes.Count;
                for (int i = 0; i < numIndexes; i++) {
                    Index ndx = shadowIndexes[i];// shadowindexes may change, see ShadowIndexCopy()
                    if (0 < ndx.RefCount) {
                        ndx.RecordChanged(record);
                    }
                }
            }
            finally{
                RestoreShadowIndexes();
            }
        }

// for each index in liveindexes invok RecordChanged
// oldIndex and newIndex keeps  position of record before delete and after insert in each index in order
// LiveIndexes[n-m] will have its information in oldIndex[n-m] and  newIndex[n-m]
        internal void RecordChanged(int[] oldIndex, int[] newIndex) {
            SetShadowIndexes();
            Debug.Assert (oldIndex.Length == newIndex.Length,  "Size oldIndexes and newIndexes should be the same");
            Debug.Assert (oldIndex.Length == shadowIndexes.Count, "Size of OldIndexes should be the same as size of Live indexes");
            try{
                int numIndexes = shadowIndexes.Count;
                for (int i = 0; i < numIndexes; i++) {
                    Index ndx = shadowIndexes[i];// shadowindexes may change, see ShadowIndexCopy()
                    if (0 < ndx.RefCount) {
                        ndx.RecordChanged(oldIndex[i], newIndex[i]);
                    }
                }

            }
            finally{
                RestoreShadowIndexes();
            }
        }

        internal void RecordStateChanged(int record, DataViewRowState oldState, DataViewRowState newState) {
            SetShadowIndexes();
            try{
                int numIndexes = shadowIndexes.Count;
                for (int i = 0; i < numIndexes; i++) {
                    Index ndx = shadowIndexes[i];// shadowindexes may change, see ShadowIndexCopy()
                    if (0 < ndx.RefCount) {
                        ndx.RecordStateChanged(record, oldState, newState);
                    }
                }
            }
            finally{
                RestoreShadowIndexes();
            }
            // System.Data.XML.Store.Store.OnROMChanged(record, oldState, newState);
        }


        internal void RecordStateChanged(int record1, DataViewRowState oldState1, DataViewRowState newState1,
                                         int record2, DataViewRowState oldState2, DataViewRowState newState2) {
            SetShadowIndexes();
            try{
                int numIndexes = shadowIndexes.Count;
                for (int i = 0; i < numIndexes; i++) {
                    Index ndx = shadowIndexes[i];// shadowindexes may change, see ShadowIndexCopy()
                    if (0 < ndx.RefCount) {
                        if (record1 != -1 && record2 != -1)
                            ndx.RecordStateChanged(record1, oldState1, newState1,
                                                   record2, oldState2, newState2);
                        else if (record1 != -1)
                            ndx.RecordStateChanged(record1, oldState1, newState1);
                        else if (record2 != -1)
                            ndx.RecordStateChanged(record2, oldState2, newState2);
                    }
                }
            }
            finally {
                RestoreShadowIndexes();
            }
            // System.Data.XML.Store.Store.OnROMChanged(record1, oldState1, newState1, record2, oldState2, newState2);
        }


// RemoveRecordFromIndexes removes the given record (using row and version) from all indexes and it  stores and returns the position of deleted
// record from each index
// IT SHOULD NOT CAUSE ANY EVENT TO BE FIRED
        internal int[] RemoveRecordFromIndexes(DataRow row, DataRowVersion  version) {
            int    indexCount          =  LiveIndexes.Count;
            int [] positionIndexes =  new int[indexCount];

            int recordNo = row.GetRecordFromVersion(version);
            DataViewRowState states = row.GetRecordState(recordNo);

            while (--indexCount >= 0) {
                if (row.HasVersion(version) && ((states & indexes[indexCount].RecordStates) != DataViewRowState.None)) {
                    int index = indexes[indexCount].GetIndex(recordNo);
                    if (index > -1) {
                        positionIndexes [indexCount] = index;
                        indexes[indexCount].DeleteRecordFromIndex(index); // this will delete the record from index and MUSt not fire event
                    }
                    else {
                        positionIndexes [indexCount] = -1; // this means record was not in index
                    }
                }
                else {
                    positionIndexes [indexCount] = -1; // this means record was not in index
                }
            }
            return positionIndexes;
        }

// InsertRecordToIndexes inserts the given record (using row and version) to all indexes and it  stores and returns the position of inserted
// record to each index
// IT SHOULD NOT CAUSE ANY EVENT TO BE FIRED
        internal int[] InsertRecordToIndexes(DataRow row, DataRowVersion  version) {
            int    indexCount          =  LiveIndexes.Count;
            int [] positionIndexes =  new int[indexCount];

            int recordNo = row.GetRecordFromVersion(version);
            DataViewRowState states = row.GetRecordState(recordNo);

            while (--indexCount >= 0) {
                if (row.HasVersion(version)) {
                    if ((states & indexes[indexCount].RecordStates) != DataViewRowState.None) {
                        positionIndexes [indexCount] = indexes[indexCount].InsertRecordToIndex(recordNo);
                    }
                    else {
                        positionIndexes [indexCount] = -1;
                    }
                }
            }
            return positionIndexes;
        }

        internal void SilentlySetValue(DataRow dr, DataColumn dc, DataRowVersion version, object newValue) {
            // get record for version
            int record = dr.GetRecordFromVersion(version);

            bool equalValues = false;
            if (DataStorage.IsTypeCustomType(dc.DataType) && newValue != dc[record]) {
                // if UDT storage, need to check if reference changed. See bug 385182
                equalValues = false;
            }
            else {
                equalValues = dc.CompareValueTo(record, newValue, true);
            }

            // if expression has changed
            if (!equalValues) {
                int[] oldIndex = dr.Table.RemoveRecordFromIndexes(dr, version);// conditional, if it exists it will try to remove with no event fired
                dc.SetValue(record, newValue);
                int[] newIndex = dr.Table.InsertRecordToIndexes(dr, version);// conditional, it will insert if it qualifies, no event will be fired
                if (dr.HasVersion(version)) {
                    if (version != DataRowVersion.Original) {
                        dr.Table.RecordChanged(oldIndex, newIndex);
                    }
                    if (dc.dependentColumns != null) {
                        //BugBug - passing in null for cachedRows.  This means expression columns as keys does not work when key changes.
                        dc.Table.EvaluateDependentExpressions(dc.dependentColumns, dr, version, null);
                    }
                }
             }
             dr.ResetLastChangedColumn();
        }

        /// <devdoc>
        ///    <para>Rolls back all changes that have been made to the table
        ///       since it was loaded, or the last time <see cref='System.Data.DataTable.AcceptChanges'/> was called.</para>
        /// </devdoc>
        public void RejectChanges() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.RejectChanges|API> %d#\n", ObjectID);
            try{
                DataRow[] oldRows = new DataRow[Rows.Count];
                Rows.CopyTo(oldRows, 0);

                for (int i = 0; i < oldRows.Length; i++) {
                    RollbackRow(oldRows[i]);
                }
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }

        }

        internal void RemoveRow(DataRow row, bool check) {
            if (row.rowID == -1) {
                throw ExceptionBuilder.RowAlreadyRemoved();
            }

            if (check && dataSet != null) {
                for (ParentForeignKeyConstraintEnumerator constraints = new ParentForeignKeyConstraintEnumerator(dataSet, this); constraints.GetNext();) {
                    constraints.GetForeignKeyConstraint().CheckCanRemoveParentRow(row);
                }
            }

            int oldRecord = row.oldRecord;
            int newRecord = row.newRecord;

            DataViewRowState oldRecordStatePre = row.GetRecordState(oldRecord);
            DataViewRowState newRecordStatePre = row.GetRecordState(newRecord);

            row.oldRecord = -1;
            row.newRecord = -1;

            if (oldRecord == newRecord) {
                oldRecord = -1;
            }

            RecordStateChanged(oldRecord, oldRecordStatePre, DataViewRowState.None,
                               newRecord, newRecordStatePre, DataViewRowState.None);

            FreeRecord(ref oldRecord);
            FreeRecord(ref newRecord);

            row.rowID = -1;
            Rows.ArrayRemove(row);
        }

        // Resets the table back to its original state.
        public virtual void Reset() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.Reset|API> %d#\n", ObjectID);
            try {
                Clear();
                ResetConstraints();

                DataRelationCollection dr = this.ParentRelations;
                int count = dr.Count;
                  while (count > 0) {
                   count--;
                   dr.RemoveAt(count);
                }

                dr = this.ChildRelations;
                count = dr.Count;
                  while (count > 0) {
                   count--;
                   dr.RemoveAt(count);
                }

                Columns.Clear();
                indexes.Clear();
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal void ResetIndexes() {
            ResetInternalIndexes(null);
        }

        internal void ResetInternalIndexes(DataColumn column) {
            Debug.Assert(null != indexes, "unexpected null indexes");
            SetShadowIndexes();
            try{
                // the length of shadowIndexes will not change
                // but the array instance may change during
                // events during Index.Reset
                int numIndexes = shadowIndexes.Count;
                for (int i = 0; i < numIndexes; i++) {
                    Index ndx = shadowIndexes[i];// shadowindexes may change, see ShadowIndexCopy()
                    if (0 < ndx.RefCount) {
                        if (null == column) {
                            ndx.Reset();
                        }
                        else {
                            // SQLBU 501916: DataTable internal index is corrupted:'5'
                            bool found = false;
                            foreach(IndexField field in ndx.IndexFields) {
                                if (Object.ReferenceEquals(column, field.Column)) {
                                    found = true;
                                    break;
                                    
                                }
                            }
                            if (found) {
                                ndx.Reset();
                            }
                        }
                    }
                }
            }
            finally {
                RestoreShadowIndexes();
            }
        }

        internal void RollbackRow(DataRow row) {
            row.CancelEdit();
            SetNewRecord(row, row.oldRecord, DataRowAction.Rollback, false, true);
        }

        private DataRowChangeEventArgs RaiseRowChanged(DataRowChangeEventArgs args, DataRow eRow, DataRowAction eAction) {
            try {
                if (UpdatingCurrent(eRow, eAction) && (IsTypedDataTable || (null != onRowChangedDelegate))) {
                    args = OnRowChanged(args, eRow, eAction);
                }
                // check if we deleting good row
                else if (DataRowAction.Delete == eAction && eRow.newRecord == -1 && (IsTypedDataTable || (null != onRowDeletedDelegate))) {
                    if (null == args) {
                        args = new DataRowChangeEventArgs(eRow, eAction);
                    }
                    OnRowDeleted(args);
                }
            }
            catch (Exception f) {
               // 
               if (!Common.ADP.IsCatchableExceptionType(f)) {
                 throw;
               }
               ExceptionBuilder.TraceExceptionWithoutRethrow(f);
               // ignore the exception
            }
            return args;
        }

        private DataRowChangeEventArgs RaiseRowChanging(DataRowChangeEventArgs args, DataRow eRow, DataRowAction eAction) {
            if (UpdatingCurrent(eRow, eAction) && (IsTypedDataTable || (null != onRowChangingDelegate))) {
                eRow.inChangingEvent = true;

                // don't catch
                try {
                    args = OnRowChanging(args, eRow, eAction);
                }
                finally {
                    eRow.inChangingEvent = false;
                }
            }
            // check if we deleting good row
            else if (DataRowAction.Delete == eAction && eRow.newRecord != -1 && (IsTypedDataTable || (null != onRowDeletingDelegate))) {
                eRow.inDeletingEvent = true;
                // don't catch
                try {
                    if (null == args) {
                        args = new DataRowChangeEventArgs(eRow, eAction);
                    }
                    OnRowDeleting(args);
                }
                finally {
                    eRow.inDeletingEvent = false;
                }
            }
            return args;
        }

        private DataRowChangeEventArgs RaiseRowChanging(DataRowChangeEventArgs args, DataRow eRow, DataRowAction eAction, bool fireEvent) {

            // check all constraints
            if (EnforceConstraints && !inLoad ) {
                int columnCount = columnCollection.Count;
                for(int i = 0; i < columnCount; ++i) {
                    DataColumn column = columnCollection[i];
                    if (!column.Computed || eAction != DataRowAction.Add) {
                        column.CheckColumnConstraint(eRow, eAction);
                    }
                }

                int constraintCount = constraintCollection.Count;
                for(int i = 0; i < constraintCount; ++i) {
                    constraintCollection[i].CheckConstraint(eRow, eAction);
                }
            }

            // $$anandra.  Check this event out. May be an issue.
            if (fireEvent) {
                args = RaiseRowChanging(args, eRow, eAction);
            }

            if (!inDataLoad) {
                // cascade things...
                if (!MergingData && eAction != DataRowAction.Nothing && eAction != DataRowAction.ChangeOriginal) {
                    CascadeAll(eRow, eAction);
                }
            }
            return args;
        }

        /// <devdoc>
        /// <para>Returns an array of all <see cref='System.Data.DataRow'/> objects.</para>
        /// </devdoc>
        public DataRow[] Select() {
            Bid.Trace("<ds.DataTable.Select|API> %d#\n", ObjectID);
            return new Select(this, "", "", DataViewRowState.CurrentRows).SelectRows();
        }

        /// <devdoc>
        /// <para>Returns an array of all <see cref='System.Data.DataRow'/> objects that match the filter criteria in order of
        ///    primary key (or lacking one, order of addition.)</para>
        /// </devdoc>
        public DataRow[] Select(string filterExpression) {
            Bid.Trace("<ds.DataTable.Select|API> %d#, filterExpression='%ls'\n", ObjectID, filterExpression);
            return new Select(this, filterExpression, "", DataViewRowState.CurrentRows).SelectRows();
        }

        /// <devdoc>
        /// <para>Returns an array of all <see cref='System.Data.DataRow'/> objects that match the filter criteria, in the the
        ///    specified sort order.</para>
        /// </devdoc>
        public DataRow[] Select(string filterExpression, string sort) {
            Bid.Trace("<ds.DataTable.Select|API> %d#, filterExpression='%ls', sort='%ls'\n", ObjectID, filterExpression, sort);
            return new Select(this, filterExpression, sort, DataViewRowState.CurrentRows).SelectRows();
        }

        /// <devdoc>
        /// <para>Returns an array of all <see cref='System.Data.DataRow'/> objects that match the filter in the order of the
        ///    sort, that match the specified state.</para>
        /// </devdoc>
        public DataRow[] Select(string filterExpression, string sort, DataViewRowState recordStates) {
            Bid.Trace("<ds.DataTable.Select|API> %d#, filterExpression='%ls', sort='%ls', recordStates=%d{ds.DataViewRowState}\n", ObjectID, filterExpression, sort, (int)recordStates);
            return new Select(this, filterExpression, sort, recordStates).SelectRows();
        }

        internal void SetNewRecord(DataRow row, int proposedRecord, DataRowAction action = DataRowAction.Change, bool isInMerge = false, bool fireEvent = true, bool suppressEnsurePropertyChanged = false) {
            Exception deferredException = null;
            SetNewRecordWorker(row, proposedRecord, action, isInMerge, suppressEnsurePropertyChanged, -1, fireEvent, out deferredException); // we are going to call below overload from insert
            if (deferredException != null) {
                throw deferredException;
            }
        }

        private void SetNewRecordWorker(DataRow row, int proposedRecord, DataRowAction action, bool isInMerge, bool suppressEnsurePropertyChanged,
            int position, bool fireEvent, out Exception deferredException) {

            // this is the event workhorse... it will throw the changing/changed events
            // and update the indexes. Used by change, add, delete, revert.

            // order of execution is as follows
            //
            // 1) set temp record
            // 2) Check constraints for non-expression columns
            // 3) Raise RowChanging/RowDeleting with temp record
            // 4) set the new record in storage
            // 5) Update indexes with recordStateChanges - this will fire ListChanged & PropertyChanged events on associated views
            // 6) Evaluate all Expressions (exceptions are deferred)- this will fire ListChanged & PropertyChanged events on associated views
            // 7) Raise RowChanged/ RowDeleted
            // 8) Check constraints for expression columns

            Debug.Assert(row != null, "Row can't be null.");
            deferredException = null;
            
            if (row.tempRecord != proposedRecord) {
                // $HACK: for performance reasons, EndUpdate calls SetNewRecord with tempRecord == proposedRecord
                if (!inDataLoad) {
                    row.CheckInTable();
                    CheckNotModifying(row);
                }
                if (proposedRecord == row.newRecord) {
                    if (isInMerge) {
                        Debug.Assert(fireEvent, "SetNewRecord is called with wrong parameter");
                        RaiseRowChanged(null, row, action);
                    }
                    return;
                }

                Debug.Assert(!row.inChangingEvent, "How can this row be in an infinite loop?");

                row.tempRecord = proposedRecord;
            }
            DataRowChangeEventArgs drcevent = null;

            try {
                row._action = action;
                drcevent = RaiseRowChanging(null, row, action, fireEvent);
            }
            catch {
                row.tempRecord = -1;
                throw;
            }
            finally {
                row._action = DataRowAction.Nothing;
            }

            row.tempRecord = -1;

            int currentRecord = row.newRecord;

            // if we're deleting, then the oldRecord value will change, so need to track that if it's distinct from the newRecord.
            int secondRecord = (proposedRecord != -1 ?
                                proposedRecord :
                                (row.RowState != DataRowState.Unchanged ?
                                 row.oldRecord :
                                 -1));

            if (action == DataRowAction.Add) { //if we come here from insert we do insert the row to collection
                if (position == -1)
                    Rows.ArrayAdd(row);
                else
                    Rows.ArrayInsert(row, position);
            }

            List<DataRow> cachedRows = null;
            if ((action == DataRowAction.Delete || action == DataRowAction.Change)
                && dependentColumns != null && dependentColumns.Count > 0) {
                // if there are expression columns, need to cache related rows for deletes and updates (key changes)
                // before indexes are modified.
                cachedRows = new List<DataRow>();
                for (int j = 0; j < ParentRelations.Count; j++) {
                    DataRelation relation = ParentRelations[j];
                    if (relation.ChildTable != row.Table) {
                        continue;
                    }
                    cachedRows.InsertRange(cachedRows.Count, row.GetParentRows(relation));
                }

                for (int j = 0; j < ChildRelations.Count; j++) {
                    DataRelation relation = ChildRelations[j];
                    if (relation.ParentTable != row.Table) {
                        continue;
                    }
                    cachedRows.InsertRange(cachedRows.Count, row.GetChildRows(relation));
                }
            }

            // Dev10 Bug 688779: DataRowView.PropertyChanged are not raised on RejectChanges
            // if the newRecord is changing, the propertychanged event should be allowed to triggered for ListChangedType.Changed or .Moved
            // unless the specific condition is known that no data has changed, like DataRow.SetModified()
            if (!suppressEnsurePropertyChanged && !row.HasPropertyChanged && (row.newRecord != proposedRecord)
                && (-1 != proposedRecord) // explictly not fixing Dev10 Bug 692044: DataRowView.PropertyChanged are not raised on DataTable.Delete when mixing current and original records in RowStateFilter
                && (-1 != row.newRecord)) // explictly not fixing parts of Dev10 Bug 697909: when mixing current and original records in RowStateFilter
            {
                // DataRow will believe multiple edits occured and
                // DataView.ListChanged event w/ ListChangedType.ItemChanged will raise DataRowView.PropertyChanged event and
                // PropertyChangedEventArgs.PropertyName will now be empty string so
                // WPF will refresh the entire row
                row.LastChangedColumn = null;
                row.LastChangedColumn = null;
            }

                // Check whether we need to update indexes
                if (LiveIndexes.Count != 0) {

                    // Dev10 bug #463087: DataTable internal index is currupted: '5'
                    if ((-1 == currentRecord) && (-1 != proposedRecord) && (-1 != row.oldRecord) && (proposedRecord != row.oldRecord)) {
                        // the transition from DataRowState.Deleted -> DataRowState.Modified
                        // with same orginal record but new current record
                        // needs to raise an ItemChanged or ItemMoved instead of ItemAdded in the ListChanged event.
                        // for indexes/views listening for both DataViewRowState.Deleted | DataViewRowState.ModifiedCurrent
                        currentRecord = row.oldRecord;
                    }

                    DataViewRowState currentRecordStatePre = row.GetRecordState(currentRecord);
                    DataViewRowState secondRecordStatePre = row.GetRecordState(secondRecord);

                    row.newRecord = proposedRecord;
                    if (proposedRecord != -1)
                        this.recordManager[proposedRecord] = row;

                    DataViewRowState currentRecordStatePost = row.GetRecordState(currentRecord);
                    DataViewRowState secondRecordStatePost = row.GetRecordState(secondRecord);

                    // may raise DataView.ListChanged event
                    RecordStateChanged(currentRecord, currentRecordStatePre, currentRecordStatePost,
                        secondRecord, secondRecordStatePre, secondRecordStatePost);
                }
                else {
                    row.newRecord = proposedRecord;
                    if (proposedRecord != -1)
                        this.recordManager[proposedRecord] = row;
                }

                // Dev10 Bug 461199 - reset the last changed column here, after all
                // DataViews have raised their DataRowView.PropertyChanged event
                row.ResetLastChangedColumn();

                // SQLBU 278737: Record manager corruption when reentrant write operations
                // free the 'currentRecord' only after all the indexes have been updated.
                // Corruption! { if (currentRecord != row.oldRecord) { FreeRecord(ref currentRecord); } }
                // RecordStateChanged raises ListChanged event at which time user may do work
                if (-1 != currentRecord) {
                    if (currentRecord != row.oldRecord)
                    {
                        if ((currentRecord != row.tempRecord) &&   // Delete, AcceptChanges, BeginEdit
                            (currentRecord != row.newRecord) &&    // RejectChanges & SetAdded
                            (row == recordManager[currentRecord])) // AcceptChanges, NewRow
                        {
                            FreeRecord(ref currentRecord);
                        }
                    }
                }

            if (row.RowState == DataRowState.Detached && row.rowID != -1) {
                RemoveRow(row, false);
            }

            if (dependentColumns != null && dependentColumns.Count > 0) {
                try {
                    EvaluateExpressions(row, action, cachedRows);
                }
                catch (Exception exc) {
                    // For DataRows being added, throwing of exception from expression evaluation is
                    // deferred until after the row has been completely added.
                    if (action != DataRowAction.Add) {
                        throw exc;
                    }
                    else {
                        deferredException = exc;
                    }
                }
            }

            try {
                if (fireEvent) {
                    RaiseRowChanged(drcevent, row, action);
                }
            }
            catch (Exception e) {
                // 
                if (!Common.ADP.IsCatchableExceptionType(e)) {
                    throw;
                }
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                // ignore the exception
            }
        }

        // this is the event workhorse... it will throw the changing/changed events
        // and update the indexes.
        internal void SetOldRecord(DataRow row, int proposedRecord) {
            if (!inDataLoad) {
                row.CheckInTable();
                CheckNotModifying(row);
            }

            if (proposedRecord == row.oldRecord) {
                return;
            }

            int originalRecord = row.oldRecord; // cache old record after potential RowChanging event
            try {
                // Check whether we need to update indexes
                if (LiveIndexes.Count != 0) {

                    // Dev10 bug #463087: DataTable internal index is currupted: '5'
                    if ((-1 == originalRecord) && (-1 != proposedRecord) && (-1 != row.newRecord) && (proposedRecord != row.newRecord)) {
                        // the transition from DataRowState.Added -> DataRowState.Modified
                        // with same current record but new original record
                        // needs to raise an ItemChanged or ItemMoved instead of ItemAdded in the ListChanged event.
                        // for indexes/views listening for both DataViewRowState.Added | DataViewRowState.ModifiedOriginal
                        originalRecord = row.newRecord;
                    }

                    DataViewRowState originalRecordStatePre = row.GetRecordState(originalRecord);
                    DataViewRowState proposedRecordStatePre = row.GetRecordState(proposedRecord);

                    row.oldRecord = proposedRecord;
                    if (proposedRecord != -1)
                        this.recordManager[proposedRecord] = row;

                    DataViewRowState originalRecordStatePost = row.GetRecordState(originalRecord);
                    DataViewRowState proposedRecordStatePost = row.GetRecordState(proposedRecord);

                    RecordStateChanged(originalRecord, originalRecordStatePre, originalRecordStatePost,
                                       proposedRecord, proposedRecordStatePre, proposedRecordStatePost);
                }
                else {
                    row.oldRecord = proposedRecord;
                    if (proposedRecord != -1)
                        this.recordManager[proposedRecord] = row;
                }
            }
            finally {
                if ((originalRecord != -1) && (originalRecord != row.tempRecord) &&
                    (originalRecord != row.oldRecord) && (originalRecord != row.newRecord)) {

                    FreeRecord(ref originalRecord);
                }
                // else during an event 'row.AcceptChanges(); row.BeginEdit(); row.EndEdit();'

                if (row.RowState == DataRowState.Detached && row.rowID != -1) {
                    RemoveRow(row, false);
                }
            }
        }

        private void RestoreShadowIndexes() {
            Debug.Assert(1 <= shadowCount, "unexpected negative shadow count");
            shadowCount--;
            if (0 == shadowCount) {
                shadowIndexes = null;
            }
        }

        private void SetShadowIndexes() {
            if (null == shadowIndexes) {
                Debug.Assert(0 == shadowCount, "unexpected count");
                shadowIndexes = LiveIndexes;
                shadowCount = 1;
            }
            else {
                Debug.Assert(1 <= shadowCount, "unexpected negative shadow count");
                shadowCount++;
            }
        }

        internal void ShadowIndexCopy(){
            if (shadowIndexes == indexes) {
                Debug.Assert(0 < indexes.Count, "unexpected");
                shadowIndexes = new List<Index>(indexes);
            }
        }

        /// <devdoc>
        /// <para>Returns the <see cref='System.Data.DataTable.TableName'/> and <see cref='System.Data.DataTable.DisplayExpression'/>, if there is one as a concatenated string.</para>
        /// </devdoc>
        public override string ToString() {
            if (this.displayExpression == null)
                return this.TableName;
            else
                return this.TableName + " + " + this.DisplayExpressionInternal;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void BeginLoadData() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.BeginLoadData|API> %d#\n", ObjectID);
            try {
                if (inDataLoad)
                    return;

                inDataLoad = true;
                Debug.Assert(null == loadIndex, "loadIndex should already be null");
                loadIndex  = null;
                // LoadDataRow may have been called before BeginLoadData and already
                // initialized loadIndexwithOriginalAdded & loadIndexwithCurrentDeleted 

                initialLoad = (Rows.Count == 0);
                if(initialLoad) {
                    SuspendIndexEvents();
                } else {
                    if (primaryKey != null) {
                        loadIndex = primaryKey.Key.GetSortIndex(DataViewRowState.OriginalRows);
                    }
                    if(loadIndex != null) {
                        loadIndex.AddRef();
                    }
                }

                if (DataSet != null) {
                    savedEnforceConstraints = DataSet.EnforceConstraints;
                    DataSet.EnforceConstraints = false;
                }
                else {
                    this.EnforceConstraints = false;
                }
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void EndLoadData() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.EndLoadData|API> %d#\n", ObjectID);
            try {
                if (!inDataLoad)
                    return;

                if(loadIndex != null) {
                    loadIndex.RemoveRef();
                }
                if (loadIndexwithOriginalAdded  != null) {
                    loadIndexwithOriginalAdded.RemoveRef();
                }
                if (loadIndexwithCurrentDeleted  != null) {
                    loadIndexwithCurrentDeleted.RemoveRef();
                }

                loadIndex  = null;
                loadIndexwithOriginalAdded = null;
                loadIndexwithCurrentDeleted = null;

                inDataLoad = false;

                RestoreIndexEvents(false);

                if (DataSet != null)
                    DataSet.EnforceConstraints = savedEnforceConstraints;
                else
                    this.EnforceConstraints = true;
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>Finds and updates a specific row. If no matching
        ///       row is found, a new row is created using the given values.</para>
        /// </devdoc>
        public DataRow LoadDataRow(object[] values, bool fAcceptChanges) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.LoadDataRow|API> %d#, fAcceptChanges=%d{bool}\n", ObjectID, fAcceptChanges);
            try {
                DataRow row;
                if (inDataLoad) {
                    int record = NewRecordFromArray(values);
                    if (loadIndex != null) {
                        // not expecting LiveIndexes to clear the index we use between calls to LoadDataRow
                        Debug.Assert(2 <= loadIndex.RefCount, "bad loadIndex.RefCount");

                        int result = loadIndex.FindRecord(record);
                        if (result != -1) {
                            int resultRecord = loadIndex.GetRecord(result);
                            row = recordManager[resultRecord];
                            Debug.Assert (row != null, "Row can't be null for index record");
                            row.CancelEdit();
                            if (row.RowState == DataRowState.Deleted)
                                SetNewRecord(row, row.oldRecord, DataRowAction.Rollback, false, true);
                            SetNewRecord(row, record, DataRowAction.Change, false, true);
                            if (fAcceptChanges)
                                row.AcceptChanges();
                            return row;
                        }
                    }
                    row = NewRow(record);
                    AddRow(row);
                    if (fAcceptChanges)
                        row.AcceptChanges();
                    return row;
                }
                else {
                    // In case, BeginDataLoad is not called yet
                    row = UpdatingAdd(values);
                    if (fAcceptChanges)
                        row.AcceptChanges();
                    return row;
                }
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>Finds and updates a specific row. If no matching
        ///       row is found, a new row is created using the given values.</para>
        /// </devdoc>
        public DataRow LoadDataRow(object[] values, LoadOption loadOption) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.LoadDataRow|API> %d#, loadOption=%d{ds.LoadOption}\n", ObjectID,  (int)loadOption);
            try {
                Index indextoUse = null;
                if (this.primaryKey != null) {
                    if (loadOption == LoadOption.Upsert) { // CurrentVersion, and Deleted
                        if (loadIndexwithCurrentDeleted == null) {
                            loadIndexwithCurrentDeleted = this.primaryKey.Key.GetSortIndex(DataViewRowState.CurrentRows |DataViewRowState.Deleted);
                            Debug.Assert(loadIndexwithCurrentDeleted != null, "loadIndexwithCurrentDeleted should not be null" );
                            if (loadIndexwithCurrentDeleted != null) {
                                loadIndexwithCurrentDeleted.AddRef();
                            }
                        }
                        indextoUse = loadIndexwithCurrentDeleted;
                    }
                    else {// CurrentVersion, and Deleted : OverwriteRow, PreserveCurrentValues
                        if (loadIndexwithOriginalAdded == null) {
                            loadIndexwithOriginalAdded  = this.primaryKey.Key.GetSortIndex(DataViewRowState.OriginalRows |DataViewRowState.Added);
                            Debug.Assert(loadIndexwithOriginalAdded != null, "loadIndexwithOriginalAdded should not be null");
                            if (loadIndexwithOriginalAdded != null) {
                                loadIndexwithOriginalAdded.AddRef();
                            }
                        }
                        indextoUse = loadIndexwithOriginalAdded;
                    }
                    // not expecting LiveIndexes to clear the index we use between calls to LoadDataRow
                    Debug.Assert(2 <= indextoUse.RefCount, "bad indextoUse.RefCount");
                }
                if(inDataLoad && !AreIndexEventsSuspended) { // we do not want to fire any listchanged in new Load/Fill
                    SuspendIndexEvents();// so suspend events here(not suspended == table already has some rows initially)
                }

                DataRow dataRow = LoadRow(values, loadOption, indextoUse);// if indextoUse == null, it means we dont have PK,
                                                                          // so LoadRow will take care of just adding the row to end

                return dataRow;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal DataRow UpdatingAdd(object[] values) {
            Index index = null;
            if (this.primaryKey != null) {
                index = this.primaryKey.Key.GetSortIndex(DataViewRowState.OriginalRows);
            }

            if (index != null) {
                int record = NewRecordFromArray(values);
                int result = index.FindRecord(record);
                if (result != -1) {
                    int resultRecord = index.GetRecord(result);
                    DataRow row = this.recordManager[resultRecord];
                    Debug.Assert (row != null, "Row can't be null for index record");
                    row.RejectChanges();
                    this.SetNewRecord(row, record);
                    return row;
                }
                DataRow row2 = NewRow(record);
                Rows.Add(row2);
                return row2;
            }

            return Rows.Add(values);
        }

        internal bool UpdatingCurrent(DataRow row, DataRowAction action) {
            return(action == DataRowAction.Add || action == DataRowAction.Change ||
                   action == DataRowAction.Rollback || action == DataRowAction.ChangeOriginal ||
                   action == DataRowAction.ChangeCurrentAndOriginal);
//                (action == DataRowAction.Rollback && row.tempRecord != -1));
}

        internal DataColumn AddUniqueKey(int position) {
            if (_colUnique != null)
                return _colUnique;

            // check to see if we can use already existant PrimaryKey
            DataColumn[] pkey = PrimaryKey;
            if (pkey.Length == 1)
                // We have one-column primary key, so we can use it in our heirarchical relation
                return pkey[0];

            // add Unique, but not primaryKey to the table

            string keyName = XMLSchema.GenUniqueColumnName(TableName + "_Id", this);
            DataColumn key = new DataColumn(keyName, typeof(Int32), null, MappingType.Hidden);
            key.Prefix = tablePrefix;
            key.AutoIncrement = true;
            key.AllowDBNull = false;
            key.Unique = true;

            if (position == -1)
                Columns.Add(key);
            else { // we do have a problem and Imy idea is it is bug. Ask Enzo while Code review. Why we do not set ordinal when we call AddAt?
                for(int i = Columns.Count -1; i >= position; i--) {
                    this.Columns[i].SetOrdinalInternal(i+1);
                }
                Columns.AddAt(position, key);
                key.SetOrdinalInternal(position);
            }

            if (pkey.Length == 0)
                PrimaryKey = new DataColumn[] {
                    key
                };

            _colUnique = key;
            return _colUnique;
        }

        internal DataColumn AddUniqueKey() {
            return AddUniqueKey(-1);
        }

        internal DataColumn AddForeignKey(DataColumn parentKey) {
            Debug.Assert(parentKey != null, "AddForeignKey: Invalid paramter.. related primary key is null");

            string      keyName = XMLSchema.GenUniqueColumnName(parentKey.ColumnName, this);
            DataColumn  foreignKey = new DataColumn(keyName, parentKey.DataType, null, MappingType.Hidden);
            Columns.Add(foreignKey);

            return foreignKey;
        }

        internal void UpdatePropertyDescriptorCollectionCache() {
            propertyDescriptorCollectionCache = null;
        }

        /// <devdoc>
        ///     Retrieves an array of properties that the given component instance
        ///     provides.  This may differ from the set of properties the class
        ///     provides.  If the component is sited, the site may add or remove
        ///     additional properties.  The returned array of properties will be
        ///     filtered by the given set of attributes.
        /// </devdoc>
        internal PropertyDescriptorCollection GetPropertyDescriptorCollection(Attribute[] attributes) {
            if (propertyDescriptorCollectionCache == null) {
                int columnsCount   = Columns.Count;
                int relationsCount = ChildRelations.Count;
                PropertyDescriptor[] props = new PropertyDescriptor[columnsCount + relationsCount]; {
                    for (int i = 0; i < columnsCount; i++) {
                        props[i] = new DataColumnPropertyDescriptor(Columns[i]);
                    }
                    for (int i = 0; i < relationsCount; i++) {
                        props[columnsCount + i] = new DataRelationPropertyDescriptor(ChildRelations[i]);
                    }
                }
                propertyDescriptorCollectionCache = new PropertyDescriptorCollection(props);
            }
            return propertyDescriptorCollectionCache;
        }

        internal XmlQualifiedName TypeName {
            get {
                return ((typeName == null) ? XmlQualifiedName.Empty : (XmlQualifiedName)typeName);
            }
            set {
                typeName = value;
            }
        }

        public void Merge(DataTable table)
        {
            Merge(table, false, MissingSchemaAction.Add);
        }

        public void Merge(DataTable table, bool preserveChanges)
        {
            Merge(table, preserveChanges, MissingSchemaAction.Add);
        }

        public void Merge(DataTable table, bool preserveChanges, MissingSchemaAction missingSchemaAction)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.Merge|API> %d#, table=%d, preserveChanges=%d{bool}, missingSchemaAction=%d{ds.MissingSchemaAction}\n", ObjectID, (table != null) ? table.ObjectID : 0, preserveChanges, (int)missingSchemaAction);
            try{
                if (table == null)
                    throw ExceptionBuilder.ArgumentNull("table");

                switch(missingSchemaAction) { // @perfnote: Enum.IsDefined
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
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        public void Load (IDataReader reader){
            Load(reader, LoadOption.PreserveChanges, null);
        }

        public void Load (IDataReader reader, LoadOption loadOption) {
            Load(reader, loadOption, null);
        }

        public virtual void Load (IDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler){
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.Load|API> %d#, loadOption=%d{ds.LoadOption}\n", ObjectID, (int)loadOption);
            try {
                if (this.PrimaryKey.Length == 0) {
                    DataTableReader dtReader = reader as DataTableReader;
                    if (dtReader != null && dtReader.CurrentDataTable == this)
                        return; // if not return, it will go to infinite loop
                }
                Common.LoadAdapter adapter = new Common.LoadAdapter();
                adapter.FillLoadOption = loadOption;
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                if (null != errorHandler) {
                    adapter.FillError += errorHandler;
                }
                adapter.FillFromReader(new DataTable[] { this }, reader, 0, 0);

                if (!reader.IsClosed && !reader.NextResult()) { // 
                    reader.Close();
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private DataRow LoadRow(object[] values, LoadOption loadOption, Index searchIndex) {
            int recordNo;
            DataRow dataRow = null;

            if (searchIndex != null) {
                int[] primaryKeyIndex = new int[0];
                if (this.primaryKey != null) { // I do check above for PK, but in case if someone else gives me some index unrelated to PK
                    primaryKeyIndex = new int[this.primaryKey.ColumnsReference.Length];
                    for(int i = 0; i < this.primaryKey.ColumnsReference.Length; i++) {
                        primaryKeyIndex[i] = this.primaryKey.ColumnsReference[i].Ordinal;
                    }
                }

                object[] keys = new object[primaryKeyIndex.Length];
                for(int i = 0; i < primaryKeyIndex.Length; i++) {
                    keys[i] = values[primaryKeyIndex[i]];
                }

                Range result = searchIndex.FindRecords(keys);

                if (!result.IsNull) {
                    int deletedRowUpsertCount = 0;
                    for(int i = result.Min; i <= result.Max; i++) {
                        int resultRecord = searchIndex.GetRecord(i);
                        dataRow = this.recordManager[resultRecord];
                        recordNo = NewRecordFromArray(values);

                        //SQLBU DT 33648
                        // values array is being reused by DataAdapter, do not modify the values array
                        for(int count = 0; count < values.Length; count++) {
                            if (null == values[count]) {
                                columnCollection[count].Copy(resultRecord, recordNo);
                            }
                        }
                        for(int count = values.Length; count < columnCollection.Count ; count++) {
                            columnCollection[count].Copy(resultRecord, recordNo); // if there are missing values
                        }

                        if (loadOption != LoadOption.Upsert || dataRow.RowState != DataRowState.Deleted) {
                            SetDataRowWithLoadOption(dataRow , recordNo, loadOption, true);
                        }
                        else {
                            deletedRowUpsertCount++;
                        }
                    }
                    if (0 == deletedRowUpsertCount) {
                        return dataRow;
                    }
                }
            }

            recordNo = NewRecordFromArray(values);
            dataRow = NewRow(recordNo);
            // fire rowChanging event here
            DataRowAction action;
            DataRowChangeEventArgs drcevent = null;
            switch(loadOption) {
                case LoadOption.OverwriteChanges:
                case LoadOption.PreserveChanges:
                    action = DataRowAction.ChangeCurrentAndOriginal;
                    break;
                case LoadOption.Upsert:
                    action = DataRowAction.Add;
                    break;
                default:
                    throw ExceptionBuilder.ArgumentOutOfRange("LoadOption");
            }

            drcevent = RaiseRowChanging(null, dataRow, action);

            this.InsertRow (dataRow, -1, -1, false);
            switch(loadOption) {
                case LoadOption.OverwriteChanges:
                case LoadOption.PreserveChanges:
                    this.SetOldRecord(dataRow,  recordNo);
                    break;
                case LoadOption.Upsert:
                    break;
                default:
                    throw ExceptionBuilder.ArgumentOutOfRange("LoadOption");
            }
            RaiseRowChanged(drcevent, dataRow, action);

            return dataRow;
        }

        private void SetDataRowWithLoadOption (DataRow dataRow, int recordNo, LoadOption loadOption, bool checkReadOnly) {
            bool hasError = false;
            if (checkReadOnly) {
                foreach(DataColumn dc in this.Columns) {
                    if (dc.ReadOnly && !dc.Computed) {
                        switch(loadOption) {
                            case LoadOption.OverwriteChanges:
                                if ((dataRow[dc, DataRowVersion.Current] != dc[recordNo]) ||(dataRow[dc, DataRowVersion.Original] != dc[recordNo]))
                                    hasError = true;
                                break;
                            case LoadOption.Upsert:
                                if (dataRow[dc, DataRowVersion.Current] != dc[recordNo])
                                    hasError = true;
                                break;
                            case LoadOption.PreserveChanges:
                                if (dataRow[dc, DataRowVersion.Original] != dc[recordNo])
                                    hasError = true;
                                break;
                        }
                    }
                }
            } // No Event should be fired  in SenNewRecord and SetOldRecord
            // fire rowChanging event here

            DataRowChangeEventArgs drcevent = null;
            DataRowAction action = DataRowAction.Nothing;
            int cacheTempRecord = dataRow.tempRecord;
            dataRow.tempRecord = recordNo;

            switch(loadOption) {
                case LoadOption.OverwriteChanges:
                    action = DataRowAction.ChangeCurrentAndOriginal;
                    break;
                case LoadOption.Upsert:
                    switch(dataRow.RowState) {
                        case DataRowState.Unchanged:
                            // let see if the incomming value has the same values as existing row, so compare records
                            foreach(DataColumn dc in dataRow.Table.Columns) {
                                if (0 != dc.Compare(dataRow.newRecord, recordNo)) {
                                    action = DataRowAction.Change;
                                    break;
                                }
                            }
                            break;
                        case DataRowState.Deleted:
                            Debug.Assert(false, "LoadOption.Upsert with deleted row, should not be here");
                            break;
                        default :
                            action = DataRowAction.Change;
                            break;
                    }
                    break;
                case LoadOption.PreserveChanges:
                    switch(dataRow.RowState) {
                        case DataRowState.Unchanged:
                            action = DataRowAction.ChangeCurrentAndOriginal;
                            break;
                        default:
                            action = DataRowAction.ChangeOriginal;
                            break;
                    }
                    break;
                default:
                    throw ExceptionBuilder.ArgumentOutOfRange("LoadOption");
            }

            try {
                drcevent = RaiseRowChanging(null, dataRow, action);
                if (action == DataRowAction.Nothing) { // RaiseRowChanging does not fire for DataRowAction.Nothing
                    dataRow.inChangingEvent = true;
                    try {
                        drcevent = OnRowChanging(drcevent, dataRow, action);
                    }
                    finally {
                        dataRow.inChangingEvent = false;
                    }
                }
            }
            finally {
                Debug.Assert(dataRow.tempRecord == recordNo, "tempRecord has been changed in event handler");
                if (DataRowState.Detached == dataRow.RowState) {
                    // 'row.Table.Remove(row);'
                    if (-1 != cacheTempRecord) {
                        FreeRecord(ref cacheTempRecord);
                    }
                }
                else {
                    if (dataRow.tempRecord != recordNo) {
                        // 'row.EndEdit(); row.BeginEdit(); '
                        if (-1 != cacheTempRecord) {
                            FreeRecord(ref cacheTempRecord);
                        }
                        if (-1 != recordNo) {
                            FreeRecord(ref recordNo);
                        }
                        recordNo = dataRow.tempRecord;
                    }
                    else {
                        dataRow.tempRecord = cacheTempRecord;
                    }
                }
            }
            if (dataRow.tempRecord != -1) {
                dataRow.CancelEdit();
            }

            switch(loadOption) {
                case LoadOption.OverwriteChanges:
                     this.SetNewRecord(dataRow,  recordNo, DataRowAction.Change, false, false);
                     this.SetOldRecord(dataRow,  recordNo);
                     break;
                case LoadOption.Upsert:
                     if (dataRow.RowState == DataRowState.Unchanged) {
                         this.SetNewRecord(dataRow,  recordNo, DataRowAction.Change, false, false);
                         if (!dataRow.HasChanges()) {
                             this.SetOldRecord(dataRow, recordNo);
                         }
                     }
                     else {
                         if (dataRow.RowState == DataRowState.Deleted)
                             dataRow.RejectChanges();
                         this.SetNewRecord(dataRow,  recordNo, DataRowAction.Change, false, false);
                     }
                     break;
                case LoadOption.PreserveChanges:
                     if (dataRow.RowState == DataRowState.Unchanged) {
                         // SQLBU 500706: DataTable internal index is corrupted: '8'
                         // if ListChanged event deletes dataRow
                         this.SetOldRecord(dataRow,  recordNo); // do not fire event
                         this.SetNewRecord(dataRow, recordNo, DataRowAction.Change, false, false);
                     }
                     else { // if modified/ added / deleted we want this operation to fire event (just for LoadOption.PreserveCurrentValues)
                        this.SetOldRecord(dataRow,  recordNo);
                     }
                     break;
                default:
                    throw ExceptionBuilder.ArgumentOutOfRange("LoadOption");
            }

            if (hasError) {
                string error = Res.GetString(Res.Load_ReadOnlyDataModified);
                if (dataRow.RowError.Length == 0) { // WebData 112272, append the row error
                    dataRow.RowError = error;
                }
                else {
                    dataRow.RowError += " ]:[ " + error ;
                }

                foreach(DataColumn dc in this.Columns) {
                    if (dc.ReadOnly && !dc.Computed)
                        dataRow.SetColumnError(dc, error);
                }
            }

            drcevent = RaiseRowChanged(drcevent, dataRow, action);
            if (action == DataRowAction.Nothing) { // RaiseRowChanged does not fire for DataRowAction.Nothing
                dataRow.inChangingEvent = true;
                try {
                    OnRowChanged(drcevent, dataRow, action);
                }
                finally {
                    dataRow.inChangingEvent = false;
                }
            }

        }

        public  DataTableReader CreateDataReader() {
            return new DataTableReader(this);
        }


        public void WriteXml(Stream stream)
        {
            WriteXml(stream, XmlWriteMode.IgnoreSchema, false);
        }

        public void WriteXml(Stream stream, bool writeHierarchy)
        {
            WriteXml(stream, XmlWriteMode.IgnoreSchema, writeHierarchy);
        }

        public void WriteXml(TextWriter writer)
        {
            WriteXml(writer, XmlWriteMode.IgnoreSchema, false);
        }

        public void WriteXml(TextWriter writer, bool writeHierarchy)
        {
            WriteXml(writer, XmlWriteMode.IgnoreSchema, writeHierarchy);
        }

        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, XmlWriteMode.IgnoreSchema, false);
        }

        public void WriteXml(XmlWriter writer, bool writeHierarchy)
        {
            WriteXml(writer, XmlWriteMode.IgnoreSchema, writeHierarchy);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXml(String fileName)
        {
            WriteXml(fileName, XmlWriteMode.IgnoreSchema, false);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXml(String fileName, bool writeHierarchy)
        {
            WriteXml(fileName, XmlWriteMode.IgnoreSchema, writeHierarchy);
        }

        public void WriteXml(Stream stream, XmlWriteMode mode)
        {
            WriteXml(stream, mode, false);
        }

        public void WriteXml(Stream stream, XmlWriteMode mode, bool writeHierarchy)
        {
            if (stream != null) {
                XmlTextWriter w =  new XmlTextWriter(stream, null) ;
                w.Formatting = Formatting.Indented;

                WriteXml( w, mode, writeHierarchy);
            }
        }

        public void WriteXml(TextWriter writer, XmlWriteMode mode)
        {
            WriteXml(writer, mode, false);
        }

        public void WriteXml(TextWriter writer, XmlWriteMode mode, bool writeHierarchy)
        {
            if (writer != null) {
                XmlTextWriter w =  new XmlTextWriter(writer) ;
                w.Formatting = Formatting.Indented;

                WriteXml(w, mode, writeHierarchy);
            }
        }

        public void WriteXml(XmlWriter writer, XmlWriteMode mode)
        {
            WriteXml(writer, mode, false);
        }
        public void WriteXml(XmlWriter writer, XmlWriteMode mode, bool writeHierarchy)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.WriteXml|API> %d#, mode=%d{ds.XmlWriteMode}\n", ObjectID,  (int)mode);
            try{
                if (this.tableName.Length == 0) {
                    throw ExceptionBuilder.CanNotSerializeDataTableWithEmptyName();
                }
                // Generate SchemaTree and write it out
                if (writer != null) {

                    if (mode == XmlWriteMode.DiffGram) { // FIX THIS
                        // Create and save the updates
                        new NewDiffgramGen(this, writeHierarchy).Save(writer, this);
                    }
                    else {
                        // Create and save xml data
                        if (mode == XmlWriteMode.WriteSchema) {
                            DataSet ds = null;
                            string tablenamespace = this.tableNamespace;
                            if (null == this.DataSet) {
                                ds = new DataSet();
                                // if user set values on DataTable, it isn't necessary
                                // to set them on the DataSet because they won't be inherited
                                // but it is simpler to set them in both places

                                // if user did not set values on DataTable, it is required
                                // to set them on the DataSet so the table will inherit
                                // the value already on the Datatable
                                ds.SetLocaleValue(_culture, _cultureUserSet);
                                ds.CaseSensitive = this.CaseSensitive;
                                ds.Namespace = this.Namespace;
                                ds.RemotingFormat = this.RemotingFormat;
                                ds.Tables.Add(this);
                            }

                            if (writer != null) {
                                XmlDataTreeWriter xmldataWriter = new XmlDataTreeWriter(this, writeHierarchy);
                                xmldataWriter.Save(writer, /*mode == XmlWriteMode.WriteSchema*/true);
                            }
                            if (null != ds) {
                                ds.Tables.Remove(this);
                                this.tableNamespace = tablenamespace;
                            }
                        }
                        else {
                            XmlDataTreeWriter xmldataWriter = new XmlDataTreeWriter(this, writeHierarchy);
                            xmldataWriter.Save(writer,/*mode == XmlWriteMode.WriteSchema*/ false);
                        }
                    }
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXml(String fileName, XmlWriteMode mode)
        {
            WriteXml(fileName, mode, false);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXml(String fileName, XmlWriteMode mode, bool writeHierarchy)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.WriteXml|API> %d#, fileName='%ls', mode=%d{ds.XmlWriteMode}\n", ObjectID, fileName, (int)mode);
            try {
                using(XmlTextWriter xw = new XmlTextWriter( fileName, null )) {
                    xw.Formatting = Formatting.Indented;
                    xw.WriteStartDocument(true);

                    WriteXml(xw, mode, writeHierarchy);

                    xw.WriteEndDocument();
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public void WriteXmlSchema(Stream stream) {
            WriteXmlSchema(stream, false);
        }

        public void WriteXmlSchema(Stream stream, bool writeHierarchy)
        {
            if (stream == null)
                return;

            XmlTextWriter w =  new XmlTextWriter(stream, null) ;
            w.Formatting = Formatting.Indented;

            WriteXmlSchema( w, writeHierarchy );
        }

        public void WriteXmlSchema( TextWriter writer ) {
            WriteXmlSchema( writer, false );
        }

        public void WriteXmlSchema( TextWriter writer, bool writeHierarchy )
        {

            if (writer == null)
                return;

            XmlTextWriter w =  new XmlTextWriter(writer);
            w.Formatting = Formatting.Indented;

            WriteXmlSchema( w, writeHierarchy );
        }

        private  bool CheckForClosureOnExpressions(DataTable dt, bool writeHierarchy) {
            List<DataTable> tableList = new List<DataTable>();
            tableList.Add(dt);
            if (writeHierarchy) { // WebData 112161
                CreateTableList(dt, tableList);
            }
            return CheckForClosureOnExpressionTables(tableList);
        }

        private bool CheckForClosureOnExpressionTables(List<DataTable> tableList) {
            Debug.Assert(tableList != null, "tableList shouldnot be null");

            foreach(DataTable datatable in tableList) {
                foreach(DataColumn dc in datatable.Columns) {
                    if (dc.Expression.Length != 0)  {
                        DataColumn[] dependency = dc.DataExpression.GetDependency();
                        for (int j = 0; j < dependency.Length; j++) {
                            if (!(tableList.Contains(dependency[j].Table))) {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }


        public void WriteXmlSchema(XmlWriter writer) {
            WriteXmlSchema(writer, false);
        }

        public void WriteXmlSchema(XmlWriter writer, bool writeHierarchy)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.WriteXmlSchema|API> %d#\n", ObjectID);
            try{
                if (this.tableName.Length == 0) {
                    throw ExceptionBuilder.CanNotSerializeDataTableWithEmptyName();
                }

                if (!CheckForClosureOnExpressions(this, writeHierarchy)) {
                    throw ExceptionBuilder.CanNotSerializeDataTableHierarchy();
                }

                DataSet ds = null;
                string tablenamespace = this.tableNamespace;//SQL BU Defect Tracking 286968

                // Generate SchemaTree and write it out
                if (null == this.DataSet) {
                    ds = new DataSet();
                    // if user set values on DataTable, it isn't necessary
                    // to set them on the DataSet because they won't be inherited
                    // but it is simpler to set them in both places

                    // if user did not set values on DataTable, it is required
                    // to set them on the DataSet so the table will inherit
                    // the value already on the Datatable
                    ds.SetLocaleValue(_culture, _cultureUserSet);
                    ds.CaseSensitive = this.CaseSensitive;
                    ds.Namespace = this.Namespace;
                    ds.RemotingFormat = this.RemotingFormat;
                    ds.Tables.Add(this);
                }

                if (writer != null) {
                    XmlTreeGen treeGen = new XmlTreeGen(SchemaFormat.Public);
                    treeGen.Save(null, this, writer, writeHierarchy);
                }
                if (null != ds) {
                    ds.Tables.Remove(this);
                    this.tableNamespace = tablenamespace;
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXmlSchema(String fileName) {
            WriteXmlSchema(fileName, false);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void WriteXmlSchema(String fileName, bool writeHierarchy)
        {
            XmlTextWriter xw = new XmlTextWriter( fileName, null );
            try {
                xw.Formatting = Formatting.Indented;
                xw.WriteStartDocument(true);
                WriteXmlSchema(xw, writeHierarchy);
                xw.WriteEndDocument();
            }
            finally {
                xw.Close();
            }
        }

        public XmlReadMode ReadXml(Stream stream)
        {
            if (stream == null)
                return XmlReadMode.Auto;

            XmlTextReader xr = new XmlTextReader(stream);

            // Prevent Dtd entity in DataTable 
            xr.XmlResolver = null;

            return ReadXml(xr, false);
        }

        public XmlReadMode ReadXml(TextReader reader)
        {
            if (reader == null)
                return XmlReadMode.Auto;

            XmlTextReader xr = new XmlTextReader(reader);

            // Prevent Dtd entity in DataTable 
            xr.XmlResolver = null;

            return ReadXml(xr, false);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public XmlReadMode ReadXml(string fileName)
        {
            XmlTextReader xr = new XmlTextReader(fileName);

            // Prevent Dtd entity in DataTable 
            xr.XmlResolver = null;

            try
            {
                return ReadXml( xr , false);
            }
            finally {
                xr.Close();
            }
        }

        public XmlReadMode ReadXml(XmlReader reader)
        {
            return ReadXml(reader, false);
        }

        private void RestoreConstraint(bool originalEnforceConstraint) {
            if (this.DataSet != null) {
                this.DataSet.EnforceConstraints = originalEnforceConstraint;
            }
            else {
                this.EnforceConstraints = originalEnforceConstraint;
            }
        }

        private bool IsEmptyXml(XmlReader reader) {
            if (reader.IsEmptyElement) {
                if (reader.AttributeCount == 0 || (reader.LocalName == Keywords.DIFFGRAM && reader.NamespaceURI == Keywords.DFFNS)) {
                    return true;
                }
                if (reader.AttributeCount == 1) {
                    reader.MoveToAttribute(0);
                    if ((this.Namespace == reader.Value) &&
                        (this.Prefix == reader.LocalName) &&
                        (reader.Prefix == Keywords.XMLNS) &&
                        (reader.NamespaceURI == Keywords.XSD_XMLNS_NS))
                    return true;
                }
            }
            return false;
        }

        internal XmlReadMode ReadXml(XmlReader reader, bool denyResolving)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.ReadXml|INFO> %d#, denyResolving=%d{bool}\n", ObjectID, denyResolving);
            try {

              DataTable.RowDiffIdUsageSection rowDiffIdUsage = new DataTable.RowDiffIdUsageSection();
              try {
                bool fDataFound = false;
                bool fSchemaFound = false;
                bool fDiffsFound = false;
                bool fIsXdr = false;
                int iCurrentDepth = -1;
                XmlReadMode ret = XmlReadMode.Auto;

                // clear the hashtable to avoid conflicts between diffgrams, SqlHotFix 782
                rowDiffIdUsage.Prepare(this);

                if (reader == null)
                    return ret;
                bool originalEnforceConstraint = false;
                if (this.DataSet != null) {
                    originalEnforceConstraint = this.DataSet.EnforceConstraints;
                    this.DataSet.EnforceConstraints = false;
                }
                else {
                    originalEnforceConstraint = this.EnforceConstraints;
                    this.EnforceConstraints = false;
                }

                if (reader is XmlTextReader)
                    ((XmlTextReader) reader).WhitespaceHandling = WhitespaceHandling.Significant;

                XmlDocument xdoc = new XmlDocument(); // we may need this to infer the schema
                XmlDataLoader xmlload = null;


                reader.MoveToContent();
                if (Columns.Count == 0) {
                    if (IsEmptyXml(reader)) {
                        reader.Read();
                        return ret;
                    }
                }

                if (reader.NodeType == XmlNodeType.Element) {
                    iCurrentDepth = reader.Depth;

                    if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                        if (Columns.Count == 0) {
                            if (reader.IsEmptyElement) {
                                reader.Read();
                                return XmlReadMode.DiffGram;
                            }
                            throw ExceptionBuilder.DataTableInferenceNotSupported();
                        }
                        this.ReadXmlDiffgram(reader);
                        // read the closing tag of the current element
                        ReadEndElement(reader);

                        RestoreConstraint(originalEnforceConstraint);
                        return XmlReadMode.DiffGram;
                    }

                    // if reader points to the schema load it
                    if (reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI==Keywords.XDRNS) {
                        // load XDR schema and exit
                        ReadXDRSchema(reader);

                        RestoreConstraint(originalEnforceConstraint);
                        return XmlReadMode.ReadSchema; //since the top level element is a schema return
                    }

                    if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI==Keywords.XSDNS) {
                        // load XSD schema and exit
                        ReadXmlSchema(reader, denyResolving);
                        RestoreConstraint(originalEnforceConstraint);
                        return XmlReadMode.ReadSchema; //since the top level element is a schema return
                    }

                    if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal)) {
                        if (this.DataSet != null) { // we should not throw for constraint, we already will throw for unsupported schema, so restore enforce cost, but not via property
                            this.DataSet.RestoreEnforceConstraints(originalEnforceConstraint);
                        }
                        else {
                            this.enforceConstraints = originalEnforceConstraint;
                        }

                        throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);
                    }

                    // now either the top level node is a table and we load it through dataReader...

                    // ... or backup the top node and all its attributes because we may need to InferSchema
                    XmlElement topNode = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    if (reader.HasAttributes) {
                        int attrCount = reader.AttributeCount;
                        for (int i=0;i<attrCount;i++) {
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

                    while(MoveToElement(reader, iCurrentDepth)) {

                        if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                            this.ReadXmlDiffgram(reader);
                            // read the closing tag of the current element
                            ReadEndElement(reader);
                            RestoreConstraint(originalEnforceConstraint);
                            return XmlReadMode.DiffGram;
                        }

                        // if reader points to the schema load it...


                        if (!fSchemaFound && !fDataFound && reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI==Keywords.XDRNS) {
                            // load XDR schema and exit
                            ReadXDRSchema(reader);
                            fSchemaFound = true;
                            fIsXdr = true;
                            continue;
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI==Keywords.XSDNS) {
                            // load XSD schema and exit
                            ReadXmlSchema(reader, denyResolving);
                            fSchemaFound = true;
                            continue;
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))  {
                            if (this.DataSet != null) { // we should not throw for constraint, we already will throw for unsupported schema, so restore enforce cost, but not via property
                                this.DataSet.RestoreEnforceConstraints(originalEnforceConstraint);
                            }
                            else {
                                this.enforceConstraints = originalEnforceConstraint;
                            }
                            throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);
                        }

                        if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                            this.ReadXmlDiffgram(reader);
                            fDiffsFound = true;
                            ret = XmlReadMode.DiffGram;
                        }
                        else {
                            // we found data here
                            fDataFound = true;

                            if (!fSchemaFound && Columns.Count == 0) {
                                XmlNode node = xdoc.ReadNode(reader);
                                topNode.AppendChild(node);
                            }
                            else {
                                if (xmlload == null)
                                    xmlload = new XmlDataLoader(this, fIsXdr, topNode, false);
                                xmlload.LoadData(reader);
                                if (fSchemaFound)
                                    ret = XmlReadMode.ReadSchema;
                                else
                                    ret = XmlReadMode.IgnoreSchema;
                            }
                        }

                    }
                    // read the closing tag of the current element
                    ReadEndElement(reader);

                    // now top node contains the data part
                    xdoc.AppendChild(topNode);

                    if (!fSchemaFound && Columns.Count == 0) {
                        if (IsEmptyXml(reader)) {
                            reader.Read();
                            return ret;
                        }
                        throw ExceptionBuilder.DataTableInferenceNotSupported();
                    }

                    if (xmlload == null)
                        xmlload = new XmlDataLoader(this, fIsXdr, false);

                    // so we InferSchema
                    if (!fDiffsFound) {// we need to add support for inference here
                    }
                }
                RestoreConstraint(originalEnforceConstraint);
                return ret;
              }
              finally {
                rowDiffIdUsage.Cleanup();
              }
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal XmlReadMode ReadXml(XmlReader reader, XmlReadMode mode, bool denyResolving)
        {
            DataTable.RowDiffIdUsageSection rowDiffIdUsage = new DataTable.RowDiffIdUsageSection();
            try {
                bool fSchemaFound = false;
                bool fDataFound = false;
                bool fIsXdr = false;
                int iCurrentDepth = -1;
                XmlReadMode ret = mode;

                // Dev11 904428: prepare and cleanup rowDiffId hashtable
                rowDiffIdUsage.Prepare(this);

                if (reader == null)
                    return ret;

                bool originalEnforceConstraint  = false;
                if (this.DataSet != null) {
                    originalEnforceConstraint  = this.DataSet.EnforceConstraints;
                    this.DataSet.EnforceConstraints = false;
                }
                else {
                    originalEnforceConstraint  = this.EnforceConstraints;
                    this.EnforceConstraints = false;
                }

                if (reader is XmlTextReader)
                    ((XmlTextReader) reader).WhitespaceHandling = WhitespaceHandling.Significant;

                XmlDocument xdoc = new XmlDocument(); // we may need this to infer the schema

                if ((mode != XmlReadMode.Fragment) && (reader.NodeType == XmlNodeType.Element))
                    iCurrentDepth = reader.Depth;

                reader.MoveToContent();
                if (Columns.Count == 0) {
                    if (IsEmptyXml(reader)) {
                        reader.Read();
                        return ret;
                    }
                }

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
                                if (Columns.Count == 0) {
                                    if(reader.IsEmptyElement) {
                                        reader.Read();
                                        return XmlReadMode.DiffGram;
                                    }
                                    throw ExceptionBuilder.DataTableInferenceNotSupported();
                                }
                                this.ReadXmlDiffgram(reader);
                                // read the closing tag of the current element
                                ReadEndElement(reader);
                            }
                            else {
                                reader.Skip();
                            }
                            RestoreConstraint(originalEnforceConstraint);
                            return ret;
                        }

                        if (reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI==Keywords.XDRNS) {
                            // load XDR schema and exit
                            if ((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema)) {
                                ReadXDRSchema(reader);
                            }
                            else {
                                reader.Skip();
                            }
                            RestoreConstraint(originalEnforceConstraint);
                            return ret; //since the top level element is a schema return
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI==Keywords.XSDNS) {
                            // load XSD schema and exit
                            if ((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema))  {
                                ReadXmlSchema(reader, denyResolving);
                            }
                            else
                                reader.Skip();
                            RestoreConstraint(originalEnforceConstraint);
                            return ret; //since the top level element is a schema return
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))  {
                             if (this.DataSet != null) { // we should not throw for constraint, we already will throw for unsupported schema, so restore enforce cost, but not via property
                                 this.DataSet.RestoreEnforceConstraints(originalEnforceConstraint);
                             }
                             else {
                                this.enforceConstraints = originalEnforceConstraint;
                            }
                            throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);
                        }

                        // now either the top level node is a table and we load it through dataReader...
                        // ... or backup the top node and all its attributes
                        topNode = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        if (reader.HasAttributes) {
                            int attrCount = reader.AttributeCount;
                            for (int i=0;i<attrCount;i++) {
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

                    while(MoveToElement(reader, iCurrentDepth)) {

                        if (reader.LocalName == Keywords.XDR_SCHEMA && reader.NamespaceURI==Keywords.XDRNS) {
                            // load XDR schema
                       	    if (!fSchemaFound && !fDataFound && (mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema))  {
                                ReadXDRSchema(reader);
                                fSchemaFound = true;
                                fIsXdr = true;
                            }
                            else {
                                reader.Skip();
                            }
                            continue;
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI==Keywords.XSDNS) {
                        // load XSD schema and exit
                            if ((mode != XmlReadMode.IgnoreSchema) && (mode != XmlReadMode.InferSchema))  {
                                ReadXmlSchema(reader, denyResolving);
                                fSchemaFound = true;
                            }
                            else {
                                reader.Skip();
                            }
                            continue;
                        }

                        if ((reader.LocalName == Keywords.DIFFGRAM) && (reader.NamespaceURI == Keywords.DFFNS)) {
                            if ((mode == XmlReadMode.DiffGram) || (mode == XmlReadMode.IgnoreSchema)) {
                                if (Columns.Count == 0) {
                                    if(reader.IsEmptyElement) {
                                        reader.Read();
                                        return XmlReadMode.DiffGram;
                                    }
                                    throw ExceptionBuilder.DataTableInferenceNotSupported();
                                }
                                this.ReadXmlDiffgram(reader);
                                ret = XmlReadMode.DiffGram;
                            }
                            else {
                                reader.Skip();
                            }
                            continue;
                        }

                        if (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI.StartsWith(Keywords.XSD_NS_START, StringComparison.Ordinal))  {
                            if (this.DataSet != null) { // we should not throw for constraint, we already will throw for unsupported schema, so restore enforce cost, but not via property
                                 this.DataSet.RestoreEnforceConstraints(originalEnforceConstraint);
                            }
                            else {
                                this.enforceConstraints = originalEnforceConstraint;
                            }
                            throw ExceptionBuilder.DataSetUnsupportedSchema(Keywords.XSDNS);
                        }

                        if (mode == XmlReadMode.DiffGram) {
                            reader.Skip();
                            continue; // we do not read data in diffgram mode
                        }

                        // if we are here we found some data
                        fDataFound = true;

                        if (mode == XmlReadMode.InferSchema) { //save the node in DOM until the end;
                            XmlNode node = xdoc.ReadNode(reader);
                            topNode.AppendChild(node);
                        }
                        else {
                            if (Columns.Count == 0) {
                                throw ExceptionBuilder.DataTableInferenceNotSupported();
                            }
                            if (xmlload == null)
                                xmlload = new XmlDataLoader(this, fIsXdr, topNode,  mode == XmlReadMode.IgnoreSchema);
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
                        RestoreConstraint(originalEnforceConstraint);
                        return ret;
                    }
    //todo
                    // Load Data
                    if (mode == XmlReadMode.InferSchema) {
                        if (Columns.Count == 0) {
                            throw ExceptionBuilder.DataTableInferenceNotSupported();
                        }

    // Microsoft                    xmlload.InferSchema(xdoc, null);
    // Microsoft                    xmlload.LoadData(xdoc);
                    }
                }
                RestoreConstraint(originalEnforceConstraint);

                return ret;
            }
            finally {
                // Dev11 904428: prepare and cleanup rowDiffId hashtable
                rowDiffIdUsage.Cleanup();
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
        internal void ReadXDRSchema(XmlReader reader) {
            XmlDocument xdoc = new XmlDocument(); // we may need this to infer the schema
            XmlNode schNode = xdoc.ReadNode(reader);;
            //consume and ignore it - No support
        }

        internal bool MoveToElement(XmlReader reader, int depth) {
            while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.Element && reader.Depth > depth) {
                reader.Read();
            }
            return (reader.NodeType == XmlNodeType.Element);
        }
        private void ReadXmlDiffgram(XmlReader reader) { // fill correctly
            int d = reader.Depth;
            bool fEnforce = this.EnforceConstraints;
            this.EnforceConstraints =false;
            DataTable newDt;
            bool isEmpty;

            if (this.Rows.Count == 0) {
                isEmpty = true;
                newDt = this;
            }
            else {
                isEmpty = false;
                newDt = this.Clone();
                newDt.EnforceConstraints = false;
            }

            newDt.Rows.nullInList = 0;

            reader.MoveToContent();

            if ((reader.LocalName != Keywords.DIFFGRAM) && (reader.NamespaceURI != Keywords.DFFNS))
                return;
            reader.Read();
            if (reader.NodeType == XmlNodeType.Whitespace) {
                MoveToElement(reader, reader.Depth - 1 /*iCurrentDepth*/); // skip over whitespaces.
            }

            newDt.fInLoadDiffgram = true;

            if (reader.Depth > d) {
                if ((reader.NamespaceURI != Keywords.DFFNS) && (reader.NamespaceURI != Keywords.MSDNS)) {
                    //we should be inside the dataset part
                    XmlDocument xdoc = new XmlDocument();
                    XmlElement node = xdoc.CreateElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    reader.Read();
                    if (reader.Depth-1 > d) {
                        XmlDataLoader xmlload = new XmlDataLoader(newDt, false, node, false);
                        xmlload.isDiffgram = true; // turn on the special processing
                        xmlload.LoadData(reader);
                    }
                    ReadEndElement(reader);
                }
                if (((reader.LocalName == Keywords.SQL_BEFORE) && (reader.NamespaceURI == Keywords.DFFNS)) ||
                    ((reader.LocalName == Keywords.MSD_ERRORS) && (reader.NamespaceURI == Keywords.DFFNS)))

                {
                    //this will consume the changes and the errors part
                    XMLDiffLoader diffLoader = new XMLDiffLoader();
                    diffLoader.LoadDiffGram(newDt, reader);
                }

                // get to the closing diff tag
                while(reader.Depth > d) {
                    reader.Read();
                }
                // read the closing tag
                ReadEndElement(reader);
            }

            if (newDt.Rows.nullInList > 0)
                throw ExceptionBuilder.RowInsertMissing(newDt.TableName);


            newDt.fInLoadDiffgram = false;
            List<DataTable> tableList = new List<DataTable>();
            tableList.Add(this);
            CreateTableList(this, tableList);

// this is terrible, optimize it
            for (int i = 0; i < tableList.Count ; i++) {
                DataRelation[] relations = tableList[i].NestedParentRelations;
                foreach(DataRelation rel in relations) {
                    if (rel != null && rel.ParentTable == tableList[i]) {
                        foreach (DataRow r in tableList[i].Rows) {
                            foreach (DataRelation rel2 in relations) {
                                r.CheckForLoops(rel2);
                            }
                        }
                    }
                }
            }

            if (!isEmpty) {
                this.Merge(newDt);
            }
            this.EnforceConstraints = fEnforce;
        }

        internal void ReadXSDSchema(XmlReader reader, bool denyResolving) {
            XmlSchemaSet sSet = new XmlSchemaSet();
            while (reader.LocalName == Keywords.XSD_SCHEMA && reader.NamespaceURI==Keywords.XSDNS) {
                XmlSchema s = XmlSchema.Read(reader, null);
                sSet.Add(s);
                //read the end tag
                ReadEndElement(reader);
            }
            sSet.Compile();

            XSDSchema schema = new XSDSchema();
            schema.LoadSchema(sSet, this);
        }


        public void ReadXmlSchema(Stream stream)
        {
            if (stream == null)
                return;

            ReadXmlSchema( new XmlTextReader( stream ), false );
        }

        public void ReadXmlSchema(TextReader reader)
        {
            if (reader == null)
                return;

            ReadXmlSchema( new XmlTextReader( reader ), false );
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public void ReadXmlSchema(String fileName)
        {
            XmlTextReader xr = new XmlTextReader(fileName);
            try {
                ReadXmlSchema( xr, false );
            }
            finally {
                xr.Close();
            }
        }

        public void ReadXmlSchema(XmlReader reader)
        {
            ReadXmlSchema(reader, false);
        }

        internal void ReadXmlSchema(XmlReader reader, bool denyResolving)
        {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataTable.ReadXmlSchema|INFO> %d#, denyResolving=%d{bool}\n", ObjectID, denyResolving);
            try{
                DataSet ds = new DataSet();
                SerializationFormat cachedRemotingFormat = this.RemotingFormat;
                // fxcop: ReadXmlSchema will provide the CaseSensitive, Locale, Namespace information
                ds.ReadXmlSchema(reader, denyResolving);

                string CurrentTableFullName = ds.MainTableName;

                if (Common.ADP.IsEmpty(this.tableName) && Common.ADP.IsEmpty(CurrentTableFullName))
                    return;

                DataTable currentTable = null;

                if (!Common.ADP.IsEmpty(this.tableName)) {
                    if (!Common.ADP.IsEmpty(this.Namespace)) {
                        currentTable = ds.Tables[this.tableName, this.Namespace];
                    }
                    else {//SQL BU defect tracking 240293
                        int tableIndex = ds.Tables.InternalIndexOf(this.tableName);
                        if (tableIndex  > -1) {
                            currentTable = ds.Tables[tableIndex];
                        }
                    }
                }
                else{  //!Common.ADP.IsEmpty(CurrentTableFullName)
                    string CurrentTableNamespace = "";
                    int nsSeperator = CurrentTableFullName.IndexOf(':');
                    if (nsSeperator > -1) {
                        CurrentTableNamespace = CurrentTableFullName.Substring(0, nsSeperator);
                    }
                    string CurrentTableName = CurrentTableFullName.Substring(nsSeperator + 1, CurrentTableFullName.Length - nsSeperator -1);

                    currentTable = ds.Tables[CurrentTableName, CurrentTableNamespace];
                }

                if (currentTable == null) { // bug fix :99186
                    string qTableName = string.Empty;
                    if (!Common.ADP.IsEmpty(this.tableName)) {
                        qTableName = (this.Namespace.Length > 0)? (this.Namespace + ":" + this.tableName):this.tableName;
                    }
                    else {
                        qTableName = CurrentTableFullName ;
                    }
                    throw ExceptionBuilder.TableNotFound(qTableName);
                }

                currentTable._remotingFormat = cachedRemotingFormat;

                List<DataTable> tableList = new List<DataTable>();
                tableList.Add(currentTable);
                CreateTableList(currentTable, tableList);
                List<DataRelation> relationList = new List<DataRelation>();
                CreateRelationList(tableList, relationList);

                if (relationList.Count == 0) {
                    if (this.Columns.Count == 0) {
                        DataTable tempTable = currentTable;
                        if (tempTable != null)
                            tempTable.CloneTo(this, null, false); // we may have issue Amir
                        if (this.DataSet == null && this.tableNamespace == null) { // webdata 105506
// for standalone table, clone wont get these correctly, since they may come with inheritance
                            this.tableNamespace =  tempTable.Namespace;
                        }
                    }
                    return;
                }
                else {
                    if (Common.ADP.IsEmpty(this.TableName)) {
                        this.TableName = currentTable.TableName;
                        if (!Common.ADP.IsEmpty(currentTable.Namespace)) {
                            this.Namespace = currentTable.Namespace;
                        }
                    }
                    if (this.DataSet == null) {
                        DataSet dataset = new DataSet(ds.DataSetName);
// webdata 105506
                        // if user set values on DataTable, it isn't necessary
                        // to set them on the DataSet because they won't be inherited
                        // but it is simpler to set them in both places

                        // if user did not set values on DataTable, it is required
                        // to set them on the DataSet so the table will inherit
                        // the value already on the Datatable
                        dataset.SetLocaleValue(ds.Locale, ds.ShouldSerializeLocale());
                        dataset.CaseSensitive = ds.CaseSensitive;
                        dataset.Namespace = ds.Namespace;
                        dataset.mainTableName = ds.mainTableName;
                        dataset.RemotingFormat = ds.RemotingFormat;

                        dataset.Tables.Add(this);
                    }

                    DataTable targetTable = CloneHierarchy(currentTable, this.DataSet, null);

                    foreach(DataTable tempTable in tableList) {
                        DataTable destinationTable = this.DataSet.Tables[tempTable.tableName, tempTable.Namespace];
                        DataTable sourceTable = ds.Tables[tempTable.tableName, tempTable.Namespace];
                        foreach(Constraint tempConstrain in sourceTable.Constraints) {
                            ForeignKeyConstraint fkc = tempConstrain as ForeignKeyConstraint;  // we have already cloned the UKC when cloning the datatable
                            if (fkc != null) {
                                if (fkc.Table != fkc.RelatedTable)  {
                                    if (tableList.Contains(fkc.Table) && tableList.Contains(fkc.RelatedTable)) {
                                        ForeignKeyConstraint newFKC = (ForeignKeyConstraint)fkc.Clone(destinationTable.DataSet);
                                        if (!destinationTable.Constraints.Contains(newFKC.ConstraintName))
                                            destinationTable.Constraints.Add(newFKC); // we know that the dest table is already in the table
                                    }
                                }
                           }
                        }
                    }
                    foreach(DataRelation rel in relationList) {
                        if (!this.DataSet.Relations.Contains(rel.RelationName))
                            this.DataSet.Relations.Add(rel.Clone(this.DataSet));
                    }

                    bool hasExternaldependency = false;

                    foreach(DataTable tempTable in tableList) {
                        foreach(DataColumn dc in tempTable.Columns) {
                            hasExternaldependency = false;
                            if (dc.Expression.Length  != 0) {
                                DataColumn[] dependency = dc.DataExpression.GetDependency();
                                for (int j = 0; j < dependency.Length; j++) {
                                    if (!tableList.Contains(dependency[j].Table)) {
                                        hasExternaldependency = true;
                                        break;
                                    }
                                }
                            }
                            if (!hasExternaldependency) {
                                this.DataSet.Tables[tempTable.TableName, tempTable.Namespace].Columns[dc.ColumnName].Expression = dc.Expression;
                            }
                        }
                        hasExternaldependency = false;
                    }

                }
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        private void CreateTableList(DataTable currentTable, List<DataTable> tableList) {
            foreach( DataRelation r in currentTable.ChildRelations ) {
                if (! tableList.Contains(r.ChildTable)) {
                    tableList.Add(r.ChildTable);
                    CreateTableList(r.ChildTable, tableList);
                }
            }
        }
        private void CreateRelationList(List<DataTable> tableList,  List<DataRelation> relationList) {
            foreach(DataTable table in tableList) {
                foreach( DataRelation r in table.ChildRelations) {
                    if (tableList.Contains(r.ChildTable) && tableList.Contains(r.ParentTable)) {
                        relationList.Add(r);
                    }
                }
            }
        }

        /**************************************************************************
        The V2.0 (no V1.0 or V1.1) WSDL for Untyped DataTable being returned as a result (no parameters)
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
                                <s:any minOccurs="0" maxOccurs="unbounded" namespace="http://www.w3.org/2001/XMLSchema" processContents="lax" /> 
                                <s:any minOccurs="1" namespace="urn:schemas-microsoft-com:xml-diffgram-v1" processContents="lax" /> 
                            </s:sequence>
                        </s:complexType>
                    </s:element>
                </s:sequence>
            </s:complexType>
        </s:element>

        Typed DataTable is not supported in WSDL (SQLBU 444636)

        either fails because xsd generates its typed DataTable with an internal parameterless ctor
        
        or System.NullReferenceException: Object reference not set to an instance of an object.   (if namespace of StronglyTyped DataTable is not set)
           at System.Data.XmlTreeGen.FindTargetNamespace(DataTable table)

        or System.InvalidOperationException: Schema Id is missing. The schema returned from WebServiceDataSetServer.Service+StudentsDataTable.GetSchema() must have an Id.
           at System.Xml.Serialization.SerializableMapping.RetrieveSerializableSchema()
        *****************************************************************************/
        public static XmlSchemaComplexType GetDataTableSchema(XmlSchemaSet schemaSet) {
            XmlSchemaComplexType type = new XmlSchemaComplexType();
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            XmlSchemaAny any = new XmlSchemaAny();
            any.Namespace = XmlSchema.Namespace;
            any.MinOccurs = 0;
            any.MaxOccurs = Decimal.MaxValue;
            any.ProcessContents = XmlSchemaContentProcessing.Lax;
            sequence.Items.Add(any);

            any = new XmlSchemaAny();
            any.Namespace = Keywords.DFFNS;
            any.MinOccurs = 1; // when recognizing WSDL - MinOccurs="0" denotes DataSet, a MinOccurs="1" for DataTable
            any.ProcessContents = XmlSchemaContentProcessing.Lax;
            sequence.Items.Add(any);

            type.Particle = sequence;

            return type;
        }

        XmlSchema IXmlSerializable.GetSchema() {
            return GetSchema();
        }

        protected virtual XmlSchema GetSchema() {
            if (GetType() == typeof(DataTable)) {
                return null;
            }
            MemoryStream stream = new MemoryStream();

            XmlWriter writer = new XmlTextWriter(stream, null);
            if (writer != null) {
                 (new XmlTreeGen(SchemaFormat.WebService)).Save(this, writer);
            }
            stream.Position = 0;
            return XmlSchema.Read(new XmlTextReader(stream), null);
        }

        void IXmlSerializable.ReadXml(XmlReader reader) {
            IXmlTextParser textReader = reader as IXmlTextParser;
            bool fNormalization = true;
            if (textReader != null) {
                fNormalization = textReader.Normalized;
                textReader.Normalized = false;
            }
            ReadXmlSerializable(reader);

            if (textReader != null) {
                textReader.Normalized = fNormalization;
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer) {
            WriteXmlSchema(writer, false);
            WriteXml(writer, XmlWriteMode.DiffGram, false);
        }

         protected virtual void ReadXmlSerializable(XmlReader reader) {
            ReadXml(reader, XmlReadMode.DiffGram, true);
        }

/*
        [
        DefaultValue(false),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DataTableSerializeHierarchy)
        ]
        public bool SerializeHierarchy {
            get {
                return this.serializeHierarchy;
            }
            set {
                this.serializeHierarchy = value;
            }
        }
*/

        // RowDiffIdUsageSection & DSRowDiffIdUsageSection Usage:
        //
        //        DataTable.[DS]RowDiffIdUsageSection rowDiffIdUsage = new DataTable.[DS]RowDiffIdUsageSection();
        //        try {
        //            rowDiffIdUsage.Prepare(DataTable or DataSet, depending on type);
        //
        //            // code that requires RowDiffId usage
        //
        //        }
        //        finally {
        //            rowDiffIdUsage.Cleanup();
        //        }
        // 
        // Nested calls are allowed on different tables. For example, user can register to row change events and trigger 
        // ReadXml on different table/ds). But, if user code will try to call ReadXml on table that is already in the scope,
        // this code will assert since nested calls on same table are unsupported.
        internal struct RowDiffIdUsageSection
        {
#if DEBUG
            // This list contains tables currently used in diffgram processing, not including new tables that might be added later during.
            // if diffgram processing is not started, this value must be null. when it starts, relevant method should call Prepare.
            // Notes:
            // * in case of ReadXml on empty DataSet, this list can be initialized as empty (so empty list != null).
            // * only one scope is allowed on single thread, either for datatable or dataset
            // * assert is triggered if same table is added to this list twice
            // 
            // do not allocate TLS data in RETAIL bits!
            [ThreadStatic]
            internal static List<DataTable> s_usedTables;
#endif //DEBUG

            DataTable _targetTable;

            internal void Prepare(DataTable table)
            {
                Debug.Assert(_targetTable == null, "do not reuse this section");
                Debug.Assert(table != null);
                Debug.Assert(table.rowDiffId == null, "rowDiffId wasn't previously cleared");
#if DEBUG
                Debug.Assert(s_usedTables == null || !s_usedTables.Contains(table), 
                    "Nested call with same table can cause data corruption!");
#endif //DEBUG

#if DEBUG
                if (s_usedTables == null)
                    s_usedTables = new List<DataTable>();
                s_usedTables.Add(table);
#endif //DEBUG
                _targetTable = table;
                table.rowDiffId = null;
            }

            [Conditional("DEBUG")]
            internal void Cleanup()
            {
                // cannot assume target table was set - ThreadAbortException can be raised before Start is called
                if (_targetTable != null)
                {
#if DEBUG
                    Debug.Assert(s_usedTables != null && s_usedTables.Contains(_targetTable), "missing Prepare before Cleanup");
                    if (s_usedTables != null)
                    {
                        s_usedTables.Remove(_targetTable);
                        if (s_usedTables.Count == 0)
                            s_usedTables = null;
                    }
#endif //DEBUG
                    _targetTable.rowDiffId = null;
                }
            }

            [Conditional("DEBUG")]
            internal static void Assert(string message)
            {
#if DEBUG
                // this code asserts scope was created, but it does not assert that the table was included in it
                // note that in case of DataSet, new tables might be added to the list in which case they won't appear in s_usedTables.
                Debug.Assert(s_usedTables != null, message);
#endif //DEBUG
            }
        }

        internal struct DSRowDiffIdUsageSection
        {
            DataSet _targetDS;

            internal void Prepare(DataSet ds)
            {
                Debug.Assert(_targetDS == null, "do not reuse this section");
                Debug.Assert(ds != null);

                _targetDS = ds;
#if DEBUG
                // initialize list of tables out of current tables
                // note: it might remain empty (still initialization is needed for assert to operate)
                if (RowDiffIdUsageSection.s_usedTables == null)
                    RowDiffIdUsageSection.s_usedTables = new List<DataTable>();
#endif //DEBUG
                for (int tableIndex = 0; tableIndex < ds.Tables.Count; ++tableIndex)
                {
                    DataTable table = ds.Tables[tableIndex];
#if DEBUG
                    Debug.Assert(!RowDiffIdUsageSection.s_usedTables.Contains(table), "Nested call with same table can cause data corruption!");
                    RowDiffIdUsageSection.s_usedTables.Add(table);
#endif //DEBUG
                    Debug.Assert(table.rowDiffId == null, "rowDiffId wasn't previously cleared");
                    table.rowDiffId = null;
                }
            }

            [Conditional("DEBUG")]
            internal void Cleanup()
            {
                // cannot assume target was set - ThreadAbortException can be raised before Start is called
                if (_targetDS != null)
                {
#if DEBUG
                    Debug.Assert(RowDiffIdUsageSection.s_usedTables != null, "missing Prepare before Cleanup");
#endif //DEBUG

                    for (int tableIndex = 0; tableIndex < _targetDS.Tables.Count; ++tableIndex)
                    {
                        DataTable table = _targetDS.Tables[tableIndex];
#if DEBUG
                        // cannot assert that table exists in the usedTables - new tables might be 
                        // created during diffgram processing in DataSet.ReadXml.
                        if (RowDiffIdUsageSection.s_usedTables != null)
                            RowDiffIdUsageSection.s_usedTables.Remove(table);
#endif //DEBUG
                        table.rowDiffId = null;
                    }
#if DEBUG
                    if (RowDiffIdUsageSection.s_usedTables != null && RowDiffIdUsageSection.s_usedTables.Count == 0)
                        RowDiffIdUsageSection.s_usedTables = null; // out-of-scope
#endif //DEBUG
                }
            }
        }

        internal Hashtable RowDiffId {
            get {
                // assert scope has been created either with RowDiffIdUsageSection.Prepare or DSRowDiffIdUsageSection.Prepare
                RowDiffIdUsageSection.Assert("missing call to RowDiffIdUsageSection.Prepare or DSRowDiffIdUsageSection.Prepare");

                if (rowDiffId == null)
                    rowDiffId = new Hashtable();
                return rowDiffId;
            }
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }

        internal void AddDependentColumn(DataColumn expressionColumn) {
            if (dependentColumns == null)
                dependentColumns = new List<DataColumn>();

            if (!dependentColumns.Contains(expressionColumn)) {
                // only remember unique columns but expect non-unique columns to be added
                dependentColumns.Add(expressionColumn);
            }
        }

        internal void RemoveDependentColumn(DataColumn expressionColumn) {
            if (dependentColumns != null && dependentColumns.Contains(expressionColumn)) {
                dependentColumns.Remove(expressionColumn);
            }
        }

        internal void EvaluateExpressions() {
            //evaluates all expressions for all rows in table
            // SQLBU 414992: Serious performance issue when calling Merge
            // this improves performance by only computing expressions when they are present
            // and iterating over the rows instead of computing their position multiple times
            if ((null != dependentColumns) && (0 < dependentColumns.Count)) {
                foreach(DataRow row in Rows) {
                    // only evaluate original values if different from current.
                    if (row.oldRecord != -1 && row.oldRecord != row.newRecord) {
                        EvaluateDependentExpressions(dependentColumns, row, DataRowVersion.Original, null);
                    }
                    if (row.newRecord != -1) {
                        EvaluateDependentExpressions(dependentColumns, row, DataRowVersion.Current, null);
                    }
                    if (row.tempRecord != -1) {
                        EvaluateDependentExpressions(dependentColumns, row, DataRowVersion.Proposed, null);
                    }
                }
            }
        }

        internal void EvaluateExpressions(DataRow row, DataRowAction action, List<DataRow> cachedRows) {
            // evaluate all expressions for specified row
            if (action == DataRowAction.Add ||
                action == DataRowAction.Change||
                (action == DataRowAction.Rollback && (row.oldRecord!=-1 || row.newRecord!=-1))) {
                 // only evaluate original values if different from current.
                if (row.oldRecord != -1 && row.oldRecord != row.newRecord) {
                    EvaluateDependentExpressions(dependentColumns, row, DataRowVersion.Original, cachedRows);
                }
                if (row.newRecord != -1) {
                    EvaluateDependentExpressions(dependentColumns, row, DataRowVersion.Current, cachedRows);
                }
                if (row.tempRecord != -1) {
                    EvaluateDependentExpressions(dependentColumns, row, DataRowVersion.Proposed, cachedRows);
                }
                return;
            }
            else if ((action == DataRowAction.Delete || (action==DataRowAction.Rollback && row.oldRecord==-1 && row.newRecord==-1)) && dependentColumns != null) {
                foreach(DataColumn col in dependentColumns) {                    
                    if (col.DataExpression != null && col.DataExpression.HasLocalAggregate() && col.Table == this) {
                        for (int j = 0; j < Rows.Count; j++) {
                            DataRow tableRow = Rows[j];

                            if (tableRow.oldRecord != -1 && tableRow.oldRecord != tableRow.newRecord) {
                                EvaluateDependentExpressions(dependentColumns, tableRow, DataRowVersion.Original, null);
                            }                          
                        }
                        for (int j = 0; j < Rows.Count; j++)
                        {
                            DataRow tableRow = Rows[j];
                         
                            if (tableRow.tempRecord != -1)
                            {
                                EvaluateDependentExpressions(dependentColumns, tableRow, DataRowVersion.Proposed, null);
                            }                           
                        }
                        // VSTFDEVDIV911434: Order is important here - we need to update proposed before current
                        // Oherwise rows that are in edit state will get ListChanged/PropertyChanged event before default value is changed
                        // It is also the reason why we are not doping it in the single loop: EvaluateDependentExpression can update the
                        // whole table, if it happens, current for all but first row is updated before proposed value
                        for (int j = 0; j < Rows.Count; j++)
                        {
                            DataRow tableRow = Rows[j];
                                                     
                            if (tableRow.newRecord != -1)
                            {
                                EvaluateDependentExpressions(dependentColumns, tableRow, DataRowVersion.Current, null);
                            }
                        }
                        break;
                    }
                }

                if (cachedRows != null) {
                    foreach (DataRow relatedRow in cachedRows) {
                        if (relatedRow.oldRecord != -1 && relatedRow.oldRecord != relatedRow.newRecord) {
                            relatedRow.Table.EvaluateDependentExpressions(relatedRow.Table.dependentColumns, relatedRow, DataRowVersion.Original, null);
                        }
                        if (relatedRow.newRecord != -1) {
                            relatedRow.Table.EvaluateDependentExpressions(relatedRow.Table.dependentColumns, relatedRow, DataRowVersion.Current, null);
                        }
                        if (relatedRow.tempRecord != -1) {
                            relatedRow.Table.EvaluateDependentExpressions(relatedRow.Table.dependentColumns, relatedRow, DataRowVersion.Proposed, null);
                        }
                    }
                }
            }
        }

        internal void EvaluateExpressions(DataColumn column) {
            // evaluates all rows for expression from specified column
            Debug.Assert(column.Computed, "Only computed columns should be re-evaluated.");
            int count = column.table.Rows.Count;
            if (column.DataExpression.IsTableAggregate() && count > 0) {
                // this value is a constant across the table.
                object aggCurrent = column.DataExpression.Evaluate();
                for (int j = 0; j < count; j++) {
                    DataRow row = column.table.Rows[j];
                    // only evaluate original values if different from current.
                    if (row.oldRecord != -1 && row.oldRecord != row.newRecord) {
                        column[row.oldRecord] = aggCurrent;
                    }
                    if (row.newRecord != -1) {
                        column[row.newRecord] = aggCurrent;
                    }
                    if (row.tempRecord != -1) {
                        column[row.tempRecord] = aggCurrent;
                    }
                }
            }
            else {
                for (int j = 0; j < count; j++) {
                    DataRow row = column.table.Rows[j];

                    if (row.oldRecord != -1 && row.oldRecord != row.newRecord) {
                        column[row.oldRecord] = column.DataExpression.Evaluate(row, DataRowVersion.Original);
                    }
                    if (row.newRecord != -1) {
                        column[row.newRecord] = column.DataExpression.Evaluate(row, DataRowVersion.Current);
                    }
                    if (row.tempRecord != -1) {
                        column[row.tempRecord] = column.DataExpression.Evaluate(row, DataRowVersion.Proposed);
                    }
                }
            }

            // SQLBU 501916 - DataTable internal index is corrupted:'5'
            column.Table.ResetInternalIndexes(column);
            EvaluateDependentExpressions(column);
        }

        internal void EvaluateDependentExpressions(DataColumn column) {
            // DataTable.Clear(), DataRowCollection.Clear() & DataColumn.set_Expression
            if (column.dependentColumns != null) {
                foreach (DataColumn dc in column.dependentColumns) {
                    if ((dc.table != null) && !Object.ReferenceEquals(column, dc)) { // SQLBU 502736
                        EvaluateExpressions(dc);
                    }
                }
            }
        }

        internal void EvaluateDependentExpressions(List<DataColumn> columns, DataRow row, DataRowVersion version, List<DataRow> cachedRows) {
            if (columns == null)
                return;
            //Expression evaluation is done first over same table expressions.
            int count = columns.Count;
            for(int i = 0; i < count; i++) {
                if (columns[i].Table == this) {// if this column is in my table
                    DataColumn dc = columns[i];
                    if (dc.DataExpression != null && dc.DataExpression.HasLocalAggregate()) {
                    // if column expression references a local Table aggregate we need to recalc it for the each row in the local table
                        DataRowVersion expressionVersion  = (version == DataRowVersion.Proposed) ? DataRowVersion.Default : version;
                        bool isConst = dc.DataExpression.IsTableAggregate(); //is expression constant for entire table?
                        object newValue = null;
                        if (isConst) {  //if new value, just compute once
                            newValue = dc.DataExpression.Evaluate(row, expressionVersion);
                        }
                        for (int j = 0; j < Rows.Count; j++) { //evaluate for all rows in the table
                            DataRow dr = Rows[j];
                            if (dr.RowState == DataRowState.Deleted) {
                                continue;
                            }
                            else if (expressionVersion == DataRowVersion.Original && (dr.oldRecord == -1 || dr.oldRecord == dr.newRecord)) {
                                continue;
                            }

                            if (!isConst) {
                                newValue = dc.DataExpression.Evaluate(dr, expressionVersion);
                            }
                            SilentlySetValue(dr, dc, expressionVersion, newValue);
                        }
                    }
                    else {
                        if (row.RowState == DataRowState.Deleted) {
                            continue;
                        }
                        else if (version == DataRowVersion.Original && (row.oldRecord == -1 || row.oldRecord == row.newRecord)) {
                            continue;
                        }
                        SilentlySetValue(row, dc, version, dc.DataExpression == null ? dc.DefaultValue : dc.DataExpression.Evaluate(row, version));
                    }
                }
            }
            // now do expression evaluation for expression columns other tables.
            count = columns.Count;
            for(int i = 0; i < count; i++) {
                DataColumn dc = columns[i];
                // if this column is NOT in my table or it is in the table and is not a local aggregate (self refs)
                if (dc.Table != this || (dc.DataExpression != null && !dc.DataExpression.HasLocalAggregate())) {
                    DataRowVersion foreignVer = (version == DataRowVersion.Proposed) ? DataRowVersion.Default : version;

                    // first - evaluate expressions for cachedRows (deletes & updates)
                    if (cachedRows != null) {
                        foreach (DataRow cachedRow in cachedRows) {
                            if (cachedRow.Table != dc.Table)
                                continue;
                             // don't update original version if child row doesn't have an oldRecord.
                            if (foreignVer == DataRowVersion.Original && cachedRow.newRecord == cachedRow.oldRecord)
                                 continue;
                            if (cachedRow != null && ((cachedRow.RowState != DataRowState.Deleted) && (version != DataRowVersion.Original || cachedRow.oldRecord != -1))) {// if deleted GetRecordFromVersion will throw
                                object newValue = dc.DataExpression.Evaluate(cachedRow, foreignVer);
                                SilentlySetValue(cachedRow, dc, foreignVer, newValue);
                            }
                        }
                    }

                    // next check parent relations
                    for (int j = 0; j < ParentRelations.Count; j++) {
                        DataRelation relation = ParentRelations[j];
                        if (relation.ParentTable != dc.Table)
                            continue;
                        foreach (DataRow parentRow in row.GetParentRows(relation, version)) {
                            if (cachedRows != null && cachedRows.Contains(parentRow))
                                continue;
                             // don't update original version if child row doesn't have an oldRecord.
                            if (foreignVer == DataRowVersion.Original && parentRow.newRecord == parentRow.oldRecord)
                                 continue;
                            if (parentRow != null && ((parentRow.RowState != DataRowState.Deleted) && (version != DataRowVersion.Original || parentRow.oldRecord != -1))) {// if deleted GetRecordFromVersion will throw
                                object newValue = dc.DataExpression.Evaluate(parentRow, foreignVer);
                                SilentlySetValue(parentRow, dc, foreignVer, newValue);
                            }
                        }
                    }
                    // next check child relations
                    for (int j = 0; j < ChildRelations.Count; j++) {
                        DataRelation relation = ChildRelations[j];
                        if (relation.ChildTable != dc.Table)
                            continue;
                        foreach (DataRow childRow in row.GetChildRows(relation, version)) {
                            // don't update original version if child row doesn't have an oldRecord.
                            if (cachedRows != null && cachedRows.Contains(childRow))
                                continue;
                            if (foreignVer == DataRowVersion.Original && childRow.newRecord == childRow.oldRecord)
                                continue;
                            if (childRow != null && ((childRow.RowState != DataRowState.Deleted) && (version != DataRowVersion.Original || childRow.oldRecord != -1))) { // if deleted GetRecordFromVersion will throw
                                object newValue = dc.DataExpression.Evaluate(childRow, foreignVer);
                                SilentlySetValue(childRow, dc, foreignVer, newValue);
                            }
                        }
                    }
                }
            }
        }
    }
}

