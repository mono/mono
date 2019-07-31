// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics.Tracing
{
    public abstract partial class DiagnosticCounter : IDisposable
    {
        internal DiagnosticCounter (string name, EventSource eventSource) { }
        internal DiagnosticCounter () { }
        public string DisplayName { get; set; }
        public string DisplayUnits { get; set; }
        public EventSource EventSource { get; }
        public string Name { get; }
        public void AddMetadata (string key, string value) { }
        public void Dispose () { }
    }
}