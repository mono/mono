//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Threading;
    using System.Security.Authentication.ExtendedProtection;

    sealed class ChannelBindingMessageProperty : IDisposable, IMessageProperty
    {
        const string propertyName = "ChannelBindingMessageProperty";

        ChannelBinding channelBinding;
        object thisLock;
        bool ownsCleanup;
        int refCount;

        public ChannelBindingMessageProperty(ChannelBinding channelBinding, bool ownsCleanup)
        {
            if (channelBinding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelBinding");
            }

            this.refCount = 1;
            this.thisLock = new object();
            this.channelBinding = channelBinding;
            this.ownsCleanup = ownsCleanup;
        }

        public static string Name { get { return propertyName; } }

        bool IsDisposed
        {
            get
            {
                return this.refCount <= 0;
            }
        }

        public ChannelBinding ChannelBinding
        {
            get
            {
                ThrowIfDisposed();
                return this.channelBinding;
            }
        }

        public static bool TryGet(Message message, out ChannelBindingMessageProperty property)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out ChannelBindingMessageProperty property)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            property = null;
            object value;

            if (properties.TryGetValue(ChannelBindingMessageProperty.Name, out value))
            {
                property = value as ChannelBindingMessageProperty;
                return property != null;
            }

            return false;
        }

        public void AddTo(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            AddTo(message.Properties);
        }

        public void AddTo(MessageProperties properties)
        {
            ThrowIfDisposed();
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            properties.Add(ChannelBindingMessageProperty.Name, this);
        }

        public IMessageProperty CreateCopy()
        {
            lock (this.thisLock)
            {
                ThrowIfDisposed();
                this.refCount++;
                return this;
            }
        }

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                lock (this.thisLock)
                {
                    if (!this.IsDisposed && --this.refCount == 0)
                    {
                        if (ownsCleanup)
                        {
                            // Accessing via IDisposable to avoid Security check (functionally the same)
                            ((IDisposable)this.channelBinding).Dispose();
                        }
                    }
                }
            }
        }

        void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }
        }
    }
}
