//------------------------------------------------------------------------------
// <copyright file="TimeSpanValidator.cs" company="Microsoft">
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

    public class TimeSpanValidator : ConfigurationValidatorBase {

        private enum ValidationFlags {
            None = 0x0000,
            ExclusiveRange = 0x0001,   // If set the value must be outside of the range instead of inside
        }

        private ValidationFlags _flags = ValidationFlags.None;
        private TimeSpan _minValue = TimeSpan.MinValue;
        private TimeSpan _maxValue = TimeSpan.MaxValue;
        private long _resolution = 0;

        public TimeSpanValidator(TimeSpan minValue, TimeSpan maxValue)
            : this(minValue, maxValue, false, 0) {
        }
        
        public TimeSpanValidator(TimeSpan minValue, TimeSpan maxValue, bool rangeIsExclusive)
            : this(minValue, maxValue, rangeIsExclusive, 0) {
        }
        
        public TimeSpanValidator(TimeSpan minValue, TimeSpan maxValue, bool rangeIsExclusive, long resolutionInSeconds) {
            if (resolutionInSeconds < 0) {
                throw new ArgumentOutOfRangeException("resolutionInSeconds");
            }

            if (minValue > maxValue) {
                throw new ArgumentOutOfRangeException("minValue", SR.GetString(SR.Validator_min_greater_than_max));
            }

            _minValue = minValue;
            _maxValue = maxValue;
            _resolution = resolutionInSeconds;

            _flags = rangeIsExclusive ? ValidationFlags.ExclusiveRange : ValidationFlags.None;
        }

        public override bool CanValidate(Type type) {
            return (type == typeof(TimeSpan));
        }
        
        public override void Validate(object value) {
            ValidatorUtils.HelperParamValidation(value, typeof(TimeSpan));

            ValidatorUtils.ValidateScalar((TimeSpan)value,
                                            _minValue,
                                            _maxValue,
                                            _resolution,
                                            _flags == ValidationFlags.ExclusiveRange);
        }
    }
}
