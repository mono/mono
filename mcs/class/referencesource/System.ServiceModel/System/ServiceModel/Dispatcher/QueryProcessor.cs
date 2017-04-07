//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    class ProcessingContext
    {
        internal ProcessingContext next; // for chaining together in free lists
        int nodeCount;
        QueryProcessor processor;
        EvalStack sequenceStack;
        EvalStack valueStack;

        internal ProcessingContext()
        {
            this.valueStack = new EvalStack(2, 4);
            this.sequenceStack = new EvalStack(1, 2);
            this.nodeCount = -1;
        }
#if NO
        internal int FrameCount
        {
            get
            {
                return this.valueStack.FrameCount;
            }
        }

        internal int FramePtr
        {
            get
            {
                return this.valueStack.FramePtr;
            }
        }
#endif
        internal StackFrame this[int frameIndex]
        {
            get
            {
                return this.valueStack[frameIndex];
            }
        }

        internal int IterationCount
        {
            get
            {
                if (-1 == this.nodeCount)
                {
                    this.nodeCount = this.sequenceStack.CalculateNodecount();
                    if (this.nodeCount == 0 && !this.sequenceStack.InUse)
                    {
                        this.nodeCount = 1;
                    }
                }

                return this.nodeCount;
            }
        }

        internal int NodeCount
        {
            get
            {
                return this.nodeCount;
            }
            set
            {
                this.nodeCount = value;
            }
        }

        internal ProcessingContext Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        internal QueryProcessor Processor
        {
            get
            {
                return this.processor;
            }
            set
            {
                this.processor = value;
            }
        }

        internal StackFrame SecondArg
        {
            get
            {
                return this.valueStack.SecondArg;
            }
        }

        internal Value[] Sequences
        {
            get
            {
                return this.sequenceStack.Buffer;
            }
        }

        internal bool SequenceStackInUse
        {
            get
            {
                return this.sequenceStack.InUse;
            }
        }

        internal bool StacksInUse
        {
            get
            {
                return (this.valueStack.frames.Count > 0 || this.sequenceStack.frames.Count > 0);
            }
        }

        internal StackFrame TopArg
        {
            get
            {
                return this.valueStack.TopArg;
            }
        }

        internal StackFrame TopSequenceArg
        {
            get
            {
                return this.sequenceStack.TopArg;
            }
        }

        internal Value[] Values
        {
            get
            {
                return this.valueStack.Buffer;
            }
        }

        internal ProcessingContext Clone()
        {
            return this.processor.CloneContext(this);
        }

        internal void ClearContext()
        {
            this.sequenceStack.Clear();
            this.valueStack.Clear();
            this.nodeCount = -1;
        }

        internal void CopyFrom(ProcessingContext context)
        {
            Fx.Assert(null != context, "");

            this.processor = context.processor;
            if (context.sequenceStack.frames.Count > 0)
            {
                this.sequenceStack.CopyFrom(ref context.sequenceStack);
            }
            else
            {
                this.sequenceStack.Clear();
            }
            if (context.valueStack.frames.Count > 0)
            {
                this.valueStack.CopyFrom(ref context.valueStack);
            }
            else
            {
                this.valueStack.Clear();
            }
            this.nodeCount = context.nodeCount;
        }

        internal NodeSequence CreateSequence()
        {
            NodeSequence sequence = this.processor.PopSequence();
            if (null == sequence)
            {
                sequence = new NodeSequence();
            }
            sequence.OwnerContext = this;
            sequence.refCount++;

            return sequence;
        }

        internal bool LoadVariable(int var)
        {
            return this.Processor.LoadVariable(this, var);
        }

        internal void EvalCodeBlock(Opcode block)
        {
            this.processor.Eval(block, this);
        }

        internal bool PeekBoolean(int index)
        {
            return this.valueStack.PeekBoolean(index);
        }

        internal double PeekDouble(int index)
        {
            return this.valueStack.PeekDouble(index);
        }
#if NO
        internal int PeekInteger(int index)
        {
            return (int) this.valueStack.PeekInteger(index);
        }
#endif
        internal NodeSequence PeekSequence(int index)
        {
            return this.valueStack.PeekSequence(index);
        }

        internal string PeekString(int index)
        {
            return this.valueStack.PeekString(index);
        }

        internal void PopFrame()
        {
            this.valueStack.PopFrame(this);
        }

        internal void PopSequenceFrame()
        {
            this.sequenceStack.PopFrame(this);
            this.nodeCount = -1;
        }

        internal void PopContextSequenceFrame()
        {
            PopSequenceFrame();
            if (!this.sequenceStack.InUse)
                this.sequenceStack.contextOnTopOfStack = false;
        }

        internal void Push(bool boolVal)
        {
            this.valueStack.Push(boolVal);
        }

        internal void Push(bool boolVal, int addCount)
        {
            this.valueStack.Push(boolVal, addCount);
        }

#if NO
        internal void Push(double doubleVal)
        {
            this.valueStack.Push(doubleVal);
        }
#endif
        internal void Push(double doubleVal, int addCount)
        {
            this.valueStack.Push(doubleVal, addCount);
        }

        internal void Push(NodeSequence sequence)
        {
            this.valueStack.Push(sequence);
        }

        internal void Push(NodeSequence sequence, int addCount)
        {
            this.valueStack.Push(sequence, addCount);
        }

        internal void Push(string stringVal)
        {
            this.valueStack.Push(stringVal);
        }

        internal void Push(string stringVal, int addCount)
        {
            this.valueStack.Push(stringVal, addCount);
        }

        internal void PushFrame()
        {
            this.valueStack.PushFrame();
        }

        internal void PopSequenceFrameToValueStack()
        {
            this.sequenceStack.PopSequenceFrameTo(ref this.valueStack);
            this.nodeCount = -1;
        }

        internal void PushSequence(NodeSequence seq)
        {
            this.sequenceStack.Push(seq);
            this.nodeCount = -1;
        }

        internal void PushSequenceFrame()
        {
            this.sequenceStack.PushFrame();
            this.nodeCount = -1;
        }

        internal void PushContextSequenceFrame()
        {
            if (!this.sequenceStack.InUse)
                this.sequenceStack.contextOnTopOfStack = true;
            PushSequenceFrame();
        }

        internal void PushSequenceFrameFromValueStack()
        {
            this.valueStack.PopSequenceFrameTo(ref this.sequenceStack);
            this.nodeCount = -1;
        }

        internal void ReleaseSequence(NodeSequence sequence)
        {
            Fx.Assert(null != sequence, "");
            if (this == sequence.OwnerContext)
            {
                sequence.refCount--;
                Fx.Assert(sequence.refCount >= 0, "");
                if (0 == sequence.refCount)
                {
                    this.processor.ReleaseSequenceToPool(sequence);
                }
            }
        }

        internal void Release()
        {
            this.processor.ReleaseContext(this);
        }

        internal void ReplaceSequenceAt(int index, NodeSequence sequence)
        {
            this.sequenceStack.ReplaceAt(index, sequence);
            this.nodeCount = -1;
        }

        internal void SaveVariable(int var, int count)
        {
            this.Processor.SaveVariable(this, var, count);
        }

        internal void SetValue(ProcessingContext context, int index, bool val)
        {
            this.valueStack.SetValue(this, index, val);
        }

        internal void SetValue(ProcessingContext context, int index, double val)
        {
            this.valueStack.SetValue(this, index, val);
        }

        internal void SetValue(ProcessingContext context, int index, string val)
        {
            this.valueStack.SetValue(this, index, val);
        }

        internal void SetValue(ProcessingContext context, int index, NodeSequence val)
        {
            this.valueStack.SetValue(this, index, val);
        }

        internal void TransferSequenceSize()
        {
            this.sequenceStack.TransferSequenceSizeTo(ref this.valueStack);
        }

        internal void TransferSequencePositions()
        {
            this.sequenceStack.TransferPositionsTo(ref this.valueStack);
        }

        #region IQueryBufferPool Members
#if NO
        public virtual void Reset()
        {
            this.valueStack.Clear();
            this.valueStack.Trim();
            this.sequenceStack.Clear();
            this.sequenceStack.Trim();
        }

        public virtual void Trim()
        {
            this.valueStack.Trim();
            this.sequenceStack.Trim();
        }
#endif
        #endregion
    }

    internal enum QueryProcessingFlags : byte
    {
        None = 0x00,
        Match = 0x01,
        Message = 0x02,
        //Select      = 0x04,
    }

    internal class QueryProcessor : ProcessingContext
    {
        SeekableXPathNavigator contextNode; // original context node off which everything started
        ProcessingContext contextPool;
        INodeCounter counter;
        QueryProcessingFlags flags;
        QueryMatcher matcher;
        Message message;
        bool matchMessageBody;
        int refCount;
        bool result; // for singleton matches...
        XPathResult queryResult; // for singleton queries...
        QueryBranchResultSet resultPool;
        Collection<MessageFilter> matchList;
        ICollection<MessageFilter> matchSet; // for inverse queries that produce multiple matches
        ICollection<KeyValuePair<MessageQuery, XPathResult>> resultSet;  // for inverse queries that produce multiple query results
        NodeSequence sequencePool;
        //string selectResults;
        SubExprVariable[] subExprVars;
        string messageAction;
        //string messageAddress;
        //string messageVia;
        string messageId;
        string messageSoapUri;
        string messageTo;

        internal QueryProcessor(QueryMatcher matcher)
            : base()
        {
            this.Processor = this;
            this.matcher = matcher;
            this.flags = QueryProcessingFlags.Match;

            // PERF, Microsoft, see if we can just let these to their default init
            this.messageAction = null;
            //this.messageAddress = null;
            //this.messageVia = null;
            this.messageId = null;
            this.messageSoapUri = null;
            this.messageTo = null;

            if (matcher.SubExprVarCount > 0)
            {
                this.subExprVars = new SubExprVariable[matcher.SubExprVarCount];
            }
        }


        internal string Action
        {
            get
            {
                return this.messageAction;
            }
            set
            {
                this.messageAction = value;
            }
        }
#if NO 
        internal string Address
        {
            get
            {
                return this.messageAddress;
            }
            set
            {
                this.messageAddress = value;
            }
        }
#endif
        // IMPORTANT: Either ContextNode.get or CounterMarker.get MUST be called before this.counter
        //            can be considered valid.
        internal SeekableXPathNavigator ContextNode
        {
            get
            {
                if (null == this.contextNode)
                {
                    if (null != this.message)
                    {
                        this.contextNode = this.matcher.CreateMessageNavigator(this.message, this.matchMessageBody);
                    }
                    else
                    {
#pragma warning suppress 56503 // Microsoft, property is more readable for this
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected));
                    }
                    this.counter = this.contextNode as INodeCounter;
                    if (null == this.counter)
                    {
                        this.counter = DummyNodeCounter.Dummy;
                    }
                }
                return this.contextNode;
            }
            set
            {
                this.contextNode = value;
                this.counter = value as INodeCounter;
            }
        }

        internal Message ContextMessage
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = value;
                if (null != value)
                {
                    this.flags |= QueryProcessingFlags.Message;
                }
                else
                {
                    this.flags &= ~QueryProcessingFlags.Message;
                }
            }
        }

        // IMPORTANT: Either ContextNode.get or CounterMarker.get MUST be called before this.counter
        //            can be considered valid.
        internal int CounterMarker
        {
            get
            {
                if (this.counter == null)
                {
                    this.counter = this.ContextNode as INodeCounter;
                    if (this.counter == null)
                        this.counter = DummyNodeCounter.Dummy;
                }
                return this.counter.CounterMarker;
            }
            set
            {
                this.counter.CounterMarker = value;
            }
        }

#if NO
        internal QueryProcessingFlags Flags
        {
            get
            {
                return this.flags;
            }
        }
        
        internal bool HasContextNode
        {
            get
            {
                return (null != this.contextNode);
            }
        }
#endif

        internal bool MatchBody
        {
#if NO
            get
            {
                return this.matchMessageBody;
            }
#endif
            set
            {
                this.matchMessageBody = value;
            }
        }

        internal QueryMatcher Matcher
        {
            get
            {
                return this.matcher;
            }
        }

        internal ICollection<KeyValuePair<MessageQuery, XPathResult>> ResultSet
        {
            get
            {
                return this.resultSet;
            }
            set
            {
                this.resultSet = value;
            }
        }

        internal string MessageId
        {
            get
            {
                return this.messageId;
            }
            set
            {
                this.messageId = value;
            }
        }

        internal bool Result
        {
            get
            {
                return this.result;
            }
            set
            {
                this.result = value;
            }
        }

        internal XPathResult QueryResult
        {
            get
            {
                return this.queryResult;
            }
            set
            {
                this.queryResult = value;
            }
        }

        internal Collection<MessageFilter> MatchList
        {
            get
            {
                return this.matchList;
            }
        }

        internal ICollection<MessageFilter> MatchSet
        {
            get
            {
                return this.matchSet;
            }
            set
            {
                this.matchSet = value;
            }
        }

        internal string SoapUri
        {
            get
            {
                return this.messageSoapUri;
            }
            set
            {
                this.messageSoapUri = value;
            }
        }

        internal string ToHeader
        {
            get
            {
                return this.messageTo;
            }
            set
            {
                this.messageTo = value;
            }
        }
#if NO 
        internal string Via
        {
            get
            {
                return this.messageVia;
            }
            set
            {
                this.messageVia = value;
            }
        }
#endif
        internal void AddRef()
        {
            Interlocked.Increment(ref this.refCount);
        }

        internal void ClearProcessor()
        {
            base.ClearContext();

            this.flags = QueryProcessingFlags.Match;

            this.messageAction = null;
            //this.messageAddress = null;
            //this.messageVia = null;
            this.messageId = null;
            this.messageSoapUri = null;
            this.messageTo = null;

            int exprCount = this.matcher.SubExprVarCount;
            if (exprCount == 0)
            {
                // No vars. Recycle entire subexpression cache
                this.subExprVars = null;
                return;
            }

            SubExprVariable[] vars = this.subExprVars; // save locally for speed
            if (vars == null)
            {
                // Allocate space for sub-expressions
                this.subExprVars = new SubExprVariable[exprCount];
                return;
            }

            int varCount = vars.Length;
            // The # of subexpressions changed since this processor was last used.
            if (varCount != exprCount)
            {
                this.subExprVars = new SubExprVariable[exprCount];
                return;
            }

            if (varCount == 1)
            {
                NodeSequence seq = vars[0].seq;
                if (seq != null)
                {
                    this.ReleaseSequenceToPool(seq);
                }
                return;
            }

            // We can reuse the sub-expression cache
            // Clear out the sub-expression results from an earlier run, and return sequences to pool
            for (int i = 0; i < varCount; ++i)
            {
                NodeSequence seq = vars[i].seq;
                if (seq != null && seq.refCount > 0)
                {
                    this.ReleaseSequenceToPool(seq);
                }
            }
            Array.Clear(vars, 0, vars.Length);
        }

        internal ProcessingContext CloneContext(ProcessingContext srcContext)
        {
            ProcessingContext context = this.PopContext();
            if (null == context)
            {
                context = new ProcessingContext();
            }
            context.CopyFrom(srcContext);

            return context;
        }

        internal QueryBranchResultSet CreateResultSet()
        {
            QueryBranchResultSet resultSet = this.PopResultSet();
            if (null == resultSet)
            {
                resultSet = new QueryBranchResultSet();
            }
            else
            {
                resultSet.Clear();
            }
            return resultSet;
        }

        internal int ElapsedCount(int marker)
        {
            return this.counter.ElapsedCount(marker);
        }

        internal void EnsureFilterCollection()
        {
            this.resultSet = null;

            if (null == this.matchSet)
            {
                if (null == this.matchList)
                {
                    this.matchList = new Collection<MessageFilter>();
                }
                else
                {
                    this.matchList.Clear();
                }
                this.matchSet = this.matchList;
            }
        }

        internal void Eval(Opcode block)
        {
            Opcode op = block;
            try
            {
                // Walk over and evaulate the entire trace
                while (null != op)
                {
                    op = op.Eval(this);
                }
            }
            catch (XPathNavigatorException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(op));
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(op));
            }
        }

        internal void Eval(Opcode block, ProcessingContext context)
        {
            Opcode op = block;
            try
            {
                // Walk over and evaulate the entire trace
                while (null != op)
                {
                    op = op.Eval(context);
                }
            }
            catch (XPathNavigatorException e)
            {
                throw TraceUtility.ThrowHelperError(e.Process(op), this.message);
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw TraceUtility.ThrowHelperError(e.Process(op), this.message);
            }
        }

        //
        // Set up the query processor to match messages
        //
        internal void Eval(Opcode block, Message message, bool matchBody)
        {
            this.result = false;
            this.ContextNode = null;
            this.ContextMessage = message;
            this.MatchBody = matchBody;
            this.Eval(block);
            this.message = null;
            this.contextNode = null;
        }

        internal void Eval(Opcode block, SeekableXPathNavigator navigator)
        {
            this.result = false;
            this.ContextNode = navigator;
            this.ContextMessage = null;
            this.Eval(block);
        }

        internal bool LoadVariable(ProcessingContext context, int var)
        {
            if (this.subExprVars[var].seq == null)
            {
                return false;
            }

            int iter = context.IterationCount;
            this.counter.IncreaseBy(iter * this.subExprVars[var].count);

            NodeSequence seq = this.subExprVars[var].seq;
            context.PushSequenceFrame();
            for (int i = 0; i < iter; ++i)
            {
                seq.refCount++;
                context.PushSequence(seq);
            }

            return true;
        }

        internal ProcessingContext PopContext()
        {
            ProcessingContext context = this.contextPool;
            if (null != context)
            {
                this.contextPool = context.Next;
                context.Next = null;
            }
            return context;
        }

        internal NodeSequence PopSequence()
        {
            NodeSequence sequence = this.sequencePool;
            if (null != sequence)
            {
                this.sequencePool = sequence.Next;
                sequence.Next = null;
            }
            return sequence;
        }

        internal QueryBranchResultSet PopResultSet()
        {
            QueryBranchResultSet resultSet = this.resultPool;
            if (null != resultSet)
            {
                this.resultPool = resultSet.Next;
                resultSet.Next = null;
            }
            return resultSet;
        }

        internal void PushContext(ProcessingContext context)
        {
            Fx.Assert(null != context, "");

            context.Next = this.contextPool;
            this.contextPool = context;
        }

        internal void PushResultSet(QueryBranchResultSet resultSet)
        {
            Fx.Assert(null != resultSet, "");

            resultSet.Next = this.resultPool;
            this.resultPool = resultSet;
        }

        internal bool ReleaseRef()
        {
            return (Interlocked.Decrement(ref this.refCount) == 0);
        }

        internal void ReleaseContext(ProcessingContext context)
        {
            Fx.Assert(null != context, "");
            this.PushContext(context);
        }

        internal void ReleaseResults(QueryBranchResultSet resultSet)
        {
            Fx.Assert(null != resultSet, "");
            this.PushResultSet(resultSet);
        }

        internal void ReleaseSequenceToPool(NodeSequence sequence)
        {
            if (NodeSequence.Empty != sequence)
            {
                sequence.Reset(this.sequencePool);
                this.sequencePool = sequence;
            }
        }

        internal void SaveVariable(ProcessingContext context, int var, int count)
        {
            NodeSequence seq = context.Sequences[context.TopSequenceArg.basePtr].Sequence;
            if (seq == null)
                seq = CreateSequence();
            seq.OwnerContext = null;
            this.subExprVars[var].seq = seq;
            this.subExprVars[var].count = count;
        }

        #region IQueryBufferPool Members
#if NO
        public override void Reset()
        {
            base.Release();            
            // Trim local pools by releasing all references
            while (null != this.PopResultSet());            
            this.resultPool = null;
            while (null != this.PopSequence());
            this.sequencePool = null;
            while (null != this.PopContext());
            this.contextPool = null;
        }

        public override void Trim()
        {
            // Trim stacks
            base.Trim();   
            // Trim local pools individually         
            QueryBranchResultSet result = this.resultPool;
            while (null != result)
            {
                result.Trim();
                result = result.Next;
            }
            NodeSequence sequence = this.sequencePool;
            while (null != sequencePool)
            {
                sequencePool.Trim();
                sequence = sequence.Next;
            }
            ProcessingContext context = this.contextPool;
            while (null != context)
            {
                context.Trim();
                context = context.Next;
            }
        }
#endif
        #endregion

        struct SubExprVariable
        {
            internal NodeSequence seq;
            internal int count;
        }
    }


#if NO 
    internal struct QueryStatistics
    {
        int backupCapacity;

        int nodeCapacity;

        int seqStackCapacity;

        int seqFrameCapacity;

        int valFrameCapacity;

        int valStackCapacity;

        internal QueryStatistics()
        {
            this.backupCapacity = 8;
            this.nodeCapacity = 8;
            this.seqFrameCapacity = 2;
            this.seqStackCapacity = 4;
            this.valFrameCapacity = 2;
            this.valStackCapacity = 4;
        }

        internal int BackupCapacity
        {
            get
            {
                return this.backupCapacity;
            }
        }

        internal int NodeCapacity
        {
            get
            {
                return this.nodeCapacity;
            }
        }

        internal int SeqFrameCapacity
        {
            get
            {
                return this.seqFrameCapacity;
            }
        }

        internal int SeqStackCapacity
        {
            get
            {
                return this.seqStackCapacity;
            }
        }

        internal int ValFrameCapacity
        {
            get
            {
                return this.valFrameCapacity;
            }
        }

        internal int ValStackCapacity
        {
            get
            {
                return this.valStackCapacity;
            }
        }
    }
#endif
}
