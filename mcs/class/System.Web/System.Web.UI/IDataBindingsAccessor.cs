//
// System.Web.UI.IDataBindingsAccessor.cs
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
        public interface IDataBindingsAccessor
        {
                DataBindingCollection DataBindings {get;}
                bool HasDataBindings {get;}
        }
}
