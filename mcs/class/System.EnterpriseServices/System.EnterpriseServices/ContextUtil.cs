// 
// System.EnterpriseServices.ContextUtil.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	public sealed class ContextUtil {

		#region Fields

		static bool deactivateOnReturn;
		static TransactionVote myTransactionVote;

		#endregion // Fields

		#region Properties

		public static Guid ActivityId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		public static Guid ApplicationId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static Guid ApplicationInstanceId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static Guid ContextId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static bool DeactivateOnReturn {
			get { return deactivateOnReturn; }
			set { deactivateOnReturn = value; }
		}

		public static bool IsInTransaction {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static bool IsSecurityEnabled {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public static TransactionVote MyTransactionVote {
			get { return myTransactionVote; }
			set { myTransactionVote = value; }
		}

		public static Guid PartitionId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static object Transaction {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public static Guid TransactionId {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
