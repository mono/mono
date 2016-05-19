// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
#if FEATURE_REMOTING
namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting.Messaging;
    using System.Reflection;


    [Serializable]
    internal sealed class BinaryMethodCallMessage
    {
        Object[] _inargs = null;
        String _methodName = null;
        String _typeName = null;
        Object _methodSignature = null;

        Type[] _instArgs = null;
        Object[] _args = null;
        [System.Security.SecurityCritical] // auto-generated
        LogicalCallContext _logicalCallContext = null;

        Object[] _properties = null;

        [System.Security.SecurityCritical]  // auto-generated
        internal BinaryMethodCallMessage(String uri, String methodName, String typeName, Type[] instArgs, Object[] args, Object methodSignature, LogicalCallContext callContext, Object[] properties)
        {
            _methodName = methodName;
            _typeName = typeName;
            //_uri = uri;
            if (args == null)
                args = new Object[0];

            _inargs = args;
            _args = args;
            _instArgs = instArgs;
            _methodSignature = methodSignature;
            if (callContext == null)
                _logicalCallContext = new LogicalCallContext();
            else
                _logicalCallContext = callContext;

            _properties = properties;

        }

        public String MethodName
        {
            get {return _methodName;}
        }

        public String TypeName
        {
            get {return _typeName;}
        }


        public Type[] InstantiationArgs
        {
            get {return _instArgs;}
        }
        
        public Object MethodSignature
        {
            get {return _methodSignature;}
        }

        public Object[] Args
        {
            get {return _args;}
        }

        public LogicalCallContext LogicalCallContext
        {
            [System.Security.SecurityCritical]  // auto-generated
            get {return _logicalCallContext;}
        }

        public bool HasProperties
        {
            get {return (_properties != null);}
        }

        internal void PopulateMessageProperties(IDictionary dict)
        {
            foreach (DictionaryEntry de in _properties)
            {
                dict[de.Key] = de.Value;
            }
        }

    }


    [Serializable]
    internal class BinaryMethodReturnMessage
    {
        Object[] _outargs = null;
        Exception _exception = null;
        Object _returnValue = null;

        Object[] _args = null;
        [System.Security.SecurityCritical] // auto-generated
        LogicalCallContext _logicalCallContext = null;

        Object[] _properties = null;

        [System.Security.SecurityCritical]  // auto-generated
        internal BinaryMethodReturnMessage(Object returnValue, Object[] args, Exception e, LogicalCallContext callContext, Object[] properties)
        {
            _returnValue = returnValue;
            if (args == null)
                args = new Object[0];

            _outargs = args;
            _args= args;
            _exception = e;

            if (callContext == null)
                _logicalCallContext = new LogicalCallContext();
            else
                _logicalCallContext = callContext;
            
            _properties = properties;
        }

        public Exception Exception
        {
            get {return _exception;}
        }

        public Object  ReturnValue
        {
            get {return _returnValue;}
        }
        
        public Object[] Args
        {
            get {return _args;}
        }

        public LogicalCallContext LogicalCallContext
        {
            [System.Security.SecurityCritical]  // auto-generated
            get {return _logicalCallContext;}
        }

        public bool HasProperties
        {
            get {return (_properties != null);}
        }

        internal void PopulateMessageProperties(IDictionary dict)
        {
            foreach (DictionaryEntry de in _properties)
            {
                dict[de.Key] = de.Value;
            }
        }
    }
}
#endif //  FEATURE_REMOTING    

