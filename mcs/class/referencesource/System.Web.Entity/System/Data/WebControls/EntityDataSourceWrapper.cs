//---------------------------------------------------------------------
// <copyright file="EntityDataSourceWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Reflection;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
    /// <summary>
    /// Wraps an entity displayed in the data source in an ICustomTypeDescriptor
    /// implementation that flattens complex types and exposes references.
    /// </summary>
    internal class EntityDataSourceWrapper : ICustomTypeDescriptor
    {
        private readonly EntityDataSourceWrapperCollection _collection;
        private readonly ObjectStateEntry _stateEntry;

        internal EntityDataSourceWrapper(EntityDataSourceWrapperCollection collection, object trackedEntity)
        {
            EntityDataSourceUtil.CheckArgumentNull(collection, "collection");
            EntityDataSourceUtil.CheckArgumentNull(trackedEntity, "trackedEntity");

            this._collection = collection;

            // retrieve state entry
            if (!this._collection.Context.ObjectStateManager.TryGetObjectStateEntry(trackedEntity, out _stateEntry))
            {
                throw new ArgumentException(Strings.ComponentNotFromProperCollection, "trackedEntity");
            }
        }

        /// <summary>
        /// Gets entity wrapped by this type descriptor.
        /// </summary>
        internal object WrappedEntity
        {
            get
            {
                return this._stateEntry.Entity;
            }
        }

        internal RelationshipManager RelationshipManager
        {
            get
            {
                return this._stateEntry.RelationshipManager;
            }
        }

        /// <summary>
        /// Gets collection containing this wrapper.
        /// </summary>
        internal EntityDataSourceWrapperCollection Collection
        {
            get { return this._collection; }
        }

        #region ICustomTypeDescriptor Implementation
        System.ComponentModel.AttributeCollection System.ComponentModel.ICustomTypeDescriptor.GetAttributes() { return new System.ComponentModel.AttributeCollection(); }
        string ICustomTypeDescriptor.GetClassName() { return null; }
        string ICustomTypeDescriptor.GetComponentName() { return null; }
        TypeConverter ICustomTypeDescriptor.GetConverter() { return null; }
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() { return null; }
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() { return null; }
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) { return null; }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() { return null; }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) { return null; }


        public PropertyDescriptorCollection GetProperties()
        {
            return ((ITypedList)this._collection).GetItemProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return ((ICustomTypeDescriptor)this).GetProperties();
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.WrappedEntity;
        }
        #endregion ICustomTypeDescriptor Implementation

        /// <summary>
        /// Use this method to set the properties on the wrapped entity
        /// </summary>
        /// <param name="propertiesFromViewState"></param>
        /// <param name="wrapper"></param>
        /// <param name="overwriteSameValue"></param>
        internal void SetAllProperties(Dictionary<string, object> propertiesFromViewState, bool overwriteSameValue,
            ref Dictionary<string, Exception> propertySettingExceptionsCaught)
        {
            // We aggregate the reference descriptors rather than setting them directly
            // to account for compound keys (we need all components of the key to create
            // an EntityKey that can be set on the EntityReference)
            var referenceList = new List<KeyValuePair<EntityDataSourceReferenceKeyColumn, object>>();

            foreach (EntityDataSourceWrapperPropertyDescriptor descriptor in _collection.AllPropertyDescriptors)
            {
                // figure out which display name to match for this descriptor
                string displayName = descriptor.Column.DisplayName;

                // if we have a controlling column, use its display name instead
                if (descriptor.Column.ControllingColumn != null)
                {
                    displayName = descriptor.Column.ControllingColumn.DisplayName;
                }

                object value;
                if (propertiesFromViewState.TryGetValue(displayName, out value))
                {
                    // get all changed ReferencePropertyDescriptor from ViewState
                    EntityDataSourceReferenceKeyColumn referenceColumn = descriptor.Column as EntityDataSourceReferenceKeyColumn;

                    // convert the value as needed
                    object adjustedValue = EntityDataSourceUtil.ConvertType(value, descriptor.PropertyType, descriptor.DisplayName);

                    if (null != referenceColumn)
                    {
                        referenceList.Add(new KeyValuePair<EntityDataSourceReferenceKeyColumn, object>(
                                referenceColumn, adjustedValue));
                        continue;
                    }

                    if (overwriteSameValue || adjustedValue != descriptor.GetValue(this))
                    {
                        if (EntityDataSourceUtil.NullCanBeAssignedTo(descriptor.PropertyType) || null != adjustedValue)
                        {
                            try
                            {
                                descriptor.SetValue(this, adjustedValue);
                            }
                            catch (Exception e)
                            {
                                // The property descriptor uses reflection to set the property. Therefore, the inner exception contains the actual message.
                                Exception exceptionToThrow = e;
                                if (e.InnerException != null)
                                {
                                    exceptionToThrow = e.InnerException;
                                }
                                if (null == propertySettingExceptionsCaught)
                                {
                                    propertySettingExceptionsCaught = new Dictionary<string, Exception>();
                                }
                                propertySettingExceptionsCaught.Add(descriptor.DisplayName, exceptionToThrow);
                            }
                        }
                    }
                }
            }

            // aggregate setting for EntityKey
            SetEntityKeyProperties(referenceList, overwriteSameValue);
        }

        private void SetEntityKeyProperties(
            List<KeyValuePair<EntityDataSourceReferenceKeyColumn, object>> referenceList, bool overwriteSameValue)
        {
            EntityDataSourceUtil.CheckArgumentNull(referenceList, "referenceList");

            var groups = referenceList.GroupBy(r => r.Key.Group);

            foreach (var group in groups)
            {
                Dictionary<string, object> partialKeys = new Dictionary<string, object>();

                foreach (KeyValuePair<EntityDataSourceReferenceKeyColumn, object> reference in group)
                {
                    // convert the value as needed
                    EntityDataSourceReferenceKeyColumn column = reference.Key;
                    object keyValue = reference.Value;

                    if (null == keyValue)
                    {
                        partialKeys = null;
                        break;
                    }

                    partialKeys.Add(column.KeyMember.Name, keyValue);
                }

                // we only set the entitykey for once, although there might be more than one 
                // properties descriptor associated with the same entitykey
                group.Key.SetKeyValues(this, partialKeys);
            }
        }
    }
}
