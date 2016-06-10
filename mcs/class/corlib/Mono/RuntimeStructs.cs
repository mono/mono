//
// Mono runtime native structs surfaced to managed code.
//
// Authors:
//   Aleksey Kliger <aleksey@xamarin.com>
//   Rodrigo Kumpera <kumpera@xamarin.com>
//
// Copyright 2016 Dot net foundation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Runtime.InteropServices;

namespace Mono {
	internal class RuntimeStructs {
		// class-internals.h MonoRemoteClass
		[StructLayout(LayoutKind.Sequential)]
		internal unsafe struct RemoteClass {
			internal IntPtr default_vtable;
			internal IntPtr xdomain_vtable;
			internal MonoClass* proxy_class;
			internal IntPtr proxy_class_name;
			internal uint interface_count;
			// FIXME: How to represent variable-length array struct member?
			// MonoClass* interfaces [];
		}

		internal struct MonoClass {
		}

		// class-internals.h MonoGenericParamInfo
		internal unsafe struct GenericParamInfo {
			internal MonoClass* pklass;
			internal IntPtr name;
			internal ushort flags;
			internal uint token;
			internal MonoClass** constraints; /* NULL terminated */
		}

		// glib.h GPtrArray
		internal unsafe struct GPtrArray {
			internal IntPtr* data;
			internal int len;
		}
	}

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
