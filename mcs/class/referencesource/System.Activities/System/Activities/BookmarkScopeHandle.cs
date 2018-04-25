//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Runtime.DurableInstancing;

    [DataContract]
    public sealed class BookmarkScopeHandle : Handle
    {
        BookmarkScope bookmarkScope;

        static BookmarkScopeHandle defaultBookmarkScopeHandle = new BookmarkScopeHandle(BookmarkScope.Default);

        public BookmarkScopeHandle()
        {
        }

        internal BookmarkScopeHandle(BookmarkScope bookmarkScope)
        {
            this.bookmarkScope = bookmarkScope;
            if (bookmarkScope != null)
            {
                this.bookmarkScope.IncrementHandleReferenceCount();
            }
        }

        public static BookmarkScopeHandle Default
        {
            get 
            {
                return defaultBookmarkScopeHandle;
            }
        }

        public BookmarkScope BookmarkScope
        {
            get 
            {
                return this.bookmarkScope;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "bookmarkScope")]
        internal BookmarkScope SerializedBookmarkScope
        {
            get { return this.bookmarkScope; }
            set
            {
                this.bookmarkScope = value;
                if (this.bookmarkScope != null)
                {
                    this.bookmarkScope.IncrementHandleReferenceCount();
                }
            }
        }

        //To be called from public APIs that need to verify the passed in context
        void ThrowIfContextIsNullOrDisposed(NativeActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            context.ThrowIfDisposed();
        }

        public void CreateBookmarkScope(NativeActivityContext context)
        {
            this.ThrowIfContextIsNullOrDisposed(context);
            if (this.bookmarkScope != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CreateBookmarkScopeFailed));
            }

            this.ThrowIfUninitialized();
            this.bookmarkScope = context.CreateBookmarkScope(Guid.Empty, this);
            this.bookmarkScope.IncrementHandleReferenceCount();
        }

        public void CreateBookmarkScope(NativeActivityContext context, Guid scopeId)
        {
            this.ThrowIfContextIsNullOrDisposed(context);
            if (this.bookmarkScope != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CreateBookmarkScopeFailed));
            }

            this.ThrowIfUninitialized();
            this.bookmarkScope = context.CreateBookmarkScope(scopeId, this);
            this.bookmarkScope.IncrementHandleReferenceCount();
        }
        
        public void Initialize(NativeActivityContext context, Guid scope)
        {
            this.ThrowIfContextIsNullOrDisposed(context);
            this.ThrowIfUninitialized();
            this.bookmarkScope.Initialize(context, scope);
        }

        protected override void OnUninitialize(HandleInitializationContext context)
        {
            if (this.bookmarkScope != null)
            {
                int scopeRefCount = this.bookmarkScope.DecrementHandleReferenceCount();
                DisassociateInstanceKeysExtension extension = context.GetExtension<DisassociateInstanceKeysExtension>();
                // We only unregister the BookmarkScope if the extension exists and is enabled and if we had the last reference to it.
                if ((extension != null) && extension.AutomaticDisassociationEnabled && (scopeRefCount == 0))
                {
                    context.UnregisterBookmarkScope(this.bookmarkScope);
                }
            }
            base.OnUninitialize(context);
        }
    }
}


