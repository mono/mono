using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace System.Runtime.DurableInstancing
{
	public abstract class InstancePersistenceEvent : IEquatable<InstancePersistenceEvent>
	{
		internal InstancePersistenceEvent (XName name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			Name = name;
		}

		public XName Name { get; private set; }

		public static bool operator == (InstancePersistenceEvent left, InstancePersistenceEvent right)
		{
			if (Object.ReferenceEquals (left, null))
				return Object.ReferenceEquals (right, null);
			if (Object.ReferenceEquals (right, null))
				return false;
			return left.Name == right.Name;
		}

		public static bool operator != (InstancePersistenceEvent left, InstancePersistenceEvent right)
		{
			if (Object.ReferenceEquals (left, null))
				return !Object.ReferenceEquals (right, null);
			if (Object.ReferenceEquals (right, null))
				return true;
			return left.Name != right.Name;
		}

		public bool Equals (InstancePersistenceEvent persistenceEvent)
		{
			var p = persistenceEvent;
			return p != null && p.Name == Name;
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as InstancePersistenceEvent);
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}
	}
	
	public abstract class InstancePersistenceEvent<T> : InstancePersistenceEvent
		where T : InstancePersistenceEvent<T>, new()
	{
		static InstancePersistenceEvent ()
		{
			Value = (T) Activator.CreateInstance (typeof (T), new object [0]);
		}

		public static T Value { get; private set; }
		
		protected InstancePersistenceEvent (XName name)
			: base (name)
		{
		}
	}
}
