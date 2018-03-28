//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    enum OpcodeID
    {
        NoOp,
        SubExpr,
        // Flow opcodes
        Branch,
        JumpIfNot,
        Filter,
        Function,
        XsltFunction,
        XsltInternalFunction,
        Cast,
        QueryTree,
        BlockEnd,
        SubRoutine,
        // Set Opcodes
        Ordinal,
        LiteralOrdinal,
        Empty,
        Union,
        Merge,
        // Boolean opcodes
        ApplyBoolean,
        StartBoolean,
        EndBoolean,
        // Relational opcodes
        Relation,
        StringEquals,
        NumberEquals,
        StringEqualsBranch,
        NumberEqualsBranch,
        NumberRelation,
        NumberInterval,
        NumberIntervalBranch,
        // Select/Node Operators
        Select,
        InitialSelect,
        SelectRoot,
        // Stack operators
        PushXsltVariable,
        PushBool,
        PushString,
        PushDouble,
        PushContextNode,
        PushNodeSequence,
        PushPosition,
        PopSequenceToValueStack,
        PopSequenceToSequenceStack,
        PopContextNodes,
        PushContextCopy,
        PopValueFrame,
        // Math opcode
        Plus,
        Minus,
        Multiply,
        Divide,
        Mod,
        Negate,
        // Specialized String operators
        StringPrefix,
        StringPrefixBranch,
        // Results
        MatchAlways,
        MatchResult,
        MatchFilterResult,
        MatchMultipleResult,
        MatchSingleFx,
        QuerySingleFx,
        QueryResult,
        QueryMultipleResult
    }

    enum OpcodeFlags
    {
        None = 0x00000000,
        Single = 0x00000001,
        Multiple = 0x00000002,
        Branch = 0x00000004,
        Result = 0x00000008,
        Jump = 0x00000010,
        Literal = 0x00000020,
        Select = 0x00000040,
        Deleted = 0x00000080,
        InConditional = 0x00000100,
        NoContextCopy = 0x00000200,
        InitialSelect = 0x00000400,
        CompressableSelect = 0x00000800,
        Fx = 0x00001000
    }

    abstract class Opcode
    {
        protected OpcodeFlags flags;
        protected Opcode next;
        OpcodeID opcodeID;
        protected Opcode prev;
#if DEBUG
        // debugging aid. Because C# references do not have displayble numeric values, hard to deduce the
        // graph structure to see what opcode is connected to what
        static long nextUniqueId = 0;
        internal long uniqueID;
#endif
        internal Opcode(OpcodeID id)
        {
            this.opcodeID = id;
            this.flags = OpcodeFlags.Single;
#if DEBUG
            this.uniqueID = Opcode.NextUniqueId();
#endif
        }

        internal OpcodeFlags Flags
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }

        internal OpcodeID ID
        {
            get
            {
                return this.opcodeID;
            }
        }

        internal Opcode Next
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

        internal Opcode Prev
        {
            get
            {
                return this.prev;
            }
            set
            {
                this.prev = value;
            }
        }

#if DEBUG
        static long NextUniqueId()
        {
            return Interlocked.Increment(ref Opcode.nextUniqueId);
        }
#endif

        internal virtual void Add(Opcode op)
        {
            Fx.Assert(null != op, "");
            Fx.Assert(null != this.prev, "");

            // Create a branch that will include both this and the new opcode
            this.prev.AddBranch(op);
        }

        internal virtual void AddBranch(Opcode opcode)
        {
            Fx.Assert(null != opcode, "");
            // Replace what follows this opcode with a branch containing .next and the new opcode
            // If this opcode is a conditional, then since the tree structure is about to change, conditional
            // reachability for everything that follows is about to change.
            // 1. Remove .next from the conditional's AlwaysBranch Table.
            // 2. Create the new branch structure.
            // 3. The branch, once in the tree, will fix up all conditional jumps
            Opcode next = this.next;
            if (this.TestFlag(OpcodeFlags.InConditional))
            {
                this.DelinkFromConditional(next);
            }

            BranchOpcode branch = new BranchOpcode();
            this.next = null;
            this.Attach(branch);

            if (null != next)
            {
                Fx.Assert(OpcodeID.Branch != next.ID, "");
                branch.Add(next);
            }
            branch.Add(opcode);
        }

        internal void Attach(Opcode op)
        {
            Fx.Assert(null == this.next, "");
            this.next = op;
            op.prev = this;
        }

        internal virtual void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            if (this.next != null)
            {
                this.next.CollectXPathFilters(filters);
            }
        }

        internal virtual bool IsEquivalentForAdd(Opcode opcode)
        {
            return (this.ID == opcode.ID);
        }

        internal bool IsMultipleResult()
        {
            return ((this.flags & (OpcodeFlags.Result | OpcodeFlags.Multiple)) ==
                (OpcodeFlags.Result | OpcodeFlags.Multiple));
        }

        internal virtual void DelinkFromConditional(Opcode child)
        {
            Fx.Assert(this.TestFlag(OpcodeFlags.InConditional), "");
            if (this.TestFlag(OpcodeFlags.InConditional))
            {
                ((QueryConditionalBranchOpcode)this.prev).RemoveAlwaysBranch(child);
            }
        }

        internal Opcode DetachChild()
        {
            Opcode child = this.next;
            if (child != null)
            {
                if (this.IsInConditional())
                {
                    this.DelinkFromConditional(child);
                }
            }

            this.next = null;
            child.prev = null;

            return child;
        }

        internal void DetachFromParent()
        {
            Opcode parent = this.prev;
            if (parent != null)
            {
                parent.DetachChild();
            }
        }

        internal virtual bool Equals(Opcode op)
        {
            return (op.ID == this.ID);
        }

        internal virtual Opcode Eval(ProcessingContext context)
        {
            return this.next;
        }

        internal virtual Opcode Eval(NodeSequence sequence, SeekableXPathNavigator node)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected));
        }

        internal virtual Opcode EvalSpecial(ProcessingContext context)
        {
            return this.Eval(context);
        }

        internal virtual bool IsInConditional()
        {
            return this.TestFlag(OpcodeFlags.InConditional);
        }

        internal bool IsReachableFromConditional()
        {
            return (null != this.prev && this.prev.IsInConditional());
        }

        internal virtual Opcode Locate(Opcode opcode)
        {
            Fx.Assert(null != opcode, "");

            if (null != this.next && this.next.Equals(opcode))
            {
                return this.next;
            }

            return null;
        }

        internal virtual void LinkToConditional(Opcode child)
        {
            Fx.Assert(this.TestFlag(OpcodeFlags.InConditional), "");
            if (this.TestFlag(OpcodeFlags.InConditional))
            {
                ((QueryConditionalBranchOpcode)this.prev).AddAlwaysBranch(this, child);
            }
        }

        internal virtual void Remove()
        {
            if (null == this.next)
            {
                Opcode prevOpcode = this.prev;
                if (null != prevOpcode)
                {
                    prevOpcode.RemoveChild(this);
                    prevOpcode.Remove();
                }
            }
        }

        internal virtual void RemoveChild(Opcode opcode)
        {
            Fx.Assert(null != opcode, "");
            Fx.Assert(this.next == opcode, "");

            if (this.IsInConditional())
            {
                this.DelinkFromConditional(opcode);
            }

            opcode.prev = null;
            this.next = null;
            opcode.Flags |= OpcodeFlags.Deleted;
        }

        internal virtual void Replace(Opcode replace, Opcode with)
        {
            Fx.Assert(null != replace && null != with, "");
            if (this.next == replace)
            {
                bool isConditional = this.IsInConditional();
                if (isConditional)
                {
                    this.DelinkFromConditional(this.next);
                }
                this.next.prev = null;
                this.next = with;
                with.prev = this;
                if (isConditional)
                {
                    this.LinkToConditional(with);
                }
            }
        }

        internal bool TestFlag(OpcodeFlags flag)
        {
            return (0 != (this.flags & flag));
        }

#if DEBUG_FILTER
        public override string ToString()
        {
#if DEBUG
            return string.Format("{0}(#{1})", this.opcodeID.ToString(), this.uniqueID);
#else
return this.opcodeID.ToString();
#endif
        }
#endif

        internal virtual void Trim()
        {
            if (this.next != null)
            {
                this.next.Trim();
            }
        }
    }

    struct OpcodeBlock
    {
        Opcode first;
        Opcode last;

        internal OpcodeBlock(Opcode first)
        {
            this.first = first;
            this.first.Prev = null;

            for (this.last = this.first; this.last.Next != null; this.last = this.last.Next);
        }

#if FILTEROPTIMIZER
        internal OpcodeBlock(Opcode first, Opcode last)
        {
            this.first = first;
            this.first.Prev = null;
            this.last = last;
            this.Last.Next = null;
        }
#endif

        internal Opcode First
        {
            get
            {
                return this.first;
            }
        }

        internal Opcode Last
        {
            get
            {
                return this.last;
            }
        }

        internal void Append(Opcode opcode)
        {
            Fx.Assert(null != opcode, "");
            if (null == this.last)
            {
                this.first = opcode;
                this.last = opcode;
            }
            else
            {
                this.last.Attach(opcode);
                opcode.Next = null;
                this.last = opcode;
            }
        }

        internal void Append(OpcodeBlock block)
        {
            if (null == this.last)
            {
                this.first = block.first;
                this.last = block.last;
            }
            else
            {
                this.last.Attach(block.first);
                this.last = block.last;
            }
        }

        internal void DetachLast()
        {
            if (null == this.last)
            {
                return;
            }
            Opcode newLast = this.last.Prev;
            this.last.Prev = null;
            this.last = newLast;
            if (null != this.last)
            {
                this.last.Next = null;
            }
        }
    }

    class OpcodeList
    {
        QueryBuffer<Opcode> opcodes;

        public OpcodeList(int capacity)
        {
            this.opcodes = new QueryBuffer<Opcode>(capacity);
        }

        public int Count
        {
            get
            {
                return this.opcodes.count;
            }
        }

        public Opcode this[int index]
        {
            get
            {
                return this.opcodes[index];
            }
            set
            {
                this.opcodes[index] = value;
            }
        }

        public void Add(Opcode opcode)
        {
            this.opcodes.Add(opcode);
        }

        public int IndexOf(Opcode opcode)
        {
            return this.opcodes.IndexOf(opcode);
        }

        public void Remove(Opcode opcode)
        {
            this.opcodes.Remove(opcode);
        }

        public void Trim()
        {
            this.opcodes.TrimToCount();
        }
    }
}
