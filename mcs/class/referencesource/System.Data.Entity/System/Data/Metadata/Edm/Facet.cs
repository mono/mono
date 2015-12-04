//---------------------------------------------------------------------
// <copyright file="Facet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Class for representing a Facet object
    /// This object is Immutable (not just set to readonly) and 
    /// some parts of the system are depending on that behavior
    /// </summary>
    [DebuggerDisplay("{Name,nq}={Value}")]
    public sealed class Facet : MetadataItem
    {
        #region Constructors
        
        /// <summary>
        /// The constructor for constructing a Facet object with the facet description and a value
        /// </summary>
        /// <param name="facetDescription">The object describing this facet</param>
        /// <param name="value">The value of the facet</param>
        /// <exception cref="System.ArgumentNullException">Thrown if facetDescription argument is null</exception>
        private Facet(FacetDescription facetDescription, object value)
            :base(MetadataFlags.Readonly)
        {
            EntityUtil.GenericCheckArgumentNull(facetDescription, "facetDescription");

            _facetDescription = facetDescription;
            _value = value;
        }
        
        /// <summary>
        /// Creates a Facet instance with the specified value for the given 
        /// facet description.
        /// </summary>
        /// <param name="facetDescription">The object describing this facet</param>
        /// <param name="value">The value of the facet</param>
        /// <exception cref="System.ArgumentNullException">Thrown if facetDescription argument is null</exception>
        internal static Facet Create(FacetDescription facetDescription, object value)
        {
            return Create(facetDescription, value, false);
        }

        /// <summary>
        /// Creates a Facet instance with the specified value for the given 
        /// facet description.
        /// </summary>
        /// <param name="facetDescription">The object describing this facet</param>
        /// <param name="value">The value of the facet</param>
        /// <param name="bypassKnownValues">true to bypass caching and known values; false otherwise.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if facetDescription argument is null</exception>
        internal static Facet Create(FacetDescription facetDescription, object value, bool bypassKnownValues)
        {
            EntityUtil.CheckArgumentNull(facetDescription, "facetDescription");

            if (!bypassKnownValues)
            {
                // Reuse facets with a null value.
                if (object.ReferenceEquals(value, null))
                {
                    return facetDescription.NullValueFacet;
                }

                // Reuse facets with a default value.
                if (object.Equals(facetDescription.DefaultValue, value))
                {
                    return facetDescription.DefaultValueFacet;
                }

                // Special case boolean facets.
                if (facetDescription.FacetType.Identity == "Edm.Boolean")
                {
                    bool boolValue = (bool)value;
                    return facetDescription.GetBooleanFacet(boolValue);
                }
            }

            Facet result = new Facet(facetDescription, value);

            // Check the type of the value only if we know what the correct CLR type is
            if (value != null && !Helper.IsUnboundedFacetValue(result) && !Helper.IsVariableFacetValue(result) && result.FacetType.ClrType != null)
            {
                Type valueType = value.GetType();
                Debug.Assert(
                    valueType == result.FacetType.ClrType 
                    || result.FacetType.ClrType.IsAssignableFrom(valueType),
                    string.Format(CultureInfo.CurrentCulture, "The facet {0} has type {1}, but a value of type {2} was supplied.", result.Name, result.FacetType.ClrType, valueType)
                );
            }

            return result;
        }
        
        #endregion

        #region Fields
        
        /// <summary>The object describing this facet.</summary>
        private readonly FacetDescription _facetDescription;
        
        /// <summary>The value assigned to this facet.</summary>
        private readonly object _value;
        
        #endregion

        #region Properties

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.Facet; } }

        /// <summary>
        /// Gets the description object for describing the facet
        /// </summary>
        public FacetDescription Description
        {
            get
            {
                return _facetDescription;
            }
        }

        /// <summary>
        /// Gets/Sets the name of the facet
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public String Name
        {
            get
            {
                return _facetDescription.FacetName;
            }
        }

        /// <summary>
        /// Gets/Sets the type of the facet
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EdmType, false)]
        public EdmType FacetType
        {
            get
            {
                return _facetDescription.FacetType;
            }
        }

        /// <summary>
        /// Gets/Sets the value of the facet
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the Facet instance is in ReadOnly state</exception>
        [MetadataProperty(typeof(Object), false)]
        public Object Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Gets the identity for this item as a string
        /// </summary>
        internal override string Identity
        {
            get
            {
                return _facetDescription.FacetName;
            }
        }

        /// <summary>
        /// Indicates whether the value of the facet is unbounded
        /// </summary>
        public bool IsUnbounded
        {
            get
            {
                return object.ReferenceEquals(Value, EdmConstants.UnboundedValue);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation 
        /// for this type.
        /// </summary>
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }
}
