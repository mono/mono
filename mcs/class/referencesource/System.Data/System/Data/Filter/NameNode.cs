//------------------------------------------------------------------------------
// <copyright file="NameNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    internal sealed class NameNode : ExpressionNode {
        internal char open = '\0';
        internal char close = '\0';
        internal string name;
        internal bool found;
        internal bool type = false;
        internal DataColumn column;

        internal NameNode(DataTable table, char[] text, int start, int pos) : base(table) {
            this.name = ParseName(text, start, pos);
        }

        internal NameNode(DataTable table, string name) : base(table) {
            this.name = name;
        }

        internal override bool IsSqlColumn{
            get{
                return column.IsSqlType;
            }
        }

        internal override void Bind(DataTable table, List<DataColumn> list) {
            BindTable(table);
            if (table == null)
                throw ExprException.UnboundName(name);

            try {
                this.column = table.Columns[name];
            }
            catch (Exception e) {
                found = false;
                // 
                if (!Common.ADP.IsCatchableExceptionType(e)) {
                    throw;
                }
                throw ExprException.UnboundName(name);
            }

            if (column == null)
                throw ExprException.UnboundName(name);

            name = column.ColumnName;
            found = true;

            // add column to the dependency list, do not add duplicate columns
            Debug.Assert(column != null, "Failed to bind column " + name);

            int i;
            for (i = 0; i < list.Count; i++) {
                // walk the list, check if the current column already on the list
                DataColumn dataColumn = list[i];
                if (column == dataColumn) {
                    break;
                }
            }
            if (i >= list.Count) {
                list.Add(column);
            }
        }

        internal override object Eval() {
            // can not eval column without ROW value;
            throw ExprException.EvalNoContext();
        }

        internal override object Eval(DataRow row, DataRowVersion version) {
            if (!found) {
                throw ExprException.UnboundName(name);
            }

            if (row == null) {
                if(IsTableConstant()) // this column is TableConstant Aggregate Function
                    return column.DataExpression.Evaluate();
                else {
                    throw ExprException.UnboundName(name);
                }
            }

            return column[row.GetRecordFromVersion(version)];
        }

        internal override object Eval(int[] records) {
            throw ExprException.ComputeNotAggregate(this.ToString());
        }

        internal override bool IsConstant() {
            return false;
        }

        internal override bool IsTableConstant() {
            if (column != null && column.Computed) {
                return this.column.DataExpression.IsTableAggregate();
            }
            return false;
        }

        internal override bool HasLocalAggregate() {
            if (column != null && column.Computed) {
                return this.column.DataExpression.HasLocalAggregate();
            }
            return false;
        }

        internal override bool HasRemoteAggregate() {
            if (column != null && column.Computed) {
                return this.column.DataExpression.HasRemoteAggregate();
            }
            return false;
        }

        internal override bool DependsOn(DataColumn column) {
            if (this.column == column)
                return true;

            if (this.column.Computed) {
                return this.column.DataExpression.DependsOn(column);
            }

            return false;
        }

        internal override ExpressionNode Optimize() {
            return this;
        }

        /// <devdoc>
        ///     Parses given name and checks it validity
        /// </devdoc>
        internal static string ParseName(char[] text, int start, int pos) {
            char esc = '\0';
            string charsToEscape = "";
            int saveStart = start;
            int savePos = pos;

            if (text[start] == '`') {
                Debug.Assert(text[checked((int)pos-1)] == '`', "Invalid identifyer bracketing, pos = " + pos.ToString(CultureInfo.InvariantCulture) + ", end = " + text[checked((int)pos-1)].ToString(CultureInfo.InvariantCulture));
                start = checked((int)start+1);
                pos = checked((int)pos-1);
                esc = '\\';
                charsToEscape = "`";
            }
            else if (text[start] == '[') {
                Debug.Assert(text[checked((int)pos-1)] == ']', "Invalid identifyer bracketing of name " + new string(text, start, pos-start) + " pos = " + pos.ToString(CultureInfo.InvariantCulture) + ", end = " + text[checked((int)pos-1)].ToString(CultureInfo.InvariantCulture));
                start = checked((int)start+1);
                pos = checked((int)pos-1);
                esc = '\\';
                charsToEscape = "]\\";
            }

            if (esc != '\0') {
                // scan the name in search for the ESC
                int posEcho = start;

                for (int i = start; i < pos; i++) {
                    if (text[i] == esc) {
                        if (i+1 < pos && charsToEscape.IndexOf(text[i+1]) >= 0) {
                            i++;
                        }
                    }
                    text[posEcho] = text[i];
                    posEcho++;
                }
                pos = posEcho;
            }

            if (pos == start)
                throw ExprException.InvalidName(new string(text, saveStart, savePos - saveStart));

            return new string(text, start, pos - start);
        }
    }
}
