//
// System.Web.UI.WebControls.WebParts.WebPartVerb.cs
//
// Authors:
//      Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//
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

#if NET_2_0

using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System;

namespace System.Web.UI.WebControls.WebParts
{
	[TypeConverterAttribute ("System.Web.UI.WebControls.WebParts.WebPartVerbConverter, System.Web")]
	public class WebPartVerb : IStateManager
	{
		private string clientClickHandler;
		private WebPartEventHandler serverClickHandler;
		private StateBag stateBag;
		private bool isChecked = false;
		private string description = string.Empty;
		private bool enabled = true;
		private string imageUrl = string.Empty;
		private string text = string.Empty;
		private bool visible = true;

		public WebPartVerb (string clientHandler)
		{
			this.clientClickHandler = clientHandler;
			stateBag = new StateBag ();
			stateBag.Add ("clientClickHandler", clientHandler);

		}

		public WebPartVerb (WebPartEventHandler serverHandler)
		{
			this.serverClickHandler = serverHandler;
			stateBag = new StateBag ();
			stateBag.Add ("serverClickHandler", serverHandler);
		}

		public WebPartVerb (WebPartEventHandler serverHandler, string clientHandler)
		{
			this.serverClickHandler = serverHandler;
			this.clientClickHandler = clientHandler;
			stateBag = new StateBag ();
			stateBag.Add ("serverClickHandler", serverHandler);
			stateBag.Add ("clientClickHandler", clientHandler);
		}

		[MonoTODO]
		protected virtual void LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual object SaveViewState()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void TrackViewState()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		void IStateManager.LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IStateManager.SaveViewState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IStateManager.TrackViewState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IStateManager.get_IsTrackingViewState ()
		{
			throw new NotImplementedException ();
		}

		[WebSysDescriptionAttribute ("Denotes verb is checked or not."),
		DefaultValueAttribute (false),
		NotifyParentPropertyAttribute (true) ]
		public virtual bool Checked {
			get { return isChecked; }
			set { isChecked = value; }
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden),
		 BrowsableAttribute (false)]
		public string ClientClickHandler {
			get { return clientClickHandler; }
		}

		[LocalizableAttribute (true),
		 WebSysDescriptionAttribute ("Gives descriptive information about the verb"),
		 NotifyParentPropertyAttribute (true)]
		 //WebSysDefaultValueAttribute (string.Empty)]			
		public virtual string Description {
			get { return description; }
			set { description = value; }
		}

		[NotifyParentPropertyAttribute (true),
		 DefaultValueAttribute (true),
		 WebSysDescriptionAttribute ("Determines whether verb is enabled.")]			
		public virtual bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}

		[WebSysDescriptionAttribute ("Denotes URL of the image to be displayed for the verb"),
		 EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design", 
				"System.Drawing.Design.UITypeEditor, System.Drawing"),
		 LocalizableAttribute (true), NotifyParentPropertyAttribute (true)]
		//UrlPropertyAttribute, DefaultValueAttribute (String.Empty)
		public string ImageUrl {
			get { return imageUrl; }
			set { imageUrl = value; }
		}

		protected virtual bool IsTrackingViewState {
			get { throw new NotImplementedException (); }
		}

		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden),
		 BrowsableAttribute (false)]
		public WebPartEventHandler ServerClickHandler
		{
			get { return serverClickHandler; }
		}

		[WebSysDescriptionAttribute ("Denotes text to be displayed for the verb"),
		 LocalizableAttribute (true), NotifyParentPropertyAttribute (true)]
		//DefaultValueAttribute (String.Empty)
		public virtual string Text
		{
			get { return text; }
			set { text = value; }
		}

		protected StateBag ViewState {
			get { return stateBag; }
		}

		[DefaultValueAttribute (true),
		 WebSysDescriptionAttribute ("Denotes whether the verb is visible"),
		 LocalizableAttribute (true), NotifyParentPropertyAttribute (true)]
		public bool Visible
		{
			get { return visible; }
			set { visible = value; }
		}
	}
}
#endif
