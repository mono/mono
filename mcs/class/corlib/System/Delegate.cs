//
// System.Delegate.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;


namespace System {

	[MonoTODO]
	public abstract class Delegate : ICloneable, ISerializable {
		protected Type target_type;
		protected object m_target;
		protected string method_name;
		protected IntPtr method_ptr;
		protected IntPtr delegate_trampoline;
		protected MethodInfo method_info;

		protected Delegate (object target, string method)
		{
			if (target == null)
				throw new ArgumentNullException (Locale.GetText ("Target object is null"));

			if (method == null)
				throw new ArgumentNullException (Locale.GetText ("method name is null"));

			this.target_type = null;
			this.method_ptr = IntPtr.Zero;
			this.m_target = target;
			this.method_name = method;
		}

		protected Delegate (Type target_type, string method)
		{
			if (target_type == null)
				throw new ArgumentNullException (Locale.GetText ("Target type is null"));

			if (method == null)
				throw new ArgumentNullException (Locale.GetText ("method string is null"));

			this.target_type = target_type;
			this.method_ptr = IntPtr.Zero;
			this.m_target = null;
			this.method_name = method;
		}

		public MethodInfo Method {
			get {
				return method_info;
			}
		}

		public object Target {
			get {
				return m_target;
			}
		}

		//
		// Methods
		//

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern Delegate CreateDelegate_internal (Type type, object target, MethodInfo info);

		public static Delegate CreateDelegate (Type type, MethodInfo info)
		{
			if (type == null)
				throw new ArgumentNullException (Locale.GetText ("Type is null"));

			if (info == null)
				throw new ArgumentNullException (Locale.GetText ("MethodInfo is null"));

			if (!type.IsSubclassOf (typeof (MulticastDelegate)))
				throw new ArgumentException ("type");

			ParameterInfo[] delargs = type.GetMethod ("Invoke").GetParameters ();
			Type[] delargtypes = new Type [delargs.Length];
			ParameterInfo[] args = info.GetParameters ();

			for (int i=0; i<delargs.Length; i++)
				delargtypes [i] = delargs [i].ParameterType;
			if (args.Length != delargtypes.Length)
				throw new ArgumentException ("info");
			for (int i=0; i<delargs.Length; i++) {
				if (delargtypes [i] != args [i].ParameterType)
					throw new ArgumentException ("info");
			}
			return CreateDelegate_internal (type, null, info);
		}

		public static Delegate CreateDelegate (Type type, object target, string method)
		{
			return CreateDelegate(type, target, method, false);
		}

 		public static Delegate CreateDelegate (Type type, Type target, string method)
		{
			if (type == null)
				throw new ArgumentNullException (Locale.GetText ("Type is null"));

			if (target == null)
				throw new ArgumentNullException (Locale.GetText ("Target type is null"));

			if (method == null)
				throw new ArgumentNullException (Locale.GetText ("method string is null"));

			if (!type.IsSubclassOf (typeof (MulticastDelegate)))
				throw new ArgumentException ("type is not subclass of MulticastDelegate");

			ParameterInfo[] delargs = type.GetMethod ("Invoke").GetParameters ();
			Type[] delargtypes = new Type [delargs.Length];

			for (int i=0; i<delargs.Length; i++)
				delargtypes [i] = delargs [i].ParameterType;

			/* 
			 * FIXME: we should check the caller has reflection permission
			 * or if it lives in the same assembly...
			 */
			BindingFlags flags = BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;
			MethodInfo info = target.GetMethod (method, flags, null, delargtypes, new ParameterModifier [0]);

			if (info == null)
				throw new ArgumentException ("Couldn't bind to method");

			return CreateDelegate_internal (type, null, info);
		}

		public static Delegate CreateDelegate (Type type, object target, string method, bool ignorecase)
		{
			if (type == null)
				throw new ArgumentNullException (Locale.GetText ("Type is null"));

			if (target == null)
				throw new ArgumentNullException (Locale.GetText ("Target object is null"));

			if (method == null)
				throw new ArgumentNullException (Locale.GetText ("method string is null"));

			if (!type.IsSubclassOf (typeof (MulticastDelegate)))
				throw new ArgumentException ("type");

			ParameterInfo[] delargs = type.GetMethod ("Invoke").GetParameters ();
			Type[] delargtypes = new Type [delargs.Length];

			for (int i=0; i<delargs.Length; i++)
				delargtypes [i] = delargs [i].ParameterType;

			/* 
			 * FIXME: we should check the caller has reflection permission
			 * or if it lives in the same assembly...
			 */
			BindingFlags flags = BindingFlags.ExactBinding | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

			if (ignorecase)
				flags |= BindingFlags.IgnoreCase;

			MethodInfo info = target.GetType ().GetMethod (method, flags, null, delargtypes, new ParameterModifier [0]);

			if (info == null)
				throw new ArgumentException ("Couldn't bind to method");

			return CreateDelegate_internal (type, target, info);
		}

		public object DynamicInvoke( object[] args )
		{
			return DynamicInvokeImpl( args );
		}

		public virtual object DynamicInvokeImpl( object[] args )
		{
			if (Method == null) {
				Type[] mtypes = new Type [args.Length];
   				for (int i = 0; i < args.Length; ++i) {
      				mtypes [i] = args [i].GetType ();
         		}
				method_info = m_target.GetType ().GetMethod (method_name, mtypes);
			}
			return Method.Invoke( m_target, args );
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public override bool Equals (object o)
		{
			if ( o == null )
				return false;
			
			Delegate d = (Delegate) o;
			if ((d.target_type == target_type) &&
			    (d.m_target == m_target) &&
			    (d.method_name == method_name) &&
			    (d.method_ptr == method_ptr))
				return true;

			return false;
		}

		public override int GetHashCode ()
		{
			return (int)method_ptr;
		}

		// This is from ISerializable
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			DelegateSerializationHolder.GetDelegateData (this, info, context);
		}

		public virtual Delegate[] GetInvocationList()
		{
			return new Delegate[] { this };
		}

		/// <symmary>
		///   Returns a new MulticastDelegate holding the
		///   concatenated invocation lists of MulticastDelegates a and b
		/// </symmary>
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

		/// <symmary>
		///   Returns a new MulticastDelegate holding the
		///   concatenated invocation lists of an Array of MulticastDelegates
		/// </symmary>
		public static Delegate Combine( Delegate[] delegates )
		{
			Delegate retval = null;

			foreach ( Delegate next in delegates )
				retval = Combine( retval, next );

			return retval;
		}


		protected virtual Delegate CombineImpl (Delegate d)
		{
			throw new MulticastNotSupportedException ("");
		}
		
		
		public static Delegate Remove( Delegate source, Delegate value ) 
		{
			if ( source == null )
				return null;
				
			return source.RemoveImpl( value );
		}

		protected virtual Delegate RemoveImpl( Delegate d )
		{
			if ( this.Equals( d ) )
				return null;
		       
			return this;
		}

		public static bool operator ==( Delegate a, Delegate b )
		{
			if ((object)a == null) {
				if ((object)b == null)
					return true;
				return false;
			}
			return a.Equals( b );
		}

		public static bool operator !=( Delegate a, Delegate b )
		{
			return !(a == b);
		}
	}
}
