//------------------------------------------------------------------------------
// <copyright file="RegisteredDisposeScript.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.Diagnostics;

    public sealed class RegisteredDisposeScript {
        private Control _control;
        private UpdatePanel _parentUpdatePanel;
        private string _script;

        internal RegisteredDisposeScript(Control control, string disposeScript, UpdatePanel parentUpdatePanel) {
            Debug.Assert(control != null);
            Debug.Assert(disposeScript != null);
            Debug.Assert(parentUpdatePanel != null);
            _control = control;
            _script = disposeScript;
            _parentUpdatePanel = parentUpdatePanel;
        }

        public Control Control {
            get {
                return _control;
            }
        }

        public string Script {
            get {
                return _script;
            }
        }

        internal UpdatePanel ParentUpdatePanel {
            get {
                return _parentUpdatePanel;
            }
        }
    }
}
