//
// System.Data.SqlXml.RowFillingEventArgs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System;

namespace System.Data.SqlXml {
        public class RowFillingEventArgs : EventArgs
        {
		#region Fields

		Exception errors;
		DataRow row;
		FillStatus status;

		#endregion // Fields

		#region Constructors

		public RowFillingEventArgs (DataRow row)
		{
			this.row = row;
		}

		#endregion // Constructors

		#region Properties

		public Exception Errors {
			get { return errors; }
			set { errors = value; }
		}

		public DataRow Row {
			get { return row; }
		}

		public FillStatus Status {
			get { return status; }
			set { status = value; }
		}

		#endregion // Properties
        }
}

#endif // NET_1_2
