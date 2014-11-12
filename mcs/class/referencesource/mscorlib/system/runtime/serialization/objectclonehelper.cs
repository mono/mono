// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
 **
 ** Class: ObjectCloneHelper
 **
 **
 ** Purpose: Helper methods used by ObjectClone to process ISerializable objects etc
 **
 **
 ===========================================================*/
#if FEATURE_REMOTING

namespace System.Runtime.Serialization
{
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Diagnostics;
    using System.Globalization;

    internal static class ObjectCloneHelper
    {
        static readonly IFormatterConverter s_converter = new FormatterConverter();
        // Currently object cloner is used only to clone stuff across domains. If its used to clone objects within a domain
        // the Clone context will need to be created too..
        static readonly StreamingContext    s_cloneContext = new StreamingContext(StreamingContextStates.CrossAppDomain);
        static readonly ISerializationSurrogate  s_RemotingSurrogate = new RemotingSurrogate();
        static readonly ISerializationSurrogate  s_ObjRefRemotingSurrogate = new ObjRefSurrogate();
        
        [System.Security.SecurityCritical]  // auto-generated
        internal static object GetObjectData(object serObj, out string typeName, out string assemName, out string[] fieldNames, out object[] fieldValues)
        {
            Type objectType = null;
            object retObj = null;
            
            if (RemotingServices.IsTransparentProxy(serObj))
                objectType = typeof(MarshalByRefObject);
            else
                objectType = serObj.GetType();
                
            SerializationInfo  si  = new SerializationInfo(objectType, s_converter);
            if (serObj is ObjRef)
            {
                s_ObjRefRemotingSurrogate.GetObjectData(serObj, si, s_cloneContext);
            }
            else if (RemotingServices.IsTransparentProxy(serObj) || serObj is MarshalByRefObject)
            {
                // We can only try to smuggle objref's for actual CLR objects
                //   or for RemotingProxy's.
                if (!RemotingServices.IsTransparentProxy(serObj) ||
                    RemotingServices.GetRealProxy(serObj) is RemotingProxy)
                {                
                    ObjRef objRef = RemotingServices.MarshalInternal((MarshalByRefObject)serObj, null, null);
                    if (objRef.CanSmuggle())
                    {
                        if (RemotingServices.IsTransparentProxy(serObj))
                        {
                            RealProxy rp = RemotingServices.GetRealProxy(serObj);
                            objRef.SetServerIdentity(rp._srvIdentity);
                            objRef.SetDomainID(rp._domainID);
                        }
                        else
                        {
                            ServerIdentity srvId = (ServerIdentity)MarshalByRefObject.GetIdentity((MarshalByRefObject)serObj);
                            srvId.SetHandle();
                            objRef.SetServerIdentity(srvId.GetHandle());
                            objRef.SetDomainID(AppDomain.CurrentDomain.GetId());
                        }
                        objRef.SetMarshaledObject();
                        retObj = objRef;
                    }
                }

                if (retObj == null)
                {
                    // Deal with the non-smugglable remoting objects
                    s_RemotingSurrogate.GetObjectData(serObj, si, s_cloneContext);
                }
                
            }
            else if (serObj is ISerializable)
            {
                ((ISerializable)serObj).GetObjectData(si, s_cloneContext);
            }
            else
            {
                // Getting here means a bug in cloner
                throw new ArgumentException(Environment.GetResourceString("Arg_SerializationException"));
            }

            if (retObj == null)
            {
                typeName = si.FullTypeName;
                assemName = si.AssemblyName;
                fieldNames = si.MemberNames;
                fieldValues = si.MemberValues;
            }
            else
            {
                typeName = null;
                assemName = null;
                fieldNames = null;
                fieldValues = null;
            }

            return retObj;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static SerializationInfo PrepareConstructorArgs(object serObj, string[] fieldNames, object[] fieldValues, out StreamingContext context)
        {
            SerializationInfo si = null;
            if (serObj is ISerializable)
            {
                si = new SerializationInfo(serObj.GetType(), s_converter);

                for (int i =0; i < fieldNames.Length; i++)
                {
                    if (fieldNames[i] != null)
                        si.AddValue(fieldNames[i], fieldValues[i]);
                }
            }
            else
            {
                // We have a case where the from object was ISerializable and to object is not
                // @

                Hashtable fields = new Hashtable();
                int incomingFieldIndex = 0;
                int numIncomingFields = 0;
                for (; incomingFieldIndex < fieldNames.Length; incomingFieldIndex++)
                {
                    if (fieldNames[incomingFieldIndex] != null)
                    {
                        fields[fieldNames[incomingFieldIndex]] = fieldValues[incomingFieldIndex];
                        numIncomingFields++;
                    }
                }
                
                MemberInfo[] mi = FormatterServices.GetSerializableMembers(serObj.GetType());

                for (int index = 0; index < mi.Length; index++)
                {
                    string fieldName = mi[index].Name;
                    if (!fields.Contains(fieldName))
                    {
                        // If we are missing a field value then it's not necessarily
                        // the end of the world: check whether the field is marked
                        // [OptionalField].
                        Object [] attrs = mi[index].GetCustomAttributes(typeof(OptionalFieldAttribute), false);
                        if (attrs == null || attrs.Length == 0)
                            throw new SerializationException(Environment.GetResourceString("Serialization_MissingMember",
                                                                           mi[index],
                                                                           serObj.GetType(),
                                                                           typeof(OptionalFieldAttribute).FullName));
                        continue;
                    }

                    object value = fields[fieldName];

                    FormatterServices.SerializationSetValue(mi[index], serObj, value);
                }
            }

            context = s_cloneContext;
            return si;
        }
    }

}
#endif // FEATURE_REMOTING
