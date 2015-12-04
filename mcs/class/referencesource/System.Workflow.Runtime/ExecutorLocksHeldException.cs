// ****************************************************************************
// Copyright (C) 2000-2001 Microsoft Corporation.  All rights reserved.
//
// CONTENTS
//     Workflow Base exception class
// 
// DESCRIPTION
//     Base class for WINOE Runtime engine exception
//
// REVISIONS
// Date          Ver     By           Remarks
// ~~~~~~~~~~    ~~~     ~~~~~~~~     ~~~~~~~~~~~~~~
// 03/08/01      1.0     [....]       Created.
// ****************************************************************************
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Workflow;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime
{
    /*
     * The Unload() method has been changed so that
     * any Unload requests made while in the middle of an atomic
     * transaction wait for the atomic transaction to complete.
     * This makes use of an ManualResetEvent. Unload() waits on the event:
     *      theEvent.WaitOne()
     * But waiting with the executor and scheduler locks held
     * will block everything else.
     * 
     * The solution is to have a custom internal exception class that has the
     * ManualResetEvent as an internal property. If Unload() finds itself in the middle
     * of an atomic transaction, it throws the Exception. The Exception is propogated upwards
     * until we reach the method that was the first to grab the executor lock. 
     *
     * We then drop that lock and wait on the event handle. As soon as the handle is
     * Set() by DisposeTransaction(), we grab the executor lock and do everything all over.
     */

    internal class ExecutorLocksHeldException : Exception
    {
        private ManualResetEvent handle;

        public ExecutorLocksHeldException(ManualResetEvent handle)
        {
            this.handle = handle;
        }

        internal ManualResetEvent Handle
        {
            get
            {
                return handle;
            }
        }
    }
}
