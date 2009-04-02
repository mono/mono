/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class ModelBindingContext {

        private static readonly Predicate<string> _defaultPropertyFilter = _ => true;

        private string _modelName;
        private ModelStateDictionary _modelState;
        private Predicate<string> _propertyFilter;

        public bool FallbackToEmptyPrefix {
            get;
            set;
        }

        public object Model {
            get;
            set;
        }

        public string ModelName {
            get {
                if (_modelName == null) {
                    _modelName = String.Empty;
                }
                return _modelName;
            }
            set {
                _modelName = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "The containing type is mutable.")]
        public ModelStateDictionary ModelState {
            get {
                if (_modelState == null) {
                    _modelState = new ModelStateDictionary();
                }
                return _modelState;
            }
            set {
                _modelState = value;
            }
        }

        public Type ModelType {
            get;
            set;
        }

        public Predicate<string> PropertyFilter {
            get {
                if (_propertyFilter == null) {
                    _propertyFilter = _defaultPropertyFilter;
                }
                return _propertyFilter;
            }
            set {
                _propertyFilter = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "The containing type is mutable.")]
        public IDictionary<string, ValueProviderResult> ValueProvider {
            get;
            set;
        }

    }
}
