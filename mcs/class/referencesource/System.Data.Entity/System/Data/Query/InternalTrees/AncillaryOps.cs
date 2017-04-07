//---------------------------------------------------------------------
// <copyright file="AnciliaryOps.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// A definition of a variable
    /// </summary>
    internal sealed class VarDefOp : AncillaryOp
    {
        #region private state
        private Var m_var;
        #endregion

        #region constructors
        internal VarDefOp(Var v) : this()
        { 
            m_var = v; 
        }
        private VarDefOp() : base(OpType.VarDef) { }
        #endregion

        #region public methods
        internal static readonly VarDefOp Pattern = new VarDefOp();

        /// <summary>
        /// 1 child - the defining expression
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// The Var being defined
        /// </summary>
        internal Var Var { get { return m_var; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }
        
        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Helps define a list of VarDefOp
    /// </summary>
    internal sealed class VarDefListOp : AncillaryOp
    {
        #region constructors
        private VarDefListOp() : base(OpType.VarDefList) { }
        #endregion

        #region public methods
        /// <summary>
        /// singleton instance
        /// </summary>
        internal static readonly VarDefListOp Instance = new VarDefListOp();
        internal static readonly VarDefListOp Pattern = Instance;

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }
        
        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }
}
