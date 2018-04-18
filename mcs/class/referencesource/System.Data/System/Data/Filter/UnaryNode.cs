//------------------------------------------------------------------------------
// <copyright file="UnaryNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Data.Common;
    using System.Data.SqlTypes;


    internal sealed class UnaryNode : ExpressionNode {
        internal readonly int op;

        internal ExpressionNode right;

        internal UnaryNode(DataTable table, int op, ExpressionNode right) : base(table) {
            this.op = op;
            this.right = right;
        }

        internal override void Bind(DataTable table, List<DataColumn> list) {
            BindTable(table);
            right.Bind(table, list);
        }

        internal override object Eval() {
            return Eval(null, DataRowVersion.Default);
        }

        internal override object Eval(DataRow row, DataRowVersion version) {
            return EvalUnaryOp(op, right.Eval(row, version));
        }

        internal override object Eval(int[] recordNos) {
            return right.Eval(recordNos);
        }

        private object EvalUnaryOp(int op, object vl) {
            object value = DBNull.Value;

            if (DataExpression.IsUnknown(vl))
                return DBNull.Value;

            StorageType storageType;
            switch (op) {
                case Operators.Noop:
                    return vl;
                case Operators.UnaryPlus:
                    storageType = DataStorage.GetStorageType(vl.GetType());
                    if (ExpressionNode.IsNumericSql(storageType)) {
                        return vl;
                    }
                    throw ExprException.TypeMismatch(this.ToString());

                case Operators.Negative:
                    // the have to be better way for doing this..
                    storageType = DataStorage.GetStorageType(vl.GetType());
                    if (ExpressionNode.IsNumericSql(storageType)) {
                        switch(storageType) {
                        case StorageType.Byte:
                            value = -(Byte) vl;
                            break;
                        case StorageType.Int16:
                            value = -(Int16) vl;
                            break;
                        case StorageType.Int32:
                            value = -(Int32) vl;
                            break;
                        case StorageType.Int64:
                            value = -(Int64) vl;
                            break;
                        case StorageType.Single:
                            value = -(Single) vl;
                            break;
                        case StorageType.Double:
                            value = -(Double) vl;
                            break;
                        case StorageType.Decimal:
                            value = -(Decimal) vl;
                            break;
                        case StorageType.SqlDecimal:
                            value = -(SqlDecimal) vl;
                            break;
                        case StorageType.SqlDouble:
                            value = -(SqlDouble) vl;
                            break;
                        case StorageType.SqlSingle:
                            value = -(SqlSingle) vl;
                            break;
                        case StorageType.SqlMoney:
                            value = -(SqlMoney) vl;
                            break;
                        case StorageType.SqlInt64:
                            value = -(SqlInt64) vl;
                            break;
                        case StorageType.SqlInt32:
                            value = -(SqlInt32) vl;
                            break;
                        case StorageType.SqlInt16:
                            value = -(SqlInt16) vl;
                            break;
                        default:
                            Debug.Assert(false, "Missing a type conversion");
                            value = DBNull.Value;
                            break;
                        }
                        return value;
                    }

                    throw ExprException.TypeMismatch(this.ToString());

                case Operators.Not:
                    if (vl is SqlBoolean){
                            if (((SqlBoolean)vl).IsFalse){
                                return SqlBoolean.True;
                            }
                            else if (((SqlBoolean)vl).IsTrue) {
                                      return SqlBoolean.False;
                            }
                            throw ExprException.UnsupportedOperator(op);  // or should the result of not SQLNull  be SqlNull ?
                    }
                    else{
                          if (DataExpression.ToBoolean(vl) != false)
                             return false;
                          return true;
                    }

                default:
                    throw ExprException.UnsupportedOperator(op);
            }
        }

        internal override bool IsConstant() {
            return(right.IsConstant());
        }

        internal override bool IsTableConstant() {
            return(right.IsTableConstant());
        }

        internal override bool HasLocalAggregate() {
            return(right.HasLocalAggregate());
        }
        
        internal override bool HasRemoteAggregate() {
            return(right.HasRemoteAggregate());
        }

        internal override bool DependsOn(DataColumn column) {
            return(right.DependsOn(column));
        }


        internal override ExpressionNode Optimize() {
            right = right.Optimize();

            if (this.IsConstant()) {
                object val = this.Eval();

                return new ConstNode(table, ValueType.Object,  val, false);
            }
            else
                return this;
        }
    }
}
