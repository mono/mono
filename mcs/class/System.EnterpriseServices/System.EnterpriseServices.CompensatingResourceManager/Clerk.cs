// 
// System.EnterpriseServices.CompensatingResourceManager.Clerk.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.EnterpriseServices;

namespace System.EnterpriseServices.CompensatingResourceManager {
	public sealed class Clerk {

		#region Constructors
		
		//internal Clerk (CrmLogControl logControl)
		//{
		//}

		[MonoTODO]
		public Clerk (string compensator, string description, CompensatorOptions flags)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Clerk (Type compensator, string description, CompensatorOptions flags)
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		public int LogRecordCount {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string TransactionUOW {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		~Clerk ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ForceLog ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ForceTransactionToAbort ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ForgetLogRecord ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WriteLogRecord (object record)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
