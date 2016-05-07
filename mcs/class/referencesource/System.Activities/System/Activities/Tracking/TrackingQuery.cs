//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System.Collections.Generic;

    public abstract class TrackingQuery
    {
        IDictionary<string, string> queryAnnotations;

        protected TrackingQuery()
        {
        }

        public IDictionary<string, string> QueryAnnotations
        {
            get
            {
                if (this.queryAnnotations == null)
                {
                    this.queryAnnotations = new Dictionary<string, string>();
                }
                return this.queryAnnotations;
            }
        }

        internal bool HasAnnotations
        {
            get
            {
                return this.queryAnnotations != null && this.queryAnnotations.Count > 0;
            }
        }
    }
}
