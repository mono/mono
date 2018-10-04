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

namespace System
{
	partial class Type : MemberInfo
	{
		internal RuntimeTypeHandle _impl;

		public virtual bool IsTypeDefinition => throw NotImplemented.ByDesign;
		public virtual bool IsGenericTypeParameter => IsGenericParameter && DeclaringMethod == null;
		public virtual bool IsGenericMethodParameter => IsGenericParameter && DeclaringMethod != null;
		public virtual bool IsByRefLike => throw new NotSupportedException(SR.NotSupported_SubclassOverride);        

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
	}
}
