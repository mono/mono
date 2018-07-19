//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    class ServiceOperationInfoTypeConverter : TypeConverter
    {
        public ServiceOperationInfoTypeConverter()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                OperationInfoBase serviceOperationInfo = value as OperationInfoBase;
                if (serviceOperationInfo != null)
                {
                    string contractName = serviceOperationInfo.GetContractFullName(null);
                    if (string.IsNullOrEmpty(contractName) || string.IsNullOrEmpty(serviceOperationInfo.Name))
                    {
                        return string.Empty;
                    }

                    return string.Format(CultureInfo.InvariantCulture,
                        "{0}.{1}",
                        contractName,
                        serviceOperationInfo.Name);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
