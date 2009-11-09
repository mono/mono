// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Collections.Generic;

namespace System.ComponentModel.Composition.Factories
{
    partial class CatalogFactory
    {
        private class DerivedComposablePartCatalog : ComposablePartCatalog
        {
            private readonly IEnumerable<ComposablePartDefinition> _definitions;

            public DerivedComposablePartCatalog(IEnumerable<ComposablePartDefinition> definitions)
            {
                _definitions = definitions;
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get { return _definitions.AsQueryable(); }
            }
        }
    }
}