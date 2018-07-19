//------------------------------------------------------------------------------
// <copyright file="Transition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.ComponentModel;
    using System.Windows.Markup;

    /// <summary>
    /// This class represents a Transition of a State.
    /// </summary>
    public sealed class Transition
    {
        /// <summary>
        /// Gets or sets the Action activity which should be executed when the Transtion is taken.
        /// It's optional.
        /// </summary>
        [DependsOn("To")]
        [DefaultValue(null)]
        public Activity Action
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Condition to decide whether the Transition should be taken after the Trigger activity is completed.
        /// It's optional. 
        /// If the Condition is null, the Transition would always be taken when the Trigger activity is completed.
        /// </summary>
        [DependsOn("Action")]
        [DefaultValue(null)]
        public Activity<bool> Condition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets DisplayName of the Transition
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the target State of the Transition.
        /// It's required.
        /// </summary>
        [DependsOn("Trigger")]
        [DefaultValue(null)]
        public State To
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Trigger activity of the Transition.
        /// When the Trigger activity is completed, the StateMachine will start to evaluate whether the Transition should be taken.
        /// It's required.
        /// </summary>
        [DefaultValue(null)]
        public Activity Trigger
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the actual Trigger activity object that should be used when scheduling Transition trigger
        /// between states.
        /// Returns the Trigger object if if it is defined by the user; otherwise, return the null trigger.
        /// </summary>
        internal Activity ActiveTrigger
        {
            get
            {
                return this.Trigger != null ? this.Trigger : this.Source.NullTrigger;
            }
        }

        /// <summary>
        /// Gets or sets Transition Id, which is unique within a State inside a StateMachine.
        /// </summary>
        internal string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Source, which represents source state of transition
        /// </summary>
        internal State Source
        {
            get;
            set;
        }
    }
}
