//
// System.Web.UI.PersistanceMode.cs
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
                Attribute,
                EncodedInnerDefaultProperty,
                InnerDefaultProperty,
                InnerProperty
        }
}
