//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Internal.PropertyEditing.Editors
{
    using System.Windows.Controls;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors;
    using System.Activities.Presentation.PropertyEditing;
    using System.Windows.Media;
    using System.Windows.Threading;

    partial class FlagEditor : ComboBox
    {
        bool isCommitting = false;

        public static readonly DependencyProperty FlagTypeProperty =
            DependencyProperty.Register("FlagType", typeof(Type), typeof(FlagEditor), new PropertyMetadata(null));

        public Type FlagType
        {
            get { return (Type)GetValue(FlagTypeProperty); }
            set { SetValue(FlagTypeProperty, value); }
        }

        public FlagEditor()
        {
            InitializeComponent();
        }

        void Cancel()
        {
            BindingExpression binding = this.GetBindingExpression(ComboBox.TextProperty);
            binding.UpdateTarget();
        }

        void Commit()
        {
            // In case of error, the popup can make the control lose focus; we don't want to commit twice.
            if (!this.isCommitting)
            {
                BindingExpression binding = this.GetBindingExpression(ComboBox.TextProperty);
                if (binding != null)
                {
                    this.isCommitting = true;
                    try
                    {
                        binding.UpdateSource();
                    }
                    catch (ArgumentException exception)
                    {
                        ErrorReporting.ShowErrorMessage(exception.Message);
                        binding.UpdateTarget();
                    }
                    this.isCommitting = false;
                }
            }
        }

        void Finish()
        {
            ValueEditorUtils.ExecuteCommand(PropertyValueEditorCommands.FinishEditing, this, null);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                this.IsDropDownOpen = true;
                e.Handled = true;
            }
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (this.IsDropDownOpen)
                {
                    this.IsDropDownOpen = false;
                }
                this.Commit();
                if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0)
                {
                    this.Finish();
                }
                //Handle this event so that the combo box item is not applied to the text box on "Enter".
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.Cancel();
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);
            this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
            {
                // The item should contains the panel we placed in the combobox,
                // and we should always have one and only one item.
                ComboBoxItem comboBoxItem = 
                    this.ItemContainerGenerator.ContainerFromIndex(0) as ComboBoxItem;
                if (comboBoxItem != null && VisualTreeHelper.GetChildrenCount(comboBoxItem) > 0)
                {
                    comboBoxItem.Focusable = false;
                    StackPanel panel = VisualTreeHelper.GetChild(comboBoxItem, 0) as StackPanel;
                    if (panel != null && VisualTreeHelper.GetChildrenCount(panel) > 0)
                    {
                        // focus on the first UIElement on the panel.
                        UIElement item = VisualTreeHelper.GetChild(panel, 0) as UIElement;
                        if (item != null)
                        {
                            item.Focus();
                        }
                    }
                }

            }));
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (!this.IsKeyboardFocusWithin)
            {
                this.Commit();
            }
            base.OnLostKeyboardFocus(e);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            this.Commit();
            base.OnDropDownClosed(e);
        }
    }
}
