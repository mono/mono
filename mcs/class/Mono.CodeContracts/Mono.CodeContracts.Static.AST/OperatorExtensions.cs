// 
// OperatorExtensions.cs
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

namespace Mono.CodeContracts.Static.AST {
	static class OperatorExtensions {
		public static bool IsConversionOperator (this UnaryOperator op)
		{
			switch (op) {
			case UnaryOperator.Conv_i:
			case UnaryOperator.Conv_i1:
			case UnaryOperator.Conv_i2:
			case UnaryOperator.Conv_i4:
			case UnaryOperator.Conv_i8:
			case UnaryOperator.Conv_r4:
			case UnaryOperator.Conv_r8:
			case UnaryOperator.Conv_u:
			case UnaryOperator.Conv_u1:
			case UnaryOperator.Conv_u2:
			case UnaryOperator.Conv_u4:
			case UnaryOperator.Conv_u8:
			case UnaryOperator.Conv_r_un:
				return true;
			default:
				return false;
			}
		}

		public static bool IsEqualityOperator (this BinaryOperator bop)
		{
			return bop == BinaryOperator.Ceq || bop == BinaryOperator.Cobjeq;
		}
	}
}