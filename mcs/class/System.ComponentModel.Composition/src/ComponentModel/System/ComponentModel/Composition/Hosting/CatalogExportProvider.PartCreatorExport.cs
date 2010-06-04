// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;

namespace System.ComponentModel.Composition.Hosting
{
    public partial class CatalogExportProvider
    {
        internal class PartCreatorExport : Export
        {
            private readonly CatalogExportProvider _catalogExportProvider;
            private readonly ComposablePartDefinition _partDefinition;
            private readonly ExportDefinition _exportDefinition;
            private ExportDefinition _partCreatorExportDefinition;
            private PartCreatorPartDefinition _partCreatorPartDefinition;

            public PartCreatorExport(CatalogExportProvider catalogExportProvider, ComposablePartDefinition partDefinition, ExportDefinition exportDefinition)
            {
                this._catalogExportProvider = catalogExportProvider;
                this._partDefinition = partDefinition;
                this._exportDefinition = exportDefinition;
                this._partCreatorExportDefinition = new PartCreatorExportDefinition(this._exportDefinition);
            }

            public override ExportDefinition Definition
            {
                get { return this._partCreatorExportDefinition; }
            }

            protected override object GetExportedValueCore()
            {
                if (this._partCreatorPartDefinition == null)
                {
                    this._partCreatorPartDefinition = new PartCreatorPartDefinition(this);
                }
                return this._partCreatorPartDefinition;
            }

            public Export CreateExportProduct()
            {
                return new NonSharedCatalogExport(this._catalogExportProvider, this._partDefinition, this._exportDefinition);
            }

            private class PartCreatorPartDefinition : ComposablePartDefinition
            {
                private readonly PartCreatorExport _partCreatorExport;

                public PartCreatorPartDefinition(PartCreatorExport partCreatorExport)
                {
                    this._partCreatorExport = partCreatorExport;
                }

                public override IEnumerable<ExportDefinition> ExportDefinitions
                {
                    get { return new ExportDefinition[] { this._partCreatorExport.Definition }; }
                }

                public override IEnumerable<ImportDefinition> ImportDefinitions
                {
                    get { return Enumerable.Empty<ImportDefinition>(); }
                }

                public ExportDefinition PartCreatorExportDefinition
                {
                    get { return this._partCreatorExport.Definition; }
                }

                public Export CreateProductExport()
                {
                    return this._partCreatorExport.CreateExportProduct();
                }

                public override ComposablePart CreatePart()
                {
                    return new PartCreatorPart(this);
                }
            }

            private sealed class PartCreatorPart : ComposablePart, IDisposable
            {
                private readonly PartCreatorPartDefinition _definition;
                private readonly Export _export;

                public PartCreatorPart(PartCreatorPartDefinition definition)
                {
                    this._definition = definition;
                    this._export = definition.CreateProductExport();
                }

                public override IEnumerable<ExportDefinition> ExportDefinitions
                {
                    get { return this._definition.ExportDefinitions; }
                }

                public override IEnumerable<ImportDefinition> ImportDefinitions
                {
                    get { return this._definition.ImportDefinitions; }
                }

                public override object GetExportedValue(ExportDefinition definition)
                {
                    if (definition != this._definition.PartCreatorExportDefinition)
                    {
                        throw ExceptionBuilder.CreateExportDefinitionNotOnThisComposablePart("definition");
                    }

                    return this._export.Value;
                }

                public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
                {
                    throw ExceptionBuilder.CreateImportDefinitionNotOnThisComposablePart("definition");
                }

                public void Dispose()
                {
                    IDisposable disposable = this._export as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}
