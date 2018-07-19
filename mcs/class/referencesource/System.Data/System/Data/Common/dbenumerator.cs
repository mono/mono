//------------------------------------------------------------------------------
// <copyright file="dbenumerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.Collections;
    using System.Data;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.ComponentModel;

    public class DbEnumerator : IEnumerator {

        internal IDataReader _reader;
        internal DbDataRecord _current;
        internal SchemaInfo[] _schemaInfo; // shared schema info among all the data records
        internal PropertyDescriptorCollection _descriptors; // cached property descriptors
        private FieldNameLookup _fieldNameLookup; // MDAC 69015
        private bool closeReader;

        // users must get enumerators off of the datareader interfaces
        public DbEnumerator(IDataReader reader) {
            if (null == reader) {
                throw ADP.ArgumentNull("reader");
            }
            _reader = reader;
        }

        public DbEnumerator(IDataReader reader, bool closeReader) { // MDAC 68670
            if (null == reader) {
                throw ADP.ArgumentNull("reader");
            }
            _reader = reader;
            this.closeReader = closeReader;
        }
        
        public DbEnumerator(DbDataReader reader)
            : this((IDataReader)reader) {
        }

        public DbEnumerator(DbDataReader reader, bool closeReader) 
            : this((IDataReader)reader, closeReader) {
        }

        public object Current {
            get {
                return _current;
            }
        }
        
        public bool MoveNext() {

            if (null == _schemaInfo) {
                  BuildSchemaInfo();
            }

            Debug.Assert(null != _schemaInfo && null != _descriptors, "unable to build schema information!");
            _current = null;

            if (_reader.Read()) {
                // setup our current record
                object[] values = new object[_schemaInfo.Length];
                _reader.GetValues(values); // this.GetValues()
                _current = new DataRecordInternal(_schemaInfo, values, _descriptors, _fieldNameLookup); // this.NewRecord()
                return true;
            }
            if (closeReader) {
                _reader.Close();
            }
            return false;
        }

        [ EditorBrowsableAttribute(EditorBrowsableState.Never) ] // MDAC 69508
        public void Reset() {
            throw ADP.NotSupported();
        }

        private void BuildSchemaInfo() {
            int count = _reader.FieldCount;
            string[] fieldnames = new string[count];
            for (int i = 0; i < count; ++i) {
                fieldnames[i] = _reader.GetName(i);
            }
            ADP.BuildSchemaTableInfoTableNames(fieldnames); // MDAC 71401

            SchemaInfo[] si = new SchemaInfo[count];
            PropertyDescriptor[] props = new PropertyDescriptor[_reader.FieldCount];
            for (int i = 0; i < si.Length; i++) {
                SchemaInfo s = new SchemaInfo();
                s.name = _reader.GetName(i);
                s.type = _reader.GetFieldType(i);
                s.typeName = _reader.GetDataTypeName(i);
                props[i] = new DbColumnDescriptor(i, fieldnames[i], s.type);
                si[i] = s;
            }

            _schemaInfo = si;
            _fieldNameLookup = new FieldNameLookup(_reader, -1); // MDAC 71470
            _descriptors = new PropertyDescriptorCollection(props);
        }

        sealed private class DbColumnDescriptor : PropertyDescriptor {
            int _ordinal;
            Type _type;

            internal DbColumnDescriptor(int ordinal, string name, Type type)
                : base(name, null) {
                _ordinal = ordinal;
                _type = type;
            }

            public override Type ComponentType {
                get {
                    return typeof(IDataRecord);
                }
            }

            public override bool IsReadOnly {
                get {
                    return true;
                }
            }

            public override Type PropertyType {
                get {
                    return _type;
                }
            }

            public override bool CanResetValue(object component) {
                return false;
            }

            public override object GetValue(object component) {
                return ((IDataRecord)component)[_ordinal];
            }

            public override void ResetValue(object component) {
                throw ADP.NotSupported();
            }

            public override void SetValue(object component, object value) {
                throw ADP.NotSupported();
            }

            public override bool ShouldSerializeValue(object component) {
                return false;
            }
        }
    }
}
