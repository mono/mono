//
// Safe handle class for Mono.RuntimeGPtrArrayHandle
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

namespace Mono {
	internal sealed class SafeGPtrArrayHandle : IDisposable {
		RuntimeGPtrArrayHandle handle;
		bool freeSeg;

		internal SafeGPtrArrayHandle (IntPtr ptr, bool freeSeg)
		{
			handle = new RuntimeGPtrArrayHandle (ptr);
			this.freeSeg = freeSeg;
		}

		~SafeGPtrArrayHandle ()
		{
			Dispose (false);
		}

		void Dispose (bool disposing)
		{
			RuntimeGPtrArrayHandle.DestroyAndFree (ref handle, freeSeg);
		}

		public void Dispose () {
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		internal int Length {
			get {
				return handle.Length;
			}
		}

		internal IntPtr this[int i] => handle[i];
	}


}
