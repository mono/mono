// 
// System.EnterpriseServices.TransactionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class TransactionAttribute : Attribute {

		#region Fields

		TransactionIsolationLevel isolation;
		int timeout;
		TransactionOption val;

		#endregion // Fields

		#region Constructors

		public TransactionAttribute ()
			: this (TransactionOption.Required)
		{
		}

		public TransactionAttribute (TransactionOption val)
		{
			this.isolation = TransactionIsolationLevel.Serializable;
			this.timeout = -1;
			this.val = val;
		}

		#endregion // Constructors

		#region Properties

		public TransactionIsolationLevel Isolation {
			get { return isolation; }
			set { isolation = value; }
		}

		public int Timeout {
			get { return timeout; }
			set { timeout = value; }
		}

		public TransactionOption Value {
			get { return val; }
		}

		#endregion // Properties
	}
}
