// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------

//Cider comment
//  This is used by PropertInspector\CategoryContainer.xaml 
//  For example automation:AutomationElement.Id="CategoryCheckBox"
//  I'm not sure that this is actually necessary 
//  But by including this we minimize the changes to CategoryContainer.xaml

//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Diagnostics
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Diagnostics.Automation
{
    using System;
    using System.Windows;
    using System.Activities.Presentation;

    // <summary>
    // This DP is intended to be used in XAML property binding scenarios since FrameworkElement.Name is no longer available.
    // </summary>
    internal static class AutomationElement
    {
        public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached("Id", typeof(string), typeof(AutomationElement));

        public static string GetId(DependencyObject o)
        {
            if (o == null)
            {
                throw FxTrace.Exception.ArgumentNull("o");
            }

            return (string)o.GetValue(AutomationElement.IdProperty);
        }

        public static void SetId(DependencyObject o, string val)
        {
            if (o == null)
            {
                throw FxTrace.Exception.ArgumentNull("o");
            }

            o.SetValue(AutomationElement.IdProperty, val);
        }
    }
}
