//
// System.RuntimeFieldHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System
{
	[MonoTODO]
	[Serializable]
	public struct RuntimeFieldHandle : ISerializable
	{
		IntPtr value;
		
		public IntPtr Value {
			get {
				return (IntPtr) value;
			}
		}

		RuntimeFieldHandle (SerializationInfo info, StreamingContext context)
		{
			Type t;
			
			if (info == null)
				throw new ArgumentNullException ("info");

			t = (Type) info.GetValue ("TypeObj", typeof (Type));

			value = t.TypeHandle.Value;
			if (value == IntPtr.Zero)
				throw new SerializationException (Locale.GetText ("Insufficient state."));
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("TypeObj", value, value.GetType ());
		}
	}
}
