// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.Interop;
    using System.Security;

    internal class EventTraceActivity
    {
        // This field is public because it needs to be passed by reference for P/Invoke
        public Guid ActivityId;
        static EventTraceActivity empty;

        public EventTraceActivity(bool setOnThread = false)
            : this(Guid.NewGuid(), setOnThread)
        {
        }

        public EventTraceActivity(Guid guid, bool setOnThread = false)
        {
            this.ActivityId = guid;
            if (setOnThread)
            {
                SetActivityIdOnThread();
            }
        }


        public static EventTraceActivity Empty
        {
            get 
            {
                if (empty == null)
                {
                    empty = new EventTraceActivity(Guid.Empty);
                }

                return empty;
            }
        }

        public static string Name
        {
            get { return "E2EActivity"; }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because the CorrelationManager property has a link demand on UnmanagedCode.",
            Safe = "We do not leak security data.")]
        [SecuritySafeCritical]
        public static EventTraceActivity GetFromThreadOrCreate(bool clearIdOnThread = false)
        {
            Guid guid = Trace.CorrelationManager.ActivityId;
            if (guid == Guid.Empty)
            {
                guid = Guid.NewGuid();
            }
            else if (clearIdOnThread)
            {
                // Reset the ActivityId on the thread to avoid using the same Id again
                Trace.CorrelationManager.ActivityId = Guid.Empty;
            }

            return new EventTraceActivity(guid);
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because the CorrelationManager property has a link demand on UnmanagedCode.",
            Safe = "We do not leak security data.")]
        [SecuritySafeCritical]
        public static Guid GetActivityIdFromThread()
        {
            return Trace.CorrelationManager.ActivityId;
        }

        public void SetActivityId(Guid guid)
        {
            this.ActivityId = guid;
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because the CorrelationManager property has a link demand on UnmanagedCode.",
            Safe = "We do not leak security data.")]
        [SecuritySafeCritical]
        void SetActivityIdOnThread()
        {
            Trace.CorrelationManager.ActivityId = this.ActivityId;
        }
    }
}
