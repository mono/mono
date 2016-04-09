//
// System.Delegate.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//   Dietmar Maurer (dietmar@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright 2014 Xamarin, Inc (http://www.xamarin.com)
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
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	/* Contains the rarely used fields of Delegate */
	sealed class DelegateData
	{
		public Type target_type;
		public string method_name;
		public bool curried_first_arg;
	}

	[ClassInterface (ClassInterfaceType.AutoDual)]
	[System.Runtime.InteropServices.ComVisible (true)]
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public abstract class Delegate : ICloneable, ISerializable
	{
		#region Sync with object-internals.h
#pragma warning disable 169, 414, 649
		private IntPtr method_ptr;
		private IntPtr invoke_impl;
		private object m_target;
		private IntPtr method;
		private IntPtr delegate_trampoline;
		private IntPtr extra_arg;
		private IntPtr method_code;
		private MethodInfo method_info;

		// Keep a ref of the MethodInfo passed to CreateDelegate.
		// Used to keep DynamicMethods alive.
		private MethodInfo original_method_info;

		private DelegateData data;

		private bool method_is_virtual;
#pragma warning restore 169, 414, 649
		#endregion

		protected Delegate (object target, string method)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			if (method == null)
				throw new ArgumentNullException ("method");

			this.m_target = target;
			this.data = new DelegateData ();
			this.data.method_name = method;
		}

		protected Delegate (Type target, string method)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			if (method == null)
				throw new ArgumentNullException ("method");

			this.data = new DelegateData ();
			this.data.method_name = method;
			this.data.target_type = target;
		}

		public MethodInfo Method {
			get {
				return GetMethodImpl ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern MethodInfo GetVirtualMethod_internal ();

		public object Target {
			get {
				return m_target;
			}
		}

		//
		// Methods
		//

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Delegate CreateDelegate_internal (Type type, object target, MethodInfo info, bool throwOnBindFailure);

		private static bool arg_type_match (Type delArgType, Type argType) {
			bool match = delArgType == argType;

			// Delegate contravariance
			if (!match) {
				if (!argType.IsValueType && argType.IsAssignableFrom (delArgType))
					match = true;
			}
			// enum basetypes
			if (!match) {
				if (delArgType.IsEnum && Enum.GetUnderlyingType (delArgType) == argType)
					match = true;
			}

			return match;
		}

		private static bool arg_type_match_this (Type delArgType, Type argType, bool boxedThis) {
			bool match;
			if (argType.IsValueType)
				match = delArgType.IsByRef && delArgType.GetElementType () == argType ||
						(boxedThis && delArgType == argType);
			else
				match = delArgType == argType || argType.IsAssignableFrom (delArgType);

			return match;
		}
		private static bool return_type_match (Type delReturnType, Type returnType) {
			bool returnMatch = returnType == delReturnType;

			if (!returnMatch) {
				// Delegate covariance
				if (!returnType.IsValueType && delReturnType.IsAssignableFrom (returnType))
					returnMatch = true;
			}

			return returnMatch;
		}

		public static Delegate CreateDelegate (Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure)
		{
			return CreateDelegate (type, firstArgument, method, throwOnBindFailure, true);
		}

		static Delegate CreateDelegate (Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure, bool allowClosed)
		{
			// The name of the parameter changed in 2.0
			object target = firstArgument;

			if (type == null)
				throw new ArgumentNullException ("type");

			if (method == null)
				throw new ArgumentNullException ("method");

			if (!type.IsSubclassOf (typeof (MulticastDelegate)))
				throw new ArgumentException ("type is not a subclass of Multicastdelegate");

			MethodInfo invoke = type.GetMethod ("Invoke");

			if (!return_type_match (invoke.ReturnType, method.ReturnType)) {
				if (throwOnBindFailure)
					throw new ArgumentException ("method return type is incompatible");
				else
					return null;
			}

			ParameterInfo[] delargs = invoke.GetParametersInternal ();
			ParameterInfo[] args = method.GetParametersInternal ();

			bool argLengthMatch;

			if (target != null) {
				// delegate closed over target
				if (!method.IsStatic)
					// target is passed as this
					argLengthMatch = (args.Length == delargs.Length);
				else
					// target is passed as the first argument to the static method
					argLengthMatch = (args.Length == delargs.Length + 1);
			} else {
				if (!method.IsStatic) {
					//
					// Net 2.0 feature. The first argument of the delegate is passed
					// as the 'this' argument to the method.
					//
					argLengthMatch = (args.Length + 1 == delargs.Length);

					if (!argLengthMatch)
						// closed over a null reference
						argLengthMatch = (args.Length == delargs.Length);
				} else {
					argLengthMatch = (args.Length == delargs.Length);

					if (!argLengthMatch)
						// closed over a null reference
						argLengthMatch = args.Length == delargs.Length + 1;
				}
			}
			if (!argLengthMatch) {
				if (throwOnBindFailure)
					throw new ArgumentException ("method argument length mismatch");
				else
					return null;
			}

			bool argsMatch;
			DelegateData delegate_data = new DelegateData ();

			if (target != null) {
				if (!method.IsStatic) {
					argsMatch = arg_type_match_this (target.GetType (), method.DeclaringType, true);
					for (int i = 0; i < args.Length; i++)
						argsMatch &= arg_type_match (delargs [i].ParameterType, args [i].ParameterType);
				} else {
					argsMatch = arg_type_match (target.GetType (), args [0].ParameterType);
					for (int i = 1; i < args.Length; i++)
						argsMatch &= arg_type_match (delargs [i - 1].ParameterType, args [i].ParameterType);

					delegate_data.curried_first_arg = true;
				}
			} else {
				if (!method.IsStatic) {
					if (args.Length + 1 == delargs.Length) {
						// The first argument should match this
						argsMatch = arg_type_match_this (delargs [0].ParameterType, method.DeclaringType, false);
						for (int i = 0; i < args.Length; i++)
							argsMatch &= arg_type_match (delargs [i + 1].ParameterType, args [i].ParameterType);
					} else {
						// closed over a null reference
						argsMatch = allowClosed;
						for (int i = 0; i < args.Length; i++)
							argsMatch &= arg_type_match (delargs [i].ParameterType, args [i].ParameterType);
					}
				} else {
					if (delargs.Length + 1 == args.Length) {
						// closed over a null reference
						argsMatch = !(args [0].ParameterType.IsValueType || args [0].ParameterType.IsByRef) && allowClosed;
						for (int i = 0; i < delargs.Length; i++)
							argsMatch &= arg_type_match (delargs [i].ParameterType, args [i + 1].ParameterType);

						delegate_data.curried_first_arg = true;
					} else {
						argsMatch = true;
						for (int i = 0; i < args.Length; i++)
							argsMatch &= arg_type_match (delargs [i].ParameterType, args [i].ParameterType);
					}
				}
			}

			if (!argsMatch) {
				if (throwOnBindFailure)
					throw new ArgumentException ("method arguments are incompatible");
				else
					return null;
			}

			Delegate d = CreateDelegate_internal (type, target, method, throwOnBindFailure);
			if (d != null)
				d.original_method_info = method;
			if (delegate_data != null)
				d.data = delegate_data;
			return d;
		}

		public static Delegate CreateDelegate (Type type, object firstArgument, MethodInfo method)
		{
			return CreateDelegate (type, firstArgument, method, true, true);
		}

		public static Delegate CreateDelegate (Type type, MethodInfo method, bool throwOnBindFailure)
		{
			return CreateDelegate (type, null, method, throwOnBindFailure, false);
		}

		public static Delegate CreateDelegate (Type type, MethodInfo method)
		{
			return CreateDelegate (type, method, true);
		}

		public static Delegate CreateDelegate (Type type, object target, string method)
		{
			return CreateDelegate (type, target, method, false);
		}

		static MethodInfo GetCandidateMethod (Type type, Type target, string method, BindingFlags bflags, bool ignoreCase, bool throwOnBindFailure)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (method == null)
				throw new ArgumentNullException ("method");

			if (!type.IsSubclassOf (typeof (MulticastDelegate)))
				throw new ArgumentException ("type is not subclass of MulticastDelegate.");

			MethodInfo invoke = type.GetMethod ("Invoke");
			ParameterInfo [] delargs = invoke.GetParametersInternal ();
			Type[] delargtypes = new Type [delargs.Length];

			for (int i=0; i<delargs.Length; i++)
				delargtypes [i] = delargs [i].ParameterType;

			/* 
			 * FIXME: we should check the caller has reflection permission
			 * or if it lives in the same assembly...
			 */

			/*
			 * since we need to walk the inheritance chain anyway to
			 * find private methods, adjust the bindingflags to ignore
			 * inherited methods
			 */
			BindingFlags flags = BindingFlags.ExactBinding |
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly | bflags;

			if (ignoreCase)
				flags |= BindingFlags.IgnoreCase;

			MethodInfo info = null;

			for (Type targetType = target; targetType != null; targetType = targetType.BaseType) {
				MethodInfo mi = targetType.GetMethod (method, flags,
					null, delargtypes, EmptyArray<ParameterModifier>.Value);
				if (mi != null && return_type_match (invoke.ReturnType, mi.ReturnType)) {
					info = mi;
					break;
				}
			}

			if (info == null) {
				if (throwOnBindFailure)
					throw new ArgumentException ("Couldn't bind to method '" + method + "'.");
				else
					return null;
			}

			return info;
		}

 		public static Delegate CreateDelegate (Type type, Type target, string method, bool ignoreCase, bool throwOnBindFailure)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			MethodInfo info = GetCandidateMethod (type, target, method,
				BindingFlags.Static, ignoreCase, throwOnBindFailure);
			if (info == null)
				return null;

			return CreateDelegate_internal (type, null, info, throwOnBindFailure);
		}

 		public static Delegate CreateDelegate (Type type, Type target, string method) {
			return CreateDelegate (type, target, method, false, true);
		}

 		public static Delegate CreateDelegate (Type type, Type target, string method, bool ignoreCase) {
			return CreateDelegate (type, target, method, ignoreCase, true);
		}

		public static Delegate CreateDelegate (Type type, object target, string method, bool ignoreCase, bool throwOnBindFailure)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			MethodInfo info = GetCandidateMethod (type, target.GetType (), method,
				BindingFlags.Instance, ignoreCase, throwOnBindFailure);
			if (info == null)
				return null;

			return CreateDelegate_internal (type, target, info, throwOnBindFailure);
		}

		public static Delegate CreateDelegate (Type type, object target, string method, bool ignoreCase) {
			return CreateDelegate (type, target, method, ignoreCase, true);
		}

		public object DynamicInvoke (params object[] args)
		{
			return DynamicInvokeImpl (args);
		}

		void InitializeDelegateData ()
		{
			DelegateData delegate_data = new DelegateData ();
			if (method_info.IsStatic) {
				if (m_target != null) {
					delegate_data.curried_first_arg = true;
				} else {
					MethodInfo invoke = GetType ().GetMethod ("Invoke");
					if (invoke.GetParametersCount () + 1 == method_info.GetParametersCount ())
						delegate_data.curried_first_arg = true;
				}
			}
			this.data = delegate_data;
		}

		protected virtual object DynamicInvokeImpl (object[] args)
		{
			if (Method == null) {
				Type[] mtypes = new Type [args.Length];
				for (int i = 0; i < args.Length; ++i) {
					mtypes [i] = args [i].GetType ();
				}
				method_info = m_target.GetType ().GetMethod (data.method_name, mtypes);
			}

			var target = m_target;
			if (this.data == null)
				InitializeDelegateData ();

			if (Method.IsStatic) {
				//
				// The delegate is bound to m_target
				//
				if (data.curried_first_arg) {
					if (args == null) {
						args = new [] { target };
					} else {
						Array.Resize (ref args, args.Length + 1);
						Array.Copy (args, 0, args, 1, args.Length - 1);
						args [0] = target;
					}

					target = null;
				}
			} else {
				if (m_target == null && args != null && args.Length > 0) {
					target = args [0];
					Array.Copy (args, 1, args, 0, args.Length - 1);
					Array.Resize (ref args, args.Length - 1);
				}
			}

			return Method.Invoke (target, args);
		}

		public virtual object Clone ()
		{
			return MemberwiseClone ();
		}

		public override bool Equals (object obj)
		{
			Delegate d = obj as Delegate;

			if (d == null)
				return false;

			// Do not compare method_ptr, since it can point to a trampoline
			if (d.m_target == m_target && d.Method == Method) {
				if (d.data != null || data != null) {
					/* Uncommon case */
					if (d.data != null && data != null)
						return (d.data.target_type == data.target_type && d.data.method_name == data.method_name);
					else {
						if (d.data != null)
							return d.data.target_type == null;
						if (data != null)
							return data.target_type == null;
						return false;
					}
				}
				return true;
			}

			return false;
		}

		public override int GetHashCode ()
		{
			/* same implementation as CoreCLR */
			return GetType ().GetHashCode ();
		}

		protected virtual MethodInfo GetMethodImpl ()
		{
			if (method_info != null) {
				return method_info;
			} else {
				if (method != IntPtr.Zero) {
					if (!method_is_virtual)
						method_info = (MethodInfo)MethodBase.GetMethodFromHandleNoGenericCheck (new RuntimeMethodHandle (method));
					else
						method_info = GetVirtualMethod_internal ();
				}
				return method_info;
			}
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
			if (a == null)
				return b;

			if (b == null)
				return a;

			if (a.GetType () != b.GetType ())
				throw new ArgumentException (Locale.GetText ("Incompatible Delegate Types. First is {0} second is {1}.", a.GetType ().FullName, b.GetType ().FullName));

			return a.CombineImpl (b);
		}

		/// <symmary>
		///   Returns a new MulticastDelegate holding the
		///   concatenated invocation lists of an Array of MulticastDelegates
		/// </symmary>
		[System.Runtime.InteropServices.ComVisible (true)]
		public static Delegate Combine (params Delegate[] delegates)
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
			throw new MulticastNotSupportedException (String.Empty);
		}

		public static Delegate Remove (Delegate source, Delegate value) 
		{
			if (source == null)
				return null;

			if (value == null)
				return source;

			if (source.GetType () != value.GetType ())
				throw new ArgumentException (Locale.GetText ("Incompatible Delegate Types. First is {0} second is {1}.", source.GetType ().FullName, value.GetType ().FullName));

			return source.RemoveImpl (value);
		}

		protected virtual Delegate RemoveImpl (Delegate d)
		{
			if (this.Equals (d))
				return null;

			return this;
		}

		public static Delegate RemoveAll (Delegate source, Delegate value)
		{
			Delegate tmp = source;
			while ((source = Delegate.Remove (source, value)) != tmp)
				tmp = source;

			return tmp;
		}

		public static bool operator == (Delegate d1, Delegate d2)
		{
			if ((object)d1 == null) {
				if ((object)d2 == null)
					return true;
				return false;
			} else if ((object) d2 == null)
				return false;
			
			return d1.Equals (d2);
		}

		public static bool operator != (Delegate d1, Delegate d2)
		{
			return !(d1 == d2);
		}

		internal bool IsTransparentProxy ()
		{
#if DISABLE_REMOTING
			return false;
#else
			return RemotingServices.IsTransparentProxy (m_target);
#endif
		}

		internal static Delegate CreateDelegateNoSecurityCheck (RuntimeType type, Object firstArgument, MethodInfo method)
		{
			return CreateDelegate_internal (type, firstArgument, method, true);
		}

		/* Internal call largely inspired from MS Delegate.InternalAllocLike */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static MulticastDelegate AllocDelegateLike_internal (Delegate d);
	}
}
