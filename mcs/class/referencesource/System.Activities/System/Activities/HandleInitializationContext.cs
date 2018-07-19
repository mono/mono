//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public sealed class HandleInitializationContext 
    {
        ActivityExecutor executor;
        ActivityInstance scope;
        bool isDiposed;

        internal HandleInitializationContext(ActivityExecutor executor, ActivityInstance scope)
        {
            this.executor = executor;
            this.scope = scope;
        }

        internal ActivityInstance OwningActivityInstance
        {
            get
            {
                return this.scope;
            }
        }

        internal ActivityExecutor Executor
        {
            get
            {
                return this.executor;
            }
        }

        public THandle CreateAndInitializeHandle<THandle>() where THandle : Handle
        {
            ThrowIfDisposed();
            THandle value = Activator.CreateInstance<THandle>();

            value.Initialize(this);

            // If we have a scope, we need to add this new handle to the LocationEnvironment.
            if (this.scope != null)
            {
                this.scope.Environment.AddHandle(value);
            }
            // otherwise add it to the Executor.
            else
            {
                this.executor.AddHandle(value);
            }

            return value;
        }

        public T GetExtension<T>() where T : class
        {
            return this.executor.GetExtension<T>();
        }

        public void UninitializeHandle(Handle handle)
        {
            ThrowIfDisposed();
            handle.Uninitialize(this);
        }

        internal object CreateAndInitializeHandle(Type handleType)
        {
            Fx.Assert(ActivityUtilities.IsHandle(handleType), "This should only be called with Handle subtypes.");

            object value = Activator.CreateInstance(handleType);

            ((Handle)value).Initialize(this);

            // If we have a scope, we need to add this new handle to the LocationEnvironment.
            if (this.scope != null)
            {
                this.scope.Environment.AddHandle((Handle)value);
            }
            // otherwise add it to the Executor.
            else
            {
                this.executor.AddHandle((Handle)value);
            }

            return value;
        }

        internal BookmarkScope CreateAndRegisterBookmarkScope()
        {
            return this.executor.BookmarkScopeManager.CreateAndRegisterScope(Guid.Empty);
        }

        internal void UnregisterBookmarkScope(BookmarkScope bookmarkScope)
        {
            Fx.Assert(bookmarkScope != null, "The sub instance should not equal null.");

            this.executor.BookmarkScopeManager.UnregisterScope(bookmarkScope);
        }

        void ThrowIfDisposed()
        {
            if (this.isDiposed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(SR.HandleInitializationContextDisposed));
            }
        }

        internal void Dispose()
        {
            this.isDiposed = true;
        }
    }
}


