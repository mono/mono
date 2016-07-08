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
	}

}
	
