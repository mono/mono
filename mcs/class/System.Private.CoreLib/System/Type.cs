using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	partial class Type
	{
#region keep in sync with object-internals.h
		internal RuntimeTypeHandle _impl;
#endregion

		public static Type GetTypeFromCLSID (Guid clsid, string server, bool throwOnError) => throw new NotImplementedException ();
		public static Type GetTypeFromProgID (string progID, string server, bool throwOnError) => throw new NotImplementedException ();

		internal bool IsRuntimeImplemented () => this.UnderlyingSystemType is RuntimeType;

		internal string FullNameOrDefault {
			get {
				throw new NotImplementedException ();
			}
		}

		internal string NameOrDefault {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsInterface {
			get {
				RuntimeType rt = this as RuntimeType;
				if (rt != null)
					return RuntimeTypeHandle.IsInterface (rt);
				return ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface);
			}
		}

		public static Type GetType (String typeName, bool throwOnError, bool ignoreCase)
		{
			throw new NotImplementedException ();
		}

		public static Type GetType (String typeName, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public static Type GetType (String typeName)
		{
			throw new NotImplementedException ();
		}

		public static Type GetType (
			string typeName,
			Func<AssemblyName, Assembly> assemblyResolver,
			Func<Assembly, string, bool, Type> typeResolver)
		{
			throw new NotImplementedException ();
		}

		public static Type GetType (
			string typeName,
			Func<AssemblyName, Assembly> assemblyResolver,
			Func<Assembly, string, bool, Type> typeResolver,
			bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public static Type GetType(
			string typeName,
			Func<AssemblyName, Assembly> assemblyResolver,
			Func<Assembly, string, bool, Type> typeResolver,
			bool throwOnError,
			bool ignoreCase)
		{
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromHandle (RuntimeTypeHandle handle)
		{
			if (handle.Value == IntPtr.Zero)
				// This is not consistent with the other GetXXXFromHandle methods, but
				// MS.NET seems to do this
				return null;

			return internal_from_handle (handle.Value);
		}

		internal string FormatTypeName () => throw new NotImplementedException ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern Type internal_from_handle (IntPtr handle);

		public static bool operator == (Type left, Type right)
		{
			return object.ReferenceEquals (left, right);
		}

		public static bool operator != (Type left, Type right)
		{
			return !object.ReferenceEquals (left, right);
		}
	}
}
