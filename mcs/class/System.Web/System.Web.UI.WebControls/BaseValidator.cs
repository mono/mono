//
// System.Web.UI.WebControls.BaseValidator
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web.UI.WebControls {

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

		protected BaseValidator ()
		{
			this.valid = true;
			this.ForeColor = Color.Red;
		}

		// New in NET1.1 sp1
		[Browsable(false)]
#if ONLY_1_1
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif		
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string AssociatedControlID {
			get {
				return base.AssociatedControlID;
			}
			set {
				base.AssociatedControlID = value;
			}
		}

#if NET_2_0
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
#endif

#if NET_2_0
		[IDReferenceProperty (typeof (Control))]
		[Themeable (false)]
#endif
		[TypeConverter(typeof(System.Web.UI.WebControls.ValidatedControlConverter))]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public string ControlToValidate {
			get { return ViewState.GetString ("ControlToValidate", String.Empty); }
			set { ViewState ["ControlToValidate"] = value; }
		}

#if NET_2_0
		[Themeable (false)]
#endif
#if ONLY_1_1		
		[Bindable(true)]
#endif		
		[DefaultValue(ValidatorDisplay.Static)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public ValidatorDisplay Display {
			get { return (ValidatorDisplay)ViewState.GetInt ("Display", (int)ValidatorDisplay.Static); }
			set { ViewState ["Display"] = (int)value; }
		}

#if NET_2_0
		[Themeable (false)]
#endif
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

#if NET_2_0
		[Localizable (true)]
#endif
#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
#if NET_2_0
		public
#else
		public virtual
#endif
		string ErrorMessage {
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
#if NET_2_0
		[Themeable (false)]
#endif
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
#if NET_2_0
		public
#else
		public virtual
#endif
		bool IsValid {
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
#if NET_2_0
					RegisterExpandoAttribute (ClientID, "controltovalidate", GetControlRenderID (ControlToValidate));
#else
					writer.AddAttribute ("controltovalidate", GetControlRenderID (ControlToValidate));
#endif

				if (ErrorMessage != String.Empty)
#if NET_2_0
					RegisterExpandoAttribute (ClientID, "errormessage", ErrorMessage, true);
#else
					writer.AddAttribute ("errormessage", ErrorMessage);

				if (Text != String.Empty)
					writer.AddAttribute ("text", Text);
#endif

#if NET_2_0
				if (ValidationGroup != String.Empty)
					RegisterExpandoAttribute (ClientID, "validationGroup", ValidationGroup, true);

				if (SetFocusOnError)
					RegisterExpandoAttribute (ClientID, "focusOnError", "t");
#endif
				if (!IsEnabled)
#if NET_2_0
					RegisterExpandoAttribute (ClientID, "enabled", "False");
#else
					writer.AddAttribute ("enabled", "false", false);
#endif

#if NET_2_0
				if (IsEnabled && !IsValid) {
					RegisterExpandoAttribute (ClientID, "isvalid", "False");
#else
				if (!IsValid) {
					writer.AddAttribute ("isvalid", "false", false);
#endif
				}
				else {
					if (Display == ValidatorDisplay.Static)
						writer.AddStyleAttribute ("visibility", "hidden");
					else
						writer.AddStyleAttribute ("display", "none");
				}

				if (Display != ValidatorDisplay.Static)
#if NET_2_0
					RegisterExpandoAttribute (ClientID, "display", Display.ToString ());
#else
					writer.AddAttribute ("display", Display.ToString());
#endif
			}

			base.AddAttributesToRender (writer);
		}

#if NET_2_0
		internal void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue) {
			RegisterExpandoAttribute (controlId, attributeName, attributeValue, false);
		}

		internal void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue, bool encode) {
			if (Page.ScriptManager != null)
				Page.ScriptManager.RegisterExpandoAttributeExternal (this, controlId, attributeName, attributeValue, encode);
			else
				Page.ClientScript.RegisterExpandoAttribute (controlId, attributeName, attributeValue, encode);
		}
#endif

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

			CheckControlValidationProperty (ControlToValidate, "");

			return true;
		}

		protected virtual bool DetermineRenderUplevel ()
		{
			if (!EnableClientScript)
				return false;
#if TARGET_J2EE
			if (HttpContext.Current == null)
				return false;

			return (
				/* From someplace on the web: "JavaScript 1.2
				 * and later (also known as ECMAScript) has
				 * built-in support for regular
				 * expressions" */
				((Page.Request.Browser.EcmaScriptVersion.Major == 1
				  && Page.Request.Browser.EcmaScriptVersion.Minor >= 2)
				 || (Page.Request.Browser.EcmaScriptVersion.Major > 1))

				/* document.getElementById, .getAttribute,
				 * etc, are all DOM level 1.  I don't think we
				 * use anything in level 2.. */
				&& Page.Request.Browser.W3CDomVersion.Major >= 1);
#else
			return UplevelHelper.IsUplevel (
				System.Web.Configuration.HttpCapabilitiesBase.GetUserAgentForDetection (HttpContext.Current.Request));
#endif
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

		public static PropertyDescriptor GetValidationProperty (object o)
		{
			PropertyDescriptorCollection props;
			System.ComponentModel.AttributeCollection col;

			props = TypeDescriptor.GetProperties (o);
			col = TypeDescriptor.GetAttributes (o);

			foreach (Attribute at in col) {
				ValidationPropertyAttribute vpa = at as ValidationPropertyAttribute;
				if (vpa != null && vpa.Name != null)
					return props[vpa.Name];
			}

			return null;
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnInit (EventArgs e)
		{
			/* according to an msdn article, this is done here */
			if (Page != null) {
				Page.Validators.Add (this);

#if NET_2_0
				Page.GetValidators (ValidationGroup).Add (this);
#endif
			}
			base.OnInit (e);
		}

		bool pre_render_called = false;

#if NET_2_0
		protected internal
#else
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			pre_render_called = true;
			
			ControlPropertiesValid ();

			render_uplevel = DetermineRenderUplevel ();
			if (render_uplevel) {
				RegisterValidatorCommonScript ();
			}
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnUnload (EventArgs e)
		{
			/* according to an msdn article, this is done here */
			if (Page != null) {
				Page.Validators.Remove (this);

#if NET_2_0
				if (ValidationGroup != "")
					Page.GetValidators (ValidationGroup).Remove (this);
#endif

			}
			base.OnUnload (e);
		}

		protected void RegisterValidatorCommonScript ()
		{
#if NET_2_0
			if (Page.ScriptManager != null) {
				Page.ScriptManager.RegisterClientScriptResourceExternal (this, typeof (BaseValidator), "WebUIValidation_2.0.js");
				Page.ScriptManager.RegisterClientScriptBlockExternal (this, typeof (BaseValidator), "ValidationInitializeScript", Page.ValidationInitializeScript, true);
				Page.ScriptManager.RegisterOnSubmitStatementExternal (this, typeof (BaseValidator), "ValidationOnSubmitStatement", Page.ValidationOnSubmitStatement);
				Page.ScriptManager.RegisterStartupScriptExternal (this, typeof (BaseValidator), "ValidationStartupScript", Page.ValidationStartupScript, true);
			}
			else
#endif
			if (!Page.ClientScript.IsClientScriptIncludeRegistered (typeof (BaseValidator), "Mono-System.Web-ValidationClientScriptBlock"))
			{
				Page.ClientScript.RegisterClientScriptInclude (typeof (BaseValidator), "Mono-System.Web-ValidationClientScriptBlock",
					Page.ClientScript.GetWebResourceUrl (typeof (BaseValidator), 
#if NET_2_0
						"WebUIValidation_2.0.js"));
				Page.ClientScript.RegisterClientScriptBlock (typeof (BaseValidator), "Mono-System.Web-ValidationClientScriptBlock.Initialize", Page.ValidationInitializeScript, true);
				Page.ClientScript.RegisterOnSubmitStatement (typeof (BaseValidator), "Mono-System.Web-ValidationOnSubmitStatement", Page.ValidationOnSubmitStatement);
				Page.ClientScript.RegisterStartupScript (typeof (BaseValidator), "Mono-System.Web-ValidationStartupScript", Page.ValidationStartupScript, true);
#else		
						"WebUIValidation.js"));

				Page.ClientScript.RegisterOnSubmitStatement ("Mono-System.Web-ValidationOnSubmitStatement",
									     "if (!ValidatorOnSubmit()) return false;");
				Page.ClientScript.RegisterStartupScript ("Mono-System.Web-ValidationStartupScript",
									 "<script language=\"JavaScript\">\n" + 
									 "<!--\n" + 
									 "var Page_ValidationActive = false;\n" + 
									 "ValidatorOnLoad();\n" +
									 "\n" + 
									 "function ValidatorOnSubmit() {\n" + 
									 "        if (Page_ValidationActive) {\n" + 
									 "                if (!ValidatorCommonOnSubmit())\n" +
									 "                        return Page_ClientValidate ();\n" +
									 "        }\n" + 
									 "        return true;\n" + 
									 "}\n" + 
									 "// -->\n" + 
									 "</script>\n");
#endif
			}
		}

		protected virtual void RegisterValidatorDeclaration ()
		{
#if NET_2_0
			if (Page.ScriptManager != null) {
				Page.ScriptManager.RegisterArrayDeclarationExternal (this, "Page_Validators", String.Concat ("document.getElementById ('", ClientID, "')"));
				Page.ScriptManager.RegisterStartupScriptExternal (this, typeof (BaseValidator), ClientID + "DisposeScript",
@"
document.getElementById('" + ClientID + @"').dispose = function() {
    Array.remove(Page_Validators, document.getElementById('" + ClientID + @"'));
}
", true);
			}
			else
#endif
			Page.ClientScript.RegisterArrayDeclaration ("Page_Validators",
								    String.Concat ("document.getElementById ('", ClientID, "')"));
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter writer)
		{
#if NET_2_0
			if (!IsEnabled && !EnableClientScript)
				return;
#endif
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
			}
			else if (render_uplevel) {
				render_tags = true;
#if NET_2_0
				render_text = Display != ValidatorDisplay.None;
#else
				if (Display != ValidatorDisplay.None)
					render_text = !v || Display == ValidatorDisplay.Dynamic;
#endif
			}
			else {
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
					if (text == "")
						text = ErrorMessage;
				} else {
					text = "&nbsp;";
				}

				writer.Write (text);
			}


			if (render_tags) {
				writer.RenderEndTag ();
			}
		}

		/* the docs say "public sealed" here */
#if NET_2_0
		public
#else
		public virtual
#endif
		void Validate ()
		{
			if (IsEnabled && Visible)
				IsValid = ControlPropertiesValid () && EvaluateIsValid ();
			else
				IsValid = true;
		}
	}

}
