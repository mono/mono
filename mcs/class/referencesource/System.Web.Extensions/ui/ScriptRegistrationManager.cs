//------------------------------------------------------------------------------
// <copyright file="ScriptRegistrationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 

namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Handlers;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Web.UI;
    using AppSettings = System.Web.Util.AppSettings;

    internal sealed class ScriptRegistrationManager {
        private static Regex ScriptTagRegex = new Regex(
            @"<script" +
            @"(" +
                @"\s+(?<attrname>\w[-\w:]*)" +          // Attribute name
                @"(" +
                    @"\s*=\s*""(?<attrval>[^""]*)""|" + // ="bar" attribute value
                    @"\s*=\s*'(?<attrval>[^']*)'" +     // ='bar' attribute value
                @")" +
            @")*" +
            @"\s*(?<empty>/)?>",
            RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static Dictionary<ScriptKey, string> _fallbackScripts;
        private ScriptManager _scriptManager;
        private List<RegisteredDisposeScript> _scriptDisposes;
        private List<RegisteredArrayDeclaration> _scriptArrays;
        private List<RegisteredScript> _clientScriptBlocks;
        private List<RegisteredScript> _startupScriptBlocks;
        private List<RegisteredHiddenField> _hiddenFields;
        private List<RegisteredExpandoAttribute> _expandos;
        private List<RegisteredScript> _submitStatements;

        public ScriptRegistrationManager(ScriptManager scriptManager) {
            _scriptManager = scriptManager;
        }

        public List<RegisteredArrayDeclaration> ScriptArrays {
            get {
                if (_scriptArrays == null) {
                    _scriptArrays = new List<RegisteredArrayDeclaration>();
                }
                return _scriptArrays;
            }
        }

        public List<RegisteredScript> ScriptBlocks {
            get {
                if (_clientScriptBlocks == null) {
                    _clientScriptBlocks = new List<RegisteredScript>();
                }
                return _clientScriptBlocks;
            }
        }

        public List<RegisteredDisposeScript> ScriptDisposes {
            get {
                if (_scriptDisposes == null) {
                    _scriptDisposes = new List<RegisteredDisposeScript>();
                }
                return _scriptDisposes;
            }
        }

        public List<RegisteredExpandoAttribute> ScriptExpandos {
            get {
                if (_expandos == null) {
                    _expandos = new List<RegisteredExpandoAttribute>();
                }
                return _expandos;
            }
        }

        public List<RegisteredHiddenField> ScriptHiddenFields {
            get {
                if (_hiddenFields == null) {
                    _hiddenFields = new List<RegisteredHiddenField>();
                }
                return _hiddenFields;
            }
        }

        public List<RegisteredScript> ScriptStartupBlocks {
            get {
                if (_startupScriptBlocks == null) {
                    _startupScriptBlocks = new List<RegisteredScript>();
                }
                return _startupScriptBlocks;
            }
        }

        public List<RegisteredScript> ScriptSubmitStatements {
            get {
                if (_submitStatements == null) {
                    _submitStatements = new List<RegisteredScript>();
                }
                return _submitStatements;
            }
        }

        private Dictionary<ScriptKey, string> FallbackScripts {
            get {
                if (_fallbackScripts == null) {
                    _fallbackScripts = new Dictionary<ScriptKey, string>();
                }
                return _fallbackScripts;
            }
        }

        private static void CheckScriptTagTweenSpace(RegisteredScript entry, string text, int start, int length) {
            // Check the range between the matches to make sure there is no extraneous content
            string tweenSpace = text.Substring(start, length);
            if (tweenSpace.Trim().Length != 0) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptRegistrationManager_InvalidChars, entry.Type.FullName, entry.Key, tweenSpace));
            }
        }

        private bool IsControlRegistrationActive(
            List<UpdatePanel> updatingUpdatePanels,
            Control child,
            bool pageAlwaysActive) {
            
            // Determines if a registered resource, like a client script block, should be included in the partial
            // update response. It should be included if the owning control is a child of an updating update panel
            // or if the owning control is the Page and this is a type of resource where Page is allowed as an owner.
            // When page is the owner, it means always include the resource regardless of which update panels are
            // updating. Expandos and dispose scripts do not support Page as the owner.

            // is this a resource that allows Page as the owner, and is the owner the Page?
            if (pageAlwaysActive) {
                Page childAsPage = child as Page;
                if (childAsPage == _scriptManager.Page) {
                    // owner is page so the registration is automatically active
                    return true;
                }
            }

            // registration is active if owner is a child of any updating update panels
            if (updatingUpdatePanels != null && updatingUpdatePanels.Count > 0) {
                // navigate up the parent controls and see if any are an updating update panel.
                while (child != null) {
                    if (child is UpdatePanel) {
                        // enumerate with for loop instead of foreach so we don't recreate an enumerator.
                        // enumerate instead of using Contains so we do not have to cast or use a comparer.
                        for (int i = 0; i < updatingUpdatePanels.Count; i++) {
                            if (child == updatingUpdatePanels[i]) {
                                return true;
                            }
                        }
                    }
                    child = child.Parent;
                }
            }

            return false;
        }

        public static void RegisterArrayDeclaration(Control control, string arrayName, string arrayValue) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }

            control.Page.ClientScript.RegisterArrayDeclaration(arrayName, arrayValue);

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm != null) {
                RegisteredArrayDeclaration entry = new RegisteredArrayDeclaration(control, arrayName, arrayValue);
                sm.ScriptRegistration.ScriptArrays.Add(entry);
            }
        }

        public static void RegisterClientScriptBlock(Control control, Type type, string key, string script, bool addScriptTags) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }

            control.Page.ClientScript.RegisterClientScriptBlock(type, key, script, addScriptTags);

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm != null) {
                RegisteredScript entry =
                    new RegisteredScript(RegisteredScriptType.ClientScriptBlock,
                        control,
                        type,
                        key,
                        script,
                        addScriptTags);
                sm.ScriptRegistration.ScriptBlocks.Add(entry);
            }
        }

        public static void RegisterClientScriptInclude(Control control, Type type, string key, string url) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }

            control.Page.ClientScript.RegisterClientScriptInclude(type, key, url);

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm != null) {
                RegisteredScript entry = new RegisteredScript(control, type, key, url);
                sm.ScriptRegistration.ScriptBlocks.Add(entry);
            }
        }

        /// <summary>
        /// Registers a fallback script that would be rendered as a data item token during Ajax postbacks.
        /// Invoking this method has no effect on non-Ajax postback scenarios.
        /// </summary>
        public static void RegisterFallbackScriptForAjaxPostbacks(Control control, Type type, string key, string fallbackExpression, string fallbackPath) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm != null) {
                sm.ScriptRegistration.FallbackScripts[new ScriptKey(type, key)] = fallbackPath;
            }
        }

        public static void RegisterClientScriptResource(Control control, Type type, string resourceName) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            if (String.IsNullOrEmpty(resourceName)) {
                throw new ArgumentNullException("resourceName");
            }

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm == null) {
                control.Page.ClientScript.RegisterClientScriptResource(type, resourceName);
            }
            else {
                Assembly assembly = AssemblyResourceLoader.GetAssemblyFromType(type);
                ScriptReference script = new ScriptReference {
                    Name = resourceName,
                    Assembly = assembly.FullName,
                    IsDirectRegistration = true,
                    ClientUrlResolver = sm
                };
                string resourceUrl = script.GetUrlInternal(sm, sm.Zip);
                control.Page.ClientScript.RegisterClientScriptInclude(type, resourceName, resourceUrl, true);
                RegisteredScript entry = new RegisteredScript(control, type, resourceName, resourceUrl);
                sm.ScriptRegistration.ScriptBlocks.Add(entry);
            }
        }

        internal void RegisterDispose(Control control, string disposeScript) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }
            if (disposeScript == null) {
                throw new ArgumentNullException("disposeScript");
            }

            // Locate the parent UpdatePanel of the control
            Control parent = control.Parent;
            UpdatePanel parentUpdatePanel = null;
            while (parent != null) {
                parentUpdatePanel = parent as UpdatePanel;
                if (parentUpdatePanel != null) {
                    break;
                }
                parent = parent.Parent;
            }
            if (parentUpdatePanel != null) {
                // During async posts we build up a list of ScriptDisposes. Later
                // we go through the list and filter out ones that aren't inside
                // UpdatePanels that are refreshing.
                // DevDiv Bugs 128123: Build the list on non-async postbacks as well,
                // so that GetRegisteredDisposeScripts returns them.
                RegisteredDisposeScript entry = new RegisteredDisposeScript(control, disposeScript, parentUpdatePanel);
                ScriptDisposes.Add(entry);

                if (!_scriptManager.IsInAsyncPostBack) {
                    // During non-async requests we register script immediately to do the
                    // dispose. This is necessary because some controls will register as late
                    // as Render(), at which point it would be too late to build up a list
                    // for processing later.
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    // 256 seems like a nice number so that we don't have to resize the StringBuilder except in very rare cases
                    StringBuilder sb = new StringBuilder(256);
                    sb.Append("Sys.WebForms.PageRequestManager.getInstance()._registerDisposeScript(");
                    serializer.Serialize(parentUpdatePanel.ClientID, sb);
                    sb.Append(", ");
                    serializer.Serialize(disposeScript, sb);
                    sb.AppendLine(");");

                    // DevDiv Bugs 128123: Register directly with ClientScriptManager so that a RegisteredScript
                    // entry is not created. Otherwise, calls to RegisterDispose would result in viewable
                    // RegisteredScript entries through GetRegisteredStartupScripts().
                    _scriptManager.IPage.ClientScript.RegisterStartupScript(typeof(ScriptRegistrationManager),
                        _scriptManager.CreateUniqueScriptKey(),
                        sb.ToString(),
                        true);
                }
            }
        }

        public static void RegisterExpandoAttribute(Control control, string controlId, string attributeName, string attributeValue, bool encode) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }

            control.Page.ClientScript.RegisterExpandoAttribute(controlId, attributeName, attributeValue, encode);

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm != null) {
                RegisteredExpandoAttribute entry =
                    new RegisteredExpandoAttribute(control,
                        controlId,
                        attributeName,
                        attributeValue,
                        encode);
                sm.ScriptRegistration.ScriptExpandos.Add(entry);
            }
        }

        public static void RegisterHiddenField(Control control, string hiddenFieldName, string hiddenFieldInitialValue) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }

            control.Page.ClientScript.RegisterHiddenField(hiddenFieldName, hiddenFieldInitialValue);

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm != null) {
                RegisteredHiddenField entry =
                    new RegisteredHiddenField(control,
                        hiddenFieldName,
                        hiddenFieldInitialValue);
                sm.ScriptRegistration.ScriptHiddenFields.Add(entry);
            }
        }

        public static void RegisterOnSubmitStatement(Control control, Type type, string key, string script) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }

            control.Page.ClientScript.RegisterOnSubmitStatement(type, key, script);

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm != null) {
                RegisteredScript entry =
                    new RegisteredScript(RegisteredScriptType.OnSubmitStatement,
                        control,
                        type,
                        key,
                        script,
                        false);
                sm.ScriptRegistration.ScriptSubmitStatements.Add(entry);
            }
        }

        public static void RegisterStartupScript(Control control, Type type, string key, string script, bool addScriptTags) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (control.Page == null) {
                throw new ArgumentException(AtlasWeb.ScriptRegistrationManager_ControlNotOnPage, "control");
            }

            control.Page.ClientScript.RegisterStartupScript(type, key, script, addScriptTags);

            ScriptManager sm = ScriptManager.GetCurrent(control.Page);
            if (sm != null) {
                RegisteredScript entry =
                    new RegisteredScript(RegisteredScriptType.ClientStartupScript,
                        control,
                        type,
                        key,
                        script,
                        addScriptTags);
                sm.ScriptRegistration.ScriptStartupBlocks.Add(entry);
            }
        }

        public void RenderActiveArrayDeclarations(List<UpdatePanel> updatePanels, HtmlTextWriter writer) {
            Debug.Assert(writer != null, "Should always have a writer");
            List<RegisteredArrayDeclaration> entriesToRender = new List<RegisteredArrayDeclaration>();

            // For each entry registered in the page, check and see which ones
            // came from controls within UpdatePanels that are going to be updated.
            Control lastControl = null;
            foreach (RegisteredArrayDeclaration entry in ScriptArrays) {
                Control child = entry.Control;
                // if the owning control is the same as the last one that we know was active,
                // no need to check IsControlRegistrationActive
                bool isActive = ((lastControl != null) && (child == lastControl)) ||
                    IsControlRegistrationActive(updatePanels, child, true);

                if (isActive) {
                    lastControl = child;
                    if (!entriesToRender.Contains(entry)) {
                        entriesToRender.Add(entry);
                    }
                }
            }

            foreach (RegisteredArrayDeclaration activeRegistration in entriesToRender) {
                PageRequestManager.EncodeString(writer,
                    PageRequestManager.ArrayDeclarationToken,
                    activeRegistration.Name,
                    activeRegistration.Value);
            }
        }

        public void RenderActiveExpandos(List<UpdatePanel> updatePanels, HtmlTextWriter writer) {
            Debug.Assert(writer != null, "Should always have a writer");
            if (updatePanels == null) {
                return;
            }
            List<RegisteredExpandoAttribute> entriesToRender = new List<RegisteredExpandoAttribute>();

            // For each entry registered in the page, check and see which ones
            // came from controls within UpdatePanels that are going to be updated.
            Control lastControl = null;
            foreach (RegisteredExpandoAttribute entry in ScriptExpandos) {
                Control child = entry.Control;

                bool isActive = ((lastControl != null) && (child == lastControl)) ||
                    IsControlRegistrationActive(updatePanels, child, false);

                if (isActive) {
                    lastControl = child;
                    if (!entriesToRender.Contains(entry)) {
                        entriesToRender.Add(entry);
                    }
                }
            }

            foreach (RegisteredExpandoAttribute activeRegistration in entriesToRender) {
                string propertyReference = "document.getElementById('" +
                    activeRegistration.ControlId + "')['" + activeRegistration.Name + "']";
                string value;
                if (activeRegistration.Encode) {
                    value = "\"" + HttpUtility.JavaScriptStringEncode(activeRegistration.Value) + "\"";
                }
                else if (activeRegistration.Value != null) {
                    value = "\"" + activeRegistration.Value + "\"";
                }
                else {
                    value = "null";
                }
                PageRequestManager.EncodeString(writer, PageRequestManager.ExpandoToken, propertyReference, value);
            }
        }

        public void RenderActiveHiddenFields(List<UpdatePanel> updatePanels, HtmlTextWriter writer) {
            Debug.Assert(writer != null, "Should always have a writer");
            List<RegisteredHiddenField> entriesToRender = new List<RegisteredHiddenField>();
            ListDictionary uniqueEntries = new ListDictionary(StringComparer.Ordinal);

            // For each entry registered in the page, check and see which ones
            // came from controls within UpdatePanels that are going to be updated.
            Control lastControl = null;
            foreach (RegisteredHiddenField entry in ScriptHiddenFields) {
                Control child = entry.Control;

                bool isActive = ((lastControl != null) && (child == lastControl)) ||
                    IsControlRegistrationActive(updatePanels, child, true);

                if (isActive) {
                    lastControl = child;
                    if (!uniqueEntries.Contains(entry.Name)) {
                        entriesToRender.Add(entry);
                        uniqueEntries.Add(entry.Name, entry);
                    }
                }
            }

            foreach (RegisteredHiddenField activeRegistration in entriesToRender) {
                PageRequestManager.EncodeString(writer,
                    PageRequestManager.HiddenFieldToken,
                    activeRegistration.Name,
                    activeRegistration.InitialValue);
            }
        }

        private void RenderActiveScriptBlocks(List<UpdatePanel> updatePanels,
            HtmlTextWriter writer,
            string token,
            List<RegisteredScript> scriptRegistrations) {
            
            List<RegisteredScript> entriesToRender = new List<RegisteredScript>();
            // no comparer needed because it will contain ScriptKeys which implement Equals
            ListDictionary uniqueEntries = new ListDictionary();

            // For each entry registered in the page, check and see which ones
            // came from controls within UpdatePanels that are going to be updated.
            Control lastControl = null;
            foreach (RegisteredScript entry in scriptRegistrations) {
                Control child = entry.Control;

                bool isActive = ((lastControl != null) && (child == lastControl)) ||
                    IsControlRegistrationActive(updatePanels, child, true);

                if (isActive) {
                    lastControl = child;
                    ScriptKey scriptKey = new ScriptKey(entry.Type, entry.Key);
                    if (!uniqueEntries.Contains(scriptKey)) {
                        entriesToRender.Add(entry);
                        uniqueEntries.Add(scriptKey, entry);
                    }
                }
            }

            foreach (RegisteredScript activeRegistration in entriesToRender) {
                if (String.IsNullOrEmpty(activeRegistration.Url)) {
                    if (activeRegistration.AddScriptTags) {
                        PageRequestManager.EncodeString(writer,
                            token,
                            "ScriptContentNoTags",
                            activeRegistration.Script);
                    }
                    else {
                        WriteScriptWithTags(writer, token, activeRegistration);
                    }
                }
                else {
                    PageRequestManager.EncodeString(writer,
                        token,
                        "ScriptPath",
                        activeRegistration.Url);
                }

                string fallbackScriptPath;
                if (_fallbackScripts != null && _fallbackScripts.TryGetValue(new ScriptKey(activeRegistration.Type, activeRegistration.Key), out fallbackScriptPath)) {
                    // Only encode the fallback path and not the expression. On the client, we would use the success flag on load / readystatechanged to 
                    // determine if the script was successfully fetched.
                    PageRequestManager.EncodeString(writer,
                        "fallbackScript",
                        fallbackScriptPath,
                        content: null);
                }
            }
        }

        public void RenderActiveScriptDisposes(List<UpdatePanel> updatePanels, HtmlTextWriter writer) {
            Debug.Assert(writer != null, "Should always have a writer");
            if (updatePanels == null) {
                return;
            }

            // For each entry registered in the page, check and see which ones
            // came from controls within UpdatePanels that are going to be updated.
            foreach (RegisteredDisposeScript entry in ScriptDisposes) {
                if (IsControlRegistrationActive(updatePanels, entry.ParentUpdatePanel, false)) {
                    PageRequestManager.EncodeString(
                        writer,
                        PageRequestManager.ScriptDisposeToken,
                        entry.ParentUpdatePanel.ClientID,
                        entry.Script);
                }
            }
        }

        public void RenderActiveScripts(List<UpdatePanel> updatePanels, HtmlTextWriter writer) {
            Debug.Assert(writer != null, "Should always have a writer");
            // Client script blocks and includes go first
            RenderActiveScriptBlocks(updatePanels,
                writer,
                PageRequestManager.ScriptBlockToken,
                ScriptBlocks);

            // Startup scripts at end
            RenderActiveScriptBlocks(updatePanels,
                writer,
                PageRequestManager.ScriptStartupBlockToken,
                ScriptStartupBlocks);
        }

        public void RenderActiveSubmitStatements(List<UpdatePanel> updatePanels, HtmlTextWriter writer) {
            Debug.Assert(writer != null, "Should always have a writer");
            List<RegisteredScript> entriesToRender = new List<RegisteredScript>();
            // no comparer needed because it will contain ScriptKeys which implement Equals
            ListDictionary uniqueEntries = new ListDictionary();

            // For each entry registered in the page, check and see which ones
            // came from controls within UpdatePanels that are going to be updated.
            Control lastControl = null;
            foreach (RegisteredScript entry in ScriptSubmitStatements) {
                Control child = entry.Control;

                bool isActive = ((lastControl != null) && (child == lastControl)) ||
                    IsControlRegistrationActive(updatePanels, child, true);

                if (isActive) {
                    lastControl = child;
                    ScriptKey scriptKey = new ScriptKey(entry.Type, entry.Key);
                    if (!uniqueEntries.Contains(scriptKey)) {
                        entriesToRender.Add(entry);
                        uniqueEntries.Add(scriptKey, entry);
                    }
                }
            }

            foreach (RegisteredScript activeRegistration in entriesToRender) {
                PageRequestManager.EncodeString(writer, PageRequestManager.OnSubmitToken, null, activeRegistration.Script);
            }
        }

        private static void WriteScriptWithTags(HtmlTextWriter writer,
            string token,
            RegisteredScript activeRegistration) {

            // If the content already has script tags, we need to parse out the contents
            // so that the client doesn't have to. The contents may include more than one
            // script tag, but no other content (such as arbitrary HTML).
            string scriptContent = activeRegistration.Script;

            int lastIndex = 0;
            for (Match match = ScriptTagRegex.Match(scriptContent, lastIndex); match.Success; match = ScriptTagRegex.Match(scriptContent, lastIndex)) {
                CheckScriptTagTweenSpace(activeRegistration, scriptContent, lastIndex, match.Index - lastIndex);

                OrderedDictionary attrs = new OrderedDictionary();

                if (match.Groups["empty"].Captures.Count > 0) {
                    // Self-closing tag

                    // No need to do anything since attributes are processed later
                    lastIndex = match.Index + match.Length;
                }
                else {
                    // Open tag with explicit close tag

                    // Need to find close tag so that we can locate the inner contents
                    int indexOfEndOfScriptBeginTag = match.Index + match.Length;
                    int indexOfScriptEndTag = scriptContent.IndexOf("</script>", indexOfEndOfScriptBeginTag, StringComparison.OrdinalIgnoreCase);
                    if (indexOfScriptEndTag == -1) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptRegistrationManager_NoCloseTag, activeRegistration.Type.FullName, activeRegistration.Key));
                    }
                    string scriptBlockContents = scriptContent.Substring(indexOfEndOfScriptBeginTag, (indexOfScriptEndTag - indexOfEndOfScriptBeginTag));

                    // Turn the text content into a text attribute
                    attrs.Add("text", scriptBlockContents);

                    lastIndex = indexOfScriptEndTag + 9;
                }

                // Process all the explicit attributes on the script tag
                CaptureCollection attrnames = match.Groups["attrname"].Captures;
                CaptureCollection attrvalues = match.Groups["attrval"].Captures;
                for (int i = 0; i < attrnames.Count; i++) {
                    string attribName = attrnames[i].ToString();
                    string attribValue = attrvalues[i].ToString();

                    // DevDev Bugs 123213: script elements registered with RegisterStartupScript are normally rendered
                    // into the html of the page. Any html encoded values in the attributes are interpreted by the
                    // browser, so the actual data is not html encoded. We must HtmlDecode any attribute values we find
                    // here to remain consistent during async posts, since the data will be dynamically injected into
                    // the dom, bypassing the browser's natural html decoding.
                    attribValue = HttpUtility.HtmlDecode(attribValue);
                    attrs.Add(attribName, attribValue);
                }

                // Serialize the attributes to JSON and write them out
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                // Dev10# 877767 - Allow configurable UpdatePanel script block length
                // The default is JavaScriptSerializer.DefaultMaxJsonLength
                if (AppSettings.UpdatePanelMaxScriptLength > 0) {
                    serializer.MaxJsonLength = AppSettings.UpdatePanelMaxScriptLength;
                }  

                string attrText = serializer.Serialize(attrs);
                PageRequestManager.EncodeString(writer, token, "ScriptContentWithTags", attrText);
            }

            CheckScriptTagTweenSpace(activeRegistration, scriptContent, lastIndex, scriptContent.Length - lastIndex);

            if (lastIndex == 0) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptRegistrationManager_NoTags, activeRegistration.Type.FullName, activeRegistration.Key));
            }
        }
    }
}
