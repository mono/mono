//
// DependentTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

#if NET_2_0
using System.Runtime.Serialization;

namespace System.Transactions
{
	[MonoTODO ("Not supported yet")]
#if !WINDOWS_PHONE && !NETFX_CORE
	[Serializable]
#endif
	public sealed class DependentTransaction : Transaction
#if !WINDOWS_PHONE && !NETFX_CORE
		, ISerializable
#endif
	{
//		Transaction parent;
//		DependentCloneOption option;
		bool completed;

		internal DependentTransaction (Transaction parent,
			DependentCloneOption option)
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

#if !WINDOWS_PHONE && !NETFX_CORE
		void ISerializable.GetObjectData (SerializationInfo info,
			StreamingContext context)
		{
//			parent = (Transaction) info.GetValue ("parent", typeof (Transaction));
//			option = (DependentCloneOption) info.GetValue (
//				"option", typeof (DependentCloneOption));
			completed = info.GetBoolean ("completed");
		}
#endif
	}
}

#endif
