//
// System.ModuleHandle.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
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

using System.Runtime.ConstrainedExecution;

namespace System
{
	[ComVisible (true)]
	public struct ModuleHandle
	{
		IntPtr value;

		public static readonly ModuleHandle EmptyHandle = new ModuleHandle (IntPtr.Zero);

		internal ModuleHandle (IntPtr v)
		{
			value = v;
		}

		internal IntPtr Value {
			get {
				return value;
			}
		}

		public int MDStreamVersion { 
			get {
				if (value == IntPtr.Zero)
					throw new ArgumentNullException (String.Empty, "Invalid handle");
				return RuntimeModule.GetMDStreamVersion (value);
			}
		}

		internal void GetPEKind (out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			if (value == IntPtr.Zero)
				throw new ArgumentNullException (String.Empty, "Invalid handle");
			RuntimeModule.GetPEKind (value, out peKind, out machine);
		}

		public RuntimeFieldHandle ResolveFieldHandle (int fieldToken)
		{
			return ResolveFieldHandle (fieldToken, null, null);
		}

		public RuntimeMethodHandle ResolveMethodHandle (int methodToken)
		{
			return ResolveMethodHandle (methodToken, null, null);
		}

		public RuntimeTypeHandle ResolveTypeHandle (int typeToken)
		{
			return ResolveTypeHandle (typeToken, null, null);
		}

		private IntPtr[] ptrs_from_handles (RuntimeTypeHandle[] handles) {
			if (handles == null)
				return null;
			else {
				IntPtr[] res = new IntPtr [handles.Length];
				for (int i = 0; i < handles.Length; ++i)
					res [i] = handles [i].Value;
				return res;
			}
		}
				
		public RuntimeTypeHandle ResolveTypeHandle (int typeToken,
													RuntimeTypeHandle[] typeInstantiationContext,
													RuntimeTypeHandle[] methodInstantiationContext) {
			ResolveTokenError error;
			if (value == IntPtr.Zero)
				throw new ArgumentNullException (String.Empty, "Invalid handle");
			IntPtr res = RuntimeModule.ResolveTypeToken (value, typeToken, ptrs_from_handles (typeInstantiationContext), ptrs_from_handles (methodInstantiationContext), out error);
			if (res == IntPtr.Zero)
				throw new TypeLoadException (String.Format ("Could not load type '0x{0:x}' from assembly '0x{1:x}'", typeToken, value.ToInt64 ()));
			else
				return new RuntimeTypeHandle (res);
		}			

		public RuntimeMethodHandle ResolveMethodHandle (int methodToken,
														RuntimeTypeHandle[] typeInstantiationContext,
														RuntimeTypeHandle[] methodInstantiationContext) {
			ResolveTokenError error;
			if (value == IntPtr.Zero)
				throw new ArgumentNullException (String.Empty, "Invalid handle");
			IntPtr res = RuntimeModule.ResolveMethodToken (value, methodToken, ptrs_from_handles (typeInstantiationContext), ptrs_from_handles (methodInstantiationContext), out error);
			if (res == IntPtr.Zero)
				throw new Exception (String.Format ("Could not load method '0x{0:x}' from assembly '0x{1:x}'", methodToken, value.ToInt64 ()));
			else
				return new RuntimeMethodHandle (res);
		}			

		public RuntimeFieldHandle ResolveFieldHandle (int fieldToken,
													  RuntimeTypeHandle[] typeInstantiationContext,
													  RuntimeTypeHandle[] methodInstantiationContext) {
			ResolveTokenError error;
			if (value == IntPtr.Zero)
				throw new ArgumentNullException (String.Empty, "Invalid handle");
			IntPtr res = RuntimeModule.ResolveFieldToken (value, fieldToken, ptrs_from_handles (typeInstantiationContext), ptrs_from_handles (methodInstantiationContext), out error);
			if (res == IntPtr.Zero)
				throw new Exception (String.Format ("Could not load field '0x{0:x}' from assembly '0x{1:x}'", fieldToken, value.ToInt64 ()));
			else
				return new RuntimeFieldHandle (res);
		}			

		public RuntimeFieldHandle GetRuntimeFieldHandleFromMetadataToken (int fieldToken) {
			return ResolveFieldHandle (fieldToken);
		}

		public RuntimeMethodHandle GetRuntimeMethodHandleFromMetadataToken (int methodToken)
		{
			return ResolveMethodHandle (methodToken);
		}

		public RuntimeTypeHandle GetRuntimeTypeHandleFromMetadataToken (int typeToken)
		{
			return ResolveTypeHandle (typeToken);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((ModuleHandle)obj).Value;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals (ModuleHandle handle)
		{
			return value == handle.Value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static bool operator == (ModuleHandle left, ModuleHandle right)
		{
			return Equals (left, right);
		}

		public static bool operator != (ModuleHandle left, ModuleHandle right)
		{
			return !Equals (left, right);
		}
	}
}

