//------------------------------------------------------------------------------
// <copyright file="Select.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Diagnostics;

    internal sealed class Select {
        private readonly DataTable table;
        private readonly IndexField[] IndexFields;
        private DataViewRowState recordStates;
        private DataExpression rowFilter;
        private ExpressionNode expression;

        private Index index;

        private int[] records;
        private int recordCount;

        private ExpressionNode linearExpression;
        private bool candidatesForBinarySearch;

        private sealed class ColumnInfo {
            public bool       flag = false;               // Misc. Use
            public bool       equalsOperator = false;     // True when the associated expr has = Operator defined
            public BinaryNode expr = null;                // Binary Search capable expression associated
        }
        ColumnInfo[] candidateColumns;
        int nCandidates;
        int matchedCandidates;

        public Select(DataTable table, string filterExpression, string sort, DataViewRowState recordStates) {
            this.table = table;
            IndexFields = table.ParseSortString(sort);
            if (filterExpression != null && filterExpression.Length > 0) {
                this.rowFilter = new DataExpression(this.table, filterExpression);
                this.expression = this.rowFilter.ExpressionNode;
            }
            this.recordStates = recordStates;
        }

        private bool IsSupportedOperator(int op) {
            return ((op >= Operators.EqualTo && op <= Operators.LessOrEqual) || op == Operators.Is || op == Operators.IsNot);
        }

        // Microsoft : Gathers all linear expressions in to this.linearExpression and all binary expressions in to their respective candidate columns expressions
        private void AnalyzeExpression(BinaryNode expr) {
            if (this.linearExpression == this.expression)
                return;

            if (expr.op == Operators.Or) {
                this.linearExpression = this.expression;
                return;
            }
            else
            if (expr.op == Operators.And) {
                bool isLeft=false, isRight=false;
                if (expr.left is BinaryNode) {
                    AnalyzeExpression((BinaryNode)expr.left);
                    if (this.linearExpression == this.expression)
                        return;
                    isLeft = true;
                }
                else {
                    UnaryNode unaryNode = expr.left as UnaryNode;
                    if (unaryNode != null) {
                        while (unaryNode.op == Operators.Noop && unaryNode.right is UnaryNode && ((UnaryNode)unaryNode.right).op == Operators.Noop) {
                            unaryNode = (UnaryNode)unaryNode.right;
                        }                        
                        if (unaryNode.op == Operators.Noop && unaryNode.right is BinaryNode) {
                            AnalyzeExpression((BinaryNode)(unaryNode.right));
                            if (this.linearExpression == this.expression) {
                                return;
                            }
                            isLeft = true;
                        } 
                    }
                }

                if (expr.right is BinaryNode) {
                    AnalyzeExpression((BinaryNode)expr.right);
                    if (this.linearExpression == this.expression)
                        return;
                    isRight = true;
                }
                else {
                    UnaryNode unaryNode = expr.right as UnaryNode;
                    if (unaryNode != null) {
                        while (unaryNode.op == Operators.Noop && unaryNode.right is UnaryNode && ((UnaryNode)unaryNode.right).op == Operators.Noop) {
                            unaryNode = (UnaryNode)unaryNode.right;
                        }
                        if (unaryNode.op == Operators.Noop && unaryNode.right is BinaryNode) { 
                            AnalyzeExpression((BinaryNode)(unaryNode.right));
                            if (this.linearExpression == this.expression) {
                                return;
                            }
                            // SQLBU 497534: DataTable.Select() returns incorrect results with multiple statements depending '(' and ')'
                            // from copy paste error fixing SQLBU 342141
                            isRight = true;
                        }
                    }
                }

                if (isLeft && isRight)
                    return;

                ExpressionNode e = isLeft ? expr.right : expr.left;
                this.linearExpression = (this.linearExpression == null ? e : new BinaryNode(table, Operators.And, e, this.linearExpression));
                return;
            }
            else
            if (IsSupportedOperator(expr.op)) {
                if (expr.left is NameNode && expr.right is ConstNode) {
                    ColumnInfo canColumn = (ColumnInfo)candidateColumns[((NameNode)(expr.left)).column.Ordinal];
                    canColumn.expr = (canColumn.expr == null ? expr : new BinaryNode(table, Operators.And, expr, canColumn.expr));
                    if (expr.op == Operators.EqualTo) {
                        canColumn.equalsOperator = true;
                    }
                    candidatesForBinarySearch = true;
                    return;
                }
                else
                if (expr.right is NameNode && expr.left is ConstNode) {
                    ExpressionNode temp = expr.left;
                    expr.left = expr.right;
                    expr.right = temp;
                    switch(expr.op) {
                        case Operators.GreaterThen:     expr.op = Operators.LessThen; break;
                        case Operators.LessThen:        expr.op = Operators.GreaterThen; break;
                        case Operators.GreaterOrEqual:  expr.op = Operators.LessOrEqual; break;
                        case Operators.LessOrEqual:     expr.op = Operators.GreaterOrEqual; break;
                        default : break;
                    }
                    ColumnInfo canColumn = (ColumnInfo)candidateColumns[((NameNode)(expr.left)).column.Ordinal];
                    canColumn.expr = (canColumn.expr == null ? expr : new BinaryNode(table, Operators.And, expr, canColumn.expr));
                    if (expr.op == Operators.EqualTo) {
                        canColumn.equalsOperator = true;
                    }
                    candidatesForBinarySearch = true;
                    return;
                }
            }

            this.linearExpression = (this.linearExpression == null ? expr : new BinaryNode(table, Operators.And, expr, this.linearExpression));
            return;
        }

        private bool CompareSortIndexDesc(IndexField[] fields) {
            if (fields.Length < IndexFields.Length)
                return false;
            int j=0;
            for (int i = 0; i < fields.Length && j < IndexFields.Length; i++) {
                if (fields[i] == IndexFields[j]) {
                    j++;
                }
                else {
                    ColumnInfo canColumn = candidateColumns[fields[i].Column.Ordinal];
                    if (!(canColumn != null && canColumn.equalsOperator))
                        return false;
                }
            }
            return j == IndexFields.Length;
        }

        private bool FindSortIndex() {
            index = null;
            this.table.indexesLock.AcquireReaderLock(-1);
             try{
                int count = this.table.indexes.Count;
                int rowsCount = this.table.Rows.Count;
                for (int i = 0; i < count; i++) {
                    Index ndx = (Index)table.indexes[i];
                    if (ndx.RecordStates != recordStates)
                        continue;
                    if(!ndx.IsSharable) {
                        continue;
                    }
                    if (CompareSortIndexDesc(ndx.IndexFields)) {
                        index = ndx;
                        return true;
                    }
                }
             }
            finally {
                this.table.indexesLock.ReleaseReaderLock();
            }
            return false;
        }

        // Returns no. of columns that are matched
        private int CompareClosestCandidateIndexDesc(IndexField[] fields) {
            int count = (fields.Length < nCandidates ? fields.Length : nCandidates);
            int i = 0;
            for (; i < count; i++) {
                ColumnInfo canColumn = candidateColumns[fields[i].Column.Ordinal];
                if (canColumn == null || canColumn.expr == null) {
                    break;
                }
                else
                if (!canColumn.equalsOperator) {
                    return i+1;
                }
            }
            return i;
        }

        // Returns whether the found index (if any) is a sort index as well
        private bool FindClosestCandidateIndex() {
            index = null;
            matchedCandidates = 0;
            bool sortPriority = true;
            this.table.indexesLock.AcquireReaderLock(-1);
            try {
                int count = this.table.indexes.Count;
                int rowsCount = this.table.Rows.Count;
                for (int i = 0; i < count; i++) {
                    Index ndx = (Index)table.indexes[i];
                    if (ndx.RecordStates != recordStates)
                        continue;
                    if(!ndx.IsSharable)
                        continue;
                    int match = CompareClosestCandidateIndexDesc(ndx.IndexFields);
                    if (match > matchedCandidates || (match == matchedCandidates && !sortPriority)) {
                        matchedCandidates = match;
                        index = ndx;
                        sortPriority = CompareSortIndexDesc(ndx.IndexFields);
                        if (matchedCandidates == nCandidates && sortPriority) {
                            return true;
                        }
                    }
                }
            }
            finally {
        this.table.indexesLock.ReleaseReaderLock();
        }

            return (index != null ? sortPriority : false);
        }

        // Initialize candidate columns to new columnInfo and leave all non candidate columns to null
        private void InitCandidateColumns() {
            nCandidates = 0;
            candidateColumns = new ColumnInfo[this.table.Columns.Count];
            if (this.rowFilter == null)
                return;
            DataColumn[] depColumns = rowFilter.GetDependency();
            for (int i = 0; i < depColumns.Length; i++) {
                if (depColumns[i].Table == this.table) {
                    candidateColumns[depColumns[i].Ordinal] = new ColumnInfo();
                    nCandidates++;
                }
            }
        }

        // Based on the required sorting and candidate columns settings, create a new index; Should be called only when there is no existing index to be reused
        private void CreateIndex() {
            if (index == null) {
                if (nCandidates == 0) {
                    index = new Index(table, IndexFields, recordStates, null);
                    index.AddRef();
                }
                else {
                    int i;
                    int lenCanColumns = candidateColumns.Length;
                    int lenIndexDesc = IndexFields.Length;
                    bool equalsOperator = true;
                    for (i=0; i<lenCanColumns; i++) {
                        if (candidateColumns[i] != null) {
                            if (!candidateColumns[i].equalsOperator) {
                                equalsOperator = false;
                                break;
                            }
                        }
                    }

                    int j=0;
                    for (i=0; i < lenIndexDesc; i++) {
                        ColumnInfo candidateColumn = candidateColumns[IndexFields[i].Column.Ordinal];
                        if (candidateColumn != null) {
                            candidateColumn.flag = true;
                            j++;
                        }
                    }
                    int indexNotInCandidates = lenIndexDesc - j;
                    int candidatesNotInIndex = nCandidates - j;
                    IndexField[] ndxFields = new IndexField[nCandidates + indexNotInCandidates];

                    if (equalsOperator) {
                        j=0;
                        for (i=0; i<lenCanColumns; i++) {
                            if (candidateColumns[i] != null) {
                                ndxFields[j++] = new IndexField(this.table.Columns[i], isDescending: false);
                                candidateColumns[i].flag = false;// this means it is processed
                            }
                        }
                        for (i=0; i<lenIndexDesc; i++) {
                            ColumnInfo canColumn = candidateColumns[IndexFields[i].Column.Ordinal];
                            if (canColumn == null || canColumn.flag) { // if sort column is not a filter col , or not processed
                                ndxFields[j++] = IndexFields[i];
                                if (canColumn != null) {
                                    canColumn.flag = false;
                                }
                            }                                
                        }

                        for(i = 0; i < candidateColumns.Length; i++) {
                            if (candidateColumns[i] != null) {
                                candidateColumns[i].flag = false;// same as before, it is false when it returns 
                            }
                            
                        }

                        // Debug.Assert(j == candidatesNotInIndex, "Whole ndxDesc should be filled!");

                        index = new Index(table, ndxFields, recordStates, null);
                        if (!IsOperatorIn(this.expression)) {
                            // if the expression contains an 'IN' operator, the index will not be shared
                            // therefore we do not need to index.AddRef, also table would track index consuming more memory until first write
                            index.AddRef();
                        }


                        matchedCandidates = nCandidates;
                     }
                     else {
                        for (i=0; i<lenIndexDesc; i++) {
                            ndxFields[i] = IndexFields[i];
                            ColumnInfo canColumn = candidateColumns[IndexFields[i].Column.Ordinal];
                            if (canColumn != null)
                                canColumn.flag = true;
                        }
                         j=i;
                        for (i=0; i<lenCanColumns; i++) {
                            if (candidateColumns[i] != null) {
                                if(!candidateColumns[i].flag) {
                                    ndxFields[j++] = new IndexField(this.table.Columns[i], isDescending: false);
                                }
                                else {
                                    candidateColumns[i].flag = false;
                                }
                            }
                        }
//                        Debug.Assert(j == nCandidates+indexNotInCandidates, "Whole ndxDesc should be filled!");
                                                
                        index = new Index(table, ndxFields, recordStates, null);
                        matchedCandidates = 0;
                        if (this.linearExpression != this.expression) {
                            IndexField[] fields = index.IndexFields;
                            while (matchedCandidates < j) { // Microsoft : j = index.IndexDesc.Length
                                ColumnInfo canColumn = candidateColumns[fields[matchedCandidates].Column.Ordinal];
                                if (canColumn == null || canColumn.expr == null)
                                    break;
                                matchedCandidates++;
                                if (!canColumn.equalsOperator)
                                    break;
                            }
                        }
                        for(i = 0; i < candidateColumns.Length; i++) {
                            if (candidateColumns[i] != null) {
                                candidateColumns[i].flag = false;// same as before, it is false when it returns 
                            }
                        }
                    }
                }
            }
        }


        private bool IsOperatorIn(ExpressionNode enode) {
            BinaryNode bnode = (enode as BinaryNode);
            if (null != bnode) {
                if (Operators.In == bnode.op  ||
                    IsOperatorIn(bnode.right) ||
                    IsOperatorIn(bnode.left))
                {
                    return true;
                }
            }
            return false;
        }



        // Based on the current index and candidate columns settings, build the linear expression; Should be called only when there is atleast something for Binary Searching
        private void BuildLinearExpression() {
            int i;
            IndexField[] fields = index.IndexFields;
            int lenId = fields.Length;
            Debug.Assert(matchedCandidates > 0 && matchedCandidates <= lenId, "BuildLinearExpression : Invalid Index");
            for (i=0; i<matchedCandidates; i++) {
                ColumnInfo canColumn = candidateColumns[fields[i].Column.Ordinal];
                Debug.Assert(canColumn != null && canColumn.expr != null, "BuildLinearExpression : Must be a matched candidate");
                canColumn.flag = true;
            }
            //this is invalid assert, assumption was that all equals operator exists at the begining of candidateColumns
            // but with QFE 1704, this assumption is not true anymore
//            Debug.Assert(matchedCandidates==1 || candidateColumns[matchedCandidates-1].equalsOperator, "BuildLinearExpression : Invalid matched candidates");
            int lenCanColumns = candidateColumns.Length;
            for (i=0; i<lenCanColumns; i++) {
                if (candidateColumns[i] != null) {
                    if (!candidateColumns[i].flag) {
                        if (candidateColumns[i].expr != null) {
                            this.linearExpression = (this.linearExpression == null ? candidateColumns[i].expr : new BinaryNode(table, Operators.And, candidateColumns[i].expr, this.linearExpression));
                        }
                    }
                    else {
                        candidateColumns[i].flag = false;
                    }
                }
            }
//            Debug.Assert(this.linearExpression != null, "BuildLinearExpression : How come there is nothing to search linearly"); bug 97446
        }

        public DataRow[] SelectRows() {
            bool needSorting = true;

            InitCandidateColumns();

            if (this.expression is BinaryNode) {
                AnalyzeExpression((BinaryNode)this.expression);
                if (!candidatesForBinarySearch) {
                    this.linearExpression = this.expression;
                }
                if (this.linearExpression == this.expression) {
                    for (int i=0; i<candidateColumns.Length; i++) {
                        if (candidateColumns[i] != null) {
                            candidateColumns[i].equalsOperator = false;
                            candidateColumns[i].expr = null;
                        }
                    }
                }
                else {
                    needSorting = !FindClosestCandidateIndex();
                }
            }
            else {
                this.linearExpression = this.expression;
            }

            if (index == null && (IndexFields.Length > 0 || this.linearExpression == this.expression)) {
                needSorting = !FindSortIndex();
            }

            if (index == null) {
                CreateIndex();
                needSorting = false;
            }

            if (index.RecordCount == 0)
                return table.NewRowArray(0);

            Range range;
            if (matchedCandidates == 0) { // Microsoft : Either dont have rowFilter or only linear search expression
                range = new Range(0, index.RecordCount-1);
                Debug.Assert(!needSorting, "What are we doing here if no real reuse of this index ?");
                this.linearExpression = this.expression;
                return GetLinearFilteredRows(range);
            }
            else {
                range = GetBinaryFilteredRecords();
                if (range.Count == 0)
                    return table.NewRowArray(0);
                if (matchedCandidates < nCandidates) {
                    BuildLinearExpression();
                }
                if (!needSorting) {
                    return GetLinearFilteredRows(range);
                }
                else {
                    this.records = GetLinearFilteredRecords(range);
                    this.recordCount = this.records.Length;
                    if (this.recordCount == 0)
                        return table.NewRowArray(0);
                    Sort(0, this.recordCount-1);
                    return GetRows();
                }
            }
        }

        public DataRow[] GetRows() {
            DataRow[] newRows = table.NewRowArray(recordCount);
            for (int i = 0; i < newRows.Length; i++) {
                newRows[i] = table.recordManager[records[i]];
            }
            return newRows;
        }

        private bool AcceptRecord(int record) {
            DataRow row = table.recordManager[record];

            if (row == null)
                return true;

            // 

            DataRowVersion version = DataRowVersion.Default;
            if (row.oldRecord == record) {
                version = DataRowVersion.Original;
            }
            else if (row.newRecord == record) {
                version = DataRowVersion.Current;
            }
            else if (row.tempRecord == record) {
                version = DataRowVersion.Proposed;
            }

            object val = this.linearExpression.Eval(row, version);
            bool result;
            try {
                result = DataExpression.ToBoolean(val);
            }
            catch (Exception e) {
                // 
                if (!ADP.IsCatchableExceptionType(e)) {
                    throw;
                }
                throw ExprException.FilterConvertion(this.rowFilter.Expression);
            }
            return result;
        }

        private int Eval(BinaryNode expr, DataRow row, DataRowVersion version) {
            if (expr.op == Operators.And) {
                int lResult = Eval((BinaryNode)expr.left,row,version);
                if (lResult != 0)
                    return lResult;
                int rResult = Eval((BinaryNode)expr.right,row,version);
                if (rResult != 0)
                    return rResult;
                return 0;
            }

            long c = 0;
            object vLeft  = expr.left.Eval(row, version);
            if (expr.op != Operators.Is && expr.op != Operators.IsNot) {
                object vRight = expr.right.Eval(row, version);
                bool isLConst = (expr.left is ConstNode);
                bool isRConst = (expr.right is ConstNode);

                if ((vLeft == DBNull.Value)||(expr.left.IsSqlColumn && DataStorage.IsObjectSqlNull(vLeft)))
                    return -1;
                if ((vRight == DBNull.Value)||(expr.right.IsSqlColumn && DataStorage.IsObjectSqlNull(vRight)))
                    return 1;

                StorageType leftType = DataStorage.GetStorageType(vLeft.GetType());
                if (StorageType.Char == leftType) {
                    if ((isRConst)||(!expr.right.IsSqlColumn))
                        vRight = Convert.ToChar(vRight, table.FormatProvider);
                    else
                       vRight = SqlConvert.ChangeType2(vRight, StorageType.Char, typeof(char), table.FormatProvider);
                }

                StorageType rightType = DataStorage.GetStorageType(vRight.GetType());
                StorageType resultType;
                if (expr.left.IsSqlColumn || expr.right.IsSqlColumn) {
                    resultType = expr.ResultSqlType(leftType, rightType, isLConst, isRConst, expr.op);
                }
                else {
                    resultType = expr.ResultType(leftType, rightType, isLConst, isRConst, expr.op);
                }
                if (StorageType.Empty == resultType) {
                    expr.SetTypeMismatchError(expr.op, vLeft.GetType(), vRight.GetType());
                }

                // if comparing a Guid column value against a string literal
                // use InvariantCulture instead of DataTable.Locale because in the Danish related cultures
                // sorting a Guid as a string has different results than in Invariant and English related cultures.
                // This fix is restricted to DataTable.Select("GuidColumn = 'string literal'") types of queries
                NameNode namedNode = null;
                System.Globalization.CompareInfo comparer =
                    ((isLConst && !isRConst && (leftType == StorageType.String) && (rightType == StorageType.Guid) && (null != (namedNode = expr.right as NameNode)) && (namedNode.column.DataType == typeof(Guid))) ||
                     (isRConst && !isLConst && (rightType == StorageType.String) && (leftType == StorageType.Guid) && (null != (namedNode = expr.left as NameNode)) && (namedNode.column.DataType == typeof(Guid))))
                     ? System.Globalization.CultureInfo.InvariantCulture.CompareInfo : null;

                c = expr.BinaryCompare(vLeft, vRight, resultType, expr.op, comparer);
            }
            switch(expr.op) {
                case Operators.EqualTo:         c = (c == 0 ? 0 : c < 0  ? -1 :  1); break;
                case Operators.GreaterThen:     c = (c > 0  ? 0 : -1); break;
                case Operators.LessThen:        c = (c < 0  ? 0 : 1); break;
                case Operators.GreaterOrEqual:  c = (c >= 0 ? 0 : -1); break;
                case Operators.LessOrEqual:     c = (c <= 0 ? 0 : 1); break;
                case Operators.Is:              c = (vLeft == DBNull.Value ? 0 : -1); break;
                case Operators.IsNot:           c = (vLeft != DBNull.Value ? 0 : 1);  break;
                default:                        Debug.Assert(true, "Unsupported Binary Search Operator!"); break;
            }
            return (int)c;
        }

        private int Evaluate(int record) {
            DataRow row = table.recordManager[record];

            if (row == null)
                return 0;

            // 

            DataRowVersion version = DataRowVersion.Default;
            if (row.oldRecord == record) {
                version = DataRowVersion.Original;
            }
            else if (row.newRecord == record) {
                version = DataRowVersion.Current;
            }
            else if (row.tempRecord == record) {
                version = DataRowVersion.Proposed;
            }

            IndexField[] fields = index.IndexFields;
            for (int i=0; i < matchedCandidates; i++) {
                int columnOrdinal = fields[i].Column.Ordinal;
                Debug.Assert(candidateColumns[columnOrdinal] != null, "How come this is not a candidate column");
                Debug.Assert(candidateColumns[columnOrdinal].expr != null, "How come there is no associated expression");
                int c = Eval(candidateColumns[columnOrdinal].expr, row, version);
                if (c != 0)
                    return fields[i].IsDescending ? -c : c;
            }
            return 0;
        }

        private int FindFirstMatchingRecord() {
            int rec = -1;
            int lo = 0;
            int hi = index.RecordCount - 1;
            while (lo <= hi) {
                int i = lo + hi >> 1;
                int recNo = index.GetRecord(i);
                int c = Evaluate(recNo);
                if (c == 0) { rec = i; }
                if (c < 0) lo = i + 1;
                else hi = i - 1;
            }
            return rec;
        }

        private int FindLastMatchingRecord(int lo) {
            int rec = -1;
            int hi = index.RecordCount - 1;
            while (lo <= hi) {
                int i = lo + hi >> 1;
                int recNo = index.GetRecord(i);
                int c = Evaluate(recNo);
                if (c == 0) { rec = i; }
                if (c <= 0) lo = i + 1;
                else hi = i - 1;
            }
            return rec;
        }

        private Range GetBinaryFilteredRecords() {
            if (matchedCandidates == 0) {
                return new Range(0, index.RecordCount-1);
            }
            Debug.Assert(matchedCandidates <= index.IndexFields.Length, "GetBinaryFilteredRecords : Invalid Index");
            int lo = FindFirstMatchingRecord();
            if (lo == -1) {
                return new Range();
            }
            int hi = FindLastMatchingRecord(lo);
            Debug.Assert (lo <= hi, "GetBinaryFilteredRecords : Invalid Search Results");
            return new Range(lo, hi);
        }

        private int[] GetLinearFilteredRecords(Range range) {
            if (this.linearExpression == null) {
                int[] resultRecords = new int[range.Count];
                RBTree<int>.RBTreeEnumerator iterator = index.GetEnumerator(range.Min);
                for (int i = 0; i < range.Count && iterator.MoveNext(); i++) {
                    resultRecords[i] = iterator.Current;
                }
                return resultRecords;
            }
            else {
                List<int> matchingRecords = new List<int>();
                RBTree<int>.RBTreeEnumerator iterator = index.GetEnumerator(range.Min);
                for (int i = 0; i < range.Count && iterator.MoveNext(); i++) {
                    if (AcceptRecord(iterator.Current)) {
                        matchingRecords.Add(iterator.Current);
                    }
                }
                return matchingRecords.ToArray();
            }
        }

        private DataRow[] GetLinearFilteredRows(Range range) {
            DataRow[] resultRows;
            if (this.linearExpression == null) {
                return index.GetRows(range);
            }            

            List<DataRow> matchingRows = new List<DataRow>();
            RBTree<int>.RBTreeEnumerator iterator = index.GetEnumerator(range.Min);
            for (int i = 0; i < range.Count && iterator.MoveNext(); i++) {
                if (AcceptRecord(iterator.Current)) {
                    matchingRows.Add(table.recordManager[iterator.Current]);
                }
            }
            resultRows = table.NewRowArray(matchingRows.Count);
            matchingRows.CopyTo(resultRows);
            return resultRows;
        }


        private int CompareRecords(int record1, int record2) {
            int lenIndexDesc = IndexFields.Length;
            for (int i = 0; i < lenIndexDesc; i++) {
                int c = IndexFields[i].Column.Compare(record1, record2);
                if (c != 0) {
                    if (IndexFields[i].IsDescending) c = -c;
                    return c;
                }
            }

            long id1 = table.recordManager[record1] == null? 0: table.recordManager[record1].rowID;
            long id2 = table.recordManager[record2] == null ? 0 : table.recordManager[record2].rowID;
            int diff = (id1 < id2) ? -1 : ((id2 < id1) ? 1 : 0);

            // if they're two records in the same row, we need to be able to distinguish them.
            if (diff == 0 && record1 != record2 &&
                table.recordManager[record1] != null && table.recordManager[record2] != null) {
                id1 = (int)table.recordManager[record1].GetRecordState(record1);
                id2 = (int)table.recordManager[record2].GetRecordState(record2);
                diff = (id1 < id2) ? -1 : ((id2 < id1) ? 1 : 0);
            }

            return diff;
        }

        private void Sort(int left, int right) {
            int i, j;
            int record;
            do {
                i = left;
                j = right;
                record = records[i + j >> 1];
                do {
                    while (CompareRecords(records[i], record) < 0) i++;
                    while (CompareRecords(records[j], record) > 0) j--;
                    if (i <= j) {
                        int r = records[i];
                        records[i] = records[j];
                        records[j] = r;
                        i++;
                        j--;
                    }
                } while (i <= j);
                if (left < j) Sort(left, j);
                left = i;
            } while (i < right);
        }
    }
}
