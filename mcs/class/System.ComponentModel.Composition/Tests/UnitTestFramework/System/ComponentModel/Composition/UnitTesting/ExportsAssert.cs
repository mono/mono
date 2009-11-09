// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.UnitTesting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.UnitTesting
{
    public static class ExportsAssert
    {
        public static void AreEqual<T>(IEnumerable<Export> actual, params T[] expected)
        {
            EnumerableAssert.AreEqual((IEnumerable)expected, (IEnumerable)actual.Select(export => export.Value));
        }

        public static void AreEqual<T>(IEnumerable<Lazy<T>> actual, params T[] expected)
        {
            EnumerableAssert.AreEqual((IEnumerable<T>)expected, (IEnumerable<T>)actual.Select(export => export.Value));
        }

        public static void AreEqual<T, TMetadataView>(IEnumerable<Lazy<T, TMetadataView>> actual, params T[] expected)
        {
            EnumerableAssert.AreEqual((IEnumerable<T>)expected, (IEnumerable<T>)actual.Select(export => export.Value));
        }
    }
}
