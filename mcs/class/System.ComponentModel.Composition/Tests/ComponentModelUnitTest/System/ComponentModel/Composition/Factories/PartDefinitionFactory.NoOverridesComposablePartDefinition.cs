// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Factories
{
    partial class PartDefinitionFactory
    {
        // NOTE: Do not add any more behavior to this class, as ComposablePartDefinitionTests.cs 
        // uses this to verify default behavior of the base class.
        private class NoOverridesComposablePartDefinition : ComposablePartDefinition
        {
            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get { return Enumerable.Empty<ExportDefinition>(); }
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return Enumerable.Empty<ImportDefinition>(); }
            }

            public override ComposablePart CreatePart()
            {
                Assert.Fail();
                return null;
            }
        }
    }
}