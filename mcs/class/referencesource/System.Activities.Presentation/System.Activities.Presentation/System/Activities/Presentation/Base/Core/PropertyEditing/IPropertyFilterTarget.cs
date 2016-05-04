namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface that is used by the host infrastructure for the PropertyEditing to handle sorting/filtering
    /// functionality.  It is used to determine whether a particular object should be filtered out.
    /// </summary>
    public interface IPropertyFilterTarget {

        /// <summary>
        /// Event raised when a PropertyFilter is changed through a call to ApplyFilter.
        /// </summary>
        event EventHandler<PropertyFilterAppliedEventArgs> FilterApplied;
        
        /// <summary>
        /// Read-only property that returns true if the PropertyFilter is a match for the object.
        /// </summary>
        bool MatchesFilter { get; }
        
        /// <summary>
        /// Used to set a new PropertyFilter on the IPropertyFilterTarget
        /// </summary>
        /// <param name="filter">The new PropertyFilter instance.</param>
        void ApplyFilter(PropertyFilter filter);
        
        /// <summary>
        /// Used to determine if this IPropertyFilterTarget is a match for a particular PropertyFilterPredicate
        /// </summary>
        /// <param name="predicate">The PropertyFilterPredicate to match against</param>
        /// <returns>True if it is a match, otherwise false</returns>
        bool MatchesPredicate(PropertyFilterPredicate predicate);
    }
}
