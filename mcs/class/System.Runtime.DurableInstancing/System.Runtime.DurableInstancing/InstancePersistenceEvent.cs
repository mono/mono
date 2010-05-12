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
			Name = name;
		}

		public XName Name { get; private set; }

		public static bool operator == (InstancePersistenceEvent left, InstancePersistenceEvent right)
		{
			throw new NotImplementedException ();
		}
		public static bool operator != (InstancePersistenceEvent left, InstancePersistenceEvent right)
		{
			throw new NotImplementedException ();
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
		public static T Value {
			get { throw new NotImplementedException (); }
		}
		
		protected InstancePersistenceEvent (XName name)
			: base (name)
		{
		}
	}
}
