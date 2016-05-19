//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;

    // this class is to share common code among classes that implements IModelTreeItem, currently ModelItemImpl, ModelItemCollectionImpl and ModelItemDictionaryImpl
    internal class ModelTreeItemHelper
    {
        private List<BackPointer> extraPropertyBackPointers;

        public ModelTreeItemHelper()
        {
            this.extraPropertyBackPointers = new List<BackPointer>();
        }

        public List<BackPointer> ExtraPropertyBackPointers
        {
            get { return this.extraPropertyBackPointers; }
        }

        public void RemoveExtraPropertyBackPointer(ModelItem parent, string propertyName)
        {
            Fx.Assert(parent != null, "parent should not be null");
            Fx.Assert(!string.IsNullOrEmpty(propertyName), "propertyName should not be null or empty");

            BackPointer backPointer = this.extraPropertyBackPointers.FirstOrDefault<BackPointer>((bp) => bp.DestinationVertex == parent && propertyName == bp.PropertyName);
            if (backPointer != null)
            {
                this.extraPropertyBackPointers.Remove(backPointer);
            }
            else
            {
                Fx.Assert("BackPointer not found");
            }
        }
    }
}
