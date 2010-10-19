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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace System.Web.UI.WebControls
{
	abstract class BaseMenuRenderer : IMenuRenderer
	{
		protected sealed class OwnerContext
		{
			BaseMenuRenderer container;
			
			string staticPopOutImageTextFormatString;
			string dynamicPopOutImageTextFormatString;
			string dynamicTopSeparatorImageUrl;
			string dynamicBottomSeparatorImageUrl;
			string staticTopSeparatorImageUrl;
			string staticBottomSeparatorImageUrl;
			List <Style> levelMenuItemLinkStyles;
			List<Style> levelSelectedLinkStyles;
			Style staticMenuItemLinkStyle;
			Style dynamicMenuItemLinkStyle;
			MenuItemStyle staticSelectedStyle;
			Style staticSelectedLinkStyle;
			MenuItemStyle dynamicSelectedStyle;
			Style dynamicSelectedLinkStyle;
			MenuItemStyleCollection levelSelectedStyles;
			ITemplate dynamicItemTemplate;
			bool dynamicItemTemplateQueried;
			
			public readonly MenuItemStyle StaticMenuItemStyle;
			public readonly MenuItemStyle DynamicMenuItemStyle;
			public readonly MenuItemStyleCollection LevelMenuItemStyles;
			public readonly Style ControlLinkStyle;
			public readonly HtmlHead Header;
			public readonly string ClientID;
			public readonly int StaticDisplayLevels;
			public readonly bool IsVertical;
			public readonly MenuItem SelectedItem;
			public readonly Unit StaticSubMenuIndent;
			
			public string StaticPopOutImageTextFormatString {
				get {
					if (staticPopOutImageTextFormatString == null)
						staticPopOutImageTextFormatString = container.Owner.StaticPopOutImageTextFormatString;

					return staticPopOutImageTextFormatString;
				}
			}

			public string DynamicPopOutImageTextFormatString {
				get {
					if (dynamicPopOutImageTextFormatString == null)
						dynamicPopOutImageTextFormatString = container.Owner.DynamicPopOutImageTextFormatString;

					return dynamicPopOutImageTextFormatString;
				}
			}

			public string DynamicTopSeparatorImageUrl {
				get {
					if (dynamicTopSeparatorImageUrl == null)
						dynamicTopSeparatorImageUrl = container.Owner.DynamicTopSeparatorImageUrl;

					return dynamicTopSeparatorImageUrl;
				}
			}
			
			public string DynamicBottomSeparatorImageUrl {
				get {
					if (dynamicBottomSeparatorImageUrl == null)
						dynamicBottomSeparatorImageUrl = container.Owner.DynamicBottomSeparatorImageUrl;

					return dynamicBottomSeparatorImageUrl;
				}
			}

			public string StaticTopSeparatorImageUrl {
				get {
					if (staticTopSeparatorImageUrl == null)
						staticTopSeparatorImageUrl = container.Owner.StaticTopSeparatorImageUrl;

					return staticBottomSeparatorImageUrl;
				}
			}
			
			public string StaticBottomSeparatorImageUrl {
				get {
					if (staticBottomSeparatorImageUrl == null)
						staticBottomSeparatorImageUrl = container.Owner.StaticBottomSeparatorImageUrl;

					return staticBottomSeparatorImageUrl;
				}
			}
			
			public List <Style> LevelMenuItemLinkStyles {
				get {
					if (levelMenuItemLinkStyles == null)
						levelMenuItemLinkStyles = container.Owner.LevelMenuItemLinkStyles;

					return levelMenuItemLinkStyles;
				}
			}

			public List<Style> LevelSelectedLinkStyles {
				get {
					if (levelSelectedLinkStyles == null)
						levelSelectedLinkStyles = container.Owner.LevelSelectedLinkStyles;

					return levelSelectedLinkStyles;
				}
			}
			
		
			public Style StaticMenuItemLinkStyle {
				get {
					if (staticMenuItemLinkStyle == null)
						staticMenuItemLinkStyle = container.Owner.StaticMenuItemLinkStyle;

					return staticMenuItemLinkStyle;
				}
			}

			public Style DynamicMenuItemLinkStyle {
				get {
					if (dynamicMenuItemLinkStyle == null)
						dynamicMenuItemLinkStyle = container.Owner.DynamicMenuItemLinkStyle;

					return dynamicMenuItemLinkStyle;
				}
			}

			public MenuItemStyle StaticSelectedStyle {
				get {
					if (staticSelectedStyle == null)
						staticSelectedStyle = container.Owner.StaticSelectedStyle;

					return staticSelectedStyle;
				}
			}
		
			public MenuItemStyle DynamicSelectedStyle {
				get {
					if (dynamicSelectedStyle == null)
						dynamicSelectedStyle = container.Owner.DynamicSelectedStyle;

					return dynamicSelectedStyle;
				}
			}

			public Style StaticSelectedLinkStyle {
				get {
					if (staticSelectedLinkStyle == null)
						staticSelectedLinkStyle = container.Owner.StaticSelectedLinkStyle;

					return staticSelectedLinkStyle;
				}
			}
		
			public Style DynamicSelectedLinkStyle {
				get {
					if (dynamicSelectedLinkStyle == null)
						dynamicSelectedLinkStyle = container.Owner.DynamicSelectedLinkStyle;

					return dynamicSelectedLinkStyle;
				}
			}

			public MenuItemStyleCollection LevelSelectedStyles {
				get {
					if (levelSelectedStyles == null)
						levelSelectedStyles = container.Owner.LevelSelectedStyles;

					return levelSelectedStyles;
				}
			}
			
			public ITemplate DynamicItemTemplate {
				get {
					if (!dynamicItemTemplateQueried && dynamicItemTemplate == null) {
						dynamicItemTemplate = container.Owner.DynamicItemTemplate;
						dynamicItemTemplateQueried = true;
					}

					return dynamicItemTemplate;
				}
			}
		
			public OwnerContext (BaseMenuRenderer container)
			{
				if (container == null)
					throw new ArgumentNullException ("container");

				this.container = container;
				Menu owner = container.Owner;
				Page page = owner.Page;

				Header = page != null ? page.Header : null;
				ClientID = owner.ClientID;
				IsVertical = owner.Orientation == Orientation.Vertical;
				StaticSubMenuIndent = owner.StaticSubMenuIndent;
				SelectedItem = owner.SelectedItem;
				ControlLinkStyle = owner.ControlLinkStyle;
				StaticDisplayLevels = owner.StaticDisplayLevels;
				StaticMenuItemStyle = owner.StaticMenuItemStyleInternal;
				DynamicMenuItemStyle = owner.DynamicMenuItemStyleInternal;
				LevelMenuItemStyles = owner.LevelMenuItemStyles;
			}
		}

		int registeredStylesCounter = -1;
		
		public abstract HtmlTextWriterTag Tag { get; }
		
		protected Menu Owner {
			get;
			private set;
		}
		
		public BaseMenuRenderer (Menu owner)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");

			this.Owner = owner;
		}
		
		public virtual void AddAttributesToRender (HtmlTextWriter writer)
		{
			Menu owner = Owner;
			Page page = owner.Page;
			SubMenuStyle staticMenuStyle = owner.StaticMenuStyleInternal;
			SubMenuStyleCollection levelSubMenuStyles = owner.LevelSubMenuStylesInternal;
			bool haveSubStyles = levelSubMenuStyles != null && levelSubMenuStyles.Count > 0;
			Style controlStyle = haveSubStyles || staticMenuStyle != null ? owner.ControlStyle : null;
			
			if (page != null && page.Header != null) {	
				// styles are registered
				if (staticMenuStyle != null) {
					AddCssClass (controlStyle, staticMenuStyle.CssClass);
					AddCssClass (controlStyle, staticMenuStyle.RegisteredCssClass);
				}
				if (haveSubStyles) {
					AddCssClass (controlStyle, levelSubMenuStyles [0].CssClass);
					AddCssClass (controlStyle, levelSubMenuStyles [0].RegisteredCssClass);
				}
			} else {
				// styles are not registered
				if (staticMenuStyle != null)
					controlStyle.CopyFrom (staticMenuStyle);
				if (haveSubStyles)
					controlStyle.CopyFrom (levelSubMenuStyles [0]);
			}
		}
		
		public abstract void PreRender (Page page, HtmlHead head, ClientScriptManager csm, string cmenu, StringBuilder script);
		public abstract void RenderMenuBeginTag (HtmlTextWriter writer, bool dynamic, int menuLevel);
		public abstract void RenderMenuBody (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic, bool notLast);
		public abstract void RenderBeginTag (HtmlTextWriter writer, string skipLinkText);
		public abstract void RenderEndTag (HtmlTextWriter writer);
		public abstract void RenderContents (HtmlTextWriter writer);
		public abstract bool IsDynamicItem (Menu owner, MenuItem item);
		
		protected abstract void RenderMenuItem (HtmlTextWriter writer, MenuItem item, bool vertical, bool notLast, bool isFirst, OwnerContext oc);
		
		public virtual void RenderMenuItem (HtmlTextWriter writer, MenuItem item, bool notLast, bool isFirst)
		{
			var oc = new OwnerContext (this);
			RenderMenuItem (writer, item, oc.IsVertical, notLast, isFirst, oc);
		}
		
		public virtual void RenderMenuEndTag (HtmlTextWriter writer, bool dynamic, int menuLevel)
		{
			writer.RenderEndTag ();
		}

		public virtual void RenderItemContent (HtmlTextWriter writer, MenuItem item, bool isDynamicItem)
		{
			Menu owner = Owner;
			
			if (!String.IsNullOrEmpty (item.ImageUrl)) {
				writer.AddAttribute (HtmlTextWriterAttribute.Src, owner.ResolveClientUrl (item.ImageUrl));
				writer.AddAttribute (HtmlTextWriterAttribute.Alt, item.ToolTip);
				writer.AddStyleAttribute (HtmlTextWriterStyle.BorderStyle, "none");
				writer.AddStyleAttribute (HtmlTextWriterStyle.VerticalAlign, "middle");
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			}

			string format;
			if (isDynamicItem && (format = owner.DynamicItemFormatString).Length > 0)
				writer.Write (String.Format (format, item.Text));
			else if (!isDynamicItem && (format = owner.StaticItemFormatString).Length > 0)
				writer.Write (String.Format (format, item.Text));
			else
				writer.Write (item.Text);
		}

		public void AddCssClass (Style style, string cssClass)
		{
			style.AddCssClass (cssClass);
		}
		
		public string GetItemClientId (string ownerClientID, MenuItem item, string suffix)
		{
			return ownerClientID + "_" + item.Path + suffix;
		}

		public virtual void RenderItemHref (Menu owner, HtmlTextWriter writer, MenuItem item)
		{
			if (!item.BranchEnabled)
				writer.AddAttribute ("disabled", "true", false);
			else if (!item.Selectable) {
				writer.AddAttribute ("href", "#", false);
				writer.AddStyleAttribute ("cursor", "text");
			} else if (item.NavigateUrl != String.Empty) {
				string target = item.Target != String.Empty ? item.Target : owner.Target;
#if TARGET_J2EE
				string navUrl = owner.ResolveClientUrl (item.NavigateUrl, String.Compare (target, "_blank", StringComparison.InvariantCultureIgnoreCase) != 0);
#else
				string navUrl = owner.ResolveClientUrl (item.NavigateUrl);
#endif
				writer.AddAttribute ("href", navUrl);
				if (target != String.Empty)
					writer.AddAttribute ("target", target);
			} else
				writer.AddAttribute ("href", GetClientEvent (owner, item));
		}

		public string GetPopOutImage (Menu owner, MenuItem item, bool isDynamicItem)
		{
			if (owner == null)
				owner = Owner;
			
			if (item.PopOutImageUrl != String.Empty)
				return item.PopOutImageUrl;

			bool needArrowResource = false;
			if (isDynamicItem) {
				if (owner.DynamicPopOutImageUrl != String.Empty)
					return owner.DynamicPopOutImageUrl;
				if (owner.DynamicEnableDefaultPopOutImage)
					needArrowResource = true;		
			} else {
				if (owner.StaticPopOutImageUrl != String.Empty)
					return owner.StaticPopOutImageUrl;
				if (owner.StaticEnableDefaultPopOutImage)
					needArrowResource = true;
			}

			if (needArrowResource)
				return GetArrowResourceUrl (owner);
			
			return null;
		}

		public string GetArrowResourceUrl (Menu owner) 
		{
			Page page = owner.Page;
			ClientScriptManager csm = page != null ? page.ClientScript : null;
			if (csm != null)
				return csm.GetWebResourceUrl (typeof (Menu), "arrow_plus.gif");

			return null;
		}
		
		public void FillMenuStyle (HtmlHead header, bool dynamic, int menuLevel, SubMenuStyle style)
		{
			Menu owner = Owner;
			if (header == null) {
				Page page = owner.Page;
				header = page != null ? page.Header : null;
			}
			
			SubMenuStyle staticMenuStyle = owner.StaticMenuStyleInternal;
//			MenuItemStyle dynamicMenuItemStyle = owner.DynamicMenuItemStyleInternal;
			SubMenuStyle dynamicMenuStyle = owner.DynamicMenuStyleInternal;
			SubMenuStyleCollection levelSubMenuStyles = owner.LevelSubMenuStylesInternal;
			
			if (header != null) {
				// styles are registered
				if (!dynamic && staticMenuStyle != null) {
					AddCssClass (style, staticMenuStyle.CssClass);
					AddCssClass (style, staticMenuStyle.RegisteredCssClass);
				}
				if (dynamic && dynamicMenuStyle != null) {
					AddCssClass (style, dynamicMenuStyle.CssClass);
					AddCssClass (style, dynamicMenuStyle.RegisteredCssClass);
				}
				if (levelSubMenuStyles != null && levelSubMenuStyles.Count > menuLevel) {
					AddCssClass (style, levelSubMenuStyles [menuLevel].CssClass);
					AddCssClass (style, levelSubMenuStyles [menuLevel].RegisteredCssClass);
				}
			} else {
				// styles are not registered
				if (!dynamic && staticMenuStyle != null)
					style.CopyFrom (staticMenuStyle);
				if (dynamic && dynamicMenuStyle != null)
					style.CopyFrom (dynamicMenuStyle);
				if (levelSubMenuStyles != null && levelSubMenuStyles.Count > menuLevel)
					style.CopyFrom (levelSubMenuStyles [menuLevel]);
			}
		}

		public void RegisterStyle (Style baseStyle, Style linkStyle, HtmlHead head)
		{
			RegisterStyle (baseStyle, linkStyle, null, head);
		}

		public void RegisterStyle (Style baseStyle, Style linkStyle, string className, HtmlHead head)
		{
			if (head == null)
				return;
			
			linkStyle.CopyTextStylesFrom (baseStyle);
			linkStyle.BorderStyle = BorderStyle.None;
			RegisterStyle (linkStyle, className, head);
			RegisterStyle (baseStyle, className, head);
		}
		
		public void RegisterStyle (Style baseStyle, HtmlHead head)
		{
			RegisterStyle (baseStyle, (string)null, head);
		}

		public void RegisterStyle (Style baseStyle, string className, HtmlHead head)
		{
			if (head == null)
				return;
			if (String.IsNullOrEmpty (className))
				className = IncrementStyleClassName ();
			baseStyle.SetRegisteredCssClass (className);
			head.StyleSheet.CreateStyleRule (baseStyle, Owner, "." + className);
		}

		public void RenderSeparatorImage (Menu owner, HtmlTextWriter writer, string url, bool standardsCompliant)
		{
			if (String.IsNullOrEmpty (url))
				return;
			
			writer.AddAttribute (HtmlTextWriterAttribute.Src, owner.ResolveClientUrl (url));
			if (standardsCompliant) {
				writer.AddAttribute (HtmlTextWriterAttribute.Alt, String.Empty);
				writer.AddAttribute (HtmlTextWriterAttribute.Class, "separator");
			}
			
			writer.RenderBeginTag (HtmlTextWriterTag.Img);
			writer.RenderEndTag ();
		}
		
		public bool IsDynamicItem (MenuItem item)
		{
			return IsDynamicItem (Owner, item);
		}
		
		string GetClientEvent (Menu owner, MenuItem item)
		{
			if (owner == null)
				owner = Owner;

			Page page = owner.Page;
			ClientScriptManager csm = page != null ? page.ClientScript : null;

			if (csm == null)
				return String.Empty;
			
			return csm.GetPostBackClientHyperlink (owner, item.Path, true);
		}

		string IncrementStyleClassName ()
		{
			registeredStylesCounter++;
			return Owner.ClientID + "_" + registeredStylesCounter;
		}
	}
}
