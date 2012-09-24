// 
// SequenceTests.cs
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

using NUnit.Framework;

namespace MonoTests.Mono.CodeContracts {
        [TestFixture]
        public class SequenceTests {
                readonly Sequence<int> empty = Sequence<int>.Empty;

                [Test]
                public void FromListShouldCreateSequenceInOrder ()
                {
                        Sequence<int> seq = Sequence<int>.From (1, 2, 3);

                        Assert.That (seq.Length (), Is.EqualTo (3));
                        Assert.That (seq.Head, Is.EqualTo (1));
                        Assert.That (seq.Tail.Head, Is.EqualTo (2));
                        Assert.That (seq.Tail.Tail.Head, Is.EqualTo (3));
                }

                [Test]
                public void MultipleConsAreInsertedAsAStack ()
                {
                        Sequence<int> seq = this.empty.Cons (1).Cons (2).Cons (3);

                        Assert.That (seq.Head, Is.EqualTo (3));
                        Assert.That (seq.Tail.Head, Is.EqualTo (2));
                        Assert.That (seq.Tail.Tail.Head, Is.EqualTo (1));
                }

                [Test]
                public void ReverseShouldReverse ()
                {
                        Sequence<int> seq = Sequence<int>.From (1, 2);
                        Sequence<int> reversed = seq.Reverse ();

                        Assert.That (reversed.Length (), Is.EqualTo (2));
                        Assert.That (reversed.Head, Is.EqualTo (2));
                        Assert.That (reversed.Tail.Head, Is.EqualTo (1));
                }

                [Test]
                public void ShouldHasCountEq1WithOneElement ()
                {
                        Sequence<int> seq = Sequence<int>.From (5);

                        Assert.That (seq.Length (), Is.EqualTo (1));
                        Assert.That (seq.Head, Is.EqualTo (5));
                        Assert.That (seq.Tail, Is.Null);
                }
        }
}