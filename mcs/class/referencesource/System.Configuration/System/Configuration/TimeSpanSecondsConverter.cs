//------------------------------------------------------------------------------
// <copyright file="TimeSpanSecondsConverter.cs" company="Microsoft">
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

namespace System.Configuration {

    public class TimeSpanSecondsConverter : ConfigurationConverterBase {

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type) {
            ValidateType(value, typeof(TimeSpan));

            long data = (long)(((TimeSpan)value).TotalSeconds);

            return data.ToString(CultureInfo.InvariantCulture);
        }
        
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data) {
            Debug.Assert(data is string, "data is string");
            long min = 0;
            try {
                min = long.Parse((string)data, CultureInfo.InvariantCulture);
            }
            catch {
                throw new ArgumentException(SR.GetString(SR.Converter_timespan_not_in_second));
            }
            return TimeSpan.FromSeconds((double)min);
        }
    }
}
