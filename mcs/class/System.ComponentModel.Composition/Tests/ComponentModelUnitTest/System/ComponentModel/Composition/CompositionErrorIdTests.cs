// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.UnitTesting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition
{
    [TestClass]
    public class CompositionErrorIdTests
    {
        [TestMethod]
        public void CompositionErrorIdsAreInSyncWithErrorIds()
        {
            ExtendedAssert.EnumsContainSameValues<CompositionErrorId, ErrorId>();
        }
    }
}
