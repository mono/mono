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
#if !MOBILE
	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Type))]
	[ClassInterface(ClassInterfaceType.None)]
	partial class Type : MemberInfo, _Type
#else
	partial class Type : MemberInfo
#endif
	{
		internal RuntimeTypeHandle _impl;

#if !MOBILE
		void _Type.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _Type.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Type.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Type.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
#endif

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

		[MethodImplAttribute (MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Type GetType (String typeName, bool throwOnError, bool ignoreCase)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.GetType (typeName, throwOnError, ignoreCase, false, ref stackMark);
		}
 
		[MethodImplAttribute (MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Type GetType (String typeName, bool throwOnError)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.GetType (typeName, throwOnError, false, false, ref stackMark);
		}
 
		[MethodImplAttribute (MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Type GetType (String typeName) {
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.GetType (typeName, false, false, false, ref stackMark);
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

#if !NETCORE
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
        public static Type ReflectionOnlyGetType (String typeName, bool throwIfNotFound, bool ignoreCase) 
        {
            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
            return RuntimeType.GetType (typeName, throwIfNotFound, ignoreCase, true /*reflectionOnly*/, ref stackMark);
        }
#endif
	}
}
