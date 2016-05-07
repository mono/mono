//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Activities.Presentation;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    public delegate object PropertyValueMorphHelper(ModelItem originalValue, ModelProperty newModelProperty);

    public static class MorphHelper
    {
        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
                           Justification = "Morph is the right word here")]

        static Dictionary<Type, PropertyValueMorphHelper> morphExtensions = new Dictionary<Type, PropertyValueMorphHelper>();

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
                            Justification = "Morph is the right word here")]
        public static void AddPropertyValueMorphHelper(Type propertyType, PropertyValueMorphHelper extension)
        {
            if (propertyType == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("propertyType"));
            }
            if (extension == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("extension"));
            }
            morphExtensions[propertyType] = extension;
        }

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
                            Justification = "Morph is the right word here")]
        public static PropertyValueMorphHelper GetPropertyValueMorphHelper(Type propertyType)
        {
            if (propertyType == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("propertyType"));
            }
            PropertyValueMorphHelper extension = null;
            morphExtensions.TryGetValue(propertyType, out extension);
            if (extension == null && propertyType.IsGenericType)
            {
                morphExtensions.TryGetValue(propertyType.GetGenericTypeDefinition(), out extension);
            }
            return extension;
        }

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
                            Justification = "Morph is the right word here")]
        // This updates back links
        public static void MorphObject(ModelItem oldModelItem, ModelItem newModelitem)
        {
            if (oldModelItem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("oldModelItem"));
            }
            if (newModelitem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("newModelitem"));
            }

            var collectionParents = from parent in oldModelItem.Parents
                                    where parent is ModelItemCollection
                                    select (ModelItemCollection)parent;
            foreach (ModelItemCollection collectionParent in collectionParents.ToList())
            {
                int index = collectionParent.IndexOf(oldModelItem);
                collectionParent.Remove(oldModelItem);
                collectionParent.Insert(index, newModelitem);
            }
            foreach (ModelProperty modelProperty in oldModelItem.Sources.ToList())
            {
                modelProperty.SetValue(newModelitem);
            }


        }

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
                            Justification = "Morph is the right word here")]
        // this updates forward links
        public static void MorphProperties(ModelItem oldModelItem, ModelItem newModelitem)
        {
            if (oldModelItem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("oldModelItem"));
            }
            if (newModelitem == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("newModelitem"));
            }

            foreach (ModelProperty modelProperty in oldModelItem.Properties)
            {
                ModelProperty propertyInNewModelItem = newModelitem.Properties[modelProperty.Name];
                if (propertyInNewModelItem != null)
                {
                    Console.WriteLine(propertyInNewModelItem.Name);
                    if (CanCopyProperty(modelProperty, propertyInNewModelItem))
                    {
                        if (propertyInNewModelItem.PropertyType.Equals(modelProperty.PropertyType))
                        {
                            propertyInNewModelItem.SetValue(modelProperty.Value);
                            modelProperty.SetValue(null);
                        }
                        else // See if there is morph helper for this type.
                        {
                            PropertyValueMorphHelper extension = GetPropertyValueMorphHelper(modelProperty.PropertyType);
                            if (extension != null)
                            {
                                propertyInNewModelItem.SetValue(extension(modelProperty.Value, propertyInNewModelItem));
                                modelProperty.SetValue(null);
                            }
                        }

                    }
                }

            }
        }

        static bool CanCopyProperty(ModelProperty modelProperty, ModelProperty propertyInNewModelItem)
        {
            bool canCopyProperty = false;
            DesignerSerializationVisibilityAttribute designerSerializationVisibility = ExtensibilityAccessor.GetAttribute<DesignerSerializationVisibilityAttribute>(modelProperty.Attributes);
            if (modelProperty.Value == null)
            {
                canCopyProperty = false;
            }
            else if (designerSerializationVisibility != null && designerSerializationVisibility.Visibility != DesignerSerializationVisibility.Visible)
            {
                canCopyProperty = false;
            }
            else if (propertyInNewModelItem != null && !propertyInNewModelItem.IsAttached && !propertyInNewModelItem.IsReadOnly)
            {
                canCopyProperty = true;
            }
            return canCopyProperty;
        }

    }
}

