//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Threading;
    using System.Net.Security;

    internal static class Msmq
    {
        static Version longhornVersion = new Version(4, 0);
        static Version version;
        static bool activeDirectoryEnabled;
        static object xpSendLock = null;
        static UriPrefixTable<ITransportManagerRegistration> transportManagerTable = new UriPrefixTable<ITransportManagerRegistration>();
        static object staticLock = new object();

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile SafeLibraryHandle errorStrings = null;

        static Msmq()
        {
            MsmqQueue.GetMsmqInformation(ref version, ref activeDirectoryEnabled);
            MsmqDiagnostics.MsmqDetected(version);
            Version osVersion = System.Environment.OSVersion.Version;
            if (osVersion.Major == 5 && osVersion.Minor == 1)
                xpSendLock = new object();
        }

        internal static bool ActiveDirectoryEnabled
        {
            get { return activeDirectoryEnabled; }
        }

        internal static Version Version
        {
            get { return version; }
        }

        internal static bool IsPerAppDeadLetterQueueSupported
        {
            get { return Msmq.Version >= longhornVersion; }
        }

        internal static bool IsAdvancedPoisonHandlingSupported
        {
            get { return Msmq.Version >= longhornVersion; }
        }

        internal static bool IsRejectMessageSupported
        {
            get { return Msmq.Version >= longhornVersion; }
        }

        internal static bool IsRemoteReceiveContextSupported
        {
            get { return Msmq.Version >= longhornVersion; }
        }

        internal static UriPrefixTable<ITransportManagerRegistration> StaticTransportManagerTable
        {
            get { return transportManagerTable; }
        }

        internal static IPoisonHandlingStrategy CreatePoisonHandler(MsmqReceiveHelper receiver)
        {
            if (receiver.Transactional)
            {
                if (Msmq.Version < longhornVersion)
                {
                    return new Msmq3PoisonHandler(receiver);
                }
                else
                {
                    if (receiver.ListenUri.AbsoluteUri.Contains(";"))
                        return new Msmq4SubqueuePoisonHandler(receiver);
                    else
                        return new Msmq4PoisonHandler(receiver);
                }
            }
            else
            {
                return new MsmqNonTransactedPoisonHandler(receiver);
            }
        }

        internal static MsmqQueue CreateMsmqQueue(MsmqReceiveHelper receiver)
        {
            if (receiver.MsmqReceiveParameters.ReceiveContextSettings.Enabled)
            {
                if (Msmq.Version < longhornVersion)
                {
                    return new MsmqDefaultLockingQueue(receiver.MsmqReceiveParameters.AddressTranslator.UriToFormatName(receiver.ListenUri), UnsafeNativeMethods.MQ_RECEIVE_ACCESS);
                }
                else
                {
                    return new MsmqSubqueueLockingQueue(receiver.MsmqReceiveParameters.AddressTranslator.UriToFormatName(receiver.ListenUri), receiver.ListenUri.Host, UnsafeNativeMethods.MQ_RECEIVE_ACCESS);
                }
            }
            else
            {
                return new MsmqQueue(receiver.MsmqReceiveParameters.AddressTranslator.UriToFormatName(receiver.ListenUri), UnsafeNativeMethods.MQ_RECEIVE_ACCESS);
            }
        }

        internal static SafeLibraryHandle ErrorStrings
        {
            get
            {
                if (null == errorStrings)
                {
                    lock (staticLock)
                    {
                        if (null == errorStrings)
                        {
#pragma warning suppress 56523 // Callers (there is only one) handle an invalid handle returned from here.
                            errorStrings = UnsafeNativeMethods.LoadLibrary("MQUTIL.DLL");
                        }
                    }
                }
                return errorStrings;
            }
        }

        internal static void EnterXPSendLock(out bool lockHeld, ProtectionLevel protectionLevel)
        {
            lockHeld = false;
            if (null != xpSendLock && (ProtectionLevel.None != protectionLevel))
            {
                Monitor.Enter(xpSendLock, ref lockHeld);
            }
        }

        internal static void LeaveXPSendLock()
        {
            Monitor.Exit(xpSendLock);
        }
    }
}

