// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    ObjRef.cs
**
**
** Purpose: Defines the marshaled object reference class and related 
**          classes
**
**
**
===========================================================*/
namespace System.Runtime.Remoting {

    using System;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;    
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.Security.Permissions;
    using Win32Native = Microsoft.Win32.Win32Native;
    using System.Runtime.ConstrainedExecution;
    using System.Globalization;
    

    
    //** Purpose: Interface for providing type information. Users can use this
    //**          interface to provide custom type information which is carried
    //**          along with the ObjRef.
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IRemotingTypeInfo
    {
        // Return the fully qualified type name 
        String TypeName
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }

        // Check whether the given type can be cast to the type this
        // interface represents
        [System.Security.SecurityCritical]  // auto-generated_required
        bool CanCastTo(Type fromType, Object o);    
    }

    //** Purpose: Interface for providing channel information. Users can use this
    //**          interface to provide custom channel information which is carried
    //**          along with the ObjRef.
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IChannelInfo
    {
        // Get/Set the channel data for each channel 
        Object[] ChannelData
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }
    }

    //** Purpose: Interface for providing envoy information. Users can use this
    //**          interface to provide custom envoy information which is carried
    //**          along with the ObjRef.
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IEnvoyInfo
    {
        // Get/Set the envoy sinks 
        IMessageSink EnvoySinks
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }
    }

    

    [Serializable]
    internal class TypeInfo : IRemotingTypeInfo
    {   
        private String              serverType;
        private String[]            serverHierarchy;
        private String[]            interfacesImplemented;

        // Return the fully qualified type name 
        public virtual String TypeName
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return serverType;}
            [System.Security.SecurityCritical]  // auto-generated
            set { serverType = value;}
        }

        // Check whether the given type can be cast to the type this
        // interface represents
        [System.Security.SecurityCritical]  // auto-generated
        public virtual bool CanCastTo(Type castType, Object o)
        {
            if (null != castType)
            {
                // check for System.Object and MBRO since those aren't included in the
                //   heirarchy
                if ((castType == typeof(MarshalByRefObject)) ||
                    (castType == typeof(System.Object)))
                {
                    return true;
                }
                else
                if (castType.IsInterface)
                { 
                    if (interfacesImplemented != null)
                        return CanCastTo(castType, InterfacesImplemented);
                    else
                        return false;
                }
                else
                if (castType.IsMarshalByRef)                
                {
                    if (CompareTypes(castType, serverType))
                        return true;
                
                    if ((serverHierarchy != null) && CanCastTo(castType, ServerHierarchy))
                        return true;
                }
            }

            return false;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static String GetQualifiedTypeName(RuntimeType type)
        {
            if (type == null)
                return null;
                
            return RemotingServices.GetDefaultQualifiedTypeName(type);
        }

        internal static bool ParseTypeAndAssembly(String typeAndAssembly, out String typeName, out String assemName)
        {
            if (typeAndAssembly == null)
            {
                typeName = null;
                assemName = null;
                return false;
            }
            
            int index = typeAndAssembly.IndexOf(',');
            if (index == -1)
            {
                typeName = typeAndAssembly;
                assemName = null;
                return true;
            }

            // type name is everything up to the first comma
            typeName = typeAndAssembly.Substring(0, index); 

            // assembly name is the rest
            assemName = typeAndAssembly.Substring(index + 1).Trim();
                        
            return true;
        } // ParseTypeAndAssembly
        

        [System.Security.SecurityCritical]  // auto-generated
        internal TypeInfo(RuntimeType typeOfObj)
        {
            ServerType = GetQualifiedTypeName(typeOfObj);        

            // Compute the length of the server hierarchy
            RuntimeType currType = (RuntimeType)typeOfObj.BaseType;
            // typeOfObj is the root of all classes, but not included in the hierarachy.
            Message.DebugOut("RemotingServices::TypeInfo: Determining length of server heirarchy\n");
            int hierarchyLen = 0;
            while ((currType != typeof(MarshalByRefObject)) && 
                   (currType != null))
            {
                currType = (RuntimeType)currType.BaseType;
                hierarchyLen++;
            }

            // Allocate an array big enough to store the hierarchy            
            Message.DebugOut("RemotingServices::TypeInfo: Determined length of server heirarchy\n");
            String[] serverHierarchy = null;
            if (hierarchyLen > 0)
            {
                serverHierarchy = new String[hierarchyLen]; 
            
                currType = (RuntimeType)typeOfObj.BaseType;
                for (int i = 0; i < hierarchyLen; i++)
                {
                    serverHierarchy[i] = GetQualifiedTypeName(currType);
                    currType = (RuntimeType)currType.BaseType;
                }
            }

            this.ServerHierarchy = serverHierarchy;

            Message.DebugOut("RemotingServices::TypeInfo: Getting implemented interfaces\n");
            // Set the interfaces implemented
            Type[] interfaces = typeOfObj.GetInterfaces();
            String[] interfaceNames = null;
            // If the requested type itself is an interface we should add that to the
            // interfaces list as well
            bool isInterface = typeOfObj.IsInterface;
            if (interfaces.Length > 0 || isInterface)
            {
                interfaceNames = new String[interfaces.Length + (isInterface ? 1 : 0)];
                for (int i = 0; i < interfaces.Length; i++)
                {
                    interfaceNames[i] = GetQualifiedTypeName((RuntimeType)interfaces[i]);
                }
                if (isInterface)
                    interfaceNames[interfaceNames.Length - 1] = GetQualifiedTypeName(typeOfObj); 
            }

            this.InterfacesImplemented = interfaceNames;
        } // TypeInfo

        internal String ServerType 
        {
            get { return serverType; }
            set { serverType = value; }
        }

        private String[] ServerHierarchy
        {
            get { return serverHierarchy;}
            set { serverHierarchy = value;}
        }

        private String[] InterfacesImplemented
        {
            get { return interfacesImplemented;}
            set { interfacesImplemented = value;}
        }

        [System.Security.SecurityCritical]  // auto-generated
        private bool CompareTypes(Type type1, String type2)
        {
            Type type = RemotingServices.InternalGetTypeFromQualifiedTypeName(type2);

            return type1 == type;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private bool CanCastTo(Type castType, String[] types)
        {
            bool fCastOK = false;

            // Run through the type names and see if there is a 
            // matching type
            
            if (null != castType)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (CompareTypes(castType,types[i]))
                    {
                        fCastOK = true;
                        break;
                    }
                }
            }

            Message.DebugOut("CanCastTo returning " + fCastOK + " for type " + castType.FullName + "\n");
            return fCastOK;
        }
    }

    [Serializable]
    internal class DynamicTypeInfo : TypeInfo
    {
        [System.Security.SecurityCritical]  // auto-generated
        internal DynamicTypeInfo(RuntimeType typeOfObj) : base(typeOfObj)
        {
        }
        [System.Security.SecurityCritical]  // auto-generated
        public override bool CanCastTo(Type castType, Object o)
        {
            // <

            return((MarshalByRefObject)o).IsInstanceOfType(castType);
        }
    }

    [Serializable]
    internal sealed class ChannelInfo : IChannelInfo
    {
        private Object[]     channelData;

        [System.Security.SecurityCritical]  // auto-generated
        internal ChannelInfo()
        {
            ChannelData = ChannelServices.CurrentChannelData;
        }

        public Object[] ChannelData
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return channelData; }
            [System.Security.SecurityCritical]  // auto-generated
            set { channelData = value; }
        }             

    }

    [Serializable]
    internal sealed class EnvoyInfo : IEnvoyInfo
    {
        private IMessageSink envoySinks;

        [System.Security.SecurityCritical]  // auto-generated
        internal static IEnvoyInfo CreateEnvoyInfo(ServerIdentity serverID)
        {
            IEnvoyInfo info = null;
            if (null != serverID)
            {
                // Set the envoy sink chain
                if (serverID.EnvoyChain == null)
                {
                    // <

                    serverID.RaceSetEnvoyChain(
                        serverID.ServerContext.CreateEnvoyChain(
                            serverID.TPOrObject));
                }

                // Create an envoy info object only if necessary
                IMessageSink sink = serverID.EnvoyChain as EnvoyTerminatorSink;
                if(null == sink)
                {
                    // The chain consists of more than a terminator sink
                    // Go ahead and create an envoy info structure, otherwise
                    // a null is returned and we recreate the terminator sink
                    // on the other side, automatically.
                    info = new EnvoyInfo(serverID.EnvoyChain);
                }
            }

            return info;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private EnvoyInfo(IMessageSink sinks)
        {
            BCLDebug.Assert(null != sinks, "null != sinks");
            EnvoySinks = sinks;
        }

        public IMessageSink EnvoySinks
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return envoySinks;}
            [System.Security.SecurityCritical]  // auto-generated
            set { envoySinks = value;}
        }
    }

    [System.Security.SecurityCritical]  // auto-generated_required
    [Serializable]
    [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]    
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ObjRef : IObjectReference, ISerializable
    {
        // This flag is used to distinguish between the case where
        // an actual object was marshaled as compared to the case
        // where someone wants to pass the ObjRef itself to a remote call
        internal const int FLG_MARSHALED_OBJECT  = 0x00000001;

        // This flag is used to mark a wellknown objRef (i.e. result
        // of marshaling a proxy that was obtained through a Connect call)
        internal const int FLG_WELLKNOWN_OBJREF  = 0x00000002;

        // This flag is used for a lightweight Object Reference. It is sent to those clients
        // which are not interested in receiving a full-fledged ObjRef. An example
        // of such a client will be a mobile device with hard memory and processing
        // constraints. 
        // NOTE: In this case ALL the fields EXCEPT the uri/flags field are NULL.
        internal const int FLG_LITE_OBJREF       = 0x00000004;

        internal const int FLG_PROXY_ATTRIBUTE   = 0x00000008;
        //
        //If you change the fields here, you must all change them in 
        //RemotingSurrogate::GetObjectData
        //
        internal String                 uri;
        internal IRemotingTypeInfo      typeInfo;
        internal IEnvoyInfo             envoyInfo;
        internal IChannelInfo           channelInfo;
        internal int                    objrefFlags;
        internal GCHandle               srvIdentity;
        internal int                    domainID;

        internal void SetServerIdentity(GCHandle hndSrvIdentity)
        {
            srvIdentity = hndSrvIdentity;
        }

        internal GCHandle GetServerIdentity()
        {
            return srvIdentity;
        }

        internal void SetDomainID(int id)
        {
            domainID = id;
        }

        internal int GetDomainID()
        {
            return domainID;
        }
        
        // Static fields
        private static Type orType = typeof(ObjRef);


        // shallow copy constructor used for smuggling.
        [System.Security.SecurityCritical]  // auto-generated
        private ObjRef(ObjRef o)
        {
            BCLDebug.Assert(o.GetType() == typeof(ObjRef), "this should be just an ObjRef");

            uri = o.uri;
            typeInfo = o.typeInfo;
            envoyInfo = o.envoyInfo;
            channelInfo = o.channelInfo;
            objrefFlags = o.objrefFlags;
            SetServerIdentity(o.GetServerIdentity());
            SetDomainID(o.GetDomainID());
        } // ObjRef

        [System.Security.SecurityCritical]  // auto-generated
        public ObjRef(MarshalByRefObject o, Type requestedType)
        {
            bool fServer;
            
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
            RuntimeType rt = requestedType as RuntimeType;
            if (requestedType != null && rt == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            Identity id = MarshalByRefObject.GetIdentity(o, out fServer);
            Init(o, id, rt);
        }

        [System.Security.SecurityCritical]  // auto-generated
        protected ObjRef(SerializationInfo info, StreamingContext context) 
        {
            String url = null; // an objref lite url
            bool bFoundFIsMarshalled = false;
        
            SerializationInfoEnumerator e = info.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Name.Equals("uri"))
                {
                    uri = (String) e.Value;
                }
                else if (e.Name.Equals("typeInfo"))
                {
                    typeInfo = (IRemotingTypeInfo) e.Value;
                }
                else if (e.Name.Equals("envoyInfo"))
                {
                    envoyInfo = (IEnvoyInfo) e.Value;
                }
                else if (e.Name.Equals("channelInfo"))
                {
                    channelInfo = (IChannelInfo) e.Value;
                }
                else if (e.Name.Equals("objrefFlags"))
                {
                    Object o = e.Value;
                    if(o.GetType() == typeof(String))
                    {
                        objrefFlags = ((IConvertible)o).ToInt32(null);
                    }
                    else
                    {
                        objrefFlags = (int)o;
                    }
                }
                else if (e.Name.Equals("fIsMarshalled"))
                {
                    int value;
                    Object o = e.Value;
                    if(o.GetType() == typeof(String))
                        value = ((IConvertible)o).ToInt32(null);
                    else
                        value = (int)o;

                    if (value == 0)
                        bFoundFIsMarshalled = true;                                        
                }            
                else if (e.Name.Equals("url"))
                {
                    url = (String)e.Value;
                }
                else if (e.Name.Equals("SrvIdentity"))
                {
                    SetServerIdentity((GCHandle)e.Value);
                }
                else if (e.Name.Equals("DomainId"))
                {
                    SetDomainID((int)e.Value);
                }
            }

            if (!bFoundFIsMarshalled)
            {
                // This ObjRef was not passed as a parameter, so we need to unmarshal it.
                objrefFlags |= FLG_MARSHALED_OBJECT; 
            }
            else
                objrefFlags &= ~FLG_MARSHALED_OBJECT;

            // If only url is present, then it is an ObjRefLite.
            if (url != null)
            {
                uri = url;
                objrefFlags |= FLG_LITE_OBJREF;
            }
            
        } // ObjRef .ctor
        

        [System.Security.SecurityCritical]  // auto-generated
        internal bool CanSmuggle()
        {
            // make sure this isn't a derived class or an ObjRefLite
            if ((this.GetType() != typeof(ObjRef)) || IsObjRefLite())
                return false;
            
            Type typeOfTypeInfo = null;
            if (typeInfo != null)
                typeOfTypeInfo = typeInfo.GetType();

            Type typeOfChannelInfo = null;
            if (channelInfo != null)
                typeOfChannelInfo = channelInfo.GetType();
            
            if (((typeOfTypeInfo == null) ||
                 (typeOfTypeInfo == typeof(TypeInfo)) ||
                 (typeOfTypeInfo == typeof(DynamicTypeInfo))) &&
                (envoyInfo == null) &&
                ((typeOfChannelInfo == null) ||
                 (typeOfChannelInfo == typeof(ChannelInfo))))
            {
                if (channelInfo != null)
                {
                    foreach (Object channelData in channelInfo.ChannelData)
                    {
                        // Only consider CrossAppDomainData smuggleable.
                        if (!(channelData is CrossAppDomainData))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        } // CanSmuggle

        [System.Security.SecurityCritical]  // auto-generated
        internal ObjRef CreateSmuggleableCopy()
        {
            BCLDebug.Assert(CanSmuggle(), "Caller should have made sure that CanSmuggle() was true first.");

            return new ObjRef(this);
        } // CreateSmuggleableCopy
        

        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info==null) {
                throw new ArgumentNullException("info");
            }

            info.SetType(orType);

            if(!IsObjRefLite())
            {
                info.AddValue("uri", uri, typeof(String));
                info.AddValue("objrefFlags", (int) objrefFlags);
                info.AddValue("typeInfo", typeInfo, typeof(IRemotingTypeInfo));
                info.AddValue("envoyInfo", envoyInfo, typeof(IEnvoyInfo));
                info.AddValue("channelInfo", GetChannelInfoHelper(), typeof(IChannelInfo));
            }
            else
            {
                info.AddValue("url", uri, typeof(String));
            }
        } // GetObjectDataHelper


        // This method retrieves the channel info object to be serialized.
        // It does special checking to see if a channel url needs to be bashed
        // (currently used for switching "http://..." url to "https://...".
        [System.Security.SecurityCritical]  // auto-generated
        private IChannelInfo GetChannelInfoHelper()
        {
            ChannelInfo oldChannelInfo = channelInfo as ChannelInfo;
            if (oldChannelInfo == null)
                return channelInfo;

            Object[] oldChannelData = oldChannelInfo.ChannelData;
            if (oldChannelData == null)
                return oldChannelInfo;

            // <


            String[] bashInfo = (String[])CallContext.GetData("__bashChannelUrl");
            if (bashInfo == null)
                return oldChannelInfo;

            String urlToBash = bashInfo[0];
            String replacementUrl = bashInfo[1];

            // Copy channel info and go Microsoft urls.
            ChannelInfo newChInfo = new ChannelInfo();
            newChInfo.ChannelData = new Object[oldChannelData.Length];
            for (int co = 0; co < oldChannelData.Length; co++)
            {
                newChInfo.ChannelData[co] = oldChannelData[co];

                // see if this is one of the ones that we need to Microsoft
                ChannelDataStore channelDataStore = newChInfo.ChannelData[co] as ChannelDataStore;
                if (channelDataStore != null)
                {
                    String[] urls = channelDataStore.ChannelUris;
                    if ((urls != null) && (urls.Length == 1) && urls[0].Equals(urlToBash))
                    {
                        // We want to Microsoft just the url, so we do a shallow copy
                        // and replace the url array with the replacementUrl.
                        ChannelDataStore newChannelDataStore = channelDataStore.InternalShallowCopy();
                        newChannelDataStore.ChannelUris = new String[1];
                        newChannelDataStore.ChannelUris[0] = replacementUrl;

                        newChInfo.ChannelData[co] = newChannelDataStore;
                    }
                }
            }                      

            return newChInfo;
        } // GetChannelInfoHelper
            
        
        
        // Note: The uri will be either objURI (for normal marshals) or
        // it will be the URL if a wellknown object's proxy is marshaled
        // Basically we will take whatever the URI getter on Identity gives us

        public virtual String URI 
        {
            get { return uri;}
            set { uri = value;}
        }

        public virtual IRemotingTypeInfo TypeInfo 
        {
            get { return typeInfo;}
            set { typeInfo = value;}
        }

        public virtual IEnvoyInfo EnvoyInfo
        {
            get { return envoyInfo;}
            set { envoyInfo = value;}
        }

        public virtual IChannelInfo ChannelInfo 
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]          
            get { return channelInfo;}
            set { channelInfo = value;}
        }


        // This is called when doing fix-ups during deserialization
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual Object GetRealObject(StreamingContext context)
        {
            return GetRealObjectHelper();
        }
        // This is the common helper called by serialization / smuggling 
        [System.Security.SecurityCritical]  // auto-generated
        internal Object GetRealObjectHelper()

        {
            // Check if we are a result of serialiazing an MBR object
            // or if someone wanted to pass an ObjRef itself 
            if (!IsMarshaledObject())
            {
                BCLDebug.Trace("REMOTE", "ObjRef.GetRealObject: Returning *this*\n");                
                return this;
            }
            else
            {
                // Check if this is a lightweight objref
                if(IsObjRefLite())
                {
                    BCLDebug.Assert(null != uri, "null != uri");
                    
                    // transform the url, if this is a local object (we know it is local
                    //   if we find the current application id in the url)
                    int index = uri.IndexOf(RemotingConfiguration.ApplicationId);

                    // we need to be past 0, since we have to back up a space and pick up
                    //   a slash.
                    if (index > 0)
                        uri = uri.Substring(index - 1);                   
                }
            
                // In the general case, 'this' is the 
                // objref of an activated object

                // It may also be a well known object ref ... which came by
                // because someone did a Connect(URL) and then passed the proxy
                // over to a remote method call.

                // The below call handles both cases.
                bool fRefine = !(GetType() == typeof(ObjRef));
                Object ret = RemotingServices.Unmarshal(this, fRefine);

                // Check for COMObject & do some special custom marshaling
                ret = GetCustomMarshaledCOMObject(ret);

                return ret;
            }

        }

        [System.Security.SecurityCritical]  // auto-generated
        private Object GetCustomMarshaledCOMObject(Object ret)
        {
            // Some special work we need to do for __COMObject 
            // (Note that we use typeInfo to detect this case instead of
            // calling GetType on 'ret' so as to not refine the proxy)
            DynamicTypeInfo dt = this.TypeInfo as DynamicTypeInfo;
            if (dt != null)
            {
                // This is a COMObject type ... we do the special work 
                // only if it is from the same process but another appDomain
                // We rely on the x-appDomain channel data in the objRef
                // to provide us with the answers.
                Object ret1 = null;
                IntPtr pUnk = IntPtr.Zero;
                if (IsFromThisProcess() && !IsFromThisAppDomain())
                {
                    try
                    {
                        bool fIsURTAggregated;
                        pUnk = ((__ComObject)ret).GetIUnknown(out fIsURTAggregated);
                        if (pUnk != IntPtr.Zero && !fIsURTAggregated)
                        {
                            // The RCW for an IUnk is per-domain. This call
                            // gets (or creates) the RCW for this pUnk for
                            // the current domain.
                            String srvTypeName = TypeInfo.TypeName;
                            String typeName = null;
                            String assemName = null;

                            System.Runtime.Remoting.TypeInfo.ParseTypeAndAssembly(srvTypeName, out typeName, out assemName);
                            BCLDebug.Assert((null != typeName) && (null != assemName), "non-null values expected");

                            Assembly asm = FormatterServices.LoadAssemblyFromStringNoThrow(assemName);
                            if (asm==null) {
                                BCLDebug.Trace("REMOTE", "ObjRef.GetCustomMarshaledCOMObject. AssemblyName is: ", assemName, " but we can't load it.");
                                throw new RemotingException(Environment.GetResourceString("Serialization_AssemblyNotFound", assemName));
                            }

                            Type serverType = asm.GetType(typeName, false, false);
                            if (serverType != null && !serverType.IsVisible) 
                                serverType = null;
                            BCLDebug.Assert(serverType!=null, "bad objRef!");

                            ret1 = InteropServices.Marshal.GetTypedObjectForIUnknown(pUnk, serverType);
                            if (ret1 != null)
                            {
                                ret = ret1;
                            }    
                        }                                
                    }
                    finally
                    {
                        if (pUnk != IntPtr.Zero)
                        {
                            InteropServices.Marshal.Release(pUnk);
                        }                            
                    }                        
                }
            }
            return ret;
        }

        public ObjRef()
        {
            objrefFlags = 0x0;
        }

        internal bool IsMarshaledObject()
        {
            return (objrefFlags & FLG_MARSHALED_OBJECT) == FLG_MARSHALED_OBJECT;
        }

        internal void SetMarshaledObject()
        {
            objrefFlags |= FLG_MARSHALED_OBJECT;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]  
        internal bool IsWellKnown()
        {
            return (objrefFlags & FLG_WELLKNOWN_OBJREF) == FLG_WELLKNOWN_OBJREF;
        }

        internal void SetWellKnown()
        {
            objrefFlags |= FLG_WELLKNOWN_OBJREF;
        }

        internal bool HasProxyAttribute()
        {
            return (objrefFlags & FLG_PROXY_ATTRIBUTE) == FLG_PROXY_ATTRIBUTE;
        }

        internal void SetHasProxyAttribute()
        {
            objrefFlags |= FLG_PROXY_ATTRIBUTE;
        }

        internal bool IsObjRefLite()
        {
            return (objrefFlags & FLG_LITE_OBJREF) == FLG_LITE_OBJREF;
        }

        internal void SetObjRefLite()
        {
            objrefFlags |= FLG_LITE_OBJREF;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]  
        private CrossAppDomainData GetAppDomainChannelData()
        {            
            BCLDebug.Assert(
                ObjRef.IsWellFormed(this), 
                "ObjRef.IsWellFormed()");
                
            // Look at the ChannelData part to find CrossAppDomainData
            int i=0;
            CrossAppDomainData xadData = null;
            while (i<ChannelInfo.ChannelData.Length)
            {
                xadData = ChannelInfo.ChannelData[i] as CrossAppDomainData;
                if (null != xadData)
                {
                    return xadData;
                }
                i++;
            }

            // AdData could be null for user-created objRefs.
            return null;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]  
        public bool IsFromThisProcess()
        {
            //Wellknown objects may or may not be in the same process
            //Hence return false;
            if (IsWellKnown())
                return false;

            CrossAppDomainData xadData = GetAppDomainChannelData();
            if (xadData != null)
            {
                return xadData.IsFromThisProcess();
            }
            return false;
        }

        [System.Security.SecurityCritical]  // auto-generated
        public bool IsFromThisAppDomain()
        {
            CrossAppDomainData xadData = GetAppDomainChannelData();
            if (xadData != null)
            {
                return xadData.IsFromThisAppDomain();
            }
            return false;
        }

        // returns the internal context ID for the server context if
        // it is from the same process && the appDomain of the server
        // is still valid. If the objRef is from this process, the domain
        // id found in the objref is always returned.

        [System.Security.SecurityCritical]  // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]  
        internal Int32 GetServerDomainId()
        {
            if (!IsFromThisProcess())
                return 0;
            CrossAppDomainData xadData = GetAppDomainChannelData();
            BCLDebug.Assert(xadData != null, "bad objRef?");
            return xadData.DomainID;

        }
        
        [System.Security.SecurityCritical]  // auto-generated
        internal IntPtr GetServerContext(out int domainId)
        {
            IntPtr contextId = IntPtr.Zero;
            domainId = 0;
            if (IsFromThisProcess())
            {
                CrossAppDomainData xadData = GetAppDomainChannelData();
                BCLDebug.Assert(xadData != null, "bad objRef?");
                domainId = xadData.DomainID;
                if (AppDomain.IsDomainIdValid(xadData.DomainID))
                {
                    contextId = xadData.ContextID;
                }
            }
            return contextId;
        }

       //
       //
        [System.Security.SecurityCritical]  // auto-generated
        internal void Init(Object o, Identity idObj, RuntimeType requestedType)
        {        
            Message.DebugOut("RemotingServices::FillObjRef: IN");
            BCLDebug.Assert(idObj != null,"idObj != null");

            // Set the URI of the object to be marshaled            
            uri = idObj.URI;

            // Figure out the type 
            MarshalByRefObject obj = idObj.TPOrObject;
            BCLDebug.Assert(null != obj, "Identity not setup correctly");

            // Get the type of the object
            RuntimeType serverType = null;
            if(!RemotingServices.IsTransparentProxy(obj))
            {
                serverType = (RuntimeType)obj.GetType();
            }
            else
            {
                serverType = (RuntimeType)RemotingServices.GetRealProxy(obj).GetProxiedType();
            }

            RuntimeType typeOfObj = (null == requestedType ? serverType : requestedType);

            // Make sure that the server and requested types are compatible
            //  (except for objects that implement IMessageSink, since we 
            //   just hand off the message instead of invoking the proxy)
            if ((null != requestedType) &&
                !requestedType.IsAssignableFrom(serverType) &&
                (!typeof(IMessageSink).IsAssignableFrom(serverType)))
            {
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString(
                            "Remoting_InvalidRequestedType"), 
                        requestedType.ToString())); ;
            }

            // Create the type info
            if(serverType.IsCOMObject)
            {
                // __ComObjects need dynamic TypeInfo
                DynamicTypeInfo dt = new DynamicTypeInfo(typeOfObj);
                TypeInfo = (IRemotingTypeInfo) dt;
            }
            else
            {
                RemotingTypeCachedData cache = (RemotingTypeCachedData)
                    InternalRemotingServices.GetReflectionCachedData(typeOfObj);
            
                TypeInfo = (IRemotingTypeInfo)cache.TypeInfo;
            }
    
            if (!idObj.IsWellKnown())
            {
                // Create the envoy info
                EnvoyInfo = System.Runtime.Remoting.EnvoyInfo.CreateEnvoyInfo(idObj as ServerIdentity);

                // Create the channel info 
                IChannelInfo chan = (IChannelInfo)new ChannelInfo();
                // Make sure the channelInfo only has x-appdomain data since the objref is agile while other
                // channelData might not be and regardless this data is useless for an appdomain proxy
                if (o is AppDomain){
                    Object[] channelData = chan.ChannelData;
                    int channelDataLength = channelData.Length;
                    Object[] newChannelData = new Object[channelDataLength];
                    // Clone the data so that we dont Microsoft the current appdomain data which is stored
                    // as a static
                    Array.Copy(channelData, newChannelData, channelDataLength);
                    for (int i = 0; i < channelDataLength; i++)
                    {
                        if (!(newChannelData[i] is CrossAppDomainData))
                            newChannelData[i] = null;
                    }
                    chan.ChannelData = newChannelData;
                }
                ChannelInfo = chan;

                if (serverType.HasProxyAttribute)
                {
                    SetHasProxyAttribute();
                }
            }
            else
            {
                SetWellKnown();
            }

            // See if we should and can use a url obj ref?
            if (ShouldUseUrlObjRef())
            {
                if (IsWellKnown())
                {
                    // full uri already supplied.
                    SetObjRefLite();
                }
                else
                {
                    String httpUri = ChannelServices.FindFirstHttpUrlForObject(URI);
                    if (httpUri != null)
                    {
                        URI = httpUri;
                        SetObjRefLite();
                    }
                }
            }
        } // Init


        // determines if a particular type should use a url obj ref
        internal static bool ShouldUseUrlObjRef()
        {
            return RemotingConfigHandler.UrlObjRefMode;
        } // ShouldUseUrlObjRef
        

        // Check whether the objref is well formed
        [System.Security.SecurityCritical]  // auto-generated
        internal static bool IsWellFormed(ObjRef objectRef)
        {
            // We skip the wellformed check for wellKnown, 
            // objref-lite and custom objrefs
            bool wellFormed = true;
            if ((null == objectRef) ||
                (null == objectRef.URI) ||
                (!(objectRef.IsWellKnown()  || objectRef.IsObjRefLite() ||
                   objectRef.GetType() != orType)
                    && (null == objectRef.ChannelInfo)))
            {
                wellFormed = false;
            }
            
            return wellFormed;
        }
    } // ObjRef 
   
    
}
