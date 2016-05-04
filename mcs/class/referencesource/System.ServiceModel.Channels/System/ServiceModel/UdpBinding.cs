//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Text;
    using System.Xml;

    public class UdpBinding : Binding, IBindingRuntimePreferences
    {
        TextMessageEncodingBindingElement textEncoding;
        UdpTransportBindingElement udpTransport;

        public UdpBinding()
            : base()
        {
            this.textEncoding = new TextMessageEncodingBindingElement();
            this.udpTransport = new UdpTransportBindingElement();
        }

        public UdpBinding(string configurationName)
            : this()
        {
            UdpBindingCollectionElement section = UdpBindingCollectionElement.GetBindingCollectionElement();
            UdpBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                    configurationName,
                    UdpTransportConfigurationStrings.UdpBindingElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        private UdpBinding(UdpTransportBindingElement transport, TextMessageEncodingBindingElement encoding)
            : this()
        {
            this.DuplicateMessageHistoryLength = transport.DuplicateMessageHistoryLength;
            this.MaxBufferPoolSize = transport.MaxBufferPoolSize;
            this.MaxPendingMessagesTotalSize = transport.MaxPendingMessagesTotalSize;
            this.MaxReceivedMessageSize = transport.MaxReceivedMessageSize;
            this.MaxRetransmitCount = Math.Max(transport.RetransmissionSettings.MaxUnicastRetransmitCount, transport.RetransmissionSettings.MaxMulticastRetransmitCount);
            this.MulticastInterfaceId = transport.MulticastInterfaceId;
            this.TimeToLive = transport.TimeToLive;
            
            this.ReaderQuotas = encoding.ReaderQuotas;
            this.TextEncoding = encoding.WriteEncoding;
        }

        [DefaultValue(UdpConstants.Defaults.DuplicateMessageHistoryLength)]
        public int DuplicateMessageHistoryLength
        {
            get
            {
                return this.udpTransport.DuplicateMessageHistoryLength;
            }
            set
            {
                this.udpTransport.DuplicateMessageHistoryLength = value;
            }
        }

        [DefaultValue(TransportDefaults.MaxBufferPoolSize)]
        public long MaxBufferPoolSize
        {
            get
            {
                return this.udpTransport.MaxBufferPoolSize; 
            }
            set
            {
                this.udpTransport.MaxBufferPoolSize = value;
            }
        }

        [DefaultValue(UdpConstants.Defaults.MaxRetransmitCount)]
        public int MaxRetransmitCount
        {
            get
            {
                return Math.Max(this.udpTransport.RetransmissionSettings.MaxUnicastRetransmitCount, this.udpTransport.RetransmissionSettings.MaxMulticastRetransmitCount);
            }
            set
            {
                this.udpTransport.RetransmissionSettings.MaxUnicastRetransmitCount = value;
                this.udpTransport.RetransmissionSettings.MaxMulticastRetransmitCount = value;
            }
        }

        [DefaultValue(UdpConstants.Defaults.DefaultMaxPendingMessagesTotalSize)]
        public long MaxPendingMessagesTotalSize
        {
            get
            {
                return this.udpTransport.MaxPendingMessagesTotalSize;
            }
            set
            {
                this.udpTransport.MaxPendingMessagesTotalSize = value;
            }
        }

        [DefaultValue(UdpConstants.Defaults.MaxReceivedMessageSize)]
        public long MaxReceivedMessageSize
        {
            get
            {
                return this.udpTransport.MaxReceivedMessageSize;
            }
            set
            {
                this.udpTransport.MaxReceivedMessageSize = value;
            }
        }

        [DefaultValue(UdpConstants.Defaults.MulticastInterfaceId)]
        public string MulticastInterfaceId
        {
            get { return this.udpTransport.MulticastInterfaceId; }
            set { this.udpTransport.MulticastInterfaceId = value; }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return this.textEncoding.ReaderQuotas; }
            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }
                value.CopyTo(this.textEncoding.ReaderQuotas);
            }
        }

        [TypeConverter(typeof(EncodingConverter))]
        public Encoding TextEncoding
        {
            get { return this.textEncoding.WriteEncoding; }
            set { this.textEncoding.WriteEncoding = value; }
        }

        [DefaultValue(UdpConstants.Defaults.TimeToLive)]
        public int TimeToLive
        {
            get { return this.udpTransport.TimeToLive; }
            set { this.udpTransport.TimeToLive = value; }
        }

        public override string Scheme
        {
            get { return this.udpTransport.Scheme; }
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection bindingElements = new BindingElementCollection();
            bindingElements.Add(this.textEncoding);
            bindingElements.Add(this.udpTransport);

            return bindingElements.Clone();
        }

        bool BindingElementsPropertiesMatch(UdpTransportBindingElement transport, MessageEncodingBindingElement encoding)
        {
            if (!this.udpTransport.IsMatch(transport))
            {
                return false;
            }

            if (!this.textEncoding.IsMatch(encoding))
            {
                return false;
            }

            return true;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.InterfaceMethodsShouldBeCallableByChildTypes, Justification = "no need to call this from derrived classes")]
        bool IBindingRuntimePreferences.ReceiveSynchronously
        {
            get { return false; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeReaderQuotas()
        {
            return (!EncoderDefaults.IsDefaultReaderQuotas(this.ReaderQuotas));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTextEncoding()
        {
            return (!this.TextEncoding.Equals(TextEncoderDefaults.Encoding));
        }

        internal static bool TryCreate(BindingElementCollection bindingElements, out Binding binding)
        {
            binding = null;

            if (bindingElements.Count > 2)
            {
                return false;
            }

            UdpTransportBindingElement transport = null;
            TextMessageEncodingBindingElement encoding = null;
            
            foreach (BindingElement bindingElement in bindingElements)
            {
                if (bindingElement is UdpTransportBindingElement)
                {
                    transport = bindingElement as UdpTransportBindingElement;
                }
                else if (bindingElement is TextMessageEncodingBindingElement)
                {
                    encoding = bindingElement as TextMessageEncodingBindingElement;
                }
                else
                {
                    return false;
                }
            }

            if (transport == null || encoding == null)
            {
                return false;
            }

            UdpBinding udpBinding = new UdpBinding(transport, encoding);

            if (!udpBinding.BindingElementsPropertiesMatch(transport, encoding))
            {
                return false;    
            }

            binding = udpBinding;
            return true;
        }
    }
}
