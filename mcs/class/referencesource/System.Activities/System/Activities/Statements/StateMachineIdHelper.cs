//------------------------------------------------------------------------------
// <copyright file="StateMachineIdHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper methods which are used by both StateMachine and State.
    /// </summary>
    static class StateMachineIdHelper
    {
        internal const char StateIdSeparator = ':';

        /// <summary>
        /// Given current stateId and descendant Id, this method returns Id of direct child state of current state.
        /// This direct child state is either the state which descendantId represents or one of ancestor states of it.
        /// </summary>
        /// <param name="stateId">Internal StateId of StateMachine.</param>
        /// <param name="descendantId">Internal StateId of the state.</param>
        /// <returns>Index position of the state in the state machine.</returns>
        public static int GetChildStateIndex(string stateId, string descendantId)
        {
            Fx.Assert(!string.IsNullOrEmpty(descendantId), "descendantId should not be null or empty.");
            Fx.Assert(!string.IsNullOrEmpty(stateId), "stateId should not be null or empty.");
            string[] child = descendantId.Split(StateIdSeparator);
            string[] parent = stateId.Split(StateIdSeparator);
            Fx.Assert(parent.Length < child.Length, "stateId should not be null or empty.");
            return int.Parse(child[parent.Length], CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Return the StateId, which is the identifier of a state.
        /// </summary>
        /// <param name="parentId">Internal StateId of the parent activity, which is StateMachine.</param>
        /// <param name="index">Internal index of the state within StateMachine.</param>
        /// <returns>Unique identifier of a state within StateMachine.</returns>
        public static string GenerateStateId(string parentId, int index)
        {
            return parentId + StateIdSeparator + index.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Return the TransitionId, which is the identifier of a transition.
        /// </summary>
        /// <param name="stateid">Internal StateId of the state.</param>
        /// <param name="transitionIndex">Internal index of the transition within state.</param>
        /// <returns>Unique identifier of a transition within a state.</returns>
        public static string GenerateTransitionId(string stateid, int transitionIndex)
        {
            return stateid + StateIdSeparator + transitionIndex.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// This method is used to see whether state1 is one of ancestors of state2.
        /// </summary>
        /// <param name="state1Id">Internal StateId of the state1.</param>
        /// <param name="state2Id">Internal StateId of the state2.</param>
        /// <returns>True if the state2.Id is identified as a child for state1.</returns>
        public static bool IsAncestor(string state1Id, string state2Id)
        {
            if (string.IsNullOrEmpty(state2Id))
            {
                return false;
            }

            return state2Id.StartsWith(state1Id + StateIdSeparator, StringComparison.Ordinal);
        }
    }
}
