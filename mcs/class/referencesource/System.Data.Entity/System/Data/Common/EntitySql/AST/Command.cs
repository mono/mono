//---------------------------------------------------------------------
// <copyright file="Command.cs" company="Microsoft">
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
    /// Represents eSQL command as node. 
    /// </summary>
    internal sealed class Command : Node
    {
        private readonly NodeList<NamespaceImport> _namespaceImportList;
        private readonly Statement _statement;

        /// <summary>
        /// Initializes eSQL command.
        /// </summary>
        /// <param name="nsDeclList">optional namespace imports</param>
        /// <param name="statement">command statement</param>
        internal Command(NodeList<NamespaceImport> nsImportList, Statement statement)
        {
            _namespaceImportList = nsImportList;
            _statement = statement;
        }

        /// <summary>
        /// Returns optional namespace imports. May be null.
        /// </summary>
        internal NodeList<NamespaceImport> NamespaceImportList
        {
            get { return _namespaceImportList; }
        }

        /// <summary>
        /// Returns command statement.
        /// </summary>
        internal Statement Statement
        {
            get { return _statement; }
        }
    }

    /// <summary>
    /// Represents base class for the following statements:
    ///     - QueryStatement
    ///     - InsertStatement
    ///     - UpdateStatement
    ///     - DeleteStatement
    /// </summary>
    internal abstract class Statement : Node { }
}
