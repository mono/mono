//
// System.Data.ObjectSpaces.Query.Binary
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

#if NET_1_2

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

		// Gets/sets the left operand of this binary expression
		[MonoTODO()]
		public Expression Left
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
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
	}
}

#endif
