// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Mono.Profiler.Log {

	public sealed class LogException : Exception {

		public LogException (string message)
			: base (message)
		{
		}
	}
}
