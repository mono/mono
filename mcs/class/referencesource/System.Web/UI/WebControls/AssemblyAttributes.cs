//------------------------------------------------------------------------------
// <copyright file="AssemblyAttributes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using System.Runtime.CompilerServices;
using WRA = System.Web.UI.WebResourceAttribute;

[assembly:DependencyAttribute("System,", LoadHint.Always)]
[assembly:TagPrefix("System.Web.UI.WebControls", "asp")]
[assembly:WebResource("WebForms.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "WebForms.js")]
[assembly:WebResource("Focus.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "Focus.js", 
    LoadSuccessExpression="window.WebForm_FindFirstFocusableChild")]
[assembly:WebResource("SmartNav.htm", "text/html")]
[assembly:WebResource("SmartNav.js", "application/x-javascript")]
[assembly:WebResource("WebUIValidation.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "WebUIValidation.js", 
    LoadSuccessExpression = "window.Page_ValidationVer")]
[assembly:WebResource("TreeView.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "TreeView.js",
    LoadSuccessExpression = "window.TreeView_HoverNode")]
[assembly:WebResource("Menu.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "Menu.js", 
    LoadSuccessExpression = "window.Menu_ClearInterval")]
[assembly:WebResource("MenuStandards.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "MenuStandards.js",
    LoadSuccessExpression = "window.Sys && Sys.WebForms && Sys.WebForms.Menu")]
[assembly:WebResource("WebParts.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "WebParts.js",
    LoadSuccessExpression = "window.WebPart")]
[assembly:WebResource("GridView.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "GridView.js",
    LoadSuccessExpression = "window.GridView")]
[assembly:WebResource("DetailsView.js", "application/x-javascript", CdnSupportsSecureConnection = true, CdnPath = WRA._microsoftCdnBasePath + "DetailsView.js",
    LoadSuccessExpression = "window.DetailsView")]

[assembly:WebResource("Spacer.gif", "image/gif")]

[assembly:WebResource("Menu_Default_Separator.gif", "image/gif")]
[assembly:WebResource("Menu_Popout.gif", "image/gif")]
[assembly:WebResource("Menu_ScrollDown.gif", "image/gif")]
[assembly:WebResource("Menu_ScrollUp.gif", "image/gif")]

[assembly:WebResource("TreeView_Default_Dash.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_DashCollapse.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_DashExpand.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_I.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_L.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_LCollapse.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_LExpand.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_R.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_RCollapse.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_RExpand.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_T.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_TCollapse.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_TExpand.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_Expand.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_Collapse.gif", "image/gif")]
[assembly:WebResource("TreeView_Default_NoExpand.gif", "image/gif")]

[assembly:WebResource("TreeView_XP_Explorer_Expand.gif", "image/gif")]
[assembly:WebResource("TreeView_XP_Explorer_Collapse.gif", "image/gif")]
[assembly:WebResource("TreeView_XP_Explorer_NoExpand.gif", "image/gif")]
[assembly:WebResource("TreeView_XP_Explorer_RootNode.gif", "image/gif")]
[assembly:WebResource("TreeView_XP_Explorer_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_XP_Explorer_LeafNode.gif", "image/gif")]

[assembly:WebResource("TreeView_MSDN_Expand.gif", "image/gif")]
[assembly:WebResource("TreeView_MSDN_Collapse.gif", "image/gif")]
[assembly:WebResource("TreeView_MSDN_NoExpand.gif", "image/gif")]

[assembly:WebResource("TreeView_Windows_Help_Expand.gif", "image/gif")]
[assembly:WebResource("TreeView_Windows_Help_Collapse.gif", "image/gif")]
[assembly:WebResource("TreeView_Windows_Help_NoExpand.gif", "image/gif")]

[assembly:WebResource("TreeView_Arrows_Collapse.gif", "image/gif")]
[assembly:WebResource("TreeView_Arrows_Expand.gif", "image/gif")]
[assembly:WebResource("TreeView_Arrows_NoExpand.gif", "image/gif")]

[assembly:WebResource("TreeView_BulletedList2_LeafNode.gif", "image/gif")]
[assembly:WebResource("TreeView_BulletedList2_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_BulletedList2_RootNode.gif", "image/gif")]

[assembly:WebResource("TreeView_BulletedList3_LeafNode.gif", "image/gif")]
[assembly:WebResource("TreeView_BulletedList3_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_BulletedList3_RootNode.gif", "image/gif")]

[assembly:WebResource("TreeView_BulletedList4_LeafNode.gif", "image/gif")]
[assembly:WebResource("TreeView_BulletedList4_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_BulletedList4_RootNode.gif", "image/gif")]

[assembly:WebResource("TreeView_BulletedList_LeafNode.gif", "image/gif")]
[assembly:WebResource("TreeView_BulletedList_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_BulletedList_RootNode.gif", "image/gif")]

[assembly:WebResource("TreeView_Contacts_Collapse.gif", "image/gif")]
[assembly:WebResource("TreeView_Contacts_Expand.gif", "image/gif")]
[assembly:WebResource("TreeView_Contacts_NoExpand.gif", "image/gif")]

[assembly:WebResource("TreeView_Events_LeafNode.gif", "image/gif")]
[assembly:WebResource("TreeView_Events_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_Events_RootNode.gif", "image/gif")]

[assembly:WebResource("TreeView_FAQ_LeafNode.gif", "image/gif")]
[assembly:WebResource("TreeView_FAQ_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_FAQ_RootNode.gif", "image/gif")]

[assembly:WebResource("TreeView_Inbox_LeafNode.gif", "image/gif")]
[assembly:WebResource("TreeView_Inbox_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_Inbox_RootNode.gif", "image/gif")]

[assembly:WebResource("TreeView_News_LeafNode.gif", "image/gif")]
[assembly:WebResource("TreeView_News_ParentNode.gif", "image/gif")]
[assembly:WebResource("TreeView_News_RootNode.gif", "image/gif")]

[assembly:WebResource("TreeView_Simple2_NoExpand.gif", "image/gif")]

[assembly:WebResource("TreeView_Simple_NoExpand.gif", "image/gif")]

[assembly:WebResource("WebPartMenu_Check.gif", "image/gif")]
