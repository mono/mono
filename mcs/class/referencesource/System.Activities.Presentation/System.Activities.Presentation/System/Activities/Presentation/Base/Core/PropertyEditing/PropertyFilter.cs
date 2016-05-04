namespace System.Activities.Presentation.PropertyEditing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Activities.Presentation;

    /// <summary>
    /// This class is used as part of the searching/filtering functionality that may provided
    /// by the property editing host.  It contains a list of predicates (i.e. strings to match against)
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    public class PropertyFilter
    {

        private List<PropertyFilterPredicate> _predicates = new List<PropertyFilterPredicate>();

        /// <summary>
        /// Creates a PropertyFilter.
        /// </summary>
        /// <param name="filterText">String representation of predicates, space delimited</param>
        public PropertyFilter(string filterText)
        {
            SetPredicates(filterText);
        }

        /// <summary>
        /// Creates a PropertyFilter.
        /// </summary>
        /// <param name="predicates">IEnumerable collection of predicates</param>
        public PropertyFilter(IEnumerable<PropertyFilterPredicate> predicates)
        {
            SetPredicates(predicates);
        }

        /// <summary>
        /// Readonly property that returns true if this PropertyFilter does not have any predicates
        /// </summary>
        public bool IsEmpty
        {
            get { return this._predicates == null || this._predicates.Count == 0; }
        }

        private void SetPredicates(string filterText)
        {

            if (string.IsNullOrEmpty(filterText))
                return;

            string[] filterParts = filterText.Split(' ');

            for (int i = 0; i < filterParts.Length; i++)
            {
                if (!string.IsNullOrEmpty(filterParts[i]))
                {
                    _predicates.Add(new PropertyFilterPredicate(filterParts[i]));
                }
            }
        }

        private void SetPredicates(IEnumerable<PropertyFilterPredicate> predicates)
        {

            if (predicates == null)
                return;

            foreach (PropertyFilterPredicate predicate in predicates)
            {
                if (predicate != null)
                {
                    _predicates.Add(predicate);
                }
            }
        }

        /// <summary>
        /// Matches this filter against a particular filter target. The
        /// filter returns true if there are no predicates or if one or more 
        /// predicates match the filter target.
        /// </summary>
        /// <param name="target">Target to attempt matching</param>
        /// <returns>True if there are no predicates or if one or more 
        /// predicates match the filter target, false otherwise</returns>
        /// <exception cref="ArgumentNullException">If target is null.</exception>
        public bool Match(IPropertyFilterTarget target)
        {
            if (target == null)
                throw FxTrace.Exception.ArgumentNull("target");

            if (this.IsEmpty)
                return true;

            // Perform an OR over all predicates
            for (int i = 0; i < this._predicates.Count; i++)
            {
                if (target.MatchesPredicate(_predicates[i]))
                    return true;
            }

            return false;
        }
    }
}
