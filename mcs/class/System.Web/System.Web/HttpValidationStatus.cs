//
// System.Web.HttpValidationStatus.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web {
	[Serializable]
        public enum HttpValidationStatus {
                Invalid = 0x1,
                IgnoreThisRequest,
                Valid
        }
}
