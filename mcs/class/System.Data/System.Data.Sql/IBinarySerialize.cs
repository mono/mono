//
// System.Data.Sql.IBinarySerialize
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.IO;

namespace System.Data.Sql {
	public interface IBinarySerialize 
	{
		#region Methods

		void Read (BinaryReader r);
		void Write (BinaryWriter r);

		#endregion // Methods
	}
}

#endif
