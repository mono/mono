//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Hosting
{
    using System.Runtime;
    using System.Windows.Input;

    [Fx.Tag.XamlVisible(false)]
    public sealed class CommandInfo
    {
        internal CommandInfo(ICommand command)
        {
            this.Command = command;
            this.IsBindingEnabledInDesigner = true;
        }

        public ICommand Command
        {
            get;
            internal set;
        }

        public bool IsBindingEnabledInDesigner
        {
            get;
            set;
        }
    }

    public interface IWorkflowCommandExtensionCallback
    {
        void OnWorkflowCommandLoaded(CommandInfo commandInfo);
    }
}
