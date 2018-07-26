using System;
using System.Linq;
using System.Collections.Generic;

using Mono.Compiler;
using SimpleJit.Metadata;
using SimpleJit.CIL;

/// <summary>
///   Implemented "III.1.5 Operand type table"
/// </summary>
namespace Mono.Compiler.BigStep
{
    // III.1.5 Operand type table
    internal class OpResultTypeLookup 
    {

        internal static ClrType? Query(Opcode op, ExtendedOpcode? exop, params ClrType[] types)
        {
            if (exop.HasValue)
            {
                ExtendedOpcode exopcode = (ExtendedOpcode)exop;
                switch(exop)
                {  
                    // Table III.4: Binary Comparison or Branch Operations
                    case ExtendedOpcode.Ceq:
                    case ExtendedOpcode.Cgt:
                    case ExtendedOpcode.CgtUn:
                    case ExtendedOpcode.Clt:
                    case ExtendedOpcode.CltUn:
                        return RuntimeInformation.BoolType;
                }
            }

            switch(op)
            {
                // Table III.2: Binary Numeric Operations
                // Table III.7: Overflow Arithmetic Operations
                case Opcode.Add:
                case Opcode.AddOvf:
                case Opcode.AddOvfUn:
                case Opcode.Sub:
                case Opcode.SubOvf:
                case Opcode.SubOvfUn:
                case Opcode.Mul:
                case Opcode.MulOvf:
                case Opcode.MulOvfUn:
                case Opcode.Div:
                case Opcode.Rem:
                    return QueryBinaryOp(types[0], types[1]);
                // Table III.3: Unary Numeric Operations
                case Opcode.Neg:
                    if (types[0] == RuntimeInformation.Int32Type && types[1] == RuntimeInformation.Int32Type){
                        return RuntimeInformation.BoolType;
                    }
                    if (types[0] == RuntimeInformation.Int64Type && types[1] == RuntimeInformation.Int64Type){
                        return RuntimeInformation.BoolType;
                    }
                    if (types[0] == RuntimeInformation.TypedRefType && types[1] == RuntimeInformation.TypedRefType){
                        return RuntimeInformation.BoolType;
                    }
                    if ((types[0] == RuntimeInformation.Float32Type || types[0] == RuntimeInformation.Float64Type) && 
                        (types[1] == RuntimeInformation.Float32Type || types[1] == RuntimeInformation.Float64Type))
                    {
                        return RuntimeInformation.BoolType;
                    }
                    // This should never happen unless there are bugs in CSC.
                    throw new Exception($"Unexpected. Operation { op.ToString() } cannot perform on operands of type { types[0].AsSystemType.Name } and { types[1].AsSystemType.Name }");
                // Table III.4: Binary Comparison or Branch Operations
                case Opcode.Beq:
                case Opcode.BeqS:
                case Opcode.Bge:
                case Opcode.BgeS:
                case Opcode.BgeUn:
                case Opcode.BgeUnS:
                case Opcode.Bgt:
                case Opcode.BgtS:
                case Opcode.BgtUn:
                case Opcode.BgtUnS:
                case Opcode.Ble:
                case Opcode.BleS:
                case Opcode.BleUn:
                case Opcode.BleUnS:
                case Opcode.Blt:
                case Opcode.BltS:
                case Opcode.BltUn:
                case Opcode.BltUnS:
                case Opcode.BneUn:
                case Opcode.BneUnS:
                    return RuntimeInformation.BoolType;
                // Table III.5: Table III.5: Integer Operations
                case Opcode.And:
                case Opcode.Not:
                case Opcode.Or:
                case Opcode.Xor:
                case Opcode.RemUn:
                case Opcode.DivUn:
                    // Reuse matrix defined for bianry ops since the valid set is a subset of the latter 
                    // and we always assume the validaity of input operands.
                    return QueryBinaryOp(types[0], types[1]);
                // Table III.6: Shift Operations
                case Opcode.Shl:
                case Opcode.Shr:
                case Opcode.ShrUn:
                    // operand 0: To Be Shifted
                    // operand 1: Shift-By
                    if (types[0] == RuntimeInformation.Int32Type && 
                       (types[1] == RuntimeInformation.Int32Type || types[1] == RuntimeInformation.NativeIntType))
                    {
                        return RuntimeInformation.Int32Type;
                    }
                    if (types[0] == RuntimeInformation.Int64Type && 
                       (types[1] == RuntimeInformation.Int32Type || types[1] == RuntimeInformation.NativeIntType))
                    {
                        return RuntimeInformation.Int64Type;
                    }
                    if (types[0] == RuntimeInformation.NativeIntType && 
                       (types[1] == RuntimeInformation.Int32Type || types[1] == RuntimeInformation.NativeIntType))
                    {
                        return RuntimeInformation.NativeIntType;
                    }
                    // This should never happen unless there are bugs in CSC.
                    throw new Exception($"Unexpected. Operation { op.ToString() } cannot perform on operands of type { types[0].AsSystemType.Name } and { types[1].AsSystemType.Name }");
                // Table III.8: Conversion Operations
                case Opcode.ConvI1:
                case Opcode.ConvU1:
                case Opcode.ConvI2:
                case Opcode.ConvU2:
                case Opcode.ConvI4:
                case Opcode.ConvU4:
                case Opcode.ConvOvfI1:
                case Opcode.ConvOvfI1Un:
                case Opcode.ConvOvfI2:
                case Opcode.ConvOvfI2Un:
                case Opcode.ConvOvfI4:
                case Opcode.ConvOvfI4Un:
                case Opcode.ConvI:
                case Opcode.ConvOvfI:
                case Opcode.ConvU:
                case Opcode.ConvOvfU:
                    // For short integers,the stack value is truncated but remains the same type.
                    return types[0];
                case Opcode.ConvI8:
                case Opcode.ConvU8:
                    if (types[0] == RuntimeInformation.Float32Type || 
                       (types[0] == RuntimeInformation.Int32Type))
                    {
                        return RuntimeInformation.Int64Type;
                    }
                    else
                    {
                        return types[0];
                    }
                case Opcode.ConvR4:
                case Opcode.ConvR8:
                case Opcode.ConvRUn:
                    return RuntimeInformation.Float64Type;
                default:
                    return null;
            }
        }

        internal class CilTypePair 
        {
            private string notation;

            private CilTypePair (ClrType ta, ClrType tb) 
            {
                string sa = ClrTypeToString(ta);
                string sb = ClrTypeToString(tb);
                notation = sb + "-" + sb;
            }

            public override int GetHashCode()
            {
                return notation.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj != null && obj is CilTypePair){
                    return notation == ((CilTypePair)obj).notation;
                }

                return false;
            }

            internal static CilTypePair Create(ClrType ta, ClrType tb)
            {
                return new CilTypePair(ta, tb);
            }
        }

        // Convert the type to one of five types tracked by CLI for operator result verification
        private static String ClrTypeToString(ClrType type)
        {
            if (type == RuntimeInformation.Int32Type)
            {
                return "int32";
            }
            if (type == RuntimeInformation.Int64Type)
            {
                return "int64";
            } 
            if (type == RuntimeInformation.NativeIntType)
            {
                return "nativeint";
            } 
            else if (type == RuntimeInformation.TypedRefType)
            {
                return "&"; // Is this even correct?
            }
            else 
            {
                Type t = type.AsSystemType;
                if (t.IsClass) {
                    return "O";
                } else {
                    return "?";
                }
            }
        }

        internal static ClrType QueryBinaryOp(ClrType ta, ClrType tb)
        {
            return binaryOpResults[CilTypePair.Create(ta, tb)];
        }

        private static Dictionary<CilTypePair, ClrType> binaryOpResults;

        static OpResultTypeLookup() 
        {
            binaryOpResults = new Dictionary<CilTypePair, ClrType>();
            binaryOpResults[CilTypePair.Create(RuntimeInformation.Int32Type, RuntimeInformation.Int32Type)] 
                = RuntimeInformation.Int32Type;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.Int64Type, RuntimeInformation.Int64Type)] 
                = RuntimeInformation.Int64Type;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.Int32Type, RuntimeInformation.NativeIntType)] 
                = RuntimeInformation.NativeIntType;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.NativeIntType, RuntimeInformation.Int32Type)] 
                = RuntimeInformation.NativeIntType;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.NativeIntType, RuntimeInformation.NativeIntType)] 
                = RuntimeInformation.NativeIntType;

            // For F types, always result in long type for now. This needs re-visiting
            binaryOpResults[CilTypePair.Create(RuntimeInformation.Float32Type, RuntimeInformation.Float32Type)] 
                = RuntimeInformation.Float64Type;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.Float32Type, RuntimeInformation.Float64Type)] 
                = RuntimeInformation.Float64Type;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.Float64Type, RuntimeInformation.Float32Type)] 
                = RuntimeInformation.Float64Type;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.Float64Type, RuntimeInformation.Float64Type)] 
                = RuntimeInformation.Float64Type;

            // For & types, they are only applicable to certain OPs. 
            // But since we assume the validity of the input, do not perform such checks.
            binaryOpResults[CilTypePair.Create(RuntimeInformation.Int32Type, RuntimeInformation.TypedRefType)] 
                = RuntimeInformation.TypedRefType;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.NativeIntType, RuntimeInformation.TypedRefType)] 
                = RuntimeInformation.TypedRefType;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.TypedRefType, RuntimeInformation.Int32Type)] 
                = RuntimeInformation.TypedRefType;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.TypedRefType, RuntimeInformation.NativeIntType)] 
                = RuntimeInformation.TypedRefType;
            binaryOpResults[CilTypePair.Create(RuntimeInformation.TypedRefType, RuntimeInformation.TypedRefType)] 
                = RuntimeInformation.TypedRefType;
        }
    }
}