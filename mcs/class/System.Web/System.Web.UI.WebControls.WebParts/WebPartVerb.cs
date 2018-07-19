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


using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System;

namespace System.Web.UI.WebControls.WebParts
{
	[TypeConverterAttribute ("System.Web.UI.WebControls.WebParts.WebPartVerbConverter, System.Web")]
	public class WebPartVerb : IStateManager
	{
		string clientClickHandler;
		WebPartEventHandler serverClickHandler;
		StateBag stateBag;
		bool isChecked = false;
		string description = string.Empty;
		bool enabled = true;
		string imageUrl = string.Empty;
		string text = string.Empty;
		bool visible = true;
		string id;
		
		public string ID {
			get { return id;}
		}

		public WebPartVerb (string id, string clientClickHandler) {
			this.id = id;
			this.clientClickHandler = clientClickHandler;
			stateBag = new StateBag ();
			stateBag.Add ("clientClickHandler", clientClickHandler);
		}


		public WebPartVerb (string id, WebPartEventHandler serverClickHandler) {
			this.id = id;
			this.serverClickHandler = serverClickHandler;
			stateBag = new StateBag ();
			stateBag.Add ("serverClickHandler", serverClickHandler);
		}

		public WebPartVerb (string id, WebPartEventHandler serverClickHandler, string clientClickHandler) {
			this.id = id;
			this.serverClickHandler = serverClickHandler;
			this.clientClickHandler = clientClickHandler;
			stateBag = new StateBag ();
			stateBag.Add ("serverClickHandler", serverClickHandler);
			stateBag.Add ("clientClickHandler", clientClickHandler);
		}

		[MonoTODO("Not implemented")]
		protected virtual void LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		protected virtual object SaveViewState()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		protected virtual void TrackViewState()
		{
			throw new NotImplementedException();
		}

		[MonoTODO("Not implemented")]
		void IStateManager.LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		object IStateManager.SaveViewState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		void IStateManager.TrackViewState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		bool IStateManager.IsTrackingViewState {
			get {
				throw new NotImplementedException ();
			}
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
				"UITypeEditor, System.Drawing"),
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
