// 
// LeafNode.cs
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
        public class LeafNode<T> : PatriciaTrieNode<T>
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