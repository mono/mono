// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//  
//   DynamicPropertyHolder manages the dynamically registered properties
//   and the sinks contributed by them. Dynamic properties may be registered
//   to contribute sinks on a per-object basis (on the proxy or server side)
//   or on a per-Context basis (in both the client and server contexts). 
//
//   See also: RemotingServices.RegisterDynamicSink() API
//

namespace System.Runtime.Remoting.Contexts {
    using System.Runtime.Remoting;   
    using System.Runtime.Remoting.Messaging;
    using System;
    using System.Collections;
    using System.Globalization;
    internal class DynamicPropertyHolder
    {
        private const int GROW_BY                        = 0x8;
    
        private IDynamicProperty[] _props;
        private int _numProps;
        private IDynamicMessageSink[] _sinks;
            
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual bool AddDynamicProperty(IDynamicProperty prop)
        {
            lock(this) {
                // We have to add a sink specific to the given context
                CheckPropertyNameClash(prop.Name, _props, _numProps);
        
                // check if we need to grow the array.
                bool bGrow=false;
                if (_props == null || _numProps == _props.Length)    
                {
                    _props = GrowPropertiesArray(_props);
                    bGrow = true;
                }
                // now add the property
                _props[_numProps++] = prop;
                
                // we need to grow the sinks if we grew the props array or we had thrown 
                // away the sinks array due to a recent removal!
                if (bGrow)
                {
                    _sinks = GrowDynamicSinksArray(_sinks);
                }
        
                if (_sinks == null)
                {
                    // Some property got unregistered -- we need to recreate
                    // the list of sinks.
                    _sinks = new IDynamicMessageSink[_props.Length];
                    for (int i=0; i<_numProps; i++)
                    {
                        _sinks[i] = 
                                ((IContributeDynamicSink)_props[i]).GetDynamicSink();
                    }                
                }
                else
                {
                    // append the Sink to the existing array of Sinks
                    _sinks[_numProps-1] = 
                                        ((IContributeDynamicSink)prop).GetDynamicSink();
                }
                
                return true;
        
            }
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual bool RemoveDynamicProperty(String name)
        {
            lock(this) {
                // We have to remove a property for a specific context
                for (int i=0; i<_numProps; i++)
                {
                    if (_props[i].Name.Equals(name))
                    {
                        _props[i] = _props[_numProps-1];
                        _numProps--;
                        // throw away the dynamic sink list                    
                        _sinks = null;
                        return true;
                    }
                }
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Contexts_NoProperty"),
                        name));
            }
        }
    
        internal virtual IDynamicProperty[] DynamicProperties
        {
            get 
            {
                if (_props == null)
                {
                    return null;
                }   
                lock (this)
                {         
                    IDynamicProperty[] retProps = new IDynamicProperty[_numProps];
                    Array.Copy(_props, retProps, _numProps);
                    return retProps;
                }
            }
        }
            
        // We have to do this ArrayWithSize thing instead of 
        // separately providing the Array and a Count ... since they
        // may not be in synch with multiple threads changing things
        // We do not want to provide a copy of the array for each
        // call for perf reasons. Besides this is used internally anyways.
        internal virtual ArrayWithSize DynamicSinks
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_numProps == 0)
                {
                    return null;
                }
                lock (this)
                {
                    if (_sinks == null)
                    {
                        // Some property got unregistered -- we need to recreate
                        // the list of sinks.
                        _sinks = new IDynamicMessageSink[_numProps+GROW_BY];
                        for (int i=0; i<_numProps; i++)
                        {
                            _sinks[i] = 
                                    ((IContributeDynamicSink)_props[i]).GetDynamicSink();
                        }                
                    }
                }
                return new ArrayWithSize(_sinks, _numProps);
            }
        }
    
        private static IDynamicMessageSink[] GrowDynamicSinksArray(IDynamicMessageSink[] sinks)
        {
            // grow the array
            int newSize = (sinks != null ? sinks.Length : 0)  + GROW_BY;
            IDynamicMessageSink[] newSinks = new IDynamicMessageSink[newSize];
            if (sinks != null)
            {
                // Copy existing properties over
                // Initial size should be chosen so that this rarely happens
                Array.Copy(sinks, newSinks, sinks.Length);
            }
            return newSinks;
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal static void NotifyDynamicSinks(IMessage msg, 
            ArrayWithSize dynSinks, bool bCliSide, bool bStart, bool bAsync)
        {
            for (int i=0; i<dynSinks.Count; i++)
            {
                if (bStart == true)
                {
                    dynSinks.Sinks[i].ProcessMessageStart(msg, bCliSide, bAsync);
                }
                else
                {
                    dynSinks.Sinks[i].ProcessMessageFinish(msg, bCliSide, bAsync);
                }                    
            }
        }    
    
        [System.Security.SecurityCritical]  // auto-generated
        internal static void CheckPropertyNameClash(String name, IDynamicProperty[] props, int count)
        {
            for (int i=0; i<count; i++)
            {
                if (props[i].Name.Equals(name))
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_DuplicatePropertyName"));
                }
            }        
        }

        internal static IDynamicProperty[] GrowPropertiesArray(IDynamicProperty[] props)
        {
            // grow the array of IContextProperty objects
            int newSize = (props != null ? props.Length : 0)  + GROW_BY;
            IDynamicProperty[] newProps = new IDynamicProperty[newSize];
            if (props != null)
            {
                // Copy existing properties over.
                Array.Copy(props, newProps, props.Length);
            }
            return newProps;
        }

    } //class DynamicPropertyHolder
    
    // Used to return a reference to an array and the current fill size
    // in cases where it is not thread safe to provide this info as two
    // separate properties. This is for internal use only.
    internal class ArrayWithSize
    {
        internal IDynamicMessageSink[] Sinks;
        internal int Count;
        internal ArrayWithSize(IDynamicMessageSink[] sinks, int count)
        {
            Sinks = sinks;
            Count = count;
        }
    } 

}
