//
// System.Data.ObjectSpaces.Query.Function
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

#if NET_1_2

using System;
using System.Xml;

namespace System.Data.ObjectSpaces.Query
{
	[MonoTODO()]
	public class Function : Expression
	{
		[MonoTODO()]
		public Function(FunctionOperator _operator,Expression[] _params) : base()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override object Clone()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override bool IsArithmetic()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override bool IsBoolean()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override bool IsFilter()
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

		[MonoTODO()]
		public override NodeType NodeType
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public Expression Operator
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public Expression Params
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
