using System;
using System.Linq;
using System.Collections.Generic;

using Mono.Compiler;
using SimpleJit.Metadata;
using SimpleJit.CIL;

/// <summary>
///   Operands used by CIL execution emulator.
/// </summary>
namespace Mono.Compiler.BigStep
{
    internal enum OperandType 
    {
        /// <summary> The operand is a method argument. </summary>
        Argument,
        /// <summary> The operand is a user-defined local variable. </summary>
        Local,
        /// <summary> The operand is a machine-defined local variable for temporary use. </summary>
        Temp,
        /// <summary> The operand is a constant value stored as part of the instruction. </summary>
        Const
    }

    internal interface IOperand 
    {
        string Name { get; }
        ClrType Type { get; }
        OperandType OperandType { get; }
    }

    abstract internal class Operand : IOperand 
    {
        public string Name { get; private set; }
        public ClrType Type { get; private set; }

        internal Operand(string name, ClrType type) {
            Name = name;
            Type = type;
        }

        public virtual OperandType OperandType { get; }
    }

    internal class ArgumentOperand : Operand 
    {
        public int Index { get; private set; }

        internal ArgumentOperand(int index, ClrType type) 
        : base("A" + index, type) 
        {
            Index = index;
        }

        public override OperandType OperandType => OperandType.Argument;
    }

    internal class LocalOperand : Operand 
    {
        public int Index { get; private set; }

        internal LocalOperand(int index, ClrType type) 
            : base("L" + index, type) 
        {
            Index = index;
        }

        public override OperandType OperandType => OperandType.Local;
    }

    internal abstract class ConstOperand : Operand 
    {

        protected ConstOperand(ClrType type) 
            : base("const", type) // name is not important, so always use "const"
        {
        }

        public override OperandType OperandType => OperandType.Const;
    }

    internal class Int32ConstOperand : ConstOperand 
    {

        public int Value { get; private set; }

        internal Int32ConstOperand(int value) 
        : base(RuntimeInformation.Int32Type) 
        {
            Value = value;
        }
    }

    internal class Int64ConstOperand : ConstOperand 
    {

        public long Value { get; private set; }

        internal Int64ConstOperand(long value) 
            : base(RuntimeInformation.Int64Type) 
        {
            Value = value;
        }
    }

    internal class Float32ConstOperand : ConstOperand 
    {

        public float Value { get; private set; }

        internal Float32ConstOperand(float value) 
            : base(RuntimeInformation.Float32Type) 
        {
            Value = value;
        }
    }

    internal class Float64ConstOperand : ConstOperand {

        public double Value { get; private set; }

        internal Float64ConstOperand(double value) 
        : base(RuntimeInformation.Float64Type) 
        {
            Value = value;
        }
    }

    internal class TempOperand : Operand 
    {

        internal TempOperand(INameGenerator nameGen, ClrType type) 
            : base(nameGen.NextName(), type) 
        {
        }

        public override OperandType OperandType => OperandType.Temp;
    }

    public interface INameGenerator 
    {
        string NextName();
    }
}

 