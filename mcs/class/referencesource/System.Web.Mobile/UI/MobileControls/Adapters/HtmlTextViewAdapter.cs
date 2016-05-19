//------------------------------------------------------------------------------
// <copyright file="HtmlTextViewAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * WmlTextViewAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlTextViewAdapter.uex' path='docs/doc[@for="HtmlTextViewAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlTextViewAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlTextViewAdapter.uex' path='docs/doc[@for="HtmlTextViewAdapter.Control"]/*' />
        protected new TextView Control
        {
            get
            {
                return (TextView)base.Control;
            }
        }

        /// <include file='doc\HtmlTextViewAdapter.uex' path='docs/doc[@for="HtmlTextViewAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            int beginElement = Control.FirstVisibleElementIndex;
            int beginOffset = Control.FirstVisibleElementOffset;
            int endElement = Control.LastVisibleElementIndex;
            int endOffset = Control.LastVisibleElementOffset;

            writer.EnterStyle(Style);
            for (int i = beginElement; i <= endElement; i++)
            {
                int begin = (i == beginElement) ? beginOffset : 0;
                int end;
                if (i == endElement)
                {
                    if (endOffset <= begin)
                    {
                        break;
                    }
                    end = endOffset;
                }
                else
                {
                    end = -1;
                }

                RenderElement(writer, i, begin, end);
            }
            writer.ExitStyle(Style, Control.BreakAfter);
        }

        private void RenderElement(HtmlMobileTextWriter writer, int index, int begin, int end)
        {
            TextViewElement element = Control.GetElement(index);
            if (end == -1)
            {
                end = element.Text.Length;
            }

            String text = element.Text;
            if (begin > 0 || end < text.Length)
            {
                text = text.Substring(begin, end - begin);
            }

            BooleanOption previousBold   = Style.Font.Bold;
            BooleanOption previousItalic = Style.Font.Italic;
            if (element.IsBold)
            {
                Style.Font.Bold = BooleanOption.True;
            }
            if (element.IsItalic)
            {
                Style.Font.Italic = BooleanOption.True;
            }
            
            writer.EnterStyle(Style);
            if (element.Url != null)
            {
                RenderBeginLink(writer, element.Url);
                writer.WriteText(text, true);
                RenderEndLink(writer);
            }
            else
            {
                writer.WriteText(text, true);
            }
            writer.ExitStyle(Style, element.BreakAfter);

            Style.Font.Bold   = previousBold;
            Style.Font.Italic = previousItalic;
        }
    }

}


