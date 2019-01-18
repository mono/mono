namespace System
{
	public partial struct RuntimeFieldHandle : System.Runtime.Serialization.ISerializable
	{
		private object _dummy;
		public System.IntPtr Value { get { throw null; } }
		public override bool Equals(object obj) { throw null; }
		public bool Equals(System.RuntimeFieldHandle handle) { throw null; }
		public override int GetHashCode() { throw null; }
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
		public static bool operator ==(System.RuntimeFieldHandle left, System.RuntimeFieldHandle right) { throw null; }
		public static bool operator !=(System.RuntimeFieldHandle left, System.RuntimeFieldHandle right) { throw null; }
	}
}