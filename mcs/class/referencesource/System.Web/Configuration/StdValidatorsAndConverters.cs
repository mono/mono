//------------------------------------------------------------------------------
// <copyright file="StdValidatorsAndConverters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.ComponentModel;

namespace System.Web.Configuration {

    // Common shared validators and type converter instances
    internal static class StdValidatorsAndConverters {
        
        static private TypeConverter s_infiniteTimeSpanConverter;
        static private TypeConverter s_timeSpanMinutesConverter;
        static private TypeConverter s_timeSpanMinutesOrInfiniteConverter;
        static private TypeConverter s_timeSpanSecondsConverter;
        static private TypeConverter s_timeSpanSecondsOrInfiniteConverter;
        static private TypeConverter s_whiteSpaceTrimStringConverter;
        static private TypeConverter s_versionConverter;

        static private ConfigurationValidatorBase s_regexMatchTimeoutValidator;
        static private ConfigurationValidatorBase s_positiveTimeSpanValidator;
        static private ConfigurationValidatorBase s_nonEmptyStringValidator;
        static private ConfigurationValidatorBase s_nonZeroPositiveIntegerValidator;
        static private ConfigurationValidatorBase s_positiveIntegerValidator;

        static internal TypeConverter InfiniteTimeSpanConverter {
            get {
                if (s_infiniteTimeSpanConverter == null) {
                    s_infiniteTimeSpanConverter = new InfiniteTimeSpanConverter();
                }

                return s_infiniteTimeSpanConverter;
            }
        }
        
        static internal TypeConverter TimeSpanMinutesConverter {
            get {
                if (s_timeSpanMinutesConverter == null) {
                    s_timeSpanMinutesConverter = new TimeSpanMinutesConverter();
                }

                return s_timeSpanMinutesConverter;
            }
        }
        
        static internal TypeConverter TimeSpanMinutesOrInfiniteConverter {
            get {
                if (s_timeSpanMinutesOrInfiniteConverter == null) {
                    s_timeSpanMinutesOrInfiniteConverter = new TimeSpanMinutesOrInfiniteConverter();
                }

                return s_timeSpanMinutesOrInfiniteConverter;
            }
        }
            
        static internal TypeConverter TimeSpanSecondsConverter  {
            get {
                if (s_timeSpanSecondsConverter == null) {
                    s_timeSpanSecondsConverter = new TimeSpanSecondsConverter();
                }

                return s_timeSpanSecondsConverter;
            }
        }
            
        static internal TypeConverter TimeSpanSecondsOrInfiniteConverter {
            get {
                if (s_timeSpanSecondsOrInfiniteConverter == null) {
                    s_timeSpanSecondsOrInfiniteConverter =  new TimeSpanSecondsOrInfiniteConverter();
                }

                return s_timeSpanSecondsOrInfiniteConverter;
            }
        }
           
        static internal TypeConverter WhiteSpaceTrimStringConverter {
            get {
                if (s_whiteSpaceTrimStringConverter == null) {
                    s_whiteSpaceTrimStringConverter = new WhiteSpaceTrimStringConverter();
                }

                return s_whiteSpaceTrimStringConverter;
            }
        }

        static internal TypeConverter VersionConverter {
            get {
                if (s_versionConverter == null) {
                    s_versionConverter = new VersionConverter();
                }

                return s_versionConverter;
            }
        }

        static internal ConfigurationValidatorBase RegexMatchTimeoutValidator {
            get {
                if (s_regexMatchTimeoutValidator == null) {
                    s_regexMatchTimeoutValidator = new RegexMatchTimeoutValidator();
                }

                return s_regexMatchTimeoutValidator;
            }
        }

        static internal ConfigurationValidatorBase PositiveTimeSpanValidator {
            get {
                if (s_positiveTimeSpanValidator == null) {
                    s_positiveTimeSpanValidator = new PositiveTimeSpanValidator();
                }

                return s_positiveTimeSpanValidator;
            }
        }

        static internal ConfigurationValidatorBase NonEmptyStringValidator {
            get {
                if (s_nonEmptyStringValidator == null) {
                    s_nonEmptyStringValidator = new StringValidator(1);
                }

                return s_nonEmptyStringValidator;
            }
        }
        
        static internal ConfigurationValidatorBase NonZeroPositiveIntegerValidator {
            get {
                if (s_nonZeroPositiveIntegerValidator == null) {
                    s_nonZeroPositiveIntegerValidator = new IntegerValidator(1, int.MaxValue);
                }

                return s_nonZeroPositiveIntegerValidator;
            }
        }
            
        static internal ConfigurationValidatorBase PositiveIntegerValidator {
            get {
                if (s_positiveIntegerValidator == null) {
                    s_positiveIntegerValidator =  new IntegerValidator(0, int.MaxValue);
                }

                return s_positiveIntegerValidator;
            }
        }
        
    }
}
