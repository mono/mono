//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors;

    // <summary>
    // DataTemplateSelector we use in some instances of ChoiceEditor (namely sub-property editor)
    // to detect whether the given item is being displayed in the popup or as an inline item.
    // Based on that determination, it returns the appropriate DataTemplate.  This is a work-around
    // for the problem where we can't determine which NewItemTypeFactory instantiated a given instance.
    // Hence, we show the instance Type inline the ComboBox and the factory DisplayName in the drop-down.
    // Ideally, we would want to use a different control to handle this scenario.
    // </summary>
    internal class QuickItemTemplateSelector : DataTemplateSelector 
    {

        private DataTemplate _popupTemplate;
        private DataTemplate _inlineTemplate;

        public DataTemplate PopupTemplate 
        {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            get { return _popupTemplate; }
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            set { _popupTemplate = value; }
        }

        public DataTemplate InlineTemplate 
        {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            get { return _inlineTemplate; }
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            set { _inlineTemplate = value; }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) 
        {
            return HasChoiceEditorParent(container) ? _inlineTemplate : _popupTemplate;
        }

        private bool HasChoiceEditorParent(DependencyObject element) 
        {
            while (element != null) 
            {
                element = VisualTreeHelper.GetParent(element);
                if (element != null && typeof(ChoiceEditor).IsAssignableFrom(element.GetType()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
