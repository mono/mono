//------------------------------------------------------------------------------
// <copyright file="ZeroOpNode.cs" company="Microsoft">
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

    internal sealed class ZeroOpNode : ExpressionNode {
        internal readonly int op;

        internal const int zop_True = 1;
        internal const int zop_False = 0;
        internal const int zop_Null = -1;


        internal ZeroOpNode(int op) : base((DataTable)null) {
            this.op = op;
            Debug.Assert(op == Operators.True || op == Operators.False || op == Operators.Null, "Invalid zero-op");
        }

        internal override void Bind(DataTable table, List<DataColumn> list) {
        }

        internal override object Eval() {
            switch (op) {
                case Operators.True:
                    return true;
                case Operators.False:
                    return false;
                case Operators.Null:
                    return DBNull.Value;
                default:
                    Debug.Assert(op == Operators.True || op == Operators.False || op == Operators.Null, "Invalid zero-op");
                    return DBNull.Value;
            }
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
    }
}
