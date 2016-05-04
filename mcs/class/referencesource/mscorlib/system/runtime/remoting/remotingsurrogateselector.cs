// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System.Runtime.Remoting.Messaging {

    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Text;
    using System.Reflection;
    using System.Threading;
    using System.Globalization;
    using System.Collections;
    using System.Security;
    using System.Security.Permissions;    
    using System.Diagnostics.Contracts;
[System.Runtime.InteropServices.ComVisible(true)]
    public delegate bool MessageSurrogateFilter(String key, Object value);

    [System.Security.SecurityCritical]  // auto-generated_required
    [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]    
    [System.Runtime.InteropServices.ComVisible(true)]
    public class RemotingSurrogateSelector : ISurrogateSelector
    {
        // Private static data
        private static Type s_IMethodCallMessageType = typeof(IMethodCallMessage);
        private static Type s_IMethodReturnMessageType = typeof(IMethodReturnMessage);
        private static Type s_ObjRefType = typeof(ObjRef);

        // Private member data
        private Object _rootObj = null;    
        private ISurrogateSelector _next = null;
        private RemotingSurrogate  _remotingSurrogate = new RemotingSurrogate();
        private ObjRefSurrogate _objRefSurrogate = new ObjRefSurrogate();
        private ISerializationSurrogate _messageSurrogate  = null;
        private MessageSurrogateFilter _filter = null;

        public RemotingSurrogateSelector()
        {
            _messageSurrogate = new MessageSurrogate(this);
        }

        public MessageSurrogateFilter Filter
        {
            set { _filter = value; }
            get { return _filter; }
        }

        public void SetRootObject(Object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Contract.EndContractBlock();
            _rootObj = obj;
            SoapMessageSurrogate soapMsg = _messageSurrogate as SoapMessageSurrogate;
            if (null != soapMsg)
            {
                soapMsg.SetRootObject(_rootObj);
            }
        }
        
        public Object GetRootObject()
        {
            return _rootObj;
        }
    
        // Specifies the next ISurrogateSelector to be examined for surrogates if the current
        // instance doesn't have a surrogate for the given type and assembly in the given context.
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void ChainSelector(ISurrogateSelector selector) {_next = selector;}
    
        // Returns the appropriate surrogate for the given type in the given context.
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector ssout)
        {        
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            Contract.EndContractBlock();
    
            Message.DebugOut("Entered  GetSurrogate for " + type.FullName + "\n");
    
            if (type.IsMarshalByRef)
            {
                Message.DebugOut("Selected surrogate for " + type.FullName);
                ssout = this;
                return _remotingSurrogate;
            }
            else if (s_IMethodCallMessageType.IsAssignableFrom(type) ||
                     s_IMethodReturnMessageType.IsAssignableFrom(type))
            {
                ssout = this;
                return _messageSurrogate;
            }
            else if (s_ObjRefType.IsAssignableFrom(type))
            {
                ssout = this;
                return _objRefSurrogate;
            }
            else if (_next != null)
            {
                return _next.GetSurrogate(type, context, out ssout);
            }
            else 
            {
                ssout = null;
                return null;
            }
          
        } // GetSurrogate
    
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual ISurrogateSelector GetNextSelector() { return _next;}
    
        public virtual void UseSoapFormat()
        {
            _messageSurrogate = new SoapMessageSurrogate(this);
            ((SoapMessageSurrogate)_messageSurrogate).SetRootObject(_rootObj);
        }
    }
    
    internal class RemotingSurrogate : ISerializationSurrogate
    {
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
        {               
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            
            if (info==null) {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();

            //
            // This code is to special case marshalling types inheriting from RemotingClientProxy
            // Check whether type inherits from RemotingClientProxy and serialize the correct ObjRef
            // after getting the correct proxy to the actual server object
            //
                
            Message.DebugOut("RemotingSurrogate::GetObjectData obj.Type: " + obj.GetType().FullName + " \n");
            if(RemotingServices.IsTransparentProxy(obj))
            {
                RealProxy rp = RemotingServices.GetRealProxy(obj);
                rp.GetObjectData(info, context);
            }
            else
            {
                    RemotingServices.GetObjectData(obj, info, context);
            }
        }


        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual Object  SetObjectData(Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        { 
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
        }

    } // class RemotingSurrogate


    internal class ObjRefSurrogate : ISerializationSurrogate
    {    
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
        {               
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            
            if (info==null) {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();

            //
            // This code is to provide special handling for ObjRef's that are supposed
            //   to be passed as parameters.
            //
                
            ((ObjRef)obj).GetObjectData(info, context);
            // add flag indicating the ObjRef was passed as a parameter
            info.AddValue("fIsMarshalled", 0);            
        } // GetObjectData

        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual Object  SetObjectData(Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        { 
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
        }

    } // class ObjRefSurrogate
    
    
    internal class SoapMessageSurrogate : ISerializationSurrogate
    {
        // Private static data
        private static Type _voidType = typeof(void);
        private static Type _soapFaultType = typeof(SoapFault);

        // Member data
        String DefaultFakeRecordAssemblyName = "http://schemas.microsoft.com/urt/SystemRemotingSoapTopRecord";
        Object _rootObj = null;
        [System.Security.SecurityCritical] // auto-generated
        RemotingSurrogateSelector _ss;

        [System.Security.SecurityCritical]  // auto-generated
        internal SoapMessageSurrogate(RemotingSurrogateSelector ss)
        {
            _ss = ss;
        }

        internal void SetRootObject(Object obj)
        {
            _rootObj = obj;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal virtual String[] GetInArgNames(IMethodCallMessage m, int c)
        {
            String[] names = new String[c];
            for (int i = 0; i < c; i++)
            {
                String name = m.GetInArgName(i);
                if (name == null)
                {
                    name = "__param" + i;
                }
                names[i] = name;
            }
            return names;
        }
       
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual String[] GetNames(IMethodCallMessage m, int c)
        {
            String[] names = new String[c];
            for (int i = 0; i < c; i++)
            {
                String name = m.GetArgName(i);
                if (name == null)
                {
                    name = "__param" + i;
                }
                names[i] = name;
            }
            return names;
        }
    
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
        {
            if (info==null) 
            {
               throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();
                        
            if ( (obj!=null) && (obj !=_rootObj))
            {
                (new MessageSurrogate(_ss)).GetObjectData(obj, info, context);
            }
            else 
            {
                IMethodReturnMessage msg = obj as IMethodReturnMessage;
                if(null != msg)
                {

                    if (msg.Exception == null)
                    {
                        String responseElementName;
                        String responseElementNS;
                        String returnElementName;                    

                        // obtain response element name namespace
                        MethodBase mb = msg.MethodBase;
                        SoapMethodAttribute attr = (SoapMethodAttribute)InternalRemotingServices.GetCachedSoapAttribute(mb);
                        responseElementName = attr.ResponseXmlElementName;
                        responseElementNS = attr.ResponseXmlNamespace;
                        returnElementName = attr.ReturnXmlElementName;

                        ArgMapper mapper = new ArgMapper(msg, true /*fOut*/);
                        Object[] args = mapper.Args;
                        info.FullTypeName = responseElementName;
                        info.AssemblyName = responseElementNS;
                        Type retType = ((MethodInfo)mb).ReturnType;                                         
                        if (!((retType == null) || (retType == _voidType)))
                        {
                            info.AddValue(returnElementName, msg.ReturnValue, retType);
                        }
                        if (args != null)
                        {
                            Type[] types = mapper.ArgTypes;
                            for (int i=0; i<args.Length; i++)
                            {
                                String name;
                                name = mapper.GetArgName(i);
                                if ((name == null) || (name.Length == 0))
                                {
                                    name = "__param" + i;
                                }
                                info.AddValue(
                                        name, 
                                        args[i], 
                                        types[i].IsByRef?
                                            types[i].GetElementType():types[i]);
                            }
                        }
                    }
                    else
                    {
                        Object oClientIsClr = CallContext.GetData("__ClientIsClr");
                        bool bClientIsClr = (oClientIsClr == null) ? true:(bool)oClientIsClr;
                        info.FullTypeName = "FormatterWrapper";
                        info.AssemblyName = DefaultFakeRecordAssemblyName;

                        Exception ex = msg.Exception;
                        StringBuilder sb = new StringBuilder();
                        bool bMustUnderstandError = false;
                        while(ex != null)
                        {
                            if (ex.Message.StartsWith("MustUnderstand", StringComparison.Ordinal))
                                bMustUnderstandError = true;

                            sb.Append(" **** ");
                            sb.Append(ex.GetType().FullName); 
                            sb.Append(" - ");
                            sb.Append(ex.Message);
                            
                            ex = ex.InnerException;
                        }

                        ServerFault serverFault = null;
                        if (bClientIsClr)
                            serverFault = new ServerFault(msg.Exception); // Clr is the Client use full exception
                        else
                            serverFault = new ServerFault(msg.Exception.GetType().AssemblyQualifiedName, sb.ToString(), msg.Exception.StackTrace); 

                        String faultType = "Server";
                        if (bMustUnderstandError)
                            faultType = "MustUnderstand";

                        SoapFault soapFault = new SoapFault(faultType, sb.ToString(), null, serverFault);
                        info.AddValue("__WrappedObject", soapFault, _soapFaultType);                   
                    }
                }
                else
                {

                    IMethodCallMessage mcm = (IMethodCallMessage)obj;

                    // obtain method namespace        
                    MethodBase mb = mcm.MethodBase;                
                    String methodElementNS = SoapServices.GetXmlNamespaceForMethodCall(mb);       

                    Object[] args = mcm.InArgs;
                    String[] names = GetInArgNames(mcm, args.Length);
                    Type[] sig = (Type[])mcm.MethodSignature;
                    info.FullTypeName = mcm.MethodName;
                    info.AssemblyName = methodElementNS;
                    RemotingMethodCachedData cache = (RemotingMethodCachedData)InternalRemotingServices.GetReflectionCachedData(mb);
                    int[] requestArgMap = cache.MarshalRequestArgMap;


                    Contract.Assert(
                        args!=null || sig.Length == args.Length, 
                        "Signature mismatch");

                    for (int i = 0; i < args.Length; i++)
                    {
                        String paramName = null;
                        if ((names[i] == null) || (names[i].Length == 0))
                            paramName = "__param" + i;
                        else
                            paramName = names[i];

                        int sigPosition = requestArgMap[i];
                        Type argType = null;

                        if (sig[sigPosition].IsByRef)
                            argType = sig[sigPosition].GetElementType();
                        else
                            argType = sig[sigPosition];

                        info.AddValue(paramName, args[i], argType);
                    }
                }
            }
        } // GetObjectData
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual Object  SetObjectData(Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        { 
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
        }

    }
    
    internal class MessageSurrogate : ISerializationSurrogate
    {
        // Private static data
        private static Type _constructionCallType = typeof(ConstructionCall);
        private static Type _methodCallType = typeof(MethodCall);
        private static Type _constructionResponseType = typeof(ConstructionResponse);
        private static Type _methodResponseType = typeof(MethodResponse);
        private static Type _exceptionType = typeof(Exception);
        private static Type _objectType = typeof(Object);

        // Private static member data
        [System.Security.SecurityCritical] // auto-generated
        private RemotingSurrogateSelector _ss;

        [SecuritySafeCritical]
        static MessageSurrogate()
        {
            // static initialization of MessageSurrogate touches critical types, so
            // the static constructor needs to be critical as well.
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal MessageSurrogate(RemotingSurrogateSelector ss)
        {
            _ss = ss;
        }
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void GetObjectData(Object obj, SerializationInfo info, StreamingContext context)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (info==null) {
                throw new ArgumentNullException("info");
            }
            Contract.EndContractBlock();

            bool returnMessage = false;
            bool constructionMessage = false;
            IMethodMessage msg = obj as IMethodMessage;
            if (null != msg)
            {
                IDictionaryEnumerator de = msg.Properties.GetEnumerator();
                if (msg is IMethodCallMessage) 
                {
                    if (obj is IConstructionCallMessage)
                        constructionMessage = true;
                    info.SetType(constructionMessage ? _constructionCallType : _methodCallType);
                }
                else 
                {
                    IMethodReturnMessage mrm = msg as IMethodReturnMessage;
                    if (null != mrm)
                    {
                        returnMessage = true;
                        info.SetType((obj is IConstructionReturnMessage) ? _constructionResponseType : _methodResponseType);
                        if (((IMethodReturnMessage)msg).Exception != null)
                        {
                            info.AddValue("__fault",((IMethodReturnMessage)msg).Exception, _exceptionType);
                        }
                            
                    }
                    else 
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_InvalidMsg"));                
                    }
                }
                    
                while (de.MoveNext()) {
                    if ((obj == _ss.GetRootObject()) && (_ss.Filter != null) && _ss.Filter((String)de.Key, de.Value))
                        continue;

                    if (de.Value!=null) {

                        String key = de.Key.ToString();
                        if (key.Equals("__CallContext"))
                        {
                            // If the CallContext has only the call Id, then there is no need to put the entire 
                            // LogicalCallContext type on the wire
                            LogicalCallContext lcc = (LogicalCallContext)de.Value;
                            if (lcc.HasInfo)
                                info.AddValue(key, lcc);
                            else
                                info.AddValue(key, lcc.RemotingData.LogicalCallID);                        
                        }
                        else if (key.Equals("__MethodSignature"))
                        {
                            // If the method is not overloaded, the method signature does not need to go on the wire
                            // note - IsMethodOverloaded does not work well with constructors
                            if (constructionMessage || RemotingServices.IsMethodOverloaded(msg))
                            {
                                info.AddValue(key, de.Value);
                                continue;
                            }
                            Message.DebugOut("MessageSurrogate::GetObjectData. Method not overloaded, so no MethodSignature \n");
                        }
                        else 
                        {
#if false
                            /* If the streaming context says this is a x-domain call, then there is no
                                need to include the following fields in the return message. Right now I am not sure 
                                how to identify a cross-domain streaming context - Ashok 
                            */
                            if (returnMessage && 
                                    (key.Equals("__Uri") ||
                                     key.Equals("__MethodName") ||
                                     key.Equals("__TypeName")))
                                     continue;                                     
                            else
#endif
#pragma warning disable 1717  // assignment to self
                                returnMessage = returnMessage;
#pragma warning restore 1717
                                info.AddValue(key, de.Value);
                        }

                    } else {
                        info.AddValue(de.Key.ToString(), de.Value, _objectType);
                    }
                }
            }
            else
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_InvalidMsg"));
            }
    
        }
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual Object  SetObjectData(Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        { 
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
        }
    }
}
