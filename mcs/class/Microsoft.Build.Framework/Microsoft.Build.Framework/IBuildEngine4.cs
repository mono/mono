using System;

namespace Microsoft.Build.Framework
{
	[MonoTODO]
	public interface IBuildEngine4 : IBuildEngine3
	{
		object GetRegisteredTaskObject (object key, RegisteredTaskObjectLifetime lifetime);
		void RegisterTaskObject (object key, object obj, RegisteredTaskObjectLifetime lifetime, bool allowEarlyCollection);
		object UnregisterTaskObject (object key,　RegisteredTaskObjectLifetime lifetime);
	}
}

