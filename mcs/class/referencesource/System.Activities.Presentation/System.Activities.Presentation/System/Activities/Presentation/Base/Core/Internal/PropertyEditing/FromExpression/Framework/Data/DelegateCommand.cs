// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.Windows.Input;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    internal sealed class DelegateCommand : ICommand
    {
        private SimpleEventHandler handler;
        private bool isEnabled = true;

        public DelegateCommand(SimpleEventHandler handler)
        {
            this.handler = handler;
        }


        public event EventHandler CanExecuteChanged;

        public bool IsEnabled
        {
            get { return this.isEnabled; }
        }
        void ICommand.Execute(object arg)
        {
            this.handler();
        }

        bool ICommand.CanExecute(object arg)
        {
            return this.IsEnabled;
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "This is required by the ICommand interface.")]
        private void OnCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }
        public delegate void SimpleEventHandler();
    }
}
