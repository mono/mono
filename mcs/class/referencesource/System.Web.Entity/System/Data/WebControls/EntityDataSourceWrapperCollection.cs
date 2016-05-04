//---------------------------------------------------------------------
// <copyright file="EntityDataSourceWrapperCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Mapping;
using System.Data.Objects;
using System.Reflection;
using System.Diagnostics;
using System.Data.Common;
using System.Data.Objects.DataClasses;
using System.Globalization;
using System.Data.EntityClient;
using System.Collections.ObjectModel;


namespace System.Web.UI.WebControls
{
    /// <summary>
    /// Summary description for EntityDataSourceWrapperCollection
    /// </summary>
    internal class EntityDataSourceWrapperCollection : IEnumerable, ICollection, ITypedList
    {
        private readonly ObjectContext _context;
        private readonly List<EntityDataSourceWrapper> _wrapperList;

        /// <summary>
        /// Gets the property descriptors exposed to the user.
        /// </summary>
        private readonly PropertyDescriptorCollection _visiblePropertyDescriptors;

        /// <summary>
        /// Gets all property descriptors.
        /// </summary>
        internal readonly ReadOnlyCollection<EntityDataSourceWrapperPropertyDescriptor> AllPropertyDescriptors; 

        private readonly bool _isReadOnly;
        private readonly Type _clrEntityType;

        internal EntityDataSourceWrapperCollection(ObjectContext context, EntitySet entitySet, EntityType restrictedEntityType)
        {
            EntityDataSourceUtil.CheckArgumentNull(context, "context");
            EntityDataSourceUtil.CheckArgumentNull(entitySet, "entitySet");

            _context = context;
            _wrapperList = new List<EntityDataSourceWrapper>();

            // get handles on the relevant workspaces
            MetadataWorkspace csWorkspace = ((EntityConnection)context.Connection).GetMetadataWorkspace();
            MetadataWorkspace ocWorkspace = context.MetadataWorkspace;

            // if no restricted type is given, we assume the entity set element type is exposed
            EntityType entityType = restrictedEntityType ?? entitySet.ElementType;
            _clrEntityType = EntityDataSourceUtil.GetClrType(ocWorkspace, entityType);
            
            // if no restricted type is given and the set is polymorphic, make the collection readonly
            if (null == restrictedEntityType &&
                1 < EntityDataSourceUtil.GetTypeAndSubtypesOf(entityType, csWorkspace.GetItemCollection(DataSpace.CSpace), true).Count())
            {
                _isReadOnly = true;
            }

            // gather the properties
            ReadOnlyCollection<EntityDataSourceColumn> columns = EntityDataSourceUtil.GetNamedColumns(csWorkspace, ocWorkspace, entitySet, entityType);
            List<PropertyDescriptor> visiblePropertyDescriptors = new List<PropertyDescriptor>(columns.Count);
            List<EntityDataSourceWrapperPropertyDescriptor> propertyDescriptors = new List<EntityDataSourceWrapperPropertyDescriptor>(columns.Count);
            foreach (EntityDataSourceColumn column in columns)
            {
                var descriptor = new EntityDataSourceWrapperPropertyDescriptor(this, column);
                propertyDescriptors.Add(descriptor);

                // if the descriptor does not have a dependent, it is exposed to the user
                if (!descriptor.Column.IsHidden)
                {
                    visiblePropertyDescriptors.Add(descriptor);
                }
            }

            _visiblePropertyDescriptors = new PropertyDescriptorCollection(visiblePropertyDescriptors.ToArray(), true);
            AllPropertyDescriptors = propertyDescriptors.AsReadOnly();
        }


        internal EntityDataSourceWrapper AddWrappedEntity(object entity)
        {
            EntityDataSourceWrapper wrapper = new EntityDataSourceWrapper(this, entity);
            this._wrapperList.Add(wrapper);
            return wrapper;
        }

        #region IEnumerable Implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._wrapperList.GetEnumerator();
        }
        #endregion IEnumerable Implementation

        #region ICollection Implementation
        public int Count
        {
            get { return this._wrapperList.Count; }
        }
        public void CopyTo(Array array, int index)
        {
            ((ICollection)this._wrapperList).CopyTo(array, index);
        }
        public bool IsSynchronized
        {
            get { return false; }
        }
        public object SyncRoot
        {
            get { return null; }
        }
        #endregion

        #region ITypedList Implementation
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (null == listAccessors)
            {
                return this._visiblePropertyDescriptors ;
            }
            else
            {
                return null;// Implement this feature when we support traversing collections.
            }

        }

        string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
        {
            return null;
        }
        #endregion

        #region Internal properties
        /// <summary>
        /// Gets CLR type or base type for entities exposed in this collection.
        /// </summary>
        internal Type ClrEntityType 
        { 
            get { return _clrEntityType; } 
        }

        /// <summary>
        /// Indicates whether this configuration supports modifications.
        /// </summary>
        internal bool IsReadOnly 
        { 
            get { return _isReadOnly; } 
        }

        /// <summary>
        /// Gets object context tracking the contents of this collection.
        /// </summary>
        internal ObjectContext Context 
        { 
            get { return _context; } 
        }
        #endregion
    }
}
