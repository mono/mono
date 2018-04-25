//------------------------------------------------------------------------------
// <copyright file="ElementInformation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Configuration  
{

    // ElementInformation
    //
    // Expose information on Configuration Elements, and the 
    // properties that they contain
    //
    public sealed class ElementInformation
    {
        private ConfigurationElement           _thisElement;
        private PropertyInformationCollection  _internalProperties;
        private ConfigurationException[]       _errors;

        internal ElementInformation(ConfigurationElement thisElement) {
            _thisElement = thisElement;
        }

        // Properties
        //
        // Retrieve Collection of properties within this element
        //
        public PropertyInformationCollection Properties 
        { 
            get {
                if (_internalProperties == null) {
                    _internalProperties = new PropertyInformationCollection(_thisElement);
                }

                return _internalProperties;
            }
        }

        // IsInherited
        //
        // Was this element inheritted, or was the property actually
        // set here
        //
        public bool IsPresent {
            get {
                return _thisElement.ElementPresent;
            }
        }

        // IsLocked
        //
        // Is this property locked?
        //
        public bool IsLocked {
            get {
                return (((_thisElement.ItemLocked & ConfigurationValueFlags.Locked) != 0) &&
                        ((_thisElement.ItemLocked & ConfigurationValueFlags.Inherited) != 0));
            }
        }

        // IsCollection
        //
        // Is this element a collection?
        //
        public bool IsCollection {
            get {
                ConfigurationElementCollection collection = _thisElement as ConfigurationElementCollection;
                if (collection == null) { // Try the default collection
                    if (_thisElement.Properties.DefaultCollectionProperty != null) { // this is not a collection but it may contain a default collection
                        collection = _thisElement[_thisElement.Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                    }
                }

                return (collection != null);
            }
        }

        // Internal method to fix SetRawXML defect...
        internal PropertySourceInfo PropertyInfoInternal() {
            return _thisElement.PropertyInfoInternal(_thisElement.ElementTagName);
        }

        internal void ChangeSourceAndLineNumber(PropertySourceInfo sourceInformation) {
            _thisElement.Values.ChangeSourceInfo(_thisElement.ElementTagName, sourceInformation);
        }

        // Source
        //
        // What is the source file where this data came from
        //
        public string Source {
            get {
                PropertySourceInfo psi = _thisElement.Values.GetSourceInfo(_thisElement.ElementTagName);

                if (psi == null) {
                    return null;
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
                PropertySourceInfo psi = _thisElement.Values.GetSourceInfo(_thisElement.ElementTagName);
                if (psi == null) {
                    return 0;
                }

                return psi.LineNumber;
            }
        }

        // Type
        //
        // What is the type for the element
        //
        public Type Type {
            get {
                return _thisElement.GetType();
            }
        }
        
        // Validator
        //
        // What is the validator to validate the element?
        //
        public ConfigurationValidatorBase Validator {
            get {
                return _thisElement.ElementProperty.Validator;
            }
        }

        // GetReadOnlyErrorsList
        //
        // Get a Read Only list of the exceptions for this 
        // element
        //
        private ConfigurationException[] GetReadOnlyErrorsList() {
            ArrayList                arrayList;
            int                      count;
            ConfigurationException[] exceptionList;
            
            arrayList = _thisElement.GetErrorsList();
            count     = arrayList.Count;

            // Create readonly array
            exceptionList = new ConfigurationException[arrayList.Count];

            if (count != 0) {
                arrayList.CopyTo(exceptionList, 0);
            }

            return exceptionList;
        }
        
        // Errors
        //
        // Retrieve the _errors for this element and sub elements
        //
        public ICollection Errors {
            get {
                if (_errors == null) {
                    _errors = GetReadOnlyErrorsList();
                }

                return _errors;
            }
        }
    }
}
