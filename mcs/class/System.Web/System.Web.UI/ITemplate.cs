//
// System.Web.UI.ITemplate.cs
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
        public interface ITemplate
        {
                void InstantiateIn(Control container);
        }
}
