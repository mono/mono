//------------------------------------------------------------------------------
// <copyright file="PipelineModuleStepContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * A simple container class for module events
 * 
 * Copyright (c) 2005 Microsoft Corporation
 */

namespace System.Web {    
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;
    using Debug=System.Web.Util.Debug;

    // this class is a container for application module events
    // there is one instance of this class per module per application instance
    // Since execution steps are tied to an application instance, this needs to be
    // as well
    internal sealed class PipelineModuleStepContainer {

#if DBG
        string _moduleName;

        internal string DebugModuleName {
            set {
                Debug.Assert( !String.IsNullOrEmpty(value), "!String.IsNullOrEmpty(value)");
                if (_moduleName != null) {
                    // make sure we're not ever crossing the modules
                    Debug.Assert(value == _moduleName, "value == _moduleName");
                }
                
                _moduleName = value;
            }
            get {
                return (String.IsNullOrEmpty(_moduleName)) ? String.Empty : _moduleName;
            }
        }
#endif        

        // request notifications are bit flags in a DWORD
        // so we will won't have more than 12
        // we could do with fewer but in order simplify indexing,
        // we'll use the whole 12
        // the arrays are lazily allocated so modules that only
        // subscribe to one type of event will only get one type of step arr
        List<HttpApplication.IExecutionStep>[] _moduleSteps;
        List<HttpApplication.IExecutionStep>[] _modulePostSteps;

        internal PipelineModuleStepContainer() {
        }

        private List<HttpApplication.IExecutionStep> GetStepArray(RequestNotification notification, bool isPostEvent) {

#if DBG
            Debug.Trace("PipelineRuntime",
                        "GetStepArray for " + DebugModuleName + " for " + notification.ToString() +
                        " and " + isPostEvent + "\r\n");
#endif            

            List<HttpApplication.IExecutionStep>[] steps = _moduleSteps;
            
            if (isPostEvent) { 
                steps = _modulePostSteps;
            }

            Debug.Assert(null != steps, "null != steps");

            int index = EventToIndex(notification);
            Debug.Assert(index != -1, "index != -1");

            Debug.Trace("PipelineRuntime",
                        "GetStepArray: " + notification.ToString() + " mapped to index " + index.ToString(CultureInfo.InvariantCulture) + "\r\n");

            List<HttpApplication.IExecutionStep> stepArray = steps[index];
            // we shouldn't be asking for events that aren't mapped to this
            // module at all
            Debug.Assert(null != stepArray, "null != stepArray");            
            
            return stepArray;                
        }

        internal int GetEventCount(RequestNotification notification, bool isPostEvent) {
            List<HttpApplication.IExecutionStep> stepArray = GetStepArray(notification, isPostEvent);
            if (null == stepArray) {
                return 0;                
            }

           return stepArray.Count; 
        }

        internal HttpApplication.IExecutionStep GetNextEvent(RequestNotification notification, bool isPostEvent, int eventIndex) {
            List<HttpApplication.IExecutionStep> stepArray = GetStepArray(notification, isPostEvent);

            Debug.Assert(eventIndex >= 0, "eventIndex >= 0");
            Debug.Assert(eventIndex < stepArray.Count, "eventIndex < stepArray.Count");

            return stepArray[eventIndex];
        }

        internal void RemoveEvent(RequestNotification notification, bool isPostEvent, Delegate handler) {

            // if module instances unregister multiple times, this can fail on subsequent attempts
            // so don't use GetStepArray which does extra checked verification
            List<HttpApplication.IExecutionStep>[] steps = _moduleSteps;
            
            if (isPostEvent) { 
                steps = _modulePostSteps;
            }

            if (steps == null) {
                return;
            }
            
            int index = EventToIndex(notification);
            List<HttpApplication.IExecutionStep> stepArray = steps[index];

            if (null == stepArray) {
                return;
            }
                                
            
            int toRemove = -1;
            
            HttpApplication.SyncEventExecutionStep syncStep;
            for (int i = 0; i < stepArray.Count; i++ ) {

                // we don't support removing async event handlers
                // but the event syntax forces us to handle [....] events
                syncStep = stepArray[i] as HttpApplication.SyncEventExecutionStep;
                if (null != syncStep) {
                    if (syncStep.Handler == (EventHandler)handler) {
                        toRemove = i;
                        break;
                    }
                }
            }

            if (toRemove != -1) {
                stepArray.RemoveAt(toRemove);
            }
        }

        
        internal void AddEvent(RequestNotification notification, bool isPostEvent, HttpApplication.IExecutionStep step) {
            int index = EventToIndex(notification);
#if DBG            
            Debug.Trace("PipelineRuntime", "Adding event: " + DebugModuleName + " " + notification.ToString() + " " +
                        isPostEvent.ToString() + "@ index " + index.ToString(CultureInfo.InvariantCulture) + "\r\n");
#endif            

            Debug.Assert(index != -1, "index != -1");

            List<HttpApplication.IExecutionStep>[] steps = null;
            
            if (isPostEvent) {
                if (null == _modulePostSteps) {
                    _modulePostSteps = new List<HttpApplication.IExecutionStep>[ 32 ];            
                }
                steps = _modulePostSteps;
            }
            else {
                if (null == _moduleSteps) {
                    _moduleSteps = new List<HttpApplication.IExecutionStep>[ 32 ];
                }

                steps = _moduleSteps;
            }

            Debug.Assert(steps != null, "steps != null");
            
            // retrieve the steps for this event (typically none at this point)
            // allocate a new container as necessary and add this step
            // in the event that a single module has registered more than once
            // for a given event, we'll have multiple steps here
            List<HttpApplication.IExecutionStep> stepArray = steps[index];                
            if (null == stepArray) {
                // first touch, instantiate and save it
                stepArray = new List<HttpApplication.IExecutionStep>();
                steps[index] = stepArray;
            }

            stepArray.Add(step);
       }

        // we have tried various techniques here for converting a request notification
        // into an index but a simple switch statement has so far performed the best
        // basically, the problem is converting a single on bit to its position
        // so 0x00000001 == 0, 0x00000002 == 1, etc.
        // Managed code doesn't support all the request flags so we only translate
        // the ones we deal with to keep the switch table as simple as possible
        private static int EventToIndex(RequestNotification notification) {
            int index = -1;

            switch (notification) {
                    // 0x00000001
                case RequestNotification.BeginRequest:
                    return 0;

                    // 0x00000002
                case RequestNotification.AuthenticateRequest:
                    return 1;

                    // 0x00000004
                case RequestNotification.AuthorizeRequest:
                    return 2;

                    // 0x00000008
                case RequestNotification.ResolveRequestCache:
                    return 3;

                    // 0x00000010
                case RequestNotification.MapRequestHandler:
                    return 4;

                    // 0x00000020
                case RequestNotification.AcquireRequestState:
                    return 5;

                    // 0x00000040
                case RequestNotification.PreExecuteRequestHandler:
                    return 6;

                    // 0x00000080
                case RequestNotification.ExecuteRequestHandler:
                    return 7;

                    // 0x00000100
                case RequestNotification.ReleaseRequestState:
                    return 8;

                    // 0x00000200
                case RequestNotification.UpdateRequestCache:
                    return 9;

                    // 0x00000400
                case RequestNotification.LogRequest:
                    return 10;

                    // 0x00000800
                case RequestNotification.EndRequest:
                    return 11;

                    // 0x20000000
                case RequestNotification.SendResponse :
                    return 12;

                default:
                    Debug.Assert(index != -1, "invalid request notification--need to update switch table?");
                    return index;
            }
        }
    }
}

