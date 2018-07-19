//---------------------------------------------------------------------
// <copyright file="ViewgenGatekeeper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Data.Common.Utils.Boolean;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Mapping.ViewGeneration.Utils;
using System.Data.Mapping.ViewGeneration.Validation;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Mapping.ViewGeneration
{
    using CellGroup = Set<Cell>;

    abstract internal class ViewgenGatekeeper : InternalBase
    {
        /// <summary>
        /// Entry point for View Generation
        /// </summary>
        /// <param name="containerMapping"></param>
        /// <param name="workSpace"></param>
        /// <param name="config"></param>
        /// <returns>Generated Views for EntitySets</returns>
        internal static ViewGenResults GenerateViewsFromMapping(StorageEntityContainerMapping containerMapping, ConfigViewGenerator config)
        {
            EntityUtil.CheckArgumentNull(containerMapping, "containerMapping");
            EntityUtil.CheckArgumentNull(config, "config");
            Debug.Assert(containerMapping.HasViews, "Precondition Violated: No mapping exists to generate views for!");

            if (config.IsNormalTracing)
            {
                containerMapping.Print(0);
            }

            //Create Cells from StorageEntityContainerMapping
            CellCreator cellCreator = new CellCreator(containerMapping);
            List<Cell> cells = cellCreator.GenerateCells(config);
            CqlIdentifiers identifiers = cellCreator.Identifiers;

            return GenerateViewsFromCells(cells, config, identifiers, containerMapping);
        }

        /// <summary>
        /// Entry point for Type specific generation of Query Views
        /// </summary>
        internal static ViewGenResults GenerateTypeSpecificQueryView(StorageEntityContainerMapping containerMapping,
                                                              ConfigViewGenerator config,
                                                              EntitySetBase entity,
                                                              EntityTypeBase type,
                                                              bool includeSubtypes,
                                                              out bool success)
        {
            EntityUtil.CheckArgumentNull(containerMapping, "containerMapping");
            EntityUtil.CheckArgumentNull(config, "config");
            EntityUtil.CheckArgumentNull(entity, "entity");
            EntityUtil.CheckArgumentNull(type, "type");
            Debug.Assert(!type.Abstract, "Can not generate OfType/OfTypeOnly query view for and abstract type");

            if (config.IsNormalTracing)
            {
                Helpers.StringTraceLine("");
                Helpers.StringTraceLine("<<<<<<<< Generating Query View for Entity [" + entity.Name + "] OfType" + (includeSubtypes ? "" : "Only") + "(" + type.Name + ") >>>>>>>");
            }

            if (containerMapping.GetEntitySetMapping(entity.Name).QueryView != null)
            {
                //Type-specific QV does not exist in the cache, but 
                // there is a EntitySet QV. So we can't generate the view (no mapping exists for this EntitySet)
                // and we rely on Query to call us again to get the EntitySet View.
                success = false;
                return null;
            }

            //Compute Cell Groups or get it from Memoizer
            InputForComputingCellGroups args = new InputForComputingCellGroups(containerMapping, config);
            OutputFromComputeCellGroups result = containerMapping.GetCellgroups(args);
            success = result.Success;

            if (!success)
            {
                return null;
            }

            List<ForeignConstraint> foreignKeyConstraints = result.ForeignKeyConstraints;
            // Get a Clone of cell groups from cache since cells are modified during viewgen, and we dont want the cached copy to change
            List<CellGroup> cellGroups = cellGroups = result.CellGroups.Select(setOfcells => new CellGroup(setOfcells.Select(cell => new Cell(cell)))).ToList();
            List<Cell> cells = result.Cells;
            CqlIdentifiers identifiers = result.Identifiers;


            ViewGenResults viewGenResults = new ViewGenResults();
            ErrorLog tmpLog = EnsureAllCSpaceContainerSetsAreMapped(cells, config, containerMapping);
            if (tmpLog.Count > 0)
            {
                viewGenResults.AddErrors(tmpLog);
                Helpers.StringTraceLine(viewGenResults.ErrorsToString());
                success = true; //atleast we tried successfully
                return viewGenResults;
            }

            foreach (CellGroup cellGroup in cellGroups)
            {
                if (!DoesCellGroupContainEntitySet(cellGroup, entity))
                {
                    continue;
                }

                ViewGenerator viewGenerator = null;
                ErrorLog groupErrorLog = new ErrorLog();
                try
                {
                    viewGenerator = new ViewGenerator(cellGroup, config, foreignKeyConstraints, containerMapping);
                }
                catch (InternalMappingException exception)
                {
                    // All exceptions have mapping errors in them
                    Debug.Assert(exception.ErrorLog.Count > 0, "Incorrectly created mapping exception");
                    groupErrorLog = exception.ErrorLog;
                }

                if (groupErrorLog.Count > 0)
                {
                    break;
                }
                Debug.Assert(viewGenerator != null); //make sure there is no exception thrown that does not add error to log

                ViewGenMode mode = includeSubtypes ? ViewGenMode.OfTypeViews : ViewGenMode.OfTypeOnlyViews;

                groupErrorLog = viewGenerator.GenerateQueryViewForSingleExtent(viewGenResults.Views, identifiers, entity, type, mode);

                if (groupErrorLog.Count != 0)
                {
                    viewGenResults.AddErrors(groupErrorLog);
                }
            }

            success = true;
            return viewGenResults;
        }


        // effects: Given a list of cells in the schema, generates the query and
        // update mapping views for OFTYPE(Extent, Type) combinations in this schema
        // container. Returns a list of generated query and update views.
        // If it is false and some columns in a table are unmapped, an
        // exception is raised
        private static ViewGenResults GenerateViewsFromCells(List<Cell> cells, ConfigViewGenerator config,
                                                                   CqlIdentifiers identifiers,
                                                                   StorageEntityContainerMapping containerMapping)
        {
            EntityUtil.CheckArgumentNull(cells, "cells");
            EntityUtil.CheckArgumentNull(config, "config");
            Debug.Assert(cells.Count > 0, "There must be at least one cell in the container mapping");


            // Go through each table and determine their foreign key constraints
            EntityContainer container = containerMapping.StorageEntityContainer;
            Debug.Assert(container != null);

            ViewGenResults viewGenResults = new ViewGenResults();
            ErrorLog tmpLog = EnsureAllCSpaceContainerSetsAreMapped(cells, config, containerMapping);
            if (tmpLog.Count > 0)
            {
                viewGenResults.AddErrors(tmpLog);
                Helpers.StringTraceLine(viewGenResults.ErrorsToString());
                return viewGenResults;
            }

            List<ForeignConstraint> foreignKeyConstraints = ForeignConstraint.GetForeignConstraints(container);

            CellPartitioner partitioner = new CellPartitioner(cells, foreignKeyConstraints);
            List<CellGroup> cellGroups = partitioner.GroupRelatedCells();
            foreach (CellGroup cellGroup in cellGroups)
            {
                ViewGenerator viewGenerator = null;
                ErrorLog groupErrorLog = new ErrorLog();
                try
                {
                    viewGenerator = new ViewGenerator(cellGroup, config, foreignKeyConstraints, containerMapping);
                }
                catch (InternalMappingException exception)
                {
                    // All exceptions have mapping errors in them
                    Debug.Assert(exception.ErrorLog.Count > 0, "Incorrectly created mapping exception");
                    groupErrorLog = exception.ErrorLog;
                }

                if (groupErrorLog.Count == 0)
                {
                    Debug.Assert(viewGenerator != null);
                    groupErrorLog = viewGenerator.GenerateAllBidirectionalViews(viewGenResults.Views, identifiers);
                }

                if (groupErrorLog.Count != 0)
                {
                    viewGenResults.AddErrors(groupErrorLog);
                }
            }
            // We used to print the errors here. Now we trace them as they are being thrown
            //if (viewGenResults.HasErrors && config.IsViewTracing) {
            //    Helpers.StringTraceLine(viewGenResults.ErrorsToString());
            //}
            return viewGenResults;
        }



        // effects: Given a container, ensures that all entity/association
        // sets in container on the C-side have been mapped
        private static ErrorLog EnsureAllCSpaceContainerSetsAreMapped(IEnumerable<Cell> cells,
                                                                      ConfigViewGenerator config,
                                                                      StorageEntityContainerMapping containerMapping)
        {

            Set<EntitySetBase> mappedExtents = new Set<EntitySetBase>();
            string mslFileLocation = null;
            EntityContainer container = null;
            // Determine the container and name of the file while determining
            // the set of mapped extents in the cells
            foreach (Cell cell in cells)
            {
                mappedExtents.Add(cell.CQuery.Extent);
                mslFileLocation = cell.CellLabel.SourceLocation;
                // All cells are from the same container
                container = cell.CQuery.Extent.EntityContainer;
            }
            Debug.Assert(container != null);

            List<EntitySetBase> missingExtents = new List<EntitySetBase>();
            // Go through all the extents in the container and determine
            // extents that are missing
            foreach (EntitySetBase extent in container.BaseEntitySets)
            {
                if (mappedExtents.Contains(extent) == false
                    && !(containerMapping.HasQueryViewForSetMap(extent.Name)))
                {
                    AssociationSet associationSet = extent as AssociationSet;
                    if (associationSet==null || !associationSet.ElementType.IsForeignKey)
                    {
                        missingExtents.Add(extent);
                    }
                }
            }
            ErrorLog errorLog = new ErrorLog();
            // If any extent is not mapped, add an error
            if (missingExtents.Count > 0)
            {
                StringBuilder extentBuilder = new StringBuilder();
                bool isFirst = true;
                foreach (EntitySetBase extent in missingExtents)
                {
                    if (isFirst == false)
                    {
                        extentBuilder.Append(", ");
                    }
                    isFirst = false;
                    extentBuilder.Append(extent.Name);
                }
                string message = System.Data.Entity.Strings.ViewGen_Missing_Set_Mapping(extentBuilder);
                // Find the cell with smallest line number - so that we can
                // point to the beginning of the file
                int lowestLineNum = -1;
                Cell smallestCell = null;
                foreach (Cell cell in cells)
                {
                    if (lowestLineNum == -1 || cell.CellLabel.StartLineNumber < lowestLineNum)
                    {
                        smallestCell = cell;
                        lowestLineNum = cell.CellLabel.StartLineNumber;
                    }
                }
                Debug.Assert(smallestCell != null && lowestLineNum >= 0);
                EdmSchemaError edmSchemaError = new EdmSchemaError(message, (int)ViewGenErrorCode.MissingExtentMapping,
                    EdmSchemaErrorSeverity.Error, containerMapping.SourceLocation, containerMapping.StartLineNumber,
                    containerMapping.StartLinePosition, null);
                ErrorLog.Record record = new ErrorLog.Record(edmSchemaError);
                errorLog.AddEntry(record);
            }
            return errorLog;
        }







        #region Static Helpers
        private static bool DoesCellGroupContainEntitySet(CellGroup group, EntitySetBase entity)
        {
            foreach (Cell cell in group)
            {
                if (cell.GetLeftQuery(ViewTarget.QueryView).Extent.Equals(entity))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        internal override void ToCompactString(StringBuilder builder)
        {

        }


    }

}
