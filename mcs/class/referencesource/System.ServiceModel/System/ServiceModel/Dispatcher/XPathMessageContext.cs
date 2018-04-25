//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.XamlIntegration;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    [TypeConverter(typeof(XPathMessageContextTypeConverter))]
    public class XPathMessageContext : XsltContext
    {
        // Namespace URIs
        internal const string S11NS = Message11Strings.Namespace;
        internal const string S12NS = Message12Strings.Namespace;
        internal const string Wsa200408NS = Addressing200408Strings.Namespace;
        internal const string Wsa10NS = Addressing10Strings.Namespace;
        internal const string WsaNoneNS = AddressingNoneStrings.Namespace;
        internal const string TempUriNS = NamingHelper.DefaultNamespace;
        internal const string SerializationNS = EndpointAddressProcessor.SerNs;
        internal const string IndigoNS = "http://schemas.microsoft.com/serviceModel/2004/05/xpathfunctions";

        // Namespace prefixes
        internal const string S11P = "s11";
        internal const string S12P = "s12";
        internal const string Wsa200408P = "wsaAugust2004";
        internal const string Wsa10P = "wsa10";
        internal const string TempUriP = "tempuri";
        internal const string SerializationP = "ser";
        internal const string IndigoP = "sm";

        internal static Dictionary<string, string> defaultNamespaces;

        // Element names
        internal const string EnvelopeE = MessageStrings.Envelope;
        internal const string HeaderE = MessageStrings.Header;
        internal const string BodyE = MessageStrings.Body;
        internal const string ActionE = AddressingStrings.Action;
        internal const string ToE = AddressingStrings.To;
        internal const string MessageIDE = AddressingStrings.MessageId;
        internal const string RelatesToE = AddressingStrings.RelatesTo;
        internal const string ReplyToE = AddressingStrings.ReplyTo;
        internal const string FromE = AddressingStrings.From;
        internal const string FaultToE = AddressingStrings.FaultTo;

        // Attribute names
        internal static string Actor11A = EnvelopeVersion.Soap11.Actor;
        internal static string Actor12A = EnvelopeVersion.Soap12.Actor;
        internal const string MandatoryA = MessageStrings.MustUnderstand;

        // Functions with no arguments
        internal static readonly XPathMessageFunction HeaderFun = new XPathMessageFunctionHeader();
        internal static readonly XPathMessageFunction BodyFun = new XPathMessageFunctionBody();
        internal static readonly XPathMessageFunction SoapUriFun = new XPathMessageFunctionSoapUri();
        internal static readonly XPathMessageFunction MessageIDFun = new XPathMessageFunctionMessageID();
        internal static readonly XPathMessageFunction RelatesToFun = new XPathMessageFunctionRelatesTo();
        internal static readonly XPathMessageFunction ReplyToFun = new XPathMessageFunctionReplyTo();
        internal static readonly XPathMessageFunction FromFun = new XPathMessageFunctionFrom();
        internal static readonly XPathMessageFunction FaultToFun = new XPathMessageFunctionFaultTo();
        internal static readonly XPathMessageFunction ToFun = new XPathMessageFunctionTo();
        internal static readonly XPathMessageFunction ActionFun = new XPathMessageFunctionAction();
        internal static readonly XPathMessageFunction DateNowFun = new XPathMessageFunctionDateNow();

        // Functions with arguments
        internal static readonly XPathMessageFunction HeadersWithActorFun = new XPathMessageFunctionHeadersWithActor();
        internal static readonly XPathMessageFunction ActorFun = new XPathMessageFunctionActor();
        internal static readonly XPathMessageFunction IsMandatoryFun = new XPathMessageFunctionIsMandatory();
        internal static readonly XPathMessageFunction IsActorNextFun = new XPathMessageFunctionIsActorNext();
        internal static readonly XPathMessageFunction IsActorUltRecFun = new XPathMessageFunctionIsActorUltimateReceiver();
        internal static readonly XPathMessageFunction DateFun = new XPathMessageFunctionDateStr();
        internal static readonly XPathMessageFunction SpanFun = new XPathMessageFunctionSpanStr();
        internal static readonly XPathMessageFunction CorrelationDataFun = new XPathMessageFunctionCorrelationData();

        // Function signatures
        static Function[] functions;

        static XPathMessageContext()
        {
            functions = new Function[]
            {
                new Function(IndigoNS, "header", HeaderFun),
                new Function(IndigoNS, "body", BodyFun),
                new Function(IndigoNS, "soap-uri", SoapUriFun),
                new Function(IndigoNS, "headers-with-actor", HeadersWithActorFun),
                new Function(IndigoNS, "actor", ActorFun),
                new Function(IndigoNS, "is-mandatory", IsMandatoryFun),
                new Function(IndigoNS, "is-actor-next", IsActorNextFun),
                new Function(IndigoNS, "is-actor-ultimate-receiver", IsActorUltRecFun),
                new Function(IndigoNS, "messageId", MessageIDFun),
                new Function(IndigoNS, "relatesTo", RelatesToFun),
                new Function(IndigoNS, "replyTo", ReplyToFun),
                new Function(IndigoNS, "from", FromFun),
                new Function(IndigoNS, "faultTo", FaultToFun),
                new Function(IndigoNS, "to", ToFun),
                new Function(IndigoNS, "action", ActionFun),
                new Function(IndigoNS, "date-time", DateFun),
                new Function(IndigoNS, "duration", SpanFun),
                new Function(IndigoNS, "utc-now", DateNowFun),
                new Function(IndigoNS, "correlation-data", CorrelationDataFun)
            };

            defaultNamespaces = new Dictionary<string, string>()
            {
                { S11P, S11NS },
                { S12P, S12NS },
                { Wsa10P, Wsa10NS },
                { Wsa200408P, Wsa200408NS },
                { TempUriP, TempUriNS },
                { SerializationP, SerializationNS },
                { IndigoP, IndigoNS }
            };
        }

        public XPathMessageContext()
            : this(new NameTable())
        {
        }

        public XPathMessageContext(NameTable table)
            : base(ArgValidator(table))
        {
            foreach (var ns in defaultNamespaces)
            {
                this.AddNamespace(ns.Key, ns.Value);
            }
        }

        static NameTable ArgValidator(NameTable table)
        {
            if (table == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("table");
            return table;
        }

        public override bool Whitespace
        {
            get
            {
                return false;
            }
        }

        public override int CompareDocument(string baseUri, string nextBaseUri)
        {
            return 0;
        }

        public override bool PreserveWhitespace(XPathNavigator node)
        {
            return false;
        }

        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] argTypes)
        {
            if (argTypes == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("argTypes");

            // PERF, Microsoft, factor ns if all same
            string ns = LookupNamespace(prefix);
            for (int i = 0; i < functions.Length; ++i)
            {
                if (functions[i].name == name && functions[i].ns == ns)
                {
                    IXsltContextFunction fun = functions[i].function;
                    if (argTypes.Length <= fun.Maxargs && argTypes.Length >= fun.Minargs)
                    {
                        // Typechecking is done in the compiler.
                        return fun;
                    }
                }
            }
            return null;
        }

        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            return null;
        }

        internal struct Function
        {
            internal string ns;
            internal string name;
            internal IXsltContextFunction function;

            internal Function(string ns, string name, IXsltContextFunction function)
            {
                this.ns = ns;
                this.name = name;
                this.function = function;
            }
        }
    }

    internal abstract class XPathMessageFunction : IXsltContextFunction
    {
        internal readonly static DateTime ZeroDate = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal readonly static XmlNamespaceManager Namespaces = new XmlNamespaceManager(new NameTable());

        XPathResultType[] argTypes;
        int maxArgs;
        int minArgs;
        XPathResultType retType;

        static XPathMessageFunction()
        {
            Namespaces.AddNamespace(XPathMessageContext.S11P, XPathMessageContext.S11NS);
            Namespaces.AddNamespace(XPathMessageContext.S12P, XPathMessageContext.S12NS);
        }

        protected XPathMessageFunction(XPathResultType[] argTypes, int max, int min, XPathResultType retType)
        {
            this.argTypes = argTypes;
            this.maxArgs = max;
            this.minArgs = min;
            this.retType = retType;
        }

        public XPathResultType[] ArgTypes
        {
            get
            {
                return this.argTypes;
            }
        }

        public int Maxargs
        {
            get
            {
                return this.maxArgs;
            }
        }

        public int Minargs
        {
            get
            {
                return this.minArgs;
            }
        }

        public XPathResultType ReturnType
        {
            get
            {
                return this.retType;
            }
        }

        public abstract object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext);
        internal abstract void InvokeInternal(ProcessingContext context, int argCount);

        // Must save/clone navigator before passing to these functions

        internal static bool MoveToAddressingHeader(XPathNavigator nav, string name)
        {
            if (!MoveToHeader(nav))
                return false;

            if (!nav.MoveToFirstChild())
            {
                return false;
            }

            do
            {
                if (nav.LocalName == name && (nav.NamespaceURI == XPathMessageContext.Wsa10NS || nav.NamespaceURI == XPathMessageContext.Wsa200408NS || nav.NamespaceURI == XPathMessageContext.WsaNoneNS))
                {
                    return true;
                }
            } while (nav.MoveToNext());

            return false;
        }

        internal static bool MoveToChild(XPathNavigator nav, string name, string ns)
        {
            if (!nav.MoveToFirstChild())
            {
                return false;
            }

            do
            {
                if (nav.LocalName == name && nav.NamespaceURI == ns)
                {
                    return true;
                }
            } while (nav.MoveToNext());

            return false;
        }

        internal static bool MoveToAddressingHeaderSibling(XPathNavigator nav, string name)
        {
            while (nav.MoveToNext())
            {
                if (nav.LocalName == name && (nav.NamespaceURI == XPathMessageContext.Wsa10NS || nav.NamespaceURI == XPathMessageContext.Wsa200408NS))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool MoveToSibling(XPathNavigator nav, string name, string ns)
        {
            while (nav.MoveToNext())
            {
                if (nav.LocalName == name && nav.NamespaceURI == ns)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool MoveToHeader(XPathNavigator nav)
        {
            nav.MoveToRoot();
            if (!nav.MoveToFirstChild())
            {
                return false;
            }

            string ns = nav.NamespaceURI;
            if (nav.LocalName != XPathMessageContext.EnvelopeE || (ns != XPathMessageContext.S11NS && ns != XPathMessageContext.S12NS))
            {
                return false;
            }

            if (!nav.MoveToFirstChild())
            {
                return false;
            }

            do
            {
                if (nav.LocalName == XPathMessageContext.HeaderE && nav.NamespaceURI == ns)
                {
                    return true;
                }
            } while (nav.MoveToNext());

            return false;
        }

        internal static bool MoveToBody(XPathNavigator nav)
        {
            nav.MoveToRoot();
            if (!nav.MoveToFirstChild())
            {
                return false;
            }

            string ns = nav.NamespaceURI;
            if (nav.LocalName != XPathMessageContext.EnvelopeE || (ns != XPathMessageContext.S11NS && ns != XPathMessageContext.S12NS))
            {
                return false;
            }

            if (!nav.MoveToFirstChild())
            {
                return false;
            }

            do
            {
                if (nav.LocalName == XPathMessageContext.BodyE && nav.NamespaceURI == ns)
                {
                    return true;
                }
            } while (nav.MoveToNext());

            return false;
        }

        internal static string ToString(object o)
        {
            if (o is bool)
            {
                return QueryValueModel.String((bool)o);
            }
            else if (o is string)
            {
                return (string)o;
            }
            else if (o is double)
            {
                return QueryValueModel.String((double)o);
            }
            else if (o is XPathNodeIterator)
            {
                XPathNodeIterator iter = (XPathNodeIterator)o;
                iter.MoveNext();
                return iter.Current.Value;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QueryFunctionStringArg)));
            }
        }

        internal static double ConvertDate(DateTime date)
        {
            if (date.Kind != DateTimeKind.Utc)
            {
                date = date.ToUniversalTime();
            }
            return (date - ZeroDate).TotalDays;
        }
    }

    internal class XPathMessageFunctionCallOpcode : Opcode
    {
        XPathMessageFunction function;
        int argCount;

        internal XPathMessageFunctionCallOpcode(XPathMessageFunction fun, int argCount)
            : base(OpcodeID.XsltInternalFunction)
        {
            this.function = fun;
            this.argCount = argCount;
        }

        internal XPathResultType ReturnType
        {
            get
            {
                return this.function.ReturnType;
            }
        }

        internal int ArgCount
        {
            get
            {
                return this.argCount;
            }
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                XPathMessageFunctionCallOpcode fun = op as XPathMessageFunctionCallOpcode;
                if (fun != null)
                {
                    // Assumption is that they're static on XPathMessageContext
                    return this.function == fun.function;
                }
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            function.InvokeInternal(context, this.argCount);
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} XPathMessageFunction", base.ToString());
        }
#endif
    }

#if NO

    // These classes are left around in case we decide to implement our own variables or any new functions.
    
    internal abstract class XPathMessageVariable : IXsltContextVariable
    {
        protected XPathMessageVariable()
        {
        }

        public abstract bool IsLocal { get; }
        public abstract bool IsParam { get; }
        public abstract XPathResultType VariableType { get; }

        public abstract object Evaluate(XsltContext xsltContext);

        internal void EvaluateInternal(ProcessingContext context)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if(count > 0)
            {
                ValuePush(context, count);
            }
        }

        protected abstract void ValuePush(ProcessingContext context, int count);
    }

    internal class PushXPathMessageVariableOpcode : Opcode
    {
        XPathMessageVariable variable;
        
        internal PushXPathMessageVariableOpcode(XPathMessageVariable var)
            : base(OpcodeID.PushXsltVariable)
        {
            this.variable = var;
        }
        
        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                PushXPathMessageVariableOpcode var = op as PushXPathMessageVariableOpcode;
                if(var != null)
                {
                    return this.variable == var.variable;
                }
            }
            return false;
        }
        
        internal override Opcode Eval(ProcessingContext context)
        {
            this.variable.EvaluateInternal(context);
            return this.next;
        }
        
#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} XPathMessageVariable: {1}", base.ToString(), this.variable.ToString());
        }
#endif
    }

    internal class XPathMessageFunctionQuick : IXsltContextFunction
    {
        static XPathNavigator navigator;
        XmlNamespaceManager context;
        XPathExpression expr;
        XPathResultType retType;

        static XPathMessageFunctionQuick()
        {
            XmlDocument doc = new XmlDocument();
            navigator = doc.CreateNavigator();
        }
        
        public XPathMessageFunctionQuick(string xpath)
        {
            this.context = new XPathMessageContext();
            this.expr = navigator.Compile(xpath);
            this.expr.SetContext(context);
            this.retType = this.expr.ReturnType;
        }
              
        public XPathResultType[] ArgTypes
        {
            get
            {
                return new XPathResultType[] {};
            }
        }
        
        public int Maxargs
        {
            get
            {
                return 0;
            }
        }
        
        public int Minargs
        {
            get
            {
                return 0;
            }
        }
        
        public XPathResultType ReturnType
        {
            get
            {
                return this.retType;
            }
        }
        
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return docContext.Evaluate(this.expr);
        }
    }
#endif

    internal class XPathMessageFunctionAction : XPathMessageFunction
    {
        public XPathMessageFunctionAction()
            : base(new XPathResultType[0], 0, 0, XPathResultType.String)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                string act = context.Processor.Action;
                if (act == null)
                {
                    Message msg = context.Processor.ContextMessage;
                    if (msg == null)
                    {
                        SeekableXPathNavigator nav = context.Processor.ContextNode;

                        long p = nav.CurrentPosition;
                        act = ExtractFromNavigator(nav);
                        nav.CurrentPosition = p;
                    }
                    else
                    {
                        act = msg.Headers.Action;
                    }
                    context.Processor.Action = act;
                }

                if (act == null)
                {
                    act = string.Empty;
                    context.Processor.Action = act;
                }

                if (count == 1)
                {
                    context.Push(act);
                }
                else
                {
                    context.Push(act, count);
                }
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            SeekableMessageNavigator nav = docContext as SeekableMessageNavigator;
            if (nav != null)
            {
                string act = nav.Message.Headers.Action;
                if (act == null)
                    return string.Empty;
                return act;
            }
            return ExtractFromNavigator(docContext.Clone());
        }

        internal static string ExtractFromNavigator(XPathNavigator nav)
        {
            if (!MoveToAddressingHeader(nav, XPathMessageContext.ActionE))
            {
                return string.Empty;
            }

            return nav.Value;
        }
    }

    internal class XPathMessageFunctionTo : XPathMessageFunction
    {
        public XPathMessageFunctionTo()
            : base(new XPathResultType[0], 0, 0, XPathResultType.String)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                string to = context.Processor.ToHeader;
                if (to == null)
                {
                    Message msg = context.Processor.ContextMessage;
                    if (msg == null)
                    {
                        SeekableXPathNavigator nav = context.Processor.ContextNode;

                        long p = nav.CurrentPosition;
                        to = ExtractFromNavigator(nav);
                        nav.CurrentPosition = p;
                    }
                    else
                    {
                        Uri tempTo = msg.Headers.To;
                        if (tempTo == null)
                            to = msg.Version.Addressing.Anonymous;
                        else
                            to = tempTo.AbsoluteUri;
                    }
                    context.Processor.ToHeader = to;
                }
                context.Push(to, count);
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            SeekableMessageNavigator nav = docContext as SeekableMessageNavigator;
            if (nav != null)
            {
                Uri to = nav.Message.Headers.To;
                if (to == null)
                    return string.Empty;
                return to.ToString();
            }
            return ExtractFromNavigator(docContext.Clone());
        }

        static string ExtractFromNavigator(XPathNavigator nav)
        {
            if (!MoveToAddressingHeader(nav, XPathMessageContext.ToE))
            {
                return string.Empty;
            }

            return nav.Value;
        }
    }

    internal class XPathMessageFunctionMessageID : XPathMessageFunction
    {
        public XPathMessageFunctionMessageID()
            : base(new XPathResultType[0], 0, 0, XPathResultType.String)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                string id = context.Processor.MessageId;
                if (id == null)
                {
                    Message msg = context.Processor.ContextMessage;
                    if (msg == null)
                    {
                        SeekableXPathNavigator nav = context.Processor.ContextNode;

                        long p = nav.CurrentPosition;
                        id = ExtractFromNavigator(nav);
                        nav.CurrentPosition = p;
                    }
                    else
                    {
                        UniqueId uid = msg.Headers.MessageId;
                        if (uid == null)
                            id = string.Empty;
                        else
                            id = uid.ToString();
                    }
                    context.Processor.MessageId = id;
                }
                context.Push(id, count);
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            SeekableMessageNavigator nav = docContext as SeekableMessageNavigator;
            if (nav != null)
            {
                UniqueId id = nav.Message.Headers.MessageId;
                if (id == null)
                    return string.Empty;
                return id.ToString();
            }
            return ExtractFromNavigator(docContext.Clone());
        }

        static string ExtractFromNavigator(XPathNavigator nav)
        {
            if (!MoveToAddressingHeader(nav, XPathMessageContext.MessageIDE))
            {
                return string.Empty;
            }

            return nav.Value;
        }
    }

    internal class XPathMessageFunctionHeader : XPathMessageFunction
    {
        XPathExpression expr;

        public XPathMessageFunctionHeader()
            : base(new XPathResultType[0], 0, 0, XPathResultType.NodeSet)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            int count = context.IterationCount;
            context.PushSequenceFrame();
            if (count > 0)
            {
                NodeSequence seq = context.CreateSequence();
                seq.StartNodeset();
                SeekableXPathNavigator nav = context.Processor.ContextNode;

                long p = nav.CurrentPosition;
                if (MoveToHeader(nav))
                {
                    seq.Add(nav);
                }
                nav.CurrentPosition = p;
                seq.StopNodeset();
                context.PushSequence(seq);
                for (int i = 1; i < count; ++i)
                {
                    context.PushSequence(seq);
                }
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (this.expr == null)
            {
                XPathExpression e = docContext.Compile("(/s11:Envelope/s11:Header | /s12:Envelope/s12:Header)[1]");
                e.SetContext(Namespaces);
                this.expr = e;
            }
            return docContext.Evaluate(this.expr);
        }
    }

    internal class XPathMessageFunctionBody : XPathMessageFunction
    {
        XPathExpression expr;

        public XPathMessageFunctionBody()
            : base(new XPathResultType[0], 0, 0, XPathResultType.NodeSet)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            int count = context.IterationCount;
            context.PushSequenceFrame();
            if (count > 0)
            {
                NodeSequence seq = context.CreateSequence();
                seq.StartNodeset();
                SeekableXPathNavigator nav = context.Processor.ContextNode;

                long p = nav.CurrentPosition;
                if (MoveToBody(nav))
                {
                    seq.Add(nav);
                }
                nav.CurrentPosition = p;

                seq.StopNodeset();
                context.PushSequence(seq);
                for (int i = 1; i < count; ++i)
                {
                    seq.refCount++;
                    context.PushSequence(seq);
                }
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (this.expr == null)
            {
                XPathExpression e = docContext.Compile("(/s11:Envelope/s11:Body | /s12:Envelope/s12:Body)[1]");
                e.SetContext(Namespaces);
                this.expr = e;
            }
            return docContext.Evaluate(this.expr);
        }
    }

    internal class XPathMessageFunctionSoapUri : XPathMessageFunction
    {
        public XPathMessageFunctionSoapUri()
            : base(new XPathResultType[0], 0, 0, XPathResultType.String)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                string soap = context.Processor.SoapUri;
                if (soap == null)
                {
                    Message msg = context.Processor.ContextMessage;
                    if (msg == null)
                    {
                        SeekableXPathNavigator nav = context.Processor.ContextNode;

                        long p = nav.CurrentPosition;
                        soap = ExtractFromNavigator(nav);
                        nav.CurrentPosition = p;
                    }
                    else
                    {
                        soap = msg.Version.Envelope.Namespace;
                    }

                    context.Processor.SoapUri = soap;
                }
                context.Push(soap, count);
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            SeekableMessageNavigator nav = docContext as SeekableMessageNavigator;
            if (nav != null)
            {
                return nav.Message.Version.Envelope.Namespace;
            }
            return ExtractFromNavigator(docContext.Clone());
        }

        internal static string ExtractFromNavigator(XPathNavigator nav)
        {
            nav.MoveToRoot();
            if (nav.MoveToFirstChild())
            {
                string ns = nav.NamespaceURI;
                if (nav.LocalName != XPathMessageContext.EnvelopeE || (ns != XPathMessageContext.S11NS && ns != XPathMessageContext.S12NS))
                {
                    return string.Empty;
                }
                else
                {
                    return ns;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }

    internal class XPathMessageFunctionActor : XPathMessageFunction
    {
        internal XPathMessageFunctionActor()
            : base(new XPathResultType[] { XPathResultType.NodeSet }, 1, 1, XPathResultType.String)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame seqArg = context.TopArg;

            while (seqArg.basePtr <= seqArg.endPtr)
            {
                string actor = string.Empty;
                NodeSequence seq = context.PeekSequence(seqArg.basePtr);

                if (seq.Count > 0)
                {
                    SeekableXPathNavigator nav = seq[0].Node.Node;
                    long p = nav.CurrentPosition;
                    nav.CurrentPosition = seq[0].Node.Position;
                    actor = ExtractFromNavigator(nav);
                    nav.CurrentPosition = p;
                }

                context.SetValue(context, seqArg.basePtr, actor);

                seqArg.basePtr++;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            XPathNodeIterator iter = (XPathNodeIterator)args[0];
            if (!iter.MoveNext())
            {
                return string.Empty;
            }

            return ExtractFromNavigator(iter.Current.Clone());
        }

        internal static string ExtractFromNavigator(XPathNavigator nav)
        {
            string actor11 = nav.GetAttribute(XPathMessageContext.Actor11A, XPathMessageContext.S11NS);
            string actor12 = nav.GetAttribute(XPathMessageContext.Actor12A, XPathMessageContext.S12NS);

            nav.MoveToRoot();
            nav.MoveToFirstChild();
            if (nav.LocalName == XPathMessageContext.EnvelopeE && nav.NamespaceURI == XPathMessageContext.S11NS)
            {
                return actor11;
            }
            else if (nav.LocalName == XPathMessageContext.EnvelopeE && nav.NamespaceURI == XPathMessageContext.S12NS)
            {
                return actor12;
            }

            return string.Empty;
        }
    }

    internal class XPathMessageFunctionIsMandatory : XPathMessageFunction
    {
        internal XPathMessageFunctionIsMandatory()
            : base(new XPathResultType[] { XPathResultType.NodeSet }, 1, 1, XPathResultType.Boolean)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame seqArg = context.TopArg;

            while (seqArg.basePtr <= seqArg.endPtr)
            {
                bool mandatory = false;
                NodeSequence seq = context.PeekSequence(seqArg.basePtr);

                if (seq.Count > 0)
                {
                    SeekableXPathNavigator nav = seq[0].Node.Node;
                    long p = nav.CurrentPosition;
                    nav.CurrentPosition = seq[0].Node.Position;
                    mandatory = ExtractFromNavigator(nav);
                    nav.CurrentPosition = p;
                }

                context.SetValue(context, seqArg.basePtr, mandatory);

                seqArg.basePtr++;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            XPathNodeIterator iter = (XPathNodeIterator)args[0];
            if (!iter.MoveNext())
            {
                return false;
            }

            return ExtractFromNavigator(iter.Current.Clone());
        }

        internal static bool ExtractFromNavigator(XPathNavigator nav)
        {
            string mand11 = nav.GetAttribute(XPathMessageContext.MandatoryA, XPathMessageContext.S11NS);
            string mand12 = nav.GetAttribute(XPathMessageContext.MandatoryA, XPathMessageContext.S12NS);

            nav.MoveToRoot();
            nav.MoveToFirstChild();
            if (nav.LocalName == XPathMessageContext.EnvelopeE && nav.NamespaceURI == XPathMessageContext.S11NS)
            {
                return mand11 == "1";
            }
            else if (nav.LocalName == XPathMessageContext.EnvelopeE && nav.NamespaceURI == XPathMessageContext.S12NS)
            {
                return mand12 == "true";
            }

            return false;
        }
    }

    internal class XPathMessageFunctionIsActorNext : XPathMessageFunction
    {
        static string S11Next = EnvelopeVersion.Soap11.NextDestinationActorValue;
        static string S12Next = EnvelopeVersion.Soap12.NextDestinationActorValue;

        internal XPathMessageFunctionIsActorNext()
            : base(new XPathResultType[] { XPathResultType.NodeSet }, 1, 1, XPathResultType.Boolean)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame seqArg = context.TopArg;

            while (seqArg.basePtr <= seqArg.endPtr)
            {
                bool next = false;
                NodeSequence seq = context.PeekSequence(seqArg.basePtr);

                if (seq.Count > 0)
                {
                    SeekableXPathNavigator nav = seq[0].Node.Node;
                    long p = nav.CurrentPosition;
                    nav.CurrentPosition = seq[0].Node.Position;
                    next = ExtractFromNavigator(nav);
                    nav.CurrentPosition = p;
                }

                context.SetValue(context, seqArg.basePtr, next);

                seqArg.basePtr++;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator nav)
        {
            XPathNodeIterator iter = (XPathNodeIterator)args[0];
            if (!iter.MoveNext())
            {
                return false;
            }
            return ExtractFromNavigator(iter.Current.Clone());
        }

        internal static bool ExtractFromNavigator(XPathNavigator nav)
        {
            string actor = XPathMessageFunctionActor.ExtractFromNavigator(nav);
            if (actor.Length == 0)
            {
                return false;
            }

            nav.MoveToRoot();
            if (!nav.MoveToFirstChild())
            {
                return false;
            }

            if (nav.LocalName == XPathMessageContext.EnvelopeE)
            {
                if (nav.NamespaceURI == XPathMessageContext.S11NS)
                {
                    return actor == S11Next;
                }
                else if (nav.NamespaceURI == XPathMessageContext.S12NS)
                {
                    return actor == S12Next;
                }
            }

            return false;
        }
    }

    internal class XPathMessageFunctionIsActorUltimateReceiver : XPathMessageFunction
    {
        static string S11UltRec = EnvelopeVersion.Soap11.UltimateDestinationActor;
        static string S12UltRec = EnvelopeVersion.Soap12.UltimateDestinationActor;

        internal XPathMessageFunctionIsActorUltimateReceiver()
            : base(new XPathResultType[] { XPathResultType.NodeSet }, 1, 1, XPathResultType.Boolean)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame seqArg = context.TopArg;

            while (seqArg.basePtr <= seqArg.endPtr)
            {
                bool ult = false;
                NodeSequence seq = context.PeekSequence(seqArg.basePtr);

                if (seq.Count > 0)
                {
                    SeekableXPathNavigator nav = seq[0].Node.Node;
                    long p = nav.CurrentPosition;
                    nav.CurrentPosition = seq[0].Node.Position;
                    ult = ExtractFromNavigator(nav);
                    nav.CurrentPosition = p;
                }

                context.SetValue(context, seqArg.basePtr, ult);

                seqArg.basePtr++;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator nav)
        {
            XPathNodeIterator iter = (XPathNodeIterator)args[0];
            if (!iter.MoveNext())
            {
                return false;
            }
            return ExtractFromNavigator(iter.Current.Clone());
        }

        internal static bool ExtractFromNavigator(XPathNavigator nav)
        {
            string actor = XPathMessageFunctionActor.ExtractFromNavigator(nav);

            nav.MoveToRoot();
            if (!nav.MoveToFirstChild())
            {
                return false;
            }

            if (nav.LocalName == XPathMessageContext.EnvelopeE)
            {
                if (nav.NamespaceURI == XPathMessageContext.S11NS)
                {
                    return actor == S11UltRec;
                }
                else if (nav.NamespaceURI == XPathMessageContext.S12NS)
                {
                    return actor == S12UltRec;
                }
            }

            return false;
        }
    }

    internal class XPathMessageFunctionHeadersWithActor : XPathMessageFunction
    {
        internal XPathMessageFunctionHeadersWithActor()
            : base(new XPathResultType[] { XPathResultType.String }, 1, 1, XPathResultType.NodeSet)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame actorArg = context.TopArg;

            SeekableXPathNavigator nav = context.Processor.ContextNode;
            long p = nav.CurrentPosition;
            while (actorArg.basePtr <= actorArg.endPtr)
            {
                string actor = context.PeekString(actorArg.basePtr);
                NodeSequence seq = context.CreateSequence();

                if (MoveToHeader(nav) && nav.MoveToFirstChild())
                {
                    do
                    {
                        // PERF, Microsoft, this will be faster if I cache the envelope namespace to do the
                        //               actor lookup by hand
                        long pos = nav.CurrentPosition;
                        string navActor = XPathMessageFunctionActor.ExtractFromNavigator(nav);
                        nav.CurrentPosition = pos;

                        if (navActor == actor)
                        {
                            seq.Add(nav);
                        }
                    } while (nav.MoveToNext());
                }

                context.SetValue(context, actorArg.basePtr, seq);

                actorArg.basePtr++;
            }
            nav.CurrentPosition = p;
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            string actor = ToString(args[0]);
            string e = string.Format(CultureInfo.InvariantCulture, "/s11:Envelope/s11:Header/*[@s11:actor='{0}'] | /s12:Envelope/s12:Header/*[@s12:role='{1}']", actor, actor);
            XPathExpression expr = docContext.Compile(e);
            expr.SetContext(xsltContext);
            return docContext.Evaluate(expr);

#if NO
            // PERF, Microsoft, I drafted this implementation before we found out that a bug in the Fx implementation would 
            //               prevent us from constructing an XPathNodeIterator that they would accept.  I'm keeping it
            //               around in the hope that I will be able to use it by M5.4.  If not, it will be deleted.
            
            XPathNavigator basicNav = docContext.Clone();
            SeekableXPathNavigator nav = basicNav as SeekableXPathNavigator;
            if(nav == null)
            {
                nav = new GenericSeekableNavigator(basicNav);
            }
            
            string actor = (string)args[0];
            NodeSequence seq = new NodeSequence();
            XPathNodeIterator result = new NodeSequenceIterator(seq);
            
            nav.MoveToRoot();
            if(!nav.MoveToFirstChild())
            {
                return result;
            }

            if(nav.LocalName != "Envelope")
            {
                return result;
            }
            
            if(nav.NamespaceURI == XPathMessageContext.S11NS)
            {
                // Move to Header
                if(nav.MoveToFirstChild() && nav.LocalName == "Header" && nav.NamespaceURI == XPathMessageContext.S11NS)
                {
                    // Move to first Header block
                    if(nav.MoveToFirstChild())
                    {
                        // Iterate over header blocks
                        do
                        {
                            if(nav.MoveToAttribute("actor", XPathMessageContext.S11NS))
                            {
                                if(nav.Value == actor)
                                {
                                    seq.Add(nav);
                                }
                                nav.MoveToParent();
                            }
                                    
                        } while(nav.MoveToNext());
                    }
                }
            }
            else if(nav.NamespaceURI == XPathMessageContext.S12NS)
            {
                // Move to Header
                if(nav.MoveToFirstChild() && nav.LocalName == "Header" && nav.NamespaceURI == XPathMessageContext.S12NS)
                {
                    // Move to first Header block
                    if(nav.MoveToFirstChild())
                    {
                        // Iterate over header blocks
                        do
                        {
                            if(nav.MoveToAttribute("role", XPathMessageContext.S12NS))
                            {
                                if(nav.Value == actor)
                                {
                                    seq.Add(nav);
                                }
                                nav.MoveToParent();
                            }
                                    
                        } while(nav.MoveToNext());
                    }
                }
            }

            return result;
#endif
        }
    }

    internal class XPathMessageFunctionRelatesTo : XPathMessageFunction
    {
        XPathExpression expr;

        internal XPathMessageFunctionRelatesTo()
            : base(new XPathResultType[] { }, 0, 0, XPathResultType.NodeSet)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            int count = context.IterationCount;
            context.PushSequenceFrame();
            if (count > 0)
            {
                NodeSequence seq = context.CreateSequence();
                seq.StartNodeset();
                SeekableXPathNavigator nav = context.Processor.ContextNode;

                long p = nav.CurrentPosition;
                if (MoveToAddressingHeader(nav, XPathMessageContext.RelatesToE))
                {
                    seq.Add(nav);
                    while (MoveToAddressingHeaderSibling(nav, XPathMessageContext.RelatesToE))
                    {
                        seq.Add(nav);
                    }
                }
                seq.StopNodeset();
                context.PushSequence(seq);
                for (int i = 1; i < count; ++i)
                {
                    seq.refCount++;
                    context.PushSequence(seq);
                }
                nav.CurrentPosition = p;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (this.expr == null)
            {
                XPathExpression e = docContext.Compile("sm:header()/wsa10:RelatesTo | sm:header()/wsaAugust2004:RelatesTo");
                e.SetContext(new XPathMessageContext());
                this.expr = e;
            }
            return docContext.Evaluate(this.expr);
        }
    }

    internal class XPathMessageFunctionReplyTo : XPathMessageFunction
    {
        XPathExpression expr;

        internal XPathMessageFunctionReplyTo()
            : base(new XPathResultType[] { }, 0, 0, XPathResultType.NodeSet)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            int count = context.IterationCount;
            context.PushSequenceFrame();
            if (count > 0)
            {
                NodeSequence seq = context.CreateSequence();
                seq.StartNodeset();
                SeekableXPathNavigator nav = context.Processor.ContextNode;

                long p = nav.CurrentPosition;
                if (MoveToAddressingHeader(nav, XPathMessageContext.ReplyToE))
                {
                    seq.Add(nav);
                }
                seq.StopNodeset();
                context.PushSequence(seq);
                for (int i = 1; i < count; ++i)
                {
                    seq.refCount++;
                    context.PushSequence(seq);
                }
                nav.CurrentPosition = p;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {

            if (this.expr == null)
            {
                XPathExpression e = docContext.Compile("(sm:header()/wsa10:ReplyTo | sm:header()/wsaAugust2004:ReplyTo)[1]");
                e.SetContext(new XPathMessageContext());
                this.expr = e;
            }
            return docContext.Evaluate(this.expr);
        }
    }

    internal class XPathMessageFunctionFrom : XPathMessageFunction
    {
        XPathExpression expr;

        internal XPathMessageFunctionFrom()
            : base(new XPathResultType[] { }, 0, 0, XPathResultType.NodeSet)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            int count = context.IterationCount;
            context.PushSequenceFrame();
            if (count > 0)
            {
                NodeSequence seq = context.CreateSequence();
                seq.StartNodeset();
                SeekableXPathNavigator nav = context.Processor.ContextNode;

                long p = nav.CurrentPosition;
                if (MoveToAddressingHeader(nav, XPathMessageContext.FromE))
                {
                    seq.Add(nav);
                }
                seq.StopNodeset();
                context.PushSequence(seq);
                for (int i = 1; i < count; ++i)
                {
                    seq.refCount++;
                    context.PushSequence(seq);
                }
                nav.CurrentPosition = p;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (this.expr == null)
            {
                XPathExpression e = docContext.Compile("(sm:header()/wsa10:From | sm:header()/wsaAugust2004:From)[1]");
                e.SetContext(new XPathMessageContext());
                this.expr = e;
            }
            return docContext.Evaluate(this.expr);
        }
    }

    internal class XPathMessageFunctionFaultTo : XPathMessageFunction
    {
        XPathExpression expr;

        internal XPathMessageFunctionFaultTo()
            : base(new XPathResultType[] { }, 0, 0, XPathResultType.NodeSet)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            int count = context.IterationCount;
            context.PushSequenceFrame();
            if (count > 0)
            {
                NodeSequence seq = context.CreateSequence();
                seq.StartNodeset();
                SeekableXPathNavigator nav = context.Processor.ContextNode;

                long p = nav.CurrentPosition;
                if (MoveToAddressingHeader(nav, XPathMessageContext.FaultToE))
                {
                    seq.Add(nav);
                }
                seq.StopNodeset();
                context.PushSequence(seq);
                for (int i = 1; i < count; ++i)
                {
                    seq.refCount++;
                    context.PushSequence(seq);
                }
                nav.CurrentPosition = p;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            if (this.expr == null)
            {
                XPathExpression e = docContext.Compile("(sm:header()/wsa10:FaultTo | sm:header()/wsaAugust2004:FaultTo)[1]");
                e.SetContext(new XPathMessageContext());
                this.expr = e;
            }
            return docContext.Evaluate(this.expr);
        }
    }

    internal class XPathMessageFunctionDateStr : XPathMessageFunction
    {
        internal XPathMessageFunctionDateStr()
            : base(new XPathResultType[1] { XPathResultType.String }, 1, 1, XPathResultType.Number)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame dateArg = context.TopArg;
            while (dateArg.basePtr <= dateArg.endPtr)
            {
                string dateStr = context.PeekString(dateArg.basePtr);
                context.SetValue(context, dateArg.basePtr, Convert(dateStr));
                dateArg.basePtr++;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return Convert(ToString(args[0]));
        }

        internal static double Convert(string dateStr)
        {
            try
            {
                return ConvertDate(DateTime.Parse(dateStr, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.RoundtripKind));
            }
            catch (FormatException)
            {
                return double.NaN;
            }
        }
    }

    internal class XPathMessageFunctionCorrelationData : XPathMessageFunction
    {
        static XPathResultType[] argTypes = new XPathResultType[1] { XPathResultType.String };

        public XPathMessageFunctionCorrelationData()
            : base(argTypes, 1, 1, XPathResultType.String)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame nameArg = context.TopArg;
            Message message = context.Processor.ContextMessage;
            CorrelationDataMessageProperty data = null;

            CorrelationDataMessageProperty.TryGet(message, out data);

            while (nameArg.basePtr <= nameArg.endPtr)
            {
                string value;

                if (data == null || !data.TryGetValue(context.PeekString(nameArg.basePtr), out value))
                {
                    value = string.Empty;
                }

                context.SetValue(context, nameArg.basePtr, value);
                nameArg.basePtr++;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            SeekableMessageNavigator nav = docContext as SeekableMessageNavigator;

            if (nav != null)
            {
                Message message = nav.Message;
                CorrelationDataMessageProperty data;
                string value;

                if (!CorrelationDataMessageProperty.TryGet(message, out data) ||
                    !data.TryGetValue((string)args[0], out value))
                {
                    value = string.Empty;
                }

                return value;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    internal class XPathMessageFunctionDateNow : XPathMessageFunction
    {
        internal XPathMessageFunctionDateNow()
            : base(new XPathResultType[0], 0, 0, XPathResultType.Number)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                context.Push(ConvertDate(DateTime.Now), count);
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return ConvertDate(DateTime.UtcNow);
        }
    }

    internal class XPathMessageFunctionSpanStr : XPathMessageFunction
    {
        internal XPathMessageFunctionSpanStr()
            : base(new XPathResultType[1] { XPathResultType.String }, 1, 1, XPathResultType.Number)
        {
        }

        internal override void InvokeInternal(ProcessingContext context, int argCount)
        {
            StackFrame spanArg = context.TopArg;
            while (spanArg.basePtr <= spanArg.endPtr)
            {
                string spanStr = context.PeekString(spanArg.basePtr);
                context.SetValue(context, spanArg.basePtr, Convert(spanStr));
                spanArg.basePtr++;
            }
        }

        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return Convert(ToString(args[0]));
        }

        internal static double Convert(string spanStr)
        {
            try
            {
                return TimeSpan.Parse(spanStr, CultureInfo.InvariantCulture).TotalDays;
            }
            catch (FormatException)
            {
                return double.NaN;
            }
            catch (OverflowException)
            {
                return double.NaN;
            }
        }
    }
}
