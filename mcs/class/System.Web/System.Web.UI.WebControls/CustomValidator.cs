/**
 * Namespace: System.Web.UI.WebControls
 * Class:     CustomValidator
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class CustomValidator : BaseValidator
	{
		private static readonly object ServerValidateEvent = new object();

		public CustomValidator()
		{
		}
		
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
		}
		
		protected virtual bool OnServerValidate(string value)
		{
			if(Events != null)
			{
				ServerValidateEventHandler sveh = (ServerValidateEventHandler)(Events[ServerValidateEvent]);
				if(sveh != null)
				{
					ServerValidateEventArgs args = new ServerValidateEventArgs(value, true));
					sveh(this, args);
					return args.IsValid;
				}
			}
			return true;
		}
	}
}
