//
// Mono.Data.Tds.Protocol.TdsVersionInternal.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) 2002 Daniel Morgan
//

namespace Mono.Data.Tds.Protocol {
	public enum TdsVersion
	{
                tds42 = 42, // used by older Sybase and Microsoft SQL (< 7.0) servers
                tds50 = 50, // used by Sybase
                tds70 = 70, // used by Microsoft SQL server 7.0/2000
                tds80 = 80  // used by Microsoft SQL server 2000
	}
}
