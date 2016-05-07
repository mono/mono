//------------------------------------------------------------------------------
// <copyright file="HtmlMobileTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mobile;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Collections;
using System.Diagnostics;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif

{

    /*
     * HtmlMobileTextWriter class.
     */
    /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlMobileTextWriter : MobileTextWriter
    {
        private bool _shouldEnsureStyle = true;

        //  mobile device type constants (should be defined somewhere else eventually)
        internal WriterState _currentState;

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.HtmlMobileTextWriter"]/*' />
        public HtmlMobileTextWriter(TextWriter writer, MobileCapabilities device)
            : base(writer, device)
        {
            RenderBold = device.SupportsBold;
            RenderItalic = device.SupportsItalic;
            RenderFontSize = device.SupportsFontSize;
            RenderFontName = device.SupportsFontName;
            RenderFontColor = device.SupportsFontColor;
            RenderBodyColor = device.SupportsBodyColor;
            RenderDivAlign = device.SupportsDivAlign;
            RenderDivNoWrap = device.SupportsDivNoWrap;
            RequiresNoBreakInFormatting = device.RequiresNoBreakInFormatting;
            _currentState = new WriterState(this);
        }

        /*
         * the following TextWriter methods are overridden to 
         * first call EnsureStyle before delegating to the base 
         * class implementation 
         */
        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.WriteBeginTag"]/*' />
        public override void WriteBeginTag(String tag)
        {
            EnsureStyle();
            base.WriteBeginTag(tag);
        }
        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.WriteFullBeginTag"]/*' />
        public override void WriteFullBeginTag(String tag)
        {
            EnsureStyle();
            base.WriteFullBeginTag(tag);
        }
        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.Write"]/*' />
        public override void Write(char c)
        {
            EnsureStyle();
            base.Write(c);
        }
        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.Write1"]/*' />
        public override void Write(String text)
        {
            EnsureStyle();
            base.Write(text);
        }
        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.WriteEncodedText"]/*' />
        public override void WriteEncodedText(String text)
        {
            EnsureStyle();
            if(Device["supportsCharacterEntityEncoding"] != "false") {
                base.WriteEncodedText(text);
                return;
            }
            if (null == text || text.Length == 0) {
                return;
            }

            int length = text.Length;
            int start = -1;
            for(int pos = 0; pos < length; pos++) {
                int ch = text[pos];
                if(ch > 160 && ch < 256) {
                    if(start != -1) {
                        base.WriteEncodedText(text.Substring(start, pos - start));
                        start = -1;
                    }
                    base.Write(text[pos]);
                }
                else {
                    if(start == -1) {
                        start = pos;
                    }
                }
            }
            if(start != -1) {
                if(start == 0) {
                    base.WriteEncodedText(text);
                }
                else {
                    base.WriteEncodedText(text.Substring(start, length - start));
                }
            }
        }
        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.WriteLine"]/*' />
        public override void WriteLine(String text)
        {
            EnsureStyle();
            base.WriteLine(text);
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.WriteBreak"]/*' />
        public new void WriteBreak()
        {
            //Do not EnsureStyle for the break
            base.WriteLine("<br>");
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.EnterLayout"]/*' />
        public override void EnterLayout(Style style)
        {
            WriterStyle writerStyle = new WriterStyle(style);
            writerStyle.Format = false;
            EnterStyle(writerStyle);
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.ExitLayout"]/*' />
        public override void ExitLayout(Style style, bool breakAfter)
        {
            ExitStyle(style, breakAfter);
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.ExitLayout1"]/*' />
        public override void ExitLayout(Style style)
        {
            ExitStyle(style, false);
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.MarkStyleContext"]/*' />
        protected internal void MarkStyleContext()
        {
            _shouldEnsureStyle = true;
            _currentState.MarkStyleContext();
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.UnMarkStyleContext"]/*' />
        protected internal void UnMarkStyleContext()
        {
            _shouldEnsureStyle = true;
            _currentState.UnMarkStyleContext();
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.EnterFormat"]/*' />
        public override void EnterFormat(Style style)
        {
            WriterStyle writerStyle = new WriterStyle(style);
            writerStyle.Layout = false;
            EnterStyle(writerStyle);
        }                                                                   

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.ExitFormat"]/*' />
        public override void ExitFormat(Style style)
        {
            ExitStyle(style);
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.ExitFormat1"]/*' />
        public override void ExitFormat(Style style, bool breakAfter)
        {
            ExitStyle(style, breakAfter);
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.BeginStyleContext"]/*' />
        public void BeginStyleContext()
        {
            if(_currentState.BreakPending)
            {
                WriteBreak();
                _currentState.BreakPending = false;
            }
            _currentState.PushState();
            EnterStyle(new WriterStyle());
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.EndStyleContext"]/*' />
        public void EndStyleContext()
        {
            if(_currentState.BreakPending)
            {
                WriteBreak();
                _currentState.BreakPending = false;
            }
            _currentState.PopState();
            _currentState.Pop();
            _currentState.Transition(new WriterStyle());
        }

        /* all calls to Enter... converge to this */
        private void EnterStyle(WriterStyle style)
        {
            _currentState.Push(style);
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.EnterStyle"]/*' />
        public new void EnterStyle(Style style)
        {
            EnterStyle(new WriterStyle(style));
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.ExitStyle"]/*' />
        public new void ExitStyle(Style style)
        {
            ExitStyle(style, false);

        }

        internal bool ShouldEnsureStyle
        {
            get
            {
                return _shouldEnsureStyle;
            }
            set
            {
                _shouldEnsureStyle = value;
            }
        }

        /*
        all calls to Exit... converge to this 
        */
        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.ExitStyle1"]/*' />
        public void ExitStyle(Style style, bool breakAfter)
        {
            _currentState.Pop();
            if((_currentState.BreakPending) && (_currentState.Count > 0))
            {
                EnsureStyle();
            }
            _currentState.BreakPending = breakAfter;
            if((_currentState.Count == 0) || (RequiresNoBreakInFormatting))
            {
                _currentState.Transition(new WriterStyle());
            }
            InputWritten = false;
        }

        internal bool _inputWritten = false;
        internal bool InputWritten
        {
            get { return _inputWritten; }
            set { _inputWritten = value; }
        }

        internal void EnsureStyle()
        {
            if (_shouldEnsureStyle)
            {
                if(_currentState.Count > 0)
                {
                    _currentState.Transition(_currentState.Peek());
                }
                _shouldEnsureStyle = false;
            }
            if(BeforeFirstControlWritten)
            {
                BeforeFirstControlWritten = false;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.WriteText"]/*' />
        public void WriteText(String text, bool encodeText)
        {
            if(text != null && text.Length == 0)
            {
                return;
            }
            EnsureStyle();
            if (encodeText)
            {
                WriteEncodedText(text);
            }
            else
            {
                Write(text);
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.WriteUrlParameter"]/*' />
        public void WriteUrlParameter(String name, String value)
        {
            WriteEncodedUrlParameter(name);
            Write("=");
            WriteEncodedUrlParameter(value);
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.WriteHiddenField"]/*' />
        public void WriteHiddenField(String name, String value)
        {
            WriteBeginTag("input");
            WriteAttribute("type", "hidden");
            WriteAttribute("name", name);
            WriteAttribute("value", value, true);
            Write(">\r\n");
        }


        // AUI 2285
        private bool _beforeFirstControlWritten = true;
        internal bool BeforeFirstControlWritten
        {
            get
            {
                return _beforeFirstControlWritten;
            }

            set
            {
                _beforeFirstControlWritten = value;
            }
        }

        private bool _maintainState = true;

        internal bool MaintainState
        {
            get
            {
                return _maintainState;
            }
            set
            {
                _maintainState = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RenderBold"]/*' />
        protected internal bool RenderBold
        {
            get
            {
                return _renderBold;
            }
            set
            {
                _renderBold = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RenderItalic"]/*' />
        protected internal bool RenderItalic
        {
            get
            {
                return _renderItalic;
            }
            set
            {
                _renderItalic = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RenderFontSize"]/*' />
        protected internal bool RenderFontSize
        {
            get
            {
                return _renderFontSize;
            }
            set
            {
                _renderFontSize = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RenderFontName"]/*' />
        protected internal bool RenderFontName
        {
            get
            {
                return _renderFontName;
            }

            set
            {
                _renderFontName = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RenderFontColor"]/*' />
        protected internal bool RenderFontColor
        {
            get
            {
                return _renderFontColor;
            }
            set
            {
                _renderFontColor = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RenderBodyColor"]/*' />
        protected internal bool RenderBodyColor
        {
            get
            {
                return _renderBodyColor;
            }
            set
            {
                _renderBodyColor = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RenderDivAlign"]/*' />
        protected internal bool RenderDivAlign
        {
            get
            {
                return _renderDivAlign;
            }
            set
            {
                _renderDivAlign = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RenderDivNoWrap"]/*' />
        protected internal bool RenderDivNoWrap
        {
            get
            {
                return _renderDivNoWrap;
            }
            set
            {
                _renderDivNoWrap = value;
            }
        }

        /// <include file='doc\HtmlMobileTextWriter.uex' path='docs/doc[@for="HtmlMobileTextWriter.RequiresNoBreakInFormatting"]/*' />
        protected internal bool RequiresNoBreakInFormatting
        {
            get
            {
                return _requiresNoBreakInFormatting;
            }
            set
            {
                _requiresNoBreakInFormatting = value;
            }
        }

        private bool _renderBold = true;
        private bool _renderItalic = true;
        private bool _renderFontSize = true;
        private bool _renderFontName = true;
        private bool _renderFontColor = true;
        private bool _renderBodyColor = true;
        private bool _renderDivAlign = true;
        private bool _renderDivNoWrap = false;
        private bool _requiresNoBreakInFormatting = false;
    }

    /*
     * the WriterStyle class is used to store and
     * control state for rendering format and layout
     */
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class WriterStyle
    {
        private Wrapping    _wrapping;
        private Alignment   _alignment;
        private String      _fontName;
        private Color       _fontColor;
        private FontSize    _fontSize;
        private bool        _bold;
        private bool        _italic;

        private bool _format;
        private bool _layout;

        internal WriterStyle()
        {
            _wrapping = Wrapping.Wrap;
            _alignment = Alignment.Left;
            _fontName = String.Empty;
            _fontColor = Color.Empty;
            _fontSize = FontSize.Normal;
            _bold = false;
            _italic = false;

            _format = true;
            _layout = true;
        }

        internal WriterStyle(Style style)
        {
            Debug.Assert(style != null, "writer style is null");
            _alignment = (Alignment)         style[Style.AlignmentKey, true];
            if(_alignment == Alignment.NotSet)
            {
                _alignment = Alignment.Left;
            }
            _wrapping = (Wrapping)           style[Style.WrappingKey, true];
            if(_wrapping == Wrapping.NotSet)
            {
                _wrapping = Wrapping.Wrap;
            }

            _fontSize  = (FontSize)         style[Style.FontSizeKey , true];
            if(_fontSize == FontSize.NotSet)
            {
                _fontSize = FontSize.Normal;
            }
            _fontName  = (String)           style[Style.FontNameKey , true];
            _fontColor = (Color)            style[Style.ForeColorKey, true]; 

            _bold = ((BooleanOption)        style[Style.BoldKey, true] == BooleanOption.True);
            _italic = ((BooleanOption)      style[Style.ItalicKey, true] == BooleanOption.True);

            _format = true;
            _layout = true;

        }


        internal bool Format
        {
            get { return _format; }
            set { _format = value; }
        }

        internal bool Layout
        {
            get { return _layout; }
            set { _layout = value; }
        }

        internal Wrapping Wrapping
        {
            get { return _wrapping; }
            set { _wrapping = value; }
        }
        internal Alignment Alignment
        {
            get { return _alignment; }
            set { _alignment = value; }
        }
        internal String FontName
        {
            get { return _fontName; }
            set { _fontName = value; }
        }
        internal Color FontColor
        {
            get { return _fontColor; }
            set { _fontColor = value; }
        }
        internal FontSize FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; }
        }
        internal bool Bold
        {
            get { return _bold; }
            set { _bold = value; }
        }
        internal bool Italic
        {
            get { return _italic; }
            set { _italic = value; }
        }
    }

    /*
     * The StyleTag class is extended for specific tags
     */
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal abstract class StyleTag
    {
        private int _level = -1;

#if UNUSED_CODE
        internal StyleTag() 
        {
        }
#endif

        internal StyleTag(int level)
        {
            _level = level;
        }

        internal virtual int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }

        internal abstract void CloseTag(WriterState state);
    }

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class BoldStyleTag : StyleTag
    {
        internal BoldStyleTag(int level) : base(level)
        {
        }

        internal override void CloseTag(WriterState state)
        {
            state.Writer.WriteEndTag("b");
            state.Current.Bold = false;
        }
    }

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ItalicStyleTag : StyleTag
    {
        internal ItalicStyleTag(int level) : base(level)
        {
        }

        internal override void CloseTag(WriterState state)
        {
            state.Writer.WriteEndTag("i");
            state.Current.Italic = false;
        }
    }

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class FontStyleTag : StyleTag
    {
        private String _name;
        private Color _color;
        private FontSize _size;

        internal FontStyleTag(int level) : base(level)
        {
            _name = String.Empty;
            _color = Color.Empty;
            _size = FontSize.Normal;
        }

#if UNUSED_CODE
        internal FontStyleTag(int level, String name, Color color, FontSize size) : base(level)
        {
            Name = _name;
            Color = color;
            FontSize = size;
        }
#endif

        internal String Name
        {
            get { return _name; }
            set { _name = value; }
        }
        internal Color Color
        {
            get { return _color; }
            set { _color = value; }
        }
        internal FontSize FontSize
        {
            get { return _size; }
            set { _size = value; }
        }

        internal override void CloseTag(WriterState state)
        {
            state.Writer.WriteEndTag("font");
            //reset FontLevel and rebuild state info
            state.FontLevel = -1;
            state.Current.FontColor = Color.Empty;
            state.Current.FontName = String.Empty;
            state.Current.FontSize = FontSize.Normal;

            //reset the FontLevel
            Stack tmpStack = new Stack();
            while(state.TagsWritten.Count > 0)
            {
                Object o = state.TagsWritten.Pop();
                tmpStack.Push(o);
                if(o is FontStyleTag)
                {
                    state.FontLevel = ((FontStyleTag)o).Level;
                    break;
                }
            }

            while(tmpStack.Count > 0)
            {
                state.TagsWritten.Push(tmpStack.Pop());
            }

            //there is a font tag in the stack
            if(state.FontLevel > -1)
            {
                if(Color != Color.Empty)
                {
                    //reset font color to something further down the stack
                    Stack tempStack = new Stack();
                    while(state.TagsWritten.Count > 0)
                    {
                        Object o = state.TagsWritten.Pop();
                        tempStack.Push(o);
                        if(o is FontStyleTag)
                        {
                            if(((FontStyleTag)o).Color != Color.Empty)
                            {
                                state.Current.FontColor = ((FontStyleTag)o).Color;
                                break;
                            }
                        }
                    }
                    while(tempStack.Count > 0)
                    {
                        state.TagsWritten.Push(tempStack.Pop());
                    }
                }
                if(Name == null || Name.Length > 0)
                {
                    //reset font name to something futher down the stack
                    Stack tempStack = new Stack();
                    while(state.TagsWritten.Count > 0)
                    {
                        Object o = state.TagsWritten.Pop();
                        tempStack.Push(o);
                        if(o is FontStyleTag)
                        {
                            String name = ((FontStyleTag)o).Name;
                            if(name == null || name.Length > 0)
                            {
                                state.Current.FontName = name;
                                break;
                            }
                        }
                    }
                    while(tempStack.Count > 0)
                    {
                        state.TagsWritten.Push(tempStack.Pop());
                    }
                }
                    //reset font size to something further down the stack
                while(state.TagsWritten.Count > 0)
                {
                    Object o = state.TagsWritten.Pop();
                    tmpStack.Push(o);
                    if (o is FontStyleTag)
                    {
                        if(((FontStyleTag)o).FontSize != FontSize.Normal)
                        {
                            state.Current.FontSize = ((FontStyleTag)o).FontSize;
                            break;
                        }
                    }
                }
                while(tmpStack.Count > 0)
                {
                    state.TagsWritten.Push(tmpStack.Pop());
                }
            }
        }
    }

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DivStyleTag : StyleTag
    {
        private Wrapping _wrapping;
        private Alignment _alignment;
        private bool _alignWritten;
        internal DivStyleTag(int level) : base(level)
        {
            _wrapping = Wrapping.Wrap;
            _alignment = Alignment.Left;
            _alignWritten = false;
        }

        internal Wrapping Wrapping
        {
            get { return _wrapping; }
            set { _wrapping = value; }
        }
        internal Alignment Alignment
        {
            get { return _alignment; }
            set { _alignment = value; }
        }
        internal bool AlignmentWritten
        {
            get { return _alignWritten; }
            set { _alignWritten = value; }
        }

        internal override void CloseTag(WriterState state)
        {
            state.Writer.WriteEndTag("div");
            state.BreakPending = false;

            //reset current div info and rebuild
            state.DivLevel = -1;
            state.Current.Alignment = Alignment.Left;
            state.Current.Wrapping = Wrapping.Wrap;
            Stack tempStack = new Stack();
            //reset alignment : ideally these resets could be combined
            //in practice, the number of items on the stack is small
            //so it may be comparable


            //reset wrapping
            while(state.TagsWritten.Count > 0)
            {
                Object o = state.TagsWritten.Pop();
                tempStack.Push(o);
                if(o is DivStyleTag)
                {
                    if(((DivStyleTag)o).Wrapping == Wrapping.NoWrap)
                    {
                        state.Current.Wrapping = Wrapping.NoWrap;
                        break;
                    }
                }
            }
            while(tempStack.Count > 0)
            {
                state.TagsWritten.Push(tempStack.Pop());
            }

            //reset alignment
            while(state.TagsWritten.Count > 0)
            {
                Object o = state.TagsWritten.Pop();
                tempStack.Push(o);
                if(o is DivStyleTag)
                {
                    if(((DivStyleTag)o).Alignment != Alignment.NotSet)
                    {
                        state.Current.Alignment = ((DivStyleTag)o).Alignment;
                        break;
                    }
                }
            }
            while(tempStack.Count > 0)
            {
                state.TagsWritten.Push(tempStack.Pop());
            }

            //reset divLevel
            while(state.TagsWritten.Count > 0)
            {
                Object o = state.TagsWritten.Pop();
                tempStack.Push(o);
                if(o is DivStyleTag)
                {
                    state.DivLevel = ((DivStyleTag)o).Level;
                    break;
                }
            }
            while(tempStack.Count > 0)
            {
                state.TagsWritten.Push(tempStack.Pop());
            }
        }
    }

    /*
     * the StyleStack class maintains WriterStyle objects
     * pushed on the stack from Enter[Style/Format/Layout]
     * and removed using Exit[Style/Format/Layout]
     */
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class StyleStack
    {
        private HtmlMobileTextWriter _writer;
        private Stack _stack;

        protected StyleStack(HtmlMobileTextWriter writer)
        {
            _writer = writer;
            _stack = new Stack();
        }

        internal void Push(WriterStyle style)
        {
            _stack.Push(style);
            _writer.ShouldEnsureStyle = true;
        }

        internal WriterStyle Pop()
        {
            _writer.ShouldEnsureStyle = true;
            return (WriterStyle)_stack.Pop();
        }

        internal WriterStyle Peek()
        {
            if(_stack.Count == 0)
            {
                return new WriterStyle(); //retrieves default values
            }
            return (WriterStyle)_stack.Peek();
        }

        internal int Count
        {
            get
            {
                return _stack.Count;
            }
        }
    }

    /* the WriterState tracks what styles have been entered, what tags have been written
        and controls transitions from the current state to a desired state
    */
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class WriterState : StyleStack
    {
        private bool _inTransition = false; //prevent recursion
        private Stack _stack;  //stack of WriterStyle objects
        private Stack _tagsWritten; //stack of StyleTag objects for written tags
        private bool _breakPending = false; //track if we owe a <br>

        private WriterStyle _current; //the current style
        private HtmlMobileTextWriter _writer; //the output stream

        private int _fontLevel = -1;
        private int _divLevel = -1;
        
        private int _mark = 0;

        internal WriterState(HtmlMobileTextWriter writer) : base(writer)
        {
            _writer = writer;
            _stack = new Stack();
            _current = new WriterStyle();
            _tagsWritten = new Stack();
        }

        internal WriterStyle Current
        {
            get
            {
                return _current;
            }
        }

        /*
        Pushes the current WriterStyle and tagsWritten stack for later use,
        starts using a new default WriterStyle
        */
        internal void PushState()
        {
            _writer.ShouldEnsureStyle = true;
            _stack.Push(_current);
            _current = new WriterStyle();
            _stack.Push(_tagsWritten);
            _tagsWritten = new Stack();
            _stack.Push(BreakPending);
            BreakPending = false;

        }

        internal int FontLevel 
        {
            get { return _fontLevel; }
            set { _fontLevel = value; }
        }

        internal int DivLevel
        {
            get { return _divLevel; }
            set { _divLevel = value; }
        }

        /*
        Pops the last WriterStyle pushed and makes it current
        and restores the tagsWritten stack
        */
        internal WriterStyle PopState()
        {
            _writer.ShouldEnsureStyle = true;
            BreakPending = (bool)_stack.Pop();
            //close all open tags
            while(_tagsWritten.Count > 0)
            {
                CloseTag();
            }
            _tagsWritten = (Stack)_stack.Pop();
            _current = (WriterStyle)_stack.Pop();
            return _current;
        }

        /*
        BreakPending property accessor
        */
        internal bool BreakPending
        {
            get { return _breakPending; }
            set { _breakPending = value; }
        }

        internal HtmlTextWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        internal Stack TagsWritten
        {
            get
            {
                return _tagsWritten;
            }
        }



        /*
        pop a tag from the stack of StyleTags,
        adjust state accordingly
        */
        internal void CloseTag()
        {
            StyleTag tag = (StyleTag)_tagsWritten.Pop();
            tag.CloseTag(this);
        }

        internal void MarkStyleContext()
        {
            _mark = _tagsWritten.Count;
        }

        internal void UnMarkStyleContext()
        {
            while(_tagsWritten.Count > _mark)
            {
                CloseTag();
            }
        }

        private bool FontChange(WriterStyle newStyle)
        {
            return (
            (( _current.FontColor != newStyle.FontColor ) && (_writer.RenderFontColor)) ||
             (( _current.FontSize != newStyle.FontSize ) && (_writer.RenderFontSize)) ||
             (( _current.FontName != newStyle.FontName ) && (_writer.RenderFontName))
            );
        }

        private bool DivChange(WriterStyle newStyle)
        {
            return (
             (newStyle.Layout) &&
             (((newStyle.Wrapping != _current.Wrapping) && (_writer.RenderDivNoWrap)) ||
              ((newStyle.Alignment != _current.Alignment) && (_writer.RenderDivAlign)) )
             );
        }

        internal void Transition(WriterStyle newStyle)
        {
            Transition(newStyle, true);
        }

        private const String _pocketPC = "Pocket IE";

        internal void Transition(WriterStyle newStyle, bool captureOutput)
        {
            HtmlMobileTextWriter tempWriter = _writer;
            try
            {
                if(!captureOutput)
                {
                    tempWriter = _writer;
                    _writer = new HtmlMobileTextWriter(
                        new HtmlTextWriter(new StringWriter(CultureInfo.InvariantCulture)), tempWriter.Device);
                }

                if(_inTransition)
                {
                    return;
                }
                else
                {
                    _inTransition = true;
                }

                if(Count == 0)
                {
                    while(_tagsWritten.Count > 0)
                    {
                        CloseTag();
                    }
                    _inTransition= false;
                    return;
                }

                //close italic if target format !italic
                if(( _current.Italic && !newStyle.Italic ) && (_writer.RenderItalic))
                {
                    while(_current.Italic)
                    {
                        CloseTag();
                    }
                }

                //close bold if target format !bold
                if(( _current.Bold && !newStyle.Bold ) && (_writer.RenderBold))
                {
                    while(_current.Bold)
                    {
                        CloseTag();
                    }
                }

                //if the target FontColor is Color.Empty, then we need to 
                //close all open color tags
                if(
                    (newStyle.FontColor == Color.Empty) && 
                    (_current.FontColor != Color.Empty) && 
                    (_writer.RenderFontColor) )
                {
                    while(_current.FontColor != Color.Empty)
                    {
                        CloseTag();
                    }
                }

                //if the target FontName is String.Empty, then we need to
                //close all open name tags
                if(
                    (newStyle.FontName != null && newStyle.FontName.Length == 0) && 
                    (_current.FontName == null || _current.FontName.Length > 0) && 
                    (_writer.RenderFontName) )
                {
                    while(_current.FontName == null || _current.FontName.Length > 0)
                    {
                        CloseTag();
                    }
                }

                //close the font if it is of the same or a later generation
                //and differs

                bool newFont = FontChange(newStyle);

                if(newFont)
                {
                    while( FontLevel >= Count )
                    {
                        CloseTag();
                    }
                }

                //if the new wrapping is Wrap, and the current is NoWrap
                //the outer NoWrap must be removed
                if(
                    (newStyle.Wrapping == Wrapping.Wrap) && 
                    (_current.Wrapping == Wrapping.NoWrap) && 
                    (_writer.RenderDivNoWrap) )
                {
                    while(_current.Wrapping != Wrapping.Wrap)
                    {
                        CloseTag();
                    }
                }
                //if the alignment differs for the same generation, close any divs at this level
                if(( newStyle.Alignment != _current.Alignment ) && ( _writer.RenderDivAlign))
                {
                    while( DivLevel >= Count )
                    {
                        CloseTag();
                    }
                }

                //determine if we will be opening a div before writing any break
                bool newDiv = DivChange(newStyle);

                //an opening div will function as a logical break
                if((BreakPending) && (!(newDiv)))  
                {
                    ((HtmlMobileTextWriter)_writer).WriteBreak();
                    BreakPending = false;
                }

                if(newDiv)
                {
                    while(_current.Bold || _current.Italic || (FontLevel == Count))
                    {
                        CloseTag();
                    }
                }

                newFont = FontChange(newStyle);
                newDiv = DivChange(newStyle);

                //open div
                if(newDiv && newStyle.Layout)
                {
                    DivStyleTag div = new DivStyleTag(Count);
                    BreakPending = false;
                    if(
                        ((_writer.BeforeFirstControlWritten) || (_writer.InputWritten)) &&
                        (_writer.Device.Type == _pocketPC) &&
                        (_writer.Device.MinorVersion == 0) &&
                        (_writer.Device.MajorVersion == 4) &&
                        (newStyle.Alignment != _current.Alignment) )
                    {
                        _writer.WriteBreak();
                        _writer.InputWritten = false;
                    }

                        
                    _writer.WriteBeginTag("div");
                    DivLevel = Count;

                    if(newStyle.Wrapping == Wrapping.NoWrap)
                    {
                        if(_writer.RenderDivNoWrap)
                        {
                            _writer.Write(" nowrap");
                        }
                        div.Wrapping = Wrapping.NoWrap;
                        _current.Wrapping = Wrapping.NoWrap;
                    }
                    else
                    {
                        div.Wrapping = Wrapping.Wrap;
                        _current.Wrapping = Wrapping.Wrap;
                    }

                    if(newStyle.Alignment != _current.Alignment)
                    {
                        if(_writer.RenderDivAlign)
                        {
                            _writer.WriteAttribute(
                                "align", 
                                Enum.GetName(typeof(Alignment), newStyle.Alignment));
                        }
                        _current.Alignment = newStyle.Alignment;
                        div.Alignment = newStyle.Alignment;
                        div.AlignmentWritten = true;
                    }
                    _tagsWritten.Push(div);
                    _writer.Write(">");
                }

                //open font
                if(newFont && newStyle.Format)
                {
                    FontStyleTag fontTag = new FontStyleTag(Count);
                    _writer.WriteBeginTag("font");
                    if(_current.FontSize != newStyle.FontSize)
                    {
                        String relativeSize;
                        if(newStyle.FontSize == FontSize.Large)
                        {
                            relativeSize = (
                               ((HtmlMobileTextWriter)_writer).Device.Type == _pocketPC) ? "+2" : "+1";
                            _current.FontSize = FontSize.Large;
                            fontTag.FontSize = FontSize.Large;
                        }
                        else if(newStyle.FontSize == FontSize.Small)
                        {
                            relativeSize = "-1";
                            _current.FontSize = FontSize.Small;
                            fontTag.FontSize = FontSize.Small;
                        }
                        else //(newStyle.FontSize == FontSize.Normal)
                        {
                            relativeSize = "+0";
                            _current.FontSize = FontSize.Normal;
                            fontTag.FontSize = FontSize.Normal;
                        }
                        if(_writer.RenderFontSize)
                        {
                            _writer.WriteAttribute("size", relativeSize);
                        }
                    }

                    if(_current.FontColor != newStyle.FontColor)
                    {
                        if(_writer.RenderFontColor)
                        {
                            _writer.WriteAttribute(
                                "color", 
                                ColorTranslator.ToHtml(newStyle.FontColor));
                        }
                        _current.FontColor = newStyle.FontColor;
                        fontTag.Color = newStyle.FontColor;
                    }
                    if(_current.FontName != newStyle.FontName)
                    {
                        if(_writer.RenderFontName)
                        {
                            _writer.WriteAttribute("face", newStyle.FontName);
                        }
                        _current.FontName = newStyle.FontName;
                        fontTag.Name = newStyle.FontName;
                    }
                    _writer.Write(">");
                    _tagsWritten.Push(fontTag);
                    FontLevel = Count;
                }

                //open bold
                if(newStyle.Format)
                {
                    if( newStyle.Bold && !_current.Bold && _writer.RenderBold )
                    {
                        _writer.WriteFullBeginTag("b");
                        _current.Bold = true;
                        _tagsWritten.Push(new BoldStyleTag(Count));
                    }

                    //open italic
                    if( newStyle.Italic && !_current.Italic && _writer.RenderItalic )
                    {
                        _writer.WriteFullBeginTag("i");
                        _current.Italic = true;
                        _tagsWritten.Push(new ItalicStyleTag(Count));
                    }
                }
                _inTransition = false;
            }
            finally
            {
                _writer = tempWriter;
            }
        }
    }
}


