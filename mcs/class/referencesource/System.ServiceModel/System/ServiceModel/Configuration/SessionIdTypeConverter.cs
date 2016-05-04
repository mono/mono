// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;

    class SessionIdTypeConvertor : Int32Converter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type type)
        {
            if (value == null)
            {
                throw FxTrace.Exception.ArgumentNull("value");
            }

            if (!(value is int))
            {
                throw FxTrace.Exception.Argument("value", InternalSR.IncompatibleArgumentType(typeof(int), value.GetType()));
            }

            if ((int)value == 0)
            {
                return ApplicationContainerSettingsDefaults.Session0ServiceSessionString;
            }
            else if ((int)value == -1)
            {
                return ApplicationContainerSettingsDefaults.CurrentUserSessionDefaultString;
            }
            else
            {
                return base.ConvertTo(context, cultureInfo, value, type);
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object data)
        {
            if (string.Equals((string)data, ApplicationContainerSettingsDefaults.CurrentUserSessionDefaultString, StringComparison.OrdinalIgnoreCase))
            {
                return ApplicationContainerSettings.CurrentSession;
            }
            else if (string.Equals((string)data, ApplicationContainerSettingsDefaults.Session0ServiceSessionString, StringComparison.OrdinalIgnoreCase))
            {
                return ApplicationContainerSettings.ServiceSession;
            }
            else
            {
                return base.ConvertFrom(context, cultureInfo, data);
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }
    }
}
