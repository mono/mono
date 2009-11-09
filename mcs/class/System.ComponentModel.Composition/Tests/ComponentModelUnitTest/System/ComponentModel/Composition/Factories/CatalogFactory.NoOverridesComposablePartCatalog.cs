// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace System.ComponentModel.Composition.Factories
{
    partial class CatalogFactory
    {
        // NOTE: Do not add any more behavior to this class, as ComposablePartCatalogTests.cs 
        // uses this to verify default behavior of the base class.
        private class NoOverridesComposablePartCatalog : ComposablePartCatalog
        {
            public NoOverridesComposablePartCatalog()
            {
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get { return Enumerable.Empty<ComposablePartDefinition>().AsQueryable(); }
            }
        }
    }
}