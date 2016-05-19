//------------------------------------------------------------------------------
// <copyright file="LinkDesigner.cs" company="Microsoft">
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
    using System.Web.UI.Design.MobileControls.Converters;
    using System.Web.UI.MobileControls;
    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;

    /// <summary>
    ///    <para>
    ///       Provides a designer for the <see cref='System.Web.UI.MobileControls.Link'/>
    ///       control.
    ///    </para>
    /// </summary>
    /// <seealso cref='System.Web.UI.MobileControls.Link'/>
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class LinkDesigner : MobileControlDesigner
    {
        private System.Web.UI.MobileControls.Link _link;

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
            Debug.Assert(component is System.Web.UI.MobileControls.Link,
                         "LinkDesigner.Initialize - Invalid Link Control");
            _link = (System.Web.UI.MobileControls.Link) component;
            base.Initialize(component);
        }

        /// <summary>
        ///    <para>
        ///       Returns the design-time HTML of the <see cref='System.Web.UI.MobileControls.Link'/>
        ///       mobile control
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The HTML of the control.
        ///    </para>
        /// </returns>
        /// <seealso cref='System.Web.UI.MobileControls.Link'/>
        protected override String GetDesignTimeNormalHtml()
        {
            Debug.Assert(null != _link.Text);

            DesignerTextWriter tw;
            Control[] children = null;

            String originalText  = _link.Text;
            bool blankText = (originalText.Trim().Length == 0);
            bool hasControls = _link.HasControls();

            if (blankText)
            {
                if (hasControls) 
                {
                    children = new Control[_link.Controls.Count];
                    _link.Controls.CopyTo(children, 0);
                }
                _link.Text = "[" + _link.ID + "]";
            }
            try
            {
                tw = new DesignerTextWriter();
                _link.Adapter.Render(tw);
            }
            finally
            {
                if (blankText)
                {
                    _link.Text = originalText;
                    if (hasControls) 
                    {
                        foreach (Control c in children) 
                        {
                            _link.Controls.Add(c);
                        }
                    }
                }
            }

            return tw.ToString();
        }

        /// <summary>
        ///    <para>
        ///       Represents the method that will handle the component change event.
        ///    </para>
        /// </summary>
        /// <param name='sender'>
        ///    The source of the event.
        /// </param>
        /// <param name=' e'>
        ///    The <see cref='System.ComponentModel.Design.ComponentChangedEventArgs'/> that provides data about the event.
        /// </param>
        public override void OnComponentChanged(Object sender, ComponentChangedEventArgs e) 
        {
            if ((e.Member != null) && e.Member.Name.Equals("NavigateUrl"))
            {
                _link.NavigateUrl = NavigateUrlConverter.GetUrl(
                    _link,
                    e.NewValue.ToString(),
                    e.OldValue.ToString()
                );

                e = new ComponentChangedEventArgs(e.Component, e.Member, e.OldValue, _link.NavigateUrl);
            }

            base.OnComponentChanged(sender, e);
        }
    }
}
