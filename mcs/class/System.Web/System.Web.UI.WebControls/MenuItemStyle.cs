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
	public class MenuItemStyle: Style
	{
		private static int HORZ_PADD = (0x01 << 16);
		private static int SPACING = (0x01 << 17);
		private static int VERT_PADD = (0x01 << 18);
		
		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public int HorizontalPadding {
			get {
				if(IsSet(HORZ_PADD))
					return (int)(ViewState["HorizontalPadding"]);
				return 0;
			}
			set {
				ViewState["HorizontalPadding"] = value;
				Set(HORZ_PADD);
			}
		}

		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public int VerticalPadding {
			get {
				if(IsSet(VERT_PADD))
					return (int)(ViewState["VerticalPadding"]);
				return 0;
			}
			set {
				ViewState["VerticalPadding"] = value;
				Set(VERT_PADD);
			}
		}

		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public int ItemSpacing {
			get {
				if(IsSet(SPACING))
					return (int)(ViewState["ItemSpacing"]);
				return 0;
			}
			set {
				ViewState["ItemSpacing"] = value;
				Set(SPACING);
			}
		}

		protected internal override bool IsEmpty {
			get { return base.IsEmpty; }
		}
		
		public override void CopyFrom (Style s)
		{
			if (s == null || s.IsEmpty)
				return;

			base.CopyFrom (s);
			MenuItemStyle from = s as MenuItemStyle;
			if (from == null)
				return;

			if (from.IsSet (HORZ_PADD))
				HorizontalPadding = from.HorizontalPadding;

			if (from.IsSet (SPACING))
				ItemSpacing = from.ItemSpacing;

			if (from.IsSet (VERT_PADD))
				VerticalPadding = from.VerticalPadding;
		}
		
		public override void MergeWith(Style s)
		{
			if(s != null && !s.IsEmpty)
			{
				if (IsEmpty) {
					CopyFrom (s);
					return;
				}
				base.MergeWith(s);

				MenuItemStyle with = s as MenuItemStyle;
				if (with == null) return;
				
				if (with.IsSet(HORZ_PADD) && !IsSet(HORZ_PADD)) {
					HorizontalPadding = with.HorizontalPadding;
				}
				if (with.IsSet(SPACING) && !IsSet(SPACING)) {
					ItemSpacing = with.ItemSpacing;
				}
				if (with.IsSet(VERT_PADD) && !IsSet(VERT_PADD)) {
					VerticalPadding = with.VerticalPadding;
				}
			}
		}

		public override void Reset()
		{
			if(IsSet(HORZ_PADD))
				ViewState.Remove("HorizontalPadding");
			if(IsSet(SPACING))
				ViewState.Remove("ItemSpacing");
			if(IsSet(VERT_PADD))
				ViewState.Remove("VerticalPadding");
			base.Reset();
		}
		
		protected override void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver)
		{
			base.FillStyleAttributes (attributes, urlResolver);
			if (IsSet (HORZ_PADD)) {
				attributes.Add (HtmlTextWriterStyle.PaddingLeft, HorizontalPadding.ToString () + "px");
				attributes.Add (HtmlTextWriterStyle.PaddingRight, HorizontalPadding.ToString () + "px");
			}
			if (IsSet (VERT_PADD)) {
				attributes.Add (HtmlTextWriterStyle.PaddingTop, VerticalPadding.ToString () + "px");
				attributes.Add (HtmlTextWriterStyle.PaddingBottom, VerticalPadding.ToString () + "px");
			}
		}
	}
}

#endif
