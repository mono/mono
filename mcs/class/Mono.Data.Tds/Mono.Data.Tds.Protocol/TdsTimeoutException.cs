//
// Mono.Data.TdsClient.TdsTimeoutException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;

namespace Mono.Data.TdsClient.Internal {
        internal class TdsTimeoutException : TdsInternalException
	{
		internal TdsTimeoutException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
			: base (theClass, lineNumber, message, number, procedure, server, source, state)
		{
		}
	}
}
