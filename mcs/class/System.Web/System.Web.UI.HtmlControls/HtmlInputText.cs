//
// System.Web.UI.HtmlControls.HtmlInputText.cs
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
using System.Globalization;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.HtmlControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("ServerChange")]
	[ValidationProperty ("Value")]
#if NET_2_0
	[SupportsEventValidation]
#endif
	public class HtmlInputText : HtmlInputControl, IPostBackDataHandler 
	{
		static readonly object serverChangeEvent = new object ();

		public HtmlInputText ()
			: base ("text")
		{
		}

		public HtmlInputText (string type)
			: base (type)
		{
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public int MaxLength {
			get {
				string s = Attributes ["maxlength"];
				return (s == null) ? -1 : Convert.ToInt32 (s);
			}
			set {
				if (value == -1)
					Attributes.Remove ("maxlength");
				else
					Attributes ["maxlength"] = value.ToString ();
			}
		}

#if NET_2_0
		[DefaultValue (-1)]
#else
		[DefaultValue ("")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public int Size {
			get {
				string s = Attributes ["size"];
				return (s == null) ? -1 : Convert.ToInt32 (s);
			}
			set {
				if (value == -1)
					Attributes.Remove ("size");
				else
					Attributes ["size"] = value.ToString ();
			}
		}

		public override string Value {
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


#if NET_2_0
		protected internal override void Render (HtmlTextWriter writer)
		{
			Page page = Page;
			if (page != null)
				page.ClientScript.RegisterForEventValidation (UniqueID);
			base.Render (writer);
		}
#endif

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
			// the Type property can be, indirectly, changed by using the Attributes property
			if (String.Compare (Type, 0, "password", 0, 8, true, Helpers.InvariantCulture) == 0) {
				Attributes.Remove ("value");
			}

			base.RenderAttributes (writer);
		}

		bool LoadPostDataInternal (string postDataKey, NameValueCollection postCollection)
		{
			string s = postCollection [postDataKey];
			if (Value != s) {
				Value = s;
				return true;
			}
			return false;
		}

		void RaisePostDataChangedEventInternal ()
		{
			OnServerChange (EventArgs.Empty);
		}

#if NET_2_0
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostDataInternal (postDataKey, postCollection);
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			ValidateEvent (UniqueID, String.Empty);
			RaisePostDataChangedEventInternal ();
		}
#endif

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
#if NET_2_0
			return LoadPostData (postDataKey, postCollection);
#else
			return LoadPostDataInternal (postDataKey, postCollection);
#endif
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
#if NET_2_0
			RaisePostDataChangedEvent ();
#else
			RaisePostDataChangedEventInternal ();
#endif
		}


		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerChange {
			add { Events.AddHandler (serverChangeEvent, value); }
			remove { Events.RemoveHandler (serverChangeEvent, value); }
		}
	}
}
