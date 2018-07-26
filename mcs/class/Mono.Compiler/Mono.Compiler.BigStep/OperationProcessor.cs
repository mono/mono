using System;
using System.Linq;
using System.Collections.Generic;

using Mono.Compiler;
using SimpleJit.Metadata;
using SimpleJit.CIL;

/// <summary>
///   Interface for operation processing driven by execution emulation.
/// </summary>
namespace Mono.Compiler.BigStep
{
    public class OperationInfo 
    {
        /// <summary> The operands may come from stack, arguments, locals or constants. </summary>
        internal IOperand[] Operands { get; set; }
        
        /// <summary> The result is pushed to the stack. </summary>
        internal TempOperand Result { get; set; }

        /// <summary> The operation. </summary>
        internal Opcode Operation { get; set; }

        /// <summary> The extended operation. Has value only if Operation == Opcode.ExtendedPrefix </summary>
        internal ExtendedOpcode? ExtOperation { get; set; }
    }

    public interface IOperationProcessor 
    {
        void Process(OperationInfo opInfo);
    }
}