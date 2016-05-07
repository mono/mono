//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Model 
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;
    using System.Runtime;

    // <summary>
    // Collection of ModelPropertyEntries used to model sub-properties of PropertyValues
    // </summary>
    internal class ModelPropertyEntryCollection : PropertyEntryCollection 
    {

        List<ModelPropertyEntry> _properties;

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="parentProperty">Parent property</param>
        public ModelPropertyEntryCollection(ModelPropertyEntry parentProperty)
            : base(parentProperty.PropertyValue) 
        {

            CreateCollection(parentProperty);
        }

        // <summary>
        // Basic ctor
        // </summary>
        // <param name="parentProperty">Parent indexer</param>
        public ModelPropertyEntryCollection(ModelPropertyIndexer parentProperty)
            : base(parentProperty.PropertyValue) 
        {

            CreateCollection(parentProperty);
        }

        // <summary>
        // Gets the number of PropertyEntries in this collection
        // </summary>
        public override int Count 
        {
            get {
                return _properties == null ? 0 : _properties.Count;
            }
        }

        // <summary>
        // Gets the property from this collection of the given name (case sensitive)
        // </summary>
        // <param name="propertyName">Name to look up</param>
        // <returns>Corresponding PropertyEntry if one exists.</returns>
        public override PropertyEntry this[string propertyName] {
            get {
                if (_properties == null)
                {
                    return null;
                }

                foreach (ModelPropertyEntry entry in _properties)
                {
                    if (string.Equals(entry.PropertyName, propertyName))
                    {
                        return entry;
                    }
                }

                return null;
            }
        }

        // <summary>
        // Gets the enumerator for this collection
        // </summary>
        // <returns></returns>
        public override IEnumerator<PropertyEntry> GetEnumerator() 
        {
            if (_properties == null)
            {
                yield break;
            }

            for (int i = 0; i < _properties.Count; i++) 
            {
                yield return _properties[i];
            }
        }

        // Parses the sub-properties of the given parent collection item and populates a corresponding
        // private list of ModelPropertyEntries that represents it.
        private void CreateCollection(ModelPropertyIndexer parentCollectionItem) 
        {

            // Assert some assumptions that should be true at this point
            Fx.Assert(parentCollectionItem.ModelItem != null, "parentCollectionItem.ModelItem should not be null");

            List<ModelProperty> subProperties = ExtensibilityAccessor.GetSubProperties(parentCollectionItem.ModelItem);

            if (subProperties == null || subProperties.Count < 1)
            {
                return;
            }

            // At this point we have at least one ModelProperty that acts as a subProperty of the 
            // given ModelItem.  Wrap the list in ModelPropertyEntries and exit.

            _properties = new List<ModelPropertyEntry>(subProperties.Count);

            for (int i = 0; i < subProperties.Count; i++) 
            {
                _properties.Add(new ModelPropertyEntry(subProperties[i], (ModelPropertyValue)parentCollectionItem.PropertyValue));
            }

            // Sort the sub-properties by their OrderToken as well as their name
            if (_properties != null)
            {
                _properties.Sort();
            }
        }

        private void CreateCollection(ModelPropertyEntry parentProperty) 
        {

            // Assert some assumptions that should be true at this point
            Fx.Assert(parentProperty != null, "parentProperty should not be null");
            Fx.Assert(parentProperty.ModelPropertySet != null, "parentProperty.ModelPropertySet should not be null");
            Fx.Assert(parentProperty.ModelPropertySet.Count > 0, "parentProperty.ModelPropertySet.Count should be > 0");

            // Ignore sub-properties of MarkupExtensions for v1
            if (parentProperty.IsMarkupExtension)
            {
                return;
            }

            IEnumerable<IList<ModelProperty>> mergedSubProperties = ModelPropertyMerger.GetMergedSubProperties(parentProperty.ModelPropertySet);

            int index = 0;
            bool multiSelect = parentProperty.ModelPropertySet.Count > 1;
            foreach (IList<ModelProperty> subPropertySet in mergedSubProperties) 
            {

                if (index == 0)
                {
                    _properties = new List<ModelPropertyEntry>();
                }

                ModelPropertyEntry entry;

                if (multiSelect)
                {
                    entry = new ModelPropertyEntry(subPropertySet, (ModelPropertyValue)parentProperty.PropertyValue);
                }
                else
                {
                    entry = new ModelPropertyEntry(subPropertySet[0], (ModelPropertyValue)parentProperty.PropertyValue);
                }

                _properties.Add(entry);
                index++;
            }

            // Sort the sub-properties by their OrderToken as well as their name
            if (_properties != null)
            {
                _properties.Sort();
            }
        }
    }
}
