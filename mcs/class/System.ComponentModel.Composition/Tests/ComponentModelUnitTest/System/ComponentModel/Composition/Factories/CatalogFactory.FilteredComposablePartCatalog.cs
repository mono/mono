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
        private class FilteredComposablePartCatalog : ComposablePartCatalog
        {
            private readonly IQueryable<ComposablePartDefinition> _filteredParts;

            public FilteredComposablePartCatalog(ComposablePartCatalog catalog, Func<ComposablePartDefinition, bool> filter)
            {
                this._filteredParts = catalog.Parts.Where(filter).AsQueryable();

                // Do we care about hooking the the catalog changed events? Not for my particular tests.
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get
                {
                    return this._filteredParts;
                }
            }
        }
    }
}