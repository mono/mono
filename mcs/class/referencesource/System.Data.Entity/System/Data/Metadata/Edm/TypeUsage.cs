//---------------------------------------------------------------------
// <copyright file="TypeUsage.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Class representing a type information for an item
    /// </summary>
    [DebuggerDisplay("EdmType={EdmType}, Facets.Count={Facets.Count}")]
    public sealed class TypeUsage : MetadataItem
    {
        #region Constructors

        /// <summary>
        /// The constructor for TypeUsage taking in a type
        /// </summary>
        /// <param name="edmType">The type which the TypeUsage object describes</param>
        /// <exception cref="System.ArgumentNullException">Thrown if edmType argument is null</exception>
        private TypeUsage(EdmType edmType)
        :base(MetadataFlags.Readonly)
        {
            EntityUtil.GenericCheckArgumentNull(edmType, "edmType");

            _edmType = edmType;

            // I would like to be able to assert that the edmType is ReadOnly, but
            // because some types are still in loading while the TypeUsage is being created
            // that won't work. We should consider a way to change this
        }

        /// <summary>
        /// The constructor for TypeUsage taking in a type and a collection of facets
        /// </summary>
        /// <param name="edmType">The type which the TypeUsage object describes</param>
        /// <param name="facets">The replacement collection of facets</param>
        /// <exception cref="System.ArgumentNullException">Thrown if edmType argument is null</exception>
        private TypeUsage(EdmType edmType, IEnumerable<Facet> facets)
            : this(edmType)
        {
            MetadataCollection<Facet> facetCollection = new MetadataCollection<Facet>(facets);
            facetCollection.SetReadOnly();
            _facets = facetCollection.AsReadOnlyMetadataCollection();
        }
        #endregion

        #region Factory Methods
        /// <summary>
        /// Factory method for creating a TypeUsage with specified EdmType
        /// </summary>
        /// <param name="edmType">EdmType for which to create a type usage</param>
        /// <returns>new TypeUsage instance with default facet values</returns>
        internal static TypeUsage Create(EdmType edmType)
        {
            return new TypeUsage(edmType);
        }

        /// <summary>
        /// Factory method for creating a TypeUsage with specified EdmType
        /// </summary>
        /// <param name="edmType">EdmType for which to create a type usage</param>
        /// <returns>new TypeUsage instance with default facet values</returns>
        internal static TypeUsage Create(EdmType edmType, FacetValues values)
        {
            return new TypeUsage(edmType,
                GetDefaultFacetDescriptionsAndOverrideFacetValues(edmType, values));
        }

        /// <summary>
        /// Factory method for creating a TypeUsage with specified EdmType and facets
        /// </summary>
        /// <param name="edmType">EdmType for which to create a type usage</param>
        /// <param name="facets">facets to be copied into the new TypeUsage</param>
        /// <returns>new TypeUsage instance</returns>
        internal static TypeUsage Create(EdmType edmType, IEnumerable<Facet> facets)
        {
            return new TypeUsage(edmType, facets);
        }

        internal TypeUsage ShallowCopy(FacetValues facetValues)
        {
            return TypeUsage.Create(_edmType, OverrideFacetValues(Facets, facetValues));
        }

        /// <summary>
        /// Factory method for creating a "readonly" TypeUsage with specified EdmType
        /// </summary>
        /// <param name="edmType">An EdmType for which to create a TypeUsage</param>
        /// <returns>A TypeUsage instance with default facet values for the specified EdmType</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#edm")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public static TypeUsage CreateDefaultTypeUsage(EdmType edmType)
        {
            EntityUtil.CheckArgumentNull<EdmType>(edmType, "edmType");
            
            TypeUsage type = TypeUsage.Create(edmType);
            return type;
        }

        /// <summary>
        /// Factory method for creating a string TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType">A PrimitiveType for which to construct the TypeUsage</param>
        /// <param name="isUnicode">Whether the string type is unicode or not</param>
        /// <param name="isFixedLength">Whether the string type is fixed length or not</param>
        /// <param name="maxLength">The max length of the string type</param>
        /// <returns>A TypeUsage object describing a string type with the given facet values</returns>
        public static TypeUsage CreateStringTypeUsage(PrimitiveType primitiveType,
                                                      bool isUnicode,
                                                      bool isFixedLength,
                                                      int maxLength)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.String)
            {
                throw EntityUtil.NotStringTypeForTypeUsage();
            }

            ValidateMaxLength(maxLength);

            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{ MaxLength = maxLength, Unicode = isUnicode, FixedLength = isFixedLength});

            return typeUsage;
        }

        /// <summary>
        /// Factory method for creating a string TypeUsage object with the specified facets and 
        /// unbounded MaxLength
        /// </summary>
        /// <param name="primitiveType">A PrimitiveType for which to construct the TypeUsage</param>
        /// <param name="isUnicode">Whether the string type is unicode or not</param>
        /// <param name="isFixedLength">Whether the string type is fixed length or not</param>
        /// <returns>A TypeUsage object describing a string type with the given facet values
        /// and unbounded MaxLength</returns>
        public static TypeUsage CreateStringTypeUsage(PrimitiveType primitiveType,
                                                      bool isUnicode,
                                                      bool isFixedLength)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.String)
            {
                throw EntityUtil.NotStringTypeForTypeUsage();
            }
            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{ MaxLength = TypeUsage.DefaultMaxLengthFacetValue,
                                  Unicode = isUnicode, FixedLength = isFixedLength});

            return typeUsage;
        }


        /// <summary>
        /// Factory method for creating a Binary TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType">A PrimitiveType for which to construct TypeUsage</param>
        /// <param name="isFixedLength">Whether the binary type is fixed length or not</param>
        /// <param name="maxLength">The max length of the binary type</param>
        /// <returns>A TypeUsage object describing a binary type with the given facet values</returns>
        public static TypeUsage CreateBinaryTypeUsage(PrimitiveType primitiveType,
                                                      bool isFixedLength,
                                                      int maxLength)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Binary)
            {
                throw EntityUtil.NotBinaryTypeForTypeUsage();
            }

            ValidateMaxLength(maxLength);

            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{MaxLength = maxLength, FixedLength = isFixedLength});

            return typeUsage;
        }

        /// <summary>
        /// Factory method for creating a Binary TypeUsage object with the specified facets and 
        /// unbounded MaxLength
        /// </summary>
        /// <param name="primitiveType">A PrimitiveType for which to construct the TypeUsage</param>
        /// <param name="isFixedLength">Whether the binary type is fixed length or not</param>
        /// <returns>A TypeUsage object describing a binary type with the given facet values</returns>
        public static TypeUsage CreateBinaryTypeUsage(PrimitiveType primitiveType, bool isFixedLength)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Binary)
            {
                throw EntityUtil.NotBinaryTypeForTypeUsage();
            }
            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{MaxLength = TypeUsage.DefaultMaxLengthFacetValue,
                                 FixedLength = isFixedLength});

            return typeUsage;
        }

        /// <summary>
        /// Factory method for creating a DateTime TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType">A PrimitiveType for which to construct the TypeUsage</param>
        /// <param name="precision">Precision for seconds</param>
        /// <returns>A TypeUsage object describing a DateTime type with the given facet values</returns>
        public static TypeUsage CreateDateTimeTypeUsage(PrimitiveType primitiveType,
                                                        byte? precision)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.DateTime)
            {
                throw EntityUtil.NotDateTimeTypeForTypeUsage();
            }
            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{Precision = precision});

            return typeUsage;
        }

        /// <summary>
        /// Factory method for creating a DateTimeOffset TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType">A PrimitiveType for which to construct the TypeUsage</param>
        /// <param name="precision">Precision for seconds</param>
        /// <returns>A TypeUsage object describing a DateTime type with the given facet values</returns>
        public static TypeUsage CreateDateTimeOffsetTypeUsage(PrimitiveType primitiveType,
                                                        byte? precision)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.DateTimeOffset)
            {
                throw EntityUtil.NotDateTimeOffsetTypeForTypeUsage();
            }

            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{ Precision = precision });

            return typeUsage;
        }

        /// <summary>
        /// Factory method for creating a Time TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType">A PrimitiveType for which to construct the TypeUsage</param>
        /// <param name="precision">Precision for seconds</param>
        /// <returns>A TypeUsage object describing a Time type with the given facet values</returns>
        public static TypeUsage CreateTimeTypeUsage(PrimitiveType primitiveType,
                                                        byte? precision)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Time)
            {
                throw EntityUtil.NotTimeTypeForTypeUsage();
            }
            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{ Precision = precision });

            return typeUsage;
        }



        /// <summary>
        /// Factory method for creating a Decimal TypeUsage object with the specified facets
        /// </summary>
        /// <param name="primitiveType">A PrimitiveType for which to construct type usage</param>
        /// <param name="precision">The precision of the decimal type</param>
        /// <param name="scale">The scale of the decimal type</param>
        /// <returns>A TypeUsage object describing a decimal type with the given facet values</returns>
        public static TypeUsage CreateDecimalTypeUsage(PrimitiveType primitiveType,
                                                       byte precision,
                                                       byte scale)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
            {
                throw EntityUtil.NotDecimalTypeForTypeUsage();
            }

            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{Precision = precision, Scale = scale });

            return typeUsage;
        }

        /// <summary>
        /// Factory method for creating a Decimal TypeUsage object with unbounded precision and scale
        /// </summary>
        /// <param name="primitiveType">The PrimitiveType for which to construct type usage</param>
        /// <returns>A TypeUsage object describing a decimal type with unbounded precision and scale</returns>
        public static TypeUsage CreateDecimalTypeUsage(PrimitiveType primitiveType)
        {
            EntityUtil.CheckArgumentNull<PrimitiveType>(primitiveType, "primitiveType");

            if (primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
            {
                throw EntityUtil.NotDecimalTypeForTypeUsage();
            }
            TypeUsage typeUsage = TypeUsage.Create(primitiveType,
                new FacetValues{ Precision = TypeUsage.DefaultPrecisionFacetValue, Scale = TypeUsage.DefaultScaleFacetValue });

            return typeUsage;
        }
        #endregion

        #region Fields
        private TypeUsage _modelTypeUsage;
        private readonly EdmType _edmType;
        private ReadOnlyMetadataCollection<Facet> _facets;
        private string _identity;

        /// <summary>
        /// Set of facets that should be included in identity for TypeUsage
        /// </summary>
        /// <remarks>keep this sorted for binary searching</remarks>
        private static readonly string[] s_identityFacets = new string[] { 
            DbProviderManifest.DefaultValueFacetName,
            DbProviderManifest.FixedLengthFacetName,
            DbProviderManifest.MaxLengthFacetName,
            DbProviderManifest.NullableFacetName,
            DbProviderManifest.PrecisionFacetName,
            DbProviderManifest.ScaleFacetName,
            DbProviderManifest.UnicodeFacetName,
            DbProviderManifest.SridFacetName,
        };

        internal static readonly EdmConstants.Unbounded DefaultMaxLengthFacetValue       = EdmConstants.UnboundedValue;
        internal static readonly EdmConstants.Unbounded DefaultPrecisionFacetValue       = EdmConstants.UnboundedValue;
        internal static readonly EdmConstants.Unbounded DefaultScaleFacetValue           = EdmConstants.UnboundedValue;
        internal static readonly bool                   DefaultUnicodeFacetValue         = true;
        internal static readonly bool                   DefaultFixedLengthFacetValue     = false;
        internal static readonly byte?                  DefaultDateTimePrecisionFacetValue = null;

        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.TypeUsage; } }

        /// <summary>
        /// Gets the type that this TypeUsage describes
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [MetadataProperty(BuiltInTypeKind.EdmType, false)]
        public EdmType EdmType
        {
            get
            {
                return _edmType;
            }
        }

        /// <summary>
        /// Gets the list of facets for the type in this TypeUsage
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.Facet, true)]
        public ReadOnlyMetadataCollection<Facet> Facets
        {
            get
            {
                if (null == _facets)
                {
                    MetadataCollection<Facet> facets = new MetadataCollection<Facet>(GetFacets());
                    // we never modify the collection so we can set it readonly from the start
                    facets.SetReadOnly();
                    System.Threading.Interlocked.CompareExchange(ref _facets, facets.AsReadOnlyMetadataCollection(), null);
                }
                return _facets;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a Model type usage for a provider type
        /// </summary>
        /// <returns>model (CSpace) type usage</returns>
        internal TypeUsage GetModelTypeUsage()
        {
            if (_modelTypeUsage == null)
            {
                EdmType edmType = this.EdmType;

                // If the edm type is already a cspace type, return the same type
                if (edmType.DataSpace == DataSpace.CSpace || edmType.DataSpace == DataSpace.OSpace)
                {
                    return this;
                }

                TypeUsage result;
                if (Helper.IsRowType(edmType))
                {
                    RowType sspaceRowType = (RowType)edmType;
                    EdmProperty[] properties = new EdmProperty[sspaceRowType.Properties.Count];
                    for (int i = 0; i < properties.Length; i++)
                    {
                        EdmProperty sspaceProperty = sspaceRowType.Properties[i];
                        TypeUsage newTypeUsage = sspaceProperty.TypeUsage.GetModelTypeUsage();
                        properties[i] = new EdmProperty(sspaceProperty.Name, newTypeUsage);
                    }
                    RowType edmRowType = new RowType(properties, sspaceRowType.InitializerMetadata);
                    result = TypeUsage.Create(edmRowType, this.Facets);
                }
                else if (Helper.IsCollectionType(edmType))
                {
                    CollectionType sspaceCollectionType = ((CollectionType)edmType);
                    TypeUsage newTypeUsage = sspaceCollectionType.TypeUsage.GetModelTypeUsage();
                    result = TypeUsage.Create(new CollectionType(newTypeUsage), this.Facets);
                }
                else if (Helper.IsRefType(edmType))
                {
                    System.Diagnostics.Debug.Assert(((RefType)edmType).ElementType.DataSpace == DataSpace.CSpace);
                    result = this;
                }
                else if (Helper.IsPrimitiveType(edmType))
                {
                    result = ((PrimitiveType)edmType).ProviderManifest.GetEdmType(this);

                    if (result == null)
                    {
                        throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.Mapping_ProviderReturnsNullType(this.ToString()));
                    }

                    if (!TypeSemantics.IsNullable(this))
                    {
                        result = TypeUsage.Create(result.EdmType,
                            OverrideFacetValues(result.Facets,
                                new FacetValues{ Nullable = false }));        
                    }
                }
                else if (Helper.IsEntityTypeBase(edmType) || Helper.IsComplexType(edmType))
                {
                    result = this;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false, "Unexpected type found in entity data reader");
                    return null;
                }
                System.Threading.Interlocked.CompareExchange(ref _modelTypeUsage, result, null);
            }
            return _modelTypeUsage;
        }

        /// <summary>
        /// check if "this" is a subtype of the specified TypeUsage
        /// </summary>
        /// <param name="typeUsage">The typeUsage to be checked</param>
        /// <returns>true if this typeUsage is a subtype of the specified typeUsage</returns>
        public bool IsSubtypeOf(TypeUsage typeUsage)
        {
            if (EdmType == null || typeUsage == null)
            {
                return false;
            }

            return EdmType.IsSubtypeOf(typeUsage.EdmType);
        }

        private IEnumerable<Facet> GetFacets()
        {
            foreach (FacetDescription facetDescription in _edmType.GetAssociatedFacetDescriptions())
            {
                yield return facetDescription.DefaultValueFacet;
            }
        }

        internal override void SetReadOnly()
        {
            Debug.Fail("TypeUsage.SetReadOnly should not need to ever be called");
            base.SetReadOnly();
        }

        /// <summary>
        /// returns the identity of the type usage
        /// </summary>
        internal override String Identity
        {
            get
            {
                if (this.Facets.Count == 0)
                {
                    return this.EdmType.Identity;
                }

                if (this._identity == null)
                {
                    StringBuilder builder = new StringBuilder(128);
                    BuildIdentity(builder);
                    string identity = builder.ToString();
                    System.Threading.Interlocked.CompareExchange(ref _identity, identity, null);
                }
                return this._identity;
            }
        }
        
        private static IEnumerable<Facet> GetDefaultFacetDescriptionsAndOverrideFacetValues(EdmType type, FacetValues values)
        {
            return OverrideFacetValues(type.GetAssociatedFacetDescriptions(),
                fd => fd,
                fd => fd.DefaultValueFacet,
                values);
        }


        private static IEnumerable<Facet> OverrideFacetValues(IEnumerable<Facet> facets, FacetValues values)
        {
            return OverrideFacetValues(facets,
                f => f.Description,
                f => f,
                values);
        }

     
        private static IEnumerable<Facet> OverrideFacetValues<T>(IEnumerable<T> facetThings,
                Func<T, FacetDescription> getDescription,
                Func<T, Facet> getFacet,
                FacetValues values)
        {
            // yield all the non custom values
            foreach (var thing in facetThings)
            {
                FacetDescription description = getDescription(thing);
                Facet facet;    
                if (!description.IsConstant && values.TryGetFacet(description, out facet))
                {
                    yield return facet;
                }
                else
                {
                    yield return getFacet(thing);
                }
            }

        }

        internal override void BuildIdentity(StringBuilder builder)
        {
            // if we've already cached the identity, simply append it
            if (null != _identity)
            {
                builder.Append(_identity);
                return;
            }

            builder.Append(this.EdmType.Identity);

            builder.Append("(");
            bool first = true;
            for (int j = 0; j < this.Facets.Count; j++)
            {
                Facet facet = this.Facets[j];
                
                if (0 <= Array.BinarySearch(s_identityFacets, facet.Name, StringComparer.Ordinal))
                {
                    if (first) { first = false; }
                    else { builder.Append(","); }

                    builder.Append(facet.Name);
                    builder.Append("=");
                    // If the facet is present, add its value to the identity
                    // We only include built-in system facets for the identity
                    builder.Append(facet.Value ?? String.Empty);
                }                
            }
            builder.Append(")");
        }

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            return EdmType.ToString();
        }

        /// <summary>
        /// EdmEquals override verifying the equivalence of all facets. Two facets are considered
        /// equal if they have the same name and the same value (Object.Equals)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal override bool EdmEquals(MetadataItem item)
        {
            // short-circuit if this and other are reference equivalent
            if (Object.ReferenceEquals(this, item)) { return true; }

            // check type of item
            if (null == item || BuiltInTypeKind.TypeUsage != item.BuiltInTypeKind) { return false; }
            TypeUsage other = (TypeUsage)item;

            // verify edm types are equivalent
            if (!this.EdmType.EdmEquals(other.EdmType)) { return false; }

            // if both usages have default facets, no need to compare
            if (null == this._facets && null == other._facets) { return true; }

            // initialize facets and compare
            if (this.Facets.Count != other.Facets.Count) { return false; }

            foreach (Facet thisFacet in this.Facets)
            {
                Facet otherFacet;
                if (!other.Facets.TryGetValue(thisFacet.Name, false, out otherFacet))
                {
                    // other type usage doesn't have the same facets as this type usage
                    return false;
                }

                // check that the facet values are the same
                if (!Object.Equals(thisFacet.Value, otherFacet.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateMaxLength(int maxLength)
        {
            if (maxLength <= 0)
            {
                throw EntityUtil.ArgumentOutOfRange(System.Data.Entity.Strings.InvalidMaxLengthSize, "maxLength");
            }
        }

        #endregion

    }
}
