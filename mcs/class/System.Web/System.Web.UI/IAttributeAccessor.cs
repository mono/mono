//
// System.Web.UI.IAttributeAccessor.cs
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
        public interface IAttributeAccessor
        {
                string GetAttribute(string key);
                void SetAttribute(string key, string value);
        }
}
