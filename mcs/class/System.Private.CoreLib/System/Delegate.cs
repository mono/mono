using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	/* Contains the rarely used fields of Delegate */
	sealed class DelegateData
	{
		public Type target_type;
		public string method_name;
		public bool curried_first_arg;
	}

	[StructLayout (LayoutKind.Sequential)]
	partial class Delegate
	{
		#region Sync with object-internals.h
		IntPtr method_ptr;
		IntPtr invoke_impl;
		object _target;
		IntPtr method;
		IntPtr delegate_trampoline;
		IntPtr extra_arg;
		IntPtr method_code;
		IntPtr interp_method;
		IntPtr interp_invoke_impl;
		MethodInfo method_info;

		// Keep a ref of the MethodInfo passed to CreateDelegate.
		// Used to keep DynamicMethods alive.
		MethodInfo original_method_info;

		DelegateData data;

		bool method_is_virtual;
		#endregion

		protected Delegate (object target, string method)
		{
			if (target == null)
				throw new ArgumentNullException (nameof(target));

			if (method == null)
				throw new ArgumentNullException (nameof(method));

			this._target = target;
			this.data = new DelegateData () {
				method_name = method
			};
		}

		protected Delegate (Type target, string method)
		{
			if (target == null)
				throw new ArgumentNullException (nameof (target));

			if (target.ContainsGenericParameters)
				throw new ArgumentException (SR.Arg_UnboundGenParam, nameof (target));

			if (method == null)
				throw new ArgumentNullException (nameof (method));

			if (!target.IsRuntimeImplemented ())
				throw new ArgumentException (SR.Argument_MustBeRuntimeType, nameof (target));

			this.data = new DelegateData () {
				method_name = method,
				target_type = target
			};
		}

		public object Target => GetTarget ();

		internal virtual object GetTarget () => _target;

		public static Delegate CreateDelegate (Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure)
		{
			return CreateDelegate (type, firstArgument, method, throwOnBindFailure, true);
		}

		public static Delegate CreateDelegate (Type type, MethodInfo method, bool throwOnBindFailure)
		{
			return CreateDelegate (type, null, method, throwOnBindFailure, false);
		}

		static Delegate CreateDelegate (Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure, bool allowClosed)
		{
			if (type == null)
				throw new ArgumentNullException (nameof (type));
			if (method == null)
				throw new ArgumentNullException (nameof (method));

			RuntimeType rtType = type as RuntimeType;
			if (rtType == null)
				throw new ArgumentException (SR.Argument_MustBeRuntimeType, nameof (type));
			if (!(method is RuntimeMethodInfo || method is System.Reflection.Emit.DynamicMethod))
				throw new ArgumentException (SR.Argument_MustBeRuntimeMethodInfo, nameof (method));

			if (!rtType.IsDelegate ())
				throw new ArgumentException (SR.Arg_MustBeDelegate, nameof (type));

			if (!IsMatchingCandidate (type, firstArgument, method, allowClosed, out DelegateData delegate_data)) {
				if (throwOnBindFailure)
					throw new ArgumentException (SR.Arg_DlgtTargMeth);

				return null;
			}

			Delegate d = CreateDelegate_internal (type, firstArgument, method, throwOnBindFailure);
			if (d != null)
				d.original_method_info = method;
			if (delegate_data != null)
				d.data = delegate_data;
			return d;
		}

		public static Delegate CreateDelegate (Type type, object target, string method, bool ignoreCase, bool throwOnBindFailure)
		{
			if (type == null)
				throw new ArgumentNullException (nameof (type));
			if (target == null)
				throw new ArgumentNullException (nameof (target));
			if (method == null)
				throw new ArgumentNullException (nameof (method));

			RuntimeType rtType = type as RuntimeType;
			if (rtType == null)
				throw new ArgumentException (SR.Argument_MustBeRuntimeType, nameof (type));
			if (!rtType.IsDelegate ())
				throw new ArgumentException (SR.Arg_MustBeDelegate, nameof (type));

			MethodInfo info = GetCandidateMethod (type, target.GetType (), method, BindingFlags.Instance, ignoreCase);
			if (info == null) {
				if (throwOnBindFailure)
					throw new ArgumentException (SR.Arg_DlgtTargMeth);

				return null;
			}

			return CreateDelegate_internal (type, null, info, throwOnBindFailure);
		}

		public static Delegate CreateDelegate (Type type, Type target, string method, bool ignoreCase, bool throwOnBindFailure)
		{
			if (type == null)
				throw new ArgumentNullException (nameof (type));
			if (target == null)
				throw new ArgumentNullException (nameof (target));
			if (target.ContainsGenericParameters)
				throw new ArgumentException (SR.Arg_UnboundGenParam, nameof (target));
			if (method == null)
				throw new ArgumentNullException (nameof (method));

			RuntimeType rtType = type as RuntimeType;
			RuntimeType rtTarget = target as RuntimeType;
			if (rtType == null)
				throw new ArgumentException (SR.Argument_MustBeRuntimeType, nameof (type));
			if (rtTarget == null)
				throw new ArgumentException (SR.Argument_MustBeRuntimeType, nameof (target));
			if (!rtType.IsDelegate ())
				throw new ArgumentException (SR.Arg_MustBeDelegate, nameof (type));

			MethodInfo info = GetCandidateMethod (type, target, method, BindingFlags.Static, ignoreCase);
			if (info == null) {
				if (throwOnBindFailure)
					throw new ArgumentException (SR.Arg_DlgtTargMeth);

				return null;
			}

			return CreateDelegate_internal (type, null, info, throwOnBindFailure);
		}

		static MethodInfo GetCandidateMethod (Type type, Type target, string method, BindingFlags bflags, bool ignoreCase)
		{
			MethodInfo invoke = type.GetMethod ("Invoke");
			ParameterInfo [] delargs = invoke.GetParametersInternal ();
			Type[] delargtypes = new Type [delargs.Length];

			for (int i = 0; i < delargs.Length; i++)
				delargtypes [i] = delargs [i].ParameterType;

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
					null, delargtypes, Array.Empty<ParameterModifier>());
				if (mi != null && IsReturnTypeMatch (invoke.ReturnType, mi.ReturnType)) {
					info = mi;
					break;
				}
			}

			return info;
		}

		static bool IsMatchingCandidate (Type type, object target, MethodInfo method, bool allowClosed, out DelegateData delegate_data)
		{
			MethodInfo invoke = type.GetMethod ("Invoke");

			if (!IsReturnTypeMatch (invoke.ReturnType, method.ReturnType)) {
				delegate_data = null;
				return false;
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
				delegate_data = null;
				return false;
			}

			bool argsMatch;
			delegate_data = new DelegateData ();

			if (target != null) {
				if (!method.IsStatic) {
					argsMatch = IsArgumentTypeMatchWithThis (target.GetType (), method.DeclaringType, true);
					for (int i = 0; i < args.Length; i++)
						argsMatch &= IsArgumentTypeMatch (delargs [i].ParameterType, args [i].ParameterType);
				} else {
					argsMatch = IsArgumentTypeMatch (target.GetType (), args [0].ParameterType);
					for (int i = 1; i < args.Length; i++)
						argsMatch &= IsArgumentTypeMatch (delargs [i - 1].ParameterType, args [i].ParameterType);

					delegate_data.curried_first_arg = true;
				}
			} else {
				if (!method.IsStatic) {
					if (args.Length + 1 == delargs.Length) {
						// The first argument should match this
						argsMatch = IsArgumentTypeMatchWithThis (delargs [0].ParameterType, method.DeclaringType, false);
						for (int i = 0; i < args.Length; i++)
							argsMatch &= IsArgumentTypeMatch (delargs [i + 1].ParameterType, args [i].ParameterType);
					} else {
						// closed over a null reference
						argsMatch = allowClosed;
						for (int i = 0; i < args.Length; i++)
							argsMatch &= IsArgumentTypeMatch (delargs [i].ParameterType, args [i].ParameterType);
					}
				} else {
					if (delargs.Length + 1 == args.Length) {
						// closed over a null reference
						argsMatch = !(args [0].ParameterType.IsValueType || args [0].ParameterType.IsByRef) && allowClosed;
						for (int i = 0; i < delargs.Length; i++)
							argsMatch &= IsArgumentTypeMatch (delargs [i].ParameterType, args [i + 1].ParameterType);

						delegate_data.curried_first_arg = true;
					} else {
						argsMatch = true;
						for (int i = 0; i < args.Length; i++)
							argsMatch &= IsArgumentTypeMatch (delargs [i].ParameterType, args [i].ParameterType);
					}
				}
			}

			return argsMatch;
		}

		static bool IsReturnTypeMatch (Type delReturnType, Type returnType)
		{
			bool returnMatch = returnType == delReturnType;

			if (!returnMatch) {
				// Delegate covariance
				if (!returnType.IsValueType && delReturnType.IsAssignableFrom (returnType))
					returnMatch = true;
			}

			return returnMatch;
		}

		static bool IsArgumentTypeMatch (Type delArgType, Type argType)
		{
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
				else if (argType.IsEnum && Enum.GetUnderlyingType (argType) == delArgType)
					match = true;
			}

			return match;
		}

		static bool IsArgumentTypeMatchWithThis (Type delArgType, Type argType, bool boxedThis)
		{
			bool match;
			if (argType.IsValueType)
				match = delArgType.IsByRef && delArgType.GetElementType () == argType ||
						(boxedThis && delArgType == argType);
			else
				match = delArgType == argType || argType.IsAssignableFrom (delArgType);

			return match;
		}

		protected virtual object DynamicInvokeImpl (object[] args)
		{
			if (Method == null) {
				Type[] mtypes = new Type [args.Length];
				for (int i = 0; i < args.Length; ++i) {
					mtypes [i] = args [i].GetType ();
				}
				method_info = _target.GetType ().GetMethod (data.method_name, mtypes);
			}

			var target = _target;
			if (this.data == null)
				InitializeDelegateData ();

			if (Method.IsStatic) {
				//
				// The delegate is bound to _target
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
				if (_target == null && args != null && args.Length > 0) {
					target = args [0];
					Array.Copy (args, 1, args, 0, args.Length - 1);
					Array.Resize (ref args, args.Length - 1);
				}
			}

			return Method.Invoke (target, args);
		}

		public override bool Equals (object obj)
		{
			Delegate d = obj as Delegate;

			if (d == null || GetType () != obj.GetType ())
				return false;

			// Do not compare method_ptr, since it can point to a trampoline
			if (d._target == _target && d.Method == Method) {
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
			MethodInfo m = Method;

			return (m != null ? m.GetHashCode () : GetType ().GetHashCode ()) ^ RuntimeHelpers.GetHashCode (_target);
		}

		protected virtual MethodInfo GetMethodImpl ()
		{
			if (method_info != null)
				return method_info;

			if (method != IntPtr.Zero) {
				if (!method_is_virtual)
					method_info = (MethodInfo) RuntimeMethodInfo.GetMethodFromHandleNoGenericCheck (new RuntimeMethodHandle (method));
				else
					method_info = GetVirtualMethod_internal ();
			}

			return method_info;
		}

		void InitializeDelegateData ()
		{
			DelegateData delegate_data = new DelegateData ();
			if (method_info.IsStatic) {
				if (_target != null) {
					delegate_data.curried_first_arg = true;
				} else {
					MethodInfo invoke = GetType ().GetMethod ("Invoke");
					if (invoke.GetParametersCount () + 1 == method_info.GetParametersCount ())
						delegate_data.curried_first_arg = true;
				}
			}
			this.data = delegate_data;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private protected extern static MulticastDelegate AllocDelegateLike_internal (Delegate d);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern Delegate CreateDelegate_internal (Type type, object target, MethodInfo info, bool throwOnBindFailure);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern MethodInfo GetVirtualMethod_internal ();
	}
}
