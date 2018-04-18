//---------------------------------------------------------------------
// <copyright file="EntityDataSourceMemberPath.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Data.Common;
using System.Data.Objects;

namespace System.Web.UI.WebControls
{
    /// <summary>
    /// A glorified linked list. Describes a chain of properties from a primitive
    /// type to a root entity.
    /// </summary>
    class EntityDataSourceMemberPath
    {
        private readonly EdmProperty property;
        private readonly PropertyInfo propertyInfo;
        private readonly EntityDataSourceMemberPath parent;
        private readonly bool isLocallyInteresting;
        private readonly Type clrType;
        private readonly bool isKey;

        internal EntityDataSourceMemberPath(MetadataWorkspace ocWorkspace, EntityDataSourceMemberPath parent, EdmProperty property, bool isLocallyInteresting)
        {
            EntityDataSourceUtil.CheckArgumentNull(ocWorkspace, "ocWorkspace");
            EntityDataSourceUtil.CheckArgumentNull(property, "property");
            
            this.property = property;
            this.parent = parent;
            this.isLocallyInteresting = isLocallyInteresting;
            this.clrType = EntityDataSourceUtil.GetMemberClrType(ocWorkspace, property);
            this.isKey = IsPropertyAKey(property);

            // retrieve PropertyInfo (with respect to parent CLR type)
            StructuralType parentType = property.DeclaringType;
            Type parentClrType = EntityDataSourceUtil.GetClrType(ocWorkspace, parentType);

            this.propertyInfo = EntityDataSourceUtil.GetPropertyInfo(parentClrType, this.property.Name);
        }

         /// <summary>
        /// Describes the member path in the form 'property1.property2...'. Use to
        /// determine display name for nested properties in the EDSC.
        /// </summary>
        /// <returns>Description of the </returns>
        internal string GetDescription()
        {
            string prefix = null == this.parent ? string.Empty : this.parent.GetDescription() + ".";
            return prefix + this.property.Name;
        }

        /// <summary>
        /// Indicates whether original values of this member should be preserved.
        /// </summary>
        internal bool IsInteresting
        {
            get
            {
                // a member path is interesting if anything along the path is interesting
                return this.isLocallyInteresting || (null != this.parent && this.parent.IsInteresting);
            }
        }

        /// <summary>
        /// Indicates whether this member represents a primary key value.
        /// </summary>
        internal bool IsKey
        {
            get { return this.isKey; }
        }

        /// <summary>
        /// Indicates whether this member can be assigned a value of null.
        /// </summary>
        internal bool IsNullable
        {
            get { return this.property.Nullable; }
        }

        internal bool IsScalar
        {
            get { return EntityDataSourceUtil.IsScalar(this.property.TypeUsage.EdmType); }
        }

        /// <summary>
        /// Gets the CLR type of the last member in the path.
        /// </summary>
        internal Type ClrType
        {
            get { return this.clrType; }
        }

        internal object GetValue(EntityDataSourceWrapper entity)
        {
            object parentObjectValue = GetParentObjectValue(entity, false /* initialize */); 
            if (null == parentObjectValue)
            {
                // use convention that property of null is null
                return null;
            }
            else
            {
                // get this property
                object propertyValue = this.propertyInfo.GetValue(parentObjectValue, new object[] { });

                return propertyValue;
            }
        }

        internal void SetValue(EntityDataSourceWrapper entity, object value)
        {
            object parentObjectValue = GetParentObjectValue(entity, true /* initialize */);

            // set property value on parent
            this.propertyInfo.SetValue(parentObjectValue, value, new object[] { });
        }

        private object Initialize(EntityDataSourceWrapper entity)
        {
            // get parent's value
            object parentObjectValue = GetParentObjectValue(entity, true /* initialize */);

            // construct type instance for this property
            object propertyValue = EntityDataSourceUtil.InitializeType(this.ClrType);

            // set property
            this.propertyInfo.SetValue(parentObjectValue, propertyValue, new object[] { });

            return propertyValue;
        }

        private object GetParentObjectValue(EntityDataSourceWrapper entity, bool initialize)
        {
            // get parent's value
            object parentObjectValue;

            if (null == this.parent)
            {
                // at the top level, the entity is the value
                parentObjectValue = entity.WrappedEntity;
            }
            else
            {
                parentObjectValue = this.parent.GetValue(entity);

                if (null == parentObjectValue && initialize)
                {
                    parentObjectValue = this.parent.Initialize(entity);
                }
            }

            return parentObjectValue;
        }

        internal string GetEntitySqlValue()
        {
            // it.[member1].[member2]...
            string prefix;

            if (null != parent)
            {
                prefix = parent.GetEntitySqlValue();
            }
            else
            {
                prefix = EntityDataSourceUtil.EntitySqlElementAlias;
            }

            string eSql = prefix + "." + EntityDataSourceUtil.QuoteEntitySqlIdentifier(this.property.Name);

            return eSql;
        }

        private bool IsPropertyAKey(EdmProperty property)
        {
            bool isKey = false;
            EntityType entityType = property.DeclaringType as EntityType;
            if (null != entityType)
            {
                isKey = entityType.KeyMembers.Contains(property);
            }
            return isKey;
        }

        public override string ToString()
        {
            string prefix = null == this.parent ? string.Empty : this.parent.ToString() + "->";
            return prefix + this.property.Name;
        }
    }
}
