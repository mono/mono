//------------------------------------------------------------------------------
// <copyright file="RowUpdatedEventArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.ComponentModel;
    using System.Collections;
    using System.Data;

/*
    public delegate void RowUpdatedEventHandler(object sender, RowUpdatedEventArgs e);
*/

    public class RowUpdatedEventArgs : System.EventArgs {
        private IDbCommand _command;
        private StatementType _statementType;
        private DataTableMapping _tableMapping;
        private Exception _errors;

        private DataRow _dataRow;
        private DataRow[] _dataRows;

        private UpdateStatus _status; // UpdateStatus.Continue; /*0*/
        private int _recordsAffected;

        public RowUpdatedEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) {
            switch(statementType) {
            case StatementType.Select:
            case StatementType.Insert:
            case StatementType.Update:
            case StatementType.Delete:
            case StatementType.Batch:
                break;
            default:
                throw ADP.InvalidStatementType(statementType);
            }
            _dataRow = dataRow;
            _command = command;
            _statementType = statementType;
            _tableMapping = tableMapping;
        }

        public IDbCommand Command {
            get {
                return _command;
            }
        }

        public Exception Errors {
            get {
                return _errors;
            }
            set {
                _errors = value;
            }
        }

        public int RecordsAffected {
            get {
                return _recordsAffected;
            }
        }

        public DataRow Row {
            get {                
                return _dataRow;
            }
        }

        internal DataRow[] Rows {
            get {
                return _dataRows;
            }
        }

        public int RowCount {
            get {
                DataRow[] dataRows = _dataRows;
                return ((null != dataRows) ? dataRows.Length : ((null != _dataRow) ? 1 : 0));
            }
        }

        public StatementType StatementType {
            get {
                return _statementType;
            }
        }
        
        public UpdateStatus Status {
            get {
                return _status;
            }
            set {
                switch(value) {
                case UpdateStatus.Continue:
                case UpdateStatus.ErrorsOccurred:
                case UpdateStatus.SkipCurrentRow:
                case UpdateStatus.SkipAllRemainingRows:
                    _status = value;
                    break;
                default:
                    throw ADP.InvalidUpdateStatus(value);
                }
            }
        }

        public DataTableMapping TableMapping {
            get {
                return _tableMapping;
            }
        }

        internal void AdapterInit(DataRow[] dataRows) {
            _statementType = StatementType.Batch;
            _dataRows = dataRows;

            if ((null != dataRows) && (1 == dataRows.Length)) { // WebData 100063
                _dataRow = dataRows[0];
            }
        }

        internal void AdapterInit(int recordsAffected) {
            _recordsAffected = recordsAffected;
        }

        public void CopyToRows(DataRow[] array) {
            CopyToRows(array, 0);
        }

        public void CopyToRows(DataRow[] array, int arrayIndex) {
            DataRow[] dataRows = _dataRows;
            if (null != dataRows) {
                dataRows.CopyTo(array, arrayIndex);
            }
            else {
                if (null == array) {
                    throw ADP.ArgumentNull("array");
                }
                array[arrayIndex] = Row;
            }
        }
    }
}
