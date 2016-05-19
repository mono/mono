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
    public sealed class BookmarkScopeInfo
    {
        Guid id;
        string temporaryId;

        internal BookmarkScopeInfo(Guid id)
        {
            this.Id = id;
        }

        internal BookmarkScopeInfo(string temporaryId)
        {
            this.TemporaryId = temporaryId;
        }

        public bool IsInitialized
        {
            get
            {
                return this.TemporaryId == null;
            }
        }
        
        public Guid Id
        {
            get
            {
                return this.id;
            }
            private set
            {
                this.id = value;
            }
        }
        
        public string TemporaryId
        {
            get
            {
                return this.temporaryId;
            }
            private set
            {
                this.temporaryId = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "Id")]
        internal Guid SerializedId
        {
            get { return this.Id; }
            set { this.Id = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "TemporaryId")]
        internal string SerializedTemporaryId
        {
            get { return this.TemporaryId; }
            set { this.TemporaryId = value; }
        }
    }
}
