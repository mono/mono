//
// System.Web.UI.IValidator.cs
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
        public interface IValidator
        {
                void Validate();
                string ErrorMessage {get; set;}
                bool IsValid {get; set;}
        }
}
