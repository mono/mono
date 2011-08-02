//
// Authors:
//	Marek Habersack <grendel@twistedcode.net>
//
// (C) 2010 Novell, Inc (http://novell.com)
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace System.Web.UI.WebControls
{
	sealed class MenuListRenderer : BaseMenuRenderer
	{
		bool haveDynamicPopOut;
		
		public override HtmlTextWriterTag Tag {
			get { return HtmlTextWriterTag.Div; }
			
		}
		
		public MenuListRenderer (Menu owner)
			: base (owner)
		{
		}

		public override void PreRender (Page page, HtmlHead head, ClientScriptManager csm, string cmenu, StringBuilder script)
		{
			Menu owner = Owner;
			script.AppendFormat ("new Sys.WebForms.Menu ({{ element: '{0}', disappearAfter: {1}, orientation: '{2}', tabIndex: {3}, disabled: {4} }});",
					     owner.ClientID,
					     ClientScriptManager.GetScriptLiteral (owner.DisappearAfter),
					     owner.Orientation.ToString ().ToLowerInvariant (),
					     ClientScriptManager.GetScriptLiteral (owner.TabIndex),
					     (!owner.Enabled).ToString ().ToLowerInvariant ());

			Type mt = typeof (Menu);
			if (!csm.IsClientScriptIncludeRegistered (mt, "MenuModern.js")) {
				string url = csm.GetWebResourceUrl (mt, "MenuModern.js");
				csm.RegisterClientScriptInclude (mt, "MenuModern.js", url);
			}
			
			if (!owner.IncludeStyleBlock)
				return;
			
			if (head == null)
				throw new InvalidOperationException ("Using Menu.IncludeStyleBlock requires Page.Header to be non-null (e.g. <head runat=\"server\" />).");

			StyleBlock block = new StyleBlock (owner.ClientID);
			Style style = owner.ControlStyle;
			bool horizontal = owner.Orientation == Orientation.Horizontal;
			if (style != null)
				block.RegisterStyle (style);
			
			// #MenuId img.icon { border-style:none;vertical-align:middle; }
			block.RegisterStyle (HtmlTextWriterStyle.BorderStyle, "none", "img.icon")
				.Add (HtmlTextWriterStyle.VerticalAlign, "middle");

			// #MenuId img.separator { border-style:none;display:block; }
			block.RegisterStyle (HtmlTextWriterStyle.BorderStyle, "none", "img.separator")
				.Add (HtmlTextWriterStyle.Display, "block");

			// #MenuId img.horizontal-separator { border-style:none;vertical-align:middle; }
			if (horizontal)
				block.RegisterStyle (HtmlTextWriterStyle.BorderStyle, "none", "img.horizontal-separator")
					.Add (HtmlTextWriterStyle.VerticalAlign, "middle");
			
			// #MenuId ul { list-style:none;margin:0;padding:0;width:auto; }
			block.RegisterStyle (HtmlTextWriterStyle.ListStyleType, "none", "ul")
				.Add (HtmlTextWriterStyle.Margin, "0")
				.Add (HtmlTextWriterStyle.Padding, "0")
				.Add (HtmlTextWriterStyle.Width, "auto");

			SubMenuStyle sms = owner.StaticMenuStyleInternal;
			if (sms != null) {
				// #MenuId ul.static { ... }
				block.RegisterStyle (sms, "ul.static");
			}

			// #MenuId ul.dynamic { ...; z-index:1; ... }
			NamedCssStyleCollection css = block.RegisterStyle ("ul.dynamic");
			sms = owner.DynamicMenuStyleInternal;
			if (sms != null) {
				sms.ForeColor = Color.Empty;
				css.Add (sms);
			}
			
			css.Add (HtmlTextWriterStyle.ZIndex, "1");
			int num = owner.DynamicHorizontalOffset;
			if (num != 0)
				css.Add (HtmlTextWriterStyle.MarginLeft, num + "px");
			num = owner.DynamicVerticalOffset;
			if (num != 0)
				css.Add (HtmlTextWriterStyle.MarginTop, num + "px");

			// BUG: rendering of LevelSubMenuStyles throws InvalidCastException on .NET
			// but I suspect the code it is supposed to generate is as follows:
			//
			// #MenuId ul.levelX { ... }
			//
			// so we will just ignore the bug and go with the above code.
			RenderLevelStyles (block, num, owner.LevelSubMenuStyles, "ul.level");
			
			// #MenuId a { text-decoration:none;white-space:nowrap;display:block; }
			block.RegisterStyle (HtmlTextWriterStyle.TextDecoration, "none", "a")
				.Add (HtmlTextWriterStyle.WhiteSpace, "nowrap")
				.Add (HtmlTextWriterStyle.Display, "block");

			// #MenuId a.static { ... }
			RenderAnchorStyle (block, owner.StaticMenuItemStyleInternal, "a.static");
			
			// #MenuId a.popout { background-image:url("...");background-repeat:no-repeat;background-position:right center;padding-right:14px; }
			bool needDynamicPopOut = false;
			string str = owner.StaticPopOutImageUrl;

			css = null;
			string urlFormat = "url(\"{0}\")";
			if (String.IsNullOrEmpty (str)) {
				if (owner.StaticEnableDefaultPopOutImage)
					css = block.RegisterStyle (HtmlTextWriterStyle.BackgroundImage, String.Format (urlFormat, GetArrowResourceUrl (owner)), "a.popout");
				else
					needDynamicPopOut = true;
			} else {
				css = block.RegisterStyle (HtmlTextWriterStyle.BackgroundImage, String.Format (urlFormat, str), "a.popout");
				needDynamicPopOut = true;
			}
			
			if (css != null)
				css.Add ("background-repeat", "no-repeat")
					.Add ("background-position", "right center")
					.Add (HtmlTextWriterStyle.PaddingRight, "14px");

			// #MenuId a.popout-dynamic { background:url("...") no-repeat right center;padding-right:14px; }
			str = owner.DynamicPopOutImageUrl;
			bool haveDynamicUrl = !String.IsNullOrEmpty (str);
			css = null;
			if (needDynamicPopOut || haveDynamicUrl) {
				urlFormat = "url(\"{0}\") no-repeat right center";
				if (!haveDynamicUrl) {
					if (owner.DynamicEnableDefaultPopOutImage)
						css = block.RegisterStyle (HtmlTextWriterStyle.BackgroundImage, String.Format (urlFormat, GetArrowResourceUrl (owner)), "a.popout-dynamic");
				} else
					css = block.RegisterStyle (HtmlTextWriterStyle.BackgroundImage, String.Format (urlFormat, str), "a.popout-dynamic");
			}
			if (css != null) {
				haveDynamicPopOut = true;
				css.Add (HtmlTextWriterStyle.PaddingRight, "14px");
			}

			// #MenuId a.dynamic { ... }
			RenderAnchorStyle (block, owner.DynamicMenuItemStyleInternal, "a.dynamic");
			
			num = owner.StaticDisplayLevels;
			Unit ssmi = owner.StaticSubMenuIndent;
			string unitName;
			double indent;
				
			if (ssmi == Unit.Empty) {
				unitName = "em";
				indent = 1;
			} else {
				unitName = Unit.GetExtension (ssmi.Type);
				indent = ssmi.Value;
			}

			// #MenuId a.levelX { ... }
			RenderLevelStyles (block, num, owner.LevelMenuItemStyles, "a.level", unitName, indent);

			// #MenuId a.selected.levelX { ... }
			RenderLevelStyles (block, num, owner.LevelSelectedStyles, "a.selected.level");

			// #MenuId a.static.selected { ...;text-decoration:none; }
			RenderAnchorStyle (block, owner.StaticSelectedStyleInternal, "a.static.selected");
			
			// #MenuId a.dynamic.selected { ...;text-decoration:none;border-style:none; }
			RenderAnchorStyle (block, owner.DynamicSelectedStyleInternal, "a.dynamic.selected");

			// #MenuId a.static.highlighted { ... }
			style = owner.StaticHoverStyleInternal;
			if (style != null)
				block.RegisterStyle (style, "a.static.highlighted");
			
			// #MenuId a.dynamic.highlighted { ... }
			style = owner.DynamicHoverStyleInternal;
			if (style != null)
				block.RegisterStyle (style, "a.dynamic.highlighted");
			
			head.Controls.Add (block);
		}
		
		public override void RenderBeginTag (HtmlTextWriter writer, string skipLinkText)
		{
			Menu owner = Owner;
			
			// <a href="#ID_SkipLink">
			writer.AddAttribute (HtmlTextWriterAttribute.Href, "#" + owner.ClientID + "_SkipLink");
			writer.RenderBeginTag (HtmlTextWriterTag.A);
				
			// <img alt="" height="0" width="0" src="" style="border-width:0px;"/>
			writer.AddAttribute (HtmlTextWriterAttribute.Alt, skipLinkText);
			Page page = owner.Page;
			ClientScriptManager csm = page != null ? page.ClientScript : new ClientScriptManager (null);
				
			writer.AddAttribute (HtmlTextWriterAttribute.Src, csm.GetWebResourceUrl (typeof (SiteMapPath), "transparent.gif"));
			writer.AddAttribute (HtmlTextWriterAttribute.Width, "0");
			writer.AddAttribute (HtmlTextWriterAttribute.Height, "0");
			
			writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
			writer.RenderBeginTag (HtmlTextWriterTag.Img);
			writer.RenderEndTag ();
				
			writer.RenderEndTag (); // </a>
		}
		
		public override void RenderEndTag (HtmlTextWriter writer)
		{
		}
		
		public override void AddAttributesToRender (HtmlTextWriter writer)
		{
			// do nothing
		}

		public override void RenderContents (HtmlTextWriter writer)
		{
			Menu owner = Owner;
			MenuItemCollection items = owner.Items;
			owner.RenderMenu (writer, items, owner.Orientation == Orientation.Vertical, false, 0, items.Count > 1);
		}
		
		public override void RenderMenuBeginTag (HtmlTextWriter writer, bool dynamic, int menuLevel)
		{
			if (dynamic || menuLevel == 0) {
				var style = new SubMenuStyle ();
				AddCssClass (style, "level" + (menuLevel + 1));
				FillMenuStyle (null, dynamic, menuLevel, style);
				style.AddAttributesToRender (writer);
				writer.RenderBeginTag (HtmlTextWriterTag.Ul);
			}
		}

		public override void RenderMenuEndTag (HtmlTextWriter writer, bool dynamic, int menuLevel)
		{
			if (dynamic || menuLevel == 0)
				base.RenderMenuEndTag (writer, dynamic, menuLevel);
		}
		
		public override void RenderMenuBody (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic, bool notLast)
		{
			Menu owner = Owner;
			int count = items.Count;
			var oc = new OwnerContext (this);
			
			for (int n = 0; n < count; n++) {
				MenuItem item = items [n];
				Adapters.MenuAdapter adapter = owner.Adapter as Adapters.MenuAdapter;
				if (adapter != null)
					adapter.RenderItem (writer, item, n);
				else
				 	RenderMenuItem (writer, item, vertical, (n + 1) == count ? notLast : true, n == 0, oc);
			}
		}

		protected override void RenderMenuItem (HtmlTextWriter writer, MenuItem item, bool vertical, bool notLast, bool isFirst, OwnerContext oc)
		{
			Menu owner = Owner;
			bool displayChildren = owner.DisplayChildren (item);
			bool isDynamicItem = IsDynamicItem (owner, item);
			int itemLevel = item.Depth + 1;
			string str;
			
			writer.RenderBeginTag (HtmlTextWriterTag.Li);

			if (isDynamicItem)
				RenderSeparatorImage (owner, writer, oc.DynamicTopSeparatorImageUrl, true);
			else
				RenderSeparatorImage (owner, writer, oc.StaticTopSeparatorImageUrl, true);
			
			var linkStyle = new Style ();
			if (displayChildren && (isDynamicItem || itemLevel >= oc.StaticDisplayLevels))
				AddCssClass (linkStyle, isDynamicItem && haveDynamicPopOut ? "popout-dynamic" : "popout");
			AddCssClass (linkStyle, "level" + itemLevel);

			MenuItemStyleCollection levelStyles = oc.LevelMenuItemStyles;
			if (levelStyles != null && levelStyles.Count >= itemLevel) {
				MenuItemStyle style = levelStyles [itemLevel - 1];
				string cssClass = style.CssClass;
				if (!String.IsNullOrEmpty (cssClass))
					AddCssClass (linkStyle, cssClass);
			}

			if (owner.SelectedItem == item)
				AddCssClass (linkStyle, "selected");
			
			str = item.ToolTip;
			if (!String.IsNullOrEmpty (str))
				writer.AddAttribute ("title", str);
			linkStyle.AddAttributesToRender (writer);
			RenderItemHref (owner, writer, item);
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			owner.RenderItemContent (writer, item, isDynamicItem);
			writer.RenderEndTag ();

			str = item.SeparatorImageUrl;
			if (String.IsNullOrEmpty (str)) {
				if (isDynamicItem)
					str = oc.DynamicBottomSeparatorImageUrl;
				else
					str = oc.StaticBottomSeparatorImageUrl;
			}
			RenderSeparatorImage (owner, writer, str, true);

			// if (itemLevel == 1)
			// 	writer.RenderEndTag (); // </li>
			
			if (displayChildren)
				owner.RenderMenu (writer, item.ChildItems, vertical, isDynamicItem, itemLevel, notLast);

			if (itemLevel > 0)
				writer.RenderEndTag (); // </li>
		}

		public override bool IsDynamicItem (Menu owner, MenuItem item)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");

			if (item == null)
				throw new ArgumentNullException ("item");
			
			return item.Depth + 1 >= Owner.StaticDisplayLevels;
		}
		
		NamedCssStyleCollection RenderAnchorStyle (StyleBlock block, Style style, string styleName)
		{
			if (style == null || block == null)
				return null;

			style.AlwaysRenderTextDecoration = true;
			NamedCssStyleCollection css = block.RegisterStyle (style, styleName);
			if (style.BorderStyle == BorderStyle.NotSet)
				css.Add (HtmlTextWriterStyle.BorderStyle, "none");

			return css;
		}

		void RenderLevelStyles (StyleBlock block, int num, IList levelStyles, string name, string unitName = null, double indent = 0)
		{
			int stylesCount = levelStyles != null ? levelStyles.Count : 0;
			bool haveStyles = stylesCount > 0;
			if (!haveStyles || block == null)
				return;

			NamedCssStyleCollection css;
			Style style;
			bool haveIndent = !String.IsNullOrEmpty (unitName) && indent != 0;
			for (int i = 0; i < stylesCount; i++) {
				if ((i == 0 && !haveStyles))
					continue;
				
				css = block.RegisterStyle (name + (i + 1));
				if (haveStyles && stylesCount > i) {
					style = levelStyles [i] as Style;
					if (style != null) {
						style.AlwaysRenderTextDecoration = true;
						css.CopyFrom (style.GetStyleAttributes (null));
					}
				}
				
				if (haveIndent && i > 0 && i < num) {
					css.Add (HtmlTextWriterStyle.PaddingLeft, indent.ToString (CultureInfo.InvariantCulture) + unitName);
					indent += indent;
				}
			}
		}
	}
}
