//
// System.Delegate.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
#if NET_1_1
	[ClassInterface (ClassInterfaceType.AutoDual)]
#endif
	public abstract class Delegate : ICloneable, ISerializable
	{
		private Type target_type;
		private object m_target;
		private string method_name;
		private IntPtr method_ptr;
		private IntPtr delegate_trampoline;
		private MethodInfo method_info;

		protected Delegate (object target, string method)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			if (method == null)
				throw new ArgumentNullException ("method");

			this.target_type = null;
			this.method_ptr = IntPtr.Zero;
			this.m_target = target;
			this.method_name = method;
		}

		protected Delegate (Type target, string method)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			if (method == null)
				throw new ArgumentNullException ("method");

			this.target_type = target;
			this.method_ptr = IntPtr.Zero;
			this.m_target = null;
			this.method_name = method;
		}

		~Delegate () {
			if (delegate_trampoline != IntPtr.Zero)
				FreeTrampoline ();
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Delegate CreateDelegate_internal (Type type, object target, MethodInfo info);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern void FreeTrampoline ();

		public static Delegate CreateDelegate (Type type, MethodInfo method)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (method == null)
				throw new ArgumentNullException ("method");

			if (!type.IsSubclassOf (typeof (MulticastDelegate)))
				throw new ArgumentException ("type");

			if (!method.IsStatic)
				throw new ArgumentException ("The method should be static.", "method");

			ParameterInfo[] delargs = type.GetMethod ("Invoke").GetParameters ();
			ParameterInfo[] args = method.GetParameters ();

			if (args.Length != delargs.Length)
				throw new ArgumentException ("method");
			
			int length = delargs.Length;
			for (int i = 0; i < length; i++)
				if (delargs [i].ParameterType != args [i].ParameterType)
					throw new ArgumentException ("method");

			return CreateDelegate_internal (type, null, method);
		}

		public static Delegate CreateDelegate (Type type, object target, string method)
		{
			return CreateDelegate(type, target, method, false);
		}

 		public static Delegate CreateDelegate (Type type, Type target, string method)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (target == null)
				throw new ArgumentNullException ("target");

			if (method == null)
				throw new ArgumentNullException ("method");

			if (!type.IsSubclassOf (typeof (MulticastDelegate)))
				throw new ArgumentException ("type is not subclass of MulticastDelegate.");

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
				throw new ArgumentException ("Couldn't bind to method.");

			return CreateDelegate_internal (type, null, info);
		}

		public static Delegate CreateDelegate (Type type, object target, string method, bool ignoreCase)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (target == null)
				throw new ArgumentNullException ("target");

			if (method == null)
				throw new ArgumentNullException ("method");

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

			if (ignoreCase)
				flags |= BindingFlags.IgnoreCase;

			MethodInfo info = target.GetType ().GetMethod (method, flags, null, delargtypes, new ParameterModifier [0]);

			if (info == null)
				throw new ArgumentException ("Couldn't bind to method '" + method + "'.");

			return CreateDelegate_internal (type, target, info);
		}

		public object DynamicInvoke (object[] args)
		{
			return DynamicInvokeImpl (args);
		}

		protected virtual object DynamicInvokeImpl (object[] args)
		{
			if (Method == null) {
				Type[] mtypes = new Type [args.Length];
				for (int i = 0; i < args.Length; ++i) {
					mtypes [i] = args [i].GetType ();
				}
				method_info = m_target.GetType ().GetMethod (method_name, mtypes);
			}
			return Method.Invoke (m_target, args);
		}

		public virtual object Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			Delegate d = (Delegate) obj;
			// Do not compare method_ptr, since it can point to a trampoline
			if ((d.target_type == target_type) && (d.m_target == m_target) &&
				(d.method_name == method_name) && (d.method_info == method_info))
				return true;

			return false;
		}

		public override int GetHashCode ()
		{
			return (int)method_ptr;
		}

		protected virtual MethodInfo GetMethodImpl ()
		{
			return Method;
		}

		// This is from ISerializable
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
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
			if (a == null) {
				if (b == null)
					return null;
				return b;
			} else 
				if (b == null)
					return a;

			if (a.GetType () != b.GetType ())
				throw new ArgumentException (Locale.GetText ("Incompatible Delegate Types."));
			
			return a.CombineImpl (b);
		}

		/// <symmary>
		///   Returns a new MulticastDelegate holding the
		///   concatenated invocation lists of an Array of MulticastDelegates
		/// </symmary>
		public static Delegate Combine (Delegate[] delegates)
		{
			if (delegates == null)
				return null;

			Delegate retval = null;

			foreach (Delegate next in delegates)
				retval = Combine (retval, next);

			return retval;
		}

		protected virtual Delegate CombineImpl (Delegate d)
		{
			throw new MulticastNotSupportedException ("");
		}

		public static Delegate Remove (Delegate source, Delegate value) 
		{
			if (source == null)
				return null;

			return source.RemoveImpl (value);
		}

		protected virtual Delegate RemoveImpl (Delegate d)
		{
			if (this.Equals (d))
				return null;

			return this;
		}
#if NET_1_1
		public static Delegate RemoveAll (Delegate source, Delegate value)
		{
			Delegate tmp = source;
			while ((source = Delegate.Remove (source, value)) != tmp)
				tmp = source;

			return tmp;
		}
#endif
		public static bool operator == (Delegate a, Delegate b)
		{
			if ((object)a == null) {
				if ((object)b == null)
					return true;
				return false;
			} else if ((object) b == null)
				return false;
			
			return a.Equals (b);
		}

		public static bool operator != (Delegate a, Delegate b)
		{
			return !(a == b);
		}
	}
}
