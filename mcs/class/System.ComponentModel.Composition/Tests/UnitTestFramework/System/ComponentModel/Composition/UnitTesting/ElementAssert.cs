// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Linq.Expressions;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition.UnitTesting
{
    internal static class ElementAssert
    {
        public static void AreEqual(ICompositionElement expected, ICompositionElement actual)
        {
            if (expected == null || actual == null)
            {
                Assert.AreEqual(expected, actual);
                return;
            }

            Assert.AreEqual(expected.DisplayName, actual.DisplayName);
            ElementAssert.AreEqual(expected.Origin, actual.Origin);
        }

        public static void AreEqual(IEnumerable<ICompositionElement> expected, IEnumerable<ICompositionElement> actual)
        {
            Assert.AreEqual(expected.Count(), actual.Count());

            int index = 0;
            foreach (var element in expected)
            {
                AreEqual(element, actual.ElementAt(index));

                index++;
            }
        }
    }
}