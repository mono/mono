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