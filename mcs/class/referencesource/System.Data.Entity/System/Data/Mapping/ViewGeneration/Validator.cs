//---------------------------------------------------------------------
// <copyright file="Validator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Mapping.ViewGeneration.Validation;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration
{

    using System.Data.Entity;
    using BasicSchemaConstraints = SchemaConstraints<BasicKeyConstraint>;
    using ViewSchemaConstraints = SchemaConstraints<ViewKeyConstraint>;

    // This class is responsible for validating the incoming cells for a schema
    class CellGroupValidator
    {

        #region Constructor
        // requires: cells are not normalized, i.e., no slot is null in the cell queries
        // effects: Constructs a validator object that is capable of
        // validating all the schema cells together
        internal CellGroupValidator(IEnumerable<Cell> cells, ConfigViewGenerator config)
        {
            m_cells = cells;
            m_config = config;
            m_errorLog = new ErrorLog();
        }
        #endregion

        #region Fields
        private IEnumerable<Cell> m_cells;
        private ConfigViewGenerator m_config;
        private ErrorLog m_errorLog; // Keeps track of errors for this set of cells
        private ViewSchemaConstraints m_cViewConstraints;
        private ViewSchemaConstraints m_sViewConstraints;
        #endregion

        #region External Methods
        // effects: Performs the validation of the cells in this and returns
        // an error log of all the errors/warnings that were discovered
        internal ErrorLog Validate()
        {

            // Check for errors not checked by "C-implies-S principle"
            if (m_config.IsValidationEnabled)
            {
                if (PerformSingleCellChecks() == false)
                {
                    return m_errorLog;
                }
            }
            else //Note that Metadata loading guarantees that DISTINCT flag is not present
            {    // when update views (and validation) is disabled

                if (CheckCellsWithDistinctFlag() == false)
                {
                    return m_errorLog;
                }
            }

            BasicSchemaConstraints cConstraints = new BasicSchemaConstraints();
            BasicSchemaConstraints sConstraints = new BasicSchemaConstraints();

            // Construct intermediate "view relations" and the basic cell
            // relations along with the basic constraints
            ConstructCellRelationsWithConstraints(cConstraints, sConstraints);

            if (m_config.IsVerboseTracing)
            {
                // Trace Basic constraints
                Trace.WriteLine(String.Empty);
                Trace.WriteLine("C-Level Basic Constraints");
                Trace.WriteLine(cConstraints);
                Trace.WriteLine("S-Level Basic Constraints");
                Trace.WriteLine(sConstraints);
            }

            // Propagate the constraints
            m_cViewConstraints = PropagateConstraints(cConstraints);
            m_sViewConstraints = PropagateConstraints(sConstraints);

            // Make some basic checks on the view and basic cell constraints
            CheckConstraintSanity(cConstraints, sConstraints, m_cViewConstraints, m_sViewConstraints);

            if (m_config.IsVerboseTracing)
            {
                // Trace View constraints
                Trace.WriteLine(String.Empty);
                Trace.WriteLine("C-Level View Constraints");
                Trace.WriteLine(m_cViewConstraints);
                Trace.WriteLine("S-Level View Constraints");
                Trace.WriteLine(m_sViewConstraints);
            }

            // Check for implication
            if (m_config.IsValidationEnabled)
            {
                CheckImplication(m_cViewConstraints, m_sViewConstraints);
            }
            return m_errorLog;
        }
        #endregion

        #region Basic Constraint Creation

        // effects: Creates the base cell relation and view cell relations
        // for each cellquery/cell. Also generates the C-Side and S-side
        // basic constraints and stores them into cConstraints and
        // sConstraints. Stores them in cConstraints and sConstraints
        private void ConstructCellRelationsWithConstraints(BasicSchemaConstraints cConstraints,
                                                           BasicSchemaConstraints sConstraints)
        {

            // Populate single cell constraints
            int cellNumber = 0;
            foreach (Cell cell in m_cells)
            {
                // We have to create the ViewCellRelation so that the
                // BasicCellRelations can be created.
                cell.CreateViewCellRelation(cellNumber);
                BasicCellRelation cCellRelation = cell.CQuery.BasicCellRelation;
                BasicCellRelation sCellRelation = cell.SQuery.BasicCellRelation;
                // Populate the constraints for the C relation and the S Relation
                PopulateBaseConstraints(cCellRelation, cConstraints);
                PopulateBaseConstraints(sCellRelation, sConstraints);
                cellNumber++;
            }

            // Populate two-cell constraints, i.e., inclusion
            foreach (Cell firstCell in m_cells)
            {
                foreach (Cell secondCell in m_cells)
                {
                    if (Object.ReferenceEquals(firstCell, secondCell))
                    {
                        // We do not want to set up self-inclusion constraints unnecessarily
                        continue;
                    }
                }
            }
        }

        // effects: Generates the single-cell key+domain constraints for
        // baseRelation and adds them to constraints
        private static void PopulateBaseConstraints(BasicCellRelation baseRelation,
                                                    BasicSchemaConstraints constraints)
        {
            // Populate key constraints
            baseRelation.PopulateKeyConstraints(constraints);
        }
        #endregion

        #region Constraint Propagation
        // effects: Propagates baseConstraints derived from the cellrelations
        // to the corresponding viewCellRelations and returns the list of
        // propagated constraints
        private static ViewSchemaConstraints PropagateConstraints(BasicSchemaConstraints baseConstraints)
        {
            ViewSchemaConstraints propagatedConstraints = new ViewSchemaConstraints();

            // Key constraint propagation
            foreach (BasicKeyConstraint keyConstraint in baseConstraints.KeyConstraints)
            {
                ViewKeyConstraint viewConstraint = keyConstraint.Propagate();
                if (viewConstraint != null)
                {
                    propagatedConstraints.Add(viewConstraint);
                }
            }
            return propagatedConstraints;
        }
        #endregion

        #region Checking for Implication
        // effects: Checks if all sViewConstraints are implied by the
        // constraints in cViewConstraints. If some S-level constraints are
        // not implied, adds errors/warnings to m_errorLog
        private void CheckImplication(ViewSchemaConstraints cViewConstraints, ViewSchemaConstraints sViewConstraints)
        {

            // Check key constraints
            // i.e., if S has a key <k1, k2>, C must have a key that is a subset of this
            CheckImplicationKeyConstraints(cViewConstraints, sViewConstraints);
            
            // For updates, we need to ensure the following: for every
            // extent E, table T pair, some key of E is implied by T's key

            // Get all key constraints for each extent and each table
            KeyToListMap<ExtentPair, ViewKeyConstraint> extentPairConstraints =
                new KeyToListMap<ExtentPair, ViewKeyConstraint>(EqualityComparer<ExtentPair>.Default);

            foreach (ViewKeyConstraint cKeyConstraint in cViewConstraints.KeyConstraints)
            {
                ExtentPair pair = new ExtentPair(cKeyConstraint.Cell.CQuery.Extent, cKeyConstraint.Cell.SQuery.Extent);
                extentPairConstraints.Add(pair, cKeyConstraint);
            }

            // Now check that we guarantee at least one constraint per
            // extent/table pair
            foreach (ExtentPair extentPair in extentPairConstraints.Keys)
            {
                ReadOnlyCollection<ViewKeyConstraint> cKeyConstraints = extentPairConstraints.ListForKey(extentPair);
                bool sImpliesSomeC = false;
                // Go through all key constraints for the extent/table pair, and find one that S implies
                foreach (ViewKeyConstraint cKeyConstraint in cKeyConstraints)
                {
                    foreach (ViewKeyConstraint sKeyConstraint in sViewConstraints.KeyConstraints)
                    {
                        if (sKeyConstraint.Implies(cKeyConstraint))
                        {
                            sImpliesSomeC = true;
                            break; // The implication holds - so no problem
                        }
                    }
                }
                if (sImpliesSomeC == false)
                {
                    // Indicate that at least one key must be ensured on the S-side
                    m_errorLog.AddEntry(ViewKeyConstraint.GetErrorRecord(cKeyConstraints));
                }
            }
        }

        // effects: Checks for key constraint implication problems from
        // leftViewConstraints to rightViewConstraints. Adds errors/warning to m_errorLog 
        private void CheckImplicationKeyConstraints(ViewSchemaConstraints leftViewConstraints,
                                                    ViewSchemaConstraints rightViewConstraints)
        {

            // if cImpliesS is true, every rightKeyConstraint must be implied
            // if it is false, at least one key constraint for each C-level
            // extent must be implied

            foreach (ViewKeyConstraint rightKeyConstraint in rightViewConstraints.KeyConstraints)
            {
                // Go through all the left Side constraints and check for implication
                bool found = false;
                foreach (ViewKeyConstraint leftKeyConstraint in leftViewConstraints.KeyConstraints)
                {
                    if (leftKeyConstraint.Implies(rightKeyConstraint))
                    {
                        found = true;
                        break; // The implication holds - so no problem
                    }
                }
                if (false == found)
                {
                    // No C-side key constraint implies this S-level key constraint
                    // Report a problem
                    m_errorLog.AddEntry(ViewKeyConstraint.GetErrorRecord(rightKeyConstraint));
                }
            }
        }

        #endregion

        #region Miscellaneous checks


        /// <summary>
        /// Checks that if a DISTINCT operator exists between some C-Extent and S-Extent, there are no additional
        /// mapping fragments between that C-Extent and S-Extent.
        /// We need to enforce this because DISTINCT is not understood by viewgen machinery, and two fragments may be merged
        /// despite one of them having DISTINCT.
        /// </summary>
        private bool CheckCellsWithDistinctFlag()
        {

            int errorLogSize = m_errorLog.Count;
            foreach (Cell cell in m_cells)
            {
                if (cell.SQuery.SelectDistinctFlag == CellQuery.SelectDistinct.Yes)
                {
                    var cExtent = cell.CQuery.Extent;
                    var sExtent = cell.SQuery.Extent;

                    //There should be no other fragments mapping cExtent to sExtent
                    var mapepdFragments = m_cells.Where(otherCell => otherCell != cell)
                                        .Where(otherCell => otherCell.CQuery.Extent == cExtent && otherCell.SQuery.Extent == sExtent);

                    if (mapepdFragments.Any())
                    {
                        var cellsToReport = Enumerable.Union(Enumerable.Repeat(cell, 1), mapepdFragments);
                        ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.MultipleFragmentsBetweenCandSExtentWithDistinct,
                            Strings.Viewgen_MultipleFragmentsBetweenCandSExtentWithDistinct(cExtent.Name, sExtent.Name), cellsToReport, String.Empty);
                        m_errorLog.AddEntry(record);
                    }
                }
            }

            return m_errorLog.Count == errorLogSize;
        }
        
        
        
        
        // effects: Check for problems in each cell that are not detected by the
        // "C-constraints-imply-S-constraints" principle. If the check fails,
        // adds relevant error info to m_errorLog and returns false. Else
        // retrns true
        private bool PerformSingleCellChecks()
        {

            int errorLogSize = m_errorLog.Count;
            foreach (Cell cell in m_cells)
            {
                // Check for duplication of element in a single cell name1, name2
                // -> name Could be done by implication but that would require
                // setting self-inclusion constraints etc That seems unnecessary

                // We need this check only for the C side. if we map cname1
                // and cmane2 to sname, that is a problem. But mapping sname1
                // and sname2 to cname is ok
                ErrorLog.Record error = cell.SQuery.CheckForDuplicateFields(cell.CQuery, cell);
                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }

                // Check that the EntityKey and the Table key are mapped
                // (Key for association is all ends)
                error = cell.CQuery.VerifyKeysPresent(cell, Strings.ViewGen_EntitySetKey_Missing,
                    Strings.ViewGen_AssociationSetKey_Missing, ViewGenErrorCode.KeyNotMappedForCSideExtent);

                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }

                error = cell.SQuery.VerifyKeysPresent(cell, Strings.ViewGen_TableKey_Missing, null, ViewGenErrorCode.KeyNotMappedForTable);
                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }

                // Check that if any side has a not-null constraint -- if so,
                // we must project that slot
                error = cell.CQuery.CheckForProjectedNotNullSlots(cell, m_cells.Where(c=> c.SQuery.Extent is AssociationSet));
                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }
                error = cell.SQuery.CheckForProjectedNotNullSlots(cell, m_cells.Where(c => c.CQuery.Extent is AssociationSet));
                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }
            }
            return m_errorLog.Count == errorLogSize;
        }

        // effects: Checks for some sanity issues between the basic and view constraints. Adds to m_errorLog if needed
        [Conditional("DEBUG")]
        private static void CheckConstraintSanity(BasicSchemaConstraints cConstraints, BasicSchemaConstraints sConstraints,
                                           ViewSchemaConstraints cViewConstraints, ViewSchemaConstraints sViewConstraints)
        {
            Debug.Assert(cConstraints.KeyConstraints.Count() == cViewConstraints.KeyConstraints.Count(),
                         "Mismatch in number of C basic and view key constraints");
            Debug.Assert(sConstraints.KeyConstraints.Count() == sViewConstraints.KeyConstraints.Count(),
                         "Mismatch in number of S basic and view key constraints");
        }
        #endregion

        // Keeps track of two extent objects
        private class ExtentPair
        {
            internal ExtentPair(EntitySetBase acExtent, EntitySetBase asExtent)
            {
                cExtent = acExtent;
                sExtent = asExtent;
            }
            internal EntitySetBase cExtent;
            internal EntitySetBase sExtent;

            public override bool Equals(object obj)
            {
                if (object.ReferenceEquals(this, obj))
                {
                    return true;
                }
                ExtentPair pair = obj as ExtentPair;
                if (pair == null)
                {
                    return false;
                }

                return pair.cExtent.Equals(cExtent) && pair.sExtent.Equals(sExtent);
            }

            public override int GetHashCode()
            {
                return cExtent.GetHashCode() ^ sExtent.GetHashCode();
            }
        }


    }

}
