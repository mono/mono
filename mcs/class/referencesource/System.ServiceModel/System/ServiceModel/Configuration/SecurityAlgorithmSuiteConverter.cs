//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.ServiceModel.Security;

    class SecurityAlgorithmSuiteConverter : TypeConverter
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
                string securityAlgorithm = (string)value;
                SecurityAlgorithmSuite retval = null;
                switch (securityAlgorithm)
                {
                    case ConfigurationStrings.Default:
                        retval = SecurityAlgorithmSuite.Default; 
                        break;
                    case ConfigurationStrings.Basic256:
                        retval = SecurityAlgorithmSuite.Basic256; 
                        break;
                    case ConfigurationStrings.Basic192:
                        retval = SecurityAlgorithmSuite.Basic192; 
                        break;
                    case ConfigurationStrings.Basic128:
                        retval = SecurityAlgorithmSuite.Basic128; 
                        break;
                    case ConfigurationStrings.TripleDes:
                        retval = SecurityAlgorithmSuite.TripleDes; 
                        break;
                    case ConfigurationStrings.Basic256Rsa15:
                        retval = SecurityAlgorithmSuite.Basic256Rsa15; 
                        break;
                    case ConfigurationStrings.Basic192Rsa15:
                        retval = SecurityAlgorithmSuite.Basic192Rsa15; 
                        break;
                    case ConfigurationStrings.Basic128Rsa15:
                        retval = SecurityAlgorithmSuite.Basic128Rsa15; 
                        break;
                    case ConfigurationStrings.TripleDesRsa15:
                        retval = SecurityAlgorithmSuite.TripleDesRsa15; 
                        break;
                    case ConfigurationStrings.Basic256Sha256:
                        retval = SecurityAlgorithmSuite.Basic256Sha256; 
                        break;
                    case ConfigurationStrings.Basic192Sha256:
                        retval = SecurityAlgorithmSuite.Basic192Sha256; 
                        break;
                    case ConfigurationStrings.Basic128Sha256:
                        retval = SecurityAlgorithmSuite.Basic128Sha256; 
                        break;
                    case ConfigurationStrings.TripleDesSha256:
                        retval = SecurityAlgorithmSuite.TripleDesSha256; 
                        break;
                    case ConfigurationStrings.Basic256Sha256Rsa15:
                        retval = SecurityAlgorithmSuite.Basic256Sha256Rsa15; 
                        break;
                    case ConfigurationStrings.Basic192Sha256Rsa15:
                        retval = SecurityAlgorithmSuite.Basic192Sha256Rsa15; 
                        break;
                    case ConfigurationStrings.Basic128Sha256Rsa15:
                        retval = SecurityAlgorithmSuite.Basic128Sha256Rsa15; 
                        break;
                    case ConfigurationStrings.TripleDesSha256Rsa15:
                        retval = SecurityAlgorithmSuite.TripleDesSha256Rsa15; 
                        break;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                            SR.GetString(SR.ConfigInvalidClassFactoryValue, securityAlgorithm, typeof(SecurityAlgorithmSuite).FullName)));
                }
                return retval;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType && value is SecurityAlgorithmSuite)
            {
                string retval = null;
                SecurityAlgorithmSuite securityAlgorithm = (SecurityAlgorithmSuite)value;

                if (securityAlgorithm == SecurityAlgorithmSuite.Default)
                    retval = ConfigurationStrings.Default;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic256)
                    retval = ConfigurationStrings.Basic256;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic192)
                    retval = ConfigurationStrings.Basic192;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic128)
                    retval = ConfigurationStrings.Basic128;
                else if (securityAlgorithm == SecurityAlgorithmSuite.TripleDes)
                    retval = ConfigurationStrings.TripleDes;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic256Rsa15)
                    retval = ConfigurationStrings.Basic256Rsa15;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic192Rsa15)
                    retval = ConfigurationStrings.Basic192Rsa15;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic128Rsa15)
                    retval = ConfigurationStrings.Basic128Rsa15;
                else if (securityAlgorithm == SecurityAlgorithmSuite.TripleDesRsa15)
                    retval = ConfigurationStrings.TripleDesRsa15;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic256Sha256)
                    retval = ConfigurationStrings.Basic256Sha256;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic192Sha256)
                    retval = ConfigurationStrings.Basic192Sha256;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic128Sha256)
                    retval = ConfigurationStrings.Basic128Sha256;
                else if (securityAlgorithm == SecurityAlgorithmSuite.TripleDesSha256)
                    retval = ConfigurationStrings.TripleDesSha256;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic256Sha256Rsa15)
                    retval = ConfigurationStrings.Basic256Sha256Rsa15;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic192Sha256Rsa15)
                    retval = ConfigurationStrings.Basic192Sha256Rsa15;
                else if (securityAlgorithm == SecurityAlgorithmSuite.Basic128Sha256Rsa15)
                    retval = ConfigurationStrings.Basic128Sha256Rsa15;
                else if (securityAlgorithm == SecurityAlgorithmSuite.TripleDesSha256Rsa15)
                    retval = ConfigurationStrings.TripleDesSha256Rsa15;
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.ConfigInvalidClassInstanceValue, typeof(SecurityAlgorithmSuite).FullName)));

                return retval;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
