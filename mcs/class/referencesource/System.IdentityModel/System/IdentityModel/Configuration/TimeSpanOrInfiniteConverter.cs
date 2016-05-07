//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Configuration
{
    using System;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.ComponentModel;    

    class TimeSpanOrInfiniteConverter : TimeSpanConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type type)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }

            if (!(value is TimeSpan))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID8028, typeof(TimeSpan), value.GetType()));
            }

            if ((TimeSpan)value == TimeSpan.MaxValue)
            {
                return "Infinite";
            }
            else
            {
                return base.ConvertTo(context, cultureInfo, value, type);
            }
        }
        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object data)
        {
            if (string.Equals((string)data, "infinite", StringComparison.OrdinalIgnoreCase))
            {
                return TimeSpan.MaxValue;
            }
            else
            {
                return base.ConvertFrom(context, cultureInfo, data);
            }
        }
    }
}
