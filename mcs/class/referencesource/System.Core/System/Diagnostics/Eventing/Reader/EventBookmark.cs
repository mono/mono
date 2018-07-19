// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: EventBookmark
**
** Purpose: 
** This public class represents an opaque Event Bookmark obtained
** from an EventRecord.  The bookmark denotes a unique identifier
** for the event instance as well as marks the location in the 
** the result set of the EventReader that the event instance was 
** obtained from.
**
============================================================*/
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace System.Diagnostics.Eventing.Reader {

    //
    // NOTE: This class must be generic enough to be used across 
    // eventing base implementations.  Cannot add anything 
    // that ties it to one particular implementation.
    //
    
    /// <summary>
    /// Represents an opaque Event Bookmark obtained from an EventRecord.  
    /// The bookmark denotes a unique identifier for the event instance as 
    /// well as marks the location in the the result set of the EventReader 
    /// that the event instance was obtained from.
    /// </summary>
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventBookmark : ISerializable {
        string bookmark;

        internal EventBookmark(string bookmarkText) {
            if (bookmarkText == null)
                throw new ArgumentNullException("bookmarkText");
            this.bookmark = bookmarkText;
        }

        protected EventBookmark(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException("info");
            this.bookmark = info.GetString("BookmarkText");    
        }

        // SecurityCritical due to inherited link demand for GetObjectData.
        [System.Security.SecurityCritical,SecurityPermissionAttribute(SecurityAction.LinkDemand,Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
             GetObjectData( info, context );
        }

        // SecurityCritical due to inherited link demand for GetObjectData.
        [System.Security.SecurityCritical,SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context) {

            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("BookmarkText", this.bookmark);
        }

        internal string BookmarkText { get { return bookmark; } } 
    }
}

