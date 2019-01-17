namespace System
{
	public partial struct ModuleHandle
	{
		private object _dummy;
		public static readonly System.ModuleHandle EmptyHandle;
		public int MDStreamVersion { get { throw null; } }
		public bool Equals(System.ModuleHandle handle) { throw null; }
		public override bool Equals(object obj) { throw null; }
		public override int GetHashCode() { throw null; }
		public System.RuntimeFieldHandle GetRuntimeFieldHandleFromMetadataToken(int fieldToken) { throw null; }
		public System.RuntimeMethodHandle GetRuntimeMethodHandleFromMetadataToken(int methodToken) { throw null; }
		public System.RuntimeTypeHandle GetRuntimeTypeHandleFromMetadataToken(int typeToken) { throw null; }
		public static bool operator ==(System.ModuleHandle left, System.ModuleHandle right) { throw null; }
		public static bool operator !=(System.ModuleHandle left, System.ModuleHandle right) { throw null; }
		public System.RuntimeFieldHandle ResolveFieldHandle(int fieldToken) { throw null; }
		public System.RuntimeFieldHandle ResolveFieldHandle(int fieldToken, System.RuntimeTypeHandle[] typeInstantiationContext, System.RuntimeTypeHandle[] methodInstantiationContext) { throw null; }
		public System.RuntimeMethodHandle ResolveMethodHandle(int methodToken) { throw null; }
		public System.RuntimeMethodHandle ResolveMethodHandle(int methodToken, System.RuntimeTypeHandle[] typeInstantiationContext, System.RuntimeTypeHandle[] methodInstantiationContext) { throw null; }
		public System.RuntimeTypeHandle ResolveTypeHandle(int typeToken) { throw null; }
		public System.RuntimeTypeHandle ResolveTypeHandle(int typeToken, System.RuntimeTypeHandle[] typeInstantiationContext, System.RuntimeTypeHandle[] methodInstantiationContext) { throw null; }

		public IntPtr Value => throw new NotImplementedException ();
	}
}