// 
// System.EnterpriseServices.CompensatingResourceManager.Compensator.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.EnterpriseServices;

namespace System.EnterpriseServices.CompensatingResourceManager {
	public class Compensator : ServicedComponent{

		#region Constructors

		[MonoTODO]
		public Compensator ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		public Clerk Clerk {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual bool AbortRecord (LogRecord rec)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void BeginAbort (bool fRecovery)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void BeginCommit (bool fRecovery)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void BeginPrepare ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void CommitRecord (LogRecord rec)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void EndAbort ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void EndCommit ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool EndPrepare ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool PrepareRecord (LogRecord rec)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
