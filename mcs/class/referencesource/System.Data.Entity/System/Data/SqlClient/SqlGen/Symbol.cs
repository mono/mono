//---------------------------------------------------------------------
// <copyright file="Symbol.cs" company="Microsoft">
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
    /// <see cref="SymbolTable"/>
    /// This class represents an extent/nested select statement,
    /// or a column.
    ///
    /// The important fields are Name, Type and NewName.
    /// NewName starts off the same as Name, and is then modified as necessary.
    ///
    ///
    /// The rest are used by special symbols.
    /// e.g. NeedsRenaming is used by columns to indicate that a new name must
    /// be picked for the column in the second phase of translation.
    ///
    /// IsUnnest is used by symbols for a collection expression used as a from clause.
    /// This allows <see cref="SqlGenerator.AddFromSymbol(SqlSelectStatement, string, Symbol, bool)"/> to add the column list
    /// after the alias.
    ///
    /// </summary>
    internal class Symbol : ISqlFragment
    {
        /// <summary>
        /// Used to track the columns originating from this Symbol when it is used
        /// in as a from extent in a SqlSelectStatement with a Join or as a From Extent
        /// in a Join Symbol.
        /// </summary>
        private Dictionary<string, Symbol> columns;
        internal Dictionary<string, Symbol> Columns
        {
            get
            {
                if (null == columns)
                {
                    columns = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
                }
                return columns;
            }
        }

        /// <summary>
        /// Used to track the output columns of a SqlSelectStatement it represents
        /// </summary>
        private Dictionary<string, Symbol> outputColumns;
        internal Dictionary<string, Symbol> OutputColumns
        {
            get
            {
                if (null == outputColumns)
                {
                    outputColumns = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
                }
                return outputColumns;
            }
        }

        private bool needsRenaming;
        internal bool NeedsRenaming
        {
            get { return needsRenaming; }
            set { needsRenaming = value; }
        }

        private bool outputColumnsRenamed;
        internal bool OutputColumnsRenamed
        {
            get { return outputColumnsRenamed; }
            set { outputColumnsRenamed = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
        }

        private string newName;
        public string NewName
        {
            get { return newName; }
            set { newName = value; }
        }

        private TypeUsage type;
        internal TypeUsage Type
        {
            get { return type; }
            set { type = value; }
        }

        public Symbol(string name, TypeUsage type)
        {
            this.name = name;
            this.newName = name;
            this.Type = type;
        }

        /// <summary>
        /// Use this constructor if the symbol represents a SqlStatement for which the output columns need to be tracked.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="outputColumns"></param>
        /// <param name="outputColumnsRenamed"></param>
        public Symbol(string name, TypeUsage type, Dictionary<string, Symbol> outputColumns, bool outputColumnsRenamed)
        {
            this.name = name;
            this.newName = name;
            this.Type = type;
            this.outputColumns = outputColumns;
            this.OutputColumnsRenamed = outputColumnsRenamed;
        }

        #region ISqlFragment Members

        /// <summary>
        /// Write this symbol out as a string for sql.  This is just
        /// the new name of the symbol (which could be the same as the old name).
        ///
        /// We rename columns here if necessary.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            if (this.NeedsRenaming)
            {
                int i;

                if (sqlGenerator.AllColumnNames.TryGetValue(this.NewName, out i))
                {
                    string newNameCandidate;
                    do
                    {
                        ++i;
                        newNameCandidate = this.NewName + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    } while (sqlGenerator.AllColumnNames.ContainsKey(newNameCandidate));

                    sqlGenerator.AllColumnNames[this.NewName] = i;

                    this.NewName = newNameCandidate;
                }

                // Add this column name to list of known names so that there are no subsequent
                // collisions
                sqlGenerator.AllColumnNames[this.NewName] = 0;

                // Prevent it from being renamed repeatedly.
                this.NeedsRenaming = false;
            }
            writer.Write(SqlGenerator.QuoteIdentifier(this.NewName));
        }

        #endregion
    }
}
