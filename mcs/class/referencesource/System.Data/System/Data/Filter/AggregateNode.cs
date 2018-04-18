//------------------------------------------------------------------------------
// <copyright file="AggregateNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal enum Aggregate {
        None = FunctionId.none,
        Sum = FunctionId.Sum,
        Avg = FunctionId.Avg,
        Min = FunctionId.Min,
        Max = FunctionId.Max,
        Count = FunctionId.Count,
        StDev = FunctionId.StDev,   // Statistical standard deviation
        Var = FunctionId.Var,       // Statistical variance
    }

    internal sealed class AggregateNode : ExpressionNode {
        private readonly AggregateType type;
        private readonly Aggregate aggregate;
        private readonly bool local;     // set to true if the aggregate calculated localy (for the current table)

        private readonly string relationName;
        private readonly string columnName;

        // 

        private DataTable childTable;
        private DataColumn column;
        private DataRelation relation;

        internal AggregateNode(DataTable table, FunctionId aggregateType, string columnName) :
            this(table, aggregateType, columnName, true, null) {
        }

        internal AggregateNode(DataTable table, FunctionId aggregateType, string columnName, string relationName) :
            this(table, aggregateType, columnName, false, relationName) {
        }

        internal AggregateNode(DataTable table, FunctionId aggregateType, string columnName, bool local, string relationName) : base(table) {
            Debug.Assert(columnName != null, "Invalid parameter column name (null).");
            this.aggregate = (Aggregate)(int)aggregateType;

            if (aggregateType == FunctionId.Sum)
                this.type = AggregateType.Sum;
            else if (aggregateType == FunctionId.Avg)
                this.type = AggregateType.Mean;
            else if (aggregateType == FunctionId.Min)
                this.type = AggregateType.Min;
            else if (aggregateType == FunctionId.Max)
                this.type = AggregateType.Max;
            else if (aggregateType == FunctionId.Count)
                this.type = AggregateType.Count;
            else if (aggregateType == FunctionId.Var)
                this.type = AggregateType.Var;
            else if (aggregateType == FunctionId.StDev)
                this.type = AggregateType.StDev;
            else {
                throw ExprException.UndefinedFunction(Function.FunctionName[(Int32)aggregateType]);
            }

            this.local = local;
            this.relationName = relationName;
            this.columnName = columnName;
        }
        internal override void Bind(DataTable table, List<DataColumn> list) {
            BindTable(table);
            if (table == null)
                throw ExprException.AggregateUnbound(this.ToString());

            if (local) {
                relation = null;
            }
            else {
                DataRelationCollection relations;
                relations = table.ChildRelations;

                if (relationName == null) {
                    // must have one and only one relation

                    if (relations.Count > 1) {
                        throw ExprException.UnresolvedRelation(table.TableName, this.ToString());
                    }
                    if (relations.Count == 1) {
                        relation = relations[0];
                    }
                    else {
                        throw ExprException.AggregateUnbound(this.ToString());
                    }
                }
                else {
                    relation = relations[relationName];
                }
            }

            childTable = (relation == null) ? table : relation.ChildTable;

            this.column = childTable.Columns[columnName];

            if (column == null)
                throw ExprException.UnboundName(columnName);

            // add column to the dependency list, do not add duplicate columns

            Debug.Assert(column != null, "Failed to bind column " + columnName);

            int i;
            for (i = 0; i < list.Count; i++) {
                // walk the list, check if the current column already on the list
                DataColumn dataColumn = (DataColumn)list[i];
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

        internal static void Bind(DataRelation relation, List<DataColumn> list)
        {
            if (null != relation) {                
                // add the ends of the relationship the expression depends on
                foreach (DataColumn c in relation.ChildColumnsReference) {
                    if (!list.Contains(c)) {
                        list.Add(c);
                    }
                }
                foreach (DataColumn c in relation.ParentColumnsReference) {
                    if (!list.Contains(c)) {
                        list.Add(c);
                    }
                }
            }
        }

        internal override object Eval() {
            return Eval(null, DataRowVersion.Default);
        }

        internal override object Eval(DataRow row, DataRowVersion version) {
            if (childTable == null)
                throw ExprException.AggregateUnbound(this.ToString());

            DataRow[] rows;

            if (local) {
                rows = new DataRow[childTable.Rows.Count];
                childTable.Rows.CopyTo(rows, 0);
            }
            else {
                if (row == null) {
                    throw ExprException.EvalNoContext();
                }
                if (relation == null) {
                    throw ExprException.AggregateUnbound(this.ToString());
                }
                rows = row.GetChildRows(relation, version);
            }

            int[] records;
            if (version == DataRowVersion.Proposed) {
                version = DataRowVersion.Default;
            }

            List<int> recordList = new List<int>();

            for (int i = 0; i < rows.Length; i++) {
                if (rows[i].RowState == DataRowState.Deleted) {
                    if (DataRowAction.Rollback != rows[i]._action) {
                        continue;
                    }
                    Debug.Assert(DataRowVersion.Original == version, "wrong version");
                    version = DataRowVersion.Original;
                }
                else if ((DataRowAction.Rollback == rows[i]._action) && (rows[i].RowState == DataRowState.Added)) {
                    continue; // WebData 91297
                }
                if (version == DataRowVersion.Original && rows[i].oldRecord == -1) {
                    continue;
                }
                recordList.Add(rows[i].GetRecordFromVersion(version));
            }
            records = recordList.ToArray();

            return column.GetAggregateValue(records, type);
        }

        // Helper for the DataTable.Compute method
        internal override object Eval(int[] records) {
            if (childTable == null)
                throw ExprException.AggregateUnbound(this.ToString());
            if (!local) {
                throw ExprException.ComputeNotAggregate(this.ToString());
            }
            return column.GetAggregateValue(records, type);
        }

        internal override bool IsConstant() {
            return false;
        }

        internal override bool IsTableConstant() {
            return local;
        }

        internal override bool HasLocalAggregate() {
            return local;
        }
        
        internal override bool HasRemoteAggregate() {
            return !local;
        }

        internal override bool DependsOn(DataColumn column) {
            if (this.column == column) {
                return true;
            }
            if (this.column.Computed) {
                return this.column.DataExpression.DependsOn(column);
            }
            return false;
        }

        internal override ExpressionNode Optimize() {
            return this;
        }
    }
}
