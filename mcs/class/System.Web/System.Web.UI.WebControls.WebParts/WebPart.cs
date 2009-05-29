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


		WebPartVerbCollection verbs = new WebPartVerbCollection();
		Allow allow;
		string auth_filter;
		string catalog_icon_url;
		WebPartExportMode exportMode = WebPartExportMode.None;
		string	titleIconImageUrl,	 
				titleUrl,			
				helpUrl;
		bool isStatic, hidden, isClosed, hasSharedData, hasUserData;
		WebPartHelpMode helpMode = WebPartHelpMode.Navigate;
		int zoneIndex ;

		protected WebPart ()
		{
			verbs = new WebPartVerbCollection();
			allow = Allow.Close | Allow.Connect | Allow.Edit | Allow.Hide | Allow.Minimize | Allow.ZoneChange;
			auth_filter = "";
			catalog_icon_url = "";
			titleIconImageUrl	= string.Empty;
			titleUrl		= string.Empty;
			helpUrl			= string.Empty;
			isStatic		= false;
			hasUserData		= false;
			hasSharedData	= false;
			hidden = false;
			isClosed = false;
		}

#if IWebEditableInterface
		[MonoTODO("Not implemented")]
		public virtual EditorPartCollection CreateEditorParts ()
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO("Not implemented")]
		protected void SetPersonalizationDirty ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
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

		internal void SetZoneIndex (int index)
		{
			zoneIndex = index;
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

		public virtual string AuthorizationFilter 
		{
			get {
				return auth_filter;
			}
			set {
				auth_filter = value;
			}
		}

		public virtual string CatalogIconImageUrl 
		{
			get {
				return catalog_icon_url;
			}
			set {
				catalog_icon_url = value;
			}
		}

		public override PartChromeState ChromeState 
		{
			get {
				return base.ChromeState;
			}
			set {
				base.ChromeState = value;
			}
		}

		public override PartChromeType ChromeType 
		{
			get {
				return base.ChromeType;
			}
			set {
				base.ChromeType = value;
			}
		}

		[MonoTODO("Not implemented")]
		public string ConnectErrorMessage 
		{
			get {
				return "";
			}
		}

		public override string Description 
		{
			get {
				return base.Description;
			}
			set {
				base.Description = value;
			}
		}

		[MonoTODO("Not implemented")]
		/* msdn2 lists this as an override, but it doesn't appear to work with our implementation */
		public override ContentDirection Direction 
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

		public virtual WebPartExportMode ExportMode 
		{
			get {
				return exportMode;
			}
			set {
				exportMode = value;
			}
		}

		public bool HasSharedData 
		{
			get {
				return hasSharedData;
			}
		}

		public bool HasUserData 
		{
			get {
				return hasUserData;
			}
		}

		public override Unit Height 
		{
			get {
				return base.Height;
			}
			set {
				base.Height = value;
			}
		}

		public virtual WebPartHelpMode HelpMode 
		{
			get {
				return helpMode;
			}
			set {
				helpMode = value;
			}
		}

		public virtual string HelpUrl 
		{
			get {
				return helpUrl;
			}
			set {
				helpUrl = value;
			}
		}

		public virtual bool Hidden 
		{
			get {
				return hidden;
			}
			set {
				hidden = value;
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

		public bool IsClosed 
		{
			get {
				return isClosed;
			}
		}

		public bool IsShared 
		{
			get {
				return false;
			}
		}

		public bool IsStandalone 
		{
			get {
				return true;
			}
		}

		public bool IsStatic 
		{
			get {
				return isStatic;
			}
		}

		public virtual string Subtitle 
		{
			get {
				return string.Empty;
			}
		}

		public override string Title 
		{
			get {
				return base.Title;
			}
			set {
				base.Title = value;
			}
		}

		public virtual string TitleIconImageUrl 
		{
			get {
				return titleIconImageUrl;
			}
			set {
				titleIconImageUrl = value;
			}
		}

		public virtual string TitleUrl 
		{
			get {
				return titleUrl;
			}
			set {
				titleUrl = value;
			}
		}

		public virtual WebPartVerbCollection Verbs 
		{
			get {
				return verbs;
			}
		}

#if IWebEditableInterface
		[MonoTODO("Not implemented")]
		public virtual object WebBrowsableObject 
		{
			get {
				throw new NotImplementedException ();
			}
		}
#endif

#if notyet
		[MonoTODO("Not implemented")]
		protected WebPartManager WebPartManager 
		{
			get {
				throw new NotImplementedException ();
			}
		}
#endif

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
		[MonoTODO("Not implemented")]
		public WebPartZoneBase Zone 
		{
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		public int ZoneIndex 
		{
			get {
				return zoneIndex;
			}
		}
	}

}

#endif
