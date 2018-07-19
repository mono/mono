//------------------------------------------------------------------------------
// <copyright file="SqlTriggerContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="true" primary="false">daltodov</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Diagnostics;

    public sealed class SqlTriggerContext {

        TriggerAction   _triggerAction;
        bool[]          _columnsUpdated;
        SqlXml          _eventInstanceData;

        internal SqlTriggerContext(TriggerAction triggerAction, bool[] columnsUpdated, SqlXml eventInstanceData) {
            _triggerAction     = triggerAction;
            _columnsUpdated    = columnsUpdated;
            _eventInstanceData = eventInstanceData;
        }

        public int ColumnCount {
            get {
                int result = 0;

                if (null != _columnsUpdated) {
                    result = _columnsUpdated.Length;
                }
                return result;
            }
        }

        public SqlXml EventData {
            get {
                return _eventInstanceData;
            }
        }

        public TriggerAction TriggerAction {
            get {
                return _triggerAction;
            }
        }

        public bool IsUpdatedColumn(int columnOrdinal) {
            if (null != _columnsUpdated) {
                return _columnsUpdated[columnOrdinal];   // will throw IndexOutOfRangeException if it's out of range...
            }
            throw ADP.IndexOutOfRange(columnOrdinal);    // if there aren't any columns, that means IndexOutOfRange too...
        }
    }
}
