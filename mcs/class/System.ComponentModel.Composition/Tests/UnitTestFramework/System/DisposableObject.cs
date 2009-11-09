// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace System
{
    public class DisposableObject : IDisposable
    {
        public int DisposeCount
        {
            get;
            private set;
        }

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}