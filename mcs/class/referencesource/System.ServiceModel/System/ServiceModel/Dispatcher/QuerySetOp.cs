//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;

    class OrdinalOpcode : Opcode
    {
        internal OrdinalOpcode()
            : base(OpcodeID.Ordinal)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame sequences = context.TopSequenceArg;
            StackFrame ordinals = context.TopArg;
            Value[] sequenceBuffer = context.Sequences;

            for (int seqIndex = sequences.basePtr, ordinalIndex = ordinals.basePtr; seqIndex <= sequences.endPtr; ++seqIndex)
            {
                NodeSequence sequence = sequenceBuffer[seqIndex].Sequence;
                for (int item = 0; item < sequence.Count; ++item)
                {
                    context.Values[ordinalIndex].Boolean = (sequence[item].Position == context.Values[ordinalIndex].Double);
                    ordinalIndex++;
                }
            }

            return this.next;
        }
    }

    internal class LiteralOrdinalOpcode : Opcode
    {
        int ordinal; // 1 based

        internal LiteralOrdinalOpcode(int ordinal)
            : base(OpcodeID.LiteralOrdinal)
        {
            Fx.Assert(ordinal > 0, "");
            this.ordinal = ordinal;
        }
#if NO
        // Never used for inverse query, so don't need this
        internal override bool Equals(Opcode op)
        {
            if (base.Equals (op))
            {
                return (this.ordinal == ((LiteralOrdinalOpcode) op).ordinal);
            }

            return false;
        }
#endif
        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame sequences = context.TopSequenceArg;
            Value[] sequenceBuffer = context.Sequences;

            context.PushFrame();
            for (int i = sequences.basePtr; i <= sequences.endPtr; ++i)
            {
                NodeSequence sequence = sequenceBuffer[i].Sequence;
                for (int item = 0; item < sequence.Count; ++item)
                {
                    context.Push(sequence[item].Position == this.ordinal);
                }
            }
            return this.next;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.ordinal);
        }
#endif
    }

    // Filters context sequences using the results of the last executed predicate
    // Does pop result values
    internal class ApplyFilterOpcode : Opcode
    {
        internal ApplyFilterOpcode()
            : base(OpcodeID.Filter)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame sequences = context.TopSequenceArg;
            StackFrame results = context.TopArg;
            NodeSequenceBuilder sequenceBuilder = new NodeSequenceBuilder(context);
            Value[] sequenceBuffer = context.Sequences;

            for (int seqIndex = sequences.basePtr, resultIndex = results.basePtr; seqIndex <= sequences.endPtr; ++seqIndex)
            {
                NodeSequence sourceSequence = sequenceBuffer[seqIndex].Sequence;
                if (sourceSequence.Count > 0)
                {
                    NodesetIterator nodesetIterator = new NodesetIterator(sourceSequence);
                    while (nodesetIterator.NextNodeset())
                    {
                        sequenceBuilder.StartNodeset();
                        while (nodesetIterator.NextItem())
                        {
                            Fx.Assert(context.Values[resultIndex].IsType(ValueDataType.Boolean), "");
                            if (context.Values[resultIndex].Boolean)
                            {
                                sequenceBuilder.Add(ref sourceSequence.Items[nodesetIterator.Index]);
                            }
                            ++resultIndex;
                        }
                        sequenceBuilder.EndNodeset();
                    }
                    context.ReplaceSequenceAt(seqIndex, sequenceBuilder.Sequence);
                    context.ReleaseSequence(sourceSequence);
                    sequenceBuilder.Sequence = null;
                }
            }

            context.PopFrame();
            return this.next;
        }
    }

    /// <summary>
    /// Union the sequences found in the top two frames of the value stack
    /// The unionized sequence 
    /// </summary>
    internal class UnionOpcode : Opcode
    {
        internal UnionOpcode()
            : base(OpcodeID.Union)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;

            Fx.Assert(topArg.Count == secondArg.Count, "");
            for (int x = topArg.basePtr, y = secondArg.basePtr; x <= topArg.endPtr; ++x, ++y)
            {
                NodeSequence seqX = context.Values[x].Sequence;
                NodeSequence seqY = context.Values[y].Sequence;

                // Replace with a new sequence that is the union of the two
                context.SetValue(context, y, seqY.Union(context, seqX));
            }

            context.PopFrame();
            return this.next;
        }
    }

    internal class MergeOpcode : Opcode
    {
        internal MergeOpcode()
            : base(OpcodeID.Merge)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            Value[] values = context.Values;
            StackFrame arg = context.TopArg;

            for (int i = arg.basePtr; i <= arg.endPtr; ++i)
            {
                Fx.Assert(ValueDataType.Sequence == values[i].Type, "");
                NodeSequence seq = values[i].Sequence;

                NodeSequence newSeq = context.CreateSequence();
                for (int j = 0; j < seq.Count; ++j)
                {
                    NodeSequenceItem item = seq[j];
                    newSeq.AddCopy(ref item);
                }
                newSeq.Merge();

                context.SetValue(context, i, newSeq);
            }

            return this.next;
        }

    }
}
