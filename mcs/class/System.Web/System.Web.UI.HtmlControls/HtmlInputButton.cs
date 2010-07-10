//
// System.Web.UI.HtmlControls.HtmlInputButton.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005-2010 Novell, Inc.
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
using System.Reflection;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEventAttribute ("ServerClick")]
	[SupportsEventValidation]
	public class HtmlInputButton : HtmlInputControl, IPostBackEventHandler 
	{
		static readonly object ServerClickEvent = new object();

		public HtmlInputButton () : this ("button")
		{
		}

		public HtmlInputButton (string type) :	base (type)
		{
		}

		[DefaultValue(true)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public virtual bool CausesValidation {
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

		[DefaultValue ("")]
		public virtual string ValidationGroup
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

		void RaisePostBackEventInternal (string eventArgument)
		{
			ValidateEvent (UniqueID, eventArgument);
			if (CausesValidation)
				Page.Validate (ValidationGroup);
			
			if (String.Compare (Type, "reset", true, Helpers.InvariantCulture) != 0)
				OnServerClick (EventArgs.Empty);
			else
				ResetForm (FindForm ());
		}

		HtmlForm FindForm ()
		{
			Page p = Page;
			if (p != null)
				return p.Form;

			return null;
		}
		
		void ResetForm (HtmlForm form)
		{
			if (form == null || !form.HasControls ())
				return;
			
			ResetChildrenValues (form.Controls);
		}

		void ResetChildrenValues (ControlCollection children)
		{
			foreach (Control child in children) {
				if (child == null)
					continue;
				
				if (child.HasControls ())
					ResetChildrenValues (child.Controls);
				ResetChildValue (child);
			}
		}

		void ResetChildValue (Control child)
		{
			Type type = child.GetType ();
			object[] attributes = type.GetCustomAttributes (false);
			if (attributes == null || attributes.Length == 0)
				return;

			string defaultProperty = null;
			DefaultPropertyAttribute defprop;
			
			foreach (object attr in attributes) {
				defprop = attr as DefaultPropertyAttribute;
				if (defprop == null)
					continue;
				defaultProperty = defprop.Name;
				break;
			}

			if (defaultProperty == null || defaultProperty.Length == 0)
				return;

			PropertyInfo pi = null;
			try {
				pi = type.GetProperty (defaultProperty,
						       BindingFlags.Instance |
						       BindingFlags.Public |
						       BindingFlags.Static |
						       BindingFlags.IgnoreCase);
			} catch (Exception) {
				// ignore
			}
			if (pi == null || !pi.CanWrite)
				return;
			
			attributes = pi.GetCustomAttributes (false);
			if (attributes == null || attributes.Length == 0)
				return;

			DefaultValueAttribute defval = null;
			object value = null;
			
			foreach (object attr in attributes) {
				defval = attr as DefaultValueAttribute;
				if (defval == null)
					continue;
				value = defval.Value;
				break;
			}
			
			if (value == null || pi.PropertyType != value.GetType ())
				return;

			try {
				pi.SetValue (child, value, null);
			} catch (Exception) {
				// ignore
			}
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEventInternal (eventArgument);
		}
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}

		protected internal override void OnPreRender (EventArgs e)
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

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			CultureInfo inv = Helpers.InvariantCulture;
			string input_type = Type;
			if (0 != String.Compare (input_type, "reset", true, inv) &&
				((0 == String.Compare (input_type, "submit", true, inv)) ||
				(0 == String.Compare (input_type, "button", true, inv) && Events [ServerClickEvent] != null))) {

				string onclick = String.Empty;
				if (Attributes ["onclick"] != null) {
					onclick = ClientScriptManager.EnsureEndsWithSemicolon (Attributes ["onclick"] + onclick);
					Attributes.Remove ("onclick");
				}

				Page page = Page;
				if (page != null) {
					PostBackOptions options = GetPostBackOptions ();
					onclick += page.ClientScript.GetPostBackEventReference (options, true);
				}

				if (onclick.Length > 0) {
					bool encode = true;
					if (Events [ServerClickEvent] != null)
						encode = false; // tests show that this is indeed
								// the case...
					writer.WriteAttribute ("onclick", onclick, encode);
					writer.WriteAttribute ("language", "javascript");
				}
			}

			Attributes.Remove ("CausesValidation");
			// LAMESPEC: MS doesn't actually remove this
			//attribute.  it shows up in the rendered
			//output.

			// Attributes.Remove("ValidationGroup");
			base.RenderAttributes (writer);
		}

		PostBackOptions GetPostBackOptions ()
		{
			Page page = Page;
			PostBackOptions options = new PostBackOptions (this);
			options.ValidationGroup = null;
			options.ActionUrl = null;
			options.Argument = String.Empty;
			options.RequiresJavaScriptProtocol = false;
			options.ClientSubmit = (0 != String.Compare (Type, "submit", true, Helpers.InvariantCulture));
			options.PerformValidation = CausesValidation && page != null && page.Validators.Count > 0;
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}

		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerClick {
			add { Events.AddHandler (ServerClickEvent, value); }
			remove { Events.RemoveHandler (ServerClickEvent, value); }
		}
	}	
}

