//------------------------------------------------------------  
// Copyright (c) Microsoft Corporation.  All rights reserved.   
//------------------------------------------------------------  

namespace System.ServiceModel.Channels
{
    using System.Runtime.CompilerServices;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Transactions;
    using System.ServiceModel.Security.Tokens;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using SR = System.ServiceModel.SR;
   
    class MsmqOutputMessage<TChannel> : NativeMsmqMessage
    {
        BufferProperty body;
        IntProperty bodyType;
        ByteProperty delivery;
        IntProperty timeToReachQueue;
        IntProperty timeToBeReceived;
        ByteProperty journal;
        StringProperty deadLetterQueue;
        IntProperty senderIdType;
        IntProperty authLevel;
        BufferProperty senderCert;
        IntProperty privLevel;
        ByteProperty trace;
        BufferProperty messageId;
        IntProperty encryptionAlgorithm;
        IntProperty hashAlgorithm;

        public MsmqOutputMessage(MsmqChannelFactoryBase<TChannel> factory, int bodySize, EndpointAddress remoteAddress)
            : this(factory, bodySize, remoteAddress, 0)
        {
        }

        protected MsmqOutputMessage(MsmqChannelFactoryBase<TChannel> factory, int bodySize, EndpointAddress remoteAddress, int additionalPropertyCount)
            : base(15 + additionalPropertyCount)
        {
            this.body = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_BODY, bodySize);
            this.messageId = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_MSGID, UnsafeNativeMethods.PROPID_M_MSGID_SIZE);

            EnsureBodyTypeProperty(UnsafeNativeMethods.VT_VECTOR | UnsafeNativeMethods.VT_UI1);
            EnsureJournalProperty((byte)UnsafeNativeMethods.MQMSG_JOURNAL, factory.UseSourceJournal);

            this.delivery = new ByteProperty(this, UnsafeNativeMethods.PROPID_M_DELIVERY);
            if (factory.Durable)
            {
                this.delivery.Value = (byte)UnsafeNativeMethods.MQMSG_DELIVERY_RECOVERABLE;
            }
            else
            {
                this.delivery.Value = (byte)UnsafeNativeMethods.MQMSG_DELIVERY_EXPRESS;
            }

            if (factory.TimeToLive != TimeSpan.MaxValue)
            {
                int totalSeconds = MsmqDuration.FromTimeSpan(factory.TimeToLive);

                EnsureTimeToReachQueueProperty(totalSeconds);

                this.timeToBeReceived = new IntProperty(this,
                                                        UnsafeNativeMethods.PROPID_M_TIME_TO_BE_RECEIVED, totalSeconds);
            }

            switch (factory.DeadLetterQueue)
            {
                case DeadLetterQueue.None:
                    EnsureJournalProperty((byte)UnsafeNativeMethods.MQMSG_DEADLETTER, false);
                    break;
                case DeadLetterQueue.System:
                    EnsureJournalProperty((byte)UnsafeNativeMethods.MQMSG_DEADLETTER, true);
                    break;
                case DeadLetterQueue.Custom:
                    EnsureJournalProperty((byte)UnsafeNativeMethods.MQMSG_DEADLETTER, true);
                    EnsureDeadLetterQueueProperty(factory.DeadLetterQueuePathName);
                    break;
            }

            if (MsmqAuthenticationMode.WindowsDomain == factory.MsmqTransportSecurity.MsmqAuthenticationMode)
            {
                EnsureSenderIdTypeProperty(UnsafeNativeMethods.MQMSG_SENDERID_TYPE_SID);

                this.authLevel = new IntProperty(this, UnsafeNativeMethods.PROPID_M_AUTH_LEVEL,
                                                 UnsafeNativeMethods.MQMSG_AUTH_LEVEL_ALWAYS);

                this.hashAlgorithm = new IntProperty(
                    this,
                    UnsafeNativeMethods.PROPID_M_HASH_ALG,
                    MsmqSecureHashAlgorithmHelper.ToInt32(factory.MsmqTransportSecurity.MsmqSecureHashAlgorithm));

                if (ProtectionLevel.EncryptAndSign == factory.MsmqTransportSecurity.MsmqProtectionLevel)
                {
                    this.privLevel = new IntProperty(this, UnsafeNativeMethods.PROPID_M_PRIV_LEVEL,
                                                     UnsafeNativeMethods.MQMSG_PRIV_LEVEL_BODY_ENHANCED);

                    this.encryptionAlgorithm = new IntProperty(
                        this,
                        UnsafeNativeMethods.PROPID_M_ENCRYPTION_ALG,
                        MsmqEncryptionAlgorithmHelper.ToInt32(factory.MsmqTransportSecurity.MsmqEncryptionAlgorithm));
                }
            }
            else if (MsmqAuthenticationMode.Certificate == factory.MsmqTransportSecurity.MsmqAuthenticationMode)
            {
                this.authLevel = new IntProperty(this, UnsafeNativeMethods.PROPID_M_AUTH_LEVEL,
                                                 UnsafeNativeMethods.MQMSG_AUTH_LEVEL_ALWAYS);

                this.hashAlgorithm = new IntProperty(
                    this,
                    UnsafeNativeMethods.PROPID_M_HASH_ALG,
                    MsmqSecureHashAlgorithmHelper.ToInt32(factory.MsmqTransportSecurity.MsmqSecureHashAlgorithm));

                if (ProtectionLevel.EncryptAndSign == factory.MsmqTransportSecurity.MsmqProtectionLevel)
                {
                    this.privLevel = new IntProperty(this, UnsafeNativeMethods.PROPID_M_PRIV_LEVEL,
                                                     UnsafeNativeMethods.MQMSG_PRIV_LEVEL_BODY_ENHANCED);

                    this.encryptionAlgorithm = new IntProperty(
                        this,
                        UnsafeNativeMethods.PROPID_M_ENCRYPTION_ALG,
                        MsmqEncryptionAlgorithmHelper.ToInt32(factory.MsmqTransportSecurity.MsmqEncryptionAlgorithm));
                }

                EnsureSenderIdTypeProperty(UnsafeNativeMethods.MQMSG_SENDERID_TYPE_NONE);
                this.senderCert = new BufferProperty(this, UnsafeNativeMethods.PROPID_M_SENDER_CERT);
            }
            else
            {
                this.authLevel = new IntProperty(this, UnsafeNativeMethods.PROPID_M_AUTH_LEVEL,
                                                 UnsafeNativeMethods.MQMSG_AUTH_LEVEL_NONE);

                EnsureSenderIdTypeProperty(UnsafeNativeMethods.MQMSG_SENDERID_TYPE_NONE);
            }

            this.trace = new ByteProperty(this, UnsafeNativeMethods.PROPID_M_TRACE, (byte)(factory.UseMsmqTracing ?
                                                                                           UnsafeNativeMethods.MQMSG_SEND_ROUTE_TO_REPORT_QUEUE : UnsafeNativeMethods.MQMSG_TRACE_NONE));
        }

        public BufferProperty Body
        {
            get { return this.body; }
        }

        public BufferProperty MessageId
        {
            get { return this.messageId; }
        }

        internal void ApplyCertificateIfNeeded(SecurityTokenProviderContainer certificateTokenProvider, MsmqAuthenticationMode authenticationMode, TimeSpan timeout)
        {
            if (MsmqAuthenticationMode.Certificate == authenticationMode)
            {
                if (certificateTokenProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificateTokenProvider");
                }
                X509Certificate2 clientCertificate = certificateTokenProvider.GetCertificate(timeout);
                if (clientCertificate == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqCertificateNotFound)));
                this.senderCert.SetBufferReference(clientCertificate.GetRawCertData());
            }
        }

        protected void EnsureBodyTypeProperty(int value)
        {
            if (this.bodyType == null)
            {
                this.bodyType = new IntProperty(this, UnsafeNativeMethods.PROPID_M_BODY_TYPE);
            }
            this.bodyType.Value = value;
        }

        protected void EnsureDeadLetterQueueProperty(string value)
        {
            if (value.Length > 0)
            {
                if (this.deadLetterQueue == null)
                {
                    this.deadLetterQueue = new StringProperty(this, UnsafeNativeMethods.PROPID_M_DEADLETTER_QUEUE, value);
                }
                else
                {
                    this.deadLetterQueue.SetValue(value);
                }
            }
        }

        protected void EnsureSenderIdTypeProperty(int value)
        {
            if (this.senderIdType == null)
            {
                this.senderIdType = new IntProperty(this, UnsafeNativeMethods.PROPID_M_SENDERID_TYPE);
            }
            this.senderIdType.Value = value;
        }

        protected void EnsureTimeToReachQueueProperty(int value)
        {
            if (this.timeToReachQueue == null)
            {
                this.timeToReachQueue = new IntProperty(this,
                                                        UnsafeNativeMethods.PROPID_M_TIME_TO_REACH_QUEUE);
            }

            this.timeToReachQueue.Value = value;
        }

        protected void EnsureJournalProperty(byte flag, bool isFlagSet)
        {
            if (this.journal == null)
            {
                this.journal = new ByteProperty(this, UnsafeNativeMethods.PROPID_M_JOURNAL);
            }

            if (isFlagSet)
            {
                this.journal.Value |= flag;
            }
            else
            {
                this.journal.Value &= (byte)(~flag);
            }
        }
    }
}

