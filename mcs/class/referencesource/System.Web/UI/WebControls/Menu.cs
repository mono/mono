//------------------------------------------------------------------------------
// <copyright file="Menu.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI.WebControls.Adapters;
    using System.Web.Util;

    /// <devdoc>
    ///     Provides a cascading pop-out hierarchical menu control
    /// </devdoc>
    [ControlValueProperty("SelectedValue")]
    [DefaultEvent("MenuItemClick")]
    [Designer("System.Web.UI.Design.WebControls.MenuDesigner, " + AssemblyRef.SystemDesign)]
    [SupportsEventValidation]
    public partial class Menu : HierarchicalDataBoundControl, IPostBackEventHandler, INamingContainer {

        internal const int ScrollUpImageIndex = 0;
        internal const int ScrollDownImageIndex = 1;
        internal const int PopOutImageIndex = 2;

        internal const int ImageUrlsCount = 3;

        private const string _getDesignTimeStaticHtml = "GetDesignTimeStaticHtml";
        private const string _getDesignTimeDynamicHtml = "GetDesignTimeDynamicHtml";

        // static readonly instead of const to be able to change it in the future without breaking existing client code
        public static readonly string MenuItemClickCommandName = "Click";

        private static readonly object _menuItemClickedEvent = new object();
        private static readonly object _menuItemDataBoundEvent = new object();

        private MenuRenderingMode _renderingMode = MenuRenderingMode.Default;

        private string[] _imageUrls;

        private SubMenuStyle _staticMenuStyle;
        private SubMenuStyle _dynamicMenuStyle;

        private MenuItemStyle _staticItemStyle;
        private MenuItemStyle _staticSelectedStyle;
        private Style _staticHoverStyle;
        private HyperLinkStyle _staticHoverHyperLinkStyle;

        private MenuItemStyle _dynamicItemStyle;
        private MenuItemStyle _dynamicSelectedStyle;
        private Style _dynamicHoverStyle;
        private HyperLinkStyle _dynamicHoverHyperLinkStyle;

        private Style _rootMenuItemStyle;

        private SubMenuStyleCollection _levelStyles;
        private MenuItemStyleCollection _levelMenuItemStyles;
        private MenuItemStyleCollection _levelSelectedStyles;

        // Cached styles. In the current implementation, the styles are the same for all items
        // and submenus at a given depth.
        private List<MenuItemStyle> _cachedMenuItemStyles;
        private List<SubMenuStyle> _cachedSubMenuStyles;
        private List<string> _cachedMenuItemClassNames;
        private List<string> _cachedMenuItemHyperLinkClassNames;
        private List<string> _cachedSubMenuClassNames;
        private Collection<int> _cachedLevelsContainingCssClass;

        private MenuItem _rootItem;
        private MenuItem _selectedItem;

        private MenuItemBindingCollection _bindings;

        private string _cachedScrollUpImageUrl;
        private string _cachedScrollDownImageUrl;
        private string _cachedPopOutImageUrl;

        private ITemplate _dynamicTemplate;
        private ITemplate _staticTemplate;

        private int _maximumDepth;
        private int _nodeIndex;

        private string _currentSiteMapNodeUrl;
        private bool _dataBound;
        private bool _subControlsDataBound;
        private bool _accessKeyRendered;

        private PopOutPanel _panel;
        private Style _panelStyle;

        private bool _isNotIE;

        private Type _designTimeTextWriterType;

        private MenuRenderer _renderer;

        /// <devdoc>
        ///     Creates a new instance of a Menu.
        /// </devdoc>
        public Menu() {
            _nodeIndex = 0;
            _maximumDepth = 0;

            IncludeStyleBlock = true;
        }

        internal bool AccessKeyRendered {
            get {
                return _accessKeyRendered;
            }
            set {
                _accessKeyRendered = value;
            }
        }

        private Collection<int> CachedLevelsContainingCssClass {
            get {
                if (_cachedLevelsContainingCssClass == null) {
                    _cachedLevelsContainingCssClass = new Collection<int>();
                }
                return _cachedLevelsContainingCssClass;
            }
        }

        private List<string> CachedMenuItemClassNames {
            get {
                if (_cachedMenuItemClassNames == null) {
                    _cachedMenuItemClassNames = new List<string>();
                }
                return _cachedMenuItemClassNames;
            }
        }

        private List<string> CachedMenuItemHyperLinkClassNames {
            get {
                if (_cachedMenuItemHyperLinkClassNames == null) {
                    _cachedMenuItemHyperLinkClassNames = new List<string>();
                }
                return _cachedMenuItemHyperLinkClassNames;
            }
        }

        private List<MenuItemStyle> CachedMenuItemStyles {
            get {
                if (_cachedMenuItemStyles == null) {
                    _cachedMenuItemStyles = new List<MenuItemStyle>();
                }
                return _cachedMenuItemStyles;
            }
        }

        private List<string> CachedSubMenuClassNames {
            get {
                if (_cachedSubMenuClassNames == null) {
                    _cachedSubMenuClassNames = new List<string>();
                }
                return _cachedSubMenuClassNames;
            }
        }

        private List<SubMenuStyle> CachedSubMenuStyles {
            get {
                if (_cachedSubMenuStyles == null) {
                    _cachedSubMenuStyles = new List<SubMenuStyle>();
                }
                return _cachedSubMenuStyles;
            }
        }


        /// <devdoc>
        ///     Gets the hidden field ID for the expand state of this Menu
        /// </devdoc>
        internal string ClientDataObjectID {
            get {
                // 
                return ClientID + "_Data";
            }
        }

        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }

        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.MenuBindingsEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Data"),
        WebSysDescription(SR.Menu_Bindings)
        ]
        public MenuItemBindingCollection DataBindings {
            get {
                if (_bindings == null) {
                    _bindings = new MenuItemBindingCollection(this);
                    if (IsTrackingViewState) {
                        ((IStateManager)_bindings).TrackViewState();
                    }
                }
                return _bindings;
            }
        }


        [WebCategory("Behavior")]
        [DefaultValue(500)]
        [WebSysDescription(SR.Menu_DisappearAfter)]
        [Themeable(false)]
        public int DisappearAfter {
            get {
                object o = ViewState["DisappearAfter"];
                if (o == null) {
                    return 500;
                }
                return (int)o;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["DisappearAfter"] = value;
            }
        }


        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [Themeable(true)]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_DynamicBottomSeparatorImageUrl)]
        public string DynamicBottomSeparatorImageUrl {
            get {
                object s = ViewState["DynamicBottomSeparatorImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["DynamicBottomSeparatorImageUrl"] = value;
            }
        }


        [
        DefaultValue(true),
        WebCategory("Appearance"),
        WebSysDescription(SR.Menu_DynamicDisplayPopOutImage)
        ]
        public bool DynamicEnableDefaultPopOutImage {
            get {
                object o = ViewState["DynamicEnableDefaultPopOutImage"];
                if (o == null) {
                    return true;
                }
                return (bool)o;
            }
            set {
                ViewState["DynamicEnableDefaultPopOutImage"] = value;
            }
        }


        [
        DefaultValue(0),
        WebCategory("Appearance"),
        WebSysDescription(SR.Menu_DynamicHorizontalOffset)
        ]
        public int DynamicHorizontalOffset {
            get {
                object o = ViewState["DynamicHorizontalOffset"];
                if (o == null) {
                    return 0;
                }
                return (int)o;
            }
            set {
                ViewState["DynamicHorizontalOffset"] = value;
            }
        }


        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Menu_DynamicHoverStyle)
        ]
        public Style DynamicHoverStyle {
            get {
                if (_dynamicHoverStyle == null) {
                    _dynamicHoverStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_dynamicHoverStyle).TrackViewState();
                    }
                }
                return _dynamicHoverStyle;
            }
        }

        [DefaultValue("")]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_DynamicItemFormatString)]
        public string DynamicItemFormatString {
            get {
                object s = ViewState["DynamicItemFormatString"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["DynamicItemFormatString"] = value;
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Menu_DynamicMenuItemStyle)
        ]
        public MenuItemStyle DynamicMenuItemStyle {
            get {
                if (_dynamicItemStyle == null) {
                    _dynamicItemStyle = new MenuItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_dynamicItemStyle).TrackViewState();
                    }
                }
                return _dynamicItemStyle;
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Menu_DynamicMenuStyle)
        ]
        public SubMenuStyle DynamicMenuStyle {
            get {
                if (_dynamicMenuStyle == null) {
                    _dynamicMenuStyle = new SubMenuStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_dynamicMenuStyle).TrackViewState();
                    }
                }
                return _dynamicMenuStyle;
            }
        }


        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_DynamicPopoutImageUrl)]
        public string DynamicPopOutImageUrl {
            get {
                object s = ViewState["DynamicPopOutImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["DynamicPopOutImageUrl"] = value;
            }
        }


        [WebSysDefaultValue(SR.MenuAdapter_Expand)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_DynamicPopoutImageText)]
        public string DynamicPopOutImageTextFormatString {
            get {
                object s = ViewState["DynamicPopOutImageTextFormatString"];
                if (s == null) {
                    return SR.GetString(SR.MenuAdapter_Expand);
                }
                return (string)s;
            }
            set {
                ViewState["DynamicPopOutImageTextFormatString"] = value;
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Menu_DynamicSelectedStyle)
        ]
        public MenuItemStyle DynamicSelectedStyle {
            get {
                if (_dynamicSelectedStyle == null) {
                    _dynamicSelectedStyle = new MenuItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_dynamicSelectedStyle).TrackViewState();
                    }
                }
                return _dynamicSelectedStyle;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(MenuItemTemplateContainer)),
        WebSysDescription(SR.Menu_DynamicTemplate)
        ]
        public ITemplate DynamicItemTemplate {
            get {
                return _dynamicTemplate;
            }
            set {
                _dynamicTemplate = value;
            }
        }


        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_DynamicTopSeparatorImageUrl)]
        public string DynamicTopSeparatorImageUrl {
            get {
                object s = ViewState["DynamicTopSeparatorImageUrl"];
                if (s == null) {
                    return string.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["DynamicTopSeparatorImageUrl"] = value;
            }
        }


        [
        DefaultValue(0),
        WebCategory("Appearance"),
        WebSysDescription(SR.Menu_DynamicVerticalOffset)
        ]
        public int DynamicVerticalOffset {
            get {
                object o = ViewState["DynamicVerticalOffset"];
                if (o == null) {
                    return 0;
                }
                return (int)o;
            }
            set {
                ViewState["DynamicVerticalOffset"] = value;
            }
        }

        /// <devdoc>
        ///     A cache of urls for the built-in images.
        /// </devdoc>
        private string[] ImageUrls {
            get {
                if (_imageUrls == null) {
                    _imageUrls = new string[ImageUrlsCount];
                }
                return _imageUrls;
            }
        }

        [DefaultValue(true)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_IncludeStyleBlock)]
        public bool IncludeStyleBlock { get; set; }

        internal bool IsNotIE {
            get {
                return _isNotIE;
            }
        }


        /// <devdoc>
        ///     Gets the collection of top-level nodes.
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.MenuItemCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        WebSysDescription(SR.Menu_Items)
        ]
        public MenuItemCollection Items {
            get {
                return RootItem.ChildItems;
            }
        }


        /// <devdoc>
        ///     Gets and sets whether the text of the items should be wrapped
        /// </devdoc>
        [DefaultValue(false)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_ItemWrap)]
        public bool ItemWrap {
            get {
                object o = ViewState["ItemWrap"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                ViewState["ItemWrap"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the collection of MenuItemStyles corresponding to each level
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.MenuItemStyleCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Menu_LevelMenuItemStyles),
        ]
        public MenuItemStyleCollection LevelMenuItemStyles {
            get {
                if (_levelMenuItemStyles == null) {
                    _levelMenuItemStyles = new MenuItemStyleCollection();
                    if (IsTrackingViewState) {
                        ((IStateManager)_levelMenuItemStyles).TrackViewState();
                    }
                }

                return _levelMenuItemStyles;
            }
        }


        /// <devdoc>
        ///     Gets the collection of MenuItemStyles corresponding to the selected item on each level
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.MenuItemStyleCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Menu_LevelSelectedStyles),
        ]
        public MenuItemStyleCollection LevelSelectedStyles {
            get {
                if (_levelSelectedStyles == null) {
                    _levelSelectedStyles = new MenuItemStyleCollection();
                    if (IsTrackingViewState) {
                        ((IStateManager)_levelSelectedStyles).TrackViewState();
                    }
                }

                return _levelSelectedStyles;
            }
        }


        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.SubMenuStyleCollectionEditor," + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Menu_LevelSubMenuStyles),
        ]
        public SubMenuStyleCollection LevelSubMenuStyles {
            get {
                if (_levelStyles == null) {
                    _levelStyles = new SubMenuStyleCollection();
                    if (IsTrackingViewState) {
                        ((IStateManager)_levelStyles).TrackViewState();
                    }
                }

                return _levelStyles;
            }
        }

        internal int MaximumDepth {
            get {
                if (_maximumDepth > 0) {
                    return _maximumDepth;
                }
                _maximumDepth = MaximumDynamicDisplayLevels + StaticDisplayLevels;
                if (_maximumDepth < MaximumDynamicDisplayLevels || _maximumDepth < StaticDisplayLevels) {
                    _maximumDepth = int.MaxValue;
                }
                return _maximumDepth;
            }
        }


        [WebCategory("Behavior")]
        [DefaultValue(3)]
        [Themeable(true)]
        [WebSysDescription(SR.Menu_MaximumDynamicDisplayLevels)]
        public int MaximumDynamicDisplayLevels {
            get {
                object o = ViewState["MaximumDynamicDisplayLevels"];
                if (o == null) {
                    return 3;
                }
                return (int)o;
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("MaximumDynamicDisplayLevels", SR.GetString(SR.Menu_MaximumDynamicDisplayLevelsInvalid));
                }
                // Note: we're not testing against the old value here because
                // the setter is not the only thing that can change the underlying
                // ViewState entry: LoadViewState modifies it directly.
                ViewState["MaximumDynamicDisplayLevels"] = value;
                // Reset max depth cache
                _maximumDepth = 0;
                // Rebind if necessary
                if (_dataBound) {
                    _dataBound = false;
                    PerformDataBinding();
                }
            }
        }


        [WebCategory("Layout")]
        [DefaultValue(Orientation.Vertical)]
        [WebSysDescription(SR.Menu_Orientation)]
        public Orientation Orientation {
            get {
                object o = ViewState["Orientation"];
                if (o == null) {
                    return Orientation.Vertical;
                }
                return (Orientation)o;
            }
            set {
                ViewState["Orientation"] = value;
            }
        }

        internal PopOutPanel Panel {
            get {
                if (_panel == null) {
                    _panel = new PopOutPanel(this, _panelStyle);
                    if (!DesignMode) {
                        _panel.Page = Page;
                    }
                }
                return _panel;
            }
        }

        [DefaultValue('/')]
        [WebSysDescription(SR.Menu_PathSeparator)]
        public char PathSeparator {
            get {
                object o = ViewState["PathSeparator"];
                if (o == null) {
                    return '/';
                }
                return (char)o;
            }
            set {
                if (value == '\0') {
                    ViewState["PathSeparator"] = null;
                }
                else {
                    ViewState["PathSeparator"] = value;
                }
                foreach (MenuItem item in Items) {
                    item.ResetValuePathRecursive();
                }
            }
        }

        internal string PopoutImageUrlInternal {
            get {
                if (_cachedPopOutImageUrl != null) {
                    return _cachedPopOutImageUrl;
                }
                _cachedPopOutImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(Menu), ("Menu_Popout.gif"));
                return _cachedPopOutImageUrl;
            }
        }

        private MenuRenderer Renderer {
            get {
                if (_renderer == null) {
                    switch (RenderingMode) {
                        case MenuRenderingMode.Table:
                            _renderer = new MenuRendererClassic(this);
                            break;
                        case MenuRenderingMode.List:
                            _renderer = new MenuRendererStandards(this);
                            break;
                        case MenuRenderingMode.Default:
                            if (RenderingCompatibility < VersionUtil.Framework40) {
                                _renderer = new MenuRendererClassic(this);
                            }
                            else {
                                _renderer = new MenuRendererStandards(this);
                            }
                            break;
                        default:
                            Debug.Fail("Unknown MenuRenderingMode.");
                            break;
                    }
                }
                return _renderer;
            }
        }

        [
        WebCategory("Layout"),
        DefaultValue(MenuRenderingMode.Default),
        WebSysDescription(SR.Menu_RenderingMode)
        ]
        public MenuRenderingMode RenderingMode {
            get {
                return _renderingMode;
            }
            set {
                if (value < MenuRenderingMode.Default || value > MenuRenderingMode.List) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (_renderer != null) {
                    throw new InvalidOperationException(SR.GetString(SR.Menu_CannotChangeRenderingMode));
                }
                _renderingMode = value;
            }
        }

        /// <devdoc>
        ///     The 'virtual' root node of the menu. This menu item is never shown.
        ///     It is the parent of the "real" root items.
        /// </devdoc>
        internal MenuItem RootItem {
            get {
                if (_rootItem == null) {
                    _rootItem = new MenuItem(this, true);
                }
                return _rootItem;
            }
        }

        // RootMenuItemStyle is roughly equivalent to ControlStyle.HyperLinkStyle if it existed.
        internal Style RootMenuItemStyle {
            get {
                EnsureRootMenuStyle();
                return _rootMenuItemStyle;
            }
        }

        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_ScrollDownImageUrl)]
        public string ScrollDownImageUrl {
            get {
                object s = ViewState["ScrollDownImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["ScrollDownImageUrl"] = value;
            }
        }

        internal string ScrollDownImageUrlInternal {
            get {
                if (_cachedScrollDownImageUrl != null) {
                    return _cachedScrollDownImageUrl;
                }
                _cachedScrollDownImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(Menu), ("Menu_ScrollDown.gif"));
                return _cachedScrollDownImageUrl;
            }
        }


        [WebSysDefaultValue(SR.Menu_ScrollDown)]
        [Localizable(true)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_ScrollDownText)]
        public string ScrollDownText {
            get {
                object s = ViewState["ScrollDownText"];
                if (s == null) {
                    return SR.GetString(SR.Menu_ScrollDown);
                }
                return (string)s;
            }
            set {
                ViewState["ScrollDownText"] = value;
            }
        }


        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_ScrollUpImageUrl)]
        public string ScrollUpImageUrl {
            get {
                object s = ViewState["ScrollUpImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["ScrollUpImageUrl"] = value;
            }
        }

        internal string ScrollUpImageUrlInternal {
            get {
                if (_cachedScrollUpImageUrl != null) {
                    return _cachedScrollUpImageUrl;
                }
                _cachedScrollUpImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(Menu), ("Menu_ScrollUp.gif"));
                return _cachedScrollUpImageUrl;
            }
        }


        [WebSysDefaultValue(SR.Menu_ScrollUp)]
        [Localizable(true)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_ScrollUpText)]
        public string ScrollUpText {
            get {
                object s = ViewState["ScrollUpText"];
                if (s == null) {
                    return SR.GetString(SR.Menu_ScrollUp);
                }
                return (string)s;
            }
            set {
                ViewState["ScrollUpText"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the Menu's selected node.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public MenuItem SelectedItem {
            get {
                return _selectedItem;
            }
        }


        [Browsable(false)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedValue {
            get {
                if (SelectedItem != null) {
                    return SelectedItem.Value;
                }

                return String.Empty;
            }
        }


        [WebSysDefaultValue(SR.Menu_SkipLinkTextDefault)]
        [Localizable(true)]
        [WebCategory("Accessibility")]
        [WebSysDescription(SR.WebControl_SkipLinkText)]
        public string SkipLinkText {
            get {
                object s = ViewState["SkipLinkText"];
                if (s == null) {
                    return SR.GetString(SR.Menu_SkipLinkTextDefault);
                }
                return (string)s;
            }
            set {
                ViewState["SkipLinkText"] = value;
            }
        }


        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_StaticBottomSeparatorImageUrl)]
        public string StaticBottomSeparatorImageUrl {
            get {
                object s = ViewState["StaticBottomSeparatorImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["StaticBottomSeparatorImageUrl"] = value;
            }
        }


        [WebCategory("Behavior")]
        [DefaultValue(1)]
        [Themeable(true)]
        [WebSysDescription(SR.Menu_StaticDisplayLevels)]
        public int StaticDisplayLevels {
            get {
                object o = ViewState["StaticDisplayLevels"];
                if (o == null) {
                    return 1;
                }
                return (int)o;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                // Note: we're not testing against the old value here because
                // the setter is not the only thing that can change the underlying
                // ViewState entry: LoadViewState modifies it directly.
                ViewState["StaticDisplayLevels"] = value;
                // Reset max depth cache
                _maximumDepth = 0;
                // Rebind if necessary
                if (_dataBound && !DesignMode) {
                    _dataBound = false;
                    PerformDataBinding();
                }
            }
        }


        [
        DefaultValue(true),
        WebCategory("Appearance"),
        WebSysDescription(SR.Menu_StaticDisplayPopOutImage)
        ]
        public bool StaticEnableDefaultPopOutImage {
            get {
                object o = ViewState["StaticEnableDefaultPopOutImage"];
                if (o == null) {
                    return true;
                }
                return (bool)o;
            }
            set {
                ViewState["StaticEnableDefaultPopOutImage"] = value;
            }
        }


        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Styles"),
        WebSysDescription(SR.Menu_StaticHoverStyle)
        ]
        public Style StaticHoverStyle {
            get {
                if (_staticHoverStyle == null) {
                    _staticHoverStyle = new Style();
                    if (IsTrackingViewState) {
                        ((IStateManager)_staticHoverStyle).TrackViewState();
                    }
                }
                return _staticHoverStyle;
            }
        }

        [DefaultValue("")]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_StaticItemFormatString)]
        public string StaticItemFormatString {
            get {
                object s = ViewState["StaticItemFormatString"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["StaticItemFormatString"] = value;
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Menu_StaticMenuItemStyle)
        ]
        public MenuItemStyle StaticMenuItemStyle {
            get {
                if (_staticItemStyle == null) {
                    _staticItemStyle = new MenuItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_staticItemStyle).TrackViewState();
                    }
                }
                return _staticItemStyle;
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Menu_StaticMenuStyle)
        ]
        public SubMenuStyle StaticMenuStyle {
            get {
                if (_staticMenuStyle == null) {
                    _staticMenuStyle = new SubMenuStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_staticMenuStyle).TrackViewState();
                    }
                }
                return _staticMenuStyle;
            }
        }


        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_StaticPopoutImageUrl)]
        public string StaticPopOutImageUrl {
            get {
                object s = ViewState["StaticPopOutImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["StaticPopOutImageUrl"] = value;
            }
        }


        [WebSysDefaultValue(SR.MenuAdapter_Expand)]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_StaticPopoutImageText)]
        public string StaticPopOutImageTextFormatString {
            get {
                object s = ViewState["StaticPopOutImageTextFormatString"];
                if (s == null) {
                    return SR.GetString(SR.MenuAdapter_Expand);
                }
                return (string)s;
            }
            set {
                ViewState["StaticPopOutImageTextFormatString"] = value;
            }
        }


        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.Menu_StaticSelectedStyle)
        ]
        public MenuItemStyle StaticSelectedStyle {
            get {
                if (_staticSelectedStyle == null) {
                    _staticSelectedStyle = new MenuItemStyle();
                    if (IsTrackingViewState) {
                        ((IStateManager)_staticSelectedStyle).TrackViewState();
                    }
                }
                return _staticSelectedStyle;
            }
        }


        [WebCategory("Appearance")]
        [DefaultValue(typeof(Unit), "")]
        [Themeable(true)]
        [WebSysDescription(SR.Menu_StaticSubMenuIndent)]
        public Unit StaticSubMenuIndent {
            get {
                object o = ViewState["StaticSubMenuIndent"];
                if (o == null) {
                    return Unit.Empty;
                }
                return (Unit)o;
            }
            set {
                if (value.Value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["StaticSubMenuIndent"] = value;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(MenuItemTemplateContainer)),
        WebSysDescription(SR.Menu_StaticTemplate)
        ]
        public ITemplate StaticItemTemplate {
            get {
                return _staticTemplate;
            }
            set {
                _staticTemplate = value;
            }
        }


        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.Menu_StaticTopSeparatorImageUrl)]
        public string StaticTopSeparatorImageUrl {
            get {
                object s = ViewState["StaticTopSeparatorImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["StaticTopSeparatorImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the target window that the MenuItems will browse to if selected
        /// </devdoc>
        [DefaultValue("")]
        [WebSysDescription(SR.MenuItem_Target)]
        public string Target {
            get {
                object s = ViewState["Target"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["Target"] = value;
            }
        }


        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Table;
            }
        }

        #region events

        /// <devdoc>
        ///     Triggered when an item has been clicked.
        /// </devdoc>
        [WebCategory("Behavior")]
        [WebSysDescription(SR.Menu_MenuItemClick)]
        public event MenuEventHandler MenuItemClick {
            add {
                Events.AddHandler(_menuItemClickedEvent, value);
            }
            remove {
                Events.RemoveHandler(_menuItemClickedEvent, value);
            }
        }


        /// <devdoc>
        ///     Triggered when a MenuItem has been databound.
        /// </devdoc>
        [WebCategory("Behavior")]
        [WebSysDescription(SR.Menu_MenuItemDataBound)]
        public event MenuEventHandler MenuItemDataBound {
            add {
                Events.AddHandler(_menuItemDataBoundEvent, value);
            }
            remove {
                Events.RemoveHandler(_menuItemDataBoundEvent, value);
            }
        }
        #endregion

        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            VerifyRenderingInServerForm();

            string oldAccessKey = AccessKey;
            try {
                AccessKey = String.Empty;
                base.AddAttributesToRender(writer);
            }
            finally {
                AccessKey = oldAccessKey;
            }
        }

        // returns true if the style contains a class name
        private static bool AppendCssClassName(StringBuilder builder, MenuItemStyle style, bool hyperlink) {
            bool containsClassName = false;
            if (style != null) {
                // We have to merge with any CssClass specified on the Style itself
                if (style.CssClass.Length != 0) {
                    builder.Append(style.CssClass);
                    builder.Append(' ');
                    containsClassName = true;
                }

                string className = (hyperlink ?
                    style.HyperLinkStyle.RegisteredCssClass :
                    style.RegisteredCssClass);
                if (className.Length > 0) {
                    builder.Append(className);
                    builder.Append(' ');
                }
            }
            return containsClassName;
        }

        private static void AppendMenuCssClassName(StringBuilder builder, SubMenuStyle style) {
            if (style != null) {
                // We have to merge with any CssClass specified on the Style itself
                if (style.CssClass.Length != 0) {
                    builder.Append(style.CssClass);
                    builder.Append(' ');
                }
                string className = style.RegisteredCssClass;
                if (className.Length > 0) {
                    builder.Append(className);
                    builder.Append(' ');
                }
            }
        }

        private static T CacheGetItem<T>(List<T> cacheList, int index) where T : class {
            Debug.Assert(cacheList != null);
            if (index < cacheList.Count) return cacheList[index];
            return null;
        }

        private static void CacheSetItem<T>(List<T> cacheList, int index, T item) where T : class {
            if (cacheList.Count > index) {
                cacheList[index] = item;
            }
            else {
                for (int i = cacheList.Count; i < index; i++) {
                    cacheList.Add(null);
                }
                cacheList.Add(item);
            }
        }

        protected internal override void CreateChildControls() {
            Controls.Clear();

            if ((StaticItemTemplate != null) || (DynamicItemTemplate != null)) {
                if (RequiresDataBinding &&
                    (!String.IsNullOrEmpty(DataSourceID) || DataSource != null)) {
                    EnsureDataBound();
                }
                else {
                    // Creating child controls from the Items tree
                    CreateChildControlsFromItems(/* dataBinding */ false);
                    ClearChildViewState();
                }
            }
        }


        private void CreateChildControlsFromItems(bool dataBinding) {
            if (StaticItemTemplate != null || DynamicItemTemplate != null) {
                int childPosition = 0;
                foreach (MenuItem child in Items) {
                    CreateTemplatedControls(StaticItemTemplate, child, childPosition++, 0, dataBinding);
                }
            }
        }

        /// <devdoc>
        ///     Creates a menu node index that is unique for this menu
        /// </devdoc>
        internal int CreateItemIndex() {
            return _nodeIndex++;
        }

        private void CreateTemplatedControls(ITemplate template, MenuItem item, int position, int depth, bool dataBinding) {

            if (template != null) {
                MenuItemTemplateContainer container = new MenuItemTemplateContainer(position, item);
                item.Container = (MenuItemTemplateContainer)container;
                template.InstantiateIn(container);
                Controls.Add(container);
                if (dataBinding) {
                    container.DataBind();
                }
            }
            int childPosition = 0;
            foreach (MenuItem child in item.ChildItems) {
                int nextDepth = depth + 1;
                if (template == DynamicItemTemplate) {
                    CreateTemplatedControls(DynamicItemTemplate, child, childPosition++, nextDepth, dataBinding);
                }
                else {
                    if (nextDepth < StaticDisplayLevels) {
                        CreateTemplatedControls(template, child, childPosition++, nextDepth, dataBinding);
                    }
                    else if (DynamicItemTemplate != null) {
                        CreateTemplatedControls(DynamicItemTemplate, child, childPosition++, nextDepth, dataBinding);
                    }
                }
            }
        }

        /// Data bound controls should override PerformDataBinding instead
        /// of DataBind.  If DataBind if overridden, the OnDataBinding and OnDataBound events will
        /// fire in the wrong order.  However, for backwards compat on ListControl and AdRotator, we 
        /// can't seal this method.  It is sealed on all new BaseDataBoundControl-derived controls.
        public override sealed void DataBind() {
            base.DataBind();
        }


        /// <devdoc>
        ///     Databinds the specified node to the datasource
        /// </devdoc>
        private void DataBindItem(MenuItem item) {
            HierarchicalDataSourceView view = GetData(item.DataPath);
            // Do nothing if no datasource was set
            if (!IsBoundUsingDataSourceID && (DataSource == null)) {
                return;
            }

            if (view == null) {
                throw new InvalidOperationException(SR.GetString(SR.Menu_DataSourceReturnedNullView, ID));
            }
            IHierarchicalEnumerable enumerable = view.Select();
            item.ChildItems.Clear();
            if (enumerable != null) {
                // If we're bound to a SiteMapDataSource, automatically select the node
                if (IsBoundUsingDataSourceID) {
                    SiteMapDataSource siteMapDataSource = GetDataSource() as SiteMapDataSource;
                    if (siteMapDataSource != null) {
                        SiteMapNode currentNode = siteMapDataSource.Provider.CurrentNode;
                        if (currentNode != null) {
                            _currentSiteMapNodeUrl = currentNode.Url;
                        }
                    }
                }

                try {
                    DataBindRecursive(item, enumerable);
                }
                finally {
                    _currentSiteMapNodeUrl = null;
                }
            }
        }


        /// <devdoc>
        ///     Databinds recursively, using the Menu's Bindings collection, until there is no more data.
        /// </devdoc>
        private void DataBindRecursive(MenuItem node, IHierarchicalEnumerable enumerable) {
            // Since we are binding children, get the level below the current node's depth
            int depth = node.Depth + 1;

            // Don't databind beyond the maximum specified depth
            if ((MaximumDynamicDisplayLevels != -1) && (depth >= MaximumDepth)) {
                return;
            }

            foreach (object item in enumerable) {
                IHierarchyData data = enumerable.GetHierarchyData(item);

                string text = null;
                string value = null;
                string navigateUrl = String.Empty;
                string imageUrl = String.Empty;
                string popOutImageUrl = String.Empty;
                string separatorImageUrl = String.Empty;
                string target = String.Empty;
                bool enabled = true;
                bool enabledSet = false;
                bool selectable = true;
                bool selectableSet = false;
                string toolTip = String.Empty;

                string dataMember = String.Empty;

                dataMember = data.Type;

                MenuItemBinding level = DataBindings.GetBinding(dataMember, depth);

                if (level != null) {

                    PropertyDescriptorCollection props = TypeDescriptor.GetProperties(item);

                    // Bind Text, using the static value if necessary
                    string textField = level.TextField;
                    if (textField.Length > 0) {
                        PropertyDescriptor desc = props.Find(textField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                if (level.FormatString.Length > 0) {
                                    text = string.Format(CultureInfo.CurrentCulture, level.FormatString, objData);
                                }
                                else {
                                    text = objData.ToString();
                                }
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, textField, "TextField"));
                        }
                    }

                    if (String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(level.Text)) {
                        text = level.Text;
                    }

                    // Bind Value, using the static value if necessary
                    string valueField = level.ValueField;
                    if (valueField.Length > 0) {
                        PropertyDescriptor desc = props.Find(valueField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                value = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, valueField, "ValueField"));
                        }
                    }

                    if (String.IsNullOrEmpty(value) && !String.IsNullOrEmpty(level.Value)) {
                        value = level.Value;
                    }

                    // Bind Target, using the static value if necessary
                    string targetField = level.TargetField;
                    if (targetField.Length > 0) {
                        PropertyDescriptor desc = props.Find(targetField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                target = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, targetField, "TargetField"));
                        }
                    }

                    if (String.IsNullOrEmpty(target)) {
                        target = level.Target;
                    }

                    // Bind ImageUrl, using the static value if necessary
                    string imageUrlField = level.ImageUrlField;
                    if (imageUrlField.Length > 0) {
                        PropertyDescriptor desc = props.Find(imageUrlField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                imageUrl = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, imageUrlField, "ImageUrlField"));
                        }
                    }

                    if (String.IsNullOrEmpty(imageUrl)) {
                        imageUrl = level.ImageUrl;
                    }

                    // Bind NavigateUrl, using the static value if necessary
                    string navigateUrlField = level.NavigateUrlField;
                    if (navigateUrlField.Length > 0) {
                        PropertyDescriptor desc = props.Find(navigateUrlField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                navigateUrl = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, navigateUrlField, "NavigateUrlField"));
                        }
                    }

                    if (String.IsNullOrEmpty(navigateUrl)) {
                        navigateUrl = level.NavigateUrl;
                    }

                    // Bind PopOutImageUrl, using the static value if necessary
                    string popOutImageUrlField = level.PopOutImageUrlField;
                    if (popOutImageUrlField.Length > 0) {
                        PropertyDescriptor desc = props.Find(popOutImageUrlField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                popOutImageUrl = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, popOutImageUrlField, "PopOutImageUrlField"));
                        }
                    }

                    if (String.IsNullOrEmpty(popOutImageUrl)) {
                        popOutImageUrl = level.PopOutImageUrl;
                    }

                    // Bind SeperatorImageUrl, using the static value if necessary
                    string separatorImageUrlField = level.SeparatorImageUrlField;
                    if (separatorImageUrlField.Length > 0) {
                        PropertyDescriptor desc = props.Find(separatorImageUrlField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                separatorImageUrl = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, separatorImageUrlField, "SeparatorImageUrlField"));
                        }
                    }

                    if (String.IsNullOrEmpty(separatorImageUrl)) {
                        separatorImageUrl = level.SeparatorImageUrl;
                    }

                    // Bind ToolTip, using the static value if necessary
                    string toolTipField = level.ToolTipField;
                    if (toolTipField.Length > 0) {
                        PropertyDescriptor desc = props.Find(toolTipField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                toolTip = objData.ToString();
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, toolTipField, "ToolTipField"));
                        }
                    }

                    if (String.IsNullOrEmpty(toolTip)) {
                        toolTip = level.ToolTip;
                    }

                    // Bind Enabled, using the static value if necessary
                    string enabledField = level.EnabledField;
                    if (enabledField.Length > 0) {
                        PropertyDescriptor desc = props.Find(enabledField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                if (objData is bool) {
                                    enabled = (bool)objData;
                                    enabledSet = true;
                                }
                                else if (bool.TryParse(objData.ToString(), out enabled)) {
                                    enabledSet = true;
                                }
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, enabledField, "EnabledField"));
                        }
                    }

                    if (!enabledSet) {
                        enabled = level.Enabled;
                    }

                    // Bind Selectable, using the static value if necessary
                    string selectableField = level.SelectableField;
                    if (selectableField.Length > 0) {
                        PropertyDescriptor desc = props.Find(selectableField, true);
                        if (desc != null) {
                            object objData = desc.GetValue(item);
                            if (objData != null) {
                                if (objData is bool) {
                                    selectable = (bool)objData;
                                    selectableSet = true;
                                }
                                else if (bool.TryParse(objData.ToString(), out selectable)) {
                                    selectableSet = true;
                                }
                            }
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDataBinding, selectableField, "SelectableField"));
                        }
                    }

                    if (!selectableSet) {
                        selectable = level.Selectable;
                    }
                }
                else if (item is INavigateUIData) {
                    INavigateUIData navigateUIData = (INavigateUIData)item;
                    text = navigateUIData.Name;
                    value = navigateUIData.Value;
                    navigateUrl = navigateUIData.NavigateUrl;
                    if (String.IsNullOrEmpty(navigateUrl)) {
                        selectable = false;
                    }
                    toolTip = navigateUIData.Description;
                }

                if (text == null) {
                    text = item.ToString();
                }

                MenuItem newItem = null;
                // Allow String.Empty for the text, but not null
                if ((text != null) || (value != null)) {
                    newItem = new MenuItem(text, value, imageUrl, navigateUrl, target);
                }

                if (newItem != null) {
                    if (toolTip.Length > 0) {
                        newItem.ToolTip = toolTip;
                    }
                    if (popOutImageUrl.Length > 0) {
                        newItem.PopOutImageUrl = popOutImageUrl;
                    }
                    if (separatorImageUrl.Length > 0) {
                        newItem.SeparatorImageUrl = separatorImageUrl;
                    }
                    newItem.Enabled = enabled;
                    newItem.Selectable = selectable;

                    newItem.SetDataPath(data.Path);
                    newItem.SetDataBound(true);

                    node.ChildItems.Add(newItem);

                    if (String.Equals(data.Path, _currentSiteMapNodeUrl, StringComparison.OrdinalIgnoreCase)) {
                        newItem.Selected = true;
                    }

                    // Make sure we call user code if they've hooked the populate event
                    newItem.SetDataItem(data.Item);
                    OnMenuItemDataBound(new MenuEventArgs(newItem));
                    newItem.SetDataItem(null);

                    if (data.HasChildren && (depth < MaximumDepth)) {
                        IHierarchicalEnumerable newEnumerable = data.GetChildren();
                        if (newEnumerable != null) {
                            DataBindRecursive(newItem, newEnumerable);
                        }
                    }
                }
            }
        }

        protected override void EnsureDataBound() {
            base.EnsureDataBound();
            if (!_subControlsDataBound) {
                foreach (Control ctrl in Controls) {
                    ctrl.DataBind();
                }
                _subControlsDataBound = true;
            }
        }

        public MenuItem FindItem(string valuePath) {
            if (valuePath == null) {
                return null;
            }
            return Items.FindItem(valuePath.Split(PathSeparator), 0);
        }

        internal string GetCssClassName(MenuItem item, bool hyperLink) {
            bool discarded;
            return GetCssClassName(item, hyperLink, out discarded);
        }

        internal string GetCssClassName(MenuItem item, bool hyperlink, out bool containsClassName) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            containsClassName = false;
            int depth = item.Depth;
            string baseClassName = CacheGetItem<string>(
                hyperlink ? CachedMenuItemHyperLinkClassNames : CachedMenuItemClassNames,
                depth);
            if (CachedLevelsContainingCssClass.Contains(depth)) {
                containsClassName = true;
            }

            if (!item.Selected && (baseClassName != null)) {
                return baseClassName;
            }

            StringBuilder builder = new StringBuilder();
            if (baseClassName != null) {
                if (!item.Selected) return baseClassName;
                builder.Append(baseClassName);
                builder.Append(' ');
            }
            else {
                // No cached style, so build it
                if (hyperlink) {
                    builder.Append(RootMenuItemStyle.RegisteredCssClass);
                    builder.Append(' ');
                }

                if (depth < StaticDisplayLevels) {
                    containsClassName |= AppendCssClassName(builder, _staticItemStyle, hyperlink);
                }
                else {
                    containsClassName |= AppendCssClassName(builder, _dynamicItemStyle, hyperlink);
                }
                if ((depth < LevelMenuItemStyles.Count) && (LevelMenuItemStyles[depth] != null)) {
                    containsClassName |= AppendCssClassName(builder, LevelMenuItemStyles[depth], hyperlink);
                }

                baseClassName = builder.ToString().Trim();
                CacheSetItem<string>(
                    hyperlink ? CachedMenuItemHyperLinkClassNames : CachedMenuItemClassNames,
                    depth,
                    baseClassName);

                if (containsClassName && !CachedLevelsContainingCssClass.Contains(depth)) {
                    CachedLevelsContainingCssClass.Add(depth);
                }
            }

            if (item.Selected) {
                if (depth < StaticDisplayLevels) {
                    containsClassName |= AppendCssClassName(builder, _staticSelectedStyle, hyperlink);
                }
                else {
                    containsClassName |= AppendCssClassName(builder, _dynamicSelectedStyle, hyperlink);
                }
                if ((depth < LevelSelectedStyles.Count) && (LevelSelectedStyles[depth] != null)) {
                    MenuItemStyle style = LevelSelectedStyles[depth];
                    containsClassName |= AppendCssClassName(builder, style, hyperlink);
                }
                return builder.ToString().Trim();
            }
            return baseClassName;
        }

        /// <devdoc>Used by GetDesignModeState to find the first dynamic submenu in Items</devdoc>
        private MenuItem GetOneDynamicItem(MenuItem item) {
            if (item.Depth >= StaticDisplayLevels) {
                return item;
            }
            for (int i = 0; i < item.ChildItems.Count; i++) {
                MenuItem result = GetOneDynamicItem(item.ChildItems[i]);
                if (result != null) {
                    return result;
                }
            }
            return null;
        }


        /// <internalonly/>
        /// <devdoc>
        /// The designer can get the static and the dynamic mode html by using the
        /// _getDesignTimeStaticHtml and _getDesignTimeDynamicHtml defined string keys
        /// of the dictionary.
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override IDictionary GetDesignModeState() {
            IDictionary dictionary = base.GetDesignModeState();
            Debug.Assert(dictionary != null && DesignMode);

            CreateChildControls();
            foreach (Control c in Controls) {
                c.DataBind();
            }

            // Create the html for the static part
            using (StringWriter staticHtmlBuilder = new StringWriter(CultureInfo.CurrentCulture)) {
                using (HtmlTextWriter htmlWriter = GetDesignTimeWriter(staticHtmlBuilder)) {
                    Renderer.RenderBeginTag(htmlWriter, true);
                    Renderer.RenderContents(htmlWriter, true);
                    Renderer.RenderEndTag(htmlWriter, true);
                    dictionary[_getDesignTimeStaticHtml] = staticHtmlBuilder.ToString();
                }
            }
            // Remember the static depth so we can lower it if necessary to make it faster and avoid overflows
            int oldStaticDisplayLevels = StaticDisplayLevels;
            try {
                // Find a dynamic sub-menu
                MenuItem dynamicSubMenu = GetOneDynamicItem(RootItem);
                if (dynamicSubMenu == null) {
                    // We need to forge a whole new dynamic submenu
                    // First lower the static display levels
                    _dataBound = false;
                    StaticDisplayLevels = 1;
                    dynamicSubMenu = new MenuItem();
                    dynamicSubMenu.SetDepth(0);
                    dynamicSubMenu.SetOwner(this);
                    // Create a single dynamic submenu, with one submenu
                    string dummyText = SR.GetString(SR.Menu_DesignTimeDummyItemText);
                    for (int i = 0; i < 5; i++) {
                        MenuItem newItem = new MenuItem(dummyText);
                        if (DynamicItemTemplate != null) {
                            MenuItemTemplateContainer container = new MenuItemTemplateContainer(i, newItem);
                            newItem.Container = container;
                            DynamicItemTemplate.InstantiateIn(container);
                            container.Site = this.Site;
                            container.DataBind();
                        }
                        dynamicSubMenu.ChildItems.Add(newItem);
                    }
                    dynamicSubMenu.ChildItems[1].ChildItems.Add(new MenuItem());
                    // Delete cached styles to ensure consistency
                    _cachedLevelsContainingCssClass = null;
                    _cachedMenuItemStyles = null;
                    _cachedSubMenuStyles = null;
                    _cachedMenuItemClassNames = null;
                    _cachedMenuItemHyperLinkClassNames = null;
                    _cachedSubMenuClassNames = null;
                }
                else {
                    dynamicSubMenu = dynamicSubMenu.Parent;
                }
                // Create the html for the dynamic part
                using (StringWriter dynamicHtmlBuilder = new StringWriter(CultureInfo.CurrentCulture)) {
                    using (HtmlTextWriter htmlWriter = GetDesignTimeWriter(dynamicHtmlBuilder)) {
                        // Render the control's position on the outer table
                        Attributes.AddAttributes(htmlWriter);
                        // Rendering a table around the div so that the designer sees it as a block
                        htmlWriter.RenderBeginTag(HtmlTextWriterTag.Table);
                        htmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
                        htmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);

                        dynamicSubMenu.Render(htmlWriter, true, false, false);

                        htmlWriter.RenderEndTag();
                        htmlWriter.RenderEndTag();
                        htmlWriter.RenderEndTag();
                        dictionary[_getDesignTimeDynamicHtml] = dynamicHtmlBuilder.ToString();
                    }
                }
            }
            finally {
                if (StaticDisplayLevels != oldStaticDisplayLevels) {
                    StaticDisplayLevels = oldStaticDisplayLevels;
                }
            }

            return dictionary;
        }

        private HtmlTextWriter GetDesignTimeWriter(StringWriter stringWriter) {
            if (_designTimeTextWriterType == null) {
                return new HtmlTextWriter(stringWriter);
            }
            else {
                Debug.Assert(_designTimeTextWriterType.IsSubclassOf(typeof(HtmlTextWriter)));
                ConstructorInfo constructor = _designTimeTextWriterType.GetConstructor(new Type[] { typeof(TextWriter) });
                if (constructor == null) {
                    return new HtmlTextWriter(stringWriter);
                }
                return (HtmlTextWriter)(constructor.Invoke(new object[] { stringWriter }));
            }
        }


        /// <devdoc>
        ///     Gets the URL for the specified image, properly pathing the image filename depending on which image it is
        /// </devdoc>
        internal string GetImageUrl(int index) {
            if (ImageUrls[index] == null) {
                switch (index) {
                    case ScrollUpImageIndex:
                        ImageUrls[index] = ScrollUpImageUrlInternal;
                        break;
                    case ScrollDownImageIndex:
                        ImageUrls[index] = ScrollDownImageUrlInternal;
                        break;
                    case PopOutImageIndex:
                        ImageUrls[index] = PopoutImageUrlInternal;
                        break;
                }

                ImageUrls[index] = ResolveClientUrl(ImageUrls[index]);
            }

            return ImageUrls[index];
        }

        internal MenuItemStyle GetMenuItemStyle(MenuItem item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            int depth = item.Depth;
            MenuItemStyle typedStyle = CacheGetItem<MenuItemStyle>(CachedMenuItemStyles, depth);

            if (!item.Selected && typedStyle != null) return typedStyle;

            if (typedStyle == null) {
                typedStyle = new MenuItemStyle();
                typedStyle.CopyFrom(RootMenuItemStyle);
                if (depth < StaticDisplayLevels) {
                    if (_staticItemStyle != null) {
                        TreeView.GetMergedStyle(typedStyle, _staticItemStyle);
                    }
                }
                else if (depth >= StaticDisplayLevels && _dynamicItemStyle != null) {
                    TreeView.GetMergedStyle(typedStyle, _dynamicItemStyle);
                }
                if ((depth < LevelMenuItemStyles.Count) && (LevelMenuItemStyles[depth] != null)) {
                    TreeView.GetMergedStyle(typedStyle, LevelMenuItemStyles[depth]);
                }
                CacheSetItem<MenuItemStyle>(CachedMenuItemStyles, depth, typedStyle);
            }

            if (item.Selected) {
                MenuItemStyle selectedStyle = new MenuItemStyle();
                selectedStyle.CopyFrom(typedStyle);
                if (depth < StaticDisplayLevels) {
                    if (_staticSelectedStyle != null) {
                        TreeView.GetMergedStyle(selectedStyle, _staticSelectedStyle);
                    }
                }
                else if (depth >= StaticDisplayLevels && _dynamicSelectedStyle != null) {
                    TreeView.GetMergedStyle(selectedStyle, _dynamicSelectedStyle);
                }
                if (depth < LevelSelectedStyles.Count && LevelSelectedStyles[depth] != null) {
                    TreeView.GetMergedStyle(selectedStyle, LevelSelectedStyles[depth]);
                }
                return selectedStyle;
            }
            return typedStyle;
        }

        internal string GetSubMenuCssClassName(MenuItem item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            int nextDepth = item.Depth + 1;
            string baseClassName = CacheGetItem<string>(CachedSubMenuClassNames, nextDepth);
            if (baseClassName != null) return baseClassName;

            StringBuilder builder = new StringBuilder();
            if (nextDepth < StaticDisplayLevels) {
                AppendMenuCssClassName(builder, _staticMenuStyle);
            }
            else {
                SubMenuStyle subMenuStyle = _panelStyle as SubMenuStyle;
                if (subMenuStyle != null) {
                    AppendMenuCssClassName(builder, subMenuStyle);
                }
                AppendMenuCssClassName(builder, _dynamicMenuStyle);
            }
            if ((nextDepth < LevelSubMenuStyles.Count) && (LevelSubMenuStyles[nextDepth] != null)) {
                SubMenuStyle style = LevelSubMenuStyles[nextDepth] as SubMenuStyle;
                AppendMenuCssClassName(builder, style);
            }

            baseClassName = builder.ToString().Trim();
            CacheSetItem<string>(CachedSubMenuClassNames, nextDepth, baseClassName);
            return baseClassName;
        }

        internal SubMenuStyle GetSubMenuStyle(MenuItem item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            int nextDepth = item.Depth + 1;
            SubMenuStyle subMenuStyle = CacheGetItem<SubMenuStyle>(CachedSubMenuStyles, nextDepth);

            if (subMenuStyle != null) return subMenuStyle;

            int staticDisplayLevels = StaticDisplayLevels;
            if (nextDepth >= staticDisplayLevels && !DesignMode) {
                subMenuStyle = new PopOutPanel.PopOutPanelStyle(Panel);
            }
            else {
                subMenuStyle = new SubMenuStyle();
            }
            if (nextDepth < staticDisplayLevels) {
                if (_staticMenuStyle != null) {
                    subMenuStyle.CopyFrom(_staticMenuStyle);
                }
            }
            else if (nextDepth >= staticDisplayLevels && _dynamicMenuStyle != null) {
                subMenuStyle.CopyFrom(_dynamicMenuStyle);
            }
            if (_levelStyles != null &&
                _levelStyles.Count > nextDepth &&
                _levelStyles[nextDepth] != null) {
                TreeView.GetMergedStyle(subMenuStyle, _levelStyles[nextDepth]);
            }

            CacheSetItem<SubMenuStyle>(CachedSubMenuStyles, nextDepth, subMenuStyle);
            return subMenuStyle;
        }

        internal void EnsureRootMenuStyle() {
            if (_rootMenuItemStyle == null) {
                _rootMenuItemStyle = new Style();
                _rootMenuItemStyle.Font.CopyFrom(Font);
                if (!ForeColor.IsEmpty) {
                    _rootMenuItemStyle.ForeColor = ForeColor;
                }
                // Not defaulting to black anymore for not entirely satisfying but reasonable reasons (VSWhidbey 356729)
                if (!ControlStyle.IsSet(System.Web.UI.WebControls.Style.PROP_FONT_UNDERLINE)) {
                    _rootMenuItemStyle.Font.Underline = false;
                }
            }
        }

        protected internal override void LoadControlState(object savedState) {
            Pair state = savedState as Pair;
            if (state == null) {
                base.LoadControlState(savedState);
                return;
            }
            base.LoadControlState(state.First);

            _selectedItem = null;
            if (state.Second != null) {
                string path = state.Second as string;
                if (path != null) {
                    _selectedItem = Items.FindItem(path.Split(TreeView.InternalPathSeparator), 0);
                }
            }
        }

        protected override void LoadViewState(object state) {
            if (state != null) {
                object[] savedState = (object[])state;

                if (savedState[1] != null) {
                    ((IStateManager)StaticMenuItemStyle).LoadViewState(savedState[1]);
                }

                if (savedState[2] != null) {
                    ((IStateManager)StaticSelectedStyle).LoadViewState(savedState[2]);
                }

                if (savedState[3] != null) {
                    ((IStateManager)StaticHoverStyle).LoadViewState(savedState[3]);
                }

                if (savedState[4] != null) {
                    ((IStateManager)StaticMenuStyle).LoadViewState(savedState[4]);
                }

                if (savedState[5] != null) {
                    ((IStateManager)DynamicMenuItemStyle).LoadViewState(savedState[5]);
                }

                if (savedState[6] != null) {
                    ((IStateManager)DynamicSelectedStyle).LoadViewState(savedState[6]);
                }

                if (savedState[7] != null) {
                    ((IStateManager)DynamicHoverStyle).LoadViewState(savedState[7]);
                }

                if (savedState[8] != null) {
                    ((IStateManager)DynamicMenuStyle).LoadViewState(savedState[8]);
                }

                if (savedState[9] != null) {
                    ((IStateManager)LevelMenuItemStyles).LoadViewState(savedState[9]);
                }

                if (savedState[10] != null) {
                    ((IStateManager)LevelSelectedStyles).LoadViewState(savedState[10]);
                }

                if (savedState[11] != null) {
                    ((IStateManager)LevelSubMenuStyles).LoadViewState(savedState[11]);
                }

                if (savedState[12] != null) {
                    ((IStateManager)Items).LoadViewState(savedState[12]);
                    if (!String.IsNullOrEmpty(DataSourceID) || DataSource != null) {
                        _dataBound = true;
                    }
                }

                // Restore the core viewstate last because some property changes here could
                // necessitate item rebinding (for example, MaximumDynamicDisplayLevels)
                if (savedState[0] != null) {
                    base.LoadViewState(savedState[0]);
                }
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e) {
            MenuEventArgs me = e as MenuEventArgs;
            if (me != null && StringUtil.EqualsIgnoreCase(me.CommandName, MenuItemClickCommandName)) {

                // Do not take any postback into account if the menu is disabled.
                if (!IsEnabled) return true;

                OnMenuItemClick(me);
                if (AdapterInternal != null) {
                    MenuAdapter menuAdapter = AdapterInternal as MenuAdapter;
                    if (menuAdapter != null) {
                        MenuItem mi = me.Item;
                        // Need to tell the adapter about bubbled click events
                        // for the templated case if the item has children
                        if (mi != null &&
                            mi.ChildItems.Count > 0 &&
                            mi.Depth + 1 >= StaticDisplayLevels) {

                            menuAdapter.SetPath(me.Item.InternalValuePath);
                        }
                    }
                }
                RaiseBubbleEvent(this, e);
                return true;
            }
            if (e is CommandEventArgs) {
                RaiseBubbleEvent(this, e);
                return true;
            }
            return false;
        }

        protected override void OnDataBinding(EventArgs e) {
            EnsureChildControls();
            base.OnDataBinding(e);
        }

        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            Page.RegisterRequiresControlState(this);
        }


        protected virtual void OnMenuItemClick(MenuEventArgs e) {
            SetSelectedItem(e.Item);
            MenuEventHandler handler = (MenuEventHandler)Events[_menuItemClickedEvent];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///     Raises the MenuItemDataBound event
        /// </devdoc>
        protected virtual void OnMenuItemDataBound(MenuEventArgs e) {
            MenuEventHandler handler = (MenuEventHandler)Events[_menuItemDataBoundEvent];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        ///     Overridden to register for postback, and if client script is enabled, renders out
        ///     the necessary script and hidden field to function.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (Items.Count > 0) {
                Renderer.PreRender(IsEnabled);
            }
        }

        /// <devdoc>
        ///     Overridden to register for postback, and if client script is enabled, renders out
        ///     the necessary script and hidden field to function.
        /// </devdoc>
        internal void OnPreRender(EventArgs e, bool registerScript) {
            base.OnPreRender(e);

            if (Items.Count > 0) {
                Renderer.PreRender(registerScript);
            }
        }

        /// <devdoc>
        ///     Overridden to create all the items based on the datasource provided
        /// </devdoc>
        protected internal override void PerformDataBinding() {
            base.PerformDataBinding();

            DataBindItem(RootItem);

            if (!DesignMode && _dataBound &&
                String.IsNullOrEmpty(DataSourceID) && DataSource == null) {

                Items.Clear();
                Controls.Clear();
                ClearChildViewState();
                TrackViewState();
                ChildControlsCreated = true;
                return;
            }

            if (!String.IsNullOrEmpty(DataSourceID) ||
                DataSource != null) {

                Controls.Clear();
                ClearChildState();
                TrackViewState();

                CreateChildControlsFromItems(true);
                ChildControlsCreated = true;
                _dataBound = true;
            }
            else if (!_subControlsDataBound) {
                foreach (Control ctrl in Controls) {
                    ctrl.DataBind();
                }
            }
            _subControlsDataBound = true;
        }

        protected internal override void Render(HtmlTextWriter writer) {
            VerifyRenderingInServerForm();

            if (Items.Count > 0) {
                Renderer.RenderBeginTag(writer, false);
                Renderer.RenderContents(writer, false);
                Renderer.RenderEndTag(writer, false);
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer) {
            Renderer.RenderBeginTag(writer, false);
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            Renderer.RenderContents(writer, false);
        }

        public override void RenderEndTag(HtmlTextWriter writer) {
            Renderer.RenderEndTag(writer, false);
        }

        internal void ResetCachedStyles() {
            // Reset all these cached values so things can pick up changes in the designer
            if (_dynamicItemStyle != null) {
                _dynamicItemStyle.ResetCachedStyles();
            }
            if (_staticItemStyle != null) {
                _staticItemStyle.ResetCachedStyles();
            }
            if (_dynamicSelectedStyle != null) {
                _dynamicSelectedStyle.ResetCachedStyles();
            }
            if (_staticSelectedStyle != null) {
                _staticSelectedStyle.ResetCachedStyles();
            }
            if (_staticHoverStyle != null) {
                _staticHoverHyperLinkStyle = new HyperLinkStyle(_staticHoverStyle);
            }
            if (_dynamicHoverStyle != null) {
                _dynamicHoverHyperLinkStyle = new HyperLinkStyle(_dynamicHoverStyle);
            }

            foreach (MenuItemStyle style in LevelMenuItemStyles) {
                style.ResetCachedStyles();
            }

            foreach (MenuItemStyle style in LevelSelectedStyles) {
                style.ResetCachedStyles();
            }

            if (_imageUrls != null) {
                for (int i = 0; i < _imageUrls.Length; i++) {
                    _imageUrls[i] = null;
                }
            }

            _cachedPopOutImageUrl = null;
            _cachedScrollDownImageUrl = null;
            _cachedScrollUpImageUrl = null;
            _cachedLevelsContainingCssClass = null;
            _cachedMenuItemClassNames = null;
            _cachedMenuItemHyperLinkClassNames = null;
            _cachedMenuItemStyles = null;
            _cachedSubMenuClassNames = null;
            _cachedSubMenuStyles = null;
        }

        protected internal override object SaveControlState() {
            object baseState = base.SaveControlState();
            if (_selectedItem != null) {
                return new Pair(baseState, _selectedItem.InternalValuePath);
            }
            else {
                return baseState;
            }
        }

        protected override object SaveViewState() {
            object[] state = new object[13];

            state[0] = base.SaveViewState();

            bool hasViewState = (state[0] != null);

            if (_staticItemStyle != null) {
                state[1] = ((IStateManager)_staticItemStyle).SaveViewState();
                hasViewState |= (state[1] != null);
            }

            if (_staticSelectedStyle != null) {
                state[2] = ((IStateManager)_staticSelectedStyle).SaveViewState();
                hasViewState |= (state[2] != null);
            }

            if (_staticHoverStyle != null) {
                state[3] = ((IStateManager)_staticHoverStyle).SaveViewState();
                hasViewState |= (state[3] != null);
            }

            if (_staticMenuStyle != null) {
                state[4] = ((IStateManager)_staticMenuStyle).SaveViewState();
                hasViewState |= (state[4] != null);
            }

            if (_dynamicItemStyle != null) {
                state[5] = ((IStateManager)_dynamicItemStyle).SaveViewState();
                hasViewState |= (state[5] != null);
            }

            if (_dynamicSelectedStyle != null) {
                state[6] = ((IStateManager)_dynamicSelectedStyle).SaveViewState();
                hasViewState |= (state[6] != null);
            }

            if (_dynamicHoverStyle != null) {
                state[7] = ((IStateManager)_dynamicHoverStyle).SaveViewState();
                hasViewState |= (state[7] != null);
            }

            if (_dynamicMenuStyle != null) {
                state[8] = ((IStateManager)_dynamicMenuStyle).SaveViewState();
                hasViewState |= (state[8] != null);
            }

            if (_levelMenuItemStyles != null) {
                state[9] = ((IStateManager)_levelMenuItemStyles).SaveViewState();
                hasViewState |= (state[9] != null);
            }

            if (_levelSelectedStyles != null) {
                state[10] = ((IStateManager)_levelSelectedStyles).SaveViewState();
                hasViewState |= (state[10] != null);
            }

            if (_levelStyles != null) {
                state[11] = ((IStateManager)_levelStyles).SaveViewState();
                hasViewState |= (state[11] != null);
            }

            state[12] = ((IStateManager)Items).SaveViewState();
            hasViewState |= (state[12] != null);

            if (hasViewState) {
                return state;
            }
            else {
                return null;
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override void SetDesignModeState(IDictionary data) {
            if (data.Contains("DesignTimeTextWriterType")) {
                Type writerType = data["DesignTimeTextWriterType"] as Type;
                if (writerType != null && writerType.IsSubclassOf(typeof(HtmlTextWriter))) {
                    _designTimeTextWriterType = writerType;
                }
            }
            base.SetDesignModeState(data);
        }


        /// <devdoc>
        /// Allows a derived Menu to set the DataBound proprety on a node
        /// </devdoc>
        protected void SetItemDataBound(MenuItem node, bool dataBound) {
            node.SetDataBound(dataBound);
        }


        /// <devdoc>
        /// Allows a derived Menu to set the DataItem on a node
        /// </devdoc>
        protected void SetItemDataItem(MenuItem node, object dataItem) {
            node.SetDataItem(dataItem);
        }


        /// <devdoc>
        /// Allows a derived Menu to set the DataPath on a node
        /// </devdoc>
        protected void SetItemDataPath(MenuItem node, string dataPath) {
            node.SetDataPath(dataPath);
        }

        internal void SetSelectedItem(MenuItem node) {
            Debug.Assert(node == null || node.Owner == this);

            if (_selectedItem != node) {
                if (node != null) {
                    if (node.Depth >= MaximumDepth) {
                        throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDepth));
                    }
                    if (!(node.IsEnabledNoOwner && node.Selectable)) {
                        throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidSelection));
                    }
                }

                // Unselect the previously selected item
                if ((_selectedItem != null) && (_selectedItem.Selected)) {
                    _selectedItem.SetSelected(false);
                }
                _selectedItem = node;
                // Notify the new selected item that it's now selected
                if ((_selectedItem != null) && !_selectedItem.Selected) {
                    _selectedItem.SetSelected(true);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Marks the starting point to begin tracking and saving changes to the
        ///    control as part of the control viewstate.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_staticItemStyle != null) {
                ((IStateManager)_staticItemStyle).TrackViewState();
            }
            if (_staticSelectedStyle != null) {
                ((IStateManager)_staticSelectedStyle).TrackViewState();
            }
            if (_staticHoverStyle != null) {
                ((IStateManager)_staticHoverStyle).TrackViewState();
            }
            if (_staticMenuStyle != null) {
                ((IStateManager)_staticMenuStyle).TrackViewState();
            }
            if (_dynamicItemStyle != null) {
                ((IStateManager)_dynamicItemStyle).TrackViewState();
            }
            if (_dynamicSelectedStyle != null) {
                ((IStateManager)_dynamicSelectedStyle).TrackViewState();
            }
            if (_dynamicHoverStyle != null) {
                ((IStateManager)_dynamicHoverStyle).TrackViewState();
            }
            if (_dynamicMenuStyle != null) {
                ((IStateManager)_dynamicMenuStyle).TrackViewState();
            }
            if (_levelMenuItemStyles != null) {
                ((IStateManager)_levelMenuItemStyles).TrackViewState();
            }
            if (_levelSelectedStyles != null) {
                ((IStateManager)_levelSelectedStyles).TrackViewState();
            }
            if (_levelStyles != null) {
                ((IStateManager)_levelStyles).TrackViewState();
            }
            if (_bindings != null) {
                ((IStateManager)_bindings).TrackViewState();
            }

            ((IStateManager)Items).TrackViewState();
        }

        internal void VerifyRenderingInServerForm() {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }
        }

        #region IPostBackEventHandler implementation

        /// <internalonly/>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }

        protected internal virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            // Do not take any postback into account if the menu is disabled.
            if (!IsEnabled) return;

            EnsureChildControls();
            if (AdapterInternal != null) {
                IPostBackEventHandler pbeh = AdapterInternal as IPostBackEventHandler;
                if (pbeh != null) {
                    pbeh.RaisePostBackEvent(eventArgument);
                }
            }
            else {
                InternalRaisePostBackEvent(eventArgument);
            }
        }

        internal void InternalRaisePostBackEvent(string eventArgument) {
            if (eventArgument.Length == 0) {
                return;
            }

            // Get the path of the node specified in the eventArgument
            string nodePath = HttpUtility.HtmlDecode(eventArgument);
            // Check the number of separator characters in the argument (should not be more than the max depth)
            int matches = 0;
            for (int i = 0; i < nodePath.Length; i++) {
                if (nodePath[i] == TreeView.InternalPathSeparator) {
                    if (++matches >= MaximumDepth) {
                        throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDepth));
                    }
                }
            }
            // Find that node in the tree
            MenuItem node = Items.FindItem(nodePath.Split(TreeView.InternalPathSeparator), 0);

            if (node != null) {
                OnMenuItemClick(new MenuEventArgs(node));
            }
        }
        #endregion
    }
}
