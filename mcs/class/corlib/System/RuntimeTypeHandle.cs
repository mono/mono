//
// System.RuntimeTypeHandle.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[MonoTODO ("Serialization needs tests")]
	[Serializable]
	public struct RuntimeTypeHandle : ISerializable
	{
		IntPtr value;

		internal RuntimeTypeHandle (IntPtr val)
		{
			value = val;
		}

		RuntimeTypeHandle (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			MonoType mt = ((MonoType) info.GetValue ("TypeObj", typeof (MonoType)));
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

			info.AddValue ("TypeObj", Type.GetTypeHandle (this), typeof (MonoType));
		}
	}
}
