//
// System.Web.HttpCacheability.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web {
	[Serializable]
	public enum HttpCacheability {
		NoCache = 0x1,
		Private,
		Server,
		Public,
	}
}
