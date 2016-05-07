//------------------------------------------------------------------------------
// <copyright file="MSHTMLHostUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Drawing;
using System.Web.UI.Design.MobileControls.Util;

namespace System.Web.UI.Design.MobileControls.Adapters
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal static class MSHTMLHostUtil
    {
        private const int CONTROL_WIDTH = 64;
        private const int CONTROL_HEIGHT = 4096;

        private static MSHTMLHost _tridentControl;
        private static NativeMethods.IHTMLElement _htmlBody;
        private static NativeMethods.IHTMLElement _htmlDivOuter;
        private static NativeMethods.IHTMLElement _htmlDivInner;

        private static void CreateControl()
        {
            if (null != _tridentControl && null != _htmlBody)
            {
                return;
            }

            _tridentControl = new MSHTMLHost();

            _tridentControl.Size = new Size(CONTROL_WIDTH, CONTROL_HEIGHT);

            _tridentControl.CreateTrident();
            _tridentControl.ActivateTrident();

            NativeMethods.IHTMLDocument2 htmlDoc2 = _tridentControl.GetDocument();
            _htmlBody = htmlDoc2.GetBody();
        }

        internal static void ApplyStyle(String enterStyle, String exitStyle, String cssStyle)
        {
            MSHTMLHostUtil.CreateControl();

            String bodyInnerHTML = "<div id=__divOuter nowrap style='width:1px; height:10px'>" +
                                   enterStyle +
                                   "<div id=__divInner" + cssStyle + "></div>" +
                                   exitStyle +
                                   "</div>";

            // MessageBox.Show("Body HTML for empty content: " + bodyInnerHTML);
            _htmlBody.SetInnerHTML(bodyInnerHTML);

            NativeMethods.IHTMLDocument3 htmlDoc3 = (NativeMethods.IHTMLDocument3) _tridentControl.GetDocument();
            Debug.Assert(null != htmlDoc3);

            _htmlDivInner = htmlDoc3.GetElementById("__divInner");
            _htmlDivOuter = htmlDoc3.GetElementById("__divOuter");
            Debug.Assert(null != _htmlDivOuter && null != _htmlDivInner);
        }

#if UNUSED_CODE
        internal static int GetTextWidth(String text)
        {
            Debug.Assert(null != _htmlDivOuter && null != _htmlDivInner);

            _htmlDivInner.SetInnerText(text);
            NativeMethods.IHTMLElement2 htmlElement2 = (NativeMethods.IHTMLElement2) _htmlDivOuter;
            Debug.Assert(null != htmlElement2);
            return htmlElement2.GetClientWidth();
        }
#endif

        internal static int GetHtmlFragmentWidth(String htmlFragment)
        {
            Debug.Assert(null != _htmlDivOuter && null != _htmlDivInner);
            _htmlDivInner.SetInnerHTML(htmlFragment);
            NativeMethods.IHTMLElement2 htmlElement2 = (NativeMethods.IHTMLElement2) _htmlDivOuter;
            Debug.Assert(null != htmlElement2);
            return htmlElement2.GetClientWidth();
        }

        internal static int GetHtmlFragmentHeight(String htmlFragment)
        {
            Debug.Assert(null != _htmlDivOuter && null != _htmlDivInner);
            _htmlDivInner.SetInnerHTML(htmlFragment);
            NativeMethods.IHTMLElement2 htmlElement2 = (NativeMethods.IHTMLElement2) _htmlDivOuter;
            Debug.Assert(null != htmlElement2);
            return htmlElement2.GetClientHeight();
        }
    }
}
