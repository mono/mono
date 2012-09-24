// 
// RationalThresholdTests.cs
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

using Mono.CodeContracts.Static.Analysis.Numerical;

using NUnit.Framework;

namespace Test {
        [TestFixture]
        [Timeout (1000)]
        public class RationalThresholdTests {
                // threshold contains -oo, 0, +oo

                #region Setup/Teardown

                [SetUp]
                public void SetUp ()
                {
                        this.threshold = new RationalThreshold (10);
                }

                #endregion

                RationalThreshold threshold;

                [Test]
                public void GetNext_ShouldReturnArgumentIfFound ()
                {
                        Rational next1 = this.threshold.GetNext (0);
                        Assert.That (next1, Is.EqualTo (Rational.Zero));

                        Rational next2 = this.threshold.GetNext (Rational.PlusInfinity);
                        Assert.That (next2, Is.EqualTo (Rational.PlusInfinity));
                }

                [Test]
                public void GetNext_ShouldReturnNextValueInDBIfNotFound ()
                {
                        Rational next3 = this.threshold.GetNext (Rational.MinusOne);
                        Assert.That (next3, Is.EqualTo (Rational.Zero));

                        Rational next4 = this.threshold.GetNext (Rational.One);
                        Assert.That (next4, Is.EqualTo (Rational.PlusInfinity));
                }

                [Test]
                public void GetPrevious_ShouldReturnArgumentIfFoundInDB ()
                {
                        Rational prev1 = this.threshold.GetPrevious (0);
                        Assert.That (prev1, Is.EqualTo (Rational.Zero));

                        Rational prev2 = this.threshold.GetPrevious (Rational.PlusInfinity);
                        Assert.That (prev2, Is.EqualTo (Rational.PlusInfinity));
                }

                [Test]
                public void GetPrevious_ShouldReturnPreviousValueIfNotFoundInDB ()
                {
                        Rational prev3 = this.threshold.GetPrevious (Rational.One);
                        Assert.That (prev3, Is.EqualTo (Rational.Zero));

                        Rational prev4 = this.threshold.GetPrevious (Rational.MinusOne);
                        Assert.That (prev4, Is.EqualTo (Rational.MinusInfinity));
                }
        }
}