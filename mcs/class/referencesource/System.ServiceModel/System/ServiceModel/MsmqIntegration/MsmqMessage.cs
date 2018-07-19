//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.Messaging;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [MessageContract(IsWrapped = false)]
    public sealed class MsmqMessage<T>
    {
        [MessageProperty(Name = MsmqIntegrationMessageProperty.Name)]
        MsmqIntegrationMessageProperty property;

        public MsmqMessage(T body)
        {
            if (body == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("body");
            this.property = new MsmqIntegrationMessageProperty();
            this.property.Body = body;
        }

        internal MsmqMessage()
        { }

        public T Body
        {
            get { return (T)this.property.Body; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.property.Body = value;
            }
        }

        public AcknowledgeTypes? AcknowledgeType
        {
            get { return this.property.AcknowledgeType; }
            set { this.property.AcknowledgeType = value; }
        }

        public Acknowledgment? Acknowledgment
        {
            get { return this.property.Acknowledgment; }
        }

        public Uri AdministrationQueue
        {
            get { return this.property.AdministrationQueue; }
            set { this.property.AdministrationQueue = value; }
        }

        public int? AppSpecific
        {
            get { return this.property.AppSpecific; }
            set { this.property.AppSpecific = value; }
        }

        public DateTime? ArrivedTime
        {
            get { return this.property.ArrivedTime; }
        }

        public bool? Authenticated
        {
            get { return this.property.Authenticated; }
        }

        public int? BodyType
        {
            get { return this.property.BodyType; }
            set { this.property.BodyType = value; }
        }

        public string CorrelationId
        {
            get { return this.property.CorrelationId; }
            set { this.property.CorrelationId = value; }
        }

        public Uri DestinationQueue
        {
            get { return this.property.DestinationQueue; }
        }

        public byte[] Extension
        {
            get { return this.property.Extension; }
            set { this.property.Extension = value; }
        }

        public string Id
        {
            get { return this.property.Id; }
        }

        public string Label
        {
            get { return this.property.Label; }
            set { this.property.Label = value; }
        }

        public MessageType? MessageType
        {
            get { return this.property.MessageType; }
        }

        public MessagePriority? Priority
        {
            get { return this.property.Priority; }
            set { this.property.Priority = value; }
        }

        public Uri ResponseQueue
        {
            get { return this.property.ResponseQueue; }
            set { this.property.ResponseQueue = value; }
        }

        public byte[] SenderId
        {
            get { return this.property.SenderId; }
        }

        public DateTime? SentTime
        {
            get { return this.property.SentTime; }
        }

        public TimeSpan? TimeToReachQueue
        {
            get { return this.property.TimeToReachQueue; }
            set { this.property.TimeToReachQueue = value; }
        }
    }
}
