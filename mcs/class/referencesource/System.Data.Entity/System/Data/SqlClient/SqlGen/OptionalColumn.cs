//---------------------------------------------------------------------
// <copyright file="OptionalColumn.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.SqlClient.SqlGen
{
    /// <summary>
    /// Represents a column in a select list that should be printed only if it is later used.
    /// Such columns get added by <see cref="SqlGenerator.AddDefaultColumns"/>.
    /// The SymbolUsageManager associated with the OptionalColumn has the information whether the column
    /// has been used based on its symbol.
    /// </summary>
    internal sealed class OptionalColumn
    {
        #region Private State
        private readonly SymbolUsageManager m_usageManager;

        // The SqlBuilder that contains the column building blocks (e.g: "c.X as X1")
        private readonly SqlBuilder m_builder = new SqlBuilder();

        // The symbol representing the optional column
        private readonly Symbol m_symbol;
        #endregion

        #region Internal Methods
        /// <summary>
        /// Append to the "fragment" representing this column
        /// </summary>
        internal void Append(object s)
        {
            m_builder.Append(s);
        }

        internal void MarkAsUsed()
        {
            this.m_usageManager.MarkAsUsed(this.m_symbol);
        }
        #endregion

        #region Constructor
        internal OptionalColumn(SymbolUsageManager usageManager, Symbol symbol)
        {
            this.m_usageManager = usageManager;
            this.m_symbol = symbol;
        }
        #endregion

        #region Internal members

        /// <summary>
        /// Writes that fragment that represents the optional column 
        /// if the usage manager says it is used.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public bool WriteSqlIfUsed(SqlWriter writer, SqlGenerator sqlGenerator, string separator)
        {
            if (m_usageManager.IsUsed(m_symbol))
            {
                writer.Write(separator);
                m_builder.WriteSql(writer, sqlGenerator);
                return true;
            }
            return false;
        }
        #endregion
    }
}
