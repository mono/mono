using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Runtime.DurableInstancing
{
	public sealed class InstanceLockQueryResult : InstanceStoreQueryResult
	{
		public InstanceLockQueryResult ()
			: this (new Dictionary<Guid, Guid> ())
		{
		}
		
		public InstanceLockQueryResult (IDictionary<Guid, Guid> instanceOwnerIds)
		{
			if (instanceOwnerIds == null)
				throw new ArgumentNullException ("instanceOwnerIds");
			InstanceOwnerIds = instanceOwnerIds;
		}
		
		public InstanceLockQueryResult (Guid instanceId, Guid instanceOwnerId)
			: this ()
		{
			InstanceOwnerIds [instanceId] = instanceOwnerId;
		}
		
		public IDictionary<Guid, Guid> InstanceOwnerIds { get; private set; }
	}
}
