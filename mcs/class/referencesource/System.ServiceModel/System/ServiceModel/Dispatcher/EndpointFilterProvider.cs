//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.Runtime.Serialization;

    class EndpointFilterProvider
    {
        SynchronizedCollection<string> initiatingActions;
        object mutex;

        public EndpointFilterProvider(params string[] initiatingActions)
        {
            this.mutex = new object();
            this.initiatingActions = new SynchronizedCollection<string>(this.mutex, initiatingActions);            
        }

        public SynchronizedCollection<string> InitiatingActions
        {
            get { return this.initiatingActions; }
        }

        public MessageFilter CreateFilter(out int priority)
        {
            lock (this.mutex)
            {
                priority = 1;
                if (initiatingActions.Count == 0)
                    return new MatchNoneMessageFilter();
                    
                string[] actions = new string[initiatingActions.Count];
                int index = 0;
                for (int i = 0; i < initiatingActions.Count; i++)
                {
                    string currentAction = initiatingActions[i];
                    if (currentAction == MessageHeaders.WildcardAction)
                    {
                        priority = 0;
                        return new MatchAllMessageFilter();
                    }
                    actions[index] = currentAction;                    
                    ++index;
                }

                return new ActionMessageFilter(actions);
            }            
        }
    }
}

