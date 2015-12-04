//------------------------------------------------------------------------------
// <copyright file="DataException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;

    // [....]: This functions are major point of localization.
    // We need to have a rules to enforce consistency there.
    // The dangerous point there are the string arguments of the exported (internal) methods.
    // This string can be argument, table or constraint name but never text of exception itself.
    // Make an invariant that all texts of exceptions coming from resources only.

    [Serializable]
    public class DataException : SystemException {
        protected DataException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public DataException()
        : base(Res.GetString(Res.DataSet_DefaultDataException)) {
            HResult = HResults.Data;
        }

        public DataException(string s)
        : base(s) {
            HResult = HResults.Data;
        }

        public DataException(string s, Exception innerException)
        : base(s, innerException) {
        }

    };

    [Serializable]
    public class ConstraintException : DataException {
        protected ConstraintException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public ConstraintException()         : base(Res.GetString(Res.DataSet_DefaultConstraintException)) {
            HResult = HResults.DataConstraint;
        }
        public ConstraintException(string s) : base(s) {
            HResult = HResults.DataConstraint;
        }

        public ConstraintException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataConstraint;
        }
    }

    [Serializable]
    public class DeletedRowInaccessibleException : DataException {
        protected DeletedRowInaccessibleException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DeletedRowInaccessibleException'/> class.
        ///    </para>
        /// </devdoc>
        public DeletedRowInaccessibleException() : base(Res.GetString(Res.DataSet_DefaultDeletedRowInaccessibleException)) {
            HResult = HResults.DataDeletedRowInaccessible;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Data.DeletedRowInaccessibleException'/> class with the specified string.
        ///    </para>
        /// </devdoc>
        public DeletedRowInaccessibleException(string s) : base(s) {
            HResult = HResults.DataDeletedRowInaccessible;
        }

        public DeletedRowInaccessibleException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataDeletedRowInaccessible;
        }
    }

    [Serializable]
    public class DuplicateNameException : DataException {
        protected DuplicateNameException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public DuplicateNameException() : base(Res.GetString(Res.DataSet_DefaultDuplicateNameException)) {
            HResult = HResults.DataDuplicateName;
        }

        public DuplicateNameException(string s) : base(s) {
            HResult = HResults.DataDuplicateName;
        }

        public DuplicateNameException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataDuplicateName;
        }
    }

    [Serializable]
    public class InRowChangingEventException : DataException {
        protected InRowChangingEventException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public InRowChangingEventException() : base(Res.GetString(Res.DataSet_DefaultInRowChangingEventException)) {
            HResult = HResults.DataInRowChangingEvent;
        }

        public InRowChangingEventException(string s) : base(s) {
            HResult = HResults.DataInRowChangingEvent;
        }

        public InRowChangingEventException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataInRowChangingEvent;
        }
    }

    [Serializable]
    public class InvalidConstraintException : DataException {
        protected InvalidConstraintException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public InvalidConstraintException() : base(Res.GetString(Res.DataSet_DefaultInvalidConstraintException)) {
            HResult = HResults.DataInvalidConstraint;
        }

        public InvalidConstraintException(string s) : base(s) {
            HResult = HResults.DataInvalidConstraint;
        }

        public InvalidConstraintException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataInvalidConstraint;
        }
    }

    [Serializable]
    public class MissingPrimaryKeyException : DataException {
        protected MissingPrimaryKeyException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public MissingPrimaryKeyException() : base(Res.GetString(Res.DataSet_DefaultMissingPrimaryKeyException)) {
            HResult = HResults.DataMissingPrimaryKey;
        }

        public MissingPrimaryKeyException(string s) : base(s) {
            HResult = HResults.DataMissingPrimaryKey;
        }

        public MissingPrimaryKeyException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataMissingPrimaryKey;
        }
    }

    [Serializable]
    public class NoNullAllowedException : DataException {
        protected NoNullAllowedException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public NoNullAllowedException() : base(Res.GetString(Res.DataSet_DefaultNoNullAllowedException)) {
            HResult = HResults.DataNoNullAllowed;
        }

        public NoNullAllowedException(string s) : base(s) {
            HResult = HResults.DataNoNullAllowed;
        }

        public NoNullAllowedException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataNoNullAllowed;
        }
    }

    [Serializable]
    public class ReadOnlyException : DataException {
        protected ReadOnlyException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public ReadOnlyException() : base(Res.GetString(Res.DataSet_DefaultReadOnlyException)) {
            HResult = HResults.DataReadOnly;
        }

        public ReadOnlyException(string s) : base(s) {
            HResult = HResults.DataReadOnly;
        }

        public ReadOnlyException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataReadOnly;
        }
    }

    [Serializable]
    public class RowNotInTableException : DataException {
        protected RowNotInTableException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public RowNotInTableException() : base(Res.GetString(Res.DataSet_DefaultRowNotInTableException)) {
            HResult = HResults.DataRowNotInTable;
        }

        public RowNotInTableException(string s) : base(s) {
            HResult = HResults.DataRowNotInTable;
        }

        public RowNotInTableException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataRowNotInTable;
        }
    }

    [Serializable]
    public class VersionNotFoundException : DataException {
        protected VersionNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        public VersionNotFoundException() : base(Res.GetString(Res.DataSet_DefaultVersionNotFoundException)) {
            HResult = HResults.DataVersionNotFound;
        }

        public VersionNotFoundException(string s) : base(s) {
            HResult = HResults.DataVersionNotFound;
        }

        public VersionNotFoundException(string message, Exception innerException)  : base(message, innerException) {
            HResult = HResults.DataVersionNotFound;
        }
    }

    internal static class ExceptionBuilder {
        // The class defines the exceptions that are specific to the DataSet.
        // The class contains functions that take the proper informational variables and then construct
        // the appropriate exception with an error string obtained from the resource Data.txt.
        // The exception is then returned to the caller, so that the caller may then throw from its
        // location so that the catcher of the exception will have the appropriate call stack.
        // This class is used so that there will be compile time checking of error messages.
        // The resource Data.txt will ensure proper string text based on the appropriate
        // locale.

        [BidMethod] // this method accepts BID format as an argument, this attribute allows FXCopBid rule to validate calls to it
        static private void TraceException(
                string trace, 
                [BidArgumentType(typeof(String))] Exception e) {
            Debug.Assert(null != e, "TraceException: null Exception");
            if (null != e) {
                Bid.Trace(trace, e.Message);
                if (Bid.AdvancedOn) {
                    try {
                        Bid.Trace(", StackTrace='%ls'", Environment.StackTrace);
                    }
                    catch(System.Security.SecurityException) {
                        // if you don't have permission - you don't get the stack trace
                    }
                }
                Bid.Trace("\n");
            }
        }

        static internal void TraceExceptionAsReturnValue(Exception e) {
            TraceException("<comm.ADP.TraceException|ERR|THROW> Message='%ls'", e);
        }
        static internal void TraceExceptionForCapture(Exception e) {
            TraceException("<comm.ADP.TraceException|ERR|CATCH> Message='%ls'", e);
        }
        static internal void TraceExceptionWithoutRethrow(Exception e) {
            TraceException("<comm.ADP.TraceException|ERR|CATCH> Message='%ls'", e);
        }

        //
        // COM+ exceptions
        //
        static internal ArgumentException _Argument(string error) {
            ArgumentException e = new ArgumentException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static internal ArgumentException _Argument(string paramName, string error) {
            ArgumentException e = new ArgumentException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static internal ArgumentException _Argument(string error, Exception innerException) {
            ArgumentException e = new ArgumentException(error, innerException);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private ArgumentNullException _ArgumentNull(string paramName, string msg) {
            ArgumentNullException e = new ArgumentNullException(paramName, msg);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static internal ArgumentOutOfRangeException _ArgumentOutOfRange(string paramName, string msg) {
            ArgumentOutOfRangeException e = new ArgumentOutOfRangeException(paramName, msg);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private IndexOutOfRangeException _IndexOutOfRange(string error) {
            IndexOutOfRangeException e = new IndexOutOfRangeException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private InvalidOperationException _InvalidOperation(string error) {
            InvalidOperationException e = new InvalidOperationException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }

        static private InvalidEnumArgumentException _InvalidEnumArgumentException(string error) {
            InvalidEnumArgumentException e = new InvalidEnumArgumentException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }

        static private InvalidEnumArgumentException _InvalidEnumArgumentException<T>(T value) {
            string msg = Res.GetString(Res.ADP_InvalidEnumerationValue, typeof(T).Name, value.ToString());
            return _InvalidEnumArgumentException(msg);
        }

        //
        // System.Data exceptions
        //
        static private DataException _Data(string error) {
            DataException e = new DataException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        /// <summary>trace and throw a DataException</summary>
        /// <param name="error">exception Message</param>
        /// <param name="innerException">exception InnerException</param>
        /// <exception cref="DataException">always thrown</exception>
        static private void ThrowDataException(string error, Exception innerException)
        {
            DataException e = new DataException(error, innerException);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            throw e;
        }
        static private ConstraintException _Constraint(string error) {
            ConstraintException e = new ConstraintException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private InvalidConstraintException _InvalidConstraint(string error) {
            InvalidConstraintException e = new InvalidConstraintException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private DeletedRowInaccessibleException _DeletedRowInaccessible(string error) {
            DeletedRowInaccessibleException e = new DeletedRowInaccessibleException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private DuplicateNameException _DuplicateName(string error) {
            DuplicateNameException e = new DuplicateNameException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private InRowChangingEventException _InRowChangingEvent(string error) {
            InRowChangingEventException e = new InRowChangingEventException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private MissingPrimaryKeyException _MissingPrimaryKey(string error) {
            MissingPrimaryKeyException e = new MissingPrimaryKeyException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private NoNullAllowedException _NoNullAllowed(string error) {
            NoNullAllowedException e = new NoNullAllowedException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private ReadOnlyException _ReadOnly(string error) {
            ReadOnlyException e = new ReadOnlyException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private RowNotInTableException _RowNotInTable(string error) {
            RowNotInTableException e = new RowNotInTableException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private VersionNotFoundException _VersionNotFound(string error) {
            VersionNotFoundException e = new VersionNotFoundException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }


        // Consider: whether we need to keep our own texts from Data_ArgumentNull and Data_ArgumentOutOfRange?
        // Unfortunately ours and the system ones are not consisten between each other. Try to raise this isue in "URT user comunity"
        static public Exception ArgumentNull(string paramName) {
            return _ArgumentNull(paramName, Res.GetString(Res.Data_ArgumentNull, paramName));
        }
        static public Exception ArgumentOutOfRange(string paramName) {
            return _ArgumentOutOfRange(paramName, Res.GetString(Res.Data_ArgumentOutOfRange, paramName));
        }
        static public Exception BadObjectPropertyAccess(string error) {
            return _InvalidOperation(Res.GetString(Res.DataConstraint_BadObjectPropertyAccess, error));
        }
        static public Exception ArgumentContainsNull(string paramName) {
            return _Argument(paramName, Res.GetString(Res.Data_ArgumentContainsNull, paramName));
        }


        //
        // Collections
        //

        static public Exception CannotModifyCollection() {
            return _Argument(Res.GetString(Res.Data_CannotModifyCollection));
        }
        static public Exception CaseInsensitiveNameConflict(string name) {
            return _Argument(Res.GetString(Res.Data_CaseInsensitiveNameConflict, name));
        }
        static public Exception NamespaceNameConflict(string name) {
            return _Argument(Res.GetString(Res.Data_NamespaceNameConflict, name));
        }
        static public Exception InvalidOffsetLength() {
            return _Argument(Res.GetString(Res.Data_InvalidOffsetLength));
        }

        //
        // DataColumnCollection
        //

        static public Exception ColumnNotInTheTable(string column, string table) {
            return _Argument(Res.GetString(Res.DataColumn_NotInTheTable, column, table));
        }

        static public Exception ColumnNotInAnyTable() {
            return _Argument(Res.GetString(Res.DataColumn_NotInAnyTable));
        }

        static public Exception ColumnOutOfRange(int index) {
            return _IndexOutOfRange(Res.GetString(Res.DataColumns_OutOfRange, (index).ToString(CultureInfo.InvariantCulture)));
        }
        static public Exception ColumnOutOfRange(string column) {
            return _IndexOutOfRange(Res.GetString(Res.DataColumns_OutOfRange, column));
        }

        static public Exception CannotAddColumn1(string column) {
            return _Argument(Res.GetString(Res.DataColumns_Add1, column));
        }

        static public Exception CannotAddColumn2(string column) {
            return _Argument(Res.GetString(Res.DataColumns_Add2, column));
        }

        static public Exception CannotAddColumn3() {
            return _Argument(Res.GetString(Res.DataColumns_Add3));
        }

        static public Exception CannotAddColumn4(string column) {
            return _Argument(Res.GetString(Res.DataColumns_Add4, column));
        }

        static public Exception CannotAddDuplicate(string column) {
            return _DuplicateName(Res.GetString(Res.DataColumns_AddDuplicate, column));
        }

        static public Exception CannotAddDuplicate2(string table) {
            return _DuplicateName(Res.GetString(Res.DataColumns_AddDuplicate2, table));
        }

        static public Exception CannotAddDuplicate3(string table) {
            return _DuplicateName(Res.GetString(Res.DataColumns_AddDuplicate3, table));
        }

        static public Exception CannotRemoveColumn() {
            return _Argument(Res.GetString(Res.DataColumns_Remove));
        }

        static public Exception CannotRemovePrimaryKey() {
            return _Argument(Res.GetString(Res.DataColumns_RemovePrimaryKey));
        }

        static public Exception CannotRemoveChildKey(string relation) {
            return _Argument(Res.GetString(Res.DataColumns_RemoveChildKey, relation));
        }

        static public Exception CannotRemoveConstraint(string constraint, string table) {
            return _Argument(Res.GetString(Res.DataColumns_RemoveConstraint, constraint, table));
        }

        static public Exception CannotRemoveExpression(string column, string expression) {
            return _Argument(Res.GetString(Res.DataColumns_RemoveExpression, column, expression));
        }

        static public Exception ColumnNotInTheUnderlyingTable(string column, string table) {
            return _Argument(Res.GetString(Res.DataColumn_NotInTheUnderlyingTable, column, table));
        }

        static public Exception InvalidOrdinal(string name, int ordinal) {
            return _ArgumentOutOfRange(name, Res.GetString(Res.DataColumn_OrdinalExceedMaximun, (ordinal).ToString(CultureInfo.InvariantCulture)));
        }

        //
        // _Constraint and ConstrainsCollection
        //

        static public Exception AddPrimaryKeyConstraint() {
            return _Argument(Res.GetString(Res.DataConstraint_AddPrimaryKeyConstraint));
        }

        static public Exception NoConstraintName() {
            return _Argument(Res.GetString(Res.DataConstraint_NoName));
        }

        static public Exception ConstraintViolation(string constraint) {
            return _Constraint(Res.GetString(Res.DataConstraint_Violation, constraint));
        }

        static public Exception ConstraintNotInTheTable(string constraint) {
            return _Argument(Res.GetString(Res.DataConstraint_NotInTheTable,constraint));
        }

        static public string KeysToString(object[] keys) {
            string values = String.Empty;
            for (int i = 0; i < keys.Length; i++) {
                values += Convert.ToString(keys[i], null) + (i < keys.Length - 1 ? ", " : String.Empty);
            }
            return values;
        }
        static public string UniqueConstraintViolationText(DataColumn[] columns, object[] values) {
            if (columns.Length > 1) {
                string columnNames = String.Empty;
                for (int i = 0; i < columns.Length; i++) {
                    columnNames += columns[i].ColumnName + (i < columns.Length - 1 ? ", " : "");
                }
                return Res.GetString(Res.DataConstraint_ViolationValue, columnNames, KeysToString(values));
            }
            else {
                return Res.GetString(Res.DataConstraint_ViolationValue, columns[0].ColumnName, Convert.ToString(values[0], null));
            }
        }
        static public Exception ConstraintViolation(DataColumn[] columns, object[] values) {
            return _Constraint(UniqueConstraintViolationText(columns, values));
        }

        static public Exception ConstraintOutOfRange(int index) {
            return _IndexOutOfRange(Res.GetString(Res.DataConstraint_OutOfRange, (index).ToString(CultureInfo.InvariantCulture)));
        }

        static public Exception DuplicateConstraint(string constraint) {
            return _Data(Res.GetString(Res.DataConstraint_Duplicate, constraint));
        }

        static public Exception DuplicateConstraintName(string constraint) {
            return _DuplicateName(Res.GetString(Res.DataConstraint_DuplicateName, constraint));
        }

        static public Exception NeededForForeignKeyConstraint(UniqueConstraint key, ForeignKeyConstraint fk) {
            return _Argument(Res.GetString(Res.DataConstraint_NeededForForeignKeyConstraint, key.ConstraintName, fk.ConstraintName));
        }

        static public Exception UniqueConstraintViolation() {
            return _Argument(Res.GetString(Res.DataConstraint_UniqueViolation));
        }

        static public Exception ConstraintForeignTable() {
            return _Argument(Res.GetString(Res.DataConstraint_ForeignTable));
        }

        static public Exception ConstraintParentValues() {
            return _Argument(Res.GetString(Res.DataConstraint_ParentValues));
        }

        static public Exception ConstraintAddFailed(DataTable table) {
            return _InvalidConstraint(Res.GetString(Res.DataConstraint_AddFailed, table.TableName));
        }

        static public Exception ConstraintRemoveFailed() {
            return _Argument(Res.GetString(Res.DataConstraint_RemoveFailed));
        }

        static public Exception FailedCascadeDelete(string constraint) {
            return _InvalidConstraint(Res.GetString(Res.DataConstraint_CascadeDelete, constraint));
        }

        static public Exception FailedCascadeUpdate(string constraint) {
            return _InvalidConstraint(Res.GetString(Res.DataConstraint_CascadeUpdate, constraint));
        }

        static public Exception FailedClearParentTable(string table, string constraint, string childTable) {
            return _InvalidConstraint(Res.GetString(Res.DataConstraint_ClearParentTable, table, constraint, childTable));
        }

        static public Exception ForeignKeyViolation(string constraint, object[] keys) {
            return _InvalidConstraint(Res.GetString(Res.DataConstraint_ForeignKeyViolation, constraint, KeysToString(keys)));
        }

        static public Exception RemoveParentRow(ForeignKeyConstraint constraint) {
            return _InvalidConstraint(Res.GetString(Res.DataConstraint_RemoveParentRow, constraint.ConstraintName));
        }

        static public string MaxLengthViolationText(string  columnName) {
            return Res.GetString(Res.DataColumn_ExceedMaxLength, columnName);
        }
        static public string NotAllowDBNullViolationText(string  columnName) {
            return Res.GetString(Res.DataColumn_NotAllowDBNull, columnName);
        }

        static public Exception CantAddConstraintToMultipleNestedTable(string tableName) {
            return _Argument(Res.GetString(Res.DataConstraint_CantAddConstraintToMultipleNestedTable, tableName));
        }

        //
        // DataColumn Set Properties conflicts
        //

        static public Exception AutoIncrementAndExpression() {
            return _Argument(Res.GetString(Res.DataColumn_AutoIncrementAndExpression));
        }
        static public Exception AutoIncrementAndDefaultValue() {
            return _Argument(Res.GetString(Res.DataColumn_AutoIncrementAndDefaultValue));
        }
        static public Exception AutoIncrementSeed() {
            return _Argument(Res.GetString(Res.DataColumn_AutoIncrementSeed));
        }
        static public Exception CantChangeDataType() {
            return _Argument(Res.GetString(Res.DataColumn_ChangeDataType));
        }
        static public Exception NullDataType() {
            return _Argument(Res.GetString(Res.DataColumn_NullDataType));
        }
        static public Exception ColumnNameRequired() {
            return _Argument(Res.GetString(Res.DataColumn_NameRequired));
        }
        static public Exception DefaultValueAndAutoIncrement() {
            return _Argument(Res.GetString(Res.DataColumn_DefaultValueAndAutoIncrement));
        }
        static public Exception DefaultValueDataType(string column, Type defaultType, Type columnType, Exception inner) {
            if (column.Length == 0) {
                return _Argument(Res.GetString(Res.DataColumn_DefaultValueDataType1, defaultType.FullName, columnType.FullName), inner);
            }
            else {
                return _Argument(Res.GetString(Res.DataColumn_DefaultValueDataType, column, defaultType.FullName, columnType.FullName), inner);
            }
        }
        static public Exception DefaultValueColumnDataType(string column, Type defaultType, Type columnType, Exception inner) {
            return _Argument(Res.GetString(Res.DataColumn_DefaultValueColumnDataType, column, defaultType.FullName, columnType.FullName), inner);
        }

        static public Exception ExpressionAndUnique() {
            return _Argument(Res.GetString(Res.DataColumn_ExpressionAndUnique));
        }
        static public Exception ExpressionAndReadOnly() {
            return _Argument(Res.GetString(Res.DataColumn_ExpressionAndReadOnly));
        }

        static public Exception ExpressionAndConstraint(DataColumn column, Constraint constraint) {
            return _Argument(Res.GetString(Res.DataColumn_ExpressionAndConstraint, column.ColumnName, constraint.ConstraintName));
        }

        static public Exception ExpressionInConstraint(DataColumn column) {
            return _Argument(Res.GetString(Res.DataColumn_ExpressionInConstraint, column.ColumnName));
        }

        static public Exception ExpressionCircular() {
            return _Argument(Res.GetString(Res.DataColumn_ExpressionCircular));
        }

        static public Exception NonUniqueValues(string column) {
            return _InvalidConstraint(Res.GetString(Res.DataColumn_NonUniqueValues, column));
        }

        static public Exception NullKeyValues(string column) {
            return _Data(Res.GetString(Res.DataColumn_NullKeyValues, column));
        }
        static public Exception NullValues(string column) {
            return _NoNullAllowed(Res.GetString(Res.DataColumn_NullValues, column));
        }

        static public Exception ReadOnlyAndExpression() {
            return _ReadOnly(Res.GetString(Res.DataColumn_ReadOnlyAndExpression));
        }

        static public Exception ReadOnly(string column) {
            return _ReadOnly(Res.GetString(Res.DataColumn_ReadOnly, column));
        }

        static public Exception UniqueAndExpression() {
            return _Argument(Res.GetString(Res.DataColumn_UniqueAndExpression));
        }

        static public Exception SetFailed(object value, DataColumn column, Type type, Exception innerException) {
            return _Argument(innerException.Message + Res.GetString(Res.DataColumn_SetFailed, value.ToString(), column.ColumnName, type.Name), innerException);
        }

        static public Exception CannotSetToNull(DataColumn column) {
            return _Argument(Res.GetString(Res.DataColumn_CannotSetToNull, column.ColumnName));
        }

        static public Exception LongerThanMaxLength(DataColumn column) {
            return _Argument(Res.GetString(Res.DataColumn_LongerThanMaxLength, column.ColumnName));
        }

        static public Exception CannotSetMaxLength(DataColumn column, int value) {
            return _Argument(Res.GetString(Res.DataColumn_CannotSetMaxLength, column.ColumnName, value.ToString(CultureInfo.InvariantCulture)));
        }

        static public Exception CannotSetMaxLength2(DataColumn column) {
            return _Argument(Res.GetString(Res.DataColumn_CannotSetMaxLength2, column.ColumnName));
        }

        static public Exception CannotSetSimpleContentType(String columnName, Type type) {
            return _Argument(Res.GetString(Res.DataColumn_CannotSimpleContentType, columnName, type));
        }

        static public Exception CannotSetSimpleContent(String columnName, Type type) {
            return _Argument(Res.GetString(Res.DataColumn_CannotSimpleContent, columnName, type));
        }

        static public Exception CannotChangeNamespace(String columnName) {
            return _Argument(Res.GetString(Res.DataColumn_CannotChangeNamespace, columnName));
        }

        static public Exception HasToBeStringType(DataColumn column) {
            return _Argument(Res.GetString(Res.DataColumn_HasToBeStringType, column.ColumnName));
        }

        static public Exception AutoIncrementCannotSetIfHasData(string typeName) {
            return _Argument(Res.GetString(Res.DataColumn_AutoIncrementCannotSetIfHasData, typeName));
        }

        static public Exception INullableUDTwithoutStaticNull(string typeName) {
            return _Argument(Res.GetString( Res.DataColumn_INullableUDTwithoutStaticNull, typeName));
        }

        static public Exception IComparableNotImplemented(string typeName) {
            return _Data(Res.GetString(Res.DataStorage_IComparableNotDefined, typeName));
        }

        static public Exception UDTImplementsIChangeTrackingButnotIRevertible(string typeName) {
            return _InvalidOperation(Res.GetString(Res.DataColumn_UDTImplementsIChangeTrackingButnotIRevertible, typeName));
        }

        static public Exception SetAddedAndModifiedCalledOnnonUnchanged() {
            return _InvalidOperation(Res.GetString(Res.DataColumn_SetAddedAndModifiedCalledOnNonUnchanged));
        }

        static public Exception InvalidDataColumnMapping(Type type) {
            return _Argument(Res.GetString(Res.DataColumn_InvalidDataColumnMapping, type.AssemblyQualifiedName));
        }

        static public Exception CannotSetDateTimeModeForNonDateTimeColumns() {
            return _InvalidOperation(Res.GetString(Res.DataColumn_CannotSetDateTimeModeForNonDateTimeColumns));
        }

        static public Exception InvalidDateTimeMode(DataSetDateTime mode) {
            return _InvalidEnumArgumentException<DataSetDateTime>(mode);
        }

        static public Exception CantChangeDateTimeMode(DataSetDateTime oldValue, DataSetDateTime newValue) {
            return _InvalidOperation(Res.GetString(Res.DataColumn_DateTimeMode, oldValue.ToString(), newValue.ToString()));
        }


        static public Exception ColumnTypeNotSupported() {
            return System.Data.Common.ADP.NotSupported(Res.GetString(Res.DataColumn_NullableTypesNotSupported));
        }

        //
        // DataView
        //

        static public Exception SetFailed(string name) {
            return _Data(Res.GetString(Res.DataView_SetFailed, name));
        }

        static public Exception SetDataSetFailed() {
            return _Data(Res.GetString(Res.DataView_SetDataSetFailed));
        }

        static public Exception SetRowStateFilter() {
            return _Data(Res.GetString(Res.DataView_SetRowStateFilter));
        }

        static public Exception CanNotSetDataSet() {
            return _Data(Res.GetString(Res.DataView_CanNotSetDataSet));
        }

        static public Exception CanNotUseDataViewManager() {
            return _Data(Res.GetString(Res.DataView_CanNotUseDataViewManager));
        }

        static public Exception CanNotSetTable() {
            return _Data(Res.GetString(Res.DataView_CanNotSetTable));
        }

        static public Exception CanNotUse() {
            return _Data(Res.GetString(Res.DataView_CanNotUse));
        }

        static public Exception CanNotBindTable() {
            return _Data(Res.GetString(Res.DataView_CanNotBindTable));
        }

        static public Exception SetTable() {
            return _Data(Res.GetString(Res.DataView_SetTable));
        }

        static public Exception SetIListObject() {
            return _Argument(Res.GetString(Res.DataView_SetIListObject));
        }

        static public Exception AddNewNotAllowNull() {
            return _Data(Res.GetString(Res.DataView_AddNewNotAllowNull));
        }

        static public Exception NotOpen() {
            return _Data(Res.GetString(Res.DataView_NotOpen));
        }

        static public Exception CreateChildView() {
            return _Argument(Res.GetString(Res.DataView_CreateChildView));
        }

        static public Exception CanNotDelete() {
            return _Data(Res.GetString(Res.DataView_CanNotDelete));
        }

        static public Exception CanNotEdit() {
            return _Data(Res.GetString(Res.DataView_CanNotEdit));
        }

        static public Exception GetElementIndex(Int32 index) {
            return _IndexOutOfRange(Res.GetString(Res.DataView_GetElementIndex, (index).ToString(CultureInfo.InvariantCulture)));
        }

        static public Exception AddExternalObject() {
            return _Argument(Res.GetString(Res.DataView_AddExternalObject));
        }

        static public Exception CanNotClear() {
            return _Argument(Res.GetString(Res.DataView_CanNotClear));
        }

        static public Exception InsertExternalObject() {
            return _Argument(Res.GetString(Res.DataView_InsertExternalObject));
        }

        static public Exception RemoveExternalObject() {
            return _Argument(Res.GetString(Res.DataView_RemoveExternalObject));
        }

        static public Exception PropertyNotFound(string property, string table) {
            return _Argument(Res.GetString(Res.DataROWView_PropertyNotFound, property, table));
        }

        static public Exception ColumnToSortIsOutOfRange(string column) {
            return _Argument(Res.GetString(Res.DataColumns_OutOfRange, column));
        }

        //
        // Keys
        //

        static public Exception KeyTableMismatch() {
            return _InvalidConstraint(Res.GetString(Res.DataKey_TableMismatch));
        }

        static public Exception KeyNoColumns() {
            return _InvalidConstraint(Res.GetString(Res.DataKey_NoColumns));
        }

        static public Exception KeyTooManyColumns(int cols) {
            return _InvalidConstraint(Res.GetString(Res.DataKey_TooManyColumns, (cols).ToString(CultureInfo.InvariantCulture)));
        }

        static public Exception KeyDuplicateColumns(string columnName) {
            return _InvalidConstraint(Res.GetString(Res.DataKey_DuplicateColumns, columnName));
        }

        //
        // Relations, constraints
        //

        static public Exception RelationDataSetMismatch() {
            return _InvalidConstraint(Res.GetString(Res.DataRelation_DataSetMismatch));
        }

        static public Exception NoRelationName() {
            return _Argument(Res.GetString(Res.DataRelation_NoName));
        }

        static public Exception ColumnsTypeMismatch() {
            return _InvalidConstraint(Res.GetString(Res.DataRelation_ColumnsTypeMismatch));
        }

        static public Exception KeyLengthMismatch() {
            return _Argument(Res.GetString(Res.DataRelation_KeyLengthMismatch));
        }

        static public Exception KeyLengthZero() {
            return _Argument(Res.GetString(Res.DataRelation_KeyZeroLength));
        }

        static public Exception ForeignRelation() {
            return _Argument(Res.GetString(Res.DataRelation_ForeignDataSet));
        }

        static public Exception KeyColumnsIdentical() {
            return _InvalidConstraint(Res.GetString(Res.DataRelation_KeyColumnsIdentical));
        }

        static public Exception RelationForeignTable(string t1, string t2) {
            return _InvalidConstraint(Res.GetString(Res.DataRelation_ForeignTable, t1, t2));
        }

        static public Exception GetParentRowTableMismatch(string t1, string t2) {
            return _InvalidConstraint(Res.GetString(Res.DataRelation_GetParentRowTableMismatch, t1, t2));
        }

        static public Exception SetParentRowTableMismatch(string t1, string t2) {
            return _InvalidConstraint(Res.GetString(Res.DataRelation_SetParentRowTableMismatch, t1, t2));
        }

        static public Exception RelationForeignRow() {
            return _Argument(Res.GetString(Res.DataRelation_ForeignRow));
        }

        static public Exception RelationNestedReadOnly() {
            return _Argument(Res.GetString(Res.DataRelation_RelationNestedReadOnly));
        }

        static public Exception TableCantBeNestedInTwoTables(string tableName) {
            return _Argument(Res.GetString(Res.DataRelation_TableCantBeNestedInTwoTables, tableName));
        }

        static public Exception LoopInNestedRelations(string tableName) {
            return _Argument(Res.GetString(Res.DataRelation_LoopInNestedRelations, tableName));
        }

        static public Exception RelationDoesNotExist() {
            return _Argument(Res.GetString(Res.DataRelation_DoesNotExist));
        }

        static public Exception ParentRowNotInTheDataSet() {
            return _Argument(Res.GetString(Res.DataRow_ParentRowNotInTheDataSet));
        }

        static public Exception ParentOrChildColumnsDoNotHaveDataSet() {
            return _InvalidConstraint(Res.GetString(Res.DataRelation_ParentOrChildColumnsDoNotHaveDataSet));
        }

        static public Exception InValidNestedRelation(string childTableName) {
            return _InvalidOperation(Res.GetString(Res.DataRelation_InValidNestedRelation, childTableName));
        }


        static public Exception InvalidParentNamespaceinNestedRelation(string childTableName) {
            return _InvalidOperation(Res.GetString(Res.DataRelation_InValidNamespaceInNestedRelation, childTableName));
        }

        //
        // Rows
        //

        static public Exception RowNotInTheDataSet() {
            return _Argument(Res.GetString(Res.DataRow_NotInTheDataSet));
        }

        static public Exception RowNotInTheTable() {
            return _RowNotInTable(Res.GetString(Res.DataRow_NotInTheTable));
        }
        static public Exception EditInRowChanging() {
            return _InRowChangingEvent(Res.GetString(Res.DataRow_EditInRowChanging));
        }

        static public Exception EndEditInRowChanging() {
            return _InRowChangingEvent(Res.GetString(Res.DataRow_EndEditInRowChanging));
        }

        static public Exception BeginEditInRowChanging() {
            return _InRowChangingEvent(Res.GetString(Res.DataRow_BeginEditInRowChanging));
        }

        static public Exception CancelEditInRowChanging() {
            return _InRowChangingEvent(Res.GetString(Res.DataRow_CancelEditInRowChanging));
        }

        static public Exception DeleteInRowDeleting() {
            return _InRowChangingEvent(Res.GetString(Res.DataRow_DeleteInRowDeleting));
        }

        static public Exception ValueArrayLength() {
            return _Argument(Res.GetString(Res.DataRow_ValuesArrayLength));
        }

        static public Exception NoCurrentData() {
            return _VersionNotFound(Res.GetString(Res.DataRow_NoCurrentData));
        }

        static public Exception NoOriginalData() {
            return _VersionNotFound(Res.GetString(Res.DataRow_NoOriginalData));
        }

        static public Exception NoProposedData() {
            return _VersionNotFound(Res.GetString(Res.DataRow_NoProposedData));
        }

        static public Exception RowRemovedFromTheTable() {
            return _RowNotInTable(Res.GetString(Res.DataRow_RemovedFromTheTable));
        }

        static public Exception DeletedRowInaccessible() {
            return _DeletedRowInaccessible(Res.GetString(Res.DataRow_DeletedRowInaccessible));
        }

        static public Exception RowAlreadyDeleted() {
            return _DeletedRowInaccessible(Res.GetString(Res.DataRow_AlreadyDeleted));
        }

        static public Exception RowEmpty() {
            return _Argument(Res.GetString(Res.DataRow_Empty));
        }

        static public Exception InvalidRowVersion() {
            return _Data(Res.GetString(Res.DataRow_InvalidVersion));
        }

        static public Exception RowOutOfRange() {
            return _IndexOutOfRange(Res.GetString(Res.DataRow_RowOutOfRange));
        }
        static public Exception RowOutOfRange(int index) {
            return _IndexOutOfRange(Res.GetString(Res.DataRow_OutOfRange, (index).ToString(CultureInfo.InvariantCulture)));
        }
        static public Exception RowInsertOutOfRange(int index) {
            return _IndexOutOfRange(Res.GetString(Res.DataRow_RowInsertOutOfRange, (index).ToString(CultureInfo.InvariantCulture)));
        }

        static public Exception RowInsertTwice(int index, string tableName) {
            return _IndexOutOfRange(Res.GetString(Res.DataRow_RowInsertTwice, (index).ToString(CultureInfo.InvariantCulture), tableName));
        }

        static public Exception RowInsertMissing( string tableName) {
            return _IndexOutOfRange(Res.GetString(Res.DataRow_RowInsertMissing, tableName));
        }

        static public Exception RowAlreadyRemoved() {
            return _Data(Res.GetString(Res.DataRow_AlreadyRemoved));
        }

        static public Exception MultipleParents() {
            return _Data(Res.GetString(Res.DataRow_MultipleParents));
        }

        static public Exception InvalidRowState(DataRowState state) {
            return _InvalidEnumArgumentException<DataRowState>(state);
        }

        static public Exception InvalidRowBitPattern() {
            return _Argument(Res.GetString(Res.DataRow_InvalidRowBitPattern));
        }

        //
        // DataSet
        //

        static internal Exception SetDataSetNameToEmpty() {
            return _Argument(Res.GetString(Res.DataSet_SetNameToEmpty));
        }
        static internal Exception SetDataSetNameConflicting(string name) {
            return _Argument(Res.GetString(Res.DataSet_SetDataSetNameConflicting, name));
        }
        static public Exception DataSetUnsupportedSchema(string ns) {
            return _Argument(Res.GetString(Res.DataSet_UnsupportedSchema, ns));
        }
        static public Exception MergeMissingDefinition(string obj) {
            return _Argument(Res.GetString(Res.DataMerge_MissingDefinition, obj));
        }
        static public Exception TablesInDifferentSets() {
            return _Argument(Res.GetString(Res.DataRelation_TablesInDifferentSets));
        }
        static public Exception RelationAlreadyExists() {
            return _Argument(Res.GetString(Res.DataRelation_AlreadyExists));
        }
        static public Exception RowAlreadyInOtherCollection() {
            return _Argument(Res.GetString(Res.DataRow_AlreadyInOtherCollection));
        }
        static public Exception RowAlreadyInTheCollection() {
            return _Argument(Res.GetString(Res.DataRow_AlreadyInTheCollection));
        }
        static public Exception TableMissingPrimaryKey() {
            return _MissingPrimaryKey(Res.GetString(Res.DataTable_MissingPrimaryKey));
        }
        static public Exception RecordStateRange() {
            return _Argument(Res.GetString(Res.DataIndex_RecordStateRange));
        }
        static public Exception IndexKeyLength(int length, int keyLength) {
            if(length == 0) {
                return _Argument(Res.GetString(Res.DataIndex_FindWithoutSortOrder));
            }
            else {
                return _Argument(Res.GetString(Res.DataIndex_KeyLength, (length).ToString(CultureInfo.InvariantCulture), (keyLength).ToString(CultureInfo.InvariantCulture)));
            }
        }

        static public Exception RemovePrimaryKey(DataTable table) {
            if (table.TableName.Length == 0) {
                return _Argument(Res.GetString(Res.DataKey_RemovePrimaryKey));
            }
            else {
                return _Argument(Res.GetString(Res.DataKey_RemovePrimaryKey1, table.TableName));
            }
        }
        static public Exception RelationAlreadyInOtherDataSet() {
            return _Argument(Res.GetString(Res.DataRelation_AlreadyInOtherDataSet));
        }
        static public Exception RelationAlreadyInTheDataSet() {
            return _Argument(Res.GetString(Res.DataRelation_AlreadyInTheDataSet));
        }
        static public Exception RelationNotInTheDataSet(string relation) {
            return _Argument(Res.GetString(Res.DataRelation_NotInTheDataSet,relation));
        }
        static public Exception RelationOutOfRange(object index) {
            return _IndexOutOfRange(Res.GetString(Res.DataRelation_OutOfRange, Convert.ToString(index, null)));
        }
        static public Exception DuplicateRelation(string relation) {
            return _DuplicateName(Res.GetString(Res.DataRelation_DuplicateName, relation));
        }
        static public Exception RelationTableNull() {
            return _Argument(Res.GetString(Res.DataRelation_TableNull));
        }
        static public Exception RelationDataSetNull() {
            return _Argument(Res.GetString(Res.DataRelation_TableNull));
        }
        static public Exception RelationTableWasRemoved() {
            return _Argument(Res.GetString(Res.DataRelation_TableWasRemoved));
        }
        static public Exception ParentTableMismatch() {
            return _Argument(Res.GetString(Res.DataRelation_ParentTableMismatch));
        }
        static public Exception ChildTableMismatch() {
            return _Argument(Res.GetString(Res.DataRelation_ChildTableMismatch));
        }
        static public Exception EnforceConstraint() {
            return _Constraint(Res.GetString(Res.Data_EnforceConstraints));
        }
        static public Exception CaseLocaleMismatch() {
            return _Argument(Res.GetString(Res.DataRelation_CaseLocaleMismatch));
        }
        static public Exception CannotChangeCaseLocale() {
            return CannotChangeCaseLocale(null);
        }
        static public Exception CannotChangeCaseLocale(Exception innerException) {
            return _Argument(Res.GetString(Res.DataSet_CannotChangeCaseLocale), innerException);
        }

        static public Exception CannotChangeSchemaSerializationMode() {
            return _InvalidOperation(Res.GetString(Res.DataSet_CannotChangeSchemaSerializationMode));
        }

        static public Exception InvalidSchemaSerializationMode(Type enumType, string mode) {
            return _InvalidEnumArgumentException(Res.GetString(Res.ADP_InvalidEnumerationValue, enumType.Name, mode));
        }

        static public Exception InvalidRemotingFormat(SerializationFormat mode) {
#if DEBUG
            switch(mode) {
            case SerializationFormat.Xml:
            case SerializationFormat.Binary:
                Debug.Assert(false, "valid SerializationFormat " + mode.ToString());
                break;
            }
#endif
            return _InvalidEnumArgumentException<SerializationFormat>(mode);
        }

        //
        // DataTable and DataTableCollection
        //
        static public Exception TableForeignPrimaryKey() {
            return _Argument(Res.GetString(Res.DataTable_ForeignPrimaryKey));
        }
        static public Exception TableCannotAddToSimpleContent() {
            return _Argument(Res.GetString(Res.DataTable_CannotAddToSimpleContent));
        }
        static public Exception NoTableName() {
            return _Argument(Res.GetString(Res.DataTable_NoName));
        }
        static public Exception MultipleTextOnlyColumns() {
            return _Argument(Res.GetString(Res.DataTable_MultipleSimpleContentColumns));
        }
        static public Exception InvalidSortString(string sort) {
            return _Argument(Res.GetString(Res.DataTable_InvalidSortString, sort));
        }
        static public Exception DuplicateTableName(string table) {
            return _DuplicateName(Res.GetString(Res.DataTable_DuplicateName, table));
        }
        static public Exception DuplicateTableName2(string table, string ns) {
            return _DuplicateName(Res.GetString(Res.DataTable_DuplicateName2, table, ns));
        }
        static public Exception SelfnestedDatasetConflictingName(string table) {
            return _DuplicateName(Res.GetString(Res.DataTable_SelfnestedDatasetConflictingName, table));
        }
        static public Exception DatasetConflictingName(string table) {
            return _DuplicateName(Res.GetString(Res.DataTable_DatasetConflictingName, table));
        }
        static public Exception TableAlreadyInOtherDataSet() {
            return _Argument(Res.GetString(Res.DataTable_AlreadyInOtherDataSet));
        }
        static public Exception TableAlreadyInTheDataSet() {
            return _Argument(Res.GetString(Res.DataTable_AlreadyInTheDataSet));
        }
        static public Exception TableOutOfRange(int index) {
            return _IndexOutOfRange(Res.GetString(Res.DataTable_OutOfRange, (index).ToString(CultureInfo.InvariantCulture)));
        }
        static public Exception TableNotInTheDataSet(string table) {
            return _Argument(Res.GetString(Res.DataTable_NotInTheDataSet, table));
        }
        static public Exception TableInRelation() {
            return _Argument(Res.GetString(Res.DataTable_InRelation));
        }
        static public Exception TableInConstraint(DataTable table, Constraint constraint) {
            return _Argument(Res.GetString(Res.DataTable_InConstraint, table.TableName, constraint.ConstraintName));
        }

        static public Exception CanNotSerializeDataTableHierarchy() {
            return _InvalidOperation(Res.GetString(Res.DataTable_CanNotSerializeDataTableHierarchy));
        }

        static public Exception CanNotRemoteDataTable() {
            return _InvalidOperation(Res.GetString(Res.DataTable_CanNotRemoteDataTable));
        }

        static public Exception CanNotSetRemotingFormat() {
            return _Argument(Res.GetString(Res.DataTable_CanNotSetRemotingFormat));
        }

        static public Exception CanNotSerializeDataTableWithEmptyName() {
            return _InvalidOperation(Res.GetString(Res.DataTable_CanNotSerializeDataTableWithEmptyName));
        }

        static public Exception TableNotFound (string tableName) {
            return _Argument(Res.GetString(Res.DataTable_TableNotFound, tableName));
        }


        //
        // Storage
        //
        static public Exception AggregateException(AggregateType aggregateType, Type type) {
            return _Data(Res.GetString(Res.DataStorage_AggregateException, aggregateType.ToString(), type.Name));
        }
        static public Exception InvalidStorageType(TypeCode typecode) {
            return _Data(Res.GetString(Res.DataStorage_InvalidStorageType, ((Enum) typecode).ToString()));
        }

        static public Exception RangeArgument(Int32 min, Int32 max) {
            return _Argument(Res.GetString(Res.Range_Argument, (min).ToString(CultureInfo.InvariantCulture), (max).ToString(CultureInfo.InvariantCulture)));
        }
        static public Exception NullRange() {
            return _Data(Res.GetString(Res.Range_NullRange));
        }
        static public Exception NegativeMinimumCapacity() {
            return _Argument(Res.GetString(Res.RecordManager_MinimumCapacity));
        }
        static public Exception ProblematicChars(char charValue) {
            string xchar = "0x" + ((UInt16)charValue).ToString("X", CultureInfo.InvariantCulture);
            return _Argument(Res.GetString(Res.DataStorage_ProblematicChars, xchar));
        }

        static public Exception StorageSetFailed() {
            return _Argument(Res.GetString(Res.DataStorage_SetInvalidDataType));
        }


        //
        // XML schema
        //
        static public Exception SimpleTypeNotSupported() {
            return _Data(Res.GetString(Res.Xml_SimpleTypeNotSupported));
        }

        static public Exception MissingAttribute(string attribute) {
            return MissingAttribute(String.Empty, attribute);
        }

        static public Exception MissingAttribute(string element, string attribute) {
            return _Data(Res.GetString(Res.Xml_MissingAttribute, element, attribute));
        }

        static public Exception InvalidAttributeValue(string name, string value) {
            return _Data(Res.GetString(Res.Xml_ValueOutOfRange, name, value));
        }

        static public Exception AttributeValues(string name, string value1, string value2) {
            return _Data(Res.GetString(Res.Xml_AttributeValues, name, value1, value2));
        }

        static public Exception ElementTypeNotFound(string name) {
            return _Data(Res.GetString(Res.Xml_ElementTypeNotFound, name));
        }

        static public Exception RelationParentNameMissing(string rel) {
            return _Data(Res.GetString(Res.Xml_RelationParentNameMissing, rel));
        }

        static public Exception RelationChildNameMissing(string rel) {
            return _Data(Res.GetString(Res.Xml_RelationChildNameMissing, rel));
        }

        static public Exception RelationTableKeyMissing(string rel) {
            return _Data(Res.GetString(Res.Xml_RelationTableKeyMissing, rel));
        }

        static public Exception RelationChildKeyMissing(string rel) {
            return _Data(Res.GetString(Res.Xml_RelationChildKeyMissing, rel));
        }

        static public Exception UndefinedDatatype(string name) {
            return _Data(Res.GetString(Res.Xml_UndefinedDatatype, name));
        }

        static public Exception DatatypeNotDefined() {
            return _Data(Res.GetString(Res.Xml_DatatypeNotDefined));
        }

        static public Exception MismatchKeyLength() {
            return _Data(Res.GetString(Res.Xml_MismatchKeyLength));
        }

        static public Exception InvalidField(string name) {
            return _Data(Res.GetString(Res.Xml_InvalidField, name));
        }

        static public Exception InvalidSelector(string name) {
            return _Data(Res.GetString(Res.Xml_InvalidSelector, name));
        }

        static public Exception CircularComplexType(string name) {
            return _Data(Res.GetString(Res.Xml_CircularComplexType, name));
        }

        static public Exception CannotInstantiateAbstract(string name) {
            return _Data(Res.GetString(Res.Xml_CannotInstantiateAbstract, name));
        }

        static public Exception InvalidKey(string name) {
            return _Data(Res.GetString(Res.Xml_InvalidKey, name));
        }

        static public Exception DiffgramMissingTable(string name) {
            return _Data(Res.GetString(Res.Xml_MissingTable, name));
        }

        static public Exception DiffgramMissingSQL() {
            return _Data(Res.GetString(Res.Xml_MissingSQL));
        }

        static public Exception DuplicateConstraintRead(string str) {
            return _Data(Res.GetString(Res.Xml_DuplicateConstraint, str));
        }

        static public Exception ColumnTypeConflict(string name) {
            return _Data(Res.GetString(Res.Xml_ColumnConflict, name));
        }

        static public Exception CannotConvert(string name, string type) {
            return _Data(Res.GetString(Res.Xml_CannotConvert, name, type));
        }

        static public Exception MissingRefer(string name) {
            return _Data(Res.GetString(Res.Xml_MissingRefer, Keywords.REFER, Keywords.XSD_KEYREF, name));
        }

        static public Exception InvalidPrefix(string name) {
            return _Data(Res.GetString(Res.Xml_InvalidPrefix, name));
        }

        static public Exception CanNotDeserializeObjectType() {
            return _InvalidOperation(Res.GetString(Res.Xml_CanNotDeserializeObjectType));
        }

        static public Exception IsDataSetAttributeMissingInSchema() {
            return _Data(Res.GetString(Res.Xml_IsDataSetAttributeMissingInSchema));
        }
        static public Exception TooManyIsDataSetAtributeInSchema() {
            return _Data(Res.GetString(Res.Xml_TooManyIsDataSetAtributeInSchema));
        }

        // XML save
        static public Exception NestedCircular(string name) {
            return _Data(Res.GetString(Res.Xml_NestedCircular, name));
        }

        static public Exception MultipleParentRows(string tableQName) {
            return _Data(Res.GetString(Res.Xml_MultipleParentRows, tableQName));
        }

        static public Exception PolymorphismNotSupported(string typeName) {
            return _InvalidOperation(Res.GetString(Res.Xml_PolymorphismNotSupported, typeName));
        }


        static public Exception DataTableInferenceNotSupported() {
            return _InvalidOperation(Res.GetString(Res.Xml_DataTableInferenceNotSupported));
        }

        /// <summary>throw DataException for multitarget failure</summary>
        /// <param name="innerException">exception from multitarget converter</param>
        /// <exception cref="DataException">always thrown</exception>
        static internal void ThrowMultipleTargetConverter(Exception innerException)
        {
            string res = (null != innerException) ? Res.Xml_MultipleTargetConverterError : Res.Xml_MultipleTargetConverterEmpty;
            ThrowDataException(Res.GetString(res), innerException);
        }

        //
        // Merge
        //
        static public Exception DuplicateDeclaration(string name) {
            return _Data(Res.GetString(Res.Xml_MergeDuplicateDeclaration, name));
        }


        //Read Xml data
        static public Exception FoundEntity() {
            return _Data(Res.GetString(Res.Xml_FoundEntity));
        }

        // ATTENTION: name has to be localized string here:
        static public Exception MergeFailed(string name) {
            return _Data(name);
        }

        // SqlConvert
        static public DataException ConvertFailed(Type type1, Type type2) {
            return _Data(Res.GetString(Res.SqlConvert_ConvertFailed, type1.FullName, type2.FullName));
        }

        // DataTableReader
        static public Exception InvalidDataTableReader(string tableName) {
            return _InvalidOperation(Res.GetString(Res.DataTableReader_InvalidDataTableReader, tableName));
        }

        static public Exception DataTableReaderSchemaIsInvalid(string tableName) {
            return _InvalidOperation(Res.GetString(Res.DataTableReader_SchemaInvalidDataTableReader, tableName));
        }

        static public Exception CannotCreateDataReaderOnEmptyDataSet() {
            return _Argument(Res.GetString(Res.DataTableReader_CannotCreateDataReaderOnEmptyDataSet));
        }

        static public Exception DataTableReaderArgumentIsEmpty() {
            return _Argument(Res.GetString(Res.DataTableReader_DataTableReaderArgumentIsEmpty));
        }

        static public Exception ArgumentContainsNullValue() {
            return _Argument(Res.GetString(Res.DataTableReader_ArgumentContainsNullValue));
        }

        static public Exception InvalidCurrentRowInDataTableReader() {
            return _DeletedRowInaccessible(Res.GetString(Res.DataTableReader_InvalidRowInDataTableReader));
        }

        static public Exception EmptyDataTableReader(string tableName) {
            return _DeletedRowInaccessible(Res.GetString(Res.DataTableReader_DataTableCleared, tableName));
        }


        //
        static internal Exception InvalidDuplicateNamedSimpleTypeDelaration(string stName, string errorStr) {
            return _Argument(Res.GetString(Res.NamedSimpleType_InvalidDuplicateNamedSimpleTypeDelaration, stName, errorStr));
        }

        // RbTree
        static internal Exception InternalRBTreeError(RBTreeError internalError) {
            return _InvalidOperation(Res.GetString(Res.RbTree_InvalidState, (int)internalError));
        }
        static public Exception EnumeratorModified() {
            return _InvalidOperation(Res.GetString(Res.RbTree_EnumerationBroken));
        }
    }// ExceptionBuilder
}
