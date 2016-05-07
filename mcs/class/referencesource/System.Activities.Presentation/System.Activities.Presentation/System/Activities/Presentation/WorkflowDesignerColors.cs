//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Media;
    using System.Windows;
    using System.Runtime;
    using System.Activities.Presentation.Internal.PropertyEditing;

    public static class WorkflowDesignerColors
    {
        static ResourceDictionary defaultColors;
        static ResourceDictionary highContrastColors;
        static ResourceDictionary fontAndColorResources;

        public const string WorkflowViewElementBorderColorKey = "WorkflowViewElementBorderColorKey";
        public const string WorkflowViewElementBackgroundColorKey = "WorkflowViewElementBackgroundColorKey";
        public const string WorkflowViewElementSelectedBackgroundColorKey = "WorkflowViewElementSelectedBackgroundColorKey";
        public const string WorkflowViewElementSelectedBorderColorKey = "WorkflowViewElementSelectedBorderColorKey";
        public const string DesignerViewStatusBarBackgroundColorKey = "DesignerViewStatusBarBackgroundColorKey";
        public const string WorkflowViewElementCaptionColorKey = "WorkflowViewElementCaptionBrushColorKey";
        public const string DesignerViewBackgroundColorKey = "DesignerViewBackgroundColorKey";
        public const string DesignerViewShellBarColorGradientBeginKey = "DesignerViewShellBarColorGradientBeginKey";
        public const string DesignerViewShellBarColorGradientEndKey = "DesignerViewShellBarColorGradientEndKey";
        public const string DesignerViewShellBarSelectedColorGradientBeginKey = "DesignerViewShellBarSelectedColorGradientBeginKey";
        public const string DesignerViewShellBarSelectedColorGradientEndKey = "DesignerViewShellBarSelectedColorGradientEndKey";
        public const string DesignerViewShellBarHoverColorGradientBeginKey = "DesignerViewShellBarSelectedHoverGradientBeginKey";
        public const string DesignerViewShellBarHoverColorGradientEndKey = "DesignerViewShellBarSelectedHoverGradientEndKey";
        public const string DesignerViewShellBarControlBackgroundColorKey = "DesignerViewShellBarControlBackgroundColorKey";
        public const string DesignerViewShellBarCaptionActiveColorKey = "DesignerViewShellBarCaptionActiveColorKey";
        public const string DesignerViewShellBarCaptionColorKey = "DesignerViewShellBarCaptionColorKey";
        public const string DesignerViewExpandAllCollapseAllButtonColorKey = "DesignerViewExpandAllCollapseAllButtonColorKey";
        public const string DesignerViewExpandAllCollapseAllButtonMouseOverColorKey = "DesignerViewExpandAllCollapseAllButtonMouseOverColorKey";
        public const string DesignerViewExpandAllCollapseAllPressedColorKey = "DesignerViewExpandAllCollapseAllPressedColorKey";

        public const string ContextMenuBackgroundGradientBeginColorKey = "ContextMenuColorGradientBeginColorKey";
        public const string ContextMenuBackgroundGradientEndColorKey = "ContextMenuColorGradientEndColorKey";
        public const string ContextMenuBorderColorKey = "ContextMenuBorderColorKey";
        public const string ContextMenuIconAreaColorKey = "ContextMenuIconAreaColorKey";
        public const string ContextMenuMouseOverBeginColorKey = "ContextMenuMouseOverBeginColorKey";
        public const string ContextMenuMouseOverMiddle1ColorKey = "ContextMenuMouseOverMiddle1ColorKey";
        public const string ContextMenuMouseOverMiddle2ColorKey = "ContextMenuMouseOverMiddle2ColorKey";
        public const string ContextMenuMouseOverEndColorKey = "ContextMenuMouseOverEndColorKey";
        public const string ContextMenuMouseOverBorderColorKey = "ContextMenuMouseOverBorderColorKey";
        public const string ContextMenuItemTextColorKey = "ContextMenuItemTextColorKey";
        public const string ContextMenuItemTextHoverColorKey = "ContextMenuItemTextHoverColorKey";
        public const string ContextMenuItemTextSelectedColorKey = "ContextMenuItemTextSelectedColorKey";
        public const string ContextMenuItemTextDisabledColorKey = "ContextMenuItemTextDisabledColorKey";
        public const string ContextMenuSeparatorColorKey = "ContextMenuSeparatorColorKey";

        public static string PropertyInspectorTextBrushKey { get { return PropertyInspectorMergedResources.TextBrushKey; } }
        public static string PropertyInspectorSelectedForegroundBrushKey { get { return PropertyInspectorMergedResources.SelectedForegroundBrushKey; } }
        public static string PropertyInspectorSelectedBackgroundBrushKey { get { return PropertyInspectorMergedResources.SelectedBackgroundBrushKey; } }
        public static string PropertyInspectorBackgroundBrushKey { get { return PropertyInspectorMergedResources.BackgroundBrushKey; } }
        public static string PropertyInspectorBorderBrushKey { get { return PropertyInspectorMergedResources.BorderBrushKey; } }
        public static string PropertyInspectorCategoryCaptionTextBrushKey { get { return PropertyInspectorMergedResources.CategoryCaptionTextBrushKey; } }
        public static string PropertyInspectorPaneBrushKey { get { return PropertyInspectorMergedResources.PaneBrushKey; } }
        public static string PropertyInspectorPopupBrushKey { get { return PropertyInspectorMergedResources.PopupBrushKey; } }
        public static string PropertyInspectorToolBarItemHoverBackgroundBrushKey { get { return PropertyInspectorMergedResources.ToolBarItemHoverBackgroundBrushKey; } }
        public static string PropertyInspectorToolBarItemHoverBorderBrushKey { get { return PropertyInspectorMergedResources.ToolBarItemHoverBorderBrushKey; } }
        public static string PropertyInspectorToolBarItemSelectedBackgroundBrushKey { get { return PropertyInspectorMergedResources.ToolBarItemSelectedBackgroundBrushKey; } }
        public static string PropertyInspectorToolBarItemSelectedBorderBrushKey { get { return PropertyInspectorMergedResources.ToolBarItemSelectedBorderBrushKey; } }
        public static string PropertyInspectorToolBarBackgroundBrushKey { get { return PropertyInspectorMergedResources.ToolBarBackgroundBrushKey; } }
        public static string PropertyInspectorToolBarSeparatorBrushKey { get { return PropertyInspectorMergedResources.ToolBarSeparatorBrushKey; } }
        public static string PropertyInspectorToolBarTextBoxBorderBrushKey { get { return PropertyInspectorMergedResources.ToolBarTextBoxBorderBrushKey; } }


        public const string FlowchartExpressionButtonColorKey = "FlowchartExpressionButtonColorKey";
        public const string FlowchartExpressionButtonMouseOverColorKey = "FlowchartExpressionButtonMouseOverColorKey";
        public const string FlowchartExpressionButtonPressedColorKey = "FlowchartExpressionButtonPressedColorKey";

        

        public const string AnnotationBackgroundGradientBeginColorKey = "AnnotationBackgroundGradientBeginColorKey";
        public const string AnnotationBackgroundGradientMiddleColorKey = "AnnotationBackgroundGradientMiddleColorKey";
        public const string AnnotationBackgroundGradientEndColorKey = "AnnotationBackgroundGradientEndColorKey";
        public const string AnnotationBorderColorKey = "AnnotationBorderColorKey";
        public const string AnnotationDockTextColorKey = "AnnotationDockTextColorKey";
        public const string AnnotationUndockTextColorKey = "AnnotationUndockTextColorKey";
        public const string AnnotationDockButtonColorKey = "AnnotationDockButtonColorKey";
        public const string AnnotationDockButtonHoverColorKey = "AnnotationDockButtonHoverColorKey";
        public const string AnnotationDockButtonHoverBorderColorKey = "AnnotationDockButtonHoverBorderColorKey";
        public const string AnnotationDockButtonHoverBackgroundColorKey = "AnnotationDockButtonHoverBackgroundColorKey";

        public const string OutlineViewItemHighlightBackgroundColorKey = "OutlineViewItemHighlightBackgroundColorKey";
        public const string OutlineViewCollapsedArrowBorderColorKey = "OutlineViewCollapsedArrowBorderColorKey";
        public const string OutlineViewCollapsedArrowHoverBorderColorKey = "OutlineViewCollapsedArrowHoverBorderColorKey";
        public const string OutlineViewExpandedArrowColorKey = "OutlineViewExpandedArrowColorKey";
        public const string OutlineViewExpandedArrowBorderColorKey = "OutlineViewExpandedArrowBorderColorKey";
        public const string OutlineViewBackgroundColorKey = "OutlineViewBackgroundColorKey";
        public const string OutlineViewItemSelectedTextColorKey = "OutlineViewTitemSelectedTextColorKey";
        public const string OutlineViewItemTextColorKey = "OutlineViewItemTextColorKey";

        public const string RubberBandRectangleColorKey = "RubberBandRectangleColorKey";

        public static readonly string FontSizeKey = CreateKey("FontSizeKey");
        public static readonly string FontFamilyKey = CreateKey("FontFamilyKey");
        public static readonly string FontWeightKey = CreateKey("FontWeightKey");


        static ResourceDictionary DefaultColors
        {
            get
            {
                if (defaultColors == null)
                {
                    Uri resourceLocator = new Uri(
                        string.Concat(
                        typeof(WorkflowDesignerColors).Assembly.GetName().Name,
                        @";component/System/Activities/Presentation/DefaultColorResources.xaml"),
                        UriKind.RelativeOrAbsolute);
                    defaultColors = (ResourceDictionary)Application.LoadComponent(resourceLocator);
                }
                Fx.Assert(defaultColors != null, "Could not load default color resources.");
                return defaultColors;
            }
        }

        static ResourceDictionary HighContrastColors
        {
            get
            {
                if (highContrastColors == null)
                {
                    Uri resourceLocator = new Uri(
                        string.Concat(
                        typeof(WorkflowDesignerColors).Assembly.GetName().Name,
                        @";component/System/Activities/Presentation/HighContrastColorResources.xaml"),
                        UriKind.RelativeOrAbsolute);
                    highContrastColors = (ResourceDictionary)Application.LoadComponent(resourceLocator);
                }
                Fx.Assert(highContrastColors != null, "Could not load high contrast color resources.");
                return highContrastColors;
            }
        }

        internal static ResourceDictionary FontAndColorResources
        {
            get
            {
                if (WorkflowDesignerColors.fontAndColorResources == null)
                {
                    InitializeDefaultResourceDictionary();
                }

                return WorkflowDesignerColors.fontAndColorResources;
            }
        }

        // Trying to figure out whether or not we are in highcontrast mode is a little tricky.
        // There are two things highcontrast mode and highcontrast color scheme. unfortunately in some platforms ( w2k3) these are not both updated from OS UX
        // here is a good article on this http://blogs.msdn.com/oldnewthing/archive/2008/12/03/9167477.aspx
        // highcontrast mode can be detected easily by using
        // applications in generatel ( e.g. VS) unfortunately handle high contrast color scheme as well, so we are forced to do it
        // This is code is dervied from the way VS does this.

        static bool IsHighContrastEnabled
        {
            get
            {
                if (SystemParameters.HighContrast)
                {
                    return true;
                }

                if (SystemColors.ControlColor == Colors.Black && SystemColors.ControlTextColor == Colors.White)
                {
                    return true;
                }

                if (SystemColors.ControlColor == Colors.White && SystemColors.ControlTextColor == Colors.Black)
                {
                    return true;
                }

                if (SystemColors.ControlColor == Colors.Black && SystemColors.ControlTextColor == Color.FromArgb(0xff, 0x00, 0xff, 0x00))
                {
                    return true;
                }

                return false;
            }
        }


        public static Color WorkflowViewElementBorderColor
        {
            get
            {
                return GetColor(WorkflowDesignerColors.WorkflowViewElementBorderColorKey);
            }
        }

        public static Color WorkflowViewElementBackgroundColor
        {
            get
            {
                return GetColor(WorkflowDesignerColors.WorkflowViewElementBackgroundColorKey);
            }
        }
        public static Color WorkflowViewElementSelectedBackgroundColor
        {
            get
            {
                return GetColor(WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey);
            }
        }

        public static Color GridViewRowHoverColor
        {
            get
            {
                return GetColor(WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey, 0xA0);
            }
        }

        public static Color WorkflowViewElementSelectedBorderColor
        {
            get
            {
                return GetColor(WorkflowDesignerColors.WorkflowViewElementSelectedBorderColorKey);
            }
        }
        public static Color DesignerViewStatusBarBackgroundColor
        {
            get
            {
                return GetColor(WorkflowDesignerColors.DesignerViewStatusBarBackgroundColorKey);
            }
        }
        public static Color WorkflowViewElementCaptionColor
        {
            get
            {
                return GetColor(WorkflowDesignerColors.WorkflowViewElementCaptionColorKey);
            }
        }
        public static Color DesignerViewBackgroundColor
        {
            get
            {
                return GetColor(WorkflowDesignerColors.DesignerViewBackgroundColorKey);
            }
        }

        public static Color DesignerViewShellBarColorGradientBeginColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarColorGradientBeginKey); }
        }

        public static Color DesignerViewShellBarColorGradientEndColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarColorGradientEndKey); }
        }

        public static Color DesignerViewShellBarSelectedColorGradientBeginColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarSelectedColorGradientBeginKey); }
        }

        public static Color DesignerViewShellBarSelectedColorGradientEndColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarSelectedColorGradientEndKey); }
        }

        public static Color DesignerViewShellBarHoverColorGradientBeginColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarHoverColorGradientBeginKey); }
        }

        public static Color DesignerViewShellBarHoverColorGradientEndColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarHoverColorGradientEndKey); }
        }

        public static Color DesignerViewShellBarControlBackgroundColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarControlBackgroundColorKey); }
        }

        public static Color DesignerViewShellBarCaptionActiveColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarCaptionActiveColorKey); }
        }

        public static Color DesignerViewShellBarCaptionColor
        {
            get { return GetColor(WorkflowDesignerColors.DesignerViewShellBarCaptionColorKey); }
        }

        public static Brush DesignerViewExpandAllCollapseAllButtonBrush
        {
            get { return GetBrush(WorkflowDesignerColors.DesignerViewExpandAllCollapseAllButtonColorKey); }
        }

        public static Brush DesignerViewExpandAllCollapseAllButtonMouseOverBrush
        {
            get { return GetBrush(WorkflowDesignerColors.DesignerViewExpandAllCollapseAllButtonMouseOverColorKey); }
        }

        public static Brush DesignerViewExpandAllCollapseAllPressedBrush
        {
            get { return GetBrush(WorkflowDesignerColors.DesignerViewExpandAllCollapseAllPressedColorKey); }
        }

        public static Color ContextMenuBackgroundGradientBeginColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuBackgroundGradientBeginColorKey); }
        }
        public static Color ContextMenuBackgroundGradientEndColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuBackgroundGradientEndColorKey); }
        }

        public static Color ContextMenuBorderColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuBorderColorKey); }
        }

        public static Color ContextMenuIconAreaColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuIconAreaColorKey); }
        }

        public static Color ContextMenuMouseOverBeginColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuMouseOverBeginColorKey); }
        }

        public static Color ContextMenuMouseOverMiddle1Color
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuMouseOverMiddle1ColorKey); }
        }

        public static Color ContextMenuMouseOverMiddle2Color
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuMouseOverMiddle2ColorKey); }
        }

        public static Color ContextMenuMouseOverEndColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuMouseOverEndColorKey); }
        }

        public static Color ContextMenuMouseOverBorderColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuMouseOverBorderColorKey); }
        }

        public static Color ContextMenuItemTextColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuItemTextColorKey); }
        }

        public static Color ContextMenuItemTextHoverColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuItemTextHoverColorKey); }
        }

        public static Color ContextMenuItemTextSelectedColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuItemTextSelectedColorKey); }
        }

        public static Color ContextMenuItemTextDisabledColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuItemTextDisabledColorKey); }
        }

        public static Color ContextMenuSeparatorColor
        {
            get { return GetColor(WorkflowDesignerColors.ContextMenuSeparatorColorKey); }
        }

        public static Brush FlowchartExpressionButtonBrush
        {
            get { return GetBrush(WorkflowDesignerColors.FlowchartExpressionButtonColorKey); }
        }

        public static Brush FlowchartExpressionButtonMouseOverBrush
        {
            get { return GetBrush(WorkflowDesignerColors.FlowchartExpressionButtonMouseOverColorKey); }
        }

        public static Brush FlowchartExpressionButtonPressedBrush
        {
            get { return GetBrush(WorkflowDesignerColors.FlowchartExpressionButtonPressedColorKey); }
        }

        public static Color AnnotationBackgroundGradientBeginColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationBackgroundGradientBeginColorKey); }
        }

        public static Color AnnotationBackgroundGradientMiddleColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationBackgroundGradientMiddleColorKey); }
        }

        public static Color AnnotationBackgroundGradientEndColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationBackgroundGradientEndColorKey); }
        }

        public static Color AnnotationBorderColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationBorderColorKey); }
        }

        public static Color AnnotationDockTextColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationDockTextColorKey); }
        }

        public static Color AnnotationUndockTextColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationUndockTextColorKey); }
        }

        public static Color AnnotationDockButtonColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationDockButtonColorKey); }
        }

        public static Color AnnotationDockButtonHoverColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationDockButtonHoverColorKey); }
        }

        public static Color AnnotationDockButtonHoverBorderColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationDockButtonHoverBorderColorKey); }
        }

        public static Color AnnotationDockButtonHoverBackgroundColor
        {
            get { return GetColor(WorkflowDesignerColors.AnnotationDockButtonHoverBackgroundColorKey); }
        }

        public static Color OutlineViewItemHighlightBackgroundColor
        {
            get { return GetColor(WorkflowDesignerColors.OutlineViewItemHighlightBackgroundColorKey); }
        }

        public static Color OutlineViewCollapsedArrowBorderColor
        {
            get { return GetColor(WorkflowDesignerColors.OutlineViewCollapsedArrowBorderColorKey); }
        }

        public static Color OutlineViewCollapsedArrowHoverBorderColor
        {
            get { return GetColor(WorkflowDesignerColors.OutlineViewCollapsedArrowHoverBorderColorKey); }
        }

        public static Color OutlineViewExpandedArrowColor
        {
            get { return GetColor(WorkflowDesignerColors.OutlineViewExpandedArrowColorKey); }
        }

        public static Color OutlineViewExpandedArrowBorderColor
        {
            get { return GetColor(WorkflowDesignerColors.OutlineViewExpandedArrowBorderColorKey); }
        }

        public static Color OutlineViewBackgroundColor
        {
            get { return GetColor(WorkflowDesignerColors.OutlineViewBackgroundColorKey); }
        }

        public static Color OutlineViewItemSelectedTextColor
        {
            get { return GetColor(WorkflowDesignerColors.OutlineViewItemSelectedTextColorKey); }
        }

        public static Color OutlineViewItemTextColor
        {
            get { return GetColor(WorkflowDesignerColors.OutlineViewItemTextColorKey); }
        }

        public static FontFamily FontFamily
        {
            get
            {
                return GetFontFamily(FontFamilyKey);
            }
        }

        public static double FontSize
        {
            get
            {
                return GetFontSize(FontSizeKey);
            }
        }

        public static FontWeight FontWeight
        {
            get
            {
                return GetFontWeight(FontWeightKey);
            }
        }

        internal static Brush RubberBandRectangleBrush
        {
            get { return GetBrush(WorkflowDesignerColors.RubberBandRectangleColorKey); }
        }

        private static Brush GetBrush(string colorKey)
        {
            SolidColorBrush brush = 
                (SolidColorBrush)GetFontOrColor(colorKey, WorkflowDesignerColors.DefaultColors[colorKey], WorkflowDesignerColors.HighContrastColors[colorKey]);

            if (brush.CanFreeze)
            {
                brush.Freeze();
            }
            return brush;
        }

        private static Color GetColor(string colorKey)
        {
            SolidColorBrush brush = (SolidColorBrush)GetBrush(colorKey);

            return brush.Color;
        }

        private static Color GetColor(string colorKey, byte alpha)
        {
            Color color = GetColor(colorKey);
            color.A = alpha;
            return color;
        }

        private static FontFamily GetFontFamily(string key)
        {
            return (FontFamily)GetFontOrColor(key, SystemFonts.MessageFontFamily, SystemFonts.MessageFontFamily);
        }

        private static double GetFontSize(string key)
        {
            return (double)GetFontOrColor(key, SystemFonts.MessageFontSize, SystemFonts.MessageFontSize);
        }

        private static FontWeight GetFontWeight(string key)
        {
            return (FontWeight)GetFontOrColor(key, SystemFonts.MessageFontWeight, SystemFonts.MessageFontWeight);
        }

        private static object GetFontOrColor(string key, object defaultValue, object valueInHighContrast)
        {
            if (WorkflowDesignerColors.FontAndColorResources.Contains(key))
            {
                return WorkflowDesignerColors.FontAndColorResources[key];
            }
            else if (IsHighContrastEnabled)
            {
                return valueInHighContrast;
            }
            else
            {
                return defaultValue;
            }
        }

        static string CreateKey(string name)
        {
            //return AdornerResources.CreateResourceKey(typeof(PropertyInspectorMergedResources), name);
            return name;
        }

        private static void InitializeDefaultResourceDictionary()
        {
            ResourceDictionary resources = new ResourceDictionary();
            resources[WorkflowDesignerColors.PropertyInspectorTextBrushKey] = new SolidColorBrush(SystemColors.ControlTextColor);
            resources[WorkflowDesignerColors.PropertyInspectorBackgroundBrushKey] = new SolidColorBrush(SystemColors.WindowColor);
            resources[WorkflowDesignerColors.PropertyInspectorBorderBrushKey] = new SolidColorBrush(SystemColors.ControlDarkColor);
            resources[WorkflowDesignerColors.PropertyInspectorPaneBrushKey] = new SolidColorBrush(SystemColors.ControlColor);
            resources[WorkflowDesignerColors.PropertyInspectorSelectedBackgroundBrushKey] = new SolidColorBrush(SystemColors.HighlightColor);
            resources[WorkflowDesignerColors.PropertyInspectorSelectedForegroundBrushKey] = new SolidColorBrush(SystemColors.HighlightTextColor);
            resources[WorkflowDesignerColors.PropertyInspectorToolBarItemHoverBackgroundBrushKey] = new SolidColorBrush(SystemColors.GradientActiveCaptionColor);
            resources[WorkflowDesignerColors.PropertyInspectorToolBarItemHoverBorderBrushKey] = new SolidColorBrush(SystemColors.ActiveCaptionTextColor);
            resources[WorkflowDesignerColors.PropertyInspectorToolBarItemSelectedBackgroundBrushKey] = new SolidColorBrush(SystemColors.GradientInactiveCaptionColor);
            resources[WorkflowDesignerColors.PropertyInspectorToolBarItemSelectedBorderBrushKey] = new SolidColorBrush(SystemColors.ActiveCaptionTextColor);
            resources[WorkflowDesignerColors.PropertyInspectorToolBarBackgroundBrushKey] = new SolidColorBrush(SystemColors.ControlColor);
            resources[WorkflowDesignerColors.PropertyInspectorToolBarSeparatorBrushKey] = new SolidColorBrush(SystemColors.ControlDarkColor);
            resources[WorkflowDesignerColors.PropertyInspectorToolBarTextBoxBorderBrushKey] = new SolidColorBrush(SystemColors.ControlDarkDarkColor);            

            WorkflowDesignerColors.fontAndColorResources = resources;               
        }
    }
}
