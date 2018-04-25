//---------------------------------------------------------------------
// <copyright file="AstNode.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql.AST
{
    using System;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents base class for nodes in the eSQL abstract syntax tree OM.
    /// </summary>
    internal abstract class Node
    {
        private ErrorContext _errCtx = new ErrorContext();

        internal Node() { }

        internal Node(string commandText, int inputPosition)
        {
            _errCtx.CommandText = commandText;
            _errCtx.InputPosition = inputPosition;
        }

        /// <summary>
        /// Ast Node error context.
        /// </summary>
        internal ErrorContext ErrCtx
        {
            get { return _errCtx; }
            set { _errCtx = value; }
        }
    }

    /// <summary>
    /// An ast node represents a generic list of ast nodes.
    /// </summary>
    internal sealed class NodeList<T> : Node, System.Collections.Generic.IEnumerable<T>
        where T : Node
    {
        private readonly List<T> _list = new List<T>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal NodeList()
        {
        }

        /// <summary>
        /// Initializes adding one item to the list.
        /// </summary>
        /// <param name="item">expression</param>
        internal NodeList(T item)
        {
            _list.Add(item);
        }

        /// <summary>
        /// Add an item to the list, return the updated list.
        /// </summary>
        internal NodeList<T> Add(T item)
        {
            _list.Add(item);
            return this;
        }

        /// <summary>
        /// Returns the number of elements in the list.
        /// </summary>
        internal int Count
        {
            get { return _list.Count; }
        }

        /// <summary>
        /// Indexer to the list entries.
        /// </summary>
        /// <param name="index">integer position of the element in the list</param>
        internal T this[int index]
        {
            get { return _list[index]; }
        }

        #region GetEnumerator
        System.Collections.Generic.IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        #endregion
    }
}
