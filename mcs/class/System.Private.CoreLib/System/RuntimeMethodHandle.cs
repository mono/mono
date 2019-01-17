namespace System
{
	public partial struct RuntimeMethodHandle : System.Runtime.Serialization.ISerializable
	{
		private object _dummy;
		public System.IntPtr Value { get { throw null; } }
		public override bool Equals(object obj) { throw null; }
		public bool Equals(System.RuntimeMethodHandle handle) { throw null; }
		public System.IntPtr GetFunctionPointer() { throw null; }
		public override int GetHashCode() { throw null; }
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
		public static bool operator ==(System.RuntimeMethodHandle left, System.RuntimeMethodHandle right) { throw null; }
		public static bool operator !=(System.RuntimeMethodHandle left, System.RuntimeMethodHandle right) { throw null; }

		internal RuntimeMethodHandle (IntPtr ptr)
		{
		}
	}
}