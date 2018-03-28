//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.SymbolStore;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Threading;

    // Define an auxiliary thread codegen technique for islands.
    // This executes the islands on a dedicated worker thread. The worker thread's
    // physical callstack then maps to the interpreter's virtual callstack.
    // It has an excellent Step-in/Step-over/Step-Out experience.
    [DebuggerNonUserCode]
    public class ThreadWorkerController
    {
        StateManager stateManager;

        // Set to true to notify to Break on first instruction. This helps the F11 on startup experience.
        // Since the islands are on a new thread, there may be no user code on the main thread and so 
        // F11 doesn't work. Thus the new worker thread needs to fire some break event.
        // This gets reset after the 'startup breakpoint'.
        // The initial Properties can override this.
        bool breakOnStartup;

        // Own the worker thread.
        Thread worker;

        // Signalled when the main thread wants to send an event to the worker thread.
        // The main thread fills out the data first. 
        AutoResetEvent eventSend;

        // Signalled by the worker thread when it's finished handling the event and 
        // the main thread can resume.
        AutoResetEvent eventDone;

        EventCode eventCode;

        // Parameter for enter message. 
        VirtualStackFrame enterStackParameter;

        internal void Initialize(string threadName, StateManager manager)
        {
            this.stateManager = manager;
            this.breakOnStartup = this.stateManager.ManagerProperties.BreakOnStartup;
            CreateWorkerThread(threadName);
        }

        internal void Exit()
        {
            // Implement with an unbalanced Leave.
            // This will get the Worker to return and the ThreadProc to exit.
            this.LeaveState();
            this.worker.Join();
        }

        [DebuggerHidden]
        void WorkerThreadProc()
        {
            Worker(false);

            this.eventDone.Set();
        }

        // Private Entry point called from islands. Must be public so that the islands can invoke it.
        [Fx.Tag.InheritThrows(From = "Worker")]
        [DebuggerHidden]
        public static void IslandWorker(ThreadWorkerController controller)
        {
            if (controller == null)
            {
                throw FxTrace.Exception.ArgumentNull("controller");
            }
            controller.Worker(true);
        }

        [DebuggerHidden]
        internal void Worker(bool isAtStartup)
        {
            if (isAtStartup)
            {
                // Fire the 1-time "startup" breakpoint.
                if (this.breakOnStartup)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    this.breakOnStartup = false;
                }
                this.eventDone.Set();
            }

            // The final terminator is when leave returns, but from a recursive call.
            bool leave = false;
            while (!leave)
            {
                this.eventSend.WaitOne();
                switch (eventCode)
                {
                    case EventCode.Enter:
                        // Call Island for enterStackParameter     
                        this.stateManager.InvokeWorker(this, enterStackParameter);

                        // resume from SendLeave()
                        this.eventDone.Set();
                        break;

                    case EventCode.Leave:
                        leave = true;
                        return;

                    case EventCode.Break:
                        Debugger.Break();
                        this.eventDone.Set();
                        break;
                }
            }
        }

        void CreateWorkerThread(string threadName)
        {
            this.eventSend = new AutoResetEvent(false);
            this.eventDone = new AutoResetEvent(false);
            this.worker = new Thread(new ThreadStart(WorkerThreadProc));

            string name = string.IsNullOrEmpty(threadName) ? this.stateManager.ManagerProperties.AuxiliaryThreadName : threadName;
            if (name != null)
            {
                this.worker.Name = name;
            }
            this.worker.Start();

        }

        internal void EnterState(VirtualStackFrame newFrame)
        {
            this.eventCode = EventCode.Enter;
            this.enterStackParameter = newFrame;

            this.eventSend.Set();

            // Block until Island executes Nop, 
            // giving BPs a chance to be hit.
            // Must block here if the island is stopped at a breakpoint.
            this.eventDone.WaitOne();
        }

        internal void LeaveState()
        {
            this.eventCode = EventCode.Leave;

            this.eventSend.Set();

            // Block until call has exited.
            this.eventDone.WaitOne();
        }

        internal void Break()
        {
            this.eventCode = EventCode.Break;

            this.eventSend.Set();
            this.eventDone.WaitOne();
        }

        // Type of event being fired.
        enum EventCode
        {
            Enter,
            Leave,
            Break
        };
    }
}
