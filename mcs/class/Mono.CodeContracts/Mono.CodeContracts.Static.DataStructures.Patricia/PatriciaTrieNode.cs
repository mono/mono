// 
// PatriciaTrieNode.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.CodeContracts.Static.DataStructures.Patricia
{
    public abstract class PatriciaTrieNode<T> : IImmutableIntMap<T>
    {
        public T this[int key]
        {
            get { return this.Lookup(key); }
        }

        public T Any { get; private set; }

        public abstract bool Contains(int key);

        public abstract int Key { get; }
        public abstract int Count { get; }
        public abstract IImmutableIntMap<T> Add(int key, T value);
        public abstract IImmutableIntMap<T> Remove(int key);

        public abstract void Visit(Action<T> action);
        public abstract void Visit(Action<int, T> action);

        public abstract T Lookup(int key);

        public IEnumerable<int> Keys
        {
            get
            {
                var list = new List<int>(this.Count);
                FillKeysTo(list);
                return list;
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                var list = new List<T>(this.Count);
                FillValuesTo(list);
                return list;
            }
        }

        public void Dump(TextWriter tw)
        {
            Dump(tw, string.Empty);
        }

        protected internal abstract void FillKeysTo(List<int> list);
        protected internal abstract void FillValuesTo(List<T> list);
        protected internal abstract void AppendToBuilder(StringBuilder sb);
        protected internal abstract void Dump(TextWriter tw, string prefix);

        public override string ToString()
        {
            var sb = new StringBuilder();
            AppendToBuilder(sb);
            return sb.ToString();
        }

        protected static IImmutableIntMap<T> Join(PatriciaTrieNode<T> left, PatriciaTrieNode<T> right)
        {
            int keyLeft = left.Key;
            int keyRight = right.Key;

            int branchingBit = BranchingBit(keyLeft, keyRight);
            var prefix = MaskWithBit(keyLeft, branchingBit);

            if (IsZeroBitAt(keyLeft, branchingBit))
                return new BranchNode<T>(prefix, branchingBit, left, right);

            return new BranchNode<T>(prefix, branchingBit, right, left);
        }

        protected static bool IsZeroBitAt(int key, int branchingBit)
        {
            return (key & branchingBit) == 0;
        }

        private static int BranchingBit(int left, int right)
        {
            return LowestBit(left ^ right);
        }

        private static int LowestBit(int x)
        {
            return x & -x;
        }

        private static int MaskWithBit(int key, int mask)
        {
            return key & (mask - 1);
        }

        protected static bool MatchPrefix(int key, int prefix, int maskBit)
        {
            return MaskWithBit(key, maskBit) == prefix;
        }
    }
}