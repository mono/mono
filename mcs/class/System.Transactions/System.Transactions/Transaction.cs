//
// Transaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0
using System.Collections;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace System.Transactions
{
	[Serializable]
	public class Transaction : IDisposable, ISerializable
	{
		[MonoTODO]
		public static Transaction Current {
			get { return TransactionManager.Current; }
			set { TransactionManager.Current = value; }
		}

		IsolationLevel level;
		TransactionInformation info;

		ArrayList dependents = new ArrayList ();

		internal Transaction ()
		{
			info = new TransactionInformation ();
		}

		internal Transaction (Transaction other)
		{
			level = other.level;
			info = other.info;
			dependents = other.dependents;
		}

		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info,
			StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public event TransactionCompletedEventHandler TransactionCompleted;

		[MonoTODO]
		public IsolationLevel IsolationLevel {
			get { return level; }
		}

		[MonoTODO]
		public TransactionInformation TransactionInformation {
			get { return info; }
		}

		[MonoTODO]
		public Transaction Clone ()
		{
			return new Transaction (this);
		}

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DependentTransaction DependentClone (
			DependentCloneOption option)
		{
			DependentTransaction d = 
				new DependentTransaction (this, option);
			dependents.Add (d);
			return d;
		}

		[MonoTODO]
		[PermissionSetAttribute (SecurityAction.LinkDemand)]
		public Enlistment EnlistDurable (Guid manager,
			IEnlistmentNotification notification,
			EnlistmentOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[PermissionSetAttribute (SecurityAction.LinkDemand)]
		public Enlistment EnlistDurable (Guid manager,
			ISinglePhaseNotification notification,
			EnlistmentOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool EnlistPromotableSinglePhase (
			IPromotableSinglePhaseNotification notification)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Enlistment EnlistVolatile (
			IEnlistmentNotification notification,
			EnlistmentOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Enlistment EnlistVolatile (
			ISinglePhaseNotification notification,
			EnlistmentOptions options)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			Transaction t = obj as Transaction;
			if (t == null)
				return false;
			return this.IsolationLevel == t.IsolationLevel &&
				this.TransactionInformation ==
				t.TransactionInformation;
		}

		public override int GetHashCode ()
		{
			return (int) IsolationLevel ^ TransactionInformation.GetHashCode () ^ dependents.GetHashCode ();
		}

		public void Rollback ()
		{
			Rollback (null);
		}

		[MonoTODO]
		public void Rollback (Exception ex)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
