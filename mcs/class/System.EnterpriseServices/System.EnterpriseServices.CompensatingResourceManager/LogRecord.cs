// 
// System.EnterpriseServices.CompensatingResourceManager.LogRecord.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.EnterpriseServices;

namespace System.EnterpriseServices.CompensatingResourceManager {

	public sealed class LogRecord {

		#region Fields

		LogRecordFlags flags;
		object record;
		int sequence;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		internal LogRecord ()
		{
		}

		[MonoTODO]
		internal LogRecord (_LogRecord logRecord)
		{
			flags = (LogRecordFlags) logRecord.dwCrmFlags;
			sequence = logRecord.dwSequenceNumber;
			record = logRecord.blobUserData;
		}

		#endregion // Constructors

		#region Properties

		public LogRecordFlags Flags {
			get { return flags; }
		}

		public object Record {
			get { return record; }
		}

		public int Sequence {
			get { return sequence; }
		}

		#endregion // Properties
	}

	internal struct _LogRecord {
		
		#region Fields

		public int dwCrmFlags;
		public int dwSequenceNumber;
		public object blobUserData; // FIXME: This is not the correct type

		#endregion // Fields
	}
}
