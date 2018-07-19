//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.Configuration;

    /// <summary>
    /// This is the Management utility class.
    /// Adding Xml
    /// </summary>
    static partial class DiagnosticUtility
    {
        public static Exception ThrowHelperArgumentNullOrEmptyString(string arg)
        {
            return ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ID0006), arg));
        }

        public static Exception ThrowHelperArgumentOutOfRange(string arg)
        {
            return ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(arg));
        }

        public static Exception ThrowHelperArgumentOutOfRange(string arg, string message)
        {
            return ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(arg, message));
        }

        public static Exception ThrowHelperArgumentOutOfRange(string arg, object actualValue, string message)
        {
            return ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(arg, actualValue, message));
        }

        public static Exception ThrowHelperConfigurationError(ConfigurationElement configElement, string propertyName, Exception inner)
        {
            //
            // ASP.NET does not properly show the inner exception in the 
            // error page or event log in the case of a 
            // ConfigurationErrorsException. To work around this, we will 
            // include the inner exception message in our message.
            //
            if (null == inner)
            {
                throw ExceptionUtility.ThrowHelperArgumentNull("inner");
            }

            if (null == configElement)
            {
                throw ExceptionUtility.ThrowHelperArgumentNull("configElement");
            }

            if (null == propertyName)
            {
                throw ExceptionUtility.ThrowHelperArgumentNull("propertyName");
            }

            if (null == configElement.ElementInformation)
            {
                throw ExceptionUtility.ThrowHelperArgument("configElement", SR.GetString(SR.ID0003, "configElement.ElementInformation"));
            }

            if (null == configElement.ElementInformation.Properties)
            {
                throw ExceptionUtility.ThrowHelperArgument("configElement", SR.GetString(SR.ID0003, "configElement.ElementInformation.Properties"));
            }

            if (null == configElement.ElementInformation.Properties[propertyName])
            {
                throw ExceptionUtility.ThrowHelperArgument("configElement", SR.GetString(SR.ID0005, "configElement.ElementInformation.Properties", propertyName));
            }

            return ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                SR.GetString(SR.ID1024, propertyName, inner.Message),
                inner,
                configElement.ElementInformation.Properties[propertyName].Source,
                configElement.ElementInformation.Properties[propertyName].LineNumber));
        }

        public static Exception ThrowHelperConfigurationError(ConfigurationElement configElement, string propertyName, string message)
        {
            if (null == configElement)
            {
                throw ExceptionUtility.ThrowHelperArgumentNull("configElement");
            }

            if (null == propertyName)
            {
                throw ExceptionUtility.ThrowHelperArgumentNull("propertyName");
            }

            if (null == configElement.ElementInformation)
            {
                throw ExceptionUtility.ThrowHelperArgument("configElement", SR.GetString(SR.ID0003, "configElement.ElementInformation"));
            }

            if (null == configElement.ElementInformation.Properties)
            {
                throw ExceptionUtility.ThrowHelperArgument("configElement", SR.GetString(SR.ID0003, "configElement.ElementInformation.Properties"));
            }

            if (null == configElement.ElementInformation.Properties[propertyName])
            {
                throw ExceptionUtility.ThrowHelperArgument("configElement", SR.GetString(SR.ID0005, "configElement.ElementInformation.Properties", propertyName));
            }

            return ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                message,
                configElement.ElementInformation.Properties[propertyName].Source,
                configElement.ElementInformation.Properties[propertyName].LineNumber));
        }

        public static Exception ThrowHelperXml(XmlReader reader, string message)
        {
            return ThrowHelperXml(reader, message, null);
        }

        public static Exception ThrowHelperXml(XmlReader reader, string message, Exception inner)
        {
            IXmlLineInfo lineInfo = reader as IXmlLineInfo;
            return ExceptionUtility.ThrowHelperError(new XmlException(
                message,
                inner,
                (null != lineInfo) ? lineInfo.LineNumber : 0,
                (null != lineInfo) ? lineInfo.LinePosition : 0));
        }

        public static Exception ThrowHelperInvalidOperation(string message)
        {
            return ExceptionUtility.ThrowHelperError(new InvalidOperationException(message));
        }
    }
}
