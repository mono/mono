/**
 * Namespace: System.Web.UI.WebControls
 * Class:     BaseValidator
 *
 * Authors:  Gaurav Vaish, Gonzalo Paniagua Javier
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>, <gonzalo@ximian.com>
 * Implementation: yes
 * Status:  80%
 *
 * (C) Gaurav Vaish (2001)
 * (c) 2002 Ximian, Inc. (http://www.ximian.com)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Drawing;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("ErrorMessage")]
	//TODO: [Designer("??")]
	public abstract class BaseValidator: Label, IValidator
	{
		private bool isValid;
		private bool isPreRenderCalled;
		private bool isPropertiesChecked;
		private bool propertiesValid;
		private bool renderUplevel;

		protected BaseValidator() : base()
		{
			isValid = true;
			ForeColor = Color.Red;
			propertiesValid = true;
			isPropertiesChecked = false;
			renderUplevel = false;
		}

		public string ControlToValidate
		{
			get
			{
				object o = ViewState["ControlToValidate"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["ControlToValidate"] = value;
			}
		}

		public ValidatorDisplay Display
		{
			get
			{
				object o = ViewState["Display"];
				if(o != null)
				{
					return (ValidatorDisplay)o;
				}
				return ValidatorDisplay.Static;
			}
			set
			{
				if(!Enum.IsDefined(typeof(ValidatorDisplay), value))
				{
					throw new ArgumentException();
				}
				ViewState["ValidatorDisplay"] = value;
			}
		}

		public bool EnableClientScript
		{
			get
			{
				object o = ViewState["EnableClientScript"];
				if(o != null)
				{
					return (bool)o;
				}
				return true;
			}
			set
			{
				ViewState["EnableClientScript"] = value;
			}
		}

		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				if (value == false)
					isValid = true;
				base.Enabled = value;
			}
		}

		public string ErrorMessage
		{
			get
			{
				object o = ViewState["ErrorMessage"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["ErrorMessage"] = value;
			}
		}

		public override Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		public bool IsValid
		{
			get { return isValid; }
			set { isValid = value; }
		}

		public static PropertyDescriptor GetValidationProperty(object component)
		{
			System.ComponentModel.AttributeCollection coll = TypeDescriptor.GetAttributes (component);
			Type type = typeof (ValidationPropertyAttribute);
			ValidationPropertyAttribute attrib = (ValidationPropertyAttribute) coll [type];
			if (attrib != null && attrib.Name != null)
				return (TypeDescriptor.GetProperties (component)) [attrib.Name];
			return null;
		}

		public void Validate()
		{
			if(!Visible || (Visible && !Enabled))
			{
				IsValid = true;
			}
			Control ctrl = Parent;
			while(ctrl != null)
			{
				if(!ctrl.Visible)
				{
					IsValid = true;
					return;
				}
				ctrl = ctrl.Parent;
			}
			isPropertiesChecked = false;
			if(!PropertiesValid)
			{
				IsValid = true;
				return;
			}
			IsValid = EvaluateIsValid();
		}

		protected bool PropertiesValid
		{
			get
			{
				if(!isPropertiesChecked)
				{
					propertiesValid = ControlPropertiesValid();
					isPropertiesChecked = true;
				}
				return propertiesValid;
			}
		}

		protected bool RenderUplevel
		{
			get
			{
				return renderUplevel;
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			bool enabled = base.Enabled;
			if (enabled)
				Enabled = true;

			base.AddAttributesToRender(writer);
			if(RenderUplevel)
			{
				if(ID == null)
				{
					writer.AddAttribute("id", ClientID);
				}
				if(ControlToValidate.Length > 0)
				{
					writer.AddAttribute("controltovalidate", GetControlRenderID(ControlToValidate));
				}
				if(ErrorMessage.Length > 0)
				{
					writer.AddAttribute("errormessage", ErrorMessage, true);
				}
				if(Display == ValidatorDisplay.Static)
				{
					writer.AddAttribute("display", Enum.Format(typeof(ValidatorDisplay), Display, "G").Replace('_','-'));
					//writer.AddAttribute("display", PropertyConverter.EnumToString(typeof(ValidatorDisplay), Display));
				}
				if(!IsValid)
				{
					writer.AddAttribute("isvalid", "False");
				}

				ControlStyle.AddAttributesToRender (writer, this);
				if(!enabled)
				{
					writer.AddAttribute("enabled", "False");
				}
			}

			if(enabled)
			{
				Enabled = false;
			}
		}

		protected void CheckControlValidationProperty(string name, string propertyName)
		{
			Control ctrl = NamingContainer.FindControl(name);
			if(ctrl == null)
			{
				throw new HttpException(HttpRuntime.FormatResourceString("Validator_control_not_found",
				                 name, propertyName/*, ID*/));
			}
			PropertyDescriptor pd = GetValidationProperty(ctrl);
			if(pd == null)
			{
				throw new HttpException(HttpRuntime.FormatResourceString("Validator_bad_control_type",
				                 name, propertyName/*, ID*/));
			}
		}

		protected virtual bool ControlPropertiesValid()
		{
			if(ControlToValidate.Length == 0)
			{
				throw new HttpException(HttpRuntime.FormatResourceString("Validator_control_blank", ID));
			}
			CheckControlValidationProperty(ControlToValidate, "ControlToValidate");
			return true;
		}

		[MonoTODO]
		protected virtual bool DetermineRenderUplevel()
		{
			Page page = Page;
			if(page == null || page.Request == null)
			{
				return false;
			}

			if(EnableClientScript)
			{
				// By now, return false
				return false;
				//throw new NotImplementedException();
				////TODO: I need to get the (Browser->Dom_version_major >= 4 &&
				////                         Brower->Ecma_script_version >= 1.2)
			}
			return false;
		}

		protected string GetControlRenderID(string name)
		{
			Control ctrl = FindControl(name);
			if(ctrl != null)
			{
				return ctrl.ClientID;
			}
			return String.Empty;
		}

		protected string GetControlValidationValue(string name)
		{
			Control ctrl = NamingContainer.FindControl(name);
			if(ctrl != null)
			{
				PropertyDescriptor pd = GetValidationProperty(ctrl);
				if(pd != null)
				{
					object item = pd.GetValue (ctrl);
					if (item is ListItem)
						return ((ListItem) item).Value;

					if (item == null)
						return String.Empty;

					return item.ToString ();
				}
			}
			return null;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			Page.Validators.Add(this);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			isPreRenderCalled   = true;
			isPropertiesChecked = false;
			renderUplevel       = DetermineRenderUplevel();
			if(renderUplevel)
			{
				RegisterValidatorCommonScript();
			}
		}

		protected override void OnUnload(EventArgs e)
		{
			if(Page != null)
			{
				Page.Validators.Remove(this);
			}
			base.OnUnload(e);
		}

		[MonoTODO("Damn_This_Is_Really_Frustrating___by_Gaurav")]
		protected void RegisterValidatorCommonScript()
		{
			if(Page.IsClientScriptBlockRegistered("ValidatorIncludeScript"))
				return;
			
			throw new NotImplementedException();
		}

		[MonoTODO("I_have_to_know_javascript_for_this_I_know_it_but_for_ALL_browsers_NO")]
		protected virtual void RegisterValidatorDeclaration()
		{
			throw new NotImplementedException();
			//TODO: Since I have to access document.<ClientID> and register
			// as page validator. Now this is Browser dependent :((

			// This does not render anything on Mozilla, and does on IE
		}

		[MonoTODO("Render_ing_always_left")]
		protected override void Render (HtmlTextWriter writer)
		{
			bool valid;

			if (!Enabled)
				return;

			if (isPreRenderCalled) {
				valid = IsValid;
			} else {
				isPropertiesChecked = true;
				propertiesValid     = true;
				renderUplevel       = false;
				valid               = true;
			}

			if (PropertiesValid) {
				if (Page != null)
					Page.VerifyRenderingInServerForm (this);

				ValidatorDisplay dis = Display;
				if (RenderUplevel) {
					//FIXME: as of now, don't do client-side validation
					throw new NotImplementedException();
				}
				
				if (!valid) {
					RenderBeginTag (writer);
					if (Text.Trim ().Length > 0 || HasControls ()) {
						RenderContents (writer);
						RenderEndTag (writer);
					} else {
						writer.Write (ErrorMessage);
					}
					RenderEndTag (writer);
					return;
				}
			} else {
				writer.Write ("&nbsp;");
			}
		}

		protected abstract bool EvaluateIsValid();
	}
}
