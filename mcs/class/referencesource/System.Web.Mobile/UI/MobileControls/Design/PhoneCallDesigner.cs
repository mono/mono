//------------------------------------------------------------------------------
// <copyright file="PhoneCallDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.Design;

    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Converters;

    /// <summary>
    ///    <para>
    ///       Provides a designer for the <see cref='System.Web.UI.MobileControls.PhoneCall'/>
    ///       control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.PhoneCall'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class PhoneCallDesigner : MobileControlDesigner
    {
        private System.Web.UI.MobileControls.PhoneCall _call;

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
            Debug.Assert(component is System.Web.UI.MobileControls.PhoneCall,
                         "PhoneCallDesigner.Initialize - Invalid PhoneCall Control");
            _call = (System.Web.UI.MobileControls.PhoneCall) component;
            base.Initialize(component);
        }

        /// <summary>
        ///    <para>
        ///       Returns the design-time HTML of the <see cref='System.Web.UI.MobileControls.PhoneCall'/>
        ///       mobile control
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The HTML of the control.
        ///    </para>
        /// </returns>
        /// <seealso cref='System.Web.UI.MobileControls.PhoneCall'/>
        protected override String GetDesignTimeNormalHtml()
        {
            Debug.Assert(_call.Text != null);

            DesignerTextWriter tw;
            Control[] children = null;

            String originalText = _call.Text;
            bool blankText = (originalText.Trim().Length == 0);
            bool hasControls = _call.HasControls();

            if (blankText)
            {
                if (hasControls) 
                {
                    children = new Control[_call.Controls.Count];
                    _call.Controls.CopyTo(children, 0);
                }
                _call.Text = "[" + _call.ID + "]";
            }
            try
            {
                tw = new DesignerTextWriter();
                _call.Adapter.Render(tw);
            }
            finally
            {
                if (blankText)
                {
                    _call.Text = originalText;
                    if (hasControls) 
                    {
                        foreach (Control c in children) 
                        {
                            _call.Controls.Add(c);
                        }
                    }
                }
            }

            return tw.ToString();
        }
        
        public override void OnComponentChanged(Object sender, ComponentChangedEventArgs e) 
        {
            if ((e.Member != null) && e.Member.Name.Equals("AlternateUrl"))
            {
                _call.AlternateUrl = NavigateUrlConverter.GetUrl(
                    _call,
                    e.NewValue.ToString(),
                    e.OldValue.ToString()
                );

                e = new ComponentChangedEventArgs(e.Component, e.Member, e.OldValue, _call.AlternateUrl);
            }
            base.OnComponentChanged(sender, e);
        }
    }
}
