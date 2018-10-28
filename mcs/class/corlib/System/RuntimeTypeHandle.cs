//
// System.RuntimeTypeHandle.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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

using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace System
{
	[ComVisible (true)]
	[Serializable]
	public struct RuntimeTypeHandle : ISerializable
	{
		IntPtr value;

		internal RuntimeTypeHandle (IntPtr val)
		{
			value = val;
		}

		internal RuntimeTypeHandle (RuntimeType type)
			: this (type._impl.value)
		{
		}

		RuntimeTypeHandle (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			RuntimeType mt = ((RuntimeType) info.GetValue ("TypeObj", typeof (RuntimeType)));
			value = mt.TypeHandle.Value;
			if (value == IntPtr.Zero)
				throw new SerializationException (Locale.GetText ("Insufficient state."));
		}

		public IntPtr Value {
			get {
				return value;
			}
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			if (value == IntPtr.Zero)
				throw new SerializationException ("Object fields may not be properly initialized");

			info.AddValue ("TypeObj", Type.GetTypeHandle (this), typeof (RuntimeType));
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((RuntimeTypeHandle)obj).Value;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals (RuntimeTypeHandle handle)
		{
			return value == handle.Value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static bool operator == (RuntimeTypeHandle left, Object right)
		{
			return (right != null) && (right is RuntimeTypeHandle) && left.Equals ((RuntimeTypeHandle)right);
		}

		public static bool operator != (RuntimeTypeHandle left, Object right)
		{
			return (right == null) || !(right is RuntimeTypeHandle) || !left.Equals ((RuntimeTypeHandle)right);
		}

		public static bool operator == (Object left, RuntimeTypeHandle right)
		{
			return (left != null) && (left is RuntimeTypeHandle) && ((RuntimeTypeHandle)left).Equals (right);
		}

		public static bool operator != (Object left, RuntimeTypeHandle right)
		{
			return (left == null) || !(left is RuntimeTypeHandle) || !((RuntimeTypeHandle)left).Equals (right);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern TypeAttributes GetAttributes (RuntimeType type);

		[CLSCompliant (false)]
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public ModuleHandle GetModuleHandle ()
		{
			// Although MS' runtime is crashing here, we prefer throwing an exception.
			// The check is needed because Type.GetTypeFromHandle returns null
			// for zero handles.
			if (value == IntPtr.Zero)
				throw new InvalidOperationException ("Object fields may not be properly initialized");

			return Type.GetTypeFromHandle (this).Module.ModuleHandle;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int GetMetadataToken (RuntimeType type);

		internal static int GetToken (RuntimeType type)
		{
			return GetMetadataToken (type);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static Type GetGenericTypeDefinition_impl (RuntimeType type);

		internal static Type GetGenericTypeDefinition (RuntimeType type)
		{
			return GetGenericTypeDefinition_impl (type);
		}

		internal static bool HasElementType (RuntimeType type)
		{
			return IsArray (type) || IsByRef (type) || IsPointer (type);
		}

		internal static bool HasProxyAttribute (RuntimeType type)
		{
			throw new NotImplementedException ("HasProxyAttribute");
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool HasInstantiation (RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool IsArray(RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool IsByRef (RuntimeType type);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static bool IsComObject (RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool IsInstanceOfType (RuntimeType type, Object o);		

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool IsPointer (RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool IsPrimitive (RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool HasReferences (RuntimeType type);

		internal static bool IsComObject (RuntimeType type, bool isGenericCOM)
		{
			return isGenericCOM ? false : IsComObject (type);
		}

		internal static bool IsContextful (RuntimeType type)
		{
			return typeof (ContextBoundObject).IsAssignableFrom (type);
		}

		internal static bool IsEquivalentTo (RuntimeType rtType1, RuntimeType rtType2)
		{
			// refence check is done earlier and we don't recognize anything else
			return false;
		}		

		internal static bool IsSzArray(RuntimeType type)
		{
			// TODO: Better check
			return IsArray (type) && type.GetArrayRank () == 1;
		}

		internal static bool IsInterface (RuntimeType type)
		{
			return (type.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static int GetArrayRank(RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static RuntimeAssembly GetAssembly (RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static RuntimeType GetElementType (RuntimeType type);

 		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static RuntimeModule GetModule (RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool IsGenericVariable (RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static RuntimeType GetBaseType (RuntimeType type);

		internal static bool CanCastTo (RuntimeType type, RuntimeType target)
		{
			return type_is_assignable_from (target, type);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool type_is_assignable_from (Type a, Type b);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool IsGenericTypeDefinition (RuntimeType type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static IntPtr GetGenericParameterInfo (RuntimeType type);

		internal static bool IsSubclassOf (RuntimeType childType, RuntimeType baseType)
		{
			return is_subclass_of (childType._impl.Value, baseType._impl.Value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool is_subclass_of (IntPtr childType, IntPtr baseType);

		[PreserveDependency (".ctor()", "System.Runtime.CompilerServices.IsByRefLikeAttribute")]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static bool IsByRefLike (RuntimeType type);
	}
}
