//
// System.Web.UI.HtmlControls.HtmlTextArea.cs
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
	public class HtmlTextArea : HtmlContainerControl, IPostBackDataHandler 
	{
		static readonly object serverChangeEvent = new object ();

		public HtmlTextArea ()
			: base ("textarea")
		{
		}


		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public int Cols {
			get {
				string s = Attributes ["cols"];
				return (s == null) ? -1 : Convert.ToInt32 (s);
			}
			set {
				if (value == -1)
					Attributes.Remove ("cols");
				else
					Attributes ["cols"] = value.ToString (Helpers.InvariantCulture);
			}
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
		[WebCategory("Appearance")]
		public int Rows {
			get {
				string s = Attributes ["rows"];
				return (s == null) ? -1 : Convert.ToInt32 (s);
			}
			set {
				if (value == -1)
					Attributes.Remove ("rows");
				else
					Attributes ["rows"] = value.ToString (Helpers.InvariantCulture);
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public string Value {
			get { return InnerText; }
			set { InnerText = value; }
		}


		protected override void AddParsedSubObject (object obj)
		{
			if (!((obj is LiteralControl) || (obj is DataBoundLiteralControl))) {
				throw new HttpException (Locale.GetText ("Wrong type."));
			}
			base.AddParsedSubObject (obj);
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
				Page.ClientScript.RegisterForEventValidation (UniqueID);
#endif
			if (Attributes ["name"] == null) {
				writer.WriteAttribute ("name", Name);
			}
			base.RenderAttributes (writer);
		}

#if NET_2_0
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return DefaultLoadPostData (postDataKey, postCollection);
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			ValidateEvent (UniqueID, String.Empty);
			OnServerChange (EventArgs.Empty);
		}
#endif

		internal bool DefaultLoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			string s = postCollection [postDataKey];
			if ((s != null) && (s != Value)) {
				Value = s;
				return true;
			}
			return false;
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
#if NET_2_0
			return LoadPostData (postDataKey, postCollection);
#else
			return DefaultLoadPostData (postDataKey, postCollection);
#endif
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
#if NET_2_0
			RaisePostDataChangedEvent ();
#else
			OnServerChange (EventArgs.Empty);
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
