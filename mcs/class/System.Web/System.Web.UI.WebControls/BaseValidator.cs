//
// System.Web.UI.WebControls.BaseValidator.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

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

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web;
using System.Web.UI;
using System.Drawing;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("ErrorMessage")]
	[Designer ("System.Web.UI.Design.WebControls.BaseValidatorDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
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

		[DefaultValue (""), WebCategory ("Behavior")]
		[TypeConverter (typeof (ValidatedControlConverter))]
		[WebSysDescription ("The ID of the control to validate.")]
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

		[DefaultValue (typeof (ValidatorDisplay), "Static"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("Determines how the validator is displayed.")]
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
				ViewState["Display"] = value;
			}
		}

		[DefaultValue (true), WebCategory ("Behavior")]
		[WebSysDescription ("Determines if client script is activated on uplevel browsers.")]
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

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("An error message that is displayed if the control validates to false.")]
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

		[DefaultValue (null)]
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

		[DefaultValue (true), Browsable (false), WebCategory ("Misc")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("Indicates if the control validated to false.")]
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
				return;
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
				base.Enabled = false;
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

		protected virtual bool DetermineRenderUplevel()
		{
			Page page = Page;
			if(page == null || page.Request == null)
			{
				return false;
			}

			if(EnableClientScript)
			{
				if(page.Request.Browser.MSDomVersion.Major > 4)
				{
					return page.Request.Browser.EcmaScriptVersion.CompareTo(new Version(1,2)) >= 0;
				}
				return false;
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
			
			string jsDirectory = System.Web.UI.Utils.GetScriptLocation(Context);
			string jsFile = jsDirectory + "/WebUIValidation.js";
			//TODO: Ok, now add the <script language="javascript"> etc
			//FIXME: Should I check for 'Explorer'? MS-Net seems to do it!
			throw new NotImplementedException();
		}

		[MonoTODO("I_have_to_know_javascript_for_this_I_know_it_but_for_ALL_browsers_NO")]
		protected virtual void RegisterValidatorDeclaration()
		{
			//FIXME: How to make is more abstract?
			//Browser Info... but future browsers???
			//I'm confused! This will make it work, at least on IE
			string val = "document.all[\"" + ClientID;
			val += "\"]";
			Page.RegisterArrayDeclaration("Page_Validators", val);
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

				if (!valid && dis != ValidatorDisplay.None) {
					RenderBeginTag (writer);
					if (Text.Trim ().Length > 0 || HasControls ())
						RenderContents (writer);
					else
						writer.Write (ErrorMessage);
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
