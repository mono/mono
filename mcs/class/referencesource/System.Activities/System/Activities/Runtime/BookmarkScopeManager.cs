//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;

    [DataContract]
    class BookmarkScopeManager
    {
        Dictionary<BookmarkScope, BookmarkManager> bookmarkManagers;
        List<BookmarkScope> uninitializedScopes;
        List<InstanceKey> keysToAssociate;
        List<InstanceKey> keysToDisassociate;

        BookmarkScope defaultScope;

        long nextTemporaryId;

        public BookmarkScopeManager()
        {
            this.nextTemporaryId = 1;
            this.defaultScope = CreateAndRegisterScope(Guid.Empty);
        }

        public BookmarkScope Default
        {
            get
            {
                return this.defaultScope;
            }
        }

        public bool HasKeysToUpdate
        {
            get
            {
                if (this.keysToAssociate != null && this.keysToAssociate.Count > 0)
                {
                    return true;
                }

                if (this.keysToDisassociate != null && this.keysToDisassociate.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        [DataMember(Name = "defaultScope")]
        internal BookmarkScope SerializedDefaultScope
        {
            get { return this.defaultScope; }
            set { this.defaultScope = value; }
        }

        [DataMember(Name = "nextTemporaryId")]
        internal long SerializedNextTemporaryId
        {
            get { return this.nextTemporaryId; }
            set { this.nextTemporaryId = value; }
        }

        [DataMember(EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal Dictionary<BookmarkScope, BookmarkManager> SerializedBookmarkManagers
        {
            get
            {
                Fx.Assert(this.bookmarkManagers != null && this.bookmarkManagers.Count > 0, "We always have the default sub instance.");

                return this.bookmarkManagers;
            }
            set
            {
                Fx.Assert(value != null, "We don't serialize null.");
                this.bookmarkManagers = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal List<BookmarkScope> SerializedUninitializedScopes
        {
            get
            {
                if (this.uninitializedScopes == null || this.uninitializedScopes.Count == 0)
                {
                    return null;
                }
                else
                {
                    return this.uninitializedScopes;
                }
            }
            set
            {
                Fx.Assert(value != null, "We don't serialize null.");
                this.uninitializedScopes = value;
            }
        }

        long GetNextTemporaryId()
        {
            long temp = this.nextTemporaryId;
            this.nextTemporaryId++;

            return temp;
        }

        public Bookmark CreateBookmark(string name, BookmarkScope scope, BookmarkCallback callback, ActivityInstance owningInstance, BookmarkOptions options)
        {
            Fx.Assert(scope != null, "We should never have a null scope.");

            BookmarkManager manager = null;
            BookmarkScope lookupScope = scope;

            if (scope.IsDefault)
            {
                lookupScope = this.defaultScope;
            }

            if (!this.bookmarkManagers.TryGetValue(lookupScope, out manager))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.RegisteredBookmarkScopeRequired));
            }

            return manager.CreateBookmark(name, callback, owningInstance, options);
        }

        public bool RemoveBookmark(Bookmark bookmark, BookmarkScope scope, ActivityInstance instanceAttemptingRemove)
        {
            Fx.Assert(scope != null, "We should never have a null scope.");

            BookmarkScope lookupScope = scope;

            if (scope.IsDefault)
            {
                lookupScope = this.defaultScope;
            }

            BookmarkManager manager;
            if (this.bookmarkManagers.TryGetValue(lookupScope, out manager))
            {
                return manager.Remove(bookmark, instanceAttemptingRemove);
            }
            else
            {
                return false;
            }
        }

        public BookmarkResumptionResult TryGenerateWorkItem(ActivityExecutor executor, ref Bookmark bookmark, BookmarkScope scope, object value, ActivityInstance isolationInstance, bool nonScopedBookmarksExist, out ActivityExecutionWorkItem workItem)
        {
            Fx.Assert(scope != null, "We should never have a null sub instance.");

            BookmarkManager manager = null;
            workItem = null;
            BookmarkScope lookupScope = scope;

            if (scope.IsDefault)
            {
                lookupScope = this.defaultScope;
            }

            // We don't really care about the return value since we'll
            // use null to know we should check uninitialized sub instances
            this.bookmarkManagers.TryGetValue(lookupScope, out manager);

            if (manager == null)
            {
                Fx.Assert(lookupScope != null, "The sub instance should not be default if we are here.");

                BookmarkResumptionResult finalResult = BookmarkResumptionResult.NotFound;

                // Check the uninitialized sub instances for a matching bookmark
                if (this.uninitializedScopes != null)
                {
                    for (int i = 0; i < this.uninitializedScopes.Count; i++)
                    {
                        BookmarkScope uninitializedScope = this.uninitializedScopes[i];

                        Fx.Assert(this.bookmarkManagers.ContainsKey(uninitializedScope), "We must always have the uninitialized sub instances.");

                        Bookmark internalBookmark;
                        BookmarkCallbackWrapper callbackWrapper;
                        BookmarkResumptionResult resumptionResult;
                        if (!this.bookmarkManagers[uninitializedScope].TryGetBookmarkFromInternalList(bookmark, out internalBookmark, out callbackWrapper))
                        {
                            resumptionResult = BookmarkResumptionResult.NotFound;
                        }
                        else if (IsExclusiveScopeUnstable(internalBookmark))
                        {
                            resumptionResult = BookmarkResumptionResult.NotReady;
                        }
                        else 
                        {
                            resumptionResult = this.bookmarkManagers[uninitializedScope].TryGenerateWorkItem(executor, true, ref bookmark, value, isolationInstance, out workItem);
                        }

                        if (resumptionResult == BookmarkResumptionResult.Success)
                        {
                            // We are using InitializeBookmarkScopeWithoutKeyAssociation because we know this is a new uninitialized scope and
                            // the key we would associate is already associated. And if we did the association here, the subsequent call to
                            // FlushBookmarkScopeKeys would try to flush it out, but it won't have the transaction correct so will hang waiting for
                            // the transaction that has the PersistenceContext locked to complete. But it won't complete successfully until
                            // we finish processing here.
                            InitializeBookmarkScopeWithoutKeyAssociation(uninitializedScope, scope.Id);

                            // We've found what we were looking for
                            return BookmarkResumptionResult.Success;
                        }
                        else if (resumptionResult == BookmarkResumptionResult.NotReady)
                        {
                            // This uninitialized sub-instance has a matching bookmark but
                            // it can't currently be resumed.  We won't return BookmarkNotFound
                            // because of this.
                            finalResult = BookmarkResumptionResult.NotReady;
                        }
                        else
                        {
                            if (finalResult == BookmarkResumptionResult.NotFound)
                            {
                                // If we still are planning on returning failure then
                                // we'll incur the cost of seeing if this scope is
                                // stable or not.

                                if (!IsStable(uninitializedScope, nonScopedBookmarksExist))
                                {
                                    // There exists an uninitialized scope which is unstable.
                                    // At the very least this means we'll return NotReady since
                                    // this uninitialized scope might eventually contain this
                                    // bookmark.
                                    finalResult = BookmarkResumptionResult.NotReady;
                                }
                            }
                        }
                    }
                }

                return finalResult;
            }
            else
            {
                Bookmark bookmarkFromList;
                BookmarkCallbackWrapper callbackWrapper;
                BookmarkResumptionResult resumptionResult;
                if (!manager.TryGetBookmarkFromInternalList(bookmark, out bookmarkFromList, out callbackWrapper))
                {
                    resumptionResult = BookmarkResumptionResult.NotFound;
                }
                else
                {
                    if (IsExclusiveScopeUnstable(bookmarkFromList))
                    {
                        resumptionResult = BookmarkResumptionResult.NotReady;
                    }
                    else
                    {
                        resumptionResult = manager.TryGenerateWorkItem(executor, true, ref bookmark, value, isolationInstance, out workItem);
                    }
               }


                if (resumptionResult == BookmarkResumptionResult.NotFound)
                {
                    if (!IsStable(lookupScope, nonScopedBookmarksExist))
                    {
                        resumptionResult = BookmarkResumptionResult.NotReady;
                    }
                }

                return resumptionResult;
            }
        }

        public void PopulateBookmarkInfo(ref List<BookmarkInfo> bookmarks)
        {
            foreach (BookmarkManager manager in this.bookmarkManagers.Values)
            {
                if (manager.HasBookmarks)
                {
                    if (bookmarks == null)
                    {
                        bookmarks = new List<BookmarkInfo>();
                    }

                    manager.PopulateBookmarkInfo(bookmarks);
                }
            }
        }

        public ReadOnlyCollection<BookmarkInfo> GetBookmarks(BookmarkScope scope)
        {
            Fx.Assert(scope != null, "We should never be passed null here.");

            BookmarkManager manager = null;
            BookmarkScope lookupScope = scope;

            if (scope.IsDefault)
            {
                lookupScope = this.defaultScope;
            }

            if (this.bookmarkManagers.TryGetValue(lookupScope, out manager))
            {
                if (!manager.HasBookmarks)
                {
                    manager = null;
                }
            }


            if (manager != null)
            {
                List<BookmarkInfo> bookmarks = new List<BookmarkInfo>();

                manager.PopulateBookmarkInfo(bookmarks);

                return new ReadOnlyCollection<BookmarkInfo>(bookmarks);
            }
            else
            {
                return null;
            }
        }

        public ICollection<InstanceKey> GetKeysToAssociate()
        {
            if (this.keysToAssociate == null || this.keysToAssociate.Count == 0)
            {
                return null;
            }

            ICollection<InstanceKey> result = this.keysToAssociate;
            this.keysToAssociate = null;
            return result;
        }

        public ICollection<InstanceKey> GetKeysToDisassociate()
        {
            if (this.keysToDisassociate == null || this.keysToDisassociate.Count == 0)
            {
                return null;
            }

            ICollection<InstanceKey> result = this.keysToDisassociate;
            this.keysToDisassociate = null;
            return result;
        }

        public void InitializeScope(BookmarkScope scope, Guid id)
        {
            Fx.Assert(!scope.IsInitialized, "This should have been checked by the caller.");

            BookmarkScope lookupScope = InitializeBookmarkScopeWithoutKeyAssociation(scope, id);
            CreateAssociatedKey(lookupScope);
        }

        public BookmarkScope InitializeBookmarkScopeWithoutKeyAssociation(BookmarkScope scope, Guid id)
        {
            Fx.Assert(!scope.IsInitialized, "This should have been checked by the caller.");

            BookmarkScope lookupScope = scope;

            if (scope.IsDefault)
            {
                lookupScope = this.defaultScope;
            }

            if (this.uninitializedScopes == null || !this.uninitializedScopes.Contains(lookupScope))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BookmarkScopeNotRegisteredForInitialize));
            }

            Fx.Assert(this.bookmarkManagers != null, "This is never null if uninitializedScopes is non-null.");

            if (this.bookmarkManagers.ContainsKey(new BookmarkScope(id)))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BookmarkScopeWithIdAlreadyExists(id)));
            }

            BookmarkManager bookmarks = this.bookmarkManagers[lookupScope];
            this.bookmarkManagers.Remove(lookupScope);
            this.uninitializedScopes.Remove(lookupScope);

            long temporaryId = lookupScope.TemporaryId;
            // We initialize and re-add to our dictionary.  We have to
            // re-add because the hash has changed.
            lookupScope.Id = id;
            this.bookmarkManagers.Add(lookupScope, bookmarks);

            if (TD.BookmarkScopeInitializedIsEnabled())
            {
                TD.BookmarkScopeInitialized(temporaryId.ToString(CultureInfo.InvariantCulture), lookupScope.Id.ToString());
            }

            return lookupScope;
        }

        public BookmarkScope CreateAndRegisterScope(Guid scopeId)
        {
            return this.CreateAndRegisterScope(scopeId, null);
        }

        internal BookmarkScope CreateAndRegisterScope(Guid scopeId, BookmarkScopeHandle scopeHandle)
        {
            if (this.bookmarkManagers == null)
            {
                this.bookmarkManagers = new Dictionary<BookmarkScope, BookmarkManager>();
            }

            BookmarkScope scope = null;
            if (scopeId == Guid.Empty)
            {
                //
                // This is the very first activity which started the sub-instance
                //
                scope = new BookmarkScope(GetNextTemporaryId());
                this.bookmarkManagers.Add(scope, new BookmarkManager(scope, scopeHandle));

                if (TD.CreateBookmarkScopeIsEnabled())
                {
                    TD.CreateBookmarkScope(ActivityUtilities.GetTraceString(scope));
                }

                if (this.uninitializedScopes == null)
                {
                    this.uninitializedScopes = new List<BookmarkScope>();
                }

                this.uninitializedScopes.Add(scope);
            }
            else
            {
                //
                // Try to find one in the existing sub-instances
                //
                foreach (BookmarkScope eachScope in this.bookmarkManagers.Keys)
                {
                    if (eachScope.Id.Equals(scopeId))
                    {
                        scope = eachScope;
                        break;
                    }
                }

                //
                // We did not find one, e.g. the first receive will get the correlation id from the 
                // correlation channel
                //
                if (scope == null)
                {
                    scope = new BookmarkScope(scopeId);
                    this.bookmarkManagers.Add(scope, new BookmarkManager(scope, scopeHandle));

                    if (TD.CreateBookmarkScopeIsEnabled())
                    {
                        TD.CreateBookmarkScope(string.Format(CultureInfo.InvariantCulture, "Id: {0}", ActivityUtilities.GetTraceString(scope)));
                    }
                }

                CreateAssociatedKey(scope);
            }

            return scope;
        }

        void CreateAssociatedKey(BookmarkScope newScope)
        {
            if (this.keysToAssociate == null)
            {
                this.keysToAssociate = new List<InstanceKey>(2);
            }
            this.keysToAssociate.Add(new InstanceKey(newScope.Id));
        }

        public void UnregisterScope(BookmarkScope scope)
        {
            Fx.Assert(!scope.IsDefault, "Cannot unregister the default sub instance.");

            if (this.bookmarkManagers == null || !this.bookmarkManagers.ContainsKey(scope))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BookmarkScopeNotRegisteredForUnregister));
            }

            if (this.bookmarkManagers[scope].HasBookmarks)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BookmarkScopeHasBookmarks));
            }

            this.bookmarkManagers.Remove(scope);

            if (!scope.IsInitialized)
            {
                Fx.Assert(this.uninitializedScopes != null && this.uninitializedScopes.Contains(scope), "Something is wrong with our housekeeping.");

                this.uninitializedScopes.Remove(scope);
            }
            else
            {
                if (this.keysToDisassociate == null)
                {
                    this.keysToDisassociate = new List<InstanceKey>(2);
                }
                this.keysToDisassociate.Add(new InstanceKey(scope.Id));
                Fx.Assert(this.uninitializedScopes == null || !this.uninitializedScopes.Contains(scope), "We shouldn't have this in the uninitialized list.");
            }
        }

        bool IsStable(BookmarkScope scope, bool nonScopedBookmarksExist)
        {
            Fx.Assert(this.bookmarkManagers.ContainsKey(scope), "The caller should have made sure this scope exists in the bookmark managers dictionary.");

            if (nonScopedBookmarksExist)
            {
                return false;
            }

            if (this.bookmarkManagers != null)
            {
                foreach (KeyValuePair<BookmarkScope, BookmarkManager> scopeBookmarks in this.bookmarkManagers)
                {
                    IEquatable<BookmarkScope> comparison = scopeBookmarks.Key;
                    if (!comparison.Equals(scope))
                    {
                        if (scopeBookmarks.Value.HasBookmarks)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool IsExclusiveScopeUnstable(Bookmark bookmark)
        {
            if (bookmark.ExclusiveHandles != null)
            {
                for (int i = 0; i < bookmark.ExclusiveHandles.Count; i++)
                {
                    ExclusiveHandle handle = bookmark.ExclusiveHandles[i];
                    Fx.Assert(handle != null, "Internal error..ExclusiveHandle was null");
                    if ((handle.ImportantBookmarks != null && handle.ImportantBookmarks.Contains(bookmark)) && (handle.UnimportantBookmarks != null && handle.UnimportantBookmarks.Count != 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void PurgeBookmarks(BookmarkManager nonScopedBookmarkManager, Bookmark singleBookmark, IList<Bookmark> multipleBookmarks)
        {
            if (singleBookmark != null)
            {
                PurgeBookmark(singleBookmark, nonScopedBookmarkManager);
            }
            else
            {
                Fx.Assert(multipleBookmarks != null, "caller should never pass null");
                for (int i = 0; i < multipleBookmarks.Count; i++)
                {
                    Bookmark bookmark = multipleBookmarks[i];

                    PurgeBookmark(bookmark, nonScopedBookmarkManager);
                }
            }
        }

        void PurgeBookmark(Bookmark bookmark, BookmarkManager nonScopedBookmarkManager)
        {
            BookmarkManager manager = null;

            if (bookmark.Scope != null)
            {
                BookmarkScope lookupScope = bookmark.Scope;

                if (bookmark.Scope.IsDefault)
                {
                    lookupScope = this.defaultScope;
                }

                Fx.Assert(this.bookmarkManagers.ContainsKey(bookmark.Scope), "We should have the single bookmark's sub instance registered");
                manager = this.bookmarkManagers[bookmark.Scope];
            }
            else
            {
                manager = nonScopedBookmarkManager;
            }

            manager.PurgeSingleBookmark(bookmark);
        }
    }
}
