//
// System.Data.StatementCompletedEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data {
	public class StatementCompletedEventArgs : EventArgs
	{
		#region Fields

		int recordCount;

		#endregion // Fields

		#region Constructors

		public StatementCompletedEventArgs (int recordCount)
		{
			this.recordCount = recordCount;
		}

		#endregion // Constructors

		#region Properties

		public int RecordCount {
			get { return recordCount; }
		}

		#endregion // Properties
	}
}

#endif // NET_1_2
