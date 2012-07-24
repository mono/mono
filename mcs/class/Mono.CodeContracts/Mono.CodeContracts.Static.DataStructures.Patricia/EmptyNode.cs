using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.CodeContracts.Static.DataStructures.Patricia
{
    class EmptyNode<T> : PatriciaTrieNode<T>
    {
        public static readonly EmptyNode<T> Instance = new EmptyNode<T>();

        public override bool Contains(int key)
        {
            return false;
        }

        public override int Key { get { throw new NotSupportedException("No key for empty node");} }

        public override int Count
        {
            get { return 0; }
        }

        public override IImmutableIntMap<T> Add(int key, T value)
        {
            return new LeafNode<T>(key, value);
        }

        public override IImmutableIntMap<T> Remove(int key)
        {
            return this;
        }

        public override void Visit(Action<T> action)
        {
        }

        public override void Visit(Action<int, T> action)
        {
        }

        protected internal override void FillKeysTo(List<int> list)
        {
        }

        protected internal override void FillValuesTo(List<T> list)
        {
        }

        protected internal override void AppendToBuilder(StringBuilder sb)
        {
            sb.Append("*");
        }

        protected internal override void Dump(TextWriter tw, string prefix)
        {
            tw.WriteLine(prefix + "<empty/>");
        }

        public override T Lookup(int key)
        {
            return default(T);
        }
    }
}