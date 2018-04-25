//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldHaveCorrectSuffix)]
    [Fx.Tag.XamlVisible(false)]
    public sealed class ExecutionProperties : IEnumerable<KeyValuePair<string, object>>
    {
        static IEnumerable<KeyValuePair<string, object>> emptyKeyValues;

        ActivityContext context;
        ActivityInstance scope;
        ExecutionPropertyManager properties;
        IdSpace currentIdSpace;

        internal ExecutionProperties(ActivityContext currentContext, ActivityInstance scope, ExecutionPropertyManager properties)
        {
            this.context = currentContext;
            this.scope = scope;
            this.properties = properties;

            if (this.context != null)
            {
                this.currentIdSpace = this.context.Activity.MemberOf;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.properties == null);
            }
        }

        static IEnumerable<KeyValuePair<string, object>> EmptyKeyValues
        {
            get
            {
                if (emptyKeyValues == null)
                {
                    emptyKeyValues = new KeyValuePair<string, object>[0];
                }
                return emptyKeyValues;
            }
        }

        [Fx.Tag.InheritThrows(From = "Register", FromDeclaringType = typeof(IPropertyRegistrationCallback))]
        public void Add(string name, object property)
        {
            Add(name, property, false, false);
        }

        [Fx.Tag.InheritThrows(From = "Add")]
        public void Add(string name, object property, bool onlyVisibleToPublicChildren)
        {
            Add(name, property, false, onlyVisibleToPublicChildren);
        }

        internal void Add(string name, object property, bool skipValidations, bool onlyVisibleToPublicChildren)
        {
            if (!skipValidations)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("name");
                }

                if (property == null)
                {
                    throw FxTrace.Exception.ArgumentNull("property");
                }

                ThrowIfActivityExecutionContextDisposed();
                ThrowIfChildrenAreExecuting();
            }

            if (this.properties != null)
            {
                this.properties.ThrowIfAlreadyDefined(name, this.scope);
            }

            IPropertyRegistrationCallback registrationCallback = property as IPropertyRegistrationCallback;

            if (registrationCallback != null)
            {
                registrationCallback.Register(new RegistrationContext(this.properties, this.currentIdSpace));
            }

            if (this.properties == null)
            {
                this.properties = new ExecutionPropertyManager(this.scope);
            }
            else if (!this.properties.IsOwner(this.scope))
            {
                // 
                this.properties = new ExecutionPropertyManager(this.scope, this.properties);
            }

            IdSpace visibility = null;

            if (onlyVisibleToPublicChildren)
            {
                Fx.Assert(this.currentIdSpace != null, "We should never call OnlyVisibleToPublicChildren when we don't have a currentIdSpace");
                visibility = this.currentIdSpace;
            }

            this.properties.Add(name, property, visibility);
        }

        [Fx.Tag.InheritThrows(From = "Unregister", FromDeclaringType = typeof(IPropertyRegistrationCallback))]
        public bool Remove(string name)
        {
            return Remove(name, false);
        }

        internal bool Remove(string name, bool skipValidations)
        {
            if (!skipValidations)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw FxTrace.Exception.ArgumentNullOrEmpty("name");
                }

                ThrowIfActivityExecutionContextDisposed();
            }

            if (this.properties != null && this.properties.IsOwner(this.scope))
            {
                object property = this.properties.GetPropertyAtCurrentScope(name);

                if (property != null)
                {
                    if (!skipValidations)
                    {
                        Handle handleProperty = property as Handle;

                        if (handleProperty == null || !handleProperty.CanBeRemovedWithExecutingChildren)
                        {
                            ThrowIfChildrenAreExecuting();
                        }
                    }

                    this.properties.Remove(name);

                    IPropertyRegistrationCallback registrationCallback = property as IPropertyRegistrationCallback;

                    if (registrationCallback != null)
                    {
                        registrationCallback.Unregister(new RegistrationContext(this.properties, this.currentIdSpace));
                    }

                    return true;
                }
            }

            return false;
        }

        public object Find(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            if (this.properties == null)
            {
                return null;
            }
            else
            {
                return this.properties.GetProperty(name, this.currentIdSpace);
            }
        }

        // Note that we don't need to pass the IdSpace here because we're
        // just checking for things that this activity has added.
        internal object FindAtCurrentScope(string name)
        {
            Fx.Assert(!string.IsNullOrEmpty(name), "We should only call this with non-null names");

            if (this.properties == null || !this.properties.IsOwner(this.scope))
            {
                return null;
            }
            else
            {
                return this.properties.GetPropertyAtCurrentScope(name);
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            if (this.properties != null)
            {
                return this.properties.GetFlattenedProperties(this.currentIdSpace);
            }
            else
            {
                return EmptyKeyValues;
            }
        }

        void ThrowIfChildrenAreExecuting()
        {
            if (this.scope.HasChildren)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CannotAddOrRemoveWithChildren));
            }
        }

        void ThrowIfActivityExecutionContextDisposed()
        {
            if (this.context.IsDisposed)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.AECForPropertiesHasBeenDisposed));
            }
        }

    }
}


