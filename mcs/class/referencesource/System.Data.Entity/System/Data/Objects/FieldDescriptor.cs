//---------------------------------------------------------------------
// <copyright file="FieldDescriptor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Objects.DataClasses;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Objects
{
    internal sealed class FieldDescriptor : PropertyDescriptor
    {
        private readonly EdmProperty _property;
        private readonly Type _fieldType;
        private readonly Type _itemType;
        private readonly bool _isReadOnly;

        /// <summary>
        /// Construct a new instance of the FieldDescriptor class that describes a property
        /// on items of the supplied type.
        /// </summary>
        /// <param name="itemType">Type of object whose property is described by this FieldDescriptor.</param>
        /// <param name="isReadOnly">
        /// <b>True</b> if property value on item can be modified; otherwise <b>false</b>.
        /// </param>
        /// <param name="property">
        /// EdmProperty that describes the property on the item.
        /// </param>
        internal FieldDescriptor(Type itemType, bool isReadOnly, EdmProperty property)
            : base(property.Name, null)
        {
            _itemType = itemType;
            _property = property;
            _isReadOnly = isReadOnly;
            _fieldType = DetermineClrType(_property.TypeUsage);
            System.Diagnostics.Debug.Assert(_fieldType != null, "FieldDescriptor's CLR type has unexpected value of null.");
        }

        /// <summary>
        /// Determine a CLR Type to use a property descriptro form an EDM TypeUsage
        /// </summary>
        /// <param name="typeUsage">The EDM TypeUsage containing metadata about the type</param>
        /// <returns>A CLR type that represents that EDM type</returns>
        private Type DetermineClrType(TypeUsage typeUsage)
        {
            Type result = null;
            EdmType edmType = typeUsage.EdmType;

            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntityType:
                case BuiltInTypeKind.ComplexType:
                    result = edmType.ClrType;
                    break;

                case BuiltInTypeKind.RefType:
                    result = typeof(EntityKey);
                    break;

                case BuiltInTypeKind.CollectionType:
                    TypeUsage elementTypeUse = ((CollectionType)edmType).TypeUsage;
                    result = DetermineClrType(elementTypeUse);
                    result = typeof(IEnumerable<>).MakeGenericType(result);
                    break;

                case BuiltInTypeKind.PrimitiveType:
                case BuiltInTypeKind.EnumType:
                    result = edmType.ClrType;
                    Facet nullable;
                    if (result.IsValueType &&
                        typeUsage.Facets.TryGetValue(DbProviderManifest.NullableFacetName, false, out nullable) &&
                        ((bool)nullable.Value))
                    {
                        result = typeof(Nullable<>).MakeGenericType(result); 
                    }
                    break;

                case BuiltInTypeKind.RowType:
                    result = typeof(IDataRecord);
                    break;

                default:
                    Debug.Fail(string.Format(CultureInfo.CurrentCulture, "The type {0} was not the expected scalar, enumeration, collection, structural, nominal, or reference type.", edmType.GetType()));
                    break;
            }

            return result;
        }

        /// <summary>
        /// Get <see cref="EdmProperty"/> instance associated with this field descriptor.
        /// </summary>
        /// <value>
        /// The <see cref="EdmProperty"/> instance associated with this field descriptor,
        /// or null if there is no EDM property association.
        /// </value>
        internal EdmProperty EdmProperty
        {
            get { return _property; }
        }

        public override Type ComponentType
        {
            get { return _itemType; }
        }
        public override bool IsReadOnly
        {
            get { return _isReadOnly; }
        }
        public override Type PropertyType
        {
            get { return _fieldType; }
        }
        public override bool CanResetValue(object item)
        {
            return false;
        }

        public override object GetValue(object item)
        {
            EntityUtil.CheckArgumentNull(item, "item");

            if (!_itemType.IsAssignableFrom(item.GetType()))
            {
                throw EntityUtil.IncompatibleArgument();
            }

            object propertyValue;

            DbDataRecord dbDataRecord = item as DbDataRecord;
            if (dbDataRecord != null)
            {
                propertyValue = (dbDataRecord.GetValue(dbDataRecord.GetOrdinal(_property.Name)));
            }
            else
            {
                propertyValue = LightweightCodeGenerator.GetValue(_property, item);
            }

            return propertyValue;
        }
        public override void ResetValue(object item)
        {
            throw EntityUtil.NotSupported();
        }

        public override void SetValue(object item, object value)
        {
            EntityUtil.CheckArgumentNull(item, "item");
            if (!_itemType.IsAssignableFrom(item.GetType()))
            {
                throw EntityUtil.IncompatibleArgument();
            }
            if (!_isReadOnly)
            {
                LightweightCodeGenerator.SetValue(_property, item, value);
            } // if not entity it must be readonly
            else
            {
                throw EntityUtil.WriteOperationNotAllowedOnReadOnlyBindingList();
            }
        }
        public override bool ShouldSerializeValue(object item)
        {
            return false;
        }
        public override bool IsBrowsable
        {
            get { return true; }
        }
    }
}
