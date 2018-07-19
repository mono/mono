//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Diagnostics;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public sealed class ActivityStateRecord : TrackingRecord
    {
        IDictionary<string, object> variables;
        IDictionary<string, object> arguments;
        ActivityInfo activity;
        string state;

        static ReadOnlyCollection<string> wildcardCollection = new ReadOnlyCollection<string>(new List<string>(1) { "*" });

        internal ActivityStateRecord(Guid instanceId, ActivityInstance instance, ActivityInstanceState state)
            : this(instanceId, new ActivityInfo(instance), state)
        {
        }

        internal ActivityStateRecord(Guid instanceId, ActivityInfo activity, ActivityInstanceState state)
            : base(instanceId)
        {
            this.Activity = activity;

            switch (state)
            {
                case ActivityInstanceState.Executing:
                    this.State = ActivityStates.Executing;
                    break;
                case ActivityInstanceState.Closed:
                    this.State = ActivityStates.Closed;
                    break;
                case ActivityInstanceState.Canceled:
                    this.State = ActivityStates.Canceled;
                    break;
                case ActivityInstanceState.Faulted:
                    this.State = ActivityStates.Faulted;
                    break;
                default:
                    throw Fx.AssertAndThrow("Invalid state value");
            }
        }

        public ActivityStateRecord(
            Guid instanceId,
            long recordNumber,
            ActivityInfo activity,
            string state)
            : base(instanceId, recordNumber)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull("activity");
            }
            if (string.IsNullOrEmpty(state))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("state");
            }

            this.Activity = activity;
            this.State = state;
        }

        ActivityStateRecord(ActivityStateRecord record)
            : base(record)
        {
            this.Activity = record.Activity;
            this.State = record.State;
            if (record.variables != null)
            {
                if (record.variables == ActivityUtilities.EmptyParameters)
                {
                    this.variables = ActivityUtilities.EmptyParameters;
                }
                else
                {
                    this.variables = new Dictionary<string, object>(record.variables);
                }
            }

            if (record.arguments != null)
            {
                if (record.arguments == ActivityUtilities.EmptyParameters)
                {
                    this.arguments = ActivityUtilities.EmptyParameters;
                }
                else
                {
                    this.arguments = new Dictionary<string, object>(record.arguments);
                }
            }
        }

        
        public ActivityInfo Activity
        {
            get
            {
                return this.activity;
            }
            private set
            {
                this.activity = value;
            }
        }
        
        public string State
        {
            get
            {
                return this.state;
            }
            private set
            {
                this.state = value;
            }
        }

        public IDictionary<string, object> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    this.variables = GetVariables(wildcardCollection);
                    Fx.Assert(this.variables.IsReadOnly, "only readonly dictionary can be set for variables");
                }
                return this.variables;
            }

            internal set
            {
                Fx.Assert(value.IsReadOnly, "only readonly dictionary can be set for variables");
                this.variables = value;
            }
        }

        public IDictionary<string, object> Arguments
        {
            get
            {
                if (this.arguments == null)
                {
                    this.arguments = GetArguments(wildcardCollection);
                    Fx.Assert(this.arguments.IsReadOnly, "only readonly dictionary can be set for arguments");
                }
                return this.arguments;
            }

            internal set
            {
                Fx.Assert(value.IsReadOnly, "only readonly dictionary can be set for arguments");
                this.arguments = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "variables")]
        internal IDictionary<string, object> SerializedVariables
        {
            get { return this.variables; }
            set { this.variables = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "arguments")]
        internal IDictionary<string, object> SerializedArguments
        {
            get { return this.arguments; }
            set { this.arguments = value; }
        }

        [DataMember(Name = "Activity")]
        internal ActivityInfo SerializedActivity
        {
            get { return this.Activity; }
            set { this.Activity = value; }
        }

        [DataMember(Name = "State")]
        internal string SerializedState
        {
            get { return this.State; }
            set { this.State = value; }
        }

        protected internal override TrackingRecord Clone()
        {
            return new ActivityStateRecord(this);
        }


        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
               "ActivityStateRecord {{ {0}, Activity {{ {1} }}, State = {2} }}",
                base.ToString(),
                this.Activity.ToString(),
                this.State);
        }

        internal IDictionary<string, object> GetVariables(ICollection<string> variables)
        {
            Dictionary<string, object> trackedVariables = null; // delay allocated through TrackData
            ActivityInstance currentInstance = this.Activity.Instance;

            if (currentInstance != null)
            {
                Activity currentElement = currentInstance.Activity;
                Activity startActivity = currentInstance.Activity;
                bool containsWildcard = variables.Contains("*");
                //count defines how many items we can get in this lookup. It represents the maximum number of items that can be extracted, 
                //if * is specified, any other names specified are expected to be variables defined in scope, not in the activity itself. 
                //if a variable name in the activity is specified, the lookup continues through the variables in scope. 
                int count = containsWildcard ? currentElement.RuntimeVariables.Count + variables.Count - 1 : variables.Count;

                IdSpace activityIdSpace = currentElement.MemberOf;

                while (currentInstance != null)
                {
                    //* only extracts variables of the current Activity and not variables in scope. 
                    bool useWildCard = containsWildcard && startActivity == currentElement;

                    // we only track public Variables, not ImplementationVariables
                    for (int i = 0; i < currentElement.RuntimeVariables.Count; i++)
                    {
                        Variable variable = currentElement.RuntimeVariables[i];
                        if (TrackData(variable.Name, variable.Id, currentInstance, variables, useWildCard, ref trackedVariables))
                        {
                            if (trackedVariables.Count == count)
                            {
                                return new ReadOnlyDictionaryInternal<string, object>(trackedVariables);
                            }
                        }
                    }

                    bool foundNext = false;

                    while (!foundNext)
                    {
                        currentInstance = currentInstance.Parent;

                        if (currentInstance != null)
                        {
                            currentElement = currentInstance.Activity;
                            foundNext = currentElement.MemberOf.Equals(activityIdSpace);
                        }
                        else
                        {
                            // We set foundNext to true to get out of our loop.
                            foundNext = true;
                        }
                    }
                }
            }

            if (trackedVariables == null)
            {
                return ActivityUtilities.EmptyParameters;
            }
            else
            {
                Fx.Assert(trackedVariables.Count > 0, "we should only allocate the dictionary if we're putting data in it");
                return new ReadOnlyDictionaryInternal<string, object>(trackedVariables);
            }
        }

        internal IDictionary<string, object> GetArguments(ICollection<string> arguments)
        {
            Dictionary<string, object> trackedArguments = null; // delay allocated through TrackData
            ActivityInstance currentInstance = this.Activity.Instance;

            if (currentInstance != null)
            {
                Activity currentElement = currentInstance.Activity;
                bool containsWildcard = arguments.Contains("*");
                int count = containsWildcard ? currentElement.RuntimeArguments.Count : arguments.Count;
                bool isActivityStateExecuting = ActivityStates.Executing.Equals(this.State, StringComparison.Ordinal);

                //look at arguments for this element. 
                for (int i = 0; i < currentElement.RuntimeArguments.Count; i++)
                {
                    RuntimeArgument argument = currentElement.RuntimeArguments[i];

                    // OutArguments will always start with default(T), so there is no need to track them when state == Executing
                    if (isActivityStateExecuting && argument.Direction == ArgumentDirection.Out)
                    {
                        continue;
                    }

                    if (TrackData(argument.Name, argument.Id, currentInstance, arguments, containsWildcard, ref trackedArguments))
                    {
                        if (trackedArguments.Count == count)
                        {
                            break;
                        }
                    }
                }
            }

            if (trackedArguments == null)
            {
                return ActivityUtilities.EmptyParameters;
            }
            else
            {
                Fx.Assert(trackedArguments.Count > 0, "we should only allocate the dictionary if we're putting data in it");
                return new ReadOnlyDictionaryInternal<string, object>(trackedArguments);
            }
        }


        bool TrackData(string name, int id, ActivityInstance currentInstance, ICollection<string> data, bool wildcard, ref Dictionary<string, object> trackedData)
        {
            if (wildcard || data.Contains(name))
            {
                Location location = currentInstance.Environment.GetSpecificLocation(id);
                if (location != null)
                {
                    if (trackedData == null)
                    {
                        trackedData = new Dictionary<string, object>(10);
                    }

                    string dataName = name ?? NameGenerator.Next();
                    trackedData[dataName] = location.Value;
                    if (TD.TrackingDataExtractedIsEnabled())
                    {
                        TD.TrackingDataExtracted(dataName, this.Activity.Name);
                    }

                    return true;
                }
            }
            return false;
        }
    }
}
