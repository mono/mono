//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;

    public abstract class DelegateArgument : LocationReference
    {
        ArgumentDirection direction;
        RuntimeDelegateArgument runtimeArgument;
        string name;
        int cacheId;

        internal DelegateArgument()
        {
            this.Id = -1;
        }

        [DefaultValue(null)]
        public new string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        protected override string NameCore
        {
            get
            {
                return this.name;
            }
        }

        public ArgumentDirection Direction
        {
            get
            {
                return this.direction;
            }
            internal set
            {
                this.direction = value;
            }
        }

        internal Activity Owner
        {
            get;
            private set;
        }

        internal bool IsInTree
        {
            get
            {
                return this.Owner != null;
            }
        }

        internal void ThrowIfNotInTree()
        {
            if (!this.IsInTree)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DelegateArgumentMustBeReferenced(this.Name)));
            }
        }

        internal void Bind(RuntimeDelegateArgument runtimeArgument)
        {
            this.runtimeArgument = runtimeArgument;
        }

        internal bool InitializeRelationship(Activity parent, ref IList<ValidationError> validationErrors)
        {
            if (this.cacheId == parent.CacheId)
            {
                Fx.Assert(this.Owner != null, "must have an owner here");
                ValidationError validationError = new ValidationError(SR.DelegateArgumentAlreadyInUseOnActivity(this.Name, parent.DisplayName, this.Owner.DisplayName), this.Owner);
                ActivityUtilities.Add(ref validationErrors, validationError);

                // Get out early since we've already initialized this argument.
                return false;
            }

            this.Owner = parent;
            this.cacheId = parent.CacheId;

            return true;
        }

        // Soft-Link: This method is referenced through reflection by
        // ExpressionUtilities.TryRewriteLambdaExpression.  Update that
        // file if the signature changes.
        public object Get(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            return context.GetValue<object>((LocationReference)this);
        }

        public override Location GetLocation(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            ThrowIfNotInTree();

            if (!context.AllowChainedEnvironmentAccess)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DelegateArgumentDoesNotExist(this.runtimeArgument.Name)));
            }

            Location location;
            if (!context.Environment.TryGetLocation(this.Id, this.Owner, out location))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DelegateArgumentDoesNotExist(this.runtimeArgument.Name)));
            }

            return location;
        }

        // Only used by the debugger
        internal Location InternalGetLocation(LocationEnvironment environment)
        {
            Fx.Assert(this.IsInTree, "DelegateArgument must be opened");

            Location location;
            if (!environment.TryGetLocation(this.Id, this.Owner, out location))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DelegateArgumentDoesNotExist(this.runtimeArgument.Name)));
            }
            return location;
        }

        internal abstract Location CreateLocation();
    }
}
