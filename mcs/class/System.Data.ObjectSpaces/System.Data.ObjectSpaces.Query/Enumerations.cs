//
// System.Data.ObjectSpaces.Query enumerations
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

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

#if NET_2_0

using System;

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
