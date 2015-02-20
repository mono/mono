//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Activities.Validation;

    [Fx.Tag.XamlVisible(false)]
    public abstract class LocationReferenceEnvironment
    {
        protected LocationReferenceEnvironment()
        {
        }

        public abstract Activity Root { get; }

        public LocationReferenceEnvironment Parent
        {
            get;
            protected set;
        }

        public abstract bool IsVisible(LocationReference locationReference);

        public abstract bool TryGetLocationReference(string name, out LocationReference result);

        public abstract IEnumerable<LocationReference> GetLocationReferences();               

    }
}
