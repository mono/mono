//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Activities.Presentation.PropertyEditing;
    using System.Runtime;
    using System.Activities.Presentation;

    // <summary>
    // This is a fix for a 















    class EditModeSwitchButtonKeyboardFix 
    {

        // <summary>
        // Property used to correct a problem with EditModeSwitchButton.  This property should only be
        // applied to EditModeSwitchButton class instances.
        // </summary>
        public static readonly DependencyProperty ApplyFixProperty = DependencyProperty.RegisterAttached(
            "ApplyFix",
            typeof(bool),
            typeof(EditModeSwitchButtonKeyboardFix),
            new PropertyMetadata(false, new PropertyChangedCallback(OnApplyFixPropertyChanged)));

        private bool _clickInitiatedByMouse;

        private EditModeSwitchButtonKeyboardFix(EditModeSwitchButton button) 
        {
            button.Click += new RoutedEventHandler(OnEditModeSwitchButtonClick);
            button.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(OnEditModeSwitchButtonPreviewMouseLeftButtonUp);
        }

        // <summary>
        // Gets the value of ApplyFix property from the specified DependencyObject.
        // </summary>
        // <param name="obj"></param>
        // <returns></returns>
        public static bool GetApplyFix(DependencyObject obj) 
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            return (bool)obj.GetValue(ApplyFixProperty);
        }

        // <summary>
        // Sets the value of ApplyFix property onto the specified DependencyObject.  Only
        // instances of EditModeSwitchButton classes should be used as the targets of this property.
        // </summary>
        // <param name="obj"></param>
        // <param name="value"></param>
        public static void SetApplyFix(DependencyObject obj, bool value) 
        {
            if (obj == null)
            {
                throw FxTrace.Exception.ArgumentNull("obj");
            }

            obj.SetValue(ApplyFixProperty, value);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        private static void OnApplyFixPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) 
        {
            EditModeSwitchButton button = obj as EditModeSwitchButton;
            if (button == null) 
            {
                Debug.Fail("EditModeSwitchButtonKeyboardFix.ApplyFix fix can only be applied to EditModeSwitchButton instances.");
                return;
            }
            if (object.Equals(e.NewValue, true)) 
            {
                // Instantiating this class will make itself hook into EditModeSwitchButton's events,
                // hence not be chewed up by garbage collector
                new EditModeSwitchButtonKeyboardFix(button);
            }
        }

        private void OnEditModeSwitchButtonPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) 
        {
            _clickInitiatedByMouse = true;

            ((DispatcherObject)sender).Dispatcher.BeginInvoke(DispatcherPriority.Input, new MethodInvoker(delegate() 
            {
                _clickInitiatedByMouse = false;
            }));
        }

        private void OnEditModeSwitchButtonClick(object sender, RoutedEventArgs e) 
        {
            if (_clickInitiatedByMouse)
            {
                return;
            }

            EditModeSwitchButton button = e.OriginalSource as EditModeSwitchButton;
            Fx.Assert(button != null, "Expected to see the EditModeSwitchButton at this point.");
            if (button == null)
            {
                return;
            }

            // At this point the click was initiated using the Invoke AutomationPeer pattern or
            // by using the keyboard.  So, make sure that the EditModeSwitchButton.OnMouseDown
            // button still executes.
            // Invoke the appropriate command
            switch (button.TargetEditMode) 
            {
                case PropertyContainerEditMode.Inline:
                    PropertyValueEditorCommands.ShowInlineEditor.Execute(null, button);
                    break;
                case PropertyContainerEditMode.ExtendedPopup:
                    PropertyValueEditorCommands.ShowExtendedPopupEditor.Execute(null, button);
                    break;
                case PropertyContainerEditMode.ExtendedPinned:
                    PropertyValueEditorCommands.ShowExtendedPinnedEditor.Execute(null, button);
                    break;
                case PropertyContainerEditMode.Dialog:
                    PropertyValueEditorCommands.ShowDialogEditor.Execute(null, button);
                    break;
                default:
                    Debug.Fail(string.Format(
                        CultureInfo.CurrentCulture,
                        "EditModeSwitchButtonKeyboardFix does not yet support PropertyContainerEditMode '{0}'.",
                        button.TargetEditMode.ToString()));
                    break;
            }
        }
        private delegate void MethodInvoker();
    }
}
