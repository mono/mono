//------------------------------------------------------------------------------
// <copyright file="CommandDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;

    /// <summary>
    ///    <para>
    ///       The designer for the <see cref='System.Web.UI.MobileControls.Command'/>
    ///       mobile control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.Command'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class CommandDesigner : MobileControlDesigner
    {
        private System.Web.UI.MobileControls.Command _command;

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
            Debug.Assert(component is System.Web.UI.MobileControls.Command,
                         "CommandDesigner.Initialize - Invalid Command Control");
            _command = (System.Web.UI.MobileControls.Command) component;
            base.Initialize(component);
        }

        /// <summary>
        ///    <para>
        ///       Returns the design-time HTML of the <see cref='System.Web.UI.MobileControls.Call'/>
        ///       mobile control
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The HTML of the control.
        ///    </para>
        /// </returns>
        /// <seealso cref='System.Web.UI.MobileControls.Call'/>
        protected override String GetDesignTimeNormalHtml()
        {
            Debug.Assert(null != _command.Text);

            DesignerTextWriter tw;
            Control[] children = null;

            String originalText = _command.Text;
            bool blankText = (originalText.Trim().Length == 0);
            bool hasControls = _command.HasControls();

            if (blankText)
            {
                if (hasControls) 
                {
                    children = new Control[_command.Controls.Count];
                    _command.Controls.CopyTo(children, 0);
                }
                _command.Text = "[" + _command.ID + "]";
            }
            try
            {
                tw = new DesignerTextWriter();
                _command.Adapter.Render(tw);
            }
            finally
            {
                if (blankText)
                {
                    _command.Text = originalText;
                    if (hasControls) 
                    {
                        foreach (Control c in children) 
                        {
                            _command.Controls.Add(c);
                        }
                    }
                }
            }

            return tw.ToString();
        }
    }
}
