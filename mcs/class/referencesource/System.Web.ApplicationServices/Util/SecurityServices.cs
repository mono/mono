//------------------------------------------------------------------------------
// <copyright file="SecUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {

    using System;
    using System.Globalization;

    internal static class SecurityServices {

        // We don't trim the param before checking with password parameters
        internal static void CheckPasswordParameter(string param, string paramName) {
            if (param == null) {
                throw new ArgumentNullException(paramName);
            }

            CheckForEmptyParameter(param, paramName);
        }

        internal static void CheckForEmptyOrWhiteSpaceParameter(ref string param, string paramName) {
            if (param == null) {
                return;
            }

            param = param.Trim();
            CheckForEmptyParameter(param, paramName);            
        }

        internal static void CheckForEmptyParameter(string param, string paramName) {
            if (param.Length < 1) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ApplicationServicesStrings.Parameter_can_not_be_empty, paramName), paramName);
            }
        }
    }
}
