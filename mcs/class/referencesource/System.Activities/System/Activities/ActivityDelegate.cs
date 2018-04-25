//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows.Markup;
    using System.Collections.ObjectModel;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldNotHaveIncorrectSuffix,
        Justification = "Part of the sanctioned, public WF OM")]
    [ContentProperty("Handler")]
    public abstract class ActivityDelegate
    {
        internal static string ArgumentName = "Argument";
        internal static string Argument1Name = "Argument1";
        internal static string Argument2Name = "Argument2";
        internal static string Argument3Name = "Argument3";
        internal static string Argument4Name = "Argument4";
        internal static string Argument5Name = "Argument5";
        internal static string Argument6Name = "Argument6";
        internal static string Argument7Name = "Argument7";
        internal static string Argument8Name = "Argument8";
        internal static string Argument9Name = "Argument9";
        internal static string Argument10Name = "Argument10";
        internal static string Argument11Name = "Argument11";
        internal static string Argument12Name = "Argument12";
        internal static string Argument13Name = "Argument13";
        internal static string Argument14Name = "Argument14";
        internal static string Argument15Name = "Argument15";
        internal static string Argument16Name = "Argument16";
        internal static string ResultArgumentName = "Result";

        Activity owner;
        bool isDisplayNameSet;
        string displayName;
        IList<RuntimeDelegateArgument> delegateParameters;
        int cacheId;
        ActivityCollectionType parentCollectionType;

        protected ActivityDelegate()
        {
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.displayName))
                {
                    this.displayName = this.GetType().Name;
                }

                return this.displayName;
            }
            set
            {
                this.isDisplayNameSet = true;
                this.displayName = value;
            }
        }

        [DefaultValue(null)]
        public Activity Handler
        {
            get;
            set;
        }

        internal LocationReferenceEnvironment Environment
        {
            get;
            set;
        }

        internal Activity Owner
        {
            get
            {
                return this.owner;
            }
        }

        internal ActivityCollectionType ParentCollectionType
        {
            get
            {
                return this.parentCollectionType;
            }
        }

        internal IList<RuntimeDelegateArgument> RuntimeDelegateArguments
        {
            get
            {
                if (this.delegateParameters != null)
                {
                    return this.delegateParameters;
                }

                return new ReadOnlyCollection<RuntimeDelegateArgument>(InternalGetRuntimeDelegateArguments());
            }
        }

        protected internal virtual DelegateOutArgument GetResultArgument()
        {
            return null;
        }

        protected virtual void OnGetRuntimeDelegateArguments(IList<RuntimeDelegateArgument> runtimeDelegateArguments)
        {
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(this))
            {
                ArgumentDirection direction;
                Type innerType;
                if (ActivityUtilities.TryGetDelegateArgumentDirectionAndType(propertyDescriptor.PropertyType, out direction, out innerType))
                {
                    runtimeDelegateArguments.Add(new RuntimeDelegateArgument(propertyDescriptor.Name, innerType, direction, (DelegateArgument)propertyDescriptor.GetValue(this)));
                }
            }
        }

        internal virtual IList<RuntimeDelegateArgument> InternalGetRuntimeDelegateArguments()
        {
            IList<RuntimeDelegateArgument> result = new List<RuntimeDelegateArgument>();
            OnGetRuntimeDelegateArguments(result);
            return result;
        }

        internal void InternalCacheMetadata()
        {
            this.delegateParameters = new ReadOnlyCollection<RuntimeDelegateArgument>(InternalGetRuntimeDelegateArguments());
        }

        internal bool CanBeScheduledBy(Activity parent)
        {
            // fast path if we're the sole (or first) child
            if (object.ReferenceEquals(parent, this.owner))
            {
                return this.parentCollectionType != ActivityCollectionType.Imports;
            }
            else
            {
                return parent.Delegates.Contains(this) || parent.ImplementationDelegates.Contains(this);
            }
        }

        internal bool InitializeRelationship(Activity parent, ActivityCollectionType collectionType, ref IList<ValidationError> validationErrors)
        {
            if (this.cacheId == parent.CacheId)
            {
                Fx.Assert(this.owner != null, "We must have set the owner when we set the cache ID");

                // This means that we've already encountered a parent in the tree

                // Validate that it is visible.

                // In order to see the activity the new parent must be
                // in the implementation IdSpace of an activity which has
                // a public reference to it.
                Activity referenceTarget = parent.MemberOf.Owner;

                if (referenceTarget == null)
                {
                    Activity handler = this.Handler;

                    if (handler == null)
                    {
                        ActivityUtilities.Add(ref validationErrors, new ValidationError(SR.ActivityDelegateCannotBeReferencedWithoutTargetNoHandler(parent.DisplayName, this.owner.DisplayName), false, parent));
                    }
                    else
                    {
                        ActivityUtilities.Add(ref validationErrors, new ValidationError(SR.ActivityDelegateCannotBeReferencedWithoutTarget(handler.DisplayName, parent.DisplayName, this.owner.DisplayName), false, parent));
                    }

                    return false;
                }
                else if (!referenceTarget.Delegates.Contains(this) && !referenceTarget.ImportedDelegates.Contains(this))
                {
                    Activity handler = this.Handler;

                    if (handler == null)
                    {
                        ActivityUtilities.Add(ref validationErrors, new ValidationError(SR.ActivityDelegateCannotBeReferencedNoHandler(parent.DisplayName, referenceTarget.DisplayName, this.owner.DisplayName), false, parent));
                    }
                    else
                    {
                        ActivityUtilities.Add(ref validationErrors, new ValidationError(SR.ActivityDelegateCannotBeReferenced(handler.DisplayName, parent.DisplayName, referenceTarget.DisplayName, this.owner.DisplayName), false, parent));
                    }

                    return false;
                }

                // This is a valid reference so we want to allow
                // normal processing to proceed.
                return true;
            }

            this.owner = parent;
            this.cacheId = parent.CacheId;
            this.parentCollectionType = collectionType;
            InternalCacheMetadata();

            // We need to setup the delegate environment so that it is
            // available when we process the Handler.
            LocationReferenceEnvironment delegateEnvironment = null;

            if (collectionType == ActivityCollectionType.Implementation)
            {
                delegateEnvironment = parent.ImplementationEnvironment;
            }
            else
            {
                delegateEnvironment = parent.PublicEnvironment;
            }

            if (this.RuntimeDelegateArguments.Count > 0)
            {
                ActivityLocationReferenceEnvironment newEnvironment = new ActivityLocationReferenceEnvironment(delegateEnvironment);
                delegateEnvironment = newEnvironment;

                for (int argumentIndex = 0; argumentIndex < this.RuntimeDelegateArguments.Count; argumentIndex++)
                {
                    RuntimeDelegateArgument runtimeDelegateArgument = this.RuntimeDelegateArguments[argumentIndex];
                    DelegateArgument delegateArgument = runtimeDelegateArgument.BoundArgument;

                    if (delegateArgument != null)
                    {
                        if (delegateArgument.Direction != runtimeDelegateArgument.Direction)
                        {
                            ActivityUtilities.Add(ref validationErrors, new ValidationError(SR.RuntimeDelegateArgumentDirectionIncorrect, parent));
                        }

                        if (delegateArgument.Type != runtimeDelegateArgument.Type)
                        {
                            ActivityUtilities.Add(ref validationErrors, new ValidationError(SR.RuntimeDelegateArgumentTypeIncorrect, parent));
                        }

                        // NOTE: We don't initialize this relationship here because
                        // at runtime we'll actually just place these variables in the
                        // environment of the Handler.  We'll initialize and set an
                        // ID when we process the Handler.
                        newEnvironment.Declare(delegateArgument, this.owner, ref validationErrors);
                    }
                }
            }

            this.Environment = delegateEnvironment;

            if (this.Handler != null)
            {
                return this.Handler.InitializeRelationship(this, collectionType, ref validationErrors);
            }

            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeDisplayName()
        {
            return this.isDisplayNameSet;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
