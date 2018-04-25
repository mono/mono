//------------------------------------------------------------------------------
// <copyright file="ValidatorUtils.cs" company="Microsoft">
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

    internal static class ValidatorUtils {

        public static void HelperParamValidation(object value, Type allowedType) {
            if (value == null) {
                return;
            }

            if (value.GetType() != allowedType) {
                throw new ArgumentException(SR.GetString(SR.Validator_value_type_invalid), String.Empty);
            }
        }
        
        public static void ValidateScalar<T>(T value, T min, T max, T resolution, bool exclusiveRange) where T : IComparable<T> {
            ValidateRangeImpl(value, min, max, exclusiveRange);

            // Validate the resolution
            ValidateResolution(resolution.ToString(), Convert.ToInt64(value, CultureInfo.InvariantCulture), Convert.ToInt64(resolution, CultureInfo.InvariantCulture));
        }
        private static void ValidateRangeImpl<T>( T value, T min, T max, bool exclusiveRange ) where T : IComparable<T> {
            IComparable<T>  itfValue           = (IComparable<T>)value;
            IComparable<T>  itfMax             = (IComparable<T>)max;
            bool            valueIsInRange     = false;

            // Check min range
            if ( itfValue.CompareTo( min ) >= 0 ) {
                // TRUE: value >= min
                valueIsInRange = true;
            }

            if ( valueIsInRange && ( itfValue.CompareTo( max ) > 0 ) ) {
                // TRUE: value > max
                valueIsInRange = false;
            }

            // Throw range validation error
            if ( !( valueIsInRange ^ exclusiveRange ) ) {
                string error = null;
                
                // First group of errors - the min and max range are the same. i.e. the valid value must be the same/equal to the min(max)
                if ( min.Equals( max ) ) {
                    if ( exclusiveRange ) {
                        // Valid values are outside of range. I.e has to be different then min( or max )
                        error =  SR.GetString( SR.Validation_scalar_range_violation_not_different );
                    }
                    else {
                        // Valid values are inside the range. I.e. has to be equal to min ( or max )
                        error = SR.GetString( SR.Validation_scalar_range_violation_not_equal );
                    }
                }
                // Second group of errors: min != max. I.e. its a range
                else {
                    if ( exclusiveRange ) {
                        // Valid values are outside of range. 
                        error =  SR.GetString( SR.Validation_scalar_range_violation_not_outside_range );
                    }
                    else {
                        // Valid values are inside the range. I.e. has to be equal to min ( or max )
                        error = SR.GetString( SR.Validation_scalar_range_violation_not_in_range );
                    }
                }
                
                throw new ArgumentException( String.Format( CultureInfo.InvariantCulture,
                                                            error, 
                                                            min.ToString(),
                                                            max.ToString() ) );
            }
        }

        private static void ValidateResolution(string resolutionAsString, long value, long resolution) {
            Debug.Assert(resolution > 0, "resolution > 0");

            if ((value % resolution) != 0) {
                throw new ArgumentException(SR.GetString(SR.Validator_scalar_resolution_violation, resolutionAsString));
            }
        }

        public static void ValidateScalar(TimeSpan value, TimeSpan min, TimeSpan max, long resolutionInSeconds, bool exclusiveRange) {
            ValidateRangeImpl(value, min, max, exclusiveRange);

            // Validate the resolution
            if (resolutionInSeconds > 0) {
                ValidateResolution(TimeSpan.FromSeconds( resolutionInSeconds ).ToString(), value.Ticks, resolutionInSeconds * TimeSpan.TicksPerSecond);
            }
        }
    }
}
