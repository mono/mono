//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.State 
{
    using System;
    using System.Diagnostics;

    // <summary>
    // Aggregate of IStateContainer objects
    // </summary>
    internal class AggregateStateContainer : IStateContainer 
    {

        private IStateContainer[] _containers;

        public AggregateStateContainer(params IStateContainer[] containers) {
            _containers = containers;
        }

        public object RetrieveState() 
        {
            object[] states = null;

            if (_containers != null) 
            {
                states = new object[_containers.Length];
                for (int i = 0; i < _containers.Length; i++)
                {
                    states[i] = _containers[i] == null ? null : _containers[i].RetrieveState();
                }
            }

            return states;
        }

        public void RestoreState(object state) 
        {
            if (_containers != null) 
            {

                object[] states = state as object[];
                if (states == null || states.Length != _containers.Length) 
                {
                    Debug.Fail("Invalid state to restore: " + (state == null ? "null" : state.ToString()));
                    return;
                }

                for (int i = 0; i < _containers.Length; i++)
                {
                    if (_containers[i] != null)
                    {
                        _containers[i].RestoreState(states[i]);
                    }
                }
            }
        }
    }
}
