namespace System.Activities.Presentation.PropertyEditing
{
    using System;
    using System.Runtime;

    /// <summary>
    /// EventArgs used to fire an event that some IPropertyFilter has been applied
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    public class PropertyFilterAppliedEventArgs : EventArgs
    {
        private PropertyFilter _filter;

        /// <summary>
        /// Creates a new PropertyFilterAppliedEventArgs
        /// </summary>
        /// <param name="filter">The PropertyFilter that was applied.</param>
        public PropertyFilterAppliedEventArgs(PropertyFilter filter)
        {
            _filter = filter;
        }

        /// <summary>
        /// Readonly property that returns the PropertyFilter that was applied.
        /// </summary>
        public PropertyFilter Filter { get { return _filter; } }
    }
}
