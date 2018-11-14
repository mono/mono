//
// Type.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using StackCrawlMark = System.Threading.StackCrawlMark;

namespace System
{
	[Serializable]
	partial class Type : MemberInfo
	{
		internal RuntimeTypeHandle _impl;

		#region Requires stack backtracing fixes in unmanaged type_from_name

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern Type internal_from_name (string name, bool throwOnError, bool ignoreCase);

		public static Type GetType(string typeName)
		{
			return GetType (typeName, false, false);
		}

		public static Type GetType(string typeName, bool throwOnError)
		{
			return GetType (typeName, throwOnError, false);
		}

		public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
		{
			if (typeName == null)
				throw new ArgumentNullException ("TypeName");

			if (typeName == String.Empty)
				if (throwOnError)
					throw new TypeLoadException ("A null or zero length string does not represent a valid Type.");
				else
					return null;
			Type t = internal_from_name (typeName, throwOnError, ignoreCase);
			if (throwOnError && t == null)
				throw new TypeLoadException ("Error loading '" + typeName + "'");

			return t;
		}

		#endregion

		// TODO: Merge with internal_from_name
		public static Type ReflectionOnlyGetType (string typeName, 
							  bool throwIfNotFound, 
							  bool ignoreCase)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");
			if (typeName == String.Empty && throwIfNotFound)
				throw new TypeLoadException ("A null or zero length string does not represent a valid Type");
			int idx = typeName.IndexOf (',');
			if (idx < 0 || idx == 0 || idx == typeName.Length - 1)
				throw new ArgumentException ("Assembly qualifed type name is required", "typeName");
			string an = typeName.Substring (idx + 1);
			Assembly a;
			try {
				a = Assembly.ReflectionOnlyLoad (an);
			} catch {
				if (throwIfNotFound)
					throw;
				return null;
			}
			return a.GetType (typeName.Substring (0, idx), throwIfNotFound, ignoreCase);
		}

		internal virtual Type InternalResolve ()
		{
			return UnderlyingSystemType;
		}

		// Called from the runtime to return the corresponding finished Type object
		internal virtual Type RuntimeResolve ()
		{
			throw new NotImplementedException ();
		}

		internal virtual bool IsUserType {
			get {
				return true;
			}
		}

		internal virtual MethodInfo GetMethod (MethodInfo fromNoninstanciated)
		{
			throw new System.InvalidOperationException ("can only be called in generic type");
		}

		internal virtual ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
		{
			throw new System.InvalidOperationException ("can only be called in generic type");
		}

		internal virtual FieldInfo GetField (FieldInfo fromNoninstanciated)
		{
			throw new System.InvalidOperationException ("can only be called in generic type");
		}

		public static Type GetTypeFromHandle (RuntimeTypeHandle handle)
		{
			if (handle.Value == IntPtr.Zero)
				// This is not consistent with the other GetXXXFromHandle methods, but
				// MS.NET seems to do this
				return null;

			return internal_from_handle (handle.Value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern Type internal_from_handle (IntPtr handle);

		internal virtual RuntimeTypeHandle GetTypeHandleInternal () => TypeHandle;

#if FEATURE_COMINTEROP || MONO_COM
		virtual internal bool IsWindowsRuntimeObjectImpl () => throw new NotImplementedException ();
		virtual internal bool IsExportedToWindowsRuntimeImpl () => throw new NotImplementedException ();
		internal bool IsWindowsRuntimeObject => IsWindowsRuntimeObjectImpl ();
		internal bool IsExportedToWindowsRuntime => IsExportedToWindowsRuntimeImpl ();
#endif // FEATURE_COMINTEROP

		internal virtual bool HasProxyAttributeImpl () => false;

		internal virtual bool IsSzArray => false;

        // This is only ever called on RuntimeType objects.
        internal string FormatTypeName () => FormatTypeName (false);

		internal virtual string FormatTypeName (bool serialization) => throw new NotImplementedException();

		public bool IsInterface {
			get {
				RuntimeType rt = this as RuntimeType;
				if (rt != null)
					return RuntimeTypeHandle.IsInterface (rt);
				return ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface);
			}
		}

		// Methods containing StackCrawlMark local var has to be marked non-inlineable
		[MethodImplAttribute (MethodImplOptions.NoInlining)] 
		public static Type GetType (
			string typeName,
			Func<AssemblyName, Assembly> assemblyResolver,
			Func<Assembly, string, bool, Type> typeResolver)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return TypeNameParser.GetType (typeName, assemblyResolver, typeResolver, false, false, ref stackMark);
		}

		// Methods containing StackCrawlMark local var has to be marked non-inlineable
		[MethodImplAttribute (MethodImplOptions.NoInlining)] 
		public static Type GetType (
			string typeName,
			Func<AssemblyName, Assembly> assemblyResolver,
			Func<Assembly, string, bool, Type> typeResolver,
			bool throwOnError)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return TypeNameParser.GetType (typeName, assemblyResolver, typeResolver, throwOnError, false, ref stackMark);
		}

		// Methods containing StackCrawlMark local var has to be marked non-inlineable
		[MethodImplAttribute (MethodImplOptions.NoInlining)] 
		public static Type GetType(
			string typeName,
			Func<AssemblyName, Assembly> assemblyResolver,
			Func<Assembly, string, bool, Type> typeResolver,
			bool throwOnError,
			bool ignoreCase)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return TypeNameParser.GetType (typeName, assemblyResolver, typeResolver, throwOnError, ignoreCase, ref stackMark);
		}

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
