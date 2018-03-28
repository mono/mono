//------------------------------------------------------------------------------
// <copyright file="LabelDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls 
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.Design;

    using System.Web.UI.MobileControls;
    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;

    /// <summary>
    ///    <para>
    ///       Provides a designer for the <see cref='System.Web.UI.MobileControls.Label'/>
    ///       control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.Label'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class LabelDesigner : MobileControlDesigner
    {
        private System.Web.UI.MobileControls.Label _label;

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
            Debug.Assert(component is System.Web.UI.MobileControls.Label,
                         "LabelDesigner.Initialize - Invalid Label Control");
            _label = (System.Web.UI.MobileControls.Label) component;
            base.Initialize(component);
        }

        /// <summary>
        ///    <para>
        ///       Returns the design-time HTML of the <see cref='System.Web.UI.MobileControls.Label'/>
        ///       mobile control
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The HTML of the control.
        ///    </para>
        /// </returns>
        /// <seealso cref='System.Web.UI.MobileControls.Label'/>
        protected override String GetDesignTimeNormalHtml()
        {
            Debug.Assert(null != _label.Text);

            String originalText  = _label.Text;
            DesignerTextWriter tw;
            Control[] children = null;

            bool blankText = (originalText.Trim().Length == 0);
            bool hasControls = _label.HasControls();

            if (blankText)
            {
                if (hasControls) 
                {
                    children = new Control[_label.Controls.Count];
                    _label.Controls.CopyTo(children, 0);
                }
                _label.Text = "[" + _label.ID + "]";
            }
            try
            {
                tw = new DesignerTextWriter();
                _label.Adapter.Render(tw);
            }
            finally
            {
                if (blankText)
                {
                    _label.Text = originalText;
                    if (hasControls) 
                    {
                        foreach (Control c in children) 
                        {
                            _label.Controls.Add(c);
                        }
                    }
                }
            }

            return tw.ToString();
        }
    }
}
