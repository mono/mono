//
// System.Object.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// This should probably be implemented in IL instead of C#, to
// use internalcalls to get its hands on the underlying Type
// of an object. 
//

namespace System {

	public class Object {

		// <summary>
		//   Compares this object to the specified object.
		//   Returns true if they are equal, false otherwise.
		// </summary>
		public virtual bool Equals (object o)
		{
			return this == o;
		}

		// <summary>
		//   Compares two objects for equality
		// </summary>
		public static bool Equals (object a, object b)
		{
			if (a == null) {
				if (b == null)
					return true;
				return false;
			} else {
				if (b == null)
					return false;
				return a.Equals (b);
			}
		}

		// <summary>
		//   Initializes a new instance of the object class.
		// </summary>
		public Object ()
		{
		}

		// <summary>
		//   Object destructor. 
		// </summary>
		~Object ()
		{
		}

		// <summary>
		//   Returns a hashcode for this object.  Each derived
		//   class should return a hash code that makes sense
		//   for that particular implementation of the object.
		// </summary>
		public virtual int GetHashCode ()
		{
			return 0;
		}

		// <summary>
		//   Returns the Type associated with the object.
		// </summary>
		public Type GetType ()
		{
			// TODO: This probably needs to be tied up
			// with the Type system.  Private communications
			// channel? 
			// Type is abstract, so the following line won't cut it.
			//return new Type ();
			return null;
		}

		// <summary>
		//   Shallow copy of the object.
		// </summary>
		protected object MemberwiseClone ()
		{
			return null;	
		}

		// <summary>
		//   Returns a stringified representation of the object.
		//   This is not supposed to be used for user presentation,
		//   use Format() for that and IFormattable.
		//
		//   ToString is mostly used for debugging purposes. 
		// </summary>
		public virtual string ToString ()
		{
			return GetType().FullName;
		}

		// <summary>
		//   Tests whether a is equal to b.
		//   Can not figure out why this even exists
		// </summary>
		public static bool ReferenceEquals (object a, object b)
		{
			return (a == b);
		}

		
	}
}
