//---------------------------------------------------------------------
// <copyright file="EntityDataSourceWrapperPropertyDescriptor.cs" company="Microsoft">
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

namespace System.Web.UI.WebControls
{
    /// <summary>
    /// Wrapper property descriptor that handles access of nested members and
    /// entity reference keys.
    /// </summary>
    /// <remarks>
    /// This class essentially glues together a wrapper collection (roughly speaking
    /// the 'data' exposed by the control) and the column (which defines the metadata 
    /// and behaviors for the current property).
    /// </remarks>
    internal sealed class EntityDataSourceWrapperPropertyDescriptor : PropertyDescriptor
    {
        private readonly EntityDataSourceWrapperCollection _collection;
        private readonly EntityDataSourceColumn _column;

        internal EntityDataSourceWrapperPropertyDescriptor(EntityDataSourceWrapperCollection collection, EntityDataSourceColumn column)
            : base(EntityDataSourceUtil.CheckArgumentNull(column, "column").DisplayName, new Attribute[] { })
        {
            EntityDataSourceUtil.CheckArgumentNull(collection, "collection");

            _collection = collection;
            _column = column;
        }

        internal EntityDataSourceColumn Column
        {
            get { return _column; }
        }

        public override Type ComponentType
        {
            get { return _collection.ClrEntityType; }
        }

        public override bool IsReadOnly
        {
            get { return _collection.IsReadOnly || !_column.CanWrite; }
        }

        public override Type PropertyType
        {
            get { return _column.ClrType; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
            throw new InvalidOperationException(Strings.ValueNotResettable(this.ComponentType.Name));
        }

        public override object GetValue(object component)
        {
            return _column.GetValue(GetWrapper(component));
        }

        public override void SetValue(object component, object value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.SetValueNotSupported);
            }
            _column.SetValue(GetWrapper(component), value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        private EntityDataSourceWrapper GetWrapper(object component)
        {
            // Validate that the component comes from the collection to which
            // this descriptor is bound. Elements of the collection are 
            // non-null wrappers instances.
            EntityDataSourceUtil.CheckArgumentNull(component, "component");

            EntityDataSourceWrapper wrapper = component as EntityDataSourceWrapper;
            if (null == wrapper || this._collection != wrapper.Collection)
            {
                throw new ArgumentException(Strings.ComponentNotFromProperCollection, "component");
            }

            return wrapper;
        }
    }
}
