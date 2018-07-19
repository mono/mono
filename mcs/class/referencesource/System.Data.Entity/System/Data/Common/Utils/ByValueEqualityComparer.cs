//---------------------------------------------------------------------
// <copyright file="ByValueEqualityComparer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Common.Utils
{
    /// <summary>
    /// An implementation of IEqualityComparer&lt;object&gt; that compares byte[] instances by value, and
    /// delegates all other equality comparisons to a specified IEqualityComparer. In the default case,
    /// this provides by-value comparison for instances of the CLR equivalents of all EDM primitive types.
    /// </summary>
    internal sealed class ByValueEqualityComparer : IEqualityComparer<object>
    {
        /// <summary>
        /// Provides by-value comparison for instances of the CLR equivalents of all EDM primitive types.
        /// </summary>
        internal static readonly ByValueEqualityComparer Default = new ByValueEqualityComparer();
        
        private ByValueEqualityComparer()
        {
        }

        public new bool Equals(object x, object y)
        {
            if (object.Equals(x, y))
            {
                return true;
            }
                        
            // If x and y are both non-null byte arrays, then perform a by-value comparison
            // based on length and element values, otherwise defer to the default comparison.
            //
            byte[] xBytes = x as byte[];
            byte[] yBytes = y as byte[];
            if (xBytes != null && yBytes != null)
            {
                return CompareBinaryValues(xBytes, yBytes);
            }

            return false;
        }

        public int GetHashCode(object obj)
        {
            if (obj != null)
            {
                byte[] bytes = obj as byte[];
                if (bytes != null)
                {
                    return ComputeBinaryHashCode(bytes);
                }
            }
            else
            {
                return 0;
            }
            
            return obj.GetHashCode();
        }

        internal static int ComputeBinaryHashCode(byte[] bytes)
        {
            Debug.Assert(bytes != null, "Byte array cannot be null");
            int hashCode = 0;
            for (int i = 0, n = Math.Min(bytes.Length, 7); i < n; i++)
            {
                hashCode = ((hashCode << 5) ^ bytes[i]);
            }
            return hashCode;
        }

        internal static bool CompareBinaryValues(byte[] first, byte[] second)
        {
            Debug.Assert(first != null && second != null, "Arguments cannot be null");
            
            if (first.Length != second.Length)
            {
                return false;
            }

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Extends IComparer support to the (non-IComparable) byte[] type, based on by-value comparison.
    /// </summary>
    internal class ByValueComparer : IComparer
    {
        internal static readonly IComparer Default = new ByValueComparer(Comparer<object>.Default);

        private readonly IComparer nonByValueComparer;
        private ByValueComparer(IComparer comparer)
        {
            Debug.Assert(comparer != null, "Non-ByValue comparer cannot be null");
            this.nonByValueComparer = comparer;
        }

        int IComparer.Compare(object x, object y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }

            
            //We can convert DBNulls to nulls for the purposes of comparison.
            Debug.Assert(!((object.ReferenceEquals(x, DBNull.Value)) && (object.ReferenceEquals(y,DBNull.Value))), "object.ReferenceEquals should catch the case when both values are dbnull");
            if (object.ReferenceEquals(x, DBNull.Value))
            {
                x = null;
            }
            if (object.ReferenceEquals(y, DBNull.Value))
            {
                y = null;
            }
            
            if (x != null && y != null)
            {
                byte[] xAsBytes = x as byte[];
                byte[] yAsBytes = y as byte[];
                if (xAsBytes != null && yAsBytes != null)
                {
                    int result = xAsBytes.Length - yAsBytes.Length;
                    if (result == 0)
                    {
                        int idx = 0;
                        while (result == 0 && idx < xAsBytes.Length)
                        {
                            byte xVal = xAsBytes[idx];
                            byte yVal = yAsBytes[idx];
                            if (xVal != yVal)
                            {
                                result = xVal - yVal;
                            }
                            idx++;
                        }
                    }
                    return result;
                }
            }

            return this.nonByValueComparer.Compare(x, y);
        }
    }
}
