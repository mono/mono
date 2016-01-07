// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Env = NUnit.Env;
using NUnit.TestUtilities;

namespace NUnitLite.Tests
{
    [TestFixture]
    class CollectionTests : IExpectException
    {
        [Test]
        public void CanMatchTwoCollections()
        {
            ICollection expected = new SimpleObjectCollection(1, 2, 3);
            ICollection actual = new SimpleObjectCollection(1, 2, 3);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CanMatchAnArrayWithACollection()
        {
            ICollection collection = new SimpleObjectCollection(1, 2, 3);
            int[] array = new int[] { 1, 2, 3 };

            Assert.That(collection, Is.EqualTo(array));
            Assert.That(array, Is.EqualTo(collection));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void FailureMatchingArrayAndCollection()
        {
            int[] expected = new int[] { 1, 2, 3 };
            ICollection actual = new SimpleObjectCollection(1, 5, 3);

            Assert.That(actual, Is.EqualTo(expected));
        }

        public void HandleException(Exception ex)
        {
            Assert.That(ex.Message, Is.EqualTo(
                "  Expected is <System.Int32[3]>, actual is <NUnit.TestUtilities.SimpleObjectCollection> with 3 elements" + Env.NewLine +
                "  Values differ at index [1]" + Env.NewLine +
                TextMessageWriter.Pfx_Expected + "2" + Env.NewLine +
                TextMessageWriter.Pfx_Actual   + "5" + Env.NewLine));
        }
    }
}
