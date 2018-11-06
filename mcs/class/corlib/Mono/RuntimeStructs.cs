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

#pragma warning disable 169

namespace Mono {
	//
	// Managed representations of mono runtime types
	//
	internal static class RuntimeStructs {
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

		// mono-error.h MonoError
		struct MonoError {
			ushort error_code;
			ushort hidden_0;
			IntPtr hidden_1, hidden_2, hidden_3, hidden_4, hidden_5, hidden_6, hidden_7, hidden_8;
			IntPtr hidden_11, hidden_12, hidden_13, hidden_14, hidden_15, hidden_16, hidden_17, hidden_18;
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
