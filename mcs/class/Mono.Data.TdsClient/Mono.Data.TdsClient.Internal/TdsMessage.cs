//
// Mono.Data.TdsClient.Internal.TdsMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsMessage {
		int number;
		int state;
		int severity;
		string message;
		string server;
		string procName;
		int line;

		[System.MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
