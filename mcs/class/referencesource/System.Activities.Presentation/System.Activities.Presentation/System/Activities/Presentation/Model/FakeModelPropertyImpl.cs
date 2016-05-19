//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// FakeModelPropertyImpl. This class is used with FakeModelItemImpl. it is used to allow full model editing expirience
    /// without actually modyfing actual model tree. Even though reference to ModelTreeManager is availabe, changes made to object
    /// using this class are not reflected in actual model. Especially, any changes made here do not affect undo/redo stack.
    /// see aslo DesignObjectWrapper class for more usage details
    /// </summary>
    sealed class FakeModelPropertyImpl : ModelPropertyImpl
    {
        IModelTreeItem parentModelTreeItem;
        FakeModelItemImpl temporaryValue;
        bool isSettingValue = false;

        public FakeModelPropertyImpl(FakeModelItemImpl parent, PropertyDescriptor propertyDescriptor)
            : base(parent, propertyDescriptor, false)
        {
            this.parentModelTreeItem = (IModelTreeItem)parent;
        }

        //no collection support
        public override ModelItemCollection Collection
        {
            get { return null; }
        }

        public override bool IsCollection
        {
            get { return false; }
        }

        //no dictionary support
        public override ModelItemDictionary Dictionary
        {
            get { return null; }
        }

        public override bool IsDictionary
        {
            get { return false; }
        }

        public override ModelItem Value
        {
            get
            {
                ModelItem result = null;
                object parentObject = this.parentModelTreeItem.ModelItem.GetCurrentValue();
                result = this.StoreValue(this.PropertyDescriptor.GetValue(parentObject));
                return result;
            }
        }

        public override void ClearValue()
        {
            //try setting default value
            this.SetValue(this.DefaultValue);
        }

        public override ModelItem SetValue(object value)
        {
            //are we already setting value? 
            if (!isSettingValue)
            {
                try
                {
                    this.isSettingValue = true;
                    //create new value
                    this.temporaryValue = this.WrapValue(value);
                    //is there a value stored already?
                    if (this.parentModelTreeItem.ModelPropertyStore.ContainsKey(this.Name))
                    {
                        //yes - cleanup references
                        IModelTreeItem item = (IModelTreeItem)this.parentModelTreeItem.ModelPropertyStore[this.Name];
                        item.RemoveSource(this);
                        item.RemoveParent(this.parentModelTreeItem.ModelItem);
                        //and remove it
                        this.parentModelTreeItem.ModelPropertyStore.Remove(this.Name);
                    }
                    //set it onto underlying object
                    this.PropertyDescriptor.SetValue(this.Parent.GetCurrentValue(), (null != this.temporaryValue ? this.temporaryValue.GetCurrentValue() : null));
                    //store it in parent's store
                    this.temporaryValue = this.StoreValue(this.temporaryValue);

                    //notify listeners - notification must be postponed until actual underlying object value is updated, otherwise, listeners might get old value
                    this.parentModelTreeItem.ModelTreeManager.AddToCurrentEditingScope(new FakeModelNotifyPropertyChange(this.parentModelTreeItem, this.Name));
                }
                catch (ValidationException e)
                {
                    Trace.WriteLine(e.ToString());
                    //it is important to rethrow exception here - otherwise, DataGrid will assume operation completed successfully
                    throw;
                }
                finally
                {
                    this.isSettingValue = false;
                }
            }

            return this.temporaryValue;
        }

        FakeModelItemImpl WrapValue(object value)
        {
            FakeModelItemImpl wrappedValue = value as FakeModelItemImpl;
            if (null == wrappedValue && null != value)
            {
                wrappedValue = new FakeModelItemImpl(this.parentModelTreeItem.ModelTreeManager, this.PropertyType, value, (FakeModelItemImpl)this.Parent);
            }
            return wrappedValue;
        }

        FakeModelItemImpl StoreValue(object value)
        {
            FakeModelItemImpl wrappedValue = WrapValue(value);
            if (null != wrappedValue)
            {
                this.parentModelTreeItem.ModelPropertyStore[this.Name] = wrappedValue;
                IModelTreeItem modelTreeItem = (IModelTreeItem)wrappedValue;
                modelTreeItem.SetSource(this);
            }
            else
            {
                ModelItem existing = null;
                if (this.parentModelTreeItem.ModelPropertyStore.TryGetValue(this.Name, out existing))
                {
                    IModelTreeItem modelTreeItem = (IModelTreeItem)existing;
                    modelTreeItem.RemoveSource(this);
                    modelTreeItem.RemoveParent(this.Parent);
                }
                this.parentModelTreeItem.ModelPropertyStore.Remove(this.Name);
            }
            return wrappedValue;
        }
    }

    //helper class - implements change
    //FakeModelPropery uses instance of this class to notify all listeners that property value has changed. the notification is deffered untill all editing operations
    //have completed, so the listener will get notified after edit is completed
    sealed class FakeModelNotifyPropertyChange : ModelChange
    {
        IModelTreeItem modelTreeItem;
        string propertyName;

        public FakeModelNotifyPropertyChange(IModelTreeItem modelTreeItem, string propertyName)
        {
            this.modelTreeItem = modelTreeItem;
            this.propertyName = propertyName;
        }

        public override string Description
        {
            get { return this.GetType().Name; }
        }

        public override bool Apply()
        {
            if (this.modelTreeItem != null)
            {
                EditingContext context = this.modelTreeItem.ModelTreeManager.Context;
                //this change shouldn't participate in Undo/Redo
                if (null != context && !context.Services.GetService<UndoEngine>().IsUndoRedoInProgress)
                {
                    this.modelTreeItem.OnPropertyChanged(this.propertyName);
                }
            }
            //return false here - i don't need that change in the change list
            return false;
        }

        public override Change GetInverse()
        {
            //this change shouldn't participate in Undo/Redo
            return null;
        }
    }
}
