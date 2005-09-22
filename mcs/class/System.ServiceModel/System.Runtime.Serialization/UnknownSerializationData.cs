#if NET_2_0
using System;
using System.Collections.ObjectModel;

namespace System.Runtime.Serialization
{
	public sealed class UnknownSerializationData
	{
		object target;

		internal UnknownSerializationData (object target)
		{
			this.target = target;
		}
	}
}
#endif
