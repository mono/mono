using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
	partial class Type
	{
		#region keep in sync with object-internals.h
		internal RuntimeTypeHandle _impl;
		#endregion

		internal bool IsRuntimeImplemented () => this.UnderlyingSystemType is RuntimeType;

		internal string FullNameOrDefault {
			get {
				return FullName;
			}
		}

		internal string NameOrDefault {
			get {
				return Name;
			}
		}

		public bool IsInterface {
			get {
				if (this is RuntimeType rt)
					return RuntimeTypeHandle.IsInterface (rt);

				return (GetAttributeFlagsImpl () & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
			}
		}

		public static Type GetType (string typeName, bool throwOnError, bool ignoreCase)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.GetType (typeName, throwOnError, ignoreCase, false, ref stackMark);
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Type GetType (string typeName, bool throwOnError)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.GetType (typeName, throwOnError, false, false, ref stackMark);
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Type GetType (string typeName)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.GetType (typeName, false, false, false, ref stackMark);
		}

		public static Type GetType (string typeName, Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver)
		{
			throw new NotImplementedException ();
		}

		public static Type GetType (string typeName, Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public static Type GetType (string typeName, Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver, bool throwOnError, bool ignoreCase)
		{
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromHandle (RuntimeTypeHandle handle)
		{
			if (handle.Value == IntPtr.Zero)
				return null;

			return internal_from_handle (handle.Value);
		}

		public static Type GetTypeFromCLSID (Guid clsid, string server, bool throwOnError) => throw new PlatformNotSupportedException ();

		public static Type GetTypeFromProgID (string progID, string server, bool throwOnError) => throw new PlatformNotSupportedException ();

		internal string FormatTypeName ()
		{
			return FormatTypeName (false);
		}

		internal virtual string FormatTypeName (bool serialization)
		{
			throw new NotImplementedException ();
		}

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
