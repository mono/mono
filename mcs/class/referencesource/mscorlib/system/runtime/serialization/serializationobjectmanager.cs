// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: SerializationObjectManager
**
**
** Purpose: Encapsulates serialization time management, mainly invoking serialization events
**
**
============================================================*/
namespace System.Runtime.Serialization {
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;

    public sealed class SerializationObjectManager{
        private Hashtable m_objectSeenTable = new Hashtable(); // Table to keep track of objects [OnSerializing] has been called on
        private SerializationEventHandler m_onSerializedHandler;
        private StreamingContext m_context;


        public SerializationObjectManager(StreamingContext context)
        {
            m_context = context;
            m_objectSeenTable = new Hashtable();
        }
        
        [System.Security.SecurityCritical]  // auto-generated_required
        public void RegisterObject(Object obj)
        {
            // Invoke OnSerializing for this object
            SerializationEvents cache = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
            // Check to make sure type has serializing events
            if (cache.HasOnSerializingEvents)
            {
                // Check to see if we have invoked the events on the object
                if (m_objectSeenTable[obj] == null) 
                {
                        m_objectSeenTable[obj] = true;
                        // Invoke the events
                        cache.InvokeOnSerializing(obj, m_context);
                        // Register for OnSerialized event
                        AddOnSerialized(obj);
                }
            }
        }

        public void RaiseOnSerializedEvent()
        {
            if (m_onSerializedHandler != null) {
                m_onSerializedHandler(m_context);
            }
        }

        [System.Security.SecuritySafeCritical]
        private void AddOnSerialized(Object obj)
        {
            SerializationEvents cache = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
            m_onSerializedHandler = cache.AddOnSerialized(obj, m_onSerializedHandler);
        }

    
    }

}

