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
    using System.Collections.ObjectModel;

    public class FieldValidationMetadata {

        private string _fieldName;
        private readonly Collection<ModelClientValidationRule> _validationRules = new Collection<ModelClientValidationRule>();

        public string FieldName {
            get {
                return _fieldName ?? String.Empty;
            }
            set {
                _fieldName = value;
            }
        }

        public bool ReplaceValidationMessageContents {
            get;
            set;
        }

        public string ValidationMessageId {
            get;
            set;
        }

        public ICollection<ModelClientValidationRule> ValidationRules {
            get {
                return _validationRules;
            }
        }

    }
}
