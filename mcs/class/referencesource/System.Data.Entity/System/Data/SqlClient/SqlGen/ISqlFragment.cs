//---------------------------------------------------------------------
// <copyright file="ISqlFragment.cs" company="Microsoft">
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
    /// Represents the sql fragment for any node in the query tree.
    /// </summary>
    /// <remarks>
    /// The nodes in a query tree produce various kinds of sql
    /// <list type="bullet">
    /// <item>A select statement.</item>
    /// <item>A reference to an extent. (symbol)</item>
    /// <item>A raw string.</item>
    /// </list>
    /// We have this interface to allow for a common return type for the methods
    /// in the expression visitor <see cref="DbExpressionVisitor{T}"/>
    /// 
    /// Add the endd of translation, the sql fragments are converted into real strings.
    /// </remarks>
    internal interface ISqlFragment
    {
        /// <summary>
        /// Write the string represented by this fragment into the stream.
        /// </summary>
        /// <param name="writer">The stream that collects the strings.</param>
        /// <param name="sqlGenerator">Context information used for renaming.
        /// The global lists are used to generated new names without collisions.</param>
        void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
    }
}
