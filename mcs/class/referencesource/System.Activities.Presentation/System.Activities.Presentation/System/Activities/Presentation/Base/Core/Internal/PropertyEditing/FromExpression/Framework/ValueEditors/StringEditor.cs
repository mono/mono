// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\ValueEditors
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using System.Activities.Presentation.View;

    internal class StringEditor : TextBox
    {

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(StringEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(StringEditor.ValueChanged), null, false, UpdateSourceTrigger.PropertyChanged));
        public static readonly DependencyProperty IsNinchedProperty = DependencyProperty.Register("IsNinched", typeof(bool), typeof(StringEditor), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(StringEditor.IsNinchedChanged)));
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(StringEditor), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(StringEditor.IsEditingChanged)));

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(double), typeof(StringEditor), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty BorderWidthProperty = DependencyProperty.Register("BorderWidth", typeof(double), typeof(StringEditor), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty BeginCommandProperty = DependencyProperty.Register("BeginCommand", typeof(ICommand), typeof(StringEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty CommitCommandProperty = DependencyProperty.Register("CommitCommand", typeof(ICommand), typeof(StringEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(StringEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty FinishEditingCommandProperty = DependencyProperty.Register("FinishEditingCommand", typeof(ICommand), typeof(StringEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty LostFocusCommandProperty = DependencyProperty.Register("LostFocusCommand", typeof(ICommand), typeof(StringEditor), new PropertyMetadata(null));

        private LostFocusAction lostFocusAction = LostFocusAction.None;
        private bool ignoreTextChanges = false;

        public StringEditor()
        {
            this.CommandBindings.Add(new CommandBinding( DesignerView.CommitCommand, OnDesignerViewCommitExecute));
        }

        public string Value
        {
            get { return (string)this.GetValue(StringEditor.ValueProperty); }
            set { this.SetValue(StringEditor.ValueProperty, value); }
        }

        public bool IsNinched
        {
            get { return (bool)this.GetValue(StringEditor.IsNinchedProperty); }
            set { this.SetValue(StringEditor.IsNinchedProperty, value); }
        }

        public bool IsEditing
        {
            get { return (bool)this.GetValue(StringEditor.IsEditingProperty); }
            set { this.SetValue(StringEditor.IsEditingProperty, value); }
        }

        public double CornerRadius
        {
            get { return (double)this.GetValue(StringEditor.CornerRadiusProperty); }
            set { this.SetValue(StringEditor.CornerRadiusProperty, value); }
        }

        public double BorderWidth
        {
            get { return (double)this.GetValue(StringEditor.BorderWidthProperty); }
            set { this.SetValue(StringEditor.BorderWidthProperty, value); }
        }

        public ICommand BeginCommand
        {
            get { return (ICommand)this.GetValue(StringEditor.BeginCommandProperty); }
            set { this.SetValue(StringEditor.BeginCommandProperty, value); }
        }

        public ICommand CommitCommand
        {
            get { return (ICommand)this.GetValue(StringEditor.CommitCommandProperty); }
            set { this.SetValue(StringEditor.CommitCommandProperty, value); }
        }

        public ICommand CancelCommand
        {
            get { return (ICommand)this.GetValue(StringEditor.CancelCommandProperty); }
            set { this.SetValue(StringEditor.CancelCommandProperty, value); }
        }

        public ICommand FinishEditingCommand
        {
            get { return (ICommand)this.GetValue(StringEditor.FinishEditingCommandProperty); }
            set { this.SetValue(StringEditor.FinishEditingCommandProperty, value); }
        }

        public ICommand LostFocusCommand
        {
            get { return (ICommand)this.GetValue(StringEditor.LostFocusCommandProperty); }
            set { this.SetValue(StringEditor.LostFocusCommandProperty, value); }
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringEditor editor = d as StringEditor;
            if (editor != null)
            {
                editor.UpdateTextFromValue();
            }
        }

        private static void IsNinchedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringEditor editor = d as StringEditor;
            if (editor != null)
            {
                editor.UpdateTextFromValue();
            }
        }

        private static void IsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StringEditor editor = d as StringEditor;
            if (editor != null)
            {
                bool isNowEditing = (bool)e.NewValue;
                if (isNowEditing)
                {
                    if (editor.IsInitialized)
                    {
                        editor.Focus();
                    }
                    else
                    {
                        editor.PostFocusCallback();
                    }
                }
            }
        }

        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
        // ###################################################
        // StringEditor should select text only when it gets the logical focus and nowhere else.
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            this.SelectAll();
        }
        // ###################################################
        // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
        // ###################################################


        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (!this.IsReadOnly)
            {
                this.IsEditing = true;
            }
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            // We have to commit on _preview_ lost focus because the ---- tab control does
            // work before we loose focus which causes keyframing stuff to happen out of order.
            base.OnPreviewLostKeyboardFocus(e);
            InternalLostFocus();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            // We have to commit on lost focus as well, since when WPF application
            // loosing focus to another Win32 window, e.g. the MFC Artboard in Acrylic,
            // the PreviewLostKeyboardFocus event is not triggered.
            InternalLostFocus();
        }

        private void InternalLostFocus()
        {
            LostFocusAction savedLostFocusAction = this.lostFocusAction;
            // Set this to none here so that we will not commit twice when re-entrant (preview lost keyboard focus, then
            // lost keyboard focus in the same callstack)
            this.lostFocusAction = LostFocusAction.None;
            if (savedLostFocusAction == LostFocusAction.Commit)
            {
                this.CommitChange();
            }
            else if (savedLostFocusAction == LostFocusAction.Cancel)
            {
                this.CancelChange();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            this.IsEditing = false;
            ValueEditorUtils.ExecuteCommand(this.LostFocusCommand, this, null);
            e.Handled = true;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (this.ignoreTextChanges)
            {
                return;
            }

            if (this.IsEditing)
            {
                // If we get any text change default to commit if focus goes away
                this.lostFocusAction = LostFocusAction.Commit;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool markHandled = ValueEditorUtils.GetHandlesCommitKeys(this);
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                LostFocusAction savedAction = this.lostFocusAction;
                this.lostFocusAction = LostFocusAction.None;
                if (savedAction == LostFocusAction.Commit)
                {
                    this.CommitChange();
                }

                if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0)
                {
                    this.OnFinishEditing();
                }

                e.Handled |= markHandled;
            }
            else if (e.Key == Key.Escape && this.IsEditing)
            {
                LostFocusAction savedAction = this.lostFocusAction;
                this.lostFocusAction = LostFocusAction.None;
                if (savedAction != LostFocusAction.None)
                {
                    this.CancelChange();
                }

                this.OnFinishEditing();

                e.Handled |= markHandled;
            }
            base.OnKeyDown(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Pen strokePen = null;
            Double borderWidth = this.BorderWidth;
            Brush borderBrush = this.BorderBrush;
            if (borderWidth > 0d && borderBrush != null)
            {
                strokePen = new Pen(borderBrush, borderWidth);
            }

            RenderUtils.DrawInscribedRoundedRect(drawingContext, this.Background, strokePen, new Rect(0d, 0d, this.ActualWidth, this.ActualHeight), this.CornerRadius);

            base.OnRender(drawingContext);
        }

        private void UpdateTextFromValue()
        {
            this.ignoreTextChanges = true;

            if (!this.IsNinched)
            {
                this.Text = this.Value;
            }
            else
            {
                this.Text = null;
            }

            // ###################################################
            // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - BEGIN
            // ###################################################

            // Whenever we set the value of a StringEditor programmatically
            // (either because the contained property value changed somehow
            // or because the user changed the value, hit enter, and we
            // are looping back to see what the new value is now), make sure
            // to clear the TextBox's UndoManager's buffer.  That way, the control
            // will not be able to perform Undo and the command will be forwarded
            // to the host.

            // TextBoxBase does not provide any API to reset its Undo stack,
            // but we can hack around it by resetting the UndoLimit, which
            // accomplishes the same thing.
            //
            int originalUndoLimit = this.UndoLimit;
            this.UndoLimit = 0;
            this.UndoLimit = originalUndoLimit;

            // ###################################################
            // CIDER-SPECIFIC CHANGE IN NEED OF PORTING - END
            // ###################################################

            this.ignoreTextChanges = false;
        }

        private void CommitChange()
        {

            ValueEditorUtils.ExecuteCommand(this.BeginCommand, this, null);
            this.Value = this.Text;
            ValueEditorUtils.ExecuteCommand(this.CommitCommand, this, null);
            ValueEditorUtils.UpdateBinding(this, StringEditor.ValueProperty, UpdateBindingType.Target);
            // Now update the text value in case the model or a binding has reformated
            this.UpdateTextFromValue();
        }

        void OnDesignerViewCommitExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.InternalLostFocus();
            e.Handled = true;
        }

        private void CancelChange()
        {
            ValueEditorUtils.ExecuteCommand(this.BeginCommand, this, null);
            ValueEditorUtils.UpdateBinding(this, StringEditor.ValueProperty, false);
            ValueEditorUtils.ExecuteCommand(this.CancelCommand, this, null);
            ValueEditorUtils.UpdateBinding(this, StringEditor.ValueProperty, UpdateBindingType.Target);
            this.UpdateTextFromValue();
        }

        private void OnFinishEditing()
        {
            ICommand finishedEditingCommand = this.FinishEditingCommand;
            if (finishedEditingCommand != null)
            {
                ValueEditorUtils.ExecuteCommand(finishedEditingCommand, this, null);
            }
            else
            {
                Keyboard.Focus(null);
            }
        }

        private void PostFocusCallback()
        {
            UIThreadDispatcher.Instance.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(this.SetFocus), null);
        }

        private object SetFocus(object o)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Focus();
            }

            return null;
        }
        private enum LostFocusAction
        {
            None,
            Commit,
            Cancel
        }

    }
}
