//
// System.Data.ObjectSpaces.Query.Binary
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
using System.Xml;

namespace System.Data.ObjectSpaces.Query
{
	[MonoTODO()]
	public class Binary : Expression
	{
		[MonoTODO()]
		public Binary(Expression left,Expression right,BinaryOperator _operator) : base()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override object Clone()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static Int64 Compare(object vLeft,object vRight,Type type,BinaryOperator op)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static object EvaluateConstant(Literal left,BinaryOperator op,Literal right)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static Type GetResultType(Expression leftExpr,Expression rightExpr,BinaryOperator op)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static bool IsInteger(Type type)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static bool IsOperatorArithmetic(BinaryOperator op)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static bool IsOperatorBoolean(BinaryOperator op)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static bool IsOperatorLogical(BinaryOperator op)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public static bool IsOperatorRelational(BinaryOperator op)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override void WriteXml(XmlWriter xmlw)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override bool IsConst
		{
			get { throw new NotImplementedException(); }
		}

		// Gets/sets the left operand of this binary expression
		[MonoTODO()]
		public Expression Left
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public override NodeType NodeType
		{
			get { throw new NotImplementedException(); }
		}

		// Gets/sets the operator used by this binary expression
		[MonoTODO()]
		public Expression Operator
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		// Gets/sets the right operand of this binary expression
		[MonoTODO()]
		public Expression Right
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public override Type ValueType
		{
			get { throw new NotImplementedException(); }
		}
	}
}

#endif
