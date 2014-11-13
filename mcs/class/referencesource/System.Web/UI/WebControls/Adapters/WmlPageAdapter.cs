//------------------------------------------------------------------------------
// <copyright file="WmlPageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.Adapters {
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using System.Collections;

        public class WmlPageAdapter : PageAdapter {

        private static String _cacheExpiry = "<head>\r\n"
            + "<meta http-equiv=\"Cache-Control\" content=\"max-age=0\" forua=\"true\"/>\r\n"
            + "</head>\r\n";
        private static String _headerBegin = "<?xml version='1.0'";
        private static String _headerEncoding = " encoding ='{0}'";
        private static String _headerEnd = "?>\r\n"
            + "<!DOCTYPE wml PUBLIC '-//WAPFORUM//DTD WML 1.1//EN' 'http://www.wapforum.org/DTD/wml_1.1.xml'>";
        private const String _postBackEventArgumentVarName = "mcsva";
        private const String _postBackEventTargetVarName = "mcsvt";
        private const String _postUrlVarName = "mcsvp";

        // Mobile Internet Toolkit 5093
        private static readonly char[] _specialEncodingChars = new char[64] {
            '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  '/',  '-', '\0',  '+',  '=',  '*',
                '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  '.', '\0', '\0',
        };
        private static readonly Encoding _utf8Encoding = Encoding.GetEncoding("UTF-8");

        private IDictionary _dynamicPostFields = new ListDictionary();

        private bool _haveRequiresNoSoftkeyLabels = false;
        private bool _haveRequiresUTF8ContentEncoding = false;
        private int  _numberOfPostBacks;
        private bool _requiresUTF8ContentEncoding = false;
        // '+' <-> '-'
        // '=' <-> '.'
        // '/' <-> '*'
        private IDictionary _formVariables = null;  // Variables set in an onenterforward setvar at the top of the card.
        private string _queryString;
        private bool _requiresNoSoftkeyLabels = false;
        private IDictionary _staticPostFields = new ListDictionary();
        private bool _usePostBackCards = false;
        private bool _writtenFormVariables = false;
        private bool _writtenPostBack = false;

        private string QueryString {
            get {
                if (_queryString == null) {
                    // Ampersands are encoded by WriteEncodedText, called from RenderFormQueryString.
                    _queryString = Page.ClientQueryString;
                }
                return _queryString;
            }
        }

        // 

        internal bool WrittenFormVariables {
            get {
                return _writtenFormVariables;
            }
        }

        // Adds a form variable.
        public void AddFormVariable(WmlTextWriter writer, String clientID, String value, bool generateRandomID) {
            // On first (analyze) pass, form variables are added to
            // an array. On second pass, they are rendered. This ensures
            // that only visible controls generate variables.
            if (!writer.AnalyzeMode) {
                return;
            }
            if (_formVariables == null) {
                _formVariables = new ListDictionary();
            }

            // Map the client ID to a short name. See
            // MapClientIDToShortName for details.
            _formVariables[writer.MapClientIDToShortName(clientID, generateRandomID)] = value;
        }

        private void AnalyzeAndRenderHtmlForm(WmlTextWriter writer, HtmlForm form) {
            if (form == null) {
                return;
            }

            writer.SetAnalyzeMode(true);
            RenderForm(writer, form);
            Page.ResetOnFormRenderCalled();
            writer.SetAnalyzeMode(false);
            RenderForm(writer, form);
            writer.WriteLine();
        }


        protected virtual void AnalyzePostBack(WmlPostFieldType postBackType) {
            _numberOfPostBacks++;
        }

        //     Extracted into separate method for intelligibility.
        private void BeginForm(WmlTextWriter writer) {
            _writtenFormVariables = false;
            if (!writer.AnalyzeMode) {
                RenderBeginForm(writer);
            }
        }

        public override NameValueCollection DeterminePostBackMode() {
            NameValueCollection collection = base.DeterminePostBackMode();
            if (collection == null) {
                return null;
            }
            if (!StringUtil.EqualsIgnoreCase((string)Browser["requiresSpecialViewStateEncoding"], "true")) {
                return collection;
            }

            collection = ReplaceSpeciallyEncodedState(collection);
            return collection;
        }

        // 

        private bool DoesBrowserSupportAccessKey() {
            return false;
        }

        internal String EncodeSpecialViewState(String pageState) {
            // Mobile Internet Toolkit 5093.
            // Note: This 'trades' certain characters for other characters, so applying it twice is an identity
            // transformation.
            char[] viewstate = pageState.ToCharArray();

            for (int i = 0; i < viewstate.Length; i++) {
                char currentChar = viewstate[i];

                // Only check character replacement if within the range
                if (currentChar < _specialEncodingChars.Length) {
                    char encodingChar = _specialEncodingChars[currentChar];
                    if (encodingChar != '\0') {
                        viewstate[i] = encodingChar;
                    }
                }
            }
            return new String(viewstate);
        }

        private void EndForm(WmlTextWriter writer) {
            if (writer.AnalyzeMode) {
                // Analyze form when done.
                ((WmlPageAdapter)PageAdapter).PostAnalyzeForm();
            }
            else {
                RenderEndForm(writer);
            }
        }

        // Return a session page state persister to reduce view state size on the client.
        public override PageStatePersister GetStatePersister() {
            return new SessionPageStatePersister(Page);
        }


        /// <internalonly/>
        // VSWhidbey 80467: Need to adapt id separator.
        public override char IdSeparator {
            get {
                return ':';
            }
        }

        // Initialization of writer state should go here.
        private void InitializeWriter(WmlTextWriter writer) {
            writer.CurrentForm = Page.Form;
        }

        public virtual void PostAnalyzeForm() {
            if (_numberOfPostBacks > 1) {
                _usePostBackCards = true;
            }
        }

        public void RegisterPostField(WmlTextWriter writer, Control control) {
            RegisterPostField(writer, control.UniqueID, control.ClientID, true, false);
        }

        public void RegisterPostField(WmlTextWriter writer, string fieldName, string clientValue, bool isDynamic, bool random) {
            if (!writer.AnalyzeMode) {
                return;
            }

            if (isDynamic) {
                // Dynamic value.
                // Map the client ID to a short name. See
                // MapClientIDToShortName for details.
                _dynamicPostFields[fieldName] = writer.MapClientIDToShortName(clientValue, random);
            }
            else {
                _staticPostFields[fieldName] = clientValue;
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            WmlTextWriter wmlWriter = (WmlTextWriter) writer;
            if (Page.Form == null) {
                throw new HttpException(SR.GetString(SR.PageAdapter_MustHaveFormRunatServer));
            }
            if (Page.HasRenderDelegate()) {
                throw new HttpException(SR.GetString(SR.PageAdapter_RenderDelegateMustBeInServerForm));
            }
            if (RequiresUTF8ContentEncoding()) {
                Page.Response.ContentEncoding = _utf8Encoding;
            }

            InitializeWriter(wmlWriter);
            RenderXmlHeader(wmlWriter);
            wmlWriter.WriteFullBeginTag("wml");
            RenderCacheExpiry(wmlWriter);
            HtmlForm form = Page.Form;
            AnalyzeAndRenderHtmlForm(wmlWriter, form);
            RenderPostBackCard(wmlWriter);
            wmlWriter.WriteEndTag("wml");
        }

        // Renders the beginning of the form.
        // 
        protected internal virtual void RenderBeginForm(WmlTextWriter writer) {

            RenderBeginCardTag(writer);

            // Write form variables.

            // 


            _writtenFormVariables = true;
            if (_formVariables == null) {
                _formVariables = new ListDictionary();
            }
            _formVariables[_postBackEventTargetVarName] = String.Empty; // Whidbey 18260
            _formVariables[_postBackEventArgumentVarName] = String.Empty;
            writer.Write("<onevent type=\"onenterforward\"><refresh>");
            RenderSetFormVariables(writer);
            RenderPostUrlFormVariable(writer);
            writer.WriteLine("</refresh></onevent>");
            writer.Write("<onevent type=\"onenterbackward\"><refresh>");
            RenderSetFormVariables(writer);
            RenderPostUrlFormVariable(writer);
            writer.WriteLine("</refresh></onevent>");
            // 
            writer.BeginFormOrPanel();
        }

        private void RenderPostUrlFormVariable(WmlTextWriter writer) {
            if (Page.ContainsCrossPagePost) {
                writer.WriteBeginTag("setvar");
                writer.WriteAttribute("name", _postUrlVarName);
                writer.Write(" value=\"");
                RenderPostBackUrl(writer, Page.RelativeFilePath);
                RenderFormQueryString(writer, QueryString);
                writer.Write("\" />");
            }
        }

        public override void RenderBeginHyperlink(HtmlTextWriter writer, string targetUrl, bool encodeUrl, string softkeyLabel, string accessKey) {
            WmlTextWriter wmlWriter = (WmlTextWriter)writer;
            if (wmlWriter.AnalyzeMode) {
                return;
            }

            // Valid values are null, String.Empty, and single character strings
            if ((accessKey != null) && (accessKey.Length > 1)) {
                throw new ArgumentOutOfRangeException("accessKey");
            }

            // If the softkey label is too long, let the device choose a default softkey label.
            softkeyLabel = ResolveSoftkeyLabel(softkeyLabel);
            wmlWriter.WriteBeginTag("a");
            wmlWriter.Write(" href=\"");
            if (encodeUrl) {
                targetUrl = targetUrl.Replace("$", "$$");
                targetUrl = HttpUtility.HtmlAttributeEncode(targetUrl); // Leaves "$" alone.
                wmlWriter.Write(targetUrl);
            }
            else {
                wmlWriter.Write(wmlWriter.EscapeAmpersand(targetUrl));
            }
            wmlWriter.Write("\"");
            if (softkeyLabel != null && softkeyLabel.Length > 0 && !RequiresNoSoftkeyLabels)
                wmlWriter.WriteAttribute("title", softkeyLabel, false /* encode */);
            if (accessKey != null && accessKey.Length > 0 && DoesBrowserSupportAccessKey())
                wmlWriter.WriteAttribute("accessKey", accessKey, false /* encode */);
            wmlWriter.Write(">");
        }

        public virtual void RenderBeginPostBack(WmlTextWriter writer, string softkeyLabel, string accessKey) {
            if (writer.AnalyzeMode) {
                return;
            }

            // If the softkey label is too long, let the device choose a default softkey label.
            softkeyLabel = ResolveSoftkeyLabel(softkeyLabel);
            writer.WriteBeginTag("anchor");
            if (softkeyLabel != null && softkeyLabel.Length > 0 && !RequiresNoSoftkeyLabels)
                writer.WriteAttribute("title", softkeyLabel, false /* encode Whidbey 17925 */);
            if (accessKey != null && accessKey.Length > 0 && DoesBrowserSupportAccessKey())
                writer.WriteAttribute("accessKey", accessKey);
            writer.Write(">");
        }

        //     Renders the cache expiry as a header or meta element.
        private void RenderCacheExpiry(WmlTextWriter writer) {
            if (!StringUtil.EqualsIgnoreCase(Browser["SupportsCacheControlMetaTag"], "false")) {
                writer.Write(_cacheExpiry);
            }
            else {
                Page.Response.AppendHeader("Cache-Control", "max-age=0");
            }
        }

        // Renders a card tag.
        protected virtual void RenderBeginCardTag(WmlTextWriter writer) {
            writer.WriteLine("<card>");
            writer.Indent++;
        }

        // Renders the end of the form.
        protected internal virtual void RenderEndForm(WmlTextWriter writer) {
            writer.CloseParagraph();
            writer.Indent--;
            writer.WriteEndTag("card");
            writer.WriteLine();
        }

        public override void RenderEndHyperlink(HtmlTextWriter writer) {
            WmlTextWriter wmlWriter = (WmlTextWriter)writer;
            if (wmlWriter.AnalyzeMode) {
                return;
            }

            wmlWriter.WriteEndTag("a");
        }

        public virtual void RenderEndPostBack(WmlTextWriter writer, String target, String argument, String postUrl) {
            if (writer.AnalyzeMode) {
                // Analyze postbacks to see if postback cards should
                // be rendered.
                AnalyzePostBack(WmlPostFieldType.Submit);
            }
            else {
                RenderGoAction(writer, target, argument, postUrl);
                writer.WriteEndTag("anchor");
            }
        }

        protected virtual void RenderForm(WmlTextWriter writer, HtmlForm form) {
            Page.OnFormRender();
            BeginForm(writer);
            form.RenderChildren(writer);
            EndForm(writer);
            Page.OnFormPostRender();
        }

        //     Render the method attribute of a go action.
        private void RenderFormMethodAttribute(WmlTextWriter writer, string method) {
            // Method defaults to get in WML, so write it if it's not.
            if (StringUtil.EqualsIgnoreCase(method, "post")) {
                writer.WriteAttribute("method", "post");
            }
        }

        //     Render a complete form post in a go action.  This is used when rendering a postback card, or when
        //     rendering a go action that posts back directly rather than redirecting to a postback card.
        private void RenderFormPostInGoAction(WmlTextWriter writer, string target, string argument, WmlPostFieldType postFieldType, String postUrl) {
            writer.WriteBeginTag("go");
            writer.Write(" href=\"");

            if (!Page.ContainsCrossPagePost) {
                RenderPostBackUrl(writer, Page.RelativeFilePath);
                RenderFormQueryString(writer, QueryString);
            }
            else if (!String.IsNullOrEmpty(postUrl)) {
                RenderPostBackUrl(writer, postUrl);
            }
            else {
                writer.Write("$(");
                writer.Write(_postUrlVarName);
                if (!StringUtil.EqualsIgnoreCase((string)Browser["requiresNoescapedPostUrl"], "false")) {
                    writer.Write(":noescape");
                }
                writer.Write(")");
             }
            writer.Write("\"");

            string method = Page.Form.Method;
            RenderFormMethodAttribute(writer, method);
            writer.Write(">");

            string clientState = ClientState;
            if (clientState != null) {
                ICollection stateChunks = Page.DecomposeViewStateIntoChunks();

                int numChunks = stateChunks.Count;
                if (numChunks > 1) {
                    RenderStatePostField(writer, Page.ViewStateFieldCountID, stateChunks.Count.ToString(CultureInfo.CurrentCulture));
                }

                int count = 0;
                foreach (String state in stateChunks) {
                    string key = Page.ViewStateFieldPrefixID;
                    if (count > 0 ) {
                        key += count.ToString(CultureInfo.CurrentCulture);
                    }
                    RenderStatePostField(writer, key, state);
                    ++count;
                }
            }

            RenderReferrerPagePostField(writer);
            RenderTargetAndArgumentPostFields(writer, target, argument, postFieldType);
            RenderPostFieldVariableDictionary(writer, _dynamicPostFields);
            RenderPostFieldDictionary(writer, _staticPostFields);
            // 
            writer.WriteEndTag("go");
        }

        //     Renders the Form query string.
        private void RenderFormQueryString(WmlTextWriter writer, string queryString) {
            if (String.IsNullOrEmpty(queryString)) {
                return;
            }
            writer.Write("?");
            // 

            if (!StringUtil.EqualsIgnoreCase((string)Browser["canRenderOneventAndPrevElementsTogether"], "false")) {
                queryString = writer.ReplaceFormsCookieWithVariable(queryString);
            }
            writer.WriteEncodedText(queryString);
        }

        public virtual void RenderGoAction(WmlTextWriter writer, String target, String argument, String postUrl) {
            if (UsePostBackCard()) {
                RenderGoActionToPostbackCard(writer, target, argument, postUrl);
            }
            else {
                RenderFormPostInGoAction(writer, target, argument, WmlPostFieldType.Normal, postUrl);
            }
        }

        private void RenderGoActionToPostbackCard(WmlTextWriter writer, String target, String argument, String postUrl) {
            // If using postback cards, render a go action to the given
            // postback card, along with setvars setting the target and
            // argument.
            writer.WriteBeginTag("go");
            writer.Write(" href=\"");
            _writtenPostBack = true;
            writer.Write("#");
            writer.Write(WmlTextWriter.PostBackWithVarsCardID);
            writer.Write("\">");
            writer.WriteBeginTag("setvar");
            writer.WriteAttribute("name", _postBackEventTargetVarName);
            writer.WriteAttribute("value", target);
            writer.Write("/>");
            writer.WriteBeginTag("setvar");
            writer.WriteAttribute("name", _postBackEventArgumentVarName);
            writer.Write(" value=\"");
            if (argument != null) {
                writer.WriteEncodedText(argument);
            }
            writer.Write("\"/>");

            if (!String.IsNullOrEmpty(postUrl)) {
                writer.WriteBeginTag("setvar");
                writer.WriteAttribute("name", _postUrlVarName);
                writer.Write(" value=\"");
                writer.WriteEncodedUrl(postUrl);
                writer.Write("\"/>");
            }

            writer.WriteEndTag("go");
        }

        //     Renders postback cards.
        private void RenderPostBackCard(WmlTextWriter writer) {
            if (!_writtenPostBack) {
                return;
            }

            writer.WriteBeginTag("card");
            writer.WriteAttribute("id", WmlTextWriter.PostBackWithVarsCardID);
            writer.WriteLine(">");

            writer.Write("<onevent type=\"onenterforward\">");
            RenderFormPostInGoAction(writer, null, _postBackEventArgumentVarName, WmlPostFieldType.Variable, String.Empty);
            // 
            writer.WriteLine("</onevent>");

            writer.WriteLine("<onevent type=\"onenterbackward\"><prev /></onevent>");
            writer.WriteLine("</card>");
        }

        public override void RenderPostBackEvent(HtmlTextWriter writer, string target, string argument, string softkeyLabel, string text, string postUrl, string accessKey) {
            WmlTextWriter wmlWriter = writer as WmlTextWriter;
            if (wmlWriter == null) {
                base.RenderPostBackEvent(writer, target, argument, softkeyLabel, text, postUrl, accessKey);
                return;
            }
            if (String.IsNullOrEmpty(softkeyLabel)) {
                softkeyLabel = text;
            }

            if (!String.IsNullOrEmpty(postUrl)) {
                Page.ContainsCrossPagePost = true;
            }

            RenderBeginPostBack((WmlTextWriter)writer, softkeyLabel, accessKey);
            wmlWriter.Write(text);
            RenderEndPostBack((WmlTextWriter)writer, target, argument, postUrl);
        }

        private void RenderPostBackUrl(WmlTextWriter writer, string path) {
            if ((String)Browser["requiresAbsolutePostbackUrl"] == "true" && Page != null && Page.Request != null && Page.Response != null) {
                // ApplyAppPathModifier makes the path absolute
                writer.WriteEncodedUrl(Page.Response.ApplyAppPathModifier(path));
            }
            else {
                writer.WriteEncodedUrl(path);
            }
        }

        //     Render a postfield dictionary with non-variable values.
        private void RenderPostFieldDictionary(WmlTextWriter writer, IDictionary postFieldDictionary) {
            foreach (DictionaryEntry entry in postFieldDictionary) {
                writer.WritePostField((string)entry.Key, (string)entry.Value);
            }
        }


        //     Render a postfield dictionary with variable values.
        private void RenderPostFieldVariableDictionary(WmlTextWriter writer, IDictionary postFieldDictionary) {
            foreach (DictionaryEntry entry in postFieldDictionary) {
                writer.WritePostFieldVariable((string)entry.Key, (string)entry.Value);
            }
        }


        //     If the form action corresponds to a cross page post, render the referrer page in a post field.
        private void RenderReferrerPagePostField(WmlTextWriter writer) {
            if (Page.ContainsCrossPagePost) {
                writer.WritePostField(Page.previousPageID, Page.EncryptString(Page.Request.CurrentExecutionFilePath));
            }
        }

        // Render a select option.
        public virtual void RenderSelectOption(WmlTextWriter writer, string text) {
            if (writer.AnalyzeMode) {
                return;
            }

            writer.WriteFullBeginTag("option");
            writer.WriteEncodedText(text);
            writer.WriteEndTag("option");
        }

        public virtual void RenderSelectOption(WmlTextWriter writer, String text, String value) {
            if (writer.AnalyzeMode) {
                return;
            }

            writer.WriteBeginTag("option");
            writer.WriteAttribute("value", value, true);
            writer.Write(">");
            writer.WriteEncodedText(text);
            writer.WriteEndTag("option");
        }

        public virtual void RenderSelectOptionWithNavigateUrl(WmlTextWriter writer, String text, string navigateUrl) {
            if (writer.AnalyzeMode) {
                return;
            }

            writer.WriteBeginTag("option");
            writer.WriteAttribute("onpick", navigateUrl);
            writer.Write(">");
            writer.WriteEncodedText(text);
            writer.WriteEndTag("option");
        }

        public virtual void RenderSelectOptionAsPostBack(WmlTextWriter writer, string text) {
            RenderSelectOptionAsPostBack(writer, text, null, null);
        }

        public virtual void RenderSelectOptionAsPostBack(WmlTextWriter writer, string text, String target, String argument) {
            if (writer.AnalyzeMode) {
                return;
            }

            writer.WriteFullBeginTag("option");
            writer.WriteBeginTag("onevent");
            writer.WriteAttribute("type", "onpick");
            writer.Write(">");
            writer.WriteBeginTag("go");
            writer.WriteAttribute("href", "#" + WmlTextWriter.PostBackWithVarsCardID);
            writer.Write(">");
            if (!String.IsNullOrEmpty(target)) {
                writer.WriteBeginTag("setvar");
                writer.WriteAttribute("name", _postBackEventTargetVarName);
                writer.WriteAttribute("value", target);
                writer.Write(" />");
            }
            if (!String.IsNullOrEmpty(argument)) {
                writer.WriteBeginTag("setvar");
                writer.WriteAttribute("name", _postBackEventArgumentVarName);
                writer.WriteAttribute("value", argument);
                writer.Write(" />");
            }
            writer.WriteEndTag("go");
            writer.WriteEndTag("onevent");
            writer.WriteEncodedText(text);
            writer.WriteEndTag("option");
            _writtenPostBack = true;
            _usePostBackCards = true;
        }

        public void RenderSelectOptionAsAutoPostBack(WmlTextWriter writer, string text, string groupName, string value) {
            if (writer.AnalyzeMode) {
                return;
            }
            writer.WriteFullBeginTag("option");
            writer.WriteBeginTag("onevent");
            writer.WriteAttribute("type", "onpick");
            writer.Write(">");
            writer.WriteBeginTag("go");
            writer.WriteAttribute("href", "#" + WmlTextWriter.PostBackWithVarsCardID);
            writer.Write(">");
            writer.WriteBeginTag("setvar");
            writer.WriteAttribute("name", writer.MapClientIDToShortName(groupName, false));
            writer.WriteAttribute("value", value);
            writer.Write(" />");
            writer.WriteEndTag("go");
            writer.WriteEndTag("onevent");
            writer.WriteEncodedText(text);
            writer.WriteEndTag("option");
            _writtenPostBack = true;
            _usePostBackCards = true;
        }

        public void RenderSelectOptionAsAutoPostBack(WmlTextWriter writer, string text, string value) {
            if (writer.AnalyzeMode) {
                return;
            }

            writer.WriteBeginTag("option");
            if (!String.IsNullOrEmpty(value)) {
                writer.WriteAttribute("value", value, true);
            }
            writer.WriteAttribute("onpick", "#" + WmlTextWriter.PostBackWithVarsCardID);
            writer.Write(">");
            writer.WriteEncodedText(text);
            writer.WriteEndTag("option");
            // force use of postback cards with variables.
            _writtenPostBack = true;
            _usePostBackCards = true;
        }

        private void RenderSetFormVariables(WmlTextWriter writer) {
            foreach (DictionaryEntry entry in _formVariables) {
                writer.WriteBeginTag("setvar");
                writer.WriteAttribute("name", (String)entry.Key);
                writer.WriteAttribute("value", (String)entry.Value, true);
                writer.Write(" />");
            }
        }

        //     Render a postfield for view state or control state.
        private void RenderStatePostField(WmlTextWriter writer, string stateName, string stateValue) {
            if (stateValue == null) {
                return;
            }
            if (Browser["requiresSpecialViewStateEncoding"] == "true") {
                stateValue = ((WmlPageAdapter) Page.Adapter).EncodeSpecialViewState(stateValue);
            }
            writer.WritePostField(stateName, stateValue);
        }

        //     Render postfields for the event target and the event argument.
        private void RenderTargetAndArgumentPostFields(WmlTextWriter writer, string target, string argument, WmlPostFieldType postFieldType) {
            // Write the event target.
            if (target != null) {
                writer.WritePostField(Page.postEventSourceID, target);
            }
            else {
                // Target is null when the action is generated from a postback
                // card itself. In this case, set the event target to whatever
                // the original event target was.
                writer.WritePostFieldVariable(Page.postEventSourceID, _postBackEventTargetVarName);
            }

            // Write the event argument, if valid.

            if (argument != null) {
                if (postFieldType == WmlPostFieldType.Variable) {
                    writer.WritePostFieldVariable(Page.postEventArgumentID, argument);
                }
                else {
                    writer.WritePostField(Page.postEventArgumentID, argument);
                }
            }
        }

        //     Transforms text for the target device.  The default transformation is the identity transformation,
        //     which does not change the text.
        internal void RenderTransformedText(WmlTextWriter writer, string text) {
            bool leadingSpace = false;
            bool setPendingP = false;
            bool trailingSpace = false;

            // p's replaced by brs as in MMIT V1 for valid containment.
            text = LiteralControlAdapterUtility.PreprocessLiteralText(text);
            bool isEmpty = (text != null && text.Length == 0);
            if (isEmpty) {
                return;
            }

            if (writer.TopOfForm) {
                while (Regex.IsMatch(text, "^(?'space'\\s*)(?:<p|</p)\\s*>")) {
                    text = Regex.Replace(text, "^(?'space'\\s*)(?:<p|</p)\\s*>", "${space}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
            }

            if (setPendingP = Regex.IsMatch(text, "</p\\s*>\\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) {
                text = Regex.Replace(text, "</p\\s*>(?'space'\\s*)$", "${space}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            text = Regex.Replace(text, "<br\\s*/?>", "<br/>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            text = Regex.Replace(text, "</p\\s*>(?'space'\\s*)<p\\s*>", "<br/>${space}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            text = Regex.Replace(text, "(?:<p|</p)\\s*>", "<br/>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (trailingSpace = Regex.IsMatch(text, "\\s+$")) {
                text = Regex.Replace(text, "\\s+$", String.Empty);
            }
            if (leadingSpace = Regex.IsMatch(text, "^\\s+")) {
                text = Regex.Replace(text, "^\\s+", String.Empty);
            }

            text = text.Replace("$", "$$");

            // Render text.
            if (text.Trim().Length > 0) {
                if (leadingSpace) {
                    writer.WriteLine();
                }
                Style emptyStyle = new Style();
                writer.BeginRender(); // write pending tags.
                writer.EnterStyle(emptyStyle); // VSWhidbey 114083
                writer.Write(text);
                writer.ExitStyle(emptyStyle);
                writer.EndRender();
                if (trailingSpace) {
                    writer.WriteLine();
                }
            }
            // Whidbey 19653 transform space as newline.  If we are at the top of the form (before the leading p),
            // don't need literal text -it won't be rendered. Similarly, if we are setting a pending p, no need to writeline.
            else if (!setPendingP && !writer.TopOfForm) {
                Debug.Assert(!isEmpty, "Empty text.  Should have returned before this point.");
                writer.WriteLine();
            }

            if (setPendingP) {
                writer.SetPendingP();
            }
        }

        private void RenderXmlHeader(WmlTextWriter writer) {
            writer.Write(_headerBegin);
            String charset = Page.Response.Charset;
            if (charset != null && charset.Length > 0 &&
                !StringUtil.EqualsIgnoreCase(charset, "utf-8")) {
                writer.Write(String.Format(_headerEncoding, charset));
            }
            writer.Write(_headerEnd);
        }

        //    Reverse the special character replacement done when
        //    writing out the viewstate value.
        private NameValueCollection ReplaceSpeciallyEncodedState(NameValueCollection baseCollection) {
            // For each viewstate field
            int numViewStateFields = Convert.ToInt32(baseCollection[Page.ViewStateFieldCountID], CultureInfo.CurrentCulture);
            Hashtable newEntries = new Hashtable();
            for (int i=0; i<numViewStateFields; ++i) {
                string key = Page.ViewStateFieldPrefixID;
                if (i > 0) {
                    key += i.ToString(CultureInfo.CurrentCulture);
                }
                // Applying EncodeSpecialViewState twice returns a string to its
                // original form.
                string speciallyEncodedState = baseCollection[key];
                if (speciallyEncodedState != null) {
                    speciallyEncodedState = EncodeSpecialViewState(speciallyEncodedState);
                }

                newEntries.Add(key, speciallyEncodedState);
            }

            // We need to regenerate the collection since the
            // original baseCollection is readonly.
            NameValueCollection collection = new NameValueCollection();

            for (int i = 0; i < baseCollection.Count; i++) {
                String name = baseCollection.GetKey(i);
                string value = newEntries[name] as string;
                if (value != null) {
                    collection.Add(name, value);
                }
                else {
                    collection.Add(name, baseCollection.Get(i));
                }
            }
            return collection;
        }

        internal bool RequiresNoSoftkeyLabels {
            get {
                if (!_haveRequiresNoSoftkeyLabels) {
                    String RequiresNoSoftkeyLabelsString = Browser["requiresNoSoftkeyLabels"];
                    if (RequiresNoSoftkeyLabelsString == null) {
                        _requiresNoSoftkeyLabels = false;
                    }
                    else {
                        _requiresNoSoftkeyLabels = Convert.ToBoolean(RequiresNoSoftkeyLabelsString, CultureInfo.InvariantCulture);
                    }
                    _haveRequiresNoSoftkeyLabels = true;
                }
                return _requiresNoSoftkeyLabels;
            }
        }


        private bool RequiresUTF8ContentEncoding() {
            if (!_haveRequiresUTF8ContentEncoding) {
                String requiresUTF8ContentEncodingString = Browser["requiresUTF8ContentEncoding"];
                if (requiresUTF8ContentEncodingString == null) {
                    _requiresUTF8ContentEncoding = false;
                }
                else {
                    _requiresUTF8ContentEncoding = Convert.ToBoolean(requiresUTF8ContentEncodingString, CultureInfo.InvariantCulture);
                }
                _haveRequiresUTF8ContentEncoding = true;
            }
            return _requiresUTF8ContentEncoding;
        }

        //     Chooses between a developer specified softkey label and null (letting the device choose the softkey label).
        private string ResolveSoftkeyLabel(string softkeyLabel) {
            int maxLength = Convert.ToInt32(Browser["maximumSoftkeyLabelLength"], CultureInfo.InvariantCulture);
            string decodedSoftkeyLabel = HttpUtility.HtmlDecode(softkeyLabel);
            if (decodedSoftkeyLabel != null && decodedSoftkeyLabel.Length <= maxLength) {
                return softkeyLabel;
            }
            return null; // Let device choose the default softkey label.
        }

        public override string TransformText(string text) {
            return LiteralControlAdapterUtility.ProcessWmlLiteralText(text);
        }

        protected virtual bool UsePostBackCard() {
            return _usePostBackCards && Browser["canRenderPostBackCard"] != "false";
        }
    }
}

#endif


