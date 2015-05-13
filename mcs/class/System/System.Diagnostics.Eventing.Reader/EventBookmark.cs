namespace System.Diagnostics.Eventing.Reader
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventBookmark : ISerializable
    {
        private string bookmark;

        internal EventBookmark(string bookmarkText)
        {
            if (bookmarkText == null)
            {
                throw new ArgumentNullException("bookmarkText");
            }
            this.bookmark = bookmarkText;
        }

        protected EventBookmark(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.bookmark = info.GetString("BookmarkText");
        }

        [SecurityCritical, SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("BookmarkText", this.bookmark);
        }

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.GetObjectData(info, context);
        }

        internal string BookmarkText
        {
            get
            {
                return this.bookmark;
            }
        }
    }
}

