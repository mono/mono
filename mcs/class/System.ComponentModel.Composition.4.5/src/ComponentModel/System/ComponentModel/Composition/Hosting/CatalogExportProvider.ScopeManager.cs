using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting
{
    public partial class CatalogExportProvider
    {
        internal class ScopeManager : ExportProvider
        {
            private CompositionScopeDefinition _scopeDefinition;
            private CatalogExportProvider _catalogExportProvider;

            public ScopeManager(CatalogExportProvider catalogExportProvider, CompositionScopeDefinition scopeDefinition)
            {
                Assumes.NotNull(catalogExportProvider);
                Assumes.NotNull(scopeDefinition);

                this._scopeDefinition = scopeDefinition;
                this._catalogExportProvider = catalogExportProvider;
            }

            protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
            {
                List<Export> exports = new List<Export>();

                ImportDefinition queryImport = TranslateImport(definition);
                if (queryImport == null)
                {
                    return exports;
                }

                // go through the catalogs and see if there's anything there of interest
                foreach (CompositionScopeDefinition childCatalog in this._scopeDefinition.Children)
                {
                    foreach (var partDefinitionAndExportDefinition in childCatalog.GetExportsFromPublicSurface(queryImport))
                    {
                        using (var container = this.CreateChildContainer(childCatalog))
                        {
                            // We create a nested AtomicComposition() because the container will be Disposed and 
                            // the RevertActions need to operate before we Dispose the child container
                            using (var ac = new AtomicComposition(atomicComposition))
                            {
                                var childCatalogExportProvider = container.CatalogExportProvider;
                                if (!childCatalogExportProvider.DetermineRejection(partDefinitionAndExportDefinition.Item1, ac))
                                {
                                    exports.Add(this.CreateScopeExport(childCatalog, partDefinitionAndExportDefinition.Item1, partDefinitionAndExportDefinition.Item2));
                                }
                            }
                        }
                    }
                }

                return exports;
            }

            private Export CreateScopeExport(CompositionScopeDefinition childCatalog, ComposablePartDefinition partDefinition, ExportDefinition exportDefinition)
            {
                return new ScopeFactoryExport(this, childCatalog, partDefinition, exportDefinition);
            }

            internal CompositionContainer CreateChildContainer(ComposablePartCatalog childCatalog)
            {
                return new CompositionContainer(childCatalog, this._catalogExportProvider._compositionOptions, this._catalogExportProvider._sourceProvider);
            }

            private static ImportDefinition TranslateImport(ImportDefinition definition)
            {
                IPartCreatorImportDefinition factoryDefinition = definition as IPartCreatorImportDefinition;
                if (factoryDefinition == null)
                {
                    return null;
                }

                // Now we need to make sure that the creation policy is handled correctly
                // We will always create a new child CatalogEP to satsify the request, so from the perspecitive of the caller, the policy should 
                // always be NonShared (or Any). From teh perspective of the callee, it's the otehr way around.
                ContractBasedImportDefinition productImportDefinition = factoryDefinition.ProductImportDefinition;
                ImportDefinition result = null;

                switch (productImportDefinition.RequiredCreationPolicy)
                {
                    case CreationPolicy.NonShared:
                    case CreationPolicy.NewScope:
                        {
                            // we need to recreate the import definition with the policy "Any", so that we can
                            // pull singletons from the inner CatalogEP. teh "non-sharedness" is achieved through 
                            // the creation of the new EPs already.
                            result = new ContractBasedImportDefinition(
                                productImportDefinition.ContractName,
                                productImportDefinition.RequiredTypeIdentity,
                                productImportDefinition.RequiredMetadata,
                                productImportDefinition.Cardinality,
                                productImportDefinition.IsRecomposable,
                                productImportDefinition.IsPrerequisite,
                                CreationPolicy.Any,
                                productImportDefinition.Metadata);
                            break;
                        }
                    case CreationPolicy.Any:
                        {
                            // "Any" works every time
                            result = productImportDefinition;
                            break;
                        }
                }

                return result;
            }
        }
    }
}
