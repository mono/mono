//
// System.Data.ObjectSpaces.Query enumerations
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

#if NET_1_2

namespace System.Data.ObjectSpaces.Query
{
	public enum AnnotationType
	{
		QilAnnotation,
		ParseAnnotation,
		AxisNode,
		DebugInfo,
		UserInterface
	}

	public enum BinaryOperator
	{
		LogicalAnd,
		LogicalOr,
		Equality,
		Inequality,
		LessThan,
		LessEqual,
		GreaterThan,
		GreaterEqual,
		Addition,
		Subtraction,
		Multiplication,
		Division,
		Modulus,
		Concatenation
	}

	public enum FunctionOperator
	{
		Trim,
		Len,
		Like,
		Substring
	}

	public enum LiteralType
	{
		String,
		SByte,
		Byte,
		Int16,
		Int32,
		Int64,
		UInt16,
		UInt32,
		UInt64,
		Char,
		Single,
		Double,
		Boolean,
		Decimal,
		Guid,
		DateTime,
		TimeSpan
	}

	public enum NodeType
	{
		Aggregate,
		Axis,
		Binary,
		Conditional,
		Context,
		Distinct,
		Empty,
		Expression,
		Filter,
		Function,
		Join,
		InOperator,
		Literal,
		ObjectSpaceNode,
		OrderBy,
		Parameter,
		Parent,
		Projection,
		Property,
		RelTraversal,
		Span,
		Reference,
		TypeCast,
		TypeConversion,
		Unary
	}

	public enum RelTraversalDirection
	{
		ToTarget,
		ToSource
	}

	public enum UnaryOperator
	{
		Negation,
		LogicalNot,
		IsNull,
		Exists
	}
}

#endif
