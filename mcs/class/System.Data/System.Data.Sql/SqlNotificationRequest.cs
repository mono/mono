//
// System.Data.Sql.SqlNotificationRequest
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;

namespace System.Data.Sql {
	public class SqlNotificationRequest 
	{
		#region Fields

		string id;
		string service;
		int timeout;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SqlNotificationRequest ()
			: this (null, null, 0)
		{
		}

		[MonoTODO]
		public SqlNotificationRequest (string id, string service, int timeout)
		{
			if (service == null)
				throw new ArgumentNullException ();
			if (timeout < 0)
				throw new ArgumentOutOfRangeException ();

			Id = id;
			Service = service;
			Timeout = timeout;
		}

		#endregion // Constructors

		#region Properties

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public string Service {
			get { return service; }
			set { service = value; }
		}

		public int Timeout {
			get { return timeout; }
			set { timeout = value; }
		}

		#endregion // Properties
	}
}

#endif
