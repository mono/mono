//
// System.Web.UI.WebControls.BulletedList.cs
//
// Authors:
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
using System.IO;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DesignerAttribute ("System.Web.UI.Design.WebControls.BulletedListDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultEventAttribute ("Click")]
	[DefaultPropertyAttribute ("BulletStyle")]
	[SupportsEventValidation]
	public class BulletedList : ListControl, IPostBackEventHandler {
		
		PostBackOptions postBackOptions;

		[MonoTODO ("we are missing a new style enum, we should be using it")]
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			const string ListStyleType = "list-style-type";
			const string ListStyleImage = "list-style-image";
			
			bool isNumeric = false;
			switch (BulletStyle)
			{
				case BulletStyle.NotSet:
					break;
				
				case BulletStyle.Numbered:
					writer.AddStyleAttribute (ListStyleType, "decimal");
					isNumeric = true;
					break;
				
				case BulletStyle.LowerAlpha:
					writer.AddStyleAttribute (ListStyleType, "lower-alpha");
					isNumeric = true;
					break;
				
				case BulletStyle.UpperAlpha:
					writer.AddStyleAttribute (ListStyleType, "upper-alpha");
					isNumeric = true;
					break;
				
				case BulletStyle.LowerRoman:
					writer.AddStyleAttribute (ListStyleType, "lower-roman");
					isNumeric = true;
					break;
				
				case BulletStyle.UpperRoman:
					writer.AddStyleAttribute (ListStyleType, "upper-roman");
					isNumeric = true;
					break;

				case BulletStyle.Disc:
					writer.AddStyleAttribute (ListStyleType, "disc");
					break;
				
				case BulletStyle.Circle:
					writer.AddStyleAttribute (ListStyleType, "circle");
					break;
				
				case BulletStyle.Square:
					writer.AddStyleAttribute (ListStyleType, "square");
					break;
								
				case BulletStyle.CustomImage:
					writer.AddStyleAttribute (ListStyleImage, "url(" + ResolveClientUrl (BulletImageUrl) + ")");
					break;
			}

			if (isNumeric && FirstBulletNumber != 1)
				writer.AddAttribute ("start", FirstBulletNumber.ToString ());
			
			base.AddAttributesToRender (writer);
		}

		protected virtual void RenderBulletText (ListItem item, int index, HtmlTextWriter writer)
		{
			string text = HttpUtility.HtmlEncode (item.Text);
			
			switch (DisplayMode) {
				case BulletedListDisplayMode.Text:
					if (!item.Enabled) {
						writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled", false);
						writer.RenderBeginTag (HtmlTextWriterTag.Span);
					}
					
					writer.Write (text);
					
					if (!item.Enabled)
						writer.RenderEndTag ();
					
					break;

				case BulletedListDisplayMode.HyperLink:
					if (IsEnabled && item.Enabled) {
						writer.AddAttribute (HtmlTextWriterAttribute.Href, item.Value);
						if (Target.Length > 0)
							writer.AddAttribute(HtmlTextWriterAttribute.Target, this.Target);
						
					}
					else
						writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled", false);
					
					writer.RenderBeginTag (HtmlTextWriterTag.A);
					writer.Write (text);
					writer.RenderEndTag ();
					break;

				case BulletedListDisplayMode.LinkButton:
					if (IsEnabled && item.Enabled)
						writer.AddAttribute (HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackEventReference (GetPostBackOptions (index.ToString (Helpers.InvariantCulture)), true));
					else
						writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled", false);
					writer.RenderBeginTag (HtmlTextWriterTag.A);
					writer.Write (text);
					writer.RenderEndTag ();
					break;
			}
		}

		PostBackOptions GetPostBackOptions (string argument) {
			if (postBackOptions == null) {
				postBackOptions = new PostBackOptions (this);
				postBackOptions.ActionUrl = null;
				postBackOptions.ValidationGroup = null;
				postBackOptions.RequiresJavaScriptProtocol = true;
				postBackOptions.ClientSubmit = true;
				postBackOptions.PerformValidation = CausesValidation && Page != null && Page.AreValidatorsUplevel (ValidationGroup);
				if (postBackOptions.PerformValidation)
					postBackOptions.ValidationGroup = ValidationGroup;
			}
			postBackOptions.Argument = argument;
			return postBackOptions;
		}
		
		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			int idx = 0;
#if NET_2_0
			bool havePage = Page != null;
#endif
			foreach (ListItem i in Items) {
#if NET_2_0
				if (havePage)
					Page.ClientScript.RegisterForEventValidation (UniqueID, i.Value);

				if (i.HasAttributes)
					i.Attributes.AddAttributes (writer);
#endif
				writer.RenderBeginTag (HtmlTextWriterTag.Li);
				this.RenderBulletText (i, idx ++, writer);
				writer.RenderEndTag ();
			}
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			base.Render (writer);
		}
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
			
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			ValidateEvent (UniqueID, eventArgument);
			if (CausesValidation)
				Page.Validate (ValidationGroup);
			
			this.OnClick (new BulletedListEventArgs (int.Parse (eventArgument, Helpers.InvariantCulture)));
		}
			
	    [BrowsableAttribute (false)]
    	[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public override bool AutoPostBack { 
			get { return base.AutoPostBack; }
			set { throw new NotSupportedException (String.Format ("This property is not supported in {0}", GetType ())); }
		}
		
		[Bindable (false)]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public override int SelectedIndex {
			get { return -1; }
			set { throw new NotSupportedException (String.Format ("This property is not supported in {0}", GetType ())); }
		}
		
	    [EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public override ListItem SelectedItem {
			get { return null; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Bindable (false)]
		public override string SelectedValue
		{
			get { return string.Empty; }
			set { throw new NotSupportedException (); }
		}
		
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		public virtual string BulletImageUrl {
			get { return ViewState.GetString ("BulletImageUrl", ""); }
			set { ViewState ["BulletImageUrl"] = value; }
		}
		
	    [DefaultValueAttribute (BulletStyle.NotSet)]
		public virtual BulletStyle BulletStyle {
			get { return (BulletStyle) ViewState.GetInt ("BulletStyle", (int) BulletStyle.NotSet); }
			set {
				if ((int) value < 0 || (int) value > 9)
					throw new ArgumentOutOfRangeException ("value");

				ViewState ["BulletStyle"] = value;
			}
		}
		
		public override ControlCollection Controls { get { return new EmptyControlCollection (this); } }
		
	    [DefaultValueAttribute (BulletedListDisplayMode.Text)]
		public virtual BulletedListDisplayMode DisplayMode {
			get { return (BulletedListDisplayMode) ViewState.GetInt ("DisplayMode", (int)BulletedListDisplayMode.Text); }
			set {
				if ((int) value < 0 || (int) value > 2)
					throw new ArgumentOutOfRangeException ("value");

				ViewState ["DisplayMode"] = value;
			}
		}
		
	    [DefaultValueAttribute (1)]
		public virtual int FirstBulletNumber {
			get { return ViewState.GetInt ("FirstBulletNumber", 1); }
			set { ViewState ["FirstBulletNumber"] = value; }
		}
		

		protected override HtmlTextWriterTag TagKey {
			get {
				switch (BulletStyle) {			
					case BulletStyle.Numbered:
					case BulletStyle.LowerAlpha:
					case BulletStyle.UpperAlpha:
					case BulletStyle.LowerRoman:
					case BulletStyle.UpperRoman:
						return HtmlTextWriterTag.Ol;
					
					case BulletStyle.NotSet:
					case BulletStyle.Disc:
					case BulletStyle.Circle:
					case BulletStyle.Square:
					case BulletStyle.CustomImage:
					default:
						return HtmlTextWriterTag.Ul;
				}
			}
		}
		
		[DefaultValueAttribute ("")]
		[TypeConverter (typeof (TargetConverter))]
		public virtual string Target {
			get { return ViewState.GetString ("Target", String.Empty); }
			set { ViewState ["Target"] = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text
		{
			get { return string.Empty; }
			set { throw new NotSupportedException (); }
		}
		
		
		static readonly object ClickEvent = new object ();
		public event BulletedListEventHandler Click
		{
			add {
				Events.AddHandler (ClickEvent, value);
			}
			remove {
				Events.RemoveHandler (ClickEvent, value);
			}
		}
		
		protected virtual void OnClick (BulletedListEventArgs e)
		{
			if (Events != null) {
				BulletedListEventHandler eh = (BulletedListEventHandler) (Events [ClickEvent]);
				if (eh != null)
					eh (this, e);
			}
		}
	}
}
#endif
