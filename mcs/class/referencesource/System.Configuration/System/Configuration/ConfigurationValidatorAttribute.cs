//------------------------------------------------------------------------------
// <copyright file="ConfigurationValidatorAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration.Internal;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Xml;
using System.Globalization;
using System.ComponentModel;
using System.Security;
using System.Text;

namespace System.Configuration {

    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurationValidatorAttribute : Attribute {
        internal Type _declaringType;
        private readonly Type _validator;

        protected ConfigurationValidatorAttribute() {
        }
        public ConfigurationValidatorAttribute(Type validator) {
            if (validator == null) {
                throw new ArgumentNullException("validator");
            }

            if (!typeof(ConfigurationValidatorBase).IsAssignableFrom(validator)) {
                throw new ArgumentException(SR.GetString(SR.Validator_Attribute_param_not_validator, "ConfigurationValidatorBase"));
            }

            _validator = validator;
        }
        public virtual ConfigurationValidatorBase ValidatorInstance {
            get {
                return (ConfigurationValidatorBase)TypeUtil.CreateInstanceRestricted(_declaringType, _validator);
            }
        }

        // Used for limiting the visibility of types that can be accessed in the reflection
        // call made by the ValidatorInstance property getter. This will normally be the
        // type that declared the attribute, but in certain cases it could be a subclass
        // of the type that declared the attribute. This should be ok from a security
        // perspective, as one wouldn't reasonably expect a derived type to have fewer
        // security constraints than its base type.
        internal void SetDeclaringType(Type declaringType) {
            if (declaringType == null) {
                Debug.Fail("Declaring type must not be null.");
                return; // don't throw in an in-place update
            }

            if (_declaringType == null) {
                // First call to this method - allow any type to be set
                _declaringType = declaringType;
            }
            else if (_declaringType != declaringType) {
                Debug.Fail("Subsequent calls cannot change the declaring type of the attribute.");
                return; // don't throw in an in-place update
            }
        }

        public Type ValidatorType {
            get {
                return _validator;
            }
        }
    }
}
