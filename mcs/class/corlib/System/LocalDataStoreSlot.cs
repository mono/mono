//
// System.LocalDataStoreSlot.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System 
{
	public sealed class LocalDataStoreSlot
	{
		// Need to remove the thread system's data slot,
		// therefore this internalcall is in the threads code
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void DataSlot_unregister();
		
		~LocalDataStoreSlot() {
			DataSlot_unregister();
		}
	}
}
