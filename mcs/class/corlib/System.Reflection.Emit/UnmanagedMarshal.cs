
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System;

namespace System.Reflection.Emit {

	public sealed class UnmanagedMarshal {
		private int count;
		private UnmanagedType t;
		
		public UnmanagedType BaseType {
			get {return t;}
		}

		public int ElementCount {
			get {return count;}
		}

		public UnmanagedType GetUnmanagedType {
			get {return t;}
		}

		public Guid IIDGuid {
			get {return Guid.Empty;}
		}

		[MonoTODO]
		public static UnmanagedMarshal DefineByValArray( int elemCount) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static UnmanagedMarshal DefineByValTStr( int elemCount) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static UnmanagedMarshal DefineLPArray( UnmanagedType elemType) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static UnmanagedMarshal DefineSafeArray( UnmanagedType elemType) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static UnmanagedMarshal DefineUnmanagedMarshal( UnmanagedType unmanagedType) {
			throw new NotImplementedException ();
		}
		
	}
}
