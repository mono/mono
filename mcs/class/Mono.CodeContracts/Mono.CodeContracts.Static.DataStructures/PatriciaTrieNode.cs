using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.CodeContracts.Static.DataStructures
{
    abstract class PatriciaTrieNode<T> : IImmutableIntMap<T>
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