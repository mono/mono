//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Activities.Runtime;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Globalization;

    [DataContract]
    [Fx.Tag.XamlVisible(false)]
    public class Bookmark : IEquatable<Bookmark>
    {
        static Bookmark asyncOperationCompletionBookmark = new Bookmark(-1);
        static IEqualityComparer<Bookmark> comparer;

        //Used only when exclusive scopes are involved
        ExclusiveHandleList exclusiveHandlesThatReferenceThis;

        long id;

        string externalName;

        Bookmark(long id)
        {
            Fx.Assert(id != 0, "id should not be zero");
            this.id = id;
        }

        public Bookmark(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            this.externalName = name;
        }

        internal static Bookmark AsyncOperationCompletionBookmark
        {
            get
            {
                return asyncOperationCompletionBookmark;
            }
        }

        internal static IEqualityComparer<Bookmark> Comparer
        {
            get
            {
                if (comparer == null)
                {
                    comparer = new BookmarkComparer();
                }

                return comparer;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "exclusiveHandlesThatReferenceThis", Order = 2)]
        internal ExclusiveHandleList SerializedExclusiveHandlesThatReferenceThis
        {
            get { return this.exclusiveHandlesThatReferenceThis; }
            set { this.exclusiveHandlesThatReferenceThis = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "id", Order = 0)]
        internal long SerializedId
        {
            get { return this.id; }
            set { this.id = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "externalName", Order = 1)]
        internal string SerializedExternalName
        {
            get { return this.externalName; }
            set { this.externalName = value; }
        }

        [DataMember(EmitDefaultValue = false)]
        internal BookmarkScope Scope
        {
            get;
            set;
        }

        internal bool IsNamed
        {
            get
            {
                return this.id == 0;
            }
        }

        public string Name
        {
            get
            {
                if (this.IsNamed)
                {
                    return this.externalName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        internal long Id
        {
            get
            {
                Fx.Assert(!this.IsNamed, "We should only get the id for unnamed bookmarks.");

                return this.id;
            }
        }

        internal ExclusiveHandleList ExclusiveHandles
        {
            get
            {
                return this.exclusiveHandlesThatReferenceThis;
            }
            set
            {
                this.exclusiveHandlesThatReferenceThis = value;
            }
        }


        internal static Bookmark Create(long id)
        {
            return new Bookmark(id);
        }

        internal BookmarkInfo GenerateBookmarkInfo(BookmarkCallbackWrapper bookmarkCallback)
        {
            Fx.Assert(this.IsNamed, "Can only generate BookmarkInfo for external bookmarks");

            BookmarkScopeInfo scopeInfo = null;

            if (this.Scope != null)
            {
                scopeInfo = this.Scope.GenerateScopeInfo();
            }

            return new BookmarkInfo(this.externalName, bookmarkCallback.ActivityInstance.Activity.DisplayName, scopeInfo);
        }

        public bool Equals(Bookmark other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            if (this.IsNamed)
            {
                return other.IsNamed && this.externalName == other.externalName;
            }
            else
            {
                return this.id == other.id;
            }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Bookmark);
        }

        public override int GetHashCode()
        {
            if (this.IsNamed)
            {
                return this.externalName.GetHashCode();
            }
            else
            {
                return this.id.GetHashCode();
            }
        }

        public override string ToString()
        {
            if (this.IsNamed)
            {
                return this.Name;
            }
            else
            {
                return this.Id.ToString(CultureInfo.InvariantCulture);
            }
        }

        [DataContract]
        internal class BookmarkComparer : IEqualityComparer<Bookmark>
        {
            public BookmarkComparer()
            {
            }

            public bool Equals(Bookmark x, Bookmark y)
            {
                if (object.ReferenceEquals(x, null))
                {
                    return object.ReferenceEquals(y, null);
                }

                return x.Equals(y);
            }

            public int GetHashCode(Bookmark obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
