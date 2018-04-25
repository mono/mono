
using System.Threading.Tasks;

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

// OpenIssue : is it better to cache the current namespace decls for each elem
//  as the current code does, or should it just always walk the namespace stack?

namespace System.Xml {

    internal partial class XmlWellFormedWriter : XmlWriter {

        public override Task WriteStartDocumentAsync() {
            return WriteStartDocumentImplAsync(XmlStandalone.Omit);
        }

        public override Task WriteStartDocumentAsync(bool standalone) {
            return WriteStartDocumentImplAsync(standalone ? XmlStandalone.Yes : XmlStandalone.No);
        }

        public override async Task WriteEndDocumentAsync() {
            try {
                // auto-close all elements
                while (elemTop > 0) {
                    await WriteEndElementAsync().ConfigureAwait(false);
                }
                State prevState = currentState;
                await AdvanceStateAsync(Token.EndDocument).ConfigureAwait(false);

                if (prevState != State.AfterRootEle) {
                    throw new ArgumentException(Res.GetString(Res.Xml_NoRoot));
                }
                if (rawWriter == null) {
                    await writer.WriteEndDocumentAsync().ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteDocTypeAsync(string name, string pubid, string sysid, string subset) {
            try {
                if (name == null || name.Length == 0) {
                    throw new ArgumentException(Res.GetString(Res.Xml_EmptyName));
                }
                XmlConvert.VerifyQName(name, ExceptionType.XmlException);

                if (conformanceLevel == ConformanceLevel.Fragment) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_DtdNotAllowedInFragment));
                }

                await AdvanceStateAsync(Token.Dtd).ConfigureAwait(false);
                if (dtdWritten) {
                    currentState = State.Error;
                    throw new InvalidOperationException(Res.GetString(Res.Xml_DtdAlreadyWritten));
                }

                if (conformanceLevel == ConformanceLevel.Auto) {
                    conformanceLevel = ConformanceLevel.Document;
                    stateTable = StateTableDocument;
                }

                int i;

                // check characters
                if (checkCharacters) {
                    if (pubid != null) {
                        if ((i = xmlCharType.IsPublicId(pubid)) >= 0) {
                            throw new ArgumentException(Res.GetString(Res.Xml_InvalidCharacter, XmlException.BuildCharExceptionArgs(pubid, i)), "pubid");
                        }
                    }
                    if (sysid != null) {
                        if ((i = xmlCharType.IsOnlyCharData(sysid)) >= 0) {
                            throw new ArgumentException(Res.GetString(Res.Xml_InvalidCharacter, XmlException.BuildCharExceptionArgs(sysid, i)), "sysid");
                        }
                    }
                    if (subset != null) {
                        if ((i = xmlCharType.IsOnlyCharData(subset)) >= 0) {
                            throw new ArgumentException(Res.GetString(Res.Xml_InvalidCharacter, XmlException.BuildCharExceptionArgs(subset, i)), "subset");
                        }
                    }
                }

                // write doctype
                await writer.WriteDocTypeAsync(name, pubid, sysid, subset).ConfigureAwait(false);
                dtdWritten = true;
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        //check if any exception before return the task
        private Task TryReturnTask(Task task) {
            if (task.IsSuccess()) {
                return AsyncHelper.DoneTask;
            }
            else {
                return _TryReturnTask(task);
            }
        }

        private async Task _TryReturnTask(Task task) {
            try {
                await task.ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        //call nextTaskFun after task finish. Check exception.
        private Task SequenceRun(Task task, Func<Task> nextTaskFun) {
            if (task.IsSuccess()) {
                return TryReturnTask( nextTaskFun() );
            }
            else {
                return _SequenceRun(task, nextTaskFun);
            }
        }

        private async Task _SequenceRun(Task task, Func<Task> nextTaskFun) {
            try {
                await task.ConfigureAwait(false);
                await nextTaskFun().ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override Task WriteStartElementAsync(string prefix, string localName, string ns) {
            try {
                // check local name
                if (localName == null || localName.Length == 0) {
                    throw new ArgumentException(Res.GetString(Res.Xml_EmptyLocalName));
                }
                CheckNCName(localName);

                Task task = AdvanceStateAsync(Token.StartElement);
                if (task.IsSuccess()) {
                    return WriteStartElementAsync_NoAdvanceState(prefix, localName, ns);
                }
                else {
                    return WriteStartElementAsync_NoAdvanceState(task, prefix, localName, ns);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private Task WriteStartElementAsync_NoAdvanceState(string prefix, string localName, string ns) {
            try {
                // lookup prefix / namespace  
                if (prefix == null) {
                    if (ns != null) {
                        prefix = LookupPrefix(ns);
                    }
                    if (prefix == null) {
                        prefix = string.Empty;
                    }
                }
                else if (prefix.Length > 0) {
                    CheckNCName(prefix);
                    if (ns == null) {
                        ns = LookupNamespace(prefix);
                    }
                    if (ns == null || (ns != null && ns.Length == 0)) {
                        throw new ArgumentException(Res.GetString(Res.Xml_PrefixForEmptyNs));
                    }
                }
                if (ns == null) {
                    ns = LookupNamespace(prefix);
                    if (ns == null) {
                        Debug.Assert(prefix.Length == 0);
                        ns = string.Empty;
                    }
                }

                if (elemTop == 0 && rawWriter != null) {
                    // notify the underlying raw writer about the root level element
                    rawWriter.OnRootElement(conformanceLevel);
                }

                // write start tag
                Task task = writer.WriteStartElementAsync(prefix, localName, ns);
                if (task.IsSuccess()) {
                    WriteStartElementAsync_FinishWrite(prefix, localName, ns);
                }
                else {
                    return WriteStartElementAsync_FinishWrite(task, prefix, localName, ns);
                }
                return AsyncHelper.DoneTask;
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private async Task WriteStartElementAsync_NoAdvanceState(Task task, string prefix, string localName, string ns) {
            try {
                await task.ConfigureAwait(false);
                await WriteStartElementAsync_NoAdvanceState(prefix, localName, ns).ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private void WriteStartElementAsync_FinishWrite(string prefix, string localName, string ns) {
            try {
                // push element on stack and add/check namespace
                int top = ++elemTop;
                if (top == elemScopeStack.Length) {
                    ElementScope[] newStack = new ElementScope[top * 2];
                    Array.Copy(elemScopeStack, newStack, top);
                    elemScopeStack = newStack;
                }
                elemScopeStack[top].Set(prefix, localName, ns, nsTop);

                PushNamespaceImplicit(prefix, ns);

                if (attrCount >= MaxAttrDuplWalkCount) {
                    attrHashTable.Clear();
                }
                attrCount = 0;
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private async Task WriteStartElementAsync_FinishWrite(Task t, string prefix, string localName, string ns) {
            try {
                await t.ConfigureAwait(false);
                WriteStartElementAsync_FinishWrite(prefix, localName, ns);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override Task WriteEndElementAsync() {
            try {
                Task task = AdvanceStateAsync(Token.EndElement);

                return SequenceRun(task, WriteEndElementAsync_NoAdvanceState);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private Task WriteEndElementAsync_NoAdvanceState() {
            try {
                int top = elemTop;
                if (top == 0) {
                    throw new XmlException(Res.Xml_NoStartTag, string.Empty);
                }
                Task task;
                // write end tag
                if (rawWriter != null) {
                    task = elemScopeStack[top].WriteEndElementAsync(rawWriter);
                }
                else {
                    task = writer.WriteEndElementAsync();
                }

                return SequenceRun(task, WriteEndElementAsync_FinishWrite);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private Task WriteEndElementAsync_FinishWrite() {
            try {
                int top = elemTop;
                // pop namespaces
                int prevNsTop = elemScopeStack[top].prevNSTop;
                if (useNsHashtable && prevNsTop < nsTop) {
                    PopNamespaces(prevNsTop + 1, nsTop);
                }
                nsTop = prevNsTop;
                elemTop = --top;

                // check "one root element" condition for ConformanceLevel.Document
                if (top == 0) {
                    if (conformanceLevel == ConformanceLevel.Document) {
                        currentState = State.AfterRootEle;
                    }
                    else {
                        currentState = State.TopLevel;
                    }
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
            return AsyncHelper.DoneTask;
        }

        public override Task WriteFullEndElementAsync() {
            try {
                Task task = AdvanceStateAsync(Token.EndElement);

                return SequenceRun(task, WriteFullEndElementAsync_NoAdvanceState);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private Task WriteFullEndElementAsync_NoAdvanceState() {
            try {
                int top = elemTop;
                if (top == 0) {
                    throw new XmlException(Res.Xml_NoStartTag, string.Empty);
                }
                Task task;
                // write end tag
                if (rawWriter != null) {
                    task = elemScopeStack[top].WriteFullEndElementAsync(rawWriter);
                }
                else {
                    task = writer.WriteFullEndElementAsync();
                }

                return SequenceRun(task, WriteEndElementAsync_FinishWrite);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        protected internal override Task WriteStartAttributeAsync(string prefix, string localName, string namespaceName) {
            try {
                // check local name
                if (localName == null || localName.Length == 0) {
                    if (prefix == "xmlns") {
                        localName = "xmlns";
                        prefix = string.Empty;
                    }
                    else {
                        throw new ArgumentException(Res.GetString(Res.Xml_EmptyLocalName));
                    }
                }
                CheckNCName(localName);

                Task task = AdvanceStateAsync(Token.StartAttribute);
                if (task.IsSuccess()) {
                    return WriteStartAttributeAsync_NoAdvanceState(prefix, localName, namespaceName);
                }
                else {
                    return WriteStartAttributeAsync_NoAdvanceState(task, prefix, localName, namespaceName);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private Task WriteStartAttributeAsync_NoAdvanceState(string prefix, string localName, string namespaceName) {
            try {
                // lookup prefix / namespace  
                if (prefix == null) {
                    if (namespaceName != null) {
                        // special case prefix=null/localname=xmlns
                        if (!(localName == "xmlns" && namespaceName == XmlReservedNs.NsXmlNs))
                            prefix = LookupPrefix(namespaceName);
                    }
                    if (prefix == null) {
                        prefix = string.Empty;
                    }
                }
                if (namespaceName == null) {
                    if (prefix != null && prefix.Length > 0) {
                        namespaceName = LookupNamespace(prefix);
                    }
                    if (namespaceName == null) {
                        namespaceName = string.Empty;
                    }
                }

                if (prefix.Length == 0) {
                    if (localName[0] == 'x' && localName == "xmlns") {
                        if (namespaceName.Length > 0 && namespaceName != XmlReservedNs.NsXmlNs) {
                            throw new ArgumentException(Res.GetString(Res.Xml_XmlnsPrefix));
                        }
                        curDeclPrefix = String.Empty;
                        SetSpecialAttribute(SpecialAttribute.DefaultXmlns);
                        goto SkipPushAndWrite;
                    }
                    else if (namespaceName.Length > 0) {
                        prefix = LookupPrefix(namespaceName);
                        if (prefix == null || prefix.Length == 0) {
                            prefix = GeneratePrefix();
                        }
                    }
                }
                else {
                    if (prefix[0] == 'x') {
                        if (prefix == "xmlns") {
                            if (namespaceName.Length > 0 && namespaceName != XmlReservedNs.NsXmlNs) {
                                throw new ArgumentException(Res.GetString(Res.Xml_XmlnsPrefix));
                            }
                            curDeclPrefix = localName;
                            SetSpecialAttribute(SpecialAttribute.PrefixedXmlns);
                            goto SkipPushAndWrite;
                        }
                        else if (prefix == "xml") {
                            if (namespaceName.Length > 0 && namespaceName != XmlReservedNs.NsXml) {
                                throw new ArgumentException(Res.GetString(Res.Xml_XmlPrefix));
                            }
                            switch (localName) {
                                case "space":
                                    SetSpecialAttribute(SpecialAttribute.XmlSpace);
                                    goto SkipPushAndWrite;
                                case "lang":
                                    SetSpecialAttribute(SpecialAttribute.XmlLang);
                                    goto SkipPushAndWrite;
                            }
                        }
                    }

                    CheckNCName(prefix);

                    if (namespaceName.Length == 0) {
                        // attributes cannot have default namespace
                        prefix = string.Empty;
                    }
                    else {
                        string definedNs = LookupLocalNamespace(prefix);
                        if (definedNs != null && definedNs != namespaceName) {
                            prefix = GeneratePrefix();
                        }
                    }
                }

                if (prefix.Length != 0) {
                    PushNamespaceImplicit(prefix, namespaceName);
                }

            SkipPushAndWrite:

                // add attribute to the list and check for duplicates
                AddAttribute(prefix, localName, namespaceName);

                if (specAttr == SpecialAttribute.No) {
                    // write attribute name
                    return TryReturnTask( writer.WriteStartAttributeAsync(prefix, localName, namespaceName) );
                }
                return AsyncHelper.DoneTask;
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private async Task WriteStartAttributeAsync_NoAdvanceState(Task task, string prefix, string localName, string namespaceName) {
            try {
                await task.ConfigureAwait(false);
                await WriteStartAttributeAsync_NoAdvanceState(prefix, localName, namespaceName).ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }


        protected internal override Task WriteEndAttributeAsync() {
            try {
                Task task = AdvanceStateAsync(Token.EndAttribute);
                return SequenceRun(task, WriteEndAttributeAsync_NoAdvance);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private Task WriteEndAttributeAsync_NoAdvance() {
            try {

                if (specAttr != SpecialAttribute.No) {
                    return WriteEndAttributeAsync_SepcialAtt();
                }
                else {
                    return TryReturnTask( writer.WriteEndAttributeAsync() );
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private async Task WriteEndAttributeAsync_SepcialAtt() {
            try {

                string value;

                switch (specAttr) {
                    case SpecialAttribute.DefaultXmlns:
                        value = attrValueCache.StringValue;
                        if (PushNamespaceExplicit(string.Empty, value)) { // returns true if the namespace declaration should be written out
                            if (rawWriter != null) {
                                if (rawWriter.SupportsNamespaceDeclarationInChunks) {
                                    await rawWriter.WriteStartNamespaceDeclarationAsync(string.Empty).ConfigureAwait(false);
                                    await attrValueCache.ReplayAsync(rawWriter).ConfigureAwait(false);
                                    await rawWriter.WriteEndNamespaceDeclarationAsync().ConfigureAwait(false);
                                }
                                else {
                                    await rawWriter.WriteNamespaceDeclarationAsync(string.Empty, value).ConfigureAwait(false);
                                }
                            }
                            else {
                                await writer.WriteStartAttributeAsync(string.Empty, "xmlns", XmlReservedNs.NsXmlNs).ConfigureAwait(false);
                                await attrValueCache.ReplayAsync(writer).ConfigureAwait(false);
                                await writer.WriteEndAttributeAsync().ConfigureAwait(false);
                            }
                        }
                        curDeclPrefix = null;
                        break;
                    case SpecialAttribute.PrefixedXmlns:
                        value = attrValueCache.StringValue;
                        if (value.Length == 0) {
                            throw new ArgumentException(Res.GetString(Res.Xml_PrefixForEmptyNs));
                        }
                        if (value == XmlReservedNs.NsXmlNs || (value == XmlReservedNs.NsXml && curDeclPrefix != "xml")) {
                            throw new ArgumentException(Res.GetString(Res.Xml_CanNotBindToReservedNamespace));
                        }
                        if (PushNamespaceExplicit(curDeclPrefix, value)) { // returns true if the namespace declaration should be written out
                            if (rawWriter != null) {
                                if (rawWriter.SupportsNamespaceDeclarationInChunks) {
                                    await rawWriter.WriteStartNamespaceDeclarationAsync(curDeclPrefix).ConfigureAwait(false);
                                    await attrValueCache.ReplayAsync(rawWriter).ConfigureAwait(false);
                                    await rawWriter.WriteEndNamespaceDeclarationAsync().ConfigureAwait(false);
                                }
                                else {
                                    await rawWriter.WriteNamespaceDeclarationAsync(curDeclPrefix, value).ConfigureAwait(false);
                                }
                            }
                            else {
                                await writer.WriteStartAttributeAsync("xmlns", curDeclPrefix, XmlReservedNs.NsXmlNs).ConfigureAwait(false);
                                await attrValueCache.ReplayAsync(writer).ConfigureAwait(false);
                                await writer.WriteEndAttributeAsync().ConfigureAwait(false);
                            }
                        }
                        curDeclPrefix = null;
                        break;
                    case SpecialAttribute.XmlSpace:
                        attrValueCache.Trim();
                        value = attrValueCache.StringValue;

                        if (value == "default") {
                            elemScopeStack[elemTop].xmlSpace = XmlSpace.Default;
                        }
                        else if (value == "preserve") {
                            elemScopeStack[elemTop].xmlSpace = XmlSpace.Preserve;
                        }
                        else {
                            throw new ArgumentException(Res.GetString(Res.Xml_InvalidXmlSpace, value));
                        }
                        await writer.WriteStartAttributeAsync("xml", "space", XmlReservedNs.NsXml).ConfigureAwait(false);
                        await attrValueCache.ReplayAsync(writer).ConfigureAwait(false);
                        await writer.WriteEndAttributeAsync().ConfigureAwait(false);
                        break;
                    case SpecialAttribute.XmlLang:
                        value = attrValueCache.StringValue;
                        elemScopeStack[elemTop].xmlLang = value;
                        await writer.WriteStartAttributeAsync("xml", "lang", XmlReservedNs.NsXml).ConfigureAwait(false);
                        await attrValueCache.ReplayAsync(writer).ConfigureAwait(false);
                        await writer.WriteEndAttributeAsync().ConfigureAwait(false);
                        break;
                }
                specAttr = SpecialAttribute.No;
                attrValueCache.Clear();
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteCDataAsync(string text) {
            try {
                if (text == null) {
                    text = string.Empty;
                }
                await AdvanceStateAsync(Token.CData).ConfigureAwait(false);
                await writer.WriteCDataAsync(text).ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteCommentAsync(string text) {
            try {
                if (text == null) {
                    text = string.Empty;
                }
                await AdvanceStateAsync(Token.Comment).ConfigureAwait(false);
                await writer.WriteCommentAsync(text).ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteProcessingInstructionAsync(string name, string text) {
            try {
                // check name
                if (name == null || name.Length == 0) {
                    throw new ArgumentException(Res.GetString(Res.Xml_EmptyName));
                }
                CheckNCName(name);

                // check text
                if (text == null) {
                    text = string.Empty;
                }

                // xml declaration is a special case (not a processing instruction, but we allow WriteProcessingInstruction as a convenience)
                if (name.Length == 3 && string.Compare(name, "xml", StringComparison.OrdinalIgnoreCase) == 0) {
                    if (currentState != State.Start) {
                        throw new ArgumentException(Res.GetString(conformanceLevel == ConformanceLevel.Document ? Res.Xml_DupXmlDecl : Res.Xml_CannotWriteXmlDecl));
                    }

                    xmlDeclFollows = true;
                    await AdvanceStateAsync(Token.PI).ConfigureAwait(false);

                    if (rawWriter != null) {
                        // Translate PI into an xml declaration
                        await rawWriter.WriteXmlDeclarationAsync(text).ConfigureAwait(false);
                    }
                    else {
                        await writer.WriteProcessingInstructionAsync(name, text).ConfigureAwait(false);
                    }
                }
                else {
                    await AdvanceStateAsync(Token.PI).ConfigureAwait(false);
                    await writer.WriteProcessingInstructionAsync(name, text).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteEntityRefAsync(string name) {
            try {
                // check name
                if (name == null || name.Length == 0) {
                    throw new ArgumentException(Res.GetString(Res.Xml_EmptyName));
                }
                CheckNCName(name);

                await AdvanceStateAsync(Token.Text).ConfigureAwait(false);
                if (SaveAttrValue) {
                    attrValueCache.WriteEntityRef(name);
                }
                else {
                    await writer.WriteEntityRefAsync(name).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteCharEntityAsync(char ch) {
            try {
                if (Char.IsSurrogate(ch)) {
                    throw new ArgumentException(Res.GetString(Res.Xml_InvalidSurrogateMissingLowChar));
                }

                await AdvanceStateAsync(Token.Text).ConfigureAwait(false);
                if (SaveAttrValue) {
                    attrValueCache.WriteCharEntity(ch);
                }
                else {
                    await writer.WriteCharEntityAsync(ch).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteSurrogateCharEntityAsync(char lowChar, char highChar) {
            try {
                if (!Char.IsSurrogatePair(highChar, lowChar)) {
                    throw XmlConvert.CreateInvalidSurrogatePairException(lowChar, highChar);
                }

                await AdvanceStateAsync(Token.Text).ConfigureAwait(false);
                if (SaveAttrValue) {
                    attrValueCache.WriteSurrogateCharEntity(lowChar, highChar);
                }
                else {
                    await writer.WriteSurrogateCharEntityAsync(lowChar, highChar).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteWhitespaceAsync(string ws) {
            try {
                if (ws == null) {
                    ws = string.Empty;
                }
                if (!XmlCharType.Instance.IsOnlyWhitespace(ws)) {
                    throw new ArgumentException(Res.GetString(Res.Xml_NonWhitespace));
                }

                await AdvanceStateAsync(Token.Whitespace).ConfigureAwait(false);
                if (SaveAttrValue) {
                    attrValueCache.WriteWhitespace(ws);
                }
                else {
                    await writer.WriteWhitespaceAsync(ws).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override Task WriteStringAsync(string text) {
            try {
                if (text == null) {
                    return AsyncHelper.DoneTask;
                }

                Task task = AdvanceStateAsync(Token.Text);

                if (task.IsSuccess()) {
                    return WriteStringAsync_NoAdvanceState(text);
                }
                else {
                    return WriteStringAsync_NoAdvanceState(task, text);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private Task WriteStringAsync_NoAdvanceState(string text) {
            try {
              
                if (SaveAttrValue) {
                    attrValueCache.WriteString(text);
                    return AsyncHelper.DoneTask;
                }
                else {
                    return TryReturnTask( writer.WriteStringAsync(text) );
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private async Task WriteStringAsync_NoAdvanceState(Task task, string text) {
            try {
                await task.ConfigureAwait(false);
                await WriteStringAsync_NoAdvanceState(text).ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteCharsAsync(char[] buffer, int index, int count) {
            try {
                if (buffer == null) {
                    throw new ArgumentNullException("buffer");
                }
                if (index < 0) {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (count < 0) {
                    throw new ArgumentOutOfRangeException("count");
                }
                if (count > buffer.Length - index) {
                    throw new ArgumentOutOfRangeException("count");
                }

                await AdvanceStateAsync(Token.Text).ConfigureAwait(false);
                if (SaveAttrValue) {
                    attrValueCache.WriteChars(buffer, index, count);
                }
                else {
                    await writer.WriteCharsAsync(buffer, index, count).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteRawAsync(char[] buffer, int index, int count) {
            try {
                if (buffer == null) {
                    throw new ArgumentNullException("buffer");
                }
                if (index < 0) {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (count < 0) {
                    throw new ArgumentOutOfRangeException("count");
                }
                if (count > buffer.Length - index) {
                    throw new ArgumentOutOfRangeException("count");
                }

                await AdvanceStateAsync(Token.RawData).ConfigureAwait(false);
                if (SaveAttrValue) {
                    attrValueCache.WriteRaw(buffer, index, count);
                }
                else {
                    await writer.WriteRawAsync(buffer, index, count).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteRawAsync(string data) {
            try {
                if (data == null) {
                    return;
                }

                await AdvanceStateAsync(Token.RawData).ConfigureAwait(false);
                if (SaveAttrValue) {
                    attrValueCache.WriteRaw(data);
                }
                else {
                    await writer.WriteRawAsync(data).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override Task WriteBase64Async(byte[] buffer, int index, int count) {
            try {
                if (buffer == null) {
                    throw new ArgumentNullException("buffer");
                }
                if (index < 0) {
                    throw new ArgumentOutOfRangeException("index");
                }
                if (count < 0) {
                    throw new ArgumentOutOfRangeException("count");
                }
                if (count > buffer.Length - index) {
                    throw new ArgumentOutOfRangeException("count");
                }

                Task task = AdvanceStateAsync(Token.Base64);

                if (task.IsSuccess()) {
                    return TryReturnTask(writer.WriteBase64Async(buffer, index, count));
                }
                else {
                    return WriteBase64Async_NoAdvanceState(task, buffer, index, count);
                }
                
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private async Task WriteBase64Async_NoAdvanceState(Task task, byte[] buffer, int index, int count) {
            try {

                await task.ConfigureAwait(false);
                await writer.WriteBase64Async(buffer, index, count).ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task FlushAsync() {
            try {
                await writer.FlushAsync().ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteQualifiedNameAsync(string localName, string ns) {
            try {
                if (localName == null || localName.Length == 0) {
                    throw new ArgumentException(Res.GetString(Res.Xml_EmptyLocalName));
                }
                CheckNCName(localName);

                await AdvanceStateAsync(Token.Text).ConfigureAwait(false);
                string prefix = String.Empty;
                if (ns != null && ns.Length != 0) {
                    prefix = LookupPrefix(ns);
                    if (prefix == null) {
                        if (currentState != State.Attribute) {
                            throw new ArgumentException(Res.GetString(Res.Xml_UndefNamespace, ns));
                        }
                        prefix = GeneratePrefix();
                        PushNamespaceImplicit(prefix, ns);
                    }
                }
                // if this is a special attribute, then just convert this to text
                // otherwise delegate to raw-writer
                if (SaveAttrValue || rawWriter == null) {
                    if (prefix.Length != 0) {
                        await WriteStringAsync(prefix).ConfigureAwait(false);
                        await WriteStringAsync(":").ConfigureAwait(false);
                    }
                    await WriteStringAsync(localName).ConfigureAwait(false);
                }
                else {
                    await rawWriter.WriteQualifiedNameAsync(prefix, localName, ns).ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        public override async Task WriteBinHexAsync(byte[] buffer, int index, int count) {
            if (IsClosedOrErrorState) {
                throw new InvalidOperationException(Res.GetString(Res.Xml_ClosedOrError));
            }
            try {
                await AdvanceStateAsync(Token.Text).ConfigureAwait(false);
                await base.WriteBinHexAsync(buffer, index, count).ConfigureAwait(false);
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        private async Task WriteStartDocumentImplAsync(XmlStandalone standalone) {
            try {
                await AdvanceStateAsync(Token.StartDocument).ConfigureAwait(false);

                if (conformanceLevel == ConformanceLevel.Auto) {
                    conformanceLevel = ConformanceLevel.Document;
                    stateTable = StateTableDocument;
                }
                else if (conformanceLevel == ConformanceLevel.Fragment) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_CannotStartDocumentOnFragment));
                }

                if (rawWriter != null) {
                    if (!xmlDeclFollows) {
                        await rawWriter.WriteXmlDeclarationAsync(standalone).ConfigureAwait(false);
                    }
                }
                else {
                    // We do not pass the standalone value here - Dev10 Bug #479769
                    await writer.WriteStartDocumentAsync().ConfigureAwait(false);
                }
            }
            catch {
                currentState = State.Error;
                throw;
            }
        }

        //call taskFun and change state in sequence
        private Task AdvanceStateAsync_ReturnWhenFinish(Task task, State newState) {
            if (task.IsSuccess()) {
                currentState = newState;
                return AsyncHelper.DoneTask;
            }
            else {
                return _AdvanceStateAsync_ReturnWhenFinish(task, newState);
            }
        }

        private async Task _AdvanceStateAsync_ReturnWhenFinish(Task task, State newState) {
            await task.ConfigureAwait(false);
            currentState = newState;
        }

        private Task AdvanceStateAsync_ContinueWhenFinish(Task task, State newState, Token token) {
            if (task.IsSuccess()) {
                currentState = newState;
                return AdvanceStateAsync(token);
            }
            else {
                return _AdvanceStateAsync_ContinueWhenFinish(task, newState, token);
            }
        }

        private async Task _AdvanceStateAsync_ContinueWhenFinish(Task task, State newState, Token token) {
            await task.ConfigureAwait(false);
            currentState = newState;
            await AdvanceStateAsync(token).ConfigureAwait(false);
        }

        // Advance the state machine
        private Task AdvanceStateAsync(Token token) {
            if ((int)currentState >= (int)State.Closed) {
                if (currentState == State.Closed || currentState == State.Error) {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_ClosedOrError));
                }
                else {
                    throw new InvalidOperationException(Res.GetString(Res.Xml_WrongToken, tokenName[(int)token], GetStateName(currentState)));
                }
            }
       Advance:
            State newState = stateTable[((int)token << 4) + (int)currentState];
            //                         [ (int)token * 16 + (int)currentState ];

            Task task;
            if ((int)newState >= (int)State.Error) {
                switch (newState) {
                    case State.Error:
                        ThrowInvalidStateTransition(token, currentState);
                        break;

                    case State.StartContent:
                        return AdvanceStateAsync_ReturnWhenFinish(StartElementContentAsync(), State.Content);

                    case State.StartContentEle:
                        return AdvanceStateAsync_ReturnWhenFinish(StartElementContentAsync(), State.Element);

                    case State.StartContentB64:
                        return AdvanceStateAsync_ReturnWhenFinish(StartElementContentAsync(), State.B64Content);

                    case State.StartDoc:
                        return AdvanceStateAsync_ReturnWhenFinish(WriteStartDocumentAsync(), State.Document);

                    case State.StartDocEle:
                        return AdvanceStateAsync_ReturnWhenFinish(WriteStartDocumentAsync(), State.Element);

                    case State.EndAttrSEle:
                        task = SequenceRun(WriteEndAttributeAsync(), StartElementContentAsync);
                        return AdvanceStateAsync_ReturnWhenFinish(task, State.Element);

                    case State.EndAttrEEle:
                        task = SequenceRun(WriteEndAttributeAsync(), StartElementContentAsync);
                        return AdvanceStateAsync_ReturnWhenFinish(task, State.Content);
                    case State.EndAttrSCont:
                        task = SequenceRun(WriteEndAttributeAsync(), StartElementContentAsync);
                        return AdvanceStateAsync_ReturnWhenFinish(task, State.Content);

                    case State.EndAttrSAttr:
                        return AdvanceStateAsync_ReturnWhenFinish(WriteEndAttributeAsync(), State.Attribute);

                    case State.PostB64Cont:
                        if (rawWriter != null) {
                            return AdvanceStateAsync_ContinueWhenFinish(rawWriter.WriteEndBase64Async(), State.Content, token);
                        }
                        currentState = State.Content;
                        goto Advance;

                    case State.PostB64Attr:
                        if (rawWriter != null) {
                            return AdvanceStateAsync_ContinueWhenFinish(rawWriter.WriteEndBase64Async(), State.Attribute, token);
                        }
                        currentState = State.Attribute;
                        goto Advance;

                    case State.PostB64RootAttr:
                        if (rawWriter != null) {
                            return AdvanceStateAsync_ContinueWhenFinish(rawWriter.WriteEndBase64Async(), State.RootLevelAttr, token);
                        }
                        currentState = State.RootLevelAttr;
                        goto Advance;

                    case State.StartFragEle:
                        StartFragment();
                        newState = State.Element;
                        break;

                    case State.StartFragCont:
                        StartFragment();
                        newState = State.Content;
                        break;

                    case State.StartFragB64:
                        StartFragment();
                        newState = State.B64Content;
                        break;

                    case State.StartRootLevelAttr:
                        return AdvanceStateAsync_ReturnWhenFinish(WriteEndAttributeAsync(), State.RootLevelAttr);


                    default:
                        Debug.Assert(false, "We should not get to this point.");
                        break;
                }
            }

            currentState = newState;
            return AsyncHelper.DoneTask;
        }

        // write namespace declarations
        private async Task StartElementContentAsync_WithNS() { 
            int start = elemScopeStack[elemTop].prevNSTop;
            for (int i = nsTop; i > start; i--) {
                if (nsStack[i].kind == NamespaceKind.NeedToWrite) {
                    await nsStack[i].WriteDeclAsync(writer, rawWriter).ConfigureAwait(false);
                }
            }
            if (rawWriter != null) {
                rawWriter.StartElementContent();
            }
        }

        private Task StartElementContentAsync() {
            if (nsTop > elemScopeStack[elemTop].prevNSTop) {
                return StartElementContentAsync_WithNS();
            }

            if (rawWriter != null) {
                rawWriter.StartElementContent();
            }
            return AsyncHelper.DoneTask;
        }
    }
}

