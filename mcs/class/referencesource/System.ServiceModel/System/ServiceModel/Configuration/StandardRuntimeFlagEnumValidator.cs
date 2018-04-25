//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    internal class StandardRuntimeFlagEnumValidator<TEnum> : ConfigurationValidatorBase where TEnum : struct
    {
        public StandardRuntimeFlagEnumValidator()
        {
            StandardRuntimeFlagEnumValidatorAttribute.ValidateFlagEnumType(typeof(TEnum));
        }

        public override bool CanValidate(Type type)
        {
            return (type == typeof(TEnum));
        }

        public override void Validate(object value)
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                TEnum dummy;
                if (!Enum.TryParse<TEnum>(value.ToString(), true, out dummy))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value, typeof(TEnum)));
                }

                int combinedValue = (int)((object)dummy);
                int[] values = (int[])Enum.GetValues(typeof(TEnum));
                if (!StandardRuntimeFlagEnumValidatorAttribute.IsCombinedValue(combinedValue, values, values.Length - 1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value, typeof(TEnum)));
                }
            }
        }
    }
}
