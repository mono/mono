//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.TreeView
{
    using System;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    class ChangeNotificationTracker
    {
        private bool? delayUpdates = null;

        public ModelProperty ParentProperty { get; set; }
        public Dictionary<ModelItem, HashSet<string>> TrackedModelItem { get; set; }
        public List<INotifyCollectionChanged> TrackedCollection { get; set; }
        public List<TreeViewItemViewModel> ChildViewModels { get; set; }

        public TreeViewItemViewModel Parent { get; private set; }

        // Guard to delay processing events while handling another event
        private bool? DelayUpdates
        {
            get
            {
                return this.delayUpdates;
            }

            set
            {
                bool? oldDelayUpdates = this.delayUpdates;
                this.delayUpdates = value;

                // If necessary, perform delayed updates when initial handling completes
                if (null == this.delayUpdates && null != oldDelayUpdates && (bool)oldDelayUpdates)
                {
                    // We do not preserve args of events that occurred within a handler
                    // Fortunately EventArgs parameter to UpdateChildren() is presently unused
                    // Pass null to fast-fail if this parameter is used in the future
                    this.Parent.UpdateChildren(this, null);
                }
            }
        }

        /// <summary>
        /// Is the tracked node still existed in the outline tree.
        /// </summary>
        private bool IsTrackedNodeAlive
        {
            get
            {
                return this.Parent.IsAlive;
            }
        }

        //prevent creating ChangeNotificationTracker without parent
        private ChangeNotificationTracker()
        {
        }

        public ChangeNotificationTracker(TreeViewItemViewModel parent, ModelProperty parentProperty)
        {
            if (parent == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("parent"));
            }
            if (parentProperty == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("parentProperty"));
            }
            this.Parent = parent;
            this.ParentProperty = parentProperty;
            this.TrackedModelItem = new Dictionary<ModelItem, HashSet<string>>();
            this.TrackedCollection = new List<INotifyCollectionChanged>();
            this.ChildViewModels = new List<TreeViewItemViewModel>();
        }

        public void Add(ModelItem modelItem, ModelProperty property)
        {
            this.Add(modelItem, property.Name);
        }

        public void Add(ModelItem modelItem, string propertyName)
        {
            HashSet<string> propertyList = null;
            if (!TrackedModelItem.TryGetValue(modelItem, out propertyList))
            {
                modelItem.PropertyChanged += new ComponentModel.PropertyChangedEventHandler(modelItem_PropertyChanged);
                propertyList = new HashSet<string>();
                TrackedModelItem.Add(modelItem, propertyList);
            }
            propertyList.Add(propertyName);
        }

        public void AddCollection(INotifyCollectionChanged collection)
        {
            this.TrackedCollection.Add(collection);
            collection.CollectionChanged += new Collections.Specialized.NotifyCollectionChangedEventHandler(collection_CollectionChanged);
        }

        void collection_CollectionChanged(object sender, Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!IsTrackedNodeAlive)
            {
                return;
            }

            this.UpdateChildren(e);
        }

        public void CleanUp()
        {
            foreach (ModelItem modelItem in TrackedModelItem.Keys)
            {
                modelItem.PropertyChanged -= new ComponentModel.PropertyChangedEventHandler(modelItem_PropertyChanged);
            }
            TrackedModelItem.Clear();
            foreach (INotifyCollectionChanged collection in TrackedCollection)
            {
                collection.CollectionChanged -= new Collections.Specialized.NotifyCollectionChangedEventHandler(collection_CollectionChanged);
            }
            TrackedCollection.Clear();
            //remove childViewModels
            foreach (TreeViewItemViewModel child in ChildViewModels)
            {
                this.Parent.InternalChildren.Remove(child);
                child.CleanUp();
            }
            this.ChildViewModels.Clear();
        }

        void modelItem_PropertyChanged(object sender, ComponentModel.PropertyChangedEventArgs e)
        {
            if (!IsTrackedNodeAlive)
            {
                return;
            }

            ModelItem modelItem = sender as ModelItem;
            if (modelItem != null)
            {
                HashSet<string> propertyList = null;
                if (TrackedModelItem.TryGetValue(modelItem, out propertyList))
                {
                    if (propertyList.Contains(e.PropertyName))
                    {
                        this.UpdateChildren(e);
                    }
                }
            }
        }

        void UpdateChildren(EventArgs e)
        {
            if (null == this.DelayUpdates)
            {
                this.DelayUpdates = false;
                this.Parent.UpdateChildren(this, e);
                this.DelayUpdates = null;
            }
            else
            {
                // Called while handling another event for this tracker
                this.DelayUpdates = true;
            }
        }
    }
}
