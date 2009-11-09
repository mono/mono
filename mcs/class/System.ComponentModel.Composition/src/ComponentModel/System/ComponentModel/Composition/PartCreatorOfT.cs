// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using Microsoft.Internal;

#if SILVERLIGHT

namespace System.ComponentModel.Composition
{
    public class PartCreator<T>
    {
        private readonly Func<PartLifetimeContext<T>> _creator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public PartCreator(Func<PartLifetimeContext<T>> creator)
        {
            Requires.NotNull(creator, "creator");
            this._creator = creator;
        }

        public PartLifetimeContext<T> CreatePart()
        {
            return this._creator();
        }
    }
}

#endif