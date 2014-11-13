//---------------------------------------------------------------------
// <copyright file="Pair.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;


namespace System.Data.Common.Utils
{
    internal class Pair<TFirst, TSecond> : InternalBase
    {
        #region Fields
        private readonly TFirst first;
        private readonly TSecond second;

        #endregion

        #region Constructor
        internal Pair(TFirst first, TSecond second)
        {
            this.first = first;
            this.second = second;
        }
        #endregion

        #region Properties
        internal TFirst First
        {
            get
            {
                return first;
            }
        }

        internal TSecond Second
        {
            get
            {
                return second;
            }
        }
        #endregion 

        #region Methods
        public override int GetHashCode()
        {
            return (first.GetHashCode()<<5) ^ second.GetHashCode();
        }

        public bool Equals(Pair<TFirst, TSecond> other)
        {
            return first.Equals(other.first) && second.Equals(other.second);
        }

        public override bool Equals(object other)
        {
            Pair<TFirst, TSecond> otherPair = other as Pair<TFirst, TSecond>;

            return (otherPair != null && Equals(otherPair));
        }
        #endregion

        #region InternalBase
        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append("<");
            builder.Append(first.ToString());
            builder.Append(", "+second.ToString());
            builder.Append(">");
        }
        #endregion


        internal class PairComparer : IEqualityComparer<Pair<TFirst, TSecond>>
        {
            private PairComparer() { }

            internal static readonly PairComparer Instance = new PairComparer();
            private static readonly EqualityComparer<TFirst> firstComparer = EqualityComparer<TFirst>.Default;
            private static readonly EqualityComparer<TSecond> secondComparer = EqualityComparer<TSecond>.Default;

            public bool Equals(Pair<TFirst, TSecond> x, Pair<TFirst, TSecond> y)
            {
                return firstComparer.Equals(x.First, y.First) && secondComparer.Equals(x.Second, y.Second);
            }

            public int GetHashCode(Pair<TFirst, TSecond> source)
            {
                return source.GetHashCode();
            }
        }
    }

    

}
