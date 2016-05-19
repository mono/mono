//------------------------------------------------------------------------------
// <copyright file="DataTableMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    [
    System.ComponentModel.TypeConverterAttribute(typeof(System.Data.Common.DataTableMapping.DataTableMappingConverter))
    ]
    public sealed class DataTableMapping : MarshalByRefObject, ITableMapping, ICloneable {
        private DataTableMappingCollection parent;
        private DataColumnMappingCollection _columnMappings;
        private string _dataSetTableName;
        private string _sourceTableName;

        public DataTableMapping() {
        }

        public DataTableMapping(string sourceTable, string dataSetTable) {
            SourceTable = sourceTable;
            DataSetTable = dataSetTable;
        }

        public DataTableMapping(string sourceTable, string dataSetTable, DataColumnMapping[] columnMappings) {
            SourceTable = sourceTable;
            DataSetTable = dataSetTable;
            if ((null != columnMappings) && (0 < columnMappings.Length)) {
                ColumnMappings.AddRange(columnMappings);
            }
        }

        // explict ITableMapping implementation
        IColumnMappingCollection ITableMapping.ColumnMappings {
            get { return ColumnMappings; }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ResCategoryAttribute(Res.DataCategory_Mapping),
        ResDescriptionAttribute(Res.DataTableMapping_ColumnMappings),
        ]
        public DataColumnMappingCollection ColumnMappings {
            get {
                DataColumnMappingCollection columnMappings = _columnMappings;
                if (null == columnMappings) {
                    columnMappings = new DataColumnMappingCollection();
                    _columnMappings = columnMappings;
                }
                return columnMappings;
            }
        }

        [
        DefaultValue(""),
        ResCategoryAttribute(Res.DataCategory_Mapping),
        ResDescriptionAttribute(Res.DataTableMapping_DataSetTable),
        ]
        public string DataSetTable {
            get {
                string dataSetTableName = _dataSetTableName;
                return ((null != dataSetTableName) ? dataSetTableName : ADP.StrEmpty);
            }
            set {
                _dataSetTableName = value;
            }
        }
        
        internal DataTableMappingCollection Parent {
            get {
                return parent;
            }
            set {
                parent = value;
            }
        }

        [
        DefaultValue(""),
        ResCategoryAttribute(Res.DataCategory_Mapping),
        ResDescriptionAttribute(Res.DataTableMapping_SourceTable),
        ]
        public string SourceTable {
            get {
                string sourceTableName = _sourceTableName;
                return ((null != sourceTableName) ? sourceTableName : ADP.StrEmpty);
            }
            set {
                if ((null != Parent) && (0 != ADP.SrcCompare(_sourceTableName, value))) {
                    Parent.ValidateSourceTable(-1, value);
                }
                _sourceTableName = value;
            }
        }

        object ICloneable.Clone() {
            DataTableMapping clone = new DataTableMapping(); // MDAC 81448
            clone._dataSetTableName = _dataSetTableName;
            clone._sourceTableName = _sourceTableName;

            if ((null != _columnMappings) && (0 < ColumnMappings.Count)) {
                DataColumnMappingCollection parameters = clone.ColumnMappings;
                foreach(ICloneable parameter in ColumnMappings) {
                    parameters.Add(parameter.Clone());
                }
            }
            return clone;
        }

        [ EditorBrowsableAttribute(EditorBrowsableState.Advanced) ] // MDAC 69508
        public DataColumn GetDataColumn(string sourceColumn, Type dataType, DataTable dataTable, MissingMappingAction mappingAction, MissingSchemaAction schemaAction) {
            return DataColumnMappingCollection.GetDataColumn(_columnMappings, sourceColumn, dataType, dataTable, mappingAction, schemaAction);
        }
        
        [ EditorBrowsableAttribute(EditorBrowsableState.Advanced) ] // MDAC 69508
        public DataColumnMapping GetColumnMappingBySchemaAction(string sourceColumn, MissingMappingAction mappingAction) {
            return DataColumnMappingCollection.GetColumnMappingBySchemaAction(_columnMappings, sourceColumn, mappingAction);
        }

        [ EditorBrowsableAttribute(EditorBrowsableState.Advanced) ] // MDAC 69508
        public DataTable GetDataTableBySchemaAction(DataSet dataSet, MissingSchemaAction schemaAction) {
            if (null == dataSet) {
                throw ADP.ArgumentNull("dataSet");
            }
            string dataSetTable = DataSetTable;

            if (ADP.IsEmpty(dataSetTable)) {
#if DEBUG
                if (AdapterSwitches.DataSchema.TraceWarning) {
                    Debug.WriteLine("explicit filtering of SourceTable \"" + SourceTable + "\"");
                }
#endif
                return null;
            }
            DataTableCollection tables = dataSet.Tables;
            int index = tables.IndexOf(dataSetTable);
            if ((0 <= index) && (index < tables.Count)) {
#if DEBUG
                if (AdapterSwitches.DataSchema.TraceInfo) {
                    Debug.WriteLine("schema match on DataTable \"" + dataSetTable);
                }
#endif
                return tables[index];
            }
            switch (schemaAction) {
                case MissingSchemaAction.Add:
                case MissingSchemaAction.AddWithKey:
#if DEBUG
                    if (AdapterSwitches.DataSchema.TraceInfo) {
                        Debug.WriteLine("schema add of DataTable \"" + dataSetTable + "\"");
                    }
#endif
                    return new DataTable(dataSetTable);

                case MissingSchemaAction.Ignore:
#if DEBUG
                    if (AdapterSwitches.DataSchema.TraceWarning) {
                        Debug.WriteLine("schema filter of DataTable \"" + dataSetTable + "\"");
                    }
#endif
                    return null;

                case MissingSchemaAction.Error:
#if DEBUG
                    if (AdapterSwitches.DataSchema.TraceError) {
                        Debug.WriteLine("schema error on DataTable \"" + dataSetTable + "\"");
                    }
#endif
                    throw ADP.MissingTableSchema(dataSetTable, SourceTable);
            }
            throw ADP.InvalidMissingSchemaAction(schemaAction);
        }

        public override String ToString() {
            return SourceTable;
        }

        sealed internal class DataTableMappingConverter : System.ComponentModel.ExpandableObjectConverter {

            // converter classes should have public ctor
            public DataTableMappingConverter() {
            }

            override public bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
                if (typeof(InstanceDescriptor) == destinationType) {
                    return true;
                }
                return base.CanConvertTo(context, destinationType);
            }

            override public object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (null == destinationType) {
                    throw ADP.ArgumentNull("destinationType");
                }

                if ((typeof(InstanceDescriptor) == destinationType) && (value is DataTableMapping)) {
                    DataTableMapping mapping = (DataTableMapping)value;

                    DataColumnMapping[] columnMappings = new DataColumnMapping[mapping.ColumnMappings.Count];
                    mapping.ColumnMappings.CopyTo(columnMappings, 0);
                    object[] values = new object[] { mapping.SourceTable, mapping.DataSetTable, columnMappings};
                    Type[] types = new Type[] { typeof(string), typeof(string), typeof(DataColumnMapping[]) };

                    ConstructorInfo ctor = typeof(DataTableMapping).GetConstructor(types);
                    return new InstanceDescriptor(ctor, values);
                }            
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
