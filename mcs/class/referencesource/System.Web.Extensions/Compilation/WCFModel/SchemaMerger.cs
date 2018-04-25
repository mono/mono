#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

#if WEB_EXTENSIONS_CODE
using System.Web.Resources;
#else
using Microsoft.VSDesigner.WCF.Resources;
#endif

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// a utility class to merge schema files, and remove duplicated part
    /// </summary>
    internal class SchemaMerger
    {
        // xml serializable attributes
        private static Type[] xmlSerializationAttributes = new Type[] {
                            typeof(System.Xml.Serialization.XmlElementAttribute),
                            typeof(System.Xml.Serialization.XmlAttributeAttribute),
                            typeof(System.Xml.Serialization.XmlAnyAttributeAttribute),
                            typeof(System.Xml.Serialization.XmlAnyElementAttribute),
                            typeof(System.Xml.Serialization.XmlTextAttribute),
        };

        // elements in the schema (we don't process annotation node)
        private static SchemaTopLevelItemType[] schemaTopLevelItemTypes = new SchemaTopLevelItemType[] {
            new SchemaTopLevelItemType(typeof(XmlSchemaType), "type"),
            new SchemaTopLevelItemType(typeof(XmlSchemaElement), "element"),
            new SchemaTopLevelItemType(typeof(XmlSchemaAttribute), "attribute"),
            new SchemaTopLevelItemType(typeof(XmlSchemaGroup), "group"),
            new SchemaTopLevelItemType(typeof(XmlSchemaAttributeGroup), "attributeGroup"),
        };

        // when properties defined in those types are different, we only report warnings, but not error messages
        private static Type[] ignorablePropertyTypes = new Type[] {
            typeof(XmlAttribute[]),
            typeof(XmlElement[]),
            typeof(XmlNode[]),
            typeof(XmlSchemaAnnotation),
        };

        private readonly static XmlAttribute[] emptyXmlAttributeCollection = new XmlAttribute[0];
        private readonly static object[] emptyCollection = new object[0];

        /// <summary>
        /// Merge and remove duplicated part from the schema list
        /// </summary>
        /// <param name="schemaList">schemas with names</param>
        /// <param name="importErrors">error messages</param>
        /// <param name="duplicatedSchemas">error messages</param>
        /// <remarks></remarks>
        internal static void MergeSchemas(IEnumerable<XmlSchema> schemaList, IList<ProxyGenerationError> importErrors, out IEnumerable<XmlSchema> duplicatedSchemas)
        {
            if (schemaList == null)
            {
                throw new ArgumentNullException("schemaList");
            }
            if (importErrors == null)
            {
                throw new ArgumentNullException("importErrors");
            }

            List<XmlSchema> duplicatedSchemaList = new List<XmlSchema>();
            duplicatedSchemas = duplicatedSchemaList;

            // types, elements, groups have their own name space
            Dictionary<XmlQualifiedName, XmlSchemaObject>[] knownItemTables = new Dictionary<XmlQualifiedName, XmlSchemaObject>[schemaTopLevelItemTypes.Length];
            for (int i = 0; i < schemaTopLevelItemTypes.Length; i++)
            {
                knownItemTables[i] = new Dictionary<XmlQualifiedName, XmlSchemaObject>();
            }

            foreach (XmlSchema schema in schemaList)
            {

                bool hasNewDefinedItems = false;
                List<XmlSchemaObject> duplicatedItems = new List<XmlSchemaObject>();

                for (int i = 0; i < schemaTopLevelItemTypes.Length; i++)
                {
                    Dictionary<XmlQualifiedName, XmlSchemaObject> knownItemTable = knownItemTables[i];
                    int knownItemCount = knownItemTable.Count;
                    FindDuplicatedItems(schema, schemaTopLevelItemTypes[i].ItemType, schemaTopLevelItemTypes[i].Name, knownItemTable, duplicatedItems, importErrors);
                    if (knownItemTable.Count > knownItemCount)
                    {
                        hasNewDefinedItems = true;
                    }
                }

                if (duplicatedItems.Count > 0)
                {
                    if (!hasNewDefinedItems)
                    {
                        // remove the whole schema...
                        duplicatedSchemaList.Add(schema);
                    }
                    else
                    {
                        // remove duplicated items only
                        foreach (XmlSchemaObject item in duplicatedItems)
                        {
                            schema.Items.Remove(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find duplicated items in a schema
        /// </summary>
        /// <remarks></remarks>
        private static void FindDuplicatedItems(
                    XmlSchema schema,
                    Type itemType,
                    string itemTypeName,
                    Dictionary<XmlQualifiedName, XmlSchemaObject> knownItemTable,
                    List<XmlSchemaObject> duplicatedItems,
                    IList<ProxyGenerationError> importErrors)
        {

            string targetNamespace = schema.TargetNamespace;
            if (String.IsNullOrEmpty(targetNamespace))
            {
                targetNamespace = String.Empty;
            }

            foreach (XmlSchemaObject item in schema.Items)
            {
                if (itemType.IsInstanceOfType(item))
                {

                    XmlQualifiedName combinedName = new XmlQualifiedName(GetSchemaItemName(item), targetNamespace);

                    XmlSchemaObject originalItem = null;
                    if (knownItemTable.TryGetValue(combinedName, out originalItem))
                    {
                        string differentLocation;

                        if (!AreSchemaObjectsEquivalent(originalItem, item, out differentLocation))
                        {
                            differentLocation = CombinePath(".", differentLocation);
                            importErrors.Add(
                                    new ProxyGenerationError(
                                        ProxyGenerationError.GeneratorState.MergeMetadata,
                                        String.Empty,
                                        new InvalidOperationException(
                                            String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_DuplicatedSchemaItems, itemTypeName, combinedName.ToString(), schema.SourceUri, originalItem.SourceUri, differentLocation)
                                        )
                                    )
                            );
                        }
                        else if (!String.IsNullOrEmpty(differentLocation))
                        {
                            // warning: ignorable difference found
                            differentLocation = CombinePath(".", differentLocation);
                            importErrors.Add(
                                    new ProxyGenerationError(
                                        ProxyGenerationError.GeneratorState.MergeMetadata,
                                        String.Empty,
                                        new InvalidOperationException(
                                            String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_DuplicatedSchemaItemsIgnored, itemTypeName, combinedName.ToString(), schema.SourceUri, originalItem.SourceUri, differentLocation)
                                        ),
                                        true        // isWarning = true
                                    )
                            );
                        }
                        duplicatedItems.Add(item);
                    }
                    else
                    {
                        item.SourceUri = schema.SourceUri;
                        knownItemTable.Add(combinedName, item);
                    }
                }
            }
        }

        /// <summary>
        /// Compare two schema objects
        /// </summary>
        /// <return></return>
        /// <remarks>
        /// For all those functions, we follow the same pattern:
        ///  return false: find not-ignorable difference between them.  differentLocation will contain the path
        ///  return true, with empty differentLocation -- no difference found
        ///  return true, with non-empty differentLocation -- ignorable difference found under that location
        /// </remarks>
        private static bool AreSchemaObjectsEquivalent(XmlSchemaObject originalItem, XmlSchemaObject item, out string differentLocation)
        {
            differentLocation = String.Empty;

            Type itemType = originalItem.GetType();
            if (itemType != item.GetType())
            {
                return false;
            }

            string ignorableDifferenceLocation = String.Empty;

            PropertyInfo[] properties = itemType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (IsPersistedProperty(property))
                {

                    bool ignorableProperty = ShouldIgnoreSchemaProperty(property);

                    object originalValue = property.GetValue(originalItem, new object[] { });
                    object newValue = property.GetValue(item, new object[] { });

                    if (!CompareSchemaPropertyValues(property, originalValue, newValue, out differentLocation) && !ignorableProperty)
                    {
                        return false;
                    }
                    if (String.IsNullOrEmpty(ignorableDifferenceLocation))
                    {
                        ignorableDifferenceLocation = differentLocation;
                    }
                }
            }

            differentLocation = ignorableDifferenceLocation;
            return true;
        }

        /// <summary>
        /// Compare two property of a schema object
        /// </summary>
        /// <return>true: if the value are same</return>
        /// <remarks></remarks>
        private static bool CompareSchemaPropertyValues(PropertyInfo propertyInfo, object originalValue, object newValue, out string differentLocation)
        {
            differentLocation = String.Empty;

            if (originalValue == null && newValue == null)
            {
                return true;
            }

            // we create empty collection so a meaningful differentLocation could be generated
            if (typeof(XmlAttribute[]) == propertyInfo.PropertyType)
            {
                if (originalValue == null)
                {
                    originalValue = emptyXmlAttributeCollection;
                }
                if (newValue == null)
                {
                    newValue = emptyXmlAttributeCollection;
                }

                XmlAttribute differentAttribute1, differentAttribute2;
                if (!CompareXmlAttributeCollections((XmlAttribute[])originalValue, (XmlAttribute[])newValue, out differentAttribute1, out differentAttribute2))
                {
                    differentLocation = GetSchemaPropertyNameInXml(propertyInfo, differentAttribute1, differentAttribute2);
                    return false;
                }
                return true;
            }

            if (typeof(System.Collections.ICollection).IsAssignableFrom(propertyInfo.PropertyType))
            {
                if (originalValue == null)
                {
                    originalValue = emptyCollection;
                }
                if (newValue == null)
                {
                    newValue = emptyCollection;
                }

                object differentItem1, differentItem2;
                if (!CompareSchemaCollections((System.Collections.ICollection)originalValue, (System.Collections.ICollection)newValue, out differentItem1, out differentItem2, out differentLocation))
                {
                    differentLocation = CombinePath(GetSchemaPropertyNameInXml(propertyInfo, differentItem1, differentItem2), differentLocation);
                    return false;
                }
                else
                {
                    if (!String.IsNullOrEmpty(differentLocation))
                    {
                        // ignorable difference...
                        differentLocation = CombinePath(GetSchemaPropertyNameInXml(propertyInfo, differentItem1, differentItem2), differentLocation);
                    }
                    return true;
                }
            }

            if (originalValue == null || newValue == null)
            {
                differentLocation = CombinePath(GetSchemaPropertyNameInXml(propertyInfo, originalValue, newValue), differentLocation);
                return false;
            }

            if (originalValue.GetType() != newValue.GetType())
            {
                differentLocation = CombinePath(GetSchemaPropertyNameInXml(propertyInfo, originalValue, newValue), differentLocation);
                return false;
            }

            if (!CompareSchemaValues(originalValue, newValue, out differentLocation))
            {
                differentLocation = CombinePath(GetSchemaPropertyNameInXml(propertyInfo, originalValue, newValue), differentLocation);
                return false;
            }
            else
            {
                // ignorable difference...
                if (!String.IsNullOrEmpty(differentLocation))
                {
                    differentLocation = CombinePath(GetSchemaPropertyNameInXml(propertyInfo, originalValue, newValue), differentLocation);
                }
                return true;
            }
        }

        /// <summary>
        /// Compare two schema values
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static bool CompareSchemaValues(object originalValue, object newValue, out string differentLocation)
        {
            differentLocation = String.Empty;

            if (originalValue == null || newValue == null)
            {
                return (originalValue == null && newValue == null);
            }

            if (originalValue.GetType() != newValue.GetType())
            {
                return false;
            }

            if (originalValue is XmlSchemaObject)
            {
                return AreSchemaObjectsEquivalent((XmlSchemaObject)originalValue, (XmlSchemaObject)newValue, out differentLocation);
            }

            if (originalValue is XmlAttribute)
            {
                return CompareXmlAttributes((XmlAttribute)originalValue, (XmlAttribute)newValue);
            }

            if (originalValue is XmlElement)
            {
                return CompareXmlElements((XmlElement)originalValue, (XmlElement)newValue, out differentLocation);
            }

            if (originalValue is XmlText)
            {
                return CompareXmlTexts((XmlText)originalValue, (XmlText)newValue);
            }

            return originalValue.Equals(newValue);
        }

        /// <summary>
        /// Compare two collections of items
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static bool CompareSchemaCollections(System.Collections.IEnumerable originalCollection, System.Collections.IEnumerable newCollection,
                out object differentItem1, out object differentItem2, out string differentLocation)
        {

            differentLocation = String.Empty;

            System.Collections.IEnumerator list1 = originalCollection.GetEnumerator();
            System.Collections.IEnumerator list2 = newCollection.GetEnumerator();

            string ignorableDifferenceLocation = String.Empty;
            object ignorableDifferenceItem1 = null;
            object ignorableDifferenceItem2 = null;

            do
            {
                differentItem1 = list1.MoveNext() ? list1.Current : null;
                differentItem2 = list2.MoveNext() ? list2.Current : null;

                if (!CompareSchemaValues(differentItem1, differentItem2, out differentLocation))
                {
                    return false;
                }

                if (String.IsNullOrEmpty(ignorableDifferenceLocation))
                {
                    ignorableDifferenceItem1 = differentItem1;
                    ignorableDifferenceItem2 = differentItem2;
                    ignorableDifferenceLocation = differentLocation;
                }
            }
            while (differentItem1 != null && differentItem2 != null);

            Debug.Assert(differentItem1 == null && differentItem2 == null);

            differentLocation = ignorableDifferenceLocation;
            differentItem1 = ignorableDifferenceItem1;
            differentItem2 = ignorableDifferenceItem2;

            return true;
        }

        /// <summary>
        /// Compare two attributes
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static bool CompareXmlAttributes(XmlAttribute attribute1, XmlAttribute attribute2)
        {
            return String.Equals(attribute1.LocalName, attribute2.LocalName, StringComparison.Ordinal) &&
                    String.Equals(attribute1.NamespaceURI, attribute2.NamespaceURI, StringComparison.Ordinal) &&
                    String.Equals(attribute1.Value, attribute2.Value, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compare two attribute collections
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static bool CompareXmlAttributeCollections(System.Collections.ICollection attributeCollection1, System.Collections.ICollection attributeCollection2, out XmlAttribute differentAttribute1, out XmlAttribute differentAttribute2)
        {
            differentAttribute1 = null;
            differentAttribute2 = null;

            XmlAttribute[] attributeArray1 = GetSortedAttributeArray(attributeCollection1);
            XmlAttribute[] attributeArray2 = GetSortedAttributeArray(attributeCollection2);

            object differentItem1, differentItem2;
            string differentLocation;
            if (!CompareSchemaCollections(attributeArray1, attributeArray2, out differentItem1, out differentItem2, out differentLocation))
            {
                differentAttribute1 = (XmlAttribute)differentItem1;
                differentAttribute2 = (XmlAttribute)differentItem2;
                return false;
            }
            return true;
        }

        /// <summary>
        /// sort XmlAttribute array, so we can compare two collections without being affected by the order
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static XmlAttribute[] GetSortedAttributeArray(System.Collections.ICollection attributeCollection)
        {
            XmlAttribute[] attributeArray = new XmlAttribute[attributeCollection.Count];
            int index = 0;
            foreach (XmlAttribute attribute in attributeCollection)
            {
                attributeArray[index++] = attribute;
            }

            Array.Sort(attributeArray, new AttributeComparer());
            return attributeArray;
        }

        /// <summary>
        /// Compare two elements
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static bool CompareXmlElements(XmlElement element1, XmlElement element2, out string differentLocation)
        {
            differentLocation = String.Empty;

            if (!String.Equals(element1.LocalName, element2.LocalName, StringComparison.Ordinal) ||
                    !String.Equals(element1.NamespaceURI, element2.NamespaceURI, StringComparison.Ordinal))
            {
                return false;
            }

            XmlAttribute differentAttribute1, differentAttribute2;
            if (!CompareXmlAttributeCollections(element1.Attributes, element2.Attributes, out differentAttribute1, out differentAttribute2))
            {
                string attributeName1 = differentAttribute1 != null ? "@" + differentAttribute1.LocalName : String.Empty;
                string attributeName2 = differentAttribute2 != null ? "@" + differentAttribute2.LocalName : String.Empty;
                differentLocation = CombineTwoNames(attributeName1, attributeName2);
                return false;
            }

            object differentChild1, differentChild2;
            if (!CompareSchemaCollections(element1.ChildNodes, element2.ChildNodes, out differentChild1, out differentChild2, out differentLocation))
            {
                string child1Name = differentChild1 != null ? ((XmlNode)differentChild1).LocalName : String.Empty;
                string child2Name = differentChild2 != null ? ((XmlNode)differentChild2).LocalName : String.Empty;
                differentLocation = CombinePath(CombineTwoNames(child1Name, child2Name), differentLocation);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Compare two text nodes
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static bool CompareXmlTexts(XmlText text1, XmlText text2)
        {
            return String.Equals(text1.Value, text2.Value, StringComparison.Ordinal);
        }

        /// <summary>
        /// Combine two path (similar to xpath) in error messages
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static string CombinePath(string path1, string path2)
        {
            if (String.IsNullOrEmpty(path1))
            {
                return path2;
            }
            else if (String.IsNullOrEmpty(path2))
            {
                return path1;
            }

            return path1 + "/" + path2;
        }

        /// <summary>
        /// Get Name of a top level schema item
        /// </summary>
        /// <param name="item"></param>
        /// <return></return>
        /// <remarks></remarks>
        private static string GetSchemaItemName(XmlSchemaObject item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            Type itemType = item.GetType();
            PropertyInfo nameProperty = itemType.GetProperty("Name");
            if (nameProperty != null)
            {
                object nameValue = nameProperty.GetValue(item, new object[] { });
                if (nameValue is string)
                {
                    return (string)nameValue;
                }
                return String.Empty;
            }

            return String.Empty;
        }

        /// <summary>
        /// Generate end-user unstandable property name -- we will use name in the schema file, but not name in object model
        /// </summary>
        /// <return></return>
        /// <remarks></remarks>
        private static string GetSchemaPropertyNameInXml(PropertyInfo property, object value1, object value2)
        {
            object[] propertyAttributes = property.GetCustomAttributes(true);
            string name = String.Empty;

            if (propertyAttributes != null)
            {
                string name1 = GetSchemaPropertyNameInXmlHelper(propertyAttributes, value1);
                string name2 = GetSchemaPropertyNameInXmlHelper(propertyAttributes, value2);

                name = CombineTwoNames(name1, name2);
            }

            if (String.IsNullOrEmpty(name))
            {
                Debug.Fail("Why we didn't get a property name with normal routine?");
                name = property.Name;
            }

            return name;
        }

        /// <summary>
        /// Combine names of two properties in error messages
        /// </summary>
        /// <remarks></remarks>
        private static string CombineTwoNames(string name1, string name2)
        {
            string name = String.Empty;
            if (name1.Length > 0)
            {
                if (name2.Length > 0)
                {
                    if (String.Equals(name1, name2, StringComparison.Ordinal))
                    {
                        name = name1;
                    }
                    else
                    {
                        name = name1 + "|" + name2;
                    }
                }
                else
                {
                    name = name1;
                }
            }
            else if (name2.Length > 0)
            {
                name = name2;
            }
            return name;
        }

        /// <summary>
        /// a helper function to generate names
        /// </summary>
        /// <remarks></remarks>
        private static string GetSchemaPropertyNameInXmlHelper(object[] propertyAttributes, object value)
        {
            if (value != null)
            {
                foreach (object attribute in propertyAttributes)
                {
                    if (attribute is System.Xml.Serialization.XmlAttributeAttribute)
                    {
                        return "@" + ((System.Xml.Serialization.XmlAttributeAttribute)attribute).AttributeName;
                    }
                    if (attribute is System.Xml.Serialization.XmlElementAttribute)
                    {
                        System.Xml.Serialization.XmlElementAttribute elementAttribute = (System.Xml.Serialization.XmlElementAttribute)attribute;
                        Type elementType = elementAttribute.Type;
                        if (elementType == null || elementType.IsInstanceOfType(value))
                        {
                            if (value is XmlSchemaObject)
                            {
                                string itemName = GetSchemaItemName((XmlSchemaObject)value);
                                if (itemName.Length > 0)
                                {
                                    return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}[@name='{1}']", elementAttribute.ElementName, itemName);
                                }
                            }
                            return elementAttribute.ElementName;
                        }
                    }
                    if (attribute is System.Xml.Serialization.XmlAnyAttributeAttribute)
                    {
                        if (value is XmlAttribute)
                        {
                            return "@" + ((XmlAttribute)value).LocalName;
                        }
                    }
                    if (attribute is System.Xml.Serialization.XmlAnyElementAttribute)
                    {
                        if (value is XmlElement)
                        {
                            return ((XmlElement)value).LocalName;
                        }
                    }
                    if (attribute is System.Xml.Serialization.XmlTextAttribute)
                    {
                        if (value is XmlText)
                        {
                            return ((XmlText)value).Name;
                        }
                    }
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Check whether a property is persisted with XmlSerialization
        /// </summary>
        /// <param name="property"></param>
        /// <return></return>
        /// <remarks></remarks>
        private static bool IsPersistedProperty(PropertyInfo property)
        {
            object[] propertyAttributes = property.GetCustomAttributes(true);
            if (propertyAttributes != null)
            {
                foreach (object attribute in propertyAttributes)
                {
                    foreach (Type serializationAttibuteType in xmlSerializationAttributes)
                    {
                        if (serializationAttibuteType.IsInstanceOfType(attribute))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// check whether we should report warning but not error messages, when the property is different
        /// </summary>
        /// <remarks></remarks>
        private static bool ShouldIgnoreSchemaProperty(PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            foreach (Type ignoreableType in ignorablePropertyTypes)
            {
                if (propertyType == ignoreableType || propertyType.IsSubclassOf(ignoreableType))
                {
                    return true;
                }
            }

            // special case constraints...
            if (String.Equals(property.Name, "Constraints", StringComparison.Ordinal))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// a helper structure to hold top level items we want to scan
        /// </summary>
        /// <remarks></remarks>
        private struct SchemaTopLevelItemType
        {
            public Type ItemType;
            public string Name;

            public SchemaTopLevelItemType(Type itemType, string name)
            {
                this.ItemType = itemType;
                this.Name = name;
            }
        };

        /// <summary>
        /// Helper class to compare two XmlAttributes
        /// </summary>
        /// <remarks></remarks>
        private class AttributeComparer : System.Collections.Generic.IComparer<XmlAttribute>
        {

            public int Compare(System.Xml.XmlAttribute x, System.Xml.XmlAttribute y)
            {
                int namespaceResult = String.Compare(x.NamespaceURI, y.NamespaceURI, StringComparison.Ordinal);
                if (namespaceResult != 0)
                {
                    return namespaceResult;
                }

                return String.Compare(x.Name, y.Name, StringComparison.Ordinal);
            }
        }
    }
}


