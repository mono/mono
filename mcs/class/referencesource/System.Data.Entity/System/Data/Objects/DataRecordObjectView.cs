//---------------------------------------------------------------------
// <copyright file="DataRecordObjectView.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Metadata;
using System.Data.Metadata.Edm;
using System.Reflection;

namespace System.Data.Objects
{
    /// <summary>
    /// ObjectView that provides binding to a list of data records.
    /// </summary>
    /// <remarks>
    /// This class provides an implementation of ITypedList that returns property descriptors
    /// for each column of results in a data record.  
    /// </remarks>
    internal sealed class DataRecordObjectView : ObjectView<DbDataRecord>, ITypedList
    {
        /// <summary>
        /// Cache of the property descriptors for the element type of the root list wrapped by ObjectView.
        /// </summary>
        private PropertyDescriptorCollection _propertyDescriptorsCache;

        /// <summary>
        /// EDM RowType that describes the shape of record elements.
        /// </summary>
        private RowType _rowType;

        internal DataRecordObjectView(IObjectViewData<DbDataRecord> viewData, object eventDataSource, RowType rowType, Type propertyComponentType)
            : base(viewData, eventDataSource)
        {
            if (!typeof(IDataRecord).IsAssignableFrom(propertyComponentType))
            {
                propertyComponentType = typeof(IDataRecord);
            }

            _rowType = rowType;
            _propertyDescriptorsCache = MaterializedDataRecord.CreatePropertyDescriptorCollection(_rowType, propertyComponentType, true);
        }

        /// <summary>
        /// Return a <see cref="PropertyInfo"/> instance that represents
        /// a strongly-typed indexer property on the specified type.
        /// </summary>
        /// <param name="typedIndexer">
        /// <see cref="Type"/> that may define the appropriate indexer.
        /// </param>
        /// <returns>
        /// <see cref="PropertyInfo"/> instance of indexer defined on supplied type
        /// that returns an object of any type but <see cref="Object"/>;
        /// or null if no such indexer is defined on the supplied type.
        /// </returns>
        /// <remarks>
        /// The algorithm here is lifted from System.Windows.Forms.ListBindingHelper,
        /// from the GetTypedIndexer method.
        /// The Entity Framework could not take a dependency on Microsoft,
        /// so we lifted the appropriate parts from the Microsoft code here.
        /// Not the best, but much better than guessing as to what algorithm is proper for data binding.
        /// </remarks>
        private static PropertyInfo GetTypedIndexer(Type type)
        {
            PropertyInfo indexer = null;

            if (typeof(IList).IsAssignableFrom(type) ||
                typeof(ITypedList).IsAssignableFrom(type) ||
                typeof(IListSource).IsAssignableFrom(type))
            {
                System.Reflection.PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                for (int idx = 0; idx < props.Length; idx++)
                {
                    if (props[idx].GetIndexParameters().Length > 0 && props[idx].PropertyType != typeof(object))
                    {
                        indexer = props[idx];
                        //Prefer the standard indexer, if there is one
                        if (indexer.Name == "Item")
                        {
                            break;
                        }
                    }
                }
            }

            return indexer;
        }

        /// <summary>
        /// Return the element type for the supplied type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>
        /// If <paramref name="type"/> represents a list type that doesn't also implement ITypedList or IListSource,
        /// return the element type for items in that list.
        /// Otherwise, return the type supplied by <paramref name="type"/>.
        /// </returns>
        /// <remarks>
        /// The algorithm here is lifted from System.Windows.Forms.ListBindingHelper,
        /// from the GetListItemType(object) method.
        /// The Entity Framework could not take a dependency on Microsoft,
        /// so we lifted the appropriate parts from the Microsoft code here.
        /// Not the best, but much better than guessing as to what algorithm is proper for data binding.
        /// </remarks>
        private static Type GetListItemType(Type type)
        {
            Type itemType;

            if (typeof(Array).IsAssignableFrom(type))
            {
                itemType = type.GetElementType();
            }
            else
            {
                PropertyInfo typedIndexer = GetTypedIndexer(type);

                if (typedIndexer != null)
                {
                    itemType = typedIndexer.PropertyType;
                }
                else
                {
                    itemType = type;
                }
            }

            return itemType;
        }

        #region ITypedList Members

        PropertyDescriptorCollection System.ComponentModel.ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            PropertyDescriptorCollection propertyDescriptors;

            if (listAccessors == null || listAccessors.Length == 0)
            {
                // Caller is requesting property descriptors for the root element type.
                propertyDescriptors = _propertyDescriptorsCache;
            }
            else
            {
                // Use the last PropertyDescriptor in the array to build the collection of returned property descriptors.
                PropertyDescriptor propertyDescriptor = listAccessors[listAccessors.Length - 1];
                FieldDescriptor fieldDescriptor = propertyDescriptor as FieldDescriptor;

                // If the property descriptor describes a data record with the EDM type of RowType,
                // construct the collection of property descriptors from the property's EDM metadata.
                // Otherwise use the CLR type of the property.
                if (fieldDescriptor != null && fieldDescriptor.EdmProperty != null && fieldDescriptor.EdmProperty.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
                {
                    // Retrieve property descriptors from EDM metadata.
                    propertyDescriptors = MaterializedDataRecord.CreatePropertyDescriptorCollection((RowType)fieldDescriptor.EdmProperty.TypeUsage.EdmType, typeof(IDataRecord), true);
                }
                else
                {
                    // Use the CLR type.
                    propertyDescriptors = TypeDescriptor.GetProperties(GetListItemType(propertyDescriptor.PropertyType));
                }
            }

            return propertyDescriptors;
        }

        string System.ComponentModel.ITypedList.GetListName(PropertyDescriptor[] listAccessors)
        {
            return _rowType.Name;
        }

        #endregion
    }
}
