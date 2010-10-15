//
// System.Web.UI.WebControls.ImageMap.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[ParseChildren (true, "HotSpots")]
	[DefaultProperty ("HotSpots")]
	[DefaultEvent ("Click")]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[SupportsEventValidation]
	public class ImageMap: Image, IPostBackEventHandler
	{
		HotSpotCollection spots;
		
		static readonly object ClickEvent = new object();
		
		[Category ("Action")]
		public event ImageMapEventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove { Events.RemoveHandler (ClickEvent, value); }
		}
		
		protected virtual void OnClick (ImageMapEventArgs e)
		{
			if (Events != null) {
				ImageMapEventHandler eh = (ImageMapEventHandler) Events [ClickEvent];
				if (eh!= null)
					eh (this, e);
			}
		}

		// Why override?
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public override bool Enabled {
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}
		
		[DefaultValueAttribute (HotSpotMode.NotSet)]
		public virtual HotSpotMode HotSpotMode {
			get {
				object o = ViewState ["HotSpotMode"];
				return o != null ? (HotSpotMode) o : HotSpotMode.NotSet;
			}
			set { ViewState ["HotSpotMode"] = value; }
		}
		
		[DefaultValueAttribute ("")]
		public virtual string Target {
			get {
				object o = ViewState ["Target"];
				return o != null ? (string) o : String.Empty;
			}
			set { ViewState ["Target"] = value; }
		}

		[NotifyParentPropertyAttribute (true)]
		[PersistenceModeAttribute (PersistenceMode.InnerDefaultProperty)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		public HotSpotCollection HotSpots {
			get {
				if (spots == null) {
					spots = new HotSpotCollection ();
					if (IsTrackingViewState)
						((IStateManager)spots).TrackViewState ();
				}
				return spots;
			}
		}
		
		protected override void TrackViewState ()
		{
			base.TrackViewState ();
			if (spots != null)
				((IStateManager)spots).TrackViewState ();
		}
		
		protected override object SaveViewState ()
		{
			object ob1 = base.SaveViewState ();
			object ob2 = spots != null ? ((IStateManager)spots).SaveViewState () : null;
			
			if (ob1 != null || ob2 != null)
				return new Pair (ob1, ob2);
			else
				return null;
		}
		
		protected override void LoadViewState (object savedState)
		{
			if (savedState == null) {
				base.LoadViewState (null);
				return;
			}
			
			Pair pair = (Pair) savedState;
			base.LoadViewState (pair.First);
			((IStateManager)HotSpots).LoadViewState (pair.Second);
		}

		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			ValidateEvent (UniqueID, eventArgument);
			HotSpot spot = HotSpots [int.Parse (eventArgument)];
			OnClick (new ImageMapEventArgs (spot.PostBackValue));
		}

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
		
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
			if (spots != null && spots.Count > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Usemap, "#ImageMap" + ClientID);
		}
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			base.Render (writer);

			if (spots != null && spots.Count > 0) {
#if NET_4_0				
				bool enabled = Enabled;
#endif
				writer.AddAttribute (HtmlTextWriterAttribute.Id, "ImageMap" + ClientID);
				writer.AddAttribute (HtmlTextWriterAttribute.Name, "ImageMap" + ClientID);
				writer.RenderBeginTag (HtmlTextWriterTag.Map);
				for (int n=0; n<spots.Count; n++) {
					HotSpot spot = spots [n];
					writer.AddAttribute (HtmlTextWriterAttribute.Shape, spot.MarkupName);
					writer.AddAttribute (HtmlTextWriterAttribute.Coords, spot.GetCoordinates ());
					writer.AddAttribute (HtmlTextWriterAttribute.Title, spot.AlternateText);
					writer.AddAttribute (HtmlTextWriterAttribute.Alt, spot.AlternateText);
					if (spot.AccessKey.Length > 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Accesskey, spot.AccessKey);
					if (spot.TabIndex != 0)
						writer.AddAttribute (HtmlTextWriterAttribute.Tabindex, spot.TabIndex.ToString ());
					
					HotSpotMode mode = spot.HotSpotMode != HotSpotMode.NotSet ? spot.HotSpotMode : HotSpotMode;
					switch (mode) {
						case HotSpotMode.Inactive:
							writer.AddAttribute ("nohref", "true", false);
							break;
						case HotSpotMode.Navigate:
							string target = spot.Target.Length > 0 ? spot.Target : Target;
							if (!String.IsNullOrEmpty (target))
								writer.AddAttribute (HtmlTextWriterAttribute.Target, target);
#if NET_4_0
							if (enabled) {
#endif
#if TARGET_J2EE
								string navUrl = ResolveClientUrl (spot.NavigateUrl, String.Compare (target, "_blank", StringComparison.InvariantCultureIgnoreCase) != 0);
#else
								string navUrl = ResolveClientUrl (spot.NavigateUrl);
#endif
								writer.AddAttribute (HtmlTextWriterAttribute.Href, navUrl);
#if NET_4_0
							}
#endif
							break;
						case HotSpotMode.PostBack:
							writer.AddAttribute (HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink (this, n.ToString(), true));
							break;
					}
						
					writer.RenderBeginTag (HtmlTextWriterTag.Area);
					writer.RenderEndTag ();
				}
				writer.RenderEndTag ();
			} 
		}
	}
}

