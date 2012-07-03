#region Copyright Header
// // 
// // SequenceTests.cs.cs
// // 
// // Authors:
// // 	Alexander Chebaturkin (chebaturkin@gmail.com)
// // 
// // Copyright (C) 2012 Alexander Chebaturkin
// // 
// // Permission is hereby granted, free of charge, to any person obtaining
// // a copy of this software and associated documentation files (the
// // "Software"), to deal in the Software without restriction, including
// // without limitation the rights to use, copy, modify, merge, publish,
// // distribute, sublicense, and/or sell copies of the Software, and to
// // permit persons to whom the Software is furnished to do so, subject to
// // the following conditions:
// // 
// // The above copyright notice and this permission notice shall be
// // included in all copies or substantial portions of the Software.
// //  
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// // 
#endregion

using Mono.CodeContracts.Static.DataStructures;

using NUnit.Framework;

namespace MonoTests.Mono.CodeContracts
{
    [TestFixture]
    public class SequenceTests
    {
        private readonly Sequence<int> empty = Sequence<int>.Empty;

        [Test]
        public void ShouldHasCountEq1WithOneElement()
        {
            var list = empty.Cons (5);

            Assert.That(list.Length(), Is.EqualTo(1));
            Assert.That(list.Head, Is.EqualTo(5));
            Assert.That(list.Tail, Is.Null);
        }

        [Test]
        public void FromListShouldCreateSequenceInOrder()
        {
            var list = Sequence<int>.From (1, 2, 3);

            Assert.That (list.Length(), Is.EqualTo (3));
            Assert.That (list.Head, Is.EqualTo (1));
            Assert.That (list.Tail.Head, Is.EqualTo (2));
            Assert.That (list.Tail.Tail.Head, Is.EqualTo (3));
        }

        [Test]
        public void MultipleConsAreInsertedFirst ()
        {
            var list = empty.Cons (1).Cons (2).Cons (3);
            
            Assert.That (list.Head, Is.EqualTo (3));
            Assert.That (list.Tail.Head, Is.EqualTo (2));
            Assert.That (list.Tail.Tail.Head, Is.EqualTo (1));
        }

        [Test]
        public void ReverseShouldReverse ()
        {
            var list = Sequence<int>.From (1, 2);
            var reversed = list.Reverse ();

            Assert.That (reversed.Length(), Is.EqualTo (2));
            Assert.That (reversed.Head, Is.EqualTo (2));
            Assert.That (reversed.Tail.Head, Is.EqualTo (1));
        }
    }
}