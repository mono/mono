//
// System.Object.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System.Runtime.CompilerServices;

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
			if (a == b)
				return true;
			
			if (a == null || b == null)
				return false;

			return a.Equals (b);
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
		public virtual int GetHashCode () {
			return InternalGetHashCode (this);
		}

		// <summary>
		//   Returns the Type associated with the object.
		// </summary>
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern Type GetType ();

		// <summary>
		//   Shallow copy of the object.
		// </summary>
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern object MemberwiseClone ();

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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern int InternalGetHashCode (object o);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern IntPtr obj_address ();
 
		void FieldGetter (string typeName, string fieldName, ref object val)
		{
			/* never called */
		}

		void FieldSetter (string typeName, string fieldName, object val)
		{
			/* never called */
		}
	}
}
