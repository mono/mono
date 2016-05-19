//------------------------------------------------------------------------------
// <copyright file="GroupLabel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.Serialization.Formatters;
    using System.Windows.Forms;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal sealed class GroupLabel : Label 
    {
        /// <summary>
        ///    Creates a new GroupLabel
        /// </summary>
        internal GroupLabel() : base() 
        {
            SetStyle(ControlStyles.UserPaint, true);
        }

        /// <summary>
        ///    Custom UI is painted here
        /// </summary>
        protected override void OnPaint(PaintEventArgs e) 
        {
            Graphics g = e.Graphics;
            Rectangle r = ClientRectangle;
            string text = Text;

            Brush foreBrush = new SolidBrush(ForeColor);
            g.DrawString(text, Font, foreBrush, 0, 0);
            foreBrush.Dispose();

            int etchLeft = r.X;
            if (text.Length != 0) 
            {
                Size sz = Size.Ceiling(g.MeasureString(text, Font));
                etchLeft += 6 + sz.Width;
            }
            int etchTop = r.Height / 2;

            g.DrawLine(SystemPens.ControlDark, etchLeft, etchTop, r.Width, etchTop);

            etchTop++;
            g.DrawLine(SystemPens.ControlLightLight, etchLeft, etchTop, r.Width, etchTop);
        }
    }
}
