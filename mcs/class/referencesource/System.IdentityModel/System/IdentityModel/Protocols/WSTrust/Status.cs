//-----------------------------------------------------------------------
// <copyright file="Status.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    /// <summary>
    /// A class encapsulating the result of a WS-Trust request.
    /// </summary>
    public class Status
    {
        string _code;
        string _reason;

        /// <summary>
        /// Creates an instance of Status
        /// </summary>
        /// <param name="code">Status code.</param>
        /// <param name="reason">Optional status reason.</param>
        public Status(string code, string reason)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("code");
            }

            _code = code;
            _reason = reason;
        }

        /// <summary>
        /// Gets or sets the status code for the validation binding in the RSTR.
        /// </summary>
        public string Code
        {
            get
            {
                return _code;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("code");
                }

                _code = value;
            }
        }

        /// <summary>
        /// Gets or sets the optional status reason for the validation binding in the RSTR.
        /// </summary>
        public string Reason
        {
            get
            {
                return _reason;
            }

            set
            {
                _reason = value;
            }
        }
    }
}
