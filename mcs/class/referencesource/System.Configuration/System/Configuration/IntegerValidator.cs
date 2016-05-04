//------------------------------------------------------------------------------
// <copyright file="IntegerValidator.cs" company="Microsoft">
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

    // 
    // Type validators: Unsealed so users can derive to extend the funcionality without repeating it
    // =============================================================================================
    public class IntegerValidator : ConfigurationValidatorBase {
        private enum ValidationFlags {
            None = 0x0000,
            ExclusiveRange = 0x0001,   // If set the value must be outside of the range instead of inside
        }

        private ValidationFlags _flags = ValidationFlags.None;
        private int _minValue = int.MinValue;
        private int _maxValue = int.MaxValue;
        private int _resolution = 1;

        public IntegerValidator(int minValue, int maxValue) :
            this(minValue, maxValue, false, 1) {
        }
        
        public IntegerValidator(int minValue, int maxValue, bool rangeIsExclusive) :
            this(minValue, maxValue, rangeIsExclusive, 1) {
        }
        
        public IntegerValidator(int minValue, int maxValue, bool rangeIsExclusive, int resolution) {
            if (resolution <= 0) {
                throw new ArgumentOutOfRangeException("resolution");
            }

            if (minValue > maxValue) {
                throw new ArgumentOutOfRangeException("minValue", SR.GetString(SR.Validator_min_greater_than_max));
            }

            _minValue = minValue;
            _maxValue = maxValue;
            _resolution = resolution;

            _flags = rangeIsExclusive ? ValidationFlags.ExclusiveRange : ValidationFlags.None;
        }

        public override bool CanValidate(Type type) {
            return (type == typeof(int));
        }
        
        public override void Validate(object value) {
            ValidatorUtils.HelperParamValidation(value, typeof(int));

            ValidatorUtils.ValidateScalar((int)value,
                                            _minValue,
                                            _maxValue,
                                            _resolution,
                                            _flags == ValidationFlags.ExclusiveRange);
        }
    }
}
