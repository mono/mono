//
// System.Data.Sql.ISqlNotificationReceiver
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Sql {
	public interface ISqlNotificationReceiver 
	{
		#region Methods

		void Invalidate (string id, SqlNotificationType type, SqlNotificationInfo info, SqlNotificationSource source);
		void InvalidateImmediate (string id, SqlNotificationType type, SqlNotificationInfo info, SqlNotificationSource source);

		#endregion // Methods
	}
}

#endif
