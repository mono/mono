//
// System.Web.UI.WebControls.WebParts.Part
//
// Authors: Chris Toshok <toshok@novell.com>
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

#if NET_2_0
using System;

namespace System.Web.UI.WebControls.WebParts
{
	public abstract class WebPart : Part, IWebPart, IWebActionable
#if IWebEditableInterface
	  , IWebEditable
#endif
	{
		[Flags]
		enum Allow {
			Close      = 0x01,
			Connect    = 0x02,
			Edit       = 0x04,
			Hide       = 0x08,
			Minimize   = 0x10,
			ZoneChange = 0x20
		}


		WebPartVerbCollection verbs;
		Allow allow;
		string auth_filter;
		string catalog_icon_url;

		protected WebPart ()
		{
			verbs = new WebPartVerbCollection();
			allow = Allow.Close | Allow.Connect | Allow.Edit | Allow.Hide | Allow.Minimize | Allow.ZoneChange;
			auth_filter = "";
			catalog_icon_url = "";
		}

#if IWebEditableInterface
		[MonoTODO]
		public virtual EditorPartCollection CreateEditorParts ()
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		protected void SetPersonalizationDirty ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetPersonalizationDirty (Control control)
		{
			throw new NotImplementedException ();
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState();

			foreach (IStateManager verb in verbs) {
				verb.TrackViewState();
			}
		}

		protected internal virtual void OnClosing (EventArgs e)
		{ /* no base class implementation */ }

		protected internal virtual void OnConnectModeChanged (EventArgs e)
		{ /* no base class implementation */ }

		protected internal virtual void OnDeleting (EventArgs e)
		{ /* no base class implementation */ }

		protected internal virtual void OnEditModeChanged (EventArgs e)
		{ /* no base class implementation */ }

		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool AllowClose 
		{
			get {
				return (allow & Allow.Close) != 0;
			}
			set {
				if (value)
					allow |= Allow.Close;
				else
					allow &= ~Allow.Close;
			}
		}

		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool AllowConnect 
		{
			get {
				return (allow & Allow.Connect) != 0;
			}
			set {
				if (value)
					allow |= Allow.Connect;
				else
					allow &= ~Allow.Connect;
			}
		}

		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool AllowEdit 
		{
			get {
				return (allow & Allow.Edit) != 0;
			}
			set {
				if (value)
					allow |= Allow.Edit;
				else
					allow &= ~Allow.Edit;
			}
		}

		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool AllowHide 
		{
			get {
				return (allow & Allow.Hide) != 0;
			}
			set {
				if (value)
					allow |= Allow.Hide;
				else
					allow &= ~Allow.Hide;
			}
		}

		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool AllowMinimize 
		{
			get {
				return (allow & Allow.Minimize) != 0;
			}
			set {
				if (value)
					allow |= Allow.Minimize;
				else
					allow &= ~Allow.Minimize;
			}
		}

		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual bool AllowZoneChange 
		{
			get {
				return (allow & Allow.ZoneChange) != 0;
			}
			set {
				if (value)
					allow |= Allow.ZoneChange;
				else
					allow &= ~Allow.ZoneChange;
			}
		}

		[MonoTODO]
		public virtual string AuthorizationFilter 
		{
			get {
				return auth_filter;
			}
			set {
				auth_filter = value;
			}
		}

		[MonoTODO]
		public virtual string CatalogIconImageUrl 
		{
			get {
				return catalog_icon_url;
			}
			set {
				catalog_icon_url = value;
			}
		}

		[MonoTODO ("why override?")]
		public override PartChromeState ChromeState 
		{
			get {
				return base.ChromeState;
			}
			set {
				base.ChromeState = value;
			}
		}

		[MonoTODO ("why override?")]
		public override PartChromeType ChromeType 
		{
			get {
				return base.ChromeType;
			}
			set {
				base.ChromeType = value;
			}
		}

		[MonoTODO]
		public string ConnectErrorMessage 
		{
			get {
				return "";
			}
		}

		[MonoTODO ("why override?")]
		public override string Description 
		{
			get {
				return base.Description;
			}
			set {
				base.Description = value;
			}
		}

		[MonoTODO]
		/* msdn2 lists this as an override, but it doesn't appear to work with our implementation */
		public /*override*/ ContentDirection Direction 
		{
			get {
			throw new NotImplementedException ();
			}
			set {
			throw new NotImplementedException ();
			}
		}

		public string DisplayTitle 
		{
			get {
				return "Untitled";
			}
		}

		[MonoTODO]
		public virtual WebPartExportMode ExportMode 
		{
			get {
			throw new NotImplementedException ();
			}
			set {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool HasSharedData 
		{
			get {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool HasUserData 
		{
			get {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO("why override?")]
		public override Unit Height 
		{
			get {
				return base.Height;
			}
			set {
				base.Height = value;
			}
		}

		[MonoTODO]
		public virtual WebPartHelpMode HelpMode 
		{
			get {
			throw new NotImplementedException ();
			}
			set {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual string HelpUrl 
		{
			get {
			throw new NotImplementedException ();
			}
			set {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool Hidden 
		{
			get {
			throw new NotImplementedException ();
			}
			set {
			throw new NotImplementedException ();
			}
		}

		public virtual string ImportErrorMessage 
		{
			get {
				return ViewState.GetString("ImportErrorMessage", "Cannot import this Web Part.");
			}
			set {
				ViewState ["ImportErrorMessage"] = value;
			}
		}

		[MonoTODO]
		public bool IsClosed 
		{
			get {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO("not virtual and no setter..")]
		public bool IsShared 
		{
			get {
				return false;
			}
		}

		[MonoTODO("not virtual and no setter..")]
		public bool IsStandalone 
		{
			get {
				return true;
			}
		}

		[MonoTODO]
		public bool IsStatic 
		{
			get {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual string Subtitle 
		{
			get {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO ("why override?")]
		public override string Title 
		{
			get {
				return base.Title;
			}
			set {
				base.Title = value;
			}
		}

		[MonoTODO]
		public virtual string TitleIconImageUrl 
		{
			get {
			throw new NotImplementedException ();
			}
			set {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual string TitleUrl 
		{
			get {
			throw new NotImplementedException ();
			}
			set {
			throw new NotImplementedException ();
			}
		}

		public virtual WebPartVerbCollection Verbs 
		{
			get {
				return verbs;
			}
		}

#if IWebEditableInterface
		[MonoTODO]
		public virtual object WebBrowsableObject 
		{
			get {
			throw new NotImplementedException ();
			}
		}
#endif

#if notyet
		[MonoTODO]
		protected WebPartManager WebPartManager 
		{
			get {
			throw new NotImplementedException ();
			}
		}
#endif

		[MonoTODO ("why override?")]
		public override Unit Width 
		{
			get {
				return base.Width;
			}
			set {
				base.Width = value;
			}
		}

#if notyet
		[MonoTODO]
		public WebPartZoneBase Zone 
		{
			get {
			throw new NotImplementedException ();
			}
		}
#endif

		[MonoTODO]
		public int ZoneIndex 
		{
			get {
			throw new NotImplementedException ();
			}
		}
	}

}

#endif
