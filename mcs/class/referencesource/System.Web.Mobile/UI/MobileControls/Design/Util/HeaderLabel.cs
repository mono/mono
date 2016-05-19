//------------------------------------------------------------------------------
// <copyright file="HeaderLabel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class HeaderLabel : RichTextBox
    {
        private String _text;
        
        internal HeaderLabel()
        {
            BackColor = SystemColors.Control;
            BorderStyle = BorderStyle.None;
            WordWrap = true;
            ReadOnly = true;
            TabStop = false;
            ScrollBars = RichTextBoxScrollBars.None;
            VisibleChanged += new EventHandler(OnVisibleChanged);
        }

        protected override void OnContentsResized(ContentsResizedEventArgs e)
        {
            HeaderPanel headerPanel = Parent as HeaderPanel;
            
            Debug.Assert(headerPanel != null,
                "HeaderLabel should be placed inside of a HeaderPanel.");
            headerPanel.RequestNewHeight(this, e.NewRectangle.Height);
            base.OnContentsResized(e);
        }

        public override String Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
            }
        }

        private void OnVisibleChanged(Object sender, EventArgs e)
        {
            if(Visible && _text != base.Text)
            {
                base.Text = _text;
            }
        }
    }
}
