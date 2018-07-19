//------------------------------------------------------------------------------
// <copyright file="HeaderPanel.cs" company="Microsoft">
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
    internal class HeaderPanel : Panel
    {
        private int _recursionCount = 0;
        
        internal void RequestNewHeight(HeaderLabel header, int height)
        {
            int offset = height - header.Height;
 
            try
            {
                // This is a workaround for a RTB issue that causes their
                // algorithm to ---- up if OnContentsResize recurses.  (Now
                // that HeaderLabel does not resize the text untill after
                // autoscaling, we do not seem to hit this, but just in case).
                // 
                // On the first call the RTB guesses its best dimensions
                // for the given text.  We correct the Width which may cause
                // a second recursive call to adjust the height.

                if(_recursionCount < 2)
                {
                    _recursionCount++;
                    header.Height = height;
                    
                    // 
                    foreach(Control child in Controls)
                    {
                        if(child.Top > header.Top)
                        {
                            child.Top += offset;
                        }
                    }

                    for(
                        Control controlIterator = this;
                        controlIterator != null;
                        controlIterator = controlIterator.Parent
                    ) {
                        controlIterator.Height += offset;
                    }
                }
                else
                {
                    Debug.Assert(offset == 0,
                        "On 3rd recursive call offset is not yet stabalized."
                    );
                }
            }
            finally
            {
                _recursionCount = 0;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            foreach(Control child in Controls)
            {
                if(child is HeaderLabel)
                {
                    child.Width = Width;
                }
            }
            base.OnSizeChanged(e);
        }
    }
}
