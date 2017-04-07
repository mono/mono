//---------------------------------------------------------------------
// <copyright file="TopClause.cs" company="Microsoft">
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
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

namespace System.Data.SqlClient.SqlGen
{
    /// <summary>
    /// TopClause represents the a TOP expression in a SqlSelectStatement. 
    /// It has a count property, which indicates how many TOP rows should be selected and a 
    /// boolen WithTies property.
    /// </summary>
    class TopClause : ISqlFragment
    {
        ISqlFragment topCount;
        bool withTies;

        /// <summary>
        /// Do we need to add a WITH_TIES to the top statement
        /// </summary>
        internal bool WithTies
        {
            get { return withTies; }
        }

        /// <summary>
        /// How many top rows should be selected.
        /// </summary>
        internal ISqlFragment TopCount
        {
            get { return topCount; }
        }

        /// <summary>
        /// Creates a TopClause with the given topCount and withTies.
        /// </summary>
        /// <param name="topCount"></param>
        /// <param name="withTies"></param>
        internal TopClause(ISqlFragment topCount, bool withTies)
        {
            this.topCount = topCount;
            this.withTies = withTies;
        }

        /// <summary>
        /// Creates a TopClause with the given topCount and withTies.
        /// </summary>
        /// <param name="topCount"></param>
        /// <param name="withTies"></param>
        internal TopClause(int topCount, bool withTies)
        {
            SqlBuilder sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(topCount.ToString(CultureInfo.InvariantCulture));
            this.topCount = sqlBuilder;
            this.withTies = withTies;
        }

        #region ISqlFragment Members

        /// <summary>
        /// Write out the TOP part of sql select statement 
        /// It basically writes TOP (X) [WITH TIES].
        /// The brackets around X are ommited for Sql8.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("TOP ");

            if (sqlGenerator.SqlVersion != SqlVersion.Sql8)
            {
                writer.Write("(");
            }

            this.TopCount.WriteSql(writer, sqlGenerator);

            if (sqlGenerator.SqlVersion != SqlVersion.Sql8)
            {
                writer.Write(")");
            }

            writer.Write(" ");

            if (this.WithTies)
            {
                writer.Write("WITH TIES ");
            }
        }

        #endregion
    }
}
