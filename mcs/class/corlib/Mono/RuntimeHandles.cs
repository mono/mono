//
// Wrapper handles for Mono Runtime internal structs
//
// Authors:
//   Aleksey Kliger <aleksey@xamarin.com>
//   Rodrigo Kumpera <kumpera@xamarin.com>
//
// Copyright 2016 Dot net foundation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mono
{
	unsafe struct RuntimeClassHandle
	{
		RuntimeStructs.MonoClass* value;

		internal RuntimeClassHandle (RuntimeStructs.MonoClass* value)
		{
			this.value = value;
		}

		internal RuntimeClassHandle (IntPtr ptr)
		{
			this.value = (RuntimeStructs.MonoClass*) ptr;
		}

		internal RuntimeStructs.MonoClass* Value => value;

		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((RuntimeClassHandle)obj).Value;
		}

		public override int GetHashCode () => ((IntPtr)value).GetHashCode ();

		public bool Equals (RuntimeClassHandle handle)
		{
			return value == handle.Value;
		}

		public static bool operator == (RuntimeClassHandle left, object right)
		{
			return right != null && right is RuntimeClassHandle rch && left.Equals (rch);
		}

		public static bool operator != (RuntimeClassHandle left, object right)
		{
			return !(left == right);
		}

		public static bool operator == (object left, RuntimeClassHandle right)
		{
			return left != null && left is RuntimeClassHandle rch && rch.Equals (right);
		}

		public static bool operator != (object left, RuntimeClassHandle right)
		{
			return !(left == right);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe extern static IntPtr GetTypeFromClass (RuntimeStructs.MonoClass *klass);

		internal RuntimeTypeHandle GetTypeHandle () => new RuntimeTypeHandle (GetTypeFromClass (value));
	}

	unsafe struct RuntimeRemoteClassHandle
	{
		RuntimeStructs.RemoteClass* value;

		internal RuntimeRemoteClassHandle (RuntimeStructs.RemoteClass* value)
		{
			this.value = value;
		}

		internal RuntimeClassHandle ProxyClass {
			get {
				return new RuntimeClassHandle (value->proxy_class);
			}
		}
	}

	unsafe struct RuntimeGenericParamInfoHandle
	{
		RuntimeStructs.GenericParamInfo* value;

		internal RuntimeGenericParamInfoHandle (RuntimeStructs.GenericParamInfo* value)
		{
			this.value = value;
		}

		internal RuntimeGenericParamInfoHandle (IntPtr ptr)
		{
			this.value = (RuntimeStructs.GenericParamInfo*) ptr;
		}

		internal Type[] Constraints => GetConstraints ();

		internal GenericParameterAttributes Attributes => (GenericParameterAttributes) value->flags;

		Type[] GetConstraints ()
		{
			int n = GetConstraintsCount ();
			var a = new Type [n];
			for (int i = 0; i < n; i++) {
				RuntimeClassHandle c = new RuntimeClassHandle (value->constraints[i]);
				a[i] = Type.GetTypeFromHandle (c.GetTypeHandle ());
			}

			return a;
		}

		int GetConstraintsCount ()
		{
			int i = 0;
			RuntimeStructs.MonoClass** p = value->constraints;
			while (p != null && *p != null)  {
				p++; i++;
			}
			return i;
		}
	}

	internal struct RuntimeEventHandle
	{
		IntPtr value;

		internal RuntimeEventHandle (IntPtr v)
		{
			value = v;
		}

		public IntPtr Value => value;

		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((RuntimeEventHandle)obj).Value;
		}

		public bool Equals (RuntimeEventHandle handle)
		{
			return value == handle.Value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static bool operator == (RuntimeEventHandle left, RuntimeEventHandle right)
		{
			return left.Equals (right);
		}

		public static bool operator != (RuntimeEventHandle left, RuntimeEventHandle right)
		{
			return !left.Equals (right);
		}
	}

	internal struct RuntimePropertyHandle
	{
		IntPtr value;

		internal RuntimePropertyHandle (IntPtr v)
		{
			value = v;
		}

		public IntPtr Value => value;

		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((RuntimePropertyHandle)obj).Value;
		}

		public bool Equals (RuntimePropertyHandle handle)
		{
			return value == handle.Value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static bool operator == (RuntimePropertyHandle left, RuntimePropertyHandle right)
		{
			return left.Equals (right);
		}

		public static bool operator != (RuntimePropertyHandle left, RuntimePropertyHandle right)
		{
			return !left.Equals (right);
		}
	}

	unsafe struct RuntimeGPtrArrayHandle
	{
		RuntimeStructs.GPtrArray* value;

		internal RuntimeGPtrArrayHandle (RuntimeStructs.GPtrArray* value)
		{
			this.value = value;
		}

		internal RuntimeGPtrArrayHandle (IntPtr ptr)
		{
			this.value = (RuntimeStructs.GPtrArray*) ptr;
		}

		internal int Length => value->len;

		internal IntPtr this [int i] => Lookup (i);

		internal IntPtr Lookup (int i)
		{
			if (i >= 0 && i < Length) {
				return value->data[i];
			} else
				throw new IndexOutOfRangeException ();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		extern static void GPtrArrayFree (RuntimeStructs.GPtrArray* value);

		internal static void DestroyAndFree (ref RuntimeGPtrArrayHandle h) {
			GPtrArrayFree (h.value);
			h.value = null;
		}
	}
}
