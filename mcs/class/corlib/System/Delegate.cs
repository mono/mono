//
// System.Delegate.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO:  Mucho left to implement
//

using System;
using System.Runtime.Serialization;

namespace System {

	public abstract class Delegate : ICloneable, ISerializable {
		protected Type target_type;
		protected object target;
		protected string method;
		protected IntPtr method_ptr;
		
		protected Delegate (object target, string method)
		{
			if (target == null)
				throw new ArgumentNullException ("Target object is null");

			if (method == null)
				throw new ArgumentNullException ("method name is null");

			this.target_type = null;
			this.method_ptr = IntPtr.Zero;
			this.target = target;
			this.method = method;
		}

		protected Delegate (Type target_type, string method)
		{
			if (target == null)
				throw new ArgumentNullException ("Target type is null");

			if (method == null)
				throw new ArgumentNullException ("method string is null");

			this.target_type = target_type;
			this.method_ptr = IntPtr.Zero;
			this.target = null;
			this.method = method;
		}

#if NOTYET
		public MethodInfo Method {
			get {
				return null;
			}
		}
#endif

		public object Target {
			get {
				return target;
			}
		}


		//
		// Methods
		//

		public abstract object Clone ();

		public override bool Equals (object o)
		{
			if (!(o is System.Delegate))
				return false;

			Delegate d = (Delegate) o;
			
			if ((d.target_type == target_type) &&
			    (d.target == target) &&
			    (d.method == method))
				return true;

			return false;
		}

		public override int GetHashCode ()
		{
			return method.GetHashCode ();
		}

		// This is from ISerializable
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// TODO: IMPLEMENT ME
		}
		
	}
}
