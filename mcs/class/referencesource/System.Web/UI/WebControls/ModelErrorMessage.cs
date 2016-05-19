//------------------------------------------------------------------------------
// <copyright file="ModelErrorMessage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Linq;
    using System.Web;
    using System.Web.ModelBinding;

    /// <summary>
    ///     Displays the first model error for a given key from the page's model state
    /// </summary>
    [
    ToolboxData("<{0}:ModelErrorMessage runat=\"server\" Key=\"ModelStateKey\"></{0}:ModelErrorMessage>"),
    DefaultProperty("Key"),
    ParseChildren(true),
    PersistChildren(false)
    ]
    public class ModelErrorMessage : Label {
        [
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.ModelErrorMessage_ModelStateKey),
        DefaultValue("")
        ]
        public string ModelStateKey {
            get {
                object o = ViewState["ModelStateKey"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["ModelStateKey"] = value;
            }
        }

        [
        DefaultValue(""),
        IDReferenceProperty,
        WebCategory("Behavior"),
        WebSysDescription(SR.ModelErrorMessage_AssociatedControlID),
        Themeable(false)
        ]
        public override string AssociatedControlID {
            get {
                return base.AssociatedControlID;
            }
            set {
                base.AssociatedControlID = value;
            }
        }

        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(false),
        WebSysDescription(SR.ModelErrorMessage_SetFocusOnError)
        ]
        public bool SetFocusOnError {
            get {
                object o = ViewState["SetFocusOnError"];
                return ((o == null) ? false : (bool)o);
            }
            set {
                ViewState["SetFocusOnError"] = value;
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public override string Text {
            // We don't want the Text property exposed to the developer as we're going to set it 
            // ourselves to the value from the model state. We don't want to store it in ViewState
            // because we want it re-evaluated on every render anyway, so storing in ViewState is
            // just overhead.
            get;
            set;
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            ModelState modelState;
            if (Page != null && Page.ModelState.TryGetValue(ModelStateKey, out modelState)) {
                ModelError error = modelState.Errors.FirstOrDefault(modelError => !String.IsNullOrEmpty(modelError.ErrorMessage));
                if (error != null) {
                    Text = HttpUtility.HtmlEncode(error.ErrorMessage);

                    // Render the script to set focus if there is a model error for the specified key
                    if (SetFocusOnError) {
                        var validateId = AssociatedControlID;
                        if (!String.IsNullOrEmpty(validateId)) {
                            var control = FindControl(validateId);
                            if (control != null) {
                                validateId = control.ClientID;
                            }

                            Page.SetValidatorInvalidControlFocus(validateId);
                        }
                    }
                }
            }
        }
    }
}
