//
// System.Web.UI.WebControls.TreeNodeStyle.cs
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
	public sealed class TreeNodeStyle: Style
	{
		const string CHILD_PADD = "ChildNodesPadding";
		const string HORZ_PADD = "HorizontalPadding";
		const string IMG_URL = "ImageUrl";
		const string SPACING = "NodeSpacing";
		const string VERT_PADD = "VerticalPadding";

		[Flags]
		enum TreeNodeStyles
		{
			ChildNodesPadding = 0x00010000,
			HorizontalPadding = 0x00020000,
			ImageUrl = 0x00040000,
			NodeSpacing = 0x00080000,
			VerticalPadding = 0x00100000,
		}
		
		public TreeNodeStyle ()
			: base ()
		{
		}

		public TreeNodeStyle (StateBag bag)
			: base (bag)
		{
		}

		[DefaultValue ("")]
		[UrlProperty]
		[NotifyParentProperty (true)]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string ImageUrl {
			get {
				if (!CheckBit ((int) TreeNodeStyles.ImageUrl))
					return String.Empty;
				return ViewState.GetString (IMG_URL, String.Empty);
			}
			set {
				if(value == null)
					throw new ArgumentNullException("value");
				ViewState [IMG_URL] = value;
				SetBit ((int) TreeNodeStyles.ImageUrl);
			}
		}

		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public Unit ChildNodesPadding {
			get {
				if (!CheckBit ((int) TreeNodeStyles.ChildNodesPadding))
					return 0;
				return ViewState [CHILD_PADD] == null ? 0 : (Unit) ViewState [CHILD_PADD];
			}
			set {
				ViewState [CHILD_PADD] = value;
				SetBit ((int) TreeNodeStyles.ChildNodesPadding);
			}
		}

		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public Unit HorizontalPadding {
			get {
				if (!CheckBit ((int) TreeNodeStyles.HorizontalPadding))
					return 0;
				return ViewState [HORZ_PADD] == null ? 0 : (Unit) ViewState [HORZ_PADD];
			}
			set {
				ViewState[HORZ_PADD] = value;
				SetBit ((int) TreeNodeStyles.HorizontalPadding);
			}
		}

		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public Unit VerticalPadding {
			get {
				if (!CheckBit ((int) TreeNodeStyles.VerticalPadding))
					return 0;
				return ViewState [VERT_PADD] == null ? 0 : (Unit) ViewState [VERT_PADD];
			}
			set {
				ViewState [VERT_PADD] = value;
				SetBit ((int) TreeNodeStyles.VerticalPadding);
			}
		}

		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public Unit NodeSpacing {
			get {
				if (!CheckBit ((int) TreeNodeStyles.NodeSpacing))
					return 0;
				return ViewState [SPACING] == null ? 0 : (Unit) ViewState [SPACING];
			}
			set {
				ViewState [SPACING] = value;
				SetBit ((int) TreeNodeStyles.NodeSpacing);
			}
		}
		
		public override void CopyFrom (Style s)
		{
			if (s == null || s.IsEmpty)
				return;

			base.CopyFrom (s);
			TreeNodeStyle from = s as TreeNodeStyle;
			if (from == null)
				return;

			if (from.CheckBit ((int) TreeNodeStyles.ChildNodesPadding))
				ChildNodesPadding = from.ChildNodesPadding;

			if (from.CheckBit ((int) TreeNodeStyles.HorizontalPadding))
				HorizontalPadding = from.HorizontalPadding;

			if (from.CheckBit ((int) TreeNodeStyles.ImageUrl))
				ImageUrl = from.ImageUrl;

			if (from.CheckBit ((int) TreeNodeStyles.NodeSpacing))
				NodeSpacing = from.NodeSpacing;

			if (from.CheckBit ((int) TreeNodeStyles.VerticalPadding))
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

				TreeNodeStyle with = s as TreeNodeStyle;
				if (with == null) return;
				
				if (with.CheckBit ((int) TreeNodeStyles.ChildNodesPadding) && !CheckBit ((int) TreeNodeStyles.ChildNodesPadding)) {
					ChildNodesPadding = with.ChildNodesPadding;
				}
				if (with.CheckBit ((int) TreeNodeStyles.HorizontalPadding) && !CheckBit ((int) TreeNodeStyles.HorizontalPadding)) {
					HorizontalPadding = with.HorizontalPadding;
				}
				if (with.CheckBit ((int) TreeNodeStyles.ImageUrl) && !CheckBit ((int) TreeNodeStyles.ImageUrl)) {
					ImageUrl = with.ImageUrl;
				}
				if (with.CheckBit ((int) TreeNodeStyles.NodeSpacing) && !CheckBit ((int) TreeNodeStyles.NodeSpacing)) {
					NodeSpacing = with.NodeSpacing;
				}
				if (with.CheckBit ((int) TreeNodeStyles.VerticalPadding) && !CheckBit ((int) TreeNodeStyles.VerticalPadding)) {
					VerticalPadding = with.VerticalPadding;
				}
			}
		}

		public override void Reset()
		{
			if (CheckBit ((int) TreeNodeStyles.ChildNodesPadding))
				ViewState.Remove(CHILD_PADD);
			if (CheckBit ((int) TreeNodeStyles.HorizontalPadding))
				ViewState.Remove(HORZ_PADD);
			if (CheckBit ((int) TreeNodeStyles.ImageUrl))
				ViewState.Remove(IMG_URL);
			if (CheckBit ((int) TreeNodeStyles.NodeSpacing))
				ViewState.Remove(SPACING);
			if (CheckBit ((int) TreeNodeStyles.VerticalPadding))
				ViewState.Remove(VERT_PADD);
			base.Reset();
		}

		protected override void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver) {
			base.FillStyleAttributes (attributes, urlResolver);
			if (CheckBit ((int) TreeNodeStyles.HorizontalPadding)) {
				attributes.Add (HtmlTextWriterStyle.PaddingLeft, HorizontalPadding.ToString ());
				attributes.Add (HtmlTextWriterStyle.PaddingRight, HorizontalPadding.ToString ());
			}
			if (CheckBit ((int) TreeNodeStyles.VerticalPadding)) {
				attributes.Add (HtmlTextWriterStyle.PaddingTop, VerticalPadding.ToString ());
				attributes.Add (HtmlTextWriterStyle.PaddingBottom, VerticalPadding.ToString ());
			}
		}
	}
}

#endif
