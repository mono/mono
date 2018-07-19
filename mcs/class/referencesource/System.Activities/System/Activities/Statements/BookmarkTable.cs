//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime;

    [DataContract]
    class BookmarkTable
    {
        //Number of bookmarks used internally       
        static int tableSize = Enum.GetValues(typeof(CompensationBookmarkName)).Length;

        Bookmark[] bookmarkTable;

        public BookmarkTable()
        {
            this.bookmarkTable = new Bookmark[tableSize];
        }

        public Bookmark this[CompensationBookmarkName bookmarkName]
        {
            get 
            {
                return this.bookmarkTable[(int)bookmarkName];
            }
            set 
            {
                this.bookmarkTable[(int)bookmarkName] = value;
            }
        }

        [DataMember(Name = "bookmarkTable")]
        internal Bookmark[] SerializedBookmarkTable
        {
            get { return this.bookmarkTable; }
            set { this.bookmarkTable = value; }
        }
    }
}
