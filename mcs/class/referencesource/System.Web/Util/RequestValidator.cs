//------------------------------------------------------------------------------
// <copyright file="RequestValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Base class providing extensibility hooks for custom request validation
 *
 * Copyright (c) 2009 Microsoft Corporation
 */

namespace System.Web.Util {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;

    public class RequestValidator {

        private static RequestValidator _customValidator;

        private static readonly Lazy<RequestValidator> _customValidatorResolver =
            new Lazy<RequestValidator>(GetCustomValidatorFromConfig);

        public static RequestValidator Current {
            get {
                if (_customValidator == null) {
                    _customValidator = _customValidatorResolver.Value;
                }
                return _customValidator;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                _customValidator = value;
            }
        }

        private static RequestValidator GetCustomValidatorFromConfig() {
            // App since this is static per AppDomain
            RuntimeConfig config = RuntimeConfig.GetAppConfig();
            HttpRuntimeSection runtimeSection = config.HttpRuntime;
            string validatorTypeName = runtimeSection.RequestValidationType;

            // validate the type
            Type validatorType = ConfigUtil.GetType(validatorTypeName, "requestValidationType", runtimeSection);
            ConfigUtil.CheckBaseType(typeof(RequestValidator) /* expectedBaseType */, validatorType, "requestValidationType", runtimeSection);

            // instantiate
            RequestValidator validator = (RequestValidator)HttpRuntime.CreatePublicInstanceByWebObjectActivator(validatorType);
            return validator;
        }

        internal static void InitializeOnFirstRequest() {
            // instantiate the validator if it hasn't already been created
            RequestValidator validator = _customValidatorResolver.Value;
        }

        // Public entry point to the IsValidRequestString method. That method shipped protected, and making it public would
        // unfortunately be a breaking change. Having a public entry point allows third parties to write wrapper classes
        // around RequestValidator instances.
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
                 Justification = "This is an appropriate way to return multiple pieces of data.")]
        public bool InvokeIsValidRequestString(HttpContext context, string value, RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex) {
            return IsValidRequestString(context, value, requestValidationSource, collectionKey, out validationFailureIndex);
        }

        private static bool IsAtoZ(char c) {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
                 Justification = "This is an appropriate way to return multiple pieces of data.")]
        protected internal virtual bool IsValidRequestString(HttpContext context, string value, RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex) {
            if (requestValidationSource == RequestValidationSource.Headers) {
                validationFailureIndex = 0;
                return true; // Ignore Headers collection in the default implementation
            }
            return !CrossSiteScriptingValidation.IsDangerousString(value, out validationFailureIndex);
        }

    }
}
