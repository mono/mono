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

    public class ModelClientValidationRule {

        private readonly Dictionary<string, object> _validationParameters = new Dictionary<string, object>();
        private string _validationType;

        public string ErrorMessage {
            get;
            set;
        }

        public IDictionary<string, object> ValidationParameters {
            get {
                return _validationParameters;
            }
        }

        public string ValidationType {
            get {
                return _validationType ?? String.Empty;
            }
            set {
                _validationType = value;
            }
        }

    }
}
