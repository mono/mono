// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

// https://silverlight.svn.codeplex.com/svn/Release/Silverlight4/Source/RiaClient.Tests/System.ComponentModel.DataAnnotations/UIHintAttributeTest.cs

#if NET_4_0

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
    [TestFixture]
    public class UIHintAttributeTest {
        [Test]
        [Description("Simple ctors set expected properties.")]
        public void UIHintAttribute_Simple_Ctors_Set_Properties() {
            var attr = new UIHintAttribute(null, null);
            Assert.IsNull(attr.UIHint);
            Assert.IsNull(attr.PresentationLayer);
            Assert.AreEqual(0, attr.ControlParameters.Count);

            attr = new UIHintAttribute(string.Empty, string.Empty);
            Assert.AreEqual(string.Empty, attr.UIHint);
            Assert.AreEqual(string.Empty, attr.PresentationLayer);
            Assert.AreEqual(0, attr.ControlParameters.Count);

            attr = new UIHintAttribute("theHint");
            Assert.AreEqual("theHint", attr.UIHint);
            Assert.IsNull(attr.PresentationLayer);
            Assert.AreEqual(0, attr.ControlParameters.Count);

            attr = new UIHintAttribute("theHint", "theLayer");
            Assert.AreEqual("theHint", attr.UIHint);
            Assert.AreEqual("theLayer", attr.PresentationLayer);
            Assert.AreEqual(0, attr.ControlParameters.Count);
        }

        [Test]
        public void ConstructorControlParameters() {
            Assert.AreEqual(2, new UIHintAttribute("", "", "a", 1, "b", 2).ControlParameters.Keys.Count);
        }

        [Test]
        public void ConstructorControlParameters_NoParams() {
            Assert.AreEqual(0, new UIHintAttribute("", "", new object[0]).ControlParameters.Keys.Count);
            Assert.AreEqual(0, new UIHintAttribute("", "", (object[])null).ControlParameters.Keys.Count);
            Assert.AreEqual(0, new UIHintAttribute("", "").ControlParameters.Keys.Count);
            Assert.AreEqual(0, new UIHintAttribute("").ControlParameters.Keys.Count);
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ConstructorControlParameters_UnevenNumber() {
            var attr = new UIHintAttribute("", "", "");
                var v = attr.ControlParameters;
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ConstructorControlParameters_NonStringKey() {
            var attr = new UIHintAttribute("", "", 1, "value");
                var v = attr.ControlParameters;
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ConstructorControlParameters_NullKey() {
            var attr = new UIHintAttribute("", "", null, "value");
                var v = attr.ControlParameters;
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ConstructorControlParameters_DuplicateKey() {
            var attr = new UIHintAttribute("", "", "key", "value1", "key", "value2");
                var v = attr.ControlParameters;
        }

        [Test]
        public void Equals_DifferentObjectType() {
            Assert.IsFalse(new UIHintAttribute("foo", "bar").Equals(new object()));
        }

        [Test]
        public void Equals_NullObject() {
            Assert.IsFalse(new UIHintAttribute("foo").Equals(null));
        }

        [Test]
        public void Equals_SameObjectType() {
            var a1 = new UIHintAttribute("foo");
            var a2 = new UIHintAttribute("foo");
            var b1 = new UIHintAttribute("foo", "bar");
            var b2 = new UIHintAttribute("foo", "bar");

            Assert.IsTrue(a1.Equals(a2));
            Assert.IsTrue(a2.Equals(a1));

            Assert.IsTrue(b1.Equals(b2));
            Assert.IsTrue(b2.Equals(b1));

            Assert.IsFalse(a1.Equals(b1));
            Assert.IsFalse(b1.Equals(a1));
        }

        [Test]
        public void Equals_SameObjectType_WithParamsDictionary() {
            var a1 = new UIHintAttribute("foo", "bar", "a", 1, "b", false);
            var a2 = new UIHintAttribute("foo", "bar", "b", false, "a", 1);

            Assert.IsTrue(a1.Equals(a2));
            Assert.IsTrue(a2.Equals(a1));
        }

        [Test]
        public void Equals_DoesNotThrow() {
            var a1 = new UIHintAttribute("foo", "bar");
            var a2 = new UIHintAttribute("foo", "bar", 1);

            Assert.IsFalse(a1.Equals(a2));
            Assert.IsFalse(a2.Equals(a1));
        }

#if !SILVERLIGHT
        [Test]
        public void TypeId_ReturnsDifferentValuesForDifferentInstances()
        {
            Assert.AreNotEqual(new UIHintAttribute("foo").TypeId, new UIHintAttribute("bar").TypeId);
        }
#endif
    }
}

#endif