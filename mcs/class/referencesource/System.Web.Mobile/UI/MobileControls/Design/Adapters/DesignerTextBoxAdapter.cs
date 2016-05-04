//------------------------------------------------------------------------------
// <copyright file="DesignerTextBoxAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Globalization;
using System.IO;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

namespace System.Web.UI.Design.MobileControls.Adapters
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DesignerTextBoxAdapter : HtmlTextBoxAdapter 
    {
        // required width may differ a little bit from actual exact pixel value
        private const int SAFETY_MARGIN = 12;
        // size after which we simply assume the control is too large to fit
        // into its container.
        private const int LARGESIZE_THRESHOLD = 100;

        public override MobileCapabilities Device
        {
            get
            {
                return DesignerCapabilities.Instance;
            }
        }

        public override void Render(HtmlMobileTextWriter writer)
        {
            // Invalid text writers are not supported in this Adapter.
            if (!(writer is DesignerTextWriter))
            {
                return;
            }

            byte templateStatus;
            bool pwd = Control.Password;
            int size = Control.Size;
            int fittingSize;

            int maxWidth = DesignerAdapterUtil.GetMaxWidthToFit(Control, out templateStatus);

            if (maxWidth == 0)
            {
                if (templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_NONEDIT)
                {
                    maxWidth = DesignerAdapterUtil.CONTROL_MAX_WIDTH_IN_TEMPLATE;
                }
                else if (DesignerAdapterUtil.InMobileUserControl(Control))
                {
                    maxWidth = Constants.ControlMaxsizeAtToplevel;
                }
            }

            if (maxWidth == 0)
            {
                return;
            }

            bool restoreEmptyFontName = false;
            if (String.IsNullOrEmpty((String) Style[Style.FontNameKey, true]))
            {
                // MSHTMLHostUtil is using another font by default.
                // Setting the font name to the one that is actually
                // used by default for the desig-time rendering
                // assures that the requiredWidth returned by
                // MSHTMLHostUtil.GetHtmlFragmentWidth is accurate.
                Style[Style.FontNameKey] = "Arial";
                restoreEmptyFontName = true;
            }

            int requiredWidth = 0;
            DesignerTextWriter tw;
            tw = new DesignerTextWriter(false);
            tw.EnterLayout(Style);
            String enterLayout = tw.ToString();

            tw = new DesignerTextWriter(false);
            tw.ExitLayout(Style);
            String exitLayout = tw.ToString();

            tw = new DesignerTextWriter(false);
            tw.WriteBeginTag("input");
            tw.WriteStyleAttribute(Style, null);
            if (size > 0)
            {
                tw.WriteAttribute("size", "{0}");
            }
            tw.Write("/>");
            String htmlFragment = tw.ToString();

            MSHTMLHostUtil.ApplyStyle(enterLayout, exitLayout, null);

            if (size < LARGESIZE_THRESHOLD)
            {
                requiredWidth = MSHTMLHostUtil.GetHtmlFragmentWidth(size > 0 ? String.Format(CultureInfo.InvariantCulture, htmlFragment, size) : htmlFragment);
            }

            if (requiredWidth + SAFETY_MARGIN > maxWidth || size >= LARGESIZE_THRESHOLD)
            {
                if (size == 0)
                {
                    tw = new DesignerTextWriter(false);
                    tw.WriteBeginTag("input");
                    tw.WriteStyleAttribute(Style, null);
                    tw.WriteAttribute("size", "{0}");
                    tw.Write("/>");
                    htmlFragment = tw.ToString();
                }
                fittingSize = 0;
                do
                {
                    fittingSize++;
                    requiredWidth = MSHTMLHostUtil.GetHtmlFragmentWidth(String.Format(CultureInfo.InvariantCulture, htmlFragment, fittingSize));
                }
                while (requiredWidth + SAFETY_MARGIN <= maxWidth);

                if (fittingSize > 1)
                {
                    fittingSize--;
                }
            }
            else
            {
                fittingSize = size;
            }

            if (restoreEmptyFontName)
            {
                Style[Style.FontNameKey] = String.Empty;
            }

            Alignment alignment = (Alignment) Style[Style.AlignmentKey, true];
            String width = DesignerAdapterUtil.GetWidth(Control);

            writer.Write("<div style='width:" + width);
            if (alignment != Alignment.NotSet)
            {
                writer.Write(";text-align:" + Enum.GetName(typeof(Alignment), alignment));
            }
            writer.Write("'>");

            ((DesignerTextWriter)writer).EnterZeroFontSizeTag();
            writer.EnterLayout(Style);

            writer.WriteBeginTag("input");
            ((DesignerTextWriter)writer).WriteStyleAttribute(Style, null);
            if (!String.IsNullOrEmpty(Control.Text))
            {
                writer.Write(" value=\"");
                writer.WriteText(Control.Text, true);
                writer.Write("\" ");
            }
            if (fittingSize > 0)
            {
                writer.WriteAttribute("size", fittingSize.ToString(CultureInfo.InvariantCulture));
            }
            if (pwd)
            {
                writer.WriteAttribute("type", "password");
            }
            writer.Write("/>");

            writer.ExitLayout(Style);
            ((DesignerTextWriter)writer).ExitZeroFontSizeTag();
            writer.Write("</div>");
        }
    }
}

