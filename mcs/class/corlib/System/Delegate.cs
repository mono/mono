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
using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	[MonoTODO]
	public abstract class Delegate : ICloneable, ISerializable {
		protected Type target_type;
		protected object m_target;
		protected string method;
		protected IntPtr method_ptr;

		protected Delegate (object target, string method)
		{
			if (target == null)
				throw new ArgumentNullException (Locale.GetText ("Target object is null"));

			if (method == null)
				throw new ArgumentNullException (Locale.GetText ("method name is null"));

			this.target_type = null;
			this.method_ptr = IntPtr.Zero;
			this.m_target = target;
			this.method = method;
		}

		protected Delegate (Type target_type, string method)
		{
			if (m_target == null)
				throw new ArgumentNullException (Locale.GetText ("Target type is null"));

			if (method == null)
				throw new ArgumentNullException (Locale.GetText ("method string is null"));

			this.target_type = target_type;
			this.method_ptr = IntPtr.Zero;
			this.m_target = null;
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
				return m_target;
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
			    (d.m_target == m_target) &&
			    (d.method == method))
				return true;

			return false;
		}

		public override int GetHashCode ()
		{
			return method.GetHashCode ();
		}

		// This is from ISerializable
		[MonoTODO]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// TODO: IMPLEMENT ME
		}

		public static Delegate Combine (Delegate a, Delegate b)
		{
			if (a == null){
				if (b == null)
					return null;
				return b;
			} else 
				if (b == null)
					return a;

			if (a.GetType () != b.GetType ())
				throw new ArgumentException (Locale.GetText ("Incompatible Delegate Types"));
			
			return a.CombineImpl (b);
		}

		protected virtual Delegate CombineImpl (Delegate d)
		{
			throw new MulticastNotSupportedException ("");
		}

		[MonoTODO]
		public static Delegate Remove( Delegate source, Delegate value) {
			throw new NotImplementedException ();
		}
	}
}
