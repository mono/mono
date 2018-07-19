// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//==========================================================================
//  File:       MessageSmuggler.cs
//
//  Summary:    Implements objects necessary to smuggle messages across 
//              AppDomains and determine when it's possible.
//
//==========================================================================

using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;


namespace System.Runtime.Remoting.Messaging
{

    internal class MessageSmuggler
    {
        private static bool CanSmuggleObjectDirectly(Object obj)
        {
            if ((obj is String) ||
                (obj.GetType() == typeof(void)) ||
                obj.GetType().IsPrimitive)
            {
                return true;
            }

            return false;
        } // CanSmuggleObjectDirectly


        [System.Security.SecurityCritical]  // auto-generated
        protected static Object[] FixupArgs(Object[] args, ref ArrayList argsToSerialize)
        {            
            Object[] newArgs = new Object[args.Length];

            int total = args.Length;
            for (int co = 0; co < total; co++)
            {
                newArgs[co] = FixupArg(args[co], ref argsToSerialize);
            }

            return newArgs;
        } // FixupArgs


        [System.Security.SecurityCritical]  // auto-generated
        protected static Object FixupArg(Object arg, ref ArrayList argsToSerialize)
        {
            // This method examines an argument and sees if it can be smuggled in some form.
            //   If it can directly be smuggled (i.e. it is a primitive or string), we
            //   just return the same object. If it's a marshal by ref object, we
            //   see if we can smuggle the obj ref. If it's a primitive or string array,
            //   we can smuggle a cloned copy of the array. In all other cases,
            //   we add it to the list of args we want serialized, and return a
            //   placeholder element (SerializedArg).
        
            if (arg == null)
                return null;

            int index;

            // IMPORTANT!!! This should be done first because CanSmuggleObjectDirectly
            //   calls GetType() and that would slow this down.
            MarshalByRefObject mbo = arg as MarshalByRefObject;
            if (mbo != null)
            {                
                // We can only try to smuggle objref's for actual CLR objects
                //   or for RemotingProxy's.
                if (!RemotingServices.IsTransparentProxy(mbo) ||
                    RemotingServices.GetRealProxy(mbo) is RemotingProxy)
                {                
                    ObjRef objRef = RemotingServices.MarshalInternal(mbo, null, null);
                    if (objRef.CanSmuggle())
                    {
                        if (!RemotingServices.IsTransparentProxy(mbo))
                        {
                            ServerIdentity srvId = (ServerIdentity)MarshalByRefObject.GetIdentity(mbo);
                            srvId.SetHandle();
                            objRef.SetServerIdentity(srvId.GetHandle());
                            objRef.SetDomainID(AppDomain.CurrentDomain.GetId());
                        }
                        ObjRef smugObjRef = objRef.CreateSmuggleableCopy();
                        smugObjRef.SetMarshaledObject();
                        return new SmuggledObjRef(smugObjRef);
                    }
                }

                // Add this arg to list of one's to serialize and return a placeholder
                //   since we couldn't smuggle the objref.
                if (argsToSerialize == null)
                    argsToSerialize = new ArrayList();
                index = argsToSerialize.Count;
                argsToSerialize.Add(arg);
                return new SerializedArg(index);
            }

            if (CanSmuggleObjectDirectly(arg))
                return arg;

            // if this is a primitive array, we can just make a copy.
            //   (IMPORTANT: We can directly use this copy from the
            //    other app domain, there is no reason to make another
            //    copy once we are on the other side)
            Array array = arg as Array;
            if (array != null)
            {
                Type elementType = array.GetType().GetElementType();
                if (elementType.IsPrimitive || (elementType == typeof(String)))
                    return array.Clone();
            }


            // Add this arg to list of one's to serialize and return a placeholder.
            if (argsToSerialize == null)
                argsToSerialize = new ArrayList();
            index = argsToSerialize.Count;
            argsToSerialize.Add(arg);
            return new SerializedArg(index);
        } // FixupArg


        [System.Security.SecurityCritical]  // auto-generated
        protected static Object[] UndoFixupArgs(Object[] args, ArrayList deserializedArgs)
        {
            Object[] newArgs = new Object[args.Length];
            int total = args.Length;
            for (int co = 0; co < total; co++)
            {
                newArgs[co] = UndoFixupArg(args[co], deserializedArgs);
            }

            return newArgs;
        } // UndoFixupArgs


        [System.Security.SecurityCritical]  // auto-generated
        protected static Object UndoFixupArg(Object arg, ArrayList deserializedArgs)
        {
            SmuggledObjRef smuggledObjRef = arg as SmuggledObjRef;
            if (smuggledObjRef != null)
            {
                // We call GetRealObject here ... that covers any
                // special unmarshaling we need to do for _ComObject
                return smuggledObjRef.ObjRef.GetRealObjectHelper();
            }

            SerializedArg serializedArg = arg as SerializedArg;
            if (serializedArg != null)
            {
                return deserializedArgs[serializedArg.Index];
            }
            
            return arg;
        } // UndoFixupArg


        // returns number of entries added to argsToSerialize
        [System.Security.SecurityCritical]  // auto-generated
        protected static int StoreUserPropertiesForMethodMessage(
            IMethodMessage msg, 
            ref ArrayList argsToSerialize)
        {
            IDictionary properties = msg.Properties;
            MessageDictionary dict = properties as MessageDictionary;
            if (dict != null)
            {
                if (dict.HasUserData())
                {
                    int co = 0;
                    foreach (DictionaryEntry entry in dict.InternalDictionary)
                    {
                        if (argsToSerialize == null)
                            argsToSerialize = new ArrayList();
                        argsToSerialize.Add(entry);
                        co++;
                    }

                    return co;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                // <

                int co = 0;
                foreach (DictionaryEntry entry in properties)
                {
                    if (argsToSerialize == null)
                        argsToSerialize = new ArrayList();
                    argsToSerialize.Add(entry);
                    co++;
                }

                return co;
            }
        } // StoreUserPropertiesForMethodMessage


        //
        // Helper classes used to smuggle transformed arguments
        //

        protected class SerializedArg
        {
            private int _index;

            public SerializedArg(int index)
            {
                _index = index;
            }

            public int Index { get { return _index; } }
        }

        //
        // end of Helper classes used to smuggle transformed arguments
        //                
    
    } // class MessageSmuggler



    // stores an object reference
    internal class SmuggledObjRef
    {
        [System.Security.SecurityCritical] // auto-generated
        ObjRef _objRef;

        [System.Security.SecurityCritical]  // auto-generated
        public SmuggledObjRef(ObjRef objRef)
        {
            _objRef = objRef;
        }            

        public ObjRef ObjRef
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return _objRef; }
        }
    } // SmuggledObjRef

    



    internal class SmuggledMethodCallMessage : MessageSmuggler
    {
        private String   _uri;
        private String   _methodName;
        private String   _typeName;
        private Object[] _args;

        private byte[] _serializedArgs = null;
#if false // This field isn't currently used
        private Object[] _serializerSmuggledArgs = null;
#endif

        // other things that might need to go through serializer
        private SerializedArg _methodSignature = null;
        private SerializedArg _instantiation = null;
        private Object        _callContext = null; // either a call id string or a SerializedArg pointing to CallContext object

        private int _propertyCount = 0; // <n> = # of user properties in dictionary
                                        //   note: first <n> entries in _deserializedArgs will be the property entries
               

        // always use this helper method to create
        [System.Security.SecurityCritical]  // auto-generated
        internal static SmuggledMethodCallMessage SmuggleIfPossible(IMessage msg)
        {        
            IMethodCallMessage mcm = msg as IMethodCallMessage;
            if (mcm == null)
                return null;        

            return new SmuggledMethodCallMessage(mcm);
        }       

        // hide default constructor
        private SmuggledMethodCallMessage(){}

        [System.Security.SecurityCritical]  // auto-generated
        private SmuggledMethodCallMessage(IMethodCallMessage mcm)
        {
            _uri = mcm.Uri;
            _methodName = mcm.MethodName;
            _typeName = mcm.TypeName;

            ArrayList argsToSerialize = null; 
            
            IInternalMessage iim = mcm as IInternalMessage;

            // user properties (everything but special entries)
            if ((iim == null) || iim.HasProperties())
                _propertyCount = StoreUserPropertiesForMethodMessage(mcm, ref argsToSerialize);

            // generic instantiation information
            if (mcm.MethodBase.IsGenericMethod)
            {
                Type[] inst = mcm.MethodBase.GetGenericArguments();
                if (inst != null && inst.Length > 0)
                {
                    if (argsToSerialize == null)
                        argsToSerialize = new ArrayList();
                    _instantiation = new SerializedArg(argsToSerialize.Count);
                    argsToSerialize.Add(inst);
                }
            }

            // handle method signature
            if (RemotingServices.IsMethodOverloaded(mcm))
            {
                if (argsToSerialize == null)
                    argsToSerialize = new ArrayList();
                _methodSignature = new SerializedArg(argsToSerialize.Count);
                argsToSerialize.Add(mcm.MethodSignature);
            }

            // handle call context
            LogicalCallContext lcc = mcm.LogicalCallContext;
            if (lcc == null)
            {
                _callContext = null;
            }
            else
            if (lcc.HasInfo)
            {
                if (argsToSerialize == null)
                    argsToSerialize = new ArrayList();
                _callContext = new SerializedArg(argsToSerialize.Count);
                argsToSerialize.Add(lcc);
            }
            else
            {
                // just smuggle the call id string
                _callContext = lcc.RemotingData.LogicalCallID;
            }
            
            _args = FixupArgs(mcm.Args, ref argsToSerialize);

            if (argsToSerialize != null)
            {
                //MemoryStream argStm = CrossAppDomainSerializer.SerializeMessageParts(argsToSerialize, out _serializerSmuggledArgs);
                MemoryStream argStm = CrossAppDomainSerializer.SerializeMessageParts(argsToSerialize);
                _serializedArgs = argStm.GetBuffer();
            }      
           
        } // SmuggledMethodCallMessage


        // returns a list of the deserialized arguments
        [System.Security.SecurityCritical]  // auto-generated
        internal ArrayList FixupForNewAppDomain()
        {   
            ArrayList deserializedArgs = null;
        
            if (_serializedArgs != null)
            {
                deserializedArgs =
                    CrossAppDomainSerializer.DeserializeMessageParts(
                        new MemoryStream(_serializedArgs));
                //deserializedArgs =
                //    CrossAppDomainSerializer.DeserializeMessageParts(
                //        new MemoryStream(_serializedArgs), _serializerSmuggledArgs);
                _serializedArgs = null;
            }                   

            return deserializedArgs;
        } // FixupForNewAppDomain

        
        internal String Uri { get { return _uri; } }
        internal String MethodName { get { return _methodName; } }
        internal String TypeName { get { return _typeName; } }

        internal Type[] GetInstantiation(ArrayList deserializedArgs)
        {
            if (_instantiation != null)                    
                return (Type[])deserializedArgs[_instantiation.Index]; 
            else
               return null;
        }

        internal Object[] GetMethodSignature(ArrayList deserializedArgs)
        {
            if (_methodSignature != null)                    
                return (Object[])deserializedArgs[_methodSignature.Index]; 
            else
               return null;
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        internal Object[] GetArgs(ArrayList deserializedArgs)
        {
            return UndoFixupArgs(_args, deserializedArgs);
        } // GetArgs 

        [System.Security.SecurityCritical]  // auto-generated
        internal LogicalCallContext GetCallContext(ArrayList deserializedArgs)
        {
            if (_callContext == null)
            {
                return null;
            }
            if (_callContext is String)
            {
                LogicalCallContext callContext = new LogicalCallContext();
                callContext.RemotingData.LogicalCallID = (String)_callContext;
                return callContext;
            }
            else
                return (LogicalCallContext)deserializedArgs[((SerializedArg)_callContext).Index];
        }

        internal int MessagePropertyCount
        {
            get { return _propertyCount; }
        }        

        internal void PopulateMessageProperties(IDictionary dict, ArrayList deserializedArgs)
        {
            for (int co = 0; co < _propertyCount; co++)
            {
                DictionaryEntry de = (DictionaryEntry)deserializedArgs[co];
                dict[de.Key] = de.Value;
            }
        }
        
    } // class SmuggledMethodCallMessage



    internal class SmuggledMethodReturnMessage : MessageSmuggler
    {
        private Object[]  _args;
        private Object    _returnValue;

        private byte[] _serializedArgs = null;
#if false // This field isn't currently used
        private Object[] _serializerSmuggledArgs = null;
#endif

        // other things that might need to go through serializer
        private SerializedArg _exception = null;
        private Object        _callContext = null; // either a call id string or a SerializedArg pointing to CallContext object

        private int _propertyCount; // <n> = # of user properties in dictionary
                                    //   note: first <n> entries in _deserializedArgs will be the property entries


        // always use this helper method to create
        [System.Security.SecurityCritical]  // auto-generated
        internal static SmuggledMethodReturnMessage SmuggleIfPossible(IMessage msg)
        {        
            IMethodReturnMessage mrm = msg as IMethodReturnMessage;
            if (mrm == null)
                return null;

            return new SmuggledMethodReturnMessage(mrm);
        }       

        // hide default constructor
        private SmuggledMethodReturnMessage(){}

        [System.Security.SecurityCritical]  // auto-generated
        private SmuggledMethodReturnMessage(IMethodReturnMessage mrm)
        {           
            ArrayList argsToSerialize = null;
            
            ReturnMessage retMsg = mrm as ReturnMessage;

            // user properties (everything but special entries)
            if ((retMsg == null) || retMsg.HasProperties())
                _propertyCount = StoreUserPropertiesForMethodMessage(mrm, ref argsToSerialize);

            // handle exception
            Exception excep = mrm.Exception;
            if (excep != null)
            {
                if (argsToSerialize == null)
                    argsToSerialize = new ArrayList();
                _exception = new SerializedArg(argsToSerialize.Count);
                argsToSerialize.Add(excep);
            }

            // handle call context
            LogicalCallContext lcc = mrm.LogicalCallContext;
            if (lcc == null)
            {
                _callContext = null;
            }
            else
            if (lcc.HasInfo)
            {
                if (lcc.Principal != null)
                    lcc.Principal = null;
            
                if (argsToSerialize == null)
                    argsToSerialize = new ArrayList();
                _callContext = new SerializedArg(argsToSerialize.Count);
                argsToSerialize.Add(lcc);
            }
            else
            {
                // just smuggle the call id string
                _callContext = lcc.RemotingData.LogicalCallID;
            }
            
            _returnValue = FixupArg(mrm.ReturnValue, ref argsToSerialize);
            _args = FixupArgs(mrm.Args, ref argsToSerialize);

            if (argsToSerialize != null)
            {
                MemoryStream argStm = CrossAppDomainSerializer.SerializeMessageParts(argsToSerialize);
                //MemoryStream argStm = CrossAppDomainSerializer.SerializeMessageParts(argsToSerialize, out _serializerSmuggledArgs);
                _serializedArgs = argStm.GetBuffer();
            }          

        } // SmuggledMethodReturnMessage


        [System.Security.SecurityCritical]  // auto-generated
        internal ArrayList FixupForNewAppDomain()
        {                
            ArrayList deserializedArgs = null;
        
            if (_serializedArgs != null)
            {
                deserializedArgs =
                    CrossAppDomainSerializer.DeserializeMessageParts(
                        new MemoryStream(_serializedArgs));
                //deserializedArgs =
                //    CrossAppDomainSerializer.DeserializeMessageParts(
                //        new MemoryStream(_serializedArgs), _serializerSmuggledArgs);
                _serializedArgs = null;
            }       

            return deserializedArgs;
        } // FixupForNewAppDomain
                
        [System.Security.SecurityCritical]  // auto-generated
        internal Object GetReturnValue(ArrayList deserializedArgs)
        {
            return UndoFixupArg(_returnValue, deserializedArgs);
        } // GetReturnValue
         
        [System.Security.SecurityCritical]  // auto-generated
        internal Object[] GetArgs(ArrayList deserializedArgs)
        {
            Object[] obj = UndoFixupArgs(_args, deserializedArgs);
            return obj;
        } // GetArgs     

        internal Exception GetException(ArrayList deserializedArgs)
        {
            if (_exception != null)
                return (Exception)deserializedArgs[_exception.Index]; 
            else
                return null;
        } // Exception
        
        [System.Security.SecurityCritical]  // auto-generated
        internal LogicalCallContext GetCallContext(ArrayList deserializedArgs)
        {
            if (_callContext == null)
            {
                return null;
            }
            if (_callContext is String)
            {
                LogicalCallContext callContext = new LogicalCallContext();
                callContext.RemotingData.LogicalCallID = (String)_callContext;
                return callContext;
            }
            else
                return (LogicalCallContext)deserializedArgs[((SerializedArg)_callContext).Index];
        }

        internal int MessagePropertyCount
        {
            get { return _propertyCount; }
        }   
        
        internal void PopulateMessageProperties(IDictionary dict, ArrayList deserializedArgs)
        {
            for (int co = 0; co < _propertyCount; co++)
            {
                DictionaryEntry de = (DictionaryEntry)deserializedArgs[co];
                dict[de.Key] = de.Value;
            }
        }
        
    } // class SmuggledMethodReturnMessage


} // namespace System.Runtime.Remoting.Messaging

