//
// JSToken.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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

namespace Microsoft.JScript {

	public enum JSToken : int {
		None = -1,
		EndOfFile,
		
		If,
		For,
		Do,
		While,
		Continue,
		Break,
		Return,
		Import,
		With,
		Switch,
		Throw,
		Try,
		Package,
		Internal,
		Abstract,
		Public,
		Static,
		Private,
		Protected,
		Final,
		Event,
		Var,
		Const,
		Class,

		Function,
		LeftCurly,
		Semicolon,

		Null,
		True,
		False,
		This,
		Identifier,
		StringLiteral,
		IntegerLiteral,
		NumericLiteral,

		LeftParen,
		LeftBracket,
		AccessField,

		// Operators

		FirstOp,
		LogicalNot = FirstOp,
		BitwiseNot,
		Delete,
		Void,
		Typeof,
		Increment,
		Decrement,
		
		FirstBinaryOp,
		Plus = FirstBinaryOp,
		Minus,
		LogicalOr,
		LogicalAnd,
		BitwiseOr,
		BitwiseXor,
		BitwiseAnd,
		Equal,
		NotEqual,
		StrictEqual,
		StrictNotEqual,
		GreaterThan,
		LessThan,
		LessThanEqual,
		GreaterThanEqual,
		LeftShift,
		RightShift,
		UnsignedRightShift,
		Multiply,
		Divide,
		Modulo,
		LastPPOperator = Modulo,
		Instanceof,
		In,
		Assign,
		PlusAssign,
		MinusAssign,
		MultiplyAssign,
		DivideAssign,
		BitwiseAndAssign,
		BitwiseOrAssign,
		BitwiseXorAssign,
		ModuloAssign,
		LeftShiftAssign,
		RightShiftAssign,
		UnsignedRightShiftAssign,
		LastAssign = UnsignedRightShiftAssign,
		LastBinaryOp = UnsignedRightShiftAssign,
		ConditionalIf,
		Colon,
		Comma,
		LastOp = Comma,

		Case,
		Catch,
		Debugger,
		Default,
		Else,
		Export,
		Extends,
		Finally,
		Get,
		Implements,
		Interface,
		New,
		Set,
		Super,
		RightParen,
		RightCurly,
		RightBracket,
		PreProcessorConstant,
		Comment,
		UnterminatedComment,

		// Reserved words

		Assert,
		Boolean,
		Byte,
		Char,
		Decimal,
		Double,
		DoubleColon,
		Enum,
		Ensure,
		Float,
		Goto,
		Int,
		Invariant,
		Long,
		Namespace,
		Native,
		Require,
		Sbyte,
		Short,
		Synchronized,
		Transient,
		Throws,
		ParamArray,
		Volatile,
		Ushort,
		Uint,
		Ulong,
		Use,

		EndOfLine,
		PreProcessDirective		
	}
}
