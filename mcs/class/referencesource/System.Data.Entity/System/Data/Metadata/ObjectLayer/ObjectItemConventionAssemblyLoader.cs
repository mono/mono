//---------------------------------------------------------------------
// <copyright file="vs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    internal sealed class ObjectItemConventionAssemblyLoader : ObjectItemAssemblyLoader
    {
        // for root entities, entities with no base type, we will additionally look 
        // at properties on the clr base hierarchy.
        private const BindingFlags RootEntityPropertyReflectionBindingFlags = PropertyReflectionBindingFlags & ~BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy;

        private new MutableAssemblyCacheEntry CacheEntry { get { return (MutableAssemblyCacheEntry)base.CacheEntry; } }
        private List<Action> _referenceResolutions = new List<Action>();

        internal ObjectItemConventionAssemblyLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
            : base(assembly, new MutableAssemblyCacheEntry(), sessionData)
        {
            Debug.Assert(Create == sessionData.ObjectItemAssemblyLoaderFactory, "Why is there a different factory creating this class");
            SessionData.RegisterForLevel1PostSessionProcessing(this);
        }

        protected override void LoadTypesFromAssembly()
        {
            foreach (Type type in EntityUtil.GetTypesSpecial(SourceAssembly))
            {
                EdmType cspaceType;
                if (TryGetCSpaceTypeMatch(type, out cspaceType))
                {
                    if (type.IsValueType && !type.IsEnum)
                    {
                        SessionData.LoadMessageLogger.LogLoadMessage(Strings.Validator_OSpace_Convention_Struct(cspaceType.FullName, type.FullName), cspaceType);
                        continue;
                    }

                    EdmType ospaceType;
                    if (TryCreateType(type, cspaceType, out ospaceType))
                    {
                        Debug.Assert(ospaceType is StructuralType || Helper.IsEnumType(ospaceType), "Only StructuralType or EnumType expected.");

                        CacheEntry.TypesInAssembly.Add(ospaceType);
                        // check for duplicates so we don't cause an ArgumentException, 
                        // Mapping will do the actual error for the duplicate type later
                        if (!SessionData.CspaceToOspace.ContainsKey(cspaceType))
                        {
                            SessionData.CspaceToOspace.Add(cspaceType, ospaceType);
                        }
                        else
                        {
                            // at this point there is already a Clr Type that is structurally matched to this CSpace type, we throw exception
                            EdmType previousOSpaceType = SessionData.CspaceToOspace[cspaceType];
                            SessionData.EdmItemErrors.Add(
                                new EdmItemError(Strings.Validator_OSpace_Convention_AmbiguousClrType(cspaceType.Name, previousOSpaceType.ClrType.FullName, type.FullName), previousOSpaceType));
                        }
                    }
                }
            }

            if (SessionData.TypesInLoading.Count == 0)
            {
                Debug.Assert(CacheEntry.ClosureAssemblies.Count == 0, "How did we get closure assemblies?");

                // since we didn't find any types, don't lock into convention based
                SessionData.ObjectItemAssemblyLoaderFactory = null;
            }
        }


        protected override void AddToAssembliesLoaded()
        {
            SessionData.AssembliesLoaded.Add(SourceAssembly, CacheEntry);
        }

        private bool TryGetCSpaceTypeMatch(Type type, out EdmType cspaceType)
        {
            // brute force try and find a matching name
            KeyValuePair<EdmType, int> pair;
            if (SessionData.ConventionCSpaceTypeNames.TryGetValue(type.Name, out pair))
            {
                if (pair.Value == 1)
                {
                    // we found a type match
                    cspaceType = pair.Key;
                    return true;
                }
                else
                {
                    Debug.Assert(pair.Value > 1, "how did we get a negative count of types in the dictionary?");
                    SessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_Convention_MultipleTypesWithSameName(type.Name), pair.Key));
                }
            }

            cspaceType = null;
            return false;
        }

        /// <summary>
        /// Creates a structural or enum OSpace type based on CLR type and CSpace type.
        /// </summary>
        /// <param name="type">CLR type.</param>
        /// <param name="cspaceType">CSpace Type</param>
        /// <param name="newOSpaceType">OSpace type created based on CLR <paramref name="type"/> and <paramref name="cspaceType"/></param>
        /// <returns><c>true</c> if the type was created successfully. Otherwise <c>false</c>.</returns>
        private bool TryCreateType(Type type, EdmType cspaceType, out EdmType newOSpaceType)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(cspaceType != null, "cspaceType != null");
            Debug.Assert(cspaceType is StructuralType || Helper.IsEnumType(cspaceType), "Structural or enum type expected");

            newOSpaceType = null;

            // if one of the types is an enum while the other is not there is no match
            if (Helper.IsEnumType(cspaceType) ^ type.IsEnum)
            {
                SessionData.LoadMessageLogger.LogLoadMessage(
                    Strings.Validator_OSpace_Convention_SSpaceOSpaceTypeMismatch(cspaceType.FullName, cspaceType.FullName),
                    cspaceType);
                return false;
            }

            if(Helper.IsEnumType(cspaceType))
            {
                return TryCreateEnumType(type, (EnumType)cspaceType, out newOSpaceType);
            }
            else
            {
                Debug.Assert(cspaceType is StructuralType);
                return TryCreateStructuralType(type, (StructuralType)cspaceType, out newOSpaceType);
            }
        }

        /// <summary>
        /// Creates a structural OSpace type based on CLR type and CSpace type.
        /// </summary>
        /// <param name="type">CLR type.</param>
        /// <param name="cspaceType">CSpace Type</param>
        /// <param name="newOSpaceType">OSpace type created based on CLR <paramref name="type"/> and <paramref name="cspaceType"/></param>
        /// <returns><c>true</c> if the type was created successfully. Otherwise <c>false</c>.</returns>
        private bool TryCreateStructuralType(Type type, StructuralType cspaceType, out EdmType newOSpaceType)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(cspaceType != null, "cspaceType != null");

            List<Action> referenceResolutionListForCurrentType = new List<Action>();
            newOSpaceType = null;
            Debug.Assert(TypesMatchByConvention(type, cspaceType), "The types passed as parameters don't match by convention.");

            StructuralType ospaceType;
            if (Helper.IsEntityType(cspaceType))
            {
                ospaceType = new ClrEntityType(type, cspaceType.NamespaceName, cspaceType.Name);
            }
            else
            {
                Debug.Assert(Helper.IsComplexType(cspaceType), "Invalid type attribute encountered");
                ospaceType = new ClrComplexType(type, cspaceType.NamespaceName, cspaceType.Name);
            }

            if (cspaceType.BaseType != null)
            {
                if (TypesMatchByConvention(type.BaseType, cspaceType.BaseType))
                {
                    TrackClosure(type.BaseType);
                    referenceResolutionListForCurrentType.Add(
                        () => ospaceType.BaseType = ResolveBaseType((StructuralType)cspaceType.BaseType, type));
                }
                else
                {
                    string message = Strings.Validator_OSpace_Convention_BaseTypeIncompatible(type.BaseType.FullName, type.FullName, cspaceType.BaseType.FullName);
                    SessionData.LoadMessageLogger.LogLoadMessage(message, cspaceType);
                    return false;
                }
            }

            // Load the properties for this type
            if (!TryCreateMembers(type, (StructuralType)cspaceType, ospaceType, referenceResolutionListForCurrentType))
            {
                return false;
            }

            // Add this to the known type map so we won't try to load it again
            SessionData.TypesInLoading.Add(type.FullName, ospaceType);

            // we only add the referenceResolution to the list unless we structrually matched this type
            foreach (var referenceResolution in referenceResolutionListForCurrentType)
            {
                this._referenceResolutions.Add(referenceResolution);
            }

            newOSpaceType = ospaceType;
            return true;
        }

        /// <summary>
        /// Creates new enum OSpace type built based on CLR <paramref name="enumType"/> and <paramref name="cspaceEnumType"/>
        /// </summary>
        /// <param name="enumType">CLR type to create OSpace type from.</param>
        /// <param name="cspaceEnumType">CSpace type used to get namespace and name for the newly created OSpace type.</param>
        /// <param name="newOSpaceType">
        /// New enum OSpace type built based on CLR <paramref name="enumType"/> and <paramref name="cspaceEnumType"/> or null
        /// if the type could not be built.
        /// </param>
        /// <returns><c>true</c> if the type was built successfully.<c>false</c> otherwise.</returns>
        private bool TryCreateEnumType(Type enumType, EnumType cspaceEnumType, out EdmType newOSpaceType)
        {
            Debug.Assert(enumType != null, "enumType != null");
            Debug.Assert(enumType.IsEnum, "enum type expected");
            Debug.Assert(cspaceEnumType != null, "cspaceEnumType != null");
            Debug.Assert(Helper.IsEnumType(cspaceEnumType), "Enum type expected");
            Debug.Assert(TypesMatchByConvention(enumType, cspaceEnumType), "The types passed as parameters don't match by convention.");

            newOSpaceType = null;

            // Check if the OSpace and CSpace enum type match
            if (!UnderlyingEnumTypesMatch(enumType, cspaceEnumType) || !EnumMembersMatch(enumType, cspaceEnumType))
            {
                return false;
            }

            newOSpaceType = new ClrEnumType(enumType, cspaceEnumType.NamespaceName, cspaceEnumType.Name);
            SessionData.TypesInLoading.Add(enumType.FullName, newOSpaceType);

            return true;
        }

        /// <summary>
        /// Verifies whether underlying types of CLR and EDM types match
        /// </summary>
        /// <param name="enumType">OSpace CLR enum type.</param>
        /// <param name="cspaceEnumType">CSpace EDM enum type.</param>
        /// <returns><c>true</c> if types match. <c>false</c> otherwise.</returns>
        private bool UnderlyingEnumTypesMatch(Type enumType, EnumType cspaceEnumType)
        {
            Debug.Assert(enumType != null, "enumType != null");
            Debug.Assert(enumType.IsEnum, "expected enum OSpace type");
            Debug.Assert(cspaceEnumType != null, "cspaceEnumType != null");
            Debug.Assert(Helper.IsEnumType(cspaceEnumType), "Enum type expected");

            // Note that TryGetPrimitiveType() will return false not only for types that are not primitive 
            // but also for CLR primitive types that are valid underlying enum types in CLR but are not 
            // a valid Edm primitive types (e.g. ulong) 
            PrimitiveType underlyingEnumType;
            if (!ClrProviderManifest.Instance.TryGetPrimitiveType(enumType.GetEnumUnderlyingType(), out underlyingEnumType))
            {
                SessionData.LoadMessageLogger.LogLoadMessage(
                    Strings.Validator_UnsupportedEnumUnderlyingType(enumType.GetEnumUnderlyingType().FullName), 
                    cspaceEnumType);

                return false;
            }
            else if (underlyingEnumType.PrimitiveTypeKind != cspaceEnumType.UnderlyingType.PrimitiveTypeKind)
            {
                SessionData.LoadMessageLogger.LogLoadMessage(
                    Strings.Validator_OSpace_Convention_NonMatchingUnderlyingTypes, cspaceEnumType);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies whether enum members of CLR and EDM types match.
        /// </summary>
        /// <param name="enumType">OSpace CLR enum type.</param>
        /// <param name="cspaceEnumType">CSpace EDM enum type.</param>
        /// <returns><c>true</c> if members match. <c>false</c> otherwise.</returns>
        private bool EnumMembersMatch(Type enumType, EnumType cspaceEnumType)
        {
            Debug.Assert(enumType != null, "enumType != null");
            Debug.Assert(enumType.IsEnum, "expected enum OSpace type");
            Debug.Assert(cspaceEnumType != null, "cspaceEnumType != null");
            Debug.Assert(Helper.IsEnumType(cspaceEnumType), "Enum type expected");
            Debug.Assert(cspaceEnumType.UnderlyingType.ClrEquivalentType == enumType.GetEnumUnderlyingType(), "underlying types should have already been checked");

            var enumUnderlyingType = enumType.GetEnumUnderlyingType();

            var cspaceSortedEnumMemberEnumerator = cspaceEnumType.Members.OrderBy(m => m.Name).GetEnumerator();
            var ospaceSortedEnumMemberNamesEnumerator = enumType.GetEnumNames().OrderBy(n => n).GetEnumerator();

            // no checks required if edm enum type does not have any members 
            if (!cspaceSortedEnumMemberEnumerator.MoveNext())
            {
                return true;
            }

            while (ospaceSortedEnumMemberNamesEnumerator.MoveNext())
            {
                if (cspaceSortedEnumMemberEnumerator.Current.Name == ospaceSortedEnumMemberNamesEnumerator.Current &&
                    cspaceSortedEnumMemberEnumerator.Current.Value.Equals(
                        Convert.ChangeType(
                            Enum.Parse(enumType, ospaceSortedEnumMemberNamesEnumerator.Current), enumUnderlyingType, CultureInfo.InvariantCulture)))
                {
                    if (!cspaceSortedEnumMemberEnumerator.MoveNext())
                    {
                        return true;
                    }
                }
            }

            SessionData.LoadMessageLogger.LogLoadMessage(
                System.Data.Entity.Strings.Mapping_Enum_OCMapping_MemberMismatch(
                        enumType.FullName,
                        cspaceSortedEnumMemberEnumerator.Current.Name,
                        cspaceSortedEnumMemberEnumerator.Current.Value,
                        cspaceEnumType.FullName), cspaceEnumType);
                
            return false;
        }

        internal override void OnLevel1SessionProcessing()
        {
            CreateRelationships();

            foreach (Action resolve in _referenceResolutions)
            {
                resolve();
            }

            base.OnLevel1SessionProcessing();
        }

        private EdmType ResolveBaseType(StructuralType baseCSpaceType, Type type)
        {
            EdmType ospaceType;
            bool foundValue = SessionData.CspaceToOspace.TryGetValue(baseCSpaceType, out ospaceType);
            if (!foundValue)
            {
                string message =
                    SessionData.LoadMessageLogger.CreateErrorMessageWithTypeSpecificLoadLogs(
                        Strings.Validator_OSpace_Convention_BaseTypeNotLoaded(type, baseCSpaceType), 
                        baseCSpaceType);
                SessionData.EdmItemErrors.Add(new EdmItemError(message, ospaceType));
            }

            Debug.Assert(!foundValue || ospaceType is StructuralType, "Structural type expected (if found).");

            return ospaceType;
        }

        private bool TryCreateMembers(Type type, StructuralType cspaceType, StructuralType ospaceType, List<Action> referenceResolutionListForCurrentType)
        {
            BindingFlags flags = cspaceType.BaseType == null ? RootEntityPropertyReflectionBindingFlags : PropertyReflectionBindingFlags;

            PropertyInfo[] clrProperties = type.GetProperties(flags);

            // required properties scalar properties first
            if (!TryFindAndCreatePrimitiveProperties(type, cspaceType, ospaceType, clrProperties))
            {
                return false;
            }

            if(!TryFindAndCreateEnumProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
            {
                return false;
            }

            if (!TryFindComplexProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
            {
                return false;
            }

            if (!TryFindNavigationProperties(type, cspaceType, ospaceType, clrProperties, referenceResolutionListForCurrentType))
            {
                return false;
            }

            return true;
        }

        private bool TryFindComplexProperties(Type type, StructuralType cspaceType, StructuralType ospaceType, PropertyInfo[] clrProperties, List<Action> referenceResolutionListForCurrentType)
        {
            List<KeyValuePair<EdmProperty, PropertyInfo>> typeClosureToTrack =
                new List<KeyValuePair<EdmProperty, PropertyInfo>>();
            foreach (EdmProperty cspaceProperty in cspaceType.GetDeclaredOnlyMembers<EdmProperty>().Where(m => Helper.IsComplexType(m.TypeUsage.EdmType)))
            {
                PropertyInfo clrProperty = clrProperties.FirstOrDefault(p => MemberMatchesByConvention(p, cspaceProperty));
                if (clrProperty != null)
                {
                    typeClosureToTrack.Add(
                        new KeyValuePair<EdmProperty, PropertyInfo>(
                            cspaceProperty, clrProperty));
                }
                else
                {
                    string message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
                    SessionData.LoadMessageLogger.LogLoadMessage(message, cspaceType);
                    return false;
                }
            }

            foreach (var typeToTrack in typeClosureToTrack)
            {
                TrackClosure(typeToTrack.Value.PropertyType);
                // prevent the lifting of these closure variables
                var ot = ospaceType;
                var cp = typeToTrack.Key;
                var clrp = typeToTrack.Value;
                referenceResolutionListForCurrentType.Add(() => CreateAndAddComplexType(type, ot, cp, clrp));
            }

            return true;
        }

        private bool TryFindNavigationProperties(Type type, StructuralType cspaceType, StructuralType ospaceType, PropertyInfo[] clrProperties, List<Action> referenceResolutionListForCurrentType)
        {
            List<KeyValuePair<NavigationProperty, PropertyInfo>> typeClosureToTrack =
                new List<KeyValuePair<NavigationProperty, PropertyInfo>>();
            foreach (NavigationProperty cspaceProperty in cspaceType.GetDeclaredOnlyMembers<NavigationProperty>())
            {
                PropertyInfo clrProperty = clrProperties.FirstOrDefault(p => NonPrimitiveMemberMatchesByConvention(p, cspaceProperty));
                if (clrProperty != null)
                {
                    bool needsSetter = cspaceProperty.ToEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many;
                    if (clrProperty.CanRead && (!needsSetter || clrProperty.CanWrite))
                    {
                        typeClosureToTrack.Add(
                            new KeyValuePair<NavigationProperty, PropertyInfo>(
                                cspaceProperty, clrProperty));
                    }
                }
                else
                {
                    string message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(
                        cspaceProperty.Name, type.FullName);
                    SessionData.LoadMessageLogger.LogLoadMessage(message, cspaceType);
                    return false;
                }
            }
            
            foreach (var typeToTrack in typeClosureToTrack)
            {
                TrackClosure(typeToTrack.Value.PropertyType);

                // keep from lifting these closure variables
                var ct = cspaceType;
                var ot = ospaceType;
                var cp = typeToTrack.Key;
                var clrp = typeToTrack.Value;

                referenceResolutionListForCurrentType.Add(() => CreateAndAddNavigationProperty(ct, ot, cp, clrp));
            }

            return true;
        }

        private void TrackClosure(Type type)
        {

            if (SourceAssembly != type.Assembly && 
                !CacheEntry.ClosureAssemblies.Contains(type.Assembly) &&
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

        private void CreateAndAddComplexType(Type type, StructuralType ospaceType, EdmProperty cspaceProperty, PropertyInfo clrProperty)
        {
            EdmType propertyType;
            if (SessionData.CspaceToOspace.TryGetValue((StructuralType)cspaceProperty.TypeUsage.EdmType, out propertyType))
            {
                Debug.Assert(propertyType is StructuralType, "Structural type expected.");

                EdmProperty property = new EdmProperty(cspaceProperty.Name, TypeUsage.Create(propertyType, new FacetValues { Nullable = false }), clrProperty, type.TypeHandle);
                ospaceType.AddMember(property);
            }
            else
            {
                string message =
                    SessionData.LoadMessageLogger.CreateErrorMessageWithTypeSpecificLoadLogs(
                        Strings.Validator_OSpace_Convention_MissingOSpaceType(cspaceProperty.TypeUsage.EdmType.FullName),
                        cspaceProperty.TypeUsage.EdmType);
                SessionData.EdmItemErrors.Add(new EdmItemError(message, ospaceType));
            }

        }

        private void CreateAndAddNavigationProperty(StructuralType cspaceType, StructuralType ospaceType, NavigationProperty cspaceProperty, PropertyInfo clrProperty)
        {
            EdmType ospaceRelationship;
            if (SessionData.CspaceToOspace.TryGetValue(cspaceProperty.RelationshipType, out ospaceRelationship))
            {
                Debug.Assert(ospaceRelationship is StructuralType, "Structural type expected.");

                bool foundTarget = false;
                EdmType targetType = null;
                if (Helper.IsCollectionType(cspaceProperty.TypeUsage.EdmType))
                {
                    EdmType findType;
                    foundTarget = SessionData.CspaceToOspace.TryGetValue((StructuralType)((CollectionType)cspaceProperty.TypeUsage.EdmType).TypeUsage.EdmType, out findType);
                    if (foundTarget)
                    {
                        Debug.Assert(findType is StructuralType, "Structural type expected.");

                        targetType = findType.GetCollectionType();
                    }
                }
                else
                {
                    EdmType findType;
                    foundTarget = SessionData.CspaceToOspace.TryGetValue((StructuralType)cspaceProperty.TypeUsage.EdmType, out findType);
                    if (foundTarget)
                    {
                        Debug.Assert(findType is StructuralType, "Structural type expected.");

                        targetType = findType;
                    }
                }


                Debug.Assert(foundTarget, "Since the relationship will only be created if it can find the types for both ends, we will never fail to find one of the ends");

                NavigationProperty navigationProperty = new NavigationProperty(cspaceProperty.Name, TypeUsage.Create(targetType), clrProperty);
                navigationProperty.RelationshipType = (RelationshipType)ospaceRelationship;

                // we can use First because o-space relationships are created directly from 
                // c-space relationship
                navigationProperty.ToEndMember = (RelationshipEndMember)((RelationshipType)ospaceRelationship).Members.First(e => e.Name == cspaceProperty.ToEndMember.Name);
                navigationProperty.FromEndMember = (RelationshipEndMember)((RelationshipType)ospaceRelationship).Members.First(e => e.Name == cspaceProperty.FromEndMember.Name);
                ospaceType.AddMember(navigationProperty);
            }
            else
            {
                EntityTypeBase missingType = cspaceProperty.RelationshipType.RelationshipEndMembers.Select(e => ((RefType)e.TypeUsage.EdmType).ElementType).First(e => e != cspaceType);
                string message =
                    SessionData.LoadMessageLogger.CreateErrorMessageWithTypeSpecificLoadLogs(
                        Strings.Validator_OSpace_Convention_RelationshipNotLoaded(cspaceProperty.RelationshipType.FullName, missingType.FullName),
                        missingType);
                SessionData.EdmItemErrors.Add(new EdmItemError(message, ospaceType));
            }
        }

        private bool TryFindAndCreatePrimitiveProperties(Type type, StructuralType cspaceType, StructuralType ospaceType, PropertyInfo[] clrProperties)
        {
            foreach (EdmProperty cspaceProperty in cspaceType.GetDeclaredOnlyMembers<EdmProperty>().Where(p => Helper.IsPrimitiveType(p.TypeUsage.EdmType)))
            {
                PropertyInfo clrProperty = clrProperties.FirstOrDefault(p => MemberMatchesByConvention(p, cspaceProperty));
                if (clrProperty != null)
                {
                    PrimitiveType propertyType;
                    if (TryGetPrimitiveType(clrProperty.PropertyType, out propertyType))
                    {
                        if (clrProperty.CanRead && clrProperty.CanWrite)
                        {
                            AddScalarMember(type, clrProperty, ospaceType, cspaceProperty, propertyType);
                        }
                        else
                        {
                            string message = Strings.Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter(clrProperty.Name, type.FullName, type.Assembly.FullName);
                            SessionData.LoadMessageLogger.LogLoadMessage(message, cspaceType);
                            return false;
                        }
                    }
                    else
                    {
                        string message = Strings.Validator_OSpace_Convention_NonPrimitiveTypeProperty(clrProperty.Name, type.FullName, clrProperty.PropertyType.FullName);
                        SessionData.LoadMessageLogger.LogLoadMessage(message, cspaceType);
                        return false;
                    }
                }
                else
                {
                    string message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
                    SessionData.LoadMessageLogger.LogLoadMessage(message, cspaceType);
                    return false;
                }
            }
            return true;
        }

        private bool TryFindAndCreateEnumProperties(Type type, StructuralType cspaceType, StructuralType ospaceType, PropertyInfo[] clrProperties, List<Action> referenceResolutionListForCurrentType)
        {
            var typeClosureToTrack = new List<KeyValuePair<EdmProperty, PropertyInfo>>();

            foreach (EdmProperty cspaceProperty in cspaceType.GetDeclaredOnlyMembers<EdmProperty>().Where(p => Helper.IsEnumType(p.TypeUsage.EdmType)))
            {
                PropertyInfo clrProperty = clrProperties.FirstOrDefault(p => MemberMatchesByConvention(p, cspaceProperty));
                if (clrProperty != null)
                {
                    typeClosureToTrack.Add(new KeyValuePair<EdmProperty, PropertyInfo>(cspaceProperty, clrProperty));
                }
                else
                {
                    string message = Strings.Validator_OSpace_Convention_MissingRequiredProperty(cspaceProperty.Name, type.FullName);
                    SessionData.LoadMessageLogger.LogLoadMessage(message, cspaceType);
                    return false;
                }
            }

            foreach (var typeToTrack in typeClosureToTrack)
            {
                TrackClosure(typeToTrack.Value.PropertyType);
                // prevent the lifting of these closure variables
                var ot = ospaceType;
                var cp = typeToTrack.Key;
                var clrp = typeToTrack.Value;
                referenceResolutionListForCurrentType.Add(() => CreateAndAddEnumProperty(type, ot, cp, clrp));
            }

            return true;
        }

        /// <summary>
        /// Creates an Enum property based on <paramref name="clrProperty"/>and adds it to the parent structural type.
        /// </summary>
        /// <param name="type">CLR type owning <paramref name="clrProperty"/>.</param>
        /// <param name="ospaceType">OSpace type the created property will be added to.</param>
        /// <param name="cspaceProperty">Corresponding property from CSpace.</param>
        /// <param name="clrProperty">CLR property used to build an Enum property.</param>
        private void CreateAndAddEnumProperty(Type type, StructuralType ospaceType, EdmProperty cspaceProperty, PropertyInfo clrProperty)
        {
            EdmType propertyType;
            if (SessionData.CspaceToOspace.TryGetValue(cspaceProperty.TypeUsage.EdmType, out propertyType))
            {
                if (clrProperty.CanRead && clrProperty.CanWrite)
                {
                    AddScalarMember(type, clrProperty, ospaceType, cspaceProperty, propertyType);
                }
                else
                {
                    string message =
                        SessionData.LoadMessageLogger.CreateErrorMessageWithTypeSpecificLoadLogs(
                            Strings.Validator_OSpace_Convention_ScalarPropertyMissginGetterOrSetter(clrProperty.Name, type.FullName, type.Assembly.FullName),
                            cspaceProperty.TypeUsage.EdmType);

                    SessionData.EdmItemErrors.Add(new EdmItemError(message, ospaceType));
                }
            }
            else
            {
                string message =
                    SessionData.LoadMessageLogger.CreateErrorMessageWithTypeSpecificLoadLogs(
                        Strings.Validator_OSpace_Convention_MissingOSpaceType(cspaceProperty.TypeUsage.EdmType.FullName),
                        cspaceProperty.TypeUsage.EdmType);

                SessionData.EdmItemErrors.Add(new EdmItemError(message, ospaceType));
            }
        }

        private void CreateRelationships()
        {
            if (SessionData.ConventionBasedRelationshipsAreLoaded)
            {
                return;                
            }
            
            
            SessionData.ConventionBasedRelationshipsAreLoaded = true;

            // find all the relationships
            foreach (AssociationType cspaceAssociation in SessionData.EdmItemCollection.GetItems<AssociationType>())
            {
                Debug.Assert(cspaceAssociation.RelationshipEndMembers.Count == 2, "Relationships are assumed to have exactly two ends");

                if (SessionData.CspaceToOspace.ContainsKey(cspaceAssociation))
                {
                    // don't try to load relationships that we already know about
                    continue;
                }

                EdmType[] ospaceEndTypes = new EdmType[2];
                if (SessionData.CspaceToOspace.TryGetValue(GetRelationshipEndType(cspaceAssociation.RelationshipEndMembers[0]), out ospaceEndTypes[0]) &&
                    SessionData.CspaceToOspace.TryGetValue(GetRelationshipEndType(cspaceAssociation.RelationshipEndMembers[1]), out ospaceEndTypes[1]))
                {
                    Debug.Assert(ospaceEndTypes[0] is StructuralType);
                    Debug.Assert(ospaceEndTypes[1] is StructuralType);

                    // if we can find both ends of the relationship, then create it

                    AssociationType ospaceAssociation = new AssociationType(cspaceAssociation.Name, cspaceAssociation.NamespaceName, cspaceAssociation.IsForeignKey, DataSpace.OSpace);
                    for (int i = 0; i < cspaceAssociation.RelationshipEndMembers.Count; i++)
                    {
                        EntityType ospaceEndType = (EntityType)ospaceEndTypes[i];
                        RelationshipEndMember cspaceEnd = cspaceAssociation.RelationshipEndMembers[i];

                        ospaceAssociation.AddKeyMember(new AssociationEndMember(cspaceEnd.Name, ospaceEndType.GetReferenceType(), cspaceEnd.RelationshipMultiplicity));
                    }
                    CacheEntry.TypesInAssembly.Add(ospaceAssociation);
                    SessionData.TypesInLoading.Add(ospaceAssociation.FullName, ospaceAssociation);
                    SessionData.CspaceToOspace.Add(cspaceAssociation, ospaceAssociation); 

                }
            }
        }

        private static StructuralType GetRelationshipEndType(RelationshipEndMember relationshipEndMember)
        {
            return ((RefType)relationshipEndMember.TypeUsage.EdmType).ElementType;
        }

        private static bool MemberMatchesByConvention(PropertyInfo clrProperty, EdmMember cspaceMember)
        {
            return clrProperty.Name == cspaceMember.Name;
        }

        private static bool NonPrimitiveMemberMatchesByConvention(PropertyInfo clrProperty, EdmMember cspaceMember)
        {
            return !clrProperty.PropertyType.IsValueType && !clrProperty.PropertyType.IsAssignableFrom(typeof(string)) && clrProperty.Name == cspaceMember.Name;
        }

        internal static bool SessionContainsConventionParameters(ObjectItemLoadingSessionData sessionData)
        {
            return sessionData.EdmItemCollection != null;
        }

        internal static bool TypesMatchByConvention(Type type, EdmType cspaceType)
        {
            return type.Name == cspaceType.Name;
        }

        private void AddScalarMember(Type type, PropertyInfo clrProperty, StructuralType ospaceType, EdmProperty cspaceProperty, EdmType propertyType)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(clrProperty != null, "clrProperty != null");
            Debug.Assert(clrProperty.CanRead && clrProperty.CanWrite, "The clr property has to have a setter and a getter.");
            Debug.Assert(ospaceType != null, "ospaceType != null");
            Debug.Assert(cspaceProperty != null, "cspaceProperty != null");
            Debug.Assert(propertyType != null, "propertyType != null");
            Debug.Assert(Helper.IsScalarType(propertyType), "Property has to be primitive or enum.");

            var cspaceType = cspaceProperty.DeclaringType;

            bool isKeyMember = Helper.IsEntityType(cspaceType) && ((EntityType)cspaceType).KeyMemberNames.Contains(clrProperty.Name);

            // the property is nullable only if it is not a key and can actually be set to null (i.e. is not a value type or is a nullable value type)
            bool nullableFacetValue = !isKeyMember && (!clrProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(clrProperty.PropertyType) != null);

            EdmProperty ospaceProperty =
                new EdmProperty(
                    cspaceProperty.Name,
                    TypeUsage.Create(propertyType, new FacetValues { Nullable = nullableFacetValue }),
                    clrProperty,
                    type.TypeHandle);

            if (isKeyMember)
            {
                ((EntityType)ospaceType).AddKeyMember(ospaceProperty);
            }
            else
            {
                ospaceType.AddMember(ospaceProperty);
            }
        }
        
        internal static ObjectItemAssemblyLoader Create(Assembly assembly, ObjectItemLoadingSessionData sessionData)
        {
            if (!ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(assembly))
            {
                return new ObjectItemConventionAssemblyLoader(assembly, sessionData);
            }
            else
            {
                // we were loading in convention mode, and ran into an assembly that can't be loaded by convention
                sessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_Convention_AttributeAssemblyReferenced(assembly.FullName), null));
                return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
            }
        }
    }
}
