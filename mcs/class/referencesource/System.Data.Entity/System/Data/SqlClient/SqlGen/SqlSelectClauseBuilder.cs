//---------------------------------------------------------------------
// <copyright file="SqlSelectClauseBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.SqlClient.SqlGen
{
    /// <summary>
    /// This class is used for building the SELECT clause of a Sql Statement
    /// It is used to gather information about required and optional columns
    /// and whether TOP and DISTINCT should be specified.
    /// 
    /// The underlying SqlBuilder is used for gathering the required columns.
    /// 
    /// The list of OptionalColumns is used for gathering the optional columns. 
    /// Whether a given OptionalColumn should be written is known only after the entire
    /// command tree has been processed. 
    /// 
    /// The IsDistinct property indicates that we want distinct columns.
    /// This is given out of band, since the input expression to the select clause
    /// may already have some columns projected out, and we use append-only SqlBuilders.
    /// The DISTINCT is inserted when we finally write the object into a string.
    /// 
    /// Also, we have a Top property, which is non-null if the number of results should
    /// be limited to certain number. It is given out of band for the same reasons as DISTINCT.
    ///
    /// </summary>
    internal class SqlSelectClauseBuilder : SqlBuilder
    {
        #region Fields and Properties
        private List<OptionalColumn> m_optionalColumns;
        internal void AddOptionalColumn(OptionalColumn column)
        {
            if (m_optionalColumns == null)
            {
                m_optionalColumns = new List<OptionalColumn>();
            }
            m_optionalColumns.Add(column);
        }

        private TopClause m_top;
        internal TopClause Top
        {
            get { return m_top; }
            set
            {
                Debug.Assert(m_top == null, "SqlSelectStatement.Top has already been set");
                m_top = value;
            }
        }

        /// <summary>
        /// Do we need to add a DISTINCT at the beginning of the SELECT
        /// </summary>
        internal bool IsDistinct
        {
            get;
            set;
        }

        /// <summary>
        /// Whether any columns have been specified.
        /// </summary>
        public override bool IsEmpty
        {
            get { return (base.IsEmpty) && (this.m_optionalColumns == null || this.m_optionalColumns.Count == 0); }
        }

        private readonly Func<bool> m_isPartOfTopMostStatement;
        #endregion

        #region Constructor
        internal SqlSelectClauseBuilder(Func<bool> isPartOfTopMostStatement)
        {
            this.m_isPartOfTopMostStatement = isPartOfTopMostStatement;
        }
        #endregion

        #region ISqlFragment Members

        /// <summary>
        /// Writes the string representing the Select statement:
        /// 
        /// SELECT (DISTINCT) (TOP topClause) (optionalColumns) (requiredColumns)
        /// 
        /// If Distinct is specified or this is part of a top most statement 
        /// all optional columns are marked as used.
        /// 
        /// Optional columns are only written if marked as used. 
        /// In addition, if no required columns are specified and no optional columns are 
        /// marked as used, the first optional column is written.
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public override void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("SELECT ");
            if (IsDistinct)
            {
                writer.Write("DISTINCT ");
            }

            if (this.Top != null)
            {
                this.Top.WriteSql(writer, sqlGenerator);
            }

            if (this.IsEmpty)
            {
                Debug.Assert(false);  // we have removed all possibilities of SELECT *.
                writer.Write("*");
            }
            else
            {
                //Print the optional columns if any
                bool printedAny = WriteOptionalColumns(writer, sqlGenerator);

                if (!base.IsEmpty)
                {
                    if (printedAny)
                    {
                        writer.Write(", ");
                    }
                    base.WriteSql(writer, sqlGenerator);
                }
                //If no optional columns were printed and there were no other columns, 
                // print at least the first optional column
                else if (!printedAny)
                {
                    this.m_optionalColumns[0].MarkAsUsed();
                    m_optionalColumns[0].WriteSqlIfUsed(writer, sqlGenerator, "");
                }
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Writes the optional columns that are used. 
        /// If this is the topmost statement or distict is specifed as part of the same statement
        /// all optoinal columns are written.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        /// <returns>Whether at least one column got written</returns>
        private bool WriteOptionalColumns(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            if (this.m_optionalColumns == null)
            {
                return false;
            }

            if (m_isPartOfTopMostStatement() || IsDistinct)
            {
                foreach (OptionalColumn column in this.m_optionalColumns)
                {
                    column.MarkAsUsed();
                }
            }

            string separator = "";
            bool printedAny = false;
            foreach (OptionalColumn column in this.m_optionalColumns)
            {
                if (column.WriteSqlIfUsed(writer, sqlGenerator, separator))
                {
                    printedAny = true;
                    separator = ", ";
                }
            }
            return printedAny;
        }
        #endregion
    }
}
