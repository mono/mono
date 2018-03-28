//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.Text;

    class EncodingConverter : TypeConverter
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
                string encoding = (string)value;

                Encoding retval;
                if (String.Compare(encoding, TextEncoderDefaults.EncodingString, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // special case for utf-8 to match with what we do in the default text encoding
                    retval = TextEncoderDefaults.Encoding;
                }
                else
                {
                    retval = Encoding.GetEncoding(encoding);
                }
                if (retval == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ConfigInvalidEncodingValue, encoding));
                }
                return retval;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType && value is Encoding)
            {
                Encoding encoding = (Encoding)value;
                return encoding.HeaderName;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
