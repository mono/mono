//
// System.Data.ObjectSpaces.Query.Span
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

#if NET_1_2

using System;
using System.Xml;
using System.Collections;

namespace System.Data.ObjectSpaces.Query
{
	[MonoTODO()]
	public class Span : Expression
	{
		[MonoTODO()]
		public Span(Expression source,SpanPropertyCollection spanProperties) : base()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override object Clone()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public void AddToSpanList(SpanProperty sp,ArrayList spanList)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public ArrayList GetSpanList()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public void SetSpanProperties(SpanPropertyCollection spanProperties)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override void WriteXml(XmlWriter xmlw)
		{
			throw new NotImplementedException();
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
		public SpanPropertyCollection SpanProperties
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
