//------------------------------------------------------------------------------
// <copyright file="IDReferencePropertyAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <devdoc>
    /// An IDReferencePropertyAttribute metadata attribute can be applied to string properties
    /// that contain ID references.
    /// This can be used to identify ID reference properties which allows design-time functionality 
    /// to do interesting things with the property values.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IDReferencePropertyAttribute : Attribute
    {

        private Type _referencedControlType;


        /// <devdoc>
        /// </devdoc>
        public IDReferencePropertyAttribute()
            : this(typeof(Control))
        {
        }


        /// <devdoc>
        /// Used to mark a property as an ID reference. In addition, the type of controls
        /// can be specified.
        /// </devdoc>
        public IDReferencePropertyAttribute(Type referencedControlType)
        {
            _referencedControlType = referencedControlType;
        }


        /// <devdoc>
        /// The types of controls allowed by the property.
        /// </devdoc>
        public Type ReferencedControlType
        {
            get
            {
                return _referencedControlType;
            }
        }


        /// <internalonly/>
        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "The types are Sytem.Web.UI.Control derived classes and not com interop types.")]
        public override int GetHashCode()
        {
            return ((ReferencedControlType != null) ? ReferencedControlType.GetHashCode() : 0);
        }


        /// <internalonly/>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            IDReferencePropertyAttribute other = obj as IDReferencePropertyAttribute;
            if (other != null)
            {
                return (ReferencedControlType == other.ReferencedControlType);
            }

            return false;
        }
    }
}
