//------------------------------------------------------------------------------
// <copyright file="ExceptionUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System;
    using System.Xml;
    using System.Configuration.Internal;

    static internal class ExceptionUtil {
        static internal ArgumentException ParameterInvalid(string parameter) {
            return new ArgumentException(SR.GetString(SR.Parameter_Invalid, parameter), parameter);
        }

        static internal ArgumentException ParameterNullOrEmpty(string parameter) {
            return new ArgumentException(SR.GetString(SR.Parameter_NullOrEmpty, parameter), parameter);
        }

        static internal ArgumentException PropertyInvalid(string property) {
            return new ArgumentException(SR.GetString(SR.Property_Invalid, property), property);
        }

        static internal ArgumentException PropertyNullOrEmpty(string property) {
            return new ArgumentException(SR.GetString(SR.Property_NullOrEmpty, property), property);
        }

        static internal InvalidOperationException UnexpectedError(string methodName) {
            return new InvalidOperationException(SR.GetString(SR.Unexpected_Error, methodName));
        }

        static internal string NoExceptionInformation {
            get {
                return SR.GetString(SR.No_exception_information_available);
            }
        }

        static internal ConfigurationErrorsException WrapAsConfigException(string outerMessage, Exception e, IConfigErrorInfo errorInfo) {
            if (errorInfo != null) {
                return WrapAsConfigException(outerMessage, e, errorInfo.Filename, errorInfo.LineNumber);
            }
            else {
                return WrapAsConfigException(outerMessage, e, null, 0);
            }
        }

        static internal ConfigurationErrorsException WrapAsConfigException(string outerMessage, Exception e, string filename, int line) {
            //
            // Preserve ConfigurationErrorsException
            //
            ConfigurationErrorsException ce = e as ConfigurationErrorsException;
            if (ce != null) {
                return ce;
            }

            //
            // Promote deprecated ConfigurationException to ConfigurationErrorsException
            //
            ConfigurationException deprecatedException = e as ConfigurationException;
            if (deprecatedException != null) {
                return new ConfigurationErrorsException(deprecatedException);
            }

            //
            // For XML exceptions, preserve the text of the exception in the outer message.
            //
            XmlException xe = e as XmlException;
            if (xe != null) {
                if (xe.LineNumber != 0) {
                    line = xe.LineNumber;
                }

                return new ConfigurationErrorsException(xe.Message, xe, filename, line);
            }

            //
            // Wrap other exceptions in an inner exception, and give as much info as possible
            //
            if (e != null) {
                return new ConfigurationErrorsException(
                        SR.GetString(SR.Wrapped_exception_message, outerMessage, e.Message),
                        e,
                        filename,
                        line);
            }

            //
            // If there is no exception, create a new exception with no further information.
            //
            return new ConfigurationErrorsException(
                    SR.GetString(SR.Wrapped_exception_message, outerMessage, ExceptionUtil.NoExceptionInformation),
                    filename,
                    line);
        }
    }
}

