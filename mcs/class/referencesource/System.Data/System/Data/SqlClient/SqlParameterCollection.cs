//------------------------------------------------------------------------------
// <copyright file="SqlParameterCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;

    [
    Editor("Microsoft.VSDesigner.Data.Design.DBParametersEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    ListBindable(false)
    ]
    public sealed partial class SqlParameterCollection : DbParameterCollection {
        private bool _isDirty;
        private static Type ItemType = typeof(SqlParameter);

        internal SqlParameterCollection() : base() {
        }

        internal bool IsDirty {
            get {
                return _isDirty;
            }
            set {
                _isDirty = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        new public SqlParameter this[int index] {
            get {
                return (SqlParameter)GetParameter(index);
            }
            set {
                SetParameter(index, value);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        new public SqlParameter this[string parameterName] {
            get {
                 return (SqlParameter)GetParameter(parameterName);
            }
            set {
                 SetParameter(parameterName, value);
            }
        }

        public SqlParameter Add(SqlParameter value) {
            Add((object)value);
            return value;
        }

        [ EditorBrowsableAttribute(EditorBrowsableState.Never) ] 
        [ ObsoleteAttribute("Add(String parameterName, Object value) has been deprecated.  Use AddWithValue(String parameterName, Object value).  http://go.microsoft.com/fwlink/?linkid=14202", false) ] // 79027
        public SqlParameter Add(string parameterName, object value) {
            return Add(new SqlParameter(parameterName, value));
        }
        public SqlParameter AddWithValue(string parameterName, object value) { // 79027
            return Add(new SqlParameter(parameterName, value));
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType) {
            return Add(new SqlParameter(parameterName, sqlDbType));
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size) {
            return Add(new SqlParameter(parameterName, sqlDbType, size));
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size, string sourceColumn) {
            return Add(new SqlParameter(parameterName, sqlDbType, size, sourceColumn));
        }

        public void AddRange(SqlParameter[] values) {
            AddRange((Array)values);
        }

        override public bool Contains(string value) { // WebData 97349
            return (-1 != IndexOf(value));
        }

        public bool Contains(SqlParameter value) {
            return (-1 != IndexOf(value));
        }

        public void CopyTo(SqlParameter[] array, int index) {
            CopyTo((Array)array, index);
        }
        
        public int IndexOf(SqlParameter value) {
            return IndexOf((object)value);
        }
    
        public void Insert(int index, SqlParameter value) {
            Insert(index, (object)value);
        }

        private void OnChange() {
            IsDirty = true;
        }

        public void Remove(SqlParameter value) {
            Remove((object)value);
        }    

    }
}
