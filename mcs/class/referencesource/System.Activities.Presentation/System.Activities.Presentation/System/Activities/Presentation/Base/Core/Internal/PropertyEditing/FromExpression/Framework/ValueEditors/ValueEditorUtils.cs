// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -------------------------------------------------------------------

//Cider comment:
//  - Removed class MouseCursor

//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\ValueEditors
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;

    internal enum UpdateBindingType
    {
        Source,
        Target,
        Both
    }
    internal static class ValueEditorUtils
    {
        // This property determines whether the commit keys (enter and escape) are marked handled by value editors or not.  It inherits
        // so can be set at any point in the tree, and all ValueEditors below that point in the UI will use this behavior.
        public static readonly DependencyProperty HandlesCommitKeysProperty = DependencyProperty.RegisterAttached("HandlesCommitKeys", typeof(bool), typeof(ValueEditorUtils), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        public static bool GetHandlesCommitKeys(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(ValueEditorUtils.HandlesCommitKeysProperty);
        }

        public static void SetHandlesCommitKeys(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(ValueEditorUtils.HandlesCommitKeysProperty, value);
        }

        public static void UpdateBinding(FrameworkElement element, DependencyProperty property, bool updateSource)
        {
            ValueEditorUtils.UpdateBinding(element, property, (updateSource ? UpdateBindingType.Both : UpdateBindingType.Target));
        }

        public static void UpdateBinding(FrameworkElement element, DependencyProperty property, UpdateBindingType updateType)
        {
            BindingExpression bindingExpression = element.GetBindingExpression(property);
            if (bindingExpression != null)
            {
                // If desired, push the current text value to the source of the binding.
                if (updateType == UpdateBindingType.Source || updateType == UpdateBindingType.Both)
                {
                    bindingExpression.UpdateSource();
                }

                // Update the text from the source of the binding.
                if (updateType == UpdateBindingType.Target || updateType == UpdateBindingType.Both)
                {
                    bindingExpression.UpdateTarget();
                }
            }
        }

        public static void ExecuteCommand(ICommand command, IInputElement element, object parameter)
        {
            RoutedCommand routedCommand = command as RoutedCommand;
            if (routedCommand != null)
            {
                if (routedCommand.CanExecute(parameter, element))
                {
                    routedCommand.Execute(parameter, element);
                }
            }
            else
            {
                if (command != null && command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
        }
    }


}
