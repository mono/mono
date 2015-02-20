//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;

    // A virtual callstack frame for the interpretter.
    // This is created by calls to EnterState and LeaveState.
    // This is explictly not named "StackFrame" so that it's not confused with 
    // System.Diagnostics.StackFrame.
    [DebuggerNonUserCode]
    [Fx.Tag.XamlVisible(false)]
    public class VirtualStackFrame
    {
        State state;
        IDictionary<string, object> locals;

        public VirtualStackFrame(State state, IDictionary<string, object> locals)
        {
            this.state = state;
            this.locals = locals;
        }

        public VirtualStackFrame(State state)
            : this(state, null)
        {
            Fx.Assert(state.NumberOfEarlyLocals == 0, "should start with empty locals");
        }

        public State State
        {
            get { return this.state; }
        }

        // All locals (both early-bound and late-bound) for this frame.
        public IDictionary<string, object> Locals
        {
            get { return this.locals; }
        }

        public override string ToString()
        {
            return this.state.ToString();
        }
    }
}
