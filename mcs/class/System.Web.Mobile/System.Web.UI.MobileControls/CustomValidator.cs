
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : CustomValidator
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.WebControls;

namespace System.Web.UI.MobileControls
{
	public class CustomValidator : BaseValidator
	{
		private static readonly object ServerValidateEvent = new object();
		private System.Web.UI.WebControls.CustomValidator webCV;

		public CustomValidator()
		{
		}

		protected override bool EvaluateIsValid()
		{
			return base.EvaluateIsValidInternal();
		}

		protected override System.Web.UI.WebControls.BaseValidator CreateWebValidator()
		{
			webCV = new System.Web.UI.WebControls.CustomValidator();
			webCV.ServerValidate += new ServerValidateEventHandler(WebServerValidate);
			return webCV;
		}

		private void WebServerValidate(object sender, ServerValidateEventArgs e)
		{
			e.IsValid = OnServerValidate(e.Value);
		}

		protected bool OnServerValidate(string value)
		{
			ServerValidateEventHandler sveh =
			         (ServerValidateEventHandler)(Events[ServerValidateEvent]);
			if(sveh != null)
			{
				ServerValidateEventArgs e =
				      new ServerValidateEventArgs(value, true);
				sveh(this, e);
				return e.IsValid;
			}
			return false;
		}

		protected override bool ControlPropertiesValid()
		{
			if(ControlToValidate.Length > 0)
				return true;

			return base.ControlPropertiesValid();
		}

		public event ServerValidateEventHandler ServerValidate
		{
			add
			{
				Events.AddHandler(ServerValidateEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ServerValidateEvent, value);
			}
		}
	}
}
