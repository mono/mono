//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    [Fx.Tag.XamlVisible(false)]
    public class ValidationSettings
    {
        IDictionary<Type, IList<Constraint>> additionalConstraints;

        public CancellationToken CancellationToken
        {
            get;
            set;
        }

        public bool SingleLevel
        {
            get;
            set;
        }

        public bool SkipValidatingRootConfiguration
        {
            get;
            set;
        }

        public bool OnlyUseAdditionalConstraints
        {
            get;
            set;
        }

        public bool PrepareForRuntime
        {
            get;
            set;
        }

        public LocationReferenceEnvironment Environment
        {
            get;
            set;
        }

        internal bool HasAdditionalConstraints
        {
            get
            {
                return this.additionalConstraints != null && this.additionalConstraints.Count > 0;
            }
        }
        
        public IDictionary<Type, IList<Constraint>> AdditionalConstraints
        {
            get
            {
                if (this.additionalConstraints == null)
                {
                    this.additionalConstraints = new Dictionary<Type, IList<Constraint>>(); 
                }

                return this.additionalConstraints;
            }
        }        
    }
}
