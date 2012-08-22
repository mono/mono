using System;
using Mono.CodeContracts.Static.Inference.Interface;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures; 

namespace Mono.CodeContracts.Static.Analysis
{
	interface IFullExpressionDecoder<Type, Variable, Expression>
	{
		Type System_Int32 { get; }

		bool IsVariable (Expression exp, out object variable);

		Variable UnderlyingVariable (Expression exp);

		bool IsNull (Expression exp);

		bool IsConstant (Expression exp, out object value, out Type type);

		bool IsSizeOf (Expression exp, out Type type);

		bool IsInst (Expression exp, out Expression arg, out Type type);

		bool IsUnaryOperator (Expression exp, out UnaryOperator op, out Expression arg);

		bool IsBinaryOperator (Expression exp, out BinaryOperator op, out Expression left, out Expression right);

		bool TryGetAssociatedExpression (Expression exp, AssociatedInfo infoKind, out Expression info);

		bool TryGetAssociatedExpression (APC pc, Expression exp, AssociatedInfo infoKind, out Expression info);

		void AddFreeVariables (Expression exp, IMutableSet<Expression> set);

		FList<PathElement> GetVariableAccessPath (Expression exp);

		bool TrySizeOfAsConstant (Expression exp, out int value);

		bool TryGetType (Expression exp, out object type);

		void Dispatch (Expression exp, IBoxedExpressionController visitor);
	}
}

