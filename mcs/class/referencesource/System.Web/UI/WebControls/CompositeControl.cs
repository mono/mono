//------------------------------------------------------------------------------
// <copyright file="CompositeControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Web.Util;

    /// <devdoc>
    /// Base class for composite controls -- controls that contain other controls and reuse their functionality
    /// via class composition.  See Chapter 12 in "Developing Microsoft ASP.NET Server Controls and Components."
    /// The following classes have copied code from this class (look for "Copied from CompositeControl" comment):
    /// - ChangePassword
    /// - Login
    /// - LoginView
    /// - SiteMapPath
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.CompositeControlDesigner, " + AssemblyRef.SystemDesign)
    ]
    public abstract class CompositeControl : WebControl, INamingContainer, ICompositeControlDesignerAccessor {


        /// <devdoc>
        /// Ensure that the child controls have been created before returning the controls collection
        /// </devdoc>
        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        /// <devdoc>
        /// Perform our own databinding, then perform our child controls' databinding.
        /// Does not call Base.DataBind(), since we need to call EnsureChildControls() between
        /// OnDataBinding() and DataBindChildren().
        /// </devdoc>
        public override void DataBind() {
            OnDataBinding(EventArgs.Empty);

            EnsureChildControls();

            DataBindChildren();
        }

        protected virtual void RecreateChildControls() {
            ChildControlsCreated = false;
            EnsureChildControls();
        }

        // Needed so the CompositeControl renders correctly in the designer, even when it does not have
        // an associated ControlDesigner (i.e. it is a child control of another CompositeControl).
        protected internal override void Render(HtmlTextWriter writer) {
            if (DesignMode) {
                EnsureChildControls();
            }

            base.Render(writer);
        }

        #region ICompositeControlDesignerAccessor implementation
        void ICompositeControlDesignerAccessor.RecreateChildControls() {
            RecreateChildControls();
        }
        #endregion
    }
}
