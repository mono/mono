using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.CodeContracts.Static.DataStructures
{
    internal class LeafNode<T> : PatriciaTrieNode<T>
    {
        private readonly int _key;

        public override bool Contains(int key)
        {
            return key == _key;
        }

        public override int Key { get { return _key; } }
        public override int Count { get { return 1; } }
        public T Value { get; private set; }

        public LeafNode(int key, T value)
        {
            _key = key;
            Value = value;
        }

        public override IImmutableIntMap<T> Add(int key, T value)
        {
            if (key == Key)
                return new LeafNode<T>(key, value);

            return Join(new LeafNode<T>(key, value), this);
        }

        public override IImmutableIntMap<T> Remove(int key)
        {
            if (key == this.Key)
                return EmptyNode<T>.Instance;

            return this;
        }

        public override void Visit(Action<T> action)
        {
            action(Value);
        }

        public override void Visit(Action<int, T> action)
        {
            action(Key, Value);
        }

        protected internal override void FillKeysTo(List<int> list)
        {
            list.Add(_key);
        }

        protected internal override void FillValuesTo(List<T> list)
        {
            list.Add(Value);
        }

        protected internal override void AppendToBuilder(StringBuilder sb)
        {
            sb.AppendFormat("{0}->'{1}' ", _key, Value);
        }

        protected internal override void Dump(TextWriter tw, string prefix)
        {
            tw.WriteLine(prefix + "<Leaf Key={0} Value='{1}' />", _key, Value);
        }

        public override T Lookup(int key)
        {
            return key == _key ? Value : default(T);
        }
    }
}