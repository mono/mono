//------------------------------------------------------------------------------
// <copyright file="StringValidator.cs" company="Microsoft">
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

    public class StringValidator : ConfigurationValidatorBase {
        private int _minLength;
        private int _maxLength;
        private string _invalidChars;

        public StringValidator(int minLength)
            : this(minLength, int.MaxValue, null) {
        }
        
        public StringValidator(int minLength, int maxLength)
            : this(minLength, maxLength, null) {
        }
        
        public StringValidator(int minLength, int maxLength, string invalidCharacters) {
            _minLength = minLength;
            _maxLength = maxLength;
            _invalidChars = invalidCharacters;
        }

        public override bool CanValidate(Type type) {
            return (type == typeof(string));
        }
        
        public override void Validate(object value) {
            ValidatorUtils.HelperParamValidation(value, typeof(string));

            string data = value as string;
            int len = (data == null ? 0 : data.Length);

            if (len < _minLength) {
                throw new ArgumentException(SR.GetString(SR.Validator_string_min_length, _minLength));
            }

            if (len > _maxLength) {
                throw new ArgumentException(SR.GetString(SR.Validator_string_max_length, _maxLength));
            }

            // Check if the string contains any invalid characters
            if ((len > 0) && (_invalidChars != null) && (_invalidChars.Length > 0)) {
                char[] array = new char[_invalidChars.Length];

                _invalidChars.CopyTo(0, array, 0, _invalidChars.Length);

                if (data.IndexOfAny(array) != -1) {
                    throw new ArgumentException(SR.GetString(SR.Validator_string_invalid_chars, _invalidChars));
                }
            }
        }
    }
}
