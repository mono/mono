// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Primitives;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition.Hosting
{
    [TestClass]
    public class CompositionElementTests
    {
        [TestMethod]
        public void Constructor_ValueAsUnderlyingObjectArgument_ShouldSetUnderlyingObjectProperty()
        {
            var expectations = Expectations.GetObjectsReferenceTypes();

            foreach (var e in expectations)
            {
                var element = new CompositionElement(e);

                Assert.AreSame(e, element.UnderlyingObject);
            }            
        }

        [TestMethod]
        public void Constructor_ValueAsUnderlyingObjectArgument_ShouldSetDisplayNamePropertyToUnderlyingObjectToString()
        {
            var expectations = Expectations.GetObjectsReferenceTypes();

            foreach (var e in expectations)
            {
                var element = new CompositionElement(e);

                Assert.AreEqual(e.ToString(), element.DisplayName);
            }
        }

        [TestMethod]
        public void Constructor_ValueAsUnderlyingObjectArgument_ShouldSetOriginToUnknown()
        {
            var expectations = Expectations.GetObjectsReferenceTypes();

            foreach (var e in expectations)
            {
                var element = new CompositionElement(e);

                Assert.IsNotNull(element.Origin);
                Assert.IsNull(element.Origin.Origin);
            }
        }

        [TestMethod]
        public void ToString_ShouldReturnDisplayNameProperty()
        {
            var expectations = Expectations.GetObjectsReferenceTypes();

            foreach (var e in expectations)
            {
                var element = CreateCompositionElement(e);

                Assert.AreEqual(element.DisplayName, element.ToString());
            }
        }

        private static CompositionElement CreateCompositionElement(object underlyingObject)
        {
            return new CompositionElement(underlyingObject);
        }
   }
}