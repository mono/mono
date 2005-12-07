//
// System.Web.UI.HtmlControls.HtmlInputButton.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc.
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
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEventAttribute ("ServerClick")]
#if NET_2_0
	[SupportsEventValidation]
#endif
	public class HtmlInputButton : HtmlInputControl, IPostBackEventHandler {

		private static readonly object ServerClickEvent = new object();

		public HtmlInputButton () : this ("button")
		{
		}

		public HtmlInputButton (string type) :	base (type)
		{
		}

		[DefaultValue(true)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
#if NET_2_0
		public virtual
#else		
		public
#endif		
		bool CausesValidation {
			get {
				string flag = Attributes["CausesValidation"];

				if (flag == null)
					return true;

				return Boolean.Parse (flag);
			}
			set {
				Attributes ["CausesValidation"] = value.ToString();
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		public string ValidationGroup
		{
			get {
				string group = Attributes["ValidationGroup"];

				if (group == null)
					return "";

				return group;
			}
			set {
				if (value == null)
					Attributes.Remove ("ValidationGroup");
				else
					Attributes["ValidationGroup"] = value;
			}
		}
#endif

		void RaisePostBackEventInternal (string eventArgument)
		{
			if (CausesValidation)
#if NET_2_0
				Page.Validate (ValidationGroup);
#else
				Page.Validate ();
#endif
			OnServerClick (EventArgs.Empty);
		}

#if NET_2_0
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEventInternal (eventArgument);
		}
#endif
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
#if NET_2_0
			RaisePostBackEvent (eventArgument);
#else
			RaisePostBackEventInternal (eventArgument);
#endif
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Events [ServerClickEvent] != null)
				Page.RequiresPostBackScript ();
		}

		protected virtual void OnServerClick (EventArgs e)
		{
			EventHandler server_click = (EventHandler) Events [ServerClickEvent];
			if (server_click != null)
				server_click (this, e);
		}

		bool RenderOnClick ()
		{
			if (Page == null || !CausesValidation)
				return false;

			CultureInfo inv = CultureInfo.InvariantCulture;
			string input_type = Type;
			if (0 == String.Compare (input_type, "submit", true, inv) &&
				Page.Validators.Count > 0)
				return true;

			if (0 == String.Compare (input_type, "button", true, inv) &&
				Events [ServerClickEvent] != null)
				return true;

			return false;
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			if (RenderOnClick ()) {
				string oc = null;
				ClientScriptManager csm = new ClientScriptManager (Page);
				if (Page.AreValidatorsUplevel ()) {
					oc = csm.GetClientValidationEvent ();
				} else if (Events [ServerClickEvent] != null) {
					oc = Attributes ["onclick"] + " " + csm.GetPostBackClientEvent (this, "");
				}
				
				if (oc != null) {
					writer.WriteAttribute ("language", "javascript");
					writer.WriteAttribute ("onclick", oc);
				}
			}

			Attributes.Remove ("CausesValidation");
#if NET_2_0
			// LAMESPEC: MS doesn't actually remove this
			//attribute.  it shows up in the rendered
			//output.

			// Attributes.Remove("ValidationGroup");
#endif
			base.RenderAttributes (writer);
		}

		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerClick {
			add { Events.AddHandler (ServerClickEvent, value); }
			remove { Events.RemoveHandler (ServerClickEvent, value); }
		}
	}
	
}

