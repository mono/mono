//------------------------------------------------------------------------------
// <copyright file="WmlTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#if WMLSUPPORT
namespace System.Web.UI {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Web.UI.Adapters;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.Adapters;
    using System.Web.Util;


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public class WmlTextWriter : HtmlTextWriter {
        private static readonly char[] _attributeCharacters = new char[] {'"', '&', '<', '>', '$'};
        private static Layout _defaultLayout = new Layout(HorizontalAlign.NotSet, true);


        public const String    PostBackWithVarsCardID = "__pbc1";

        private bool            _alwaysScrambleClientIDs = false;
        private EmptyTextWriter _analyzeWriter;
        private bool            _analyzeMode = false;
        private const String    _boldTag   = "b";
        private bool            _boldTagOpen   = false;
        // UNDONE: This HttpBrowserCapabilities instance couples the writer with the HttpContext and can be removed.
        private HttpBrowserCapabilities _browser = null;
        private IDictionary     _controlShortNames = null;
        private HtmlForm        _currentForm = null;
        private static Style    _defaultStyle = new Style();
        private bool            _paragraphOpen = false;
        private const String    _italicTag = "i";
        private bool            _italicTagOpen = false;
        private const String    _largeTag  = "big";
        private bool            _largeTagOpen = false;
        private Stack           _layoutStack = new Stack();
        private const int       _maxShortNameLength = 16;
        private int             _numberOfSoftkeys;
        private bool            _openingPWritten = true;  // True if the current control was immediately preceded by an opening p.  Valid between BeginRender and EndRender.
        private bool            _pendingP = false;
        private Stack           _panelStyleStack = new Stack();
        private const String    _postBackEventArgumentVarName = "mcsva";
        private const String    _postBackEventTargetVarName = "mcsvt";
        private IDictionary     _radioButtonGroups = new ListDictionary();
        private static Random   _random = new Random();
        private TextWriter      _realInnerWriter;
        private const String    _shortNamePrefix = "mcsv";
        private const String    _smallTag  = "small";
        private bool            _smallTagOpen = false;
        private Stack           _styleStack = new Stack();
        private bool            _topOfFormOrPanel = false; // True if at top of form or panel, before opening p.


        public WmlTextWriter(TextWriter writer) : this(writer, DefaultTabString) {
        }


        public WmlTextWriter(TextWriter writer, string tabString) : base(writer, tabString) {
            _realInnerWriter = writer;

            _numberOfSoftkeys = Convert.ToInt32(Browser["NumberOfSoftkeys"], CultureInfo.InvariantCulture);
            if (_numberOfSoftkeys > 2) {
                _numberOfSoftkeys = 2;
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        // AnalyzeMode is set to true during first analysis pass of rendering.
        public bool AnalyzeMode
        {
            get {
                return _analyzeMode;
            }
        }

        // UNDONE: This property couples the writer to the HttpContext and can be removed.
        private HttpBrowserCapabilities Browser {
            get {
                if (_browser == null && HttpContext.Current != null) {
                    _browser = HttpContext.Current.Request.Browser;
                }

                return _browser;
            }
        }



        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual HtmlForm CurrentForm {
            set {
                _currentForm = value;
            }
            get {
                return _currentForm;
            }
        }

        /// <devdoc>
        ///     <para> [To be supplied.]</para>
        /// <devdoc>
        private Layout CurrentLayout {
            get {
                if (_layoutStack.Count > 0) {
                    return(Layout) _layoutStack.Peek();
                }
                else {
                    return _defaultLayout;
                }
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public Style CurrentStyle {
            get {
                if (_styleStack.Count > 0) {
                    return(Style)_styleStack.Peek();
                }
                else {
                    return DefaultStyle;
                }
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual Style DefaultStyle
        {
            get
            {
                return _defaultStyle;
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected int NumberOfSoftkeys
        {
            get
            {
                return _numberOfSoftkeys;
            }
        }

        // See Whidbey 33012.
        internal override bool SkipRenderDelegates {
            get {
                return AnalyzeMode;
            }
        }



        /// <devdoc>
        /// <para>True if at top of form or panel, before opening p.</para>
        /// </devdoc>
        public bool TopOfForm {
            get {
                return _topOfFormOrPanel;
            }
        }



        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        // UNDONE: See whether this method can be replaced by calls to RenderBeginTag.
        public override void BeginRender() {
            if (AnalyzeMode) {
                return;
            }

            _openingPWritten = WritePendingP();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal void BeginFormOrPanel() {
            if (AnalyzeMode) {
                return;
            }

            _topOfFormOrPanel = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void BeginBlockLevelControl() {
            if (AnalyzeMode) {
                return;
            }
            if (_openingPWritten || _topOfFormOrPanel) {
                return;
            }

            CloseParagraph();
            OpenParagraph();
        }

        // Helper to reset all the internal state whenever Analyze mode changes.
        private void ClearFlags() {
            _paragraphOpen = false;
            _italicTagOpen = false;
            _largeTagOpen = false;
            _layoutStack = new Stack();
            _openingPWritten = true;  // True if the current control was immediately preceded by an opening p.  Valid between BeginRender and EndRender.
            _pendingP = false;
            _panelStyleStack = new Stack();
            _smallTagOpen = false;
            _styleStack = new Stack();
            _topOfFormOrPanel = false; // True if at top of form or panel, before opening p.

        }


        /// <devdoc>
        /// <para>Close any open paragraph.</para>
        /// </devdoc>
        protected internal virtual void CloseParagraph() {
            if (!_paragraphOpen) {
                return;
            }
            CloseCurrentStyleTags();
            Indent--;
            WriteLine();
            WriteEndTag("p");
            WriteLine();
            _paragraphOpen = false;
        }

        /// <devdoc>
        ///    <para>Close any open character formatting tags.  Public for TextBox adapter.</para>
        /// </devdoc>
        internal void CloseCurrentStyleTags() {
            if (_largeTagOpen) {
                WriteEndTag(_largeTag);
                _largeTagOpen = false;
            }
            if (_smallTagOpen) {
                WriteEndTag(_smallTag);
                _smallTagOpen = false;
            }
            if (_italicTagOpen) {
                WriteEndTag(_italicTag);
                _italicTagOpen = false;
            }
            if (_boldTagOpen) {
                WriteEndTag(_boldTag);
                _boldTagOpen = false;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void EndBlockLevelControl() {
            if (AnalyzeMode) {
                return;
            }

            _pendingP = true;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override void EnterStyle(Style style, HtmlTextWriterTag tag) {
            // Ignore tag for wml.
            if (AnalyzeMode) {
                return;
            }

            // All "block level controls" (controls that render using block level elements in HTML) call enterStyle
            // using a div.  Here we ensure that a new p is open for these controls to ensure line breaking behavior.
            if (tag == HtmlTextWriterTag.Div) {
                BeginBlockLevelControl();
            }

            Style stackStyle = new Style();
            stackStyle.CopyFrom(style);
            stackStyle.MergeWith(CurrentStyle);
            if (_panelStyleStack.Count > 0) {
                stackStyle.MergeWith((Style)_panelStyleStack.Peek());
            }
            _styleStack.Push(stackStyle); // updates CurrentStyle

            if (_paragraphOpen) {
                OpenCurrentStyleTags();
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override void ExitStyle(Style style, HtmlTextWriterTag tag) {
            // Ignore tag for wml.
            if (AnalyzeMode) {
                return;
            }
            // No need to call CloseCurrentStyleTags() here, because OpenCurrentStyleTags() closes anything
            // that is not current and already open.  Call to CurrentStyleTags() results in correct but
            // unnecessary extra tags.  VSWhidbey 156207.
            if (_styleStack.Count > 0) {
                _styleStack.Pop();
            }
            OpenCurrentStyleTags();

            // All "block level controls" (controls that render using block level elements in HTML) call exitStyle
            // using a div.  Here we ensure that a new p is open for these controls to ensure line breaking behavior.
            if (tag == HtmlTextWriterTag.Div) {
                EndBlockLevelControl();
            }

        }

        /// <devdoc>
        ///    <para>Escape '&' in XML if it hasn't been.</para>
        /// </devdoc>
        internal String EscapeAmpersand(String url) {
            if (url == null) {
                return null;
            }

            char ampersand = '&';
            string ampEscaped = "amp;";
            int ampPos = url.IndexOf(ampersand);
            while (ampPos != -1) {
                if (url.Length - ampPos <= ampEscaped.Length ||
                    url.Substring(ampPos + 1, ampEscaped.Length) != ampEscaped) {
                    url = url.Insert(ampPos + 1, ampEscaped);
                }
                ampPos = url.IndexOf(ampersand, ampPos + ampEscaped.Length + 1);
            }

            return url;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override void Flush() {
            if (AnalyzeMode) {
                return;
            }

            base.Flush();
        }


        /// <devdoc>
        ///    <para>Makes sure the writer has rendered character formatting tags corresponding to the current format.</para>
        /// </devdoc>
        private String GetRandomID(int length) {
            Byte[] randomBytes = new Byte[length];
            _random.NextBytes(randomBytes);

            char[] randomChars = new char[length];
            for (int i = 0; i < length; i++) {
                randomChars[i] = (char)((((int)randomBytes[i]) % 26) + 'a');
            }

            return new String(randomChars);
        }


        /// <devdoc>
        /// <para>
        /// MapClientIDToShortName provides a unique map of control ClientID properties
        /// to shorter names. In cases where a control has a very long ClientID, a
        /// shorter unique name is used. All references to the client ID on the page
        /// are mapped, resulting in the same postback regardless of mapping.
        /// MapClientIDToShortName also scrambles client IDs that need to be
        /// scrambled for security reasons.
        /// </para>
        /// </devdoc>
        protected internal String MapClientIDToShortName(String clientID, bool generateRandomID) {
            if (_alwaysScrambleClientIDs) {
                generateRandomID = true;
            }
            if (_controlShortNames != null) {
                String lookup = (String)_controlShortNames[clientID];
                if (lookup != null) {
                    return lookup;
                }
            }
            if (!generateRandomID) {
                bool shortID = clientID.Length < _maxShortNameLength;
                // Map names with underscores, colons, and conflicting names regardless of length.
                bool goodID = (clientID.IndexOf(':') == -1) &&
                    (clientID.IndexOf('_') == -1) &&
                    !NameConflicts(clientID);

                if (shortID && goodID) {
                    return clientID;
                }
            }
            if (_controlShortNames == null) {
                _controlShortNames = new ListDictionary();
            }

            String shortName;
            if (generateRandomID) {
                shortName = GetRandomID(5);
            }
            else {
                shortName = String.Empty;
            }
            shortName = String.Concat(_shortNamePrefix, shortName, _controlShortNames.Count.ToString(CultureInfo.InvariantCulture));
            _controlShortNames[clientID] = shortName;
            return shortName;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private bool NameConflicts(String name) {
            if (name == null) {
                return false;
            }

            Debug.Assert(_postBackEventTargetVarName.ToLower(CultureInfo.InvariantCulture) == _postBackEventTargetVarName &&
                _postBackEventArgumentVarName.ToLower(CultureInfo.InvariantCulture) == _postBackEventArgumentVarName &&
                _shortNamePrefix.ToLower(CultureInfo.InvariantCulture) == _shortNamePrefix);

            name = name.ToLower(CultureInfo.InvariantCulture);
            return name == _postBackEventTargetVarName ||
                name == _postBackEventArgumentVarName ||
                StringUtil.StringStartsWith(name, _shortNamePrefix);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void OpenParagraph() {
            Layout layout = CurrentLayout;
            OpenParagraph(layout,
                (layout != null) && (layout.Align != HorizontalAlign.NotSet),
                (layout != null) && !layout.Wrap);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        private void OpenParagraph(Layout layout, bool writeHorizontalAlign, bool writeWrapping) {
            CloseParagraph(); // does nothing if paragraph is not open.
            WriteBeginTag("p");
            if (writeHorizontalAlign) {
                String alignment;
                switch (layout.Align) {
                case HorizontalAlign.Right:
                    alignment = "right";
                    break;

                case HorizontalAlign.Center:
                    alignment = "center";
                    break;

                default:
                    alignment = "left";
                    break;
                }

                WriteAttribute("align", alignment);
            }
            if (writeWrapping) {
                WriteAttribute("mode",
                    layout.Wrap == true ? "wrap" : "nowrap");
            }
            Write(">");
            _paragraphOpen = true;
            Indent++;
            WriteLine();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal void OpenCurrentStyleTags() {
            // Review: This results in extra tags in some situations.  May want to get rid of the extra tags and make
            // this logic more object oriented.

            // Opening font tags are always kept in order in order <b>, <i>, <size>.  If anything is open out of order,
            // for example <i> is open and we want to open <b>, we close the "out of order" tag, then reopen it later,
            // if necessary.
            Style format = CurrentStyle;

            // First close all open elements we no longer want.
            if (_smallTagOpen) {
                if (format.Font.Size == FontUnit.Empty ||
                    format.Font.Size == FontUnit.Larger ||
                    format.Font.Size == FontUnit.Large ||
                    format.Font.Size == FontUnit.XLarge ||
                    format.Font.Size == FontUnit.XXLarge) {
                    WriteEndTag(_smallTag);
                    _smallTagOpen = false;
                }
            }
            if (_largeTagOpen) {
                if (format.Font.Size == FontUnit.Empty ||
                    format.Font.Size == FontUnit.Smaller ||
                    format.Font.Size == FontUnit.Small ||
                    format.Font.Size == FontUnit.XSmall ||
                    format.Font.Size == FontUnit.XXSmall) {
                    WriteEndTag(_largeTag);
                    _largeTagOpen = false;
                }
            }
            if (!format.Font.Italic && _italicTagOpen) {
                if (_smallTagOpen) {
                    WriteEndTag(_smallTag);
                    _smallTagOpen = false;
                }
                if (_largeTagOpen) {
                    WriteEndTag(_largeTag);
                    _largeTagOpen = false;
                }
                WriteEndTag(_italicTag);
                _italicTagOpen = false;
            }
            if (!format.Font.Bold  && _boldTagOpen) {
                if (_smallTagOpen) {
                    WriteEndTag(_smallTag);
                    _smallTagOpen = false;
                }
                if (_largeTagOpen) {
                    WriteEndTag(_largeTag);
                    _largeTagOpen = false;
                }
                if (_italicTagOpen) {
                    WriteEndTag(_italicTag);
                    _italicTagOpen = false;
                }
                WriteEndTag(_boldTag);
                _boldTagOpen = false;
            }

            // Now open any elements we need which are not yet open.
            if (format.Font.Bold  && !_boldTagOpen) {
                if (_smallTagOpen) {
                    WriteEndTag(_smallTag);
                    _smallTagOpen = false;
                }
                if (_largeTagOpen) {
                    WriteEndTag(_largeTag);
                    _largeTagOpen = false;
                }
                if (_italicTagOpen) {
                    WriteEndTag(_italicTag);
                    _italicTagOpen = false;
                }
                WriteFullBeginTag(_boldTag);
                _boldTagOpen = true;
            }
            if (format.Font.Italic && !_italicTagOpen) {
                if (_smallTagOpen) {
                    WriteEndTag(_smallTag);
                    _smallTagOpen = false;
                }
                if (_largeTagOpen) {
                    WriteEndTag(_largeTag);
                    _largeTagOpen = false;
                }
                WriteFullBeginTag(_italicTag);
                _italicTagOpen = true;
            }
            if (format.Font.Size != FontUnit.Empty) {
                if (format.Font.Size == FontUnit.Larger ||
                    format.Font.Size == FontUnit.Large ||
                    format.Font.Size == FontUnit.XLarge ||
                    format.Font.Size == FontUnit.XXLarge) {
                    if (!_largeTagOpen) {
                        WriteFullBeginTag(_largeTag);
                        _largeTagOpen = true;
                    }
                }
                if (format.Font.Size ==  FontUnit.Smaller ||
                    format.Font.Size == FontUnit.Small ||
                    format.Font.Size == FontUnit.XSmall ||
                    format.Font.Size == FontUnit.XXSmall) {
                    if (!_smallTagOpen) {
                        WriteFullBeginTag(_smallTag);
                        _smallTagOpen = true;
                    }
                }
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public void PopLayout() {
            if (_layoutStack.Count == 0) {
                Debug.Fail("Layout stack is empty.");
                return;
            }
            _layoutStack.Pop();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal void PopPanelStyle() {
            if (_panelStyleStack.Count == 0) {
                Debug.Fail("Stack is empty.");
                return;
            }
            _panelStyleStack.Pop();
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public void PushLayout(HorizontalAlign align, bool wrap) {
            Layout newLayout = new Layout(align, wrap);
            newLayout.MergeWith(CurrentLayout);
            _layoutStack.Push(newLayout);
        }

        /// <devdoc>
        ///    <para>Push style on stack, but do not open it.  Used for pending styles, as in the panel and page adapters.</para>
        /// </devdoc>
        internal void PushPanelStyle(Style style) {
            Style stackStyle = new Style();
            if (_panelStyleStack.Count == 0) {
                stackStyle.CopyFrom(style);
                _panelStyleStack.Push(stackStyle);
                return;
            }
            stackStyle.CopyFrom((Style)_panelStyleStack.Peek());
            stackStyle.MergeWith(style);
            _panelStyleStack.Push(stackStyle);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void RenderImage(String source, String localSource, String alternateText) {
            if (AnalyzeMode) {
                return;
            }

            WriteBeginTag("img");
            WriteAttribute("src", source, true);
            if (localSource != null) {
                WriteAttribute("localsrc", localSource, true);
            }
            WriteTextEncodedAttribute("alt", alternateText != null ? alternateText : String.Empty);
            Write(" />");
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal String ReplaceFormsCookieWithVariable(String queryString) {
            // UNDONE: MMIT FormsAuthentication not integrated yet
            return queryString;
        }



        public void SetAnalyzeMode(bool analyzeMode) {
            _analyzeMode = analyzeMode;
            if (analyzeMode) {
                _analyzeWriter = new EmptyTextWriter();
                InnerWriter = _analyzeWriter;
            }
            else {
                InnerWriter = _realInnerWriter;
            }
            ClearFlags();
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public void SetPendingP() {
            if (AnalyzeMode) {
                return;
            }

            _pendingP = true;
        }


        /// <devdoc>
        /// [To be supplied.]
        /// </devdoc>
        public override void WriteAttribute(String attribute, String value) {
            // Must double $'s for valid WML, so call WriteAttribute with encode == true.
            WriteAttribute(attribute, value, true /* encode */);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteAttribute(String attribute, String value, bool encode) {
            // If in analyze mode, we don't actually have to perform the conversion, because
            // it's not getting written anyway.

            // If the value is null, we return without writing anything.  This is different
            // from HtmlTextWriter, which writes the name of the attribute, but no value at all.
            // A name with no value is illegal in Wml.
            if (value == null) {
                return;
            }
            if (AnalyzeMode || !encode) {
                base.WriteAttribute(attribute, value, false /* encode */);
                return;
            }

            WriteTextEncodedAttribute(attribute, value);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void WriteBeginSelect(String name, String value, String iname, String ivalue, String title, bool multiSelect) {
            if (AnalyzeMode) {
                return;
            }

            // Select tags cannot appear inside character formatting tags,
            // so close any character formatting.
            CloseCurrentStyleTags();

            // Certain devices always render a break before a <select>.  If
            // we're on such a device, cancel any pending breaks.
            if (Browser["rendersBreakBeforeWmlSelectAndInput"] == "true") {
                // _pendingBreak = false;
            }

            //EnsureLayout();
            WriteBeginTag("select");
            if (!String.IsNullOrEmpty(name)) {
                // Map the client ID to a short name. See
                // MapClientIDToShortName for details.
                WriteAttribute("name", MapClientIDToShortName(name,  false));
            }
            if (!String.IsNullOrEmpty(value)) {
                WriteTextEncodedAttribute("value", value);
            }
            if (!String.IsNullOrEmpty(iname)) {
                // Map the client ID to a short name. See
                // MapClientIDToShortName for details.
                WriteAttribute("iname", MapClientIDToShortName(iname, false));
            }
            // UNDONE: The FormAdapter WrittenFormVariables property couples this writer with the form adapter.  Consider
            // removing the property somehow.
            if (!((WmlPageAdapter)CurrentForm.Page.Adapter).WrittenFormVariables && ivalue != null && ivalue.Length > 0) {
                WriteTextEncodedAttribute("ivalue", ivalue);
            }
            if (!String.IsNullOrEmpty(title)) {
                WriteTextEncodedAttribute("title", title);
            }
            if (multiSelect) {
                WriteAttribute("multiple", "true");
            }
            Write(">");
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteBreak() {
            WriteLine("<br/>");
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected internal void WriteTextEncodedAttribute(String attribute, String value) {
            // Unlike HTML encoding, we need to replace $ with $$, and <> with &lt; and &gt;.
            // We can't do this by piggybacking HtmlTextWriter.WriteAttribute, because it
            // would translate the & in &lt; or &gt; to &amp;. So we more or less copy the
            // ASP.NET code that does similar encoding.
            Write(' ');
            Write(attribute);
            Write("=\"");
            int cb = value.Length;
            int pos = value.IndexOfAny(_attributeCharacters);
            if (pos == -1) {
                Write(value);
            }
            else {
                char[] s = value.ToCharArray();
                int startPos = 0;
                while (pos < cb) {
                    if (pos > startPos) {
                        Write(s, startPos, pos - startPos);
                    }

                    char ch = s[pos];
                    switch (ch) {
                    case '\"':
                        Write("&quot;");
                        break;
                    case '&':
                        Write("&amp;");
                        break;
                    case '<':
                        Write("&lt;");
                        break;
                    case '>':
                        Write("&gt;");
                        break;
                    case '$':
                        Write("$$");
                        break;
                    }

                    startPos = pos + 1;
                    pos = value.IndexOfAny(_attributeCharacters, startPos);
                    if (pos == -1) {
                        Write(s, startPos, cb - startPos);
                        break;
                    }
                }
            }
            Write('\"');
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteEncodedUrl(String url) {
            if (url == null) {
                return;
            }

            int i = url.IndexOf('?');
            if (i != -1) {
                WriteUrlEncodedString(url.Substring(0, i), false);

                String s = url.Substring(i);
                if (s.IndexOf('$') != -1) {
                    s = s.Replace("$", "%24");
                }
                base.WriteEncodedText(s);
            }
            else {
                WriteUrlEncodedString(url, false);
            }
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteEncodedText(String text) {
            if (text == null) {
                return;
            }
            if (text.IndexOf('$') != -1) {
                text = text.Replace("$", "$$");
            }

            base.WriteEncodedText(text);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void WriteEndSelect() {
            if (AnalyzeMode) {
                return;
            }

            WriteEndTag("select");
            WriteLine();
            OpenCurrentStyleTags(); // Style tags are closed before writing a begin select, so reopen them here.
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private bool WritePendingP() {
            if (!_topOfFormOrPanel && !_pendingP) {
                return false; // no P written.
            }
            OpenParagraph();
            _topOfFormOrPanel = _pendingP = false;
            return true; // P written.
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void WritePlainText(String text) {
            if (text == null) {
                return;
            }
            if (text.IndexOf('$') != -1) {
                text = text.Replace("$", "$$");
            }

            Write(text);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal void WritePostField(String name, String value) {
            WritePostField(name, value, WmlPostFieldType.Normal);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal void WritePostField(String name, String value, WmlPostFieldType type) {
            Write("<postfield name=\"");
            Write(name);
            Write("\" value=\"");
            if (type == WmlPostFieldType.Variable) {
                Write("$(");
            }
            if (type == WmlPostFieldType.Normal) {
                if (Browser["requiresUrlEncodedPostfieldValues"] != "false") {
                    WriteEncodedUrlParameter(value);
                }
                else {
                    WriteEncodedText(value);
                }
            }
            else {
                Write(value);
            }
            if (type == WmlPostFieldType.Variable) {
                Write(")");
            }
            Write("\" />");
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal void WritePostFieldVariable(String name, String arg) {
            WritePostField(name, arg, WmlPostFieldType.Variable);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public void WriteText(String text, bool encodeText) {
            if (encodeText) {
                WriteEncodedText(text);
            }
            else {
                WritePlainText(text);
            }
        }
    }
}
#endif
