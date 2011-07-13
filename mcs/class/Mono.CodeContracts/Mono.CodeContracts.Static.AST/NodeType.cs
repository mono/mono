// 
// NodeType.cs
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
	enum NodeType {
		Unknown,
		Block,
		Nop,
		LoadArg,
		LoadConstant,
		Clt,
		Cgt,
		Ceq,
		Return,
		Box,
		Conv,
		Add,
		Sub,
		Rem,
		Expression,
		Literal,
		Instruction,
		AssignmentStatement,
		Local,
		Parameter,
		Branch,
		ExpressionStatement,
		Le,
		Mul,
		Div,
		Div_Un,
		Rem_Un,
		And,
		Or,
		Shr,
		Xor,
		Shl,
		Shr_Un,
		Neg,
		Not,
		Conv_I1,
		Conv_I2,
		Conv_I8,
		Conv_I4,
		Conv_R4,
		Conv_R8,
		LogicalNot,
		Ne,
		Ge,
		Gt,
		Lt,
		Eq,
		This,
		Method,
		MethodContract,
		Requires,
		Ensures,
		ExceptionHandler,
		Filter,
		Catch,
		Finally,
		FaultHandler,
		TypeNode,
		EndFinally,
		Call,
		Calli,
		Jmp,
		MethodCall,
		MemberBinding,
		Construct,
		Class,
		Property,
		Assembly,
		Module,
		BlockExpression,
		CallVirt,
		Field,
		Reference
	}
}
