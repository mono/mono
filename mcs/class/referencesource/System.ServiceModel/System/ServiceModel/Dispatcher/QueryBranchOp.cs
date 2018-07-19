//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;

    abstract class JumpOpcode : Opcode
    {
        Opcode jump;

        internal JumpOpcode(OpcodeID id, Opcode jump)
            : base(id)
        {
            this.Jump = jump;
            this.flags |= OpcodeFlags.Jump;
        }

        internal Opcode Jump
        {
            get
            {
                return this.jump;
            }
            set
            {
                Fx.Assert(value.ID == OpcodeID.BlockEnd, "");
                this.AddJump((BlockEndOpcode)value);
            }
        }

        internal void AddJump(BlockEndOpcode jumpTo)
        {
            bool conditional = this.IsReachableFromConditional();
            if (conditional)
            {
                this.prev.DelinkFromConditional(this);
            }

            if (null == this.jump)
            {
                this.jump = jumpTo;
            }
            else
            {
                BranchOpcode jumpBranch;
                if (this.jump.ID == OpcodeID.Branch)
                {
                    // already a branch
                    jumpBranch = (BranchOpcode)this.jump;
                }
                else
                {
                    BlockEndOpcode currentJump = (BlockEndOpcode)this.jump;
                    jumpBranch = new BranchOpcode();
                    jumpBranch.Branches.Add(currentJump);
                    this.jump = jumpBranch;
                }
                jumpBranch.Branches.Add(jumpTo);
            }
            jumpTo.LinkJump(this);

            if (conditional && null != this.jump)
            {
                this.prev.LinkToConditional(this);
            }
        }

        internal override void Remove()
        {
            if (null == this.jump)
            {
                base.Remove();
            }
        }

        internal void RemoveJump(BlockEndOpcode jumpTo)
        {
            Fx.Assert(null != this.jump, "");

            bool conditional = this.IsReachableFromConditional();
            if (conditional)
            {
                this.prev.DelinkFromConditional(this);
            }

            if (this.jump.ID == OpcodeID.Branch)
            {
                BranchOpcode jumpBranch = (BranchOpcode)this.jump;
                jumpTo.DeLinkJump(this);
                jumpBranch.RemoveChild(jumpTo);
                if (0 == jumpBranch.Branches.Count)
                {
                    this.jump = null;
                }
            }
            else
            {
                Fx.Assert(object.ReferenceEquals(jumpTo, this.jump), "");
                jumpTo.DeLinkJump(this);
                this.jump = null;
            }

            if (conditional && null != this.jump)
            {
                this.prev.LinkToConditional(this);
            }
        }

        internal override void Trim()
        {
            if (this.jump.ID == OpcodeID.Branch)
            {
                this.jump.Trim();
            }
        }

    }

    class JumpIfOpcode : JumpOpcode
    {
        protected bool test;

        internal JumpIfOpcode(Opcode jump, bool test)
            : this(OpcodeID.JumpIfNot, jump, test)
        {
        }

        protected JumpIfOpcode(OpcodeID id, Opcode jump, bool test)
            : base(id, jump)
        {
            this.test = test;
        }

        internal bool Test
        {
            get
            {
                return this.test;
            }
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return (this.test == ((JumpIfOpcode)op).test);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            Fx.Assert(null != context, "");

            StackFrame arg = context.TopArg;
            for (int i = arg.basePtr; i <= arg.endPtr; ++i)
            {
                Fx.Assert(context.Values[i].IsType(ValueDataType.Boolean), "");
                if (this.test == context.Values[i].Boolean)
                {
                    // At least one result satisfies the test. Don't branch
                    return this.next;
                }
            }

            // Jump, since no result is satisfactory..
            return this.Jump;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.test);
        }
#endif
    }

    class ApplyBooleanOpcode : JumpIfOpcode
    {
        internal ApplyBooleanOpcode(Opcode jump, bool test)
            : this(OpcodeID.ApplyBoolean, jump, test)
        {
        }

        protected ApplyBooleanOpcode(OpcodeID id, Opcode jump, bool test)
            : base(id, jump, test)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            int matchCount = this.UpdateResultMask(context);
            context.PopFrame();
            if (0 == matchCount)
            {
                return this.Jump;
            }

            return this.next;
        }

        protected int UpdateResultMask(ProcessingContext context)
        {
            StackFrame results = context.TopArg;
            StackFrame resultMask = context.SecondArg;
            Value[] values = context.Values;
            int testCount = 0;

            for (int maskIndex = resultMask.basePtr, resultIndex = results.basePtr; maskIndex <= resultMask.endPtr; ++maskIndex)
            {
                if (this.test == values[maskIndex].Boolean)
                {
                    bool boolResult = values[resultIndex].Boolean;
                    if (this.test == boolResult)
                    {
                        testCount++;
                    }
                    values[maskIndex].Boolean = boolResult;
                    ++resultIndex;
                }
            }
            return testCount;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.test);
        }
#endif
    }

    // 'And' booleans: test is true
    // 'Or' booleans: test is false
    class StartBooleanOpcode : Opcode
    {
        bool test;

        internal StartBooleanOpcode(bool test)
            : base(OpcodeID.StartBoolean)
        {
            this.test = test;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return (((StartBooleanOpcode)op).test == this.test);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame sequences = context.TopSequenceArg;
            Value[] values = context.Values;
            StackFrame resultMask = context.TopArg;
            Value[] sequenceBuffer = context.Sequences;

            context.PushSequenceFrame();
            for (int seqIndex = sequences.basePtr; seqIndex <= sequences.endPtr; ++seqIndex)
            {
                NodeSequence sourceSeq = sequenceBuffer[seqIndex].Sequence;
                if (sourceSeq.Count > 0)
                {
                    NodeSequenceItem[] items = sourceSeq.Items;
                    NodeSequence newSeq = null;
                    // Loop over the sequence, selecting those items for which the previous expression returned a result that
                    // matches the test value. Only those items will be processed further
                    // Note that the original position and size information is retained.
                    for (int i = resultMask.basePtr, n = 0; i <= resultMask.endPtr; ++i, ++n)
                    {
                        if (this.test == values[i].Boolean)
                        {
                            if (null == newSeq)
                            {
                                newSeq = context.CreateSequence();
                            }
                            newSeq.AddCopy(ref items[n], NodeSequence.GetContextSize(sourceSeq, n));
                        }
                        else
                        {
                            if (items[n].Last && null != newSeq)
                            {
                                newSeq.Items[newSeq.Count - 1].Last = true; // maintain nodeset boundaries...
                            }
                        }
                    }
                    context.PushSequence((null == newSeq) ? NodeSequence.Empty : newSeq);
                    newSeq = null;
                }
            }

            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.test);
        }
#endif
    }

    // 'And' booleans: test is true
    // 'Or' booleans: test is false
    class EndBooleanOpcode : ApplyBooleanOpcode
    {
        internal EndBooleanOpcode(Opcode jump, bool test)
            : base(OpcodeID.EndBoolean, jump, test)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            int matchCount = this.UpdateResultMask(context);
            context.PopFrame();
            context.PopSequenceFrame();
            if (0 == matchCount)
            {
                return this.Jump;
            }

            return this.next;
        }
    }

    class NoOpOpcode : Opcode
    {
        internal NoOpOpcode(OpcodeID id)
            : base(id)
        {
        }
    }

    class BlockEndOpcode : Opcode
    {
        QueryBuffer<Opcode> sourceJumps;

        internal BlockEndOpcode()
            : base(OpcodeID.BlockEnd)
        {
            this.sourceJumps = new QueryBuffer<Opcode>(1);
        }

        internal void DeLinkJump(Opcode jump)
        {
            this.sourceJumps.Remove(jump);
        }

        internal void LinkJump(Opcode jump)
        {
            this.sourceJumps.Add(jump);
        }

        internal override void Remove()
        {
            // Before we can remove this blockEnd from the query tree, we have delink all jumps to it
            while (this.sourceJumps.Count > 0)
            {
                ((JumpOpcode)this.sourceJumps[0]).RemoveJump(this);
            }
            base.Remove();
        }

    }

    class TypecastOpcode : Opcode
    {
        ValueDataType newType;
#if NO
        internal TypecastOpcode(OpcodeID opcode, ValueDataType newType)
            : base(opcode)
        {
            this.newType = newType;
        }
#endif
        internal TypecastOpcode(ValueDataType newType)
            : base(OpcodeID.Cast)
        {
            this.newType = newType;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return (this.newType == ((TypecastOpcode)op).newType);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame frame = context.TopArg;
            Value[] values = context.Values;
            for (int i = frame.basePtr; i <= frame.endPtr; ++i)
            {
                values[i].ConvertTo(context, newType);
            }

            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.newType.ToString());
        }
#endif
    }

    struct BranchContext
    {
        ProcessingContext branchContext;
        ProcessingContext sourceContext;

        internal BranchContext(ProcessingContext context)
        {
            this.sourceContext = context;
            this.branchContext = null;
        }

        internal ProcessingContext Create()
        {
            if (null == this.branchContext)
            {
                this.branchContext = this.sourceContext.Clone();
            }
            else
            {
                this.branchContext.CopyFrom(this.sourceContext);
            }
            return this.branchContext;
        }

        internal void Release()
        {
            if (null != this.branchContext)
            {
                this.branchContext.Release();
            }
        }
    }

    class QueryBranch
    {
        internal Opcode branch;
        internal int id;
#if NO
        internal QueryBranch(Opcode branch)
            : this(branch, int.MinValue)
        {
        }
#endif
        internal QueryBranch(Opcode branch, int id)
        {
            this.branch = branch;
            this.id = id;
        }

        internal Opcode Branch
        {
            get
            {
                return this.branch;
            }
#if NO
            set
            {
                this.branch = value;
            }
#endif
        }

        internal int ID
        {
            get
            {
                return this.id;
            }
        }
    }

    class QueryBranchTable
    {
        int count;
        QueryBranch[] branches;

        internal QueryBranchTable()
            : this(1)
        {
        }

        internal QueryBranchTable(int capacity)
        {
            this.branches = new QueryBranch[capacity];
        }

        internal int Count
        {
            get
            {
                return this.count;
            }
        }

        internal QueryBranch this[int index]
        {
            get
            {
                return this.branches[index];
            }
        }
#if NO
        internal void Add(QueryBranch entry)
        {
            Fx.Assert(null != entry, "");
            this.InsertAt(this.count, entry);
        }
#endif
        internal void AddInOrder(QueryBranch branch)
        {
            // Insert in sorted order always
            int index;
            for (index = 0; index < this.count; ++index)
            {
                // if current node is >= key, we've found the spot
                if (this.branches[index].ID >= branch.ID)
                {
                    break;
                }
            }

            this.InsertAt(index, branch);
        }

        void Grow()
        {
            QueryBranch[] branches = new QueryBranch[this.branches.Length + 1];
            Array.Copy(this.branches, branches, this.branches.Length);
            this.branches = branches;
        }

        public int IndexOf(Opcode opcode)
        {
            for (int i = 0; i < this.count; ++i)
            {
                if (object.ReferenceEquals(opcode, this.branches[i].Branch))
                {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOfID(int id)
        {
            for (int i = 0; i < this.count; ++i)
            {
                if (this.branches[i].ID == id)
                {
                    return i;
                }
            }
            return -1;
        }

#if NO
        public int IndexOfEquals(Opcode opcode)
        {
            for (int i = 0; i < this.count; ++i)
            {
                if (this.branches[i].Branch.Equals(opcode))
                {
                    return i;
                }
            }
            return -1;
        }
#endif
        internal void InsertAt(int index, QueryBranch branch)
        {
            if (this.count == this.branches.Length)
            {
                this.Grow();
            }
            if (index < this.count)
            {
                Array.Copy(this.branches, index, this.branches, index + 1, this.count - index);
            }
            this.branches[index] = branch;
            this.count++;
        }

        internal bool Remove(Opcode branch)
        {
            Fx.Assert(null != branch, "");
            int index = this.IndexOf(branch);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }

            return false;
        }

        internal void RemoveAt(int index)
        {
            Fx.Assert(index < this.count, "");
            if (index < this.count - 1)
            {
                Array.Copy(this.branches, index + 1, this.branches, index, this.count - index - 1);
            }
            else
            {
                this.branches[index] = null;
            }
            this.count--;
        }

        internal void Trim()
        {
            if (this.count < this.branches.Length)
            {
                QueryBranch[] branches = new QueryBranch[this.count];
                Array.Copy(this.branches, branches, this.count);
                this.branches = branches;
            }

            for (int i = 0; i < this.branches.Length; ++i)
            {
                if (this.branches[i] != null && this.branches[i].Branch != null)
                {
                    this.branches[i].Branch.Trim();
                }
            }
        }
    }

    class BranchOpcode : Opcode
    {
        OpcodeList branches;

        internal BranchOpcode()
            : this(OpcodeID.Branch)
        {
        }

        internal BranchOpcode(OpcodeID id)
            : base(id)
        {
            this.flags |= OpcodeFlags.Branch;
            this.branches = new OpcodeList(2);
        }

        internal OpcodeList Branches
        {
            get
            {
                return this.branches;
            }
        }

        internal override void Add(Opcode opcode)
        {
            // Add with maximum affinity.. find a similar opcode, if we can, and call its Add method
            for (int i = 0; i < this.branches.Count; ++i)
            {
                if (this.branches[i].IsEquivalentForAdd(opcode))
                {
                    this.branches[i].Add(opcode);
                    return;
                }
            }

            // Ok.. no similar opcode. Add it just like any other branch
            this.AddBranch(opcode);
        }

        internal override void AddBranch(Opcode opcode)
        {
            Fx.Assert(null != opcode, "");

            // Make sure the same opcode doesn't already exist..
            Fx.Assert(-1 == this.branches.IndexOf(opcode), "");
            this.branches.Add(opcode);
            opcode.Prev = this;

            // If this branch is in a conditional, then this new opcode must be added to that conditional..
            if (this.IsInConditional())
            {
                this.LinkToConditional(opcode);
            }
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            for (int i = 0; i < this.branches.Count; ++i)
            {
                this.branches[i].CollectXPathFilters(filters);
            }
        }

        internal override void DelinkFromConditional(Opcode child)
        {
            if (null != this.prev)
            {
                this.prev.DelinkFromConditional(child);
            }
        }
        internal override Opcode Eval(ProcessingContext context)
        {
            QueryProcessor processor = context.Processor;
            SeekableXPathNavigator contextNode = processor.ContextNode;
            int marker = processor.CounterMarker;
            long pos = contextNode.CurrentPosition;
            Opcode branch;
            int i = 0;
            int branchCount = this.branches.Count;
            try
            {
                if (context.StacksInUse)
                {
                    // If we have N branches, eval N-1 in a cloned context and the remainder in the
                    // original one
                    if (--branchCount > 0)
                    {
                        // Evaluate all but the first branch with a clone of the current context
                        // The first branch (right most) can be evaluated with the current context                        
                        BranchContext branchContext = new BranchContext(context); // struct. fast
                        for (; i < branchCount; ++i)
                        {
                            branch = this.branches[i];
                            if (0 != (branch.Flags & OpcodeFlags.Fx))
                            {
                                branch.Eval(context);
                            }
                            else
                            {
                                // This allocates a solitary context and then reuses it repeatedly
                                ProcessingContext newContext = branchContext.Create();
                                while (null != branch)
                                {
                                    branch = branch.Eval(newContext);
                                }
                            }
                            contextNode.CurrentPosition = pos;  // restore the navigator to its original position
                            processor.CounterMarker = marker;   // and the node count marker
                        }
                        branchContext.Release();
                    }

                    // Do the final branch with the existing context
                    branch = branches[i];
                    while (null != branch)
                    {
                        branch = branch.Eval(context);
                    }
                }
                else // since there is nothing on the stack, there is nothing to clone
                {
                    int nodeCountSav = context.NodeCount;
                    for (; i < branchCount; ++i)
                    {
                        branch = branches[i];
                        // Evaluate this branch
                        while (null != branch)
                        {
                            branch = branch.Eval(context);
                        }
                        // Restore the current context to its pristine state, so we can reuse it
                        context.ClearContext();
                        context.NodeCount = nodeCountSav;
                        contextNode.CurrentPosition = pos;
                        processor.CounterMarker = marker;
                    }
                }
            }
            catch (XPathNavigatorException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(branches[i]));
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(branches[i]));
            }
            processor.CounterMarker = marker;

            return this.next;
        }

        internal override bool IsInConditional()
        {
            if (null != this.prev)
            {
                return this.prev.IsInConditional();
            }

            return true;
        }

        internal override void LinkToConditional(Opcode child)
        {
            if (null != this.prev)
            {
                this.prev.LinkToConditional(child);
            }
        }

        /// <summary>
        /// Loop over all branches, trying to locate one that is equal to the given opcode
        /// If the branch is a branch itself, perform the locate recursively.
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        internal override Opcode Locate(Opcode opcode)
        {
            Fx.Assert(!opcode.TestFlag(OpcodeFlags.Branch), "");

            for (int i = 0, count = this.branches.Count; i < count; ++i)
            {
                Opcode branch = this.branches[i];
                if (branch.TestFlag(OpcodeFlags.Branch))
                {
                    // The branch is itself a branch. Since branch opcodes serve as branches in the exection
                    // path for a query, but don't comprise one of the opcodes used to actually perform it, we
                    // recursively try to locate an equivalent opcode inside the branch
                    Opcode subBranch = branch.Locate(opcode);
                    if (null != subBranch)
                    {
                        return subBranch;
                    }
                }
                else if (branch.Equals(opcode))
                {
                    return branch;
                }
            }

            return null;
        }

        internal override void Remove()
        {
            if (0 == this.branches.Count)
            {
                base.Remove();
            }
        }

        internal override void RemoveChild(Opcode opcode)
        {
            Fx.Assert(null != opcode, "");

            if (this.IsInConditional())
            {
                this.DelinkFromConditional(opcode);
            }
            this.branches.Remove(opcode);
            this.branches.Trim();
        }

        internal override void Replace(Opcode replace, Opcode with)
        {
            int i = this.branches.IndexOf(replace);
            if (i >= 0)
            {
                replace.Prev = null;
                this.branches[i] = with;
                with.Prev = this;
            }
        }

        internal override void Trim()
        {
            this.branches.Trim();
            for (int i = 0; i < this.branches.Count; ++i)
            {
                this.branches[i].Trim();
            }
        }
    }

    struct QueryBranchResult
    {
        internal QueryBranch branch;
        int valIndex;

        internal QueryBranchResult(QueryBranch branch, int valIndex)
        {
            this.branch = branch;
            this.valIndex = valIndex;
        }

        internal QueryBranch Branch
        {
            get
            {
                return this.branch;
            }
        }

        internal int ValIndex
        {
            get
            {
                return this.valIndex;
            }
        }
#if NO
        internal void Set(QueryBranch branch, int valIndex)
        {
            this.branch = branch;
            this.valIndex = valIndex;
        }
#endif
    }

    internal class QueryBranchResultSet
    {
        QueryBuffer<QueryBranchResult> results;
        QueryBranchResultSet next;
        internal static SortComparer comparer = new SortComparer();

        internal QueryBranchResultSet()
            : this(2)
        {
        }

        internal QueryBranchResultSet(int capacity)
        {
            this.results = new QueryBuffer<QueryBranchResult>(capacity);
        }

        internal int Count
        {
            get
            {
                return this.results.count;
            }
        }

        internal QueryBranchResult this[int index]
        {
            get
            {
                return this.results[index];
            }
        }

        internal QueryBranchResultSet Next
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

        internal void Add(QueryBranch branch, int valIndex)
        {
            this.results.Add(new QueryBranchResult(branch, valIndex));
        }

        internal void Clear()
        {
            this.results.count = 0;
        }

        internal void Sort()
        {
            this.results.Sort(QueryBranchResultSet.comparer);
        }

        internal class SortComparer : IComparer<QueryBranchResult>
        {
            public bool Equals(QueryBranchResult x, QueryBranchResult y)
            {
                return x.branch.id == y.branch.id;
            }
            public int Compare(QueryBranchResult x, QueryBranchResult y)
            {
                return x.branch.id - y.branch.id;
            }

            public int GetHashCode(QueryBranchResult obj)
            {
                return obj.branch.id;
            }
        }
    }

    struct BranchMatcher
    {
        int resultCount;
        QueryBranchResultSet resultTable;

        internal BranchMatcher(int resultCount, QueryBranchResultSet resultTable)
        {
            this.resultCount = resultCount;
            this.resultTable = resultTable;
        }

        internal QueryBranchResultSet ResultTable
        {
            get
            {
                return this.resultTable;
            }
        }

        void InitResults(ProcessingContext context)
        {
            context.PushFrame();
            // Push this.resultsCount booleans onto the stack, all set to false
            context.Push(false, this.resultCount);
        }

        internal void InvokeMatches(ProcessingContext context)
        {
            Fx.Assert(null != context, "");

            switch (this.resultTable.Count)
            {
                default:
                    this.InvokeMultiMatch(context);
                    break;

                case 0:
                    break;

                case 1:
                    this.InvokeSingleMatch(context);
                    break;
            }
        }

        void InvokeMultiMatch(ProcessingContext context)
        {
            int marker = context.Processor.CounterMarker;
            BranchContext branchContext = new BranchContext(context);   // struct. quick.
            int resultTableCount = this.resultTable.Count;
            for (int i = 0; i < resultTableCount; )
            {
                QueryBranchResult result = this.resultTable[i];
                QueryBranch branch = result.Branch;
                // Branches can arbitrarily alter context stacks, rendering them unuseable to other branches. 
                // Therefore, before following a branch, we have to clone the context. Cloning is relatively efficient because
                // can avoid allocating memory in most cases. We cannot, unfortunately, avoid Array copies. 
                //
                // Optimization: 
                // We can avoid cloning altogether when we can predict that the branch does NOT tamper with the stack,
                // or does so in a predictable way. If we are sure that we can restore the stack easily after the branch
                // completes, we have no reason to copy the stack.
                ProcessingContext newContext;
                Opcode nextOpcode = branch.Branch.Next;
                if (nextOpcode.TestFlag(OpcodeFlags.NoContextCopy))
                {
                    newContext = context;
                }
                else
                {
                    newContext = branchContext.Create();
                }

                this.InitResults(newContext);

                //
                // Matches are sorted by their branch ID.
                // It is very possible that the a literal matches multiple times, especially when the value being 
                // compared is a sequence. A literal may match multiple items in a single sequence 
                // OR multiple items in multiple sequences. If there were 4 context sequences, the literal may have 
                // matched one item each in 3 of them. The branchID for that literal will be present 3 times in the
                // resultTable.
                // Sorting the matches groups them by their branch Ids. We only want to take the branch ONCE, so now we
                // iterate over all the duplicate matches..
                // result.ValIndex will give us the index of the value that was matched. Thus if the 3rd sequence 
                // matched, ValIndex == 2 (0 based)
                newContext.Values[newContext.TopArg[result.ValIndex]].Boolean = true;
                while (++i < resultTableCount)
                {
                    result = this.resultTable[i];
                    if (branch.ID == result.Branch.ID)
                    {
                        newContext.Values[newContext.TopArg[result.ValIndex]].Boolean = true;
                    }
                    else
                    {
                        break;
                    }
                }
                try
                {
                    newContext.EvalCodeBlock(nextOpcode);
                }
                catch (XPathNavigatorException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(nextOpcode));
                }
                catch (NavigatorInvalidBodyAccessException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(nextOpcode));
                }
                context.Processor.CounterMarker = marker;
            }
            branchContext.Release();
        }

        internal void InvokeNonMatches(ProcessingContext context, QueryBranchTable nonMatchTable)
        {
            Fx.Assert(null != context && null != nonMatchTable, "");

            int marker = context.Processor.CounterMarker;
            BranchContext branchContext = new BranchContext(context);
            int nonMatchIndex = 0;
            int matchIndex = 0;
            while (matchIndex < this.resultTable.Count && nonMatchIndex < nonMatchTable.Count)
            {
                int compare = this.resultTable[matchIndex].Branch.ID - nonMatchTable[nonMatchIndex].ID;
                if (compare > 0)
                {
                    // Nonmatch < match
                    // Invoke..
                    ProcessingContext newContext = branchContext.Create();
                    this.InvokeNonMatch(newContext, nonMatchTable[nonMatchIndex]);
                    context.Processor.CounterMarker = marker;
                    ++nonMatchIndex;
                }
                else if (0 == compare)
                {
                    ++nonMatchIndex;
                }
                else
                {
                    ++matchIndex;
                }
            }
            // Add remaining
            while (nonMatchIndex < nonMatchTable.Count)
            {
                ProcessingContext newContext = branchContext.Create();
                this.InvokeNonMatch(newContext, nonMatchTable[nonMatchIndex]);
                context.Processor.CounterMarker = marker;
                ++nonMatchIndex;
            }
            branchContext.Release();
        }

        void InvokeNonMatch(ProcessingContext context, QueryBranch branch)
        {
            context.PushFrame();
            context.Push(false, this.resultCount);
            try
            {
                context.EvalCodeBlock(branch.Branch);
            }
            catch (XPathNavigatorException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(branch.Branch));
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(branch.Branch));
            }
        }

        void InvokeSingleMatch(ProcessingContext context)
        {
            int marker = context.Processor.CounterMarker;
            QueryBranchResult result = this.resultTable[0];
            this.InitResults(context);
            context.Values[context.TopArg[result.ValIndex]].Boolean = true;

            try
            {
                context.EvalCodeBlock(result.Branch.Branch.Next);
            }
            catch (XPathNavigatorException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(result.Branch.Branch.Next));
            }
            catch (NavigatorInvalidBodyAccessException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.Process(result.Branch.Branch.Next));
            }

            context.Processor.CounterMarker = marker;
        }

        internal void Release(ProcessingContext context)
        {
            context.Processor.ReleaseResults(this.resultTable);
        }
    }

    abstract class QueryBranchIndex
    {
        internal abstract int Count
        {
            get;
        }
        internal abstract QueryBranch this[object key]
        {
            get;
            set;
        }

        internal abstract void CollectXPathFilters(ICollection<MessageFilter> filters);

#if NO
        internal abstract IEnumerator GetEnumerator();
#endif
        internal abstract void Match(int valIndex, ref Value val, QueryBranchResultSet results);
        internal abstract void Remove(object key);

        internal abstract void Trim();
    }

    class QueryConditionalBranchOpcode : Opcode
    {
        QueryBranchTable alwaysBranches;
        QueryBranchIndex branchIndex;
        int nextID;

        internal QueryConditionalBranchOpcode(OpcodeID id, QueryBranchIndex branchIndex)
            : base(id)
        {
            Fx.Assert(null != branchIndex, "");
            this.flags |= OpcodeFlags.Branch;
            this.branchIndex = branchIndex;
            this.nextID = 0;
        }

        internal QueryBranchTable AlwaysBranches
        {
            get
            {
                if (null == this.alwaysBranches)
                {
                    this.alwaysBranches = new QueryBranchTable();
                }
                return this.alwaysBranches;
            }
        }
#if NO
        internal QueryBranchIndex BranchIndex
        {
            get
            {
                return this.branchIndex;
            }
        }
#endif
        internal override void Add(Opcode opcode)
        {
            LiteralRelationOpcode literal = this.ValidateOpcode(opcode);
            if (null == literal)
            {
                base.Add(opcode);
                return;
            }

            // Was this literal already added to the index?
            QueryBranch queryBranch = this.branchIndex[literal.Literal];
            if (null == queryBranch)
            {
                // First time. New branch
                this.nextID++;
                queryBranch = new QueryBranch(literal, this.nextID);
                literal.Prev = this;
                this.branchIndex[literal.Literal] = queryBranch;
            }
            else
            {
                Fx.Assert(!object.ReferenceEquals(queryBranch.Branch, literal), "");
                Fx.Assert(literal.ID == queryBranch.Branch.ID, "");
                // literal already exists.. but what follows the literal must be branched
                // Should never get here, but just in case
                queryBranch.Branch.Next.Add(literal.Next);
            }
            literal.Flags |= OpcodeFlags.InConditional;

            this.AddAlwaysBranch(queryBranch, literal.Next);
        }

        internal void AddAlwaysBranch(Opcode literal, Opcode next)
        {
            LiteralRelationOpcode literalOp = this.ValidateOpcode(literal);
            Fx.Assert(null != literalOp, "");
            if (null != literalOp)
            {
                this.AddAlwaysBranch(literalOp, next);
            }
        }

        // Whether or not the given literal matches, we must always take the branch rooted at 'next'
        // Add to the AlwaysBranches table if not already there..
        internal void AddAlwaysBranch(LiteralRelationOpcode literal, Opcode next)
        {
            Fx.Assert(null != literal && null != next, "");

            QueryBranch literalBranch = this.branchIndex[literal.Literal];
            Fx.Assert(null != literalBranch, "");

            this.AddAlwaysBranch(literalBranch, next);
        }

        void AddAlwaysBranch(QueryBranch literalBranch, Opcode next)
        {
            if (OpcodeID.Branch == next.ID)
            {
                BranchOpcode opcode = (BranchOpcode)next;
                OpcodeList branches = opcode.Branches;
                for (int i = 0; i < branches.Count; ++i)
                {
                    Opcode branch = branches[i];
                    if (this.IsAlwaysBranch(branch))
                    {
                        this.AlwaysBranches.AddInOrder(new QueryBranch(branch, literalBranch.ID));
                    }
                    else
                    {
                        branch.Flags |= OpcodeFlags.NoContextCopy;
                    }
                }
            }
            else
            {
                Fx.Assert(!next.TestFlag(OpcodeFlags.Branch), "");
                if (this.IsAlwaysBranch(next))
                {
                    this.AlwaysBranches.AddInOrder(new QueryBranch(next, literalBranch.ID));
                }
                else
                {
                    next.Flags |= OpcodeFlags.NoContextCopy;
                }
            }
        }

        internal virtual void CollectMatches(int valIndex, ref Value val, QueryBranchResultSet results)
        {
            this.branchIndex.Match(valIndex, ref val, results);
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            if (this.alwaysBranches != null)
            {
                for (int i = 0; i < this.alwaysBranches.Count; ++i)
                {
                    this.alwaysBranches[i].Branch.CollectXPathFilters(filters);
                }
            }

            this.branchIndex.CollectXPathFilters(filters);
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame arg = context.TopArg;
            int argCount = arg.Count;

            if (argCount > 0)
            {
                QueryBranchResultSet resultSet = context.Processor.CreateResultSet();
                BranchMatcher matcher = new BranchMatcher(argCount, resultSet);
                // Operate on values at the the top frame of the value stack
                // For each source value, find the branch that could be taken
                for (int i = 0; i < argCount; ++i)
                {
                    this.CollectMatches(i, ref context.Values[arg[i]], resultSet);
                }
                // Done with whatever we were testing equality against
                context.PopFrame();
                if (resultSet.Count > 1)
                {
                    // Sort results
                    resultSet.Sort();
                }

                // First, do non-true branches..
                if (null != this.alwaysBranches && this.alwaysBranches.Count > 0)
                {
                    matcher.InvokeNonMatches(context, this.alwaysBranches);
                }

                // Iterate through matches, invoking each matched branch
                matcher.InvokeMatches(context);

                matcher.Release(context);
            }
            else
            {
                context.PopFrame();
            }
            return this.next;
        }

        internal QueryBranch GetBranch(Opcode op)
        {
            if (op.TestFlag(OpcodeFlags.Literal))
            {
                LiteralRelationOpcode relOp = this.ValidateOpcode(op);
                if (null != relOp)
                {
                    QueryBranch branch = this.branchIndex[relOp.Literal];
                    if (null != branch && branch.Branch.ID == op.ID)
                    {
                        return branch;
                    }
                }
            }

            return null;
        }

        bool IsAlwaysBranch(Opcode next)
        {
            Fx.Assert(null != next, "");

            // Opcodes subsequent to matching literals must obviously be branched to.
            // The question is whether we should branch to the opcodes following those literals that do *not* match.
            // Naturally, the answer depends on the sort of opcode that succeeds the literal.
            //
            // If the literal is within a boolean conjunction, the succeeding opcode will either be a JumpIfNot
            // Or a BlockEnd.
            // 
            // -If the JumpIfNot is multiway, then always evaluate if it contains ANY non-result only opcodes.
            // -If JumpIfNot(False) -i.e. AND - only evaluate if the opcode succeeding the jump is NOT a result opcode.
            // -If JumpIfNot(True) - i.e. OR - always evaluate
            // 
            // -If BlockEnd - evaluate only if not followed by a result
            //
            // When branching for matching literals, we push trues onto the ValueStack corresponding to the items that 
            // matched. When branching for non-matching literals, we push ALL FALSE values... and then eval.

            // is it a the termination of a conditional?
            JumpIfOpcode jump = next as JumpIfOpcode;
            if (null != jump)
            {
                // Is the conditional JumpIfNot(False) = i.e. OR? 
                if (!jump.Test)
                {
                    return true;
                }

                // Does the conditional actually jump to anything? Should never be the case, but paranoia demands..
                Opcode jumpTo = jump.Jump;
                if (null == jumpTo)
                {
                    return false;
                }

                // Lets see where the jump will take us
                Opcode postJump;
                if (jumpTo.TestFlag(OpcodeFlags.Branch))
                {
                    // Multiway jump
                    OpcodeList branches = ((BranchOpcode)jumpTo).Branches;
                    for (int i = 0; i < branches.Count; ++i)
                    {
                        postJump = branches[i].Next;
                        if (null != postJump && !postJump.TestFlag(OpcodeFlags.Result))
                        {
                            // There is at least one jump here that leads to a non-result.
                            // For now, this dooms everybody to being branched to, whether or not their respective literals
                            // matched
                            return true;
                        }
                    }
                    return false;
                }

                // single jump
                postJump = jump.Jump.Next;
                if (null != postJump && postJump.TestFlag(OpcodeFlags.Result))
                {
                    return false;
                }

                return true;
            }

            // If the next opcode is a BlockEnd, then only bother processing if what follows the block is not a result
            if (OpcodeID.BlockEnd == next.ID)
            {
                Fx.Assert(null != next.Next, "");
                return (!next.Next.TestFlag(OpcodeFlags.Result));
            }

            // The literal is not inside a boolean conjunction
            // If the literal is not followed by a result, then we must do further processing after the branch
            return (!next.TestFlag(OpcodeFlags.Result));
        }

        /// <summary>
        /// Returns true if this branch can accept 'opcode' being added to it
        /// </summary>
        internal override bool IsEquivalentForAdd(Opcode opcode)
        {
            if (null != this.ValidateOpcode(opcode))
            {
                return true;
            }

            return base.IsEquivalentForAdd(opcode);
        }

        internal override Opcode Locate(Opcode opcode)
        {
            QueryBranch queryBranch = this.GetBranch(opcode);
            if (null != queryBranch)
            {
                return queryBranch.Branch;
            }

            return null;
        }

        internal override void Remove()
        {
            if (null == this.branchIndex || 0 == this.branchIndex.Count)
            {
                base.Remove();
            }
        }

        internal override void RemoveChild(Opcode opcode)
        {
            LiteralRelationOpcode literal = this.ValidateOpcode(opcode);
            Fx.Assert(null != literal, "");

            QueryBranch branch = this.branchIndex[literal.Literal];
            Fx.Assert(null != branch, "");
            this.branchIndex.Remove(literal.Literal);
            branch.Branch.Flags &= (~OpcodeFlags.NoContextCopy);
            if (null != this.alwaysBranches)
            {
                int removeAt = this.alwaysBranches.IndexOfID(branch.ID);
                if (removeAt >= 0)
                {
                    this.alwaysBranches.RemoveAt(removeAt);
                    if (0 == this.alwaysBranches.Count)
                    {
                        this.alwaysBranches = null;
                    }
                }
            }
        }

        internal void RemoveAlwaysBranch(Opcode opcode)
        {
            if (null == this.alwaysBranches)
            {
                return;
            }

            // Note: if the opcodes below are not in the alwaysBranches table, nothing will happen
            // So arbitrary calls are safe - just makes the removal processor slower (we'll speed it up if necessary)
            if (OpcodeID.Branch == opcode.ID)
            {
                OpcodeList branches = ((BranchOpcode)opcode).Branches;
                for (int i = 0; i < branches.Count; ++i)
                {
                    this.alwaysBranches.Remove(branches[i]);
                }
            }
            else
            {
                this.alwaysBranches.Remove(opcode);
            }
            if (0 == this.alwaysBranches.Count)
            {
                this.alwaysBranches = null;
            }
        }

        internal override void Replace(Opcode replace, Opcode with)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new NotImplementedException(SR.GetString(SR.FilterUnexpectedError)));
        }

        internal override void Trim()
        {
            if (this.alwaysBranches != null)
            {
                this.alwaysBranches.Trim();
            }
            this.branchIndex.Trim();
        }

        internal virtual LiteralRelationOpcode ValidateOpcode(Opcode opcode)
        {
            return opcode as LiteralRelationOpcode;
        }
    }
}
