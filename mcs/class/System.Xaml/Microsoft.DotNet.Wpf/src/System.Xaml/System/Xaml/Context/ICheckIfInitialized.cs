// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MS.Internal.Xaml.Context
{
    // This interface allows ObjectWriterContext to call into ObjectWriter to get the live initialization
    // status of an object. We direct calls through this interface to avoid breaking our internal layering
    // by having a direct reference from ObjectWriterContext to ObjectWriter.
    internal interface ICheckIfInitialized
    {
        bool IsFullyInitialized(object obj);
    }
}
