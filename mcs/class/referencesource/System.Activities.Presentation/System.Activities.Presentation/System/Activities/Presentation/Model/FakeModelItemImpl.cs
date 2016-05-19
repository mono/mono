//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{

    /// <summary>
    /// FakeModelItemImpl - purpose of this class is to allow full model editing expirience, without need to participate within model tree operations
    /// If you use this class, even though it contains reference to ModelTreeManager, you are not affecting actual model tree. Any changes made to the 
    /// model, do not result in any undo/redo operations
    /// see aslo DesignObjectWrapper class for more usage details
    /// </summary>
    sealed class FakeModelItemImpl : ModelItemImpl
    {
        public FakeModelItemImpl(ModelTreeManager modelTreeManager, Type itemType, object instance, FakeModelItemImpl parent) 
            : base(modelTreeManager, itemType, instance, parent)
        {
        }

        public override ModelItem Root
        {
            get 
            {
                if (this.Parent == null)
                {
                    return this;
                }
                else
                {
                    return this.Parent.Root;
                }
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            IModelTreeItem modelTreeItem = (IModelTreeItem)this;
            ModelItem currentValue;
            //if property value has changed - remove existing value, so the ModelPropertyImplementation will 
            //force reading the value from the underlying object
            if (modelTreeItem.ModelPropertyStore.TryGetValue(propertyName, out currentValue))
            {
                IModelTreeItem valueAsTreeItem = (IModelTreeItem)currentValue;
                //cleanup references
                valueAsTreeItem.RemoveParent(this);
                valueAsTreeItem.RemoveSource(this.Properties[propertyName]);
                //remove from store
                modelTreeItem.ModelPropertyStore.Remove(propertyName);
            }
            base.OnPropertyChanged(propertyName);
        }
    }
}
