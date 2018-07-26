using System;
using System.Collections.Generic;
using System.Threading;

using Mono.Compiler;
using SimpleJit.Metadata;
using SimpleJit.CIL;

using LLVMSharp;

/// <summary>
///   Emit LLVM bitcode via LLVMSharp.
/// </summary>
namespace Mono.Compiler.BigStep.LLVMBackend
{
	public class BitCodeEmitter :  IOperationProcessor
	{
		private const string FirstBasicBlock = "entry";
		private static readonly LLVMBool Success = new LLVMBool (0);
		private static readonly LLVMBool True = new LLVMBool (1);

		private static int moduleSeq;

		private LLVMModuleRef module;
		private LLVMBuilderRef builder;
		private LLVMValueRef function;
		private LLVMBasicBlockRef entry;

		private LLVMValueRef[] argAddrs;
		private LLVMValueRef[] localAddrs;
		private Dictionary<string, LLVMValueRef> temps;

		public BitCodeEmitter(IRuntimeInformation runtimeInfo, MethodInfo method)
        {
			int seq = Interlocked.Increment(ref moduleSeq);
			string modName = "llvmmodule_" + seq;
			module = LLVM.ModuleCreateWithName(modName);
			builder = LLVM.CreateBuilder();
			temps = new Dictionary<string, LLVMValueRef>();
			
			IReadOnlyCollection<ParameterInfo> prms = method.Parameters;
			LLVMTypeRef[] largs = new LLVMTypeRef[prms.Count];
			int i = 0;
			foreach(ParameterInfo pinfo in prms) {
				largs[i] = TranslateType(pinfo.ParameterType);
			}
			LLVMTypeRef rtyp = TranslateType(method.ReturnType);

			var funTy = LLVM.FunctionType(rtyp, largs, false);
			string funcName = modName + "_" + method.Name;
			function = LLVM.AddFunction(module, funcName, funTy);
			entry = LLVM.AppendBasicBlock(function, FirstBasicBlock);
			LLVM.PositionBuilderAtEnd(builder, entry);

			IList<LocalVariableInfo> locals = method.Body.LocalVariables;
			AllocateArgsAndLocals(largs, locals);
		}

		private void AllocateArgsAndLocals(LLVMTypeRef[] args, IList<LocalVariableInfo> locals)
		{
			this.argAddrs = new LLVMValueRef[args.Length];
			uint i = 0;
			for (; i < args.Length; i++)
			{
				LLVMValueRef vref = LLVM.GetParam(function, i);
				LLVMValueRef vaddr = LLVM.BuildAlloca(builder, args[i], "A" + i);
				LLVM.BuildStore(builder, vref, vaddr);
				this.argAddrs[i] = vaddr;
			}

			i = 0;
			foreach (LocalVariableInfo lvi in locals)
			{
				LLVMTypeRef ltyp = TranslateType(lvi.LocalType);
				LLVMValueRef lref = LLVM.BuildAlloca(builder, ltyp, "L" + i);
				this.localAddrs[i] = lref;
			}
		}

		// Emit LLVM instruction per CIL operation
        public void Process(OperationInfo opInfo)
		{
			Opcode op = opInfo.Operation;
			ExtendedOpcode? exop = opInfo.ExtOperation;
			IOperand[] operands = opInfo.Operands;
			TempOperand result = opInfo.Result;

			LLVMValueRef addr0;
			LLVMValueRef tmp;
			switch(op)
			{
				case Opcode.Nop:
					break;
				case Opcode.Ldarg0:
					// arg => tmp
					addr0 = GetArgAddr(operands, 0);
					tmp = LLVM.BuildLoad(builder, addr0, result.Name);
					SetTempValue(tmp, result.Name);
					break;
				case Opcode.Stloc0:
					// tmp, local
					tmp = GetTempValue(operands, 0);
					addr0 = GetLocalAddr(operands, 1);
					LLVM.BuildStore(builder, tmp, addr0);
					break;
			}
		}

		private LLVMValueRef GetArgAddr(IOperand[] operands, int index)
		{
			ArgumentOperand aod = (ArgumentOperand)operands[index];
			return this.argAddrs[aod.Index];
		}

		private LLVMValueRef GetLocalAddr(IOperand[] operands, int index)
		{
			LocalOperand lod = (LocalOperand)operands[index];
			return this.localAddrs[lod.Index];
		}

		private LLVMValueRef GetTempValue(IOperand[] operands, int index)
		{
			TempOperand tod = (TempOperand)operands[index];
			return temps[tod.Name];
		}

		private void SetTempValue(LLVMValueRef tmp, string name)
		{
			temps[name] = tmp;
		}

		private void SetTempValue(LLVMValueRef tmp)
		{
			String name = LLVM.GetValueName(tmp);
			temps[name] = tmp;
		}

		private static LLVMTypeRef TranslateType(ClrType ctyp)
		{
			if (ctyp == RuntimeInformation.BoolType)
			{
				return LLVM.Int1Type();
			}
			if (ctyp == RuntimeInformation.Int8Type)
			{
				return LLVM.Int8Type();
			}
			if (ctyp == RuntimeInformation.Int16Type || ctyp == RuntimeInformation.Int8Type)
			{
				return LLVM.Int16Type();
			}
			if (ctyp == RuntimeInformation.Int32Type || ctyp == RuntimeInformation.UInt16Type)
			{
				return LLVM.Int32Type();
			}
			if (ctyp == RuntimeInformation.Int64Type || ctyp == RuntimeInformation.UInt32Type)
			{
				return LLVM.Int64Type();
			}
			if (ctyp == RuntimeInformation.CharType)
			{
				return LLVM.Int16Type(); // Unicode
			}
			if (ctyp == RuntimeInformation.Float32Type || ctyp == RuntimeInformation.Float64Type)
			{
				return LLVM.FloatType();
			}
			if (ctyp == RuntimeInformation.NativeIntType || ctyp == RuntimeInformation.NativeUnsignedIntType)
			{
				return LLVM.Int64Type();
			}
			if (ctyp == RuntimeInformation.StringType)
			{
				return LLVM.PointerType(LLVM.Int16Type(), 0); // 0 = default address sapce 
			}
			if (ctyp == RuntimeInformation.VoidTypeInstance)
			{
				return LLVM.VoidType();
			}
			
			Type typ = ctyp.AsSystemType;
			if (typ.IsClass) 
			{
				return LLVM.PointerType(LLVM.Int64Type(), 0); // 0 = default address sapce 
			}

			throw new Exception($"TODO: Cannot handle type { typ.Name } yet.");
		}
    }
}