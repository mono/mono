using System;
using System.Runtime.Serialization;

namespace System.Runtime.DurableInstancing
{
	[Serializable]
	public class InstanceCollisionException : InstancePersistenceCommandException
	{
		public InstanceCollisionException () : this ("Instance collision") {}
		public InstanceCollisionException (string msg) : base (msg) {}
		public InstanceCollisionException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceCollisionException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstanceCompleteException : InstancePersistenceCommandException
	{
		public InstanceCompleteException () : this ("Instance has completed") {}
		public InstanceCompleteException (string msg) : base (msg) {}
		public InstanceCompleteException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceCompleteException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstanceHandleConflictException : InstancePersistenceCommandException
	{
		public InstanceHandleConflictException () : this ("Instance handle conflict") {}
		public InstanceHandleConflictException (string msg) : base (msg) {}
		public InstanceHandleConflictException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceHandleConflictException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstanceKeyCompleteException : InstancePersistenceCommandException
	{
		public InstanceKeyCompleteException () : this ("Instance key completed") {}
		public InstanceKeyCompleteException (string msg) : base (msg) {}
		public InstanceKeyCompleteException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceKeyCompleteException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstanceKeyNotReadyException : InstancePersistenceCommandException
	{
		public InstanceKeyNotReadyException () : this ("Instance key is not ready") {}
		public InstanceKeyNotReadyException (string msg) : base (msg) {}
		public InstanceKeyNotReadyException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceKeyNotReadyException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstanceLockedException : InstancePersistenceCommandException
	{
		public InstanceLockedException () : this ("Instance is locked") {}
		public InstanceLockedException (string msg) : base (msg) {}
		public InstanceLockedException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceLockedException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstanceLockLostException : InstancePersistenceCommandException
	{
		public InstanceLockLostException () : this ("Instance lock is lost") {}
		public InstanceLockLostException (string msg) : base (msg) {}
		public InstanceLockLostException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceLockLostException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstanceNotReadyException : InstancePersistenceCommandException
	{
		public InstanceNotReadyException () : this ("Instance key is not ready") {}
		public InstanceNotReadyException (string msg) : base (msg) {}
		public InstanceNotReadyException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceNotReadyException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}

	[Serializable]
	public class InstanceKeyCollisionException : InstancePersistenceCommandException
	{
		public InstanceKeyCollisionException () : this ("Instance key collision") {}
		public InstanceKeyCollisionException (string msg) : base (msg) {}
		public InstanceKeyCollisionException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceKeyCollisionException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstanceOwnerException : InstancePersistenceException
	{
		public InstanceOwnerException () : this ("Instance owner error") {}
		public InstanceOwnerException (string msg) : base (msg) {}
		public InstanceOwnerException (string msg, Exception inner) : base (msg, inner) {}
		protected InstanceOwnerException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstancePersistenceCommandException : InstancePersistenceException
	{
		public InstancePersistenceCommandException () : this ("Instance persistence command error") {}
		public InstancePersistenceCommandException (string msg) : base (msg) {}
		public InstancePersistenceCommandException (string msg, Exception inner) : base (msg, inner) {}
		protected InstancePersistenceCommandException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
	
	[Serializable]
	public class InstancePersistenceException : Exception
	{
		public InstancePersistenceException () : this ("Instance persistence error") {}
		public InstancePersistenceException (string msg) : base (msg) {}
		public InstancePersistenceException (string msg, Exception inner) : base (msg, inner) {}
		protected InstancePersistenceException (SerializationInfo info, StreamingContext context) :
			base (info, context) {}
	}
}
