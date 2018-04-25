//------------------------------------------------------------------------------
// <copyright file="SectionRecord.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using System.Threading;
    using System.Reflection;
    using System.Xml;

    [System.Diagnostics.DebuggerDisplay("SectionRecord {ConfigKey}")]
    internal class SectionRecord {
        //
        // Flags constants
        //

        //
        // Runtime flags below 0x10000
        //

        // locked by parent input, either because a parent section is locked,
        // a parent section locks all children, or a location input for this 
        // configPath has allowOverride=false.
        private const int Flag_Locked                               = 0x00000001;

        // lock children of this section
        private const int Flag_LockChildren                         = 0x00000002;

        // propagation of FactoryRecord.IsFactoryTrustedWithoutAptca
        private const int Flag_IsResultTrustedWithoutAptca          = 0x00000004;

        // propagation of FactoryRecord.RequirePermission
        private const int Flag_RequirePermission                    = 0x00000008;

        // Look at AddLocationInput for explanation of this flag's purpose
        private const int Flag_LocationInputLockApplied             = 0x00000010;

        // Look at AddIndirectLocationInput for explanation of this flag's purpose
        private const int Flag_IndirectLocationInputLockApplied     = 0x00000020;

        // The flag gives us the inherited lock mode for this section record without the file input
        // We need this to support SectionInformation.OverrideModeEffective.
        private const int Flag_ChildrenLockWithoutFileInput         = 0x00000040;

        //
        // Designtime flags at or above 0x00010000
        //

        // the section has been added to the update list
        private const int Flag_AddUpdate                            = 0x00010000;

        // result can be null, so we use this object to indicate whether it has been evaluated
        static object                           s_unevaluated = new object();

        private SafeBitVector32                _flags;

        // config key
        private string                          _configKey;

        // The input from location sections
        // This list is ordered to keep oldest ancestors at the front
        private List<SectionInput>              _locationInputs;    

        // The input from this file
        private SectionInput                    _fileInput;

        // This special input is used only when creating a location config record.
        // The inputs are from location tags which are found in the same config file as the
        // location config configPath, but point to the parent paths of the location config
        // configPath.  See the comment for VSWhidbey 540184 in Init() in
        // BaseConfigurationRecord.cs for more details.
        private List<SectionInput>              _indirectLocationInputs;

        // the cached result of evaluating this section
        private object                          _result;
        
        // the cached result of evaluating this section after GetRuntimeObject is called
        private object                          _resultRuntimeObject;


        internal SectionRecord(string configKey) {
            _configKey = configKey;
            _result = s_unevaluated;
            _resultRuntimeObject = s_unevaluated;
        }

        internal string ConfigKey {
            get {return _configKey;}
        }

        internal bool Locked {
            get {return _flags[Flag_Locked];}
        }

        internal bool LockChildren {
            get {return _flags[Flag_LockChildren];}
        }

        internal bool LockChildrenWithoutFileInput {
            get {

                // Start assuming we dont have a file input
                // When we don't have file input the lock mode for children is the same for LockChildren and LockChildrenWithoutFileInput
                bool result = LockChildren;

                if (HasFileInput) {
                    result =  _flags[Flag_ChildrenLockWithoutFileInput];
                }

                return result;
            }
        }

        internal bool IsResultTrustedWithoutAptca {
            get {return _flags[Flag_IsResultTrustedWithoutAptca];}
            set {_flags[Flag_IsResultTrustedWithoutAptca] = value;}
        }

        internal bool RequirePermission {
            get {return _flags[Flag_RequirePermission];}
            set {_flags[Flag_RequirePermission] = value;}
        }

        internal bool AddUpdate {
            get {return _flags[Flag_AddUpdate];}
            set {_flags[Flag_AddUpdate] = value;}
        }

        internal bool HasLocationInputs {
            get {
                return _locationInputs != null && _locationInputs.Count > 0;
            }
        }

        internal List<SectionInput> LocationInputs {
            get {return _locationInputs;}
        }

        internal SectionInput LastLocationInput {
            get {
                if (HasLocationInputs) {
                    return _locationInputs[_locationInputs.Count - 1];
                }
                else {
                    return null;
                }
            }
        }

        internal void 
        AddLocationInput(SectionInput sectionInput) {
            AddLocationInputImpl(sectionInput, false);
        }

        internal bool HasFileInput {
            get {
                return _fileInput != null;
            }
        }

        internal SectionInput FileInput {
            get { return _fileInput; }
        }

        internal void ChangeLockSettings(OverrideMode forSelf, OverrideMode forChildren) {

            if (forSelf != OverrideMode.Inherit) {
                _flags[Flag_Locked]         = (forSelf == OverrideMode.Deny);
                _flags[Flag_LockChildren]   = (forSelf == OverrideMode.Deny);
            }

            if (forChildren != OverrideMode.Inherit) {
                _flags[Flag_LockChildren] = ((forSelf == OverrideMode.Deny) || (forChildren == OverrideMode.Deny));
            }
        }

        // AddFileInput
        internal void AddFileInput(SectionInput sectionInput) {
            Debug.Assert(sectionInput != null);
        
            _fileInput = sectionInput;

            if (!sectionInput.HasErrors) {
                
                // If the file input has an explciti value for its children locking - use it
                // Note we dont change the current lock setting
                if (sectionInput.SectionXmlInfo.OverrideModeSetting.OverrideMode != OverrideMode.Inherit) {

                    // Store the current setting before applying the lock from the file input
                    // So that if the user changes the current OverrideMode on this configKey to "Inherit"
                    // we will know what we are going to inherit ( used in SectionInformation.OverrideModeEffective )
                    // Note that we cannot use BaseConfigurationRecord.ResolveOverrideModeFromParent as it gives us only the lock
                    // resolved up to our immediate parent which does not inlcude normal and indirect location imputs
                    _flags[Flag_ChildrenLockWithoutFileInput] = LockChildren;

                    ChangeLockSettings(OverrideMode.Inherit, sectionInput.SectionXmlInfo.OverrideModeSetting.OverrideMode);
                }
            }
        }

        internal void RemoveFileInput() {
            if (_fileInput != null) {
                _fileInput = null;

                // Reset LockChildren flag to the value provided by 
                // location input or inherited sections.
                _flags[Flag_LockChildren] = Locked;
            }
        }

        internal bool HasIndirectLocationInputs {
            get {
                return _indirectLocationInputs != null && _indirectLocationInputs.Count > 0;
            }
        }

        internal List<SectionInput> IndirectLocationInputs {
            get {return _indirectLocationInputs;}
        }

        internal SectionInput LastIndirectLocationInput {
            get {
                if (HasIndirectLocationInputs) {
                    return _indirectLocationInputs[_indirectLocationInputs.Count - 1];
                }
                else {
                    return null;
                }
            }
        }

        internal void 
        AddIndirectLocationInput(SectionInput sectionInput) {
            AddLocationInputImpl(sectionInput, true);
        }

        private void
        AddLocationInputImpl(SectionInput sectionInput, bool isIndirectLocation) {
            List<SectionInput>  inputs = isIndirectLocation ? 
                                        _indirectLocationInputs : 
                                        _locationInputs;
                                        
            int                 flag = isIndirectLocation ?
                                        Flag_IndirectLocationInputLockApplied :
                                        Flag_LocationInputLockApplied;
            
            if (inputs == null) {
                inputs = new List<SectionInput>(1);
                
                if (isIndirectLocation) {
                    _indirectLocationInputs = inputs;
                }
                else {
                    _locationInputs = inputs;
                }
            }

            // The list of locationSections is traversed from child to parent,
            // so insert at the beginning of the list.
            inputs.Insert(0, sectionInput);

            // Only the overrideMode from the parent thats closest to the SectionRecord has effect
            //
            // For location input:
            // Remember that this method will be called for location inputs comming from the immediate parent first
            // and then walking the hierarchy up to the root level
            //
            // For indirect location input:
            // This method will be first called for indirect input closest to the location config
            if (!sectionInput.HasErrors) {
                
                if (!_flags[flag]) {
                    OverrideMode modeLocation = sectionInput.SectionXmlInfo.OverrideModeSetting.OverrideMode;

                    if (modeLocation != OverrideMode.Inherit) { 
                        ChangeLockSettings(modeLocation, modeLocation);
                        _flags[flag] = true;   
                    }
                }
            }
        }

        internal bool HasInput {
            get {
                return HasLocationInputs || HasFileInput || HasIndirectLocationInputs;
            }
        }

        internal void ClearRawXml() {
            if (HasLocationInputs) {
                foreach (SectionInput locationInput in LocationInputs) {
                    locationInput.SectionXmlInfo.RawXml = null;
                }
            }

            if (HasIndirectLocationInputs) {
                foreach (SectionInput indirectLocationInput in IndirectLocationInputs) {
                    indirectLocationInput.SectionXmlInfo.RawXml = null;
                }
            }

            if (HasFileInput) {
                FileInput.SectionXmlInfo.RawXml = null;
            }
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

            set {
                _resultRuntimeObject = value;
            }
        }

        internal void ClearResult() {
            if (_fileInput != null) {
                _fileInput.ClearResult();
            }

            if (_locationInputs != null) {
                foreach (SectionInput input in _locationInputs) {
                    input.ClearResult();
                }
            }

            _result = s_unevaluated;
            _resultRuntimeObject = s_unevaluated;
        }

        //
        // Error handling.
        //

        private List<ConfigurationException> GetAllErrors() {
            List<ConfigurationException> allErrors = null;

            if (HasLocationInputs) {
                foreach (SectionInput input in LocationInputs) {
                    ErrorsHelper.AddErrors(ref allErrors, input.Errors);
                }
            }

            if (HasIndirectLocationInputs) {
                foreach (SectionInput input in IndirectLocationInputs) {
                    ErrorsHelper.AddErrors(ref allErrors, input.Errors);
                }
            }

            if (HasFileInput) {
                ErrorsHelper.AddErrors(ref allErrors, FileInput.Errors);
            }

            return allErrors;
        }

        internal bool HasErrors {
            get {
                if (HasLocationInputs) {
                    foreach (SectionInput input in LocationInputs) {
                        if (input.HasErrors) {
                            return true;
                        }
                    }
                }

                if (HasIndirectLocationInputs) {
                    foreach (SectionInput input in IndirectLocationInputs) {
                        if (input.HasErrors) {
                            return true;
                        }
                    }
                }

                if (HasFileInput) {
                    if (FileInput.HasErrors) {
                        return true;
                    }
                }

                return false;
            }
        }

        internal void ThrowOnErrors() {
            if (HasErrors) {
                throw new ConfigurationErrorsException(GetAllErrors());
            }
        }
    }
}

