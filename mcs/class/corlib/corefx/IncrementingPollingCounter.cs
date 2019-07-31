// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics.Tracing
{
    public partial class IncrementingPollingCounter : DiagnosticCounter
    {
        public IncrementingPollingCounter (string name, EventSource eventSource, Func<double> totalValueProvider) : base (name, eventSource) {}
        public TimeSpan DisplayRateTimeScale { get; set; }
    }
}
