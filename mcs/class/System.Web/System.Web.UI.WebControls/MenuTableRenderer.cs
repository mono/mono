//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Igor Zelmanovich (igorz@mainsoft.com)
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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace System.Web.UI.WebControls
{
	sealed class MenuTableRenderer : BaseMenuRenderer
	{
		const string onPreRenderScript = "var {0} = new Object ();\n{0}.webForm = {1};\n{0}.disappearAfter = {2};\n{0}.vertical = {3};";

		public override HtmlTextWriterTag Tag {
			get { return HtmlTextWriterTag.Table; }
			
		}
		
		public MenuTableRenderer (Menu owner)
			: base (owner)
		{
		}
		
		public override void AddAttributesToRender (HtmlTextWriter writer)
		{
			writer.AddAttribute ("cellpadding", "0", false);
			writer.AddAttribute ("cellspacing", "0", false);
			writer.AddAttribute ("border", "0", false);

			base.AddAttributesToRender (writer);
		}
		
		public override void PreRender (Page page, HtmlHead head, ClientScriptManager csm, string cmenu, StringBuilder script)
		{
			Menu owner = Owner;
			MenuItemStyle staticMenuItemStyle = owner.StaticMenuItemStyleInternal;
			SubMenuStyle staticMenuStyle = owner.StaticMenuStyleInternal;
			MenuItemStyle dynamicMenuItemStyle = owner.DynamicMenuItemStyleInternal;
			SubMenuStyle dynamicMenuStyle = owner.DynamicMenuStyleInternal;
			MenuItemStyleCollection levelMenuItemStyles = owner.LevelMenuItemStyles;
			List<Style> levelMenuItemLinkStyles = owner.LevelMenuItemLinkStyles;
			SubMenuStyleCollection levelSubMenuStyles = owner.LevelSubMenuStylesInternal;
			MenuItemStyle staticSelectedStyle = owner.StaticSelectedStyleInternal;
			MenuItemStyle dynamicSelectedStyle = owner.DynamicSelectedStyleInternal;
			MenuItemStyleCollection levelSelectedStyles = owner.LevelSelectedStylesInternal;
			List<Style> levelSelectedLinkStyles = owner.LevelSelectedLinkStyles;
			Style staticHoverStyle = owner.StaticHoverStyleInternal;
			Style dynamicHoverStyle = owner.DynamicHoverStyleInternal;
			
			if (!csm.IsClientScriptIncludeRegistered (typeof (Menu), "Menu.js")) {
				string url = csm.GetWebResourceUrl (typeof (Menu), "Menu.js");
				csm.RegisterClientScriptInclude (typeof (Menu), "Menu.js", url);
			}
			
			script.AppendFormat (onPreRenderScript,
					     cmenu,
					     page.IsMultiForm ? page.theForm : "window",
					     ClientScriptManager.GetScriptLiteral (owner.DisappearAfter),
					     ClientScriptManager.GetScriptLiteral (owner.Orientation == Orientation.Vertical));

			if (owner.DynamicHorizontalOffset != 0)
				script.Append (String.Concat (cmenu, ".dho = ", ClientScriptManager.GetScriptLiteral (owner.DynamicHorizontalOffset), ";\n"));
			if (owner.DynamicVerticalOffset != 0)
				script.Append (String.Concat (cmenu, ".dvo = ", ClientScriptManager.GetScriptLiteral (owner.DynamicVerticalOffset), ";\n"));

			// The order in which styles are defined matters when more than one class
			// is assigned to an element
			RegisterStyle (owner.PopOutBoxStyle, head);
			RegisterStyle (owner.ControlStyle, owner.ControlLinkStyle, head);
			
			if (staticMenuItemStyle != null)
				RegisterStyle (owner.StaticMenuItemStyle, owner.StaticMenuItemLinkStyle, head);

			if (staticMenuStyle != null)
				RegisterStyle (owner.StaticMenuStyle, head);
			
			if (dynamicMenuItemStyle != null)
				RegisterStyle (owner.DynamicMenuItemStyle, owner.DynamicMenuItemLinkStyle, head);

			if (dynamicMenuStyle != null)
				RegisterStyle (owner.DynamicMenuStyle, head);

			if (levelMenuItemStyles != null && levelMenuItemStyles.Count > 0) {
				levelMenuItemLinkStyles = new List<Style> (levelMenuItemStyles.Count);
				foreach (Style style in levelMenuItemStyles) {
					Style linkStyle = new Style ();
					levelMenuItemLinkStyles.Add (linkStyle);
					RegisterStyle (style, linkStyle, head);
				}
			}
		
			if (levelSubMenuStyles != null)
				foreach (Style style in levelSubMenuStyles)
					RegisterStyle (style, head);

			if (staticSelectedStyle != null)
				RegisterStyle (staticSelectedStyle, owner.StaticSelectedLinkStyle, head);
			
			if (dynamicSelectedStyle != null)
				RegisterStyle (dynamicSelectedStyle, owner.DynamicSelectedLinkStyle, head);

			if (levelSelectedStyles != null && levelSelectedStyles.Count > 0) {
				levelSelectedLinkStyles = new List<Style> (levelSelectedStyles.Count);
				foreach (Style style in levelSelectedStyles) {
					Style linkStyle = new Style ();
					levelSelectedLinkStyles.Add (linkStyle);
					RegisterStyle (style, linkStyle, head);
				}
			}
			
			if (staticHoverStyle != null) {
				if (head == null)
					throw new InvalidOperationException ("Using Menu.StaticHoverStyle requires Page.Header to be non-null (e.g. <head runat=\"server\" />).");
				RegisterStyle (staticHoverStyle, owner.StaticHoverLinkStyle, head);
				script.Append (String.Concat (cmenu, ".staticHover = ", ClientScriptManager.GetScriptLiteral (staticHoverStyle.RegisteredCssClass), ";\n"));
				script.Append (String.Concat (cmenu, ".staticLinkHover = ", ClientScriptManager.GetScriptLiteral (owner.StaticHoverLinkStyle.RegisteredCssClass), ";\n"));
			}
			
			if (dynamicHoverStyle != null) {
				if (head == null)
					throw new InvalidOperationException ("Using Menu.DynamicHoverStyle requires Page.Header to be non-null (e.g. <head runat=\"server\" />).");
				RegisterStyle (dynamicHoverStyle, owner.DynamicHoverLinkStyle, head);
				script.Append (String.Concat (cmenu, ".dynamicHover = ", ClientScriptManager.GetScriptLiteral (dynamicHoverStyle.RegisteredCssClass), ";\n"));
				script.Append (String.Concat (cmenu, ".dynamicLinkHover = ", ClientScriptManager.GetScriptLiteral (owner.DynamicHoverLinkStyle.RegisteredCssClass), ";\n"));
			}
		}

		public override void RenderBeginTag (HtmlTextWriter writer, string skipLinkText)
		{
			Menu owner = Owner;
			
			// <a href="#ID_SkipLink">
			writer.AddAttribute (HtmlTextWriterAttribute.Href, "#" + owner.ClientID + "_SkipLink");
			writer.RenderBeginTag (HtmlTextWriterTag.A);
				
			// <img alt="" height="0" width="0" src="" style="border-width:0px;"/>
			writer.AddAttribute (HtmlTextWriterAttribute.Alt, skipLinkText);
			writer.AddAttribute (HtmlTextWriterAttribute.Height, "0");
			writer.AddAttribute (HtmlTextWriterAttribute.Width, "0");
				
			Page page = owner.Page;
			ClientScriptManager csm = page != null ? page.ClientScript : new ClientScriptManager (null);
				
			writer.AddAttribute (HtmlTextWriterAttribute.Src, csm.GetWebResourceUrl (typeof (SiteMapPath), "transparent.gif"));
			writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
			writer.RenderBeginTag (HtmlTextWriterTag.Img);
			writer.RenderEndTag ();
				
			writer.RenderEndTag (); // </a>
		}
		
		public override void RenderEndTag (HtmlTextWriter writer)
		{
			Menu owner = Owner;
			if (owner.StaticDisplayLevels == 1 && owner.MaximumDynamicDisplayLevels > 0)
				owner.RenderDynamicMenu (writer, owner.Items);
		}

		public override void RenderContents (HtmlTextWriter writer)
		{
			Menu owner = Owner;
			RenderMenuBody (writer, owner.Items, owner.Orientation == Orientation.Vertical, false, false);
		}
		
		void RenderMenuBeginTagAttributes (HtmlTextWriter writer, bool dynamic, int menuLevel)
		{
			writer.AddAttribute ("cellpadding", "0", false);
			writer.AddAttribute ("cellspacing", "0", false);
			writer.AddAttribute ("border", "0", false);

			if (!dynamic) {
				SubMenuStyle style = new SubMenuStyle ();
				FillMenuStyle (null, dynamic, menuLevel, style);
				style.AddAttributesToRender (writer);
			}
		}
		
		public override void RenderMenuBeginTag (HtmlTextWriter writer, bool dynamic, int menuLevel)
		{
			RenderMenuBeginTagAttributes (writer, dynamic, menuLevel);
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
		}

		void RenderMenuItemSpacing (HtmlTextWriter writer, Unit itemSpacing, bool vertical)
		{
			if (vertical) {
				writer.AddStyleAttribute ("height", itemSpacing.ToString ());
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.RenderEndTag ();
				writer.RenderEndTag ();
			} else {
				writer.AddStyleAttribute ("width", itemSpacing.ToString ());
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.RenderEndTag ();
			}
		}
		
		public override void RenderMenuBody (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic, bool notLast)
		{
			Menu owner = Owner;
			if (!vertical)
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);

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

			if (!vertical)
				writer.RenderEndTag ();	// TR
		}

		protected override void RenderMenuItem (HtmlTextWriter writer, MenuItem item, bool vertical, bool notLast, bool isFirst, OwnerContext oc)
		{
			Menu owner = Owner;
			string clientID = oc.ClientID;
			bool displayChildren = owner.DisplayChildren (item);
			bool dynamicChildren = displayChildren && (item.Depth + 1 >= oc.StaticDisplayLevels);
			bool isDynamicItem = IsDynamicItem (owner, item);
			bool isVertical = oc.IsVertical || isDynamicItem;
			Unit itemSpacing = owner.GetItemSpacing (item, isDynamicItem);

			if (itemSpacing != Unit.Empty && (item.Depth > 0 || !isFirst))
				RenderMenuItemSpacing (writer, itemSpacing, isVertical);

			if(!String.IsNullOrEmpty(item.ToolTip))
				writer.AddAttribute (HtmlTextWriterAttribute.Title, item.ToolTip);
			if (isVertical)
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);

			string parentId = isDynamicItem ? "'" + item.Parent.Path + "'" : "null";
			if (dynamicChildren) {
				writer.AddAttribute ("onmouseover",
						     "javascript:Menu_OverItem ('" + clientID + "','" + item.Path + "'," + parentId + ")");
				writer.AddAttribute ("onmouseout",
						     "javascript:Menu_OutItem ('" + clientID + "','" + item.Path + "')");
			} else if (isDynamicItem) {
				writer.AddAttribute ("onmouseover",
						     "javascript:Menu_OverDynamicLeafItem ('" + clientID + "','" + item.Path + "'," + parentId + ")");
				writer.AddAttribute ("onmouseout",
						     "javascript:Menu_OutItem ('" + clientID + "','" + item.Path + "'," + parentId + ")");
			} else {
				writer.AddAttribute ("onmouseover",
						     "javascript:Menu_OverStaticLeafItem ('" + clientID + "','" + item.Path + "')");
				writer.AddAttribute ("onmouseout",
						     "javascript:Menu_OutItem ('" + clientID + "','" + item.Path + "')");
			}

			writer.RenderBeginTag (HtmlTextWriterTag.Td);

			// Top separator image

			if (isDynamicItem)
				RenderSeparatorImage (owner, writer, oc.DynamicTopSeparatorImageUrl, false);
			else
				RenderSeparatorImage (owner, writer, oc.StaticTopSeparatorImageUrl, false);

			// Menu item box
			
			MenuItemStyle style = new MenuItemStyle ();
				
			if (oc.Header != null) {
				// styles are registered
				if (!isDynamicItem && oc.StaticMenuItemStyle != null) {
					AddCssClass (style, oc.StaticMenuItemStyle.CssClass);
					AddCssClass (style, oc.StaticMenuItemStyle.RegisteredCssClass);
				}
				if (isDynamicItem && oc.DynamicMenuItemStyle != null) {
					AddCssClass (style, oc.DynamicMenuItemStyle.CssClass);
					AddCssClass (style, oc.DynamicMenuItemStyle.RegisteredCssClass);
				}
				if (oc.LevelMenuItemStyles != null && oc.LevelMenuItemStyles.Count > item.Depth) {
					AddCssClass (style, oc.LevelMenuItemStyles [item.Depth].CssClass);
					AddCssClass (style, oc.LevelMenuItemStyles [item.Depth].RegisteredCssClass);
				}
				if (item == oc.SelectedItem) {
					if (!isDynamicItem && oc.StaticSelectedStyle != null) {
						AddCssClass (style, oc.StaticSelectedStyle.CssClass);
						AddCssClass (style, oc.StaticSelectedStyle.RegisteredCssClass);
					}
					if (isDynamicItem && oc.DynamicSelectedStyle != null) {
						AddCssClass (style, oc.DynamicSelectedStyle.CssClass);
						AddCssClass (style, oc.DynamicSelectedStyle.RegisteredCssClass);
					}
					if (oc.LevelSelectedStyles != null && oc.LevelSelectedStyles.Count > item.Depth) {
						AddCssClass (style, oc.LevelSelectedStyles [item.Depth].CssClass);
						AddCssClass (style, oc.LevelSelectedStyles [item.Depth].RegisteredCssClass);
					}
				}
			} else {
				// styles are not registered
				if (!isDynamicItem && oc.StaticMenuItemStyle != null)
					style.CopyFrom (oc.StaticMenuItemStyle);
				if (isDynamicItem && oc.DynamicMenuItemStyle != null)
					style.CopyFrom (oc.DynamicMenuItemStyle);
				if (oc.LevelMenuItemStyles != null && oc.LevelMenuItemStyles.Count > item.Depth)
					style.CopyFrom (oc.LevelMenuItemStyles [item.Depth]);
				if (item == oc.SelectedItem) {
					if (!isDynamicItem && oc.StaticSelectedStyle != null)
						style.CopyFrom (oc.StaticSelectedStyle);
					if (isDynamicItem && oc.DynamicSelectedStyle != null)
						style.CopyFrom (oc.DynamicSelectedStyle);
					if (oc.LevelSelectedStyles != null && oc.LevelSelectedStyles.Count > item.Depth)
						style.CopyFrom (oc.LevelSelectedStyles [item.Depth]);
				}
			}
			style.AddAttributesToRender (writer);

			writer.AddAttribute ("id", GetItemClientId (clientID, item, "i"));
			writer.AddAttribute ("cellpadding", "0", false);
			writer.AddAttribute ("cellspacing", "0", false);
			writer.AddAttribute ("border", "0", false);
			writer.AddAttribute ("width", "100%", false);
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);

			// Menu item text

			if (isVertical)
				writer.AddStyleAttribute (HtmlTextWriterStyle.Width, "100%");
			if (!owner.ItemWrap)
				writer.AddStyleAttribute ("white-space", "nowrap");
			writer.RenderBeginTag (HtmlTextWriterTag.Td);

			RenderItemHref (owner, writer, item);
			
			Style linkStyle = new Style ();
			if (oc.Header != null) {
				// styles are registered
				AddCssClass (linkStyle, oc.ControlLinkStyle.RegisteredCssClass);

				if (!isDynamicItem && oc.StaticMenuItemStyle != null) {
					AddCssClass (linkStyle, oc.StaticMenuItemStyle.CssClass);
					AddCssClass (linkStyle, oc.StaticMenuItemLinkStyle.RegisteredCssClass);
				}
				if (isDynamicItem && oc.DynamicMenuItemStyle != null) {
					AddCssClass (linkStyle, oc.DynamicMenuItemStyle.CssClass);
					AddCssClass (linkStyle, oc.DynamicMenuItemLinkStyle.RegisteredCssClass);
				}
				if (oc.LevelMenuItemStyles != null && oc.LevelMenuItemStyles.Count > item.Depth) {
					AddCssClass (linkStyle, oc.LevelMenuItemStyles [item.Depth].CssClass);
					AddCssClass (linkStyle, oc.LevelMenuItemLinkStyles [item.Depth].RegisteredCssClass);
				}
				if (item == oc.SelectedItem) {
					if (!isDynamicItem && oc.StaticSelectedStyle != null) {
						AddCssClass (linkStyle, oc.StaticSelectedStyle.CssClass);
						AddCssClass (linkStyle, oc.StaticSelectedLinkStyle.RegisteredCssClass);
					}
					if (isDynamicItem && oc.DynamicSelectedStyle != null) {
						AddCssClass (linkStyle, oc.DynamicSelectedStyle.CssClass);
						AddCssClass (linkStyle, oc.DynamicSelectedLinkStyle.RegisteredCssClass);
					}
					if (oc.LevelSelectedStyles != null && oc.LevelSelectedStyles.Count > item.Depth) {
						AddCssClass (linkStyle, oc.LevelSelectedStyles [item.Depth].CssClass);
						AddCssClass (linkStyle, oc.LevelSelectedLinkStyles [item.Depth].RegisteredCssClass);
					}
				}
			} else {
				// styles are not registered
				linkStyle.CopyFrom (oc.ControlLinkStyle);

				if (!isDynamicItem && oc.StaticMenuItemStyle != null)
					linkStyle.CopyFrom (oc.StaticMenuItemLinkStyle);
				if (isDynamicItem && oc.DynamicMenuItemStyle != null)
					linkStyle.CopyFrom (oc.DynamicMenuItemLinkStyle);
				if (oc.LevelMenuItemStyles != null && oc.LevelMenuItemStyles.Count > item.Depth)
					linkStyle.CopyFrom (oc.LevelMenuItemLinkStyles [item.Depth]);
				if (item == oc.SelectedItem) {
					if (!isDynamicItem && oc.StaticSelectedStyle != null)
						linkStyle.CopyFrom (oc.StaticSelectedLinkStyle);
					if (isDynamicItem && oc.DynamicSelectedStyle != null)
						linkStyle.CopyFrom (oc.DynamicSelectedLinkStyle);
					if (oc.LevelSelectedStyles != null && oc.LevelSelectedStyles.Count > item.Depth)
						linkStyle.CopyFrom (oc.LevelSelectedLinkStyles [item.Depth]);
				}

				linkStyle.AlwaysRenderTextDecoration = true;
			}
			linkStyle.AddAttributesToRender (writer);

			writer.AddAttribute ("id", GetItemClientId (clientID, item, "l"));
			
			if (item.Depth > 0 && !isDynamicItem) {
				double value;
#if NET_4_0
				Unit unit = oc.StaticSubMenuIndent;
				if (unit == Unit.Empty)
					value = 16;
				else
					value = unit.Value;
#else
				value = oc.StaticSubMenuIndent.Value;
#endif
				Unit indent = new Unit (value * item.Depth, oc.StaticSubMenuIndent.Type);
				writer.AddStyleAttribute (HtmlTextWriterStyle.MarginLeft, indent.ToString ());
			}
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			owner.RenderItemContent (writer, item, isDynamicItem);
			writer.RenderEndTag ();	// A

			writer.RenderEndTag ();	// TD

			// Popup image

			if (dynamicChildren) {
				string popOutImage = GetPopOutImage (owner, item, isDynamicItem);
				if (popOutImage != null) {
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
					writer.AddAttribute ("src", owner.ResolveClientUrl (popOutImage));
					writer.AddAttribute ("border", "0");
					string toolTip = String.Format (isDynamicItem ? oc.DynamicPopOutImageTextFormatString : oc.StaticPopOutImageTextFormatString, item.Text);
					writer.AddAttribute (HtmlTextWriterAttribute.Alt, toolTip);
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();	// IMG
					writer.RenderEndTag ();	// TD
				}
			}

			writer.RenderEndTag ();	// TR
			writer.RenderEndTag ();	// TABLE
			
			writer.RenderEndTag ();	// TD

			if (!isVertical && itemSpacing == Unit.Empty && (notLast || (displayChildren && !dynamicChildren))) {
				writer.AddStyleAttribute ("width", "3px");
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.RenderEndTag ();
			}
			
			// Bottom separator image
			string separatorImg = item.SeparatorImageUrl;
			if (separatorImg.Length == 0) {
				if (isDynamicItem)
					separatorImg = oc.DynamicBottomSeparatorImageUrl;
				else
					separatorImg = oc.StaticBottomSeparatorImageUrl;
			}
			
			if (separatorImg.Length > 0) {
				if (!isVertical)
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
				RenderSeparatorImage (owner, writer, separatorImg, false);
				if (!isVertical)
					writer.RenderEndTag (); // TD
			}

			if (isVertical)
				writer.RenderEndTag ();	// TR

			if (itemSpacing != Unit.Empty)
				RenderMenuItemSpacing (writer, itemSpacing, isVertical);

			// Submenu

			if (displayChildren && !dynamicChildren) {
				if (isVertical)
					writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.AddAttribute ("width", "100%");
				owner.RenderMenu (writer, item.ChildItems, vertical, false, item.Depth + 1, notLast);
				if (item.Depth + 2 == oc.StaticDisplayLevels)
					owner.RenderDynamicMenu (writer, item.ChildItems);
				writer.RenderEndTag ();	// TD
				if (isVertical)
					writer.RenderEndTag ();	// TR
			}
		}

		public override bool IsDynamicItem (Menu owner, MenuItem item)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");

			if (item == null)
				throw new ArgumentNullException ("item");
			
			return item.Depth + 1 > owner.StaticDisplayLevels;
		}
	}
}
