//---------------------------------------------------------------------
// <copyright file="EntityProxyFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Objects.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Factory for creating proxy classes that can intercept calls to a class' members.
    /// </summary>
    internal class EntityProxyFactory
    {
        private const string ProxyTypeNameFormat = "System.Data.Entity.DynamicProxies.{0}_{1}";
        internal const string ResetFKSetterFlagFieldName = "_resetFKSetterFlag";
        internal const string CompareByteArraysFieldName = "_compareByteArrays";

        /// <summary>
        /// A hook such that test code can change the AssemblyBuilderAccess of the
        /// proxy assembly through reflection into the EntityProxyFactory.
        /// </summary>
        private static AssemblyBuilderAccess s_ProxyAssemblyBuilderAccess = AssemblyBuilderAccess.Run;
        /// <summary>
        /// Dictionary of proxy class type information, keyed by the pair of the CLR type and EntityType CSpaceName of the type being proxied.
        /// A null value for a particular EntityType name key records the fact that 
        /// no proxy Type could be created for the specified type.
        /// </summary>
        private static Dictionary<Tuple<Type, string>, EntityProxyTypeInfo> s_ProxyNameMap = new Dictionary<Tuple<Type, string>, EntityProxyTypeInfo>();
        /// <summary>
        /// Dictionary of proxy class type information, keyed by the proxy type
        /// </summary>
        private static Dictionary<Type, EntityProxyTypeInfo> s_ProxyTypeMap = new Dictionary<Type, EntityProxyTypeInfo>();
        private static Dictionary<Assembly, ModuleBuilder> s_ModuleBuilders = new Dictionary<Assembly, ModuleBuilder>();
        private static ReaderWriterLockSlim s_TypeMapLock = new ReaderWriterLockSlim();

        /// <summary>
        /// The runtime assembly of the proxy types.
        /// This is not the same as the AssemblyBuilder used to create proxy types.
        /// </summary>
        private static HashSet<Assembly> ProxyRuntimeAssemblies = new HashSet<Assembly>();

        private static ModuleBuilder GetDynamicModule(EntityType ospaceEntityType)
        {
            Assembly assembly = ospaceEntityType.ClrType.Assembly;
            ModuleBuilder moduleBuilder;
            if (!s_ModuleBuilders.TryGetValue(assembly, out moduleBuilder))
            {
                AssemblyName assemblyName = new AssemblyName(String.Format(CultureInfo.InvariantCulture, "EntityFrameworkDynamicProxies-{0}", assembly.FullName));
                assemblyName.Version = new Version(1, 0, 0, 0);
                
                // Mark assembly as security transparent, meaning it cannot cause an elevation of privilege.
                // This also means the assembly cannot satisfy a link demand. Instead link demands become full demands.
                ConstructorInfo securityTransparentAttributeConstructor = typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes);

                // Mark assembly with [SecurityRules(SecurityRuleSet.Level1)]. In memory, the assembly will inherit
                // this automatically from SDE, but when persisted it needs this attribute to be considered Level1.
                ConstructorInfo securityRulesAttributeConstructor = typeof(SecurityRulesAttribute).GetConstructor(new Type[] { typeof(SecurityRuleSet) });

                CustomAttributeBuilder[] attributeBuilders = new CustomAttributeBuilder[] { 
                    new CustomAttributeBuilder(securityTransparentAttributeConstructor, new object[0]),
                    new CustomAttributeBuilder(securityRulesAttributeConstructor, new object[1] { SecurityRuleSet.Level1 })
                };

                AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, s_ProxyAssemblyBuilderAccess, attributeBuilders);

                if (s_ProxyAssemblyBuilderAccess == AssemblyBuilderAccess.RunAndSave)
                {
                    // Make the module persistable if the AssemblyBuilderAccess is changed to be RunAndSave.
                    moduleBuilder = assemblyBuilder.DefineDynamicModule("EntityProxyModule", "EntityProxyModule.dll");
                }
                else
                {
                    moduleBuilder = assemblyBuilder.DefineDynamicModule("EntityProxyModule");
                }

                s_ModuleBuilders.Add(assembly, moduleBuilder);
            }
            return moduleBuilder;
        }

        internal static bool TryGetProxyType(Type clrType, string entityTypeName, out EntityProxyTypeInfo proxyTypeInfo)
        {
            s_TypeMapLock.EnterReadLock();
            try
            {
                return s_ProxyNameMap.TryGetValue(new Tuple<Type, string>(clrType, entityTypeName), out proxyTypeInfo);
            }
            finally
            {
                s_TypeMapLock.ExitReadLock();
            }
        }

        internal static bool TryGetProxyType(Type proxyType, out EntityProxyTypeInfo proxyTypeInfo)
        {
            s_TypeMapLock.EnterReadLock();
            try
            {
                return s_ProxyTypeMap.TryGetValue(proxyType, out proxyTypeInfo);
            }
            finally
            {
                s_TypeMapLock.ExitReadLock();
            }
        }

        internal static bool TryGetProxyWrapper(object instance, out IEntityWrapper wrapper)
        {
            Debug.Assert(instance != null, "the instance should not be null");
            wrapper = null;
            EntityProxyTypeInfo proxyTypeInfo;
            if (IsProxyType(instance.GetType()) &&
                TryGetProxyType(instance.GetType(), out proxyTypeInfo))
            {
                wrapper = proxyTypeInfo.GetEntityWrapper(instance);
            }
            return wrapper != null;
        }

        /// <summary>
        /// Return proxy type information for the specified O-Space EntityType.
        /// </summary>
        /// <param name="ospaceEntityType">
        /// EntityType in O-Space that represents the CLR type to be proxied.
        /// Must not be null.
        /// </param>
        /// <returns>
        /// A non-null EntityProxyTypeInfo instance that contains information about the type of proxy for
        /// the specified O-Space EntityType; or null if no proxy can be created for the specified type.
        /// </returns>
        internal static EntityProxyTypeInfo GetProxyType(ClrEntityType ospaceEntityType)
        {
            Debug.Assert(ospaceEntityType != null, "ospaceEntityType must be non-null");
            Debug.Assert(ospaceEntityType.DataSpace == DataSpace.OSpace, "ospaceEntityType.DataSpace must be OSpace");

            EntityProxyTypeInfo proxyTypeInfo = null;

            // Check if an entry for the proxy type already exists.
            if (TryGetProxyType(ospaceEntityType.ClrType, ospaceEntityType.CSpaceTypeName, out proxyTypeInfo))
            {
                if (proxyTypeInfo != null)
                {
                    proxyTypeInfo.ValidateType(ospaceEntityType);
                }
                return proxyTypeInfo;
            }

            // No entry found, may need to create one.
            // Acquire an upgradeable read lock so that:
            // 1. Other readers aren't blocked while the second existence check is performed.
            // 2. Other threads that may have also detected the absence of an entry block while the first thread handles proxy type creation.

            s_TypeMapLock.EnterUpgradeableReadLock();
            try
            {
                return TryCreateProxyType(ospaceEntityType);
            }
            finally
            {
                s_TypeMapLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// A mechanism to lookup AssociationType metadata for proxies for a given entity and association information
        /// </summary>
        /// <param name="wrappedEntity">The entity instance used to lookup the proxy type</param>
        /// <param name="relationshipName">The name of the relationship (FullName or Name)</param>
        /// <param name="targetRoleName">Target role of the relationship</param>
        /// <param name="associationType">The AssociationType for that property</param>
        /// <returns>True if an AssociationType is found in proxy metadata, false otherwise</returns>
        internal static bool TryGetAssociationTypeFromProxyInfo(IEntityWrapper wrappedEntity, string relationshipName, string targetRoleName, out AssociationType associationType)
        {
            EntityProxyTypeInfo proxyInfo = null;
            associationType = null;
            return (EntityProxyFactory.TryGetProxyType(wrappedEntity.Entity.GetType(), out proxyInfo) && proxyInfo != null &&
                    proxyInfo.TryGetNavigationPropertyAssociationType(relationshipName, targetRoleName, out associationType));
        }

        /// <summary>
        /// Enumerate list of supplied O-Space EntityTypes, 
        /// and generate a proxy type for each EntityType (if possible for the particular type).
        /// </summary>
        /// <param name="ospaceEntityType">
        /// Enumeration of O-Space EntityType objects.
        /// Must not be null.
        /// In addition, the elements of the enumeration must not be null.
        /// </param>
        internal static void TryCreateProxyTypes(IEnumerable<EntityType> ospaceEntityTypes)
        {
            Debug.Assert(ospaceEntityTypes != null, "ospaceEntityTypes must be non-null");

            // Acquire an upgradeable read lock for the duration of the enumeration so that:
            // 1. Other readers aren't blocked while existence checks are performed.
            // 2. Other threads that may have detected the absence of an entry block while the first thread handles proxy type creation.

            s_TypeMapLock.EnterUpgradeableReadLock();
            try
            {
                foreach (EntityType ospaceEntityType in ospaceEntityTypes)
                {
                    Debug.Assert(ospaceEntityType != null, "Null EntityType element reference present in enumeration.");
                    TryCreateProxyType(ospaceEntityType);
                }
            }
            finally
            {
                s_TypeMapLock.ExitUpgradeableReadLock();
            }
        }

        private static EntityProxyTypeInfo TryCreateProxyType(EntityType ospaceEntityType)
        {
            Debug.Assert(s_TypeMapLock.IsUpgradeableReadLockHeld, "EntityProxyTypeInfo.TryCreateProxyType method was called without first acquiring an upgradeable read lock from s_TypeMapLock.");

            EntityProxyTypeInfo proxyTypeInfo;
            ClrEntityType clrEntityType = (ClrEntityType)ospaceEntityType;

            Tuple<Type, string> proxyIdentiy = new Tuple<Type, string>(clrEntityType.ClrType, clrEntityType.HashedDescription);

            if (!s_ProxyNameMap.TryGetValue(proxyIdentiy, out proxyTypeInfo) && CanProxyType(ospaceEntityType))
            {
                ModuleBuilder moduleBuilder = GetDynamicModule(ospaceEntityType);
                proxyTypeInfo = BuildType(moduleBuilder, clrEntityType);

                s_TypeMapLock.EnterWriteLock();
                try
                {
                    s_ProxyNameMap[proxyIdentiy] = proxyTypeInfo;
                    if (proxyTypeInfo != null)
                    {
                        // If there is a proxy type, create the reverse lookup
                        s_ProxyTypeMap[proxyTypeInfo.ProxyType] = proxyTypeInfo;
                    }
                }
                finally
                {
                    s_TypeMapLock.ExitWriteLock();
                }
            }

            return proxyTypeInfo;
        }

        /// <summary>
        /// Determine if the specified type represents a known proxy type.
        /// </summary>
        /// <param name="type">
        /// The Type to be examined.
        /// </param>
        /// <returns>
        /// True if the type is a known proxy type; otherwise false.
        /// </returns>
        internal static bool IsProxyType(Type type)
        {
            Debug.Assert(type != null, "type is null, was this intended?");
            return type != null && ProxyRuntimeAssemblies.Contains(type.Assembly);
        }

        /// <summary>
        /// Return an enumerable of the current set of CLR proxy types.
        /// </summary>
        /// <returns>
        /// Enumerable of the current set of CLR proxy types.
        /// This value will never be null.
        /// </returns>
        /// <remarks>
        /// The enumerable is based on a shapshot of the current list of types.
        /// </remarks>
        internal static IEnumerable<Type> GetKnownProxyTypes()
        {
            s_TypeMapLock.EnterReadLock();
            try
            {
                var proxyTypes = from info in s_ProxyNameMap.Values
                                 where info != null
                                 select info.ProxyType;
                return proxyTypes.ToArray();
            }
            finally
            {
                s_TypeMapLock.ExitReadLock();
            }
        }

        public Func<object, object> CreateBaseGetter(Type declaringType, PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo != null, "Null propertyInfo");

            ParameterExpression Object_Parameter = Expression.Parameter(typeof(object), "instance");
            Func<object, object> nonProxyGetter = Expression.Lambda<Func<object, object>>(
                                    Expression.PropertyOrField(
                                        Expression.Convert(Object_Parameter, declaringType),
                                        propertyInfo.Name),
                                    Object_Parameter).Compile();

            string propertyName = propertyInfo.Name;
            return (entity) =>
            {
                Type type = entity.GetType();
                if (IsProxyType(type))
                {
                    object value;
                    if (TryGetBasePropertyValue(type, propertyName, entity, out value))
                    {
                        return value;
                    }
                }
                return nonProxyGetter(entity);
            };
        }

        private static bool TryGetBasePropertyValue(Type proxyType, string propertyName, object entity, out object value)
        {
            EntityProxyTypeInfo typeInfo;
            value = null;
            if (TryGetProxyType(proxyType, out typeInfo) && typeInfo.ContainsBaseGetter(propertyName))
            {
                value = typeInfo.BaseGetter(entity, propertyName);
                return true;
            }
            return false;
        }

        public Action<object, object> CreateBaseSetter(Type declaringType, PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo != null, "Null propertyInfo");

            Action<object, object> nonProxySetter = LightweightCodeGenerator.CreateNavigationPropertySetter(declaringType, propertyInfo);

            string propertyName = propertyInfo.Name;
            return (entity, value) =>
            {
                Type type = entity.GetType();
                if (IsProxyType(type))
                {
                    if (TrySetBasePropertyValue(type, propertyName, entity, value))
                    {
                        return;
                    }
                }
                nonProxySetter(entity, value);
            };
        }

        private static bool TrySetBasePropertyValue(Type proxyType, string propertyName, object entity, object value)
        {
            EntityProxyTypeInfo typeInfo;
            if (TryGetProxyType(proxyType, out typeInfo) && typeInfo.ContainsBaseSetter(propertyName))
            {
                typeInfo.BaseSetter(entity, propertyName, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Build a CLR proxy type for the supplied EntityType.
        /// </summary>
        /// <param name="ospaceEntityType">
        /// EntityType in O-Space that represents the CLR type to be proxied.
        /// </param>
        /// <returns>
        /// EntityProxyTypeInfo object that contains the constructed proxy type,
        /// along with any behaviors associated with that type;
        /// or null if a proxy type cannot be constructed for the specified EntityType.
        /// </returns>
        private static EntityProxyTypeInfo BuildType(ModuleBuilder moduleBuilder, ClrEntityType ospaceEntityType)
        {
            Debug.Assert(s_TypeMapLock.IsUpgradeableReadLockHeld, "EntityProxyTypeInfo.BuildType method was called without first acquiring an upgradeable read lock from s_TypeMapLock.");

            EntityProxyTypeInfo proxyTypeInfo;

            ProxyTypeBuilder proxyTypeBuilder = new ProxyTypeBuilder(ospaceEntityType);
            Type proxyType = proxyTypeBuilder.CreateType(moduleBuilder);

            if (proxyType != null)
            {
                // Set the runtime assembly of the proxy types if it hasn't already been set.
                // This is used by the IsProxyType method.
                Assembly typeAssembly = proxyType.Assembly;
                if (!ProxyRuntimeAssemblies.Contains(typeAssembly))
                {
                    ProxyRuntimeAssemblies.Add(typeAssembly);
                    AddAssemblyToResolveList(typeAssembly);
                }

                proxyTypeInfo = new EntityProxyTypeInfo(proxyType, ospaceEntityType,
                    proxyTypeBuilder.CreateInitalizeCollectionMethod(proxyType),
                    proxyTypeBuilder.BaseGetters, proxyTypeBuilder.BaseSetters);

                foreach (EdmMember member in proxyTypeBuilder.LazyLoadMembers)
                {
                    InterceptMember(member, proxyType, proxyTypeInfo);
                }

                SetResetFKSetterFlagDelegate(proxyType, proxyTypeInfo);
                SetCompareByteArraysDelegate(proxyType, proxyTypeInfo);
            }
            else
            {
                proxyTypeInfo = null;
            }

            return proxyTypeInfo;
        }

        /// <summary>
        /// In order for deserialization of proxy objects to succeed in this AppDomain,
        /// an assembly resolve handler must be added to the AppDomain to resolve the dynamic assembly,
        /// since it is not present in a location discoverable by fusion.
        /// </summary>
        /// <param name="assembly">Proxy assembly to be resolved.</param>
        [SecuritySafeCritical]
        private static void AddAssemblyToResolveList(Assembly assembly)
        {
            if (ProxyRuntimeAssemblies.Contains(assembly)) // If the assembly is not a known proxy assembly, ignore it.
            {
                ResolveEventHandler resolveHandler = new ResolveEventHandler((sender, args) => args.Name == assembly.FullName ? assembly : null);
                AppDomain.CurrentDomain.AssemblyResolve += resolveHandler;
            }
        }

        /// <summary>
        /// Construct an interception delegate for the specified proxy member.
        /// </summary>
        /// <param name="member">
        /// EdmMember that specifies the member to be intercepted.
        /// </param>
        /// <param name="proxyType">
        /// Type of the proxy.
        /// </param>
        /// <param name="lazyLoadBehavior">
        /// LazyLoadBehavior object that supplies the behavior to load related ends.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void InterceptMember(EdmMember member, Type proxyType, EntityProxyTypeInfo proxyTypeInfo)
        {
            PropertyInfo property = EntityUtil.GetTopProperty(proxyType, member.Name);
            Debug.Assert(property != null, String.Format(CultureInfo.CurrentCulture, "Expected property {0} to be defined on proxy type {1}", member.Name, proxyType.FullName));

            FieldInfo interceptorField = proxyType.GetField(LazyLoadImplementor.GetInterceptorFieldName(member.Name), BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Assert(interceptorField != null, String.Format(CultureInfo.CurrentCulture, "Expected interceptor field for property {0} to be defined on proxy type {1}", member.Name, proxyType.FullName));

            Delegate interceptorDelegate = typeof(LazyLoadBehavior).GetMethod("GetInterceptorDelegate", BindingFlags.NonPublic | BindingFlags.Static).
                MakeGenericMethod(proxyType, property.PropertyType).
                Invoke(null, new object[] { member, proxyTypeInfo.EntityWrapperDelegate }) as Delegate;

            AssignInterceptionDelegate(interceptorDelegate, interceptorField);
        }

        /// <summary>
        /// Set the interceptor on a proxy member.
        /// </summary>
        /// <param name="interceptorDelegate">
        /// Delegate to be set
        /// </param>
        /// <param name="interceptorField">
        /// Field define on the proxy type to store the reference to the interception delegate.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2128")]
        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void AssignInterceptionDelegate(Delegate interceptorDelegate, FieldInfo interceptorField)
        {
            interceptorField.SetValue(null, interceptorDelegate);
        }

        /// <summary>
        /// Sets a delegate onto the _resetFKSetterFlag field such that it can be executed to make
        /// a call into the state manager to reset the InFKSetter flag.
        /// </summary>
        private static void SetResetFKSetterFlagDelegate(Type proxyType, EntityProxyTypeInfo proxyTypeInfo)
        {
            var resetFKSetterFlagField = proxyType.GetField(ResetFKSetterFlagFieldName, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Assert(resetFKSetterFlagField != null, "Expected resetFKSetterFlagField to be defined on the proxy type.");

            var resetFKSetterFlagDelegate = GetResetFKSetterFlagDelegate(proxyTypeInfo.EntityWrapperDelegate);

            AssignInterceptionDelegate(resetFKSetterFlagDelegate, resetFKSetterFlagField);
        }

        /// <summary>
        /// Returns the delegate that takes a proxy instance and uses it to reset the InFKSetter flag maintained
        /// by the state manager of the context associated with the proxy instance.
        /// </summary>
        private static Action<object> GetResetFKSetterFlagDelegate(Func<object, object> getEntityWrapperDelegate)
        {
            return (proxy) =>
            {
                Debug.Assert(getEntityWrapperDelegate != null, "entityWrapperDelegate must not be null");
                
                ResetFKSetterFlag(getEntityWrapperDelegate(proxy));
            };
        }

        /// <summary>
        /// Called in the finally clause of each overridden property setter to ensure that the flag
        /// indicating that we are in an FK setter is cleared.  Note that the wrapped entity is passed as
        /// an obejct becayse IEntityWrapper is an internal type and is therefore not accessable to
        /// the proxy type.  Once we're in the framework it is cast back to an IEntityWrapper.
        /// </summary>
        private static void ResetFKSetterFlag(object wrappedEntityAsObject)
        {
            Debug.Assert(wrappedEntityAsObject == null || wrappedEntityAsObject is IEntityWrapper, "wrappedEntityAsObject must be an IEntityWrapper");
            var wrappedEntity = (IEntityWrapper)wrappedEntityAsObject; // We want an exception if the cast fails.
            if (wrappedEntity != null && wrappedEntity.Context != null)
            {
                wrappedEntity.Context.ObjectStateManager.EntityInvokingFKSetter = null;
            }
        }

        /// <summary>
        /// Sets a delegate onto the _compareByteArrays field such that it can be executed to check
        /// whether two byte arrays are the same by value comparison.
        /// </summary>
        private static void SetCompareByteArraysDelegate(Type proxyType, EntityProxyTypeInfo proxyTypeInfo)
        {
            var compareByteArraysField = proxyType.GetField(CompareByteArraysFieldName, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Assert(compareByteArraysField != null, "Expected compareByteArraysField to be defined on the proxy type.");

            AssignInterceptionDelegate(new Func<object, object, bool>(ByValueEqualityComparer.Default.Equals), compareByteArraysField);
        }

        /// <summary>
        /// Return boolean that specifies if the specified type can be proxied.
        /// </summary>
        /// <param name="ospaceEntityType">O-space EntityType</param>
        /// <returns>
        /// True if the class is not abstract or sealed, does not implement IEntityWithRelationships,
        /// and has a public or protected default constructor; otherwise false.
        /// </returns>
        /// <remarks>
        /// While it is technically possible to derive from an abstract type
        /// in order to create a proxy, we avoid this so that the proxy type 
        /// has the same "concreteness" of the type being proxied.
        /// The check for IEntityWithRelationships ensures that codegen'ed
        /// entities that derive from EntityObject as well as properly
        /// constructed IPOCO entities will not be proxied.
        /// 
        /// </remarks>
        private static bool CanProxyType(EntityType ospaceEntityType)
        {
            TypeAttributes access = ospaceEntityType.ClrType.Attributes & TypeAttributes.VisibilityMask;

            ConstructorInfo ctor = ospaceEntityType.ClrType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, Type.EmptyTypes, null);
            bool accessableCtor = ctor != null && (((ctor.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) ||
                                                   ((ctor.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family) ||
                                                   ((ctor.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem));

            return (!(ospaceEntityType.Abstract ||
                     ospaceEntityType.ClrType.IsSealed ||
                     typeof(IEntityWithRelationships).IsAssignableFrom(ospaceEntityType.ClrType) ||
                     !accessableCtor) &&
                     access == TypeAttributes.Public);
        }

        private static bool CanProxyMethod(MethodInfo method)
        {
            bool result = false;

            if (method != null)
            {
                MethodAttributes access = method.Attributes & MethodAttributes.MemberAccessMask;
                result = method.IsVirtual &&
                         !method.IsFinal &&
                         (access == MethodAttributes.Public || 
                          access == MethodAttributes.Family || 
                          access == MethodAttributes.FamORAssem);
            }

            return result;
        }

        internal static bool CanProxyGetter(PropertyInfo clrProperty)
        {
            Debug.Assert(clrProperty != null, "clrProperty should have a value");
            return CanProxyMethod(clrProperty.GetGetMethod(true));
        }

        internal static bool CanProxySetter(PropertyInfo clrProperty)
        {
            Debug.Assert(clrProperty != null, "clrProperty should have a value");
            return CanProxyMethod(clrProperty.GetSetMethod(true));
        }

        private class ProxyTypeBuilder
        {
            private TypeBuilder _typeBuilder;
            private BaseProxyImplementor _baseImplementor;
            private IPOCOImplementor _ipocoImplementor;
            private LazyLoadImplementor _lazyLoadImplementor;
            private DataContractImplementor _dataContractImplementor;
            private ISerializableImplementor _iserializableImplementor;
            private ClrEntityType _ospaceEntityType;
            private ModuleBuilder _moduleBuilder;
            private List<FieldBuilder> _serializedFields = new List<FieldBuilder>(3);

            public ProxyTypeBuilder(ClrEntityType ospaceEntityType)
            {
                _ospaceEntityType = ospaceEntityType;
                _baseImplementor = new BaseProxyImplementor();
                _ipocoImplementor = new IPOCOImplementor(ospaceEntityType);
                _lazyLoadImplementor = new LazyLoadImplementor(ospaceEntityType);
                _dataContractImplementor = new DataContractImplementor(ospaceEntityType);
                _iserializableImplementor = new ISerializableImplementor(ospaceEntityType);
            }

            public Type BaseType
            {
                get { return _ospaceEntityType.ClrType; }
            }

            public DynamicMethod CreateInitalizeCollectionMethod(Type proxyType)
            {
                return _ipocoImplementor.CreateInitalizeCollectionMethod(proxyType);
            }

            public List<PropertyInfo> BaseGetters
            {
                get
                {
                    return _baseImplementor.BaseGetters;
                }
            }

            public List<PropertyInfo> BaseSetters
            {
                get
                {
                    return _baseImplementor.BaseSetters;
                }
            }

            public IEnumerable<EdmMember> LazyLoadMembers
            {
                get { return _lazyLoadImplementor.Members; }
            }

            public Type CreateType(ModuleBuilder moduleBuilder)
            {
                _moduleBuilder = moduleBuilder;
                bool hadProxyProperties = false;

                if (_iserializableImplementor.TypeIsSuitable)
                {
                    foreach (EdmMember member in _ospaceEntityType.Members)
                    {
                        if (_ipocoImplementor.CanProxyMember(member) ||
                            _lazyLoadImplementor.CanProxyMember(member))
                        {
                            PropertyInfo baseProperty = EntityUtil.GetTopProperty(BaseType, member.Name);
                            PropertyBuilder propertyBuilder = TypeBuilder.DefineProperty(member.Name, System.Reflection.PropertyAttributes.None, baseProperty.PropertyType, Type.EmptyTypes);

                            if (!_ipocoImplementor.EmitMember(TypeBuilder, member, propertyBuilder, baseProperty, _baseImplementor))
                            {
                                EmitBaseSetter(TypeBuilder, propertyBuilder, baseProperty);
                            }
                            if (!_lazyLoadImplementor.EmitMember(TypeBuilder, member, propertyBuilder, baseProperty, _baseImplementor))
                            {
                                EmitBaseGetter(TypeBuilder, propertyBuilder, baseProperty);
                            }

                            hadProxyProperties = true;
                        }
                    }

                    if (_typeBuilder != null)
                    {
                        _baseImplementor.Implement(TypeBuilder, RegisterInstanceField);
                        _iserializableImplementor.Implement(TypeBuilder, _serializedFields);
                    }
                }

                return hadProxyProperties ? TypeBuilder.CreateType() : null;
            }

            private TypeBuilder TypeBuilder
            {
                get
                {
                    if (_typeBuilder == null)
                    {
                        TypeAttributes proxyTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
                        if ((BaseType.Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable)
                        {
                            proxyTypeAttributes |= TypeAttributes.Serializable;
                        }

                        // If the type as a long name, then use only the first part of it so that there is no chance that the generated
                        // name will be too long.  Note that the full name always gets used to compute the hash.
                        string baseName = BaseType.Name.Length <= 20 ? BaseType.Name : BaseType.Name.Substring(0, 20);
                        string proxyTypeName = String.Format(CultureInfo.InvariantCulture, ProxyTypeNameFormat, baseName, _ospaceEntityType.HashedDescription);

                        _typeBuilder = _moduleBuilder.DefineType(proxyTypeName, proxyTypeAttributes, BaseType, _ipocoImplementor.Interfaces);
                        _typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName);

                        Action<FieldBuilder, bool> registerField = RegisterInstanceField;
                        _ipocoImplementor.Implement(_typeBuilder, registerField);
                        _lazyLoadImplementor.Implement(_typeBuilder, registerField);

                        // WCF data contract serialization is not compatible with types that implement ISerializable.
                        if (!_iserializableImplementor.TypeImplementsISerializable)
                        {
                            _dataContractImplementor.Implement(_typeBuilder, registerField);
                        }
                    }
                    return _typeBuilder;
                }
            }

            private void EmitBaseGetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty)
            {
                if (CanProxyGetter(baseProperty))
                {
                    MethodInfo baseGetter = baseProperty.GetGetMethod(true);
                    const MethodAttributes getterAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
                    MethodAttributes getterAccess = baseGetter.Attributes & MethodAttributes.MemberAccessMask;

                    // Define a property getter override in the proxy type
                    MethodBuilder getterBuilder = typeBuilder.DefineMethod("get_" + baseProperty.Name, getterAccess | getterAttributes, baseProperty.PropertyType, Type.EmptyTypes);
                    ILGenerator gen = getterBuilder.GetILGenerator();

                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Call, baseGetter);
                    gen.Emit(OpCodes.Ret);

                    propertyBuilder.SetGetMethod(getterBuilder);
                }
            }

            private void EmitBaseSetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty)
            {
                if (CanProxySetter(baseProperty))
                {

                    MethodInfo baseSetter = baseProperty.GetSetMethod(true); ;
                    const MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
                    MethodAttributes methodAccess = baseSetter.Attributes & MethodAttributes.MemberAccessMask;

                    MethodBuilder setterBuilder = typeBuilder.DefineMethod("set_" + baseProperty.Name, methodAccess | methodAttributes, null, new Type[] { baseProperty.PropertyType });
                    ILGenerator generator = setterBuilder.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Call, baseSetter);
                    generator.Emit(OpCodes.Ret);
                    propertyBuilder.SetSetMethod(setterBuilder);
                }
            }

            private void RegisterInstanceField(FieldBuilder field, bool serializable)
            {
                if (serializable)
                {
                    _serializedFields.Add(field);
                }
                else
                {
                    MarkAsNotSerializable(field); 
                }
            }

            private static readonly ConstructorInfo s_NonSerializedAttributeConstructor = typeof(NonSerializedAttribute).GetConstructor(Type.EmptyTypes);
            private static readonly ConstructorInfo s_IgnoreDataMemberAttributeConstructor = typeof(IgnoreDataMemberAttribute).GetConstructor(Type.EmptyTypes);
            private static readonly ConstructorInfo s_XmlIgnoreAttributeConstructor = typeof(System.Xml.Serialization.XmlIgnoreAttribute).GetConstructor(Type.EmptyTypes);
            private static readonly ConstructorInfo s_ScriptIgnoreAttributeConstructor = TryGetScriptIgnoreAttributeType().GetConstructor(Type.EmptyTypes);

            private static Type TryGetScriptIgnoreAttributeType()
            {
                try
                {
                    var scriptIgnoreAttributeAssembly = Assembly.Load(AssemblyRef.SystemWebExtensions);
                    return scriptIgnoreAttributeAssembly.GetType(@"System.Web.Script.Serialization.ScriptIgnoreAttribute");
                }
                catch 
                {
                }
                // We should not assert in EF6, at least when produce a build compatible with .NET 4.0 client SKU
                Debug.Assert(false, "Unable to find ScriptIgnoreAttribute type");
                return null;
            }

            private static void MarkAsNotSerializable(FieldBuilder field)
            {
                object[] emptyArray = new object[0];

                field.SetCustomAttribute(new CustomAttributeBuilder(s_NonSerializedAttributeConstructor, emptyArray));

                if (field.IsPublic)
                {
                    field.SetCustomAttribute(new CustomAttributeBuilder(s_IgnoreDataMemberAttributeConstructor, emptyArray));
                    field.SetCustomAttribute(new CustomAttributeBuilder(s_XmlIgnoreAttributeConstructor, emptyArray));

                    if (s_ScriptIgnoreAttributeConstructor != null)
                    {
                        field.SetCustomAttribute(new CustomAttributeBuilder(s_ScriptIgnoreAttributeConstructor, emptyArray));
                    }
                }
            }
        }
    }

    internal class LazyLoadImplementor
    {
        HashSet<EdmMember> _members;

        public LazyLoadImplementor(EntityType ospaceEntityType)
        {
            CheckType(ospaceEntityType);
        }

        public IEnumerable<EdmMember> Members
        {
            get { return _members; }
        }

        private void CheckType(EntityType ospaceEntityType)
        {
            _members = new HashSet<EdmMember>();

            foreach (EdmMember member in ospaceEntityType.Members)
            {
                PropertyInfo clrProperty = EntityUtil.GetTopProperty(ospaceEntityType.ClrType, member.Name);
                if (clrProperty != null &&
                    EntityProxyFactory.CanProxyGetter(clrProperty) &&
                    LazyLoadBehavior.IsLazyLoadCandidate(ospaceEntityType, member))
                {
                    _members.Add(member);
                }
            }
        }

        public bool CanProxyMember(EdmMember member)
        {
            return _members.Contains(member);
        }

        public void Implement(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            // Add instance field to store IEntityWrapper instance
            // The field is typed as object, for two reasons:
            // 1. The practical one, IEntityWrapper is internal and not accessible from the dynamic assembly.
            // 2. We purposely want the wrapper field to be opaque on the proxy type.
            FieldBuilder wrapperField = typeBuilder.DefineField(EntityProxyTypeInfo.EntityWrapperFieldName, typeof(object), FieldAttributes.Public);
            registerField(wrapperField, false);
        }

        public bool EmitMember(TypeBuilder typeBuilder, EdmMember member, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, BaseProxyImplementor baseImplementor)
        {
            if (_members.Contains(member))
            {
                MethodInfo baseGetter = baseProperty.GetGetMethod(true);
                const MethodAttributes getterAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
                MethodAttributes getterAccess = baseGetter.Attributes & MethodAttributes.MemberAccessMask;

                // Define field to store interceptor Func
                // Signature of interceptor Func delegate is as follows:
                //
                //    bool intercept(ProxyType proxy, PropertyType propertyValue)
                //
                // where
                //     PropertyType is the type of the Property, such as ICollection<Customer>,
                //     ProxyType is the type of the proxy object,
                //     propertyValue is the value returned from the proxied type's property getter.

                Type interceptorType = typeof(Func<,,>).MakeGenericType(typeBuilder, baseProperty.PropertyType, typeof(bool));
                MethodInfo interceptorInvoke = TypeBuilder.GetMethod(interceptorType, typeof(Func<,,>).GetMethod("Invoke"));
                FieldBuilder interceptorField = typeBuilder.DefineField(GetInterceptorFieldName(baseProperty.Name), interceptorType, FieldAttributes.Private | FieldAttributes.Static);

                // Define a property getter override in the proxy type
                MethodBuilder getterBuilder = typeBuilder.DefineMethod("get_" + baseProperty.Name, getterAccess | getterAttributes, baseProperty.PropertyType, Type.EmptyTypes);
                ILGenerator generator = getterBuilder.GetILGenerator();

                // Emit instructions for the following call:
                //   T value = base.SomeProperty;
                //   if(this._interceptorForSomeProperty(this, value))
                //   {  return value; }
                //   return base.SomeProperty;
                // where _interceptorForSomeProperty represents the interceptor Func field.

                Label lableTrue = generator.DefineLabel();
                generator.DeclareLocal(baseProperty.PropertyType);       // T value
                generator.Emit(OpCodes.Ldarg_0);            // call base.SomeProperty
                generator.Emit(OpCodes.Call, baseGetter); // call to base property getter
                generator.Emit(OpCodes.Stloc_0);            // value = result
                generator.Emit(OpCodes.Ldarg_0);            // load this
                generator.Emit(OpCodes.Ldfld, interceptorField); // load this._interceptor
                generator.Emit(OpCodes.Ldarg_0);            // load this
                generator.Emit(OpCodes.Ldloc_0);            // load value
                generator.Emit(OpCodes.Callvirt, interceptorInvoke); // call to interceptor delegate with (this, value)
                generator.Emit(OpCodes.Brtrue_S, lableTrue); // if true, just return
                generator.Emit(OpCodes.Ldarg_0); // else, call the base propertty getter again
                generator.Emit(OpCodes.Call, baseGetter); // call to base property getter
                generator.Emit(OpCodes.Ret);
                generator.MarkLabel(lableTrue);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getterBuilder);

                baseImplementor.AddBasePropertyGetter(baseProperty);
                return true;
            }
            return false;
        }


        internal static string GetInterceptorFieldName(string memberName)
        {
            return "ef_proxy_interceptorFor" + memberName;
        }

    }

    internal class BaseProxyImplementor
    {
        private readonly List<PropertyInfo> _baseGetters;
        private readonly List<PropertyInfo> _baseSetters;

        public BaseProxyImplementor()
        {
            _baseGetters = new List<PropertyInfo>();
            _baseSetters = new List<PropertyInfo>();
        }

        public List<PropertyInfo> BaseGetters
        {
            get { return _baseGetters; }
        }

        public List<PropertyInfo> BaseSetters
        {
            get { return _baseSetters; }
        }
        public void AddBasePropertyGetter(PropertyInfo baseProperty)
        {
            _baseGetters.Add(baseProperty);
        }

        public void AddBasePropertySetter(PropertyInfo baseProperty)
        {
            _baseSetters.Add(baseProperty);
        }

        public void Implement(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            if (_baseGetters.Count > 0)
            {
                ImplementBaseGetter(typeBuilder);
            }
            if (_baseSetters.Count > 0)
            {
                ImplementBaseSetter(typeBuilder);
            }
        }

        static readonly MethodInfo s_StringEquals = typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) });
        static readonly ConstructorInfo s_InvalidOperationConstructor = typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes);

        private void ImplementBaseGetter(TypeBuilder typeBuilder)
        {
            // Define a property getter in the proxy type
            MethodBuilder getterBuilder = typeBuilder.DefineMethod("GetBasePropertyValue", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(object), new Type[] { typeof(string) });
            ILGenerator gen = getterBuilder.GetILGenerator();
            Label[] labels = new Label[_baseGetters.Count];

            for (int i = 0; i < _baseGetters.Count; i++)
            {
                labels[i] = gen.DefineLabel();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldstr, _baseGetters[i].Name);
                gen.Emit(OpCodes.Call, s_StringEquals);
                gen.Emit(OpCodes.Brfalse_S, labels[i]);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, _baseGetters[i].GetGetMethod(true));
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(labels[i]);
            }
            gen.Emit(OpCodes.Newobj, s_InvalidOperationConstructor);
            gen.Emit(OpCodes.Throw);
        }

        private void ImplementBaseSetter(TypeBuilder typeBuilder)
        {
            MethodBuilder setterBuilder = typeBuilder.DefineMethod("SetBasePropertyValue", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void), new Type[] { typeof(string), typeof(object) });
            ILGenerator gen = setterBuilder.GetILGenerator();

            Label[] labels = new Label[_baseSetters.Count];

            for (int i = 0; i < _baseSetters.Count; i++)
            {
                labels[i] = gen.DefineLabel();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldstr, _baseSetters[i].Name);
                gen.Emit(OpCodes.Call, s_StringEquals);
                gen.Emit(OpCodes.Brfalse_S, labels[i]);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_2);
                gen.Emit(OpCodes.Castclass, _baseSetters[i].PropertyType);
                gen.Emit(OpCodes.Call, _baseSetters[i].GetSetMethod(true));
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(labels[i]);
            }
            gen.Emit(OpCodes.Newobj, s_InvalidOperationConstructor);
            gen.Emit(OpCodes.Throw);
        }
    }

    internal class IPOCOImplementor
    {
        private EntityType _ospaceEntityType;

        FieldBuilder _changeTrackerField;
        FieldBuilder _relationshipManagerField;
        FieldBuilder _resetFKSetterFlagField;
        FieldBuilder _compareByteArraysField;

        MethodBuilder _entityMemberChanging;
        MethodBuilder _entityMemberChanged;
        MethodBuilder _getRelationshipManager;

        private List<KeyValuePair<NavigationProperty, PropertyInfo>> _referenceProperties;
        private List<KeyValuePair<NavigationProperty, PropertyInfo>> _collectionProperties;
        private bool _implementIEntityWithChangeTracker;
        private bool _implementIEntityWithRelationships;
        private HashSet<EdmMember> _scalarMembers;
        private HashSet<EdmMember> _relationshipMembers;
        
        static readonly MethodInfo s_EntityMemberChanging = typeof(IEntityChangeTracker).GetMethod("EntityMemberChanging", new Type[] { typeof(string) });
        static readonly MethodInfo s_EntityMemberChanged = typeof(IEntityChangeTracker).GetMethod("EntityMemberChanged", new Type[] { typeof(string) });
        static readonly MethodInfo s_CreateRelationshipManager = typeof(RelationshipManager).GetMethod("Create", new Type[] { typeof(IEntityWithRelationships) });
        static readonly MethodInfo s_GetRelationshipManager = typeof(IEntityWithRelationships).GetProperty("RelationshipManager").GetGetMethod();
        static readonly MethodInfo s_GetRelatedReference = typeof(RelationshipManager).GetMethod("GetRelatedReference", new Type[] { typeof(string), typeof(string) });
        static readonly MethodInfo s_GetRelatedCollection = typeof(RelationshipManager).GetMethod("GetRelatedCollection", new Type[] { typeof(string), typeof(string) });
        static readonly MethodInfo s_GetRelatedEnd = typeof(RelationshipManager).GetMethod("GetRelatedEnd", new Type[] { typeof(string), typeof(string) });
        static readonly MethodInfo s_ObjectEquals = typeof(object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) });
        static readonly ConstructorInfo s_InvalidOperationConstructor = typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) });
        static readonly MethodInfo s_IEntityWrapper_GetEntity = typeof(IEntityWrapper).GetProperty("Entity").GetGetMethod();
        static readonly MethodInfo s_Action_Invoke = typeof(Action<object>).GetMethod("Invoke", new Type[] { typeof(object) });
        static readonly MethodInfo s_Func_object_object_bool_Invoke = typeof(Func<object, object, bool>).GetMethod("Invoke", new Type[] { typeof(object), typeof(object) });

        private static readonly ConstructorInfo s_BrowsableAttributeConstructor = typeof(BrowsableAttribute).GetConstructor(new Type[] { typeof(bool) });

        public IPOCOImplementor(EntityType ospaceEntityType)
        {
            Type baseType = ospaceEntityType.ClrType;
            _referenceProperties = new List<KeyValuePair<NavigationProperty, PropertyInfo>>();
            _collectionProperties = new List<KeyValuePair<NavigationProperty, PropertyInfo>>();

            _implementIEntityWithChangeTracker = (null == baseType.GetInterface(typeof(IEntityWithChangeTracker).Name));
            _implementIEntityWithRelationships = (null == baseType.GetInterface(typeof(IEntityWithRelationships).Name));

            CheckType(ospaceEntityType);

            _ospaceEntityType = ospaceEntityType;
        }

        private void CheckType(EntityType ospaceEntityType)
        {
            _scalarMembers = new HashSet<EdmMember>();
            _relationshipMembers = new HashSet<EdmMember>();

            foreach (EdmMember member in ospaceEntityType.Members)
            {
                PropertyInfo clrProperty = EntityUtil.GetTopProperty(ospaceEntityType.ClrType, member.Name);
                if (clrProperty != null && EntityProxyFactory.CanProxySetter(clrProperty))
                {
                    if (member.BuiltInTypeKind == BuiltInTypeKind.EdmProperty)
                    {
                        if (_implementIEntityWithChangeTracker)
                        {
                            _scalarMembers.Add(member);
                        }
                    }
                    else if (member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
                    {
                        if (_implementIEntityWithRelationships)
                        {
                            NavigationProperty navProperty = (NavigationProperty)member;
                            RelationshipMultiplicity multiplicity = navProperty.ToEndMember.RelationshipMultiplicity;

                            if (multiplicity == RelationshipMultiplicity.Many)
                            {
                                if (clrProperty.PropertyType.IsGenericType &&
                                    clrProperty.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                                {
                                    _relationshipMembers.Add(member);
                                }
                            }
                            else
                            {
                                _relationshipMembers.Add(member);
                            }
                        }
                    }
                }
            }

            if (ospaceEntityType.Members.Count != _scalarMembers.Count + _relationshipMembers.Count)
            {
                _scalarMembers.Clear();
                _relationshipMembers.Clear();
                _implementIEntityWithChangeTracker = false;
                _implementIEntityWithRelationships = false;
            }
        }

        public void Implement(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            if (_implementIEntityWithChangeTracker)
            {
                ImplementIEntityWithChangeTracker(typeBuilder, registerField);
            }
            if (_implementIEntityWithRelationships)
            {
                ImplementIEntityWithRelationships(typeBuilder, registerField);
            }

            _resetFKSetterFlagField = typeBuilder.DefineField(EntityProxyFactory.ResetFKSetterFlagFieldName, typeof(Action<object>), FieldAttributes.Private| FieldAttributes.Static);
            _compareByteArraysField = typeBuilder.DefineField(EntityProxyFactory.CompareByteArraysFieldName, typeof(Func<object, object, bool>), FieldAttributes.Private | FieldAttributes.Static);
        }

        public Type[] Interfaces
        {
            get
            {
                List<Type> types = new List<Type>();
                if (_implementIEntityWithChangeTracker) { types.Add(typeof(IEntityWithChangeTracker)); }
                if (_implementIEntityWithRelationships) { types.Add(typeof(IEntityWithRelationships)); }
                return types.ToArray();
            }
        }

        public DynamicMethod CreateInitalizeCollectionMethod(Type proxyType)
        {
            if (_collectionProperties.Count > 0)
            {
                DynamicMethod initializeEntityCollections = LightweightCodeGenerator.CreateDynamicMethod(proxyType.Name + "_InitializeEntityCollections", typeof(IEntityWrapper), new Type[] { typeof(IEntityWrapper) });
                ILGenerator generator = initializeEntityCollections.GetILGenerator();
                generator.DeclareLocal(proxyType);
                generator.DeclareLocal(typeof(RelationshipManager));
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Callvirt, s_IEntityWrapper_GetEntity);
                generator.Emit(OpCodes.Castclass, proxyType);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Callvirt, s_GetRelationshipManager);
                generator.Emit(OpCodes.Stloc_1);

                foreach (KeyValuePair<NavigationProperty, PropertyInfo> navProperty in _collectionProperties)
                {
                    // Update Constructor to initialize this property
                    MethodInfo getRelatedCollection = s_GetRelatedCollection.MakeGenericMethod(EntityUtil.GetCollectionElementType(navProperty.Value.PropertyType));

                    generator.Emit(OpCodes.Ldloc_0);
                    generator.Emit(OpCodes.Ldloc_1);
                    generator.Emit(OpCodes.Ldstr, navProperty.Key.RelationshipType.FullName);
                    generator.Emit(OpCodes.Ldstr, navProperty.Key.ToEndMember.Name);
                    generator.Emit(OpCodes.Callvirt, getRelatedCollection);
                    generator.Emit(OpCodes.Callvirt, navProperty.Value.GetSetMethod(true));
                }
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ret);

                return initializeEntityCollections;
            }
            return null;
        }

        public bool CanProxyMember(EdmMember member)
        {
            return _scalarMembers.Contains(member) || _relationshipMembers.Contains(member);
        }

        public bool EmitMember(TypeBuilder typeBuilder, EdmMember member, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, BaseProxyImplementor baseImplementor)
        {
            if (_scalarMembers.Contains(member))
            {
                bool isKeyMember = _ospaceEntityType.KeyMembers.Contains(member.Identity);
                EmitScalarSetter(typeBuilder, propertyBuilder, baseProperty, isKeyMember);
                return true;
            }
            else if (_relationshipMembers.Contains(member))
            {
                Debug.Assert(member != null, "member is null");
                Debug.Assert(member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty);
                NavigationProperty navProperty = member as NavigationProperty;
                if (navProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                {
                    EmitCollectionProperty(typeBuilder, propertyBuilder, baseProperty, navProperty);
                }
                else
                {
                    EmitReferenceProperty(typeBuilder, propertyBuilder, baseProperty, navProperty);
                }
                baseImplementor.AddBasePropertySetter(baseProperty);
                return true;
            }
            return false;
        }

        private void EmitScalarSetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, bool isKeyMember)
        {
            MethodInfo baseSetter = baseProperty.GetSetMethod(true); 
            const MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            MethodAttributes methodAccess = baseSetter.Attributes & MethodAttributes.MemberAccessMask;

            MethodBuilder setterBuilder = typeBuilder.DefineMethod("set_" + baseProperty.Name, methodAccess | methodAttributes, null, new Type[] { baseProperty.PropertyType });
            ILGenerator generator = setterBuilder.GetILGenerator();
            Label endOfMethod = generator.DefineLabel();

            // If the CLR property represents a key member of the Entity Type,
            // ignore attempts to set the key value to the same value.
            if (isKeyMember)
            {
                MethodInfo baseGetter = baseProperty.GetGetMethod(true);

                if (baseGetter != null)
                {
                    // if (base.[Property] != value)
                    // { 
                    //     // perform set operation
                    // }
                    
                    Type propertyType = baseProperty.PropertyType;

                    if (propertyType == typeof(int) ||         // signed integer types
                        propertyType == typeof(short) ||
                        propertyType == typeof(Int64) ||
                        propertyType == typeof(bool) ||        // boolean
                        propertyType == typeof(byte) ||         
                        propertyType == typeof(UInt32) ||
                        propertyType == typeof(UInt64)||
                        propertyType == typeof(float) ||
                        propertyType == typeof(double) ||
                        propertyType.IsEnum)
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Call, baseGetter);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Beq_S, endOfMethod);
                    }
                    else if (propertyType == typeof(byte[]))
                    {
                        // Byte arrays must be compared by value
                        generator.Emit(OpCodes.Ldsfld, _compareByteArraysField);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Call, baseGetter);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Callvirt, s_Func_object_object_bool_Invoke);
                        generator.Emit(OpCodes.Brtrue_S, endOfMethod);
                    }
                    else
                    {
                        // Get the specific type's inequality method if it exists
                        MethodInfo op_inequality = propertyType.GetMethod("op_Inequality", new Type[] { propertyType, propertyType });
                        if (op_inequality != null)
                        {
                            generator.Emit(OpCodes.Ldarg_0);
                            generator.Emit(OpCodes.Call, baseGetter);
                            generator.Emit(OpCodes.Ldarg_1);
                            generator.Emit(OpCodes.Call, op_inequality);
                            generator.Emit(OpCodes.Brfalse_S, endOfMethod);
                        }
                        else
                        {
                            // Use object inequality
                            generator.Emit(OpCodes.Ldarg_0);
                            generator.Emit(OpCodes.Call, baseGetter);
                            if (propertyType.IsValueType)
                            {
                                generator.Emit(OpCodes.Box, propertyType);
                            }
                            generator.Emit(OpCodes.Ldarg_1);
                            if (propertyType.IsValueType)
                            {
                                generator.Emit(OpCodes.Box, propertyType);
                            }
                            generator.Emit(OpCodes.Call, s_ObjectEquals);
                            generator.Emit(OpCodes.Brtrue_S, endOfMethod);
                        }
                    }
                }
            }

            // Creates code like this:
            //
            // try
            // {
            //     MemberChanging(propertyName);
            //     base.Property_set(value);
            //     MemberChanged(propertyName);
            // }
            // finally
            // {
            //     _resetFKSetterFlagField(this);
            // }
            //
            // Note that the try/finally ensures that even if an exception causes
            // the setting of the property to be aborted, we still clear the flag that
            // indicates that we are in a property setter.

            generator.BeginExceptionBlock();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldstr, baseProperty.Name);
            generator.Emit(OpCodes.Call, _entityMemberChanging);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, baseSetter);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldstr, baseProperty.Name);
            generator.Emit(OpCodes.Call, _entityMemberChanged);
            generator.BeginFinallyBlock();
            generator.Emit(OpCodes.Ldsfld, _resetFKSetterFlagField);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, s_Action_Invoke);
            generator.EndExceptionBlock();
            generator.MarkLabel(endOfMethod);
            generator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setterBuilder);
        }

        private void EmitReferenceProperty(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, NavigationProperty navProperty)
        {
            const MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            MethodInfo baseSetter = baseProperty.GetSetMethod(true); ;
            MethodAttributes methodAccess = baseSetter.Attributes & MethodAttributes.MemberAccessMask;

            MethodInfo specificGetRelatedReference = s_GetRelatedReference.MakeGenericMethod(baseProperty.PropertyType);
            MethodInfo specificEntityReferenceSetValue = typeof(EntityReference<>).MakeGenericType(baseProperty.PropertyType).GetMethod("set_Value"); ;

            MethodBuilder setterBuilder = typeBuilder.DefineMethod("set_" + baseProperty.Name, methodAccess | methodAttributes, null, new Type[] { baseProperty.PropertyType });
            ILGenerator generator = setterBuilder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, _getRelationshipManager);
            generator.Emit(OpCodes.Ldstr, navProperty.RelationshipType.FullName);
            generator.Emit(OpCodes.Ldstr, navProperty.ToEndMember.Name);
            generator.Emit(OpCodes.Callvirt, specificGetRelatedReference);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, specificEntityReferenceSetValue);
            generator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setterBuilder);

            _referenceProperties.Add(new KeyValuePair<NavigationProperty,PropertyInfo>(navProperty, baseProperty));
        }

        private void EmitCollectionProperty(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, NavigationProperty navProperty)
        {
            const MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            MethodInfo baseSetter = baseProperty.GetSetMethod(true); ;
            MethodAttributes methodAccess = baseSetter.Attributes & MethodAttributes.MemberAccessMask;
            
            string cannotSetException = System.Data.Entity.Strings.EntityProxyTypeInfo_CannotSetEntityCollectionProperty(propertyBuilder.Name, typeBuilder.Name);
            MethodBuilder setterBuilder = typeBuilder.DefineMethod("set_" + baseProperty.Name, methodAccess | methodAttributes, null, new Type[] { baseProperty.PropertyType });
            ILGenerator generator = setterBuilder.GetILGenerator();
            Label instanceEqual = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, _getRelationshipManager);
            generator.Emit(OpCodes.Ldstr, navProperty.RelationshipType.FullName);
            generator.Emit(OpCodes.Ldstr, navProperty.ToEndMember.Name);
            generator.Emit(OpCodes.Callvirt, s_GetRelatedEnd);
            generator.Emit(OpCodes.Beq_S, instanceEqual);
            generator.Emit(OpCodes.Ldstr, cannotSetException);
            generator.Emit(OpCodes.Newobj, s_InvalidOperationConstructor);
            generator.Emit(OpCodes.Throw);
            generator.MarkLabel(instanceEqual);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, baseProperty.GetSetMethod(true));
            generator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setterBuilder);

            _collectionProperties.Add(new KeyValuePair<NavigationProperty, PropertyInfo>(navProperty, baseProperty));
        }

        #region Interface Implementation

        private void ImplementIEntityWithChangeTracker(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            _changeTrackerField = typeBuilder.DefineField("_changeTracker", typeof(IEntityChangeTracker), FieldAttributes.Private);
            registerField(_changeTrackerField, false);

            // Implement EntityMemberChanging(string propertyName)
            _entityMemberChanging = typeBuilder.DefineMethod("EntityMemberChanging", MethodAttributes.Private | MethodAttributes.HideBySig, typeof(void), new Type[] { typeof(string) });
            ILGenerator generator = _entityMemberChanging.GetILGenerator();
            Label methodEnd = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _changeTrackerField);
            generator.Emit(OpCodes.Brfalse_S, methodEnd);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _changeTrackerField);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, s_EntityMemberChanging);
            generator.MarkLabel(methodEnd);
            generator.Emit(OpCodes.Ret);

            // Implement EntityMemberChanged(string propertyName)
            _entityMemberChanged = typeBuilder.DefineMethod("EntityMemberChanged", MethodAttributes.Private | MethodAttributes.HideBySig, typeof(void), new Type[] { typeof(string) });
            generator = _entityMemberChanged.GetILGenerator();
            methodEnd = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _changeTrackerField);
            generator.Emit(OpCodes.Brfalse_S, methodEnd);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _changeTrackerField);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, s_EntityMemberChanged);
            generator.MarkLabel(methodEnd);
            generator.Emit(OpCodes.Ret);

            // Implement IEntityWithChangeTracker.SetChangeTracker(IEntityChangeTracker changeTracker)
            MethodBuilder setChangeTracker = typeBuilder.DefineMethod("SetChangeTracker", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(void), new Type[] { typeof(IEntityChangeTracker) });
            generator = setChangeTracker.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, _changeTrackerField);
            generator.Emit(OpCodes.Ret);
        }

        private void ImplementIEntityWithRelationships(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            _relationshipManagerField = typeBuilder.DefineField("_relationshipManager", typeof(RelationshipManager), FieldAttributes.Private);
            registerField(_relationshipManagerField, true);

            PropertyBuilder relationshipManagerProperty = typeBuilder.DefineProperty("RelationshipManager", System.Reflection.PropertyAttributes.None, typeof(RelationshipManager), Type.EmptyTypes);

            // Implement IEntityWithRelationships.get_RelationshipManager
            _getRelationshipManager = typeBuilder.DefineMethod("get_RelationshipManager", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(RelationshipManager), Type.EmptyTypes);
            ILGenerator generator = _getRelationshipManager.GetILGenerator();
            Label trueLabel = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _relationshipManagerField);
            generator.Emit(OpCodes.Brtrue_S, trueLabel);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, s_CreateRelationshipManager);
            generator.Emit(OpCodes.Stfld, _relationshipManagerField);
            generator.MarkLabel(trueLabel);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _relationshipManagerField);
            generator.Emit(OpCodes.Ret);
            relationshipManagerProperty.SetGetMethod(_getRelationshipManager);
        }

        #endregion
    }

    /// <summary>
    /// Add a DataContractAttribute to the proxy type, based on one that may have been applied to the base type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// From http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractattribute.aspx:
    /// 
    /// A data contract has two basic requirements: a stable name and a list of members. 
    /// The stable name consists of the namespace uniform resource identifier (URI) and the local name of the contract. 
    /// By default, when you apply the DataContractAttribute to a class, 
    /// it uses the class name as the local name and the class's namespace (prefixed with "http://schemas.datacontract.org/2004/07/") 
    /// as the namespace URI. You can override the defaults by setting the Name and Namespace properties. 
    /// You can also change the namespace by applying the ContractNamespaceAttribute to the namespace. 
    /// Use this capability when you have an existing type that processes data exactly as you require 
    /// but has a different namespace and class name from the data contract. 
    /// By overriding the default values, you can reuse your existing type and have the serialized data conform to the data contract. 
    /// </para>
    /// <para>
    /// The first attempt at WCF serialization of proxies involved adding a DataContractAttribute to the proxy type in such a way
    /// so that the name and namespace of the proxy's data contract matched that of the base class.
    /// This worked when serializing proxy objects for the root type of the DataContractSerializer, 
    /// but not for proxy objects of types derived from the root type.
    /// 
    /// Attempting to add the proxy type to the list of known types failed as well, 
    /// since the data contract of the proxy type did not match the base type as intended.
    /// This was due to the fact that inheritance is captured in the data contract.
    /// So while the proxy and base data contracts had the same members, the proxy data contract differed in that is declared itself
    /// as an extension of the base data contract.  So the data contracts were technically not equivalent.
    /// 
    /// The approach used instead is to allow proxy types to have their own DataContract.
    /// Users then have at least two options available to them.
    /// 
    /// The first approach is to add the proxy types to the list of known types.
    /// 
    /// The second approach is to implement an IDataContractSurrogate that can map a proxy instance to a surrogate that does have a data contract
    /// equivalent to the base type (you could use the base type itself for this purpose).  
    /// While more complex to implement, it allows services to hide the use of proxies from clients.
    /// This can be quite useful in order to maximize potential interoperability.
    /// </para>
    /// </remarks>
    internal sealed class DataContractImplementor
    {
        private static readonly ConstructorInfo s_DataContractAttributeConstructor = typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes);
        private static readonly PropertyInfo[] s_DataContractProperties = new PropertyInfo[] {
            typeof(DataContractAttribute).GetProperty("IsReference")
        };

        private readonly Type _baseClrType;
        private readonly DataContractAttribute _dataContract;

        internal DataContractImplementor(EntityType ospaceEntityType)
        {
            _baseClrType = ospaceEntityType.ClrType;

            DataContractAttribute[] attributes = (DataContractAttribute[])_baseClrType.GetCustomAttributes(typeof(DataContractAttribute), false);
            if (attributes.Length > 0)
            {
                _dataContract = attributes[0];
            }
        }

        internal void Implement(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            if (_dataContract != null)
            {
                // Use base data contract properties to help determine values of properties the proxy type's data contract.
                object[] propertyValues = new object[] {
                    // IsReference
                    _dataContract.IsReference
                };

                CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(s_DataContractAttributeConstructor, new object[0], s_DataContractProperties, propertyValues);
                typeBuilder.SetCustomAttribute(attributeBuilder);
            }
        }
    }

    /// <summary>
    /// This class determines if the proxied type implements ISerializable with the special serialization constructor.
    /// If it does, it adds the appropriate members to the proxy type.
    /// </summary>
    internal sealed class ISerializableImplementor 
    {
        private readonly Type _baseClrType;
        private readonly bool _baseImplementsISerializable;
        private readonly bool _canOverride;
        private readonly MethodInfo _getObjectDataMethod;
        private readonly ConstructorInfo _serializationConstructor;

        internal ISerializableImplementor(EntityType ospaceEntityType)
        {
            _baseClrType = ospaceEntityType.ClrType;
            _baseImplementsISerializable = _baseClrType.IsSerializable && typeof(ISerializable).IsAssignableFrom(_baseClrType);

            if (_baseImplementsISerializable)
            {
                // Determine if interface implementation can be overridden.
                // Fortunately, there's only one method to check.
                InterfaceMapping mapping = _baseClrType.GetInterfaceMap(typeof(ISerializable));
                _getObjectDataMethod = mapping.TargetMethods[0];

                // Members that implement interfaces must be public, unless they are explicitly implemented, in which case they are private and sealed (at least for C#).
                bool canOverrideMethod = (_getObjectDataMethod.IsVirtual && !_getObjectDataMethod.IsFinal) && _getObjectDataMethod.IsPublic;

                if (canOverrideMethod)
                {
                    // Determine if proxied type provides the special serialization constructor.
                    // In order for the proxy class to properly support ISerializable, this constructor must not be private.
                    _serializationConstructor = _baseClrType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);

                    _canOverride = _serializationConstructor != null && (_serializationConstructor.IsPublic || _serializationConstructor.IsFamily || _serializationConstructor.IsFamilyOrAssembly);
                }

                Debug.Assert(!(_canOverride && (_getObjectDataMethod == null || _serializationConstructor == null)), "Both GetObjectData method and Serialization Constructor must be present when proxy overrides ISerializable implementation.");
            }
        }

        internal bool TypeIsSuitable
        {
            get 
            {
                // To be suitable,
                // either proxied type doesn't implement ISerializable,
                // or it does and it can be suitably overridden.
                return !_baseImplementsISerializable || _canOverride;
            }
        }

        internal bool TypeImplementsISerializable
        {
            get
            {
                return _baseImplementsISerializable;
            }
        }

        internal void Implement(TypeBuilder typeBuilder, IEnumerable<FieldBuilder> serializedFields)
        {
            if (_baseImplementsISerializable && _canOverride)
            {
                PermissionSet serializationFormatterPermissions = new PermissionSet(null);
                serializationFormatterPermissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));

                Type[] parameterTypes = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
                MethodInfo getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
                MethodInfo addValue = typeof(SerializationInfo).GetMethod("AddValue", new Type[] { typeof(string), typeof(object), typeof(Type) });
                MethodInfo getValue = typeof(SerializationInfo).GetMethod("GetValue", new Type[] { typeof(string), typeof(Type) });

                //
                // Define GetObjectData method override
                //
                // [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
                // public void GetObjectData(SerializationInfo info, StreamingContext context)
                //
                MethodBuilder proxyGetObjectData = typeBuilder.DefineMethod(_getObjectDataMethod.Name,
                                                                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                                                                            null,
                                                                            parameterTypes);
                proxyGetObjectData.AddDeclarativeSecurity(SecurityAction.Demand, serializationFormatterPermissions);

                {
                    ILGenerator generator = proxyGetObjectData.GetILGenerator();

                    // Call SerializationInfo.AddValue to serialize each field value
                    foreach (FieldBuilder field in serializedFields)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldstr, field.Name);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldfld, field);
                        generator.Emit(OpCodes.Ldtoken, field.FieldType);
                        generator.Emit(OpCodes.Call, getTypeFromHandle);
                        generator.Emit(OpCodes.Callvirt, addValue);
                    }

                    // Emit call to base method
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, _getObjectDataMethod);
                    generator.Emit(OpCodes.Ret);
                }

                //
                // Define serialization constructor
                //
                // [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
                // .ctor(SerializationInfo info, StreamingContext context)
                //
                MethodAttributes constructorAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                constructorAttributes |= _serializationConstructor.IsPublic? MethodAttributes.Public : MethodAttributes.Private;

                ConstructorBuilder proxyConstructor = typeBuilder.DefineConstructor(constructorAttributes, CallingConventions.Standard | CallingConventions.HasThis, parameterTypes);
                proxyConstructor.AddDeclarativeSecurity(SecurityAction.Demand, serializationFormatterPermissions);

                {
                    //Emit call to base serialization constructor
                    ILGenerator generator = proxyConstructor.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, _serializationConstructor);

                    // Call SerializationInfo.GetValue to retrieve the value of each field
                    foreach (FieldBuilder field in serializedFields)
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldstr, field.Name);
                        generator.Emit(OpCodes.Ldtoken, field.FieldType);
                        generator.Emit(OpCodes.Call, getTypeFromHandle);
                        generator.Emit(OpCodes.Callvirt, getValue);
                        generator.Emit(OpCodes.Castclass, field.FieldType);
                        generator.Emit(OpCodes.Stfld, field);
                    }

                    generator.Emit(OpCodes.Ret);
                }
            }
        }
    }
}
