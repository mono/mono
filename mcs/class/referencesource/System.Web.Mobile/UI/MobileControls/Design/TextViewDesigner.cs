//------------------------------------------------------------------------------
// <copyright file="TextViewDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI.MobileControls;
    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;

    /// <summary>
    ///    <para>
    ///       Provides a designer for the <see cref='System.Web.UI.MobileControls.TextView'/>
    ///       control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.TextView'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class TextViewDesigner : MobileControlDesigner
    {
        private System.Web.UI.MobileControls.TextView _textView;

        /// <summary>
        ///    <para>
        ///       Initializes the designer with the component for design.
        ///    </para>
        /// </summary>
        /// <param name='component'>
        ///    The control element for design.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called by the designer host to establish the component for
        ///       design.
        ///    </para>
        /// </remarks>
        /// <seealso cref='System.ComponentModel.Design.IDesigner'/>
        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is System.Web.UI.MobileControls.TextView,
                         "TextViewDesigner.Initialize - Invalid TextView Control");
            _textView = (System.Web.UI.MobileControls.TextView) component;
            base.Initialize(component);
        }

        private bool ValidContainment
        {
            get
            {
                return (
                    ContainmentStatus == ContainmentStatus.InForm ||
                    ContainmentStatus == ContainmentStatus.InPanel ||
                    ContainmentStatus == ContainmentStatus.InTemplateFrame);
            }
        }

        /// <summary>
        ///    <para>
        ///       Returns the design-time HTML of the <see cref='System.Web.UI.MobileControls.TextBox'/>
        ///       mobile control
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The HTML of the control.
        ///    </para>
        /// </returns>
        /// <seealso cref='System.Web.UI.MobileControls.TextBox'/>
        protected override String GetDesignTimeNormalHtml()
        {
            Debug.Assert (_textView.Text != null);

            DesignerTextWriter tw;
            Control[] children = null;

            String originalText = _textView.Text;
            bool blankText = (originalText.Trim().Length == 0);
            bool hasControls =  _textView.HasControls();

            if (blankText)
            {
                if (hasControls) 
                {
                    children = new Control[_textView.Controls.Count];
                    _textView.Controls.CopyTo(children, 0);
                }
                _textView.Text = "[" + _textView.ID + "]";
            }
            try
            {
                tw = new DesignerTextWriter();
                _textView.Adapter.Render(tw);
            }
            finally
            {
                if (blankText)
                {
                    _textView.Text = originalText;
                    if (hasControls) 
                    {
                        foreach (Control c in children) 
                        {
                            _textView.Controls.Add(c);
                        }
                    }
                }
            }

            return tw.ToString();
        }

        protected override String GetErrorMessage(out bool infoMode)
        {
            infoMode = false;

            if (DesignerAdapterUtil.InMobileUserControl(_textView))
            {
                return null;
            }

            if (DesignerAdapterUtil.InUserControl(_textView))
            {
                infoMode = true;
                return MobileControlDesigner._userControlWarningMessage;
            }

            if (!DesignerAdapterUtil.InMobilePage(_textView))
            {
                return MobileControlDesigner._mobilePageErrorMessage;
            }

            if (!ValidContainment)
            {
                return MobileControlDesigner._formPanelContainmentErrorMessage;
            }

            // Containment is valid, return null;
            return null;
        }
    }
}

