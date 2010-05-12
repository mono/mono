using System;

namespace System.Runtime.DurableInstancing
{
	public sealed class InstanceOwner
	{
		internal InstanceOwner (Guid ownerId)
		{
			InstanceOwnerId = ownerId;
		}

		public Guid InstanceOwnerId { get; private set; }
	}
}
