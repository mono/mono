//---------------------------------------------------------------------
// <copyright file="EntityProxyTypeInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Contains the Type of a proxy class, along with any behaviors associated with that proxy Type.
    /// </summary>
    internal sealed class EntityProxyTypeInfo
    {
        private readonly Type _proxyType;
        private readonly ClrEntityType _entityType;        // The OSpace entity type that created this proxy info

        internal const string EntityWrapperFieldName = "_entityWrapper";
        private const string InitializeEntityCollectionsName = "InitializeEntityCollections";
        private readonly DynamicMethod _initializeCollections;

        private readonly Func<object, string, object> _baseGetter;
        private readonly HashSet<string> _propertiesWithBaseGetter;
        private readonly Action<object, string, object> _baseSetter;
        private readonly HashSet<string> _propertiesWithBaseSetter;
        private readonly Func<object, object> Proxy_GetEntityWrapper;
        private readonly Func<object, object, object> Proxy_SetEntityWrapper; // IEntityWrapper Func(object proxy, IEntityWrapper value)

        private readonly Func<object> _createObject;

        // An index of relationship metadata strings to an AssociationType
        // This is used when metadata is not otherwise available to the proxy
        private readonly Dictionary<Tuple<string, string>, AssociationType> _navigationPropertyAssociationTypes;

        internal EntityProxyTypeInfo(Type proxyType, ClrEntityType ospaceEntityType, DynamicMethod initializeCollections, List<PropertyInfo> baseGetters, List<PropertyInfo> baseSetters)
        {
            Debug.Assert(proxyType != null, "proxyType must be non-null");

            _proxyType = proxyType;
            _entityType = ospaceEntityType;

            _initializeCollections = initializeCollections;

            _navigationPropertyAssociationTypes = new Dictionary<Tuple<string, string>, AssociationType>();
            foreach (NavigationProperty navigationProperty in ospaceEntityType.NavigationProperties)
            {
                _navigationPropertyAssociationTypes.Add(
                    new Tuple<string, string>(
                        navigationProperty.RelationshipType.FullName,
                        navigationProperty.ToEndMember.Name),
                    (AssociationType)navigationProperty.RelationshipType);

                if (navigationProperty.RelationshipType.Name != navigationProperty.RelationshipType.FullName)
                {
                    // Sometimes there isn't enough metadata to have a container name
                    // Default codegen doesn't qualify names
                    _navigationPropertyAssociationTypes.Add(
                        new Tuple<string, string>(
                            navigationProperty.RelationshipType.Name,
                            navigationProperty.ToEndMember.Name),
                        (AssociationType)navigationProperty.RelationshipType);
                }
            }

            FieldInfo entityWrapperField = proxyType.GetField(EntityWrapperFieldName, BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            ParameterExpression Object_Parameter = Expression.Parameter(typeof(object), "proxy");
            ParameterExpression Value_Parameter = Expression.Parameter(typeof(object), "value");

            Debug.Assert(entityWrapperField != null, "entityWrapperField does not exist");   

            // Create the Wrapper Getter
            Expression<Func<object, object>> lambda = Expression.Lambda<Func<object, object>>(
                    Expression.Field(
                        Expression.Convert(Object_Parameter, entityWrapperField.DeclaringType), entityWrapperField),
                        Object_Parameter);
            Func<object, object> getEntityWrapperDelegate = lambda.Compile();
            Proxy_GetEntityWrapper = (object proxy) =>
            {
                // This code validates that the wrapper points to the proxy that holds the wrapper.
                // This guards against mischief by switching this wrapper out for another one obtained
                // from a different object.
                IEntityWrapper wrapper = ((IEntityWrapper)getEntityWrapperDelegate(proxy));
                if (wrapper != null && !object.ReferenceEquals(wrapper.Entity, proxy))
                {
                    throw new InvalidOperationException(System.Data.Entity.Strings.EntityProxyTypeInfo_ProxyHasWrongWrapper);
                }
                return wrapper;
            };

            // Create the Wrapper setter
            Proxy_SetEntityWrapper = Expression.Lambda<Func<object, object, object>>(
                    Expression.Assign(
                        Expression.Field(
                            Expression.Convert(Object_Parameter, entityWrapperField.DeclaringType),
                            entityWrapperField),
                        Value_Parameter),
                    Object_Parameter, Value_Parameter).Compile();


            ParameterExpression PropertyName_Parameter = Expression.Parameter(typeof(string), "propertyName");
            MethodInfo baseGetterMethod = proxyType.GetMethod("GetBasePropertyValue", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);
            if (baseGetterMethod != null)
            {
                _baseGetter = Expression.Lambda<Func<object, string, object>>(
                    Expression.Call(Expression.Convert(Object_Parameter, proxyType), baseGetterMethod, PropertyName_Parameter),
                    Object_Parameter, PropertyName_Parameter).Compile();
            }

            ParameterExpression PropertyValue_Parameter = Expression.Parameter(typeof(object), "propertyName");
            MethodInfo baseSetterMethod = proxyType.GetMethod("SetBasePropertyValue", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object) }, null);
            if (baseSetterMethod != null)
            {
                _baseSetter = Expression.Lambda<Action<object, string, object>>(
                        Expression.Call(Expression.Convert(Object_Parameter, proxyType), baseSetterMethod, PropertyName_Parameter, PropertyValue_Parameter),
                        Object_Parameter, PropertyName_Parameter, PropertyValue_Parameter).Compile();
            }

            _propertiesWithBaseGetter = new HashSet<string>(baseGetters.Select(p => p.Name));
            _propertiesWithBaseSetter = new HashSet<string>(baseSetters.Select(p => p.Name));

            _createObject = LightweightCodeGenerator.CreateConstructor(proxyType) as Func<object>;
        }

        internal object CreateProxyObject()
        {
            return _createObject();
        }

        internal Type ProxyType
        {
            get { return _proxyType; }
        }

        internal DynamicMethod InitializeEntityCollections
        {
            get { return _initializeCollections; }
        }

        public Func<object, string, object> BaseGetter
        {
            get { return _baseGetter; }
        }

        public bool ContainsBaseGetter(string propertyName)
        {
            return BaseGetter != null && _propertiesWithBaseGetter.Contains(propertyName);
        }

        public bool ContainsBaseSetter(string propertyName)
        {
            return BaseSetter != null && _propertiesWithBaseSetter.Contains(propertyName);
        }

        public Action<object, string, object> BaseSetter
        {
            get { return _baseSetter; }
        }

        public bool TryGetNavigationPropertyAssociationType(string relationshipName, string targetRoleName, out AssociationType associationType)
        {
            return _navigationPropertyAssociationTypes.TryGetValue(new Tuple<string, string>(relationshipName, targetRoleName), out associationType);
        }

        public void ValidateType(ClrEntityType ospaceEntityType)
        {
            if (ospaceEntityType != _entityType && ospaceEntityType.HashedDescription != _entityType.HashedDescription)
            {
                Debug.Assert(ospaceEntityType.ClrType == _entityType.ClrType);
                throw EntityUtil.DuplicateTypeForProxyType(ospaceEntityType.ClrType);
            }
        }

        #region Wrapper on the Proxy

        /// <summary>
        /// Set the proxy object's private entity wrapper field value to the specified entity wrapper object.
        /// The proxy object (representing the wrapped entity) is retrieved from the wrapper itself.
        /// </summary>
        /// <param name="wrapper">Wrapper object to be referenced by the proxy.</param>
        /// <returns>
        /// The supplied entity wrapper.
        /// This is done so that this method can be more easily composed within lambda expressions (such as in the materializer).
        /// </returns>
        internal IEntityWrapper SetEntityWrapper(IEntityWrapper wrapper)
        {
            Debug.Assert(wrapper != null, "wrapper must be non-null");
            Debug.Assert(wrapper.Entity != null, "proxy must be non-null");
            return Proxy_SetEntityWrapper(wrapper.Entity, wrapper) as IEntityWrapper;
        }

        /// <summary>
        /// Gets the proxy object's entity wrapper field value
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal IEntityWrapper GetEntityWrapper(object entity)
        {
            return Proxy_GetEntityWrapper(entity) as IEntityWrapper;
        }

        internal Func<object, object> EntityWrapperDelegate
        {
            get { return Proxy_GetEntityWrapper; }
        }

        #endregion
    }
}
