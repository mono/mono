//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;

    internal enum RelationOperator
    {
        None,
        Eq,
        Ne,
        Gt,
        Ge,
        Lt,
        Le
    }

    /// <summary>
    /// General relation opcode: compares any two values on the value stack
    /// </summary>
    internal class RelationOpcode : Opcode
    {
        protected RelationOperator op;

        internal RelationOpcode(RelationOperator op)
            : this(OpcodeID.Relation, op)
        {
        }

        protected RelationOpcode(OpcodeID id, RelationOperator op)
            : base(id)
        {
            this.op = op;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return (this.op == ((RelationOpcode)op).op);
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame argX = context.TopArg;
            StackFrame argY = context.SecondArg;

            Fx.Assert(argX.Count == argY.Count, "");

            Value[] values = context.Values;
            while (argX.basePtr <= argX.endPtr)
            {
                values[argY.basePtr].Update(context, values[argY.basePtr].CompareTo(ref values[argX.basePtr], op));
                argX.basePtr++;
                argY.basePtr++;
            }

            context.PopFrame();
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.op.ToString());
        }
#endif
    }

    internal abstract class LiteralRelationOpcode : Opcode
    {
        internal LiteralRelationOpcode(OpcodeID id)
            : base(id)
        {
            this.flags |= OpcodeFlags.Literal;
        }
#if NO
        internal abstract ValueDataType DataType
        {
            get;
        }
#endif
        internal abstract object Literal
        {
            get;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} '{1}'", base.ToString(), this.Literal);
        }
#endif
    }

    internal class StringEqualsOpcode : LiteralRelationOpcode
    {
        string literal;

        internal StringEqualsOpcode(string literal)
            : base(OpcodeID.StringEquals)
        {
            Fx.Assert(null != literal, "");
            this.literal = literal;
        }
#if NO
        internal override ValueDataType DataType
        {
            get
            {
                return ValueDataType.String;
            }
        }
#endif
        internal override object Literal
        {
            get
            {
                return this.literal;
            }
        }

        internal override void Add(Opcode op)
        {
            StringEqualsOpcode strEqOp = op as StringEqualsOpcode;
            if (null == strEqOp)
            {
                base.Add(op);
                return;
            }
            Fx.Assert(null != this.prev, "");

            StringEqualsBranchOpcode branch = new StringEqualsBranchOpcode();
            this.prev.Replace(this, branch);
            branch.Add(this);
            branch.Add(strEqOp);
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                StringEqualsOpcode strEqOp = (StringEqualsOpcode)op;
                return (strEqOp.literal == this.literal);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            Value[] values = context.Values;
            StackFrame arg = context.TopArg;
            if (1 == arg.Count)
            {
                values[arg.basePtr].Update(context, values[arg.basePtr].Equals(this.literal));
            }
            else
            {
                for (int i = arg.basePtr; i <= arg.endPtr; ++i)
                {
                    values[i].Update(context, values[i].Equals(this.literal));
                }
            }

            return this.next;
        }
    }

    internal class NumberEqualsOpcode : LiteralRelationOpcode
    {
        double literal;

        internal NumberEqualsOpcode(double literal)
            : base(OpcodeID.NumberEquals)
        {
            this.literal = literal;
        }
#if NO
        internal override ValueDataType DataType
        {
            get
            {
                return ValueDataType.Double;
            }
        }
#endif
        internal override object Literal
        {
            get
            {
                return this.literal;
            }
        }

        internal override void Add(Opcode op)
        {
            NumberEqualsOpcode numEqOp = op as NumberEqualsOpcode;
            if (null == numEqOp)
            {
                base.Add(op);
                return;
            }

            Fx.Assert(null != this.prev, "");

            NumberEqualsBranchOpcode branch = new NumberEqualsBranchOpcode();
            this.prev.Replace(this, branch);
            branch.Add(this);
            branch.Add(numEqOp);
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                NumberEqualsOpcode numEqOp = (NumberEqualsOpcode)op;
                return (numEqOp.literal == this.literal);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            Value[] values = context.Values;
            StackFrame arg = context.TopArg;
            if (1 == arg.Count)
            {
                values[arg.basePtr].Update(context, values[arg.basePtr].Equals(this.literal));
            }
            else
            {
                for (int i = arg.basePtr; i <= arg.endPtr; ++i)
                {
                    values[i].Update(context, values[i].Equals(this.literal));
                }
            }
            return this.next;
        }
    }

    internal abstract class HashBranchIndex : QueryBranchIndex
    {
        Dictionary<object, QueryBranch> literals;

        internal HashBranchIndex()
        {
            this.literals = new Dictionary<object, QueryBranch>();
        }

        internal override int Count
        {
            get
            {
                return this.literals.Count;
            }
        }

        internal override QueryBranch this[object literal]
        {
            get
            {
                QueryBranch result;
                if (this.literals.TryGetValue(literal, out result))
                {
                    return result;
                }
                return null;
            }
            set
            {
                this.literals[literal] = value;
            }
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            foreach (QueryBranch branch in this.literals.Values)
            {
                branch.Branch.CollectXPathFilters(filters);
            }
        }

#if NO
        internal override IEnumerator GetEnumerator()
        {
            return this.literals.GetEnumerator();
        }
#endif

        internal override void Remove(object key)
        {
            this.literals.Remove(key);
        }

        internal override void Trim()
        {
            // Can't compact Hashtable
        }
    }

    internal class StringBranchIndex : HashBranchIndex
    {
        internal override void Match(int valIndex, ref Value val, QueryBranchResultSet results)
        {
            QueryBranch branch = null;
            if (ValueDataType.Sequence == val.Type)
            {
                NodeSequence sequence = val.Sequence;
                for (int i = 0; i < sequence.Count; ++i)
                {
                    branch = this[sequence.Items[i].StringValue()];
                    if (null != branch)
                    {
                        results.Add(branch, valIndex);
                    }
                }
            }
            else
            {
                Fx.Assert(val.Type == ValueDataType.String, "");
                branch = this[val.String];
                if (null != branch)
                {
                    results.Add(branch, valIndex);
                }
            }
        }
    }

    internal class StringEqualsBranchOpcode : QueryConditionalBranchOpcode
    {
        internal StringEqualsBranchOpcode()
            : base(OpcodeID.StringEqualsBranch, new StringBranchIndex())
        {
        }

        internal override LiteralRelationOpcode ValidateOpcode(Opcode opcode)
        {
            StringEqualsOpcode numOp = opcode as StringEqualsOpcode;
            if (null != numOp)
            {
                return numOp;
            }

            return null;
        }
    }

    internal class NumberBranchIndex : HashBranchIndex
    {
        internal override void Match(int valIndex, ref Value val, QueryBranchResultSet results)
        {
            QueryBranch branch = null;
            if (ValueDataType.Sequence == val.Type)
            {
                NodeSequence sequence = val.Sequence;
                for (int i = 0; i < sequence.Count; ++i)
                {
                    branch = this[sequence.Items[i].NumberValue()];
                    if (null != branch)
                    {
                        results.Add(branch, valIndex);
                    }
                }
            }
            else
            {
                branch = this[val.ToDouble()];
                if (null != branch)
                {
                    results.Add(branch, valIndex);
                }
            }
        }
    }

    internal class NumberEqualsBranchOpcode : QueryConditionalBranchOpcode
    {
        internal NumberEqualsBranchOpcode()
            : base(OpcodeID.NumberEqualsBranch, new NumberBranchIndex())
        {
        }

        internal override LiteralRelationOpcode ValidateOpcode(Opcode opcode)
        {
            NumberEqualsOpcode numOp = opcode as NumberEqualsOpcode;
            if (null != numOp)
            {
                return numOp;
            }

            return null;
        }
    }
}
