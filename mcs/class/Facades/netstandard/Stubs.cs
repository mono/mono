// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if MOBILE || XAMMAC_4_5
namespace System.Data
{
    public static partial class DataRowComparer
    {
        public static System.Data.DataRowComparer<System.Data.DataRow> Default { get => throw new PlatformNotSupportedException(); }
    }
    public sealed partial class DataRowComparer<TRow> : System.Collections.Generic.IEqualityComparer<TRow> where TRow : System.Data.DataRow
    {
        internal DataRowComparer() { }
        public static System.Data.DataRowComparer<TRow> Default { get => throw new PlatformNotSupportedException(); }
        public bool Equals(TRow leftRow, TRow rightRow) => throw new PlatformNotSupportedException();
        public int GetHashCode(TRow row) => throw new PlatformNotSupportedException();
    }
    public static partial class DataRowExtensions
    {
        public static T Field<T>(this System.Data.DataRow row, System.Data.DataColumn column) => throw new PlatformNotSupportedException();
        public static T Field<T>(this System.Data.DataRow row, System.Data.DataColumn column, System.Data.DataRowVersion version) => throw new PlatformNotSupportedException();
        public static T Field<T>(this System.Data.DataRow row, int columnIndex) => throw new PlatformNotSupportedException();
        public static T Field<T>(this System.Data.DataRow row, int columnIndex, System.Data.DataRowVersion version) => throw new PlatformNotSupportedException();
        public static T Field<T>(this System.Data.DataRow row, string columnName) => throw new PlatformNotSupportedException();
        public static T Field<T>(this System.Data.DataRow row, string columnName, System.Data.DataRowVersion version) => throw new PlatformNotSupportedException();
        public static void SetField<T>(this System.Data.DataRow row, System.Data.DataColumn column, T value) { }
        public static void SetField<T>(this System.Data.DataRow row, int columnIndex, T value) { }
        public static void SetField<T>(this System.Data.DataRow row, string columnName, T value) { }
    }
    public static partial class DataTableExtensions
    {
        public static System.Data.DataView AsDataView(this System.Data.DataTable table) => throw new PlatformNotSupportedException();
        public static System.Data.DataView AsDataView<T>(this System.Data.EnumerableRowCollection<T> source) where T : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static System.Data.EnumerableRowCollection<System.Data.DataRow> AsEnumerable(this System.Data.DataTable source) => throw new PlatformNotSupportedException();
        public static System.Data.DataTable CopyToDataTable<T>(this System.Collections.Generic.IEnumerable<T> source) where T : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static void CopyToDataTable<T>(this System.Collections.Generic.IEnumerable<T> source, System.Data.DataTable table, System.Data.LoadOption options) where T : System.Data.DataRow { }
        public static void CopyToDataTable<T>(this System.Collections.Generic.IEnumerable<T> source, System.Data.DataTable table, System.Data.LoadOption options, System.Data.FillErrorEventHandler errorHandler) where T : System.Data.DataRow { }
    }
    public abstract partial class EnumerableRowCollection : System.Collections.IEnumerable
    {
        internal EnumerableRowCollection() { }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new PlatformNotSupportedException();
    }
    public static partial class EnumerableRowCollectionExtensions
    {
        public static System.Data.EnumerableRowCollection<TResult> Cast<TResult>(this System.Data.EnumerableRowCollection source) => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(this System.Data.EnumerableRowCollection<TRow> source, System.Func<TRow, TKey> keySelector) => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(this System.Data.EnumerableRowCollection<TRow> source, System.Func<TRow, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(this System.Data.EnumerableRowCollection<TRow> source, System.Func<TRow, TKey> keySelector) => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(this System.Data.EnumerableRowCollection<TRow> source, System.Func<TRow, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) => throw new PlatformNotSupportedException();
        public static System.Data.EnumerableRowCollection<S> Select<TRow, S>(this System.Data.EnumerableRowCollection<TRow> source, System.Func<TRow, S> selector) => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> ThenByDescending<TRow, TKey>(this System.Data.OrderedEnumerableRowCollection<TRow> source, System.Func<TRow, TKey> keySelector) => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> ThenByDescending<TRow, TKey>(this System.Data.OrderedEnumerableRowCollection<TRow> source, System.Func<TRow, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> ThenBy<TRow, TKey>(this System.Data.OrderedEnumerableRowCollection<TRow> source, System.Func<TRow, TKey> keySelector) => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> ThenBy<TRow, TKey>(this System.Data.OrderedEnumerableRowCollection<TRow> source, System.Func<TRow, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) => throw new PlatformNotSupportedException();
        public static System.Data.EnumerableRowCollection<TRow> Where<TRow>(this System.Data.EnumerableRowCollection<TRow> source, System.Func<TRow, bool> predicate) => throw new PlatformNotSupportedException();
    }
    public partial class EnumerableRowCollection<TRow> : System.Data.EnumerableRowCollection, System.Collections.Generic.IEnumerable<TRow>, System.Collections.IEnumerable
    {
        internal EnumerableRowCollection() { }
        public System.Collections.Generic.IEnumerator<TRow> GetEnumerator() => throw new PlatformNotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new PlatformNotSupportedException();
    }
    public sealed partial class OrderedEnumerableRowCollection<TRow> : System.Data.EnumerableRowCollection<TRow>
    {
        internal OrderedEnumerableRowCollection() { }
    }
    public static partial class TypedTableBaseExtensions
    {
        public static System.Data.EnumerableRowCollection<TRow> AsEnumerable<TRow>(this System.Data.TypedTableBase<TRow> source) where TRow : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static TRow ElementAtOrDefault<TRow>(this System.Data.TypedTableBase<TRow> source, int index) where TRow : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(this System.Data.TypedTableBase<TRow> source, System.Func<TRow, TKey> keySelector) where TRow : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey>(this System.Data.TypedTableBase<TRow> source, System.Func<TRow, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) where TRow : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(this System.Data.TypedTableBase<TRow> source, System.Func<TRow, TKey> keySelector) where TRow : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static System.Data.OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey>(this System.Data.TypedTableBase<TRow> source, System.Func<TRow, TKey> keySelector, System.Collections.Generic.IComparer<TKey> comparer) where TRow : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static System.Data.EnumerableRowCollection<S> Select<TRow, S>(this System.Data.TypedTableBase<TRow> source, System.Func<TRow, S> selector) where TRow : System.Data.DataRow => throw new PlatformNotSupportedException();
        public static System.Data.EnumerableRowCollection<TRow> Where<TRow>(this System.Data.TypedTableBase<TRow> source, System.Func<TRow, bool> predicate) where TRow : System.Data.DataRow => throw new PlatformNotSupportedException();
    }
    public abstract partial class TypedTableBase<T> : System.Data.DataTable, System.Collections.Generic.IEnumerable<T>, System.Collections.IEnumerable where T : System.Data.DataRow
    {
        protected TypedTableBase() { }
        protected TypedTableBase(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        public System.Data.EnumerableRowCollection<TResult> Cast<TResult>() => throw new PlatformNotSupportedException();
        public System.Collections.Generic.IEnumerator<T> GetEnumerator() => throw new PlatformNotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new PlatformNotSupportedException();
    }
}
#endif