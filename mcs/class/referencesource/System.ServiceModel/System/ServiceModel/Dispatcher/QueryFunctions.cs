//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class FunctionCallOpcode : Opcode
    {
        QueryFunction function;

        internal FunctionCallOpcode(QueryFunction function)
            : base(OpcodeID.Function)
        {
            Fx.Assert(null != function, "");
            this.function = function;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                FunctionCallOpcode functionCall = (FunctionCallOpcode)op;
                return functionCall.function.Equals(this.function);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            this.function.Eval(context);
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.function.ToString());
        }
#endif
    }

    internal class XsltFunctionCallOpcode : Opcode
    {
        static object[] NullArgs = new object[0];

        int argCount;
        XsltContext xsltContext;
        IXsltContextFunction function;
        List<NodeSequenceIterator> iterList;


        // REFACTOR, [....], make this a function on QueryValueModel
        internal XsltFunctionCallOpcode(XsltContext context, IXsltContextFunction function, int argCount)
            : base(OpcodeID.XsltFunction)
        {
            Fx.Assert(null != context && null != function, "");
            this.xsltContext = context;
            this.function = function;
            this.argCount = argCount;

            for (int i = 0; i < function.Maxargs; ++i)
            {
                if (function.ArgTypes[i] == XPathResultType.NodeSet)
                {
                    this.iterList = new List<NodeSequenceIterator>();
                    break;
                }
            }

            // Make sure the return type is valid
            switch (this.function.ReturnType)
            {
                case XPathResultType.String:
                case XPathResultType.Number:
                case XPathResultType.Boolean:
                case XPathResultType.NodeSet:
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidType, SR.GetString(SR.QueryFunctionTypeNotSupported, this.function.ReturnType.ToString())));
            }

        }

        internal override bool Equals(Opcode op)
        {
            // We have no way of knowing if an Xslt function is stateless and can be merged
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            XPathNavigator nav = context.Processor.ContextNode;
            if (nav != null && context.Processor.ContextMessage != null)
            {
                ((SeekableMessageNavigator)nav).Atomize();
            }

            if (this.argCount == 0)
            {
                context.PushFrame();
                int count = context.IterationCount;
                if (count > 0)
                {
                    object ret = this.function.Invoke(this.xsltContext, NullArgs, nav);
                    switch (this.function.ReturnType)
                    {
                        case XPathResultType.String:
                            context.Push((string)ret, count);
                            break;

                        case XPathResultType.Number:
                            context.Push((double)ret, count);
                            break;

                        case XPathResultType.Boolean:
                            context.Push((bool)ret, count);
                            break;

                        case XPathResultType.NodeSet:
                            NodeSequence seq = context.CreateSequence();
                            XPathNodeIterator iter = (XPathNodeIterator)ret;
                            seq.Add(iter);
                            context.Push(seq, count);
                            break;

                        default:
                            // This should never be reached
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryFunctionTypeNotSupported, this.function.ReturnType.ToString())));
                    }
                }
            }
            else
            {
                // PERF, [....], see if we can cache these arrays to avoid allocations
                object[] xsltArgs = new object[this.argCount];
                int iterationCount = context.TopArg.Count;
                for (int iteration = 0; iteration < iterationCount; ++iteration)
                {
                    for (int i = 0; i < this.argCount; ++i)
                    {
                        StackFrame arg = context[i];
                        Fx.Assert(iteration < arg.Count, "");

                        switch (this.function.ArgTypes[i])
                        {
                            case XPathResultType.String:
                                xsltArgs[i] = context.PeekString(arg[iteration]);
                                break;

                            case XPathResultType.Number:
                                xsltArgs[i] = context.PeekDouble(arg[iteration]);
                                break;

                            case XPathResultType.Boolean:
                                xsltArgs[i] = context.PeekBoolean(arg[iteration]);
                                break;

                            case XPathResultType.NodeSet:
                                NodeSequenceIterator iter = new NodeSequenceIterator(context.PeekSequence(arg[iteration]));
                                xsltArgs[i] = iter;
                                this.iterList.Add(iter);
                                break;

                            default:
                                // This should never be reached
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryFunctionTypeNotSupported, this.function.ArgTypes[i].ToString())));
                        }
                    }

                    object ret = this.function.Invoke(this.xsltContext, xsltArgs, nav);

                    if (this.iterList != null)
                    {
                        for (int i = 0; i < this.iterList.Count; ++i)
                        {
                            this.iterList[i].Clear();
                        }
                        this.iterList.Clear();
                    }

                    switch (this.function.ReturnType)
                    {
                        case XPathResultType.String:
                            context.SetValue(context, context[this.argCount - 1][iteration], (string)ret);
                            break;

                        case XPathResultType.Number:
                            context.SetValue(context, context[this.argCount - 1][iteration], (double)ret);
                            break;

                        case XPathResultType.Boolean:
                            context.SetValue(context, context[this.argCount - 1][iteration], (bool)ret);
                            break;

                        case XPathResultType.NodeSet:
                            NodeSequence seq = context.CreateSequence();
                            XPathNodeIterator iter = (XPathNodeIterator)ret;
                            seq.Add(iter);
                            context.SetValue(context, context[this.argCount - 1][iteration], seq);
                            break;

                        default:
                            // This should never be reached
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryFunctionTypeNotSupported, this.function.ReturnType.ToString())));
                    }
                }

                for (int i = 0; i < this.argCount - 1; ++i)
                {
                    context.PopFrame();
                }
            }
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} IXsltContextFunction", base.ToString());
        }
#endif
    }

    internal enum QueryFunctionFlag
    {
        None = 0x00000000,
        UsesContextNode = 0x00000001
    }

    internal abstract class QueryFunction
    {
        static ValueDataType[] emptyParams = new ValueDataType[0];
        QueryFunctionFlag flags;
        protected string name;
        ValueDataType[] paramTypes;
        ValueDataType returnType;

        internal QueryFunction(string name, ValueDataType returnType)
            : this(name, returnType, QueryFunction.emptyParams, QueryFunctionFlag.None)
        {
        }

        internal QueryFunction(string name, ValueDataType returnType, QueryFunctionFlag flags)
            : this(name, returnType, QueryFunction.emptyParams, flags)
        {
        }

        internal QueryFunction(string name, ValueDataType returnType, ValueDataType[] paramTypes)
            : this(name, returnType, paramTypes, QueryFunctionFlag.None)
        {
        }

        internal QueryFunction(string name, ValueDataType returnType, ValueDataType[] paramTypes, QueryFunctionFlag flags)
        {
            Fx.Assert(null != paramTypes, "");
            Fx.Assert(null != name, "");

            this.name = name;
            this.returnType = returnType;
            this.paramTypes = paramTypes;
            this.flags = flags;
        }

        internal ValueDataType[] ParamTypes
        {
            get
            {
                return this.paramTypes;
            }
        }

        internal ValueDataType ReturnType
        {
            get
            {
                return this.returnType;
            }
        }

        internal bool Bind(string name, XPathExprList args)
        {
            Fx.Assert(null != name && null != args, "");

            if (
                0 != string.CompareOrdinal(this.name, name)
                || this.paramTypes.Length != args.Count
                )
            {
                return false;
            }

            return (this.paramTypes.Length == args.Count);
        }

        internal abstract bool Equals(QueryFunction function);
        internal abstract void Eval(ProcessingContext context);

        internal bool TestFlag(QueryFunctionFlag flag)
        {
            return (0 != (this.flags & flag));
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();

            text.Append(this.name);
            text.Append('(');
            for (int i = 0; i < this.paramTypes.Length; ++i)
            {
                if (i > 0)
                {
                    text.Append(',');
                }
                text.Append(this.paramTypes[i].ToString());
            }
            text.Append(')');

            return text.ToString();
        }
#endif
    }

    internal interface IFunctionLibrary
    {
        QueryFunction Bind(string functionName, string functionNamespace, XPathExprList args);
    }

    internal enum XPathFunctionID
    {
        // Set
        IterateSequences,
        Count,
        Position,
        Last,
        LocalName,
        LocalNameDefault,
        Name,
        NameDefault,
        NamespaceUri,
        NamespaceUriDefault,
        // Boolean
        Boolean,
        Not,
        True,
        False,
        Lang,
        // Number
        Number,
        NumberDefault,
        Ceiling,
        Floor,
        Round,
        Sum,
        // String
        String,
        StringDefault,
        StartsWith,
        ConcatTwo,
        ConcatThree,
        ConcatFour,
        Contains,
        NormalizeSpace,
        NormalizeSpaceDefault,
        StringLength,
        StringLengthDefault,
        SubstringBefore,
        SubstringAfter,
        Substring,
        SubstringLimit,
        Translate
    }

    internal class XPathFunctionLibrary : IFunctionLibrary
    {
        static XPathFunction[] functionTable;

        static XPathFunctionLibrary()
        {
            XPathFunctionLibrary.functionTable = new XPathFunction[] {
                new XPathFunction(XPathFunctionID.Boolean, "boolean", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.None }),
                new XPathFunction(XPathFunctionID.False, "false", ValueDataType.Boolean),
                new XPathFunction(XPathFunctionID.True, "true", ValueDataType.Boolean),
                new XPathFunction(XPathFunctionID.Not, "not", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.Boolean }),
                new XPathFunction(XPathFunctionID.Lang, "lang", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.String }),

                new XPathFunction(XPathFunctionID.Number, "number", ValueDataType.Double, new ValueDataType[] { ValueDataType.None }),
                new XPathFunction(XPathFunctionID.NumberDefault, "number", ValueDataType.Double),
                new XPathFunction(XPathFunctionID.Sum, "sum", ValueDataType.Double, new ValueDataType[] { ValueDataType.Sequence }),
                new XPathFunction(XPathFunctionID.Floor, "floor", ValueDataType.Double, new ValueDataType[] { ValueDataType.Double }),
                new XPathFunction(XPathFunctionID.Ceiling, "ceiling", ValueDataType.Double, new ValueDataType[] { ValueDataType.Double }),
                new XPathFunction(XPathFunctionID.Round, "round", ValueDataType.Double, new ValueDataType[] { ValueDataType.Double }),

                new XPathFunction(XPathFunctionID.String, "string", ValueDataType.String, new ValueDataType[] { ValueDataType.None }),
                new XPathFunction(XPathFunctionID.StringDefault, "string", ValueDataType.String, QueryFunctionFlag.UsesContextNode),
                new XPathFunction(XPathFunctionID.ConcatTwo, "concat", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String }),
                new XPathFunction(XPathFunctionID.ConcatThree, "concat", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String, ValueDataType.String }),
                new XPathFunction(XPathFunctionID.ConcatFour, "concat", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String, ValueDataType.String, ValueDataType.String }),
                new XPathFunction(XPathFunctionID.StartsWith, "starts-with", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.String, ValueDataType.String }),
                new XPathFunction(XPathFunctionID.NormalizeSpace, "normalize-space", ValueDataType.String, new ValueDataType[] { ValueDataType.String }),
                new XPathFunction(XPathFunctionID.NormalizeSpaceDefault, "normalize-space", ValueDataType.String, QueryFunctionFlag.UsesContextNode),
                new XPathFunction(XPathFunctionID.Contains, "contains", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.String, ValueDataType.String }),
                new XPathFunction(XPathFunctionID.SubstringBefore, "substring-before", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String }),
                new XPathFunction(XPathFunctionID.SubstringAfter, "substring-after", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String }),
                new XPathFunction(XPathFunctionID.Substring, "substring", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.Double }),
                new XPathFunction(XPathFunctionID.SubstringLimit, "substring", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.Double, ValueDataType.Double }),
                new XPathFunction(XPathFunctionID.StringLength, "string-length", ValueDataType.Double, new ValueDataType[] { ValueDataType.String }),
                new XPathFunction(XPathFunctionID.StringLengthDefault, "string-length", ValueDataType.Double, QueryFunctionFlag.UsesContextNode),
                new XPathFunction(XPathFunctionID.Translate, "translate", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String, ValueDataType.String }),

                new XPathFunction(XPathFunctionID.Last, "last", ValueDataType.Double, QueryFunctionFlag.UsesContextNode),
                new XPathFunction(XPathFunctionID.Position, "position", ValueDataType.Double, QueryFunctionFlag.UsesContextNode),
                new XPathFunction(XPathFunctionID.Count, "count", ValueDataType.Double, new ValueDataType[] { ValueDataType.Sequence }),
                new XPathFunction(XPathFunctionID.LocalName, "local-name", ValueDataType.String, new ValueDataType[] { ValueDataType.Sequence }),
                new XPathFunction(XPathFunctionID.LocalNameDefault, "local-name", ValueDataType.String, QueryFunctionFlag.UsesContextNode),
                new XPathFunction(XPathFunctionID.Name, "name", ValueDataType.String, new ValueDataType[] { ValueDataType.Sequence }),
                new XPathFunction(XPathFunctionID.NameDefault, "name", ValueDataType.String, QueryFunctionFlag.UsesContextNode),
                new XPathFunction(XPathFunctionID.NamespaceUri, "namespace-uri", ValueDataType.String, new ValueDataType[] { ValueDataType.Sequence }),
                new XPathFunction(XPathFunctionID.NamespaceUriDefault, "namespace-uri", ValueDataType.String, QueryFunctionFlag.UsesContextNode)
            };
        }

        internal XPathFunctionLibrary()
        {
        }

        public QueryFunction Bind(string functionName, string functionNamespace, XPathExprList args)
        {
            Fx.Assert(null != functionName && null != args, "");

            // Variable length argument list requires a special case here
            if (functionName == "concat" && args.Count > 4)
            {
                ConcatFunction f = new ConcatFunction(args.Count);
                if (f.Bind(functionName, args))
                {
                    return f;
                }
            }
            else
            {
                for (int i = 0; i < XPathFunctionLibrary.functionTable.Length; ++i)
                {
                    // XPath functions are typeless, so don't check types
                    if (XPathFunctionLibrary.functionTable[i].Bind(functionName, args))
                    {
                        return XPathFunctionLibrary.functionTable[i];
                    }
                }
            }

            return null;
        }
    }

    internal class ConcatFunction : QueryFunction
    {
        int argCount;

        internal ConcatFunction(int argCount)
            : base("concat", ValueDataType.String, ConcatFunction.MakeTypes(argCount))
        {
            Fx.Assert(argCount >= 2, "");
            this.argCount = argCount;
        }

        internal override bool Equals(QueryFunction function)
        {
            ConcatFunction f = function as ConcatFunction;
            if (f != null && this.argCount == f.argCount)
            {
                return true;
            }
            return false;
        }

        internal override void Eval(ProcessingContext context)
        {
            Fx.Assert(context != null, "");

            StackFrame[] args = new StackFrame[argCount];
            for (int i = 0; i < this.argCount; ++i)
            {
                args[i] = context[i];
            }

            StringBuilder builder = new StringBuilder();
            while (args[0].basePtr <= args[0].endPtr)
            {
                builder.Length = 0;

                for (int i = 0; i < this.argCount; ++i)
                {
                    builder.Append(context.PeekString(args[i].basePtr));
                }

                context.SetValue(context, args[this.argCount - 1].basePtr, builder.ToString());
                for (int i = 0; i < this.argCount; ++i)
                {
                    args[i].basePtr++;
                }
            }

            for (int i = 0; i < this.argCount - 1; ++i)
            {
                context.PopFrame();
            }
        }

        internal static ValueDataType[] MakeTypes(int size)
        {
            ValueDataType[] t = new ValueDataType[size];
            for (int i = 0; i < size; ++i)
            {
                t[i] = ValueDataType.String;
            }
            return t;
        }
    }

    internal class XPathFunction : QueryFunction
    {
        XPathFunctionID functionID;

        internal XPathFunction(XPathFunctionID functionID, string name, ValueDataType returnType)
            : base(name, returnType)
        {
            this.functionID = functionID;
        }

        internal XPathFunction(XPathFunctionID functionID, string name, ValueDataType returnType, QueryFunctionFlag flags)
            : base(name, returnType, flags)
        {
            this.functionID = functionID;
        }

        internal XPathFunction(XPathFunctionID functionID, string name, ValueDataType returnType, ValueDataType[] argTypes)
            : base(name, returnType, argTypes)
        {
            this.functionID = functionID;
        }

        internal XPathFunctionID ID
        {
            get
            {
                return this.functionID;
            }
        }

        internal override bool Equals(QueryFunction function)
        {
            XPathFunction xpathFunction = function as XPathFunction;
            if (null == xpathFunction)
            {
                return false;
            }

            return (xpathFunction.ID == this.ID);
        }

        static void ConvertFirstArg(ProcessingContext context, ValueDataType type)
        {
            StackFrame arg = context.TopArg;
            Value[] values = context.Values;

            while (arg.basePtr <= arg.endPtr)
            {
                values[arg.basePtr++].ConvertTo(context, type);
            }
        }

        internal override void Eval(ProcessingContext context)
        {
            Fx.Assert(null != context, "");

            switch (this.functionID)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.QueryNotImplemented, this.name)));

                case XPathFunctionID.IterateSequences:
                    XPathFunction.IterateAndPushSequences(context);
                    break;

                case XPathFunctionID.Count:
                    XPathFunction.NodesetCount(context);
                    break;

                case XPathFunctionID.Position:
                    XPathFunction.NodesetPosition(context);
                    break;

                case XPathFunctionID.Last:
                    XPathFunction.NodesetLast(context);
                    break;

                case XPathFunctionID.LocalName:
                    XPathFunction.NodesetLocalName(context);
                    break;

                case XPathFunctionID.LocalNameDefault:
                    XPathFunction.NodesetLocalNameDefault(context);
                    break;

                case XPathFunctionID.Name:
                    XPathFunction.NodesetName(context);
                    break;

                case XPathFunctionID.NameDefault:
                    XPathFunction.NodesetNameDefault(context);
                    break;

                case XPathFunctionID.NamespaceUri:
                    XPathFunction.NodesetNamespaceUri(context);
                    break;

                case XPathFunctionID.NamespaceUriDefault:
                    XPathFunction.NodesetNamespaceUriDefault(context);
                    break;

                case XPathFunctionID.Boolean:
                    XPathFunction.BooleanBoolean(context);
                    break;

                case XPathFunctionID.False:
                    XPathFunction.BooleanFalse(context);
                    break;

                case XPathFunctionID.True:
                    XPathFunction.BooleanTrue(context);
                    break;

                case XPathFunctionID.Not:
                    XPathFunction.BooleanNot(context);
                    break;

                case XPathFunctionID.Lang:
                    XPathFunction.BooleanLang(context);
                    break;

                case XPathFunctionID.Contains:
                    XPathFunction.StringContains(context);
                    break;

                case XPathFunctionID.Number:
                    XPathFunction.NumberNumber(context);
                    break;

                case XPathFunctionID.NumberDefault:
                    XPathFunction.NumberNumberDefault(context);
                    break;

                case XPathFunctionID.Ceiling:
                    XPathFunction.NumberCeiling(context);
                    break;

                case XPathFunctionID.Floor:
                    XPathFunction.NumberFloor(context);
                    break;

                case XPathFunctionID.Round:
                    XPathFunction.NumberRound(context);
                    break;

                case XPathFunctionID.Sum:
                    XPathFunction.NumberSum(context);
                    break;

                case XPathFunctionID.String:
                    XPathFunction.StringString(context);
                    break;

                case XPathFunctionID.StringDefault:
                    XPathFunction.StringStringDefault(context);
                    break;

                case XPathFunctionID.ConcatTwo:
                    XPathFunction.StringConcatTwo(context);
                    break;

                case XPathFunctionID.ConcatThree:
                    XPathFunction.StringConcatThree(context);
                    break;

                case XPathFunctionID.ConcatFour:
                    XPathFunction.StringConcatFour(context);
                    break;

                case XPathFunctionID.StartsWith:
                    XPathFunction.StringStartsWith(context);
                    break;

                case XPathFunctionID.StringLength:
                    XPathFunction.StringLength(context);
                    break;

                case XPathFunctionID.StringLengthDefault:
                    XPathFunction.StringLengthDefault(context);
                    break;

                case XPathFunctionID.SubstringBefore:
                    XPathFunction.SubstringBefore(context);
                    break;

                case XPathFunctionID.SubstringAfter:
                    XPathFunction.SubstringAfter(context);
                    break;

                case XPathFunctionID.Substring:
                    XPathFunction.Substring(context);
                    break;

                case XPathFunctionID.SubstringLimit:
                    XPathFunction.SubstringLimit(context);
                    break;

                case XPathFunctionID.Translate:
                    XPathFunction.Translate(context);
                    break;

                case XPathFunctionID.NormalizeSpace:
                    XPathFunction.NormalizeSpace(context);
                    break;

                case XPathFunctionID.NormalizeSpaceDefault:
                    XPathFunction.NormalizeSpaceDefault(context);
                    break;
            }
        }

        internal static void BooleanBoolean(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            Value[] values = context.Values;
            while (arg.basePtr <= arg.endPtr)
            {
                values[arg.basePtr++].ConvertTo(context, ValueDataType.Boolean);
            }
        }

        internal static void BooleanFalse(ProcessingContext context)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                context.Push(false, count);
            }
        }

        internal static void BooleanNot(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            Value[] values = context.Values;
            while (arg.basePtr <= arg.endPtr)
            {
                values[arg.basePtr++].Not();
            }
        }

        internal static void BooleanTrue(ProcessingContext context)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                context.Push(true, count);
            }
        }

        internal static void BooleanLang(ProcessingContext context)
        {
            StackFrame langArg = context.TopArg;
            StackFrame sequences = context.TopSequenceArg;
            Value[] sequenceBuffer = context.Sequences;

            while (sequences.basePtr <= sequences.endPtr)
            {
                NodeSequence sourceSeq = sequenceBuffer[sequences.basePtr++].Sequence;

                for (int item = 0; item < sourceSeq.Count; ++item)
                {
                    string lang = context.PeekString(langArg.basePtr).ToUpperInvariant();

                    QueryNode node = sourceSeq.Items[item].Node;
                    long pos = node.Node.CurrentPosition;
                    node.Node.CurrentPosition = node.Position;
                    string docLang = node.Node.XmlLang.ToUpperInvariant();
                    node.Node.CurrentPosition = pos;

                    if (lang.Length == docLang.Length && string.CompareOrdinal(lang, docLang) == 0)
                    {
                        context.SetValue(context, langArg.basePtr++, true);
                    }
                    else if (docLang.Length > 0 && lang.Length < docLang.Length && docLang.StartsWith(lang, StringComparison.Ordinal) && docLang[lang.Length] == '-')
                    {
                        context.SetValue(context, langArg.basePtr++, true);
                    }
                    else
                    {
                        context.SetValue(context, langArg.basePtr++, false);
                    }
                }

                sequences.basePtr++;
            }
        }

        internal static void IterateAndPushSequences(ProcessingContext context)
        {
            StackFrame sequences = context.TopSequenceArg;
            Value[] sequenceBuffer = context.Sequences;

            context.PushFrame();
            while (sequences.basePtr <= sequences.endPtr)
            {
                NodeSequence sourceSeq = sequenceBuffer[sequences.basePtr++].Sequence;
                int count = sourceSeq.Count;
                if (count == 0)
                {
                    context.PushSequence(NodeSequence.Empty);
                }
                else
                {
                    for (int item = 0; item < sourceSeq.Count; ++item)
                    {
                        NodeSequence newSequence = context.CreateSequence();
                        newSequence.StartNodeset();
                        newSequence.Add(ref sourceSeq.Items[item]);
                        newSequence.StopNodeset();
                        context.Push(newSequence);
                    }
                }
            }
        }

        internal static void NodesetCount(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            while (arg.basePtr <= arg.endPtr)
            {
                context.SetValue(context, arg.basePtr, context.PeekSequence(arg.basePtr).Count);
                arg.basePtr++;
            }
        }

        internal static void NodesetLast(ProcessingContext context)
        {
            context.TransferSequenceSize();
        }

        internal static void NodesetLocalName(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;

            while (arg.basePtr <= arg.endPtr)
            {
                NodeSequence sequence = context.PeekSequence(arg.basePtr);
                context.SetValue(context, arg.basePtr, sequence.LocalName);
                arg.basePtr++;
            }
        }

        internal static void NodesetLocalNameDefault(ProcessingContext context)
        {
            XPathFunction.IterateAndPushSequences(context);
            XPathFunction.NodesetLocalName(context);
        }

        internal static void NodesetName(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;

            while (arg.basePtr <= arg.endPtr)
            {
                NodeSequence sequence = context.PeekSequence(arg.basePtr);
                context.SetValue(context, arg.basePtr, sequence.Name);
                arg.basePtr++;
            }
        }

        internal static void NodesetNameDefault(ProcessingContext context)
        {
            XPathFunction.IterateAndPushSequences(context);
            XPathFunction.NodesetName(context);
        }

        internal static void NodesetNamespaceUri(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;

            while (arg.basePtr <= arg.endPtr)
            {
                NodeSequence sequence = context.PeekSequence(arg.basePtr);
                context.SetValue(context, arg.basePtr, sequence.Namespace);
                arg.basePtr++;
            }
        }

        internal static void NodesetNamespaceUriDefault(ProcessingContext context)
        {
            XPathFunction.IterateAndPushSequences(context);
            XPathFunction.NodesetNamespaceUri(context);
        }

        internal static void NodesetPosition(ProcessingContext context)
        {
            context.TransferSequencePositions();
        }

        internal static void NumberCeiling(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;

            while (arg.basePtr <= arg.endPtr)
            {
                context.SetValue(context, arg.basePtr, Math.Ceiling(context.PeekDouble(arg.basePtr)));
                arg.basePtr++;
            }
        }

        internal static void NumberNumber(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            Value[] values = context.Values;

            while (arg.basePtr <= arg.endPtr)
            {
                values[arg.basePtr++].ConvertTo(context, ValueDataType.Double);
            }
        }

        internal static void NumberNumberDefault(ProcessingContext context)
        {
            XPathFunction.IterateAndPushSequences(context);
            XPathFunction.NumberNumber(context);
        }

        internal static void NumberFloor(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;

            while (arg.basePtr <= arg.endPtr)
            {
                context.SetValue(context, arg.basePtr, Math.Floor(context.PeekDouble(arg.basePtr)));
                arg.basePtr++;
            }
        }

        internal static void NumberRound(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;

            while (arg.basePtr <= arg.endPtr)
            {
                double val = context.PeekDouble(arg.basePtr);
                context.SetValue(context, arg.basePtr, QueryValueModel.Round(context.PeekDouble(arg.basePtr)));
                arg.basePtr++;
            }
        }

        internal static void NumberSum(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            while (arg.basePtr <= arg.endPtr)
            {
                NodeSequence sequence = context.PeekSequence(arg.basePtr);
                double sum = 0.0;
                for (int item = 0; item < sequence.Count; ++item)
                {
                    sum += QueryValueModel.Double(sequence[item].StringValue());
                }

                context.SetValue(context, arg.basePtr, sum);
                arg.basePtr++;
            }
        }

        internal static void StringString(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            Value[] values = context.Values;
            while (arg.basePtr <= arg.endPtr)
            {
                values[arg.basePtr++].ConvertTo(context, ValueDataType.String);
            }
        }

        internal static void StringStringDefault(ProcessingContext context)
        {
            XPathFunction.IterateAndPushSequences(context);
            XPathFunction.StringString(context);
        }

        internal static void StringConcatTwo(ProcessingContext context)
        {
            StackFrame arg1 = context[0];
            StackFrame arg2 = context[1];
            while (arg1.basePtr <= arg1.endPtr)
            {
                string str1 = context.PeekString(arg1.basePtr);
                string str2 = context.PeekString(arg2.basePtr);
                context.SetValue(context, arg2.basePtr, str1 + str2);
                arg1.basePtr++;
                arg2.basePtr++;
            }
            context.PopFrame();
        }

        internal static void StringConcatThree(ProcessingContext context)
        {
            StackFrame arg1 = context[0];
            StackFrame arg2 = context[1];
            StackFrame arg3 = context[2];
            while (arg1.basePtr <= arg1.endPtr)
            {
                string str1 = context.PeekString(arg1.basePtr);
                string str2 = context.PeekString(arg2.basePtr);
                string str3 = context.PeekString(arg3.basePtr);
                context.SetValue(context, arg3.basePtr, str1 + str2 + str3);
                arg1.basePtr++;
                arg2.basePtr++;
                arg3.basePtr++;
            }
            context.PopFrame();
            context.PopFrame();
        }

        internal static void StringConcatFour(ProcessingContext context)
        {
            StackFrame arg1 = context[0];
            StackFrame arg2 = context[1];
            StackFrame arg3 = context[2];
            StackFrame arg4 = context[3];
            while (arg1.basePtr <= arg1.endPtr)
            {
                string str1 = context.PeekString(arg1.basePtr);
                string str2 = context.PeekString(arg2.basePtr);
                string str3 = context.PeekString(arg3.basePtr);
                string str4 = context.PeekString(arg4.basePtr);
                context.SetValue(context, arg4.basePtr, str1 + str2 + str3 + str4);
                arg1.basePtr++;
                arg2.basePtr++;
                arg3.basePtr++;
                arg4.basePtr++;
            }
            context.PopFrame();
            context.PopFrame();
            context.PopFrame();
        }

        internal static void StringContains(ProcessingContext context)
        {
            StackFrame arg1 = context.TopArg;
            StackFrame arg2 = context.SecondArg;
            Fx.Assert(arg1.Count == arg2.Count, "");

            while (arg1.basePtr <= arg1.endPtr)
            {
                string leftString = context.PeekString(arg1.basePtr);
                string rightString = context.PeekString(arg2.basePtr);
                context.SetValue(context, arg2.basePtr, (-1 != leftString.IndexOf(rightString, StringComparison.Ordinal)));
                arg1.basePtr++;
                arg2.basePtr++;
            }
            context.PopFrame();
        }

        internal static void StringLength(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            while (arg.basePtr <= arg.endPtr)
            {
                context.SetValue(context, arg.basePtr, context.PeekString(arg.basePtr).Length);
                arg.basePtr++;
            }
        }

        internal static void StringLengthDefault(ProcessingContext context)
        {
            XPathFunction.IterateAndPushSequences(context);
            XPathFunction.ConvertFirstArg(context, ValueDataType.String);
            XPathFunction.StringLength(context);
        }

        internal static void StringStartsWith(ProcessingContext context)
        {
            StackFrame arg1 = context.TopArg;
            StackFrame arg2 = context.SecondArg;

            Fx.Assert(arg1.Count == arg2.Count, "");
            while (arg1.basePtr <= arg1.endPtr)
            {
                string leftString = context.PeekString(arg1.basePtr);
                string rightString = context.PeekString(arg2.basePtr);
                context.SetValue(context, arg2.basePtr, leftString.StartsWith(rightString, StringComparison.Ordinal));
                arg1.basePtr++;
                arg2.basePtr++;
            }
            context.PopFrame();
        }

        internal static void SubstringBefore(ProcessingContext context)
        {
            StackFrame arg1 = context.TopArg;
            StackFrame arg2 = context.SecondArg;

            Fx.Assert(arg1.Count == arg2.Count, "");
            while (arg1.basePtr <= arg1.endPtr)
            {
                string str1 = context.PeekString(arg1.basePtr);
                string str2 = context.PeekString(arg2.basePtr);
                int idx = str1.IndexOf(str2, StringComparison.Ordinal);
                context.SetValue(context, arg2.basePtr, idx == -1 ? string.Empty : str1.Substring(0, idx));
                arg1.basePtr++;
                arg2.basePtr++;
            }
            context.PopFrame();
        }

        internal static void SubstringAfter(ProcessingContext context)
        {
            StackFrame arg1 = context.TopArg;
            StackFrame arg2 = context.SecondArg;

            Fx.Assert(arg1.Count == arg2.Count, "");
            while (arg1.basePtr <= arg1.endPtr)
            {
                string str1 = context.PeekString(arg1.basePtr);
                string str2 = context.PeekString(arg2.basePtr);
                int idx = str1.IndexOf(str2, StringComparison.Ordinal);
                context.SetValue(context, arg2.basePtr, idx == -1 ? string.Empty : str1.Substring(idx + str2.Length));
                arg1.basePtr++;
                arg2.basePtr++;
            }
            context.PopFrame();
        }

        internal static void Substring(ProcessingContext context)
        {
            StackFrame arg1 = context.TopArg;
            StackFrame arg2 = context.SecondArg;

            Fx.Assert(arg1.Count == arg2.Count, "");
            while (arg1.basePtr <= arg1.endPtr)
            {
                string str = context.PeekString(arg1.basePtr);
                int startAt = ((int)Math.Round(context.PeekDouble(arg2.basePtr))) - 1;
                if (startAt < 0)
                {
                    startAt = 0;
                }
                context.SetValue(context, arg2.basePtr, (startAt >= str.Length) ? string.Empty : str.Substring(startAt));
                arg1.basePtr++;
                arg2.basePtr++;
            }
            context.PopFrame();
        }

        internal static void SubstringLimit(ProcessingContext context)
        {
            StackFrame argString = context.TopArg;
            StackFrame argStartAt = context.SecondArg;
            StackFrame argLimit = context[2];

            Fx.Assert(argString.Count == argStartAt.Count, "");
            Fx.Assert(argString.Count == argLimit.Count, "");

            while (argString.basePtr <= argString.endPtr)
            {
                string str = context.PeekString(argString.basePtr);
                int startAt = ((int)Math.Round(context.PeekDouble(argStartAt.basePtr))) - 1;
                if (startAt < 0)
                {
                    startAt = 0;
                }
                int length = (int)Math.Round(context.PeekDouble(argLimit.basePtr));

                string substr;
                if (length < 1 || ((startAt + length) >= str.Length))
                {
                    substr = string.Empty;
                }
                else
                {
                    substr = str.Substring(startAt, length);
                }
                context.SetValue(context, argLimit.basePtr, substr);
                argStartAt.basePtr++;
                argString.basePtr++;
                argLimit.basePtr++;
            }

            context.PopFrame();
            context.PopFrame();
        }

        internal static void Translate(ProcessingContext context)
        {
            StackFrame argSource = context.TopArg;
            StackFrame argKeys = context.SecondArg;
            StackFrame argValues = context[2];

            // PERF, [....], this is really slow.
            StringBuilder builder = new StringBuilder();
            while (argSource.basePtr <= argSource.endPtr)
            {
                builder.Length = 0;

                string source = context.PeekString(argSource.basePtr);
                string keys = context.PeekString(argKeys.basePtr);
                string values = context.PeekString(argValues.basePtr);
                for (int i = 0; i < source.Length; ++i)
                {
                    char c = source[i];
                    int idx = keys.IndexOf(c);
                    if (idx < 0)
                    {
                        builder.Append(c);
                    }
                    else if (idx < values.Length)
                    {
                        builder.Append(values[idx]);
                    }
                }
                context.SetValue(context, argValues.basePtr, builder.ToString());
                argSource.basePtr++;
                argKeys.basePtr++;
                argValues.basePtr++;
            }

            context.PopFrame();
            context.PopFrame();
        }

        internal static void NormalizeSpace(ProcessingContext context)
        {
            StackFrame argStr = context.TopArg;

            StringBuilder builder = new StringBuilder();
            while (argStr.basePtr <= argStr.endPtr)
            {
                char[] whitespace = new char[] { ' ', '\t', '\r', '\n' };
                string str = context.PeekString(argStr.basePtr).Trim(whitespace);

                bool eatingWhitespace = false;
                builder.Length = 0;
                for (int i = 0; i < str.Length; ++i)
                {
                    char c = str[i];
                    if (XPathCharTypes.IsWhitespace(c))
                    {
                        if (!eatingWhitespace)
                        {
                            builder.Append(' ');
                            eatingWhitespace = true;
                        }
                    }
                    else
                    {
                        builder.Append(c);
                        eatingWhitespace = false;
                    }
                }

                context.SetValue(context, argStr.basePtr, builder.ToString());
                argStr.basePtr++;
            }
        }

        internal static void NormalizeSpaceDefault(ProcessingContext context)
        {
            XPathFunction.IterateAndPushSequences(context);
            XPathFunction.ConvertFirstArg(context, ValueDataType.String);
            XPathFunction.NormalizeSpace(context);
        }
#if NO
        internal static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\r' || c == '\n' || c == '\t';
        }
#endif
    }
}
