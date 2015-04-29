//------------------------------------------------------------------------------
// <copyright file="IntegerValidatorAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration.Internal;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
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
    public sealed class IntegerValidatorAttribute : ConfigurationValidatorAttribute {
        private int _min = int.MinValue;
        private int _max = int.MaxValue;
        private bool _excludeRange = false;

        public IntegerValidatorAttribute() {
        }

        public override ConfigurationValidatorBase ValidatorInstance {
            get {
                return new IntegerValidator(_min, _max, _excludeRange);
            }
        }

        public int MinValue {
            get {
                return _min;
            }
            set {
                if (_max < value) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.Validator_min_greater_than_max));
                }

                _min = value;
            }
        }

        public int MaxValue {
            get {
                return _max;
            }
            set {
                if (_min > value) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.Validator_min_greater_than_max));
                }

                _max = value;
            }
        }

        public bool ExcludeRange {
            get {
                return _excludeRange;
            }
            set {
                _excludeRange = value;
            }
        }
    }
}
