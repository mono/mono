//
// System.CodeDom CodeBinaryOperatorType Enum implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom 
{
	[Serializable]
	[ComVisible(true)]
	public enum CodeBinaryOperatorType {
		Add,
		Assign,
		BitwiseAnd,
		BitwiseOr,
		BooleanAnd,
		BooleanOr,
		Divide,
		GreaterThan,
		GreaterThanOrEqual,
		IdentityEquality,
		IdentityInequality,
		LessThan,
		LessThanOrEqual,
		Modulus,
		Multiply,
		Subtract,
		ValueEquality
	}
}
