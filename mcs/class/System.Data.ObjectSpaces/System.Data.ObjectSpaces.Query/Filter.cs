//
// System.Data.ObjectSpaces.Query.Filter
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
	public class Filter : Expression
	{
		[MonoTODO()]
		public Filter(Expression source,Expression constraint) : base()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override object Clone()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public bool FindContextCallback(Expression oe,object[] oParams)
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
		public Expression Constraint
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
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
		public Expression Source
		{
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public override Type ValueType
		{
			get { throw new NotImplementedException(); }
		}
	}
}

#endif
