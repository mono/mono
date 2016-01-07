// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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
// ***********************************************************************

using System;

namespace NUnit.Framework.Attributes
{
    [TestFixture]
    public class CombinatorialTests
    {
        [Test]
        public void SingleArgument(
            [Values(1.3, 1.7, 1.5)] double x)
        {
            Assert.That(x > 1.0 && x < 2.0);
        }

        [Test, Combinatorial]
        public void TwoArguments_Combinatorial(
            [Values(1, 2, 3)] int x,
            [Values(10, 20)] int y)
        {
            Assert.That(x > 0 && x < 4 && y % 10 == 0);
        }

        [Test, Sequential]
        public void TwoArguments_Sequential(
            [Values(1, 2, 3)] int x,
            [Values(10, 20)] int y)
        {
            Assert.That(x > 0 && x < 4 && y % 10 == 0);
        }

        [Test, Combinatorial]
        public void ThreeArguments_Combinatorial(
            [Values(1, 2, 3)] int x,
            [Values(10, 20)] int y,
            [Values("Charlie", "Joe", "Frank")] string name)
        {
            Assert.That(x > 0 && x < 4 && y % 10 == 0);
            Assert.That(name.Length >= 2);
        }

        [Test, Sequential]
        public void ThreeArguments_Sequential(
            [Values(1, 2, 3)] int x,
            [Values(10, 20)] int y,
            [Values("Charlie", "Joe", "Frank")] string name)
        {
            Assert.That(x > 0 && x < 4 && y % 10 == 0);
            Assert.That(name.Length >= 2);
        }

        [Test]
        public void RangeTest(
            [Range(0.2, 0.6, 0.2)] double a,
            [Range(10, 20, 5)] int b)
        {
        }
        
        [Test, Sequential]
        public void RandomTest(
            [Random(32, 212, 5)] int x,
            [Random(5)] double y,
            [Random(5)] AttributeTargets z)
        {
            Assert.That(x,Is.InRange(32,212));
            Assert.That(y,Is.InRange(0.0,1.0));
            Assert.That(z, Is.TypeOf(typeof(AttributeTargets)));
        }

        [Test, Sequential]
        public void RandomArgsAreIndependent(
            [Random(1)] double x,
            [Random(1)] double y)
        {
            Assert.AreNotEqual(x, y);
        }
    }
}
