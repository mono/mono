//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.Runtime;
using System.ServiceModel;
using System.ServiceModel.Diagnostics;

namespace System.ServiceModel
{
    /// <summary>
    /// Defines the mapping to be used for translating exceptions to faults.
    /// </summary>
    public class ExceptionMapper
    {
        internal const string SoapSenderFaultCode = "Sender";

        /// <summary>
        /// ExceptionMapper constructor.
        /// </summary>
        public ExceptionMapper()
        {
        }

        /// <summary>
        /// Translates the input exception to a fault using the mapping defined in ExceptionMap.
        /// </summary>
        /// <param name="ex">The exception to be mapped to a fault.</param>
        /// <returns>The fault corresponding to the input exception.</returns>
        public virtual FaultException FromException(Exception ex)
        {
            return FromException(ex, String.Empty, String.Empty);
        }

        /// <summary>
        /// Translates the input exception to a fault using the mapping defined in ExceptionMap.
        /// </summary>
        /// <param name="ex">The exception to be mapped to a fault.</param>
        /// <param name="soapNamespace">The SOAP Namespace to be used when generating the mapped fault.</param>
        /// <param name="trustNamespace">The WS-Trust Namespace to be used when generating the mapped fault.</param>
        /// <returns>The fault corresponding to the input exception.</returns>
        public virtual FaultException FromException(Exception ex, string soapNamespace, string trustNamespace)
        {
            return null;
        }

        /// <summary>
        /// Determines whether an exception that occurred during the processing of a security token 
        /// should be handled using the defined ExceptionMap.
        /// </summary>
        /// <param name="ex">The input exception.</param>
        /// <returns>A boolean value indicating whether the exception should be handled using the defined ExceptionMap.</returns>
        public virtual bool HandleSecurityTokenProcessingException(Exception ex)
        {
            if (Fx.IsFatal(ex))
            {
                return false;
            }

            if (ex is FaultException)
            {
                // Just throw the original exception.
                return false;
            }
            else
            {
                FaultException faultException = FromException(ex);
                if (faultException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(faultException);
                }

                // The exception is not one of the recognized exceptions. Just throw the original exception.
                return false;
            }
        }
    }
}
