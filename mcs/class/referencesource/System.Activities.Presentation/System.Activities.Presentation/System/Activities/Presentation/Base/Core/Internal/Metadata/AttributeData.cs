//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.Metadata 
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.Runtime;

    // This class groups Attribute with its AttributeUsageAttributes that we care about.
    // Basically, each Attribute has characteristics that are useful to know about,
    // such as whether it can be inherited and whether there can be more than
    // one instance of that attribute extending whatever it is the attribute
    // is extending (class, method, property, or event).  Those characteristics
    // are stored as attributes themselves and, as such, are costly to retrieve.
    // This class retrieves that information exactly once, on demand, and caches it for
    // further use.
    internal class AttributeData 
    {

        private Type _attributeType;
        private bool? _isInheritable;
        private bool? _allowsMultiple;

        // <summary>
        // Creates an AttributeData wrapper around Attribute type to expose its
        // Inherit and AllowMultiple characteristics
        // </summary>
        // <param name="attributeType">Attribute type to wrap around</param>
        internal AttributeData(Type attributeType) 
        {
            Fx.Assert(attributeType != null, "attributeType parameter should not be null");
            _attributeType = attributeType;
        }

        // <summary>
        // Gets the contained attribute type
        // </summary>
        internal Type AttributeType 
        {
            get {
                return _attributeType;
            }
        }

        // <summary>
        // Gets the AllowMultiple characteristic of the
        // contained attribute and caches the result for subsequent
        // calls to this property.
        // </summary>
        internal bool AllowsMultiple 
        {
            get {
                if (_allowsMultiple == null)
                {
                    ParseUsageAttributes();
                }

                return (bool)_allowsMultiple;
            }
        }

        // <summary>
        // Gets the Inherit characteristic of the
        // contained attribute and caches the result for subsequent
        // calls to this property.
        // </summary>
        internal bool IsInheritable 
        {
            get {
                if (_isInheritable == null)
                {
                    ParseUsageAttributes();
                }

                return (bool)_isInheritable;
            }
        }

        private void ParseUsageAttributes() 
        {
            _isInheritable = false;
            _allowsMultiple = false;
            object[] usageAttributes = _attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), true);

            if (usageAttributes != null && usageAttributes.Length > 0) 
            {
                for (int i = 0; i < usageAttributes.Length; i++) 
                {
                    Fx.Assert(usageAttributes[i] is AttributeUsageAttribute, "usageAttributes should be of type AttributeUsageAttribute");
                    AttributeUsageAttribute usageAttribute = (AttributeUsageAttribute)usageAttributes[i];
                    _isInheritable = usageAttribute.Inherited;
                    _allowsMultiple = usageAttribute.AllowMultiple;
                }
            }
        }
    }
}
