//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Runtime;
    using System.Windows;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Activities.Presentation.View;
    using System.Windows.Interop;
    using System.Windows.Input;
    using System.ComponentModel;

    [Fx.Tag.XamlVisible(false)]
    internal partial class WorkflowElementDialogWindow
    {
        WorkflowElementDialog payload;
        bool okCancel;

        //default MinButton and MaxButton to true
        private bool enableMinButton = true;
        private bool enableMaxButton = true;

        private Func<bool> onOk;

        public WorkflowElementDialogWindow(WorkflowElementDialog payload, bool okCancel, bool enableMinButton, bool enableMaxButton, Func<bool> onOk)
        {
            this.payload = payload;
            this.okCancel = okCancel;
            this.enableMinButton = enableMinButton;
            this.enableMaxButton = enableMaxButton;
            this.onOk = onOk;
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.payload.Window = this;
            this.payloadHolder.Child = this.payload;

            this.MinWidth = this.payload.MinWidth;
            this.MinHeight = this.payload.MinHeight;
            this.MaxWidth = this.payload.MaxWidth;
            this.MaxHeight = this.payload.MaxHeight;
            this.ResizeMode = this.payload.WindowResizeMode;
            this.SizeToContent = this.payload.WindowSizeToContent;

            this.Context = payload.Context;
            if (payload.HelpKeyword != null)
            {
                this.HelpKeyword = payload.HelpKeyword;
            }

            if (0.0 != this.payload.MinWidth)
            {
                this.Width = this.payload.MinWidth;
            }
            if (0.0 != this.payload.MinHeight)
            {
                this.Height = this.payload.MinHeight;
            }

            this.payload.MinWidth = this.payload.MinHeight = 0.0;
            this.payload.MaxWidth = this.payload.MaxWidth = double.PositiveInfinity;

            if (!this.okCancel)
            {
                this.buttonPanel.Children.Remove(this.cancelButton);
            }
        }

        void OK_Click(object sender, RoutedEventArgs e)
        {
            if (this.onOk == null || this.onOk())
            {
                this.DialogResult = true;
            }
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "This function is called in the xaml file")]
        void OnWindowClosed(object sender, EventArgs e)
        {
            this.payload.Window = null;
            this.payloadHolder.Child = null;
        }
    }
}
