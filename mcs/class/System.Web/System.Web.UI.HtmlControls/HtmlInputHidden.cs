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
//
// System.Web.UI.HtmlControls.HtmlInputHidden.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc.

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
	public class HtmlInputHidden : HtmlInputControl, IPostBackDataHandler {

		static readonly object ServerChangeEvent = new object ();

		public HtmlInputHidden () : base ("hidden")
		{
		}

		bool LoadPostDataInternal (string postDataKey, NameValueCollection postCollection)
		{
			string data = postCollection [postDataKey];
			if (data != null && data != Value) {
#if NET_2_0
				ValidateEvent (postDataKey, String.Empty);
#endif
				Value = data;
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
			RaisePostDataChangedEventInternal ();
		}
#endif		
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
							NameValueCollection postCollection)
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

#if NET_2_0
		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			Page page = Page;
			if (page != null)
				page.ClientScript.RegisterForEventValidation (Name);
			base.RenderAttributes (writer);
		}		

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
			EventHandler handler = (EventHandler) Events [ServerChangeEvent];
			if (handler != null)
				handler (this, e);
		}
			
		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerChange {
			add { Events.AddHandler (ServerChangeEvent, value); }
			remove { Events.RemoveHandler (ServerChangeEvent, value); }
		}
	}
}
