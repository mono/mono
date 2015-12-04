//------------------------------------------------------------------------------
// <copyright file="Merger.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <devdoc>
    /// Merge Utilities.
    /// </devdoc>
    internal sealed class Merger {
        private DataSet dataSet =  null;
        private DataTable dataTable =  null;
        private bool preserveChanges;
        private MissingSchemaAction missingSchemaAction;
        private bool isStandAlonetable = false;
        private bool _IgnoreNSforTableLookup = false; // Everett Behavior : SQL BU DT 370850

        internal Merger(DataSet dataSet, bool preserveChanges, MissingSchemaAction missingSchemaAction) {
            this.dataSet = dataSet;
            this.preserveChanges = preserveChanges;

            // map AddWithKey -> Add
            if (missingSchemaAction == MissingSchemaAction.AddWithKey)
                this.missingSchemaAction = MissingSchemaAction.Add;
            else
                this.missingSchemaAction = missingSchemaAction;
        }

        internal Merger(DataTable dataTable, bool preserveChanges, MissingSchemaAction missingSchemaAction) {
            isStandAlonetable = true;
            this.dataTable = dataTable;
            this.preserveChanges = preserveChanges;

            // map AddWithKey -> Add
            if (missingSchemaAction == MissingSchemaAction.AddWithKey)
                this.missingSchemaAction = MissingSchemaAction.Add;
            else
                this.missingSchemaAction = missingSchemaAction;
        }

        internal void MergeDataSet(DataSet source) {
            if (source == dataSet) return;  //somebody is doing an 'automerge'
            bool fEnforce = dataSet.EnforceConstraints;
            dataSet.EnforceConstraints = false;
            _IgnoreNSforTableLookup = (dataSet.namespaceURI != source.namespaceURI); // if two DataSets have different 
            // Namespaces, ignore NS for table lookups as we wont be able to find the right tables which inherits its NS

            List<DataColumn> existingColumns = null;// need to cache existing columns

            if (MissingSchemaAction.Add == missingSchemaAction)  {
                existingColumns = new List<DataColumn>(); // need to cache existing columns
                foreach(DataTable dt in dataSet.Tables) {
                    foreach(DataColumn dc in dt.Columns) {
                        existingColumns.Add(dc);
                    }
                }
            }


            for (int i = 0; i < source.Tables.Count;  i++) {
                MergeTableData(source.Tables[i]); // since column expression might have dependency on relation, we do not set
                //column expression at this point. We need to set it after adding relations
            }

            if (MissingSchemaAction.Ignore != missingSchemaAction) {
                // Add all independent constraints
                MergeConstraints(source);

                // Add all relationships
                for (int i = 0; i < source.Relations.Count;  i++) {
                    MergeRelation(source.Relations[i]);
                }
            }
// WebData 88234
            if (MissingSchemaAction.Add == missingSchemaAction) { // for which other options we should add expressions also?
                foreach (DataTable sourceTable in source.Tables) {
                    DataTable targetTable;
                    if (_IgnoreNSforTableLookup) {
                        targetTable = dataSet.Tables[sourceTable.TableName];
                    }
                    else {
                        targetTable = dataSet.Tables[sourceTable.TableName, sourceTable.Namespace];// we know that target table wont be null since MissingSchemaAction is Add , we have already added it!
                    }
                        
                    foreach(DataColumn dc in sourceTable.Columns) { // Should we overwrite the previous expression column? No, refer to spec, if it is new column we need to add the schema
                        if (dc.Computed) {
                            DataColumn targetColumn = targetTable.Columns[dc.ColumnName];
                            if (!existingColumns.Contains(targetColumn)) {
                                targetColumn.Expression = dc.Expression;
                            }
                        }
                    }
                 }
            }

            MergeExtendedProperties(source.ExtendedProperties, dataSet.ExtendedProperties);
            foreach(DataTable dt in dataSet.Tables)
                dt.EvaluateExpressions();

            dataSet.EnforceConstraints = fEnforce;
        }

        internal void MergeTable(DataTable src) {
            bool fEnforce = false;
            if (!isStandAlonetable) {
                if (src.DataSet == dataSet) return; //somebody is doing an 'automerge'
                fEnforce = dataSet.EnforceConstraints;
                dataSet.EnforceConstraints = false;
            }
            else {
                if (src == dataTable) return; //somebody is doing an 'automerge'
                dataTable.SuspendEnforceConstraints = true;
            }

            if (this.dataSet != null) { // this is ds.Merge
                // if source does not have a DS, or if NS of both DS does not match, ignore the NS
                if (src.DataSet == null || src.DataSet.namespaceURI != this.dataSet.namespaceURI) {
                    _IgnoreNSforTableLookup = true;
                }
            }
            else { // this is dt.Merge
                if (this.dataTable.DataSet == null || src.DataSet == null ||
                    src.DataSet.namespaceURI != this.dataTable.DataSet.namespaceURI) {
                    _IgnoreNSforTableLookup = true;
                }
            }            

            MergeTableData(src);

            DataTable dt = dataTable;
            if (dt == null && dataSet != null) {
                if (_IgnoreNSforTableLookup) {
                    dt = dataSet.Tables[src.TableName];
                }
                else {
                    dt = dataSet.Tables[src.TableName, src.Namespace];
                }
            }

            if (dt != null) {
                dt.EvaluateExpressions();
            }

            if (!isStandAlonetable) {
                dataSet.EnforceConstraints = fEnforce;
            }
            else {
                dataTable.SuspendEnforceConstraints = false;
                try {
                    if (dataTable.EnforceConstraints) {
                        dataTable.EnableConstraints();
                    }
                }
                catch(ConstraintException) {
                    if (dataTable.DataSet != null) {
                        dataTable.DataSet.EnforceConstraints = false;
                    }
                    throw;
                }
            }
        }

        private void MergeTable(DataTable src, DataTable dst) {
            int  rowsCount = src.Rows.Count;
            bool wasEmpty  = dst.Rows.Count == 0;
            if(0 < rowsCount) {
                Index   ndxSearch = null;
                DataKey key = default(DataKey);
                dst.SuspendIndexEvents();
                try {
                    if(! wasEmpty && dst.primaryKey != null) {
                        key = GetSrcKey(src, dst);
                        if (key.HasValue)
                            ndxSearch = dst.primaryKey.Key.GetSortIndex(DataViewRowState.OriginalRows | DataViewRowState.Added );
                    }
                    // SQLBU 414992: Serious performance issue when calling Merge
                    // this improves performance by iterating over the rows instead of computing their position
                    foreach(DataRow sourceRow in src.Rows) {
                        DataRow targetRow = null;
                        if(ndxSearch != null) {
                            targetRow = dst.FindMergeTarget(sourceRow, key, ndxSearch);
                        }
                        dst.MergeRow(sourceRow, targetRow, preserveChanges, ndxSearch);
                    }
                }
                finally {
                    dst.RestoreIndexEvents(true);
                }
            }
            MergeExtendedProperties(src.ExtendedProperties, dst.ExtendedProperties);
        }

        internal void MergeRows(DataRow[] rows) {
            DataTable src = null;
            DataTable dst = null;
            DataKey   key = default(DataKey);
            Index     ndxSearch = null;

            bool fEnforce = dataSet.EnforceConstraints;
            dataSet.EnforceConstraints = false;

            for (int i = 0; i < rows.Length; i++) {
                DataRow row = rows[i];

                if (row == null) {
                    throw ExceptionBuilder.ArgumentNull("rows[" + i + "]");
                }
                if (row.Table == null) {
                    throw ExceptionBuilder.ArgumentNull("rows[" + i + "].Table");
                }

                //somebody is doing an 'automerge'
                if (row.Table.DataSet == dataSet)
                    continue;

                if (src != row.Table) {                     // row.Table changed from prev. row.
                    src = row.Table;
                    dst = MergeSchema(row.Table);
                    if (dst == null) {
                        Debug.Assert(MissingSchemaAction.Ignore == missingSchemaAction, "MergeSchema failed");
                        dataSet.EnforceConstraints = fEnforce;
                        return;
                    }
                    if(dst.primaryKey != null) {
                        key = GetSrcKey(src, dst);
                    }
                    if (key.HasValue) {
                        // Getting our own copy instead. ndxSearch = dst.primaryKey.Key.GetSortIndex();
                        // IMO, Better would be to reuse index
                        // ndxSearch = dst.primaryKey.Key.GetSortIndex(DataViewRowState.OriginalRows | DataViewRowState.Added );
                        if (null != ndxSearch) {
                            ndxSearch.RemoveRef();
                            ndxSearch = null;
                        }
                        ndxSearch = new Index(dst, dst.primaryKey.Key.GetIndexDesc(), DataViewRowState.OriginalRows | DataViewRowState.Added, (IFilter)null);
                        ndxSearch.AddRef(); // need to addref twice, otherwise it will be collected
                        ndxSearch.AddRef(); // in past first adref was done in const
                    }
                }

                if (row.newRecord == -1 && row.oldRecord == -1)
                    continue;

                DataRow targetRow = null;
                if(0 < dst.Rows.Count && ndxSearch != null) {
                    targetRow = dst.FindMergeTarget(row, key, ndxSearch);
                }

                targetRow = dst.MergeRow(row, targetRow, preserveChanges, ndxSearch);

                if (targetRow.Table.dependentColumns != null && targetRow.Table.dependentColumns.Count > 0)
                    targetRow.Table.EvaluateExpressions(targetRow, DataRowAction.Change, null);
            }
            if (null != ndxSearch) {
                ndxSearch.RemoveRef();
                ndxSearch = null;
            }

            dataSet.EnforceConstraints = fEnforce;
        }

        private DataTable MergeSchema(DataTable table) {
            DataTable targetTable = null;
            if (!isStandAlonetable) {
                if (dataSet.Tables.Contains(table.TableName, true))
                    if (_IgnoreNSforTableLookup) {
                        targetTable = dataSet.Tables[table.TableName];
                        }
                    else {
                        targetTable = dataSet.Tables[table.TableName, table.Namespace];
                    }
            }
            else {
                targetTable = dataTable;
            }

            if (targetTable == null) { // in case of standalone table, we make sure that targetTable is not null, so if this check passes, it will be when it is called via detaset
                if (MissingSchemaAction.Add == missingSchemaAction) {
                    targetTable =  table.Clone(table.DataSet); // if we are here mainly we are called from DataSet.Merge at this point we don't set
                    //expression columns, since it might have refer to other columns via relation, so it wont find the table and we get exception;
                    // do it after adding relations.
                    dataSet.Tables.Add(targetTable);
                }
                else if (MissingSchemaAction.Error == missingSchemaAction) {
                    throw ExceptionBuilder.MergeMissingDefinition(table.TableName);
                }
            }
            else {
                if (MissingSchemaAction.Ignore != missingSchemaAction) {
                    // Do the columns
                    int oldCount = targetTable.Columns.Count;
                    for (int i = 0; i < table.Columns.Count; i++) {
                        DataColumn src = table.Columns[i];
                        DataColumn dest = (targetTable.Columns.Contains(src.ColumnName, true)) ? targetTable.Columns[src.ColumnName] : null;
                        if (dest == null) {
                            if (MissingSchemaAction.Add == missingSchemaAction) {
                                dest = src.Clone();
                                targetTable.Columns.Add(dest);
                            }
                            else {
                                if (!isStandAlonetable)
                                    dataSet.RaiseMergeFailed(targetTable, Res.GetString(Res.DataMerge_MissingColumnDefinition, table.TableName, src.ColumnName), missingSchemaAction);
                                else
                                    throw ExceptionBuilder.MergeFailed(Res.GetString(Res.DataMerge_MissingColumnDefinition, table.TableName, src.ColumnName));
                            }
                        }
                        else {
                            if (dest.DataType != src.DataType || 
                                ((dest.DataType ==  typeof(DateTime)) && (dest.DateTimeMode != src.DateTimeMode) && ((dest.DateTimeMode & src.DateTimeMode) != DataSetDateTime.Unspecified))) {
                                if (!isStandAlonetable)
                                    dataSet.RaiseMergeFailed(targetTable, Res.GetString(Res.DataMerge_DataTypeMismatch, src.ColumnName), MissingSchemaAction.Error);
                                else
                                    throw ExceptionBuilder.MergeFailed(Res.GetString(Res.DataMerge_DataTypeMismatch, src.ColumnName));
                            }
                            // 


                            MergeExtendedProperties(src.ExtendedProperties, dest.ExtendedProperties);
                        }
                    }
                    
                    // Set DataExpression
                    if (isStandAlonetable) {
                        for (int i = oldCount; i < targetTable.Columns.Count; i++) {
                            targetTable.Columns[i].Expression = table.Columns[targetTable.Columns[i].ColumnName].Expression;
                        }
                    }

                    // check the PrimaryKey
                    DataColumn[] targetPKey = targetTable.PrimaryKey;
                    DataColumn[] tablePKey = table.PrimaryKey;
                    if (targetPKey.Length != tablePKey.Length) {
                        // special case when the target table does not have the PrimaryKey

                        if (targetPKey.Length == 0) {
                            DataColumn[] key = new DataColumn[tablePKey.Length];
                            for (int i = 0; i < tablePKey.Length; i++) {
                                key[i] = targetTable.Columns[tablePKey[i].ColumnName];
                            }
                            targetTable.PrimaryKey = key;
                        }
                        else if (tablePKey.Length != 0) {
                            dataSet.RaiseMergeFailed(targetTable, Res.GetString(Res.DataMerge_PrimaryKeyMismatch), missingSchemaAction);
                        }
                    }
                    else {
                        for (int i = 0; i < targetPKey.Length; i++) {
                            if (String.Compare(targetPKey[i].ColumnName, tablePKey[i].ColumnName, false, targetTable.Locale) != 0) {
                                dataSet.RaiseMergeFailed(table,
                                    Res.GetString(Res.DataMerge_PrimaryKeyColumnsMismatch, targetPKey[i].ColumnName, tablePKey[i].ColumnName),
                                    missingSchemaAction
                            );
                            }
                        }
                    }
                }

                MergeExtendedProperties(table.ExtendedProperties, targetTable.ExtendedProperties);
            }

            return targetTable;
        }

        private void MergeTableData(DataTable src) {
            DataTable dest = MergeSchema(src);
            if (dest == null) return;

            dest.MergingData = true;
            try {
                MergeTable(src, dest);
            }
            finally {
                dest.MergingData = false;
            }
        }

        private void MergeConstraints(DataSet source) {
            for (int i = 0; i < source.Tables.Count; i ++) {
                MergeConstraints(source.Tables[i]);
            }
        }

        private void MergeConstraints(DataTable table) {
            // Merge constraints
            for (int i = 0; i < table.Constraints.Count; i++) {
                Constraint src = table.Constraints[i];
                Constraint dest = src.Clone(dataSet, _IgnoreNSforTableLookup);

                if (dest == null) {
                    dataSet.RaiseMergeFailed(table,
                        Res.GetString(Res.DataMerge_MissingConstraint, src.GetType().FullName, src.ConstraintName),
                        missingSchemaAction
                    );
                }
                else {
                    Constraint cons = dest.Table.Constraints.FindConstraint(dest);
                    if (cons == null) {
                        if (MissingSchemaAction.Add == missingSchemaAction) {
                            try {
                                // try to keep the original name
                                dest.Table.Constraints.Add(dest);
                            }
                            catch (DuplicateNameException) {
                                // if fail, assume default name
                                dest.ConstraintName = "";
                                dest.Table.Constraints.Add(dest);
                            }
                        }
                        else if (MissingSchemaAction.Error == missingSchemaAction) {
                            dataSet.RaiseMergeFailed(table,
                                Res.GetString(Res.DataMerge_MissingConstraint, src.GetType().FullName, src.ConstraintName),
                                missingSchemaAction
                            );
                        }
                    }
                    else {
                        MergeExtendedProperties(src.ExtendedProperties, cons.ExtendedProperties);
                    }
                }
            }
        }

        private void MergeRelation(DataRelation relation) {
            Debug.Assert(MissingSchemaAction.Error == missingSchemaAction ||
                         MissingSchemaAction.Add == missingSchemaAction,
                         "Unexpected value of MissingSchemaAction parameter : " + ((Enum) missingSchemaAction).ToString());
            DataRelation destRelation = null;

            // try to find given relation in this dataSet

            int iDest = dataSet.Relations.InternalIndexOf(relation.RelationName);

            if (iDest >= 0) {
                // check the columns and Relation properties..
                destRelation = dataSet.Relations[iDest];

                if (relation.ParentKey.ColumnsReference.Length != destRelation.ParentKey.ColumnsReference.Length) {
                    dataSet.RaiseMergeFailed(null,
                        Res.GetString(Res.DataMerge_MissingDefinition, relation.RelationName),
                        missingSchemaAction
                    );
                }
                for (int i = 0; i < relation.ParentKey.ColumnsReference.Length; i++) {
                    DataColumn dest = destRelation.ParentKey.ColumnsReference[i];
                    DataColumn src = relation.ParentKey.ColumnsReference[i];

                    if (0 != string.Compare(dest.ColumnName, src.ColumnName, false, dest.Table.Locale)) {
                        dataSet.RaiseMergeFailed(null,
                            Res.GetString(Res.DataMerge_ReltionKeyColumnsMismatch, relation.RelationName),
                            missingSchemaAction
                        );
                    }

                    dest = destRelation.ChildKey.ColumnsReference[i];
                    src = relation.ChildKey.ColumnsReference[i];

                    if (0 != string.Compare(dest.ColumnName, src.ColumnName, false, dest.Table.Locale)) {
                        dataSet.RaiseMergeFailed(null,
                            Res.GetString(Res.DataMerge_ReltionKeyColumnsMismatch, relation.RelationName),
                            missingSchemaAction
                        );
                    }
                }

            }
            else {
                if (MissingSchemaAction.Add == missingSchemaAction) {

                    // create identical realtion in the current dataset


                    DataTable parent;
                    if (_IgnoreNSforTableLookup){
                        parent = dataSet.Tables[relation.ParentTable.TableName];
                    }
                    else {
                        parent = dataSet.Tables[relation.ParentTable.TableName, relation.ParentTable.Namespace];
                    }


                    DataTable child;
                    if (_IgnoreNSforTableLookup) {
                        child = dataSet.Tables[relation.ChildTable.TableName];
                    }
                    else {
                        child = dataSet.Tables[relation.ChildTable.TableName,relation.ChildTable.Namespace];
                    }
                    
                    DataColumn[] parentColumns = new DataColumn[relation.ParentKey.ColumnsReference.Length];
                    DataColumn[] childColumns = new DataColumn[relation.ParentKey.ColumnsReference.Length];
                    for (int i = 0; i < relation.ParentKey.ColumnsReference.Length; i++) {
                        parentColumns[i] = parent.Columns[relation.ParentKey.ColumnsReference[i].ColumnName];
                        childColumns[i] = child.Columns[relation.ChildKey.ColumnsReference[i].ColumnName];
                    }
                    try {
                        destRelation = new DataRelation(relation.RelationName, parentColumns, childColumns, relation.createConstraints);
                        destRelation.Nested = relation.Nested;
                        dataSet.Relations.Add(destRelation);
                    }
                    catch (Exception e) {
                        // 
                        if (!Common.ADP.IsCatchableExceptionType(e)) {
                            throw;
                        }
                        ExceptionBuilder.TraceExceptionForCapture(e);
                        dataSet.RaiseMergeFailed(null, e.Message, missingSchemaAction);
                    }
                }
                else {
                    Debug.Assert(MissingSchemaAction.Error == missingSchemaAction, "Unexpected value of MissingSchemaAction parameter : " + ((Enum) missingSchemaAction).ToString());
                    throw ExceptionBuilder.MergeMissingDefinition(relation.RelationName);
                }
            }

            MergeExtendedProperties(relation.ExtendedProperties, destRelation.ExtendedProperties);

            return;
        }

        private void MergeExtendedProperties(PropertyCollection src, PropertyCollection dst) {
            if (MissingSchemaAction.Ignore == missingSchemaAction) {
                return;
            }

            IDictionaryEnumerator srcDE = src.GetEnumerator();
            while (srcDE.MoveNext()) {
                if (!preserveChanges || dst[srcDE.Key] == null)
                    dst[srcDE.Key] = srcDE.Value;
            }
        }

        private DataKey GetSrcKey(DataTable src, DataTable dst) {
            if (src.primaryKey != null)
                return src.primaryKey.Key;

            DataKey key = default(DataKey);
            if (dst.primaryKey != null) {
                DataColumn[] dstColumns = dst.primaryKey.Key.ColumnsReference;
                DataColumn[] srcColumns = new DataColumn[dstColumns.Length];
                for (int j = 0; j < dstColumns.Length; j++) {
                    srcColumns[j] = src.Columns[dstColumns[j].ColumnName];
                }

                key = new DataKey(srcColumns, false); // DataKey will take ownership of srcColumns
            }

            return key;
        }
    }
}
