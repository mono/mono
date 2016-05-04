//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation.Factories
{
    using System.Activities;
    using System.Activities.Presentation;
    using System.Windows;
    using System.Activities.Statements;

    /// <summary>
    /// The type that is added to the toolbox, which defines the factory method 
    /// that creates an instance of StateMachine Activity with an initial State.
    /// </summary>
    public sealed class StateMachineWithInitialStateFactory : IActivityTemplateFactory
    {
        /// <summary>
        /// Creates an instance of StateMachine Activity with an initial State.
        /// </summary>
        /// <param name="target">Not used.</param>
        /// <returns>An instance of StateMachine Activity with an initial State.</returns>
        public Activity Create(DependencyObject target)
        {
            State state = new State()
            {
                DisplayName = StateContainerEditor.DefaultStateDisplayName + "1"
            };
            return new StateMachine()
            {
                States = { state },
                InitialState = state
            };
        }
    }
}
