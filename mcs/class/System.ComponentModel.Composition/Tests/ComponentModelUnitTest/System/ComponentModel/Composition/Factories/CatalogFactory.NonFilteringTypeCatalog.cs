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
        private class NonFilteringTypeCatalog : ComposablePartCatalog
        {
            private readonly List<ComposablePartDefinition> _definitions;

            public NonFilteringTypeCatalog(params Type[] types)
            {
                this._definitions = new List<ComposablePartDefinition>();
                foreach (Type type in types)
                {
                    this._definitions.Add(AttributedModelServices.CreatePartDefinition(type, null));
                }
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get { return this._definitions.AsQueryable(); }
            }
        }
    }
}