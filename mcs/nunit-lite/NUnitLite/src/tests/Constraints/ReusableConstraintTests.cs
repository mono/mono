// ****************************************************************
// Copyright 2012, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************

using System;

namespace NUnit.Framework.Constraints
{
    [TestFixture]
    public class ReusableConstraintTests
    {
        [Datapoints]
        internal static readonly ReusableConstraint[] constraints = new ReusableConstraint[] {
            Is.Not.Empty,
            Is.Not.Null,
            Has.Length.GreaterThan(3),
            Has.Property("Length").EqualTo(4).And.StartsWith("te")
        };

        [Theory]
        public void CanReuseReusableConstraintMultipleTimes(ReusableConstraint c)
        {
            string s = "test";

            Assume.That(s, c);

            Assert.That(s, c, "Should pass first time");
            Assert.That(s, c, "Should pass second time");
            Assert.That(s, c, "Should pass third time");
        }

        [Test]
        public void CanCreateReusableConstraintByImplicitConversion()
        {
            ReusableConstraint c = Is.Not.Null;

            string s = "test";
            Assert.That(s, c, "Should pass first time");
            Assert.That(s, c, "Should pass second time");
            Assert.That(s, c, "Should pass third time");
        }
    }
}
