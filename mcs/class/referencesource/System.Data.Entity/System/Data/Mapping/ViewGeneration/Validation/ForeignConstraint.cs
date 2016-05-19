//---------------------------------------------------------------------
// <copyright file="ForeignConstraint.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Mapping.ViewGeneration.Validation
{

    // An abstraction that captures a foreign key constraint:
    // <child, columns> --> <parent, columns>
    internal class ForeignConstraint : InternalBase
    {

        #region Constructor
        // effects: Creates a foreign key constraint of the form:
        // <i_childTable, i_childColumns> --> <i_parentTable, i_childColumns>
        // i_fkeySet is the name of the constraint
        internal ForeignConstraint(AssociationSet i_fkeySet, EntitySet i_parentTable, EntitySet i_childTable,
                                   ReadOnlyMetadataCollection<EdmProperty> i_parentColumns, ReadOnlyMetadataCollection<EdmProperty> i_childColumns)
        {
            m_fKeySet = i_fkeySet;
            m_parentTable = i_parentTable;
            m_childTable = i_childTable;
            m_childColumns = new List<MemberPath>();
            // Create parent and child paths using the table names
            foreach (EdmProperty property in i_childColumns)
            {
                MemberPath path = new MemberPath(m_childTable, property);
                m_childColumns.Add(path);
            }

            m_parentColumns = new List<MemberPath>();
            foreach (EdmProperty property in i_parentColumns)
            {
                MemberPath path = new MemberPath(m_parentTable, property);
                m_parentColumns.Add(path);
            }
        }
        #endregion

        #region Fields
        private AssociationSet m_fKeySet; // Just for debugging
        private EntitySet m_parentTable;
        private EntitySet m_childTable;
        private List<MemberPath> m_parentColumns;
        private List<MemberPath> m_childColumns;
        #endregion

        #region Properties
        internal EntitySet ParentTable
        {
            get { return m_parentTable; }
        }

        internal EntitySet ChildTable
        {
            get { return m_childTable; }
        }

        internal IEnumerable<MemberPath> ChildColumns
        {
            get { return m_childColumns; }
        }

        internal IEnumerable<MemberPath> ParentColumns
        {
            get { return m_parentColumns; }
        }
        #endregion

        #region Externally available Methods
        // effects: Given a store-side container, returns all the foreign key
        // constraints specified for different tables
        internal static List<ForeignConstraint> GetForeignConstraints(EntityContainer container)
        {
            List<ForeignConstraint> foreignKeyConstraints = new List<ForeignConstraint>();

            // Go through all the extents and get the associations
            foreach (EntitySetBase extent in container.BaseEntitySets)
            {
                AssociationSet relationSet = extent as AssociationSet;

                if (relationSet == null) continue;
                // Keep track of the end to EntitySet mapping
                Dictionary<string, EntitySet> endToExtents = new Dictionary<string, EntitySet>();

                foreach (AssociationSetEnd end in relationSet.AssociationSetEnds)
                {
                    endToExtents.Add(end.Name, end.EntitySet);
                }

                AssociationType relationType = relationSet.ElementType;
                // Go through each referential constraint, determine the name
                // of the tables that the constraint refers to and then
                // create the foreign key constraint between the tables
                // Wow! We go to great lengths to make it cumbersome for a
                // programmer to deal with foreign keys
                foreach (ReferentialConstraint constraint in relationType.ReferentialConstraints)
                {
                    // Note: We are correlating the constraint's roles with
                    // the ends above using the role names, i.e.,
                    // FromRole.Name and ToRole.Name here and end.Role above
                    EntitySet parentExtent = endToExtents[constraint.FromRole.Name];
                    EntitySet childExtent = endToExtents[constraint.ToRole.Name];
                    ForeignConstraint foreignKeyConstraint = new ForeignConstraint(relationSet, parentExtent, childExtent,
                                                                       constraint.FromProperties, constraint.ToProperties);
                    foreignKeyConstraints.Add(foreignKeyConstraint);

                }
            }
            return foreignKeyConstraints;
        }

        // effects: Checks that this foreign key constraints for all the
        // tables are being ensured on the C-side as well. If not, adds 
        // errors to the errorLog
        internal void CheckConstraint(Set<Cell> cells, QueryRewriter childRewriter, QueryRewriter parentRewriter,
                                      ErrorLog errorLog, ConfigViewGenerator config)
        {
            if (IsConstraintRelevantForCells(cells) == false)
            {
                // if the constraint does not deal with any cell in this group, ignore it
                return;
            }

            if (config.IsNormalTracing)
            {
                Trace.WriteLine(String.Empty);
                Trace.WriteLine(String.Empty);
                Trace.Write("Checking: ");
                Trace.WriteLine(this);
            }

            if (childRewriter == null && parentRewriter == null)
            {
                // Neither table is mapped - so we are fine
                return;
            }

            // If the child table has not been mapped, we used to say that we
            // are fine. However, if we have SPerson(pid) and SAddress(aid,
            // pid), where pid is an FK into SPerson, we are in trouble if
            // SAddress is not mapped - SPerson could get deleted. So we
            // check for it as well
            // if the parent table is not mapped, we also have a problem

            if (childRewriter == null)
            {
                string message = System.Data.Entity.Strings.ViewGen_Foreign_Key_Missing_Table_Mapping(
                                               ToUserString(), ChildTable.Name);
                // Get the cells from the parent table
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ForeignKeyMissingTableMapping, message, parentRewriter.UsedCells, String.Empty);
                errorLog.AddEntry(record);
                return;
            }

            if (parentRewriter == null)
            {
                string message = System.Data.Entity.Strings.ViewGen_Foreign_Key_Missing_Table_Mapping(
                                               ToUserString(), ParentTable.Name);
                // Get the cells from the child table
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ForeignKeyMissingTableMapping, message, childRewriter.UsedCells, String.Empty);
                errorLog.AddEntry(record);
                return;
            }

            // Note: we do not check if the parent columns correspond to the
            // table's keys - metadata checks for that

            //First check if the FK is covered by Foreign Key Association
            //If we find this, we don't need to check for independent associations. If user maps the Fk to both FK and independent associations,
            //the regular round tripping validation will catch the error.
            if (CheckIfConstraintMappedToForeignKeyAssociation(childRewriter, parentRewriter, cells, errorLog))
            {
                return;
            }

            // Check if the foreign key in the child table corresponds to the primary key, i.e., if
            // the foreign key (e.g., pid, pid2) is a superset of the actual key members (e.g., pid), it means
            // that the foreign key is also the primary key for this table -- so we can propagate the queries upto C-Space
            // rather than doing the cell check

            int initialErrorLogSize = errorLog.Count;
            if (IsForeignKeySuperSetOfPrimaryKeyInChildTable())
            {
                GuaranteeForeignKeyConstraintInCSpace(childRewriter, parentRewriter, errorLog, config);
            }
            else
            {
                GuaranteeMappedRelationshipForForeignKey(childRewriter, parentRewriter, cells, errorLog, config);
            }

            if (initialErrorLogSize == errorLog.Count)
            {
                // Check if the order of columns in foreign key correponds to the
                // mappings in m_cellGroup, e.g., if <pid1, pid2> in SAddress is
                // a foreign key into <pid1, pid2> of the SPerson table, make
                // sure that this order is preserved through the mappings in m_cellGroup
                CheckForeignKeyColumnOrder(cells, errorLog);
            }
        }
        #endregion

        #region Methods (mostly) for Query Containment Check via Keys
        // requires: constraint.ChildColumns form a key in
        // constraint.ChildTable (actually they should subsume the primary key)
        private void GuaranteeForeignKeyConstraintInCSpace(QueryRewriter childRewriter, QueryRewriter parentRewriter,
                                                           ErrorLog errorLog, ConfigViewGenerator config)
        {
            ViewgenContext childContext = childRewriter.ViewgenContext;
            ViewgenContext parentContext = parentRewriter.ViewgenContext;
            CellTreeNode cNode = childRewriter.BasicView;
            CellTreeNode pNode = parentRewriter.BasicView;

            FragmentQueryProcessor qp = FragmentQueryProcessor.Merge(childContext.RightFragmentQP, parentContext.RightFragmentQP);
            bool cImpliesP = qp.IsContainedIn(cNode.RightFragmentQuery, pNode.RightFragmentQuery);

            if (false == cImpliesP)
            {
                // Foreign key constraint not being ensured in C-space
                string childExtents = LeftCellWrapper.GetExtentListAsUserString(cNode.GetLeaves());
                string parentExtents = LeftCellWrapper.GetExtentListAsUserString(pNode.GetLeaves());
                string message = System.Data.Entity.Strings.ViewGen_Foreign_Key_Not_Guaranteed_InCSpace(
                                               ToUserString());
                // Add all wrappers into allWrappers
                Set<LeftCellWrapper> allWrappers = new Set<LeftCellWrapper>(pNode.GetLeaves());
                allWrappers.AddRange(cNode.GetLeaves());
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ForeignKeyNotGuaranteedInCSpace, message, allWrappers, String.Empty);
                errorLog.AddEntry(record);
            }
        }

        #endregion

        #region Methods for Foreign Keys mapped to association

        // effects: Ensures that there is a relationship mapped into the C-space for some cell in m_cellGroup. Else
        // adds an error to errorLog
        private void GuaranteeMappedRelationshipForForeignKey(QueryRewriter childRewriter, QueryRewriter parentRewriter,
                                                              IEnumerable<Cell> cells,
                                                              ErrorLog errorLog, ConfigViewGenerator config)
        {

            ViewgenContext childContext = childRewriter.ViewgenContext;
            ViewgenContext parentContext = parentRewriter.ViewgenContext;

            // Find a cell where this foreign key is mapped as a relationship
            MemberPath prefix = new MemberPath(ChildTable);
            ExtentKey primaryKey = ExtentKey.GetPrimaryKeyForEntityType(prefix, ChildTable.ElementType);
            IEnumerable<MemberPath> primaryKeyFields = primaryKey.KeyFields;
            bool foundCell = false;

            bool foundValidParentColumnsForForeignKey = false; //we need to find only one, dont error on any one check being false
            List<ErrorLog.Record> errorListForInvalidParentColumnsForForeignKey = null;
            foreach (Cell cell in cells)
            {
                if (cell.SQuery.Extent.Equals(ChildTable) == false)
                {
                    continue;
                }

                // The childtable is mapped to a relationship in the C-space in cell
                // Check that all the columns of the foreign key and the primary key in the child table are mapped to some
                // property in the C-space

                AssociationEndMember parentEnd = GetRelationEndForColumns(cell, ChildColumns);
                if (parentEnd != null && CheckParentColumnsForForeignKey(cell, cells, parentEnd, ref errorListForInvalidParentColumnsForForeignKey) == false)
                {
                    // Not an error unless we find no valid case
                    continue;
                }
                else
                {
                    foundValidParentColumnsForForeignKey = true;
                }

                AssociationEndMember childEnd = GetRelationEndForColumns(cell, primaryKeyFields);
                Debug.Assert(childEnd == null || parentEnd != childEnd,
                             "Ends are same => PKey and child columns are same - code should gone to other method");
                // Note: If both of them are not-null, they are mapped to the
                // same association set -- since we checked that particular cell

                if (childEnd != null && parentEnd != null &&
                    FindEntitySetForColumnsMappedToEntityKeys(cells, primaryKeyFields) != null)
                {
                    foundCell = true;
                    CheckConstraintWhenParentChildMapped(cell, errorLog, parentEnd, config);
                    break; // Done processing for the foreign key - either it was mapped correctly or it was not
                }
                else if (parentEnd != null)
                {
                    // At this point, we know cell corresponds to an association set
                    AssociationSet assocSet = (AssociationSet)cell.CQuery.Extent;
                    EntitySet parentSet = MetadataHelper.GetEntitySetAtEnd(assocSet, parentEnd);
                    foundCell = CheckConstraintWhenOnlyParentMapped(cell, parentSet, assocSet, parentEnd, childRewriter, parentRewriter, config);
                    if (foundCell)
                    {
                        break;
                    }
                }
            }

            //CheckParentColumnsForForeignKey has returned no matches, Error.
            if (!foundValidParentColumnsForForeignKey)
            {
                Debug.Assert(errorListForInvalidParentColumnsForForeignKey != null && errorListForInvalidParentColumnsForForeignKey.Count > 0);
                foreach (var errorRecord in errorListForInvalidParentColumnsForForeignKey)
                {
                    errorLog.AddEntry(errorRecord);
                }
                return;
            }

            if (foundCell == false)
            {
                // No cell found -- Declare error
                string message = System.Data.Entity.Strings.ViewGen_Foreign_Key_Missing_Relationship_Mapping(ToUserString());

                IEnumerable<LeftCellWrapper> parentWrappers = GetWrappersFromContext(parentContext, ParentTable);
                IEnumerable<LeftCellWrapper> childWrappers = GetWrappersFromContext(childContext, ChildTable);
                Set<LeftCellWrapper> bothExtentWrappers =
                    new Set<LeftCellWrapper>(parentWrappers);
                bothExtentWrappers.AddRange(childWrappers);
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ForeignKeyMissingRelationshipMapping, message, bothExtentWrappers, String.Empty);
                errorLog.AddEntry(record);
            }
        }

        private bool CheckIfConstraintMappedToForeignKeyAssociation(QueryRewriter childRewriter, QueryRewriter parentRewriter,
                                                              Set<Cell> cells, ErrorLog errorLog)
        {
            ViewgenContext childContext = childRewriter.ViewgenContext;
            ViewgenContext parentContext = parentRewriter.ViewgenContext;

            //First collect the sets of properties that the principal and dependant ends of this FK
            //are mapped to in the Edm side.
            var childPropertiesSet = new List<Set<EdmProperty>>();
            var parentPropertiesSet = new List<Set<EdmProperty>>();
            foreach (Cell cell in cells)
            {
                if (cell.CQuery.Extent.BuiltInTypeKind != BuiltInTypeKind.AssociationSet)
                {
                    var childProperties = cell.GetCSlotsForTableColumns(ChildColumns);
                    if ( (childProperties != null) && (childProperties.Count != 0))
                    {
                        childPropertiesSet.Add(childProperties);
                    }
                    var parentProperties = cell.GetCSlotsForTableColumns(ParentColumns);
                    if ((parentProperties != null) && (parentProperties.Count != 0))
                    {
                        parentPropertiesSet.Add(parentProperties);
                    }

                }
            }

            //Now Check if the properties on the Edm side are connected via an FK relationship.
            if ((childPropertiesSet.Count != 0) && (parentPropertiesSet.Count != 0))
            {
                var foreignKeyAssociations = childContext.EntityContainerMapping.EdmEntityContainer.BaseEntitySets.OfType<AssociationSet>().Where(it => it.ElementType.IsForeignKey).Select(it => it.ElementType);                
                foreach (AssociationType association in foreignKeyAssociations)
                {
                    ReferentialConstraint refConstraint = association.ReferentialConstraints.FirstOrDefault();
                    //We need to check to see if the dependent properties that were mapped from S side are present as
                    //dependant properties of this ref constraint on the Edm side. We need to do the same for principal side but
                    //we can not enforce equality since the order of the properties participating in the constraint on the S side and
                    //C side could be different. This is OK as long as they are mapped appropriately. We also can not use Existance as a sufficient
                    //condition since it will allow invalid mapping where FK columns could have been flipped when mapping to the Edm side. So
                    //we make sure that the index of the properties in the principal and dependant are same on the Edm side even if they are in
                    //different order for ref constraints for Edm and store side.
                    var childRefPropertiesCollection = childPropertiesSet.Where(it => it.SetEquals(new Set<EdmProperty>(refConstraint.ToProperties)));
                    var parentRefPropertiesCollection = parentPropertiesSet.Where(it => it.SetEquals(new Set<EdmProperty>(refConstraint.FromProperties)));
                    if ((childRefPropertiesCollection.Count() != 0 && parentRefPropertiesCollection.Count() != 0))
                    {
                        foreach (var parentRefProperties in parentRefPropertiesCollection)                        
                        {
                            var parentIndexes = GetPropertyIndexes(parentRefProperties, refConstraint.FromProperties);
                            foreach (var childRefProperties in childRefPropertiesCollection)
                            {
                                var childIndexes = GetPropertyIndexes(childRefProperties, refConstraint.ToProperties);
                                
                                if (childIndexes.SequenceEqual(parentIndexes))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        //Return a set of integers that represent the indexes of first set of properties in the second set
        private Set<int> GetPropertyIndexes(IEnumerable<EdmProperty> properties1, ReadOnlyMetadataCollection<EdmProperty> properties2)
        {
            var propertyIndexes = new Set<int>();
            foreach (var prop in properties1)
            {
                propertyIndexes.Add(properties2.IndexOf(prop));
            }
            return propertyIndexes;
        }

        // requires: IsForeignKeySuperSetOfPrimaryKeyInChildTable() is false
        // and primaryKeys of ChildTable are not mapped in cell. cell
        // corresponds to an association set. parentSet is the set
        // corresponding to the end that we are looking at
        // effects: Checks if the constraint is correctly maintained in
        // C-space via an association set (being a subset of the
        // corresponding entitySet)
        private bool CheckConstraintWhenOnlyParentMapped(Cell cell, EntitySet parentSet, AssociationSet assocSet, AssociationEndMember endMember,
                                                         QueryRewriter childRewriter, QueryRewriter parentRewriter,
                                                         ConfigViewGenerator config)
        {

            ViewgenContext childContext = childRewriter.ViewgenContext;
            ViewgenContext parentContext = parentRewriter.ViewgenContext;

            CellTreeNode pNode = parentRewriter.BasicView;
            Debug.Assert(pNode != null);

            RoleBoolean endRoleBoolean = new RoleBoolean(assocSet.AssociationSetEnds[endMember.Name]);
            // use query in pNode as a factory to create a bool expression for the endRoleBoolean
            BoolExpression endCondition = pNode.RightFragmentQuery.Condition.Create(endRoleBoolean);
            FragmentQuery cNodeQuery = FragmentQuery.Create(pNode.RightFragmentQuery.Attributes, endCondition);

            FragmentQueryProcessor qp = FragmentQueryProcessor.Merge(childContext.RightFragmentQP, parentContext.RightFragmentQP);
            bool cImpliesP = qp.IsContainedIn(cNodeQuery, pNode.RightFragmentQuery);
            return cImpliesP;
        }

        // requires: IsForeignKeySuperSetOfPrimaryKeyInChildTable() is false
        // effects: Given that both the ChildColumns in this and the
        // primaryKey of ChildTable are mapped. Return true iff no error occurred
        private bool CheckConstraintWhenParentChildMapped(Cell cell, ErrorLog errorLog,
                                                          AssociationEndMember parentEnd, ConfigViewGenerator config)
        {
            bool ok = true;

            // The foreign key constraint has been mapped to a
            // relationship. Check if the multiplicities are consistent
            // If all columns in the child table (corresponding to
            // the constraint) are nullable, the parent end can be
            // 0..1 or 1..1. Else if must be 1..1
            if (parentEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                // Parent should at most one since we are talking
                // about foreign keys here
                string message = System.Data.Entity.Strings.ViewGen_Foreign_Key_UpperBound_MustBeOne(ToUserString(),
                                               cell.CQuery.Extent.Name, parentEnd.Name);
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ForeignKeyUpperBoundMustBeOne, message, cell, String.Empty);
                errorLog.AddEntry(record);
                ok = false;
            }

            if (MemberPath.AreAllMembersNullable(ChildColumns) == false && parentEnd.RelationshipMultiplicity != RelationshipMultiplicity.One)
            {
                // Some column in the constraint in the child table
                // is non-nullable and lower bound is not 1
                string message = System.Data.Entity.Strings.ViewGen_Foreign_Key_LowerBound_MustBeOne(ToUserString(),
                                               cell.CQuery.Extent.Name, parentEnd.Name);
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ForeignKeyLowerBoundMustBeOne, message, cell, String.Empty);
                errorLog.AddEntry(record);
                ok = false;
            }

            if (config.IsNormalTracing && ok)
            {
                Trace.WriteLine("Foreign key mapped to relationship " + cell.CQuery.Extent.Name);
            }
            return ok;
        }

        // effects: Given the foreign key constraint, checks if the
        // constraint.ParentColumns are mapped to the entity set E'e keys in
        // C-space where E corresponds to the entity set corresponding to end
        // Returns true iff such a mapping exists in cell
        private bool CheckParentColumnsForForeignKey(Cell cell, IEnumerable<Cell> cells, AssociationEndMember parentEnd, ref List<ErrorLog.Record> errorList)
        {
            // The child columns are mapped to some end of cell.CQuery.Extent. ParentColumns
            // must correspond to the EntitySet for this end
            AssociationSet relationSet = (AssociationSet)cell.CQuery.Extent;
            EntitySet endSet = MetadataHelper.GetEntitySetAtEnd(relationSet, parentEnd);

            // Check if the ParentColumns are mapped to endSet's keys

            // Find the entity set that they map to - if any
            EntitySet entitySet = FindEntitySetForColumnsMappedToEntityKeys(cells, ParentColumns);
            if (entitySet == null || endSet.Equals(entitySet) == false)
            {

                if (errorList == null) //lazily initialize only if there is an error
                {
                    errorList = new List<ErrorLog.Record>();
                }

                // childColumns are mapped to parentEnd but ParentColumns are not mapped to the end
                // corresponding to the parentEnd -- this is an error
                string message = System.Data.Entity.Strings.ViewGen_Foreign_Key_ParentTable_NotMappedToEnd(
                                               ToUserString(), ChildTable.Name,
                                               cell.CQuery.Extent.Name, parentEnd.Name, ParentTable.Name, endSet.Name);
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ForeignKeyParentTableNotMappedToEnd, message, cell, String.Empty);
                errorList.Add(record);
                return false;
            }
            return true;
        }
        #endregion

        #region Static Helper Methods
        // effects: Returns the entity set to which tableFields are mapped
        // and if the mapped fields correspond precisely to the entity set's
        // keys. Else returns null
        private static EntitySet FindEntitySetForColumnsMappedToEntityKeys(IEnumerable<Cell> cells, IEnumerable<MemberPath> tableColumns)
        {
            foreach (Cell cell in cells)
            {
                CellQuery cQuery = cell.CQuery;
                if (cQuery.Extent is AssociationSet)
                {
                    continue;
                }

                Set<EdmProperty> cSideMembers = cell.GetCSlotsForTableColumns(tableColumns);
                if (cSideMembers == null)
                {
                    continue;
                }
                // Now check if these fields correspond to the key fields of
                // the entity set

                EntitySet entitySet = cQuery.Extent as EntitySet;

                // Construct a List<EdmMember>
                List<EdmProperty> propertyList = new List<EdmProperty>();
                foreach (EdmProperty property in entitySet.ElementType.KeyMembers)
                {
                    propertyList.Add(property);
                }

                Set<EdmProperty> keyMembers = new Set<EdmProperty>(propertyList).MakeReadOnly();
                if (keyMembers.SetEquals(cSideMembers))
                {
                    return entitySet;
                }
            }
            return null;
        }

        // effects: Returns the end to which columns are exactly mapped in the
        // relationship set given by cell.CQuery.Extent -- if this extent is
        // an entityset returns null. If the columns are not mapped in this cell to an
        // end exactly or columns are not projected in cell, returns null
        private static AssociationEndMember GetRelationEndForColumns(Cell cell, IEnumerable<MemberPath> columns)
        {
            if (cell.CQuery.Extent is EntitySet)
            {
                return null;
            }

            AssociationSet relationSet = (AssociationSet)cell.CQuery.Extent;

            // Go through all the ends and see if they are mapped in this cell
            foreach (AssociationSetEnd relationEnd in relationSet.AssociationSetEnds)
            {

                AssociationEndMember endMember = relationEnd.CorrespondingAssociationEndMember;
                MemberPath prefix = new MemberPath(relationSet, endMember);
                // Note: primaryKey is the key for the entity set but
                // prefixed with the relationship's path - we are trying to
                // check if the entity's keys are mapped in this cell as an end
                ExtentKey primaryKey = ExtentKey.GetPrimaryKeyForEntityType(prefix, relationEnd.EntitySet.ElementType);

                // Check if this end is mapped in this cell -- we are
                // checking on the C-side -- we get all the indexes of the
                // end's keyfields
                List<int> endIndexes = cell.CQuery.GetProjectedPositions(primaryKey.KeyFields);
                if (endIndexes != null)
                {
                    // Get all the slots corresponding to the columns
                    // But stick to the slots with in these ends since the same column might be 
                    //projected twice in different ends
                    List<int> columnIndexes = cell.SQuery.GetProjectedPositions(columns, endIndexes);
                    if (columnIndexes == null)
                    {
                        continue; // columns are not projected with in this end
                    }
                    // Note that the positions need not match exactly - we have a
                    // separate test that will do that for us: CheckForeignKeyColumnOrder
                    if (Helpers.IsSetEqual(columnIndexes, endIndexes, EqualityComparer<int>.Default))
                    {
                        // The columns map exactly to this end -- return it
                        return endMember;
                    }
                }
            }
            return null;
        }

        // effects: Returns wrappers for extent if there are some available in the context. Else returns an empty enumeration
        private static List<LeftCellWrapper> GetWrappersFromContext(ViewgenContext context, EntitySetBase extent)
        {
            List<LeftCellWrapper> wrappers;
            if (context == null)
            {
                wrappers = new List<LeftCellWrapper>();
            }
            else
            {
                Debug.Assert(context.Extent.Equals(extent), "ViewgenContext extent and expected extent different");
                wrappers = context.AllWrappersForExtent;
            }
            return wrappers;
        }
        #endregion

        #region Regular Helper Methods
        // requires: all columns in constraint.ParentColumns and
        // constraint.ChildColumns must have been mapped in some cell in m_cellGroup
        // effects: Given the foreign key constraint, checks if the
        // constraint.ChildColumns are mapped to the constraint.ParentColumns
        // in m_cellGroup in the right oder. If not, adds an error to m_errorLog and returns
        // false. Else returns true
        private bool CheckForeignKeyColumnOrder(Set<Cell> cells, ErrorLog errorLog)
        {
            // Go through every cell and find the cells that are relevant to
            // parent and those that are relevant to child
            // Then for each cell pair (parent, child) make sure that the
            // projected foreign keys columns in C-space are aligned

            List<Cell> parentCells = new List<Cell>();
            List<Cell> childCells = new List<Cell>();

            foreach (Cell cell in cells)
            {
                if (cell.SQuery.Extent.Equals(ChildTable))
                {
                    childCells.Add(cell);
                }

                if (cell.SQuery.Extent.Equals(ParentTable))
                {
                    parentCells.Add(cell);
                }
            }

            // Make sure that all child cells and parent cells align on
            // the columns, i.e., for each DISTINCT pair C and P, get the columns
            // on the S-side. Then get the corresponding fields on the
            // C-side. The fields on the C-side should match

            bool foundParentCell = false;
            bool foundChildCell = false;

            foreach (Cell childCell in childCells)
            {
                List<List<int>> allChildSlotNums = GetSlotNumsForColumns(childCell, ChildColumns);

                if (allChildSlotNums.Count == 0)
                { // slots in present in S-side, ignore
                    continue;
                }

                List<MemberPath> childPaths = null;
                List<MemberPath> parentPaths = null;
                Cell errorParentCell = null;

                foreach (List<int> childSlotNums in allChildSlotNums)
                {
                    foundChildCell = true;

                    // Get the fields on the C-side
                    childPaths = new List<MemberPath>(childSlotNums.Count);
                    foreach (int childSlotNum in childSlotNums)
                    {
                        // Initial slots only have JoinTreeSlots
                        MemberProjectedSlot childSlot = (MemberProjectedSlot)childCell.CQuery.ProjectedSlotAt(childSlotNum);
                        Debug.Assert(childSlot != null);
                        childPaths.Add(childSlot.MemberPath);
                    }

                    foreach (Cell parentCell in parentCells)
                    {
                        List<List<int>> allParentSlotNums = GetSlotNumsForColumns(parentCell, ParentColumns);
                        if (allParentSlotNums.Count == 0)
                        {
                            // * Parent and child cell are the same - we do not
                            // need to check since we want to check the foreign
                            // key constraint mapping across cells
                            // * Some slots not in present in S-side, ignore
                            continue;
                        }
                        foreach (List<int> parentSlotNums in allParentSlotNums)
                        {
                            foundParentCell = true;

                            parentPaths = new List<MemberPath>(parentSlotNums.Count);
                            foreach (int parentSlotNum in parentSlotNums)
                            {
                                MemberProjectedSlot parentSlot = (MemberProjectedSlot)parentCell.CQuery.ProjectedSlotAt(parentSlotNum);
                                Debug.Assert(parentSlot != null);
                                parentPaths.Add(parentSlot.MemberPath);
                            }

                            // Make sure that the last member of each of these is the same
                            // or the paths are essentially equivalent via referential constraints
                            // We need to check that the last member is essentially the same because it could
                            // be a regular scenario where aid is mapped to PersonAddress and Address - there
                            // is no ref constraint. So when projected into C-Space, we will get Address.aid
                            // and PersonAddress.Address.aid
                            if (childPaths.Count == parentPaths.Count)
                            {
                                bool notAllPathsMatched = false;
                                for (int i = 0; i < childPaths.Count && !notAllPathsMatched; i++)
                                {
                                    MemberPath parentPath = parentPaths[i];
                                    MemberPath childPath = childPaths[i];

                                    if (!parentPath.LeafEdmMember.Equals(childPath.LeafEdmMember)) //Child path did not match
                                    {
                                        if (parentPath.IsEquivalentViaRefConstraint(childPath))
                                        {
                                            //Specifying the referential constraint once in the C space should be enough.
                                            //This is the only way possible today.
                                            //We might be able to derive more knowledge by using boolean logic
                                            return true;
                                        }
                                        else
                                        {
                                            notAllPathsMatched = true;
                                        }
                                    }
                                }

                                if (!notAllPathsMatched)
                                {
                                    return true; //all childPaths matched parentPaths
                                }
                                else
                                {
                                    //If not this one, some other Parent Cell may match.
                                    errorParentCell = parentCell;
                                }
                            }
                        }
                    } //foreach parentCell

                }

                //If execution is at this point, no parent cell's end has matched (otherwise it would have returned true)

                Debug.Assert(childPaths != null, "child paths should be set");
                Debug.Assert(parentPaths != null, "parent paths should be set");
                Debug.Assert(errorParentCell != null, "errorParentCell should be set");
                // using EntityRes. instead of Strings. because the generated method includes 6 instead of 9 parameters
                string message = EntityRes.GetString(EntityRes.ViewGen_Foreign_Key_ColumnOrder_Incorrect,
                                               ToUserString(),
                                               MemberPath.PropertiesToUserString(ChildColumns, false),
                                               ChildTable.Name,
                                               MemberPath.PropertiesToUserString(childPaths, false),
                                               childCell.CQuery.Extent.Name,
                                               MemberPath.PropertiesToUserString(ParentColumns, false),
                                               ParentTable.Name,
                                               MemberPath.PropertiesToUserString(parentPaths, false),
                                               errorParentCell.CQuery.Extent.Name);
                ErrorLog.Record record = new ErrorLog.Record(true, ViewGenErrorCode.ForeignKeyColumnOrderIncorrect, message, new Cell[] { errorParentCell, childCell }, String.Empty);
                errorLog.AddEntry(record);
                return false;

            }
            Debug.Assert(foundParentCell == true, "Some cell that mapped the parent's key must be present!");
            Debug.Assert(foundChildCell == true, "Some cell that mapped the child's foreign key must be present according to the requires clause!");
            return true;
        }

        private static List<List<int>> GetSlotNumsForColumns(Cell cell, IEnumerable<MemberPath> columns)
        {
            List<List<int>> slotNums = new List<List<int>>();
            AssociationSet set = cell.CQuery.Extent as AssociationSet;
            //If it is an association set, the columns could be projected
            //in either end so get the slotNums from both the ends
            if (set != null)
            {
                foreach (AssociationSetEnd setEnd in set.AssociationSetEnds)
                {
                    List<int> endSlots = cell.CQuery.GetAssociationEndSlots(setEnd.CorrespondingAssociationEndMember);
                    Debug.Assert(endSlots.Count > 0);
                    List<int> localslotNums = cell.SQuery.GetProjectedPositions(columns, endSlots);
                    if (localslotNums != null)
                    {
                        slotNums.Add(localslotNums);
                    }
                }
            }
            else
            {
                List<int> localslotNums = cell.SQuery.GetProjectedPositions(columns);
                if (localslotNums != null)
                {
                    slotNums.Add(localslotNums);
                }
            }
            return slotNums;
        }

        // effects: Returns true iff the foreign keys "cover" the primary key
        // in the child table, e.g., <k1, k2> covers <k2> (if k2 is the key
        // of the child table)
        private bool IsForeignKeySuperSetOfPrimaryKeyInChildTable()
        {
            bool isForeignKeySuperSet = true;
            foreach (EdmProperty keyMember in m_childTable.ElementType.KeyMembers)
            {
                // Look for this member in the foreign key members
                bool memberFound = false;
                foreach (MemberPath foreignKeyMember in m_childColumns)
                {
                    // Getting the last member is good enough since it
                    // effectively captures the path (we are not comparing
                    // string names here)
                    if (foreignKeyMember.LeafEdmMember.Equals(keyMember))
                    {
                        memberFound = true;
                        break;
                    }
                }
                if (memberFound == false)
                {
                    isForeignKeySuperSet = false;
                    break;
                }
            }
            return isForeignKeySuperSet;
        }

        // effects: Returns true iff some cell in this refers to the
        // constraint's parent table or child table
        private bool IsConstraintRelevantForCells(IEnumerable<Cell> cells)
        {
            // if the constraint does not deal with any cell in this group,
            // return false
            bool found = false;
            foreach (Cell cell in cells)
            {
                EntitySetBase table = cell.SQuery.Extent;
                if (table.Equals(m_parentTable) || table.Equals(m_childTable))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        #endregion

        #region String methods
        internal string ToUserString()
        {
            string childColsString = MemberPath.PropertiesToUserString(m_childColumns, false);
            string parentColsString = MemberPath.PropertiesToUserString(m_parentColumns, false);
            string result = System.Data.Entity.Strings.ViewGen_Foreign_Key(m_fKeySet.Name,
                                          m_childTable.Name, childColsString, m_parentTable.Name, parentColsString);
            return result;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append(m_fKeySet.Name + ": ");
            builder.Append(ToUserString());
        }
        #endregion
    }
}
