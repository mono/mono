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
		Add = 0,
		Subtract = 1,
		Multiply = 2,
		Divide = 3,
		Modulus = 4,
		Assign = 5,
		IdentityInequality = 6,
		IdentityEquality = 7,
		ValueEquality = 8,
		BitwiseOr = 9,
		BitwiseAnd = 10,
		BooleanOr = 11,
		BooleanAnd = 12,
		LessThan = 13,
		LessThanOrEqual = 14,
		GreaterThan = 15,
		GreaterThanOrEqual = 16
	}
}
