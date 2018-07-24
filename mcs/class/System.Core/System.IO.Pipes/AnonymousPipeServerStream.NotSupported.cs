// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Security;

namespace System.IO.Pipes
{
	/// <summary>
	/// Anonymous pipe server stream
	/// </summary>
	public sealed partial class AnonymousPipeServerStream : PipeStream
	{
		public AnonymousPipeServerStream (PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity)
			: base (PipeDirection.In, 0)
		{
			throw new PlatformNotSupportedException();
		}

		// Creates the anonymous pipe.
		private unsafe void Create (PipeDirection direction, HandleInheritability inheritability, int bufferSize)
		{
			throw new PlatformNotSupportedException();
		}
	}
}