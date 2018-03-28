//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class StandardRuntimeFlagEnumValidatorAttribute : ConfigurationValidatorAttribute
    {
        Type enumType;
        Type validatorType;

        public StandardRuntimeFlagEnumValidatorAttribute(Type enumType)
        {
            StandardRuntimeFlagEnumValidatorAttribute.ValidateFlagEnumType(enumType);
            this.EnumType = enumType;
        }

        public Type EnumType
        {
            get { return this.enumType; }
            set
            {
                StandardRuntimeFlagEnumValidatorAttribute.ValidateFlagEnumType(value);
                this.enumType = value;
            }
        }

        static bool IsPowerOfTwo(int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }


        internal static void ValidateFlagEnumType(Type value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("EnumType");
            }

            bool hasFlags = value.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0;
            if (!value.IsEnum || !hasFlags)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("EnumType", SR.GetString(SR.FlagEnumTypeExpected, value));
            }

            int[] values = (int[])Enum.GetValues(value);

            if (values != null &&
                values.Length > 0)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] != 0 && !IsPowerOfTwo(values[i]))
                    {
                        if (!StandardRuntimeFlagEnumValidatorAttribute.IsCombinedValue(values[i], values, i - 1))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("EnumType", SR.GetString(SR.InvalidFlagEnumType));
                        }
                    }
                }
            }
        }

        internal static bool IsCombinedValue(int combinedValue, int[] allowedValues, int startPosition)
        {
            int n = startPosition;

            while (n >= 0 &&
                   combinedValue > 0)
            {
                if ((combinedValue & allowedValues[n]) == allowedValues[n])
                {
                    combinedValue -= allowedValues[n];
                }

                n--;
            }

            return combinedValue == 0;
        }

        private void EnsureValidatorType()
        {
            if (this.validatorType == null)
            {
                validatorType = typeof(StandardRuntimeFlagEnumValidator<>).MakeGenericType(new System.Type[] { this.enumType });
            }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                this.EnsureValidatorType();
                return (ConfigurationValidatorBase)Activator.CreateInstance(validatorType, null);
            }
        }
    }
}
