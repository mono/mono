//
// Marshaling routines for the Mono runtime
//
// Authors:
//   Aleksey Kliger <aleksey@xamarin.com>
//   Rodrigo Kumpera <kumpera@xamarin.com>
//
// Copyright 2016 Dot net foundation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono {
	internal sealed class RuntimeMarshaling {

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private unsafe extern static RuntimeTypeHandle mono_class_get_type (RuntimeStructs.MonoClass* klass);

		internal unsafe static RuntimeTypeHandle RuntimeTypeHandleFromClass (RuntimeStructs.MonoClass* klass)
		{
			return mono_class_get_type (klass);
		}
	}
}
