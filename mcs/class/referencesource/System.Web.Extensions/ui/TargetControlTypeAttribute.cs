//------------------------------------------------------------------------------
// <copyright file="TargetControlTypeAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    [
    AttributeUsage(AttributeTargets.Class, AllowMultiple = true)
    ]
    public sealed class TargetControlTypeAttribute : Attribute {
        private Type _targetControlType;

        public TargetControlTypeAttribute(Type targetControlType) {
            if (targetControlType == null) {
                throw new ArgumentNullException("targetControlType");
            }
            _targetControlType = targetControlType;
        }

        public Type TargetControlType {
            get {
                return _targetControlType;
            }
        }

        // For attributes with AllowMultiple set to true, TypeDescriptor.GetAttributes() removes duplicate instances.
        // These are instances in which TypeId returns equal values.  So we must override the TypeId property to
        // return a unique key.  For this attribute, the unique key is just the target control type itself.
        // (DevDiv Bugs 111475)
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override object TypeId {
            get {
                return _targetControlType;
            }
        }
    }
}
