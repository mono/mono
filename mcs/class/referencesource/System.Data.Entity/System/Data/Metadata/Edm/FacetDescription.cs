//---------------------------------------------------------------------
// <copyright file="FacetDescription.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class for representing a FacetDescription object
    /// </summary>
    public sealed class FacetDescription
    {
        #region Constructors



        internal FacetDescription(string facetName,
                                  EdmType facetType,
                                  int? minValue,
                                  int? maxValue,
                                  object defaultValue,
                                  bool isConstant,
                                  string declaringTypeName)
        {
            _facetName = facetName;
            _facetType = facetType;
            _minValue = minValue;
            _maxValue = maxValue;
            
            // this ctor doesn't allow you to set the defaultValue to null
            if (defaultValue != null)
            {
                _defaultValue = defaultValue;
            }
            else
            {
                _defaultValue = _notInitializedSentinel;
            }
            _isConstant = isConstant;
            
            Validate(declaringTypeName);
            if (_isConstant)
            {
                UpdateMinMaxValueForConstant(_facetName, _facetType, ref _minValue, ref _maxValue, _defaultValue);
            }
        }
        
        /// <summary>
        /// The constructor for constructing a facet description object
        /// </summary>
        /// <param name="facetName">The name of this facet</param>
        /// <param name="facetType">The type of this facet</param>
        /// <param name="minValue">The min value for this facet</param>
        /// <param name="maxValue">The max value for this facet</param>
        /// <param name="defaultValue">The default value for this facet</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either facetName, facetType or applicableType arguments are null</exception>
        internal FacetDescription(string facetName,
                                  EdmType facetType,
                                  int? minValue,
                                  int? maxValue,
                                  object defaultValue)
        {
            EntityUtil.CheckStringArgument(facetName, "facetName");
            EntityUtil.GenericCheckArgumentNull(facetType, "facetType");

            if (minValue.HasValue || maxValue.HasValue)
            {
                Debug.Assert(FacetDescription.IsNumericType(facetType), "Min and Max Values can only be specified for numeric facets");

                if (minValue.HasValue && maxValue.HasValue)
                {
                    Debug.Assert(minValue != maxValue, "minValue should not be equal to maxValue");
                }
            }

            _facetName = facetName;
            _facetType = facetType;
            _minValue = minValue;
            _maxValue = maxValue;
            _defaultValue = defaultValue;
        }

        #endregion

        #region Fields
        private readonly string _facetName;
        private readonly EdmType _facetType;
        private readonly int? _minValue;
        private readonly int? _maxValue;
        private readonly object _defaultValue;
        private readonly bool _isConstant;

        /// <summary>A facet with the default value for this description.</summary>
        private Facet _defaultValueFacet;

        /// <summary>A facet with a null value for this description.</summary>
        private Facet _nullValueFacet;

        /// <summary>Type-dependant cache for additional values (possibly null).</summary>
        private Facet[] _valueCache;

        // we need to differentiate when the default value is null vs when the default value is not initialized
        private static object _notInitializedSentinel = new object();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of this facet
        /// </summary>
        public string FacetName
        {
            get
            {
                return _facetName;
            }
        }

        /// <summary>
        /// Gets the type of this facet
        /// </summary>
        public EdmType FacetType
        {
            get
            {
                return _facetType;
            }
        }

        /// <summary>
        /// Gets the lower bound a facet with this facet description can take
        /// </summary>
        public int? MinValue
        {
            get
            {
                return _minValue;
            }
        }

        /// <summary>
        /// Gets the upper bound a facet with this facet description can take
        /// </summary>
        public int? MaxValue
        {
            get
            {
                return _maxValue;
            }
        }

        /// <summary>
        /// Gets the default value of a facet with this facet description
        /// </summary>
        public object DefaultValue
        {
            get
            {
                if (_defaultValue == _notInitializedSentinel)
                {
                    return null;
                }
                return _defaultValue;
            }
        }

        /// <summary>
        /// Gets whether the value of this facet must be constant
        /// </summary>
        public bool IsConstant
        {
            get
            {
                return _isConstant;
            }
        }

        /// <summary>
        /// Gets whether this facet is a required facet or not
        /// </summary>
        public bool IsRequired
        {
            get
            {
                return _defaultValue == _notInitializedSentinel;
            }
        }

        #region Internal properties

        /// <summary>
        /// Gets a facet with the default value for this description.
        /// </summary>
        internal Facet DefaultValueFacet
        {
            get
            {
                if (_defaultValueFacet == null)
                {
                    Facet defaultValueFacet = Facet.Create(this, this.DefaultValue, true);
                    Interlocked.CompareExchange(ref _defaultValueFacet, defaultValueFacet, null);
                }
                return _defaultValueFacet;
            }
        }

        /// <summary>
        /// Gets a facet with a null value for this description.
        /// </summary>
        internal Facet NullValueFacet
        {
            get
            {
                if (_nullValueFacet == null)
                {
                    Facet nullValueFacet = Facet.Create(this, null, true);
                    Interlocked.CompareExchange(ref _nullValueFacet, nullValueFacet, null);
                }
                return _nullValueFacet;
            }
        }

        #endregion Internal properties

        #endregion

        #region Methods
        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation 
        /// for this type.
        /// </summary>
        public override string ToString()
        {
            return this.FacetName;
        }

        /// <summary>
        /// Gets a cached facet instance with the specified boolean value.
        /// </summary>
        /// <param name="value">Value for the Facet result.</param>
        /// <returns>A cached facet instance with the specified boolean value.</returns>
        internal Facet GetBooleanFacet(bool value)
        {
            System.Diagnostics.Debug.Assert(this.FacetType.Identity == "Edm.Boolean");
            if (_valueCache == null)
            {
                Facet[] valueCache = new Facet[2];
                valueCache[0] = Facet.Create(this, true, true);
                valueCache[1] = Facet.Create(this, false, true);

                System.Threading.Interlocked.CompareExchange(
                                                    ref _valueCache,
                                                    valueCache,
                                                    null
                                                );
            }
            return (value) ? _valueCache[0] : _valueCache[1];
        }

        /// <summary>
        /// Returns true if the facet type is of numeric type
        /// </summary>
        /// <param name="facetType">Type of the facet</param>
        /// <returns></returns>
        internal static bool IsNumericType(EdmType facetType)
        {
            if (Helper.IsPrimitiveType(facetType))
            {
                PrimitiveType primitiveType = (PrimitiveType)facetType;

                return primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Byte ||
                       primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.SByte ||
                       primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Int16 ||
                       primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Int32;
            }

            return false;
        }

        private static void UpdateMinMaxValueForConstant(string facetName, EdmType facetType, ref int? minValue, ref int? maxValue, object defaultValue)
        {
            if (FacetDescription.IsNumericType(facetType))
            {
                if (facetName == EdmProviderManifest.PrecisionFacetName ||
                    facetName == EdmProviderManifest.ScaleFacetName)
                {
                    minValue = (int?)((byte?)defaultValue);
                    maxValue = (int?)((byte?)defaultValue);
                }
                else
                {
                    minValue = (int?)defaultValue;
                    maxValue = (int?)defaultValue;
                }
            }
        }

        private void Validate(string declaringTypeName)
        {
            if (_defaultValue == _notInitializedSentinel)
            {
                if (_isConstant)
                {
                    throw EntityUtil.MissingDefaultValueForConstantFacet(_facetName, declaringTypeName);
                }
            }
            else if (FacetDescription.IsNumericType(_facetType))
            {
                if (_isConstant)
                {
                    // Either both of them are not specified or both of them have the same value
                    if ((_minValue.HasValue != _maxValue.HasValue) ||
                        (_minValue.HasValue && _minValue.Value != _maxValue.Value))
                    {
                        throw EntityUtil.MinAndMaxValueMustBeSameForConstantFacet(_facetName, declaringTypeName);
                    }
                }

                // If its not constant, then both of the minValue and maxValue must be specified
                else if (!_minValue.HasValue || !_maxValue.HasValue)
                {
                    throw EntityUtil.BothMinAndMaxValueMustBeSpecifiedForNonConstantFacet(_facetName, declaringTypeName);
                }
                else if (_minValue.Value == _maxValue)
                {
                    throw EntityUtil.MinAndMaxValueMustBeDifferentForNonConstantFacet(_facetName, declaringTypeName);
                }
                else if (_minValue < 0 || _maxValue < 0)
                {
                    throw EntityUtil.MinAndMaxMustBePositive(_facetName, declaringTypeName);
                }
                else if (_minValue > _maxValue)
                {
                    throw EntityUtil.MinMustBeLessThanMax(_minValue.ToString(), _facetName, declaringTypeName);
                }
            }
        }

        #endregion
    }
}
