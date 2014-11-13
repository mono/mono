//------------------------------------------------------------------------------
// <copyright file="VersionValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Configuration;

    internal sealed class VersionValidator : ConfigurationValidatorBase {
        private readonly Version _minimumVersion;

        public VersionValidator(Version minimumVersion) {
            _minimumVersion = minimumVersion;
        }

        public override bool CanValidate(Type type) {
            return typeof(Version).Equals(type);
        }

        public override void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            if (((Version)value) < _minimumVersion) {
                throw new ArgumentOutOfRangeException("value", 
                    SR.GetString(SR.Config_control_rendering_compatibility_version_is_less_than_minimum_version));
            }
        }
    }
}
