//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    abstract class LocationFactory
    {
        public Location CreateLocation(ActivityContext context)
        {
            return CreateLocationCore(context);
        }

        protected abstract Location CreateLocationCore(ActivityContext context);
    }

    abstract class LocationFactory<T> : LocationFactory
    {
        public abstract new Location<T> CreateLocation(ActivityContext context);

        protected override Location CreateLocationCore(ActivityContext context)
        {
            return this.CreateLocation(context);
        }
    }
}
