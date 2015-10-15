//------------------------------------------------------------------------------
// <copyright file="SqlBulkCopyColumnMapping.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

// Todo: rename the file
// Caution! ndp\fx\src\data\netmodule\sources needs to follow this change

namespace System.Data.SqlClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.ComponentModel;

    using System.Collections;
    using System.Diagnostics;

    // -------------------------------------------------------------------------------------------------
    // this class helps allows the user to create association between source- and targetcolumns
    //
    //

    public sealed class SqlBulkCopyColumnMapping {
        internal string         _destinationColumnName;
        internal int            _destinationColumnOrdinal;
        internal string         _sourceColumnName;
        internal int            _sourceColumnOrdinal;

        // devnote: we don't want the user to detect the columnordinal after WriteToServer call.
        // _sourceColumnOrdinal(s) will be copied to _internalSourceColumnOrdinal when WriteToServer executes.
        internal int            _internalDestinationColumnOrdinal;
        internal int            _internalSourceColumnOrdinal;   // -1 indicates an undetermined value

        public string DestinationColumn {
            get {
                if (_destinationColumnName != null) {
                    return _destinationColumnName;
                }
                return string.Empty;
            }
            set {
                _destinationColumnOrdinal = _internalDestinationColumnOrdinal = -1;
                _destinationColumnName = value;
            }
        }

        public int DestinationOrdinal {
            get {
                    return _destinationColumnOrdinal;
            }
            set {
                if (value >= 0) {
                    _destinationColumnName = null;
                    _destinationColumnOrdinal = _internalDestinationColumnOrdinal = value;
                }
                else {
                    throw ADP.IndexOutOfRange(value);
                }
            }
        }

        public string SourceColumn {
            get {
                if (_sourceColumnName != null) {
                    return _sourceColumnName;
                }
                return string.Empty;
            }
            set {
                _sourceColumnOrdinal = _internalSourceColumnOrdinal = -1;
                _sourceColumnName = value;
            }
        }

        public int SourceOrdinal {
            get {
                    return _sourceColumnOrdinal;
            }
            set {
                if (value >= 0) {
                    _sourceColumnName = null;
                    _sourceColumnOrdinal = _internalSourceColumnOrdinal = value;
                }
                else {
                    throw ADP.IndexOutOfRange(value);
                }
            }
        }

        public SqlBulkCopyColumnMapping () {
            _internalSourceColumnOrdinal = -1;
        }

        public SqlBulkCopyColumnMapping (string sourceColumn, string destinationColumn) {
            SourceColumn = sourceColumn;
            DestinationColumn = destinationColumn;
        }

        public SqlBulkCopyColumnMapping (int sourceColumnOrdinal, string destinationColumn) {
            SourceOrdinal = sourceColumnOrdinal;
            DestinationColumn = destinationColumn;
        }

        public SqlBulkCopyColumnMapping (string sourceColumn, int destinationOrdinal) {
            SourceColumn = sourceColumn;
            DestinationOrdinal = destinationOrdinal;
        }

        public SqlBulkCopyColumnMapping (int sourceColumnOrdinal, int destinationOrdinal) {
            SourceOrdinal = sourceColumnOrdinal;
            DestinationOrdinal = destinationOrdinal;
        }
    }
}
