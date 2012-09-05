// 
// PatriciaTrieTests.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
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

using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.DataStructures.Patricia;

using NUnit.Framework;

namespace MonoTests.Mono.CodeContracts {
        [TestFixture]
        public class PatriciaTrieTests {
                readonly IImmutableIntMap<string> empty = ImmutableIntMap<string>.Empty;

                [Test]
                public void AddOnEmptyShouldCreateLeafNode ()
                {
                        var one = this.empty.Add (1, "hello") as LeafNode<string>;

                        Assert.That (one.Key, Is.EqualTo (1));
                        Assert.That (one.Value, Is.EqualTo ("hello"));
                        Assert.That (one.Count, Is.EqualTo (1));
                }

                [Test]
                public void AddOnLeafShouldCreateAnotherLeafIfKeysAreEqual ()
                {
                        var leaf = new LeafNode<string> (1, "hello");

                        var result = (LeafNode<string>) leaf.Add (1, "there");

                        Assert.That (result.Key, Is.EqualTo (1));
                        Assert.That (result.Value, Is.EqualTo ("there"));
                        Assert.That (result.Count, Is.EqualTo (1));
                }

                [Test]
                public void AddOnLeafShouldCreateBranchIfKeysAreDifferent ()
                {
                        var leaf = new LeafNode<string> (5, "hello"); //101

                        var branch = (BranchNode<string>) leaf.Add (7, "there"); //111

                        Assert.That (branch.Prefix, Is.EqualTo (1)); //x*1
                        Assert.That (branch.BranchingBit, Is.EqualTo (2)); //
                        Assert.That (branch.Left, Is.SameAs (leaf));

                        var right = (branch.Right as LeafNode<string>);
                        Assert.That (right, Is.Not.Null);
                        Assert.That (right.Key, Is.EqualTo (7));
                        Assert.That (right.Value, Is.EqualTo ("there"));
                }

                [Test]
                public void RemoveFromBranchWithLeafKeyEqualToArgumentShouldStayAnotherChild ()
                {
                        IImmutableIntMap<string> left = this.empty.Add (5, "hello");
                        IImmutableIntMap<string> branch = left.Add (7, "there");

                        IImmutableIntMap<string> node1 = branch.Remove (7);
                        Assert.That (node1, Is.SameAs (left));

                        IImmutableIntMap<string> node2 = branch.Remove (5);
                        Assert.That (node2 is LeafNode<string>);
                        Assert.That ((node2 as LeafNode<string>).Key, Is.EqualTo (7));
                        Assert.That ((node2 as LeafNode<string>).Value, Is.EqualTo ("there"));
                }

                [Test]
                public void RemoveOnEmptyShouldStayEmpty ()
                {
                        var one = (EmptyNode<string>) this.empty.Remove (1);

                        Assert.That (one, Is.SameAs (this.empty));
                }

                [Test]
                public void RemoveOnLeafShouldCreateEmptyIfKeysAreEqual ()
                {
                        var leaf = new LeafNode<string> (1, "hello");

                        var result = (EmptyNode<string>) leaf.Remove (1);

                        Assert.That (result.Count, Is.EqualTo (0));
                }

                [Test]
                public void RemoveOnLeafShouldStayTheSameIfKeysAreDifferent ()
                {
                        var leaf = new LeafNode<string> (1, "hello");

                        var result = (LeafNode<string>) leaf.Remove (2);

                        Assert.That (result, Is.SameAs (leaf));
                }
        }
}