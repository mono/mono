//------------------------------------------------------------------------------
// <copyright file="DataExpression.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.SqlTypes;
    using System.Data.Common;

    internal sealed class DataExpression : IFilter {
        internal string originalExpression = null;  // original, unoptimized string

        private bool parsed = false;
        private bool bound = false;
        private ExpressionNode expr = null;
        private DataTable table = null;
        private readonly StorageType _storageType;
        private readonly Type _dataType;  // This set if the expression is part of ExpressionCoulmn
        private DataColumn[] dependency = DataTable.zeroColumns;

        internal DataExpression(DataTable table, string expression) : this(table, expression, null) {
        }

        internal DataExpression(DataTable table, string expression, Type type) {
            ExpressionParser parser = new ExpressionParser(table);
            parser.LoadExpression(expression);

            originalExpression = expression;
            expr = null;

            if (expression != null) {
                _storageType = DataStorage.GetStorageType(type);
                if (_storageType == StorageType.BigInteger)
                {
                    throw ExprException.UnsupportedDataType(type);
                }

                _dataType = type;
                expr = parser.Parse();
                parsed = true;
                if (expr != null && table != null) {
                    this.Bind(table);
                }
                else {
                    bound = false;
                }
            }
        }

        internal string Expression {
            get {
                return (originalExpression != null ? originalExpression : ""); // 
            }
        }

        internal ExpressionNode ExpressionNode {
            get {
                return expr;
            }
        }

        internal bool HasValue {
            get {
                return (null != expr);
            }
        }

        internal void Bind(DataTable table) {
            this.table = table;

            if (table == null)
                return;

            if (expr != null) {
                Debug.Assert(parsed, "Invalid calling order: Bind() before Parse()");
                List<DataColumn> list = new List<DataColumn>();
                expr.Bind(table, list);
                expr = expr.Optimize();
                this.table = table;
                bound = true;
                dependency = list.ToArray();
            }
        }

        internal bool DependsOn(DataColumn column) {
            if (expr != null) {
                return expr.DependsOn(column);
            }
            else {
                return false;
            }
        }

        internal object Evaluate() {
            return Evaluate((DataRow)null, DataRowVersion.Default);
        }

        internal object Evaluate(DataRow row, DataRowVersion version) {
            object result;

            if (!bound) {
                this.Bind(this.table);
            }
            if (expr != null) {
                result = expr.Eval(row, version);
                // if the type is a SqlType (StorageType.Uri < _storageType), convert DBNull values.
                if (result != DBNull.Value || StorageType.Uri < _storageType) {
                    // we need to convert the return value to the column.Type;
                    try {
                        if (StorageType.Object != _storageType) {
                            result = SqlConvert.ChangeType2(result, _storageType, _dataType, table.FormatProvider);
                        }
                    }
                    catch (Exception e) {
                        // 
                        if (!ADP.IsCatchableExceptionType(e)) {
                            throw;
                        }
                        ExceptionBuilder.TraceExceptionForCapture(e);

                        // 

                        throw ExprException.DatavalueConvertion(result, _dataType, e);
                    }
                }
            }
            else {
                result = null;
            }
            return result;
        }

        internal object Evaluate(DataRow[] rows) {
            return Evaluate(rows, DataRowVersion.Default);
        }


        internal object Evaluate(DataRow[] rows, DataRowVersion version) {
            if (!bound) {
                this.Bind(this.table);
            }
            if (expr != null) {
                List<int> recordList = new List<int>();
                foreach(DataRow row in rows) {
                    if (row.RowState == DataRowState.Deleted)
                        continue;
                    if (version == DataRowVersion.Original && row.oldRecord == -1)
                        continue;
                    recordList.Add(row.GetRecordFromVersion(version));
                }
                int[]  records = recordList.ToArray();
                return expr.Eval(records);
            }
            else {
                return DBNull.Value;
            }
        }

        public bool Invoke(DataRow row, DataRowVersion version) {
            if (expr == null)
                return true;

            if (row == null) {
                throw ExprException.InvokeArgument();
            }
            object val = expr.Eval(row, version);
            bool result;
            try {
                result = ToBoolean(val);
            }
            catch (EvaluateException) {
                throw ExprException.FilterConvertion(Expression);
            }
            return result;
        }

        internal DataColumn[] GetDependency() {
            Debug.Assert(dependency != null, "GetDependencies: null, we should have created an empty list");
            return dependency;
        }

        internal bool IsTableAggregate() {
            if (expr != null)
                return expr.IsTableConstant();
            else
                return false;
        }

        internal static bool IsUnknown(object value) {
            return DataStorage.IsObjectNull(value);
        }

        internal bool HasLocalAggregate() {
            if (expr != null)
                return expr.HasLocalAggregate();
            else
                return false;
        }
        
        internal bool HasRemoteAggregate() {
            if (expr != null)
                return expr.HasRemoteAggregate();
            else
                return false;
        }

        internal static bool ToBoolean(object value) {
            if (IsUnknown(value))
                return false;
            if (value is bool)
                return(bool)value;
            if (value is SqlBoolean){
                return (((SqlBoolean)value).IsTrue);
            }
//check for SqlString is not added, value for true and false should be given with String, not with SqlString
            if (value is string) {
                try {
                    return Boolean.Parse((string)value);
                }
                catch (Exception e) {
                    // 
                    if (!ADP.IsCatchableExceptionType(e)) {
                        throw;
                    }
                    ExceptionBuilder.TraceExceptionForCapture(e);
                    // 
                    throw ExprException.DatavalueConvertion(value, typeof(bool), e);
                }
            }

            throw ExprException.DatavalueConvertion(value, typeof(bool), null);
        }
    }

    
}
