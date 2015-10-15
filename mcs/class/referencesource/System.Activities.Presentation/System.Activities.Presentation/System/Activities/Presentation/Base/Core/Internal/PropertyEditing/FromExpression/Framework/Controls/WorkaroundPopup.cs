// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Controls
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.UserInterface;

    // <summary>
    // This class contains specific behavior for the Popup associated with PropertyContainer.
    // Basically, it is a workaround for Windows OS 







    internal class WorkaroundPopup : Popup
    {
        private bool releasingCapture = false;

        protected override void OnOpened(EventArgs e)
        {
            this.releasingCapture = false;

            if (this.Child != null)
            {
                this.Child.Focusable = true;
                this.Child.Focus();
                Mouse.Capture(this.Child, CaptureMode.SubTree);
            }
            this.SetValue(FocusScopeManager.FocusScopePriorityProperty, 1);
            base.OnOpened(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
        }

        protected override void OnLostMouseCapture(System.Windows.Input.MouseEventArgs e)
        {
            object sender = this;
            // This code is a stripped down implementation of Popup.OnMouseLostCapture
            if (!this.releasingCapture && Mouse.Captured != this.Child)
            {
                if (e.OriginalSource == this.Child)
                {
                    if (Mouse.Captured == null)
                    {
                        this.IsOpen = false;
                    }
                }
                else if (this.IsDescendentOfPopup(sender as DependencyObject))
                {
                    if (this.IsOpen && Mouse.Captured == null)
                    {
                        Mouse.Capture(this.Child, CaptureMode.SubTree);
                    }
                }
                else
                {
                    this.IsOpen = false;
                }
            }
            base.OnLostMouseCapture(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            // Check if the mouse down occured within the popup, if it did, leave the popup open.  If it didn't, then close
            // the popup and release capture.
            if (e.OriginalSource == this.Child && this.Child.InputHitTest(e.GetPosition(this.Child)) == null)
            {
                this.IsOpen = false;
                this.ReleaseChildMouseCapture();
            }
            base.OnMouseDown(e);
        }

        private bool IsDescendentOfPopup(DependencyObject currentObject)
        {
            while (currentObject != null)
            {
                if (currentObject == this || currentObject == this.Child)
                {
                    return true;
                }
                currentObject = VisualTreeHelper.GetParent(currentObject);
            }

            return false;
        }

        private void ReleaseChildMouseCapture()
        {
            if (Mouse.Captured == this.Child)
            {
                this.releasingCapture = true;
                Mouse.Capture(null);
                this.releasingCapture = false;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.IsOpen = false;
                this.ReleaseChildMouseCapture();
            }
            base.OnKeyDown(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            this.ReleaseChildMouseCapture();
            base.OnClosed(e);
        }
    }
}
