//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Threading;

    partial class CaseKeyBox : UserControl, ICaseKeyBoxView
    {
        public static readonly DependencyProperty DisplayHintTextProperty =
            DependencyProperty.Register("DisplayHintText", typeof(bool), typeof(CaseKeyBox));

        public static readonly DependencyProperty LabelTextProperty =
          DependencyProperty.Register("LabelText", typeof(string), typeof(CaseKeyBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(CaseKeyBox), new PropertyMetadata(OnValueChanged));

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.Register("ValueType", typeof(Type), typeof(CaseKeyBox), new PropertyMetadata(OnValueTypeChanged));
        
        public static RoutedEvent ValueCommittedEvent =
            EventManager.RegisterRoutedEvent("ValueCommitted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CaseKeyBox));

        public static RoutedEvent EditCancelledEvent =
            EventManager.RegisterRoutedEvent("EditCancelled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CaseKeyBox));

        public static readonly DependencyProperty CaseKeyValidationCallbackProperty =
            DependencyProperty.Register("CaseKeyValidationCallback", typeof(CaseKeyValidationCallbackDelegate), typeof(CaseKeyBox));

        public static readonly DependencyProperty ErrorCallbackProperty =
            DependencyProperty.Register("ErrorCallback", typeof(Action<CaseKeyBox>), typeof(CaseKeyBox));

        public static readonly DependencyProperty CommitExplicitlyProperty =
            DependencyProperty.Register("CommitExplicitly", typeof(bool), typeof(CaseKeyBox), new PropertyMetadata(false));

        Control visibleBox;


        public CaseKeyBox()
        {
            this.ViewModel = new CaseKeyBoxViewModel(this);
            InitializeComponent();
        }

        public event RoutedEventHandler ValueCommitted
        {
            add { AddHandler(ValueCommittedEvent, value); }
            remove { RemoveHandler(ValueCommittedEvent, value); }
        }

        public virtual void OnValueCommitted()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ValueCommittedEvent;
            RaiseEvent(args);
        }

        public event RoutedEventHandler EditCancelled
        {
            add { AddHandler(EditCancelledEvent, value); }
            remove { RemoveHandler(EditCancelledEvent, value); }
        }

        public virtual void OnEditCancelled()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = EditCancelledEvent;
            RaiseEvent(args);
        }

        public CaseKeyValidationCallbackDelegate CaseKeyValidationCallback
        {
            get { return (CaseKeyValidationCallbackDelegate)GetValue(CaseKeyValidationCallbackProperty); }
            set { SetValue(CaseKeyValidationCallbackProperty, value); }
        }

        public Action<CaseKeyBox> ErrorCallback
        {
            get { return (Action<CaseKeyBox>)GetValue(ErrorCallbackProperty); }
            set { SetValue(ErrorCallbackProperty, value); }
        }

        public bool CommitExplicitly
        {
            get { return (bool)GetValue(CommitExplicitlyProperty); }
            set { SetValue(CommitExplicitlyProperty, value); }
        }

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        void DisableKeyboardLostFocus()
        {
            if (this.visibleBox != null)
            {
                this.visibleBox.LostKeyboardFocus -= OnLostKeyboardFocus;
            }
        }

        void EnableKeyboardLostFocus()
        {
            if (!this.CommitExplicitly)
            {
                if (this.visibleBox != null)
                {
                    this.visibleBox.LostKeyboardFocus += OnLostKeyboardFocus;
                }
            }
        }

        void ReportError(string errorMessage)
        {
            // Invoking error message box will cause LostFocus of the control.
            // Thus we need to disable LostFocus first and then add the handlers back.
            DisableKeyboardLostFocus();
            ErrorReporting.ShowErrorMessage(errorMessage);

            this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
            {
                if (this.ErrorCallback != null)
                {
                    this.ErrorCallback(this);
                    this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                    {
                        RegainFocus();
                        EnableKeyboardLostFocus();
                    }));
                }
                else
                {
                    RegainFocus();
                    EnableKeyboardLostFocus();
                }
            }));
        }

        void OnBoxMouseUp(object sender, MouseButtonEventArgs e)
        {
            // disable the context menu for textbox and combobox
            if (e.ChangedButton == MouseButton.Right && e.RightButton == MouseButtonState.Released)
            {
                e.Handled = true;
            }
        }

        #region ICaseKeyBoxView Implementation

        public bool DisplayHintText
        {
            get { return (bool)GetValue(DisplayHintTextProperty); }
            set { SetValue(DisplayHintTextProperty, value); }
        }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public Type ValueType
        {
            get { return (Type)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        public void RegainFocus()
        {
            if (this.visibleBox != null)
            {
                Keyboard.Focus((IInputElement)this.visibleBox);
            }
        }

        #endregion

        #region Delegating Event Handlers

        public CaseKeyBoxViewModel ViewModel { get; set; }

        static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((CaseKeyBox)sender).ViewModel.OnValueChanged();
        }

        static void OnValueTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((CaseKeyBox)sender).ViewModel.OnValueTypeChanged();
        }

        void OnLabelGotFocus(object sender, RoutedEventArgs e)
        {
            this.ViewModel.OnLabelGotFocus();
            e.Handled = true;
        }

        void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;

            if (ComboBoxHelper.ShouldFilterUnnecessaryComboBoxEvent(sender as ComboBox))
            {
                return;
            }

            CommitChanges();
        }

        public bool CommitChanges()
        {
            UpdateSource(this.visibleBox);
            if (this.CommitExplicitly || this.ViewModel.TextHasBeenChanged())
            {
                string reason = null;
                if (!this.ViewModel.CanResolveInputText(out reason))
                {
                    ReportError(reason);
                    return false;
                }
                else
                {
                    return this.ViewModel.OnLostFocus();
                }
            }
            else
            {
                CancelChanges();
                return false;
            }
        }

        public void CancelChanges()
        {
            DisableKeyboardLostFocus();
            this.ViewModel.OnEscapePressed(); // simulate cancel
        }

        void OnBoxLoaded(object sender, RoutedEventArgs e)
        {
            UIElement box = (UIElement)sender;
            ComboBox comboBox = box as ComboBox;
            if (comboBox != null && comboBox.IsVisible)
            {
                ComboBoxHelper.SynchronizeComboBoxSelection(comboBox, this.ViewModel.Text);
            }
            if (box.IsVisible)
            {
                box.Focus();
            }
            Control control = sender as Control;
            if (control != null && control.Visibility == Visibility.Visible)
            {
                this.visibleBox = control;
                EnableKeyboardLostFocus();
            }

            this.ViewModel.SaveOldText();
        }

        void OnBoxUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.visibleBox != null)
            {
                DisableKeyboardLostFocus();
                this.visibleBox = null;
            }
        }

        void OnBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (!CommitExplicitly)
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    CancelChanges();
                }
                else if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    CommitChanges();
                }
            }
        }

        void UpdateSource(object sender)
        {
            if (sender is TextBox)
            {
                BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                if (binding != null)
                {
                    binding.UpdateSource();
                }
            }
            else if (sender is ComboBox)
            {
                BindingExpression binding = ((ComboBox)sender).GetBindingExpression(ComboBox.TextProperty);
                if (binding != null)
                {
                    binding.UpdateSource();
                }
            }
        }

        #endregion

        public void ResetText()
        {
            this.ViewModel.ResetText();
        }
    }

}
