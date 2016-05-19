using System.Diagnostics.CodeAnalysis;

namespace System.Activities.Presentation.PropertyEditing
{
    using System.ComponentModel;
    using System.Collections;
    using System;
    using System.Diagnostics;
    using System.Activities.Presentation;

    /// <summary>
    /// The PropertyEntry class provides additional, mostly type-specific data for a property.
    /// </summary>
    public abstract class PropertyEntry : INotifyPropertyChanged, IPropertyFilterTarget {

        private PropertyValue _parentValue;
        private bool _matchesFilter = true;
        private PropertyValue _value;

        /// <summary>
        /// Creates a PropertyEntry.  For host infrastructure derived classes.
        /// </summary>
        protected PropertyEntry() : this(null) { }

        /// <summary>
        /// Creates a PropertyEntry that acts as a sub-property of the specified PropertyValue.
        /// For host infrastructure derived classes.
        /// </summary>
        /// <param name="parentValue">The parent PropertyValue.
        /// Root properties do not have a parent PropertyValue.</param>
        protected PropertyEntry(PropertyValue parentValue) {
            _parentValue = parentValue;
        }

        /// <summary>
        /// Gets the name of the encapsulated property.
        /// </summary>
        public abstract string PropertyName { get; }

        /// <summary>
        /// Gets the DisplayName for the property. By default, it is the
        /// PropertyName.
        /// </summary>
        public virtual string DisplayName { get { return this.PropertyName; } }

        /// <summary>
        /// Gets the Type of the encapsulated property.
        /// </summary>
        public abstract Type PropertyType { get; }

        /// <summary>
        /// Gets the name of the category that this property resides in.
        /// </summary>
        public abstract string CategoryName { get; }

        /// <summary>
        /// Gets the description of the encapsulated property.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Returns true if there are standard values for this property.
        /// The default implementation checks if the StandardValues property
        /// returns a non-null collection with a count > 0.
        /// </summary>
        protected virtual bool HasStandardValues {
            get {
                ICollection values = StandardValues;
                return values != null && values.Count > 0;
            }
        }

        /// <summary>
        /// Accessor because we use this property in the property container.
        /// </summary>
        internal bool HasStandardValuesInternal {
            get { return HasStandardValues; }
        }

        /// <summary>
        /// Gets the read-only attribute of the encapsulated property.
        /// </summary>
        public abstract bool IsReadOnly { get; }

        /// <summary>
        /// Gets a flag indicating whether the encapsulated property is an advanced property.
        /// </summary>
        public abstract bool IsAdvanced { get; }

        /// <summary>
        /// Gets any StandardValues that the encapsulated property supports.
        /// </summary>
        public abstract ICollection StandardValues { get; }

        /// <summary>
        /// Gets to PropertyValueEditor to be used for editing of this PropertyEntry.
        /// May be null.  PropertyContainer listens to changes made to this property.
        /// If the value changes, it's the responsibility of the deriving class to fire the
        /// appropriate PropertyChanged event.
        /// </summary>
        public abstract PropertyValueEditor PropertyValueEditor { get; }

        /// <summary>
        /// Gets the parent PropertyValue.  This is only used for sub-properties and,
        /// hence, its balue may be null.
        /// </summary>
        public PropertyValue ParentValue {
            get {
                return _parentValue;
            }
        }

        /// <summary>
        /// Gets the PropertyValue (data model) for this PropertyEntry.
        /// </summary>
        public PropertyValue PropertyValue {
            get {
                if (_value == null)
                    _value = CreatePropertyValueInstance();

                return _value;
            }
        }

        /// <summary>
        /// Used by the host infrastructure to create a new host-specific PropertyValue instance.
        /// </summary>
        /// <returns>new PropertyValue</returns>
        protected abstract PropertyValue CreatePropertyValueInstance();

        // IPropertyFilterTarget Members

        /// <summary>
        /// IPropertyFilterTarget event
        /// </summary>
        public event EventHandler<PropertyFilterAppliedEventArgs> FilterApplied;

        /// <summary>
        /// IPropertyFilterTarget method. PropertyContainer listens to changes made to this property.
        /// </summary>
        public bool MatchesFilter {
            get {
                return _matchesFilter;
            }
            protected set {
                if (value != _matchesFilter) {
                    _matchesFilter = value;
                    OnPropertyChanged("MatchesFilter");
                }
            }
        }

        /// <summary>
        /// IPropertyFilterTarget method
        /// </summary>
        /// <param name="predicate">the predicate to match against</param>
        /// <returns>true if there is a match</returns>
        public virtual bool MatchesPredicate(PropertyFilterPredicate predicate) {
            return predicate == null ?
                false :
                predicate.Match(this.DisplayName) || predicate.Match(this.PropertyType.Name);
        }

        /// <summary>
        /// IPropertyFilterTarget method
        /// </summary>
        /// <param name="filter">the PropertyFilter to apply</param>
        public virtual void ApplyFilter(PropertyFilter filter) {
            this.MatchesFilter = filter == null ? true : filter.Match(this);
            OnFilterApplied(filter);
        }

        /// <summary>
        /// Used to raise the IPropertyFilterTarget FilterApplied event
        /// </summary>
        /// <param name="filter"></param>
        protected virtual void OnFilterApplied(PropertyFilter filter) {
            if (FilterApplied != null)
                FilterApplied(this, new PropertyFilterAppliedEventArgs(filter));
        }


        // INotifyPropertyChanged

        /// <summary>
        /// INotifyPropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Used to raise the INotifyPropertyChanged PropertyChanged event
        /// </summary>
        /// <param name="e">EventArgs for this event</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (e == null)
                throw FxTrace.Exception.ArgumentNull("e");

            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        /// <summary>
        /// Used to raise the INotifyPropertyChanged event
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException">When propertyName is null</exception>
        protected virtual void OnPropertyChanged(string propertyName) {
            if (propertyName == null)
                throw FxTrace.Exception.ArgumentNull("propertyName");

            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

