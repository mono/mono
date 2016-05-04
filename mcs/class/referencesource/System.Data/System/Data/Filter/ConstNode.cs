//------------------------------------------------------------------------------
// <copyright file="ConstNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;

    internal sealed class ConstNode : ExpressionNode {
        internal readonly object val;

        internal ConstNode(DataTable table, ValueType type, object constant) : this(table, type, constant, true) {
        }

        internal ConstNode(DataTable table, ValueType type, object constant, bool fParseQuotes) : base(table) {
            switch (type) {
                case ValueType.Null:
                    this.val = DBNull.Value;
                    break;

                case ValueType.Numeric:
                    this.val = SmallestNumeric(constant);
                    break;
                case ValueType.Decimal:
                    this.val = SmallestDecimal(constant);
                    break;
                case ValueType.Float:
                    this.val = Convert.ToDouble(constant, NumberFormatInfo.InvariantInfo);
                    break;

                case ValueType.Bool:
                    this.val = Convert.ToBoolean(constant, CultureInfo.InvariantCulture);
                    break;

                case ValueType.Str:
                    if (fParseQuotes) {
                        // replace '' with one '
                        this.val = ((string)constant).Replace("''", "'");
                    }
                    else {
                        this.val = (string)constant;
                    }
                    break;

                case ValueType.Date:
                    this.val = DateTime.Parse((string)constant, CultureInfo.InvariantCulture);
                    break;

                case ValueType.Object:
                    this.val = constant;
                    break;

                default:
                    Debug.Assert(false, "NYI");
                    goto case ValueType.Object;
            }
        }

        internal override void Bind(DataTable table, List<DataColumn> list) {
            BindTable(table);
        }

        internal override object Eval() {
            return val;
        }

        internal override object Eval(DataRow row, DataRowVersion version) {
            return Eval();
        }

        internal override object Eval(int[] recordNos) {
            return Eval();
        }

        internal override bool IsConstant() {
            return true;
        }

        internal override bool IsTableConstant() {
            return true;
        }

        internal override bool HasLocalAggregate() {
            return false;
        }
        internal override bool HasRemoteAggregate() {
            return false;
        }

        internal override ExpressionNode Optimize() {
            return this;
        }

        private object SmallestDecimal(object constant) {
            if (null == constant) {
                return 0d;
            }
            else {
                string sval = (constant as string);
                if (null != sval) {
                    decimal r12;
                    if (Decimal.TryParse(sval, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out r12)) {
                        return r12;
                    }
                    
                    double r8;
                    if (Double.TryParse(sval, NumberStyles.Float| NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out r8)) {
                        return r8;
                    }                    
                }
                else {
                    IConvertible convertible = (constant as IConvertible);
                    if (null != convertible) {
                        try {
                            return convertible.ToDecimal(NumberFormatInfo.InvariantInfo);
                        }
                        catch (System.ArgumentException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.FormatException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.InvalidCastException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.OverflowException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        try {
                            return convertible.ToDouble(NumberFormatInfo.InvariantInfo);
                        }
                        catch (System.ArgumentException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.FormatException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.InvalidCastException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.OverflowException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                    }
                }
            }
            return constant;
        }

        private object SmallestNumeric(object constant) {
            if (null == constant) {
                return (int)0;
            }
            else {
                string sval = (constant as string);
                if (null != sval) {
                    int i4;
                    if (Int32.TryParse(sval, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out i4)) {
                        return i4;
                    }
                    long i8;
                    if (Int64.TryParse(sval, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out i8)) {
                        return i8;
                    }
                    double r8;
                    if (Double.TryParse(sval, NumberStyles.Float| NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out r8)) {
                        return r8;
                    }
                }
                else {
                    IConvertible convertible = (constant as IConvertible);
                    if (null != convertible) {
                        try {
                            return convertible.ToInt32(NumberFormatInfo.InvariantInfo);
                        }
                        catch (System.ArgumentException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.FormatException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.InvalidCastException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.OverflowException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }

                        try {
                            return convertible.ToInt64(NumberFormatInfo.InvariantInfo);
                        }
                        catch (System.ArgumentException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.FormatException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.InvalidCastException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.OverflowException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }

                        try {
                            return convertible.ToDouble(NumberFormatInfo.InvariantInfo);
                        }
                        catch (System.ArgumentException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.FormatException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.InvalidCastException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                        catch (System.OverflowException e) {
                            ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        }
                    }
                }
            }
            return constant;
        }
    }
}
