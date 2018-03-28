//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal enum QueryCompilerFlags
    {
        None = 0x00000000,
        InverseQuery = 0x00000001
    }

    internal struct FilterResult
    {
        QueryProcessor processor;
        bool result;

        internal FilterResult(QueryProcessor processor)
        {
            this.processor = processor;
            this.result = this.processor.Result;
        }

        internal FilterResult(bool result)
        {
            this.processor = null;
            this.result = result;
        }

#if NO
        internal ICollection<MessageFilter> Matches
        {
            get
            {
                return this.processor.ResultSet;
            }            
        }
#endif
        internal QueryProcessor Processor
        {
            get
            {
                return this.processor;
            }
        }

        internal bool Result
        {
            get
            {
                return this.result;
            }
        }

        internal MessageFilter GetSingleMatch()
        {
            Collection<MessageFilter> matches = processor.MatchList;
            MessageFilter match;
            switch (matches.Count)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(SR.GetString(SR.FilterMultipleMatches), null, matches));

                case 0:
                    match = null;
                    break;

                case 1:
                    match = matches[0];
                    break;
            }

            return match;
        }
    }

    // XPathResult.GetResultAsString and XPathResult.GetResultAsBoolean,
    // drive knowledge of TResult into the engine.
    internal class QueryResult<TResult> : IEnumerable<KeyValuePair<MessageQuery, TResult>>
    {
        bool evalBody;
        QueryMatcher matcher;
        Message message;

        internal QueryResult(QueryMatcher matcher, Message message, bool evalBody)
        {
            this.matcher = matcher;
            this.message = message;
            this.evalBody = evalBody;
        }

        public TResult GetSingleResult()
        {
            QueryProcessor processor = this.matcher.CreateProcessor();
            XPathResult result;

            try
            {
                processor.Eval(this.matcher.RootOpcode, this.message, this.evalBody);
            }
            catch (XPathNavigatorException e)
            {
                throw TraceUtility.ThrowHelperError(e.Process(this.matcher.RootOpcode), this.message);
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw TraceUtility.ThrowHelperError(e.Process(this.matcher.RootOpcode), this.message);
            }
            finally
            {
                if (this.evalBody)
                {
                    this.message.Close();
                }

                result = processor.QueryResult;
                this.matcher.ReleaseProcessor(processor);
            }

            if (typeof(TResult) == typeof(XPathResult) || typeof(TResult) == typeof(object))
            {
                return (TResult)(object)result;
            }
            else if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)result.GetResultAsString();
            }
            else if (typeof(TResult) == typeof(bool))
            {
                return (TResult)(object)result.GetResultAsBoolean();
            }
            else
            {
                throw Fx.AssertAndThrowFatal("unsupported type");
            }
        }

        public IEnumerator<KeyValuePair<MessageQuery, TResult>> GetEnumerator()
        {
            QueryProcessor processor = this.matcher.CreateProcessor();
            Collection<KeyValuePair<MessageQuery, XPathResult>> results =
                new Collection<KeyValuePair<MessageQuery, XPathResult>>();
            processor.ResultSet = results;

            try
            {
                processor.Eval(this.matcher.RootOpcode, this.message, this.evalBody);

                if (typeof(TResult) == typeof(XPathResult))
                {
                    return (IEnumerator<KeyValuePair<MessageQuery, TResult>>)(object)results.GetEnumerator();
                }
                else if (typeof(TResult) == typeof(string) ||
                    typeof(TResult) == typeof(bool) ||
                    typeof(TResult) == typeof(object))
                {
                    Collection<KeyValuePair<MessageQuery, TResult>> typedResults =
                        new Collection<KeyValuePair<MessageQuery, TResult>>();

                    foreach (var result in results)
                    {
                        if (typeof(TResult) == typeof(string))
                        {
                            typedResults.Add(
                                new KeyValuePair<MessageQuery, TResult>(
                                    result.Key, (TResult)(object)result.Value.GetResultAsString()));
                        }
                        else if (typeof(TResult) == typeof(bool))
                        {
                            typedResults.Add(
                                new KeyValuePair<MessageQuery, TResult>(
                                    result.Key, (TResult)(object)result.Value.GetResultAsBoolean()));
                        }
                        else
                        {
                            typedResults.Add(new KeyValuePair<MessageQuery, TResult>(
                                result.Key, (TResult)(object)result.Value));
                        }
                    }

                    return (IEnumerator<KeyValuePair<MessageQuery, TResult>>)typedResults.GetEnumerator();
                }
                else
                {
                    throw Fx.AssertAndThrowFatal("unsupported type");
                }
            }
            catch (XPathNavigatorException e)
            {
                throw TraceUtility.ThrowHelperError(e.Process(this.matcher.RootOpcode), this.message);
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw TraceUtility.ThrowHelperError(e.Process(this.matcher.RootOpcode), this.message);
            }
            finally
            {
                if (this.evalBody)
                {
                    this.message.Close();
                }

                this.matcher.ReleaseProcessor(processor);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal abstract class QueryMatcher
    {
        static IFunctionLibrary[] defaultFunctionLibs;  // The set of function libraries that our XPath compiler will link to
        static XPathNavigator fxCompiler;       // fx compiler

        protected int maxNodes;     // Maximum # of nodes that we will process while performing any individual match
        protected Opcode query;     // root opcode - this is where query evaluation starts
        protected int subExprVars;  // the number of subexpr node sequences the processing context must hold

        // Processor Pool
        protected WeakReference processorPool;

        internal class QueryProcessorPool
        {
            QueryProcessor processor;

            internal QueryProcessorPool()
            {
            }

            internal QueryProcessor Pop()
            {
                QueryProcessor p = this.processor;
                if (null != p)
                {
                    this.processor = (QueryProcessor)p.next;
                    p.next = null;
                    return p;
                }
                return null;
            }

            internal void Push(QueryProcessor p)
            {
                p.next = this.processor;
                this.processor = p;
            }
        }

        [SuppressMessage("Microsoft.Security.Xml", "CA3057:DoNotUseLoadXml")]
        static QueryMatcher()
        {
            QueryMatcher.defaultFunctionLibs = new IFunctionLibrary[] { new XPathFunctionLibrary() };

            // For some incomprehensible reason, the Framework XPath compiler requires an instance of an XPath navigator
            // to compile an xpath. This compiler uses a dummy xml document to create a navigator
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<a/>");
            QueryMatcher.fxCompiler = doc.CreateNavigator();
        }

        internal QueryMatcher()
        {
            this.maxNodes = int.MaxValue;
            this.query = null;
            this.processorPool = new WeakReference(null);
            this.subExprVars = 0;
        }
#if NO       
        internal QueryMatcher(QueryMatcher matcher)
        {
            this.processorPool = new WeakReference(null); 
            this.maxNodes = matcher.maxNodes;
            this.query = matcher.query;
            this.subExprVars = matcher.subExprVars;
        }
#endif
        internal bool IsCompiled
        {
            get
            {
                return (null != this.query);
            }
        }

        internal int NodeQuota
        {
            get
            {
                return this.maxNodes;
            }
            set
            {
                Fx.Assert(value > 0, "");
                this.maxNodes = value;
            }
        }

        internal Opcode RootOpcode
        {
            get
            {
                return this.query;
            }
        }

        internal int SubExprVarCount
        {
            get
            {
                return this.subExprVars;
            }
        }

        /// <summary>
        /// Compile the given filter to run on an external (fx) xpath engine
        /// </summary>
        internal static OpcodeBlock CompileForExternalEngine(string expression, XmlNamespaceManager namespaces, object item, bool match)
        {
            // Compile...            
            XPathExpression xpathExpr = QueryMatcher.fxCompiler.Compile(expression);

            // Fx will bind prefixes and functions here.
            if (namespaces != null)
            {
                // There's a bug in System.Xml.XPath.  If we pass an XsltContext to SetContext it won't throw if there's
                // an undefined prefix.
                if (namespaces is XsltContext)
                {
                    // Lex the xpath to find all prefixes used
                    XPathLexer lexer = new XPathLexer(expression, false);
                    while (lexer.MoveNext())
                    {
                        string prefix = lexer.Token.Prefix;

                        if (prefix.Length > 0 && namespaces.LookupNamespace(prefix) == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XsltException(SR.GetString(SR.FilterUndefinedPrefix, prefix)));
                        }
                    }
                }

                xpathExpr.SetContext(namespaces);
            }

            //
            // FORCE the function to COMPILE - they won't bind namespaces unless we check the return type
            //
            if (XPathResultType.Error == xpathExpr.ReturnType)
            {
                // This should never be reached.  The above property should throw if there's an error
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XPathException(SR.GetString(SR.FilterCouldNotCompile, expression)));
            }

            OpcodeBlock codeBlock = new OpcodeBlock();
            SingleFxEngineResultOpcode op;

            if (!match)
            {
                op = new QuerySingleFxEngineResultOpcode();
            }
            else
            {
                op = new MatchSingleFxEngineResultOpcode();
            }

            op.XPath = xpathExpr;
            op.Item = item;

            codeBlock.Append(op);
            return codeBlock;
        }

        /// <summary>
        /// Compile the given filter for evaluation using the internal engine. 
        /// </summary>
        /// <param name="flags">Caller customizes optimizations via the flags parameter</param>
        /// <param name="returnType">Every xpath expression has a return type</param>
        /// <returns>The opcode block we execute to evaluate</returns>
        internal static OpcodeBlock CompileForInternalEngine(XPathMessageFilter filter, QueryCompilerFlags flags, IFunctionLibrary[] functionLibs, out ValueDataType returnType)
        {
            return QueryMatcher.CompileForInternalEngine(filter.XPath.Trim(), filter.namespaces, flags, functionLibs, out returnType);
        }

        internal static OpcodeBlock CompileForInternalEngine(string xpath, XmlNamespaceManager nsManager, QueryCompilerFlags flags, IFunctionLibrary[] functionLibs, out ValueDataType returnType)
        {
            OpcodeBlock codeBlock;

            returnType = ValueDataType.None;
            if (0 == xpath.Length)
            {
                // 0 length XPaths always match
                codeBlock = new OpcodeBlock();
                codeBlock.Append(new PushBooleanOpcode(true)); // Always match by pushing true on the eval stack
            }
            else
            {
                // Try to parse the xpath. Bind to default function libraries
                // The parser returns an expression tree
                XPathParser parser = new XPathParser(xpath, nsManager, functionLibs);
                XPathExpr parseTree = parser.Parse();

                if (null == parseTree)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.CouldNotParseExpression));
                }

                returnType = parseTree.ReturnType;

                // Compile the expression tree
                XPathCompiler compiler = new XPathCompiler(flags);

                codeBlock = compiler.Compile(parseTree);
            }

            return codeBlock;
        }

        internal static OpcodeBlock CompileForInternalEngine(string xpath, XmlNamespaceManager ns, QueryCompilerFlags flags, out ValueDataType returnType)
        {
            return QueryMatcher.CompileForInternalEngine(xpath, ns, flags, QueryMatcher.defaultFunctionLibs, out returnType);
        }

        internal SeekableXPathNavigator CreateMessageNavigator(Message message, bool matchBody)
        {
            SeekableXPathNavigator nav = message.GetNavigator(matchBody, this.maxNodes);

            // Position the navigator at the root element
            // This allows a caller to run relative XPaths on message
            nav.MoveToRoot();
            return nav;
        }

        /// <summary>
        /// Checks the context pool for a generic navigator first. If none is available, creates a new one
        /// </summary>
        internal SeekableXPathNavigator CreateSeekableNavigator(XPathNavigator navigator)
        {
            return new GenericSeekableNavigator(navigator);
        }

        internal SeekableXPathNavigator CreateSafeNavigator(SeekableXPathNavigator navigator)
        {
            INodeCounter counter = navigator as INodeCounter;
            if (counter != null)
            {
                counter.CounterMarker = this.maxNodes;
                counter.MaxCounter = this.maxNodes;
            }
            else
            {
                navigator = new SafeSeekableNavigator(navigator, this.maxNodes);
            }
            return navigator;
        }

        /// <summary>
        /// Checks the context pool for a processor first. If none is available, creates a new one
        /// </summary>
        internal QueryProcessor CreateProcessor()
        {
            QueryProcessor p = null;

            lock (this.processorPool)
            {
                QueryProcessorPool pool = this.processorPool.Target as QueryProcessorPool;
                if (null != pool)
                {
                    p = pool.Pop();
                }
            }

            if (null != p)
            {
                p.ClearProcessor();
            }
            else
            {
                p = new QueryProcessor(this);
            }

            p.AddRef();
            return p;
        }

        internal FilterResult Match(MessageBuffer messageBuffer, ICollection<MessageFilter> matches)
        {
            Message message = messageBuffer.CreateMessage();
            FilterResult result;
            try
            {
                result = this.Match(message, true, matches);
            }
            finally
            {
                message.Close();
            }

            return result;
        }

        internal FilterResult Match(Message message, bool matchBody, ICollection<MessageFilter> matches)
        {
            QueryProcessor processor = this.CreateProcessor();
            processor.MatchSet = matches;
            processor.EnsureFilterCollection();
            try
            {
                processor.Eval(this.query, message, matchBody);
            }
            catch (XPathNavigatorException e)
            {
                throw TraceUtility.ThrowHelperError(e.Process(this.query), message);
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw TraceUtility.ThrowHelperError(e.Process(this.query), message);
            }

            return new FilterResult(processor);
        }

        internal QueryResult<TResult> Evaluate<TResult>(MessageBuffer messageBuffer)
        {
            Message message = messageBuffer.CreateMessage();
            return this.Evaluate<TResult>(message, true);
        }

        internal QueryResult<TResult> Evaluate<TResult>(Message message, bool matchBody)
        {
            return new QueryResult<TResult>(this, message, matchBody);
        }

        /// <summary>
        /// Execute matches over the given seekable navigator. If the navigator is not safe, wrap it with one that is
        /// </summary>
        internal FilterResult Match(SeekableXPathNavigator navigator, ICollection<MessageFilter> matches)
        {
            // If the matcher places restrictions on the # of nodes we will inspect, and the navigator passed does
            // not do any nodecounting itself, we must make that navigator safe by wrapping it
            if (this.maxNodes < int.MaxValue)
            {
                navigator = this.CreateSafeNavigator(navigator);
            }

            QueryProcessor processor = this.CreateProcessor();
            processor.MatchSet = matches;
            processor.EnsureFilterCollection();
            try
            {
                processor.Eval(this.query, navigator);
            }
            catch (XPathNavigatorException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(this.query));
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(this.query));
            }

            return new FilterResult(processor);
        }

        /// <summary>
        /// Execute matches over the given navigator by wrapping it with a Seekable Navigator
        /// </summary>
        internal FilterResult Match(XPathNavigator navigator, ICollection<MessageFilter> matches)
        {
            SeekableXPathNavigator nav = this.CreateSeekableNavigator(navigator);
            return this.Match(nav, matches);
        }

        /// <summary>
        /// Release the given processor and place it back in the context pool
        /// </summary>
        internal void ReleaseProcessor(QueryProcessor processor)
        {
            if (!processor.ReleaseRef())
            {
                return;
            }

            lock (this.processorPool)
            {
                QueryProcessorPool pool = this.processorPool.Target as QueryProcessorPool;
                if (null == pool)
                {
                    pool = new QueryProcessorPool();
                    this.processorPool.Target = pool;
                }
                pool.Push(processor);
            }
        }

        internal void ReleaseResult(FilterResult result)
        {
            if (null != result.Processor)
            {
                result.Processor.MatchSet = null;
                this.ReleaseProcessor(result.Processor);
            }
        }

        /// <summary>
        /// Trim all pool
        /// </summary>
        internal virtual void Trim()
        {
            if (this.query != null)
            {
                this.query.Trim();
            }
        }
    }

    internal enum XPathFilterFlags
    {
        None = 0x00,
        AlwaysMatch = 0x01,     // filter always matches
        IsFxFilter = 0x02,      // filter is matched using the framework engine
    }

    /// <summary>
    /// A matcher used to evalute single XPath expressions
    /// </summary>    
    internal class XPathQueryMatcher : QueryMatcher
    {
        XPathFilterFlags flags;
        bool match;
        static PushBooleanOpcode matchAlwaysFilter; // used for compiling xpaths that always match - i.e. xpath.Length == 0
        static OpcodeBlock rootFilter;        // used for compiling "/"

        static XPathQueryMatcher()
        {
            XPathQueryMatcher.matchAlwaysFilter = new PushBooleanOpcode(true); //dummy

            ValueDataType returnType;
            XPathQueryMatcher.rootFilter = QueryMatcher.CompileForInternalEngine("/", null, QueryCompilerFlags.None, out returnType);
            XPathQueryMatcher.rootFilter.Append(new MatchResultOpcode());
        }

        internal XPathQueryMatcher(bool match)
            : base()
        {
            this.flags = XPathFilterFlags.None;
            this.match = match;
        }
#if NO        
        internal XPathFilterMatcher(XPathFilterMatcher matcher)
            : base(matcher)
        {
            this.flags = matcher.flags;
        }
#endif
        internal bool IsAlwaysMatch
        {
            get
            {
                return (0 != (this.flags & XPathFilterFlags.AlwaysMatch));
            }
        }

        internal bool IsFxFilter
        {
            get
            {
                return (0 != (this.flags & XPathFilterFlags.IsFxFilter));
            }
        }

        /// <summary>
        /// If the xpath is an empty string, there is nothing to compile and the filter always matches
        /// If not, try to compile the filter for execution within the filter engine's own query processor
        /// If that query processor cannot accept the filter (it doesn't fall within the class of xpaths it can handle),
        /// then revert to the fall-back solution - the slower Fx engine
        /// </summary>
        internal void Compile(string expression, XmlNamespaceManager namespaces)
        {
            if (null == this.query)
            {
                // Try to compile for the internal engine first
                try
                {
                    this.CompileForInternal(expression, namespaces);
                }
                catch (QueryCompileException)
                {
                }
                if (null == this.query)
                {
                    // Try for an external engine that might work..
                    this.CompileForExternal(expression, namespaces);
                }
            }
        }

        /// <summary>
        /// Compile this xpath to run on an external (fx) xpath engine
        /// </summary>
        internal void CompileForExternal(string xpath, XmlNamespaceManager names)
        {
            Opcode op = QueryMatcher.CompileForExternalEngine(xpath, names, null, this.match).First;
            this.query = op;
            this.flags |= XPathFilterFlags.IsFxFilter;
        }

        /// <summary>
        /// Compile for the internal engine with default flags
        /// By defalt, we compile an xpath to run stand alone, with standard optimizations
        /// </summary>
        internal void CompileForInternal(string xpath, XmlNamespaceManager names)
        {
            this.query = null;
            xpath = xpath.Trim();

            if (0 == xpath.Length)
            {
                // Empty xpaths always match. Same for xpaths that refer to the root only
                // We will evaluate such filters with minimal overhead. However, we
                // don't want a null value for this.query, so we stick a dummy value in there
                this.query = XPathQueryMatcher.matchAlwaysFilter;
                this.flags |= (XPathFilterFlags.AlwaysMatch);
            }
            else if (1 == xpath.Length && '/' == xpath[0])
            {
                this.query = XPathQueryMatcher.rootFilter.First;
                this.flags |= (XPathFilterFlags.AlwaysMatch);
            }
            else
            {
                ValueDataType returnType;
                OpcodeBlock codeBlock = QueryMatcher.CompileForInternalEngine(xpath, names, QueryCompilerFlags.None, out returnType);
                // Inject a final opcode that will place the query result on the query context
                // This query is now ready for execution STAND ALONE
                if (this.match)
                {
                    codeBlock.Append(new MatchResultOpcode());
                }
                else
                {
                    codeBlock.Append(new QueryResultOpcode());
                }

                this.query = codeBlock.First;
            }

            this.flags &= ~XPathFilterFlags.IsFxFilter;
        }

        internal FilterResult Match(MessageBuffer messageBuffer)
        {
            Message message = messageBuffer.CreateMessage();
            FilterResult result;

            try
            {
                result = this.Match(message, true);
            }
            finally
            {
                message.Close();
            }
            return result;
        }

        internal FilterResult Match(Message message, bool matchBody)
        {
            if (this.IsAlwaysMatch)
            {
                // No need to do any expensive query evaluation if we know that the query will always match
                return new FilterResult(true);
            }

            return base.Match(message, matchBody, null);
        }

        internal FilterResult Match(SeekableXPathNavigator navigator)
        {
            if (this.IsAlwaysMatch)
            {
                // No need to do any expensive query evaluation if we know that the query will always match
                return new FilterResult(true);
            }

            // Is it a filter that we will evaluate using the framework engine?
            // We can evaluate that without having to allocate a query processor
            if (this.IsFxFilter)
            {
                return new FilterResult(this.MatchFx(navigator));
            }

            return base.Match(navigator, null);
        }

        internal FilterResult Match(XPathNavigator navigator)
        {
            Fx.Assert(null != this.query, "");
            if (this.IsAlwaysMatch)
            {
                return new FilterResult(true);
            }
            // Is it a filter that we will evaluate using the framework engine?
            // We can evaluate that without having to allocate a query processor
            if (this.IsFxFilter)
            {
                return new FilterResult(this.MatchFx(navigator));
            }

            return base.Match(navigator, null);
        }

        /// <summary>
        /// Evaluates the filter over infosets surfaced via the given navigator by using the Fx engine
        /// We assume that the filter was pre-compiled using the framework engine
        /// </summary>
        internal bool MatchFx(XPathNavigator navigator)
        {
            INodeCounter counter = navigator as INodeCounter;
            if (counter == null)
            {
                navigator = new SafeSeekableNavigator(new GenericSeekableNavigator(navigator), this.NodeQuota);
            }
            else
            {
                counter.CounterMarker = this.NodeQuota;
                counter.MaxCounter = this.NodeQuota;
            }
            Fx.Assert(null != this.query && OpcodeID.MatchSingleFx == this.query.ID, "");
            try
            {
                return ((MatchSingleFxEngineResultOpcode)this.query).Match(navigator);
            }
            catch (XPathNavigatorException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(this.query));
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(this.query));
            }
        }
    }

    internal class InverseQueryMatcher : QueryMatcher
    {
        SubExprEliminator elim;
        Dictionary<object, Opcode> lastLookup;
        bool match;

        internal InverseQueryMatcher(bool match)
            : base()
        {
            this.elim = new SubExprEliminator();
            this.lastLookup = new Dictionary<object, Opcode>();
            this.match = match;
        }

        internal void Add(string expression, XmlNamespaceManager names, object item, bool forceExternal)
        {
            Fx.Assert(null != item, "");

            // Compile the new filter

            bool compiled = false;
            OpcodeBlock codeBlock = new OpcodeBlock();

            codeBlock.Append(new NoOpOpcode(OpcodeID.QueryTree));
            if (!forceExternal)
            {
                try
                {
                    ValueDataType returnType = ValueDataType.None;

                    // Try to compile and merge the compiled query into the query tree
                    codeBlock.Append(QueryMatcher.CompileForInternalEngine(expression, names, QueryCompilerFlags.InverseQuery, out returnType));

                    MultipleResultOpcode opcode;

                    if (!this.match)
                    {
                        opcode = new QueryMultipleResultOpcode();
                    }
                    else
                    {
                        opcode = new MatchMultipleResultOpcode();
                    }

                    opcode.AddItem(item);
                    codeBlock.Append(opcode);
                    compiled = true;

                    // Perform SubExpression Elimination
                    codeBlock = new OpcodeBlock(this.elim.Add(item, codeBlock.First));
                    this.subExprVars = this.elim.VariableCount;
                }
                catch (QueryCompileException)
                {
                    // If the filter couldn't be compiled, we drop down to the framework engine
                }
            }

            if (!compiled)
            {
                codeBlock.Append(QueryMatcher.CompileForExternalEngine(expression, names, item, this.match));
            }

            // Merge the compiled query into the query tree
            QueryTreeBuilder builder = new QueryTreeBuilder();
            this.query = builder.Build(this.query, codeBlock);
            // To de-merge this filter from the tree, we'll have to walk backwards up the tree... so we
            // have to remember the last opcode that is executed on behalf of this filter
            this.lastLookup[item] = builder.LastOpcode;
        }

        internal void Clear()
        {
            foreach (object item in this.lastLookup.Keys)
            {
                this.Remove(this.lastLookup[item], item);
                this.elim.Remove(item);
            }
            this.subExprVars = this.elim.VariableCount;
            this.lastLookup.Clear();
        }

        internal void Remove(object item)
        {
            Fx.Assert(this.lastLookup.ContainsKey(item), "");

            this.Remove(this.lastLookup[item], item);
            this.lastLookup.Remove(item);

            // Remove filter from subexpr eliminator
            this.elim.Remove(item);
            this.subExprVars = this.elim.VariableCount;
        }

        void Remove(Opcode opcode, object item)
        {
            MultipleResultOpcode multiOpcode = opcode as MultipleResultOpcode;

            if (multiOpcode != null)
            {
                multiOpcode.RemoveItem(item);
            }
            else
            {
                opcode.Remove();
            }
        }

        internal override void Trim()
        {
            base.Trim();
            elim.Trim();
        }
    }
}
