//
// System.RuntimeFieldHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;
using System.Runtime.Serialization;

namespace System
{
	[MonoTODO ("Serialization needs tests")]
	[Serializable]
	public struct RuntimeFieldHandle : ISerializable
	{
		IntPtr value;

		RuntimeFieldHandle (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			MonoField mf = ((MonoField) info.GetValue ("FieldObj", typeof (MonoField)));
			value = mf.FieldHandle.Value;
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

			info.AddValue ("FieldObj", (MonoField) FieldInfo.GetFieldFromHandle (this), typeof (MonoField));
		}
	}
}
