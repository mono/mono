// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !CLR40
using System;

namespace System
{
    // This is a very minimalistic implementation of Tuple'2 that allows us
    // to compile and work on versions of .Net eariler then 4.0.
    public struct Tuple<TItem1, TItem2>
    {
        public Tuple(TItem1 item1, TItem2 item2)
        {
            this = new Tuple<TItem1, TItem2>();
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public TItem1 Item1 { get; private set; }
        public TItem2 Item2 { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is Tuple<TItem1, TItem2>)
            {
                Tuple<TItem1, TItem2> that = (Tuple<TItem1, TItem2>)obj;
                return object.Equals(this.Item1, that.Item1) && object.Equals(this.Item2, that.Item2);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return ((this.Item1 != null) ? this.Item1.GetHashCode() : 0) ^ ((this.Item2 != null) ? this.Item2.GetHashCode() : 0);
        }

        public static bool operator ==(Tuple<TItem1, TItem2> left, Tuple<TItem1, TItem2> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Tuple<TItem1, TItem2> left, Tuple<TItem1, TItem2> right)
        {
            return !left.Equals(right);
        }
    }
}
#endif