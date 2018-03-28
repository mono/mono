//------------------------------------------------------------------------------
// <copyright file="BufferedWebEventProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System;
    using System.Web;
    using System.Diagnostics;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Configuration.Provider;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Security;
    using Debug=System.Web.Util.Debug;
    using System.Security.Permissions;

    // Interface for buffered event provider
    public abstract class BufferedWebEventProvider : WebEventProvider {
        bool    _buffer = true;
        string  _bufferMode;
        WebEventBuffer  _webEventBuffer;

        public override void Initialize(string name, NameValueCollection config)
        {
            // create buffer according to the buffer mode settings specified, like we do in sql/mail providers
            // wire up the delegate to the ProcessEventFlush method
            Debug.Trace("BufferedWebEventProvider", "Initializing: name=" + name);

            ProviderUtil.GetAndRemoveBooleanAttribute(config, "buffer", name, ref _buffer);

            if (_buffer) {
                ProviderUtil.GetAndRemoveRequiredNonEmptyStringAttribute(config, "bufferMode", name, ref _bufferMode);
                _webEventBuffer = new WebEventBuffer(this, _bufferMode, new WebEventBufferFlushCallback(this.ProcessEventFlush));
            }
            else {
                ProviderUtil.GetAndRemoveStringAttribute(config, "bufferMode", name, ref _bufferMode);
            }

            base.Initialize(name, config);
            
            ProviderUtil.CheckUnrecognizedAttributes(config, name);            
        }

        public bool UseBuffering {
            get { return _buffer; }
        }

        public string BufferMode {
            get { return _bufferMode; }
        }
        
        public override void ProcessEvent(WebBaseEvent eventRaised) 
        { 
            if (_buffer) {
                // register the event with the buffer instead of writing it out 
                Debug.Trace("BufferedWebEventProvider", "Saving event to buffer: event=" + eventRaised.GetType().Name);
                _webEventBuffer.AddEvent(eventRaised);
            }
            else {
                WebEventBufferFlushInfo flushInfo = new WebEventBufferFlushInfo(
                                new WebBaseEventCollection(eventRaised),
                                EventNotificationType.Unbuffered,
                                0,
                                DateTime.MinValue,
                                0,
                                0);

                ProcessEventFlush(flushInfo);
            }
        } 

        public abstract void ProcessEventFlush(WebEventBufferFlushInfo flushInfo);

        public override void Flush() {
            if (_buffer) {
                _webEventBuffer.Flush(Int32.MaxValue, FlushCallReason.StaticFlush);
            }
        }

        public override void Shutdown() {
            if (_webEventBuffer != null) {
                _webEventBuffer.Shutdown();
            }
        }
    }

}
