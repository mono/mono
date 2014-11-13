//------------------------------------------------------------------------------
// <copyright file="MenuRendererClassic.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Drawing;
    using System.Globalization;
    using System.Text;

    public partial class Menu {
        /// <devdoc>The classic (pre-ASP.NET 4.0) Menu renderer</devdoc>
        private class MenuRendererClassic : MenuRenderer {
            private int _cssStyleIndex;

            public MenuRendererClassic(Menu menu) : base(menu) { }

            /// <devdoc>
            ///     Make sure we are set up to render
            /// </devdoc>
            internal void EnsureRenderSettings() {
                if (Menu.Page == null) {
                    return;
                }

                // If we don't have access to the header, we can't add the necessary styles
                if (Menu.Page.Header == null) {
                    if (Menu._staticHoverStyle != null) {
                        throw new InvalidOperationException(SR.GetString(SR.NeedHeader, "Menu.StaticHoverStyle"));
                    }
                    if (Menu._dynamicHoverStyle != null) {
                        throw new InvalidOperationException(SR.GetString(SR.NeedHeader, "Menu.DynamicHoverStyle"));
                    }
                    return;
                }

                Menu._isNotIE = (Menu.Page.Request.Browser.MSDomVersion.Major < 4);

                if (Menu.Page.SupportsStyleSheets || (Menu.Page.ScriptManager != null && Menu.Page.ScriptManager.IsInAsyncPostBack)) {
                    // Register the styles. NB the order here is important: later wins over earlier
                    Menu._panelStyle = Menu.Panel.GetEmptyPopOutPanelStyle();
                    RegisterStyle(Menu._panelStyle);

                    RegisterStyle(Menu.RootMenuItemStyle);

                    RegisterStyle(Menu.ControlStyle);

                    // It's also vitally important to register hyperlinkstyles BEFORE
                    // their associated styles as we need to copy the data from this style
                    // and a registered style appears empty except for RegisteredClassName
                    if (Menu._staticItemStyle != null) {
                        Menu._staticItemStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                        RegisterStyle(Menu._staticItemStyle.HyperLinkStyle);
                        RegisterStyle(Menu._staticItemStyle);
                    }
                    if (Menu._staticMenuStyle != null) {
                        RegisterStyle(Menu._staticMenuStyle);
                    }

                    if (Menu._dynamicItemStyle != null) {
                        Menu._dynamicItemStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                        RegisterStyle(Menu._dynamicItemStyle.HyperLinkStyle);
                        RegisterStyle(Menu._dynamicItemStyle);
                    }
                    if (Menu._dynamicMenuStyle != null) {
                        RegisterStyle(Menu._dynamicMenuStyle);
                    }

                    foreach (MenuItemStyle style in Menu.LevelMenuItemStyles) {
                        style.HyperLinkStyle.DoNotRenderDefaults = true;
                        RegisterStyle(style.HyperLinkStyle);
                        RegisterStyle(style);
                    }
                    foreach (SubMenuStyle style in Menu.LevelSubMenuStyles) {
                        RegisterStyle(style);
                    }

                    if (Menu._staticSelectedStyle != null) {
                        Menu._staticSelectedStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                        RegisterStyle(Menu._staticSelectedStyle.HyperLinkStyle);
                        RegisterStyle(Menu._staticSelectedStyle);
                    }
                    if (Menu._dynamicSelectedStyle != null) {
                        Menu._dynamicSelectedStyle.HyperLinkStyle.DoNotRenderDefaults = true;
                        RegisterStyle(Menu._dynamicSelectedStyle.HyperLinkStyle);
                        RegisterStyle(Menu._dynamicSelectedStyle);
                    }
                    foreach (MenuItemStyle style in Menu.LevelSelectedStyles) {
                        style.HyperLinkStyle.DoNotRenderDefaults = true;
                        RegisterStyle(style.HyperLinkStyle);
                        RegisterStyle(style);
                    }

                    if (Menu._staticHoverStyle != null) {
                        Menu._staticHoverHyperLinkStyle = new HyperLinkStyle(Menu._staticHoverStyle);
                        Menu._staticHoverHyperLinkStyle.DoNotRenderDefaults = true;
                        RegisterStyle(Menu._staticHoverHyperLinkStyle);
                        RegisterStyle(Menu._staticHoverStyle);
                    }
                    if (Menu._dynamicHoverStyle != null) {
                        Menu._dynamicHoverHyperLinkStyle = new HyperLinkStyle(Menu._dynamicHoverStyle);
                        Menu._dynamicHoverHyperLinkStyle.DoNotRenderDefaults = true;
                        RegisterStyle(Menu._dynamicHoverHyperLinkStyle);
                        RegisterStyle(Menu._dynamicHoverStyle);
                    }
                }
            }

            public override void PreRender(bool registerScript) {
                EnsureRenderSettings();

                if (Menu.Page != null && registerScript) {
                    // The menu script requires the general webforms script
                    Menu.Page.RegisterWebFormsScript();
                    // Register the external Menu javascript file.
                    Menu.Page.ClientScript.RegisterClientScriptResource(Menu, typeof(Menu), "Menu.js");

                    string clientDataObjectID = Menu.ClientDataObjectID;

                    // 
                    StringBuilder createDataObjectScript = new StringBuilder("var ");
                    createDataObjectScript.Append(clientDataObjectID);
                    createDataObjectScript.Append(" = new Object();\r\n");
                    createDataObjectScript.Append(clientDataObjectID);
                    createDataObjectScript.Append(".disappearAfter = ");
                    createDataObjectScript.Append(Menu.DisappearAfter);
                    createDataObjectScript.Append(";\r\n");
                    createDataObjectScript.Append(clientDataObjectID);
                    createDataObjectScript.Append(".horizontalOffset = ");
                    createDataObjectScript.Append(Menu.DynamicHorizontalOffset);
                    createDataObjectScript.Append(";\r\n");
                    createDataObjectScript.Append(clientDataObjectID);
                    createDataObjectScript.Append(".verticalOffset = ");
                    createDataObjectScript.Append(Menu.DynamicVerticalOffset);
                    createDataObjectScript.Append(";\r\n");
                    if (Menu._dynamicHoverStyle != null) {
                        createDataObjectScript.Append(clientDataObjectID);
                        createDataObjectScript.Append(".hoverClass = '");
                        createDataObjectScript.Append(Menu._dynamicHoverStyle.RegisteredCssClass);
                        if (!String.IsNullOrEmpty(Menu._dynamicHoverStyle.CssClass)) {
                            if (!String.IsNullOrEmpty(Menu._dynamicHoverStyle.RegisteredCssClass)) {
                                createDataObjectScript.Append(' ');
                            }
                            createDataObjectScript.Append(Menu._dynamicHoverStyle.CssClass);
                        }
                        createDataObjectScript.Append("';\r\n");
                        if (Menu._dynamicHoverHyperLinkStyle != null) {
                            createDataObjectScript.Append(clientDataObjectID);
                            createDataObjectScript.Append(".hoverHyperLinkClass = '");
                            createDataObjectScript.Append(Menu._dynamicHoverHyperLinkStyle.RegisteredCssClass);
                            if (!String.IsNullOrEmpty(Menu._dynamicHoverStyle.CssClass)) {
                                if (!String.IsNullOrEmpty(Menu._dynamicHoverHyperLinkStyle.RegisteredCssClass)) {
                                    createDataObjectScript.Append(' ');
                                }
                                createDataObjectScript.Append(Menu._dynamicHoverStyle.CssClass);
                            }
                            createDataObjectScript.Append("';\r\n");
                        }
                    }
                    if (Menu._staticHoverStyle != null && Menu._staticHoverHyperLinkStyle != null) {
                        createDataObjectScript.Append(clientDataObjectID);
                        createDataObjectScript.Append(".staticHoverClass = '");
                        createDataObjectScript.Append(Menu._staticHoverStyle.RegisteredCssClass);
                        if (!String.IsNullOrEmpty(Menu._staticHoverStyle.CssClass)) {
                            if (!String.IsNullOrEmpty(Menu._staticHoverStyle.RegisteredCssClass)) {
                                createDataObjectScript.Append(' ');
                            }
                            createDataObjectScript.Append(Menu._staticHoverStyle.CssClass);
                        }
                        createDataObjectScript.Append("';\r\n");
                        if (Menu._staticHoverHyperLinkStyle != null) {
                            createDataObjectScript.Append(clientDataObjectID);
                            createDataObjectScript.Append(".staticHoverHyperLinkClass = '");
                            createDataObjectScript.Append(Menu._staticHoverHyperLinkStyle.RegisteredCssClass);
                            if (!String.IsNullOrEmpty(Menu._staticHoverStyle.CssClass)) {
                                if (!String.IsNullOrEmpty(Menu._staticHoverHyperLinkStyle.RegisteredCssClass)) {
                                    createDataObjectScript.Append(' ');
                                }
                                createDataObjectScript.Append(Menu._staticHoverStyle.CssClass);
                            }
                            createDataObjectScript.Append("';\r\n");
                        }
                    }
                    if ((Menu.Page.RequestInternal != null) &&
                        (String.Equals(Menu.Page.Request.Url.Scheme, "https", StringComparison.OrdinalIgnoreCase))) {

                        createDataObjectScript.Append(clientDataObjectID);
                        createDataObjectScript.Append(".iframeUrl = '");
                        createDataObjectScript.Append(Util.QuoteJScriptString(
                            Menu.Page.ClientScript.GetWebResourceUrl(typeof(Menu), "SmartNav.htm"),
                            false));
                        createDataObjectScript.Append("';\r\n");
                    }

                    // Register a startup script that creates a tree data object
                    Menu.Page.ClientScript.RegisterStartupScript(Menu, GetType(),
                        Menu.ClientID + "_CreateDataObject",
                        createDataObjectScript.ToString(),
                        true);
                }
            }

            private void RegisterStyle(Style style) {
                if (Menu.Page != null && Menu.Page.SupportsStyleSheets) {
                    string name = Menu.ClientID + "_" + _cssStyleIndex++.ToString(NumberFormatInfo.InvariantInfo);
                    Menu.Page.Header.StyleSheet.CreateStyleRule(style, Menu, "." + name);
                    style.SetRegisteredCssClass(name);
                }
            }

            public override void RenderBeginTag(HtmlTextWriter writer, bool staticOnly) {
                ControlRenderingHelper.WriteSkipLinkStart(writer, Menu.RenderingCompatibility, Menu.DesignMode, Menu.SkipLinkText, Menu.SpacerImageUrl, Menu.ClientID);

                // <table cellpadding="0" cellspacing="0" border="0" height="nodespacing">
                // Determine root menu style
                // First initialize the root menu style that depends on the control style before we change it. (VSWhidbey 354878)
                Menu.EnsureRootMenuStyle();
                if (Menu.Font != null) {
                    // Relative sizes should not be multiplied (VSWhidbey 457610)
                    Menu.Font.Reset();
                }
                Menu.ForeColor = Color.Empty;
                SubMenuStyle rootMenuStyle = Menu.GetSubMenuStyle(Menu.RootItem);
                if (Menu.Page != null && Menu.Page.SupportsStyleSheets) {
                    string styleClass = Menu.GetSubMenuCssClassName(Menu.RootItem);
                    if (styleClass.Length > 0) {
                        if (Menu.CssClass.Length == 0) {
                            Menu.CssClass = styleClass;
                        }
                        else {
                            Menu.CssClass += ' ' + styleClass;
                        }
                    }
                }
                else {
                    if (rootMenuStyle != null && !rootMenuStyle.IsEmpty) {
                        // Remove FontInfo and ForeColor from the submenustyle where they are not relevant
                        // but may get copied from by ControlStyle (VSWhidbey 438980)
                        rootMenuStyle.Font.Reset();
                        rootMenuStyle.ForeColor = Color.Empty;
                        // It's ok to change the control style at this point because viewstate has already been saved
                        Menu.ControlStyle.CopyFrom(rootMenuStyle);
                    }
                }

                Menu.AddAttributesToRender(writer);

                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
            }

            public override void RenderContents(HtmlTextWriter writer, bool staticOnly) {
                if (Menu.Orientation == Orientation.Horizontal) {
                    // Render one global tr as items won't render any
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
                bool enabled = Menu.IsEnabled;
                if (Menu.StaticDisplayLevels > 1) {
                    if (Menu.Orientation == Orientation.Vertical) {
                        // Render the items themselves
                        for (int i = 0; i < Menu.Items.Count; i++) {
                            Menu.Items[i].RenderItem(writer, i, enabled, Menu.Orientation, staticOnly);
                            // And their static subitems
                            if (Menu.Items[i].ChildItems.Count != 0) {
                                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                Menu.Items[i].Render(writer, enabled, staticOnly);
                                writer.RenderEndTag(); // td
                                writer.RenderEndTag(); // tr
                            }
                        }
                    }
                    else {
                        // Render the items themselves
                        for (int i = 0; i < Menu.Items.Count; i++) {
                            Menu.Items[i].RenderItem(writer, i, enabled, Menu.Orientation, staticOnly);
                            // And their static subitems
                            if (Menu.Items[i].ChildItems.Count != 0) {
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                Menu.Items[i].Render(writer, enabled, staticOnly);
                                writer.RenderEndTag(); // td
                            }
                        }
                    }
                }
                else {
                    // Render the items themselves
                    for (int i = 0; i < Menu.Items.Count; i++) {
                        Menu.Items[i].RenderItem(writer, i, enabled, Menu.Orientation, staticOnly);
                    }
                }

                if (Menu.Orientation == Orientation.Horizontal) {
                    // Render global /tr
                    writer.RenderEndTag();
                }

                // Reset all these cached values so things can pick up changes in the designer
                if (Menu.DesignMode) {
                    Menu.ResetCachedStyles();
                }
            }

            public override void RenderEndTag(HtmlTextWriter writer, bool staticOnly) {
                writer.RenderEndTag(); // Table

                // Render the submenus
                if (Menu.StaticDisplayLevels <= 1 && !staticOnly) {
                    bool enabled = Menu.IsEnabled;
                    for (int i = 0; i < Menu.Items.Count; i++) {
                        Menu.Items[i].Render(writer, enabled, staticOnly);
                    }
                }

                ControlRenderingHelper.WriteSkipLinkEnd(writer, Menu.DesignMode, Menu.SkipLinkText, Menu.ClientID);
            }
        }
    }
}
