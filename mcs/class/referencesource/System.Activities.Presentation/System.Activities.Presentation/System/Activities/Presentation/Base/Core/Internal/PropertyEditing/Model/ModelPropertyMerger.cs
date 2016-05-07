//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Activities.Presentation.Model;

    // <summary>
    // Helper class that knows how to merge ModelProperties across multiple ModelItems
    // </summary>
    internal static class ModelPropertyMerger
    {

        private static IEnumerable<IList<ModelProperty>> _emptyCollection;

        private static IEnumerable<IList<ModelProperty>> EmptyCollection
        {
            get
            {
                if (_emptyCollection == null)
                {
                    _emptyCollection = new List<IList<ModelProperty>>();
                }

                return _emptyCollection;
            }
        }

        // <summary>
        // Uber method that returns a list of list of ModelProperties that represent the
        // merged set of properties across the specified ModelItems
        // </summary>
        // <param name="items">ModelItems to examine</param>
        // <param name="itemCount">Count on ModelItems to examine</param>
        // <returns>List of list of merged properties</returns>
        public static IEnumerable<IList<ModelProperty>> GetMergedProperties(IEnumerable<ModelItem> items, int itemCount)
        {
            return GetMergedPropertiesHelper(new ModelItemExpander(items, itemCount));
        }

        // <summary>
        // Uber method that returns a list of list of ModelProperties that represent the
        // merged set of sub-properties across the values of the specified parent properties
        // </summary>
        // <param name="parentProperties">ModelProperties to examine</param>
        // <returns>List of list of merged properties</returns>
        public static IEnumerable<IList<ModelProperty>> GetMergedSubProperties(ICollection<ModelProperty> parentProperties)
        {
            return GetMergedPropertiesHelper(new SubPropertyExpander(parentProperties));
        }

        // <summary>
        // Finds the consolidated default property name and returns it.  If there is no shared
        // default property betweem the specified items, null is returned.
        // </summary>
        // <param name="items">Items to examine</param>
        // <returns>Shared default property, if any.</returns>
        public static string GetMergedDefaultProperty(IEnumerable<ModelItem> items)
        {
            if (items == null)
            {
                return null;
            }

            bool firstIteration = true;
            string mergedDefaultProperty = null;

            foreach (ModelItem item in items)
            {
                string currentDefaultProperty = ExtensibilityAccessor.GetDefaultProperty(item.ItemType);

                if (firstIteration)
                {
                    mergedDefaultProperty = currentDefaultProperty;
                }
                else if (!string.Equals(currentDefaultProperty, mergedDefaultProperty))
                {
                    mergedDefaultProperty = null;
                }

                if (string.IsNullOrEmpty(mergedDefaultProperty))
                {
                    return null;
                }

                firstIteration = false;
            }

            return mergedDefaultProperty;
        }

        // Optimization that speeds up the common case (single selection)
        private static IEnumerable<IList<ModelProperty>> GetMergedPropertiesHelper(PropertyExpander expander)
        {
            // Check empty list
            if (expander == null || expander.ContainerCount == 0)
            {
                return EmptyCollection;
            }

            if (expander.ContainerCount == 1)
            {
                // Corner case - one object selected, don't bother with merging
                return GetFirstProperties(expander);
            }
            else
            {
                // Calculate the list anew
                return GetMergedPropertiesCore(expander);
            }
        }

        // Optimization that speeds up the common case (single selection)
        private static IEnumerable<IList<ModelProperty>> GetFirstProperties(PropertyExpander expander)
        {
            IEnumerator<IEnumerable<ModelProperty>> propertyContainers = expander.GetEnumerator();
            propertyContainers.MoveNext();

            if (propertyContainers.Current != null)
            {
                foreach (ModelProperty property in propertyContainers.Current)
                {
                    yield return new ModelProperty[] { property };
                }
            }
        }

        private static IEnumerable<IList<ModelProperty>> GetMergedPropertiesCore(PropertyExpander expander)
        {

            Dictionary<string, IList<ModelProperty>> counter = new Dictionary<string, IList<ModelProperty>>();

            int containerCounter = 0;
            foreach (IEnumerable<ModelProperty> properties in expander)
            {

                if (properties == null)
                {
                    yield break;
                }

                foreach (ModelProperty property in properties)
                {

                    IList<ModelProperty> existingModelPropertiesForProperty;
                    if (!counter.TryGetValue(property.Name, out existingModelPropertiesForProperty))
                    {

                        if (containerCounter == 0)
                        {
                            existingModelPropertiesForProperty = new List<ModelProperty>(expander.ContainerCount);
                            counter[property.Name] = existingModelPropertiesForProperty;
                        }
                        else
                        {
                            // This property has not been encountered yet in the previous objects,
                            // so skip it altogether.
                            continue;
                        }

                    }

                    if (existingModelPropertiesForProperty.Count < containerCounter)
                    {
                        // There has been a ModelItem in the list that didn't have this property,
                        // so delete any record of it and skip it in the future.
                        counter.Remove(property.Name);
                        continue;
                    }

                    // Verify that the properties are equivalent
                    if (containerCounter > 0 &&
                        !ModelUtilities.AreEquivalent(
                        existingModelPropertiesForProperty[containerCounter - 1], property))
                    {
                        // They are not, so scrap this property altogether
                        counter.Remove(property.Name);
                        continue;
                    }

                    existingModelPropertiesForProperty.Add(property);
                }

                containerCounter++;
            }

            foreach (KeyValuePair<string, IList<ModelProperty>> pair in counter)
            {
                // Once again, if there is a property that is not shared by all
                // selected items, ignore it
                if (pair.Value.Count < containerCounter)
                {
                    continue;
                }

                // We should not set the same instance to multiple properties, 
                // so ignore types that are not value type or string in case of multi-selection
                if (pair.Value.Count > 1 && !(pair.Value[0].PropertyType.IsValueType || pair.Value[0].PropertyType.Equals(typeof(string))))
                {
                    continue;
                }

                yield return (IList<ModelProperty>)pair.Value;
            }
        }

        // <summary>
        // We use the same code to merge properties across a set of ModelItems as well
        // as to merge sub-properties across a set of ModelProperties.  PropertyExpander
        // class is a helper that abstracts the difference between these two inputs, so
        // that the merge methods don't have to worry about it.
        // </summary>
        private abstract class PropertyExpander : IEnumerable<IEnumerable<ModelProperty>>
        {
            public abstract int ContainerCount
            { get; }
            public abstract IEnumerator<IEnumerable<ModelProperty>> GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        // <summary>
        // Version of PropertyExpander that returns the properties of a set of ModelItems
        // </summary>
        private class ModelItemExpander : PropertyExpander
        {

            private IEnumerable<ModelItem> _items;
            private int _itemCount;

            public ModelItemExpander(IEnumerable<ModelItem> items, int itemCount)
            {
                _items = items;
                _itemCount = itemCount;
            }

            public override int ContainerCount
            {
                get { return _itemCount; }
            }

            public override IEnumerator<IEnumerable<ModelProperty>> GetEnumerator()
            {
                if (_items == null)
                {
                    yield break;
                }

                foreach (ModelItem item in _items)
                {
                    if (item.Properties == null)
                    {
                        continue;
                    }

                    yield return item.Properties;
                }
            }
        }

        // <summary>
        // Version of PropertyExpander that returns the sub-properties of a set of
        // ModelProperty values.
        // </summary>
        private class SubPropertyExpander : PropertyExpander
        {

            private ICollection<ModelProperty> _parentProperties;

            public SubPropertyExpander(ICollection<ModelProperty> parentProperties)
            {
                _parentProperties = parentProperties;
            }

            public override int ContainerCount
            {
                get { return _parentProperties == null ? 0 : _parentProperties.Count; }
            }

            public override IEnumerator<IEnumerable<ModelProperty>> GetEnumerator()
            {
                if (_parentProperties == null)
                {
                    yield break;
                }

                foreach (ModelProperty property in _parentProperties)
                {
                    yield return ExtensibilityAccessor.GetSubProperties(property);
                }
            }
        }
    }
}
