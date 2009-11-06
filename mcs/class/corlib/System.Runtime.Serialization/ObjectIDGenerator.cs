//
// System.Runtime.Serialization.ObjectIDGenerator.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez (lsg@ctv.es)
//
// (C) Ximian, Inc.
// Copyright (C) 2004,2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;

namespace System.Runtime.Serialization
{
	[Serializable]
	[MonoTODO ("Serialization format not compatible with.NET")]
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
	public class ObjectIDGenerator
	{
		// Private field
		Hashtable table;
		long current; // this is the current ID, starts at 1
		static InstanceComparer comparer = new InstanceComparer ();


		// ObjectIDGenerator must generate a new id for each object instance.
		// If two objects have the same state (i.e. the method Equals() returns true),
		// each one should have a different id.
		// Thus, the object instance cannot be directly used as key of the hashtable.
		// InstanceComparer compares object references instead of object content
		// (unless the object is inmutable, like strings).

		class InstanceComparer: IComparer, IHashCodeProvider
		{
			int IComparer.Compare (object o1, object o2)
			{
				if (o1 is string)
					return o1.Equals(o2) ? 0 : 1;
				else 
					return (o1 == o2) ? 0 : 1;
			}

			int IHashCodeProvider.GetHashCode (object o)
			{
				return object.InternalGetHashCode (o);
			}
		}
		
		// constructor
		public ObjectIDGenerator ()
			: base ()
		{
			table = new Hashtable (comparer, comparer);
			current = 1;
		}

		// Methods
		public virtual long GetId (object obj, out bool firstTime)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			object val = table [obj];

			if (val != null) {
				firstTime = false;
				return (long) val;

			} else {
				firstTime = true;
				table.Add (obj, current);
				return current ++; 
			}
		}

		public virtual long HasId (object obj, out bool firstTime)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

 			object val = table [obj];
 
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
