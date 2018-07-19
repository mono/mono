//---------------------------------------------------------------------
// <copyright file="PocoPropertyAccessorStrategy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
namespace System.Data.Objects.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Implementation of the property accessor strategy that gets and sets values on POCO entities.  That is,
    /// entities that do not implement IEntityWithRelationships.
    /// </summary>
    internal sealed class PocoPropertyAccessorStrategy : IPropertyAccessorStrategy
    {
        private static readonly MethodInfo s_AddToCollectionGeneric = typeof(PocoPropertyAccessorStrategy).GetMethod("AddToCollection", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo s_RemoveFromCollectionGeneric = typeof(PocoPropertyAccessorStrategy).GetMethod("RemoveFromCollection", BindingFlags.NonPublic | BindingFlags.Static);

        private object _entity;

        /// <summary>
        /// Constructs a strategy object to work with the given entity.
        /// </summary>
        /// <param name="entity">The entity to use</param>
        public PocoPropertyAccessorStrategy(object entity)
        {
            _entity = entity;
        }

        #region Navigation Property Accessors

        #region GetNavigationPropertyValue

        // See IPropertyAccessorStrategy
        public object GetNavigationPropertyValue(RelatedEnd relatedEnd)
        {
            object navPropValue = null;
            if (relatedEnd != null)
            {
                if (relatedEnd.TargetAccessor.ValueGetter == null)
                {
                    Type type = GetDeclaringType(relatedEnd);
                    PropertyInfo propertyInfo = EntityUtil.GetTopProperty(ref type, relatedEnd.TargetAccessor.PropertyName);
                    if (propertyInfo == null)
                    {
                        throw new EntityException(System.Data.Entity.Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, type.FullName));
                    }
                    EntityProxyFactory factory = new EntityProxyFactory();
                    relatedEnd.TargetAccessor.ValueGetter = factory.CreateBaseGetter(type, propertyInfo);
                }
                try
                {
                    navPropValue = relatedEnd.TargetAccessor.ValueGetter(_entity);
                }
                catch (Exception ex)
                {
                    throw new EntityException(System.Data.Entity.Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, _entity.GetType().FullName), ex);
                }
            }
            return navPropValue;
        }

        #endregion

        #region SetNavigationPropertyValue

        // See IPropertyAccessorStrategy
        public void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
            if (relatedEnd != null)
            {
                if (relatedEnd.TargetAccessor.ValueSetter == null)
                {
                    Type type = GetDeclaringType(relatedEnd);
                    PropertyInfo propertyInfo = EntityUtil.GetTopProperty(ref type, relatedEnd.TargetAccessor.PropertyName);
                    if (propertyInfo == null)
                    {
                        throw new EntityException(System.Data.Entity.Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, type.FullName));
                    }
                    EntityProxyFactory factory = new EntityProxyFactory();
                    relatedEnd.TargetAccessor.ValueSetter = factory.CreateBaseSetter(type, propertyInfo);
                }
                try
                {
                    relatedEnd.TargetAccessor.ValueSetter(_entity, value);
                }
                catch (Exception ex)
                {
                    throw new EntityException(System.Data.Entity.Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, _entity.GetType().FullName), ex);
                }
            }
        }

        private static Type GetDeclaringType(RelatedEnd relatedEnd)
        {
            if (relatedEnd.NavigationProperty != null)
            {
                EntityType declaringEntityType = (EntityType)relatedEnd.NavigationProperty.DeclaringType;
                ObjectTypeMapping mapping = System.Data.Common.Internal.Materialization.Util.GetObjectMapping(declaringEntityType, relatedEnd.WrappedOwner.Context.MetadataWorkspace);
                return mapping.ClrType.ClrType;
            }
            else
            {
                return relatedEnd.WrappedOwner.IdentityType;
            }
        }

        private static Type GetNavigationPropertyType(Type entityType, string propertyName)
        {
            Type navPropType;
            PropertyInfo property = EntityUtil.GetTopProperty(entityType, propertyName);
            if (property != null)
            {
                navPropType = property.PropertyType;
            }
            else
            {
                FieldInfo field = entityType.GetField(propertyName);
                if (field != null)
                {
                    navPropType = field.FieldType;
                }
                else
                {
                    throw new EntityException(System.Data.Entity.Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(propertyName, entityType.FullName));
                }
            }
            return navPropType;
        }

        #endregion

        #endregion

        #region Collection Navigation Property Accessors

        #region CollectionAdd

        // See IPropertyAccessorStrategy
        public void CollectionAdd(RelatedEnd relatedEnd, object value)
        {
            object entity = _entity;
            try
            {
                object collection = GetNavigationPropertyValue(relatedEnd);
                if (collection == null)
                {
                    collection = CollectionCreate(relatedEnd);
                    SetNavigationPropertyValue(relatedEnd, collection);
                }
                Debug.Assert(collection != null, "Collection is null");

                // do not call Add if the collection is a RelatedEnd instance
                if (collection == relatedEnd)
                {
                    return;
                }

                if (relatedEnd.TargetAccessor.CollectionAdd == null)
                {
                    relatedEnd.TargetAccessor.CollectionAdd = CreateCollectionAddFunction(entity.GetType(), relatedEnd.TargetAccessor.PropertyName);
                }


                relatedEnd.TargetAccessor.CollectionAdd(collection, value);
            }
            catch (Exception ex)
            {
                throw new EntityException(System.Data.Entity.Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, entity.GetType().FullName), ex);
            }
        }

        // Helper method to create delegate with property setter
        private static Action<object, object> CreateCollectionAddFunction(Type type, string propertyName)
        {
            Type navPropType = GetNavigationPropertyType(type, propertyName);
            Type elementType = EntityUtil.GetCollectionElementType(navPropType);
            Type collectionType = typeof(ICollection<>).MakeGenericType(elementType);

            MethodInfo addToCollection = s_AddToCollectionGeneric.MakeGenericMethod(elementType);
            return (Action<object, object>)addToCollection.Invoke(null, null);
            
        }

        private static Action<object, object> AddToCollection<T>()
        {
            return (collectionArg, item) =>
                {
                    ICollection<T> collection = (ICollection<T>)collectionArg;
                    Array array = collection as Array;
                    if (array != null && array.IsFixedSize)
                    {
                        throw EntityUtil.CannotAddToFixedSizeArray(array);
                    }
                    collection.Add((T)item);
                };
        }

        #endregion

        #region CollectionRemove

        // See IPropertyAccessorStrategy
        public bool CollectionRemove(RelatedEnd relatedEnd, object value)
        {
            object entity = _entity;
            try
            {
                object collection = GetNavigationPropertyValue(relatedEnd);
                if (collection != null)
                {
                    // do not call Add if the collection is a RelatedEnd instance
                    if (collection == relatedEnd)
                    {
                        return true;
                    }

                    if (relatedEnd.TargetAccessor.CollectionRemove == null)
                    {
                        relatedEnd.TargetAccessor.CollectionRemove = CreateCollectionRemoveFunction(entity.GetType(), relatedEnd.TargetAccessor.PropertyName);
                    }

                    return relatedEnd.TargetAccessor.CollectionRemove(collection, value);
                }
            }
            catch (Exception ex)
            {
                throw new EntityException(System.Data.Entity.Strings.PocoEntityWrapper_UnableToSetFieldOrProperty(relatedEnd.TargetAccessor.PropertyName, entity.GetType().FullName), ex);
            }
            return false;
        }

        // Helper method to create delegate with property setter
        private static Func<object, object, bool> CreateCollectionRemoveFunction(Type type, string propertyName)
        {
            Type navPropType = GetNavigationPropertyType(type, propertyName);
            Type elementType = EntityUtil.GetCollectionElementType(navPropType);
            Type collectionType = typeof(ICollection<>).MakeGenericType(elementType);

            MethodInfo removeFromCollection = s_RemoveFromCollectionGeneric.MakeGenericMethod(elementType);
            return (Func<object, object, bool>)removeFromCollection.Invoke(null, null);
        }

        private static Func<object, object, bool> RemoveFromCollection<T>()
        {
            return (collectionArg, item) =>
            {
                ICollection<T> collection = (ICollection<T>)collectionArg;
                Array array = collection as Array;
                if (array != null && array.IsFixedSize)
                {
                    throw EntityUtil.CannotRemoveFromFixedSizeArray(array);
                }
                return collection.Remove((T)item);
            };
        }

        #endregion

        #region CollectionCreate

        // See IPropertyAccessorStrategy
        public object CollectionCreate(RelatedEnd relatedEnd)
        {
            if (_entity is IEntityWithRelationships)
            {
                return relatedEnd;
            }
            else
            {
                if (relatedEnd.TargetAccessor.CollectionCreate == null)
                {
                    Type entityType = _entity.GetType();
                    string propName = relatedEnd.TargetAccessor.PropertyName;
                    Type navPropType = GetNavigationPropertyType(entityType, propName);
                    relatedEnd.TargetAccessor.CollectionCreate = CreateCollectionCreateDelegate(entityType, navPropType, propName);
                }
                return relatedEnd.TargetAccessor.CollectionCreate();

            }
        }

        /// <summary>
        /// We only get here if a navigation property getter returns null.  In this case, we try to set the
        /// navigation property to some collection that will work.
        /// </summary>
        private static Func<object> CreateCollectionCreateDelegate(Type entityType, Type navigationPropertyType, string propName)
        {
            var typeToInstantiate = EntityUtil.DetermineCollectionType(navigationPropertyType);

            if (typeToInstantiate == null)
            {
                throw new EntityException(System.Data.Entity.Strings.PocoEntityWrapper_UnableToMaterializeArbitaryNavPropType(propName, navigationPropertyType));
            }

            return Expression.Lambda<Func<object>>(Expression.New(typeToInstantiate)).Compile();
        }

        #endregion

        #endregion
    }
}
