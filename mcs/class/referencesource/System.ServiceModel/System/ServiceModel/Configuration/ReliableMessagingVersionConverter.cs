//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    //using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    class ReliableMessagingVersionConverter : TypeConverter
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

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string version = value as string;
#pragma warning suppress 56507 // Microsoft, Really checking for null (meaning value was not a string) versus String.Empty
            if (version != null)
            {
                switch (version)
                {
                    case ConfigurationStrings.Default:
                        return ReliableMessagingVersion.Default;
                    case ConfigurationStrings.WSReliableMessaging11:
                        return ReliableMessagingVersion.WSReliableMessaging11;
                    case ConfigurationStrings.WSReliableMessagingFebruary2005:
                        return ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidReliableMessagingVersionValue, version));
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType && value is ReliableMessagingVersion)
            {
                ReliableMessagingVersion version = (ReliableMessagingVersion)value;
                
                if (version == ReliableMessagingVersion.Default)
                {
                    return ConfigurationStrings.Default;
                }
                else if (version == ReliableMessagingVersion.WSReliableMessaging11)
                {
                    return ConfigurationStrings.WSReliableMessaging11;
                }
                else if (version == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    return ConfigurationStrings.WSReliableMessagingFebruary2005;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.ConfigInvalidClassInstanceValue, typeof(ReliableMessagingVersion).FullName)));
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
