//------------------------------------------------------------------------------
// <copyright file="SectionInput.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace System.Configuration {

    [System.Diagnostics.DebuggerDisplay("SectionInput {_sectionXmlInfo.ConfigKey}")]
    internal class SectionInput {
        // result can be null, so we use this object to indicate whether it has been evaluated
        private static object                   s_unevaluated = new object();

        // input from the XML file
        private SectionXmlInfo                  _sectionXmlInfo;        

        // Provider to use for encryption
        private ProtectedConfigurationProvider  _protectionProvider;    

        // Has the protection provider been determined for this input?
        private bool                            _isProtectionProviderDetermined;    

        // the result of evaluating this section
        private object                          _result;                

        // the result of evaluating this section after GetRuntimeObject is called
        private object                          _resultRuntimeObject;   

        // accummulated errors related to this input
        private List<ConfigurationException>    _errors;

        internal SectionInput(SectionXmlInfo sectionXmlInfo, List<ConfigurationException> errors) {
            _sectionXmlInfo = sectionXmlInfo;
            _errors = errors;

            _result = s_unevaluated;
            _resultRuntimeObject = s_unevaluated;
        }

        internal SectionXmlInfo SectionXmlInfo {
            get {return _sectionXmlInfo;}
        }

        internal bool HasResult {
            get {return _result != s_unevaluated;}
        }

        internal bool HasResultRuntimeObject {
            get {return _resultRuntimeObject != s_unevaluated;}
        }

        internal object Result {
            get {
                // Useful assert, but it fires in the debugger when using automatic property evaluation
                // Debug.Assert(_result != s_unevaluated, "_result != s_unevaluated");

                return _result;
            }

            set {_result = value;}
        }

        internal object ResultRuntimeObject {
            get {
                // Useful assert, but it fires in the debugger when using automatic property evaluation
                // Debug.Assert(_resultRuntimeObject != s_unevaluated, "_resultRuntimeObject != s_unevaluated");

                return _resultRuntimeObject;
            }

            set {_resultRuntimeObject = value;}
        }

        internal void ClearResult() {
            _result = s_unevaluated;
            _resultRuntimeObject = s_unevaluated;
        }

        internal bool IsProtectionProviderDetermined {
            get {return _isProtectionProviderDetermined;}
        }

        internal ProtectedConfigurationProvider ProtectionProvider {
            get {return _protectionProvider;}
            set { 
                _protectionProvider = value;
                _isProtectionProviderDetermined = true;
            }
        }

        // Errors associated with a section input.
        internal ICollection<ConfigurationException> Errors {
            get {
                return _errors;
            }
        }

        internal bool HasErrors {
            get {
                return ErrorsHelper.GetHasErrors(_errors);
            }
        }

        internal void ThrowOnErrors() {
            ErrorsHelper.ThrowOnErrors(_errors);
        }
    }
}
