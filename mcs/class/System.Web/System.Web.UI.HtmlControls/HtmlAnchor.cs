//
// System.Web.UI.HtmlControls.HtmlAnchor.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
	[DefaultEvent ("ServerClick")]
	[SupportsEventValidation]
	public class HtmlAnchor : HtmlContainerControl, IPostBackEventHandler 
	{
		static readonly object serverClickEvent = new object ();

		public HtmlAnchor ()
			: base ("a")
		{
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Action")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[UrlProperty]
		public string HRef {
			get {
				string s = Attributes ["href"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null || value.Length == 0) {
					Attributes.Remove ("href");
				} else {
					Attributes ["href"] = value;
				}
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Navigation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Name {
			get {
				string s = Attributes ["name"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null || value.Length == 0)
					Attributes.Remove ("name");
				else
					Attributes ["name"] = value;
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Navigation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Target {
			get {
				string s = Attributes ["target"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null || value.Length == 0)
					Attributes.Remove ("target");
				else
					Attributes ["target"] = value;
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Localizable (true)]
		public string Title {
			get {
				string s = Attributes ["title"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null || value.Length == 0)
					Attributes.Remove ("title");
				else
					Attributes ["title"] = value;
			}
		}

		[DefaultValue (true)]
		public virtual bool CausesValidation {
			get {
				return ViewState.GetBool ("CausesValidation", true);
			}
			set {
				ViewState ["CausesValidation"] = value;
			}
		}

		[DefaultValue ("")]
		public virtual string ValidationGroup {
			get {
				return ViewState.GetString ("ValidationGroup", String.Empty);
			}
			set {
				ViewState ["ValidationGroup"] = value;
			}
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected virtual void OnServerClick (EventArgs e)
		{
			EventHandler serverClick = (EventHandler) Events [serverClickEvent];
			if (serverClick != null)
				serverClick (this, e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			// we don't want to render the "user" URL, so we either render:
			EventHandler serverClick = (EventHandler) Events [serverClickEvent];
			if (serverClick != null) {
				ClientScriptManager csm;

				// a script
				PostBackOptions options = GetPostBackOptions ();
				csm = Page.ClientScript;
				csm.RegisterForEventValidation (options);
				Attributes ["href"] = csm.GetPostBackEventReference (options, true);
			} else {
				string hr = HRef;
				if (hr != string.Empty)
#if TARGET_J2EE
					// For J2EE portlets we need to genreate a render URL.
					HRef = ResolveClientUrl (hr, String.Compare (Target, "_blank", StringComparison.InvariantCultureIgnoreCase) != 0);
#else
					HRef = ResolveClientUrl (hr);
#endif
			}

			base.RenderAttributes (writer);

			// but we never set back the href attribute after the rendering
			// nor is the property available after rendering
			Attributes.Remove ("href");
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			ValidateEvent (UniqueID, eventArgument);
			if (CausesValidation)
				Page.Validate (ValidationGroup);
			
			OnServerClick (EventArgs.Empty);
		}
	
		PostBackOptions GetPostBackOptions ()
		{
			Page page = Page;
			PostBackOptions options = new PostBackOptions (this);
			options.ValidationGroup = null;
			options.ActionUrl = null;
			options.Argument = String.Empty;
			options.RequiresJavaScriptProtocol = true;
			options.ClientSubmit = true;
			options.PerformValidation = CausesValidation && page != null && page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}

		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerClick {
			add { Events.AddHandler (serverClickEvent, value); }
			remove { Events.RemoveHandler (serverClickEvent, value); }
		}
	}
}
