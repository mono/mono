//
// System.Web.UI.IParserAccessor.cs
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
        public interface IParserAccessor
        {
                void AddParsedSubObject(object obj);
        }
}
