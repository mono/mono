//---------------------------------------------------------------------
// <copyright file="SymbolPair.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

namespace System.Data.SqlClient.SqlGen
{
    /// <summary>
    /// The SymbolPair exists to solve the record flattening problem.
    /// <see cref="SqlGenerator.Visit(DbPropertyExpression)"/>
    /// Consider a property expression D(v, "j3.j2.j1.a.x")
    /// where v is a VarRef, j1, j2, j3 are joins, a is an extent and x is a columns.
    /// This has to be translated eventually into {j'}.{x'}
    /// 
    /// The source field represents the outermost SqlStatement representing a join
    /// expression (say j2) - this is always a Join symbol.
    /// 
    /// The column field keeps moving from one join symbol to the next, until it
    /// stops at a non-join symbol.
    /// 
    /// This is returned by <see cref="SqlGenerator.Visit(DbPropertyExpression)"/>,
    /// but never makes it into a SqlBuilder.
    /// </summary>
    class SymbolPair : ISqlFragment
    {
        public Symbol Source;
        public Symbol Column;

        public SymbolPair(Symbol source, Symbol column)
        {
            this.Source = source;
            this.Column = column;
        }

        #region ISqlFragment Members

        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            // Symbol pair should never be part of a SqlBuilder.
            Debug.Assert(false);
        }

        #endregion
    }
}
