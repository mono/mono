//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
namespace System.Management
{
	internal enum WMIQ_RPN_TOKEN_FLAGS
	{
		WMIQ_RPN_OP_UNDEFINED = 0,
		WMIQ_RPN_LEFT_PROPERTY_NAME = 1,
		WMIQ_RPN_TOKEN_EXPRESSION = 1,
		WMIQ_RPN_GET_TOKEN_TYPE = 1,
		WMIQ_RPN_FROM_UNARY = 1,
		WMIQ_RPN_OP_EQ = 1,
		WMIQ_RPN_NEXT_TOKEN = 1,
		WMIQ_RPN_GET_EXPR_SHAPE = 2,
		WMIQ_RPN_RIGHT_PROPERTY_NAME = 2,
		WMIQ_RPN_OP_NE = 2,
		WMIQ_RPN_TOKEN_AND = 2,
		WMIQ_RPN_FROM_PATH = 2,
		WMIQ_RPN_GET_LEFT_FUNCTION = 3,
		WMIQ_RPN_TOKEN_OR = 3,
		WMIQ_RPN_OP_GE = 3,
		WMIQ_RPN_GET_RIGHT_FUNCTION = 4,
		WMIQ_RPN_CONST2 = 4,
		WMIQ_RPN_FROM_CLASS_LIST = 4,
		WMIQ_RPN_OP_LE = 4,
		WMIQ_RPN_TOKEN_NOT = 4,
		WMIQ_RPN_OP_LT = 5,
		WMIQ_RPN_GET_RELOP = 5,
		WMIQ_RPN_OP_GT = 6,
		WMIQ_RPN_OP_LIKE = 7,
		WMIQ_RPN_CONST = 8,
		WMIQ_RPN_OP_ISA = 8,
		WMIQ_RPN_OP_ISNOTA = 9,
		WMIQ_RPN_RELOP = 16,
		WMIQ_RPN_LEFT_FUNCTION = 32,
		WMIQ_RPN_RIGHT_FUNCTION = 64
	}
}