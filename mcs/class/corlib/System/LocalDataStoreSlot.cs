//
// System.LocalDataStoreSlot.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;
using System.Runtime.CompilerServices;

namespace System 
{
	public sealed class LocalDataStoreSlot
	{
		internal LocalDataStoreSlot ()
		{
		}

		[MonoTODO("Spec: Finalize locks the data store manager before marking the data slot as unoccupied")]
		~LocalDataStoreSlot()
		{
		}
	}
}
