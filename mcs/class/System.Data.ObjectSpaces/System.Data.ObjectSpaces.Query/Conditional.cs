//
// System.Data.ObjectSpaces.Query.Conditional
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
	public class Conditional : Expression
	{
		[MonoTODO()]
		public Conditional(Expression condition,Expression tBranch,Expression fBranch) : base()
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

		// Gets/sets the condition expression
		[MonoTODO()]
		public Expression Condition
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		// Gets/sets the true branch expression
		[MonoTODO()]
		public Expression TBranch
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		// Gets/sets the false branch expression
		[MonoTODO()]
		public Expression FBranch
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}
}

#endif
