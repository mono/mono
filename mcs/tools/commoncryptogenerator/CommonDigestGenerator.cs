//
// CommonCrypto code generator for digest algorithms
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2014 Xamarin Inc.
//

using System;
using System.IO;

namespace Xamarin {

	public static class CommonDigest {
		
#if !MONOTOUCH && !XAMMAC
		// we do not add anything in MonoTouch, just replacing, so this is not needed
		// however we can avoid a dependency on Mono.Security for Crimson.CommonCrypto.dll by including the base classes
		static public void GenerateBaseClass (string namespaceName, string typeName, string baseTypeName, int hashSize,
			string visibilityStart = "", string visibilityEnd = "")
		{
			string template = @"// Generated file to bind CommonCrypto/CommonDigest - DO NOT EDIT
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2014 Xamarin Inc.

using System;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace %NAMESPACE% {

%VISIBILITY_START%
	public
%VISIBILITY_END%
	abstract class %BASE% : HashAlgorithm {

		protected %BASE% () 
		{
			HashSizeValue = %HASHSIZE%; 
		}

		public static new %BASE% Create ()
		{
			return Create (""%BASE%"");
		}

		public static new %BASE% Create (string hashName)
		{
			object o = CryptoConfig.CreateFromName (hashName);
			return (%BASE%) o ?? new %TYPE% ();
		}
	}
}";
			
			File.WriteAllText (baseTypeName + ".g.cs", template.Replace ("%NAMESPACE%", namespaceName).
				Replace ("%TYPE%", typeName).Replace ("%BASE%", baseTypeName).
				Replace ("%VISIBILITY_START%", visibilityStart).Replace ("%VISIBILITY_END%", visibilityEnd).
				Replace ("%HASHSIZE%", hashSize.ToString ()));
		}
#endif
		static public void Generate (string namespaceName, string typeName, string baseTypeName, int contextSize,
			string visibilityStart = "", string visibilityEnd = "")
		{
			string template = @"// Generated file to bind CommonCrypto/CommonDigest - DO NOT EDIT
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2011-2014 Xamarin Inc.

using System;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using Mono.Security.Cryptography;

namespace %NAMESPACE% {

%VISIBILITY_START%
	public
%VISIBILITY_END%
	sealed class %TYPE% : %BASE% {

		IntPtr ctx;

		[DllImport (""/usr/lib/libSystem.dylib"", EntryPoint=""CC_%BASE%_Init"")]
		static extern int Init (/* CC_%BASE%_CTX */ IntPtr c);

		[DllImport (""/usr/lib/libSystem.dylib"", EntryPoint=""CC_%BASE%_Update"")]
		static extern int Update (/* CC_%BASE%_CTX */ IntPtr c, /* const void * */ IntPtr data, /* uint32_t */ uint len);

		[DllImport (""/usr/lib/libSystem.dylib"", EntryPoint=""CC_%BASE%_Final"")]
		static extern int Final (/* unsigned char * */ byte [] md, /* CC_%BASE%_CTX */ IntPtr c);

		public %TYPE% ()
		{
			ctx = IntPtr.Zero;
		}

		~%TYPE% ()
		{
			Dispose (false);
		}

		protected override void Dispose (bool disposing)
		{
			if (ctx != IntPtr.Zero) {
				Marshal.FreeHGlobal (ctx);
				ctx = IntPtr.Zero;
			}
			base.Dispose (disposing);
			GC.SuppressFinalize (this);
		}

		public override void Initialize ()
		{
			if (ctx == IntPtr.Zero)
				ctx = Marshal.AllocHGlobal (%CTX_SIZE%);
			
			int hr = Init (ctx);
			if (hr != 1)
				throw new CryptographicException (hr);
		}

		protected override void HashCore (byte[] data, int start, int length) 
		{
			if (ctx == IntPtr.Zero)
				Initialize ();

			if (data.Length == 0)
				return;

			unsafe {
				fixed (byte* p = &data [0]) {
					int hr = Update (ctx, (IntPtr) (p + start), (uint) length);
					if (hr != 1)
						throw new CryptographicException (hr);
				}
			}
		}

		protected override byte[] HashFinal () 
		{
			if (ctx == IntPtr.Zero)
				Initialize ();
			
			byte[] data = new byte [HashSize >> 3];
			int hr = Final (data, ctx);
			if (hr != 1)
				throw new CryptographicException (hr);

			return data;
		}
	}
}";
			
			File.WriteAllText (typeName + ".g.cs", template.Replace ("%NAMESPACE%", namespaceName).
				Replace ("%TYPE%", typeName).Replace ("%BASE%", baseTypeName).
				Replace ("%VISIBILITY_START%", visibilityStart).Replace ("%VISIBILITY_END%", visibilityEnd).
				Replace ("%CTX_SIZE%", contextSize.ToString ()));
		}
	}
}
