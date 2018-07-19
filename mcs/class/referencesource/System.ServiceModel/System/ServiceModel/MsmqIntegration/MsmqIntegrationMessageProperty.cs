//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.ComponentModel;
    using System.Messaging;
    using System.Runtime;

    public sealed class MsmqIntegrationMessageProperty
    {
        public const string Name = "MsmqIntegrationMessageProperty";

        public static MsmqIntegrationMessageProperty Get(System.ServiceModel.Channels.Message message)
        {
            if (null == message)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            if (null == message.Properties)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message.Properties");

            return message.Properties[Name] as MsmqIntegrationMessageProperty;
        }

        object body;
        public object Body 
        {
            get { return this.body; }
            set { this.body = value; }
        }

        AcknowledgeTypes? acknowledgeType = null;
        public AcknowledgeTypes? AcknowledgeType
        {
            get { return this.acknowledgeType; }
            set { this.acknowledgeType = value; }
        }

        Acknowledgment? acknowledgment = null;
        public Acknowledgment? Acknowledgment
        {
            get { return this.acknowledgment; }
            internal set { this.acknowledgment = value; }
        }
            
        Uri administrationQueue = null;
        public Uri AdministrationQueue
        {
            get { return this.administrationQueue; }
            set { this.administrationQueue = value; }
        }
        
        int? appSpecific = null;
        public int? AppSpecific
        {
            get { return this.appSpecific; }
            set { this.appSpecific = value; }
        }
        
        DateTime? arrivedTime = null;
        public DateTime? ArrivedTime 
        {
            get { return this.arrivedTime; }
            internal set { this.arrivedTime = value; }
        }

        bool? authenticated = null;
        public bool? Authenticated 
        {
            get { return this.authenticated; }
            internal set { this.authenticated = value; }
        }

        int? bodyType = null;
        public int? BodyType
        {
            get { return this.bodyType; }
            set { this.bodyType = value; }
        }

        string correlationId = null;
        public string CorrelationId
        {
            get { return this.correlationId; }
            set { this.correlationId = value; }
        }
        
        Uri destinationQueue = null;
        public Uri DestinationQueue
        {
            get { return this.destinationQueue; }
            internal set { this.destinationQueue = value; }
        }

        byte[] extension = null;
        public byte[] Extension
        {
            get { return this.extension; }
            set { this.extension = value; }
        }

        string id = null;
        public string Id
        {
            get { return this.id; }
            internal set { this.id = value; }
        }

        string label = null;
        public string Label
        {
            get { return this.label; }
            set { this.label = value; }
        }

        MessageType? messageType = null;
        public MessageType? MessageType
        {
            get { return this.messageType; }
            internal set { this.messageType = value; }
        }

        MessagePriority? priority = null;
        public MessagePriority? Priority
        {
            get { return this.priority; }
            set 
            { 
                ValidateMessagePriority(value);
                this.priority = value; 
            }
        }
        
        Uri responseQueue = null;
        public Uri ResponseQueue
        {
            get { return this.responseQueue; }
            set { this.responseQueue = value; }
        }
        
        byte[] senderId = null;
        public byte[] SenderId 
        {
            get { return this.senderId; }
            internal set { this.senderId = value; }
        }

        DateTime? sentTime = null;
        public DateTime? SentTime 
        {
            get { return this.sentTime; }
            internal set { this.sentTime = value; }
        }
        
        TimeSpan? timeToReachQueue = null;
        public TimeSpan? TimeToReachQueue
        {
            get { return this.timeToReachQueue; }
            set 
            { 
                ValidateTimeToReachQueue(value);
                this.timeToReachQueue = value; 
            }
        }

        internal void InternalSetTimeToReachQueue(TimeSpan timeout)
        {
            this.timeToReachQueue = timeout;
        }

        static void ValidateMessagePriority(MessagePriority? priority)
        {
            if (priority.HasValue && (priority.Value < MessagePriority.Lowest || priority.Value > MessagePriority.Highest))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("priority", (int)priority, typeof(MessagePriority)));
        }

        static void ValidateTimeToReachQueue(TimeSpan? timeout)
        {
            if (timeout.HasValue && timeout.Value < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", timeout,
                    SR.GetString(SR.SFxTimeoutOutOfRange0)));
            }

            if (timeout.HasValue && TimeoutHelper.IsTooLarge(timeout.Value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", timeout,
                    SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
            }

        }
    }
}
