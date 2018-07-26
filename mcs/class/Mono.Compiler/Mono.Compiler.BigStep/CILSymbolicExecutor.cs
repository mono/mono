using System;
using System.Linq;
using System.Collections.Generic;

using Mono.Compiler;
using SimpleJit.Metadata;
using SimpleJit.CIL;

namespace Mono.Compiler.BigStep
{
    /// <summary>
    ///   Emulate CIL execution and delegates further handling for each operation to a processor.
    /// </summary>
    /// <remarks>
    ///   This class partially implements stack-based virtual machine as codified by ECMA-335. It tracks
    ///   stack depth change and types associated with each operand, which may come from stack, locals
    ///   or arguments. Upon completion of each operation it invokes a processor to perform customized
    ///   operation. LLVM bitcode emitter is implemented as a processor.
    /// </remarks>
    public class CILSymbolicExecutor : INameGenerator
	{
        private IOperationProcessor processor;
        private BigStep.Env env;
        private IRuntimeInformation runtime;
        private MethodBody body;

        private Stack<TempOperand> stack;
        private int tempSeq = 0;

        private List<LocalOperand> locals;
        private List<ArgumentOperand> args;

        // INameGenerator
        public string NextName()
        {
            return (tempSeq++).ToString();
        }

		public CILSymbolicExecutor (
            IOperationProcessor processor,
            BigStep.Env env, 
            IRuntimeInformation runtime, 
            MethodInfo methodInfo)
		{
            this.processor = processor;
			this.env = env;
            this.runtime = runtime;
			this.body = methodInfo.Body;

            this.stack = new Stack<TempOperand>();

            this.locals = body.LocalVariables
                .Select(lvi => new LocalOperand(lvi.LocalIndex, lvi.LocalType))
                //.OrderBy(lod => lod.Name) // Not necessary since the input is sorted by index already
                .ToList();
            this.args = methodInfo.Parameters
                .Select(lvi => new ArgumentOperand(lvi.Position, lvi.ParameterType))
                //.OrderBy(lod => lod.Name) // Not necessary since the input is sorted by index already
                .ToList();
		}

        public void Execute()
        {
            var iter = body.GetIterator();
			while (iter.MoveNext()) {
				Opcode opcode = iter.Opcode;
                ExtendedOpcode? extOpCode = null;
				if (opcode == Opcode.ExtendedPrefix){
                    extOpCode = iter.ExtOpcode;
                }
				OpcodeFlags opflags = iter.Flags;
                int opParam = 0;
                IOperand output = null;

                // 1) Collect operands
                List<IOperand> operands = new List<IOperand>();
                // 1.1) operands not from stack
                switch(opcode){
                    // 1.1.1) operands from Arguments
                    case Opcode.Ldarg0:
                        operands.Add(output = args[0]);
                        break;
                    case Opcode.Ldarg1:
                        operands.Add(output = args[1]);
                        break;
                    case Opcode.Ldarg2:
                        operands.Add(output = args[2]);
                        break;
                    case Opcode.Ldarg3:
                        operands.Add(output = args[3]);
                        break;
                    case Opcode.LdargS:
                        opParam = iter.DecodeParamI();
                        operands.Add(output = args[opParam]);
                        break;
                    // 1.1.2) operands from Locals
                    case Opcode.Ldloc1:
                        operands.Add(output = locals[1]);
                        break;
                    case Opcode.Ldloc2:
                        operands.Add(output = locals[2]);
                        break;
                    case Opcode.Ldloc3:
                        operands.Add(output = locals[3]);
                        break;
                    case Opcode.LdlocS:
                        opParam = iter.DecodeParamI();
                        operands.Add(output = locals[opParam]);
                        break;
                    // 1.1.3) operands from constants
                    case Opcode.LdcI4_0:
                        operands.Add(output = new Int32ConstOperand(0));
                        break;
                    case Opcode.LdcI4_1:
                        operands.Add(output = new Int32ConstOperand(1));
                        break;
                    case Opcode.LdcI4_2:
                        operands.Add(output = new Int32ConstOperand(2));
                        break;
                    case Opcode.LdcI4_3:
                        operands.Add(output = new Int32ConstOperand(3));
                        break;
                    case Opcode.LdcI4_4:
                        operands.Add(output = new Int32ConstOperand(4));
                        break;
                    case Opcode.LdcI4_5:
                        operands.Add(new Int32ConstOperand(5));
                        break;
                    case Opcode.LdcI4_6:
                        operands.Add(output = new Int32ConstOperand(6));
                        break;
                    case Opcode.LdcI4_7:
                        operands.Add(output = new Int32ConstOperand(7));
                        break;
                    case Opcode.LdcI4M1:
                        operands.Add(output = new Int32ConstOperand(-1));
                        break;
                    case Opcode.LdcI4:
                    case Opcode.LdcI4S:
                        opParam = iter.DecodeParamI();
                        operands.Add(output = new Int32ConstOperand(opParam));
                        break;
                    case Opcode.LdcI8:
                    case Opcode.LdcR4:
                    case Opcode.LdcR8:
                        throw new Exception($"TODO: Cannot handle {opcode.ToString()} yet.");
                    // TODO:  ExtendedOpcode.Ldloc
                }

                // 1.2) operands to be popped from stack
				PopBehavior popbhv = iter.PopBehavior;
                int popCount = 0;
                switch(popbhv){
                    case PopBehavior.Pop0:
                        popCount = 0;
                        break;
                    case PopBehavior.Pop1:
                    case PopBehavior.Popi:
                    case PopBehavior.Popref:
                        popCount = 1;
                        break;
                    case PopBehavior.Pop1_pop1:
                    case PopBehavior.Popi_popi:
                    case PopBehavior.Popi_popi8:
                    case PopBehavior.Popi_popr4:
                    case PopBehavior.Popi_popr8:
                    case PopBehavior.Popref_pop1:
                    case PopBehavior.Popi_pop1:
                    case PopBehavior.Popref_popi:
                        popCount = 2;
                        break;
                    case PopBehavior.Popi_popi_popi:
                    case PopBehavior.Popref_popi_popi:
                    case PopBehavior.Popref_popi_popi8:
                    case PopBehavior.Popref_popi_popr4:
                    case PopBehavior.Popref_popi_popr8:
                    case PopBehavior.Popref_popi_popref:
                        popCount = 3;
                        break;
                    case PopBehavior.PopAll:
                        popCount = stack.Count;
                        break;
                    case PopBehavior.Varpop:
                        throw new Exception("TODO: Cannot handle PopBehavior.Varpop yet.");
                }

                int count = popCount;
                ClrType[] exprOdTypes = new ClrType[count];
                while (count > 0) 
                {
                    TempOperand tmp = stack.Pop();
                    operands.Add(tmp);
                    exprOdTypes[count - 1] = tmp.Type;
                    count--;
                }

                // Additional operands
                switch (opcode)
                {
                    case Opcode.Stloc0:
                        operands.Add(locals[0]);
                        break;
                    case Opcode.Stloc1:
                        operands.Add(locals[1]);
                        break;
                    case Opcode.Stloc2:
                        operands.Add(locals[2]);
                        break;
                    case Opcode.Stloc3:
                        operands.Add(locals[3]);
                        break;
                    case Opcode.StlocS:
                        opParam = iter.DecodeParamI();
                        operands.Add(locals[opParam]);
                        break;
                    // TODO:  ExtendedOpcode.Stloc
                }

                // 2) Determine the result type for values to push into stack
                TempOperand tod = null;
                if (output != null) 
                {
                    tod = new TempOperand(this, output.Type);
                }
                else
                {
                    ClrType? ctyp = OpResultTypeLookup.Query(opcode, extOpCode, exprOdTypes);
                    if (ctyp.HasValue) 
                    {
                        tod = new TempOperand(this, (ClrType)ctyp);
                    }
                }

                // 3) Push result
				PushBehavior pushbhv = iter.PushBehavior;
                switch(pushbhv)
                {
                    case PushBehavior.Push0:
                        break;
                    case PushBehavior.Push1:
                    case PushBehavior.Pushi:
                    case PushBehavior.Pushi8:
                    case PushBehavior.Pushr4:
                    case PushBehavior.Pushr8:
                        if (tod == null) {
                            throw new Exception("Unexpected: no value to push to stack.");
                        }
                        stack.Push(tod);
                        break;
                    case PushBehavior.Push1_push1:
                        // This only applies to Opcode.Dup
                        if (tod == null) {
                            throw new Exception("Unexpected: no value to push to stack.");
                        }
                        stack.Push(tod);
                        stack.Push(tod);
                        break;
                    case PushBehavior.Varpush:
                        // This is a huge TODO. Function call (Opcode.Call/Calli/Callvirt) relies on this behavior.
                        throw new Exception("TODO: Cannot handle PushBehavior.Varpush yet.");
                }

                // 4) Send the info to operation processor
                OperationInfo opInfo = new OperationInfo
                {
                    Operation = opcode,
                    ExtOperation = extOpCode,
                    Operands = operands.ToArray(),
                    Result = tod
                };
                processor.Process(opInfo);
			}
        }
    }
}