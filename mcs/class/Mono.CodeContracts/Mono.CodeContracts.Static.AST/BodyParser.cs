// 
// BodyParser.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Mono.CodeContracts.Static.AST {
	class BodyParser {
		private readonly MethodDefinition method;
		private readonly Dictionary<int, Block> block_map = new Dictionary<int, Block> ();
		private readonly Stack<Expression> evaluation_stack;

		public BodyParser (MethodDefinition method)
		{
			this.method = method;
			this.evaluation_stack = new Stack<Expression> (2);

			CoreSystemTypes.ModuleDefinition = this.method.Module;
		}

		public List<Statement> ParseBlocks ()
		{
			CreateBlockMap ();
			var statements = new List<Statement> ();
			Block block = null;

			int counter = 0;
			Collection<Instruction> instructions = this.method.Body.Instructions;
			int size = instructions.Count;
			while (counter < size) {
				if (block == null) {
					int offset = instructions [counter].Offset;
					block = GetOrCreateBlock (offset);
					statements.Add (block);
				}

				bool isNewBlock;
				counter = ParseStatement (instructions, counter, block.Statements, out isNewBlock);
				if (isNewBlock)
					block = null;
			}

			statements.Add (GetOrCreateBlock (instructions [size - 1].Offset + 1));

			return statements;
		}

		private int ParseStatement (Collection<Instruction> instructions, int index, List<Statement> result, out bool isNewBlock)
		{
			Expression expression = null;
			Statement statement = null;
			isNewBlock = false;

			bool needToRepeat = true;
			while (index < instructions.Count) {
				Instruction inst = instructions [index++];
				Code opcode = inst.OpCode.Code;
				bool isStatement = false;
				switch (opcode) {
				case Code.Nop:
					statement = new Statement (NodeType.Nop);
					needToRepeat = false;
					break;
				case Code.Ldarg_0:
				case Code.Ldarg_1:
				case Code.Ldarg_2:
				case Code.Ldarg_3:
					expression = GetParameterExpression (opcode - Code.Ldarg_0);
					break;
				case Code.Ldarg_S:
					expression = GetParameterExpression ((int) inst.Operand);
					break;
				case Code.Ldloc_0:
				case Code.Ldloc_1:
				case Code.Ldloc_2:
				case Code.Ldloc_3:
					expression = GetLocalExpression (opcode - Code.Ldloc_0);
					break;
				case Code.Ldloc_S:
					expression = GetLocalExpression ((int) (inst.Operand));
					break;
				case Code.Stloc_0:
				case Code.Stloc_1:
				case Code.Stloc_2:
				case Code.Stloc_3:
					statement = new AssignmentStatement (PopOperand (), GetLocalExpression ((opcode - Code.Stloc_0)));
					needToRepeat = false;
					break;
				case Code.Stloc_S:
					statement = new AssignmentStatement (PopOperand (), GetLocalExpression ((VariableDefinition) (inst.Operand)));
					needToRepeat = false;
					break;
				case Code.Starg_S:
					statement = new AssignmentStatement (PopOperand (), GetParameterExpression ((int) (inst.Operand)));
					needToRepeat = false;
					break;
				case Code.Ldarga_S:
					throw new NotImplementedException ();
				case Code.Ldloca_S:
					throw new NotImplementedException ();
				case Code.Ldnull:
					expression = Literal.Null;
					break;
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
					expression = GetLiteral ((opcode - Code.Ldc_I4_0), CoreSystemTypes.Instance.TypeInt32);
					break;
				case Code.Ldc_I8:
					expression = GetLiteral ((Int64) inst.Operand, CoreSystemTypes.Instance.TypeInt64);
					break;
				case Code.Ldc_I4_S:
					expression = GetLiteral ((int) (sbyte) inst.Operand, CoreSystemTypes.Instance.TypeInt32);
					break;
				case Code.Ldc_I4:
					expression = GetLiteral ((int) inst.Operand, CoreSystemTypes.Instance.TypeInt32);
					break;
				case Code.Ldc_R4:
					expression = GetLiteral ((float) inst.Operand, CoreSystemTypes.Instance.TypeSingle);
					break;
				case Code.Ldc_R8:
					expression = GetLiteral ((double) inst.Operand, CoreSystemTypes.Instance.TypeDouble);
					break;
				case Code.Ret:
					statement = new Return (TypeIsVoid (this.method.ReturnType) ? null : PopOperand ());
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Br_S:
					statement = ParseBranch (inst, NodeType.Nop, 0, true, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Brfalse_S:
					statement = ParseBranch (inst, NodeType.LogicalNot, 1, true, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Brtrue_S:
					statement = ParseBranch (inst, NodeType.Nop, 1, true, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Bne_Un_S:
					statement = ParseBranch (inst, NodeType.Ne, 2, true, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Bge_Un_S:
					statement = ParseBranch (inst, NodeType.Ge, 2, true, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Bgt_Un_S:
					statement = ParseBranch (inst, NodeType.Gt, 2, true, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Ble_S:
					statement = ParseBranch (inst, NodeType.Le, 2, true, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Blt_Un_S:
					statement = ParseBranch (inst, NodeType.Lt, 2, true, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Br:
					statement = ParseBranch (inst, NodeType.Nop, 0, false, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Brfalse:
					statement = ParseBranch (inst, NodeType.LogicalNot, 0, false, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Brtrue:
					statement = ParseBranch (inst, NodeType.Nop, 1, false, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Beq:
					statement = ParseBranch (inst, NodeType.Eq, 2, false, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Bge:
					statement = ParseBranch (inst, NodeType.Ge, 2, false, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Bgt:
					statement = ParseBranch (inst, NodeType.Gt, 2, false, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Ble:
					statement = ParseBranch (inst, NodeType.Le, 2, false, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Blt:
					statement = ParseBranch (inst, NodeType.Lt, 2, false, false);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Bne_Un:
					statement = ParseBranch (inst, NodeType.LogicalNot, 2, false, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Bge_Un:
					statement = ParseBranch (inst, NodeType.Ge, 2, false, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Bgt_Un:
					statement = ParseBranch (inst, NodeType.Gt, 2, false, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Ble_Un:
					statement = ParseBranch (inst, NodeType.Le, 2, false, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Blt_Un:
					statement = ParseBranch (inst, NodeType.Lt, 2, false, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Leave:
					statement = ParseBranch (inst, NodeType.Nop, 0, false, false, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Leave_S:
					statement = ParseBranch (inst, NodeType.Nop, 0, true, false, true);
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Endfinally:
					statement = new EndFinally ();
					isNewBlock = true;
					needToRepeat = false;
					break;
				case Code.Switch:
					break;
				case Code.Ldind_I1:
					break;
				case Code.Ldind_U1:
					break;
				case Code.Ldind_I2:
					break;
				case Code.Ldind_U2:
					break;
				case Code.Ldind_I4:
					break;
				case Code.Ldind_U4:
					break;
				case Code.Ldind_I8:
					break;
				case Code.Ldind_I:
					break;
				case Code.Ldind_R4:
					break;
				case Code.Ldind_R8:
					break;
				case Code.Ldind_Ref:
					break;
				case Code.Stind_Ref:
					break;
				case Code.Stind_I1:
					break;
				case Code.Stind_I2:
					break;
				case Code.Stind_I4:
					break;
				case Code.Stind_I8:
					break;
				case Code.Stind_R4:
					break;
				case Code.Stind_R8:
					break;
				case Code.Add:
					expression = ParseBinaryOperation (NodeType.Add);
					break;
				case Code.Sub:
					expression = ParseBinaryOperation (NodeType.Sub);
					break;
				case Code.Mul:
					expression = ParseBinaryOperation (NodeType.Mul);
					break;
				case Code.Div:
					expression = ParseBinaryOperation (NodeType.Div);
					break;
				case Code.Div_Un:
					expression = ParseBinaryOperation (NodeType.Div_Un);
					break;
				case Code.Rem:
					expression = ParseBinaryOperation (NodeType.Rem);
					break;
				case Code.Rem_Un:
					expression = ParseBinaryOperation (NodeType.Rem_Un);
					break;
				case Code.And:
					expression = ParseBinaryOperation (NodeType.And);
					break;
				case Code.Or:
					expression = ParseBinaryOperation (NodeType.Or);
					break;
				case Code.Xor:
					expression = ParseBinaryOperation (NodeType.Xor);
					break;
				case Code.Shl:
					expression = ParseBinaryOperation (NodeType.Shl);
					break;
				case Code.Shr:
					expression = ParseBinaryOperation (NodeType.Shr);
					break;
				case Code.Shr_Un:
					expression = ParseBinaryOperation (NodeType.Shr_Un);
					break;
				case Code.Neg:
					expression = ParseUnaryOperation (NodeType.Neg);
					break;
				case Code.Not:
					expression = ParseUnaryOperation (NodeType.Not);
					break;
				case Code.Conv_I1:
					expression = new UnaryExpression (NodeType.Conv_I1, PopOperand (), CoreSystemTypes.Instance.TypeSByte);
					break;
				case Code.Conv_I2:
					expression = new UnaryExpression (NodeType.Conv_I2, PopOperand (), CoreSystemTypes.Instance.TypeInt16);
					break;
				case Code.Conv_I4:
					expression = new UnaryExpression (NodeType.Conv_I4, PopOperand (), CoreSystemTypes.Instance.TypeInt32);
					break;
				case Code.Conv_I8:
					expression = new UnaryExpression (NodeType.Conv_I8, PopOperand (), CoreSystemTypes.Instance.TypeInt64);
					break;
				case Code.Conv_R4:
					expression = new UnaryExpression (NodeType.Conv_R4, PopOperand (), CoreSystemTypes.Instance.TypeSingle);
					break;
				case Code.Conv_R8:
					expression = new UnaryExpression (NodeType.Conv_R8, PopOperand (), CoreSystemTypes.Instance.TypeDouble);
					break;
				case Code.Conv_U4:
					expression = new UnaryExpression (NodeType.Conv_R8, PopOperand (), CoreSystemTypes.Instance.TypeUInt32);
					break;
				case Code.Conv_U8:
					expression = new UnaryExpression (NodeType.Conv_R8, PopOperand (), CoreSystemTypes.Instance.TypeUInt64);
					break;
				case Code.Call:
					expression = ParseCall (inst, NodeType.Call, out isStatement);
					if (isStatement)
						needToRepeat = false;
					break;
				case Code.Callvirt:
					expression = ParseCall (inst, NodeType.CallVirt, out isStatement);
					if (isStatement)
						needToRepeat = false;
					break;
				case Code.Cpobj:
					break;
				case Code.Ldobj:
					break;
				case Code.Ldstr:
					expression = GetLiteral (inst.Operand, CoreSystemTypes.Instance.TypeString);
					break;
				case Code.Newobj:
					expression = ParseNewObjectCreation (inst);
					break;
				case Code.Castclass:
					break;
				case Code.Isinst:
					break;
				case Code.Conv_R_Un:
					break;
				case Code.Unbox:
					break;
				case Code.Throw:
					isNewBlock = true;
					break;
				case Code.Ldfld:
					break;
				case Code.Ldflda:
					break;
				case Code.Stfld:
					break;
				case Code.Ldsfld:
					break;
				case Code.Ldsflda:
					break;
				case Code.Stsfld:
					break;
				case Code.Stobj:
					break;
				case Code.Conv_Ovf_I1_Un:
					break;
				case Code.Conv_Ovf_I2_Un:
					break;
				case Code.Conv_Ovf_I4_Un:
					break;
				case Code.Conv_Ovf_I8_Un:
					break;
				case Code.Conv_Ovf_U1_Un:
					break;
				case Code.Conv_Ovf_U2_Un:
					break;
				case Code.Conv_Ovf_U4_Un:
					break;
				case Code.Conv_Ovf_U8_Un:
					break;
				case Code.Conv_Ovf_I_Un:
					break;
				case Code.Conv_Ovf_U_Un:
					break;
				case Code.Box:
					break;
				case Code.Newarr:
					break;
				case Code.Ldlen:
					break;
				case Code.Ldelema:
					break;
				case Code.Ldelem_I1:
					break;
				case Code.Ldelem_U1:
					break;
				case Code.Ldelem_I2:
					break;
				case Code.Ldelem_U2:
					break;
				case Code.Ldelem_I4:
					break;
				case Code.Ldelem_U4:
					break;
				case Code.Ldelem_I8:
					break;
				case Code.Ldelem_I:
					break;
				case Code.Ldelem_R4:
					break;
				case Code.Ldelem_R8:
					break;
				case Code.Ldelem_Ref:
					break;
				case Code.Stelem_I:
					break;
				case Code.Stelem_I1:
					break;
				case Code.Stelem_I2:
					break;
				case Code.Stelem_I4:
					break;
				case Code.Stelem_I8:
					break;
				case Code.Stelem_R4:
					break;
				case Code.Stelem_R8:
					break;
				case Code.Stelem_Ref:
					break;
				case Code.Ldelem_Any:
					break;
				case Code.Stelem_Any:
					break;
				case Code.Unbox_Any:
					break;
				case Code.Conv_Ovf_I1:
					break;
				case Code.Conv_Ovf_U1:
					break;
				case Code.Conv_Ovf_I2:
					break;
				case Code.Conv_Ovf_U2:
					break;
				case Code.Conv_Ovf_I4:
					break;
				case Code.Conv_Ovf_U4:
					break;
				case Code.Conv_Ovf_I8:
					break;
				case Code.Conv_Ovf_U8:
					break;
				case Code.Refanyval:
					break;
				case Code.Ckfinite:
					break;
				case Code.Mkrefany:
					break;
				case Code.Ldtoken:
					break;
				case Code.Conv_U2:
					break;
				case Code.Conv_U1:
					break;
				case Code.Conv_I:
					break;
				case Code.Conv_Ovf_I:
					break;
				case Code.Conv_Ovf_U:
					break;
				case Code.Add_Ovf:
					break;
				case Code.Add_Ovf_Un:
					break;
				case Code.Mul_Ovf:
					break;
				case Code.Mul_Ovf_Un:
					break;
				case Code.Sub_Ovf:
					break;
				case Code.Sub_Ovf_Un:
					break;

				case Code.Stind_I:
					break;
				case Code.Conv_U:
					break;
				case Code.Arglist:
					break;
				case Code.Ceq:
					expression = ParseBinaryComparison (NodeType.Ceq);
					break;
				case Code.Cgt:
					expression = ParseBinaryComparison (NodeType.Cgt);
					break;
				case Code.Cgt_Un:
					break;
				case Code.Clt:
					expression = ParseBinaryComparison (NodeType.Clt);
					break;
				case Code.Clt_Un:
					break;
				case Code.Ldftn:
					break;
				case Code.Ldvirtftn:
					break;
				case Code.Ldarg:
					break;
				case Code.Ldarga:
					break;
				case Code.Starg:
					break;
				case Code.Ldloc:
					break;
				case Code.Ldloca:
					break;
				case Code.Stloc:
					break;
				case Code.Localloc:
					break;
				case Code.Endfilter:
					isNewBlock = true;
					break;
				case Code.Unaligned:
					break;
				case Code.Volatile:
					break;
				case Code.Tail:
					break;
				case Code.Initobj:
					break;
				case Code.Constrained:
					break;
				case Code.Cpblk:
					break;
				case Code.Initblk:
					break;
				case Code.No:
					break;
				case Code.Rethrow:
					isNewBlock = true;
					break;
				case Code.Sizeof:
					break;
				case Code.Refanytype:
					break;
				case Code.Readonly:
					break;
				default:
					needToRepeat = false;
					break;
				}
				if (!needToRepeat)
					break;

				int offset = inst.Next == null ? inst.Offset + 1 : inst.Next.Offset;
				if (!this.block_map.ContainsKey (offset))
					this.evaluation_stack.Push (expression);
				else {
					isNewBlock = true;
					break;
				}
			}


			while (this.evaluation_stack.Count > 0) {
				Statement stmt = new ExpressionStatement (this.evaluation_stack.Pop ());
				result.Add (stmt);
			}

			if (statement == null)
				statement = new ExpressionStatement (expression);

			result.Add (statement);
			if (!isNewBlock)
				isNewBlock = this.block_map.ContainsKey (instructions [index].Offset);

			return index;
		}


		private Expression ParseNewObjectCreation (Instruction inst)
		{
			Method method = GetMethodFromMethodCallInstruction (inst);
			int count = method.Parameters.Count;
			List<Expression> arguments;
			{
				var expressions = new Expression[count];
				for (int index = count - 1; index >= 0; index--)
					expressions [index] = PopOperand ();

				arguments = new List<Expression> (expressions);
			}

			var construct = new Construct (new MemberBinding (null, method), arguments) {Type = method.DeclaringType};

			return construct;
		}

		private Expression ParseUnaryOperation (NodeType operatorType)
		{
			Expression operand = PopOperand ();
			return new UnaryExpression (operatorType, operand, operand.Type);
		}

		private Expression ParseBinaryComparison (NodeType comparisonType)
		{
			Expression op1;
			Expression op2;
			Expression binaryExpression = ParseBinaryExpressionWithoutType (comparisonType, out op1, out op2);

			binaryExpression.Type = CoreSystemTypes.Instance.TypeByte;

			return binaryExpression;
		}

		private Expression ParseBinaryOperation (NodeType operatorType)
		{
			Expression op1;
			Expression op2;
			Expression binaryExpression = ParseBinaryExpressionWithoutType (operatorType, out op1, out op2);

			binaryExpression.Type = op1.Type ?? op2.Type;

			return binaryExpression;
		}

		private Expression ParseBinaryExpressionWithoutType (NodeType operatorType, out Expression op1, out Expression op2)
		{
			op2 = PopOperand ();
			op1 = PopOperand ();

			return new BinaryExpression (operatorType, op1, op2);
		}

		private Statement ParseBranch (Instruction inst, NodeType operatorType, int operandCount, bool isShortOffset, bool unsigned)
		{
			return ParseBranch (inst, operatorType, operandCount, isShortOffset, unsigned, false);
		}

		private Statement ParseBranch (Instruction inst, NodeType operatorType, int operandCount, bool isShortOffset, bool unsigned, bool leavesExceptionBlock)
		{
			Expression operand = operandCount > 1 ? PopOperand () : null;
			Expression expression = operandCount > 0 ? PopOperand () : null;
			Expression condition = operandCount > 1
			                       	? new BinaryExpression (operatorType, expression, operand)
			                       	: (operandCount > 0
			                       	   	? (operatorType == NodeType.Nop
			                       	   	   	? expression
			                       	   	   	: new UnaryExpression (operatorType, expression)
			                       	   	  )
			                       	   	: null);

			int offset = ((Instruction) inst.Operand).Offset;
			Block target = this.block_map [offset];

			return new Branch (condition, target, isShortOffset, unsigned, leavesExceptionBlock);
		}

		private MethodCall ParseCall (Instruction instruction, NodeType typeOfCall, out bool isStatement)
		{
			Method method = GetMethodFromMethodCallInstruction (instruction);
			isStatement = TypeIsVoid (method.ReturnType);
			int parametersCount = method.Parameters == null ? 0 : method.Parameters.Count;
			int n = typeOfCall != NodeType.Jmp ? parametersCount : 0;
			List<Expression> arguments;
			{
				var expressions = new Expression[n];

				for (int index = n - 1; index >= 0; --index)
					expressions [index] = PopOperand ();
				arguments = new List<Expression> (expressions);
			}

			var methodCall = new MethodCall (new MemberBinding (method.IsStatic ? null : PopOperand (), method), arguments, typeOfCall) {Type = method.ReturnType};
			return methodCall;
		}

		private Method GetMethodFromMethodCallInstruction (Instruction instruction)
		{
			var methodReference = instruction.Operand as MethodReference;

			var method = new Method (methodReference.Resolve ());
			return method;
		}

		private Expression GetParameterExpression (int index)
		{
			if (this.method.IsStatic)
				return new Parameter (this.method.Parameters [index]);
			if (index == 0)
				return new Parameter (this.method.Body.ThisParameter);

			return new Parameter (this.method.Parameters [index - 1]);
		}

		private static bool TypeIsVoid (TypeNode type)
		{
			return type.FullName.Equals ("System.Void");
		}

		private static bool TypeIsVoid (TypeReference type)
		{
			return type.FullName.Equals ("System.Void");
		}

		private Expression GetLocalExpression (int index)
		{
			return new Local (this.method.Body.Variables [index]);
		}

		private Expression GetLocalExpression (VariableDefinition variable)
		{
			return new Local (variable);
		}

		private Expression PopOperand ()
		{
			return this.evaluation_stack.Pop ();
		}

		private Literal GetLiteral (object constant, TypeNode type)
		{
			return new Literal (constant, type);
		}

		#region Block map creation
		private void CreateBlockMap ()
		{
			foreach (Instruction inst in this.method.Body.Instructions)
				ProcessInstructionForBlockMap (inst);
		}

		private void ProcessInstructionForBlockMap (Instruction inst)
		{
			switch (inst.OpCode.Code) {
			case Code.Leave:
			case Code.Br:
			case Code.Brfalse:
			case Code.Brtrue:
			case Code.Beq:
			case Code.Bge:
			case Code.Bgt:
			case Code.Ble:
			case Code.Blt:
			case Code.Bne_Un:
			case Code.Bge_Un:
			case Code.Bgt_Un:
			case Code.Ble_Un:
			case Code.Blt_Un:
				GetOrCreateBlock (((Instruction) inst.Operand).Offset);
				break;
			case Code.Leave_S:
			case Code.Br_S:
			case Code.Brfalse_S:
			case Code.Brtrue_S:
			case Code.Beq_S:
			case Code.Bge_S:
			case Code.Bgt_S:
			case Code.Ble_S:
			case Code.Blt_S:
			case Code.Bne_Un_S:
			case Code.Bge_Un_S:
			case Code.Bgt_Un_S:
			case Code.Ble_Un_S:
			case Code.Blt_Un_S:
				GetOrCreateBlock (((Instruction) inst.Operand).Offset);
				break;
			case Code.Switch:
				break;
			}
		}

		private Block GetOrCreateBlock (int address)
		{
			Block block;
			if (!this.block_map.TryGetValue (address, out block)) {
				this.block_map [address] = (block = new Block (new List<Statement> ()));
				block.ILOffset = address;
			}
			return block;
		}
		#endregion
	}
}
