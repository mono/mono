//------------------------------------------------------------------------------
// <copyright file="RegisteredArrayDeclaration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.Diagnostics;

    public sealed class RegisteredArrayDeclaration {
        private Control _control;
        private string _name;
        private string _value;

        internal RegisteredArrayDeclaration(Control control, string arrayName, string arrayValue) {
            Debug.Assert(arrayName != null);
            // null value allowed by asp.net
            _control = control;
            _name = arrayName;
            _value = arrayValue;
        }

        public string Name {
            get {
                return _name;
            }
        }

        public string Value {
            get {
                // may be null
                return _value;
            }
        }

        public Control Control {
            get {
                return _control;
            }
        }
    }
}
