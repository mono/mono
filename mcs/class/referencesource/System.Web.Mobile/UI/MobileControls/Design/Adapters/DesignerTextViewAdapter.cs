//------------------------------------------------------------------------------
// <copyright file="DesignerTextViewAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Globalization;
using System.Text;
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
    internal class DesignerTextViewAdapter : System.Web.UI.MobileControls.Adapters.HtmlControlAdapter 
    {
        protected new TextView Control
        {
            get
            {
                return (TextView)base.Control;
            }
        }

        public override MobileCapabilities Device
        {
            get
            {
                return DesignerCapabilities.Instance;
            }
        }

        public override void Render(HtmlMobileTextWriter writer)
        {
            Alignment alignment = (Alignment) Style[Style.AlignmentKey, true];
            Wrapping wrapping = (Wrapping) Style[Style.WrappingKey, true];
            bool wrap = (wrapping == Wrapping.Wrap || wrapping == Wrapping.NotSet);
            String width = DesignerAdapterUtil.GetWidth(Control);

            ((DesignerTextWriter)writer).EnterZeroFontSizeTag();
            writer.WriteBeginTag("div");
            if (!wrap)
            {
                byte templateStatus;
                int maxWidth = DesignerAdapterUtil.GetMaxWidthToFit(Control, out templateStatus);
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

            MSHTMLHostUtil.ApplyStyle(null, null, null);
            String filteredText = FilterTags(Control.Text.Trim());
            int uniqueLineHeight = MSHTMLHostUtil.GetHtmlFragmentHeight("a");
            int requiredHeight = MSHTMLHostUtil.GetHtmlFragmentHeight(filteredText);
            int requiredWidth = MSHTMLHostUtil.GetHtmlFragmentWidth(filteredText);

            ((DesignerTextWriter)writer).WriteCssStyleText(Style, null, (requiredHeight > uniqueLineHeight || requiredWidth > 1) ? filteredText : "&nbsp;", false);
            writer.WriteEndTag("div");
            ((DesignerTextWriter)writer).ExitZeroFontSizeTag();
        }

        private enum CursorStatus
        {
            OutsideTag,
            InsideTagName,
            InsideAttributeName,
            InsideAttributeValue,
            ExpectingAttributeValue
        }

        private String FilterTags(String text)
        {
            StringBuilder filteredText = new StringBuilder();
            // StringBuilder hrefValue = null;
            int len = text.Length, i;
            int tagBegin = 0; //, attribBegin = 0;
            bool doubleQuotedAttributeValue = false;
            // bool cacheHRefValue = false;
            CursorStatus cs = CursorStatus.OutsideTag;
            String tagName = String.Empty;

            for (i = 0; i < len; i++)
            {
                switch (text[i])
                {
                    case '<':
                    {
                        switch (cs)
                        {
                            case CursorStatus.OutsideTag:
                            {
                                cs = CursorStatus.InsideTagName;
                                tagBegin = i;
                                break;
                            }
                        }
                        break;
                    }
                    case '=':
                    {
                        switch (cs)
                        {
                            case CursorStatus.InsideAttributeName:
                            {
                                // cacheHRefValue = text.Substring(attribBegin, i-attribBegin).Trim().ToUpper() == "HREF";
                                // hrefValue = null;
                                cs = CursorStatus.ExpectingAttributeValue;
                                break;
                            }
                            case CursorStatus.OutsideTag:
                            {
                                filteredText.Append(text[i]);
                                break;
                            }
                        }
                        break;
                    }
                    case '"':
                    {
                        switch (cs)
                        {
                            case CursorStatus.ExpectingAttributeValue:
                            {
                                cs = CursorStatus.InsideAttributeValue;
                                doubleQuotedAttributeValue = true;
                                //if (cacheHRefValue)
                                //{
                                //    hrefValue = new StringBuilder("\"");
                                //}
                                break;
                            }
                            case CursorStatus.InsideAttributeValue:
                            {
                                //if (cacheHRefValue)
                                //{
                                //    hrefValue.Append('"');
                                //}
                                if (text[i-1] != '\\' && doubleQuotedAttributeValue)
                                {
                                    // leaving attribute value
                                    cs = CursorStatus.InsideAttributeName;
                                    // attribBegin = i;
                                    break;
                                }
                                break;
                            }
                            case CursorStatus.OutsideTag:
                            {
                                filteredText.Append(text[i]);
                                break;
                            }
                        }
                        break;
                    }
                    case '\'':
                    {
                        switch (cs)
                        {
                            case CursorStatus.ExpectingAttributeValue:
                            {
                                cs = CursorStatus.InsideAttributeValue;
                                //if (cacheHRefValue)
                                //{
                                //    hrefValue = new StringBuilder("'");
                                //}
                                doubleQuotedAttributeValue = false;
                                break;
                            }
                            case CursorStatus.InsideAttributeValue:
                            {
                                //if (cacheHRefValue)
                                //{
                                //    hrefValue.Append('\'');
                                //}
                                if (text[i-1] != '\\' && !doubleQuotedAttributeValue)
                                {
                                    // leaving attribute value
                                    cs = CursorStatus.InsideAttributeName;
                                    // attribBegin = i;
                                    break;
                                }
                                break;
                            }
                            case CursorStatus.OutsideTag:
                            {
                                filteredText.Append(text[i]);
                                break;
                            }
                        }
                        break;
                    }
                    case '/':
                    {
                        switch (cs)
                        {
                            case CursorStatus.InsideTagName:
                            {
                                tagName = text.Substring(tagBegin+1, i-tagBegin-1).Trim().ToUpper(CultureInfo.InvariantCulture);

                                if (tagName.Trim().Length > 0)
                                {
                                    cs = CursorStatus.InsideAttributeName;
                                    // attribBegin = i;
                                }
                                break;
                            }
                            case CursorStatus.OutsideTag:
                            {
                                filteredText.Append(text[i]);
                                break;
                            }
                        }
                        break;
                    }
                    case '>':
                    {
                        switch (cs)
                        {
                            case CursorStatus.InsideTagName:
                            case CursorStatus.InsideAttributeName:
                            case CursorStatus.ExpectingAttributeValue:
                            {
                                // leaving tag
                                if (cs == CursorStatus.InsideTagName)
                                {
                                    tagName = text.Substring(tagBegin+1, i-tagBegin-1).Trim().ToUpper(CultureInfo.InvariantCulture);
                                }
                                cs = CursorStatus.OutsideTag;
                                switch (tagName)
                                {
                                    case "A":
                                    {
                                        //filteredText.Append(String.Format("<A HREF={0}>", 
                                        //    hrefValue == null ? String.Empty : hrefValue.ToString()));
                                        filteredText.Append("<A HREF=\"\">");
                                        break;
                                    }

                                    case "/A":
                                    case "B":
                                    case "/B":
                                    case "BR":
                                    case "/BR":
                                    case "I":
                                    case "/I":
                                    case "P":
                                    case "/P":
                                    {
                                        filteredText.Append("<" + tagName + ">");
                                        break;
                                    }
                                }
                                tagName = String.Empty;
                                break;
                            }
                            case CursorStatus.OutsideTag:
                            {
                                filteredText.Append(text[i]);
                                break;
                            }
                        }
                        break;
                    }
                    default:
                    {
                        if (Char.IsWhiteSpace(text[i]))
                        {
                            switch (cs)
                            {
                                case CursorStatus.OutsideTag:
                                {
                                    filteredText.Append(text[i]);
                                    break;
                                }
                                case CursorStatus.InsideTagName:
                                {
                                    cs = CursorStatus.InsideAttributeName;
                                    // attribBegin = i;
                                    tagName = text.Substring(tagBegin+1, i-tagBegin-1).Trim().ToUpper(CultureInfo.InvariantCulture);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            switch (cs)
                            {
                                case CursorStatus.OutsideTag:
                                {
                                    filteredText.Append(text[i]);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            return filteredText.ToString();
        }
    }
}

