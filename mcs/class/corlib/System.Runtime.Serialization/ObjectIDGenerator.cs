//
// System.Runtime.Serialization.ObjectIDGenerator.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez (lsg@ctv.es)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization
{
	[Serializable]
	public class ObjectIDGenerator
	{
		// Private field
		Hashtable table;
		long current; // this is the current ID, starts at 1


		// ObjectIDGenerator must generate a new id for each object instance.
		// If two objects have the same state (i.e. the method Equals() returns true),
		// each one should have a different id.
		// Thus, the object instance cannot be directly used as key of the hashtable.
		// The key is then a wrapper of the object that compares object references
		// instead of object content (unless the object is inmutable, like strings).

		class InstanceWrapper
		{
			object _instance;

			public InstanceWrapper (object instance)
			{
				_instance = instance;
			}

			public override bool Equals (object other)
			{
				InstanceWrapper ow = (InstanceWrapper)other;
				if (_instance is string)
					return _instance.Equals(ow._instance);
				else 
					return (_instance == ow._instance);
			}

			public override int GetHashCode ()
			{
				return _instance.GetHashCode();
			}
		}
		
		// constructor
		public ObjectIDGenerator ()
			: base ()
		{
			table = new Hashtable ();
			current = 1;
		}

		// Methods
		public virtual long GetId (object obj, out bool firstTime)
		{
			if (obj == null)
				throw new ArgumentNullException ("The obj parameter is null.");

			InstanceWrapper iw = new InstanceWrapper(obj);

			object val = table [iw];

			if (val != null) {
				firstTime = false;
				return (long) val;

			} else {
				firstTime = true;
				table.Add (iw, current);
				return current ++; 
			}
		}

		public virtual long HasId (object obj, out bool firstTime)
		{
			if (obj == null)
				throw new ArgumentNullException ("The obj parameter is null.");

 			object val = table [new InstanceWrapper(obj)];
 
 			if (val != null) {
 				firstTime = false;
 				return (long) val;

			} else {				
				firstTime = true;
				return 0L; // 0 is the null ID
			}
		}

		internal long NextId
		{
			get { return current ++; }
		}
	}
}
