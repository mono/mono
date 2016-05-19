//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;
    using System.Collections.Generic;

    internal class HybridObjectCache
    {
        Dictionary<string, object> objectDictionary;
        Dictionary<string, object> referencedObjectDictionary;

        internal HybridObjectCache()
        {
        }

        internal void Add(string id, object obj)
        {
            if (objectDictionary == null)
                objectDictionary = new Dictionary<string, object>();

            object existingObject;
            if (objectDictionary.TryGetValue(id, out existingObject))
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.MultipleIdDefinition, id)));
            objectDictionary.Add(id, obj);
        }

        internal void Remove(string id)
        {
            if (objectDictionary != null)
                objectDictionary.Remove(id);
        }

        internal object GetObject(string id)
        {
            if (referencedObjectDictionary == null)
            {
                referencedObjectDictionary = new Dictionary<string, object>();
                referencedObjectDictionary.Add(id, null);
            }
            else if (!referencedObjectDictionary.ContainsKey(id))
            {
                referencedObjectDictionary.Add(id, null);
            }

            if (objectDictionary != null)
            {
                object obj;
                objectDictionary.TryGetValue(id, out obj);
                return obj;
            }

            return null;
        }

        internal bool IsObjectReferenced(string id)
        {
            if (referencedObjectDictionary != null)
            {
                return referencedObjectDictionary.ContainsKey(id);
            }
            return false;
        }

    }
}
