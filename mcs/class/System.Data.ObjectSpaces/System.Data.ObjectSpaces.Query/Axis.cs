//
// System.Data.ObjectSpaces.Query.Axis
//
//
// Authors:
//	Richard Thombs (stony@stony.org)
//      Tim Coleman (tim@timcoleman.com)
//

#if NET_1_2

using System.Xml;

namespace System.Data.ObjectSpaces.Query
{
	[MonoTODO()]
	public class Axis : Filter
	{
		[MonoTODO()]
		public Axis(Expression source,Expression constraint) : base(source,constraint)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override bool IsConst {
			get { throw new NotImplementedException(); }
		}

		[MonoTODO()]
		public override NodeType NodeType {
			get { throw new NotImplementedException(); }
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
	}
}

#endif
