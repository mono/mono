//------------------------------------------------------------------------------
// <copyright file="SessionStateUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * SessionStateUtil
 *
 */
namespace System.Web.SessionState {
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Util;
    using System.Xml;

    public static class SessionStateUtility {
        internal const String SESSION_KEY = "AspSession";

        // Used by AltSerialization's BinaryFormatter for session serialization customization
        public static ISurrogateSelector SerializationSurrogateSelector {
            [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
            get;
            [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
            set;
        }

        // Called by custom session state module if they want to raise Session_End.
        static public void RaiseSessionEnd(IHttpSessionState session, Object eventSource, EventArgs eventArgs) {
            HttpApplicationFactory.EndSession(new HttpSessionState(session), eventSource, eventArgs);
        }

        // Called by custom session state module
        static public void AddHttpSessionStateToContext(HttpContext context, IHttpSessionState container) {
            HttpSessionState sessionState = new HttpSessionState(container);

            try {
                context.Items.Add(SESSION_KEY, sessionState);
            }
            catch (ArgumentException) {
                throw new HttpException(SR.GetString(SR.Cant_have_multiple_session_module));
            }
        }

        static internal void AddHttpSessionStateModuleToContext(HttpContext context, SessionStateModule module, bool delayed) {
            context.AddHttpSessionStateModule(module, delayed);
        }

        static internal void RemoveHttpSessionStateFromContext(HttpContext context, bool delayed) {
            if (!delayed) {
                context.Items.Remove(SESSION_KEY);
            }

            context.RemoveHttpSessionStateModule();
         }

        // Called by custom session state module
        static public void RemoveHttpSessionStateFromContext(HttpContext context) {
            RemoveHttpSessionStateFromContext(context, false);
        }

        // Called by custom session state module
        static public IHttpSessionState GetHttpSessionStateFromContext(HttpContext context) {
                return context.Session.Container;
        }

        static public HttpStaticObjectsCollection GetSessionStaticObjects(HttpContext context) {
            return context.Application.SessionStaticObjects.Clone();
        }

        internal static SessionStateStoreData CreateLegitStoreData(HttpContext context,
                                                    ISessionStateItemCollection sessionItems,
                                                    HttpStaticObjectsCollection staticObjects,
                                                    int timeout) {
            if (sessionItems == null) {
                sessionItems = new SessionStateItemCollection();
            }

            if (staticObjects == null && context != null) {
                staticObjects = SessionStateUtility.GetSessionStaticObjects(context);
            }

            return new SessionStateStoreData(sessionItems, staticObjects, timeout);
        }


        // This method will take an item and serialize it
        [SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
        internal static void Serialize(SessionStateStoreData item, Stream stream) {
            bool    hasItems = true;
            bool    hasStaticObjects = true;

            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(item.Timeout);

            if (item.Items == null || item.Items.Count == 0) {
                hasItems = false;
            }
            writer.Write(hasItems);

            if (item.StaticObjects == null || item.StaticObjects.NeverAccessed) {
                hasStaticObjects = false;
            }
            writer.Write(hasStaticObjects);

            if (hasItems) {
                ((SessionStateItemCollection)item.Items).Serialize(writer);
            }

            if (hasStaticObjects) {
                item.StaticObjects.Serialize(writer);
            }

            // Prevent truncation of the stream
            writer.Write(unchecked((byte)0xff));
        }

        // This will deserialize and return an item.
        // This version uses the default classes for SessionStateItemCollection, HttpStaticObjectsCollection
        // and SessionStateStoreData
        [SecurityPermission(SecurityAction.Assert, SerializationFormatter = true)]
        internal static SessionStateStoreData Deserialize(HttpContext context, Stream    stream) {

            int                 timeout;
            SessionStateItemCollection   sessionItems;
            bool                hasItems;
            bool                hasStaticObjects;
            HttpStaticObjectsCollection staticObjects;
            Byte                eof;

            Debug.Assert(context != null);

            try {
                BinaryReader reader = new BinaryReader(stream);
                timeout = reader.ReadInt32();
                hasItems = reader.ReadBoolean();
                hasStaticObjects = reader.ReadBoolean();

                if (hasItems) {
                    sessionItems = SessionStateItemCollection.Deserialize(reader);
                }
                else {
                    sessionItems = new SessionStateItemCollection();
                }

                if (hasStaticObjects) {
                    staticObjects = HttpStaticObjectsCollection.Deserialize(reader);
                }
                else {
                    staticObjects = SessionStateUtility.GetSessionStaticObjects(context);
                }

                eof = reader.ReadByte();
                if (eof != 0xff) {
                    throw new HttpException(SR.GetString(SR.Invalid_session_state));
                }
            }
            catch (EndOfStreamException) {
                throw new HttpException(SR.GetString(SR.Invalid_session_state));
            }

            return new SessionStateStoreData(sessionItems, staticObjects, timeout);
        }

        static internal void SerializeStoreData(SessionStateStoreData item, int initialStreamSize, out byte[] buf, out int length, bool compressionEnabled) {
            using(MemoryStream s = new MemoryStream(initialStreamSize)) {
                SessionStateUtility.Serialize(item, s);
                if(compressionEnabled) {
                    byte[] serializedBuffer = s.GetBuffer();
                    int serializedLength = (int)s.Length;
                    // truncate the MemoryStream so we can write the compressed data in it
                    s.SetLength(0);
                    // compress the serialized bytes
                    using(DeflateStream zipStream = new DeflateStream(s, CompressionMode.Compress, true)) {
                        zipStream.Write(serializedBuffer, 0, serializedLength);
                    }
                    // if the session state tables have ANSI_PADDING disabled, last )s are trimmed.
                    // This shouldn't happen, but to be sure, we are padding with an extra byte
                    s.WriteByte((byte)0xff);
                }
                buf = s.GetBuffer();
                length = (int) s.Length;
            }
        }

        static internal SessionStateStoreData DeserializeStoreData(HttpContext context, Stream stream, bool compressionEnabled) {
            if(compressionEnabled) {
                // apply the compression decorator on top of the stream
                // the data should not be bigger than 4GB - compression doesn't work for more than that
                using(DeflateStream zipStream = new DeflateStream(stream, CompressionMode.Decompress, true)) {
                    return SessionStateUtility.Deserialize(context, zipStream);
                }
            }
            return SessionStateUtility.Deserialize(context, stream);
        }
    }

}
