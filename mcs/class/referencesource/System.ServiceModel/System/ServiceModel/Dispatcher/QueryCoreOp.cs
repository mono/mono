//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    class PushContextNodeOpcode : Opcode
    {
        internal PushContextNodeOpcode()
            : base(OpcodeID.PushContextNode)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushContextSequenceFrame();
            NodeSequence seq = context.CreateSequence();

            seq.StartNodeset();
            seq.Add(context.Processor.ContextNode);
            seq.StopNodeset();

            context.PushSequence(seq);

            return this.next;
        }
    }

    class PushContextPositionOpcode : Opcode
    {
        internal PushContextPositionOpcode()
            : base(OpcodeID.PushPosition)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.TransferSequencePositions();
            return this.next;
        }
    }

    class PopSequenceToValueStackOpcode : Opcode
    {
        internal PopSequenceToValueStackOpcode()
            : base(OpcodeID.PopSequenceToValueStack)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PopSequenceFrameToValueStack();
            return this.next;
        }
    }

    class PopSequenceToSequenceStackOpcode : Opcode
    {
        internal PopSequenceToSequenceStackOpcode()
            : base(OpcodeID.PopSequenceToSequenceStack)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushSequenceFrameFromValueStack();
            return this.next;
        }
    }

#if NO
    internal class PushContextCopy : Opcode
    {
        internal PushContextCopy()
            : base(OpcodeID.PushContextCopy)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            NodeSequenceStack stack = context.SequenceStack;
            StackFrame sequences = stack.TopArg;

            stack.PushFrame();
            for (int i = 0; i < sequences.count; ++i)
            {
                NodeSequence sourceSeq = stack.Sequences[sequences[i]];
                sourceSeq.refCount++;
                stack.Push(sourceSeq);
            }

            return this.next;
        }
    }
#endif

    class PopContextNodes : Opcode
    {
        internal PopContextNodes()
            : base(OpcodeID.PopContextNodes)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PopContextSequenceFrame();
            return this.next;
        }
    }

#if NO
    internal class PopValueFrameOpcode : Opcode
    {
        internal PopValueFrameOpcode()
            : base(OpcodeID.PopValueFrame)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PopFrame();
            return this.next;
        }
    }
#endif

    class PushStringOpcode : Opcode
    {
        string literal;

        internal PushStringOpcode(string literal)
            : base(OpcodeID.PushString)
        {
            Fx.Assert(null != literal, "");
            this.literal = literal;
            this.flags |= OpcodeFlags.Literal;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return (this.literal == ((PushStringOpcode)op).literal);
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                context.Push(this.literal, count);
            }
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.literal);
        }
#endif
    }

    class PushNumberOpcode : Opcode
    {
        double literal;

        internal PushNumberOpcode(double literal)
            : base(OpcodeID.PushDouble)
        {
            this.literal = literal;
            this.flags |= OpcodeFlags.Literal;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return (this.literal == ((PushNumberOpcode)op).literal);
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                context.Push(this.literal, count);
            }
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.literal);
        }
#endif
    }

    class PushBooleanOpcode : Opcode
    {
        bool literal;

        internal PushBooleanOpcode(bool literal)
            : base(OpcodeID.PushBool)
        {
            this.literal = literal;
            this.flags |= OpcodeFlags.Literal;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return (this.literal == ((PushBooleanOpcode)op).literal);
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                context.Push(this.literal, count);
            }
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.literal);
        }
#endif
    }

    class PushXsltVariableOpcode : Opcode
    {
        XsltContext xsltContext;
        IXsltContextVariable variable;
        ValueDataType type;

        internal PushXsltVariableOpcode(XsltContext context, IXsltContextVariable variable)
            : base(OpcodeID.PushXsltVariable)
        {
            Fx.Assert(null != context && null != variable, "");

            this.xsltContext = context;
            this.variable = variable;
            this.type = XPathXsltFunctionExpr.ConvertTypeFromXslt(variable.VariableType);

            // Make sure the type is supported
            switch (this.type)
            {
                case ValueDataType.Boolean:
                case ValueDataType.Double:
                case ValueDataType.String:
                case ValueDataType.Sequence:
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidType, SR.GetString(SR.QueryVariableTypeNotSupported, this.variable.VariableType.ToString())));
            }
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                PushXsltVariableOpcode var = op as PushXsltVariableOpcode;
                if (var != null)
                {
                    return this.xsltContext == var.xsltContext && this.variable == var.variable;
                }
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushFrame();
            int count = context.IterationCount;
            if (count > 0)
            {
                object o = this.variable.Evaluate(this.xsltContext);
                if (o == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryVariableNull)));
                }

                switch (this.type)
                {
                    case ValueDataType.Boolean:
                        context.Push((bool)o, count);
                        break;

                    case ValueDataType.Double:
                        context.Push((double)o, count);
                        break;

                    case ValueDataType.String:
                        context.Push((string)o, count);
                        break;

                    case ValueDataType.Sequence:
                        XPathNodeIterator iter = (XPathNodeIterator)o;
                        NodeSequence seq = context.CreateSequence();
                        while (iter.MoveNext())
                        {
                            SeekableXPathNavigator nav = iter.Current as SeekableXPathNavigator;
                            if (nav != null)
                            {
                                seq.Add(nav);
                            }
                            else
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryMustBeSeekable)));
                            }
                        }
                        context.Push(seq, count);
                        break;

                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryVariableTypeNotSupported, this.variable.VariableType.ToString())));
                }
            }
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} IXsltContextVariable: {1}", base.ToString(), this.variable.ToString());
        }
#endif
    }
}
