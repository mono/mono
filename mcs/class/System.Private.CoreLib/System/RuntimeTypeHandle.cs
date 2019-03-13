namespace System
{
	public partial struct RuntimeTypeHandle : System.Runtime.Serialization.ISerializable
	{
		private object _dummy;
		public System.IntPtr Value { get { throw null; } }
		public override bool Equals(object obj) { throw null; }
		public bool Equals(System.RuntimeTypeHandle handle) { throw null; }
		public override int GetHashCode() { throw null; }
		public System.ModuleHandle GetModuleHandle() { throw null; }
		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
		public static bool operator ==(object left, System.RuntimeTypeHandle right) { throw null; }
		public static bool operator ==(System.RuntimeTypeHandle left, object right) { throw null; }
		public static bool operator !=(object left, System.RuntimeTypeHandle right) { throw null; }
		public static bool operator !=(System.RuntimeTypeHandle left, object right) { throw null; }


		public RuntimeTypeHandle (IntPtr ptr)
		{           
		}

		internal static bool HasReferences (RuntimeType rt) => false;

		internal static bool IsInterface (RuntimeType rt) => false;
	}
}
