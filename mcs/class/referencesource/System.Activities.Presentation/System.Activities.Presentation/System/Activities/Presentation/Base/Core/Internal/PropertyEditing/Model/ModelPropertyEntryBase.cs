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
    using System.Text;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Services;

    // <summary>
    // Cider-specific base class for PropertyEntry.  It is shared by both
    // ModelPropertyEntry and ModelPropertyIndexer.  ModelPropertyEntry is used
    // to model regular properties that are backed by Cider's ModelProperty.
    // ModelPropertyIndexers are used to model items in collections that are
    // backed by Cider's ModelItems.
    // </summary>
    internal abstract class ModelPropertyEntryBase : PropertyEntry 
    {

        // Cache the depth of this property because once a property entry is create
        // it doesn't jump levels.  Depth is only used to track sub-property tree's
        // not collection trees.
        private int _depth;
        private string _propertyPath;

        protected ModelPropertyEntryBase() : this(null) 
        {
        }
        protected ModelPropertyEntryBase(PropertyValue parentValue) : base(parentValue) 
        {
            UpdateDepth();
        }

        public abstract PropertyValueSource Source 
        { get; }
        public abstract bool IsMixedValue 
        { get; }
        public abstract Type CommonValueType 
        { get; }
        public abstract TypeConverter Converter 
        { get; }

        // <summary>
        // Gets a flag indicating whether this property exposes any sub-properties.
        // We rely on TypeConverter.GetPropertiesSupported() for this value.
        // </summary>
        public bool HasSubProperties 
        {
            get {
                return this.PropertyValue.Value != null && this.Converter != null &&
                    this.Converter.GetPropertiesSupported() && !this.IsMarkupExtension;
            }
        }

        public abstract PropertyEntryCollection SubProperties 
        { get; }

        // <summary>
        // Gets a flag indicating whether the type of the contained property
        // can be assigned to an IList
        // </summary>
        public bool IsCollection 
        {
            get {
                return typeof(IList).IsAssignableFrom(this.PropertyType);
            }
        }
        public abstract PropertyValueCollection Collection 
        { get; }

        // <summary>
        // Gets the depth of this property in the PI sub-property tree
        // </summary>
        public virtual int Depth 
        {
            get {
                return _depth;
            }
        }

        // <summary>
        // Gets a ',' separated path of this property through its
        // sub-property hierarchy.
        // </summary>
        public string SubPropertyHierarchyPath 
        {
            get {
                if (_propertyPath == null)
                {
                    _propertyPath = ModelUtilities.GetSubPropertyHierarchyPath(this);
                }

                return _propertyPath;
            }
        }

        // <summary>
        // Checks to see if the value of this property comes from a MarkupExtension
        // </summary>
        internal bool IsMarkupExtension 
        {
            get {
                DependencyPropertyValueSource source = this.Source as DependencyPropertyValueSource;
                return source != null && source.IsExpression;
            }
        }

        internal abstract bool CollectionInstanceExists 
        { get; }

        // <summary>
        // Convenience accessor
        // </summary>
        protected ModelPropertyValue ModelPropertyValue 
        {
            get {
                return (ModelPropertyValue)this.PropertyValue;
            }
        }
        public abstract object GetValueCore();
        public abstract void SetValueCore(object value);
        public abstract void ClearValue();

        // Calculate the depth of this property in the sub-property
        // hierarchy
        //
        private void UpdateDepth() 
        {
            if (ParentValue != null)
            {
                _depth = ((ModelPropertyEntryBase)ParentValue.ParentProperty).Depth + 1;
            }
        }

        // <summary>
        // Called when one of the sub-properties exposed by this class changes.
        // There is a call to the concrete implementation of this class so that it
        // can do any internal cache clean up as needed, followed by the firing
        // of the appropriate changed events.
        // </summary>
        public void OnUnderlyingSubModelChanged() 
        {
            OnUnderlyingSubModelChangedCore();
            this.ModelPropertyValue.OnUnderlyingSubModelChanged();
        }

        // <summary>
        // Called when one of the sub-properties exposed by this class changes
        // that allows the concrete implementation of this class to clean up
        // any internal state.
        // </summary>
        protected abstract void OnUnderlyingSubModelChangedCore();

        // <summary>
        // Clears or updates any cached values - call this method
        // when the underlying ModelProperty changes and cached values
        // may have become invalid
        // </summary>
        protected virtual void RefreshCache() 
        {
            UpdateDepth();
            _propertyPath = null;
        }

        internal abstract ModelEditingScope BeginEdit(string description);
    }
}
