/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Reflection;

    public abstract class ParameterDescriptor : ICustomAttributeProvider {

        private static readonly EmptyParameterBindingInfo _emptyBindingInfo = new EmptyParameterBindingInfo();

        public abstract ActionDescriptor ActionDescriptor {
            get;
        }

        public virtual ParameterBindingInfo BindingInfo {
            get {
                return _emptyBindingInfo;
            }
        }

        public virtual object DefaultValue {
            get {
                return null;
            }
        }

        public abstract string ParameterName {
            get;
        }

        public abstract Type ParameterType {
            get;
        }

        public virtual object[] GetCustomAttributes(bool inherit) {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit) {
            if (attributeType == null) {
                throw new ArgumentNullException("attributeType");
            }

            return (object[])Array.CreateInstance(attributeType, 0);
        }

        public virtual bool IsDefined(Type attributeType, bool inherit) {
            if (attributeType == null) {
                throw new ArgumentNullException("attributeType");
            }

            return false;
        }

        private sealed class EmptyParameterBindingInfo : ParameterBindingInfo {
        }

    }
}
