//------------------------------------------------------------------------------
// <copyright file="TriggerCompletedEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.Runtime.Serialization;

    /// <summary>
    /// TriggerCompletedEvent represents an event which is triggered when a trigger is completed.
    /// </summary>
    [DataContract]
    class TriggerCompletedEvent
    {
        /// <summary>
        /// Gets or sets Bookmark that starts evaluating condition(s).
        /// </summary>
        [DataMember]
        public Bookmark Bookmark
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets TriggerId, which is unique within a state
        /// </summary>
        [DataMember]
        public int TriggedId
        {
            get;
            set;
        }
    }
}
