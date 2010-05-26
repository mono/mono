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

    public class ModelValidationResult {

        private string _memberName;
        private string _message;

        public string MemberName {
            get {
                return _memberName ?? String.Empty;
            }
            set {
                _memberName = value;
            }
        }

        public string Message {
            get {
                return _message ?? String.Empty;
            }
            set {
                _message = value;
            }
        }

    }
}
