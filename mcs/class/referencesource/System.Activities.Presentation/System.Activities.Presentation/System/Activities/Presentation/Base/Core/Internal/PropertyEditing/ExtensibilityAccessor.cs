//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;

    using System.Activities.Presentation.Internal.PropertyEditing.Editors;
    using ModelUtilities = System.Activities.Presentation.Internal.PropertyEditing.Model.ModelUtilities;

    // <summary>
    // Static helper class that contains all extensibility-related code.  No other code
    // under PropertyEditing should be looking up attributes and interpretting them.
    // In most cases, the methods here delegate to Sparkle's binaries to make sure that
    // both products behave consistently.
    // </summary>
    internal static class ExtensibilityAccessor 
    {

        // Cache of Types to their respective DefaultProperties
        private static Dictionary<Type, string> _defaultPropertyCache = new Dictionary<Type, string>();

        // <summary>
        // Gets the name of the category that the specified ModelProperty belongs to
        // </summary>
        // <param name="property">ModelProperty to examine</param>
        // <returns>Name of the category that the specified ModelProperty belongs to</returns>
        public static string GetCategoryName(ModelProperty property) 
        {
            CategoryAttribute attribute = GetAttribute<CategoryAttribute>(property);

            if (attribute == null || string.IsNullOrEmpty(attribute.Category))
            {
                return CategoryAttribute.Default.Category;
            }
            else
            {
                return attribute.Category;
            }
        }

        // <summary>
        // Gets the StandardValues that are exposed by the specified TypeConverter
        // </summary>
        // <param name="converter">TypeConverter to examine</param>
        // <returns>StandardValues that are exposed by the specified TypeConverter</returns>
        public static ArrayList GetStandardValues(TypeConverter converter) 
        {
            if (converter == null)
            {
                return null;
            }

            if (!converter.GetStandardValuesSupported())
            {
                return null;
            }

            ICollection values = converter.GetStandardValues();
            if (values == null)
            {
                return null;
            }

            // unwrap ModelItems if that's what the converter gives us
            ArrayList convertedValues = new ArrayList(values.Count);
            foreach (object value in values) 
            {
                ModelItem item = value as ModelItem;
                if (item != null)
                {
                    convertedValues.Add(item.GetCurrentValue());
                }
                else
                {
                    convertedValues.Add(value);
                }
            }

            return convertedValues;
        }

        // <summary>
        // Gets a flag indicating if a further call to GetStandardValues will
        // give back a non-zero collection.
        // </summary>
        // <param name="converter">The type converter to check.</param>
        // <returns>True if the type converter supports standard values.</returns>
        public static bool GetStandardValuesSupported(TypeConverter converter) 
        {
            return (converter != null && converter.GetStandardValuesSupported());
        }

        // <summary>
        // Look for and return any custom PropertyValueEditor defined for the specified ModelProperty
        // </summary>
        // <param name="property">ModelProperty to examine</param>
        // <returns>A custom PropertyValueEditor for the specified ModelProperty (may be null)</returns>
        public static PropertyValueEditor GetCustomPropertyValueEditor(ModelProperty property) 
        {
            if (property == null)
            {
                return null;
            }

            PropertyValueEditor editor = ExtensibilityMetadataHelper.GetValueEditor(property.Attributes, MessageLogger.Instance);

            //if property is a generic type, check for designer defined at generic type definition
            if (editor == null && property.PropertyType.IsGenericType)
            {
                Type genericType = property.PropertyType.GetGenericTypeDefinition();
                editor = ExtensibilityMetadataHelper.GetValueEditor(TypeDescriptor.GetAttributes(genericType), MessageLogger.Instance);
            }

            return editor;
        }
        // <summary>
        // Returns an instance of SubPropertyEditor if the specified ModelProperty can be edited
        // using sub-properties, null otherwise.
        // </summary>
        // <param name="property">ModelProperty to examine</param>
        // <returns>An instance of SubPropertyEditor if the specified ModelProperty can be edited
        // using sub-properties, null otherwise.</returns>
        public static PropertyValueEditor GetSubPropertyEditor(ModelProperty property) 
        {
            if (property == null)
            {
                return null;
            }

            if (property.Converter == null ||
                property.Converter.GetPropertiesSupported() == false)
            {
                // if it's a property of a generic type, check for converter defined at the property of generic type definition
                if (property.Parent.ItemType.IsGenericType)
                {
                    Type genericType = property.Parent.ItemType.GetGenericTypeDefinition();
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(genericType);
                    PropertyDescriptor propertyDescriptor = properties.Find(property.Name, false);
                    if (propertyDescriptor != null)
                    {
                        // wrap the converter and check if it supports GetProperties()
                        TypeConverter converter = new ModelTypeConverter(((IModelTreeItem)property.Parent).ModelTreeManager, propertyDescriptor.Converter);
                        if (converter.GetPropertiesSupported())
                        {
                            return SubPropertyViewEditor.Instance;
                        }
                    }
                }

                return null;
            }

            //Dont support Arrays
            if (typeof(Array).IsAssignableFrom(property.PropertyType))
            {
                return null;
            }

            return SubPropertyViewEditor.Instance;
        }

        // <summary>
        // ----s open the specified Type and looks for EditorAttributes that represent
        // CategoryEditors - returns the Types of those editors, if any are found, as a list
        // </summary>
        // <param name="ownerType">Type to ---- open</param>
        // <returns>List of CategoryEditors associated with the specified type, if any.  Null otherwise.</returns>
        public static IEnumerable<Type> GetCategoryEditorTypes(Type ownerType) 
        {

            List<Type> editorTypes = null;

            foreach (EditorAttribute editorAttribute in GetAttributes<EditorAttribute>(ownerType)) 
            {

                // A ---- attempt at using the same extensibility code
                Type editorType = ExtensibilityMetadataHelper.GetCategoryEditorType(editorAttribute, MessageLogger.Instance);
                if (editorType == null)
                {
                    continue;
                }

                if (editorTypes == null)
                {
                    editorTypes = new List<Type>();
                }

                editorTypes.Add(editorType);
            }

            return editorTypes;
        }

        // <summary>
        // Decides whether the specified ModelProperty should be advanced
        // </summary>
        // <param name="property">ModelProperty to look up</param>
        // <returns>True if the property should be advanced, false otherwise</returns>
        public static bool GetIsAdvanced(ModelProperty property) 
        {
            EditorBrowsableAttribute browsable = GetAttribute<EditorBrowsableAttribute>(property);

            if (browsable == null)
            {
                return false;
            }

            return browsable.State == EditorBrowsableState.Advanced;
        }

        // <summary>
        // Decides whether the specified CategoryEditor should be advanced.
        // Note: Blend uses custom, baked-in logic to determine whether a given CategoryEditor
        // is advanced or not.  The logic is the same as the one here, but we can't share it
        // because they don't expose it.  In v2, this is definitely something we should share.
        // </summary>
        // <param name="editor">CategoryEditor to look up</param>
        // <returns>True if the specified editor should be advanced, false otherwise</returns>
        public static bool GetIsAdvanced(CategoryEditor editor) 
        {
            EditorBrowsableAttribute browsable = GetAttribute<EditorBrowsableAttribute>(editor.GetType());

            if (browsable == null)
            {
                return false;
            }

            return browsable.State == EditorBrowsableState.Advanced;
        }

        // <summary>
        // Looks up the DefaultPropertyAttribute on the given type and returns the default property,
        // if any.
        // </summary>
        // <param name="type">Type to look up</param>
        // <returns>Default property associated with the specified type, if any.</returns>
        public static string GetDefaultProperty(Type type) 
        {
            if (type == null)
            {
                return null;
            }

            string defaultProperty;
            if (_defaultPropertyCache.TryGetValue(type, out defaultProperty))
            {
                return defaultProperty;
            }

            DefaultPropertyAttribute dpa = GetAttribute<DefaultPropertyAttribute>(type);
            defaultProperty = dpa == null ? null : dpa.Name;
            defaultProperty = defaultProperty == null ? null : defaultProperty.Trim();
            _defaultPropertyCache[type] = defaultProperty;
            return defaultProperty;
        }

        // <summary>
        // Attempts to look up a custom display name from the DisplayNameAttribute.
        // Returns null if the attribute is not defined.
        // </summary>
        // <param name="property">ModelProperty to examine</param>
        // <returns>Custom DisplayName for the property, if any.</returns>
        public static string GetDisplayName(ModelProperty property) 
        {
            DisplayNameAttribute displayNameAttribute = GetAttribute<DisplayNameAttribute>(property);

            if (displayNameAttribute == null)
            {
                return null;
            }

            string displayName = displayNameAttribute.DisplayName;
            if (string.IsNullOrEmpty(displayName))
            {
                return null;
            }

            return displayName;
        }

        // <summary>
        // Gets the description associated with the specified ModelProperty
        // </summary>
        // <param name="property">ModelProperty to examine</param>
        // <returns>The description associated with the specified ModelProperty</returns>
        public static string GetDescription(ModelProperty property) 
        {
            DescriptionAttribute description = GetAttribute<DescriptionAttribute>(property);

            if (description == null || string.IsNullOrEmpty(description.Description))
            {
                return DescriptionAttribute.Default.Description;
            }

            return description.Description;
        }

        // <summary>
        // Instantiates a TypeConverter from a potential TypeConverterAttribute, if one exists.
        // </summary>
        // <param name="item">ModelItem to examine</param>
        // <returns>Instantiated TypeConverter from a potential TypeConverterAttribute, if one exists,
        // null otherwise.</returns>
        public static TypeConverter GetTypeConverter(ModelItem item) 
        {
            return InstantiateTypeConverter(GetAttribute<TypeConverterAttribute>(item));
        }

        // <summary>
        // Gets the TypeConverter associated with the specified ModelProperty, returning
        // null when no TypeConverter is found.
        // </summary>
        // <param name="property">property to examine</param>
        // <returns>Associated TypeConverter if one exists, null otherwise.</returns>
        public static TypeConverter GetTypeConverter(ModelProperty property) 
        {
            return property == null ? null : property.Converter;
        }

        // <summary>
        // Computes the IsReadOnly flag for the specified set of properties, ORing
        // results together for sets of properties larger than 1.
        // </summary>
        // <param name="properties">Properties to examine</param>
        // <param name="isMixedValueDelegate">Delegate that evaluates the IsMixedValue flag for
        // the passed in property values (added as an optimization, since we don't always require
        // the value and it may be computationally expensive)</param>
        // <returns>Flag indicating whether the set of properties is read only or not</returns>
        public static bool IsReadOnly(List<ModelProperty> properties, IsMixedValueEvaluator isMixedValueEvaluator) 
        {
            if (properties == null || properties.Count == 0) 
            {
                Debug.Fail("ExtensibilityAccessor.IsReadOnly: No properties specified.");
                return true;
            }

            Type propertyType = properties[0].PropertyType;

            // ILists are readonly only if value is null
            if (typeof(IList).IsAssignableFrom(propertyType)) 
            {

                if (OrReadOnlyValues(properties)) 
                {
                    IList list = null;
                    if (isMixedValueEvaluator != null)
                    {
                        list = isMixedValueEvaluator() ? null : (ModelUtilities.GetSafeRawValue(properties[0]) as IList);
                    }
                    else
                    {
                        Debug.Fail("No delegate to evaluate IsMixedValue specified.");
                    }

                    if (list == null) 
                    {
                        return true;
                    }
                }

                return false;
            }

            // Arrays and ICollections are readonly
            if (typeof(Array).IsAssignableFrom(propertyType) || typeof(ICollection).IsAssignableFrom(propertyType)) 
            {
                return true;
            }

            // Types that implement ONLY ICollection<> or ONLY IList<> (meaning they
            // don't also implement ICollection or IList, which we handle above)
            // are also readonly
            if (ModelUtilities.ImplementsICollection(propertyType) || ModelUtilities.ImplementsIList(propertyType)) 
            {
                return true;
            }

            // Otherwise, go off of the IsReadOnly value in ModelProperty
            return OrReadOnlyValues(properties);
        }


        // <summary>
        // Looks up and returns the BrowsableAttribute on the specified property.
        // </summary>
        // <param name="property">ModelProperty to examine</param>
        // <returns>True, if the property is marked as browsable, false if it is
        // marked as non-browsable, null if it is unmarked.</returns>
        public static bool? IsBrowsable(ModelProperty property)
        {
            if (property == null)
            {
                return false;
            }

            // Check if the Browsable(true) attribute is explicitly defined.
            BrowsableAttribute browsable = GetAttribute<BrowsableAttribute>(property);

            // If explicit browsable then honor that.
            if (browsable != null)
            {
                return browsable.Browsable;
            }
            return null;
        }

        // <summary>
        // Gets the PropertyOrder token associated with the given ModelProperty
        // </summary>
        // <param name="property">ModelProperty to examine</param>
        // <returns>Associated PropertyOrder token if one exists, null otherwise.</returns>
        public static PropertyOrder GetPropertyOrder(ModelProperty property) 
        {
            if (property == null)
            {
                return null;
            }

            PropertyOrderAttribute attr = GetAttribute<PropertyOrderAttribute>(property);
            if (attr == null)
            {
                return null;
            }

            return attr.Order;
        }

        // <summary>
        // Returns the list of NewItemTypesAttributes that are associated with the
        // specified ModelProperty.  Note that we should never be returning attributes
        // from any of the public methods of ExtensibilityAccessor.  The only reason
        // why we do so here is to pass them to a Blend API that requires it.  However,
        // this is a design flaw and we should not follow suite elsewhere.
        // This method is guaranteed not to return null.
        // </summary>
        // <param name="modelProperty">ModelProperty instance to look up</param>
        // <returns>List of NewItemTypesSttributes associated with the given ModelProperty.</returns>
        public static List<NewItemTypesAttribute> GetNewItemTypesAttributes(ModelProperty property) 
        {

            List<NewItemTypesAttribute> newItemTypesList = new List<NewItemTypesAttribute>();

            foreach (NewItemTypesAttribute newItemTypesAttribute in GetAttributes<NewItemTypesAttribute>(property)) 
            {

                // if there is no custom ItemFactory defined
                if (newItemTypesAttribute.FactoryType == typeof(NewItemFactory)) 
                {
                    foreach (Type type in newItemTypesAttribute.Types) 
                    {
                        //Check if the type "IsConcreteWithDefaultCtor"
                        if (EditorUtilities.IsConcreteWithDefaultCtor(type)) 
                        {
                            newItemTypesList.Add(new NewItemTypesAttribute(type));
                        }
                    }
                }
                else 
                {
                    newItemTypesList.Add(newItemTypesAttribute);
                }
            }
            return newItemTypesList;
        }

        // <summary>
        // Examines the specified ModelProperty for NewItemTypesAttributes and, if found, returns
        // an enumerable of all NewItemFactoryTypeModels specified through them.
        // </summary>
        // <param name="modelProperty">ModelProperty instance to look up</param>
        // <returns>Returns an enumerable of all NewItemFactoryTypeModels specified through custom
        // NewItemFactoryTypeModels, if any.</returns>
        public static IEnumerable<NewItemFactoryTypeModel> GetNewItemFactoryTypeModels(ModelProperty modelProperty, Size desiredIconSize) 
        {
            List<NewItemTypesAttribute> attributes = GetNewItemTypesAttributes(modelProperty);
            if (attributes == null)
            {
                yield break;
            }

            foreach (NewItemTypesAttribute attribute in attributes) 
            {
                NewItemFactory factory = (NewItemFactory)Activator.CreateInstance(attribute.FactoryType);

                foreach (Type type in attribute.Types) 
                {

                    NewItemFactoryTypeModel model = null;

                    if (attribute.FactoryType == typeof(NewItemFactory)) 
                    {
                        if (EditorUtilities.IsConcreteWithDefaultCtor(type)) 
                        {
                            model = new NewItemFactoryTypeModel(type, factory, MessageLogger.Instance);
                        }
                    }
                    else 
                    {
                        model = new NewItemFactoryTypeModel(type, factory, MessageLogger.Instance);
                    }

                    if (model != null) 
                    {
                        model.DesiredSize = desiredIconSize;
                        yield return model;
                    }
                }
            }
        }

        // <summary>
        // Gets all relevant sub-properties from the given ModelItem
        // </summary>
        // <param name="item">Item to examine</param>
        // <returns>Sub-properties exposed by the given ModelItem</returns>
        public static List<ModelProperty> GetSubProperties(ModelItem item) 
        {
            if (item == null)
            {
                return null;
            }

            // First, see if there is a custom TypeConverter.  If so, get the subProperties
            // from there.  Otherwise, get all subProperties including those that aren't browsable,
            // since the Browsability call should be made by the UI, not by the model.
            return GetTypeConverterSubProperties(item) ?? GetAllSubProperties(item);
        }

        // <summary>
        // Gets all relevant sub-properties from the value of the specified
        // ModelProperty
        // </summary>
        // <param name="property">ModelProperty to examine</param>
        // <returns>Sub-properties exposed by the value of the specified ModelProperty</returns>
        public static List<ModelProperty> GetSubProperties(ModelProperty property) 
        {
            if (property.Value == null || ModelUtilities.GetSafeRawValue(property) == null)
            {
                return null;
            }

            // First, see if there is a custom TypeConverter.  If so, get the subProperties
            // from there.  Otherwise, get all subProperties including those that aren't browsable,
            // since the Browsability call should be made by the UI, not by the model.
            return GetTypeConverterSubProperties(property) ?? GetAllSubProperties(property);
        }

        // <summary>
        // try / catch wrapper artound Activator.CreateInstance()
        // </summary>
        // <param name="type">Type to instantiate</param>
        // <returns>Instantiated object or null on error</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static object SafeCreateInstance(Type type) 
        {
            try 
            {
                return Activator.CreateInstance(type);
            }
            catch 
            {
                // ignore errors...
            }

            return null;
        }

        // <summary>
        // Returns the property source based on the following heuristic (in line with
        // Blend's behavior):
        //
        // Xaml                                    Source
        // -------------------------------------------------------------------
        // "123"                                   Local
        // "{Binding}"                             DataBound
        // not specified (default value)           Default
        // not specified (inherited value)         Inherited
        // not specified (from style)              Inherited
        // "{DynamicResource ...}"                 LocalDynamicResource
        // "{StaticResource ...}"                  LocalStaticResource
        // "{x:Static ...}"                        SystemResource
        // "{TemplateBinding ...}"                 TemplateBinding
        // "{CustomMarkup ...}"                    CustomMarkupExtension
        //
        // </summary>
        // <param name="property">Property to examine</param>
        // <returns>Source of the specified property, if any</returns>
        public static PropertyValueSource GetPropertySource(ModelProperty property) 
        {
            if (property == null)
            {
                return null;
            }

            ModelItem valueItem = property.Value;
            PropertyValueSource source = null;

            // Binding or any other known markup extension?
            if (valueItem != null) 
            {
                Type valueType = valueItem.ItemType;

                if (IsStaticExtension(valueType))
                {
                    source = DependencyPropertyValueSource.SystemResource;
                }
                else if (typeof(StaticResourceExtension).IsAssignableFrom(valueType))
                {
                    source = DependencyPropertyValueSource.LocalStaticResource;
                }
                else if (typeof(DynamicResourceExtension).IsAssignableFrom(valueType))
                {
                    source = DependencyPropertyValueSource.LocalDynamicResource;
                }
                else if (typeof(TemplateBindingExtension).IsAssignableFrom(valueType))
                {
                    source = DependencyPropertyValueSource.TemplateBinding;
                }
                else if (typeof(Binding).IsAssignableFrom(valueType))
                {
                    source = DependencyPropertyValueSource.DataBound;
                }
                else if (typeof(MarkupExtension).IsAssignableFrom(valueType))
                {
                    source = DependencyPropertyValueSource.CustomMarkupExtension;
                }
            }

            // If not, is this a local, inherited, or default value?
            if (source == null) 
            {

                if (property.IsSet)
                {
                    source = DependencyPropertyValueSource.Local;
                }
                else 
                {

                    object value = property.ComputedValue;

                    if (object.Equals(value, property.DefaultValue))
                    {
                        source = DependencyPropertyValueSource.DefaultValue;
                    }
                    else if (valueItem != null && valueItem.Source != property)
                    {
                        source = DependencyPropertyValueSource.Inherited;
                    }
                }
            }

            return source;
        }

        // Helper method that ORs the ModelProperty.IsReadOnly values together and 
        // returns the result
        private static bool OrReadOnlyValues(List<ModelProperty> properties) 
        {
            if (properties == null) 
            {
                return true;
            }

            for (int i = 0; i < properties.Count; i++) 
            {
                if (properties[i].IsReadOnly)
                {
                    return true;
                }
            }

            return false;
        }
       
        // Helper method to find if the propertyvalueeditor is reusable for the 
        // given properties collection.
        public static bool IsEditorReusable(IEnumerable<ModelProperty> properties)
        {
            if (properties == null)
            {
                return true;
            }

            foreach (ModelProperty property in properties)
            {
                // even if one property says the editor is not reusable, then 
                // the editor is not reusable for this whole list.
                if (!ExtensibilityMetadataHelper.IsEditorReusable(property.Attributes))
                {
                    return false;
                }
            }
            return true;
        }

        // Hack to deal with {x:Static ...} extensions.  The Cider Markup code currently
        // replaces all StaticExtensions with internal versions of the same class.
        // Once 
        private static bool IsStaticExtension(Type type) 
        {
            return type != null && (
                typeof(StaticExtension).IsAssignableFrom(type) ||
                string.Equals("System.Activities.Presentation.Internal.Xaml.Builtins.StaticExtension", type.FullName));
        }

        // Gets all subProperties from the TypeConverter, if one is explicitely specified
        private static List<ModelProperty> GetTypeConverterSubProperties(ModelItem item) 
        {
            return GetTypeConverterSubPropertiesHelper(item, null);
        }

        // Gets all subProperties from the TypeConverter, if one is explicitely specified
        private static List<ModelProperty> GetTypeConverterSubProperties(ModelProperty property) 
        {
            TypeConverter propertySpecificConverter = property.Converter;
            return GetTypeConverterSubPropertiesHelper(property.Value, propertySpecificConverter);
        }

        private static List<ModelProperty> GetTypeConverterSubPropertiesHelper(ModelItem item, TypeConverter customConverter) 
        {

            if (item == null)
            {
                return null;
            }

            List<ModelProperty> subProperties = null;

            TypeConverter converter = customConverter;

            if (converter == null) 
            {
                // See if there is a converter associated with the item type itself
                converter = ExtensibilityAccessor.GetTypeConverter(item);
            }

            if (converter != null) 
            {
                PropertyDescriptorCollection subPropertyDescriptors =
                    converter.GetProperties(item.GetCurrentValue());

                if (subPropertyDescriptors != null && subPropertyDescriptors.Count > 0) 
                {

                    foreach (PropertyDescriptor subPropertyDescriptor in subPropertyDescriptors) 
                    {

                        ModelProperty subProperty = item.Properties[subPropertyDescriptor.Name];

                        // We want to expose all properties through the model regardless of whether they
                        // are browsable or not.  That distinction should be made by the UI utilizing it
                        if (subProperty != null) 
                        {

                            if (subProperties == null)
                            {
                                subProperties = new List<ModelProperty>();
                            }

                            subProperties.Add(subProperty);
                        }
                    }
                }
            }
            return subProperties;
        }

        // Gets all subProperties that exist
        private static List<ModelProperty> GetAllSubProperties(ModelItem item) 
        {

            if (item == null)
            {
                return null;
            }

            ModelPropertyCollection subModelProperties = item.Properties;
            if (subModelProperties == null)
            {
                return null;
            }

            List<ModelProperty> subProperties = null;

            // We want to expose all properties through the model regardless of whether they
            // are browsable or not.  That distinction should be made by the UI utilizing it
            foreach (ModelProperty subModelProperty in subModelProperties) 
            {

                if (subProperties == null)
                {
                    subProperties = new List<ModelProperty>();
                }

                subProperties.Add(subModelProperty);
            }

            return subProperties;
        }

        // Gets all subProperties that exist
        private static List<ModelProperty> GetAllSubProperties(ModelProperty property) 
        {
            return GetAllSubProperties(property.Value);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        private static TypeConverter InstantiateTypeConverter(TypeConverterAttribute typeConverterAttribute) 
        {
            if (typeConverterAttribute == null)
            {
                return null;
            }

            try 
            {
                Type typeConverterType = Type.GetType(typeConverterAttribute.ConverterTypeName);
                if (typeConverterType != null) 
                {
                    return (TypeConverter)Activator.CreateInstance(typeConverterType);
                }
            }
            catch (Exception) 
            {
                // Ignore failures.  In the future, log these somewhere for 3rd parties to see and debug.
            }

            return null;
        }
    
        // GetAttributes() and GetAttribute<T>()

        public static T GetAttribute<T>(ModelProperty property) where T : Attribute 
        {
            return GetAttribute<T>(property == null ? null : property.Attributes);
        }

        public static T GetAttribute<T>(ModelItem item) where T : Attribute 
        {
            return GetAttribute<T>(item == null ? null : item.Attributes);
        }

        public static T GetAttribute<T>(Type type) where T : Attribute 
        {
            return GetAttribute<T>(type == null ? null : TypeDescriptor.GetAttributes(type));
        }

        public static IEnumerable<T> GetAttributes<T>(ModelProperty property) where T : Attribute 
        {
            return GetAttributes<T>(property == null ? null : property.Attributes);
        }

        public static IEnumerable<T> GetAttributes<T>(Type type) where T : Attribute 
        {
            return GetAttributes<T>(type == null ? null : TypeDescriptor.GetAttributes(type));
        }

        // Note: Calling AttributeCollection[typeof(MyAttribute)] creates a default attribute if 
        // the specified attribute is not found.  That's generally not what we want.
        public static T GetAttribute<T>(AttributeCollection attributes) where T : Attribute 
        {
            T foundAttribute = null;
            if (attributes != null) 
            {
                foreach (Attribute attribute in attributes) 
                {
                    if (typeof(T).IsAssignableFrom(attribute.GetType()))
                    {
                        foundAttribute = attribute as T;
                    }
                }
            }

            return foundAttribute;
        }

        // Note: Calling AttributeCollection[typeof(MyAttribute)] creates a default attribute if 
        // the specified attribute is not found.  That's generally not what we want.
        private static IEnumerable<T> GetAttributes<T>(AttributeCollection attributes) where T : Attribute 
        {
            if (attributes != null) 
            {
                foreach (Attribute attribute in attributes) 
                {
                    if (typeof(T).IsAssignableFrom(attribute.GetType()))
                    {
                        yield return (T)attribute;
                    }
                }
            }
        }

        // <summary>
        // Delegate intended to wrap logic that evaluates the IsMixedValue flag of
        // some property or set of properties.
        // </summary>
        // <returns>True if values are mixed, false otherwise</returns>
        public delegate bool IsMixedValueEvaluator();

        // 
        private class MessageLogger : IMessageLogger 
        {

            private static MessageLogger _instance = new MessageLogger();

            public static MessageLogger Instance 
            { get { return _instance; } }

            public void Clear() 
            {
            }

            public void Write(string text) 
            {
                Debug.Write(text);
            }

            public void WriteLine(string text) 
            {
                Debug.WriteLine(text);
            }
        }
    }
}
