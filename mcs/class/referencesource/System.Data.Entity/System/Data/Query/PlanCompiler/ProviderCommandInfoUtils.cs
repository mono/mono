//---------------------------------------------------------------------
// <copyright file="ProviderCommandInfoUtils.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

using System.Data.Common.CommandTrees;
using System.Data.Common;
using md = System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;
using System.Data.Query.PlanCompiler;

namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// Helper class for creating a ProviderCommandInfo given an Iqt Node. 
    /// </summary>
    internal static class ProviderCommandInfoUtils
    {
        #region Public Methods

        /// <summary>
        /// Creates a ProviderCommandInfo for the given node. 
        /// This method should be called when the keys, foreign keys and sort keys are known ahead of time.
        /// Typically it is used when the original command is factored into multiple commands. 
        /// </summary>
        /// <param name="command">The owning command, used for creating VarVecs, etc</param>
        /// <param name="node">The root of the sub-command for which a ProviderCommandInfo should be generated</param>
        /// <param name="children">A list of ProviderCommandInfos that were created for the child sub-commands.</param>
        /// <returns>The resulting ProviderCommandInfo</returns>
        internal static ProviderCommandInfo Create(
            Command command,
            Node node, 
            List<ProviderCommandInfo> children)
        {
            PhysicalProjectOp projectOp = node.Op as PhysicalProjectOp;
            PlanCompiler.Assert(projectOp != null, "Expected root Op to be a physical Project");

            // build up the CQT
            DbCommandTree ctree = CTreeGenerator.Generate(command, node);
            DbQueryCommandTree cqtree = ctree as DbQueryCommandTree;
            PlanCompiler.Assert(cqtree != null, "null query command tree");

            // Get the rowtype for the result cqt
            md.CollectionType collType = TypeHelpers.GetEdmType<md.CollectionType>(cqtree.Query.ResultType);
            PlanCompiler.Assert(md.TypeSemantics.IsRowType(collType.TypeUsage), "command rowtype is not a record");

            // Build up a mapping from Vars to the corresponding output property/column
            Dictionary<Var, md.EdmProperty> outputVarMap = BuildOutputVarMap(projectOp, collType.TypeUsage);

            return new ProviderCommandInfo(ctree, children);
        }

        /// <summary>
        /// Creates a ProviderCommandInfo for the given node. 
        /// This method should be called when the keys and the sort keys are not known ahead of time.
        /// Typically it is used when there is only one command, that is no query factoring is done.
        /// This method also has the option of pulling up keys and sort information. 
        /// </summary>
        /// <param name="command">The owning command, used for creating VarVecs, etc</param>
        /// <param name="node">The root of the sub-command for which a ProviderCommandInfo should be generated</param>
        /// <returns>The resulting ProviderCommandInfo</returns>
        internal static ProviderCommandInfo Create(
            Command command,
            Node node)
        {   
            return Create(
                command, 
                node, 
                new List<ProviderCommandInfo>() //children 
                );
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Build up a mapping from Vars to the corresponding property of the output row type
        /// </summary>
        /// <param name="projectOp">the physical projectOp</param>
        /// <param name="outputType">output type</param>
        /// <returns>a map from Vars to the output type member</returns>
        private static Dictionary<Var, md.EdmProperty> BuildOutputVarMap(PhysicalProjectOp projectOp, md.TypeUsage outputType)
        {
            Dictionary<Var, md.EdmProperty> outputVarMap = new Dictionary<Var, md.EdmProperty>();

            PlanCompiler.Assert(md.TypeSemantics.IsRowType(outputType), "PhysicalProjectOp result type is not a RowType?");

            IEnumerator<md.EdmProperty> propertyEnumerator = TypeHelpers.GetEdmType<md.RowType>(outputType).Properties.GetEnumerator();
            IEnumerator<Var> varEnumerator = projectOp.Outputs.GetEnumerator();
            while (true)
            {
                bool foundProp = propertyEnumerator.MoveNext();
                bool foundVar = varEnumerator.MoveNext();
                if (foundProp != foundVar)
                {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 1);
                }
                if (!foundProp)
                {
                    break;
                }
                outputVarMap[varEnumerator.Current] = propertyEnumerator.Current;
            }
            return outputVarMap;
        }

        #endregion
    }
}
