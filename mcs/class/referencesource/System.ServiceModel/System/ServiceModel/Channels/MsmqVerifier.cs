//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Net.Security;
    using System.ServiceModel;

    internal static class MsmqVerifier
    {
        internal static void VerifySender<TChannel>(MsmqChannelFactoryBase<TChannel> factory)
        {
            // no assurances if messages are volatile
            if (!factory.Durable && factory.ExactlyOnce)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqNoAssurancesForVolatile)));

            MsmqChannelFactory<TChannel> transportFactory = factory as MsmqChannelFactory<TChannel>;
            if (null != transportFactory && transportFactory.UseActiveDirectory && QueueTransferProtocol.Native != transportFactory.QueueTransferProtocol)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqActiveDirectoryRequiresNativeTransfer)));

            bool? useActiveDirectory = null;
            if (null != transportFactory)
                useActiveDirectory = transportFactory.UseActiveDirectory;
            VerifySecurity(factory.MsmqTransportSecurity, useActiveDirectory);

            if (null != factory.CustomDeadLetterQueue)
            {
                if (DeadLetterQueue.Custom != factory.DeadLetterQueue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqPerAppDLQRequiresCustom)));
                }

                if (!Msmq.IsPerAppDeadLetterQueueSupported)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqPerAppDLQRequiresMsmq4)));
                }

                if (!factory.ExactlyOnce)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqPerAppDLQRequiresExactlyOnce)));
                }

                string dlqFormatName = MsmqUri.NetMsmqAddressTranslator.UriToFormatName(factory.CustomDeadLetterQueue);

                if (!MsmqQueue.IsWriteable(dlqFormatName))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqDLQNotWriteable)));

                bool isQueueTx;
                if (!MsmqQueue.TryGetIsTransactional(dlqFormatName, out isQueueTx) || !isQueueTx)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqTransactedDLQExpected)));
            }

            if (null == factory.CustomDeadLetterQueue && DeadLetterQueue.Custom == factory.DeadLetterQueue)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqCustomRequiresPerAppDLQ)));
            }

            // token provider needed if Certificate mode requested
            if (MsmqAuthenticationMode.Certificate == factory.MsmqTransportSecurity.MsmqAuthenticationMode)
                EnsureSecurityTokenManagerPresent<TChannel>(factory);
        }

        internal static void VerifyReceiver(MsmqReceiveParameters receiveParameters, Uri listenUri)
        {
            if (!receiveParameters.Durable && receiveParameters.ExactlyOnce)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqNoAssurancesForVolatile)));
            }
            if (receiveParameters.ReceiveContextSettings.Enabled && !receiveParameters.ExactlyOnce)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqExactlyOnceNeededForReceiveContext)));
            }

            VerifySecurity(receiveParameters.TransportSecurity, null);

            string formatName = receiveParameters.AddressTranslator.UriToFormatName(listenUri);

            if (receiveParameters.ReceiveContextSettings.Enabled && formatName.Contains(";"))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqReceiveContextSubqueuesNotSupported)));
            }

            // check if can open the queue for read
            MsmqException msmqException;
            if (!MsmqQueue.IsReadable(formatName, out msmqException))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqQueueNotReadable), msmqException));
            }

            // check if the queue is transactional
            bool knownTxStatus = false;
            bool isQueueTx;
            knownTxStatus = MsmqQueue.TryGetIsTransactional(formatName, out isQueueTx);
            try
            {
                if (!knownTxStatus && (receiveParameters is MsmqTransportReceiveParameters))
                    knownTxStatus = MsmqQueue.TryGetIsTransactional(MsmqUri.ActiveDirectoryAddressTranslator.UriToFormatName(listenUri), out isQueueTx);
            }
            catch (MsmqException ex) // active directory lookup may cause exceptions for certain scenarios
            {
                MsmqDiagnostics.ExpectedException(ex);
            }
            if (knownTxStatus)
            {
                if (!receiveParameters.ExactlyOnce && isQueueTx)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqNonTransactionalQueueNeeded)));
                if (receiveParameters.ExactlyOnce && !isQueueTx)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqTransactionalQueueNeeded)));
            }

            // check poison handling settings
            if (receiveParameters.ExactlyOnce)
            {
                if (Msmq.IsAdvancedPoisonHandlingSupported) // msmq 4
                {
                    if (formatName.Contains(";"))
                    {
                        // no retry queues for subqueues
                        if (ReceiveErrorHandling.Move == receiveParameters.ReceiveErrorHandling)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqNoMoveForSubqueues)));
                    }
                    else
                    {
                        // should be able to open the retry queue for move
                        if (!MsmqQueue.IsMoveable(formatName + ";retry"))
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqDirectFormatNameRequiredForPoison)));
                    }
                }
                else
                {
                    if (ReceiveErrorHandling.Reject == receiveParameters.ReceiveErrorHandling || ReceiveErrorHandling.Move == receiveParameters.ReceiveErrorHandling)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqAdvancedPoisonHandlingRequired)));
                    }
                }
            }
        }

        static void VerifySecurity(MsmqTransportSecurity security, bool? useActiveDirectory)
        {
            if (security.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain && !Msmq.ActiveDirectoryEnabled)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqWindowsAuthnRequiresAD)));

            // MsmqAuthenticationMode.None implies MsmqProtectionLevel.None
            if (security.MsmqAuthenticationMode == MsmqAuthenticationMode.None && security.MsmqProtectionLevel != ProtectionLevel.None)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqAuthNoneRequiresProtectionNone)));

            // MsmqAuthenticationMode.Certificate implies MsmqProtectionLevel.Sign or MsmqProtectionLevel.SignAndEncrypt
            if (security.MsmqAuthenticationMode == MsmqAuthenticationMode.Certificate && security.MsmqProtectionLevel == ProtectionLevel.None)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqAuthCertificateRequiresProtectionSign)));

            // MsmqAuthenticationMode.WindowsDomain doesn't allow MsmqProtectionLevel.None
            if (security.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain)
            {
                if (security.MsmqProtectionLevel == ProtectionLevel.None)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqAuthWindowsRequiresProtectionNotNone)));
            }

            // public queues (thus: AD) needed for encryption
            if (security.MsmqProtectionLevel == ProtectionLevel.EncryptAndSign && useActiveDirectory.HasValue && !useActiveDirectory.Value)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqEncryptRequiresUseAD)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void EnsureSecurityTokenManagerPresent<TChannel>(MsmqChannelFactoryBase<TChannel> factory)
        {
            if (null == factory.SecurityTokenManager)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqTokenProviderNeededForCertificates)));
        }
    }
}
