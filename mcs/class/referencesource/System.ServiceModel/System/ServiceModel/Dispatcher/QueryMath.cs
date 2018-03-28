//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;

    internal enum MathOperator
    {
        None,
        Plus,
        Minus,
        Div,
        Multiply,
        Mod,
        Negate
    }

    internal class MathOpcode : Opcode
    {
        MathOperator mathOp;

        internal MathOpcode(OpcodeID id, MathOperator op)
            : base(id)
        {
            this.mathOp = op;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return (this.mathOp == ((MathOpcode) op).mathOp);
            }

            return false;
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.mathOp.ToString());
        }
#endif
    }

    internal class PlusOpcode : MathOpcode
    {
        internal PlusOpcode()
            : base(OpcodeID.Plus, MathOperator.Plus)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame argX = context.TopArg;
            StackFrame argY = context.SecondArg;
            Fx.Assert(argX.Count == argY.Count, "");

            Value[] values = context.Values;

            for (int x = argX.basePtr, y = argY.basePtr; x <= argX.endPtr; ++x, ++y)
            {
                Fx.Assert(values[x].IsType(ValueDataType.Double), "");
                Fx.Assert(values[y].IsType(ValueDataType.Double), "");
                values[y].Add(values[x].Double);
            }

            context.PopFrame();
            return this.next;
        }
    }

    internal class MinusOpcode : MathOpcode
    {
        internal MinusOpcode()
            : base(OpcodeID.Minus, MathOperator.Minus)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame argX = context.TopArg;
            StackFrame argY = context.SecondArg;
            Fx.Assert(argX.Count == argY.Count, "");
            
            Value[] values = context.Values;

            for (int x = argX.basePtr, y = argY.basePtr; x <= argX.endPtr; ++x, ++y)
            {
                Fx.Assert(values[x].IsType(ValueDataType.Double), "");
                Fx.Assert(values[y].IsType(ValueDataType.Double), "");
                values[y].Double = values[x].Double - values[y].Double;
            }

            context.PopFrame();
            return this.next;
        }
    }

    internal class MultiplyOpcode : MathOpcode
    {
        internal MultiplyOpcode()
            : base(OpcodeID.Multiply, MathOperator.Multiply)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame argX = context.TopArg;
            StackFrame argY = context.SecondArg;
            Fx.Assert(argX.Count == argY.Count, "");

            Value[] values = context.Values;

            for (int x = argX.basePtr, y = argY.basePtr; x <= argX.endPtr; ++x, ++y)
            {
                Fx.Assert(values[x].IsType(ValueDataType.Double), "");
                Fx.Assert(values[y].IsType(ValueDataType.Double), "");
                values[y].Multiply(values[x].Double);
            }

            context.PopFrame();
            return this.next;
        }
    }

    internal class DivideOpcode : MathOpcode
    {
        internal DivideOpcode()
            : base(OpcodeID.Divide, MathOperator.Div)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame argX = context.TopArg;
            StackFrame argY = context.SecondArg;
            Fx.Assert(argX.Count == argY.Count, "");
            Value[] values = context.Values;

            for (int x = argX.basePtr, y = argY.basePtr; x <= argX.endPtr; ++x, ++y)
            {
                Fx.Assert(values[x].IsType(ValueDataType.Double), "");
                Fx.Assert(values[y].IsType(ValueDataType.Double), "");
                values[y].Double = values[x].Double / values[y].Double;
            }

            context.PopFrame();
            return this.next;
        }
    }

    internal class ModulusOpcode : MathOpcode
    {
        internal ModulusOpcode()
            : base(OpcodeID.Mod, MathOperator.Mod)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame argX = context.TopArg;
            StackFrame argY = context.SecondArg;
            Value[] values = context.Values;

            Fx.Assert(argX.Count == argY.Count, "");
            for (int x = argX.basePtr, y = argY.basePtr; x <= argX.endPtr; ++x, ++y)
            {
                Fx.Assert(values[x].IsType(ValueDataType.Double), "");
                Fx.Assert(values[y].IsType(ValueDataType.Double), "");
                values[y].Double = values[x].Double % values[y].Double;
            }

            context.PopFrame();
            return this.next;
        }
    }

    internal class NegateOpcode : MathOpcode
    {
        internal NegateOpcode()
            : base(OpcodeID.Negate, MathOperator.Negate)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame frame = context.TopArg;
            Value[] values = context.Values;

            for (int i = frame.basePtr; i <= frame.endPtr; ++i)
            {
                values[i].Negate();
            }
            return this.next;
        }
    }

}
