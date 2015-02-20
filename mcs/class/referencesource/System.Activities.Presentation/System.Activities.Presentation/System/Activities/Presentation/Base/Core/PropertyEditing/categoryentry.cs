namespace System.Activities.Presentation.PropertyEditing {
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Activities.Presentation;
    using System;

    /// <summary>
    /// The CategoryEntry class is a part of the property editing object model.  It models a 
    /// Category which has a localized name along with a collection of properties.
    /// </summary>
    public abstract class CategoryEntry : INotifyPropertyChanged, IPropertyFilterTarget {

        private string _name;
        private bool _matchesFilter;

        /// <summary>
        /// Creates a new CategoryEntry.  For host Infrastructure use
        /// </summary>
        /// <param name="name">The localized name of the corresponding Category as defined by the 
        /// CategoryAttribute</param>
        /// <exception cref="ArgumentNullException">When name is either empty or null.</exception>
        protected CategoryEntry(string name) {
            if (string.IsNullOrEmpty(name))
                throw FxTrace.Exception.ArgumentNull("name");

            _name = name;
        }

        /// <summary>
        /// Returns the localized Category name
        /// </summary>
        public string CategoryName {
            get { return _name; }
        }

        /// <summary>
        /// Returns an IEnumerable collection of all of the properties in the category.
        /// </summary>
        public abstract IEnumerable<PropertyEntry> Properties { get; }

        /// <summary>
        /// Indexer that returns a Property instance given the property name.
        /// </summary>
        /// <param name="propertyName">The string property name to return a Property instance for.</param>
        /// <returns>Property corresponding to the passed in propertyName if it exists, otherwise null</returns>
        public abstract PropertyEntry this[string propertyName] { get; }

        // INotifyPropertyChanged Members

        /// <summary>
        /// INotifyPropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the INotifyPropertyChanged.PropertyChanged event
        /// </summary>
        /// <param name="propertyName">the name of the property that is changing</param>
        /// <exception cref="ArgumentNullException">When propertyName is null</exception>
        protected virtual void OnPropertyChanged(string propertyName) {
            if (propertyName == null)
                throw FxTrace.Exception.ArgumentNull("propertyName");

            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        // IPropertyFilterTarget Members

        /// <summary>
        /// IPropertyFilterTarget event
        /// </summary>
        public event EventHandler<PropertyFilterAppliedEventArgs> FilterApplied;

        /// <summary>
        /// Raises the IPropertyFilterTarget.FilterApplied event
        /// </summary>
        /// <param name="filter">The PropertyFilter being applied</param>
        protected virtual void OnFilterApplied(PropertyFilter filter) {
            if (FilterApplied != null) {
                FilterApplied(this, new PropertyFilterAppliedEventArgs(filter));
            }
        }

        /// <summary>
        /// IPropertyFilterTarget method
        /// </summary>
        /// <param name="filter"></param>
        public virtual void ApplyFilter(PropertyFilter filter) {
            this.MatchesFilter = filter == null ? true : filter.Match(this);
            OnFilterApplied(filter);
        }
        
        /// <summary>
        /// IPropertyFilterTarget property
        /// </summary>
        public virtual bool MatchesFilter {
            get { return _matchesFilter; }
            protected set {
                if (_matchesFilter != value) {
                    _matchesFilter = value;
                    this.OnPropertyChanged("MatchesFilter");
                }
            }
        }
        
        /// <summary>
        /// IPropertyFilterTarget method
        /// </summary>
        /// <param name="predicate">The PropertyFilterPredicate to match against</param>
        /// <returns>true if there is a match, otherwise false</returns>
        public abstract bool MatchesPredicate(PropertyFilterPredicate predicate);

    }
}

