//
// System.Runtime.Serialization.ObjectIDGenerator.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
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

			if (table.ContainsKey (obj)) {
				firstTime = false;
				return (long) table [obj];

			} else {
				firstTime = true;
				table.Add (obj, current);
				return current ++; 
			}
		}

		public virtual long HasId (object obj, out bool firstTime)
		{
			if (obj == null)
				throw new ArgumentNullException ("The obj parameter is null.");

			if (table.ContainsKey (obj)) {
				firstTime = false;
				return (long) table [obj];

			} else {				
				firstTime = true;
				return 0L; // 0 is the null ID
			}
		}
	}
}
