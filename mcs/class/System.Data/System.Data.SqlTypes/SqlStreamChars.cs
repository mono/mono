//
// System.Data.SqlTypes.SqlStreamChars
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;
using System.Globalization;
using System.Xml.Serialization;

namespace System.Data.SqlTypes
{
	[MonoTODO]
	public abstract class SqlStreamChars : INullable, IDisposable
	{
		#region Fields

		public static readonly SqlStreamChars Null;

		bool notNull;

		#endregion

		#region Constructors

		protected SqlStreamChars ()
		{
			notNull = true;
		}

		#endregion

		#region Properties

		public abstract bool CanRead { get; }
		public abstract bool CanSeek { get; }
		public abstract bool CanWrite { get; }
		public abstract bool IsNull { get; }
		public abstract long Length { get; }
		public abstract long Position { get; }

		#endregion

		#region Methods

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

#endif
