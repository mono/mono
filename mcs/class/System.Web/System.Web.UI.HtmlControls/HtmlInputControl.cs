//
// System.Web.UI.HtmlControls.HtmlInputControl.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
#if NET_2_0
	[ControlBuilder (typeof (HtmlEmptyTagControlBuilder))]
#else
	[ControlBuilder (typeof (HtmlControlBuilder))]
#endif
	public abstract class HtmlInputControl : HtmlControl {

#if NET_2_0
		private string inputType;
#endif

		protected HtmlInputControl (string type)
			: base ("input")
		{
			if (type == null)
				type = String.Empty;
#if NET_2_0
			inputType = type;
#else
			Attributes ["type"] = type;
#endif
		}


		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public virtual string Name {
			get { return UniqueID; }
			set { ; }
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public string Type {
#if NET_2_0
			get { return inputType; }
#else
			get { return Attributes ["type"]; }
#endif
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public virtual string Value {
			get {
				string s = Attributes ["value"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("value");
				else
					Attributes ["value"] = value;
			}
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			if (Attributes ["name"] == null) {
				writer.WriteAttribute ("name", Name);
			}
#if NET_2_0
			Attributes.Remove ("type");
			writer.WriteAttribute ("type", inputType);
#endif
			base.RenderAttributes (writer);
			writer.Write (" /");

#if NET_2_0
			if (Page != null && Page.Form != null && Page.Form.SubmitDisabledControls && Page.Form.DetermineRenderUplevel() && !Disabled)
				Page.ClientScript.RegisterArrayDeclaration ("__enabledControlArray", String.Format ("'{0}'", ClientID));
#endif
		}
	}
}

