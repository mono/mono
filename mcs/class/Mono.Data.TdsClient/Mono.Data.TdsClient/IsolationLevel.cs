//
// Mono.Data.TdsClient.TdsIsolationLevel.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//
//
// Copied from System.Data namespace, to avoid dependency purgatory
//

using System;

namespace Mono.Data.TdsClient {
       	[Flags]
       	[Serializable]
       	public enum IsolationLevel
       	{
               	Unspecified = -1,
               	Chaos = 16,
               	ReadUncommitted = 256,
               	ReadCommitted = 4096,
               	RepeatableRead = 65536,
               	Serializable = 1048576
	}
}
