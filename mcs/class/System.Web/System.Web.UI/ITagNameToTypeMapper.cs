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
using System.Collections;

namespace System.Web.UI
{
	interface ITagNameToTypeMapper
        {
                Type GetControlType(string tagName, IDictionary attribs);
        }
}
