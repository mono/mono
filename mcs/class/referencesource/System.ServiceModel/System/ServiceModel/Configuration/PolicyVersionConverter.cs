//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Description;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.ServiceModel;
    using System.Globalization;

    class PolicyVersionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(string) == sourceType)
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(InstanceDescriptor) == destinationType)
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                string policyVersion = (string)value;
                PolicyVersion retval = null;
                switch (policyVersion)
                {
                    case ConfigurationStrings.Policy12:
                        retval = PolicyVersion.Policy12;
                        break;
                    case ConfigurationStrings.Policy15:
                        retval = PolicyVersion.Policy15;
                        break;
                    case ConfigurationStrings.Default:
                        retval = PolicyVersion.Default;
                        break;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                            SR.GetString(SR.ConfigInvalidClassFactoryValue, policyVersion, typeof(PolicyVersion).FullName)));
                }
                return retval;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType && value is PolicyVersion)
            {
                string retval = null;
                PolicyVersion policyVersion = (PolicyVersion)value;
                if (policyVersion == PolicyVersion.Default)
                {
                    retval = ConfigurationStrings.Default;
                }
                else if (policyVersion == PolicyVersion.Policy12)
                {
                    retval = ConfigurationStrings.Policy12;
                }
                else if (policyVersion == PolicyVersion.Policy15)
                {
                    retval = ConfigurationStrings.Policy15;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.ConfigInvalidClassInstanceValue, typeof(PolicyVersion).FullName)));
                }
                return retval;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

