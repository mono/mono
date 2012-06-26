using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.CodeContracts.Static.DataStructures
{
    internal class BranchNode<T> : PatriciaTrieNode<T>
    {
        private readonly int count;

        public readonly int Prefix;
        public readonly int BranchingBit;
        public readonly PatriciaTrieNode<T> Left;
        public readonly PatriciaTrieNode<T> Right;

        public override int Key { get { return Prefix; } }
        public override int Count { get { return count; } }

        public BranchNode(int prefix, int branchingBit, PatriciaTrieNode<T> left, PatriciaTrieNode<T> right)
        {
            Prefix = prefix;
            BranchingBit = branchingBit;
            Left = left;
            Right = right;
            
            count = left.Count + right.Count;
        }

        public override bool Contains(int key)
        {
            if (!MatchPrefix(key, Prefix, BranchingBit))
                return false;

            var child = IsZeroBitAt(key, BranchingBit) ? Left : Right;

            return child.Contains(key);
        }

        public override IImmutableIntMap<T> Add(int key, T value)
        {
            if (!MatchPrefix(key, Prefix, BranchingBit))
                return Join(new LeafNode<T>(key, value), this);

            if (IsZeroBitAt(key, BranchingBit))
                return new BranchNode<T>(Prefix, BranchingBit, (PatriciaTrieNode<T>)Left.Add(key, value), Right);
            
            return new BranchNode<T>(Prefix, BranchingBit, Left, (PatriciaTrieNode<T>)Right.Add(key, value));
        }

        public override IImmutableIntMap<T> Remove(int key)
        {
            var left = Left;
            var right = Right;
            if (IsZeroBitAt(key, BranchingBit))
            {
                left = (PatriciaTrieNode<T>)left.Remove(key);
                if (left.Count == 0)
                    return right;
            } 
            else 
            {
                right = (PatriciaTrieNode<T>)right.Remove(key);
                if (right.Count == 0)
                    return left;
            }

            return Join(left, right);
        }

        public override void Visit(Action<T> action)
        {
            Left.Visit(action);
            Right.Visit(action);
        }

        public override void Visit(Action<int, T> action)
        {
            Left.Visit(action);
            Right.Visit(action);
        }

        public override T Lookup(int key)
        {
            BranchNode<T> current = this;

            PatriciaTrieNode<T> child;
            do
            {
                child = IsZeroBitAt(key, current.BranchingBit) ? current.Left : current.Right;
                current = child as BranchNode<T>;
            }
            while (current != null);

            return child.Lookup(key);
        }

        protected internal override void FillKeysTo(List<int> list)
        {
            Left.FillKeysTo(list);
            Right.FillKeysTo(list);
        }

        protected internal override void FillValuesTo(List<T> list)
        {
            Left.FillValuesTo(list);
            Right.FillValuesTo(list);
        }

        protected internal override void AppendToBuilder(StringBuilder sb)
        {
            Left.AppendToBuilder(sb);
            Right.AppendToBuilder(sb);
        }

        protected internal override void Dump(TextWriter tw, string prefix)
        {
            tw.WriteLine(prefix + "<Branch Prefix={0} BranchingBit={1}>", Prefix, BranchingBit);
            Left.Dump(tw, prefix + "  ");
            Right.Dump(tw, prefix + "  ");
            tw.WriteLine(prefix + "</Branch>");
        }
    }
}