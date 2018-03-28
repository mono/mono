//---------------------------------------------------------------------
// <copyright file="EntityDataSourceColumn.cs" company="Microsoft">
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
using System.ComponentModel;
using System.Data.Common;
using System.Data.Objects.DataClasses;
using System.Data.Objects;
using System.Data;
using System.Runtime.CompilerServices;

namespace System.Web.UI.WebControls
{
    /// <summary>
    /// Represents a column in EntityDataSourceView.
    /// </summary>
    internal abstract class EntityDataSourceColumn
    {
        protected EntityDataSourceColumn(string displayName)
            : this(displayName, (EntityDataSourceColumn)null)
        {
        }

        protected EntityDataSourceColumn(string displayName, EntityDataSourceColumn controllingColumn)
        {
            EntityDataSourceUtil.CheckArgumentNull(displayName, "displayName");

            this.DisplayName = displayName;
            this.ControllingColumn = controllingColumn;
        }

        /// <summary>
        /// Gets the display name for this column.
        /// </summary>
        internal readonly string DisplayName;

        /// <summary>
        /// Gets the column exposed to the user. For instance, the reference key
        /// it.Order.OrderID might have a dependent it.OrderID where there is a
        /// ReferentialConstraint.
        /// </summary>
        internal readonly EntityDataSourceColumn ControllingColumn;

        /// <summary>
        /// Gets value indicating whether the column should be exposed to the user.
        /// </summary>
        internal bool IsHidden
        {
            get 
            {
                // Columns with dependents are not shown to the user. They are
                // merely used to plumb values (e.g. via referential integrity
                // constraints)
                return this.ControllingColumn != null; 
            }
        }

        /// <summary>
        /// Gets the CLR type for the column value.
        /// </summary>
        internal abstract Type ClrType { get; }

        /// <summary>
        /// Gets a value indicating whether the original value for the column
        /// needs to be preserved.
        /// </summary>
        internal abstract bool IsInteresting { get; }

        /// <summary>
        /// Gets a value indicating whether the column can be modified. Can be
        /// overridden by the collection (which may be readonly).
        /// </summary>
        internal abstract bool CanWrite { get; }

        /// <summary>
        /// Indicates whether this column can be assigned a value of null.
        /// </summary>
        internal abstract bool IsNullable { get; }

        /// <summary>
        /// Indicates whether this column represents a scalar type.
        /// </summary>
        internal abstract bool IsScalar { get; }

        /// Returns an Entity-SQL representation of this column with respect
        /// to entity parameter 'it'.
        /// </summary>
        /// <returns>Entity-SQL string.</returns>
        internal abstract string GetEntitySqlValue();

        internal abstract object GetValue(EntityDataSourceWrapper entity);
        internal abstract void SetValue(EntityDataSourceWrapper entity, object value);
    }

    /// <summary>
    /// An EntityDataSourceView column that is an entity type or complex type property.
    /// </summary>
    internal class EntityDataSourcePropertyColumn : EntityDataSourceColumn
    {
        private readonly EntityDataSourceMemberPath memberPath;

        internal EntityDataSourcePropertyColumn(EntityDataSourceMemberPath memberPath)
            : base(EntityDataSourceUtil.CheckArgumentNull(memberPath, "memberPath").GetDescription())
        {
            this.memberPath = memberPath;
        }

        internal override bool IsInteresting
        {
            get 
            {
                // the member path knows if its interesting...
                return this.memberPath.IsInteresting;
            }
        }

        internal override bool CanWrite
        {
            get 
            {
                // can always write
                return true; 
            }
        }

        internal override bool IsNullable
        {
            get { return memberPath.IsNullable; }
        }

        internal override bool IsScalar
        {
            get { return memberPath.IsScalar; }
        }

        internal override Type ClrType
        {
            get { return this.memberPath.ClrType; }
        }

        override internal object GetValue(EntityDataSourceWrapper entity)
        {
            return this.memberPath.GetValue(entity);
        }

        override internal void SetValue(EntityDataSourceWrapper entity, object value)
        {
            this.memberPath.SetValue(entity, value);
        }

        internal override string GetEntitySqlValue()
        {
            return this.memberPath.GetEntitySqlValue();
        }

        public override string ToString()
        {
            return this.memberPath.ToString();
        }

        /// <summary>
        /// Indicates whether this column represents a primary key value;
        /// </summary>
        internal bool IsKey
        {
            get { return memberPath.IsKey; }
        }
    }

    /// <summary>
    /// An EntityDataSourceView column 
    /// </summary>
    internal class EntityDataSourceReferenceKeyColumn : EntityDataSourceColumn
    {
        private readonly EntityDataSourceReferenceGroup group;
        private readonly EdmProperty keyMember;
        private readonly Type clrType;
        private readonly bool isNullable;

        internal EntityDataSourceReferenceKeyColumn(MetadataWorkspace workspace, EntityDataSourceReferenceGroup group, EdmProperty keyMember, EntityDataSourceColumn dependent)
            : base(CreateDisplayName(group, keyMember), dependent)
        {
            EntityDataSourceUtil.CheckArgumentNull(group, "group");
            EntityDataSourceUtil.CheckArgumentNull(keyMember, "keyMember");
            Debug.Assert(EntityDataSourceUtil.IsScalar(keyMember.TypeUsage.EdmType), "Expected primitive or enum type for key members.");

            this.group = group;
            this.keyMember = keyMember;
            this.clrType = EntityDataSourceUtil.GetMemberClrType(workspace, keyMember);

            // if the association end is optional (0..1), make sure the CLR type
            // is also nullable
            if (this.group.End.CorrespondingAssociationEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)
            {
                this.clrType = EntityDataSourceUtil.MakeNullable(clrType);
                this.isNullable = true;
            }
        }

        internal override bool IsInteresting
        {
            get 
            {
                // references are always interesting
                return true;
            }
        }

        internal override bool CanWrite
        {
            get 
            {
                // references can always be written
                return true;
            }
        }

        internal override bool IsNullable
        {
            get { return this.isNullable; }
        }

        internal override bool IsScalar
        {
            get { return EntityDataSourceUtil.IsScalar(keyMember.TypeUsage.EdmType); }
        }

        internal override Type ClrType
        {
            get { return this.clrType; }
        }

        internal EntityDataSourceReferenceGroup Group
        {
            get { return this.group; }
        }

        internal EdmMember KeyMember
        {
            get { return this.keyMember; }
        }

        private static string CreateDisplayName(EntityDataSourceReferenceGroup group, EdmProperty keyMember)
        {
            EntityDataSourceUtil.CheckArgumentNull(group, "group");
            EntityDataSourceUtil.CheckArgumentNull(keyMember, "keyMember");

            NavigationProperty navigationProperty;

            string result;

            if (EntityDataSourceUtil.TryGetCorrespondingNavigationProperty(group.End.CorrespondingAssociationEndMember, out navigationProperty))
            {
                result = navigationProperty.Name + "." + keyMember.Name;
            }
            else
            {
                // if there is no Navigation property, use the TargetTole and KeyMember name
                // TargetRole.KeyMember
                result = group.End.Name + "." + keyMember.Name;
            }

            return result;
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "<Set = {0}, Role = {1}>",
                this.group.End.ParentAssociationSet.Name, this.group.End.Name);
        }

        internal override object GetValue(EntityDataSourceWrapper entity)
        {
            EntityKey entityKey = this.Group.GetEntityKey(entity);
            if (null == entityKey)
            {
                return null;
            }
            else
            {
                object value = null;
                // loop through to find the correct keymember, take compound key into consideration
                foreach (EntityKeyMember entityKeyValue in entityKey.EntityKeyValues)
                {
                    if (entityKeyValue.Key == this.KeyMember.Name)
                    {
                        value = entityKeyValue.Value;
                    }
                }
                return value;
            }
        }

        internal override void SetValue(EntityDataSourceWrapper entity, object value)
        {
            throw new InvalidOperationException(Strings.SetValueNotSupported);
        }

        internal override string GetEntitySqlValue()
        {
            // syntax: NAVIGATE(it, _association_type_name_, _target_role_name_)._key_member_
            StringBuilder builder = new StringBuilder();

            builder.Append("NAVIGATE(");
            builder.Append(EntityDataSourceUtil.EntitySqlElementAlias);
            builder.Append(", ");
            builder.Append(EntityDataSourceUtil.CreateEntitySqlTypeIdentifier(this.Group.End.ParentAssociationSet.ElementType));
            builder.Append(", ");
            builder.Append(EntityDataSourceUtil.QuoteEntitySqlIdentifier(this.Group.End.CorrespondingAssociationEndMember.Name));
            builder.Append(").");
            builder.Append(EntityDataSourceUtil.QuoteEntitySqlIdentifier(this.keyMember.Name));
            string result = builder.ToString();

            return result;
        }
    }

    internal abstract class EntityDataSourceReferenceValueColumn : EntityDataSourceColumn
    {
        private readonly NavigationProperty navigationProperty;

        protected EntityDataSourceReferenceValueColumn(MetadataWorkspace ocWorkspace, NavigationProperty navigationProperty)
            : base(EntityDataSourceUtil.CheckArgumentNull(navigationProperty, "navigationProperty").Name)
        {
            EntityDataSourceUtil.CheckArgumentNull(ocWorkspace, "ocWorkspace");

            this.navigationProperty = navigationProperty;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static EntityDataSourceReferenceValueColumn Create(Type clrToType, MetadataWorkspace ocWorkspace, NavigationProperty navigationProperty)
        {
            EntityDataSourceUtil.CheckArgumentNull(clrToType, "clrToType");

            Type columnType = typeof(EntityDataSourceReferenceValueColumn<>).MakeGenericType(clrToType);
            EntityDataSourceReferenceValueColumn result = (EntityDataSourceReferenceValueColumn)Activator.CreateInstance(columnType, ocWorkspace, navigationProperty);
            return result;
        }

        internal override bool CanWrite
        {
            get
            {
                // can never write to a navigation reference
                return false;
            }
        }

        internal override bool IsNullable
        {
            get { return navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne; }
        }

        protected NavigationProperty NavigationProperty
        {
            get { return this.navigationProperty; }
        }

        internal override bool IsScalar
        {
            get { return false; }
        }

        internal override string GetEntitySqlValue()
        {
            // it.NavigationPropertyName
            string result = EntityDataSourceUtil.EntitySqlElementAlias + "." + EntityDataSourceUtil.QuoteEntitySqlIdentifier(this.navigationProperty.Name);
            return result;
        }

        internal override bool IsInteresting
        {
            get
            {
                // a navigation reference is not written, so its original values aren't interesting
                return false;
            }
        }

        internal override void SetValue(EntityDataSourceWrapper entity, object value)
        {
            throw new InvalidOperationException(Strings.SetValueNotSupported);
        }
    }

    internal class EntityDataSourceReferenceValueColumn<T> : EntityDataSourceReferenceValueColumn
        where T : class
    {
        public EntityDataSourceReferenceValueColumn(MetadataWorkspace ocWorkspace, NavigationProperty navigationProperty)
            : base(ocWorkspace, navigationProperty)
        {
        }

        internal override object GetValue(EntityDataSourceWrapper entity)
        {
            object result;
            EntityReference<T> reference = GetRelatedReference(entity);
            if (reference.IsLoaded)
            {
                result = reference.Value;
            }
            else
            {
                result = null;
            }
            return result;
        }

        internal override Type ClrType
        {
            get
            {
                return typeof(T);
            }
        }

        private EntityReference<T> GetRelatedReference(EntityDataSourceWrapper entity)
        {
            RelationshipManager relationshipManager = entity.RelationshipManager;
            Debug.Assert(relationshipManager != null, "Coldn't retrieve a RelationshipManager");
            EntityReference<T> reference = relationshipManager.GetRelatedReference<T>(
                this.NavigationProperty.RelationshipType.FullName,
                this.NavigationProperty.ToEndMember.Name);
            return reference;
        }
    }
}
