//
// Decompile.cs
//
// Authors:
//	Chris Bacon (chrisbacon76@gmail.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Rewrite.Ast;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mono.CodeContracts.Rewrite {

	class Decompile {

		public Decompile (MethodDefinition method)
		{
			this.method = method;
			this.exprs = new Stack<Expr> ();
			this.Instructions = new Dictionary<Expr, Instruction> ();
			this.methodInfo = new MethodInfo (method);
			this.gen = new ExprGen (this.methodInfo);
		}

		private MethodInfo methodInfo;
		private MethodDefinition method;
		private Stack<Expr> exprs;
		private ExprGen gen;

		public Dictionary<Expr, Instruction> Instructions { get; private set; }

		public Expr Go (bool failQuietly = true)
		{
			Instruction unknownInst = null;
			var insts = this.method.Body.Instructions;
			foreach (var inst in insts) {
				if (failQuietly) {
					if (unknownInst == null) {
						try {
							Expr expr = this.ProcessInst (inst);
							this.Instructions.Add (expr, inst);
							this.exprs.Push (expr);
						} catch (NotSupportedException) {
							unknownInst = inst;
						}
					} else {
						// Met unknown instruction, so check that there are no more contracts
						if (inst.OpCode.OperandType == OperandType.InlineMethod) {
							MethodReference method = (MethodReference) inst.Operand;
							if (method.DeclaringType.FullName == "System.Diagnostics.Contracts.Contract") {
								throw new NotSupportedException ("Unknown instruction in contract: " + unknownInst);
							}
						}
					}
				} else {
					Expr expr = this.ProcessInst (inst);
					this.Instructions.Add (expr, inst);
					this.exprs.Push (expr);
				}
			}

			Expr decompiled = new ExprBlock (this.methodInfo, this.exprs.Reverse ().ToArray ());
			return decompiled;
		}

		private Expr ProcessInst (Instruction inst)
		{
			var opcode = inst.OpCode.Code;
			switch (opcode) {
			case Code.Nop:
				return this.gen.Nop ();
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				return this.gen.LoadArg ((int) (opcode - Code.Ldarg_0));
			case Code.Ldarg_S:
				return this.gen.LoadArg ((ParameterDefinition) inst.Operand);
			case Code.Ldnull:
				return this.gen.LoadConstant (null);
			case Code.Ldc_I4_M1:
			case Code.Ldc_I4_0:
			case Code.Ldc_I4_1:
			case Code.Ldc_I4_2:
			case Code.Ldc_I4_3:
			case Code.Ldc_I4_4:
			case Code.Ldc_I4_5:
			case Code.Ldc_I4_6:
			case Code.Ldc_I4_7:
			case Code.Ldc_I4_8:
				return this.gen.LoadConstant ((int) (opcode - Code.Ldc_I4_0));
			case Code.Ldc_I4_S:
				return this.gen.LoadConstant ((int) (sbyte) inst.Operand);
			case Code.Ldc_I4:
				return this.gen.LoadConstant ((int) inst.Operand);
			case Code.Ldc_R4:
			case Code.Ldc_R8:
			case Code.Ldstr:
				return this.gen.LoadConstant(inst.Operand);
			case Code.Clt:
			case Code.Clt_Un:
			case Code.Cgt:
			case Code.Cgt_Un:
			case Code.Ceq:
			case Code.Add:
			case Code.Sub:
				return this.ProcessBinaryOp (opcode);
			case Code.Call:
				return this.ProcessCall ((MethodReference) inst.Operand);
			case Code.Ret:
				return this.gen.Return ();
			case Code.Conv_I4:
				return this.ProcessConv (TypeCode.Int32);
			case Code.Conv_I8:
				return this.ProcessConv (TypeCode.Int64);
			default:
				throw new NotSupportedException ("Cannot handle opcode: " + inst.OpCode);
			}
		}

		private Expr ProcessBinaryOp (Code opcode)
		{
			Expr right = this.exprs.Pop ();
			Expr left = this.exprs.Pop ();
			switch (opcode) {
			case Code.Ceq:
				return this.gen.CompareEqual (left, right);
			case Code.Clt:
				return this.gen.CompareLessThan (left, right, Sn.Signed);
			case Code.Clt_Un:
				return this.gen.CompareLessThan (left, right, Sn.Unsigned);
			case Code.Cgt:
				return this.gen.CompareGreaterThan (left, right, Sn.Signed);
			case Code.Cgt_Un:
				return this.gen.CompareGreaterThan (left, right, Sn.Unsigned);
			case Code.Add:
				return this.gen.Add (left, right, Sn.None, false);
			case Code.Sub:
				return this.gen.Sub (left, right, Sn.None, false);
			default:
				throw new NotSupportedException ("Unknown binary opcode: " + opcode);
			}
		}

		private Expr ProcessCall (MethodReference method)
		{
			int paramCount = method.Parameters.Count;
			Expr [] parameterExprs = new Expr [paramCount];
			for (int i = 0; i < paramCount; i++) {
				Expr parameter = this.exprs.Pop ();
				parameterExprs [paramCount - i - 1] = parameter;
			}
			return this.gen.Call(method, parameterExprs);
		}

		private Expr ProcessConv (TypeCode convToType)
		{
			Expr exprToConvert = this.exprs.Pop ();
			return this.gen.Conv(exprToConvert, convToType);
		}

	}
}
