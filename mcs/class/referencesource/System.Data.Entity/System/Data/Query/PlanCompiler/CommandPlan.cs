//---------------------------------------------------------------------
// <copyright file="CommandPlan.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.Common;
using md = System.Data.Metadata.Edm;
using cqt = System.Data.Common.CommandTrees;
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


//
// A CommandPlan represents the plan for a query.
//
namespace System.Data.Query.PlanCompiler
{
    #region CommandInfo

    /// <summary>
    /// Captures information about a single provider command
    /// </summary>
    internal sealed class ProviderCommandInfo
    {
        #region public apis

        /// <summary>
        /// Internal methods to get the command tree
        /// </summary>
        internal cqt.DbCommandTree CommandTree
        {
            get { return _commandTree; }
        }
        #endregion

        #region private state
        private cqt.DbCommandTree _commandTree;
        private ProviderCommandInfo _parent;
        private List<ProviderCommandInfo> _children;
        #endregion

        #region constructors

        /// <summary>
        /// Internal constructor for a ProviderCommandInfo object
        /// </summary>
        /// <param name="commandTree">command tree for the provider command</param>
        /// <param name="children">children command infos</param>
        internal ProviderCommandInfo(cqt.DbCommandTree commandTree, 
            List<ProviderCommandInfo> children)
        {
            _commandTree = commandTree;
            _children = children;

            if (_children == null)
            {
                _children = new List<ProviderCommandInfo>();
            }

            foreach (ProviderCommandInfo child in _children)
            {
                child._parent = this;
            }
        }
        #endregion
    }

    #endregion
}
