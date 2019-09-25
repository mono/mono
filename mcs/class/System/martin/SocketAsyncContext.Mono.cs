#if MARTIN_USE_MONO_IO
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	internal partial class SocketAsyncContext
	{
		void Register()
		{
			Debug.Assert (_nonBlockingSet);
			lock (_registerLock) {
				if (!_registered) {
					_asyncEngineToken = new SocketAsyncEngine.Token (this, _socket);
					_registered = true;

					Trace ("Registered");
				}
			}
		}
	}
}
#endif
