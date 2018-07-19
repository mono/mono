//------------------------------------------------------------------------------
// <copyright file="PropertyInformation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Configuration {

    // PropertyInformation
    //
    // Contains information about a property
    //
    public sealed class PropertyInformation {
        
        private ConfigurationElement ThisElement = null;
        private string PropertyName;
        private ConfigurationProperty _Prop = null;
        private const string LockAll = "*";
        
        private ConfigurationProperty Prop {
            get {
                if (_Prop == null) {
                    _Prop = ThisElement.Properties[PropertyName];
                }
                return _Prop;
            }
        }

        internal PropertyInformation(ConfigurationElement thisElement, string propertyName) {
            PropertyName = propertyName;
            ThisElement = thisElement;
        }

        
        public string Name {
            get {
                return PropertyName;
            }
        }


        internal string ProvidedName {
            get {
                return Prop.ProvidedName;
            }
        }


        public object Value {
            get {
                return ThisElement[PropertyName];
            }
            set {
                ThisElement[PropertyName] = value;
            }
        }

        // DefaultValue
        //
        // What is the default value for this property
        //
        public object DefaultValue {
            get {
                return Prop.DefaultValue;
            }
        }

        // ValueOrigin
        //
        // Where was the property retrieved from
        //
        public PropertyValueOrigin ValueOrigin {
            get {
                if (ThisElement.Values[PropertyName] == null) {
                    return PropertyValueOrigin.Default;
                }
                if (ThisElement.Values.IsInherited(PropertyName)) {
                    return PropertyValueOrigin.Inherited;
                }
                return PropertyValueOrigin.SetHere;
            }
        }

        // IsModified
        //
        // Was the property Modified
        //
        public bool IsModified {
            get {
                if (ThisElement.Values[PropertyName] == null) {
                    return false;
                }
                if (ThisElement.Values.IsModified(PropertyName)) {
                    return true;
                }
                return false;
            }
        }
        // IsKey
        //
        // Is this property a key?
        //
        public bool IsKey {
            get {
                return Prop.IsKey;
            }
        }

        // IsRequired
        //
        // Is this property required?
        //
        public bool IsRequired {
            get {
                return Prop.IsRequired;
            }
        }

        // IsLocked
        //
        // Is this property locked?
        //
        public bool IsLocked {
            get {
                return ((ThisElement.LockedAllExceptAttributesList != null && !ThisElement.LockedAllExceptAttributesList.DefinedInParent(PropertyName)) ||
                    (ThisElement.LockedAttributesList != null && 
                        (ThisElement.LockedAttributesList.DefinedInParent(PropertyName) || 
                         ThisElement.LockedAttributesList.DefinedInParent(LockAll))) ||
                        (((ThisElement.ItemLocked & ConfigurationValueFlags.Locked)    != 0) &&
                         ((ThisElement.ItemLocked & ConfigurationValueFlags.Inherited) != 0)));
            }
        }

        // Source
        //
        // What is the source file where this data came from
        //
        public string Source {
            get {
                PropertySourceInfo psi = ThisElement.Values.GetSourceInfo(PropertyName);
                if (psi == null) {
                    psi = ThisElement.Values.GetSourceInfo(String.Empty);
                }
                if (psi == null) {
                    return String.Empty;
                }
                return psi.FileName;
            }
        }

        // LineNumber
        //
        // What is the line number associated with the source
        //
        // Note:
        //   1 is the first line in the file.  0 is returned when there is no 
        //   source
        //
        public int LineNumber {
            get {
                PropertySourceInfo psi = ThisElement.Values.GetSourceInfo(PropertyName);
                if (psi == null) {
                    psi = ThisElement.Values.GetSourceInfo(String.Empty);
                }
                if (psi == null) {
                    return 0;
                }
                return psi.LineNumber;
            }
        }

        // Type
        //
        // What is the type for the property
        //
        public Type Type {
            get {
                return Prop.Type;
            }
        }

        // Validator
        //
        public ConfigurationValidatorBase Validator {
            get {
                return Prop.Validator;
            }
        }

        // Converter
        //
        public TypeConverter Converter {
            get {
                return Prop.Converter;
            }
        }

        // Property description ( comments etc )
        public string Description {
            get {
                return Prop.Description;
            }
        }
    }
}
