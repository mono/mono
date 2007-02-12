//
// System.Web.UI.WebControls.MenuItemStyle.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
	public sealed class MenuItemStyle: Style
	{
		[Flags]
		enum MenuItemStyles
		{
			HorizontalPadding = 0x00010000,
			VerticalPadding = 0x00020000,
			ItemSpacing = 0x00040000,
		}

		public MenuItemStyle ()
			: base ()
		{
		}

		public MenuItemStyle (StateBag bag)
			: base (bag)
		{
		}

		[DefaultValue (typeof (Unit), "")]
		[NotifyParentProperty (true)]
		public Unit HorizontalPadding {
			get {
				if (CheckBit ((int) MenuItemStyles.HorizontalPadding))
					return (Unit) (ViewState ["HorizontalPadding"]);
				return Unit.Empty;
			}
			set {
				ViewState["HorizontalPadding"] = value;
				SetBit ((int) MenuItemStyles.HorizontalPadding);
			}
		}

		[DefaultValue (typeof (Unit), "")]
		[NotifyParentProperty (true)]
		public Unit VerticalPadding {
			get {
				if (CheckBit ((int) MenuItemStyles.VerticalPadding))
					return (Unit) (ViewState ["VerticalPadding"]);
				return Unit.Empty;
			}
			set {
				ViewState["VerticalPadding"] = value;
				SetBit ((int) MenuItemStyles.VerticalPadding);
			}
		}

		[DefaultValue (typeof (Unit), "")]
		[NotifyParentProperty (true)]
		public Unit ItemSpacing {
			get {
				if (CheckBit ((int) MenuItemStyles.ItemSpacing))
					return (Unit) (ViewState ["ItemSpacing"]);
				return Unit.Empty;
			}
			set {
				ViewState["ItemSpacing"] = value;
				SetBit ((int) MenuItemStyles.ItemSpacing);
			}
		}

		public override void CopyFrom (Style s)
		{
			if (s == null || s.IsEmpty)
				return;

			base.CopyFrom (s);
			MenuItemStyle from = s as MenuItemStyle;
			if (from == null)
				return;

			if (from.CheckBit ((int) MenuItemStyles.HorizontalPadding))
				HorizontalPadding = from.HorizontalPadding;

			if (from.CheckBit ((int) MenuItemStyles.ItemSpacing))
				ItemSpacing = from.ItemSpacing;

			if (from.CheckBit ((int) MenuItemStyles.VerticalPadding))
				VerticalPadding = from.VerticalPadding;
		}
		
		public override void MergeWith(Style s)
		{
			if ((s == null) || (s.IsEmpty))
				return;

			base.MergeWith (s);
			MenuItemStyle with = s as MenuItemStyle;
			if (with == null)
				return;

			if (!CheckBit ((int) MenuItemStyles.HorizontalPadding) && with.CheckBit ((int) MenuItemStyles.HorizontalPadding))
				HorizontalPadding = with.HorizontalPadding;

			if (!CheckBit ((int) MenuItemStyles.ItemSpacing) && with.CheckBit ((int) MenuItemStyles.ItemSpacing))
				ItemSpacing = with.ItemSpacing;

			if (!CheckBit ((int) MenuItemStyles.VerticalPadding) && with.CheckBit ((int) MenuItemStyles.VerticalPadding))
				VerticalPadding = with.VerticalPadding;
				
		}

		public override void Reset()
		{
			ViewState.Remove ("HorizontalPadding");
			ViewState.Remove ("ItemSpacing");
			ViewState.Remove ("VerticalPadding");
			base.Reset();
		}
		
		protected override void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver)
		{
			base.FillStyleAttributes (attributes, urlResolver);
			if (CheckBit ((int) MenuItemStyles.HorizontalPadding)) {
				attributes.Add (HtmlTextWriterStyle.PaddingLeft, HorizontalPadding.ToString ());
				attributes.Add (HtmlTextWriterStyle.PaddingRight, HorizontalPadding.ToString ());
			}
			if (CheckBit ((int) MenuItemStyles.VerticalPadding)) {
				attributes.Add (HtmlTextWriterStyle.PaddingTop, VerticalPadding.ToString ());
				attributes.Add (HtmlTextWriterStyle.PaddingBottom, VerticalPadding.ToString ());
			}
		}
	}
}

#endif
