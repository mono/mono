//---------------------------------------------------------------------
// <copyright file="SqlSelectStatement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Common.CommandTrees;

namespace System.Data.SqlClient.SqlGen
{
    /// <summary>
    /// A SqlSelectStatement represents a canonical SQL SELECT statement.
    /// It has fields for the 5 main clauses
    /// <list type="number">
    /// <item>SELECT</item>
    /// <item>FROM</item>
    /// <item>WHERE</item>
    /// <item>GROUP BY</item>
    /// <item>ORDER BY</item>
    /// </list>
    /// We do not have HAVING, since the CQT does not have such a node.
    /// Each of the fields is a SqlBuilder, so we can keep appending SQL strings
    /// or other fragments to build up the clause.
    ///
    /// The FromExtents contains the list of inputs in use for the select statement.
    /// There is usually just one element in this - Select statements for joins may
    /// temporarily have more than one.
    ///
    /// If the select statement is created by a Join node, we maintain a list of
    /// all the extents that have been flattened in the join in AllJoinExtents
    /// <example>
    /// in J(j1= J(a,b), c)
    /// FromExtents has 2 nodes JoinSymbol(name=j1, ...) and Symbol(name=c)
    /// AllJoinExtents has 3 nodes Symbol(name=a), Symbol(name=b), Symbol(name=c)
    /// </example>
    ///
    /// If any expression in the non-FROM clause refers to an extent in a higher scope,
    /// we add that extent to the OuterExtents list.  This list denotes the list
    /// of extent aliases that may collide with the aliases used in this select statement.
    /// It is set by <see cref="SqlGenerator.Visit(DbVariableReferenceExpression)"/>.
    /// An extent is an outer extent if it is not one of the FromExtents.
    ///
    ///
    /// </summary>
    internal sealed class SqlSelectStatement : ISqlFragment
    {
       
        /// <summary>
        /// Whether the columns ouput by this sql statement were renamed from what given in the command tree.
        /// </summary>
        internal bool OutputColumnsRenamed
        {
            get;
            set;
        }
        
        /// <summary>
        /// A dictionary of output columns
        /// </summary>
        internal Dictionary<string, Symbol> OutputColumns
        {
            get;
            set;
        }

        internal List<Symbol> AllJoinExtents
        {
            get;
            // We have a setter as well, even though this is a list,
            // since we use this field only in special cases.
            set;
        }

        private List<Symbol> fromExtents;
        internal List<Symbol> FromExtents
        {
            get
            {
                if (null == fromExtents)
                {
                    fromExtents = new List<Symbol>();
                }
                return fromExtents;
            }
        }

        private Dictionary<Symbol, bool> outerExtents;
        internal Dictionary<Symbol, bool> OuterExtents
        {
            get
            {
                if (null == outerExtents)
                {
                    outerExtents = new Dictionary<Symbol, bool>();
                }
                return outerExtents;
            }
        }

        private readonly SqlSelectClauseBuilder select;
        internal SqlSelectClauseBuilder Select
        {
            get { return select; }
        }

        private readonly SqlBuilder from = new SqlBuilder();
        internal SqlBuilder From
        {
            get { return from; }
        }


        private SqlBuilder where;
        internal SqlBuilder Where
        {
            get
            {
                if (null == where)
                {
                    where = new SqlBuilder();
                }
                return where;
            }
        }

        private SqlBuilder groupBy;
        internal SqlBuilder GroupBy
        {
            get
            {
                if (null == groupBy)
                {
                    groupBy = new SqlBuilder();
                }
                return groupBy;
            }
        }

        private SqlBuilder orderBy;
        public SqlBuilder OrderBy
        {
            get
            {
                if (null == orderBy)
                {
                    orderBy = new SqlBuilder();
                }
                return orderBy;
            }
        }

        //indicates whether it is the top most select statement, 
        // if not Order By should be omitted unless there is a corresponding TOP
        internal bool IsTopMost
        {
            get;
            set;
        }

        #region Internal Constructor
        internal SqlSelectStatement()
        {
            this.select = new SqlSelectClauseBuilder(delegate() { return this.IsTopMost; });
        }
        #endregion

        #region ISqlFragment Members

        /// <summary>
        /// Write out a SQL select statement as a string.
        /// We have to
        /// <list type="number">
        /// <item>Check whether the aliases extents we use in this statement have
        /// to be renamed.
        /// We first create a list of all the aliases used by the outer extents.
        /// For each of the FromExtents( or AllJoinExtents if it is non-null),
        /// rename it if it collides with the previous list.
        /// </item>
        /// <item>Write each of the clauses (if it exists) as a string</item>
        /// </list>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            #region Check if FROM aliases need to be renamed

            // Create a list of the aliases used by the outer extents
            // JoinSymbols have to be treated specially.
            List<string> outerExtentAliases = null;
            if ((null != outerExtents) && (0 < outerExtents.Count))
            {
                foreach (Symbol outerExtent in outerExtents.Keys)
                {
                    JoinSymbol joinSymbol = outerExtent as JoinSymbol;
                    if (joinSymbol != null)
                    {
                        foreach (Symbol symbol in joinSymbol.FlattenedExtentList)
                        {
                            if (null == outerExtentAliases) { outerExtentAliases = new List<string>(); }
                            outerExtentAliases.Add(symbol.NewName);
                        }
                    }
                    else
                    {
                        if (null == outerExtentAliases) { outerExtentAliases = new List<string>(); }
                        outerExtentAliases.Add(outerExtent.NewName);
                    }
                }
            }        

            // An then rename each of the FromExtents we have
            // If AllJoinExtents is non-null - it has precedence.
            // The new name is derived from the old name - we append an increasing int.
            List<Symbol> extentList = this.AllJoinExtents ?? this.fromExtents;
            if (null != extentList)
            {
                foreach (Symbol fromAlias in extentList)
                {
                    if ((null != outerExtentAliases) && outerExtentAliases.Contains(fromAlias.Name))
                    {
                        int i = sqlGenerator.AllExtentNames[fromAlias.Name];
                        string newName;
                        do
                        {
                            ++i;
                            newName = fromAlias.Name + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                        while (sqlGenerator.AllExtentNames.ContainsKey(newName));
                        sqlGenerator.AllExtentNames[fromAlias.Name] = i;
                        fromAlias.NewName = newName;

                        // Add extent to list of known names (although i is always incrementing, "prefix11" can
                        // eventually collide with "prefix1" when it is extended)
                        sqlGenerator.AllExtentNames[newName] = 0;
                    }

                    // Add the current alias to the list, so that the extents
                    // that follow do not collide with me.
                    if (null == outerExtentAliases) { outerExtentAliases = new List<string>(); }
                    outerExtentAliases.Add(fromAlias.NewName);
                }
            }
            #endregion

            // Increase the indent, so that the Sql statement is nested by one tab.
            writer.Indent += 1; // ++ can be confusing in this context

            this.select.WriteSql(writer, sqlGenerator);

            writer.WriteLine();
            writer.Write("FROM ");
            this.From.WriteSql(writer, sqlGenerator);

            if ((null != this.where) && !this.Where.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("WHERE ");
                this.Where.WriteSql(writer, sqlGenerator);
            }

            if ((null != this.groupBy) && !this.GroupBy.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("GROUP BY ");
                this.GroupBy.WriteSql(writer, sqlGenerator);
            }

            if ((null != this.orderBy) && !this.OrderBy.IsEmpty && (this.IsTopMost || this.Select.Top != null))
            {
                writer.WriteLine();
                writer.Write("ORDER BY ");
                this.OrderBy.WriteSql(writer, sqlGenerator);
            }

            --writer.Indent;
        }

        #endregion
    }
}
