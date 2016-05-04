//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Hosting
{
    using System;    
    using System.Runtime;
    using System.Collections.ObjectModel;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Model;

    
    [Fx.Tag.XamlVisible(false)]
    public sealed class ImportedNamespaceContextItem : ContextItem
    {
        bool initialized = false;
        Collection<string> importedNamespaces;

        public Collection<string> ImportedNamespaces
        {
            get
            {
                if (this.importedNamespaces == null)
                {
                    initialized = true;
                    this.importedNamespaces = new Collection<string>();
                }
                return this.importedNamespaces;
            }
        }

        public override Type ItemType
        {
            get { return typeof(ImportedNamespaceContextItem); }
        }

        public void EnsureInitialized(EditingContext context)
        {
            if (!initialized)
            {
                ModelService modelService = context.Services.GetService<ModelService>();                
                Fx.Assert(modelService != null, "ModelService shouldn't be null in EditingContext.");
                Fx.Assert(modelService.Root != null, "model must have a root");
                ModelItemCollection importsModelItem = modelService.Root.Properties[NamespaceListPropertyDescriptor.ImportCollectionPropertyName].Collection;
                Fx.Assert(importsModelItem != null, "root must have imports");                
                foreach (ModelItem import in importsModelItem)
                {
                    this.ImportedNamespaces.Add(import.Properties[NamespaceListPropertyDescriptor.NamespacePropertyName].ComputedValue as string);
                }
            }
        }
    }
}
