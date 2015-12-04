//---------------------------------------------------------------------
// <copyright file="CodeGen.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
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

using System.Globalization;
using md = System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
using System.Data.Query.InternalTrees;
using System.Data.Query.PlanCompiler;

//
// The CodeGen module is responsible for translating the ITree finally into a query
//  We assume that various tree transformations have taken place, and the tree
// is finally ready to be executed. The CodeGen module
//   * converts the Itree into one or more CTrees (in S space)
//   * produces a ColumnMap to facilitate result assembly
//   * and wraps up everything in a plan object
//
// 

namespace System.Data.Query.PlanCompiler
{
    internal class CodeGen
    {
        #region public methods
        /// <summary>
        /// This involves 
        ///   * Converting the ITree into a set of ProviderCommandInfo objects
        ///   * Creating a column map to enable result assembly
        /// Currently, we only produce a single ITree, and correspondingly, the 
        /// following steps are trivial
        /// </summary>
        /// <param name="compilerState">current compiler state</param>
        /// <param name="childCommands">CQTs for each store command</param>
        /// <param name="resultColumnMap">column map to help in result assembly</param>
        internal static void Process(PlanCompiler compilerState, out List<ProviderCommandInfo> childCommands, out ColumnMap resultColumnMap, out int columnCount)
        {
            CodeGen codeGen = new CodeGen(compilerState);
            codeGen.Process(out childCommands, out resultColumnMap, out columnCount);
        }

        #endregion

        #region constructors
        private CodeGen(PlanCompiler compilerState)
        {
            m_compilerState = compilerState;           
        }
        #endregion

        #region private methods

        /// <summary>
        /// The real driver. This routine walks the tree, converts each subcommand
        /// into a CTree, and converts the columnmap into a real column map. 
        /// Finally, it produces a "real" plan that can be used by the bridge execution, and
        /// returns this plan
        /// 
        /// The root of the tree must be a PhysicalProjectOp. Each child of this Op
        /// represents a command to be executed, and the ColumnMap of this Op represents
        /// the eventual columnMap to be used for result assembly
        /// </summary>
        /// <param name="childCommands">CQTs for store commands</param>
        /// <param name="resultColumnMap">column map for result assembly</param>
        private void Process(out List<ProviderCommandInfo> childCommands, out ColumnMap resultColumnMap, out int columnCount)
        {
            PhysicalProjectOp projectOp = (PhysicalProjectOp)this.Command.Root.Op;

            this.m_subCommands = new List<Node>(new Node[] { this.Command.Root });
            childCommands = new List<ProviderCommandInfo>(new ProviderCommandInfo[] { 
                ProviderCommandInfoUtils.Create(
                this.Command,                       
                this.Command.Root                  // input node
            )});

            // Build the final column map, and count the columns we expect for it.
            resultColumnMap = BuildResultColumnMap(projectOp);

            columnCount = projectOp.Outputs.Count;
        }

        private ColumnMap BuildResultColumnMap(PhysicalProjectOp projectOp)
        {
            // convert the column map into a real column map
            // build up a dictionary mapping Vars to their real positions in the commands
            Dictionary<Var, KeyValuePair<int, int>> varMap = BuildVarMap();
            ColumnMap realColumnMap = ColumnMapTranslator.Translate(projectOp.ColumnMap, varMap);

            return realColumnMap;
        }

        /// <summary>
        /// For each subcommand, build up a "location-map" for each top-level var that 
        /// is projected out. This location map will ultimately be used to convert VarRefColumnMap
        /// into SimpleColumnMap
        /// </summary>
        private Dictionary<Var, KeyValuePair<int, int>> BuildVarMap()
        {
            Dictionary<Var, KeyValuePair<int, int>> varMap =
                new Dictionary<Var, KeyValuePair<int, int>>();

            int commandId = 0;
            foreach (Node subCommand in m_subCommands)
            {
                PhysicalProjectOp projectOp = (PhysicalProjectOp)subCommand.Op;

                int columnPos = 0;
                foreach (Var v in projectOp.Outputs)
                {
                    KeyValuePair<int, int> varLocation = new KeyValuePair<int, int>(commandId, columnPos);
                    varMap[v] = varLocation;
                    columnPos++;
                }

                commandId++;
            }
            return varMap;
        }
        #endregion

        #region private state
        private PlanCompiler m_compilerState;
        private Command Command { get { return m_compilerState.Command; } }
        private List<Node> m_subCommands;

        #endregion
    }
}
