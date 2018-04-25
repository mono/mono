//------------------------------------------------------------------------------
// <copyright file="DBConcurrencyException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class DBConcurrencyException  : SystemException {
        private DataRow[] _dataRows;

        public DBConcurrencyException() : this(Res.GetString(Res.ADP_DBConcurrencyExceptionMessage), null) { // MDAC 84941
        }

        public DBConcurrencyException(string message) : this(message, null) {
        }

        public DBConcurrencyException(string message, Exception inner) : base(message, inner) {
            HResult = HResults.DBConcurrency; // MDAC 84941
        }

        public DBConcurrencyException(string message, Exception inner, DataRow[] dataRows) : base(message, inner) {
            HResult = HResults.DBConcurrency; // MDAC 84941
            _dataRows = dataRows;
        }

        // runtime will call even if private...
        private DBConcurrencyException(SerializationInfo si, StreamingContext sc) : base(si, sc) {
            // dataRow = (DataRow) si.GetValue("dataRow", typeof(DataRow)); - do not do this till v.next with serialization support for DataRow.  MDAC 72136
        }

        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.SerializationFormatter)]
        override public void GetObjectData(SerializationInfo si, StreamingContext context) { // MDAC 72003
            if (null == si) {
                throw new ArgumentNullException("si");
            }
            // si.AddValue("dataRow", dataRow, typeof(DataRow)); - do not do this till v.next with serialization support for DataRow.    MDAC 72136
            base.GetObjectData(si, context);
        }

        public DataRow Row { // MDAC 55735
            get {
                DataRow[] rows = _dataRows;
                return (((null != rows) && (0 < rows.Length)) ? rows[0] : null);
            }
            set {
                _dataRows = new DataRow[1] { value };
            }
        }
        
        public int RowCount {
            get {
                DataRow[] dataRows = _dataRows;
                return ((null != dataRows) ? dataRows.Length : 0);
            }
        }
        
        public void CopyToRows(DataRow[] array) {
            CopyToRows(array, 0);
        }
        
        public void CopyToRows(DataRow[] array, int arrayIndex) {
            DataRow[] dataRows = _dataRows;
            if (null != dataRows) {
                dataRows.CopyTo(array, arrayIndex);
            }
        }
    }
}
