//------------------------------------------------------------------------------
// <copyright file="SubclassTypeValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Xml;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Configuration {

    // This class validates that the value is a subclass of a specified type
    public sealed class SubclassTypeValidator : ConfigurationValidatorBase {
        private Type _base;

        public SubclassTypeValidator(Type baseClass) {
            if (baseClass == null) {
                throw new ArgumentNullException("baseClass");
            }

            _base = baseClass;
        }

        public override bool CanValidate(Type type) {
            return (type == typeof(Type));
        }
        
        public override void Validate(object value) {
            if (value == null) {
                return;
            }

            // Make a check here since value.GetType() returns RuntimeType rather then Type
            if (!(value is Type)) {
                ValidatorUtils.HelperParamValidation(value, typeof(Type));
            }

            if (!_base.IsAssignableFrom((Type)value)) {
                throw new ArgumentException(SR.GetString(SR.Subclass_validator_error, ((Type)value).FullName, _base.FullName));
            }
        }
    }
}
