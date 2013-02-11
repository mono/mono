// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

// https://silverlight.svn.codeplex.com/svn/Release/Silverlight4/Source/RiaClient.Tests/System.ComponentModel.DataAnnotations/FilterUIHintAttributeTest.cs

#if NET_4_0

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
    [TestFixture]
    public class FilterUIHintAttributeTest {
        [Test]
        [Description("Simple ctors set expected properties.")]
        public void FilterUIHintAttribute_Simple_Ctors_Set_Properties() {
            var attr = new FilterUIHintAttribute(null, null);
            Assert.IsNull(attr.FilterUIHint);
            Assert.IsNull(attr.PresentationLayer);
            Assert.AreEqual(0, attr.ControlParameters.Count);

            attr = new FilterUIHintAttribute(string.Empty, string.Empty);
            Assert.AreEqual(string.Empty, attr.FilterUIHint);
            Assert.AreEqual(string.Empty, attr.PresentationLayer);
            Assert.AreEqual(0, attr.ControlParameters.Count);

            attr = new FilterUIHintAttribute("theHint");
            Assert.AreEqual("theHint", attr.FilterUIHint);
            Assert.IsNull(attr.PresentationLayer);
            Assert.AreEqual(0, attr.ControlParameters.Count);

            attr = new FilterUIHintAttribute("theHint", "theLayer");
            Assert.AreEqual("theHint", attr.FilterUIHint);
            Assert.AreEqual("theLayer", attr.PresentationLayer);
            Assert.AreEqual(0, attr.ControlParameters.Count);
        }

        [Test]
        public void ConstructorControlParameters() {
            Assert.AreEqual(2, new FilterUIHintAttribute("", "", "a", 1, "b", 2).ControlParameters.Keys.Count);
        }

        [Test]
        public void ConstructorControlParameters_NoParams() {
            Assert.AreEqual(0, new FilterUIHintAttribute("", "", new object[0]).ControlParameters.Keys.Count);
            Assert.AreEqual(0, new FilterUIHintAttribute("", "", (object[])null).ControlParameters.Keys.Count);
            Assert.AreEqual(0, new FilterUIHintAttribute("", "").ControlParameters.Keys.Count);
            Assert.AreEqual(0, new FilterUIHintAttribute("").ControlParameters.Keys.Count);
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ConstructorControlParameters_UnevenNumber() {
            var attr = new FilterUIHintAttribute("", "", "");
                var v = attr.ControlParameters;
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ConstructorControlParameters_NonStringKey() {
            var attr = new FilterUIHintAttribute("", "", 1, "value");
                var v = attr.ControlParameters;
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ConstructorControlParameters_NullKey() {
            var attr = new FilterUIHintAttribute("", "", null, "value");
                var v = attr.ControlParameters;
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ConstructorControlParameters_DuplicateKey() {
            var attr = new FilterUIHintAttribute("", "", "key", "value1", "key", "value2");
                var v = attr.ControlParameters;
        }

#if !SILVERLIGHT
        [Test]
        public void TypeId_ReturnsDifferentValuesForDifferentInstances() {
            Assert.AreNotEqual(new FilterUIHintAttribute("foo").TypeId, new FilterUIHintAttribute("bar").TypeId);
        }
#endif
    }
}

#endif