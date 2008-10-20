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

		[Flags]
		enum TableItemStyles
		{
			HorizontalAlign = 0x00010000,
			VerticalAlign = 0x00020000,
			Wrap = 0x00040000,
		}

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
				if (!CheckBit ((int) TableItemStyles.HorizontalAlign))
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
				SetBit ((int) TableItemStyles.HorizontalAlign);
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
				if (!CheckBit ((int) TableItemStyles.VerticalAlign))
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
				SetBit ((int) TableItemStyles.VerticalAlign);
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
				if (!CheckBit ((int) TableItemStyles.Wrap))
					return true;
				return (bool) ViewState ["Wrap"];
			}
			set {
				ViewState ["Wrap"] = value;
				SetBit ((int) TableItemStyles.Wrap);
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
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "left", false);
				break;
			case HorizontalAlign.Center:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "center", false);
				break;
			case HorizontalAlign.Right:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "right", false);
				break;
			case HorizontalAlign.Justify:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "justify", false);
				break;
			}

			// note: avoid ToString on the enum
			switch (VerticalAlign) {
			case VerticalAlign.Top:
				writer.AddAttribute (HtmlTextWriterAttribute.Valign, "top", false);
				break;
			case VerticalAlign.Middle:
				writer.AddAttribute (HtmlTextWriterAttribute.Valign, "middle", false);
				break;
			case VerticalAlign.Bottom:
				writer.AddAttribute (HtmlTextWriterAttribute.Valign, "bottom", false);
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

		void Copy (string name, TableItemStyles s, Style source)
		{
			if (source.CheckBit((int) s)) {
				object o = source.ViewState [name];
				if (o != null) {
					ViewState [name] = o;
					SetBit ((int) s);
				}
			}
		}

		public override void CopyFrom (Style s)
		{
			base.CopyFrom (s);
			if (s != null && !s.IsEmpty) {
				Copy ("HorizontalAlign", TableItemStyles.HorizontalAlign, s);
				Copy ("VerticalAlign", TableItemStyles.VerticalAlign, s);
				Copy ("Wrap", TableItemStyles.Wrap, s);
			}
		}

		void Merge (string name, TableItemStyles s, Style source)
		{
			if ((!CheckBit ((int) s)) && (source.CheckBit ((int) s))) {
				object o = source.ViewState [name];
				if (o != null) {
					ViewState [name] = o;
					SetBit ((int) s);
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
					Merge ("HorizontalAlign", TableItemStyles.HorizontalAlign, s);
					Merge ("VerticalAlign", TableItemStyles.VerticalAlign, s);
					Merge ("Wrap", TableItemStyles.Wrap, s);
				}
			}
		}

		public override void Reset ()
		{
			if (CheckBit ((int) TableItemStyles.HorizontalAlign))
				ViewState.Remove ("HorizontalAlign");
			if (CheckBit ((int) TableItemStyles.VerticalAlign))
				ViewState.Remove ("VerticalAlign");
			if (CheckBit ((int) TableItemStyles.Wrap))
				ViewState.Remove ("Wrap");
			// call base at the end because "styles" will reset there
			base.Reset ();
		}
	}
}
