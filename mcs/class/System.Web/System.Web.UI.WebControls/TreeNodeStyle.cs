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
		private static int CHILD_PADD = (0x01 << 16);
		private static int HORZ_PADD = (0x01 << 17);
		private static int IMG_URL = (0x01 << 18);
		private static int SPACING = (0x01 << 19);
		private static int VERT_PADD = (0x01 << 20);
		
		[DefaultValue ("")]
		[UrlProperty]
		[NotifyParentProperty (true)]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string ImageUrl {
			get {
				if(IsSet(IMG_URL))
					return (string)(ViewState["ImageUrl"]);
				return String.Empty;
			}
			set {
				if(value == null)
					throw new ArgumentNullException("value");
				ViewState["ImageUrl"] = value;
				Set(IMG_URL);
			}
		}

		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public int ChildNodesPadding {
			get {
				if(IsSet(CHILD_PADD))
					return (int)(ViewState["ChildNodesPadding"]);
				return 0;
			}
			set {
				ViewState["ChildNodesPadding"] = value;
				Set(CHILD_PADD);
			}
		}

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
		public int NodeSpacing {
			get {
				if(IsSet(SPACING))
					return (int)(ViewState["NodeSpacing"]);
				return 0;
			}
			set {
				ViewState["NodeSpacing"] = value;
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
			TreeNodeStyle from = s as TreeNodeStyle;
			if (from == null)
				return;

			if (from.IsSet (CHILD_PADD))
				ChildNodesPadding = from.ChildNodesPadding;

			if (from.IsSet (HORZ_PADD))
				HorizontalPadding = from.HorizontalPadding;

			if (from.IsSet (IMG_URL))
				ImageUrl = from.ImageUrl;

			if (from.IsSet (SPACING))
				NodeSpacing = from.NodeSpacing;

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

				TreeNodeStyle with = s as TreeNodeStyle;
				if (with == null) return;
				
				if (with.IsSet(CHILD_PADD) && !IsSet(CHILD_PADD)) {
					ChildNodesPadding = with.ChildNodesPadding;
				}
				if (with.IsSet(HORZ_PADD) && !IsSet(HORZ_PADD)) {
					HorizontalPadding = with.HorizontalPadding;
				}
				if (with.IsSet(IMG_URL) && !IsSet(IMG_URL)) {
					ImageUrl = with.ImageUrl;
				}
				if (with.IsSet(SPACING) && !IsSet(SPACING)) {
					NodeSpacing = with.NodeSpacing;
				}
				if (with.IsSet(VERT_PADD) && !IsSet(VERT_PADD)) {
					VerticalPadding = with.VerticalPadding;
				}
			}
		}

		public override void Reset()
		{
			if(IsSet(CHILD_PADD))
				ViewState.Remove("ChildNodesPadding");
			if(IsSet(HORZ_PADD))
				ViewState.Remove("HorizontalPadding");
			if(IsSet(IMG_URL))
				ViewState.Remove("ImageUrl");
			if(IsSet(SPACING))
				ViewState.Remove("NodeSpacing");
			if(IsSet(VERT_PADD))
				ViewState.Remove("VerticalPadding");
			base.Reset();
		}
	}
}

#endif
