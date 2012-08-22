using System;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Inference.Interface
{
	interface IBoxedExpressionController
	{
		void Unary(UnaryOperator unaryOp, UnaryOperator arg, BoxedExpression source);
		
		void Binary(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source);
		
		void SizeOf<Type>(Type type, int size, BoxedExpression source);
		
		void ArrayIndex(Type type, BoxedExpression array, BoxedExpression itemIndex, BoxedExpression source);
		
		void Result<Type>(Type type, BoxedExpression source);
		
		void ReturnedValue<Type>(Type type, BoxedExpression expression, BoxedExpression source);
		
		void Variable(object var, PathElement[] path, BoxedExpression source);
		
		void Constant<Type>(Type type, object value, BoxedExpression source);
	}
}

