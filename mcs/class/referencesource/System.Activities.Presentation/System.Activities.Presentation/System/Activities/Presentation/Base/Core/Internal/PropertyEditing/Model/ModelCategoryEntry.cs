//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Model 
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation;

    // <summary>
    // Cider's concrete implementation of CategoryEntry (CategoryBase comes from Sparkle
    // and it has a few extra goodies that we want to reuse).  This class implements
    // INotifyCollectionChanged.  We need to push this implementation to the base class
    // in v2.
    // </summary>
    internal class ModelCategoryEntry : CategoryBase, INotifyCollectionChanged 
    {

        private ObservableCollectionWorkaround<PropertyEntry> _basicProperties;
        private ObservableCollectionWorkaround<PropertyEntry> _advancedProperties;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="categoryName">Localized name for this category</param>
        public ModelCategoryEntry(string categoryName) : base(categoryName) 
        {
            _basicProperties = new ObservableCollectionWorkaround<PropertyEntry>();
            _advancedProperties = new ObservableCollectionWorkaround<PropertyEntry>();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        // <summary>
        // Gets the advanced properties contained in this category
        // </summary>
        public override ObservableCollection<PropertyEntry> AdvancedProperties 
        {
            get {
                return _advancedProperties;
            }
        }

        // <summary>
        // Gets the basic properties contained in this category
        // </summary>
        public override ObservableCollection<PropertyEntry> BasicProperties 
        {
            get {
                return _basicProperties;
            }
        }

        // <summary>
        // Gets a flag indicating whether this category contains any properties
        // </summary>
        internal bool IsEmpty 
        {
            get {
                return _advancedProperties.Count + _basicProperties.Count == 0;
            }
        }

        // <summary>
        // Returns either the basic or the advanced bucket based on the IsAdvanced flag
        // set in the PropertyEntry itself
        // </summary>
        // <param name="property">Property to examine</param>
        // <returns>The corresponding basic or advanced bucket</returns>
        internal ObservableCollectionWorkaround<PropertyEntry> GetBucket(PropertyEntry property) 
        {
            if (property == null) 
            {
                throw FxTrace.Exception.ArgumentNull("property");
            }
            return property.IsAdvanced ? _advancedProperties : _basicProperties;
        }

        // <summary>
        // Adds the given property to the specified property bucket (use
        // ModelCategoryEntry.BasicProperties, ModelCategoryEntry.AdvancedProperties, or
        // ModelCategoryEntry.GetBucket()) sorted using the specified comparer.
        // </summary>
        // <param name="property">Property to add</param>
        // <param name="bucket">Property bucket to populate</param>
        // <param name="comparer">Sort algorithm to use</param>
        // <param name="fireCollectionChangedEvent">If set to true, NotifyCollectionChanged event is fired</param>
        internal void Add(
            PropertyEntry property,
            ObservableCollection<PropertyEntry> bucket,
            IComparer<PropertyEntry> comparer) 
        {
            Add(property, bucket, comparer, true);
        }

        //
        // Adds the given property to the specified property bucket (use
        // ModelCategoryEntry.BasicProperties, ModelCategoryEntry.AdvancedProperties, or
        // ModelCategoryEntry.GetBucket()) sorted using the specified comparer.
        //
        private void Add(
            PropertyEntry property,
            ObservableCollection<PropertyEntry> bucket,
            IComparer<PropertyEntry> comparer,
            bool fireCollectionChangedEvent) 
        {

            if (property == null) 
            {
                throw FxTrace.Exception.ArgumentNull("property");
            }
            if (bucket == null) 
            {
                throw FxTrace.Exception.ArgumentNull("bucket");
            }
            if (comparer == null) 
            {
                throw FxTrace.Exception.ArgumentNull("comparer");
            }

            ObservableCollectionWorkaround<PropertyEntry> castBucket = bucket as ObservableCollectionWorkaround<PropertyEntry>;
            int insertionIndex = 0;

            if (castBucket == null) 
            {
                Debug.Fail("Invalid property bucket.  The property sort order will be broken.");
            }
            else 
            {
                insertionIndex = castBucket.BinarySearch(property, comparer);
                if (insertionIndex < 0) 
                {
                    insertionIndex = ~insertionIndex;
                }
            }

            bucket.Insert(insertionIndex, property);

            if (fireCollectionChangedEvent)
            {
                FirePropertiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, property));
            }
        }

        // <summary>
        // Removes and re-adds the specified property from this category, if it existed
        // there to begin with.  Noop otherwise.
        //
        // Use this method to refresh the cate----zation of a property if it suddenly
        // becomes Advanced if it was Basic before, or if its IsBrowsable status changes.
        // </summary>
        // <param name="property">Property to refresh</param>
        // <param name="bucket">Property bucket to repopulate</param>
        // <param name="sortComparer">Comparer to use to reinsert the given property in its new place</param>
        internal void Refresh(ModelPropertyEntry property, ObservableCollection<PropertyEntry> bucket, IComparer<PropertyEntry> sortComparer) 
        {
            if (property == null) 
            {
                throw FxTrace.Exception.ArgumentNull("property");
            }
            if (bucket != _basicProperties && bucket != _advancedProperties) 
            {
                Debug.Fail("Invalid bucket specified.  Property was not refreshed.");
                return;
            }

            // Let's see if we know about this property
            ObservableCollectionWorkaround<PropertyEntry> collection;
            collection = _advancedProperties;

            int index = collection.BinarySearch(property, null);
            if (index < 0) 
            {
                collection = _basicProperties;
                index = collection.BinarySearch(property, null);
            }

            // If not, noop
            if (index < 0)
            {
                return;
            }

            // We know about this property, so refresh it.  It may have changed
            // somehow (eg. switched from basic to advanced, become hidden, etc.)
            // so make sure it's thrown into the right bucket.
            collection.RemoveAt(index);
            Add(property, bucket, sortComparer, false);
        }

        // <summary>
        // This is a work-around fix because Blend's CategoryBase does not handle null filters (valid value)
        // correctly.  We need to ask Blend to eventually fix this issue.
        // </summary>
        // <param name="filter">Filter to apply, can be null</param>
        public override void ApplyFilter(PropertyFilter filter) 
        {
            if (filter == null) 
            {
                this.MatchesFilter = true;
                this.BasicPropertyMatchesFilter = true;
                this.AdvancedPropertyMatchesFilter = true;

                foreach (PropertyEntry property in this.BasicProperties)
                {
                    property.ApplyFilter(filter);
                }

                foreach (PropertyEntry property in this.AdvancedProperties)
                {
                    property.ApplyFilter(filter);
                }
            }
            else 
            {
                base.ApplyFilter(filter);
            }
        }

        // Another Blend work-around - we expose all properties through the OM, not just the
        // Browsable ones.  However, as a result, we need to cull the non-browsable ones from
        // consideration.  Otherwise, empty categories may appear.
        protected override bool DoesPropertyMatchFilter(PropertyFilter filter, PropertyEntry property) 
        {
            property.ApplyFilter(filter);

            bool isBrowsable = true;
            ModelPropertyEntry modelPropertyEntry = property as ModelPropertyEntry;
            if (modelPropertyEntry != null)
            {
                //display given property if it is browsable or
                isBrowsable = modelPropertyEntry.IsBrowsable || 
                    // it may not be browsable, but if there is a category editor associated - display it anyway
                    (this.CategoryEditors != null && this.CategoryEditors.Count != 0);
            }

            return isBrowsable && property.MatchesFilter;
        }

        // <summary>
        // Sets the Disassociated flag on all contained properties to True
        // </summary>
        internal void MarkAllPropertiesDisassociated() 
        {
            MarkAllPropertiesDisassociated(_basicProperties);
            MarkAllPropertiesDisassociated(_advancedProperties);
        }

        // <summary>
        // Sets the Disassociated flag on all contained attached properties to True
        // </summary>
        internal void MarkAttachedPropertiesDisassociated() 
        {
            MarkAttachedPropertiesDisassociated(_basicProperties);
            MarkAttachedPropertiesDisassociated(_advancedProperties);
        }

        // <summary>
        // Removes all properties from this category whose Disassociated flag is set to True
        // </summary>
        internal void CullDisassociatedProperties() 
        {
            bool propertiesCulled = false;
            propertiesCulled |= CullDisassociatedProperties(_basicProperties);
            propertiesCulled |= CullDisassociatedProperties(_advancedProperties);

            if (propertiesCulled)
            {
                FirePropertiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private static void MarkAllPropertiesDisassociated(ObservableCollectionWorkaround<PropertyEntry> propertyList) 
        {
            foreach (ModelPropertyEntry property in propertyList)
            {
                property.Disassociated = true;
            }
        }

        private static void MarkAttachedPropertiesDisassociated(ObservableCollectionWorkaround<PropertyEntry> propertyList) 
        {
            foreach (ModelPropertyEntry property in propertyList)
            {
                if (property.IsAttached)
                {
                    property.Disassociated = true;
                }
            }
        }

        private static bool CullDisassociatedProperties(ObservableCollectionWorkaround<PropertyEntry> propertyList) 
        {
            bool propertiesCulled = false;
            for (int i = propertyList.Count - 1; i >= 0; i--) 
            {
                ModelPropertyEntry property = (ModelPropertyEntry)propertyList[i];
                if (property.Disassociated) 
                {
                    property.Disconnect();
                    propertyList.RemoveAt(i);
                    propertiesCulled = true;
                }
            }

            return propertiesCulled;
        }

        // INotifyCollectionChanged Members

        private void FirePropertiesChanged(NotifyCollectionChangedEventArgs collectionChangedEventArgs) 
        {
            // Fire both "Properties" changed events
            OnPropertyChanged("Properties");
            OnPropertyChanged("Item[]");

            // as well as the appropriate collection-changed event
            if (CollectionChanged != null)
            {
                CollectionChanged(this, collectionChangedEventArgs);
            }
        }

    }
}
