//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Model 
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.Properties;

    // <summary>
    // ModelPropertyIndexer is used to represent ModelItems in a collection.  As such
    // and unlike ModelProperty, the class wraps around a single ModelItem instead of
    // one or more ModelProperties.
    // </summary>
    internal class ModelPropertyIndexer : ModelPropertyEntryBase 
    {

        private static readonly ICollection EmptyCollection = new object[0];

        private ModelItem _modelItem;
        private int _index;
        private ModelPropertyValueCollection _parentCollection;

        private CachedValues _valueCache;

        // <summary>
        // Basic ctor.  Note, however, that this class should only be created by ModelPropertyValueCollection
        // as that class ensures that the new instance is correctly added and removed from the
        // ModelItemMap.
        // </summary>
        // <param name="modelItem">ModelItem to wrap around</param>
        // <param name="index">Index of the ModelItem in the collection</param>
        // <param name="parentCollection">Parent collection</param>
        public ModelPropertyIndexer(
            ModelItem modelItem,
            int index,
            ModelPropertyValueCollection parentCollection)
            : base(parentCollection.ParentValue) 
        {

            if (modelItem == null)
            {
                throw FxTrace.Exception.ArgumentNull("modelItem");
            }
            if (parentCollection == null)
            {
                throw FxTrace.Exception.ArgumentNull("parentCollection");
            }

            _modelItem = modelItem;
            _index = index;
            _parentCollection = parentCollection;
            _valueCache = new CachedValues(this);
        }


        // <summary>
        // Gets the index of the underlying ModelItem.  If index &lt; 0, this
        // ModelPropertyIndexer no longer belongs to a collection and setting its value
        // will fail.
        // </summary>
        public int Index 
        {
            get {
                return _index;
            }
            internal set {
                _index = value;
            }
        }

        // <summary>
        // Gets the name category name of the parent collection
        // </summary>
        public override string CategoryName 
        {
            get {
                return _parentCollection.ParentValue.ParentProperty.CategoryName;
            }
        }

        // <summary>
        // Gets the description of the parent collection
        // </summary>
        public override string Description 
        {
            get {
                return _parentCollection.ParentValue.ParentProperty.Description;
            }
        }

        // <summary>
        // Gets the IsAdvanced flag of the parent collection
        // </summary>
        public override bool IsAdvanced 
        {
            get {
                return _parentCollection.ParentValue.ParentProperty.IsAdvanced;
            }
        }

        // <summary>
        // Returns true
        // </summary>
        public override bool IsReadOnly 
        {
            get { return true; }
        }

        // <summary>
        // Gets the index of this item as string
        // </summary>
        public override string PropertyName 
        {
            get {
                return _index.ToString(CultureInfo.InvariantCulture);
            }
        }

        // <summary>
        // Gets the type of items in the parent collection
        // </summary>
        public override Type PropertyType 
        {
            get {
                return _modelItem.ItemType;
            }
        }

        // <summary>
        // Returns null because there are no ValueEditors for values that belong to a collection
        // </summary>
        public override PropertyValueEditor PropertyValueEditor 
        {
            get {
                // There are no ValueEditors for items in a collection
                return null;
            }
        }

        // <summary>
        // Returns an empty collection - there are no StandardValues for items in a collection
        // </summary>
        public override ICollection StandardValues 
        {
            get {
                // There are no StandardValues for items in a collection
                return EmptyCollection;
            }
        }

        // <summary>
        // Returns false - ModelPropertyIndexers always wrap around a single ModelItem
        // </summary>
        public override bool IsMixedValue 
        {
            get { return false; }
        }

        // <summary>
        // Returns Local - this PropertyEntry always contains a collection item value which is local
        // </summary>
        public override PropertyValueSource Source 
        {
            get { return DependencyPropertyValueSource.Local; }
        }

        // <summary>
        // Gets the TypeConverter
        // </summary>
        public override TypeConverter Converter 
        {
            get {
                return _valueCache.Converter;
            }
        }

        // <summary>
        // Gets the Type of the contained ModelItem
        // </summary>
        public override Type CommonValueType 
        {
            get {
                return _modelItem.ItemType;
            }
        }

        // <summary>
        // Gets the sub-properties of the underlying item
        // </summary>
        public override PropertyEntryCollection SubProperties 
        {
            get {
                return _valueCache.SubProperties;
            }
        }

        // <summary>
        // Gets the collection of the underlying ModelItem
        // </summary>
        public override PropertyValueCollection Collection 
        {
            get {
                return _valueCache.Collection;
            }
        }

        // <summary>
        // Gets the depth of this property in the PI sub-property tree.
        // Since this class represents an item in the collection, it's depth
        // resets to -1 so that it's children start at depth 0 ( -1 + 1 = 0) again.
        // </summary>
        public override int Depth 
        {
            get {
                return -1;
            }
        }


        // <summary>
        // Gets the underlying ModelItem
        // </summary>
        internal ModelItem ModelItem 
        {
            get {
                return _modelItem;
            }
        }

        // <summary>
        // Gets a flag indicating whether the underlying collection instance has already been
        // initialized.  Optimization.
        // </summary>
        internal override bool CollectionInstanceExists 
        {
            get {
                return _valueCache.CollectionInstanceExists;
            }
        }

        // <summary>
        // Creates a new ModelPropertyValue instance
        // </summary>
        // <returns>New ModelPropertyValue instance</returns>
        protected override PropertyValue CreatePropertyValueInstance() 
        {
            return new ModelPropertyValue(this);
        }

        // <summary>
        // Gets the actual object instance respresented by this class
        // </summary>
        // <returns>Actual object instance respresented by this class</returns>
        public override object GetValueCore() 
        {
            return _modelItem.GetCurrentValue();
        }

        // <summary>
        // Sets the value of the collection item at the same position as the
        // ModelItem represented by this class.  Identical to removing the old
        // item and adding a new one
        // </summary>
        // <param name="value">Value to set</param>
        public override void SetValueCore(object value) 
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(Resources.PropertyEditing_ErrorSetValueOnIndexer));
        }

        // <summary>
        // Throws an exception -- invalid operation
        // </summary>
        public override void ClearValue() 
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(Resources.PropertyEditing_ClearIndexer));
        }

        // <summary>
        // Opens a new ModelEditingScope with the specified description.
        // </summary>
        // <param name="description">Change description (may be null).</param>
        // <returns>New, opened ModelEditingScope with the specified description</returns>
        internal override ModelEditingScope BeginEdit(string description) 
        {
            return description == null ? _modelItem.BeginEdit() : _modelItem.BeginEdit(description);
        }

        // <summary>
        // Called when one of the sub-properties exposed by this class changes
        // </summary>
        protected override void OnUnderlyingSubModelChangedCore() 
        {
            // Do nothing.  There is nothing in CachedValues right now that would need to
            // be refreshed as a result of one of our sub-properties changing value
        }

        // Cached values that need to be nixed when the underlying ModelItem changes
        // (ie. someone calls SetValueCore()).  Pretty much everything in here is an "expensive"
        // calculation which requires us to evaluate some attributes associated with the given
        // property of set of properties, so we cache the return values and keep that cache
        // in a single place so that it's easy to know what needs to be ----d when the underlying
        // ModelItem changes.
        private class CachedValues 
        {

            private static readonly TypeConverter NoTypeConverter = new TypeConverter();

            private ModelPropertyIndexer _parent;

            private TypeConverter _converter;
            private ModelPropertyEntryCollection _subProperties;
            private ModelPropertyValueCollection _collection;

            public CachedValues(ModelPropertyIndexer indexer) 
            {
                _parent = indexer;
            }

            public TypeConverter Converter 
            {
                get {
                    if (_converter == null) 
                    {
                        _converter = ExtensibilityAccessor.GetTypeConverter(_parent._modelItem);
                        _converter = _converter ?? NoTypeConverter;
                    }

                    return _converter == NoTypeConverter ? null : _converter;
                }
            }

            public ModelPropertyEntryCollection SubProperties 
            {
                get {
                    if (_subProperties == null)
                    {
                        _subProperties = new ModelPropertyEntryCollection(_parent);
                    }

                    return _subProperties;
                }
            }

            public bool CollectionInstanceExists 
            {
                get {
                    return _collection != null;
                }
            }

            public ModelPropertyValueCollection Collection 
            {
                get {
                    if (_collection == null)
                    {
                        _collection = new ModelPropertyValueCollection(_parent.ModelPropertyValue);
                    }

                    return _collection;
                }
            }
        }
    }
}
