//
// System.Data.ObjectSpaces.Query.TypeFilter
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
	public class TypeFilter : Expression
	{
		[MonoTODO()]
		public TypeFilter(Expression source,Type type) : base()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public TypeFilter(Expression source,Type type,bool baseTypeOnly) : base()
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
		public bool BaseTypeOnly
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
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
			set { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public Type Type
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
