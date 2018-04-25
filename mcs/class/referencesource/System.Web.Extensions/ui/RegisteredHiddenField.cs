//------------------------------------------------------------------------------
// <copyright file="RegisteredHiddenField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.Diagnostics;

    public sealed class RegisteredHiddenField {
        private Control _control;
        private string _name;
        private string _initialValue;

        internal RegisteredHiddenField(Control control, string hiddenFieldName, string hiddenFieldInitialValue) {
            Debug.Assert(control != null);
            Debug.Assert(hiddenFieldName != null);
            _control = control;
            _name = hiddenFieldName;
            _initialValue = hiddenFieldInitialValue;
        }

        public Control Control {
            get {
                return _control;
            }
        }

        public string InitialValue {
            get {
                // may be null
                return _initialValue;
            }
        }

        public string Name {
            get {
                return _name;
            }
        }
    }
}
