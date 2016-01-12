// ****************************************************************
// Copyright 2010, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************

using System;

namespace NUnit.Framework.Constraints
{
    public class ToStringTests
    {
        [Test]
        public void CanDisplaySimpleConstraints_Unresolved()
        {
            Assert.That(Is.EqualTo(5).ToString(), Is.EqualTo("<equal 5>"));
            Assert.That(Has.Property("X").ToString(), Is.EqualTo("<propertyexists X>"));
            Assert.That(Has.Attribute(typeof(TestAttribute)).ToString(), 
                Is.EqualTo("<attributeexists NUnit.Framework.TestAttribute>"));
        }

        [Test]
        public void CanDisplaySimpleConstraints_Resolved()
        {
            IResolveConstraint constraint = Is.EqualTo(5);
            Assert.That(constraint.Resolve().ToString(), Is.EqualTo("<equal 5>"));
            constraint = Has.Property("X");
            Assert.That(constraint.Resolve().ToString(), Is.EqualTo("<propertyexists X>"));
            constraint = Has.Attribute(typeof(TestAttribute)).With.Property("Description").EqualTo("smoke");
            Assert.That(constraint.Resolve().ToString(),
                Is.EqualTo("<attribute NUnit.Framework.TestAttribute <property Description <equal \"smoke\">>>"));
        }

        [Test]
        public void DisplayPrefixConstraints_Unresolved()
        {
            Assert.That(Is.Not.EqualTo(5).ToString(), Is.EqualTo("<unresolved <equal 5>>"));
            Assert.That(Is.Not.All.EqualTo(5).ToString(), Is.EqualTo("<unresolved <equal 5>>"));
            Assert.That(Has.Property("X").EqualTo(5).ToString(), Is.EqualTo("<unresolved <equal 5>>"));
            Assert.That(Has.Attribute(typeof(TestAttribute)).With.Property("Description").EqualTo("smoke").ToString(),
                Is.EqualTo("<unresolved <equal \"smoke\">>"));
        }

        [Test]
        public void CanDisplayPrefixConstraints_Resolved()
        {
            IResolveConstraint constraint = Is.Not.EqualTo(5);
            Assert.That(constraint.Resolve().ToString(), Is.EqualTo("<not <equal 5>>"));
            constraint = Is.Not.All.EqualTo(5);
            Assert.That(constraint.Resolve().ToString(), Is.EqualTo("<not <all <equal 5>>>"));
            constraint = Has.Property("X").EqualTo(5);
            Assert.That(constraint.Resolve().ToString(), Is.EqualTo("<property X <equal 5>>"));
        }

        [Test]
        public void DisplayBinaryConstraints_Resolved()
        {
            IResolveConstraint constraint = Is.GreaterThan(0).And.LessThan(100);
            Assert.That(constraint.Resolve().ToString(), Is.EqualTo("<and <greaterthan 0> <lessthan 100>>"));
        }

        [Test]
        public void DisplayBinaryConstraints_UnResolved()
        {
            IResolveConstraint constraint = Is.GreaterThan(0).And.LessThan(100);
            Assert.That(constraint.ToString(), Is.EqualTo("<unresolved <lessthan 100>>"));
        }
    }
}
