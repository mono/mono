//
// System.RuntimeMethodHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace System
{
	[MonoTODO ("Serialization needs tests")]
	[Serializable]
	public struct RuntimeMethodHandle : ISerializable
	{
		IntPtr value;

		internal RuntimeMethodHandle (IntPtr v)
		{
			value = v;
		}

		RuntimeMethodHandle (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			MonoMethod mm = ((MonoMethod) info.GetValue ("MethodObj", typeof (MonoMethod)));
			value = mm.MethodHandle.Value;
			if (value == IntPtr.Zero)
				throw new SerializationException (Locale.GetText ("Insufficient state."));
		}

		public IntPtr Value {
			get {
				return value;
			}
		}

		// This is from ISerializable
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("MethodObj", (MonoMethod) MethodBase.GetMethodFromHandle (this), typeof (MonoMethod));
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern IntPtr GetFunctionPointer (IntPtr m);

		public IntPtr GetFunctionPointer ()
		{
			return GetFunctionPointer (value);
		}
	}
}
