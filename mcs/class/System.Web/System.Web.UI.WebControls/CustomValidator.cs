//
// System.Web.UI.WebControls.CustomValidator.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("ServerValidate")]
	[ToolboxData("<{0}:CustomValidator runat=\"server\""
	             + "ErrorMessage=\"CustomValidator\">"
	             + "</{0}:CustomValidator>")]
	public class CustomValidator : BaseValidator
	{
		private static readonly object ServerValidateEvent = new object();

		public CustomValidator()
		{
		}

		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("A client script that performs the validation.")]
		public string ClientValidationFunction
		{
			get
			{
				object o = ViewState["ClientValidationFunction"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["ClientValidationFunction"] = value;
			}
		}

		[WebSysDescription ("Raised for validation on the server.")]
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

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(RenderUplevel)
			{
				writer.AddAttribute("evaluationfunction", "CustomValidatorEvaluateIsValid");
				if(ClientValidationFunction.Length > 0)
				{
					writer.AddAttribute("clientvalidationfunction", ClientValidationFunction);
				}
			}
		}

		protected override bool ControlPropertiesValid()
		{
			if(ControlToValidate.Length > 0)
			{
				CheckControlValidationProperty(ControlToValidate, "ControlToValidate");
			}
			return true;
		}

		protected virtual bool OnServerValidate(string value)
		{
			if(Events != null)
			{
				ServerValidateEventHandler sveh = (ServerValidateEventHandler)(Events[ServerValidateEvent]);
				if(sveh != null)
				{
					ServerValidateEventArgs args = new ServerValidateEventArgs(value, true);
					sveh(this, args);
					return args.IsValid;
				}
			}
			return true;
		}

		protected override bool EvaluateIsValid()
		{
			string ctrl = ControlToValidate;
			if(ctrl.Length > 0)
			{
				ctrl = GetControlValidationValue(ctrl);
				if(ctrl== null || ctrl.Length == 0)
				{
					return true;
				}
			}
			return OnServerValidate(ctrl);
		}
	}
}
