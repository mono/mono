namespace System.Activities.Presentation.PropertyEditing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Activities.Presentation;
    using System.Collections;

    /// <summary>
    /// Collection class that is used to keep the collection of PropertyEntry instances for 
    /// subproperties of a given PropertyEntry
    /// </summary>
    public abstract class PropertyEntryCollection : IEnumerable<PropertyEntry>
    {

        private PropertyValue _parentValue;

        /// <summary>
        /// Creates a PropertyEntryCollection.  For host implementations.
        /// </summary>
        /// <param name="parentValue">The parent PropertyValue</param>
        /// <exception cref="ArgumentNullException">When parentValue is null</exception>
        protected PropertyEntryCollection(PropertyValue parentValue)
        {
            if (parentValue == null)
                throw FxTrace.Exception.ArgumentNull("parentValue");

            _parentValue = parentValue;
        }

        /// <summary>
        /// Gets the parent PropertyValue
        /// </summary>
        public PropertyValue ParentValue { get { return _parentValue; } }

        /// <summary>
        /// Gets a PropertyEntry from this collection of the given name.  Used for
        /// sub-property retrieval.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>PropertyEntry instance of the given name or null if it doesn't exist.</returns>
        public abstract PropertyEntry this[string propertyName] { get; }

        /// <summary>
        /// Gets the number of PropertyEntry instances in this collection
        /// (typically the number of sub-properties for the parent PropertyValue)
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Returns an IEnumerator of all the PropertyEntry instances in this collection.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<PropertyEntry> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
