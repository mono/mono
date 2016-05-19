//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{

    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Activities.Presentation.Internal.PropertyEditing.Resources;

    // <summary>
    // This class merges the font and brush resources for PropertyInspector.
    // PropertyInspectorMergedResources uses AdornerResources to hook on to the theme-changes to
    // re-apply the color values.
    // </summary>
    [SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes", Justification = "Suppress to avoid churning the code base.")]
    internal static class PropertyInspectorMergedResources 
    {
        private static readonly string _fontSizeKey = CreateKey("FontSizeKey");
        private static readonly string _fontFamilyKey = CreateKey("FontFamilyKey");
        private static readonly string _fontWeightKey = CreateKey("FontWeightKey");
        private static readonly string _textBrushKey = CreateKey("TextBrushKey");
        private static readonly string _selectedForegroundBrushKey = CreateKey("SelectedForegroundBrushKey");
        private static readonly string _selectedBackgroundBrushKey = CreateKey("SelectedBackgroundBrushKey");
        private static readonly string _backgroundBrushKey = CreateKey("BackgroundBrushKey");
        private static readonly string _borderBrushKey = CreateKey("BorderBrushKey");
        private static readonly string _categoryCaptionTextBrushKey = CreateKey("CategoryCaptionTextBrushKey");
        private static readonly string _paneBrushKey = CreateKey("PaneBrushKey");
        private static readonly string _popupBrushKey = CreateKey("PopupBrushKey");
        private static readonly string _toolBarItemHoverBackgroundBrushKKey = CreateKey("ToolBarItemHoverBackgroundBrushKey");
        private static readonly string _toolBarItemHoverBorderBrushKey = CreateKey("ToolBarItemHoverBorderBrushKey");
        private static readonly string _toolBarItemSelectedBackgroundBrushKey = CreateKey("ToolBarItemSelectedBackgroundBrushKey");
        private static readonly string _toolBarItemSelectedBorderBrushKey = CreateKey("ToolBarItemSelectedBorderBrushKey");
        private static readonly string _toolBarBackgroundBrushKey = CreateKey("ToolBarBackgroundBrushKey");
        private static readonly string _toolBarSeparatorBrushKey = CreateKey("ToolBarSeparatorBrushKey");
        private static readonly string _toolBarTextBoxBorderBrushKey = CreateKey("ToolBarTextBoxBorderBrushKey");        

        // <summary>
        // Specifies a FontSizeKey.
        // </summary>
        public static string FontSizeKey 
        {
            get { return _fontSizeKey; }
        }

        // <summary>
        // Specifies FontFamilyKey
        // </summary>
        public static string FontFamilyKey 
        {
            get { return _fontFamilyKey; }
        }

        // <summary>
        // Specifies FontWeightKey
        // </summary>
        public static string FontWeightKey 
        {
            get { return _fontWeightKey; }
        }


        public static string TextBrushKey 
        {
            get { return _textBrushKey; }
        }

        public static string SelectedForegroundBrushKey 
        {
            get { return _selectedForegroundBrushKey; }
        }



        public static string SelectedBackgroundBrushKey 
        {
            get { return _selectedBackgroundBrushKey; }
        }



        public static string BackgroundBrushKey 
        {
            get { return _backgroundBrushKey; }
        }



        public static string BorderBrushKey 
        {
            get { return _borderBrushKey; }
        }




        public static string CategoryCaptionTextBrushKey 
        {
            get { return _categoryCaptionTextBrushKey; }
        }




        public static string PaneBrushKey 
        {
            get { return _paneBrushKey; }
        }




        public static string PopupBrushKey 
        {
            get { return _popupBrushKey; }
        }


      


        public static string ToolBarItemHoverBackgroundBrushKey 
        {
            get { return _toolBarItemHoverBackgroundBrushKKey; }
        }


   

        public static string ToolBarItemHoverBorderBrushKey 
        {
            get { return _toolBarItemHoverBorderBrushKey; }
        }




        public static string ToolBarItemSelectedBackgroundBrushKey 
        {
            get { return _toolBarItemSelectedBackgroundBrushKey; }
        }




        public static string ToolBarItemSelectedBorderBrushKey 
        {
            get { return _toolBarItemSelectedBorderBrushKey; }
        }



        public static string ToolBarBackgroundBrushKey 
        {
            get { return _toolBarBackgroundBrushKey; }
        }




        public static string ToolBarSeparatorBrushKey 
        {
            get { return _toolBarSeparatorBrushKey; }
        }




        public static string ToolBarTextBoxBorderBrushKey 
        {
            get { return _toolBarTextBoxBorderBrushKey; }
        }


        //HelperFunctions

        // <summary>
        // Creates a new key.  Used at static construction time.
        // </summary>
        private static string CreateKey(string name) 
        {
            //return AdornerResources.CreateResourceKey(typeof(PropertyInspectorMergedResources), name);
            return name;
        }

      

        //<summary>
        //return (Collection) for custom collection editor defined in other assemblies to use, e.g. DynamicArgumentDictionary
        //<summary>
        public static string DefaultCollectionStringValue
        {
            get
            {                
                return System.Activities.Presentation.Internal.Properties.Resources.PropertyEditing_DefaultCollectionStringValue;
            }
        }

    }
}
