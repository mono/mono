//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;

    // Base class for opcodes that produce the query's final result
    abstract class ResultOpcode : Opcode
    {
        internal ResultOpcode(OpcodeID id)
            : base(id)
        {
            this.flags |= OpcodeFlags.Result;
        }
    }    
        
    internal class MatchResultOpcode : ResultOpcode
    {
        internal MatchResultOpcode() 
            : base(OpcodeID.MatchResult)
        {           
        }       

        internal override Opcode Eval(ProcessingContext context)
        {
            context.Processor.Result = this.IsSuccess(context);
            context.PopFrame();
            return this.next;       
        }

        protected bool IsSuccess(ProcessingContext context)
        {
            StackFrame topFrame = context.TopArg;

            if (1 == topFrame.Count)
            {
                return context.Values[topFrame.basePtr].ToBoolean();
            }
            else
            {
                context.Processor.Result = false;
                for (int i = topFrame.basePtr; i <= topFrame.endPtr; ++i)
                {
                    if (context.Values[i].ToBoolean())
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }

    internal class QueryResultOpcode : ResultOpcode
    {
        internal QueryResultOpcode()
            : base(OpcodeID.QueryResult)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topFrame = context.TopArg;
            ValueDataType resultType = context.Values[topFrame.basePtr].Type;
            XPathResult result;

            switch (resultType)
            {
                case ValueDataType.Sequence:
                    {
                        SafeNodeSequenceIterator value = new SafeNodeSequenceIterator(context.Values[topFrame.basePtr].GetSequence(), context);
                        result = new XPathResult(value);
                    }
                    break;
                case ValueDataType.Boolean:
                    {
                        bool value = context.Values[topFrame.basePtr].GetBoolean();
                        result = new XPathResult(value);
                    }
                    break;
                case ValueDataType.String:
                    {
                        string value = context.Values[topFrame.basePtr].GetString();
                        result = new XPathResult(value);
                    }
                    break;
                case ValueDataType.Double:
                    {
                        double value = context.Values[topFrame.basePtr].GetDouble();
                        result = new XPathResult(value);
                    }
                    break;
                default:
                    throw Fx.AssertAndThrow("Unexpected result type.");
            }

            context.Processor.QueryResult = result;
            context.PopFrame();
            return this.next;       
        }
    }

    internal abstract class MultipleResultOpcode : ResultOpcode
    {
        protected QueryBuffer<object> results;

        internal MultipleResultOpcode(OpcodeID id)
            : base(id)
        {
            this.flags |= OpcodeFlags.Multiple;
            this.results = new QueryBuffer<object>(1);
        }

        internal override void Add(Opcode op)
        {
            MultipleResultOpcode results = op as MultipleResultOpcode;
            if (null != results)
            {
                this.results.Add(ref results.results);
                this.results.TrimToCount();
                return;
            }

            base.Add(op);
        }      

        public void AddItem(object item)
        {
            this.results.Add(item);
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            for (int i = 0; i < this.results.Count; ++i)
            {                
                XPathMessageFilter filter = this.results[i] as XPathMessageFilter;

                if (filter != null)
                {
                    filters.Add(filter);
                }
            }
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return object.ReferenceEquals(this, op);
            }

            return false;
        }

        public void RemoveItem(object item)
        {
            this.results.Remove(item);
            this.Remove();
        }

        internal override void Remove()
        {
            if (0 == this.results.Count)
            {
                base.Remove();
            }
        }

        internal override void Trim()
        {
            this.results.TrimToCount();
        }
    }

    internal class QueryMultipleResultOpcode : MultipleResultOpcode
    {
        internal QueryMultipleResultOpcode() : base(OpcodeID.QueryMultipleResult) { }

        internal override Opcode Eval(ProcessingContext context)
        {
            Fx.Assert(this.results.Count > 0, "QueryMultipleQueryResultOpcode in the eval tree but no query present");
            Fx.Assert(context.Processor.ResultSet != null, "QueryMultipleQueryResultOpcode should only be used in eval cases");

            StackFrame topFrame = context.TopArg;
            ValueDataType resultType = context.Values[topFrame.basePtr].Type;
            XPathResult result;

            switch (resultType)
            {
                case ValueDataType.Sequence:
                    {
                        SafeNodeSequenceIterator value = new SafeNodeSequenceIterator(context.Values[topFrame.basePtr].GetSequence(), context);
                        result = new XPathResult(value);
                    }
                    break;
                case ValueDataType.Boolean:
                    {
                        bool value = context.Values[topFrame.basePtr].GetBoolean();
                        result = new XPathResult(value);
                    }
                    break;
                case ValueDataType.String:
                    {
                        string value = context.Values[topFrame.basePtr].GetString();
                        result = new XPathResult(value);
                    }
                    break;
                case ValueDataType.Double:
                    {
                        double value = context.Values[topFrame.basePtr].GetDouble();
                        result = new XPathResult(value);
                    }
                    break;
                default:
                    throw Fx.AssertAndThrow("Unexpected result type.");
            }

            context.Processor.ResultSet.Add(new KeyValuePair<MessageQuery, XPathResult>((MessageQuery)this.results[0], result));

            for (int i = 1; i < this.results.Count; i++)
            {
                context.Processor.ResultSet.Add(new KeyValuePair<MessageQuery, XPathResult>((MessageQuery)this.results[i], result.Copy()));
            }

            context.PopFrame();
            return this.next;
        }
    }

    internal class MatchMultipleResultOpcode : MultipleResultOpcode
    {
        internal MatchMultipleResultOpcode() : base(OpcodeID.MatchMultipleResult) { }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topFrame = context.TopArg;
            bool match = false;

            if (1 == topFrame.Count)
            {
                match = context.Values[topFrame.basePtr].ToBoolean();
            }
            else
            {
                context.Processor.Result = false;
                for (int i = topFrame.basePtr; i <= topFrame.endPtr; ++i)
                {
                    if (context.Values[i].ToBoolean())
                    {
                        match = true;
                        break;
                    }
                }
            }

            if (match)
            {
                ICollection<MessageFilter> matches = context.Processor.MatchSet;

                for (int i = 0, count = this.results.Count; i < count; ++i)
                {
                    matches.Add((MessageFilter)this.results[i]);
                }
            }

            context.PopFrame();
            return this.next;
        }

       
    }        
}
