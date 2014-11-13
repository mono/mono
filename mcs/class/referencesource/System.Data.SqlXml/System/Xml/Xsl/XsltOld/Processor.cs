//------------------------------------------------------------------------------
// <copyright file="Processor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System.Globalization;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.XPath;
    using MS.Internal.Xml.XPath;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Xsl.XsltOld.Debugger;
    using System.Reflection;
    using System.Security;

    internal sealed class Processor : IXsltProcessor {
        //
        // Static constants
        //

        const int StackIncrement = 10;

        //
        // Execution result
        //

        internal enum ExecResult {
            Continue,           // Continues next iteration immediately
            Interrupt,          // Returns to caller, was processed enough
            Done                // Execution finished
        }

        internal enum OutputResult {
            Continue,
            Interrupt,
            Overflow,
            Error,
            Ignore
        }

        private ExecResult     execResult;

        //
        // Compiled stylesheet
        //

        private Stylesheet      stylesheet;     // Root of import tree of template managers
        private RootAction      rootAction;
        private Key[]           keyList;
        private List<TheQuery>  queryStore;
        public  PermissionSet   permissions;   // used by XsltCompiledContext in document and extension functions

        //
        // Document Being transformed
        //

        private XPathNavigator    document;

        //
        // Execution action stack
        //

        private HWStack         actionStack;
        private HWStack         debuggerStack;

        //
        // Register for returning value from calling nested action
        //

        private StringBuilder   sharedStringBuilder;

        //
        // Output related member variables
        //
        int                     ignoreLevel;
        StateMachine            xsm;
        RecordBuilder           builder;

        XsltOutput              output;

        XmlNameTable            nameTable      = new NameTable();

        XmlResolver             resolver;

#pragma warning disable 618
        XsltArgumentList        args;
#pragma warning restore 618
        Hashtable               scriptExtensions;

        ArrayList               numberList;
        //
        // Template lookup action
        //

        TemplateLookupAction    templateLookup = new TemplateLookupAction();

        private IXsltDebugger   debugger;
        Query[]                 queryList;

        private ArrayList sortArray;

        private Hashtable documentCache;

        // NOTE: ValueOf() can call Matches() through XsltCompileContext.PreserveWhitespace(),
        // that's why we use two different contexts here, valueOfContext and matchesContext
        private XsltCompileContext  valueOfContext;
        private XsltCompileContext  matchesContext;

        internal XPathNavigator Current {
            get {
                ActionFrame frame = (ActionFrame) this.actionStack.Peek();
                return frame != null ? frame.Node : null;
            }
        }

        internal ExecResult ExecutionResult {
            get { return this.execResult; }

            set {
                Debug.Assert(this.execResult == ExecResult.Continue);
                this.execResult = value;
            }
        }

        internal Stylesheet Stylesheet {
            get { return this.stylesheet; }
        }

        internal XmlResolver Resolver {
            get {
                Debug.Assert(this.resolver != null, "Constructor should create it if null passed");
                return this.resolver;
            }
        }

        internal ArrayList SortArray {
            get {
                Debug.Assert(this.sortArray != null, "InitSortArray() wasn't called");
                return this.sortArray;
            }
        }

        internal Key[] KeyList {
            get { return this.keyList; }
        }

        internal XPathNavigator GetNavigator(Uri ruri) {
            XPathNavigator result = null;
            if (documentCache != null) {
                result = documentCache[ruri] as XPathNavigator;
                if (result != null) {
                    return result.Clone();
                }
            }
            else {
                documentCache = new Hashtable();
            }

            Object input = resolver.GetEntity(ruri, null, null);
            if (input is Stream) {
                XmlTextReaderImpl tr  = new XmlTextReaderImpl(ruri.ToString(), (Stream) input); {
                    tr.XmlResolver = this.resolver;
                }
                // reader is closed by Compiler.LoadDocument()
                result = ((IXPathNavigable)Compiler.LoadDocument(tr)).CreateNavigator();
            }
            else if (input is XPathNavigator){
                result = (XPathNavigator) input;
            }
            else {
                throw XsltException.Create(Res.Xslt_CantResolve, ruri.ToString());
            }
            documentCache[ruri] = result.Clone();
            return result;
        }

        internal void AddSort(Sort sortinfo) {
            Debug.Assert(this.sortArray != null, "InitSortArray() wasn't called");
            this.sortArray.Add(sortinfo);
        }

        [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
        internal void InitSortArray() {
            if (this.sortArray == null) {
                this.sortArray = new ArrayList();
            }
            else {
                this.sortArray.Clear();
            }
        }

        internal object GetGlobalParameter(XmlQualifiedName qname) {
            object parameter = args.GetParam(qname.Name, qname.Namespace);
            if (parameter == null) {
                return null;
            }
            // 
            if (
                parameter is XPathNodeIterator ||
                parameter is XPathNavigator ||
                parameter is Boolean ||
                parameter is Double ||
                parameter is String
            ) {
                // doing nothing
            } else if (
                parameter is Int16 || parameter is UInt16 ||
                parameter is Int32 || parameter is UInt32 ||
                parameter is Int64 || parameter is UInt64 ||
                parameter is Single || parameter is Decimal
            ) {
                parameter = XmlConvert.ToXPathDouble(parameter);
            } else {
                parameter = parameter.ToString();
            }
            return parameter;
        }

        internal object GetExtensionObject(string nsUri) {
            return args.GetExtensionObject(nsUri);
        }

        internal object GetScriptObject(string nsUri) {
            return scriptExtensions[nsUri];
        }

        internal RootAction RootAction {
            get { return this.rootAction; }
        }

        internal XPathNavigator Document {
            get { return this.document; }
        }

#if DEBUG
        private bool stringBuilderLocked = false;
#endif

        internal StringBuilder GetSharedStringBuilder() {
#if DEBUG
            Debug.Assert(! stringBuilderLocked);
#endif
            if (sharedStringBuilder == null) {
                sharedStringBuilder = new StringBuilder();
            }
            else {
                sharedStringBuilder.Length = 0;
            }
#if DEBUG
            stringBuilderLocked = true;
#endif
            return sharedStringBuilder;
        }

        internal void ReleaseSharedStringBuilder() {
            // don't clean stringBuilderLocked here. ToString() will happen after this call
#if DEBUG
            stringBuilderLocked = false;
#endif
        }

        internal ArrayList NumberList {
            get {
                if (this.numberList == null) {
                    this.numberList = new ArrayList();
                }
                return this.numberList;
            }
        }

        internal IXsltDebugger Debugger {
            get { return this.debugger; }
        }

        internal HWStack ActionStack {
            get { return this.actionStack; }
        }

        internal RecordBuilder Builder {
            get { return this.builder; }
        }

        internal XsltOutput Output {
            get { return this.output; }
        }

        //
        // Construction
        //
        public Processor(
            XPathNavigator doc, XsltArgumentList args, XmlResolver resolver,
            Stylesheet stylesheet, List<TheQuery> queryStore, RootAction rootAction,
            IXsltDebugger debugger
        ) {
            this.stylesheet = stylesheet;
            this.queryStore = queryStore;
            this.rootAction = rootAction;
            this.queryList  = new Query[queryStore.Count]; {
                for(int i = 0; i < queryStore.Count; i ++) {
                    queryList[i] = Query.Clone(queryStore[i].CompiledQuery.QueryTree);
                }
            }

            this.xsm                 = new StateMachine();
            this.document            = doc;
            this.builder             = null;
            this.actionStack         = new HWStack(StackIncrement);
            this.output              = this.rootAction.Output;
            this.permissions         = this.rootAction.permissions;
            this.resolver            = resolver ?? XmlNullResolver.Singleton;
            this.args                = args     ?? new XsltArgumentList();
            this.debugger            = debugger;
            if (this.debugger != null) {
                this.debuggerStack = new HWStack(StackIncrement, /*limit:*/1000);
                templateLookup     = new TemplateLookupActionDbg();
            }

            // Clone the compile-time KeyList
            if (this.rootAction.KeyList != null) {
                this.keyList = new Key[this.rootAction.KeyList.Count];
                for (int i = 0; i < this.keyList.Length; i ++) {
                    this.keyList[i] = this.rootAction.KeyList[i].Clone();
                }
            }

            this.scriptExtensions = new Hashtable(this.stylesheet.ScriptObjectTypes.Count); {
                foreach(DictionaryEntry entry in this.stylesheet.ScriptObjectTypes) {
                    string namespaceUri = (string)entry.Key;
                    if (GetExtensionObject(namespaceUri) != null) {
                        throw XsltException.Create(Res.Xslt_ScriptDub, namespaceUri);
                    }
                    scriptExtensions.Add(namespaceUri, Activator.CreateInstance((Type)entry.Value,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null));
                }
            }

            this.PushActionFrame(this.rootAction, /*nodeSet:*/null);
        }

        [System.Runtime.TargetedPatchingOptOutAttribute("Performance critical to inline across NGen image boundaries")]
        public ReaderOutput StartReader() {
            ReaderOutput output = new ReaderOutput(this);
            this.builder = new RecordBuilder(output, this.nameTable);
            return output;
        }

        public void Execute(Stream stream) {
            RecordOutput recOutput = null;

            switch (this.output.Method) {
            case XsltOutput.OutputMethod.Text:
                recOutput = new TextOnlyOutput(this, stream);
                break;
            case XsltOutput.OutputMethod.Xml:
            case XsltOutput.OutputMethod.Html:
            case XsltOutput.OutputMethod.Other:
            case XsltOutput.OutputMethod.Unknown:
                recOutput = new TextOutput(this, stream);
                break;
            }
            this.builder = new RecordBuilder(recOutput, this.nameTable);
            Execute();
        }

        public void Execute(TextWriter writer) {
            RecordOutput recOutput = null;

            switch (this.output.Method) {
            case XsltOutput.OutputMethod.Text:
                recOutput = new TextOnlyOutput(this, writer);
                break;
            case XsltOutput.OutputMethod.Xml:
            case XsltOutput.OutputMethod.Html:
            case XsltOutput.OutputMethod.Other:
            case XsltOutput.OutputMethod.Unknown:
                recOutput = new TextOutput(this, writer);
                break;
            }
            this.builder = new RecordBuilder(recOutput, this.nameTable);
            Execute();
        }

        public void Execute(XmlWriter writer) {
            this.builder = new RecordBuilder(new WriterOutput(this, writer), this.nameTable);
            Execute();
        }

        //
        //  Execution part of processor
        //
        internal void Execute() {
            Debug.Assert(this.actionStack != null);

            while (this.execResult == ExecResult.Continue) {
                ActionFrame frame = (ActionFrame) this.actionStack.Peek();

                if (frame == null) {
                    Debug.Assert(this.builder != null);
                    this.builder.TheEnd();
                    ExecutionResult = ExecResult.Done;
                    break;
                }

                // Execute the action which was on the top of the stack
                if (frame.Execute(this)) {
                    this.actionStack.Pop();
                }
            }

            if (this.execResult == ExecResult.Interrupt) {
                this.execResult = ExecResult.Continue;
            }
        }

        //
        // Action frame support
        //

        internal ActionFrame PushNewFrame() {
            ActionFrame prent = (ActionFrame) this.actionStack.Peek();
            ActionFrame frame = (ActionFrame) this.actionStack.Push();
            if (frame == null) {
                frame = new ActionFrame();
                this.actionStack.AddToTop(frame);
            }
            Debug.Assert(frame != null);

            if (prent != null) {
                frame.Inherit(prent);
            }

            return frame;
        }

        internal void PushActionFrame(Action action, XPathNodeIterator nodeSet) {
            ActionFrame frame = PushNewFrame();
            frame.Init(action, nodeSet);
        }

        internal void PushActionFrame(ActionFrame container) {
            this.PushActionFrame(container, container.NodeSet);
        }

        internal void PushActionFrame(ActionFrame container, XPathNodeIterator nodeSet) {
            ActionFrame frame = PushNewFrame();
            frame.Init(container, nodeSet);
        }

        internal void PushTemplateLookup(XPathNodeIterator nodeSet, XmlQualifiedName mode, Stylesheet importsOf) {
            Debug.Assert(this.templateLookup != null);
            this.templateLookup.Initialize(mode, importsOf);
            PushActionFrame(this.templateLookup, nodeSet);
        }

        internal string GetQueryExpression(int key) {
            Debug.Assert(key != Compiler.InvalidQueryKey);
            return this.queryStore[key].CompiledQuery.Expression;
        }

        internal Query GetCompiledQuery(int key) {
            Debug.Assert(key != Compiler.InvalidQueryKey);
            TheQuery theQuery = this.queryStore[key];
            theQuery.CompiledQuery.CheckErrors();
            Query expr = Query.Clone(this.queryList[key]);
            expr.SetXsltContext(new XsltCompileContext(theQuery._ScopeManager, this));
            return expr;
        }

        internal Query GetValueQuery(int key) {
            return GetValueQuery(key, null);
        }

        internal Query GetValueQuery(int key, XsltCompileContext context) {
            Debug.Assert(key != Compiler.InvalidQueryKey);
            TheQuery theQuery = this.queryStore[key];
            theQuery.CompiledQuery.CheckErrors();
            Query expr = this.queryList[key];

            if (context == null) {
                context = new XsltCompileContext(theQuery._ScopeManager, this);
            } else {
                context.Reinitialize(theQuery._ScopeManager, this);
            }

            expr.SetXsltContext(context);
            return expr;
        }

        private XsltCompileContext GetValueOfContext() {
            if (this.valueOfContext == null) {
                this.valueOfContext = new XsltCompileContext();
            }
            return this.valueOfContext;
        }

        [Conditional("DEBUG")]
        private void RecycleValueOfContext() {
            if (this.valueOfContext != null) {
                this.valueOfContext.Recycle();
            }
        }

        private XsltCompileContext GetMatchesContext() {
            if (this.matchesContext == null) {
                this.matchesContext = new XsltCompileContext();
            }
            return this.matchesContext;
        }

        [Conditional("DEBUG")]
        private void RecycleMatchesContext() {
            if (this.matchesContext != null) {
                this.matchesContext.Recycle();
            }
        }

        internal String ValueOf(ActionFrame context, int key) {
            string result;

            Query query = this.GetValueQuery(key, GetValueOfContext());
            object value = query.Evaluate(context.NodeSet);
            if (value is XPathNodeIterator) {
                XPathNavigator n = query.Advance();
                result = n != null ? ValueOf(n) : string.Empty;
            } else {
                result = XmlConvert.ToXPathString(value);
            }

            RecycleValueOfContext();
            return result;
        }

        internal String ValueOf(XPathNavigator n) {
            if (this.stylesheet.Whitespace && n.NodeType == XPathNodeType.Element) {
                StringBuilder builder = this.GetSharedStringBuilder();
                ElementValueWithoutWS(n, builder);
                this.ReleaseSharedStringBuilder();
                return builder.ToString();
            }
            return n.Value;
        }

        private void ElementValueWithoutWS(XPathNavigator nav, StringBuilder builder) {
            Debug.Assert(nav.NodeType == XPathNodeType.Element);
            bool preserve = this.Stylesheet.PreserveWhiteSpace(this, nav);
            if (nav.MoveToFirstChild()) {
                do {
                    switch (nav.NodeType) {
                    case XPathNodeType.Text :
                    case XPathNodeType.SignificantWhitespace :
                        builder.Append(nav.Value);
                        break;
                    case XPathNodeType.Whitespace :
                        if (preserve) {
                            builder.Append(nav.Value);
                        }
                        break;
                    case XPathNodeType.Element :
                        ElementValueWithoutWS(nav, builder);
                        break;
                    }
                }while (nav.MoveToNext());
                nav.MoveToParent();
            }
        }

        internal XPathNodeIterator StartQuery(XPathNodeIterator context, int key) {
            Query query = GetCompiledQuery(key);
            object result = query.Evaluate(context);
            if (result is XPathNodeIterator) {
                // ToDo: We create XPathSelectionIterator to count positions, but it's better create special query in this case at compile time.
                return new XPathSelectionIterator(context.Current, query);
            }
            throw XsltException.Create(Res.XPath_NodeSetExpected);
        }

        internal object Evaluate(ActionFrame context, int key) {
            return GetValueQuery(key).Evaluate(context.NodeSet);
        }

        internal object RunQuery(ActionFrame context, int key) {
            Query query = GetCompiledQuery(key);
            object value = query.Evaluate(context.NodeSet);
            XPathNodeIterator it = value as XPathNodeIterator;
            if (it != null) {
                return new XPathArrayIterator(it);
            }

            return value;
        }

        internal string EvaluateString(ActionFrame context, int key) {
            object objValue = Evaluate(context, key);
            string value = null;
            if (objValue != null)
                value = XmlConvert.ToXPathString(objValue);
            if (value == null)
                value = string.Empty;
            return value;
        }

        internal bool EvaluateBoolean(ActionFrame context, int key) {
            object objValue = Evaluate(context, key);

            if (objValue != null) {
                XPathNavigator nav = objValue as XPathNavigator;
                return nav != null ? Convert.ToBoolean(nav.Value, CultureInfo.InvariantCulture) : Convert.ToBoolean(objValue, CultureInfo.InvariantCulture);
            }
            else {
                return false;
            }
        }

        internal bool Matches(XPathNavigator context, int key) {
            // We don't use XPathNavigator.Matches() to avoid cloning of Query on each call
            Query query = this.GetValueQuery(key, GetMatchesContext());

            try {
                bool result = query.MatchNode(context) != null;

                RecycleMatchesContext();
                return result;
            } catch(XPathException) {
                throw XsltException.Create(Res.Xslt_InvalidPattern, this.GetQueryExpression(key));
            }
        }

        //
        // Outputting part of processor
        //

        internal XmlNameTable NameTable {
            get { return this.nameTable; }
        }

        internal bool CanContinue {
            get { return this.execResult == ExecResult.Continue; }
        }

        internal bool ExecutionDone {
            get { return this.execResult == ExecResult.Done; }
        }

        internal void ResetOutput() {
            Debug.Assert(this.builder != null);
            this.builder.Reset();
        }
        internal bool BeginEvent(XPathNodeType nodeType, string prefix, string name, string nspace, bool empty) {
            return BeginEvent(nodeType, prefix, name,  nspace,  empty, null, true);
        }

        internal bool BeginEvent(XPathNodeType nodeType, string prefix, string name, string nspace, bool empty, Object htmlProps, bool search) {
            Debug.Assert(this.xsm != null);

            int stateOutlook = this.xsm.BeginOutlook(nodeType);

            if (this.ignoreLevel > 0 || stateOutlook == StateMachine.Error) {
                this.ignoreLevel ++;
                return true;                        // We consumed the event, so pretend it was output.
            }

            switch (this.builder.BeginEvent(stateOutlook, nodeType, prefix, name, nspace, empty, htmlProps, search)) {
            case OutputResult.Continue:
                this.xsm.Begin(nodeType);
                Debug.Assert(StateMachine.StateOnly(stateOutlook) == this.xsm.State);
                Debug.Assert(ExecutionResult == ExecResult.Continue);
                return true;
            case OutputResult.Interrupt:
                this.xsm.Begin(nodeType);
                Debug.Assert(StateMachine.StateOnly(stateOutlook) == this.xsm.State);
                ExecutionResult = ExecResult.Interrupt;
                return true;
            case OutputResult.Overflow:
                ExecutionResult = ExecResult.Interrupt;
                return false;
            case OutputResult.Error:
                this.ignoreLevel ++;
                return true;
            case OutputResult.Ignore:
                return true;
            default:
                Debug.Fail("Unexpected result of RecordBuilder.BeginEvent()");
                return true;
            }
        }

        internal bool TextEvent(string text) {
            return this.TextEvent(text, false);
        }

        internal bool TextEvent(string text, bool disableOutputEscaping) {
            Debug.Assert(this.xsm != null);

            if (this.ignoreLevel > 0) {
                return true;
            }

            int stateOutlook = this.xsm.BeginOutlook(XPathNodeType.Text);

            switch (this.builder.TextEvent(stateOutlook, text, disableOutputEscaping)) {
                case OutputResult.Continue:
                this.xsm.Begin(XPathNodeType.Text);
                Debug.Assert(StateMachine.StateOnly(stateOutlook) == this.xsm.State);
                Debug.Assert(ExecutionResult == ExecResult.Continue);
                return true;
            case OutputResult.Interrupt:
                this.xsm.Begin(XPathNodeType.Text);
                Debug.Assert(StateMachine.StateOnly(stateOutlook) == this.xsm.State);
                ExecutionResult = ExecResult.Interrupt;
                return true;
            case OutputResult.Overflow:
                ExecutionResult = ExecResult.Interrupt;
                return false;
            case OutputResult.Error:
            case OutputResult.Ignore:
                return true;
            default:
                Debug.Fail("Unexpected result of RecordBuilder.TextEvent()");
                return true;
            }
        }

        internal bool EndEvent(XPathNodeType nodeType) {
            Debug.Assert(this.xsm != null);

            if (this.ignoreLevel > 0) {
                this.ignoreLevel --;
                return true;
            }

            int stateOutlook = this.xsm.EndOutlook(nodeType);

            switch (this.builder.EndEvent(stateOutlook, nodeType)) {
                case OutputResult.Continue:
                this.xsm.End(nodeType);
                Debug.Assert(StateMachine.StateOnly(stateOutlook) == this.xsm.State);
                return true;
            case OutputResult.Interrupt:
                this.xsm.End(nodeType);
                Debug.Assert(StateMachine.StateOnly(stateOutlook) == this.xsm.State,
                             "StateMachine.StateOnly(stateOutlook) == this.xsm.State");
                ExecutionResult = ExecResult.Interrupt;
                return true;
            case OutputResult.Overflow:
                ExecutionResult = ExecResult.Interrupt;
                return false;
            case OutputResult.Error:
            case OutputResult.Ignore:
            default:
                Debug.Fail("Unexpected result of RecordBuilder.TextEvent()");
                return true;
            }
        }

        internal bool CopyBeginEvent(XPathNavigator node, bool emptyflag) {
            switch (node.NodeType) {
            case XPathNodeType.Element:
            case XPathNodeType.Attribute:
            case XPathNodeType.ProcessingInstruction:
            case XPathNodeType.Comment:
                return BeginEvent(node.NodeType, node.Prefix, node.LocalName, node.NamespaceURI, emptyflag);
            case XPathNodeType.Namespace:
                // value instead of namespace here!
                return BeginEvent(XPathNodeType.Namespace, null, node.LocalName, node.Value, false);
            case XPathNodeType.Text:
                // Text will be copied in CopyContents();
                break;

            case XPathNodeType.Root:
            case XPathNodeType.Whitespace:
            case XPathNodeType.SignificantWhitespace:
            case XPathNodeType.All:
                break;

            default:
                Debug.Fail("Invalid XPathNodeType in CopyBeginEvent");
                break;
            }

            return true;
        }

        internal bool CopyTextEvent(XPathNavigator node) {
            switch (node.NodeType) {
            case XPathNodeType.Element:
            case XPathNodeType.Namespace:
                break;

            case XPathNodeType.Attribute:
            case XPathNodeType.ProcessingInstruction:
            case XPathNodeType.Comment:
            case XPathNodeType.Text:
            case XPathNodeType.Whitespace:
            case XPathNodeType.SignificantWhitespace:
                string text = node.Value;
                return TextEvent(text);

            case XPathNodeType.Root:
            case XPathNodeType.All:
                break;

            default:
                Debug.Fail("Invalid XPathNodeType in CopyTextEvent");
                break;
            }

            return true;
        }

        internal bool CopyEndEvent(XPathNavigator node) {
            switch (node.NodeType) {
            case XPathNodeType.Element:
            case XPathNodeType.Attribute:
            case XPathNodeType.ProcessingInstruction:
            case XPathNodeType.Comment:
            case XPathNodeType.Namespace:
                return EndEvent(node.NodeType);

            case XPathNodeType.Text:
                // Text was copied in CopyContents();
                break;


            case XPathNodeType.Root:
            case XPathNodeType.Whitespace:
            case XPathNodeType.SignificantWhitespace:
            case XPathNodeType.All:
                break;

            default:
                Debug.Fail("Invalid XPathNodeType in CopyEndEvent");
                break;
            }

            return true;
        }

        internal static bool IsRoot(XPathNavigator navigator) {
            Debug.Assert(navigator != null);

            if (navigator.NodeType == XPathNodeType.Root) {
                return true;
            }
            else if (navigator.NodeType == XPathNodeType.Element) {
                XPathNavigator clone = navigator.Clone();
                clone.MoveToRoot();
                return clone.IsSamePosition(navigator);
            }
            else {
                return false;
            }
        }

        //
        // Builder stack
        //
        internal void PushOutput(RecordOutput output) {
            Debug.Assert(output != null);
            this.builder.OutputState = this.xsm.State;
            RecordBuilder lastBuilder = this.builder;
            this.builder      = new RecordBuilder(output, this.nameTable);
            this.builder.Next = lastBuilder;

            this.xsm.Reset();
        }

        internal RecordOutput PopOutput() {
            Debug.Assert(this.builder != null);

            RecordBuilder topBuilder = this.builder;
            this.builder              = topBuilder.Next;
            this.xsm.State            = this.builder.OutputState;

            topBuilder.TheEnd();

            return topBuilder.Output;
        }

        internal bool SetDefaultOutput(XsltOutput.OutputMethod method) {
            if(Output.Method != method) {
                this.output = this.output.CreateDerivedOutput(method);
                return true;
            }
            return false;
        }

        internal object GetVariableValue(VariableAction variable) {
            int variablekey = variable.VarKey;
            if (variable.IsGlobal) {
                ActionFrame rootFrame = (ActionFrame) this.actionStack[0];
                object result = rootFrame.GetVariable(variablekey);
                if (result == VariableAction.BeingComputedMark) {
                    throw XsltException.Create(Res.Xslt_CircularReference, variable.NameStr);
                }
                if (result != null) {
                    return result;
                }
                // Variable wasn't evaluated yet
                int saveStackSize = this.actionStack.Length;
                ActionFrame varFrame = PushNewFrame();
                varFrame.Inherit(rootFrame);
                varFrame.Init(variable, rootFrame.NodeSet);
                do {
                    bool endOfFrame = ((ActionFrame) this.actionStack.Peek()).Execute(this);
                    if (endOfFrame) {
                        this.actionStack.Pop();
                    }
                } while (saveStackSize < this.actionStack.Length);
                Debug.Assert(saveStackSize == this.actionStack.Length);
                result = rootFrame.GetVariable(variablekey);
                Debug.Assert(result != null, "Variable was just calculated and result can't be null");
                return result;
            } else {
                return ((ActionFrame) this.actionStack.Peek()).GetVariable(variablekey);
            }
        }

        internal void SetParameter(XmlQualifiedName name, object value) {
            Debug.Assert(1 < actionStack.Length);
            ActionFrame parentFrame = (ActionFrame) this.actionStack[actionStack.Length - 2];
            parentFrame.SetParameter(name, value);
        }

        internal void ResetParams() {
            ActionFrame frame = (ActionFrame) this.actionStack[actionStack.Length - 1];
            frame.ResetParams();
        }

        internal object GetParameter(XmlQualifiedName name) {
            Debug.Assert(2 < actionStack.Length);
            ActionFrame parentFrame = (ActionFrame) this.actionStack[actionStack.Length - 3];
            return parentFrame.GetParameter(name);
        }

        // ---------------------- Debugger stack -----------------------

        internal class DebuggerFrame {
            internal ActionFrame        actionFrame;
            internal XmlQualifiedName   currentMode;
        }

        internal void PushDebuggerStack() {
            Debug.Assert(this.Debugger != null, "We don't generate calls this function if ! debugger");
            DebuggerFrame dbgFrame = (DebuggerFrame) this.debuggerStack.Push();
            if (dbgFrame == null) {
                dbgFrame = new DebuggerFrame();
                this.debuggerStack.AddToTop(dbgFrame);
            }
            dbgFrame.actionFrame = (ActionFrame) this.actionStack.Peek(); // In a case of next builtIn action.
        }

        internal void PopDebuggerStack() {
            Debug.Assert(this.Debugger != null, "We don't generate calls this function if ! debugger");
            this.debuggerStack.Pop();
        }

        internal void OnInstructionExecute() {
            Debug.Assert(this.Debugger != null, "We don't generate calls this function if ! debugger");
            DebuggerFrame dbgFrame = (DebuggerFrame) this.debuggerStack.Peek();
            Debug.Assert(dbgFrame != null, "PushDebuggerStack() wasn't ever called");
            dbgFrame.actionFrame = (ActionFrame) this.actionStack.Peek();
            this.Debugger.OnInstructionExecute((IXsltProcessor) this);
        }

        internal XmlQualifiedName GetPrevioseMode() {
            Debug.Assert(this.Debugger != null, "We don't generate calls this function if ! debugger");
            Debug.Assert(2 <= this.debuggerStack.Length);
            return ((DebuggerFrame) this.debuggerStack[this.debuggerStack.Length - 2]).currentMode;
        }

        internal void SetCurrentMode(XmlQualifiedName mode) {
            Debug.Assert(this.Debugger != null, "We don't generate calls this function if ! debugger");
            ((DebuggerFrame) this.debuggerStack[this.debuggerStack.Length - 1]).currentMode = mode;
        }

        // ----------------------- IXsltProcessor : --------------------
        int IXsltProcessor.StackDepth {
            get {return this.debuggerStack.Length;}
        }

        IStackFrame IXsltProcessor.GetStackFrame(int depth) {
            return ((DebuggerFrame) this.debuggerStack[depth]).actionFrame;
        }
    }
}
