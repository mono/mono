//
// DependentTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

using System.Runtime.Serialization;

namespace System.Transactions
{
	[MonoTODO ("Not supported yet")]
	[Serializable]
	public sealed class DependentTransaction : Transaction, ISerializable
	{
//		Transaction parent;
//		DependentCloneOption option;
		bool completed;

		internal DependentTransaction (Transaction parent,
			DependentCloneOption option)
			: base(parent.IsolationLevel)
		{
//			this.parent = parent;
//			this.option = option;
		}

		internal bool Completed {
			get { return completed; }
		}

		[MonoTODO]
		public void Complete ()
		{
			throw new NotImplementedException ();
		}

		void ISerializable.GetObjectData (SerializationInfo info,
			StreamingContext context)
		{
//			parent = (Transaction) info.GetValue ("parent", typeof (Transaction));
//			option = (DependentCloneOption) info.GetValue (
//				"option", typeof (DependentCloneOption));
			completed = info.GetBoolean ("completed");
		}
	}
}

