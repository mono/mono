//------------------------------------------------------------------------------
// <copyright file="EnumRowCollectionExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Globalization;
using System.Diagnostics;

namespace System.Data
{
    /// <summary>
    /// This static class defines the extension methods that add LINQ operator functionality
    /// within IEnumerableDT and IOrderedEnumerableDT.
    /// </summary>
    public static class TypedTableBaseExtensions
    {
        /// <summary>
        /// LINQ's Where operator for generic EnumerableRowCollection.
        /// </summary>
        public static EnumerableRowCollection<TRow> Where<TRow>(
                                                this TypedTableBase<TRow> source,
                                                Func<TRow, bool> predicate) where TRow : DataRow
        {
            DataSetUtil.CheckArgumentNull(source, "source");
            EnumerableRowCollection<TRow> erc = new EnumerableRowCollection<TRow>((DataTable)source);
            return erc.Where<TRow>(predicate);
        }

        /// <summary>
        /// LINQ's OrderBy operator for generic EnumerableRowCollection.
        /// </summary>
        public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(
                                                        this TypedTableBase<TRow> source,
                                                        Func<TRow, TKey> keySelector) where TRow : DataRow
        {
            DataSetUtil.CheckArgumentNull(source, "source");
            EnumerableRowCollection<TRow> erc = new EnumerableRowCollection<TRow>((DataTable)source);
            return erc.OrderBy<TRow, TKey>(keySelector);
        }

        /// <summary>
        /// LINQ's OrderBy operator for generic EnumerableRowCollection.
        /// </summary>
        public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(
                                                        this TypedTableBase<TRow> source,
                                                        Func<TRow, TKey> keySelector,
                                                        IComparer<TKey> comparer) where TRow : DataRow
        {
            DataSetUtil.CheckArgumentNull(source, "source");
            EnumerableRowCollection<TRow> erc = new EnumerableRowCollection<TRow>((DataTable)source);
            return erc.OrderBy<TRow, TKey>(keySelector, comparer);
        }

        /// <summary>
        /// LINQ's OrderByDescending operator for generic EnumerableRowCollection.
        /// </summary>
        public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(
                                                        this TypedTableBase<TRow> source,
                                                        Func<TRow, TKey> keySelector) where TRow : DataRow
        {
            DataSetUtil.CheckArgumentNull(source, "source");
            EnumerableRowCollection<TRow> erc = new EnumerableRowCollection<TRow>((DataTable)source);
            return erc.OrderByDescending<TRow, TKey>(keySelector);
        }

        /// <summary>
        /// LINQ's OrderByDescending operator for generic EnumerableRowCollection.
        /// </summary>
        public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(
                                                        this TypedTableBase<TRow> source,
                                                        Func<TRow, TKey> keySelector,
                                                        IComparer<TKey> comparer) where TRow : DataRow
        {
            DataSetUtil.CheckArgumentNull(source, "source");
            EnumerableRowCollection<TRow> erc = new EnumerableRowCollection<TRow>((DataTable)source);
            return erc.OrderByDescending<TRow, TKey>(keySelector, comparer);
        }


        /// <summary>
        /// Executes a Select (Projection) on EnumerableDataTable. If the selector returns a different
        /// type than the type of rows, then AsLinqDataView is disabled, and the returning EnumerableDataTable
        /// represents an enumerable over the LINQ Query.
        /// </summary>
        public static EnumerableRowCollection<S> Select<TRow, S>(
                                                this TypedTableBase<TRow> source,
                                                Func<TRow, S> selector) where TRow : DataRow
        {
            DataSetUtil.CheckArgumentNull(source, "source");
            EnumerableRowCollection<TRow> erc = new EnumerableRowCollection<TRow>((DataTable)source);
            return erc.Select<TRow, S>(selector);
        }





        /// <summary>
        ///   This method returns a IEnumerable of TRow.
        /// </summary>
        /// <param name="source">
        ///   The source DataTable to make enumerable.
        /// </param>
        /// <returns>
        ///   IEnumerable of datarows.
        /// </returns>
        public static EnumerableRowCollection<TRow> AsEnumerable<TRow>(this TypedTableBase<TRow> source) where TRow : DataRow
        {
            DataSetUtil.CheckArgumentNull(source, "source");
            return new EnumerableRowCollection<TRow>(source as DataTable);
        }

        public static TRow ElementAtOrDefault<TRow>(this TypedTableBase<TRow> source, int index) where TRow : DataRow
        {
            if ((index >= 0) && (index < source.Rows.Count)) 
            {
                return (TRow)source.Rows[index];
            }
            else
            {
                return default(TRow);
            }
        }

    } //end class
}
