//
// System.Web.UI.WebControls.BaseValidator
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Web.Configuration;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultProperty("ErrorMessage")]
	[Designer("System.Web.UI.Design.WebControls.BaseValidatorDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class BaseValidator : Label, IValidator
	{
		bool render_uplevel;
		bool valid;
		Color forecolor;
		bool pre_render_called = false;

		protected BaseValidator ()
		{
			this.valid = true;
			this.ForeColor = Color.Red;
		}

		// New in NET1.1 sp1
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string AssociatedControlID {
			get { return base.AssociatedControlID; }
			set { base.AssociatedControlID = value; }
		}

		[Themeable (false)]
		[DefaultValue ("")]
		public virtual string ValidationGroup {
			get { return ViewState.GetString ("ValidationGroup", String.Empty); }
			set { ViewState["ValidationGroup"] = value; }
		}

		[Themeable (false)]
		[DefaultValue (false)]
		public bool SetFocusOnError {
			get { return ViewState.GetBool ("SetFocusOnError", false); }
			set { ViewState["SetFocusOnError"] = value; }
		}

		/* listed in corcompare */
		[MonoTODO("Why override?")]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[DefaultValue ("")]
		public override string Text 
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[IDReferenceProperty (typeof (Control))]
		[Themeable (false)]
		[TypeConverter(typeof(System.Web.UI.WebControls.ValidatedControlConverter))]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public string ControlToValidate {
			get { return ViewState.GetString ("ControlToValidate", String.Empty); }
			set { ViewState ["ControlToValidate"] = value; }
		}

		[Themeable (false)]
		[DefaultValue(ValidatorDisplay.Static)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public ValidatorDisplay Display {
			get { return (ValidatorDisplay)ViewState.GetInt ("Display", (int)ValidatorDisplay.Static); }
			set { ViewState ["Display"] = (int)value; }
		}

		[Themeable (false)]
		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public bool EnableClientScript {
			get { return ViewState.GetBool ("EnableClientScript", true); }
			set { ViewState ["EnableClientScript"] = value; }
		}

		public override bool Enabled {
			get { return ViewState.GetBool ("BaseValidatorEnabled", true); }
			set { ViewState ["BaseValidatorEnabled"] = value; }
		}

		[Localizable (true)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public string ErrorMessage {
			get { return ViewState.GetString ("ErrorMessage", String.Empty); }
			set { ViewState ["ErrorMessage"] = value; }
		}

		[DefaultValue(typeof (Color), "Red")]
		public override Color ForeColor {
			get { return forecolor; }
			set {
				forecolor = value;
				base.ForeColor = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(true)]
		[Themeable (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public bool IsValid {
			get { return valid; }
			set { valid = value; }
		}

		protected bool PropertiesValid {
			get {
				Control control = NamingContainer.FindControl (ControlToValidate);
				if (control == null)
					return false;
				else
					return true;
			}
		}

		protected bool RenderUplevel {
			get { return render_uplevel; }
		}

		internal bool GetRenderUplevel ()
		{
			return render_uplevel;
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			/* if we're rendering uplevel, add our attributes */
			if (render_uplevel) {
				/* force an ID here if we weren't assigned one */
				if (ID == null)
					writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);

				if (ControlToValidate != String.Empty)
					RegisterExpandoAttribute (ClientID, "controltovalidate", GetControlRenderID (ControlToValidate));

				if (ErrorMessage != String.Empty)
					RegisterExpandoAttribute (ClientID, "errormessage", ErrorMessage, true);

				if (ValidationGroup != String.Empty)
					RegisterExpandoAttribute (ClientID, "validationGroup", ValidationGroup, true);

				if (SetFocusOnError)
					RegisterExpandoAttribute (ClientID, "focusOnError", "t");

				bool enabled = IsEnabled;
				if (!enabled)
					RegisterExpandoAttribute (ClientID, "enabled", "False");

				if (enabled && !IsValid)
					RegisterExpandoAttribute (ClientID, "isvalid", "False");
				else {
					if (Display == ValidatorDisplay.Static)
						writer.AddStyleAttribute ("visibility", "hidden");
					else
						writer.AddStyleAttribute ("display", "none");
				}

				if (Display != ValidatorDisplay.Static)
					RegisterExpandoAttribute (ClientID, "display", Display.ToString ());
			}

			base.AddAttributesToRender (writer);
		}

		internal void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue)
		{
			RegisterExpandoAttribute (controlId, attributeName, attributeValue, false);
		}

		internal void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue, bool encode)
		{
			Page page = Page;
			if (page.ScriptManager != null)
				page.ScriptManager.RegisterExpandoAttributeExternal (this, controlId, attributeName, attributeValue, encode);
			else
				page.ClientScript.RegisterExpandoAttribute (controlId, attributeName, attributeValue, encode);
		}

		protected void CheckControlValidationProperty (string name, string propertyName)
		{
			Control control = NamingContainer.FindControl (name);
			PropertyDescriptor prop = null;

			if (control == null)
				throw new HttpException (String.Format ("Unable to find control id '{0}'.", name));

			prop = BaseValidator.GetValidationProperty (control);
			if (prop == null)
				throw new HttpException (String.Format ("Unable to find ValidationProperty attribute '{0}' on control '{1}'", propertyName, name));
		}

		protected virtual bool ControlPropertiesValid ()
		{
			if (ControlToValidate.Length == 0) {
				throw new HttpException (String.Format ("ControlToValidate property of '{0}' cannot be blank.", ID));
			}

			CheckControlValidationProperty (ControlToValidate, String.Empty);

			return true;
		}

		protected virtual bool DetermineRenderUplevel ()
		{
			if (!EnableClientScript)
				return false;
			return UplevelHelper.IsUplevel (
				System.Web.Configuration.HttpCapabilitiesBase.GetUserAgentForDetection (HttpContext.Current.Request));
		}

		protected abstract bool EvaluateIsValid ();

		protected string GetControlRenderID (string name)
		{
			Control control = NamingContainer.FindControl (name);
			if (control == null)
				return null;

			return control.ClientID;
		}

		protected string GetControlValidationValue (string name)
		{
			Control control = NamingContainer.FindControl (name);

			if (control == null)
				return null;

			PropertyDescriptor prop = BaseValidator.GetValidationProperty (control);
			if (prop == null)
				return null;

			object o = prop.GetValue (control);

			if (o == null)
				return String.Empty;
			
			if (o is ListItem)
				return ((ListItem) o).Value;
			
			return o.ToString ();
		}

		public static PropertyDescriptor GetValidationProperty (object component)
		{
			PropertyDescriptorCollection props;
			System.ComponentModel.AttributeCollection col;

			props = TypeDescriptor.GetProperties (component);
			col = TypeDescriptor.GetAttributes (component);

			foreach (Attribute at in col) {
				ValidationPropertyAttribute vpa = at as ValidationPropertyAttribute;
				if (vpa != null && vpa.Name != null)
					return props[vpa.Name];
			}

			return null;
		}

		protected internal override void OnInit (EventArgs e)
		{
			Page page = Page;
			/* according to an msdn article, this is done here */
			if (page != null) {
				page.Validators.Add (this);
				page.GetValidators (ValidationGroup).Add (this);
			}
			base.OnInit (e);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			pre_render_called = true;
			
			ControlPropertiesValid ();

			render_uplevel = DetermineRenderUplevel ();
			if (render_uplevel)
				RegisterValidatorCommonScript ();
		}

		protected internal override void OnUnload (EventArgs e)
		{
			Page page = Page;
			/* according to an msdn article, this is done here */
			if (page != null) {
				page.Validators.Remove (this);
				string validationGroup = ValidationGroup;
				if (!String.IsNullOrEmpty (validationGroup))
					page.GetValidators (ValidationGroup).Remove (this);
			}
			base.OnUnload (e);
		}

		protected void RegisterValidatorCommonScript ()
		{
			Page page = Page;
			if (page != null) {
				if (page.ScriptManager != null) {
					page.ScriptManager.RegisterClientScriptResourceExternal (this, typeof (BaseValidator), "WebUIValidation_2.0.js");
					page.ScriptManager.RegisterClientScriptBlockExternal (this, typeof (BaseValidator), "ValidationInitializeScript", page.ValidationInitializeScript, true);
					page.ScriptManager.RegisterOnSubmitStatementExternal (this, typeof (BaseValidator), "ValidationOnSubmitStatement", page.ValidationOnSubmitStatement);
					page.ScriptManager.RegisterStartupScriptExternal (this, typeof (BaseValidator), "ValidationStartupScript", page.ValidationStartupScript, true);
				} else if (!page.ClientScript.IsClientScriptIncludeRegistered (typeof (BaseValidator), "Mono-System.Web-ValidationClientScriptBlock")) {
					page.ClientScript.RegisterClientScriptInclude (typeof (BaseValidator), "Mono-System.Web-ValidationClientScriptBlock",
										       page.ClientScript.GetWebResourceUrl (typeof (BaseValidator), "WebUIValidation_2.0.js"));
					page.ClientScript.RegisterClientScriptBlock (typeof (BaseValidator), "Mono-System.Web-ValidationClientScriptBlock.Initialize", page.ValidationInitializeScript, true);
					page.ClientScript.RegisterOnSubmitStatement (typeof (BaseValidator), "Mono-System.Web-ValidationOnSubmitStatement", page.ValidationOnSubmitStatement);
					page.ClientScript.RegisterStartupScript (typeof (BaseValidator), "Mono-System.Web-ValidationStartupScript", page.ValidationStartupScript, true);
				}
			}
		}

		protected virtual void RegisterValidatorDeclaration ()
		{
			Page page = Page;
			if (page != null) {
				if (page.ScriptManager != null) {
					page.ScriptManager.RegisterArrayDeclarationExternal (this, "Page_Validators", String.Concat ("document.getElementById ('", ClientID, "')"));
					page.ScriptManager.RegisterStartupScriptExternal (this, typeof (BaseValidator), ClientID + "DisposeScript",
											  @"
document.getElementById('" + ClientID + @"').dispose = function() {
    Array.remove(Page_Validators, document.getElementById('" + ClientID + @"'));
}
", true);
				} else
					page.ClientScript.RegisterArrayDeclaration ("Page_Validators", String.Concat ("document.getElementById ('", ClientID, "')"));
			}
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			if (!IsEnabled && !EnableClientScript)
				return;

			if (render_uplevel) {
				/* according to an msdn article, this is done here */
				RegisterValidatorDeclaration ();
			}

			bool render_tags = false;
			bool render_text = false;
			bool render_nbsp = false;
			bool v = IsValid;

			if (!pre_render_called) {
				render_tags = true;
				render_text = true;
			} else if (render_uplevel) {
				render_tags = true;
				render_text = Display != ValidatorDisplay.None;
			} else {
				if (Display != ValidatorDisplay.None) {
					render_tags = !v;
					render_text = !v;
					render_nbsp = v && Display == ValidatorDisplay.Static;
				}
			}

			if (render_tags) {
				AddAttributesToRender (writer);
				writer.RenderBeginTag (HtmlTextWriterTag.Span);
			}

			if (render_text || render_nbsp) {
				string text;
				if (render_text) {
					text = Text;
					if (String.IsNullOrEmpty (text))
						text = ErrorMessage;
				} else
					text = "&nbsp;";

				writer.Write (text);
			}


			if (render_tags)
				writer.RenderEndTag ();
		}

		/* the docs say "public sealed" here */
		public void Validate ()
		{
			if (IsEnabled && Visible)
				IsValid = ControlPropertiesValid () && EvaluateIsValid ();
			else
				IsValid = true;
		}
	}
}
