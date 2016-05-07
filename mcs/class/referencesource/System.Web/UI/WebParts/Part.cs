//------------------------------------------------------------------------------
// <copyright file="Part.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web.UI.WebControls;

    /// <devdoc>
    /// Base class for all Part classes in the WebPart framework.  Common to all Part controls are properties that
    /// allow the control to be rendered in a modular fashion.
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.WebParts.PartDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(true),
    PersistChildren(false),
    ]
    public abstract class Part : Panel, INamingContainer, ICompositeControlDesignerAccessor {

        // Prevent class from being subclassed outside of our assembly
        internal Part() {
        }

        /// <devdoc>
        /// The UI state of the part
        /// </devdoc>
        [
        DefaultValue(PartChromeState.Normal),
        WebCategory("WebPartAppearance"),
        WebSysDescription(SR.Part_ChromeState),
        ]
        public virtual PartChromeState ChromeState {
            get {
                object o = ViewState["ChromeState"];
                return (o != null) ? (PartChromeState)o : PartChromeState.Normal;
            }
            set {
                if ((value < PartChromeState.Normal) || (value > PartChromeState.Minimized)) {
                    throw new ArgumentOutOfRangeException("value");
                }

                ViewState["ChromeState"] = value;
            }
        }

        /// <devdoc>
        /// The type of frame/border for the part.
        /// </devdoc>
        [
        DefaultValue(PartChromeType.Default),
        WebCategory("WebPartAppearance"),
        WebSysDescription(SR.Part_ChromeType),
        ]
        public virtual PartChromeType ChromeType {
            get {
                object o = ViewState["ChromeType"];
                return (o != null) ? (PartChromeType)(int)o : PartChromeType.Default;
            }
            set {
                if ((value < PartChromeType.Default) || (value > PartChromeType.BorderOnly)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ChromeType"] = (int)value;
            }
        }

        // Copied from CompositeControl
        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }

        /// <devdoc>
        /// Short descriptive text used in tooltips and web part catalog descriptions
        /// </devdoc>
        [
        DefaultValue(""),
        Localizable(true),
        WebCategory("WebPartAppearance"),
        WebSysDescription(SR.Part_Description),
        ]
        public virtual string Description {
            get {
                string s = (string)ViewState["Description"];
                return (s != null) ? s : String.Empty;
            }
            set {
                ViewState["Description"] = value;
            }
        }

        [
        // Must use WebSysDefaultValue instead of DefaultValue, since it is overridden in extending classes
        Localizable(true),
        WebSysDefaultValue(""),
        WebCategory("WebPartAppearance"),
        WebSysDescription(SR.Part_Title),
        ]
        public virtual string Title {
            get {
                string s = (string)ViewState["Title"];
                return (s != null) ? s : String.Empty;
            }
            set {
                ViewState["Title"] = value;
            }
        }

        // Copied from CompositeControl
        /// <devdoc>
        /// Perform our own databinding, then perform our child controls' databinding.
        /// </devdoc>
        public override void DataBind() {
            OnDataBinding(EventArgs.Empty);
            EnsureChildControls();
            DataBindChildren();
        }

        // Copied from CompositeControl
        #region ICompositeControlDesignerAccessor implementation
        void ICompositeControlDesignerAccessor.RecreateChildControls() {
            ChildControlsCreated = false;
            EnsureChildControls();
        }
        #endregion
    }
}
