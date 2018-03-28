//------------------------------------------------------------------------------
// <copyright file="DesignerTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Web.UI.Design.MobileControls;

namespace System.Web.UI.Design.MobileControls.Adapters
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DesignerTextWriter : HtmlMobileTextWriter
    {
        private readonly WriterStyle _defaultWriterStyle;
        
        internal DesignerTextWriter() : this(false)
        {
        }

        internal DesignerTextWriter(bool maintainState) : 
            base(new StringWriter(CultureInfo.CurrentCulture), DesignerCapabilities.Instance)
        {
            MaintainState = maintainState;
            _defaultWriterStyle = new WriterStyle();
        }

        internal void EnterZeroFontSizeTag()
        {
            WriteBeginTag("font");
            WriteAttribute("size", "+0");
            Write("/>");
            WriteBeginTag("div");
            WriteAttribute("style", "font-weight:normal;font-style:normal");
            Write(">");
        }

        internal void ExitZeroFontSizeTag()
        {
            WriteEndTag("div");
            WriteEndTag("font");
        }

        public override String ToString()
        {
            return InnerWriter.ToString();
        }

        internal void WriteDesignerStyleAttributes(MobileControl control, 
            Style style)
        {
            Alignment alignment = (Alignment) style[Style.AlignmentKey, true];
            Wrapping wrapping = (Wrapping) style[Style.WrappingKey, true];
            Color backColor = (Color) style[Style.BackColorKey, true];

            bool align  = (alignment != Alignment.NotSet);
            bool wrap = (wrapping == Wrapping.Wrap || wrapping == Wrapping.NotSet);

            String width = DesignerAdapterUtil.GetWidth(control);

            byte templateStatus;
            int maxWidth = DesignerAdapterUtil.GetMaxWidthToFit(control, out templateStatus);
            if (templateStatus == DesignerAdapterUtil.CONTROL_IN_TEMPLATE_EDIT)
            {
                width = maxWidth.ToString(CultureInfo.InvariantCulture) + "px";
            }

            if (!wrap)
            {
                Write(" style=\"overflow-x:hidden;width:" + width);
            }
            else
            {
                Write(" style=\"word-wrap:break-word;overflow-x:hidden;width:" + width);
            }

            if (backColor != Color.Empty)
            {
                Write(";background-color:" + ColorTranslator.ToHtml(backColor));
            }

            if (align)
            {
                Write(";text-align:" + Enum.GetName(typeof(Alignment), alignment));
            }
        }

        internal void WriteStyleAttribute(Style style)
        {
            WriteStyleAttribute(style, null);
        }

        internal void WriteStyleAttribute(Style style, String additionalStyle)
        {
            // Style attributes not written for device without CSS support
            if (!Device.SupportsCss)
            {
                return;
            }

            bool bold = (BooleanOption)style[Style.BoldKey, true] == BooleanOption.True;
            bool italic = (BooleanOption)style[Style.ItalicKey, true] == BooleanOption.True;
            FontSize  fontSize  = (FontSize) style[Style.FontSizeKey , true];
            String    fontName  = (String)   style[Style.FontNameKey , true];
            Color     foreColor = (Color)    style[Style.ForeColorKey, true];
            Color     backColor = (Color)    style[Style.BackColorKey, true];

            Write(" style=\"");

            if (null != additionalStyle)
            {
                Write(additionalStyle);
            }

            if (bold)
            {
                Write("font-weight:bold;");
            }

            if (italic)
            {
                Write("font-style:italic;");
            }

            if (fontSize == FontSize.Large)
            {
                Write("font-size:larger;");
            }
            else if (fontSize == FontSize.Small)
            {
                Write("font-size:smaller;");
            }

            if (!String.IsNullOrEmpty(fontName))
            {
                Write("font-family:");
                Write(fontName);
                Write(';');
            }

            if (foreColor != Color.Empty)
            {
                Write("color:");
                Write(ColorTranslator.ToHtml(foreColor));
                Write(';');
            }

            if (backColor != Color.Empty)
            {
                Write("background-color:");
                Write(ColorTranslator.ToHtml(backColor));
                Write(';');
                Write("border-color:");
                Write(ColorTranslator.ToHtml(backColor));
                Write(';');
            }

            Write("\"");
        }

        internal void WriteCssStyleText(Style style,
                                      String additionalStyle,
                                      String text,
                                      bool encodeText)
        {
            EnterLayout(style);
            WriteBeginTag("div");
            WriteStyleAttribute(style, additionalStyle);
            Write(">");
            WriteText(text, encodeText);
            WriteEndTag("div");
            ExitLayout(style);
        }

        public override void EnterLayout(Style style)
        {
            if(MaintainState)
            {
                base.EnterLayout(style);
                return;
            }
            //we are not maintaining state, so begin a new context
            BeginStyleContext();
            //create a WriterStyle and turn off formatting output
            WriterStyle newStyle = new WriterStyle(style);
            newStyle.Format = false;
            //transition to the new style, capturing output
            _currentState.Transition(newStyle);
            //Clear stack so we do not interfere with Write*()
            _currentState.Transition(_defaultWriterStyle, false);
            //restore the context
            EndStyleContext();
        }

        public override void ExitLayout(Style style, bool breakAfter)
        {
            if(MaintainState)
            {
                base.ExitLayout(style, breakAfter);
                return;
            }
            //we are not maintaining state, so begin a new context
            BeginStyleContext();
            //create a WriterStyle and turn off formatting output
            WriterStyle newStyle = new WriterStyle(style);
            newStyle.Format = false;
            //Setup stack like it would be after base.EnterLayout()
            _currentState.Transition(newStyle, false);
            //transition to default state and capture output
            _currentState.Transition(_defaultWriterStyle);
            //close the context, to flush all pending tags
            EndStyleContext();
        }

        public override void ExitLayout(Style style)
        {
            ExitLayout(style, false);
        }
    }
}

