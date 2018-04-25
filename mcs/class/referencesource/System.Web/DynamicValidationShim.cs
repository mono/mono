//------------------------------------------------------------------------------
// <copyright file="DynamicValidationShim.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// This is a special class used by Microsoft.Web.Infrastructure.dll to interface with the request validation system.
// M.W.I was created before the existence of this class and has its own implementation of granular request validation,
// but it uses reflection to look for this class. If this class exists, M.W.I calls the methods defined on this class
// rather than using its own internal implementation.

namespace Microsoft.Web.Infrastructure.DynamicValidationHelper {
    using System;
    using System.Collections.Specialized;
    using System.Web;

    internal static class DynamicValidationShim {

        // Enables granular request validation for the current request.
        internal static void EnableDynamicValidation(HttpContext context) {
            // Because .NET 4.5 is an in-place update granular request validation is disabled by default for back-compat
            // reasons (request validation defaults to the 4.0 behavior).
            // We need to enable it for the current request so that MVC 3 and Web Pages 1 continue to work on .NET 4.5.
            context.Request.EnableGranularRequestValidation();
        }

        // Returns a value indicating whether request validation was ever turned on for this request.
        internal static bool IsValidationEnabled(HttpContext context) {
            return context.Request.ValidateInputWasCalled;
        }

        // Given an HttpContext object, provides access to the unvalidated Form and QueryString collections.
        internal static void GetUnvalidatedCollections(HttpContext context, out Func<NameValueCollection> formGetter, out Func<NameValueCollection> queryStringGetter) {
            UnvalidatedRequestValues unvalidated = context.Request.Unvalidated;
            formGetter = () => unvalidated.Form;
            queryStringGetter = () => unvalidated.QueryString;
        }

    }
}
