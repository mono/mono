//
// System.Data.Sql.SerializationHelper
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.IO;

namespace System.Data.Sql {
	public class SerializationHelper
	{
		#region Methods

		[MonoTODO]
		public static object Deserialize (Stream s, Type resultType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static byte[] GetSerializationBlob (Type t)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Serialize (Stream s, object instance)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int SizeInBytes (object instance)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int SizeInBytes (Type t)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool ValidateSerializationBlob (byte[] blob, Type t)
		{
			throw new NotImplementedException ();
		}


		#endregion // Methods
	}
}

#endif
