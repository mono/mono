//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;

    class TransactionProtocolConverter : TypeConverter
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
            string protocol = value as string;
#pragma warning suppress 56507 // Microsoft, Really checking for null (meaning value was not a string) versus String.Empty
            if (protocol != null)
            {
                switch (protocol)
                {
                    case ConfigurationStrings.OleTransactions:
                        return TransactionProtocol.OleTransactions;
                    case ConfigurationStrings.WSAtomicTransactionOctober2004:
                        return TransactionProtocol.WSAtomicTransactionOctober2004;
                    case ConfigurationStrings.WSAtomicTransaction11:
                        return TransactionProtocol.WSAtomicTransaction11;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTransactionFlowProtocolValue, protocol));
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType && value is TransactionProtocol)
            {
                TransactionProtocol protocol = (TransactionProtocol)value;
                return protocol.Name;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
