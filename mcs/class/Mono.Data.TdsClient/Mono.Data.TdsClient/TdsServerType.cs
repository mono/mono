//
// Mono.Data.TdsClient.TdsServerType.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

namespace Mono.Data.TdsClient {
        public enum TdsServerType 
	{
		SqlServer, // use TDS version 4.2, 7.0, 8.0
		Sybase     // use TDS version 4,2, 5.0
	}
}
