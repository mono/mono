//
// System.Data.ObjectSpaces.OneToOneRelationship.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.ObjectSpaces {
	public class OneToOneRelationship : ObjectKeyManager 
	{
		#region Fields

		ObjectKey foreignKey;

		#endregion // Fields

		#region Properties

		public ObjectKey ForeignKey {
			get { return foreignKey; }
			set { foreignKey = value; }
		}

		#endregion // Properties
	}
}

#endif
