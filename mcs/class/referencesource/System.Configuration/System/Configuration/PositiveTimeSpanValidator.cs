//------------------------------------------------------------------------------
// <copyright file="PositiveTimeSpanValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.ComponentModel;

namespace System.Configuration {

    public class PositiveTimeSpanValidator : ConfigurationValidatorBase {
        public override bool CanValidate(Type type) {
            return (type == typeof(TimeSpan));
        }
        public override void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            if (((TimeSpan)value) <= TimeSpan.Zero) {
                throw new ArgumentException(SR.GetString(SR.Validator_timespan_value_must_be_positive));
            }
        }
    }
}
