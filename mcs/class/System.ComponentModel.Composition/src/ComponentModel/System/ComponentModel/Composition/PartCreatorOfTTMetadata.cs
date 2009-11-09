// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;

#if SILVERLIGHT

namespace System.ComponentModel.Composition
{
    public class PartCreator<T, TMetadata> : PartCreator<T>
    {
        private readonly TMetadata _metadata;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public PartCreator(Func<PartLifetimeContext<T>> creator, TMetadata metadata)
            : base(creator)
        {
            this._metadata = metadata;
        }

        public TMetadata Metadata
        {
            get { return this._metadata; }
        }
    }
}

#endif
