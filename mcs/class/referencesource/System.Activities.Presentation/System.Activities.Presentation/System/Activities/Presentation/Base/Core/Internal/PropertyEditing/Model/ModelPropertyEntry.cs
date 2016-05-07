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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;

    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Services;

    using System.Activities.Presentation.Internal.Properties;
    using System.Runtime;
    using System.Activities.Presentation.Internal.PropertyEditing.Editors;

    // <summary>
    // ModelPropertyEntry is a wrapper around Cider's ModelProperty and that
    // exposes its functionality through the PropertyEntry object model.  It handles
    // all get / set / clear functionality.
    // </summary>
    internal class ModelPropertyEntry : ModelPropertyEntryBase, IComparable 
    {
        private const string _textBlockInlinesPropertyName = "Inlines";

        // Property names for TextBlock properties that require special handling
        private static string _textBlockTextPropertyName = System.Windows.Controls.TextBlock.TextProperty.Name;

        // Cached values that need to be nixed when the underlying ModelProperty changes
        // (ie. someone calls SetProperty())
        private CachedValues _valueCache;

        // List of ModelProperties that this instance wraps around.  It
        // is guaranteed to contain at least one ModelProperty instance (single
        // selection scenario), but it may contain multiple ModelProperty instances
        // (multi-select scenario)
        private List<ModelProperty> _properties = new List<ModelProperty>();

        // Flag indicating whether this instance points to something valid.
        // Used both as a perf optimization from PropertyInspector.UpdateCategories()
        // as well as to disable the making of changes to ModelPropertyEntries
        // when the underlying ModelProperties are no longer available.
        private bool _disassociated;

        // Bool indicating whether this property is a wrapper around the Name property
        // (which we special case for display purposes)
        private bool _wrapsAroundNameProperty;

        // <summary>
        // Basic ctor that wraps around a single ModelProperty
        // </summary>
        // <param name="property">ModelProperty to wrap around</param>
        // <param name="parentValue">Parent PropertyValue, if any</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ModelPropertyEntry(ModelProperty property, ModelPropertyValue parentValue)
            : base(parentValue) 
        {

            _valueCache = new CachedValues(this);
            SetUnderlyingModelPropertyHelper(property, false);
        }

        // <summary>
        // Basic ctor that wraps around multiple ModelProperties in the
        // multi-select scenario.  The code assumes that the ModelProperties in
        // the set all represent the same property (eg. Background) across different
        // ModelItems (eg. Button, Grid, and ComboBox).
        // </summary>
        // <param name="propertySet">Set of ModelProperties to wrap around</param>
        // <param name="parentValue">Parent PropertyValue, if any</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ModelPropertyEntry(IEnumerable<ModelProperty> propertySet, ModelPropertyValue parentValue)
            : base(parentValue) 
        {

            _valueCache = new CachedValues(this);
            SetUnderlyingModelPropertyHelper(propertySet, false);
        }


        // <summary>
        // Gets the name of the contained property
        // </summary>
        public override string PropertyName 
        {
            get {
                return _properties[0].Name;
            }
        }

        // <summary>
        // Gets the display name of the contained property, if any.
        // Defaults to property name if none is found.
        // </summary>
        public override string DisplayName 
        {
            get {
                return _valueCache.DisplayName;
            }
        }

        // <summary>
        // Gets the type of the contained property
        // </summary>
        public override Type PropertyType 
        {
            get {
                return _properties[0].PropertyType;
            }
        }

        // <summary>
        // Gets the category name of the contained property
        // </summary>
        public override string CategoryName 
        {
            get {
                return _valueCache.CategoryName;
            }
        }

        // <summary>
        // Gets the description of the contained property
        // </summary>
        public override string Description 
        {
            get {
                return _valueCache.Description;
            }
        }

        // <summary>
        // Gets a flad indicating whether the property is read-only
        // </summary>
        public override bool IsReadOnly 
        {
            get {
                return _valueCache.IsReadOnly;
            }
        }

        // <summary>
        // Gets a flag indicating whether the property is advanced
        // </summary>
        public override bool IsAdvanced 
        {
            get {
                return _valueCache.IsAdvanced;
            }
        }

        // <summary>
        // Gets a flag indicating whether this property is browsable or not
        // (All properties are exposed through the object model.  It's up to the
        // UI to make the display / don't-display decision)
        // </summary>
        public bool IsBrowsable 
        {
            get {
                return _valueCache.IsBrowsable;
            }
        }

        // <summary>
        // Gets a collection of standard values that can be assigned to
        // the property
        // </summary>
        public override ICollection StandardValues 
        {
            get {
                return _valueCache.StandardValues;
            }
        }

        // <summary>
        // Gets a flag indicating whether the list of StandardValues is complete
        // or whether the user can type a value that's different from the ones in the list
        // Note: this property is referenced from XAML
        // </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool StandardValuesExclusive 
        {
            get {
                return _valueCache.StandardValuesExclusive;
            }
        }

        // <summary>
        // Gets the PropertyValueEditor for this property
        // </summary>
        public override PropertyValueEditor PropertyValueEditor 
        {
            get {
                return _valueCache.PropertyValueEditor;
            }
        }

        // <summary>
        // Gets the TypeConverted for the contained property
        // </summary>
        public override TypeConverter Converter 
        {
            get {
                return _valueCache.Converter;
            }
        }

        // <summary>
        // Gets the value Type for all contained properties if it matches,
        // null otherwise
        // </summary>
        public override Type CommonValueType 
        {
            get {
                return _valueCache.CommonValueType;
            }
        }

        // <summary>
        // Returns true if the contained ModelProperties don't
        // share the same value
        // </summary>
        public override bool IsMixedValue 
        {
            get {
                return _valueCache.IsMixedValue;
            }
        }

        // <summary>
        // Gets the source of the value contained by this property
        // </summary>
        public override PropertyValueSource Source 
        {
            get {
                return _valueCache.Source;
            }
        }

        // <summary>
        // Gets the sub-properties of the contained property
        // </summary>
        public override PropertyEntryCollection SubProperties 
        {
            get {
                return _valueCache.SubProperties;
            }
        }

        // <summary>
        // Gets the collection of PropertyValues if this ModelProperty represents
        // a collection
        // </summary>
        public override PropertyValueCollection Collection 
        {
            get {
                return _valueCache.Collection;
            }
        }

        // <summary>
        // Gets a flag indicating whether the collection instance has already
        // been instantiated (perf optimization)
        // </summary>
        internal override bool CollectionInstanceExists 
        {
            get {
                return _valueCache.CollectionInstanceExists;
            }
        }

        // <summary>
        // Gets the underlying collection of ModelProperties
        // </summary>
        internal ICollection<ModelProperty> ModelPropertySet 
        {
            get {
                return _properties;
            }
        }

        // <summary>
        // Gets the first underlying ModelProperty for cases when
        // this class wraps around multiple
        // </summary>
        internal ModelProperty FirstModelProperty 
        {
            get {
                return _properties[0];
            }
        }

        // <summary>
        // Gets an order token for property ordering
        // </summary>
        internal PropertyOrder PropertyOrder 
        {
            get {
                return _valueCache.PropertyOrder;
            }
        }

        // <summary>
        // Gets or sets the disassociated flag
        // </summary>
        internal bool Disassociated 
        {
            get { return _disassociated; }
            set { _disassociated = value; }
        }

        // <summary>
        // Gets a flag indicating whether this instance represents an attached DP
        // </summary>
        internal bool IsAttached 
        {
            get {
                return _valueCache.IsAttached;
            }
        }

        // <summary>
        // Gets a list of CategoryEditor types associated with this PropertyEntry
        // </summary>
        internal IEnumerable<Type> CategoryEditorTypes 
        {
            get {
                return _valueCache.CategoryEditorTypes;
            }
        }


        // <summary>
        // Returns true if there are standard values for this property.
        // </summary>
        protected override bool HasStandardValues 
        {
            get { return _valueCache.StandardValuesSupported; }
        }

        // <summary>
        // Replaces the underlying ModelProperty/ies with the specified ModelProperties.
        // Fires the appropriate PropertyChanged events
        // </summary>
        // <param name="propertySet">Property set to wrap around</param>
        public void SetUnderlyingModelProperty(IEnumerable<ModelProperty> propertySet) 
        {
            SetUnderlyingModelPropertyHelper(propertySet, true);
        }

        private void SetUnderlyingModelPropertyHelper(ModelProperty property, bool firePropertyValueChangedEvents) 
        {
            if (property == null)
            {
                throw FxTrace.Exception.ArgumentNull("property");
            }

            // Store the value
            ClearUnderlyingModelProperties();
            AddUnderlyingModelProperty(property);

            // Clear any cached values
            RefreshCache();

            if (firePropertyValueChangedEvents) 
            {
                // Update the PropertyValue (always, except when it doesn't exist yet (ctor time))
                this.ModelPropertyValue.OnUnderlyingModelChanged();
            }
        }

        private void SetUnderlyingModelPropertyHelper(IEnumerable<ModelProperty> propertySet, bool firePropertyValueChangedEvents) 
        {
            if (propertySet == null)
            {
                throw FxTrace.Exception.ArgumentNull("propertySet");
            }

            // Attempt to store the values
            int count = 0;
            foreach (ModelProperty property in propertySet) 
            {
                if (property == null)
                {
                    continue;
                }

                if (count == 0)
                {
                    ClearUnderlyingModelProperties();
                }

                AddUnderlyingModelProperty(property);
                count++;
            }

            // Throw if the underlying property set was invalid
            if (count == 0)
            {
                throw FxTrace.Exception.AsError(new ArgumentException("Cannot set the underlying ModelProperty to an empty set."));
            }

            // Clear any cached values
            RefreshCache();

            if (firePropertyValueChangedEvents) 
            {
                // Update the PropertyValue (always, except when it doesn't exist yet (ctor time))
                this.ModelPropertyValue.OnUnderlyingModelChanged();
            }
        }

        // Adds the property to the internal collection list and hooks into its PropertyChanged event
        private void AddUnderlyingModelProperty(ModelProperty property) 
        {
            if (property == null)
            {
                return;
            }

            property.Parent.PropertyChanged += new PropertyChangedEventHandler(OnUnderlyingPropertyChanged);
            _properties.Add(property);
            _wrapsAroundNameProperty = "Name".Equals(property.Name);
        }

        internal void Disconnect()
        {
            foreach (ModelProperty property in _properties)
            {
                property.Parent.PropertyChanged -= new PropertyChangedEventHandler(OnUnderlyingPropertyChanged);
            }
        }

        // Removes all properties from the internal collection and unhooks from their PropertyChanged events
        private void ClearUnderlyingModelProperties() 
        {
            foreach (ModelProperty property in _properties) 
            {
                property.Parent.PropertyChanged -= new PropertyChangedEventHandler(OnUnderlyingPropertyChanged);
            }

            _properties.Clear();
            _wrapsAroundNameProperty = false;
        }

        // Event handler for PropertyChanged event.  Called whenever any of the underlying properties that
        // this ModelPropertyEntry wraps around changes.
        private void OnUnderlyingPropertyChanged(object sender, PropertyChangedEventArgs e) 
        {
            if (!this.PropertyName.Equals(e.PropertyName))
            {
                return;
            }

            this.OnUnderlyingModelChanged();

            // If this property is a sub-property of some other property we know and care
            // about, notify the parents as well
            PropertyValue parentValue = this.ParentValue;
            while (parentValue != null) 
            {
                ModelPropertyEntryBase parentProperty = (ModelPropertyEntryBase)parentValue.ParentProperty;
                parentProperty.OnUnderlyingSubModelChanged();
                parentValue = parentProperty.ParentValue;
            }
        }

        // <summary>
        // Clear any cached values
        // </summary>
        protected override void RefreshCache() 
        {
            base.RefreshCache();
            _valueCache.ClearAll();
        }

        // <summary>
        // Gets the underlying value as an object instance.  Mixed values will
        // return null.
        // </summary>
        // <returns>Underlying value contained by this property.</returns>
        public override object GetValueCore() 
        {
            if (this.IsMixedValue)
            {
                return null;
            }

            object retValue = ModelUtilities.GetSafeComputedValue(_properties[0]);

            return retValue;
        }

        // <summary>
        // Sets the value of the underlying property / ies.
        // </summary>
        // <param name="value">Value to set</param>
        public override void SetValueCore(object value) 
        {
            // If this ModelPropertyEntry instance is no longer hooked up into
            // the underlying model, ignore calls to SetValueCore()
            if (_disassociated)
            {
                return;
            }

            bool textBlockTextHackNeeded = false;
            List<ModelProperty> textBlockTextProperties = null;
            if (typeof(System.Windows.Controls.TextBlock).IsAssignableFrom(_properties[0].Parent.ItemType)) {
                textBlockTextHackNeeded = true;
            }

            // POSSIBLE OPTIMIZATION: remember which properties were altered.  When on Idle we
            // receive global property changed events, ignore the ones we know about
            using (ModelEditingScope group = _properties[0].Parent.BeginEdit(
                string.Format(
                CultureInfo.CurrentCulture,
                Resources.PropertyEditing_UndoText,
                this.DisplayName))) 
            {

                for (int i = 0; i < _properties.Count; i++) 
                {
                    if (textBlockTextHackNeeded && _properties[i].Name.Equals(_textBlockTextPropertyName)) {
                        // We need to set Text after we clear inlines!
                        if (textBlockTextProperties == null)
                        {
                            textBlockTextProperties = new List<ModelProperty>();
                        }
                        textBlockTextProperties.Add(_properties[i]);
                        continue;
                    }
                    _properties[i].SetValue(value);
                }

                // TextBlock has very bad IAddChild behavior with two properties contributing and having different
                // views into the content (Text and Inlines).  To simplify editing, we clear Inlines when Text is set
                // which is what most users want anyways
                if (textBlockTextProperties != null) 
                {
                    foreach (ModelProperty textBlockTextProperty in textBlockTextProperties) 
                    {
                        ModelProperty inlinesProperty = textBlockTextProperty.Parent.Properties[_textBlockInlinesPropertyName];
                        if (inlinesProperty != null && inlinesProperty.Collection != null)
                        {
                            inlinesProperty.Collection.Clear();
                        }
                        textBlockTextProperty.SetValue(value);
                    }
                }

                if (group != null)
                {
                    group.Complete();
                }
            }

            _valueCache.ClearValueRelatedCacheItems();
            NotifyParentOfNameChanged();
        }

        // <summary>
        // Clears the underlying property / ies.
        // </summary>
        public override void ClearValue() 
        {

            // If this ModelPropertyEntry instance is no longer hooked up into
            // the underlying model, ignore calls to ClearValue()
            if (_disassociated)
            {
                return;
            }

            // POSSIBLE OPTIMIZATION: remember which properties were altered.  When on Idle we
            // receive global property changed events, ignore the ones we know about

            using (ModelEditingScope group = _properties[0].Parent.BeginEdit(
                string.Format(
                CultureInfo.CurrentCulture,
                Resources.PropertyEditing_UndoText,
                this.DisplayName))) 
            {

                for (int i = 0; i < _properties.Count; i++) 
                {
                    _properties[i].ClearValue();
                }

                group.Complete();
            }

            _valueCache.ClearValueRelatedCacheItems();
            NotifyParentOfNameChanged();
        }

        // If this property happens to wrap around the "Name" property, give our parent
        // (if one exists) a heads-up that the value has changed.  We use this mechanism
        // to update display names of items in a collection editor.
        private void NotifyParentOfNameChanged() 
        {
            if (!_wrapsAroundNameProperty)
            {
                return;
            }

            ModelPropertyValue parentValue = this.ParentValue as ModelPropertyValue;
            if (parentValue == null)
            {
                return;
            }

            // This PropertyEntry is the Name sub-property of another PropertyValue,
            // so let our parent know that its name has changed.
            parentValue.OnNameSubPropertyChanged();
        }

        // <summary>
        // Called when the underlying ModelProperty changes.  Clears any cached
        // values and fires the appropriate changed events.
        // </summary>
        internal void OnUnderlyingModelChanged() 
        {
            _valueCache.ClearValueRelatedCacheItems();
            this.ModelPropertyValue.OnUnderlyingModelChanged();
        }

        // <summary>
        // Called when the sub-property of the underlying ModelProperty changes.
        // </summary>
        protected override void OnUnderlyingSubModelChangedCore() 
        {
            _valueCache.ClearSubValueRelatedCacheItems();
        }

        // <summary>
        // Creates new instance of ModelPropertyValue
        // </summary>
        // <returns>New instance of ModelPropertyValue</returns>
        protected override PropertyValue CreatePropertyValueInstance() 
        {
            return new ModelPropertyValue(this);
        }

        // <summary>
        // Opens a new ModelEditingScope
        // </summary>
        // <param name="description">Change description (may be null)</param>
        // <returns>A new, opened ModelEditingScope</returns>
        internal override ModelEditingScope BeginEdit(string description) 
        {
            return description == null ? FirstModelProperty.Parent.BeginEdit() : FirstModelProperty.Parent.BeginEdit(description);
        }

        // IPropertyFilterTarget Members

        // <summary>
        // IPropertyFilterTarget method.  We override the default behavior which matches
        // both property DisplayName as well as the property Type name.
        // </summary>
        // <param name="predicate">the predicate to match against</param>
        // <returns>true if there is a match</returns>
        public override bool MatchesPredicate(PropertyFilterPredicate predicate) 
        {
            return predicate == null ? false : predicate.Match(this.DisplayName);
        }


        // IComparable Members

        // <summary>
        // Compares 'this' with the object passed into it using the ModelPropertyEntryComparer,
        // which looks at both PropertyOrder as well as DisplayName to do the comparison
        // </summary>
        // <param name="obj">Object to compare this instance to</param>
        // <returns>Comparison result</returns>
        public int CompareTo(object obj) 
        {
            return PropertyEntryPropertyOrderComparer.Instance.Compare(this, obj);
        }


        // <summary>
        // Debuging-friendly ToString()
        // </summary>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override string ToString() 
        {
            try 
            {
                if (string.Equals(this.PropertyName, this.DisplayName)) 
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0} (PropertyEntry)", this.PropertyName);
                }
                else 
                {
                    return string.Format(CultureInfo.CurrentCulture, "{0} (\"{1}\" - PropertyEntry)", this.PropertyName, this.DisplayName);
                }
            }
            catch 
            {
                return base.ToString();
            }
        }

        // Cached values that need to be nixed when the underlying ModelProperty changes
        // (ie. someone calls SetProperty()).  Pretty much everything in here is an "expensive"
        // calculation which requires us to evaluate some attributes associated with the given
        // property or a set of properties, so we cache the return values and keep that cache
        // in a single place so that it's easy to know what needs to be ----d when the underlying
        // ModelProperties change.
        private class CachedValues 
        {

            private static readonly PropertyValueEditor NoPropertyValueEditor = new PropertyValueEditor();
            private static readonly PropertyOrder NoPropertyOrder = PropertyOrder.CreateAfter(PropertyOrder.Default);
            private static readonly TypeConverter NoTypeConverter = new TypeConverter();
            private static readonly ValueSerializer NoSerializer = new NoValueSerializer();
            private static readonly Type NoCommonValueType = typeof(CachedValues); // some private type that we can use as a marker
            private static readonly List<Type> NoCategoryEditorTypes = new List<Type>();
            private static readonly PropertyValueSource NoSource = new NoPropertyValueSource();

            private ModelPropertyEntry _parent;

            // Cached values
            private string _displayName;
            private string _categoryName;
            private string _description;
            private bool? _isAdvanced;
            private bool? _isBrowsable;
            private bool? _isReadOnly;
            private bool? _isAttached;
            private ArrayList _standardValues;
            private bool? _standardValuesExclusive;
            private bool? _standardValuesSupported;
            private PropertyValueEditor _propertyValueEditor;
            private bool? _isMixedValue;
            private PropertyValueSource _source;
            private ModelPropertyEntryCollection _subProperties;
            private ModelPropertyValueCollection _collection;
            private TypeConverter _converter;
            private ValueSerializer _valueSerializer;
            private Type _commonValueType;
            private PropertyOrder _propertyOrder;
            private IEnumerable<Type> _categoryEditorTypes;

            public CachedValues(ModelPropertyEntry parent) 
            {
                _parent = parent;
            }

            // <summary>
            // Gets the display name of the contained property, if any.
            // Defaults to property name if none is found.
            // </summary>
            public string DisplayName 
            {
                get {
                    if (_displayName == null) 
                    {
                        _displayName =
                            ExtensibilityAccessor.GetDisplayName(_parent.FirstModelProperty) ??
                            _parent.PropertyName;
                    }

                    Fx.Assert(_displayName != null, "_displayName should not be null");
                    return _displayName;
                }
            }

            // <summary>
            // Pick the first category name
            // </summary>
            public string CategoryName 
            {
                get {
                    if (_categoryName == null)
                    {
                        _categoryName = ExtensibilityAccessor.GetCategoryName(_parent.FirstModelProperty);
                    }

                    Fx.Assert(_categoryName != null, "_categoryName should not be null");
                    return _categoryName;
                }
            }

            // <summary>
            // Pick the first description
            // </summary>
            public string Description 
            {
                get {
                    if (_description == null)
                    {
                        _description = ExtensibilityAccessor.GetDescription(_parent.FirstModelProperty);
                    }

                    Fx.Assert(_description != null, "_description should not be null");
                    return _description;
                }
            }

            // <summary>
            // OR mutliple values of IsAdvanced together
            // </summary>
            public bool IsAdvanced 
            {
                get {
                    if (_isAdvanced == null) 
                    {
                        _isAdvanced = false;
                        for (int i = 0; i < _parent._properties.Count; i++) 
                        {
                            _isAdvanced |= ExtensibilityAccessor.GetIsAdvanced(_parent._properties[i]);
                            if (_isAdvanced == true)
                            {
                                break;
                            }
                        }
                    }

                    Fx.Assert(_isAdvanced != null, "_isAdvanced should not be null");
                    return (bool)_isAdvanced;
                }
            }

            // <summary>
            // AND multiple values of IsBrowsable together
            // </summary>
            public bool IsBrowsable 
            {
                get {
                    if (_isBrowsable == null) 
                    {
                        _isBrowsable = true;
                        for (int i = 0; i < _parent._properties.Count; i++) 
                        {

                            // Look for the BrowsableAttribute
                            bool? temp = ExtensibilityAccessor.IsBrowsable(_parent._properties[i]);

                            // Go by the IsReadOnly flag, if not found
                            if (temp == null)
                            {
                                temp = !this.IsReadOnly;
                            }
                            
                            // AND multiple values together
                            _isBrowsable &= (bool)temp;

                            if (_isBrowsable == false)
                            {
                                break;
                            }
                        }
                    }

                    Fx.Assert(_isBrowsable != null, "_isBrowsable should not be null");
                    return (bool)_isBrowsable;
                }
            }

            // <summary>
            // Gets a flags indicating whether this property is read only.
            // </summary>
            public bool IsReadOnly 
            {
                get {
                    if (_isReadOnly == null) 
                    {

                        _isReadOnly = ExtensibilityAccessor.IsReadOnly(
                            _parent._properties,
                            new ExtensibilityAccessor.IsMixedValueEvaluator(delegate() 
                        {
                            return this.IsMixedValue;
                        }));

                        Fx.Assert(_isReadOnly != null, "_isReadOnly should not be null");
                    }

                    return (bool)_isReadOnly;
                }
            }

            // <summary>
            // Merge collection of standard values and only present the subset that exists in all.
            // We do fancy magic here because presenting the user with invalid set of StandardValues
            // could actually cause bad things to happen (read: exceptions when the value is actually changed)
            // </summary>
            public ICollection StandardValues 
            {
                get {
                    if (_standardValues == null) 
                    {

                        // Note: this.Converter will return the converter associated with _parent._properties[0]
                        if (ExtensibilityAccessor.GetStandardValuesSupported(this.Converter)) 
                        {
                            _standardValues = ExtensibilityAccessor.GetStandardValues(this.Converter);
                        }

                        if (_standardValues == null)
                        {
                            _standardValues = new ArrayList();
                        }

                        for (int i = 1; i < _parent._properties.Count && _standardValues.Count > 0; i++) 
                        {
                            ArrayList nextSetOfValues = null;

                            if (ExtensibilityAccessor.GetStandardValuesSupported(_parent._properties[i].Converter))
                            {
                                nextSetOfValues = ExtensibilityAccessor.GetStandardValues(_parent._properties[i].Converter);
                            }

                            if (nextSetOfValues == null || nextSetOfValues.Count == 0) 
                            {
                                // The AND of something and nothing = nothing, so clear any remaining list and exit
                                _standardValues.Clear();
                                break;
                            }

                            for (int j = 0; j < _standardValues.Count; j++) 
                            {

                                object expectedValue = _standardValues[j];

                                if (!nextSetOfValues.Contains(expectedValue)) 
                                {
                                    _standardValues.RemoveAt(j);
                                    j--;
                                    continue;
                                }
                            }
                        }
                    }

                    Fx.Assert(_standardValues != null, "_standardValues should not be null");
                    return _standardValues;
                }
            }

            // <summary>
            // Gets a flag indicating whether the list of StandardValues is complete
            // or whether the user can type a value that's different from the ones in the list
            // </summary>
            public bool StandardValuesExclusive 
            {
                get {
                    if (_standardValuesExclusive == null) 
                    {
                        _standardValuesExclusive = (this.Converter == null || this.Converter.GetStandardValuesExclusive());
                    }

                    Fx.Assert(_standardValuesExclusive != null, "_standardValuesExclusive should not be null");
                    return (bool)_standardValuesExclusive;
                }
            }

            // <summary>
            // Gets a flag indicating whether the StandardValues list has any contents.
            // </summary>
            public bool StandardValuesSupported 
            {
                get {
                    if (_standardValuesSupported == null) 
                    {
                        _standardValuesSupported = (this.Converter != null && this.Converter.GetStandardValuesSupported());
                    }

                    Fx.Assert(_standardValuesSupported != null, "_standardValuesSupported should not be null");
                    return (bool)_standardValuesSupported;
                }
            }

            // <summary>
            // Pick the editor of the first ModelProperty
            // </summary>
            public PropertyValueEditor PropertyValueEditor 
            {
                get {
                    if (_propertyValueEditor == null) 
                    {

                        _propertyValueEditor =
                            ExtensibilityAccessor.GetCustomPropertyValueEditor(_parent.FirstModelProperty) ??
                            ExtensibilityAccessor.GetSubPropertyEditor(_parent.FirstModelProperty);

                        if (_propertyValueEditor == null && _parent.PropertyType == typeof(bool))
                        {
                            _propertyValueEditor = new BoolViewEditor();
                        }

                        _propertyValueEditor = _propertyValueEditor == null ? NoPropertyValueEditor : _propertyValueEditor;
                    }

                    return _propertyValueEditor == NoPropertyValueEditor ? null : _propertyValueEditor;
                }
            }

            public bool IsMixedValue 
            {
                get {
                    if (_isMixedValue == null) 
                    {

                        _isMixedValue = false;

                        if (_parent._properties.Count > 1) 
                        {

                            object mergedValue = null;
                            string mergedValueString = null;
                            ValueSerializer valueSerializer = null;

                            for (int i = 0; i < _parent._properties.Count; i++) 
                            {
                                ModelProperty property = _parent._properties[i];
                                if (i == 0) 
                                {

                                    // Note: Calling GetValue on ModelProperty has the potential to
                                    // to reset internal stores and, even though the value doesn't change,
                                    // we get a value changed notification.  That notification clears 
                                    // our _isMixedValue, which, in fact, we want to retain.
                                    //
                                    bool oldIsMixedValue = (bool)_isMixedValue;
                                    mergedValue = ModelUtilities.GetSafeRawValue(property);
                                    _isMixedValue = oldIsMixedValue;
                                }
                                else 
                                {

                                    // See comment above
                                    bool oldIsMixedValue = (bool)_isMixedValue;
                                    object nextValue = ModelUtilities.GetSafeRawValue(property);
                                    _isMixedValue = oldIsMixedValue;

                                    // Are the objects equal?
                                    if (object.Equals(mergedValue, nextValue))
                                    {
                                        continue;
                                    }

                                    // No, so if any of them is null, we might as well bail
                                    if (mergedValue == null || nextValue == null) 
                                    {
                                        _isMixedValue = true;
                                        break;
                                    }

                                    valueSerializer = valueSerializer ?? this.ValueSerializer;

                                    // If there is no ValueSerializer found, we can't
                                    // be clever and need to bail
                                    if (valueSerializer == null) 
                                    {
                                        _isMixedValue = true;
                                        break;
                                    }

                                    // If we can't even convert the original value to string,
                                    // there is nothing to compare, so we bail
                                    // the CanConvertToString call may throw an ArgumentException, for
                                    // example if mergedValue isn't a supported type
                                    try 
                                    {
                                        if (mergedValueString == null &&
                                            !valueSerializer.CanConvertToString(mergedValue, null)) 
                                        {
                                            _isMixedValue = true;
                                            break;
                                        }
                                    }
                                    catch (ArgumentException) 
                                    {
                                        _isMixedValue = true;
                                        break;
                                    }

                                    if (mergedValueString == null)
                                    {
                                        mergedValueString = valueSerializer.ConvertToString(mergedValue, null);
                                    }

                                    // Finally, check to see if the nextValue can be converted to string
                                    // and, if so, compare it to the mergedValue.
                                    if (!valueSerializer.CanConvertToString(nextValue, null) ||
                                        string.CompareOrdinal(mergedValueString, valueSerializer.ConvertToString(nextValue, null)) != 0) 
                                    {
                                        _isMixedValue = true;
                                        break;
                                    }
                                }
                            }
                        }

                    }

                    return (bool)_isMixedValue;
                }
            }

            // <summary>
            // Gets the source of the given property
            // </summary>
            public PropertyValueSource Source 
            {
                get {
                    if (_source == null && this.IsMixedValue)
                    {
                        _source = NoSource;
                    }

                    if (_source == null) 
                    {

                        foreach (ModelProperty property in _parent._properties) 
                        {

                            if (_source == null) 
                            {
                                _source = ExtensibilityAccessor.GetPropertySource(property);

                                // Default value if we can't figure out anything else (this should never happen)
                                Fx.Assert(_source != null, "Could not figure out the source for property " + _parent.PropertyName);
                                _source = _source ?? DependencyPropertyValueSource.Local;
                            }
                            else if (_source != ExtensibilityAccessor.GetPropertySource(property)) 
                            {
                                _source = NoSource;
                                break;
                            }
                        }
                    }

                    return _source == NoSource ? null : _source;
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

            // <summary>
            // Pick the first converter
            // </summary>
            public TypeConverter Converter 
            {
                get {
                    if (_converter == null)
                    {
                        _converter = ExtensibilityAccessor.GetTypeConverter(_parent.FirstModelProperty) ?? NoTypeConverter;
                    }

                    return _converter == NoTypeConverter ? null : _converter;
                }
            }

            // <summary>
            // Gets the Type of value instance for this property.  For multi-properties,
            // CommonValueType returns the Type of all properties if it matches, null otherwise.
            // </summary>
            public Type CommonValueType 
            {
                get {
                    if (_commonValueType == null) 
                    {

                        foreach (ModelProperty modelProperty in _parent.ModelPropertySet) 
                        {
                            object value = modelProperty.ComputedValue;
                            if (value != null) 
                            {
                                Type valueType = value.GetType();

                                if (_commonValueType == null) 
                                {
                                    _commonValueType = valueType;
                                }
                                else if (_commonValueType != valueType) 
                                {
                                    _commonValueType = NoCommonValueType;
                                    break;
                                }
                            }
                            else 
                            {
                                _commonValueType = NoCommonValueType;
                                break;
                            }
                        }

                        _commonValueType = _commonValueType ?? NoCommonValueType;
                    }

                    return _commonValueType == NoCommonValueType ? null : _commonValueType;
                }
            }

            // <summary>
            // Pick the first PropertyOrder
            // </summary>
            public PropertyOrder PropertyOrder 
            {
                get {
                    if (_propertyOrder == null) 
                    {
                        _propertyOrder = ExtensibilityAccessor.GetPropertyOrder(_parent.FirstModelProperty);
                        _propertyOrder = _propertyOrder ?? NoPropertyOrder;
                    }

                    return _propertyOrder == NoPropertyOrder ? null : _propertyOrder;
                }
            }

            // <summary>
            // Gets a list of CategoryEditor types associated with this PropertyEntry
            // </summary>
            public IEnumerable<Type> CategoryEditorTypes 
            {
                get {
                    if (_categoryEditorTypes == null) 
                    {
                        if (_parent.FirstModelProperty.IsAttached) 
                        {
                            _categoryEditorTypes = ExtensibilityAccessor.GetCategoryEditorTypes(_parent.FirstModelProperty.AttachedOwnerType);
                        }
                        _categoryEditorTypes = _categoryEditorTypes ?? NoCategoryEditorTypes;
                    }

                    return _categoryEditorTypes == NoCategoryEditorTypes ? null : _categoryEditorTypes;
                }
            }

            public bool IsAttached 
            {
                get {
                    if (_isAttached == null)
                    {
                        _isAttached = _parent.PropertyName.IndexOf('.') > -1;
                    }

                    return (bool)_isAttached;
                }
            }

            // <summary>
            // Gets the ValueSerializer corresponding to the property type
            // </summary>
            private ValueSerializer ValueSerializer 
            {
                get {
                    if (_valueSerializer == null)
                    {
                        _valueSerializer = ValueSerializer.GetSerializerFor(_parent.PropertyType) ?? NoSerializer;
                    }

                    return _valueSerializer == NoSerializer ? null : _valueSerializer;
                }
            }

            // Clear everything this class caches
            public void ClearAll() 
            {
                _categoryName = null;
                _description = null;
                _isAdvanced = null;
                _isBrowsable = null;
                _propertyValueEditor = null;
                _propertyOrder = null;
                _categoryEditorTypes = null;
                _displayName = null;

                // Internal properties we don't bind to and, hence,
                // don't need to fire PropertyChanged event:
                _isAttached = null;

                ClearValueRelatedCacheItems();

                _parent.OnPropertyChanged("CategoryName");
                _parent.OnPropertyChanged("Description");
                _parent.OnPropertyChanged("IsAdvanced");
                _parent.OnPropertyChanged("IsBrowsable");
                _parent.OnPropertyChanged("PropertyValueEditor");
                _parent.OnPropertyChanged("PropertyOrder");
                _parent.OnPropertyChanged("CategoryEditorTypes");
                _parent.OnPropertyChanged("DisplayName");
            }

            // Clear value-related things that this class caches
            public void ClearValueRelatedCacheItems() 
            {
                _subProperties = null;
                _collection = null;
                _standardValues = null;
                _standardValuesExclusive = null;
                _converter = null;
                _commonValueType = null;
                _source = null;
                _isReadOnly = null;
                _valueSerializer = null;

                ClearSubValueRelatedCacheItems();

                _parent.OnPropertyChanged("StandardValues");
                _parent.OnPropertyChanged("StandardValuesExclusive");
                _parent.OnPropertyChanged("Converter");
                _parent.OnPropertyChanged("CommonValueType");
                _parent.OnPropertyChanged("IsReadOnly");

                // The following properties are only exposed by ModelPropertyEntry, not PropertyEntry.
                // People should bind to these properties through the PropertyValue.
                // However, if they ---- up in Xaml, the binding will still work and if that happens
                // we should try to update them when things change.
                _parent.OnPropertyChanged("SubProperties");
                _parent.OnPropertyChanged("Collection");
                _parent.OnPropertyChanged("Source");
            }

            public void ClearSubValueRelatedCacheItems() 
            {
                _isMixedValue = null;

                // The following property is only exposed by ModelPropertyEntry, not PropertyEntry.
                // People should bind to this property through the PropertyValue.
                // However, if they ---- up in Xaml, the binding will still work and if that happens
                // we should try to update them when things change.
                _parent.OnPropertyChanged("IsMixedValue");
            }

            private class NoPropertyValueSource : PropertyValueSource 
            {
                public NoPropertyValueSource() 
                {
                }
            }

            private class NoValueSerializer : ValueSerializer 
            {
                public NoValueSerializer() 
                {
                }
            }
        }
    }
}
