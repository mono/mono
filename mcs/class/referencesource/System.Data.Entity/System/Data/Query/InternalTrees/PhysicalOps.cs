//---------------------------------------------------------------------
// <copyright file="PhysicalOps.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Data.Query.PlanCompiler;
using md = System.Data.Metadata.Edm;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// A PhysicalProjectOp is a physical Op capping the entire command tree (and the 
    /// subtrees of CollectOps). 
    /// </summary>
    internal class PhysicalProjectOp : PhysicalOp
    {
        #region public methods
        /// <summary>
        /// Instance for pattern matching in rules
        /// </summary>
        internal static readonly PhysicalProjectOp Pattern = new PhysicalProjectOp();
 
        /// <summary>
        /// Get the column map that describes how the result should be reshaped
        /// </summary>
        internal SimpleCollectionColumnMap ColumnMap 
        { 
            get { return m_columnMap; } 
        }

        /// <summary>
        /// Get the (ordered) list of output vars that this node produces
        /// </summary>
        internal VarList Outputs 
        { 
            get { return m_outputVars; } 
        }

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

        #region private constructors
        /// <summary>
        /// basic constructor
        /// </summary>
        /// <param name="outputVars">List of outputs from this Op</param>
        /// <param name="columnMap">column map that describes the result to be shaped</param>
        internal PhysicalProjectOp(VarList outputVars, SimpleCollectionColumnMap columnMap)
            : this()
        {
            Debug.Assert(null != columnMap, "null columnMap?");
            m_outputVars = outputVars;
            m_columnMap = columnMap;
        }

        private PhysicalProjectOp()
            : base(OpType.PhysicalProject)
        {
        }
        #endregion

        #region private state
        private SimpleCollectionColumnMap m_columnMap;
        private VarList m_outputVars;
        #endregion
    }

    /// <summary>
    /// Represents information about one collection being managed by the NestOps. 
    /// The CollectionVar is a Var that represents the entire collection. 
    /// </summary>
    internal class CollectionInfo
    {
        #region public methods
        /// <summary>
        /// The collection-var
        /// </summary>
        internal Var CollectionVar
        {
            get { return m_collectionVar; }
        }
        /// <summary>
        /// the column map for the collection element
        /// </summary>
        internal ColumnMap ColumnMap
        {
            get { return m_columnMap; }
        }

        /// <summary>
        /// list of vars describing the collection element; flattened to remove 
        /// nested collections
        /// </summary>
        internal VarList FlattenedElementVars
        {
            get { return m_flattenedElementVars; }
        }

        /// <summary>
        /// list of keys specific to this collection
        /// </summary>
        internal VarVec Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        /// list of sort keys specific to this collection
        /// </summary>
        internal List<InternalTrees.SortKey> SortKeys
        {
            get { return m_sortKeys; }
        }

        /// <summary>
        /// Discriminator Value for this collection (for a given NestOp).
        /// Should we break this out into a subtype of CollectionInfo
        /// </summary>
        internal object DiscriminatorValue
        {
            get { return m_discriminatorValue; }
        }

        #endregion

        #region constructors
        internal CollectionInfo(Var collectionVar, ColumnMap columnMap, VarList flattenedElementVars, VarVec keys, List<InternalTrees.SortKey> sortKeys, object discriminatorValue)
        {
            m_collectionVar = collectionVar;
            m_columnMap = columnMap;
            m_flattenedElementVars = flattenedElementVars;
            m_keys = keys;
            m_sortKeys = sortKeys;
            m_discriminatorValue = discriminatorValue;
        }
        #endregion

        #region private state
        private Var m_collectionVar;    // the collection Var
        private ColumnMap m_columnMap;  // column map for the collection element
        private VarList m_flattenedElementVars; // elementVars, removing collections;
        private VarVec m_keys;              //list of keys specific to this collection
        private List<InternalTrees.SortKey> m_sortKeys;          //list of sort keys specific to this collection
        private object m_discriminatorValue;
        #endregion
    }

    /// <summary>
    /// Base class for Nest operations
    /// </summary>
    internal abstract class NestBaseOp : PhysicalOp
    {
        #region publics

        /// <summary>
        /// (Ordered) list of prefix sort keys (defines ordering of results)
        /// </summary>
        internal List<SortKey> PrefixSortKeys
        {
            get { return m_prefixSortKeys; }
        }

        /// <summary>
        /// Outputs of the NestOp. Includes the Keys obviously, and one Var for each of
        /// the collections produced. In addition, this may also include non-key vars
        /// from the outer row
        /// </summary>
        internal VarVec Outputs 
        { 
            get { return m_outputs; } 
        }
        
        /// <summary>
        /// Information about each collection managed by the NestOp
        /// </summary>
        internal List<CollectionInfo> CollectionInfo
        {
            get { return m_collectionInfoList; }
        }
        #endregion

        #region constructors
        internal NestBaseOp(OpType opType, List<SortKey> prefixSortKeys, 
            VarVec outputVars, 
            List<CollectionInfo> collectionInfoList)
            : base(opType)
        {
            m_outputs = outputVars;
            m_collectionInfoList = collectionInfoList;
            m_prefixSortKeys = prefixSortKeys;
        }
        #endregion

        #region private state
        private List<SortKey> m_prefixSortKeys; // list of sort key prefixes
        private VarVec m_outputs; // list of all output vars
        private List<CollectionInfo> m_collectionInfoList;
        #endregion
    }

    /// <summary>
    /// Single-stream nest aggregation Op. 
    /// (Somewhat similar to a group-by op - should we merge these?)
    /// </summary>
    internal class SingleStreamNestOp : NestBaseOp
    {
        #region publics
        /// <summary>
        /// 1 child - the input
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// The discriminator Var (when there are multiple collections)
        /// </summary>
        internal Var Discriminator 
        { 
            get { return m_discriminator; } 
        }
        /// <summary>
        /// List of postfix sort keys (mostly to deal with multi-level nested collections)
        /// </summary>
        internal List<SortKey> PostfixSortKeys
        {
            get { return m_postfixSortKeys; }
        }
        /// <summary>
        /// Set of keys for this nest operation
        /// </summary>
        internal VarVec Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) 
        { 
            v.Visit(this, n); 
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) 
        { 
            return v.Visit(this, n); 
        }

        #endregion

        #region constructors
        internal SingleStreamNestOp(VarVec keys,
            List<SortKey> prefixSortKeys, List<SortKey> postfixSortKeys, 
            VarVec outputVars, List<CollectionInfo> collectionInfoList, 
            Var discriminatorVar)
            : base(OpType.SingleStreamNest, prefixSortKeys, outputVars, collectionInfoList)
        {
            m_keys = keys;
            m_postfixSortKeys = postfixSortKeys;
            m_discriminator = discriminatorVar;
        }
        #endregion

        #region private state
        private VarVec m_keys; // keys for this operation
        private Var m_discriminator; // Var describing the discriminator
        List<SortKey> m_postfixSortKeys; // list of postfix sort keys 
        #endregion
    }

    /// <summary>
    /// Represents a multi-stream nest operation. The first input represents the 
    /// container row, while all the other inputs represent collections
    /// </summary>
    internal class MultiStreamNestOp : NestBaseOp
    {
        #region publics
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

        #region constructors
        internal MultiStreamNestOp(List<SortKey> prefixSortKeys, VarVec outputVars, 
            List<CollectionInfo> collectionInfoList)
            : base(OpType.MultiStreamNest, prefixSortKeys, outputVars, collectionInfoList)
        {
        }
        #endregion

        #region private state
        #endregion
    }
}
