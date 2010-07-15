//
// System.Web.UI.WebControls.MenuItemStyle.cs
//
// Authors:
//	Igor Zelmanovich (igorz@mainsoft.com)
//
// (C) 2007 Mainsoft, Inc (http://www.mainsoft.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public class PanelStyle : Style
	{
		[Flags]
		enum PanelStyles
		{
			BackImageUrl = 0x00010000,
			Direction = 0x00020000,
			HorizontalAlign = 0x00040000,
			ScrollBars = 0x00080000,
			Wrap = 0x00100000,
		}

		public PanelStyle (StateBag bag)
			: base (bag)
		{
		}

		[DefaultValue ("")]
		[UrlProperty]
		public virtual string BackImageUrl {
			get {
				if (!CheckBit ((int) PanelStyles.BackImageUrl))
					return String.Empty;

				return ViewState.GetString ("BackImageUrl", String.Empty);
			}

			set {
				ViewState ["BackImageUrl"] = value;
				SetBit ((int) PanelStyles.BackImageUrl);
			}
		}

		[DefaultValue (ContentDirection.NotSet)]
		public virtual ContentDirection Direction {
			get {
				if (!CheckBit ((int) PanelStyles.Direction))
					return ContentDirection.NotSet;

				return (ContentDirection) ViewState ["Direction"];
			}
			set {
				ViewState ["Direction"] = value;
				SetBit ((int) PanelStyles.Direction);
			}
		}

		[DefaultValue (HorizontalAlign.NotSet)]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if (!CheckBit ((int) PanelStyles.HorizontalAlign))
					return HorizontalAlign.NotSet;

				return (HorizontalAlign) ViewState ["HorizontalAlign"];
			}
			set {
				ViewState ["HorizontalAlign"] = value;
				SetBit ((int) PanelStyles.HorizontalAlign);
			}
		}

		[DefaultValue (ScrollBars.None)]
		public virtual ScrollBars ScrollBars {
			get {
				if (!CheckBit ((int) PanelStyles.ScrollBars))
					return ScrollBars.None;

				return (ScrollBars) ViewState ["ScrollBars"];
			}
			set {
				ViewState ["ScrollBars"] = value;
				SetBit ((int) PanelStyles.ScrollBars);
			}
		}

		[DefaultValue (true)]
		public virtual bool Wrap {
			get {
				if (!CheckBit ((int) PanelStyles.Wrap))
					return true;

				return (bool) ViewState ["Wrap"];
			}
			set {
				ViewState ["Wrap"] = value;
				SetBit ((int) PanelStyles.Wrap);
			}
		}

		public override void CopyFrom (Style s)
		{
			if ((s == null) || s.IsEmpty)
				return;

			base.CopyFrom (s);

			PanelStyle ps = s as PanelStyle;
			if (ps == null)
				return;

			if (s.CheckBit ((int) PanelStyles.BackImageUrl)) {
				this.BackImageUrl = ps.BackImageUrl;
			}
			if (s.CheckBit ((int) PanelStyles.Direction)) {
				this.Direction = ps.Direction;
			}
			if (s.CheckBit ((int) PanelStyles.HorizontalAlign)) {
				this.HorizontalAlign = ps.HorizontalAlign;
			}
			if (s.CheckBit ((int) PanelStyles.ScrollBars)) {
				this.ScrollBars = ps.ScrollBars;
			}
			if (s.CheckBit ((int) PanelStyles.Wrap)) {
				this.Wrap = ps.Wrap;
			}
		}

		public override void MergeWith (Style s)
		{
			if ((s == null) || (s.IsEmpty))
				return;

			base.MergeWith (s);

			PanelStyle ps = s as PanelStyle;
			if (ps == null)
				return;

			if (!CheckBit ((int) PanelStyles.BackImageUrl) && s.CheckBit ((int) PanelStyles.BackImageUrl)) {
				this.BackImageUrl = ps.BackImageUrl;
			}
			if (!CheckBit ((int) PanelStyles.Direction) && s.CheckBit ((int) PanelStyles.Direction)) {
				this.Direction = ps.Direction;
			}
			if (!CheckBit ((int) PanelStyles.HorizontalAlign) && s.CheckBit ((int) PanelStyles.HorizontalAlign)) {
				this.HorizontalAlign = ps.HorizontalAlign;
			}
			if (!CheckBit ((int) PanelStyles.ScrollBars) && s.CheckBit ((int) PanelStyles.ScrollBars)) {
				this.ScrollBars = ps.ScrollBars;
			}
			if (!CheckBit ((int) PanelStyles.Wrap) && s.CheckBit ((int) PanelStyles.Wrap)) {
				this.Wrap = ps.Wrap;
			}
		}

		public override void Reset ()
		{
			base.Reset ();

			ViewState.Remove ("BackImageUrl");
			ViewState.Remove ("Direction");
			ViewState.Remove ("HorizontalAlign");
			ViewState.Remove ("ScrollBars");
			ViewState.Remove ("Wrap");
		}
	}
}

#endif
