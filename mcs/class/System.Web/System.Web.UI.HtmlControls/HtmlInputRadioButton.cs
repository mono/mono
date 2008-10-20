//
// System.Web.UI.HtmlControls.HtmlInputRadioButton.cs
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
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("ServerChange")]
#if NET_2_0
	[SupportsEventValidation]
#endif
	public class HtmlInputRadioButton : HtmlInputControl, IPostBackDataHandler 
	{
		static readonly object serverChangeEvent = new object ();

		public HtmlInputRadioButton ()
			: base ("radio")
		{
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Misc")]
		public bool Checked {
			get { return (Attributes ["checked"] == "checked"); }
			set {
				if (value)
					Attributes ["checked"] = "checked";
				else
					Attributes.Remove ("checked");
			}
		}

		public override string Name {
			get {
				string s = Attributes ["name"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("name");
				else
					Attributes ["name"] = value;
			}
		}

		public override string Value {
			get {
				string s = Attributes ["value"];
				if (s == null || s.Length == 0) {
					s = ID;
					if ((s != null) && (s.Length == 0))
						s = null;
				}
				return s;
			}
			set {
				if (value == null)
					Attributes.Remove ("value");
				else
					Attributes ["value"] = value;
			}
		}

#if NET_2_0
		protected internal
#else
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			if (Page != null && !Disabled) {
				Page.RegisterRequiresPostBack (this);
#if NET_2_0
				Page.RegisterEnabledControl (this);
#endif
			}
		}

		protected virtual void OnServerChange (EventArgs e)
		{
			EventHandler serverChange = (EventHandler) Events [serverChangeEvent];
			if (serverChange != null)
				serverChange (this, e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
#if NET_2_0
			if (Page != null)
				Page.ClientScript.RegisterForEventValidation (this.UniqueID, Value);
#endif
			writer.WriteAttribute ("value", Value, true);
			Attributes.Remove ("value");
			base.RenderAttributes (writer);
		}
#if NET_2_0
		protected virtual 
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			bool checkedOnClient = postCollection [Name] == Value;
			if (Checked == checkedOnClient)
				return false;

#if NET_2_0
			ValidateEvent (UniqueID, Value);
#endif
			Checked = checkedOnClient;
			return checkedOnClient;
		}

#if NET_2_0
		protected virtual 
#endif
		void RaisePostDataChangedEvent ()
		{
			OnServerChange (EventArgs.Empty);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}


		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerChange {
			add { Events.AddHandler (serverChangeEvent, value); }
			remove { Events.RemoveHandler (serverChangeEvent, value); }
		}
	}
}
