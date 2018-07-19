//---------------------------------------------------------------------
// <copyright file="FacetValueContainer.cs" company="Microsoft">
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
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// This Class is never expected to be used except for by the FacetValues class.  
    /// 
    /// The purpose of this class is to allow strong type checking by the compiler while setting facet values which
    /// are typically stored as Object because they can either on of these things
    /// 
    /// 1. null
    /// 2. scalar type (bool, int, byte)
    /// 3. Unbounded object
    /// 
    /// without this class it would be very easy to accidentally set precision to an int when it really is supposed to be 
    /// a byte value.  Also you would be able to set the facet value to any Object derived class (ANYTHING!!!) when really only
    /// null and Unbounded are allowed besides an actual scalar value.  The magic of the class happens in the implicit constructors with 
    /// allow patterns like
    /// 
    /// new FacetValues( MaxLength = EdmConstants.UnboundedValue, Nullable = true};
    /// 
    /// and these are type checked at compile time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct FacetValueContainer<T>
    {
        T _value;
        bool _hasValue;
        bool _isUnbounded;

        internal T Value
        {
            set
            {
                _isUnbounded = false;
                _hasValue = true;
                _value = value;
            }
        }

        private void SetUnbounded()
        {
            _isUnbounded = true;
            _hasValue = true;
        }

        // don't add an implicit conversion from object because it will kill the compile time type checking.
        public static implicit operator FacetValueContainer<T>(System.Data.Metadata.Edm.EdmConstants.Unbounded unbounded)
        {
            Debug.Assert(object.ReferenceEquals(unbounded, EdmConstants.UnboundedValue), "you must pass the unbounded value.  If you are trying to set null, use the T parameter overload");
            FacetValueContainer<T> container = new FacetValueContainer<T>();
            container.SetUnbounded();
            return container;
        }

        public static implicit operator FacetValueContainer<T>(T value)
        {
            FacetValueContainer<T> container = new FacetValueContainer<T>();
            container.Value = value;
            return container;
        }

        internal object GetValueAsObject()
        {
            Debug.Assert(_hasValue, "Don't get the value if it has not been set");
            if (_isUnbounded)
            {
                return EdmConstants.UnboundedValue;
            }
            else
            {
                return (object)_value;
            }
        }

        internal bool HasValue
        {
            get
            {
                return _hasValue;
            }
        }
    }
}
