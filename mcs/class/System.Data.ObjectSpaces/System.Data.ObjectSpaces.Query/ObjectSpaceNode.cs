//
// System.Data.ObjectSpaces.Query.ObjectSpaceNode
//
//
// Author:
//     Richard Thombs (stony@stony.org)
//     Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.ObjectSpaces.Query {
	public class ObjectSpaceNode : Expression
	{
		#region Constructors

		public ObjectSpaceNode ()
			: base ()
		{
		}

		#endregion

		#region Methods

		[MonoTODO()]
		public override object Clone()
		{
			throw new NotImplementedException();
		}

		#endregion // Methods
	}
}

#endif
