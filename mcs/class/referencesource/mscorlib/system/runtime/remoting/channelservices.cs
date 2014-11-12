using System.Diagnostics.Contracts;
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//* File:    Channel.cs
//*
//* <EMAIL>Author:  Tarun Anand ([....])</EMAIL>
//*
//* Purpose: Defines the general purpose remoting proxy
//*
//* Date:    May 27, 1999
//*
namespace System.Runtime.Remoting.Channels {
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;  
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;   
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata; 
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
 
    // ChannelServices
    
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    internal struct Perf_Contexts {
        internal volatile int cRemoteCalls;
        internal volatile int cChannels;
    };
    
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class ChannelServices
    {
        // This gets refreshed when a channel is registered/unregistered.
        private static volatile Object[] s_currentChannelData = null;

        [System.Security.SecuritySafeCritical] // static constructors should be safe to call
        static ChannelServices()
        { 
        }

        internal static Object[] CurrentChannelData
        {
            [System.Security.SecurityCritical]  // auto-generated
            get 
            {
                if (s_currentChannelData == null)
                    RefreshChannelData();

                return s_currentChannelData; 
            }
        } // CurrentChannelData


        // hide the default constructor
        private ChannelServices()
        {
        }

        // list of registered channels and a lock to take when adding or removing channels
        // Note that the channel list is read outside of the lock, which is why it's marked volatile.
        private static Object s_channelLock = new Object();
        private static volatile RegisteredChannelList s_registeredChannels = new RegisteredChannelList();
        
    
        // Private member variables        
        // These have all been converted to getters and setters to get the effect of
        // per-AppDomain statics (note: statics are per-AppDomain now, so these members
        // could just be declared as statics on ChannelServices).

        private static long remoteCalls
        { 
            get { return Thread.GetDomain().RemotingData.ChannelServicesData.remoteCalls; }
            set { Thread.GetDomain().RemotingData.ChannelServicesData.remoteCalls = value; }
        }
        
        private static volatile IMessageSink xCtxChannel;
        

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]      
        [ResourceExposure(ResourceScope.None)]
        static unsafe extern Perf_Contexts* GetPrivateContextsPerfCounters();
    
        [SecurityCritical]
        unsafe private static volatile Perf_Contexts *perf_Contexts = GetPrivateContextsPerfCounters(); 
    

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static void RegisterChannel(IChannel chnl, bool ensureSecurity)
        {
            RegisterChannelInternal(chnl, ensureSecurity);
        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        [Obsolete("Use System.Runtime.Remoting.ChannelServices.RegisterChannel(IChannel chnl, bool ensureSecurity) instead.", false)]
        public static void RegisterChannel(IChannel chnl)
        {
            RegisterChannelInternal(chnl, false/*ensureSecurity*/);
        }

        
        static bool unloadHandlerRegistered = false;
        [System.Security.SecurityCritical]  // auto-generated
        unsafe internal static void RegisterChannelInternal(IChannel chnl, bool ensureSecurity)
        {
            // Validate arguments
            if(null == chnl)
            {
                throw new ArgumentNullException("chnl");
            }
            Contract.EndContractBlock();
        
            bool fLocked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_channelLock, ref fLocked);
                String chnlName = chnl.ChannelName;

                RegisteredChannelList regChnlList = s_registeredChannels;
        
                // Check to make sure that the channel has not been registered
                if((chnlName == null) ||
                   (chnlName.Length == 0) ||
                   (-1 == regChnlList.FindChannelIndex(chnl.ChannelName)))
                {
                    if (ensureSecurity)
                    {
                        ISecurableChannel securableChannel = chnl as ISecurableChannel;
                        if (securableChannel != null)
                            securableChannel.IsSecured = ensureSecurity;
                        else
                            throw new RemotingException(Environment.GetResourceString("Remoting_Channel_CannotBeSecured", chnl.ChannelName??chnl.ToString()));
                            
                    }
                    RegisteredChannel[] oldList = regChnlList.RegisteredChannels;
                    RegisteredChannel[] newList = null;
                    if (oldList == null)
                    {                                            
                        newList = new RegisteredChannel[1];
                    }
                    else
                        newList = new RegisteredChannel[oldList.Length + 1];

                    if (!unloadHandlerRegistered && !(chnl is CrossAppDomainChannel))
                    {
                        // Register a unload handler only once and if the channel being registered
                        // is not the x-domain channel. x-domain channel does nothing inside its 
                        // StopListening implementation
                        AppDomain.CurrentDomain.DomainUnload += new EventHandler(UnloadHandler);
                        unloadHandlerRegistered = true;
                    }

                    // Add the interface to the array in priority order
                    int priority = chnl.ChannelPriority;
                    int current = 0;
    
                    // Find the place in the array to insert
                    while (current < oldList.Length)
                    {
                        RegisteredChannel oldChannel = oldList[current];
                        if (priority > oldChannel.Channel.ChannelPriority)
                        {
                            newList[current] = new RegisteredChannel(chnl);
                            break;
                        }
                        else
                        {
                            newList[current] = oldChannel;
                            current++;
                        }
                    }

                    if (current == oldList.Length)
                    {
                        // chnl has lower priority than all old channels, so we insert
                        //   it at the end of the list.
                        newList[oldList.Length] = new RegisteredChannel(chnl);
                    }
                    else
                    {
                        // finish copying rest of the old channels
                        while (current < oldList.Length)
                        {
                            newList[current + 1] = oldList[current];
                            current++;
                        }
                    }

                    if (perf_Contexts != null) {
                        perf_Contexts->cChannels++;
                    }

                    s_registeredChannels = new RegisteredChannelList(newList);
                }
                else
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_ChannelNameAlreadyRegistered", chnl.ChannelName));
                }

                RefreshChannelData();
            } // lock (s_channelLock)
            finally
            {
                if (fLocked)
                {
                    Monitor.Exit(s_channelLock);
                }
            }
        } // RegisterChannelInternal
    
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        unsafe public static void UnregisterChannel(IChannel chnl)
        {
            // we allow null to be passed in, so we can use this api to trigger the
            //   refresh of the channel data <

            
            bool fLocked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_channelLock, ref fLocked);
                if (chnl != null)
                {
                    RegisteredChannelList regChnlList = s_registeredChannels;
                
                    // Check to make sure that the channel has been registered
                    int matchingIdx = regChnlList.FindChannelIndex(chnl);
                    if(-1 == matchingIdx)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_ChannelNotRegistered", chnl.ChannelName));
                    }

                    RegisteredChannel[] oldList = regChnlList.RegisteredChannels;
                    RegisteredChannel[] newList = null;
                    Contract.Assert((oldList != null) && (oldList.Length != 0), "channel list should not be empty");

                    newList = new RegisteredChannel[oldList.Length - 1];

                    // Call stop listening on the channel if it is a receiver.
                    IChannelReceiver srvChannel = chnl as IChannelReceiver;
                    if (srvChannel != null)
                        srvChannel.StopListening(null);

                    int current = 0;
                    int oldPos = 0;
                    while (oldPos < oldList.Length)
                    {
                        if (oldPos == matchingIdx)
                        {
                            oldPos++;
                        }
                        else
                        {
                            newList[current] = oldList[oldPos];
                            current++;
                            oldPos++;
                        }
                    }
        
                    if (perf_Contexts != null) {
                        perf_Contexts->cChannels--;
                    }

                    s_registeredChannels = new RegisteredChannelList(newList);
                } 

                RefreshChannelData();
            } // lock (s_channelLock)
            finally
            {
                if (fLocked)
                {
                    Monitor.Exit(s_channelLock);
                }
            }
        } // UnregisterChannel

    
        public static IChannel[] RegisteredChannels
        {       
            [System.Security.SecurityCritical]  // auto-generated_required
            get 
            {
                RegisteredChannelList regChnlList = s_registeredChannels;
                int count = regChnlList.Count;
            
                if (0 == count)
                {
                    return new IChannel[0];
                }
                else 
                {
                    // we hide the CrossAppDomainChannel, so the number of visible
                    //   channels is one less than the number of registered channels.
                    int visibleChannels = count - 1;

                    // Copy the array of visible channels into a new array
                    // and return
                    int co = 0;
                    IChannel[] temp = new IChannel[visibleChannels];
                    for (int i = 0; i < count; i++)
                    {
                        IChannel channel = regChnlList.GetChannel(i);
                        // add the channel to the array if it is not the CrossAppDomainChannel
                        if (!(channel is CrossAppDomainChannel))
                            temp[co++] = channel;
                    }
                    return temp;
                }
            }
        } // RegisteredChannels
        
        [System.Security.SecurityCritical]  // auto-generated
        internal static IMessageSink CreateMessageSink(String url, Object data, out String objectURI) 
        {
            BCLDebug.Trace("REMOTE", "ChannelServices::CreateMessageSink for url " + url + "\n");
            IMessageSink msgSink = null;
            objectURI = null;

            RegisteredChannelList regChnlList = s_registeredChannels;
            int count = regChnlList.Count;
            
            for(int i = 0; i < count; i++)
            {
                if(regChnlList.IsSender(i))
                {
                    IChannelSender chnl = (IChannelSender)regChnlList.GetChannel(i);
                    msgSink = chnl.CreateMessageSink(url, data, out objectURI);
                    
                    if(msgSink != null)
                        break;
                }
            }
            
            // If the object uri has not been set, set it to the url as 
            // default value
            if(null == objectURI)
            {
                objectURI = url;
            }
            
            return msgSink;
        } // CreateMessageSink
    
        [System.Security.SecurityCritical]  // auto-generated
        internal static IMessageSink CreateMessageSink(Object data)
        {
            String objectUri;
            return CreateMessageSink(null, data, out objectUri);
        } // CreateMessageSink
    
    
        [System.Security.SecurityCritical]  // auto-generated_required
        public static IChannel GetChannel(String name)
        {
            RegisteredChannelList regChnlList = s_registeredChannels;
        
            int matchingIdx = regChnlList.FindChannelIndex(name);
            if(0 <= matchingIdx)
            {
                IChannel channel = regChnlList.GetChannel(matchingIdx);
                if ((channel is CrossAppDomainChannel) || (channel is CrossContextChannel))
                    return null;
                else
                    return channel;
            }
            else
            {
                return null;
            }
        } // GetChannel
        
        
        [System.Security.SecurityCritical]  // auto-generated_required
        public static String[] GetUrlsForObject(MarshalByRefObject obj)
        {        
            if(null == obj)
            {
                return null;
            }

            RegisteredChannelList regChnlList = s_registeredChannels;
            int count = regChnlList.Count;
            
            Hashtable table = new Hashtable();
            bool fServer;
            Identity id = MarshalByRefObject.GetIdentity(obj, out fServer);

            if(null != id) 
            {
                String uri = id.ObjURI;

                if (null != uri)
                {
                    for(int i = 0; i < count; i++)
                    {
                        if(regChnlList.IsReceiver(i))
                        {
                            try
                            {
                                String[] urls = ((IChannelReceiver)regChnlList.GetChannel(i)).GetUrlsForUri(uri);
                                // Add the strings to the table
                                for(int j = 0; j < urls.Length; j++)
                                {
                                    table.Add(urls[j], urls[j]);
                                }
                            }
                            catch(NotSupportedException )
                            {
                                // We do not count the channels that do not 
                                // support this method
                            }
                        }
                    }
                }
            }            

            // copy url's into string array
            ICollection keys = table.Keys;
            String[] urlList = new String[keys.Count];
            int co = 0;
            foreach (String key in keys)
            {
                urlList[co++] = key;
            }
            return urlList;
        }

       // Find the channel message sink associated with a given proxy
        // <
        [System.Security.SecurityCritical]  // auto-generated
        internal static IMessageSink GetChannelSinkForProxy(Object obj)
        {
            IMessageSink sink = null;
            if (RemotingServices.IsTransparentProxy(obj))
            {
                RealProxy rp = RemotingServices.GetRealProxy(obj);
                RemotingProxy remProxy = rp as RemotingProxy;
                if (null != remProxy)
                {
                    Identity idObj = remProxy.IdentityObject;
                    Contract.Assert(null != idObj,"null != idObj");
                    sink = idObj.ChannelSink;
                }
            }

            return sink;
        } // GetChannelSinkForProxy
        

        //  Get the message sink dictionary of properties for a given proxy

        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public static IDictionary GetChannelSinkProperties(Object obj)
        {
            IMessageSink sink = GetChannelSinkForProxy(obj);
            IClientChannelSink chnlSink = sink as IClientChannelSink;
            if (null != chnlSink)
            {
                // collect dictionaries for all channel sinks and return
                //   aggregate dictionary
                ArrayList dictionaries = new ArrayList();

                do                
                { 
                    IDictionary dict = chnlSink.Properties;
                    if (dict != null)
                        dictionaries.Add(dict);
                
                    chnlSink = chnlSink.NextChannelSink;
                } while (chnlSink != null);
                
                return new AggregateDictionary(dictionaries);
            }
            else
            {
                IDictionary dict = sink as IDictionary;
                if(null != dict)    
                {
                    return dict;
                }
                else
                {
                    return null;
                }
            }
        } // GetChannelSinkProperties

    
        internal static IMessageSink GetCrossContextChannelSink()
        {
            if(null == xCtxChannel)
            {
                xCtxChannel = CrossContextChannel.MessageSink;
            }
    
            return xCtxChannel;
        } // GetCrossContextChannelSink
               
    
#if DEBUG
        // A few methods to count the number of calls made across appdomains,
        // processes and machines
        internal static long GetNumberOfRemoteCalls()
        {
            return remoteCalls;
        } // GetNumberOfRemoteCalls
#endif //DEBUG
    
        [System.Security.SecurityCritical]  // auto-generated
        unsafe internal static void IncrementRemoteCalls(long cCalls)
        {
            remoteCalls += cCalls;
            if (perf_Contexts != null)
              perf_Contexts->cRemoteCalls += (int)cCalls;
        } // IncrementRemoteCalls
        
        [System.Security.SecurityCritical]  // auto-generated
        internal static void IncrementRemoteCalls()
        {
            IncrementRemoteCalls( 1 );
        } // IncrementRemoteCalls


        [System.Security.SecurityCritical]  // auto-generated
        internal static void RefreshChannelData()
        {
            bool fLocked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(s_channelLock, ref fLocked);
                s_currentChannelData = CollectChannelDataFromChannels();
            }
            finally
            {
                if (fLocked)
                {
                    Monitor.Exit(s_channelLock);
                }
            }
        } // RefreshChannelData

        [System.Security.SecurityCritical]  // auto-generated
        private static Object[] CollectChannelDataFromChannels()
        {
            // Ensure that our native cross-context & cross-domain channels
            // are registered
            RemotingServices.RegisterWellKnownChannels();

            RegisteredChannelList regChnlList = s_registeredChannels;
            int count = regChnlList.Count;            

            // Compute the number of channels that implement IChannelReceiver
            int numChnls = regChnlList.ReceiverCount;

            // Allocate array for channel data
            Object[] data = new Object[numChnls];

            // we need to remove null entries
            int nonNullDataCount = 0;                        

            // Set the channel data, names and mime types
            for (int i = 0, j = 0; i < count; i++)
            {

                IChannel chnl = regChnlList.GetChannel(i);

                if (null == chnl)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_ChannelNotRegistered", ""));
                }

                if (regChnlList.IsReceiver(i))
                {
                    BCLDebug.Trace("REMOTE", "Setting info for receiver " + j.ToString(CultureInfo.InvariantCulture) + "\n");
                    // Extract the data
                    Object channelData = ((IChannelReceiver)chnl).ChannelData;                    
                    data[j] = channelData;
                    if (channelData != null)
                        nonNullDataCount++;

                    // Increment the counter
                    j++;
                }
            }

            if (nonNullDataCount != numChnls)
            {
                // there were null entries, so remove them.
                Object[] nonNullData = new Object[nonNullDataCount];
                int nonNullCounter = 0;
                for (int co = 0; co < numChnls; co++)
                {
                    Object channelData = data[co];
                    if (channelData != null)
                        nonNullData[nonNullCounter++] = channelData;
                }

                data = nonNullData;
            }
            
            return data;
        } // CollectChannelDataFromChannels

        // Checks to make sure the remote method being invoked is callable
        static bool IsMethodReallyPublic(MethodInfo mi)
        {
            if (!mi.IsPublic || mi.IsStatic)
                return false;
     
            if (!mi.IsGenericMethod)
                return true;
     
            foreach (Type t in mi.GetGenericArguments())
                if (!t.IsVisible)
                    return false;
     
            return true;
        }

        //--------------------------------------------------------------------
        //-----------------------  Dispatch Support   ------------------------
        //--------------------------------------------------------------------

        [System.Security.SecurityCritical]  // auto-generated_required
        public static ServerProcessing DispatchMessage(
            IServerChannelSinkStack sinkStack,
            IMessage msg, 
            out IMessage replyMsg)
        {
            ServerProcessing processing = ServerProcessing.Complete;
            replyMsg = null;
            
            try
            {            
                if(null == msg)
                {
                    throw new ArgumentNullException("msg");
                }

                BCLDebug.Trace("REMOTE", "Dispatching for URI " + InternalSink.GetURI(msg));

                // we must switch to the target context of the object and call the context chains etc...
                // Currenly XContextChannel does exactly so. So this method is just a wrapper..
    
                // <

                
                // Make sure that incoming calls are counted as a remote call. This way it 
                // makes more sense on a server.
                IncrementRemoteCalls();
        
                // Check if the object has been disconnected or if it is 
                // a well known object then we have to create it lazily.
                ServerIdentity srvId = CheckDisconnectedOrCreateWellKnownObject(msg);

                // Make sure that this isn't an AppDomain object since we don't allow
                //   calls to the AppDomain from out of process (and x-process calls
                //   are always dispatched through this method)
                if (srvId.ServerType == typeof(System.AppDomain))
                {
                    throw new RemotingException(
                        Environment.GetResourceString(
                            "Remoting_AppDomainsCantBeCalledRemotely"));
                }
                

                IMethodCallMessage mcm = msg as IMethodCallMessage;

                if (mcm == null)
                {
                    // It's a plain IMessage, so just check to make sure that the
                    //   target object implements IMessageSink and dispatch synchronously.

                    if (!typeof(IMessageSink).IsAssignableFrom(srvId.ServerType))
                    {
                        throw new RemotingException(
                            Environment.GetResourceString(
                                "Remoting_AppDomainsCantBeCalledRemotely"));
                    }

                    processing = ServerProcessing.Complete;
                    replyMsg = ChannelServices.GetCrossContextChannelSink().SyncProcessMessage(msg);
                }
                else
                {
                    // It's an IMethodCallMessage.
                
                    // Check if the method is one way. Dispatch one way calls in 
                    // an asynchronous manner
                    MethodInfo method = (MethodInfo)mcm.MethodBase;                                  
    
                    // X-process / X-machine calls should be to non-static
                    // public methods only! Non-public or static methods can't
                    // be called remotely.
                    if (!IsMethodReallyPublic(method) && 
                          !RemotingServices.IsMethodAllowedRemotely(method))
                    {
                        throw new RemotingException(
                            Environment.GetResourceString(
                                "Remoting_NonPublicOrStaticCantBeCalledRemotely"));
                    }

                    RemotingMethodCachedData cache = (RemotingMethodCachedData)
                        InternalRemotingServices.GetReflectionCachedData(method);
                        
                    /*
                        


























*/                  
                    if(RemotingServices.IsOneWay(method))                    
                    {
                        processing = ServerProcessing.OneWay;
                        ChannelServices.GetCrossContextChannelSink().AsyncProcessMessage(msg, null);
                    }
                    else
                    {                    
                        // regular processing
                        processing = ServerProcessing.Complete;
                        if (!srvId.ServerType.IsContextful)
                        {
                            Object[] args = new Object[]{msg, srvId.ServerContext};
                            replyMsg = (IMessage) CrossContextChannel.SyncProcessMessageCallback(args);                            
                        }
                        else 
                            replyMsg = ChannelServices.GetCrossContextChannelSink().SyncProcessMessage(msg);
                    }
                } // end of case for IMethodCallMessage
            }
            catch(Exception e)
            {
                if(processing != ServerProcessing.OneWay)
                {
                    try
                    {                    
                        IMethodCallMessage mcm = 
                            (IMethodCallMessage) ((msg!=null)?msg:new ErrorMessage());
                        replyMsg = (IMessage)new ReturnMessage(e, mcm);
                        if (msg != null)
                        {
                            ((ReturnMessage)replyMsg).SetLogicalCallContext(
                                    (LogicalCallContext)
                                        msg.Properties[Message.CallContextKey]);
                        }
                    }
                    catch(Exception )
                    {
                        // Fatal exception .. ignore
                    }
                }
            }               

            return processing;
        } // DispatchMessage
        
       // This method is used by the channel to dispatch the incoming messages
       // to the server side chain(s) based on the URI embedded in the message.
       // The URI uniquely identifies the receiving object.
       // 
        [System.Security.SecurityCritical]  // auto-generated_required
        public static IMessage SyncDispatchMessage(IMessage msg)
        {            
            IMessage msgRet = null;
            bool fIsOneWay = false;
            
            try
            {            
                if(null == msg)
                {
                    throw new ArgumentNullException("msg");
                }



                // For ContextBoundObject's,
                // we must switch to the target context of the object and call the context chains etc...
                // Currenly XContextChannel does exactly so. So this method is just a wrapper..
    
                
                // Make sure that incoming calls are counted as a remote call. This way it 
                // makes more sense on a server.
                IncrementRemoteCalls();

                // <
                if (!(msg is TransitionCall))
                {
                    // Check if the object has been disconnected or if it is 
                    // a well known object then we have to create it lazily.
                    CheckDisconnectedOrCreateWellKnownObject(msg);

                    MethodBase method = ((IMethodMessage)msg).MethodBase;

                    // Check if the method is one way. Dispatch one way calls in 
                    // an asynchronous manner                    
                    fIsOneWay = RemotingServices.IsOneWay(method);
                }

                // <
                IMessageSink nextSink = ChannelServices.GetCrossContextChannelSink();
                
                if(!fIsOneWay)
                {                    
                    msgRet = nextSink.SyncProcessMessage(msg);  
                }
                else
                {
                    nextSink.AsyncProcessMessage(msg, null);
                }
            }
            catch(Exception e)
            {
                if(!fIsOneWay)
                {
                    try
                    {                    
                        IMethodCallMessage mcm = 
                            (IMethodCallMessage) ((msg!=null)?msg:new ErrorMessage());
                        msgRet = (IMessage)new ReturnMessage(e, mcm);
                        if (msg!=null)
                        {
                            ((ReturnMessage)msgRet).SetLogicalCallContext(
                                mcm.LogicalCallContext);
                        }
                    }
                    catch(Exception )
                    {
                        // Fatal exception .. ignore
                    }
                }
            }               

            return msgRet;
        }

       // This method is used by the channel to dispatch the incoming messages
       // to the server side chain(s) based on the URI embedded in the message.
       // The URI uniquely identifies the receiving object.
       // 
        [System.Security.SecurityCritical]  // auto-generated_required
        public static IMessageCtrl AsyncDispatchMessage(IMessage msg, IMessageSink replySink)
        {
            IMessageCtrl ctrl = null;

            try
            {
                if(null == msg)
                {
                    throw new ArgumentNullException("msg");
                }
            
                // we must switch to the target context of the object and call the context chains etc...
                // Currenly XContextChannel does exactly so. So this method is just a wrapper..
    
                // Make sure that incoming calls are counted as a remote call. This way it 
                // makes more sense on a server.
                IncrementRemoteCalls();
                
                if (!(msg is TransitionCall))
                {
                    // Check if the object has been disconnected or if it is 
                    // a well known object then we have to create it lazily.
                    CheckDisconnectedOrCreateWellKnownObject(msg);    
                }
    
                // <

                ctrl = ChannelServices.GetCrossContextChannelSink().AsyncProcessMessage(msg, replySink);
            }
            catch(Exception e)
            {
                if(null != replySink)
                {
                    try
                    {
                        IMethodCallMessage mcm = (IMethodCallMessage)msg;
                        ReturnMessage retMsg = new ReturnMessage(e, (IMethodCallMessage)msg);
                        if (msg!=null)
                        {
                            retMsg.SetLogicalCallContext(mcm.LogicalCallContext);
                        }
                        replySink.SyncProcessMessage(retMsg);
                    }
                    catch(Exception )
                    {
                        // Fatal exception... ignore
                    }                    
                }
            }

            return ctrl;
        } // AsyncDispatchMessage


        // Creates a channel sink chain (adds special dispatch sink to the end of the chain)
        [System.Security.SecurityCritical]  // auto-generated_required
        public static IServerChannelSink CreateServerChannelSinkChain(
            IServerChannelSinkProvider provider, IChannelReceiver channel)
        {
            if (provider == null)
                return new DispatchChannelSink();       
            
            // add dispatch provider to end (first find last provider)
            IServerChannelSinkProvider lastProvider = provider;
            while (lastProvider.Next != null)
                lastProvider = lastProvider.Next;
            lastProvider.Next = new DispatchChannelSinkProvider();

            IServerChannelSink sinkChain = provider.CreateSink(channel);

            // remove dispatch provider from end
            lastProvider.Next = null;            

            return sinkChain;
        } // CreateServerChannelSinkChain
        
        

        // Check if the object has been disconnected or if it is 
        // a well known object then we have to create it lazily.
        [System.Security.SecurityCritical]  // auto-generated
        internal static ServerIdentity CheckDisconnectedOrCreateWellKnownObject(IMessage msg)
        {
            ServerIdentity ident = InternalSink.GetServerIdentity(msg);
            
            BCLDebug.Trace("REMOTE", "Identity found = " + (ident == null ? "null" : "ServerIdentity"));

            // If the identity is null, then we should check whether the 
            // request if for a well known object. If yes, then we should 
            // create the well known object lazily and marshal it.
            if ((ident == null) || ident.IsRemoteDisconnected())
            {
                String uri = InternalSink.GetURI(msg);
                BCLDebug.Trace("REMOTE", "URI " + uri);
                if (uri != null)
                {
                    ServerIdentity newIdent = RemotingConfigHandler.CreateWellKnownObject(uri);
                    if (newIdent != null)
                    {
                        // The uri was a registered wellknown object.
                        ident = newIdent;
                        BCLDebug.Trace("REMOTE", "Identity created = " + (ident == null ? "null" : "ServerIdentity"));
                    }
                }  

            }


            if ((ident == null) || (ident.IsRemoteDisconnected()))
            {
                String uri = InternalSink.GetURI(msg);
                throw new RemotingException(Environment.GetResourceString("Remoting_Disconnected",uri));                
            }
            return ident;
        }
        
        // Channel Services AppDomain Unload Event Handler
        [System.Security.SecurityCritical]  // auto-generated
        internal static void UnloadHandler(Object sender, EventArgs e)
        {
            StopListeningOnAllChannels();
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static void StopListeningOnAllChannels()
        {
            try
            {
                RegisteredChannelList regChnlList = s_registeredChannels;
                int count = regChnlList.Count;    
            
                for(int i = 0; i < count; i++)
                {
                    if(regChnlList.IsReceiver(i))
                    {
                        IChannelReceiver chnl = (IChannelReceiver)regChnlList.GetChannel(i);
                        chnl.StopListening(null);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore ... appdomain is shutting down..
            }
        }




        //
        // INTERNAL PROFILER NOTIFICATION SERVICES
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static void NotifyProfiler(IMessage msg, RemotingProfilerEvent profilerEvent)
        {
            switch (profilerEvent)
            {
            
            case RemotingProfilerEvent.ClientSend:
            {
                if (RemotingServices.CORProfilerTrackRemoting())
                {
                    Guid g;

                    RemotingServices.CORProfilerRemotingClientSendingMessage(out g, false);

                    if (RemotingServices.CORProfilerTrackRemotingCookie())
                        msg.Properties["CORProfilerCookie"] = g;
                }
                break;
            } // case RemotingProfilerEvent.ClientSend

            case RemotingProfilerEvent.ClientReceive:
            {
                if (RemotingServices.CORProfilerTrackRemoting())
                {
                    Guid g = Guid.Empty;

                    if (RemotingServices.CORProfilerTrackRemotingCookie())
                    {
                        Object obj = msg.Properties["CORProfilerCookie"];

                        if (obj != null)
                        {
                            g = (Guid) obj;
                        }
                    }

                    RemotingServices.CORProfilerRemotingClientReceivingReply(g, false);
                }
                break;
            } // case RemotingProfilerEvent.ClientReceive
            
            } // switch (event)
        } // NotifyProfiler        



        // This is a helper used by UrlObjRef's.
        // Finds an http channel and returns first url for this object.
        [System.Security.SecurityCritical]  // auto-generated
        internal static String FindFirstHttpUrlForObject(String objectUri)
        {                    
            if (objectUri == null)
                return null;       

            RegisteredChannelList regChnlList = s_registeredChannels;
            int count = regChnlList.Count;    

            for (int i = 0; i < count; i++)
            {
                if(regChnlList.IsReceiver(i))
                {       
                    IChannelReceiver chnl = (IChannelReceiver)regChnlList.GetChannel(i);
                    String chnlType = chnl.GetType().FullName;
                    if ((String.CompareOrdinal(chnlType, "System.Runtime.Remoting.Channels.Http.HttpChannel") == 0) ||
                        (String.CompareOrdinal(chnlType, "System.Runtime.Remoting.Channels.Http.HttpServerChannel") == 0))
                    {                                            
                        String[] urls = chnl.GetUrlsForUri(objectUri);
                        if ((urls != null) && (urls.Length > 0))
                            return urls[0];
                    }
                }                               
            }      

            return null;
        } // FindFirstHttpUrlForObject


        //
        // DEBUG Helpers
        //   Note: These methods should be included even in retail builds so that 
        //     they can be called from the debugger.
        //
#if DEBUG
        internal static void DumpRegisteredChannels()
        {
            // To use from cordbg: 
            //   f System.Runtime.Remoting.Channels.ChannelServices::DumpRegisteredChannels

            RegisteredChannelList regChnlList = s_registeredChannels;
            int count = regChnlList.Count; 
        
            Console.Error.WriteLine("Registered Channels:");            
        
            for (int i = 0; i < count; i++)
            {
                IChannel chnl = regChnlList.GetChannel(i);
                Console.Error.WriteLine(chnl);
            }
        } // DumpRegisteredChannels
#endif // DEBUG


    } // class ChannelServices


    // used by ChannelServices.NotifyProfiler
    [Serializable]
    internal enum RemotingProfilerEvent
    {
        ClientSend,
        ClientReceive
    } // RemotingProfilerEvent

    
    
    
    internal class RegisteredChannel
    {
        // private member variables
        private IChannel channel;
        private byte flags;
        private const byte SENDER      = 0x1;
        private const byte RECEIVER    = 0x2;
    
        internal RegisteredChannel(IChannel chnl)
        {
            channel = chnl;
            flags = 0;
            if(chnl is IChannelSender)
            {
                flags |= SENDER;
            }
            if(chnl is IChannelReceiver)
            {
                flags |= RECEIVER;
            }
        }
    
        internal virtual IChannel Channel
        {
            get { return channel; }
        }
    
        internal virtual bool IsSender()
        {
            return ((flags & SENDER) != 0);
        }
    
        internal virtual bool IsReceiver()
        {
            return ((flags & RECEIVER) != 0);
        }
    }// class RegisteredChannel



    // This list should be considered immutable once created.
    //   <STRIP>Ideally, this class would encapsulate more functionality, but
    //   to help minimize the number of changes in the RTM tree, only
    //   a small amount of code has been moved here.</STRIP>
    internal class RegisteredChannelList
    {
        private RegisteredChannel[] _channels;

        internal RegisteredChannelList()
        {
            _channels = new RegisteredChannel[0];
        } // RegisteredChannelList

        internal RegisteredChannelList(RegisteredChannel[] channels)
        {
            _channels = channels;
        } // RegisteredChannelList

        internal RegisteredChannel[] RegisteredChannels
        {
            get { return _channels; }
        } // RegisteredChannels

        internal int Count
        {
            get 
            {
                if (_channels == null)
                    return 0;

                return _channels.Length;
            }
        } // Count

        internal IChannel GetChannel(int index)
        {                
            return _channels[index].Channel;
        } // GetChannel

        internal bool IsSender(int index)
        {
            return _channels[index].IsSender();
        } // IsSender

        internal bool IsReceiver(int index)
        {
            return _channels[index].IsReceiver();
        } // IsReceiver        

        internal int ReceiverCount
        {
            get 
            {
                if (_channels == null)
                    return 0;
                
                int total = 0;
                for (int i = 0; i < _channels.Length; i++)
                {
                    if (IsReceiver(i))
                        total++;
                }
                
                return total;
            }
        } // ReceiverCount
    
        internal int FindChannelIndex(IChannel channel)
        {
            Object chnlAsObject = (Object)channel;
        
            for (int i = 0; i < _channels.Length; i++)
            {
                if (chnlAsObject == (Object)GetChannel(i))
                    return i;                    
            }

            return -1;
        } // FindChannelIndex

        [System.Security.SecurityCritical]  // auto-generated
        internal int FindChannelIndex(String name)
        {        
            for (int i = 0; i < _channels.Length; i++)
            {
                if(String.Compare(name, GetChannel(i).ChannelName, StringComparison.OrdinalIgnoreCase) == 0)
                    return i;                
            }

            return -1;
        } // FindChannelIndex
        
        
    } // class RegisteredChannelList
    



    internal class ChannelServicesData
    {        
        internal long remoteCalls = 0;
        internal CrossContextChannel xctxmessageSink = null;
        internal CrossAppDomainChannel xadmessageSink = null;
        internal bool fRegisterWellKnownChannels = false;
    }

   //
   // Terminator sink used for profiling so that we can intercept asynchronous
   // replies on the server side.
   //  
    
    /* package scope */
    internal class ServerAsyncReplyTerminatorSink : IMessageSink
    {
        internal IMessageSink _nextSink;

        internal ServerAsyncReplyTerminatorSink(IMessageSink nextSink)
        {
            Contract.Assert(nextSink != null,
                            "null IMessageSink passed to ServerAsyncReplyTerminatorSink ctor.");
            _nextSink = nextSink;
        }

        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessage SyncProcessMessage(IMessage replyMsg)
        {
            // If this class has been brought into the picture, then the following must be true.
            Contract.Assert(RemotingServices.CORProfilerTrackRemoting(),
                            "CORProfilerTrackRemoting returned false, but we're in AsyncProcessMessage!");
            Contract.Assert(RemotingServices.CORProfilerTrackRemotingAsync(),
                            "CORProfilerTrackRemoting returned false, but we're in AsyncProcessMessage!");

            Guid g;

            // Notify the profiler that we are receiving an async reply from the server-side
            RemotingServices.CORProfilerRemotingServerSendingReply(out g, true);

            // If GUID cookies are active, then we save it for the other end of the channel
            if (RemotingServices.CORProfilerTrackRemotingCookie())
                replyMsg.Properties["CORProfilerCookie"] = g;

            // Now that we've done the intercepting, pass the message on to the regular chain
            return _nextSink.SyncProcessMessage(replyMsg);
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessageCtrl AsyncProcessMessage(IMessage replyMsg, IMessageSink replySink)
        {
            // Since this class is only used for intercepting async replies, this function should
            // never get called. (Async replies are synchronous, ironically)
            Contract.Assert(false, "ServerAsyncReplyTerminatorSink.AsyncProcessMessage called!");

            return null;
        }
    
        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                return _nextSink;
            }
        }

        // Do I need a finalize here?
    }
}
