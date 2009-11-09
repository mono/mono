// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Factories;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.UnitTesting;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition.Hosting
{
    [TestClass]
    public class SerializableCompositionElementTests
    {
        [TestMethod]
        public void FromICompositionElement_NullAsElementArgument_ShouldReturnNull()
        {
            var result = SerializableCompositionElement.FromICompositionElement((ICompositionElement)null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FromICompositionElement_ValueWithNullDisplayNameAsElementArgument_ShouldSetDisplayNamePropertyToEmptyString()
        {
            var element = ElementFactory.Create((string)null);
            Assert.IsNull(element.DisplayName);

            var result = SerializableCompositionElement.FromICompositionElement(element);

            Assert.AreEqual(string.Empty, result.DisplayName);
        }

        [TestMethod]
        public void FromICompositionElement_ValueAsElementArgument_ShouldSetDisplayNameProperty()
        {
            var expectations = Expectations.GetDisplayNames();

            foreach (var e in expectations)
            {
                var element = ElementFactory.Create(e);

                var result = SerializableCompositionElement.FromICompositionElement(element);

                Assert.AreEqual(e, result.DisplayName);
            }
        }

        [TestMethod]
        public void FormICompositionElement_ValueWithNullOriginAsElementArgument_ShouldSetOriginPropertyToNull()
        {
            var element = ElementFactory.Create((ICompositionElement)null);
            Assert.IsNull(element.Origin);

            var result = SerializableCompositionElement.FromICompositionElement(element);

            Assert.IsNull(element.Origin);
        }

        [TestMethod]
        public void FromICompositionElement_ValueAsElementArgument_ShouldSetOriginProperty()
        {
            var expectations = Expectations.GetCompositionElements();

            foreach (var e in expectations)
            {
                var element = ElementFactory.Create(e);

                var result = SerializableCompositionElement.FromICompositionElement(element);

                ElementAssert.AreEqual(e, result.Origin);
            }
        }

        [TestMethod]
        public void ToString_ShouldReturnDisplayNameProperty()
        {
            var expectations = Expectations.GetDisplayNames();

            foreach (var e in expectations)
            {
                var element = ElementFactory.Create(e);

                var result = SerializableCompositionElement.FromICompositionElement(element);

                Assert.AreEqual(e, result.ToString());
            }
        }

#if !SILVERLIGHT

        [TestMethod]
        public void Origin_CanBeSerialized()
        {
            var expectations = Expectations.GetCompositionElements();

            foreach (var e in expectations)
            {
                var element = CreateSerializableCompositionElement(e);

                var result = SerializationTestServices.RoundTrip(element);

                ElementAssert.AreEqual(e, result);
            }
        }

        [TestMethod]
        public void DisplayName_CanBeSerialized()
        {
            var expectations = Expectations.GetDisplayNames();

            foreach (var e in expectations)
            {
                var element = CreateSerializableCompositionElement(e);

                var result = SerializationTestServices.RoundTrip(element);

                Assert.AreEqual(e, result.DisplayName);
            }
        }

#endif

        private static SerializableCompositionElement CreateSerializableCompositionElement(string displayName)
        {
            var element = ElementFactory.Create(displayName);

            return CreateSerializableCompositionElement(element);
        }

        private static SerializableCompositionElement CreateSerializableCompositionElement(ICompositionElement element)
        {
            return (SerializableCompositionElement)SerializableCompositionElement.FromICompositionElement(element);
        }
    }
}