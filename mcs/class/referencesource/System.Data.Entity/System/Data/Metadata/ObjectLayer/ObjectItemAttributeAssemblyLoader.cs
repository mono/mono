//---------------------------------------------------------------------
// <copyright file="ObjectItemAttributeAssemblyLoader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class for representing a collection of items for the object layer.
    /// Most of the implemetation for actual maintainance of the collection is
    /// done by ItemCollection
    /// </summary>
    internal sealed class ObjectItemAttributeAssemblyLoader : ObjectItemAssemblyLoader
    {
        #region Fields

        // list of unresolved navigation properties
        private readonly List<Action> _unresolvedNavigationProperties = new List<Action>();
        private new MutableAssemblyCacheEntry CacheEntry { get { return (MutableAssemblyCacheEntry)base.CacheEntry; } }
        private List<Action> _referenceResolutions = new List<Action>();

        #endregion

        #region Constructor
        internal ObjectItemAttributeAssemblyLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
            :base(assembly, new MutableAssemblyCacheEntry(), sessionData)
        {
            Debug.Assert(Create == sessionData.ObjectItemAssemblyLoaderFactory, "Why is there a different factory creating this class");

        }
        #endregion

        #region Methods

        internal override void OnLevel1SessionProcessing()
        {
            foreach (Action resolve in _referenceResolutions)
            {
                resolve();
            }
        }

        internal override void OnLevel2SessionProcessing()
        {
            foreach (Action resolve in _unresolvedNavigationProperties)
            {
                resolve();
            }
        }
        /// <summary>
        /// Loads the given assembly and all the other referencd assemblies in the cache. If the assembly was already present
        /// then it loads from the cache
        /// </summary>
        /// <param name="context"></param>
        /// <returns>true if the assembly was already loaded in the cache</returns>
        internal override void Load()
        {
            Debug.Assert(IsSchemaAttributePresent(SourceAssembly), "LoadAssembly shouldn't be called with assembly having no schema attribute");
            Debug.Assert(!SessionData.KnownAssemblies.Contains(SourceAssembly, SessionData.ObjectItemAssemblyLoaderFactory, SessionData.EdmItemCollection), "InternalLoadAssemblyFromCache: This assembly must not be present in the list of known assemblies");

            base.Load();
        }

        protected override void AddToAssembliesLoaded()
        {
            SessionData.AssembliesLoaded.Add(SourceAssembly, CacheEntry);
        }
        /// <summary>
        /// Check to see if the type is already loaded - either in the typesInLoading, or ObjectItemCollection or
        /// in the global cache
        /// </summary>
        /// <param name="clrType"></param>
        /// <param name="edmType"></param>
        /// <returns></returns>
        private bool TryGetLoadedType(Type clrType, out EdmType edmType)
        {
            if (SessionData.TypesInLoading.TryGetValue(clrType.FullName, out edmType) ||
                TryGetCachedEdmType(clrType, out edmType))
            {
                // Check to make sure the CLR type we got is the same as the given one
                if (edmType.ClrType != clrType)
                {
                    SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NewTypeConflictsWithExistingType(
                                                clrType.AssemblyQualifiedName, edmType.ClrType.AssemblyQualifiedName), edmType));
                    edmType = null;
                    return false;
                }
                return true;
            }


            // Let's check to see if this type is a ref type, a nullable type, or a collection type, these are the types that
            // we need to take special care of them
            if (clrType.IsGenericType)
            {
                Type genericType = clrType.GetGenericTypeDefinition();

                // Try to resolve the element type into a type object
                EdmType elementType;
                if (!TryGetLoadedType(clrType.GetGenericArguments()[0], out elementType))
                    return false;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(clrType))
                {
                    EntityType entityType = elementType as EntityType;
                    if (entityType == null)
                    {
                        // return null and let the caller deal with the error handling
                        return false;
                    }
                    edmType = entityType.GetCollectionType();
                }
                else
                {
                    edmType = elementType;
                }

                return true;
            }


            edmType = null;
            return false;
        }
        
        private bool TryGetCachedEdmType(Type clrType, out EdmType edmType)
        {
            Debug.Assert(!SessionData.TypesInLoading.ContainsKey(clrType.FullName), "This should be called only after looking in typesInLoading");
            Debug.Assert(SessionData.EdmItemErrors.Count > 0 || // had an error during loading
                        clrType.GetCustomAttributes(typeof(EdmTypeAttribute), false /*inherit*/).Length == 0 || // not a type we track
                        SourceAssembly != clrType.Assembly, // not from this assembly
                        "Given that we don't have any error, if the type is part of this assembly, it should not be loaded from the cache");

            ImmutableAssemblyCacheEntry immutableCacheEntry;
            if (SessionData.LockedAssemblyCache.TryGetValue(clrType.Assembly, out immutableCacheEntry))
            {
                Debug.Assert(SessionData.KnownAssemblies.Contains(clrType.Assembly, SessionData.LoaderCookie, SessionData.EdmItemCollection), "We should only be loading things directly from the cache if they are already in the collection");
                return immutableCacheEntry.TryGetEdmType(clrType.FullName, out edmType);
            }

            edmType = null;
            return false;
        }

        #endregion
        /// <summary>
        /// Loads the set of types from the given assembly and adds it to the given list of types
        /// </summary>
        /// <param name="context">context containing information for loading</param>
        protected override void LoadTypesFromAssembly()
        {
            Debug.Assert(CacheEntry.TypesInAssembly.Count == 0);

            LoadRelationshipTypes();

            // Loop through each type in the assembly and process it
            foreach (Type type in EntityUtil.GetTypesSpecial(SourceAssembly))
            {
                // If the type doesn't have the same EdmTypeAttribute defined, then it's not a special type
                // that we care about, skip it.
                if (!type.IsDefined(typeof(EdmTypeAttribute), false))
                {
                    continue;
                }

                // Generic type is not supported, if the user attributed this generic type using EdmTypeAttribute,
                // then the exception message can help them better understand what is going on instead of just
                // failing at a much later point of OC type mapping lookup with a super generic error message
                if (type.IsGenericType)
                {
                    SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.GenericTypeNotSupported(type.FullName), null));
                    continue;
                }

                // Load the metadata for this type
                LoadType(type);
            }

            if (_referenceResolutions.Count != 0)
            {
                SessionData.RegisterForLevel1PostSessionProcessing(this);
            }

            if (_unresolvedNavigationProperties.Count != 0)
            {
                SessionData.RegisterForLevel2PostSessionProcessing(this);
            }
        }

        /// <summary>
        /// This method loads all the relationship type that this entity takes part in
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="context"></param>
        private void LoadRelationshipTypes()
        {
            foreach (EdmRelationshipAttribute roleAttribute in SourceAssembly.GetCustomAttributes(typeof(EdmRelationshipAttribute), false /*inherit*/))
            {
                // Check if there is an entry already with this name
                if (TryFindNullParametersInRelationshipAttribute(roleAttribute))
                {
                    // don't give more errors for these same bad parameters
                    continue;
                }

                bool errorEncountered = false;

                // return error if the role names are the same
                if (roleAttribute.Role1Name == roleAttribute.Role2Name)
                {
                    SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.SameRoleNameOnRelationshipAttribute(roleAttribute.RelationshipName, roleAttribute.Role2Name),
                               null));
                    errorEncountered = true;
                }


                if (!errorEncountered)
                {
                    AssociationType associationType = new AssociationType(roleAttribute.RelationshipName, roleAttribute.RelationshipNamespaceName, roleAttribute.IsForeignKey, DataSpace.OSpace);
                    SessionData.TypesInLoading.Add(associationType.FullName, associationType);
                    TrackClosure(roleAttribute.Role1Type);
                    TrackClosure(roleAttribute.Role2Type);

                    // prevent lifting of loop vars
                    string r1Name = roleAttribute.Role1Name;
                    Type r1Type = roleAttribute.Role1Type;
                    RelationshipMultiplicity r1Multiplicity = roleAttribute.Role1Multiplicity;
                    AddTypeResolver(() =>
                        ResolveAssociationEnd(associationType, r1Name, r1Type, r1Multiplicity));

                    // prevent lifting of loop vars
                    string r2Name = roleAttribute.Role2Name;
                    Type r2Type = roleAttribute.Role2Type;
                    RelationshipMultiplicity r2Multiplicity = roleAttribute.Role2Multiplicity;
                    AddTypeResolver(() =>
                        ResolveAssociationEnd(associationType, r2Name, r2Type, r2Multiplicity));

                    // get assembly entry and add association type to the list of types in the assembly
                    Debug.Assert(!CacheEntry.ContainsType(associationType.FullName), "Relationship type must not be present in the list of types");
                    CacheEntry.TypesInAssembly.Add(associationType);
                }
            }
        }

        private void ResolveAssociationEnd(AssociationType associationType, string roleName, Type clrType, RelationshipMultiplicity multiplicity)
        {
            EntityType entityType;
            if (!TryGetRelationshipEndEntityType(clrType, out entityType))
            {
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.RoleTypeInEdmRelationshipAttributeIsInvalidType(associationType.Name, roleName, clrType),
                           null));
                return;
            }
            associationType.AddKeyMember(new AssociationEndMember(roleName, entityType.GetReferenceType(), multiplicity));
        }
        /// <summary>
        /// Load metadata of the given type - when you call this method, you should check and make sure that the type has
        /// edm attribute. If it doesn't,we won't load the type and it will be returned as null
        /// </summary>
        /// <param name="clrType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private void LoadType(Type clrType)
        {
            Debug.Assert(clrType.Assembly == SourceAssembly, "Why are we loading a type that is not in our assembly?");
            Debug.Assert(!SessionData.TypesInLoading.ContainsKey(clrType.FullName), "Trying to load a type that is already loaded???");
            Debug.Assert(!clrType.IsGenericType, "Generic type is not supported");

            EdmType edmType = null;

            EdmTypeAttribute[] typeAttributes = (EdmTypeAttribute[])clrType.GetCustomAttributes(typeof(EdmTypeAttribute), false /*inherit*/);

            // the CLR doesn't allow types to have duplicate/multiple attribute declarations

            if (typeAttributes.Length != 0)
            {
                if (clrType.IsNested)
                {
                    SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NestedClassNotSupported(clrType.FullName, clrType.Assembly.FullName), null));
                    return;
                }
                EdmTypeAttribute typeAttribute = typeAttributes[0];
                string cspaceTypeName = String.IsNullOrEmpty(typeAttribute.Name) ? clrType.Name : typeAttribute.Name;
                if (String.IsNullOrEmpty(typeAttribute.NamespaceName) && clrType.Namespace == null)
                {
                    SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_TypeHasNoNamespace, edmType));
                    return;
                }

                string cspaceNamespaceName = String.IsNullOrEmpty(typeAttribute.NamespaceName) ? clrType.Namespace : typeAttribute.NamespaceName;

                if (typeAttribute.GetType() == typeof(EdmEntityTypeAttribute))
                {
                    edmType = new ClrEntityType(clrType, cspaceNamespaceName, cspaceTypeName);
                }
                else if(typeAttribute.GetType() == typeof(EdmComplexTypeAttribute))
                {
                    edmType = new ClrComplexType(clrType, cspaceNamespaceName, cspaceTypeName);
                }
                else 
                {
                    Debug.Assert(typeAttribute is EdmEnumTypeAttribute, "Invalid type attribute encountered");

                    // Note that TryGetPrimitiveType() will return false not only for types that are not primitive 
                    // but also for CLR primitive types that are valid underlying enum types in CLR but are not 
                    // a valid Edm primitive types (e.g. ulong) 
                    PrimitiveType underlyingEnumType;
                    if (!ClrProviderManifest.Instance.TryGetPrimitiveType(clrType.GetEnumUnderlyingType(), out underlyingEnumType))
                    {
                        SessionData.EdmItemErrors.Add(
                            new EdmItemError(
                                Strings.Validator_UnsupportedEnumUnderlyingType(clrType.GetEnumUnderlyingType().FullName),
                                edmType));

                        return;
                    }

                    edmType = new ClrEnumType(clrType, cspaceNamespaceName, cspaceTypeName);
                }
            }
            else
            {
                // not a type we are interested
                return;
            }

            Debug.Assert(!CacheEntry.ContainsType(edmType.Identity), "This type must not be already present in the list of types for this assembly");
            // Also add this to the list of the types for this assembly
            CacheEntry.TypesInAssembly.Add(edmType);

            // Add this to the known type map so we won't try to load it again
            SessionData.TypesInLoading.Add(clrType.FullName, edmType);

            // Load properties for structural type
            if (Helper.IsStructuralType(edmType))
            {
                //Load base type only for entity type - not sure if we will allow complex type inheritance
                if (Helper.IsEntityType(edmType))
                {
                    TrackClosure(clrType.BaseType);
                    AddTypeResolver(
                        () => edmType.BaseType = ResolveBaseType(clrType.BaseType));
                }

                // Load the properties for this type
                LoadPropertiesFromType((StructuralType)edmType);
            }

            return;
        }

        private void AddTypeResolver(Action resolver)
        {
            _referenceResolutions.Add(resolver);
        }

        private EdmType ResolveBaseType(Type type)
        {
            EdmType edmType;
            if (type.GetCustomAttributes(typeof(EdmEntityTypeAttribute), false).Length > 0 && TryGetLoadedType(type, out edmType))
            {
                return edmType;
            }
            return null;            
        }

        private bool TryFindNullParametersInRelationshipAttribute(EdmRelationshipAttribute roleAttribute)
        {
            if (roleAttribute.RelationshipName == null)
            {
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NullRelationshipNameforEdmRelationshipAttribute(SourceAssembly.FullName), null));
                return true;
            }

            bool nullsFound = false;

            if (roleAttribute.RelationshipNamespaceName == null)
            {
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NullParameterForEdmRelationshipAttribute(
                    "RelationshipNamespaceName", roleAttribute.RelationshipName), null));
                nullsFound = true;
            }

            if (roleAttribute.Role1Name == null)
            {
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NullParameterForEdmRelationshipAttribute(
                    "Role1Name", roleAttribute.RelationshipName), null));
                nullsFound = true;
            }

            if (roleAttribute.Role1Type == null)
            {
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NullParameterForEdmRelationshipAttribute(
                    "Role1Type", roleAttribute.RelationshipName), null));
                nullsFound = true;
            }

            if (roleAttribute.Role2Name == null)
            {
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NullParameterForEdmRelationshipAttribute(
                    "Role2Name", roleAttribute.RelationshipName), null));
                nullsFound = true;
            }

            if (roleAttribute.Role2Type == null)
            {
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NullParameterForEdmRelationshipAttribute(
                    "Role2Type", roleAttribute.RelationshipName), null));
                nullsFound = true;
            }

            return nullsFound;
        }


        private bool TryGetRelationshipEndEntityType(Type type, out EntityType entityType)
        {
            if (type == null)
            {
                entityType = null;
                return false;
            }

            EdmType edmType;
            if (!TryGetLoadedType(type, out edmType) || !Helper.IsEntityType(edmType))
            {
                entityType = null;
                return false;
            }
            entityType = (EntityType)edmType;
            return true;
        }

        /// <summary>
        /// Load all the property metadata of the given type
        /// </summary>
        /// <param name="type">The CLR entity type</param>
        /// <param name="structuralType">The type where properties are loaded</param>
        /// <param name="context"></param>
        private void LoadPropertiesFromType(StructuralType structuralType)
        {
            // Look at both public, internal, and private instanced properties declared at this type, inherited members
            // are not looked at.  Internal and private properties are also looked at because they are also schematized fields
            PropertyInfo[] properties = structuralType.ClrType.GetProperties(PropertyReflectionBindingFlags);

            foreach (PropertyInfo property in properties)
            {
                EdmMember newMember = null;
                bool isEntityKeyProperty = false; //used for EdmScalarProperties only

                // EdmScalarPropertyAttribute, EdmComplexPropertyAttribute and EdmRelationshipNavigationPropertyAttribute
                // are all EdmPropertyAttributes that we need to process. If the current property is not an EdmPropertyAttribute
                // we will just ignore it and skip to the next property.
                if (property.IsDefined(typeof(EdmRelationshipNavigationPropertyAttribute), false))
                {
                    // keep the loop var from being lifted
                    PropertyInfo pi = property;
                    _unresolvedNavigationProperties.Add(() =>
                            ResolveNavigationProperty(structuralType, pi));
                }
                else if (property.IsDefined(typeof(EdmScalarPropertyAttribute), false))
                {
                    if ((Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType).IsEnum)
                    {
                        TrackClosure(property.PropertyType);
                        PropertyInfo local = property;
                        AddTypeResolver(() => ResolveEnumTypeProperty(structuralType, local));
                    }
                    else
                    {
                        newMember = LoadScalarProperty(structuralType.ClrType, property, out isEntityKeyProperty);
                    }
                }
                else if (property.IsDefined(typeof(EdmComplexPropertyAttribute), false))
                {
                    TrackClosure(property.PropertyType);
                    // keep loop var from being lifted
                    PropertyInfo local = property;
                    AddTypeResolver(() => ResolveComplexTypeProperty(structuralType, local));
                }

                if (newMember == null)
                {
                    // Property does not have one of the following attributes:
                    //     EdmScalarPropertyAttribute, EdmComplexPropertyAttribute, EdmRelationshipNavigationPropertyAttribute
                    // This means its an unmapped property and can be ignored.
                    // Or there were error encountered while loading the properties
                    continue;
                }

                // Add the property object to the type
                structuralType.AddMember(newMember);

                // Add to the entity's collection of key members
                // Do this here instead of in the if condition above for scalar properties because
                // we want to make sure the AddMember call above did not fail before updating the key members
                if (Helper.IsEntityType(structuralType) && isEntityKeyProperty)
                {
                    ((EntityType)structuralType).AddKeyMember(newMember);
                }
            }
        }

        internal void ResolveNavigationProperty(StructuralType declaringType, PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo.IsDefined(typeof(EdmRelationshipNavigationPropertyAttribute), false), "The property must have navigation property defined");

            // EdmScalarPropertyAttribute, EdmComplexPropertyAttribute and EdmRelationshipNavigationPropertyAttribute
            // are all EdmPropertyAttributes that we need to process. If the current property is not an EdmPropertyAttribute
            // we will just ignore it and skip to the next property.
            object[] relationshipPropertyAttributes = propertyInfo.GetCustomAttributes(typeof(EdmRelationshipNavigationPropertyAttribute), false);

            Debug.Assert(relationshipPropertyAttributes.Length == 1, "There should be exactly one property for every navigation property");

            // The only valid return types from navigation properties are:
            //     (1) EntityType
            //     (2) CollectionType containing valid EntityType

            // If TryGetLoadedType returned false, it could mean that we couldn't validate any part of the type, or it could mean that it's a generic
            // where the main generic type was validated, but the generic type parameter was not. We can't tell the difference, so just fail
            // with the same error message in both cases. The user will have to figure out which part of the type is wrong.
            // We can't just rely on checking for a generic because it can lead to a scenario where we report that the type parameter is invalid
            // when really it's the main generic type. That is more confusing than reporting the full name and letting the user determine the problem.
            EdmType propertyType;
            if (!TryGetLoadedType(propertyInfo.PropertyType, out propertyType) || !(propertyType.BuiltInTypeKind == BuiltInTypeKind.EntityType || propertyType.BuiltInTypeKind == BuiltInTypeKind.CollectionType))
            {
                // Once an error is detected the property does not need to be validated further, just add to the errors
                // collection and continue with the next property. The failure will cause an exception to be thrown later during validation of all of the types.
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.Validator_OSpace_InvalidNavPropReturnType(propertyInfo.Name, propertyInfo.DeclaringType.FullName, propertyInfo.PropertyType.FullName), null));
                return;
            }
            // else we have a valid EntityType or CollectionType that contains EntityType. ResolveNonSchemaType enforces that a collection type
            // must contain an EntityType, and if it doesn't, propertyType will be null here. If propertyType is EntityType or CollectionType we know it is valid

            // Expecting EdmRelationshipNavigationPropertyAttribute to have AllowMultiple=False, so only look at first element in the attribute array

            EdmRelationshipNavigationPropertyAttribute attribute = (EdmRelationshipNavigationPropertyAttribute)relationshipPropertyAttributes[0];

            EdmMember member = null;
            EdmType type;
            if (SessionData.TypesInLoading.TryGetValue(attribute.RelationshipNamespaceName + "." + attribute.RelationshipName, out type) &&
                Helper.IsAssociationType(type))
            {
                AssociationType relationshipType = (AssociationType)type;
                if (relationshipType != null)
                {
                    // The return value of this property has been verified, so create the property now
                    NavigationProperty navigationProperty = new NavigationProperty(propertyInfo.Name, TypeUsage.Create(propertyType), propertyInfo);
                    navigationProperty.RelationshipType = relationshipType;
                    member = navigationProperty;

                    if (relationshipType.Members[0].Name == attribute.TargetRoleName)
                    {
                        navigationProperty.ToEndMember = (RelationshipEndMember)relationshipType.Members[0];
                        navigationProperty.FromEndMember = (RelationshipEndMember)relationshipType.Members[1];
                    }
                    else if (relationshipType.Members[1].Name == attribute.TargetRoleName)
                    {
                        navigationProperty.ToEndMember = (RelationshipEndMember)relationshipType.Members[1];
                        navigationProperty.FromEndMember = (RelationshipEndMember)relationshipType.Members[0];
                    }
                    else
                    {
                        SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.TargetRoleNameInNavigationPropertyNotValid(
                                                    propertyInfo.Name, propertyInfo.DeclaringType.FullName, attribute.TargetRoleName, attribute.RelationshipName), navigationProperty));
                        member = null;
                    }

                    if (member != null &&
                        ((RefType)navigationProperty.FromEndMember.TypeUsage.EdmType).ElementType.ClrType != declaringType.ClrType)
                    {
                        SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.NavigationPropertyRelationshipEndTypeMismatch(
                                                    declaringType.FullName,
                                                    navigationProperty.Name,
                                                    relationshipType.FullName,
                                                    navigationProperty.FromEndMember.Name,
                                                    ((RefType)navigationProperty.FromEndMember.TypeUsage.EdmType).ElementType.ClrType), navigationProperty));
                        member = null;
                    }
                }
            }
            else
            {
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.RelationshipNameInNavigationPropertyNotValid(
                                            propertyInfo.Name, propertyInfo.DeclaringType.FullName, attribute.RelationshipName), declaringType));
            }

            if (member != null)
            {
                declaringType.AddMember(member);
            }
        }


        /// <summary>
        /// Load the property with scalar property attribute.
        /// Note that we pass the CLR type in because in the case where the property is declared on a generic
        /// base class the DeclaringType of propert won't work for us and we need the real entity type instead.
        /// </summary>
        /// <param name="type">The CLR type of the entity</param>
        /// <param name="property">Metadata representing the property</param>
        /// <param name="isEntityKeyProperty">True if the property forms part of the entity's key</param>
        /// <returns></returns>
        private EdmMember LoadScalarProperty(Type clrType, PropertyInfo property, out bool isEntityKeyProperty)
        {
            Debug.Assert(property.IsDefined(typeof(EdmScalarPropertyAttribute), false), "The property must have a scalar attribute");
            EdmMember member = null;
            isEntityKeyProperty = false;

            // Load the property type and create a new property object
            PrimitiveType primitiveType;

            // If the type could not be loaded it's definitely not a primitive type, so that's an error
            // If it could be loaded but is not a primitive that's an error as well
            if (!TryGetPrimitiveType(property.PropertyType, out primitiveType))
            {
                // This property does not need to be validated further, just add to the errors collection and continue with the next property
                // This failure will cause an exception to be thrown later during validation of all of the types
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.Validator_OSpace_ScalarPropertyNotPrimitive(property.Name, property.DeclaringType.FullName, property.PropertyType.FullName), null));
            }
            else
            {
                object[] attrs = property.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false);

                Debug.Assert(attrs.Length == 1, "Every property can exactly have one ScalarProperty Attribute");
                // Expecting EdmScalarPropertyAttribute to have AllowMultiple=False, so only look at first element in the attribute array
                isEntityKeyProperty = ((EdmScalarPropertyAttribute)attrs[0]).EntityKeyProperty;
                bool isNullable = ((EdmScalarPropertyAttribute)attrs[0]).IsNullable;

                member = new EdmProperty(property.Name,
                    TypeUsage.Create(primitiveType, new FacetValues { Nullable = isNullable }),
                    property, clrType.TypeHandle);

            }
            return member;
        }

        /// <summary>
        /// Resolves enum type property.
        /// </summary>
        /// <param name="declaringType">The type to add the declared property to.</param>
        /// <param name="clrProperty">Property to resolve.</param>
        private void ResolveEnumTypeProperty(StructuralType declaringType, PropertyInfo clrProperty)
        {
            Debug.Assert(declaringType != null, "type != null");
            Debug.Assert(clrProperty != null, "clrProperty != null");
            Debug.Assert(
                (Nullable.GetUnderlyingType(clrProperty.PropertyType) ?? clrProperty.PropertyType).IsEnum, 
                "This method should be called for enums only");

            EdmType propertyType;

            if (!TryGetLoadedType(clrProperty.PropertyType, out propertyType) || !Helper.IsEnumType(propertyType))
            {
                SessionData.EdmItemErrors.Add(
                    new EdmItemError(
                        System.Data.Entity.Strings.Validator_OSpace_ScalarPropertyNotPrimitive(
                            clrProperty.Name,
                            clrProperty.DeclaringType.FullName,
                            clrProperty.PropertyType.FullName), null));
            }
            else
            {
                var edmScalarPropertyAttribute = (EdmScalarPropertyAttribute)clrProperty.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false).Single();

                EdmProperty enumProperty = new EdmProperty(
                    clrProperty.Name,
                    TypeUsage.Create(propertyType, new FacetValues() { Nullable = edmScalarPropertyAttribute.IsNullable }),
                    clrProperty,
                    declaringType.ClrType.TypeHandle);

                declaringType.AddMember(enumProperty);

                if (declaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType && edmScalarPropertyAttribute.EntityKeyProperty)
                {
                    ((EntityType)declaringType).AddKeyMember(enumProperty);
                }
            }
        }

        private void ResolveComplexTypeProperty(StructuralType type, PropertyInfo clrProperty)
        {

            // Load the property type and create a new property object
            EdmType propertyType;
            // If the type could not be loaded it's definitely not a complex type, so that's an error
            // If it could be loaded but is not a complex type that's an error as well
            if (!TryGetLoadedType(clrProperty.PropertyType, out propertyType) || propertyType.BuiltInTypeKind != BuiltInTypeKind.ComplexType)
            {
                // This property does not need to be validated further, just add to the errors collection and continue with the next property
                // This failure will cause an exception to be thrown later during validation of all of the types
                SessionData.EdmItemErrors.Add(new EdmItemError(System.Data.Entity.Strings.Validator_OSpace_ComplexPropertyNotComplex(clrProperty.Name, clrProperty.DeclaringType.FullName, clrProperty.PropertyType.FullName), null));
            }
            else
            {
                EdmProperty newProperty = new EdmProperty(clrProperty.Name,
                    TypeUsage.Create(propertyType, new FacetValues { Nullable = false }),
                    clrProperty, type.ClrType.TypeHandle);

                type.AddMember(newProperty);
            }

        }

        private void TrackClosure(Type type)
        {

            if (SourceAssembly != type.Assembly &&
                !CacheEntry.ClosureAssemblies.Contains(type.Assembly) &&
                IsSchemaAttributePresent(type.Assembly) &&
                !(type.IsGenericType &&
                  (
                    EntityUtil.IsAnICollection(type) || // EntityCollection<>, List<>, ICollection<>
                    type.GetGenericTypeDefinition() == typeof(System.Data.Objects.DataClasses.EntityReference<>) ||
                    type.GetGenericTypeDefinition() == typeof(System.Nullable<>)
                  )
                 )
                )
            {
                CacheEntry.ClosureAssemblies.Add(type.Assembly);
            }

            if (type.IsGenericType)
            {
                foreach (Type genericArgument in type.GetGenericArguments())
                {
                    TrackClosure(genericArgument);
                }
            }
        }

        internal static bool IsSchemaAttributePresent(Assembly assembly)
        {
            return assembly.IsDefined(typeof(EdmSchemaAttribute), false /*inherit*/);
        }

        internal static ObjectItemAssemblyLoader Create(Assembly assembly, ObjectItemLoadingSessionData sessionData)
        {
            if (ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(assembly))
            {
                return new ObjectItemAttributeAssemblyLoader(assembly, sessionData);
            }
            else
            {
                return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
            }
        }

    }
}
