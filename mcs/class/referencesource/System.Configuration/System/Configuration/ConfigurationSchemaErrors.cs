//------------------------------------------------------------------------------
// <copyright file="ConfigurationSchemaErrors.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text;

    internal class ConfigurationSchemaErrors {
        // Errors with ExceptionAction.Local are logged to this list.
        // This list is reset when processing of a section is complete.
        // Errors on this list may be added to the _errorsAll list
        // when RetrieveAndResetLocalErrors is called.
        private List<ConfigurationException> _errorsLocal;

        // Errors with ExceptionAction.Global are logged to this list.
        private List<ConfigurationException> _errorsGlobal;

        // All errors related to a config file are logged to this list.
        // This includes all global errors, all non-specific errors,
        // and local errors for input that applies to this config file.
        private List<ConfigurationException> _errorsAll;

        internal ConfigurationSchemaErrors() {}

        internal bool HasLocalErrors {
            get {
                return ErrorsHelper.GetHasErrors(_errorsLocal);
            }
        }

        internal bool HasGlobalErrors {
            get {
                return ErrorsHelper.GetHasErrors(_errorsGlobal);
            }
        }

        private bool HasAllErrors {
            get {
                return ErrorsHelper.GetHasErrors(_errorsAll);
            }
        }

        internal int GlobalErrorCount {
            get {
                return ErrorsHelper.GetErrorCount(_errorsGlobal);
            }
        }

        //
        // Add a configuration Error.
        //
        internal void AddError(ConfigurationException ce, ExceptionAction action) {
            switch (action) {
                case ExceptionAction.Global:
                    ErrorsHelper.AddError(ref _errorsAll, ce);
                    ErrorsHelper.AddError(ref _errorsGlobal, ce);
                    break;

                case ExceptionAction.NonSpecific:
                    ErrorsHelper.AddError(ref _errorsAll, ce);
                    break;

                case ExceptionAction.Local:
                    ErrorsHelper.AddError(ref _errorsLocal, ce);
                    break;
            }
        }

        internal void SetSingleGlobalError(ConfigurationException ce) {
            _errorsAll = null;
            _errorsLocal = null;
            _errorsGlobal = null;

            AddError(ce, ExceptionAction.Global);
        }

        internal bool HasErrors(bool ignoreLocal) {
            if (ignoreLocal) {
                return HasGlobalErrors;
            }
            else {
                return HasAllErrors;
            }
        }

        // ThrowIfErrors
        //
        // Throw if Errors were detected and remembered.
        //
        // Parameters:
        //   IgnoreLocal - Should we be using the local errors also to
        //                  detemine if we should throw?
        //
        // Note: We will always return all the errors, no matter what
        //       IgnoreLocal is.
        //
        internal void ThrowIfErrors(bool ignoreLocal) {
            if (HasErrors(ignoreLocal)) {
                if (HasGlobalErrors) {
                    // Throw just the global errors, as they invalidate
                    // all other config file parsing.
                    throw new ConfigurationErrorsException(_errorsGlobal);
                }
                else {
                    // Throw all errors no matter what
                    throw new ConfigurationErrorsException(_errorsAll);
                }
            }
        }

        // RetrieveAndResetLocalErrors
        //
        // Retrieve the Local Errors, and Reset them to none.
        //
        internal List<ConfigurationException> RetrieveAndResetLocalErrors(bool keepLocalErrors) {
            List<ConfigurationException> list = _errorsLocal;
            _errorsLocal = null;

            if (keepLocalErrors) {
                ErrorsHelper.AddErrors(ref _errorsAll, list);
            }

            return list;
        }

        //
        // Add errors that have been saved for a specific section.
        //
        internal void AddSavedLocalErrors(ICollection<ConfigurationException> coll) {
            ErrorsHelper.AddErrors(ref _errorsAll, coll);
        }

        // ResetLocalErrors
        //
        // Remove all the Local Errors, so we can start from scratch
        //
        internal void ResetLocalErrors() {
            RetrieveAndResetLocalErrors(false);
        }
    }
}
