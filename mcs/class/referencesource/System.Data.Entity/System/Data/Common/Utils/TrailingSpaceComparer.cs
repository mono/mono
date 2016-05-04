//---------------------------------------------------------------------
// <copyright file="TrailingSpaceComparer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
namespace System.Data.Common.Utils
{
    /// <summary>
    /// Comparer that treats two strings as equivalent if they differ only by trailing
    /// spaces, e.g. 'A' eq 'A   '. Useful when determining if a set of values is unique
    /// even given the possibility of padding (consider SQL Server char and nchar columns)
    /// or to lookup values when the set of values is known to honor this uniqueness constraint.
    /// </summary>
    internal class TrailingSpaceComparer : IEqualityComparer<object>
    {
        private TrailingSpaceComparer() { }
        internal readonly static TrailingSpaceComparer Instance = new TrailingSpaceComparer();
        private readonly static IEqualityComparer<object> s_template = EqualityComparer<object>.Default;
        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            string xAsString = x as string;
            if (null != xAsString)
            {
                string yAsString = y as string;
                if (null != yAsString)
                {
                    return TrailingSpaceStringComparer.Instance.Equals(xAsString, yAsString);
                }
            }
            return s_template.Equals(x, y);
        }
        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            string value = obj as string;
            if (null != value)
            {
                return TrailingSpaceStringComparer.Instance.GetHashCode(value);
            }
            return s_template.GetHashCode(obj);
        }
    }

    /// <summary>
    /// Typed version of TrailingSpaceComparer.
    /// </summary>
    internal class TrailingSpaceStringComparer : IEqualityComparer<string>
    {
        internal static readonly TrailingSpaceStringComparer Instance = new TrailingSpaceStringComparer();
        private TrailingSpaceStringComparer() { }
        public bool Equals(string x, string y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(NormalizeString(x), NormalizeString(y));
        }
        public int GetHashCode(string obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizeString(obj));
        }
        internal static string NormalizeString(string value)
        {
            if (null == value || !value.EndsWith(" ", StringComparison.Ordinal)) { return value; }
            else { return value.TrimEnd(' '); }
        }
    }
}
