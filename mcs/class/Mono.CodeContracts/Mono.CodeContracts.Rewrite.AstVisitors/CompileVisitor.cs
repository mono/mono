//
// CompileVisitor.cs
//
// Authors:
//	Chris Bacon (chrisbacon76@gmail.com)
//
// Copyright (C) 2010 Chris Bacon
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
using Mono.Cecil.Cil;
using Mono.CodeContracts.Rewrite.Ast;

namespace Mono.CodeContracts.Rewrite.AstVisitors {
	class CompileVisitor : ExprVisitor {

		public CompileVisitor (ILProcessor il, Dictionary<Expr, Instruction> instructionLookup)
			: this (il, instructionLookup, il.Append)
		{
		}

		public CompileVisitor (ILProcessor il, Dictionary<Expr, Instruction> instructionLookup, Action<Instruction> fnEmit)
		{
			this.il = il;
			this.instructionLookup = instructionLookup;
			this.fnEmit = fnEmit;
		}

		private ILProcessor il;
		private Dictionary<Expr, Instruction> instructionLookup;
		private Action<Instruction> fnEmit;

		private void Emit (Expr originalExpr, Instruction inst)
		{
			Instruction originalInst;
			if (this.instructionLookup != null) {
				// TODO: Doesn't handle inherited contracts - need to check what to do in this case.
				if (this.instructionLookup.TryGetValue (originalExpr, out originalInst)) {
					inst.SequencePoint = originalInst.SequencePoint;
				}
			}
			this.fnEmit (inst);
		}

		private void Emit (Expr originalExpr, Func<Instruction> fnCreateInstruction)
		{
			Instruction inst = fnCreateInstruction();
			this.Emit (originalExpr, inst);
		}

		private void Emit (Expr originalExpr, Func<IEnumerable<Instruction>> fnCreateInstruction)
		{
			throw new NotImplementedException ();
		}

		protected override Expr VisitNop (ExprNop e)
		{
			var instNop = this.il.Create (OpCodes.Nop);
			this.Emit (e, instNop);
			return e;
		}

		protected override Expr VisitLoadArg (ExprLoadArg e)
		{
			this.Emit (e, () => {
				int index = e.Index;
				switch (index) {
				case 0:
					return this.il.Create (OpCodes.Ldarg_0);
				case 1:
					return this.il.Create (OpCodes.Ldarg_1);
				case 2:
					return this.il.Create (OpCodes.Ldarg_2);
				case 3:
					return this.il.Create (OpCodes.Ldarg_3);
				default:
					if (e.Index <= 255) {
						return this.il.Create (OpCodes.Ldarg_S, (byte) index);
					} else {
						return this.il.Create (OpCodes.Ldarg, index);
					}
				}
				// Required due to bug in compiler
				throw new NotSupportedException();
			});
			
			return e;
		}

		protected override Expr VisitLoadConstant (ExprLoadConstant e)
		{
			this.Emit (e, () => {
				object v = e.Value;
				if (v == null) {
					return this.il.Create (OpCodes.Ldnull);
				}
				Type vType = v.GetType ();
				TypeCode vTypeCode = Type.GetTypeCode (vType);
				switch (vTypeCode) {
				case TypeCode.Int32:
					int value = (int) v;
					switch (value) {
					case -1:
						return this.il.Create (OpCodes.Ldc_I4_M1);
					case 0:
						return this.il.Create (OpCodes.Ldc_I4_0);
					case 1:
						return this.il.Create (OpCodes.Ldc_I4_1);
					case 2:
						return this.il.Create (OpCodes.Ldc_I4_2);
					case 3:
						return this.il.Create (OpCodes.Ldc_I4_3);
					case 4:
						return this.il.Create (OpCodes.Ldc_I4_4);
					case 5:
						return this.il.Create (OpCodes.Ldc_I4_5);
					case 6:
						return this.il.Create (OpCodes.Ldc_I4_6);
					case 7:
						return this.il.Create (OpCodes.Ldc_I4_7);
					case 8:
						return this.il.Create (OpCodes.Ldc_I4_8);
					default:
						if (value >= -128 && value <= 127) {
							return this.il.Create (OpCodes.Ldc_I4_S, (sbyte) value);
						} else {
							return this.il.Create (OpCodes.Ldc_I4, value);
						}
					}
					// Required due to bug in compiler
					throw new NotSupportedException();
				case TypeCode.Single:
					return this.il.Create (OpCodes.Ldc_R4, (float) v);
				case TypeCode.Double:
					return this.il.Create (OpCodes.Ldc_R8, (double) v);
				case TypeCode.String:
					return this.il.Create (OpCodes.Ldstr, (string) v);
				default:
					throw new NotSupportedException ("Cannot handle constant: " + vTypeCode);
				}
				// Required due to bug in compiler
				throw new NotSupportedException();
			});

			return e;
		}

		private Expr VisitBinary (ExprBinaryOp e, Func<Instruction> fnCreateIl)
		{
			this.Visit (e.Left);
			this.Visit (e.Right);
			var inst = fnCreateIl ();
			this.Emit (e, inst);
			return e;
		}

		protected override Expr VisitCompareLessThan (ExprCompareLessThan e)
		{
			return this.VisitBinary (e, () => this.il.Create (e.IsSigned ? OpCodes.Clt : OpCodes.Clt_Un));
		}

		protected override Expr VisitCompareGreaterThan (ExprCompareGreaterThan e)
		{
			return this.VisitBinary (e, () => this.il.Create (e.IsSigned ? OpCodes.Cgt : OpCodes.Cgt_Un));
		}

		protected override Expr VisitCompareEqual (ExprCompareEqual e)
		{
			return this.VisitBinary (e, () => this.il.Create (OpCodes.Ceq));
		}

		protected override Expr VisitAdd (ExprAdd e)
		{
			return this.VisitBinary (e, () => {
				if (!e.Overflow) {
					return this.il.Create (OpCodes.Add);
				} else {
					return this.il.Create (e.IsSigned ? OpCodes.Add_Ovf : OpCodes.Add_Ovf_Un);
				}
			});
		}

		protected override Expr VisitSub (ExprSub e)
		{
			return this.VisitBinary (e, () => {
				if (!e.Overflow) {
					return this.il.Create (OpCodes.Sub);
				} else {
					return this.il.Create (e.IsSigned ? OpCodes.Sub_Ovf : OpCodes.Sub_Ovf_Un);
				}
			});
		}

		protected override Expr VisitCall (ExprCall e)
		{
			foreach (var param in e.Parameters) {
				this.Visit (param);
			}
			var instCall = this.il.Create (OpCodes.Call, e.Method);
			this.Emit (e, instCall);
			return e;
		}

		protected override Expr VisitReturn (ExprReturn e)
		{
			var instReturn = this.il.Create (OpCodes.Ret);
			this.Emit (e, instReturn);
			return e;
		}

		protected override Expr VisitBox (ExprBox e)
		{
			this.Visit (e.ExprToBox);
			var instBox = this.il.Create (OpCodes.Box, e.ReturnType);
			this.Emit (e, instBox);
			return e;
		}

		protected override Expr VisitConv (ExprConv e)
		{
			this.Visit (e.ExprToConvert);
			Instruction instConv;
			switch (e.ConvToType) {
			case TypeCode.Int32:
				instConv = this.il.Create (OpCodes.Conv_I4);
				break;
			case TypeCode.Int64:
				instConv = this.il.Create (OpCodes.Conv_I8);
				break;
			default:
				throw new NotSupportedException ("Cannot conv to: " + e.ConvToType);
			}
			this.Emit (e, instConv);
			return e;
		}

	}
}
