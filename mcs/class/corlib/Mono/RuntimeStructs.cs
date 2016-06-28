using System;

namespace Mono {
	//Maps to metadata-internals.h:: MonoAssemblyName
	internal unsafe struct MonoAssemblyName
	{
		const int MONO_PUBLIC_KEY_TOKEN_LENGTH = 17;

		internal IntPtr name;
		internal IntPtr culture;
		internal IntPtr hash_value;
		internal IntPtr public_key;
		internal fixed byte public_key_token [MONO_PUBLIC_KEY_TOKEN_LENGTH];
		internal uint hash_alg;
		internal uint hash_len;
		internal uint flags;
		internal ushort major, minor, build, revision;
		internal ushort arch;
	}
}
