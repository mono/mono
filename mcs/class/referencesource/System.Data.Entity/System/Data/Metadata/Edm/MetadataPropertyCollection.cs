//---------------------------------------------------------------------
// <copyright file="MetadataPropertyCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Data.Common.Utils;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Metadata collection class supporting delay-loading of system item attributes and
    /// extended attributes.
    /// </summary>
    internal sealed class MetadataPropertyCollection : MetadataCollection<MetadataProperty>
    {
        /// <summary>
        /// Constructor taking item.
        /// </summary>
        /// <param name="item">Item with which the collection is associated.</param>
        internal MetadataPropertyCollection(MetadataItem item)
            : base(GetSystemMetadataProperties(item))
        {
        }

        private readonly static Memoizer<Type, ItemTypeInformation> s_itemTypeMemoizer =
            new Memoizer<Type, ItemTypeInformation>(clrType => new ItemTypeInformation(clrType), null);

        // Given an item, returns all system type attributes for the item.
        private static IEnumerable<MetadataProperty> GetSystemMetadataProperties(MetadataItem item)
        {
            EntityUtil.CheckArgumentNull(item, "item");
            Type type = item.GetType();
            ItemTypeInformation itemTypeInformation = GetItemTypeInformation(type);
            return itemTypeInformation.GetItemAttributes(item);
        }

        // Retrieves metadata for type.
        private static ItemTypeInformation GetItemTypeInformation(Type clrType)
        {
            return s_itemTypeMemoizer.Evaluate(clrType);
        }

        /// <summary>
        /// Encapsulates information about system item attributes for a particular item type.
        /// </summary>
        private class ItemTypeInformation
        {
            /// <summary>
            /// Retrieves system attribute information for the given type.
            /// Requires: type must derive from MetadataItem
            /// </summary>
            /// <param name="clrType">Type</param>
            internal ItemTypeInformation(Type clrType)
            {
                Debug.Assert(null != clrType);

                _itemProperties = GetItemProperties(clrType);
            }

            private readonly List<ItemPropertyInfo> _itemProperties;

            // Returns system item attributes for the given item.
            internal IEnumerable<MetadataProperty> GetItemAttributes(MetadataItem item)
            {
                foreach (ItemPropertyInfo propertyInfo in _itemProperties)
                {
                    yield return propertyInfo.GetMetadataProperty(item);
                }
            }

            // Gets type information for item with the given type. Uses cached information where 
            // available.
            private static List<ItemPropertyInfo> GetItemProperties(Type clrType)
            {
                List<ItemPropertyInfo> result = new List<ItemPropertyInfo>();
                foreach (PropertyInfo propertyInfo in clrType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    foreach (MetadataPropertyAttribute attribute in propertyInfo.GetCustomAttributes(
                        typeof(MetadataPropertyAttribute), false))
                    {
                        result.Add(new ItemPropertyInfo(propertyInfo, attribute));
                    }
                }
                return result;
            }
        }


        /// <summary>
        /// Encapsulates information about a CLR property of an item class.
        /// </summary>
        private class ItemPropertyInfo
        {
            /// <summary>
            /// Initialize information.
            /// Requires: attribute must belong to the given property.
            /// </summary>
            /// <param name="propertyInfo">Property referenced.</param>
            /// <param name="attribute">Attribute for the property.</param>
            internal ItemPropertyInfo(PropertyInfo propertyInfo, MetadataPropertyAttribute attribute)
            {
                Debug.Assert(null != propertyInfo);
                Debug.Assert(null != attribute);

                _propertyInfo = propertyInfo;
                _attribute = attribute;
            }

            private readonly MetadataPropertyAttribute _attribute;
            private readonly PropertyInfo _propertyInfo;

            /// <summary>
            /// Given an item, returns an instance of the item attribute described by this class.
            /// </summary>
            /// <param name="item">Item from which to retrieve attribute.</param>
            /// <returns>Item attribute.</returns>
            internal MetadataProperty GetMetadataProperty(MetadataItem item)
            {
                return new MetadataProperty(_propertyInfo.Name, _attribute.Type, _attribute.IsCollectionType,
                    new MetadataPropertyValue(_propertyInfo, item));
            }
        }
    }
}
