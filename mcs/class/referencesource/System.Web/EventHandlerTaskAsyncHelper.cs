//------------------------------------------------------------------------------
// <copyright file="EventHandlerTaskAsyncHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Assists in converting an EventHandler written using the Task Asynchronous Pattern to a Begin/End method pair.
 * 
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class EventHandlerTaskAsyncHelper {

        public EventHandlerTaskAsyncHelper(TaskEventHandler handler) {
            if (handler == null) {
                throw new ArgumentNullException("handler");
            }

            BeginEventHandler = (sender, e, cb, extraData) => TaskAsyncHelper.BeginTask(() => handler(sender, e), cb, extraData);
            EndEventHandler = TaskAsyncHelper.EndTask;
        }

        public BeginEventHandler BeginEventHandler {
            get;
            private set;
        }

        public EndEventHandler EndEventHandler {
            get;
            private set;
        }

    }
}
