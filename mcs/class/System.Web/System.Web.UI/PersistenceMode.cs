//
// System.Web.UI.PersistenceMode.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Web;

namespace System.Web.UI
{
        public enum PersistenceMode
        {
                Attribute = 0,
		InnerProperty = 1,
		InnerDefaultProperty = 2,
                EncodedInnerDefaultProperty = 3,
        }
}
