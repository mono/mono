//
// System.Data.ObjectSpaces.Query.RelTraversal
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
	public class RelTraversal : Expression
	{
		[MonoTODO()]
		public RelTraversal(RelTraversalDirection direction) : base()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public RelTraversal(string relName,RelTraversalDirection direction) : base()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override object Clone()
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public override void WriteXml(XmlWriter xmlw)
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		public RelTraversalDirection Direction
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
		public string RelName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}
}

#endif
