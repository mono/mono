using Mono.CodeContracts.Static.Analysis.Numerical;

using NUnit.Framework;

namespace MonoTests.Mono.CodeContracts {
        static class RationalTestExtensions {
                public static void ShouldBeLessEqualThan (this Rational l, Rational r)
                {
                        Assert.IsTrue (l <= r);
                }

                public static void ShouldNotBeLessEqualThan (this Rational l, Rational r)
                {
                        Assert.IsFalse (l <= r);
                }

                public static void ShouldBeLessThan (this Rational l, Rational r)
                {
                        Assert.IsTrue (l < r);
                }

                public static void ShouldNotBeLessThan (this Rational l, Rational r)
                {
                        Assert.IsFalse (l < r);
                }
        }
}