using System;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.ControlFlow;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis {
	struct ExpressionDecoderAdapter<SymbolicValue, Data, Result, Visitor> 
		: IExpressionILVisitor<APC, SymbolicValue, SymbolicValue, Data, Result>
		where SymbolicValue : IEquatable<SymbolicValue> 
		where Visitor : ISymbolicExpressionVisitor<LabeledSymbol<APC, SymbolicValue>, LabeledSymbol<APC, SymbolicValue>, SymbolicValue, Data, Result> {

		private readonly Visitor visitor;

		public ExpressionDecoderAdapter (Visitor visitor)
		{
			this.visitor = visitor;
		}

		#region Implementation of IExpressionILVisitor<APC,Type,SymbolicValue,SymbolicValue,Data,Result>
		public Result Binary (APC pc, BinaryOperator op, SymbolicValue dest, SymbolicValue operand1, SymbolicValue operand2, Data data)
		{
			return this.visitor.Binary (Unrefine (pc, dest), op, dest, Unrefine (pc, operand1), Unrefine (pc, operand2), data);
		}

		public Result Isinst (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue obj, Data data)
		{
			return this.visitor.Isinst (Unrefine (pc, dest), type, dest, Unrefine (pc, obj), data);
		}

		public Result LoadNull (APC pc, SymbolicValue dest, Data polarity)
		{
			return this.visitor.LoadNull (Unrefine (pc, dest), dest, polarity);
		}

		public Result LoadConst (APC pc, TypeNode type, object constant, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadConst (Unrefine (pc, dest), type, constant, dest, data);
		}

		public Result Sizeof (APC pc, TypeNode type, SymbolicValue dest, Data data)
		{
			return this.visitor.Sizeof (Unrefine (pc, dest), type, dest, data);
		}

		public Result Unary (APC pc, UnaryOperator op, bool unsigned, SymbolicValue dest, SymbolicValue source, Data data)
		{
			return this.visitor.Unary (Unrefine (pc, dest), op, unsigned, dest, Unrefine (pc, source), data);
		}

		private LabeledSymbol<APC, SymbolicValue> Unrefine (APC pc, SymbolicValue dest)
		{
			return new LabeledSymbol<APC, SymbolicValue> (pc, dest);
		}
		#endregion
	}
}
