//
// SignatureOptions.cs: Signature options enumeration
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security {

	[Flags]
	[Serializable]
	public enum SignatureOptions {
		IncludeNone = 0,
		IncludePath = 15,
#if WSE1
		IncludePathAction = 1,
		IncludePathFrom = 2,
		IncludePathId = 4,
		IncludePathTo = 8,
#endif
		IncludeSoapBody = 16,
		IncludeTimestamp = 96,
		IncludeTimestampCreated = 32,
		IncludeTimestampExpires = 64,
	}
}
