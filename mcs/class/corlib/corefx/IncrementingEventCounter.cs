// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics.Tracing
{
    public partial class IncrementingEventCounter : DiagnosticCounter
    {
        public IncrementingEventCounter (string name, EventSource eventSource) : base (name, eventSource) {}
        public void Increment (double increment = 1) {}
        public TimeSpan DisplayRateTimeScale { get; set; }
    }
}
