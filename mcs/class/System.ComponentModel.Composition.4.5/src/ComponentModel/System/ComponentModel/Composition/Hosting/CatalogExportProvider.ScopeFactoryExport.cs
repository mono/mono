// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Threading;

namespace System.ComponentModel.Composition.Hosting
{
    public partial class CatalogExportProvider
    {
        internal class ScopeFactoryExport : FactoryExport
        {
            private readonly ScopeManager _scopeManager;
            private readonly CompositionScopeDefinition _catalog;

            internal ScopeFactoryExport(ScopeManager scopeManager, CompositionScopeDefinition catalog, ComposablePartDefinition partDefinition, ExportDefinition exportDefinition) :
                base(partDefinition, exportDefinition)
            {
                this._scopeManager = scopeManager;
                this._catalog = catalog;
            }

            public virtual Export CreateExportProduct(Func<ComposablePartDefinition, bool> filter)
            {
                return new ScopeCatalogExport(this, filter);
            }

            public override Export CreateExportProduct()
            {
                return new ScopeCatalogExport(this, null);
            }

            private sealed class ScopeCatalogExport : Export, IDisposable
            {
                private readonly ScopeFactoryExport _scopeFactoryExport;
                private Func<ComposablePartDefinition, bool> _catalogFilter;
                private CompositionContainer _childContainer;
                private Export _export;
                private readonly object _lock = new object();

                public ScopeCatalogExport(ScopeFactoryExport scopeFactoryExport, Func<ComposablePartDefinition, bool> catalogFilter)
                {
                    this._scopeFactoryExport = scopeFactoryExport;
                    this._catalogFilter = catalogFilter;
                }

                public override ExportDefinition Definition
                {
                    get
                    {
                        return this._scopeFactoryExport.UnderlyingExportDefinition;
                    }
                }

                protected override object GetExportedValueCore()
                {
                    if (this._export == null)
                    {
                        // Need to create a new scopedefinition that is filtered by the ExportProvider
                        var filteredScopeDefinition = new CompositionScopeDefinition(
                            new FilteredCatalog(this._scopeFactoryExport._catalog, this._catalogFilter), 
                            this._scopeFactoryExport._catalog.Children);
                        var childContainer = this._scopeFactoryExport._scopeManager.CreateChildContainer(filteredScopeDefinition);

                        var export = childContainer.CatalogExportProvider.CreateExport(this._scopeFactoryExport.UnderlyingPartDefinition, this._scopeFactoryExport.UnderlyingExportDefinition, false, CreationPolicy.Any);
                        lock (this._lock)
                        {
                            if (this._export == null)
                            {
                                this._childContainer = childContainer;
                                Thread.MemoryBarrier();
                                this._export = export;

                                childContainer = null;
                                export = null;
                            }
                        }
                        if (childContainer != null)
                        {
                            childContainer.Dispose();
                        }
                    }

                    return this._export.Value;
                }

                public void Dispose()
                {
                    CompositionContainer childContainer = null;
                    Export export = null;

                    if (this._export != null)
                    {
                        lock (this._lock)
                        {
                            export = this._export;
                            childContainer = this._childContainer;

                            this._childContainer = null;
                            Thread.MemoryBarrier();
                            this._export = null;
                        }
                    }

                    if(childContainer != null)
                    {
                        childContainer.Dispose();
                    }
                }
            }
        }
    }
}
