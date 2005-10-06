//
// System.Web.UI.WebControls.TableItemStyle.cs
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
using System.Globalization;
using System.Security.Permissions;
using System.Web.UI;

namespace System.Web.UI.WebControls {

	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class TableItemStyle : Style {

		public TableItemStyle ()
		{
		}

		public TableItemStyle (StateBag bag)
			: base (bag)
		{
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (HorizontalAlign.NotSet)]
		[NotifyParentProperty (true)]
		[WebSysDescription ("")]
		[WebCategory("Layout")]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if ((styles & Styles.HorizontalAlign) == 0)
					return HorizontalAlign.NotSet;
				return (HorizontalAlign) ViewState ["HorizontalAlign"];
			}
			set {
				// avoid reflection
				if ((value < HorizontalAlign.NotSet) || (value > HorizontalAlign.Justify)) {
					// LAMESPEC: documented as ArgumentException
					throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid HorizontalAlign value."));
				}
				ViewState ["HorizontalAlign"] = value;
				styles |= Styles.HorizontalAlign;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (VerticalAlign.NotSet)]
		[NotifyParentProperty (true)]
		[WebSysDescription ("")]
		[WebCategory("Layout")]
		public virtual VerticalAlign VerticalAlign {
			get {
				if ((styles & Styles.VerticalAlign) == 0)
					return VerticalAlign.NotSet;
				return (VerticalAlign) ViewState ["VerticalAlign"];
			}
			set {
				// avoid reflection
				if ((value < VerticalAlign.NotSet) || (value > VerticalAlign.Bottom)) {
					// LAMESPEC: documented as ArgumentException
					throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid VerticalAlign value."));
				}
				ViewState ["VerticalAlign"] = value;
				styles |= Styles.VerticalAlign;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (true)]
		[NotifyParentProperty (true)]
		[WebSysDescription ("")]
		[WebCategory("Layout")]
		public virtual bool Wrap {
			get {
				if ((styles & Styles.Wrap) == 0)
					return true;
				return (bool) ViewState ["Wrap"];
			}
			set {
				ViewState ["Wrap"] = value;
					styles |= Styles.Wrap;
			}
		}


		public override void AddAttributesToRender (HtmlTextWriter writer, WebControl owner)
		{
			base.AddAttributesToRender (writer, owner);
			if (writer == null)
				return;

			// note: avoid ToString on the enum
			switch (HorizontalAlign) {
			case HorizontalAlign.Left:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "left");
				break;
			case HorizontalAlign.Center:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "center");
				break;
			case HorizontalAlign.Right:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "right");
				break;
			case HorizontalAlign.Justify:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "justify");
				break;
			}

			// note: avoid ToString on the enum
			switch (VerticalAlign) {
			case VerticalAlign.Top:
				writer.AddAttribute (HtmlTextWriterAttribute.Valign, "top");
				break;
			case VerticalAlign.Middle:
				writer.AddAttribute (HtmlTextWriterAttribute.Valign, "middle");
				break;
			case VerticalAlign.Bottom:
				writer.AddAttribute (HtmlTextWriterAttribute.Valign, "bottom");
				break;
			}

			if (!Wrap) {
#if NET_2_0
				writer.AddStyleAttribute (HtmlTextWriterStyle.WhiteSpace, "nowrap");
#else
				writer.AddAttribute (HtmlTextWriterAttribute.Nowrap, "nowrap");
#endif
			}
		}

		private void Copy (string name, Styles s, Style source)
		{
			if ((source.styles & s) != 0) {
				object o = source.ViewState [name];
				if (o != null) {
					ViewState [name] = o;
					styles |= s;
				}
			}
		}

		public override void CopyFrom (Style s)
		{
			base.CopyFrom (s);
			if (s != null && !s.IsEmpty) {
				Copy ("HorizontalAlign", Styles.HorizontalAlign, s);
				Copy ("VerticalAlign", Styles.VerticalAlign, s);
				Copy ("Wrap", Styles.Wrap, s);
			}
		}

		private void Merge (string name, Styles s, Style source)
		{
			if ((styles & s) == 0 && (source.styles & s) != 0) {
				object o = source.ViewState [name];
				if (o != null) {
					ViewState [name] = o;
					styles |= s;
				}
			}
		}

		public override void MergeWith (Style s)
		{
			// if we're empty then it's like a copy
			if (IsEmpty) {
				CopyFrom (s);
			} else {
				base.MergeWith (s);
				if (s != null) {
					Merge ("HorizontalAlign", Styles.HorizontalAlign, s);
					Merge ("VerticalAlign", Styles.VerticalAlign, s);
					Merge ("Wrap", Styles.Wrap, s);
				}
			}
		}

		public override void Reset ()
		{
			if ((styles & Styles.HorizontalAlign) != 0)
				ViewState.Remove ("HorizontalAlign");
			if ((styles & Styles.VerticalAlign) != 0)
				ViewState.Remove ("VerticalAlign");
			if ((styles & Styles.Wrap) != 0)
				ViewState.Remove ("Wrap");
			// call base at the end because "styles" will reset there
			base.Reset ();
		}

		internal override void LoadViewStateInternal()
		{
			if (viewstate["VerticalAlign"] != null) {
				styles |= Styles.VerticalAlign;
			}
			if (viewstate["Wrap"] != null) {
				styles |= Styles.Wrap;
			}

			base.LoadViewStateInternal();
		}
	}
}
