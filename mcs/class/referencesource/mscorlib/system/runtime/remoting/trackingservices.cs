// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    TrackingServices.cs
**
**
** Purpose: Defines the services for tracking lifetime and other 
**          operations on objects.
**
**
===========================================================*/
namespace System.Runtime.Remoting.Services {
    using System.Security.Permissions;    
    using System;
    using System.Threading;
    using System.Globalization;
    using System.Diagnostics.Contracts;
    using System.Diagnostics.CodeAnalysis;

[System.Runtime.InteropServices.ComVisible(true)]
public interface ITrackingHandler
{
    // Notify a handler that an object has been marshaled
    [System.Security.SecurityCritical]  // auto-generated_required
    void MarshaledObject(Object obj, ObjRef or);
    
    // Notify a handler that an object has been unmarshaled
    [System.Security.SecurityCritical]  // auto-generated_required
    void UnmarshaledObject(Object obj, ObjRef or);
    
    // Notify a handler that an object has been disconnected
    [System.Security.SecurityCritical]  // auto-generated_required
    void DisconnectedObject(Object obj);
}


[System.Security.SecurityCritical]  // auto-generated_required
[System.Runtime.InteropServices.ComVisible(true)]
public class TrackingServices
{
    // Private member variables        
    private static volatile ITrackingHandler[] _Handlers = new ITrackingHandler[0];  // Array of registered tracking handlers
    private static volatile int _Size = 0;                                           // Number of elements in the array

    private static Object s_TrackingServicesSyncObject = null;

    private static Object TrackingServicesSyncObject 
    {
        get
        {
            if (s_TrackingServicesSyncObject == null)
            {
                Object o = new Object();
                Interlocked.CompareExchange(ref s_TrackingServicesSyncObject, o, null);
            }
            return s_TrackingServicesSyncObject;
        }
    }
        
    [System.Security.SecurityCritical]  // auto-generated
    public static void RegisterTrackingHandler(ITrackingHandler handler)
    {
        // Validate arguments
        if (null == handler)
        {
            throw new ArgumentNullException("handler");
        }
        Contract.EndContractBlock();

        lock (TrackingServicesSyncObject)
        {            
            // Check to make sure that the handler has not been registered
            if(-1 == Match(handler))
            {
                // Allocate a new array if necessary
                if((null == _Handlers) || (_Size == _Handlers.Length))
                {
                    ITrackingHandler[] temp = new ITrackingHandler[_Size*2+4];
                    if(null != _Handlers)
                    {
                        Array.Copy(_Handlers, temp, _Size);
                    }
                    _Handlers = temp;
                }        
                
                Volatile.Write(ref _Handlers[_Size++], handler);
            }
            else
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_TrackingHandlerAlreadyRegistered", "handler"));
            }
        }
    }
    
    [System.Security.SecurityCritical]  // auto-generated
    public static void UnregisterTrackingHandler(ITrackingHandler handler)
    {
        // Validate arguments
        if (null == handler)
        {
            throw new ArgumentNullException("handler");
        }
        Contract.EndContractBlock();

        lock (TrackingServicesSyncObject)
        {
            // Check to make sure that the channel has been registered
            int matchingIdx = Match(handler);
            if(-1 == matchingIdx)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_HandlerNotRegistered", handler));
            }

            // Delete the entry by copying the remaining entries        
            Array.Copy(_Handlers, matchingIdx+1, _Handlers, matchingIdx, _Size-matchingIdx-1);
            _Size--;
        }        
    }
    
    public static ITrackingHandler[] RegisteredHandlers
    {
        [System.Security.SecurityCritical]  // auto-generated
        get 
        {
            lock(TrackingServicesSyncObject)
            {
                if(0 == _Size)
                {
                    return new ITrackingHandler[0];
                }
                else 
                {
                    // Copy the array of registered handlers into a new array
                    // and return
                    ITrackingHandler[] temp = new ITrackingHandler[_Size];
                    for(int i = 0; i < _Size; i++)
                    {
                        temp[i] = _Handlers[i];
                    }
                    return temp;
                }
            }
        }
    }

    // Notify all the handlers that an object has been marshaled
    [System.Security.SecurityCritical]  // auto-generated
    internal static void MarshaledObject(Object obj, ObjRef or)
    {
        try{
            ITrackingHandler[] temp = _Handlers;
            for(int i = 0; i < _Size; i++)
            {
                Volatile.Read(ref temp[i]).MarshaledObject(obj, or);
            }
        }
        catch {}
    }
    
    // Notify all the handlers that an object has been unmarshaled
    [System.Security.SecurityCritical]  // auto-generated
    internal static void UnmarshaledObject(Object obj, ObjRef or)
    {
        try{
            ITrackingHandler[] temp = _Handlers;
            for(int i = 0; i < _Size; i++)
            {
                Volatile.Read(ref temp[i]).UnmarshaledObject(obj, or);
            }
        }
        catch {}
    }
    
    // Notify all the handlers that an object has been disconnected
    [System.Security.SecurityCritical]  // auto-generated
    internal static void DisconnectedObject(Object obj)
    {
        try{
            ITrackingHandler[] temp = _Handlers;
            for(int i = 0; i < _Size; i++)
            {
                Volatile.Read(ref temp[i]).DisconnectedObject(obj);
            }
        }
        catch {}
    }

    [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety")]
    private static int Match(ITrackingHandler handler)
    {
        int idx = -1;

        for(int i = 0; i < _Size; i++)
        {
            if(_Handlers[i] == handler)
            {
                idx = i;
                break;
            }
        }

        return idx;
    }
}

} // namespace 
