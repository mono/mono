// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Net.Http
{
    public sealed partial class ReadOnlyMemoryContent : System.Net.Http.HttpContent
    {
        public ReadOnlyMemoryContent(System.ReadOnlyMemory<byte> content) => throw new PlatformNotSupportedException();
        protected override System.Threading.Tasks.Task SerializeToStreamAsync(System.IO.Stream stream, System.Net.TransportContext context) => throw new PlatformNotSupportedException();
        protected internal override bool TryComputeLength(out long length) => throw new PlatformNotSupportedException();
    }
}