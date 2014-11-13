//------------------------------------------------------------------------------
// <copyright file="RowUpdatingEventArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.Data;

/*
    public delegate void RowUpdatingEventHandler(object sender, RowUpdatingEventArgs e);
*/

    public class RowUpdatingEventArgs : System.EventArgs {
        private IDbCommand _command;
        private StatementType _statementType;
        private DataTableMapping _tableMapping;
        private Exception _errors;

        private DataRow _dataRow;
        private UpdateStatus _status; // UpdateStatus.Continue; /*0*/

        public RowUpdatingEventArgs(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) {
            ADP.CheckArgumentNull(dataRow, "dataRow");
            ADP.CheckArgumentNull(tableMapping, "tableMapping");
            switch(statementType) {
            case StatementType.Select:
            case StatementType.Insert:
            case StatementType.Update:
            case StatementType.Delete:
                break;
            case StatementType.Batch:
                throw ADP.NotSupportedStatementType(statementType, "RowUpdatingEventArgs");                
            default:
                throw ADP.InvalidStatementType(statementType);
            }
            _dataRow = dataRow;
            _command = command; // maybe null
            _statementType = statementType;
            _tableMapping = tableMapping; 
        }

        // 
        virtual protected IDbCommand BaseCommand {
            get {
                return _command;
            }
            set {
                _command = value;
            }
        }

        public IDbCommand Command {
            get {
                return BaseCommand;
            }
            set {
                BaseCommand = value;
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

        public DataRow Row {
            get {
                return _dataRow;
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
    }
}
