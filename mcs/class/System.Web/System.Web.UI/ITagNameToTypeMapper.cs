//
// System.Web.UI.ITagNameToTypeMapper.cs
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
        public interface ITagNameToTypeMapper
        {
                Type GetControlType(string tagName, IDictionary attribs);
        }
}
