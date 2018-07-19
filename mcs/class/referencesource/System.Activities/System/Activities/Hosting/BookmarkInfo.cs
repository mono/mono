//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Hosting
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime;

    [DataContract]
    [Fx.Tag.XamlVisible(false)]
    public sealed class BookmarkInfo
    {
        string bookmarkName;
        BookmarkScopeInfo scopeInfo;
        string ownerDisplayName;

        internal BookmarkInfo(string bookmarkName, string ownerDisplayName, BookmarkScopeInfo scopeInfo)
        {
            this.BookmarkName = bookmarkName;
            this.OwnerDisplayName = ownerDisplayName;
            this.ScopeInfo = scopeInfo;
        }
        
        public string BookmarkName
        {
            get
            {
                return this.bookmarkName;
            }
            private set
            {
                this.bookmarkName = value;
            }
        }
        
        public string OwnerDisplayName
        {
            get
            {
                return this.ownerDisplayName;
            }
            private set
            {
                this.ownerDisplayName = value;
            }
        }
        
        public BookmarkScopeInfo ScopeInfo
        {
            get
            {
                return this.scopeInfo;
            }
            private set
            {
                this.scopeInfo = value;
            }
        }

        [DataMember(Name = "BookmarkName")]
        internal string SerializedBookmarkName
        {
            get { return this.BookmarkName; }
            set { this.BookmarkName = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "OwnerDisplayName")]
        internal string SerializedOwnerDisplayName
        {
            get { return this.OwnerDisplayName; }
            set { this.OwnerDisplayName = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "ScopeInfo")]
        internal BookmarkScopeInfo SerializedScopeInfo
        {
            get { return this.ScopeInfo; }
            set { this.ScopeInfo = value; }
        }
    }
}
