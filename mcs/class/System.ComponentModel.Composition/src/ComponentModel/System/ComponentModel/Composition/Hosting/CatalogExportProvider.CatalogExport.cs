// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting
{
    public partial class CatalogExportProvider
    {
        private class CatalogExport : Export
        {
            protected readonly CatalogExportProvider _catalogExportProvider;
            protected readonly ComposablePartDefinition _partDefinition;
            protected readonly ExportDefinition _definition;
            protected ComposablePart _part;

            public CatalogExport(CatalogExportProvider catalogExportProvider,
                ComposablePartDefinition partDefinition, ExportDefinition definition)
            {
                this._catalogExportProvider = catalogExportProvider;
                this._partDefinition = partDefinition;
                this._definition = definition;
            }

            public override ExportDefinition Definition
            {
                get
                {
                    return this._definition;
                }
            }

            protected virtual bool IsSharedPart
            {
                get
                {
                    return true;
                }
            }

            protected override object GetExportedValueCore()
            {
                ComposablePart part = this._catalogExportProvider.GetComposablePart(this._partDefinition, this.IsSharedPart);
                object exportedValue = this._catalogExportProvider.GetExportedValue(part, this._definition, this.IsSharedPart);
                this._part = part;

                return exportedValue;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
            public static CatalogExport CreateExport(CatalogExportProvider catalogExportProvider,
                ComposablePartDefinition partDefinition, ExportDefinition definition, CreationPolicy importCreationPolicy)
            {
                CreationPolicy partPolicy = partDefinition.Metadata.GetValue<CreationPolicy>(CompositionConstants.PartCreationPolicyMetadataName);
                bool isSharedPart = ShouldUseSharedPart(partPolicy, importCreationPolicy);

                if (isSharedPart)
                {
                    return new CatalogExport(catalogExportProvider, partDefinition, definition);
                }
                else
                {
                    return new NonSharedCatalogExport(catalogExportProvider, partDefinition, definition);
                }
            }

            private static bool ShouldUseSharedPart(CreationPolicy partPolicy, CreationPolicy importPolicy)
            {
                // Matrix that details which policy to use for a given part to satisfy a given import.
                //                   Part.Any   Part.Shared  Part.NonShared
                // Import.Any        Shared     Shared       NonShared
                // Import.Shared     Shared     Shared       N/A
                // Import.NonShared  NonShared  N/A          NonShared

                switch (partPolicy)
                {
                    case CreationPolicy.Any:
                        {
                            if (importPolicy == CreationPolicy.Any ||
                                importPolicy == CreationPolicy.Shared)
                            {
                                return true;
                            }
                            return false;
                        }

                    case CreationPolicy.NonShared:
                        {
                            Assumes.IsTrue(importPolicy != CreationPolicy.Shared);
                            return false;
                        }

                    default:
                        {
                            Assumes.IsTrue(partPolicy == CreationPolicy.Shared);
                            Assumes.IsTrue(importPolicy != CreationPolicy.NonShared);
                            return true;
                        }
                }
            }
        }

        private sealed class NonSharedCatalogExport : CatalogExport, IDisposable
        {
            public NonSharedCatalogExport(CatalogExportProvider catalogExportProvider,
                ComposablePartDefinition partDefinition, ExportDefinition definition)
                : base(catalogExportProvider, partDefinition, definition)
            {
            }

            protected override bool IsSharedPart
            {
                get
                {
                    return false;
                }
            }

            void IDisposable.Dispose()
            {
                if (this._part != null)
                {
                    this._catalogExportProvider.ReleasePart(this.Value, this._part, null);
                    this._part = null;
                }
            }
        }
    }
}
