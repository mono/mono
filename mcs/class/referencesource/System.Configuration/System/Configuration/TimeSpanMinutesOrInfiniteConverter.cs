//------------------------------------------------------------------------------
// <copyright file="TimeSpanMinutesOrInfiniteConverter.cs" company="Microsoft">
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

    public sealed class TimeSpanMinutesOrInfiniteConverter : TimeSpanMinutesConverter {

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type) {
            ValidateType(value, typeof(TimeSpan));

            if ((TimeSpan)value == TimeSpan.MaxValue) {
                return "Infinite";
            }
            else {
                return base.ConvertTo(ctx, ci, value, type);
            }
        }
        
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data) {
            Debug.Assert(data is string, "data is string");

            if ((string)data == "Infinite") {
                // The "infinite" value is the max minutes a int can hold
                return TimeSpan.MaxValue;
            }
            else {
                return base.ConvertFrom(ctx, ci, data);
            }
        }
    }
}
