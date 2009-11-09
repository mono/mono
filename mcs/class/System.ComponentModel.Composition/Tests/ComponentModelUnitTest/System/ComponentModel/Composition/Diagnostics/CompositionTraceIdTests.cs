// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.UnitTesting;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ComponentModel.Composition.Diagnostics
{
    [TestClass]
    public class CompositionTraceIdTests
    {
        [TestMethod]
        public void CompositionTraceIdsAreInSyncWithTraceIds()
        {
            ExtendedAssert.EnumsContainSameValues<CompositionTraceId, TraceId>();
        }
    }
}
