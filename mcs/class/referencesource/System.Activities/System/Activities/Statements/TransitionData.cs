//------------------------------------------------------------------------------
// <copyright file="TransitionData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;

    /// <summary>
    /// TransitionData is used by InternalTransition to store data from Transition.
    /// </summary>
    sealed class TransitionData
    {
        /// <summary>
        /// Gets or sets Action of transition.
        /// </summary>
        public Activity Action
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Condition of transition.
        /// If condition is null, it means it's an unconditional transition.
        /// </summary>
        public Activity<bool> Condition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets To of transition, which represent the target InternalState.
        /// </summary>
        public InternalState To
        {
            get;
            set;
        }
    }
}
