//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Activities.Presentation.Services;
    using System.Runtime;

    // This is the implementaion of the ModelService, this is published by the ModelTreeManager
    // on the editingContext. This is just a facade to the modelTreemanager methods.

    class ModelServiceImpl : ModelService
    {
        ModelTreeManager modelTreeManager;

        public ModelServiceImpl(ModelTreeManager modelTreeManager)
        {
            if (modelTreeManager == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("modelTreeManager"));
            }
            this.modelTreeManager = modelTreeManager;
        }

        public override event EventHandler<ModelChangedEventArgs> ModelChanged;

        public override ModelItem Root
        {
            get
            {
                return modelTreeManager.Root;
            }
        }

        public override IEnumerable<ModelItem> Find(ModelItem startingItem, Predicate<Type> match)
        {
            return ModelTreeManager.Find(startingItem, delegate(ModelItem m) { return match(m.ItemType); }, false);
        }

        public override IEnumerable<ModelItem> Find(ModelItem startingItem, Type type)
        {
            if (startingItem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("startingItem"));
            }

            if (type == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("type"));
            }
            Fx.Assert(!type.IsValueType, "hmm why would some one search for modelitems for value types?");
            return ModelTreeManager.Find(startingItem, delegate(ModelItem modelItem)
            {
                return type.IsAssignableFrom(modelItem.ItemType);
            }, false);
        }

        public override ModelItem FromName(ModelItem scope, string name, StringComparison comparison)
        {
            // The workflow component model does not implement a unique named activity object right now
            // so we cannot support this feature.
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        internal void OnModelItemAdded(ModelItem modelItem, ModelChangeInfo changeInfo)
        {
            Fx.Assert(modelItem != null, "modelItem should not be null");
            if (ModelChanged != null)
            {
                Fx.Assert(modelItem != null, "trying to add empty model item");
                List<ModelItem> modelItemsAdded = new List<ModelItem>(1);
                modelItemsAdded.Add(modelItem);
                ModelChanged.Invoke(this, new ModelChangedEventArgsImpl(modelItemsAdded, null, null, changeInfo));
                modelTreeManager.SyncModelAndText();
            }
        }

        internal void OnModelItemRemoved(ModelItem modelItem, ModelChangeInfo changInfo)
        {
            Fx.Assert(modelItem != null, "modelItem should not be null");
            if (ModelChanged != null)
            {
                List<ModelItem> modelItemsRemoved = new List<ModelItem>(1);
                modelItemsRemoved.Add(modelItem);
                ModelChanged.Invoke(this, new ModelChangedEventArgsImpl(null, modelItemsRemoved, null, changInfo));
                modelTreeManager.SyncModelAndText();
            }
        }

        internal void EmitModelChangeInfo(ModelChangeInfo changInfo)
        {
            Fx.Assert(changInfo != null, "changInfo should not be null");

            if (ModelChanged != null)
            {
                ModelChanged.Invoke(this, new ModelChangedEventArgsImpl(null, null, null, changInfo));
                modelTreeManager.SyncModelAndText();
            }
        }

        internal void OnModelItemsRemoved(IEnumerable<ModelItem> modelItems)
        {
            Fx.Assert(modelItems != null, "modelItem should not be null");
            if (ModelChanged != null)
            {
                List<ModelItem> modelItemsRemoved = new List<ModelItem>();
                modelItemsRemoved.AddRange(modelItems);
                ModelChanged.Invoke(this, new ModelChangedEventArgsImpl(null, modelItemsRemoved, null));
                modelTreeManager.SyncModelAndText();
            }
        }

        internal void OnModelPropertyChanged(ModelProperty property, ModelChangeInfo changeInfo)
        {
            Fx.Assert(property != null, "property cannot be null");
            Fx.Assert(changeInfo != null, "changeInfo cannot be null");

            if (ModelChanged != null)
            {
                List<ModelProperty> propertiesChanged = new List<ModelProperty>(1);
                propertiesChanged.Add(property);
                ModelChanged.Invoke(this, new ModelChangedEventArgsImpl(null, null, propertiesChanged, changeInfo));
                modelTreeManager.SyncModelAndText();
            }
        }

        protected override ModelItem CreateItem(object instance)
        {
            return modelTreeManager.CreateModelItem(null, instance);
        }

        protected override ModelItem CreateItem(Type itemType, CreateOptions options, params object[] arguments)
        {
            Object instance = Activator.CreateInstance(itemType, arguments);
            return modelTreeManager.CreateModelItem(null, instance);
        }

        protected override ModelItem CreateStaticMemberItem(Type type, string memberName)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        internal ModelItem WrapAsModelItem(object instance)
        {
            return CreateItem(instance);
        }
    }
}
