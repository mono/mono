//------------------------------------------------------------------------------
// <copyright file="StateMachineStateRecord.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Statements.Tracking
{
    using System;
    using System.Activities.Statements;
    using System.Activities.Tracking;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Serialization;
    
    /// <summary>
    /// Represents a tracking record that is created when an state machine instance transitions to a state.
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public sealed class StateMachineStateRecord : CustomTrackingRecord
    {
        internal static readonly string StateMachineStateRecordName = "System.Activities.Statements.StateMachine";

        private const string StateKey = "currentstate";
        private const string StateMachineKey = "stateMachine";

        /// <summary>
        /// Initializes a new instance of the StateMachineStateRecord class.
        /// </summary>
        public StateMachineStateRecord()
            : this(StateMachineStateRecordName)
        {
        }

        // Disable the user from arbitrary specifying a name for StateMachine specific tracking record.
        internal StateMachineStateRecord(string name)
            : base(name)
        {
        }

        internal StateMachineStateRecord(string name, TraceLevel level)
            : base(name, level)
        {
        } 

        internal StateMachineStateRecord(Guid instanceId, string name, TraceLevel level)
            : base(instanceId, name, level)
        {
        }

        private StateMachineStateRecord(StateMachineStateRecord record)
            : base(record)
        {
        }

        /// <summary>
        /// Gets the display name of the State Machine activity that contains the state.
        /// </summary>
        public string StateMachineName
        {
            get
            {
                if (Data.ContainsKey(StateMachineKey))
                {
                    return Data[StateMachineKey].ToString();
                }

                return string.Empty;
            }

            internal set
            {
                Data[StateMachineKey] = value;
            }
        }

        /// <summary>
        /// Gets the display name of executing state when the record is generated.
        /// </summary>
        [DataMember]
        public string StateName
        {
            get
            {
                if (Data.ContainsKey(StateKey))
                {
                    return Data[StateKey].ToString();
                }

                return string.Empty;
            }       
            
            internal set
            {
                Data[StateKey] = value;
            }
        }

        /// <summary>
        /// Creates a copy of the StateMachineTrackingRecord. (Overrides CustomTrackingRecord.Clone().)
        /// </summary>
        /// <returns>A copy of the StateMachineTrackingRecord instance.</returns>
        protected internal override TrackingRecord Clone()
        {
            return new StateMachineStateRecord(this);
        }
    }
}
