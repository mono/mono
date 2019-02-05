#if FULL_AOT_RUNTIME

using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	public class DynamicILInfo {

		DynamicILInfo ()
		{
		}

		public DynamicMethod DynamicMethod { 
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public int GetTokenFor (byte[] signature)
		{
			throw new PlatformNotSupportedException ();
		}

		public int GetTokenFor (DynamicMethod method)
		{
			throw new PlatformNotSupportedException ();
		}

		public int GetTokenFor (RuntimeFieldHandle field)
		{
			throw new PlatformNotSupportedException ();
		}

		public int GetTokenFor (RuntimeMethodHandle method)
		{
			throw new PlatformNotSupportedException ();
		}

		public int GetTokenFor (RuntimeTypeHandle type)
		{
			throw new PlatformNotSupportedException ();
		}

		public int GetTokenFor (string literal)
		{
			throw new PlatformNotSupportedException ();
		}

		public int GetTokenFor (RuntimeMethodHandle method, RuntimeTypeHandle contextType)
		{
			throw new PlatformNotSupportedException ();
		}

		public int GetTokenFor (RuntimeFieldHandle field, RuntimeTypeHandle contextType)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetCode (byte[] code, int maxStackSize)
		{
			throw new PlatformNotSupportedException ();
		}

		public unsafe void SetCode (byte* code, int codeSize, int maxStackSize)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetExceptions (byte[] exceptions)
		{
			throw new PlatformNotSupportedException ();
		}

		public unsafe void SetExceptions (byte* exceptions, int exceptionsSize)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetLocalSignature (byte[] localSignature)
		{
			throw new PlatformNotSupportedException ();
		}

		public unsafe void SetLocalSignature (byte* localSignature, int signatureSize)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
