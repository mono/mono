//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    [Fx.Tag.XamlVisible(false)]
    public sealed class BookmarkScope : IEquatable<BookmarkScope>
    {
        static BookmarkScope defaultBookmarkScope;

        long temporaryId;
                
        Guid id;

        int handleReferenceCount;

        internal BookmarkScope(long temporaryId)
        {
            Fx.Assert(temporaryId != default(long), "Should never call this constructor with the default value.");
            this.temporaryId = temporaryId;
        }

        public BookmarkScope(Guid id)
        {
            this.id = id;
        }

        BookmarkScope()
        {
            // Only called for making the default sub instance
            // which has an Id of Guid.Empty
        }

        public bool IsInitialized
        {
            get
            {
                return this.temporaryId == default(long);
            }
        }

        public Guid Id
        {
            get
            {
                return this.id;
            }
            internal set
            {
                Fx.Assert(value != Guid.Empty, "Cannot set this to Guid.Empty.");
                Fx.Assert(!this.IsInitialized, "Can only set this when uninitialized.");

                this.id = value;
                this.temporaryId = default(long);
            }
        }

        internal int IncrementHandleReferenceCount()
        {
            return ++this.handleReferenceCount;
        }

        internal int DecrementHandleReferenceCount()
        {
            return --this.handleReferenceCount;
        }

        [DataMember(EmitDefaultValue = false, Name = "temporaryId")]
        internal long SerializedTemporaryId
        {
            get { return this.temporaryId; }
            set { this.temporaryId = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "id")]
        internal Guid SerializedId
        {
            get { return this.id; }
            set { this.id = value; }
        }

        internal long TemporaryId
        {
            get
            {
                return this.temporaryId;
            }
        }

        public static BookmarkScope Default
        {
            get
            {
                if (defaultBookmarkScope == null)
                {
                    defaultBookmarkScope = new BookmarkScope();
                }

                return defaultBookmarkScope;
            }
        }

        internal bool IsDefault
        {
            get
            {
                // In the strictest sense the default is not initiailized.
                // The Default BookmarkScope is really just a loose reference
                // to the instance specific default that you can get to
                // through NativeActivityContext.DefaultBookmarkScope.
                // We use a scope initialized to Guid.Empty to signify this
                // "loose reference".
                return this.IsInitialized && this.id == Guid.Empty;
            }
        }

        public void Initialize(NativeActivityContext context, Guid id)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            if (id == Guid.Empty)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("id");
            }

            if (this.IsInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BookmarkScopeAlreadyInitialized));
            }

            context.InitializeBookmarkScope(this, id);
        }

        public override int GetHashCode()
        {
            if (this.IsInitialized)
            {
                return this.id.GetHashCode();
            }
            else
            {
                return this.temporaryId.GetHashCode();
            }
        }

        internal BookmarkScopeInfo GenerateScopeInfo()
        {
            if (this.IsInitialized)
            {
                return new BookmarkScopeInfo(this.Id);
            }
            else
            {
                return new BookmarkScopeInfo(this.temporaryId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool Equals(BookmarkScope other)
        {
            if (other == null)
            {
                return false;
            }

            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.IsDefault)
            {
                return other.IsDefault;
            }
            else if (this.IsInitialized)
            {
                Fx.Assert(this.id != Guid.Empty, "If we're not the default but we're initialized then we must have a non-Empty Guid.");

                if (other.id == this.id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Fx.Assert(this.temporaryId != 0, "We should have a non-zero temp id if we're not the default and not initialized.");

                if (other.temporaryId == this.temporaryId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
