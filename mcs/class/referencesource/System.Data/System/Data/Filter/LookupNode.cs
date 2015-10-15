//------------------------------------------------------------------------------
// <copyright file="LookupNode.cs" company="Microsoft">
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

    internal sealed class LookupNode : ExpressionNode {
        private readonly string relationName;    // can be null
        private readonly string columnName;

        private DataColumn column;
        private DataRelation relation;

        internal LookupNode(DataTable table, string columnName, string relationName) : base(table) {
            this.relationName = relationName;
            this.columnName = columnName;
        }

        internal override void Bind(DataTable table, List<DataColumn> list) {
            BindTable(table);
            column = null;  // clear for rebinding (if original binding was valid)
            relation = null;

            if (table == null)
                throw ExprException.ExpressionUnbound(this.ToString());

            // First find parent table

            DataRelationCollection relations;
            relations = table.ParentRelations;

            if (relationName == null) {
                // must have one and only one relation

                if (relations.Count > 1) {
                    throw ExprException.UnresolvedRelation(table.TableName, this.ToString());
                }
                relation = relations[0];
            }
            else {
                relation = relations[relationName];
            }
            if (null == relation) {
                throw ExprException.BindFailure(relationName);// WebData 112162: this operation is not clne specific, throw generic exception
            }
            DataTable parentTable = relation.ParentTable;

            Debug.Assert(relation != null, "Invalid relation: no parent table.");
            Debug.Assert(columnName != null, "All Lookup expressions have columnName set.");

            this.column = parentTable.Columns[columnName];

            if (column == null)
                throw ExprException.UnboundName(columnName);

            // add column to the dependency list

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

            // SQLBU 383715: Staleness of computed values in expression column as the relationship end columns are not being added to the dependent column list.
            AggregateNode.Bind(relation, list);
        }

        internal override object Eval() {
            throw ExprException.EvalNoContext();
        }

        internal override object Eval(DataRow row, DataRowVersion version) {
            if (column == null || relation == null)
                throw ExprException.ExpressionUnbound(this.ToString());

            DataRow parent = row.GetParentRow(relation, version);
            if (parent == null)
                return DBNull.Value;

            return parent[column, parent.HasVersion(version) ? version : DataRowVersion.Current]; // Microsoft : 
        }

        internal override object Eval(int[] recordNos) {
            throw ExprException.ComputeNotAggregate(this.ToString());
        }

        internal override bool IsConstant() {
            return false;
        }

        internal override bool IsTableConstant() {
            return false;
        }

        internal override bool HasLocalAggregate() {
            return false;
        }

        internal override bool HasRemoteAggregate() {
            return false;
        }

        internal override bool DependsOn(DataColumn column) {
            if (this.column == column) {
                return true;
            }
            return false;
        }

        internal override ExpressionNode Optimize() {
            return this;
        }
    }
}
