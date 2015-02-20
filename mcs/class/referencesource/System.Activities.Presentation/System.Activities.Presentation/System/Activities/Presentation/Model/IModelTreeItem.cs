//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Windows;
    using System.Collections.Generic;
    using System.Activities.Presentation.Model;

    // Interface for the things common among ModelItemImpl and ModelItemCollectionImpl and ModelItemDictionaryImpl.

    interface IModelTreeItem
    {
        ModelItem ModelItem
        { 
            get; 
        }

        Dictionary<string, ModelItem> ModelPropertyStore
        { 
            get; 
        }

        ModelTreeManager ModelTreeManager
        { 
            get; 
        }

        IEnumerable<ModelItem> ItemBackPointers { get; }

        // In case instance has implemented ICustomTypeDescriptor, IModelTreeItem.ModelPropertyStore may contain some property that ModelItem.Properties doesn't contain.
        // The BackPointers to those properties will be added to this collection.
        List<BackPointer> ExtraPropertyBackPointers { get; }

        void OnPropertyChanged(string propertyName);

        void SetParent(ModelItem dataModelItem);

        // This is needed because the source property in the ModelItem Base class is get only
        // and we want a common setter for ModelItemImpl and ModelItemCollectionImpl.
        void SetSource(ModelProperty dataModelProperty);

        void RemoveParent(ModelItem oldParent);

        void RemoveSource(ModelProperty oldModelProperty);

        void RemoveSource(ModelItem parent, string propertyName);

        // Used to set the  the current view for this Model item.
        void SetCurrentView(DependencyObject view);
    }
}
