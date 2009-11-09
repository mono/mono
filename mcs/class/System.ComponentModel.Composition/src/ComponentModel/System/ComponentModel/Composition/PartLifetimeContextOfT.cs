// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

#if SILVERLIGHT

namespace System.ComponentModel.Composition
{
    public sealed class PartLifetimeContext<T> : IDisposable
    {
        private readonly T _exportedValue;
        private readonly Action _dispose;

        public PartLifetimeContext(T exportedValue, Action dispose)
        {
            this._exportedValue = exportedValue;
            this._dispose = dispose;
        }

        public T ExportedValue 
        {
            get { return this._exportedValue; }
        }

        public void Dispose()
        {
            this._dispose();
        }
    }
}

#endif