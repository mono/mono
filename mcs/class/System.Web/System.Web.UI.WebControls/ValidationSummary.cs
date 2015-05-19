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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[Designer ("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ValidationSummary : WebControl {
		#region Public Constructors
		public ValidationSummary() : base(HtmlTextWriterTag.Div) {
			this.ForeColor = Color.Red;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[DefaultValue(ValidationSummaryDisplayMode.BulletList)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public ValidationSummaryDisplayMode DisplayMode {
			get {
				object obj;

				obj = ViewState["DisplayMode"];
				if (obj != null) {
					return (ValidationSummaryDisplayMode)obj;
				}
				return ValidationSummaryDisplayMode.BulletList;
			}

			set {
				ViewState["DisplayMode"] = value;
			}
		}

		[DefaultValue(true)]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public bool EnableClientScript {
			get {
				return ViewState.GetBool("EnableClientScript", true);
			}

			set {
				ViewState["EnableClientScript"] = value;
			}
		}

		[DefaultValue(typeof (Color), "Red")]
		public override System.Drawing.Color ForeColor {
			get {
				return base.ForeColor;
			}

			set {
				base.ForeColor = value;
			}
		}

		[DefaultValue("")]
		[Localizable (true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public string HeaderText {
			get {
				return ViewState.GetString("HeaderText", string.Empty);
			}

			set {
				ViewState["HeaderText"] = value;
			}
		}

		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public bool ShowMessageBox {
			get {
				return ViewState.GetBool("ShowMessageBox", false);
			}

			set {
				ViewState["ShowMessageBox"] = value;
			}
		}

		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public bool ShowSummary {
			get {
				return ViewState.GetBool("ShowSummary", true);
			}

			set {
				ViewState["ShowSummary"] = value;
			}
		}

		[DefaultValue ("")]
		[Themeable (false)]
		public virtual string ValidationGroup
		{
			get {
				return ViewState.GetString("ValidationGroup", string.Empty);
			}
			set {
				ViewState["ValidationGroup"] = value;
			}
		}
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		[MonoTODO ()]
		// for 2.0: not XHTML attributes must be registered with RegisterExpandoAttribute 
		// when it will be implemented, in this case WebUIValidation_2.0.js muist be refactored
		protected override void AddAttributesToRender(HtmlTextWriter writer) {
			base.AddAttributesToRender (writer);

			if (EnableClientScript && pre_render_called && Page.AreValidatorsUplevel (ValidationGroup)) {
				/* force an ID here if we weren't assigned one */
				if (ID == null)
					writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
				if (ValidationGroup != String.Empty)
					RegisterExpandoAttribute (ClientID, "validationGroup", ValidationGroup);

				if (HeaderText.Length > 0)
					RegisterExpandoAttribute (ClientID, "headertext", HeaderText);

				if (ShowMessageBox)
					RegisterExpandoAttribute (ClientID, "showmessagebox", "True");

				if (!ShowSummary)
					RegisterExpandoAttribute (ClientID, "showsummary", "False");

				if (DisplayMode != ValidationSummaryDisplayMode.BulletList)
					RegisterExpandoAttribute (ClientID, "displaymode", DisplayMode.ToString ());

				if (!has_errors)
					writer.AddStyleAttribute ("display", "none");
			}
		}

		internal void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue) {
			RegisterExpandoAttribute (controlId, attributeName, attributeValue, false);
		}

		internal void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue, bool encode) {
			if (Page.ScriptManager != null)
				Page.ScriptManager.RegisterExpandoAttributeExternal (this, controlId, attributeName, attributeValue, encode);
			else
				Page.ClientScript.RegisterExpandoAttribute (controlId, attributeName, attributeValue, encode);
		}
		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			if (!RenderingCompatibilityLessThan40)
				return;

			if (ForeColor == Color.Empty)
				ForeColor = Color.Red;
		}
		protected internal
		override void OnPreRender(EventArgs e) {
			base.OnPreRender (e);

			pre_render_called = true;
		}

		protected internal
		override void Render(HtmlTextWriter writer) {
			if (!IsEnabled)
				return;
			ValidatorCollection	validators;
			ArrayList		errors;

			// First, figure out if there's even data to deal with
			validators = Page.GetValidators (ValidationGroup);

			// We have validators
			errors = new ArrayList(validators.Count);
			for (int i = 0; i < validators.Count; i++) {
				if (!validators[i].IsValid) {
					errors.Add(validators[i].ErrorMessage);
				}
			}

			has_errors = errors.Count > 0;

			if (EnableClientScript && pre_render_called && Page.AreValidatorsUplevel (ValidationGroup)) {
				if (Page.ScriptManager != null) {
					Page.ScriptManager.RegisterArrayDeclarationExternal (this, "Page_ValidationSummaries", String.Concat ("document.getElementById ('", ClientID, "')"));
					Page.ScriptManager.RegisterStartupScriptExternal (this, typeof (BaseValidator), ClientID + "DisposeScript",
@"
document.getElementById('" + ClientID + @"').dispose = function() {
	Array.remove(Page_ValidationSummaries, document.getElementById('" + ClientID + @"'));
}
", true);
					}
				else
				Page.ClientScript.RegisterArrayDeclaration ("Page_ValidationSummaries",
									    String.Concat ("document.getElementById ('", ClientID, "')"));
			}

			if ((ShowSummary && has_errors) || (EnableClientScript && pre_render_called))
				base.RenderBeginTag(writer);

			if (ShowSummary && has_errors) {
				switch(DisplayMode) {
					case ValidationSummaryDisplayMode.BulletList: {
						if (HeaderText.Length > 0) {
							writer.Write(HeaderText);
						}

						writer.Write("<ul>");
						for (int i = 0; i < errors.Count; i++) {
							writer.Write("<li>");
							writer.Write(errors[i]);
							writer.Write("</li>");
						}
						writer.Write("</ul>");
						break;
					}

					case ValidationSummaryDisplayMode.List: {
						if (HeaderText.Length > 0) {
							writer.Write(HeaderText);
							writer.Write("<br />");
						}

						for (int i = 0; i < errors.Count; i++) {
							writer.Write(errors[i]);
							writer.Write("<br />");
						}
						break;
					}

					case ValidationSummaryDisplayMode.SingleParagraph: {
						if (HeaderText.Length > 0) {
							writer.Write(HeaderText);
							writer.Write(" ");
						}

						for (int i = 0; i < errors.Count; i++) {
							writer.Write(errors[i]);
							writer.Write(" ");
						}
						writer.Write("<br />");

						break;
					}
				}
			}

			if ((ShowSummary && has_errors) || (EnableClientScript && pre_render_called))
				base.RenderEndTag(writer);
		}
		#endregion	// Public Instance Methods

		bool pre_render_called;
		bool has_errors;
	}
}
