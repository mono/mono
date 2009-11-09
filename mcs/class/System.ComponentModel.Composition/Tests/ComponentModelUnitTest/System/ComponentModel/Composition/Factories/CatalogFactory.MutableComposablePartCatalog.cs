// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;

namespace System.ComponentModel.Composition.Factories
{
    partial class CatalogFactory
    {
        public class MutableComposablePartCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
        {
            private readonly HashSet<ComposablePartDefinition> _definitions;

            public MutableComposablePartCatalog(IEnumerable<ComposablePartDefinition> definitions)
            {
                _definitions = new HashSet<ComposablePartDefinition>(definitions);
            }

            public void AddDefinition(ComposablePartDefinition definition)
            {
                OnDefinitionsChanged(definition, true);
            }

            public void RemoveDefinition(ComposablePartDefinition definition)
            {
                OnDefinitionsChanged(definition, false);
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get { return _definitions.AsQueryable(); }
            }

            private void OnDefinitionsChanged(ComposablePartDefinition definition, bool added)
            {
                ComposablePartDefinition[] addedDefinitions = added ? new ComposablePartDefinition[] { definition } : new ComposablePartDefinition[0];
                ComposablePartDefinition[] removeDefinitions = added ? new ComposablePartDefinition[0] : new ComposablePartDefinition[] { definition };

                var e = new ComposablePartCatalogChangeEventArgs(addedDefinitions, removeDefinitions, null);
                Changing(this, e);

                if (added)
                {
                    _definitions.Add(definition);
                }
                else
                {
                    _definitions.Remove(definition);
                }

                if (Changed != null)
                {
                    Changed(this, e);
                }
            }

            public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

            public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;
        }
    }
}