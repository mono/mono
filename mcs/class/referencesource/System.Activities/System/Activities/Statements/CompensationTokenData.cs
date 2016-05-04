//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    class CompensationTokenData
    {
        internal CompensationTokenData(long compensationId, long parentCompensationId)
        {
            this.CompensationId = compensationId;
            this.ParentCompensationId = parentCompensationId;
            this.BookmarkTable = new BookmarkTable();
            this.ExecutionTracker = new ExecutionTracker();
            this.CompensationState = CompensationState.Creating;
        }

        [DataMember(EmitDefaultValue = false)]
        internal long CompensationId
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        internal long ParentCompensationId
        {
            get;
            set;
        }

        [DataMember]
        internal BookmarkTable BookmarkTable
        {
            get;
            set;
        }

        [DataMember]
        internal ExecutionTracker ExecutionTracker
        {
            get;
            set;
        }

        [DefaultValue(CompensationState.Active)]
        [DataMember(EmitDefaultValue = false)]
        internal CompensationState CompensationState
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        internal string DisplayName
        {
            get;
            set;
        }
        
        [DataMember(EmitDefaultValue = false)]
        internal bool IsTokenValidInSecondaryRoot
        {
            get;
            set;
        }

        internal void RemoveBookmark(NativeActivityContext context, CompensationBookmarkName bookmarkName)
        {
            Bookmark bookmark = this.BookmarkTable[bookmarkName];

            if (bookmark != null)
            {
                context.RemoveBookmark(bookmark); 
                this.BookmarkTable[bookmarkName] = null;
            }
        }
    }
}
