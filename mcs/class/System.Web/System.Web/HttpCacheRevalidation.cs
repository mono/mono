//
// System.Web.HttpCacheRevalidation.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web {
	[Serializable]
	public enum HttpCacheRevalidation {
		AllCaches = 0x1,
		ProxyCaches,
		None,
	}
}
