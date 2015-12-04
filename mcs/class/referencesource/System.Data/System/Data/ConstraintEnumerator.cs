//------------------------------------------------------------------------------
// <copyright file="ConstraintEnumerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;

    /// <devdoc>
    /// ConstraintEnumerator is an object for enumerating all constraints in a DataSet
    /// </devdoc>
    internal class ConstraintEnumerator {

        System.Collections.IEnumerator tables;
        System.Collections.IEnumerator constraints;
        Constraint currentObject; 

        public ConstraintEnumerator(DataSet dataSet) {
            tables = (dataSet != null) ? dataSet.Tables.GetEnumerator() : null;
            currentObject = null;
        }

        public bool GetNext() {
            Constraint candidate;
            currentObject = null;
            while (tables != null) {
                if (constraints == null) {
                    if (!tables.MoveNext()) {
                        tables = null;
                        return false;
                    }
                    constraints = ((DataTable)tables.Current).Constraints.GetEnumerator();
                }

                if (!constraints.MoveNext()) {
                    constraints = null;
                    continue;
                }

                Debug.Assert(constraints.Current is Constraint, "ConstraintEnumerator, contains object which is not constraint");
                candidate = (Constraint)constraints.Current;
                if (IsValidCandidate(candidate)) {
                    currentObject = candidate;
                    return true;
                }

            }
            return false;
        }

        public Constraint GetConstraint() {
            // If currentObject is null we are before first GetNext or after last GetNext--consumer is bad
            Debug.Assert (currentObject != null, "GetObject should never be called w/ null currentObject.");
            return currentObject;   
        }

        protected virtual bool IsValidCandidate(Constraint constraint) {
            return true;
        }

        protected Constraint CurrentObject {
            get {
                return currentObject;
            }
        }

    }

    internal class ForeignKeyConstraintEnumerator : ConstraintEnumerator {

        public ForeignKeyConstraintEnumerator(DataSet dataSet) : base(dataSet) {

        }

        protected override bool IsValidCandidate(Constraint constraint) {
            return(constraint is ForeignKeyConstraint);
        }

        public ForeignKeyConstraint GetForeignKeyConstraint() {
            // If CurrentObject is null we are before first GetNext or after last GetNext--consumer is bad
            Debug.Assert (CurrentObject != null, "GetObject should never be called w/ null currentObject.");
            return(ForeignKeyConstraint)CurrentObject;   
        }
    }

    internal sealed class ChildForeignKeyConstraintEnumerator : ForeignKeyConstraintEnumerator {

        // this is the table to do comparisons against
        DataTable table;
        public ChildForeignKeyConstraintEnumerator(DataSet dataSet, DataTable inTable) : base(dataSet) {
            this.table = inTable;
        }

        protected override bool IsValidCandidate(Constraint constraint) {
            return((constraint is ForeignKeyConstraint) && (((ForeignKeyConstraint)constraint).Table == table));
        }
    }

    internal sealed class ParentForeignKeyConstraintEnumerator : ForeignKeyConstraintEnumerator {

        // this is the table to do comparisons against
        DataTable table;
        public ParentForeignKeyConstraintEnumerator(DataSet dataSet, DataTable inTable) : base(dataSet) {
            this.table = inTable;
        }

        protected override bool IsValidCandidate(Constraint constraint) {
            return((constraint is ForeignKeyConstraint) && (((ForeignKeyConstraint)constraint).RelatedTable == table));
        }
    }
}
