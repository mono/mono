//------------------------------------------------------------------------------
// <copyright file="DesignerCommandAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Globalization;
using System.IO;
using System.Web.Mobile;
using System.Web.UI.Design.MobileControls;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

namespace System.Web.UI.Design.MobileControls.Adapters
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DesignerCommandAdapter : HtmlCommandAdapter
    {
        // required width may differ a little bit from actual exact pixel value
        private const int SAFETY_MARGIN = 8;

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

            Alignment alignment = (Alignment)Style[Style.AlignmentKey, true];
            byte templateStatus;
            int maxWidth = DesignerAdapterUtil.GetMaxWidthToFit(Control, out templateStatus);
            String width = DesignerAdapterUtil.GetWidth(Control);

            if (Control.ImageUrl.Length == 0)
            {
                if (Control.Format == CommandFormat.Button)
                {
                    if (maxWidth == 0 && templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_NONEDIT)
                    {
                        maxWidth = DesignerAdapterUtil.CONTROL_MAX_WIDTH_IN_TEMPLATE;
                    }

                    if (maxWidth == 0 && DesignerAdapterUtil.InMobileUserControl(Control))
                    {
                        maxWidth = Constants.ControlMaxsizeAtToplevel;
                    }

                    if (maxWidth == 0)
                    {
                        // Render will be called a second time for which maxWidth != 0
                        return;
                    }

                    String additionalStyle = null;
                    String controlText = Control.Text;
                    String commandCaption;
                    int requiredWidth = 0;

                    DesignerTextWriter twTmp;
                    twTmp = new DesignerTextWriter();
                    twTmp.WriteBeginTag("input");
                    twTmp.WriteStyleAttribute(Style, null);
                    twTmp.WriteAttribute("type", "submit");
                    twTmp.Write(" value=\"");
                    twTmp.WriteText(controlText, true);
                    twTmp.Write("\"/>");
                    String htmlFragment = twTmp.ToString();

                    MSHTMLHostUtil.ApplyStyle(String.Empty, String.Empty, null);
                    requiredWidth = MSHTMLHostUtil.GetHtmlFragmentWidth(htmlFragment);

                    ((DesignerTextWriter)writer).EnterZeroFontSizeTag();
                    writer.WriteBeginTag("div");
                    if (requiredWidth + SAFETY_MARGIN > maxWidth)
                    {
                        if (templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_EDIT)
                        {
                            int tmpRequiredWidth, allowedLength;
                            int captionLength = controlText.Length;
                            twTmp = new DesignerTextWriter();
                            twTmp.WriteBeginTag("input");
                            twTmp.WriteStyleAttribute(Style, null);
                            twTmp.WriteAttribute("type", "submit");
                            twTmp.WriteAttribute("value", "{0}");
                            twTmp.Write("/>");
                            htmlFragment = twTmp.ToString();
                            // At least 10 characters can fit into the caption of the command
                            for (allowedLength = (captionLength < 10 ? captionLength : 10); allowedLength <= captionLength; allowedLength++)
                            {
                                tmpRequiredWidth = MSHTMLHostUtil.GetHtmlFragmentWidth(String.Format(CultureInfo.CurrentCulture, htmlFragment, HttpUtility.HtmlEncode(controlText.Substring(0, allowedLength))));
                                if (tmpRequiredWidth + SAFETY_MARGIN > maxWidth)
                                {
                                    break;
                                }
                            }
                            commandCaption = controlText.Substring(0, allowedLength - 1);
                        }
                        else
                        {
                            commandCaption = controlText;
                        }
                    }
                    else
                    {
                        writer.WriteAttribute("style", "width:" + width);
                        commandCaption = controlText;
                    }

                    if (alignment != Alignment.NotSet)
                    {
                        writer.WriteAttribute("align", Enum.GetName(typeof(Alignment), alignment));
                    }
                    writer.Write(">");

                    writer.EnterLayout(Style);

                    writer.WriteBeginTag("input");
                    if (requiredWidth + SAFETY_MARGIN > maxWidth)
                    {
                        additionalStyle = String.Format(CultureInfo.CurrentCulture, "width:{0};", width);
                    }
                    ((DesignerTextWriter)writer).WriteStyleAttribute(Style, additionalStyle);
                    writer.WriteAttribute("type", "submit");

                    writer.Write(" value=\"");
                    writer.WriteText(commandCaption, true);
                    writer.Write("\"/>");

                    writer.ExitLayout(Style);
                }
                else
                {
                    Wrapping wrapping = (Wrapping) Style[Style.WrappingKey, true];
                    bool wrap = (wrapping == Wrapping.Wrap || wrapping == Wrapping.NotSet);

                    ((DesignerTextWriter)writer).EnterZeroFontSizeTag();
                    writer.WriteBeginTag("div");

                    if (!wrap)
                    {
                        if (templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_EDIT)
                        {
                            width = maxWidth.ToString(CultureInfo.InvariantCulture) + "px";
                        }
                        writer.WriteAttribute("style", "overflow-x:hidden;width:" + width);
                    }
                    else
                    {
                        writer.WriteAttribute("style", "word-wrap:break-word;width:" + width);
                    }

                    if (alignment != Alignment.NotSet)
                    {
                        writer.WriteAttribute("align", Enum.GetName(typeof(Alignment), alignment));
                    }
                    writer.Write(">");

                    writer.WriteBeginTag("a");
                    writer.WriteAttribute("href", "NavigationUrl");
                    writer.Write(">");
                    ((DesignerTextWriter)writer).WriteCssStyleText(Style, null, Control.Text, true);
                    writer.WriteEndTag("a");
                }
                writer.WriteEndTag("div");
                ((DesignerTextWriter)writer).ExitZeroFontSizeTag();
            }
            else
            {
                if (templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_EDIT)
                {
                    width = maxWidth.ToString(CultureInfo.InvariantCulture) + "px";
                }

                writer.WriteBeginTag("div");
                if (alignment == Alignment.Center)
                {
                    writer.WriteAttribute("align", "center");
                }
                writer.WriteAttribute("style", "overflow-x:hidden;width:" + width);
                writer.Write(">");

                writer.WriteBeginTag("img");
                ((DesignerTextWriter)writer).WriteStyleAttribute(Style);
                writer.WriteAttribute("src", Control.ImageUrl, true);

                // center alignment not part of HTML for images.
                if (alignment == Alignment.Right ||
                    alignment == Alignment.Left)
                {
                    writer.WriteAttribute("align", Enum.GetName(typeof(Alignment), alignment));
                }

                writer.WriteAttribute("border", "0");
                writer.Write(">");
                writer.WriteEndTag("div");
            }
        }
    }
}
