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
